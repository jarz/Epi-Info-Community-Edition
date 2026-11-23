using System;
using System.Linq;
using Epi.Core.EnterInterpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Epi.UnitTests.CheckCode
{
    /// <summary>
    /// Behaviour tests for the Desktop Enter check-code interpreter (Epi.Core.EnterInterpreter) driven through a
    /// recording IEnterInterpreterHost. Expected behaviour comes from the CDC Epi Info 7 User Guide, Chapter 13
    /// (https://stacks.cdc.gov/view/cdc/40256) and from the operator precedence encoded in
    /// Epi.Core.EnterInterpreter\grammar\EpiInfo.Enter.Grammar.grm.
    /// </summary>
    [TestClass]
    public class CheckCodeExecutionTests
    {
        private static RecordingEnterHost RunAfter(string fieldName, string statements, Action<RecordingEnterHost> setup = null)
        {
            RecordingEnterHost host = new RecordingEnterHost();
            if (setup != null)
            {
                setup(host);
            }

            EpiInterpreterParser parser = CheckCodeTestSupport.CreateInterpreter(host);
            parser.Execute("Field " + fieldName + "\r\nAfter\r\n" + statements + "\r\nEnd-After\r\nEnd-Field\r\n");
            Assert.IsFalse(parser.IsExecuteError, "Interpreter reported an execution error while registering the check code.");
            CheckCodeTestSupport.RunFieldAfter(parser, fieldName);
            return host;
        }

        private static double AssignedNumber(RecordingEnterHost host, string name)
        {
            Assert.IsTrue(host.Assigned.ContainsKey(name), name + " was not assigned. Calls: " + string.Join("; ", host.Calls));
            return Convert.ToDouble(host.Assigned[name]);
        }

        [TestMethod]
        public void Arithmetic_MultiplicationBindsTighterThanAddition()
        {
            // grammar: <Add Exp> ::= <Add Exp> '+' <Mult Exp> ...; <Mult Exp> ::= <Mult Exp> '*' <Negate Exp> ...
            RecordingEnterHost host = RunAfter("X", "ASSIGN Result = 2 + 3 * 4\r\nASSIGN Grouped = (2 + 3) * 4");
            Assert.AreEqual(14d, AssignedNumber(host, "Result"), 1e-12);
            Assert.AreEqual(20d, AssignedNumber(host, "Grouped"), 1e-12);
        }

        [TestMethod]
        public void Arithmetic_SubtractionAndDivisionAreLeftAssociative()
        {
            // grammar productions are left-recursive (<Add Exp> ::= <Add Exp> '-' <Mult Exp>).
            RecordingEnterHost host = RunAfter("X", "ASSIGN Diff = 10 - 4 - 3\r\nASSIGN Quot = 64 / 4 / 2");
            Assert.AreEqual(3d, AssignedNumber(host, "Diff"), 1e-12);
            Assert.AreEqual(8d, AssignedNumber(host, "Quot"), 1e-12);
        }

        [TestMethod]
        public void UserGuideBmiExample_UsesFieldValuesAndDocumentedFormula()
        {
            // User Guide 13-3, ASSIGN example 3: ASSIGN BMI = (Weight / (Height * Height)) * 703 (pounds, inches).
            RecordingEnterHost host = RunAfter("Height", "ASSIGN BMI = (Weight / (Height * Height)) * 703", h =>
            {
                h.SetField("Weight", EpiInfo.Plugin.DataType.Number, "150");
                h.SetField("Height", EpiInfo.Plugin.DataType.Number, "65");
            });

            Assert.AreEqual((150d / (65d * 65d)) * 703d, AssignedNumber(host, "BMI"), 1e-9);
        }

        [TestMethod]
        public void UserGuideConcatenationExample_JoinsTextWithAmpersand()
        {
            // User Guide 13-3, ASSIGN example 4.
            RecordingEnterHost host = RunAfter("MI", "ASSIGN ID = LastName & \" - \" & Sex & \" (\" & MI & \")\"", h =>
            {
                h.SetField("LastName", EpiInfo.Plugin.DataType.Text, "Smith");
                h.SetField("Sex", EpiInfo.Plugin.DataType.Text, "F");
                h.SetField("MI", EpiInfo.Plugin.DataType.Text, "J");
            });

            Assert.AreEqual("Smith - F (J)", Convert.ToString(host.Assigned["ID"]));
        }

        [TestMethod]
        public void UserGuideMinorExample_AssignsYesOnlyWhenConditionIsTrue()
        {
            // User Guide 13-2, ASSIGN example 2: IF Age < 18 THEN ASSIGN Minor = (+) END; (+) is Yes.
            const string code = "IF Age < 18 THEN\r\nASSIGN Minor = (+)\r\nEND";

            RecordingEnterHost child = RunAfter("Age", code, h => h.SetField("Age", EpiInfo.Plugin.DataType.Number, "12"));
            Assert.IsTrue(child.Assigned.ContainsKey("Minor"), "Minor should be assigned for Age 12.");
            Assert.AreEqual(true, Convert.ToBoolean(child.Assigned["Minor"]));

            RecordingEnterHost adult = RunAfter("Age", code, h => h.SetField("Age", EpiInfo.Plugin.DataType.Number, "40"));
            Assert.IsFalse(adult.Assigned.ContainsKey("Minor"), "No IF block commands run when the condition is false (13-22).");
        }

        [TestMethod]
        public void UserGuideHideUnhideExample_RunsIfOrElseBranch()
        {
            // User Guide 13-20 HIDE/UNHIDE example 1 and 13-22: ELSE commands run when the condition is false.
            const string code = "IF Sex = \"M\" THEN\r\nHIDE Pregnant\r\nHIDE ChildBirth\r\nELSE\r\nUNHIDE Pregnant\r\nUNHIDE ChildBirth\r\nEND-IF";

            RecordingEnterHost male = RunAfter("Sex", code, h => h.SetField("Sex", EpiInfo.Plugin.DataType.Text, "M"));
            CollectionAssert.AreEqual(new[] { "HIDE Pregnant", "HIDE ChildBirth" }, male.Calls.ToArray());

            RecordingEnterHost female = RunAfter("Sex", code, h => h.SetField("Sex", EpiInfo.Plugin.DataType.Text, "F"));
            CollectionAssert.AreEqual(new[] { "UNHIDE Pregnant", "UNHIDE ChildBirth" }, female.Calls.ToArray());
        }

        [TestMethod]
        public void HideFieldList_AndHideExcept_PassFieldNamesAndExceptFlagToHost()
        {
            // User Guide 13-20 example 3 (HIDE with several fields); grammar <Hide_Except_Statement> ::= HIDE '*' EXCEPT <IdentifierList>.
            RecordingEnterHost host = RunAfter("Sex", "HIDE Pregnant ChildBirth Complications\r\nHIDE * EXCEPT Sex Age");
            CollectionAssert.AreEqual(new[] { "HIDE Pregnant ChildBirth Complications", "HIDE * EXCEPT Sex Age" }, host.Calls.ToArray());
        }

        [TestMethod]
        public void UserGuideClearAndGotoExample_ClearsThenMovesCursor()
        {
            // User Guide 13-8, CLEAR example 3 (dates compared as numbers here to avoid locale-dependent date parsing).
            const string code = "IF DOB > SurveyDate THEN\r\nCLEAR SurveyDate\r\nGOTO SurveyDate\r\nEND-IF";

            RecordingEnterHost invalid = RunAfter("SurveyDate", code, h =>
            {
                h.SetField("DOB", EpiInfo.Plugin.DataType.Number, "20");
                h.SetField("SurveyDate", EpiInfo.Plugin.DataType.Number, "10");
            });
            CollectionAssert.AreEqual(new[] { "CLEAR SurveyDate", "GOTO SurveyDate" }, invalid.Calls.ToArray());

            RecordingEnterHost valid = RunAfter("SurveyDate", code, h =>
            {
                h.SetField("DOB", EpiInfo.Plugin.DataType.Number, "10");
                h.SetField("SurveyDate", EpiInfo.Plugin.DataType.Number, "20");
            });
            Assert.AreEqual(0, valid.Calls.Count, "Nothing should run when DOB <= SurveyDate.");
        }

        [TestMethod]
        public void ClearWithSeveralFields_ClearsAllNamedFields()
        {
            // User Guide 13-7: "More than one field may be specified" (example 2: CLEAR DOB SurveyDate).
            RecordingEnterHost host = RunAfter("SurveyDate", "CLEAR DOB SurveyDate");
            CollectionAssert.AreEqual(new[] { "CLEAR DOB SurveyDate" }, host.Calls.ToArray());
        }

        [TestMethod]
        public void DefinedStandardVariable_IsStoredInInterpreterScopeNotSentToHost()
        {
            // User Guide 13-10: DEFINE creates a variable; STANDARD is the default scope and the variable lives in
            // check code (DefineVariables section), not in a form field.
            RecordingEnterHost host = new RecordingEnterHost();
            EpiInterpreterParser parser = CheckCodeTestSupport.CreateInterpreter(host);
            parser.Execute("DefineVariables\r\nDEFINE BodyMassIndex NUMERIC\r\nEnd-DefineVariables\r\nField Height\r\nAfter\r\nASSIGN BodyMassIndex = 6 * 7\r\nEnd-After\r\nEnd-Field\r\n");
            Assert.IsFalse(parser.IsExecuteError);

            CheckCodeTestSupport.RunFieldAfter(parser, "Height");

            EpiInfo.Plugin.IVariable variable = parser.RuleContext.CurrentScope.Resolve("BodyMassIndex");
            Assert.IsNotNull(variable, "DEFINE should create BodyMassIndex in the interpreter scope.");
            Assert.AreEqual(EpiInfo.Plugin.VariableScope.Standard, variable.VariableScope);
            Assert.AreEqual(EpiInfo.Plugin.DataType.Number, variable.DataType);
            Assert.AreEqual(42d, Convert.ToDouble(variable.Expression), 1e-12);
            Assert.IsFalse(host.Assigned.ContainsKey("BodyMassIndex"), "A defined variable must not be written to a form field.");
        }
    }
}
