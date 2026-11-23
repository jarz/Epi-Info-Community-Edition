using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Epi.UnitTests.CheckCode
{
    /// <summary>
    /// Syntax tests for the Desktop Enter check-code grammar (Epi.Core.EnterInterpreter\grammar\EpiInfo.Enter.Grammar.grm,
    /// compiled into the embedded EpiInfo.Enter.Grammar.cgt resource). Positive cases are the documented examples in
    /// the CDC Epi Info 7 User Guide, Chapter 13 "Command Reference" (https://stacks.cdc.gov/view/cdc/40256), wrapped in
    /// the Field ... After ... End-After ... End-Field block the guide says they belong in.
    /// </summary>
    [TestClass]
    public class CheckCodeGrammarTests
    {
        private static string InFieldAfter(string fieldName, string statements)
        {
            return "Field " + fieldName + "\r\n\tAfter\r\n" + statements + "\r\n\tEnd-After\r\nEnd-Field\r\n";
        }

        private static readonly Dictionary<string, string> UserGuideExamples = new Dictionary<string, string>
        {
            // p. 13-2 (PDF p. 512) ASSIGN example 1 and 2
            { "ASSIGN ex1 (13-2)", InFieldAfter("SurveyDate", "IF NOT BirthDate = (.) AND NOT SurveyDate = (.) THEN\r\nASSIGN Age = YEARS(BirthDate, SurveyDate)\r\nEND-IF") },
            { "ASSIGN ex2 (13-2)", InFieldAfter("Age", "IF Age < 18 THEN\r\nASSIGN Minor = (+)\r\nEND") },
            // p. 13-3 ASSIGN examples 3 and 4
            { "ASSIGN ex3 (13-3)", InFieldAfter("Height", "ASSIGN BMI = (Weight / (Height * Height)) * 703") },
            { "ASSIGN ex4 (13-3)", InFieldAfter("MI", "ASSIGN ID = LastName & \" - \" & Sex & \" (\" & MI & \")\"") },
            // p. 13-4 AUTOSEARCH
            { "AUTOSEARCH (13-4)", InFieldAfter("LastName", "AUTOSEARCH FirstName LastName") },
            // p. 13-7/13-8 CLEAR examples 1-3
            { "CLEAR ex1 (13-7)", InFieldAfter("Age", "IF Age >= 18 THEN\r\nBEEP\r\nDIALOG \"Do not include records for adults.\"\r\nCLEAR Age\r\nEND-IF") },
            { "CLEAR ex2 (13-7)", InFieldAfter("SurveyDate", "IF (DOB > SurveyDate) OR (SurveyDate > SYSTEMDATE) THEN\r\nCLEAR DOB SurveyDate\r\nDIALOG \"Invalid date detected. Please try again.\"\r\nEND-IF") },
            { "CLEAR ex3 (13-8)", InFieldAfter("SurveyDate", "IF DOB > SurveyDate THEN\r\nCLEAR SurveyDate\r\nGOTO SurveyDate\r\nDIALOG \"Survey date invalid. Please try again.\"\r\nEND-IF") },
            // p. 13-18/13-19 GOTO examples 1-3
            { "GOTO ex1 (13-18)", InFieldAfter("DoYouSmoke", "IF DoYouSmoke = (+) THEN\r\nGOTO HeartDisease\r\nELSE\r\nGOTO PacksPerDay\r\nEND-IF") },
            { "GOTO ex2 (13-18)", InFieldAfter("LungDisease", "IF LungDisease = (-) THEN\r\nGOTO 2\r\nEND-IF") },
            { "GOTO ex3 (13-19)", InFieldAfter("DOB", "IF NOT DOB = (.) THEN\r\nGOTO +1\r\nEND-IF") },
            // p. 13-20/13-21 HIDE / UNHIDE examples 1 and 3
            { "UNHIDE ex1 (13-20)", InFieldAfter("Sex", "IF Sex = \"M\" THEN\r\nHIDE Pregnant\r\nHIDE ChildBirth\r\nELSE\r\nUNHIDE Pregnant\r\nUNHIDE ChildBirth\r\nEND-IF") },
            { "UNHIDE ex3 (13-20)", InFieldAfter("Sex", "IF Sex = \"M\" THEN\r\nHIDE Pregnant ChildBirth Complications\r\nEND-IF") },
            // p. 13-22 IF THEN ELSE example 1
            { "IF ex1 (13-22)", InFieldAfter("Sex", "IF (Sex = \"Male\") THEN\r\nHIDE Pregnancy Childbirth\r\nEND-IF") },
            // p. 13-14 BEFORE/AFTER block example (DIALOG in each block of field Demo1)
            { "BEFORE/AFTER (13-14)", "Field Demo1\r\nBefore\r\nDIALOG \"This is Before entry\"\r\nEnd-Before\r\nAfter\r\nDIALOG \"This is After entry\"\r\nEnd-After\r\nEnd-Field" },
            // p. 13-10/13-11 DEFINE examples 1-3, in the DefineVariables section the guide says user variables are saved in
            { "DEFINE ex1-3 (13-10)", "DefineVariables\r\nDEFINE BodyMassIndex NUMERIC\r\nDEFINE DoYouSmoke YN\r\nDEFINE DateOfBirth DATEFORMAT\r\nEnd-DefineVariables" },
            { "DEFINE ex4-5 (13-11)", "DefineVariables\r\nDEFINE StateID PERMANENT\r\nDEFINE PatientID GLOBAL TEXTINPUT\r\nEnd-DefineVariables" },
            // p. 13-9 comment example (block comment followed by an ASSIGN using YEARS)
            { "COMMENTS (13-9)", InFieldAfter("DOB", "/* Written by Jason D. Veloper, MPH - 06/30/2010\r\nThe block of code below uses the YEARS function\r\n*/\r\nASSIGN AGE = YEARS(DOB, SYSTEMDATE)") },
        };

        [TestMethod]
        public void UserGuideCheckCodeExamples_AreAcceptedByEnterGrammar()
        {
            List<string> failures = new List<string>();
            foreach (KeyValuePair<string, string> example in UserGuideExamples)
            {
                try
                {
                    CheckCodeTestSupport.ParseSyntaxOnly(example.Value);
                }
                catch (Exception ex)
                {
                    failures.Add(example.Key + ": " + ex.Message);
                }
            }

            Assert.AreEqual(0, failures.Count, "Documented examples rejected:\r\n" + string.Join("\r\n", failures));
        }

        [TestMethod]
        public void CheckCodeKeywords_AreCaseInsensitive()
        {
            // grammar: "Case Sensitive" = False
            CheckCodeTestSupport.ParseSyntaxOnly("field age\r\nafter\r\nif age < 18 then\r\nassign minor = (+)\r\nend-if\r\nend-after\r\nend-field");
        }

        [TestMethod]
        public void IfWithoutEndIf_IsRejected()
        {
            AssertRejected(InFieldAfter("Age", "IF Age < 18 THEN\r\nASSIGN Minor = (+)"));
        }

        [TestMethod]
        public void AssignWithoutEqualsSign_IsRejected()
        {
            AssertRejected(InFieldAfter("Age", "ASSIGN Minor (+)"));
        }

        [TestMethod]
        public void StatementOutsideCheckCodeBlock_IsRejected()
        {
            // grammar: <Program> ::= <CheckCodeBlocks>; statements only appear inside Field/Page/Record/Form/Sub blocks.
            AssertRejected("ASSIGN Minor = (+)");
        }

        [TestMethod]
        public void FieldBlockWithoutEndField_IsRejected()
        {
            AssertRejected("Field Age\r\nAfter\r\nASSIGN Minor = (+)\r\nEnd-After\r\n");
        }

        [TestMethod]
        public void CheckedInTemplateViewCheckCode_IsAcceptedByEnterGrammar()
        {
            string templatesDirectory = CheckCodeTestSupport.FindRepositoryContentDirectory("Templates");
            int programsParsed = 0;
            List<string> failures = new List<string>();

            foreach (string file in Directory.GetFiles(templatesDirectory, "*.xml", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                XmlDocument document = new XmlDocument();
                document.Load(file);

                foreach (XmlElement view in document.GetElementsByTagName("View"))
                {
                    string checkCode = view.GetAttribute("CheckCode");
                    if (string.IsNullOrWhiteSpace(checkCode))
                    {
                        continue;
                    }

                    try
                    {
                        CheckCodeTestSupport.ParseSyntaxOnly(checkCode);
                        programsParsed++;
                    }
                    catch (Exception ex)
                    {
                        failures.Add(file.Substring(templatesDirectory.Length).TrimStart('\\') + " [View " + view.GetAttribute("Name") + "]: " + ex.Message);
                    }
                }
            }

            Assert.IsTrue(programsParsed > 0, "No non-empty View CheckCode programs found under " + templatesDirectory);
            Assert.AreEqual(0, failures.Count, "Checked-in template check code rejected by the Enter grammar:\r\n" + string.Join("\r\n", failures));
        }

        private static void AssertRejected(string checkCode)
        {
            try
            {
                CheckCodeTestSupport.ParseSyntaxOnly(checkCode);
            }
            catch (CheckCodeSyntaxException)
            {
                return;
            }

            Assert.Fail("Expected the Enter grammar to reject:\r\n" + checkCode);
        }
    }
}
