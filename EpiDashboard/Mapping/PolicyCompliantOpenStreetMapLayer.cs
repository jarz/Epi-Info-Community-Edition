using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Epi;
using ESRI.ArcGIS.Client.Toolkit.DataSources;

namespace EpiDashboard.Mapping
{
    internal sealed class PolicyCompliantOpenStreetMapLayer : OpenStreetMapLayer
    {
        private const string TileUrlFormat = "https://tile.openstreetmap.org/{0}/{1}/{2}.png";
        private const string UserAgent = "EpiInfo/7.2.7 (https://github.com/jarz/Epi-Info-Community-Edition)";
        private const int DefaultCacheSeconds = 7 * 24 * 60 * 60;

        private static readonly string CacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EpiInfo",
            "OpenStreetMapTiles",
            "v1");
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> TileLocks =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        public PolicyCompliantOpenStreetMapLayer()
        {
            CopyrightText = "© OpenStreetMap contributors";
        }

        public override string GetTileUrl(int level, int row, int col)
        {
            return string.Format(CultureInfo.InvariantCulture, TileUrlFormat, level, col, row);
        }

        protected override void GetTileSource(int level, int row, int col, System.Action<ImageSource> callback)
        {
            string tileUrl = GetTileUrl(level, row, col);

            Task.Run(() => LoadImageAsync(level, row, col, tileUrl))
                .ContinueWith(task =>
                {
                    ImageSource image = null;
                    if (task.IsFaulted)
                    {
                        Logger.Log("OpenStreetMap tile request failed for " + tileUrl + ": " +
                            task.Exception.GetBaseException().Message);
                    }
                    else if (!task.IsCanceled)
                    {
                        image = task.Result;
                    }

                    Dispatcher.BeginInvoke(new System.Action(() => callback(image)));
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private static async Task<ImageSource> LoadImageAsync(int level, int row, int col, string tileUrl)
        {
            string cacheKey = level.ToString(CultureInfo.InvariantCulture) + Path.DirectorySeparatorChar +
                col.ToString(CultureInfo.InvariantCulture) + Path.DirectorySeparatorChar +
                row.ToString(CultureInfo.InvariantCulture);
            string imagePath = Path.Combine(CacheDirectory, cacheKey + ".png");
            string metadataPath = Path.Combine(CacheDirectory, cacheKey + ".txt");
            SemaphoreSlim tileLock = TileLocks.GetOrAdd(imagePath, path => new SemaphoreSlim(1, 1));

            await tileLock.WaitAsync().ConfigureAwait(false);
            try
            {
                CacheMetadata metadata = ReadMetadata(metadataPath);
                bool hasCachedImage = metadata != null && File.Exists(imagePath);
                byte[] cachedImage = hasCachedImage ? File.ReadAllBytes(imagePath) : null;

                if (hasCachedImage && !metadata.RequiresValidation && metadata.ExpiresUtc > DateTime.UtcNow)
                {
                    return CreateImageSource(cachedImage);
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tileUrl);
                request.UserAgent = UserAgent;
                request.Accept = "image/png";
                request.Timeout = 15000;
                request.ReadWriteTimeout = 15000;

                if (hasCachedImage)
                {
                    if (!string.IsNullOrEmpty(metadata.ETag))
                    {
                        request.Headers[HttpRequestHeader.IfNoneMatch] = metadata.ETag;
                    }
                    if (metadata.LastModifiedUtc.HasValue)
                    {
                        request.IfModifiedSince = metadata.LastModifiedUtc.Value;
                    }
                }

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                    using (Stream responseStream = response.GetResponseStream())
                    using (MemoryStream imageStream = new MemoryStream())
                    {
                        await responseStream.CopyToAsync(imageStream).ConfigureAwait(false);
                        byte[] imageBytes = imageStream.ToArray();
                        CacheMetadata newMetadata = CreateMetadata(response, DateTime.UtcNow, null);

                        if (newMetadata.NoStore)
                        {
                            DeleteCacheFiles(imagePath, metadataPath);
                        }
                        else
                        {
                            TryWriteCache(imagePath, metadataPath, imageBytes, newMetadata);
                        }

                        return CreateImageSource(imageBytes);
                    }
                }
                catch (WebException exception)
                {
                    HttpWebResponse response = exception.Response as HttpWebResponse;
                    if (response == null || response.StatusCode != HttpStatusCode.NotModified || !hasCachedImage)
                    {
                        throw;
                    }

                    using (response)
                    {
                        CacheMetadata refreshedMetadata = CreateMetadata(response, DateTime.UtcNow, metadata);
                        if (refreshedMetadata.NoStore)
                        {
                            DeleteCacheFiles(imagePath, metadataPath);
                        }
                        else
                        {
                            TryWriteCache(imagePath, metadataPath, cachedImage, refreshedMetadata);
                        }
                    }

                    return CreateImageSource(cachedImage);
                }
            }
            finally
            {
                tileLock.Release();
            }
        }

        private static ImageSource CreateImageSource(byte[] imageBytes)
        {
            BitmapImage image = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private static CacheMetadata ReadMetadata(string metadataPath)
        {
            try
            {
                if (!File.Exists(metadataPath))
                {
                    return null;
                }

                string[] values = File.ReadAllLines(metadataPath);
                if (values.Length != 6 || values[0] != "1")
                {
                    return null;
                }

                long expiryTicks;
                int lifetimeSeconds;
                long lastModifiedTicks;
                if (!long.TryParse(values[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out expiryTicks) ||
                    !int.TryParse(values[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out lifetimeSeconds))
                {
                    return null;
                }

                DateTime? lastModifiedUtc = null;
                if (long.TryParse(values[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out lastModifiedTicks))
                {
                    lastModifiedUtc = new DateTime(lastModifiedTicks, DateTimeKind.Utc);
                }

                return new CacheMetadata
                {
                    ExpiresUtc = new DateTime(expiryTicks, DateTimeKind.Utc),
                    ETag = string.IsNullOrEmpty(values[2]) ? null : Encoding.UTF8.GetString(Convert.FromBase64String(values[2])),
                    LastModifiedUtc = lastModifiedUtc,
                    RequiresValidation = values[5] == "true",
                    FreshnessLifetimeSeconds = lifetimeSeconds
                };
            }
            catch (Exception exception)
            {
                Logger.Log("Unable to read OpenStreetMap tile cache metadata: " + exception.Message);
                return null;
            }
        }

        private static CacheMetadata CreateMetadata(HttpWebResponse response, DateTime nowUtc, CacheMetadata previous)
        {
            string cacheControl = response.Headers[HttpResponseHeader.CacheControl] ?? string.Empty;
            bool noStore = false;
            bool requiresValidation = false;
            int? maxAgeSeconds = null;

            foreach (string directive in cacheControl.Split(','))
            {
                string[] parts = directive.Trim().Split(new[] { '=' }, 2);
                string name = parts[0].Trim();
                if (string.Equals(name, "no-store", StringComparison.OrdinalIgnoreCase))
                {
                    noStore = true;
                }
                else if (string.Equals(name, "no-cache", StringComparison.OrdinalIgnoreCase))
                {
                    requiresValidation = true;
                }
                else if (string.Equals(name, "max-age", StringComparison.OrdinalIgnoreCase) && parts.Length == 2)
                {
                    int seconds;
                    if (int.TryParse(parts[1].Trim().Trim('"'), NumberStyles.Integer, CultureInfo.InvariantCulture, out seconds))
                    {
                        maxAgeSeconds = Math.Max(0, seconds);
                    }
                }
            }

            int lifetimeSeconds;
            DateTime expiresUtc;
            string expiresHeader = response.Headers[HttpResponseHeader.Expires];
            if (maxAgeSeconds.HasValue)
            {
                lifetimeSeconds = maxAgeSeconds.Value;
                int ageSeconds;
                if (int.TryParse(response.Headers["Age"], NumberStyles.Integer, CultureInfo.InvariantCulture, out ageSeconds))
                {
                    lifetimeSeconds = Math.Max(0, lifetimeSeconds - ageSeconds);
                }
                expiresUtc = nowUtc.AddSeconds(lifetimeSeconds);
            }
            else if (!string.IsNullOrEmpty(expiresHeader))
            {
                DateTime parsedExpiry;
                if (DateTime.TryParse(expiresHeader, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out parsedExpiry))
                {
                    expiresUtc = parsedExpiry;
                    lifetimeSeconds = Math.Max(0, (int)Math.Min(int.MaxValue, (expiresUtc - nowUtc).TotalSeconds));
                }
                else
                {
                    lifetimeSeconds = DefaultCacheSeconds;
                    expiresUtc = nowUtc.AddSeconds(lifetimeSeconds);
                }
            }
            else if (previous != null && string.IsNullOrEmpty(cacheControl))
            {
                lifetimeSeconds = previous.FreshnessLifetimeSeconds;
                expiresUtc = requiresValidation ? nowUtc : nowUtc.AddSeconds(lifetimeSeconds);
                requiresValidation = previous.RequiresValidation;
            }
            else
            {
                lifetimeSeconds = requiresValidation ? 0 : DefaultCacheSeconds;
                expiresUtc = nowUtc.AddSeconds(lifetimeSeconds);
            }

            if (requiresValidation)
            {
                expiresUtc = nowUtc;
            }

            DateTime lastModifiedUtc;
            DateTime? lastModified = DateTime.TryParse(
                response.Headers[HttpResponseHeader.LastModified],
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out lastModifiedUtc)
                ? (DateTime?)lastModifiedUtc
                : (previous == null ? null : previous.LastModifiedUtc);

            return new CacheMetadata
            {
                ExpiresUtc = expiresUtc,
                ETag = response.Headers[HttpResponseHeader.ETag] ?? (previous == null ? null : previous.ETag),
                LastModifiedUtc = lastModified,
                RequiresValidation = requiresValidation,
                FreshnessLifetimeSeconds = lifetimeSeconds,
                NoStore = noStore
            };
        }

        private static void TryWriteCache(string imagePath, string metadataPath, byte[] imageBytes, CacheMetadata metadata)
        {
            string temporaryImagePath = imagePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            string temporaryMetadataPath = metadataPath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                File.WriteAllBytes(temporaryImagePath, imageBytes);
                File.WriteAllLines(temporaryMetadataPath, new[]
                {
                    "1",
                    metadata.ExpiresUtc.Ticks.ToString(CultureInfo.InvariantCulture),
                    string.IsNullOrEmpty(metadata.ETag) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(metadata.ETag)),
                    metadata.FreshnessLifetimeSeconds.ToString(CultureInfo.InvariantCulture),
                    metadata.LastModifiedUtc.HasValue ? metadata.LastModifiedUtc.Value.Ticks.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    metadata.RequiresValidation ? "true" : "false"
                });

                ReplaceFile(temporaryImagePath, imagePath);
                ReplaceFile(temporaryMetadataPath, metadataPath);
            }
            catch (Exception exception)
            {
                Logger.Log("Unable to cache OpenStreetMap tile: " + exception.Message);
            }
            finally
            {
                TryDeleteTemporaryFile(temporaryImagePath);
                TryDeleteTemporaryFile(temporaryMetadataPath);
            }
        }

        private static void ReplaceFile(string temporaryPath, string targetPath)
        {
            if (File.Exists(targetPath))
            {
                File.Replace(temporaryPath, targetPath, null);
            }
            else
            {
                File.Move(temporaryPath, targetPath);
            }
        }

        private static void DeleteCacheFiles(string imagePath, string metadataPath)
        {
            TryDeleteTemporaryFile(imagePath);
            TryDeleteTemporaryFile(metadataPath);
        }

        private static void TryDeleteTemporaryFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException exception)
            {
                Logger.Log("Unable to remove an OpenStreetMap tile cache file: " + exception.Message);
            }
            catch (UnauthorizedAccessException exception)
            {
                Logger.Log("Unable to remove an OpenStreetMap tile cache file: " + exception.Message);
            }
        }

        private sealed class CacheMetadata
        {
            public DateTime ExpiresUtc { get; set; }
            public string ETag { get; set; }
            public DateTime? LastModifiedUtc { get; set; }
            public bool RequiresValidation { get; set; }
            public int FreshnessLifetimeSeconds { get; set; }
            public bool NoStore { get; set; }
        }
    }
}
