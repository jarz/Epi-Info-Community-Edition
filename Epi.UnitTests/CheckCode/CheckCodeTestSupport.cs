using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using com.calitha.goldparser;
using Epi.Core.EnterInterpreter;
using EpiInfo.Plugin;

namespace Epi.UnitTests.CheckCode
{
    /// <summary>
    /// Test-only IEnterInterpreterHost that records the calls the Enter check-code interpreter
    /// makes and serves field values from an in-memory dictionary (no UI, database or config).
    /// </summary>
    internal sealed class RecordingEnterHost : IEnterInterpreterHost
    {
        public readonly Dictionary<string, KeyValuePair<EpiInfo.Plugin.DataType, string>> Fields =
            new Dictionary<string, KeyValuePair<EpiInfo.Plugin.DataType, string>>(StringComparer.OrdinalIgnoreCase);
        public readonly Dictionary<string, object> Assigned = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public readonly List<string> Calls = new List<string>();

        public void SetField(string name, EpiInfo.Plugin.DataType type, string value)
        {
            Fields[name] = new KeyValuePair<EpiInfo.Plugin.DataType, string>(type, value);
        }

        private void Record(string command, string[] names, bool isExcept)
        {
            Calls.Add(command + (isExcept ? " * EXCEPT " : " ") + string.Join(" ", names ?? new string[0]));
        }

        public bool Register(IEnterInterpreter enterInterpreter) { return true; }
        public bool IsExecutionEnabled { get { return true; } }
        public bool IsSuppressErrorsEnabled { get { return false; } }

        public bool Assign(string pName, object pValue)
        {
            Assigned[pName] = pValue;
            Calls.Add("ASSIGN " + pName);
            return true;
        }

        public bool AssignColor(string pName, object pValue) { Calls.Add("ASSIGNCOLOR " + pName); return true; }
        public bool AssignGrid(string pName, object pValue, int pIndex0, object pIndex1) { Calls.Add("ASSIGNGRID " + pName); return true; }
        public bool Geocode(string address, string latName, string longName) { Calls.Add("GEOCODE"); return true; }
        public bool IOCode(string iinput, string oinput, string industry, string occupation, string icodeName, string ocodeName, string ititleName, string otitleName, string schemeName) { Calls.Add("IOCODE"); return true; }
        public void AutoSearch(string[] pIdentifierList, string[] pDisplayList, bool pAlwaysShow) { Record("AUTOSEARCH", pIdentifierList, false); }
        public int AutosearchFunction(string[] pIdentifierList, string[] pDisplayList, bool pAlwaysShow) { Record("AUTOSEARCHFUNCTION", pIdentifierList, false); return 0; }
        public bool IsUnique(string[] pIdentifierList) { return true; }
        public void Clear(string[] pIdentifierList) { Record("CLEAR", pIdentifierList, false); }
        public void Dialog(string pTextPrompt, string pTitleText) { Calls.Add("DIALOG " + pTextPrompt); }
        public void Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText) { Calls.Add("DIALOG " + pTextPrompt); }
        public bool Dialog(string text, string caption, string mask, string modifier, ref object input, EpiInfo.Plugin.DataType dataType) { Calls.Add("DIALOG " + text); return true; }

        public string GetValue(string pName)
        {
            KeyValuePair<EpiInfo.Plugin.DataType, string> field;
            return Fields.TryGetValue(pName, out field) ? field.Value : null;
        }

        public EpiInfo.Plugin.IVariable GetGridValue(string pName, int pIndex0, object pIndex1) { return null; }
        public void GoTo(string pDesitnation, string targetPage = "", string targetForm = "") { Calls.Add("GOTO " + pDesitnation); }
        public void Hide(string[] pNameList, bool pIsAnExceptList) { Record("HIDE", pNameList, pIsAnExceptList); }
        public void Highlight(string[] pNameList, bool pIsAnExceptList) { Record("HIGHLIGHT", pNameList, pIsAnExceptList); }
        public void UnHighlight(string[] pNameList, bool pIsAnExceptList) { Record("UNHIGHLIGHT", pNameList, pIsAnExceptList); }
        public void Enable(string[] pNameList, bool pIsAnExceptList) { Record("ENABLE", pNameList, pIsAnExceptList); }
        public void Disable(string[] pNameList, bool pIsAnExceptList) { Record("DISABLE", pNameList, pIsAnExceptList); }
        public void SetRequired(string[] pNameList) { Record("SET-REQUIRED", pNameList, false); }
        public void SetNotRequired(string[] pNameList) { Record("SET-NOT-REQUIRED", pNameList, false); }
        public void NewRecord() { Calls.Add("NEWRECORD"); }
        public bool SaveRecord() { Calls.Add("SAVERECORD"); return true; }
        public int RecordCount() { return 0; }
        public void UnHide(string[] pNameList, bool pIsAnExceptList) { Record("UNHIDE", pNameList, pIsAnExceptList); }

