using System;
using System.Data;
using Epi.Data.MongoDB.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;

namespace Epi.UnitTests.MongoDBAdapter
{
    /// <summary>
    /// DateTime round trips through Epi.Data.MongoDB without a server. MongoDBDatabase.InsertBulkRows stores each
    /// Kind=Unspecified wall-clock value with new BsonDateTime(value), which converts it from local time to UTC.
    /// </summary>
    [TestClass]
    public class MongoDBDateTimeTests
    {
        private const string ConnectionString = "mongodb://localhost:27017~epi";

        private static readonly DateTime[] WrittenValues =
        {
            new DateTime(2024, 2, 29, 13, 45, 30, 123),
            new DateTime(2024, 7, 15),
            new DateTime(2024, 11, 3, 1, 30, 0)
        };

        private static DataRow Read(MongoDBWrapper wrapper, BsonDateTime stored)
        {
            BsonDocument document = new BsonDocument { { "Visited", stored }, { "Doses", new BsonArray { stored } } };
            return wrapper.GetDataTableFromBson(new DataTable("visits"), document).Rows[0];
        }

        [TestMethod]
        public void DatesWrittenOnThisMachineReadBackUnchanged()
        {
            MongoDBWrapper wrapper = new MongoDBWrapper(ConnectionString);
            foreach (DateTime written in WrittenValues)
            {
                DataRow row = Read(wrapper, new BsonDateTime(written));
                Assert.AreEqual(written, (DateTime)row["Visited"], written.ToString("o"));
                Assert.AreEqual(written, (DateTime)row["Doses_0"], written.ToString("o"));
            }
        }

        [TestMethod]
        public void DatesReadBackUnchangedInTheWritersTimeZone()
        {
            foreach (string zoneId in new[] { "Eastern Standard Time", "India Standard Time", "E. Africa Standard Time" })
            {
                TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
                MongoDBWrapper wrapper = new MongoDBWrapper(ConnectionString, zone);
                foreach (DateTime written in WrittenValues)
                {
                    DataRow row = Read(wrapper, new BsonDateTime(TimeZoneInfo.ConvertTimeToUtc(written, zone)));
                    Assert.AreEqual(written, (DateTime)row["Visited"], zoneId + " " + written.ToString("o"));
                    Assert.AreEqual(written, (DateTime)row["Doses_0"], zoneId + " " + written.ToString("o"));
                }
            }
        }

        [TestMethod]
        public void DatesAreShownInTheReadersTimeZone()
        {
            TimeZoneInfo eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            MongoDBWrapper pacificReader = new MongoDBWrapper(ConnectionString, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));

            DataRow row = Read(pacificReader, new BsonDateTime(TimeZoneInfo.ConvertTimeToUtc(new DateTime(2024, 7, 15), eastern)));

            Assert.AreEqual(new DateTime(2024, 7, 14, 21, 0, 0), (DateTime)row["Visited"]);
        }
    }
}
