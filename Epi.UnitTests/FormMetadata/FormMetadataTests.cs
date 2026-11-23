using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Epi.UnitTests.CheckCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Epi.UnitTests.FormMetadata
{
    /// <summary>
    /// Form/project metadata serialization tests that need no Jet/ACE, OLE DB, UI or EpiInfo.Config.xml.
    /// Evidence: the checked-in sample projects (Epi.Core\Projects\**\*.prj) and templates (Epi.Core\Templates\**\*.xml),
    /// and the attribute handling in Epi.Windows.MakeView\Template.cs, which is what Desktop uses to load templates.
    /// </summary>
    [TestClass]
    public class FormMetadataTests
    {
        private static readonly string[] TemplateLevelsAcceptedByMakeView = { "Project", "Form", "View", "Page", "Field" };

        private static IEnumerable<string> CheckedInProjectFiles(bool epiInfoProjectsOnly)
        {
            string projectsDirectory = CheckCodeTestSupport.FindRepositoryContentDirectory("Projects");
            foreach (string file in Directory.GetFiles(projectsDirectory, "*.prj", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                bool isXml = File.ReadAllText(file).TrimStart().StartsWith("<", StringComparison.Ordinal);
                if (isXml == epiInfoProjectsOnly)
                {
                    yield return file;
                }
            }
        }

        private static IEnumerable<string> CheckedInTemplateFiles()
        {
            string templatesDirectory = CheckCodeTestSupport.FindRepositoryContentDirectory("Templates");
            return Directory.GetFiles(templatesDirectory, "*.xml", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
        }

        private static string Describe(XmlElement element)
        {
            return element.Name + "(" + string.Join(",", element.Attributes.Cast<XmlAttribute>().Select(a => a.Name)) + ")";
        }

        private static string Shape(XmlDocument document)
        {
            XmlElement root = document.DocumentElement;
            List<string> parts = new List<string> { Describe(root) };
            foreach (XmlElement child in root.ChildNodes.OfType<XmlElement>())
            {
                parts.Add(Describe(child));
                foreach (XmlElement grandChild in child.ChildNodes.OfType<XmlElement>())
                {
                    parts.Add(child.Name + "/" + Describe(grandChild));
                }
            }

            return string.Join(" ", parts);
        }

        [TestMethod]
        public void NewProjectDocument_HasSameShapeAsCheckedInSampleProjects()
        {
            string expected = Shape(new Project().GetXmlDocument());
            List<string> files = CheckedInProjectFiles(true).ToList();
            Assert.IsTrue(files.Count > 0, "No checked-in Epi Info .prj files found.");

            foreach (string file in files)
            {
                XmlDocument document = new XmlDocument();
                document.Load(file);
                Assert.AreEqual(expected, Shape(document), "Element/attribute layout differs for " + file);
            }
        }

        [TestMethod]
        public void NewProjectDocument_DefaultsMatchCheckedInSampleProjects()
        {
            XmlElement root = new Project().GetXmlDocument().DocumentElement;
            Assert.AreEqual("102", root.GetAttribute("schemaVersion"));
            Assert.AreEqual("Epi.Core.EnterInterpreter", root["EnterMakeviewInterpreter"].GetAttribute("source"));

            foreach (string file in CheckedInProjectFiles(true))
            {
                XmlDocument document = new XmlDocument();
                document.Load(file);
                Assert.AreEqual(root.GetAttribute("schemaVersion"), document.DocumentElement.GetAttribute("schemaVersion"), file);
            }
        }

        [TestMethod]
        public void ProjectDocument_RoundTripsThroughXmlText()
        {
            Project project = new Project();
            project.Name = "Synthetic & <Project>";
            project.Location = @"C:\Synthetic\Projects\Synthetic";

            string xml = project.GetXmlDocument().OuterXml;
            XmlDocument reloaded = new XmlDocument();
            reloaded.LoadXml(xml);

            Assert.AreEqual(Shape(project.GetXmlDocument()), Shape(reloaded));
            Assert.AreEqual("Synthetic & <Project>", reloaded.DocumentElement.GetAttribute("name"));
            Assert.AreEqual(@"C:\Synthetic\Projects\Synthetic", reloaded.DocumentElement.GetAttribute("location"));
            Assert.AreEqual(((int)MetadataSource.Unknown).ToString(CultureInfo.InvariantCulture), reloaded.DocumentElement["Metadata"].GetAttribute("source"));
            Assert.AreEqual(project.GetXmlDocument().OuterXml, reloaded.OuterXml);
        }

        [TestMethod]
        public void OpeningCheckedInEsriShapefilePrj_IsRejectedAsNotAnEpiInfoProject()
        {
            // Project.Construct guards against ESRI .prj projection files (GEOGCS[/PROJCS[/GEOCCS[) before loading XML.
            List<string> esriFiles = CheckedInProjectFiles(false).ToList();
            Assert.IsTrue(esriFiles.Count > 0, "Expected the checked-in CommunityHealthAssessment shapefile .prj files.");

            foreach (string file in esriFiles)
            {
                try
                {
                    new Project(file);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                Assert.Fail("Expected ArgumentException for ESRI projection file " + file);
            }
        }

        [TestMethod]
        public void CheckedInTemplates_UseTemplateRootWithLevelAcceptedByMakeView()
        {
            List<string> failures = new List<string>();
            int count = 0;
            foreach (string file in CheckedInTemplateFiles())
            {
                count++;
                XmlDocument document = new XmlDocument();
                document.Load(file);
                XmlElement root = document.DocumentElement;

                if (root.Name != "Template")
                {
                    failures.Add(file + ": root element is " + root.Name);
                }
                else if (root.GetAttribute("Level") == "Project" && string.IsNullOrEmpty(root.GetAttribute("Name")))
                {
                    // Epi.Template.GetProjectTable lists project templates through GetFiles, which skips unnamed templates.
                    failures.Add(file + ": project Template has no Name (Epi.Template.GetFiles ignores such files)");
                }
                else if (!TemplateLevelsAcceptedByMakeView.Contains(root.GetAttribute("Level")))
                {
                    failures.Add(file + ": unsupported Level '" + root.GetAttribute("Level") + "'");
                }
            }

            Assert.IsTrue(count > 0, "No checked-in templates found.");
            Assert.AreEqual(0, failures.Count, string.Join("\r\n", failures));
        }

        [TestMethod]
        public void CheckedInTemplateFields_HaveAttributesRequiredByMakeViewTemplateLoader()
        {
            // Epi.Windows.MakeView.Template.CreateFields creates the field only when FieldTypeId is seen after the first
            // attribute, then reads FieldId, Name, ControlLeftPositionPercentage and ControlTopPositionPercentage.
            List<string> failures = new List<string>();
            int fieldCount = 0;

            foreach (string file in CheckedInTemplateFiles())
            {
                XmlDocument document = new XmlDocument();
                document.Load(file);

                foreach (XmlElement field in document.SelectNodes("//Page/Field").OfType<XmlElement>())
                {
                    fieldCount++;
                    string where = Path.GetFileName(file) + " field '" + field.GetAttribute("Name") + "'";
                    XmlAttribute first = field.Attributes.Count > 0 ? field.Attributes[0] : null;
                    int fieldTypeId;

                    if (first == null || !field.HasAttribute("FieldTypeId") || string.Equals(first.Name, "FieldTypeId", StringComparison.OrdinalIgnoreCase))
                    {
                        failures.Add(where + ": FieldTypeId missing or first attribute");
                    }
                    else if (!int.TryParse(field.GetAttribute("FieldTypeId"), NumberStyles.Integer, CultureInfo.InvariantCulture, out fieldTypeId)
                             || !Enum.IsDefined(typeof(MetaFieldType), fieldTypeId))
                    {
                        failures.Add(where + ": FieldTypeId '" + field.GetAttribute("FieldTypeId") + "' is not a MetaFieldType");
                    }

                    int fieldId;
                    if (!int.TryParse(field.GetAttribute("FieldId"), NumberStyles.Integer, CultureInfo.InvariantCulture, out fieldId))
                    {
                        failures.Add(where + ": FieldId missing or not an integer");
                    }

                    if (string.IsNullOrEmpty(field.GetAttribute("Name")))
                    {
                        failures.Add(where + ": Name missing");
                    }

                    foreach (string position in new[] { "ControlLeftPositionPercentage", "ControlTopPositionPercentage" })
                    {
                        double value;
                        if (!double.TryParse(field.GetAttribute(position), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                        {
                            failures.Add(where + ": " + position + " missing or not a number");
                        }
                    }
                }
            }

            Assert.IsTrue(fieldCount > 0, "No Page/Field elements found in checked-in templates.");
            Assert.AreEqual(0, failures.Count, string.Join("\r\n", failures));
        }
    }
}