        public bool TryGetFieldInfo(string name, out EpiInfo.Plugin.DataType dataType, out string value)
        {
            KeyValuePair<EpiInfo.Plugin.DataType, string> field;
            if (Fields.TryGetValue(name, out field))
            {
                dataType = field.Key;
                value = field.Value;
                return true;
            }

            dataType = EpiInfo.Plugin.DataType.Unknown;
            value = null;
            return false;
        }

        public List<string> GetDbListValues(string ptablename, string pvariablename) { return new List<string>(); }
        public void Quit() { Calls.Add("QUIT"); }
        public GeoPosition<GeoCoordinate> LastPosition { get; set; }
    }

    internal sealed class CheckCodeSyntaxException : Exception
    {
        public CheckCodeSyntaxException(string message) : base(message) { }
    }

    internal static class CheckCodeTestSupport
    {
        /// <summary>
        /// Parses check code against the compiled Enter grammar embedded in Epi.Core.EnterInterpreter,
        /// without building or executing interpreter rules. Throws CheckCodeSyntaxException on error.
        /// </summary>
        public static void ParseSyntaxOnly(string checkCode)
        {
            using (Stream grammar = EpiInterpreterParser.GetEnterCompiledGrammarTable())
            {
                if (grammar == null)
                {
                    throw new InvalidOperationException("Embedded Enter grammar table was not found.");
                }

                LALRParser parser = new CGTReader(grammar).CreateNewParser();
                parser.TrimReductions = false;
                parser.OnTokenError += (p, args) =>
                {
                    throw new CheckCodeSyntaxException("Token error at " + DescribeLocation(args.Token) + ": " + args.Token);
                };
                parser.OnParseError += (p, args) =>
                {
                    throw new CheckCodeSyntaxException("Parse error at " + DescribeLocation(args.UnexpectedToken) + ": unexpected '" + args.UnexpectedToken + "', expected " + args.ExpectedTokens);
                };

                if (parser.Parse(checkCode) == null)
                {
                    throw new CheckCodeSyntaxException("Parser did not accept the input.");
                }
            }
        }

        private static string DescribeLocation(TerminalToken token)
        {
            if (token == null || token.Location == null)
            {
                return "unknown location";
            }

            return "line " + (token.Location.LineNr + 1) + ", column " + (token.Location.ColumnNr + 1);
        }

        public static EpiInterpreterParser CreateInterpreter(RecordingEnterHost host)
        {
            // A non-"global" scope avoids Rule_Context() loading permanent variables from EpiInfo.Config.xml.
            EpiInterpreterParser parser = new EpiInterpreterParser(host, new SymbolTable("unittest", null));
            parser.Host = host;
            return parser;
        }

        public static void RunFieldAfter(EpiInterpreterParser parser, string fieldName)
        {
            ICommand command = parser.RuleContext.GetCommand("level=field&event=after&identifier=" + fieldName);
            if (command == null)
            {
                throw new InvalidOperationException("No AFTER block registered for field " + fieldName);
            }

            command.Execute();
        }

        public static string FindRepositoryContentDirectory(string relativeDirectory)
        {
            string start = Path.GetDirectoryName(typeof(CheckCodeTestSupport).Assembly.Location);
            string candidate = Path.Combine(start, relativeDirectory);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            DirectoryInfo current = new DirectoryInfo(start);
            while (current != null)
            {
                candidate = Path.Combine(current.FullName, "Epi.Core", relativeDirectory);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Could not find '" + relativeDirectory + "' beside the test assembly or in an Epi.Core folder above " + start);
        }
    }
}
