﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcisExtensionCodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string operation = "";
            string operationDisplayName = "";
            string operationType = "";
            string operationGroup = "";
            string operationGroupDisplayName = "";
            bool isApproverParameterNeeded = false;
            bool isApproverLinkParameterNeeded = false;
            string helperFunctionName = "";
            List<Parameter> parameterList = new List<Parameter>();

            // Check input arguments
            if (args.Length == 0)
            {
                Console.WriteLine("Use -h or --help to see the help");
                return;
            }
            if (args[0].Equals("-h") || args[0].Equals("--help"))
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To generate source code: AcisExtensionCodeGenerator.exe <InputFileName>");
                Console.WriteLine("To generate test file: AcisExtensionCodeGenerator.exe <InputFileName> --GenerateTestFileOnly");
                return;
            }

            // Read a input file from command line.
            string inputFileName = "";
            string generateTestFlag = "";
            if (args.Length == 1)
            {
                inputFileName = args[0];
            }
            else if (args.Length == 2)
            {
                inputFileName = args[0];
                generateTestFlag = args[1];
            }

            string inputFile = inputFileName; 
            ParseInputFile(inputFile, ref operation, ref operationDisplayName, ref operationType, ref operationGroup, ref operationGroupDisplayName, 
                ref isApproverParameterNeeded, ref isApproverLinkParameterNeeded, ref helperFunctionName, ref parameterList);

            /*//Test:
            Console.WriteLine("operation is=" + operation);
            Console.WriteLine("operationDisplayName=" + operationDisplayName);
            Console.WriteLine("operationType=" + operationType);
            Console.WriteLine("operationGroup=" + operationGroup);
            Console.WriteLine("operationGroupDisplayName=" + operationGroupDisplayName);
            Console.WriteLine("isApproverParameterNeeded=" + isApproverParameterNeeded);
            Console.WriteLine("isApproverLinkParameterNeed=" + isApproverLinkParameterNeeded);
            Console.WriteLine("helperFunctionName=" + helperFunctionName);
            int numParameters = parameterList.Count;
            Console.WriteLine("Number of parameters=" + numParameters);
            Console.WriteLine("The last parameter: parameter=" + parameterList[numParameters-1].ParamName + " variableType=" + 
                parameterList[numParameters-1].ParamVariableType + " ,parameterDisplayName=" + parameterList[numParameters-1].ParamDisplayName + 
                "parameterType=" + parameterList[numParameters-1].ParamType);
            Console.ReadLine();
            */

            if (!generateTestFlag.Equals("--GenerateTestFileOnly"))
            {
                foreach (Parameter param in parameterList)
                    GenerateParameterCode(param);
                GenerateOperationCode(operation, operationDisplayName, operationType, operationGroup, helperFunctionName,
                    parameterList, isApproverParameterNeeded, isApproverLinkParameterNeeded);
                //if (!string.IsNullOrEmpty(operationGroup)) GenerateOperationGroupCode(operationGroup, operationGroupDisplayName); // All the operation group exist already.
                GenerateHelperFunction(helperFunctionName, parameterList);
                GenerateTestFile(operationGroup, operation, parameterList);
            }
            else
            {
                GenerateTestFile(operationGroup, operation, parameterList);
            }
        }

        private static void GenerateTestFile(string operationGroup, string operation, List<Parameter> parameterList)
        {
            using (
                System.IO.StreamWriter file =
                    new System.IO.StreamWriter(
                        @"C:\Users\xiowei\Documents\Visual Studio 2015\Projects\AcisExtensionCodeGenerator\AcisExtensionCodeGenerator\bin\Debug\outputFiles\" +
                        operation + "Tests.txt", true))
            {
                file.WriteLine("using System;");
                file.WriteLine("using System.ServiceModel;");
                file.WriteLine("using Microsoft.Cis.DevExp.Services.Rdfe.ServiceManagement;");
                file.WriteLine("using Microsoft.Cloud.Engineering.RdfeExtension.DisplayHelpers;");
                file.WriteLine("using Microsoft.Cloud.Engineering.RdfeExtension.Operations.OS;");
                file.WriteLine("using Microsoft.Cloud.Engineering.RdfeExtension.RdfeExtension_UTestCommon;");
                file.WriteLine("using Microsoft.Cloud.Engineering.RdfeExtension.RdfeExtension_UTestCommon.RdfeClient_Mock;");
                file.WriteLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
                file.WriteLine("using Microsoft.WindowsAzure.Wapd.Acis.Contracts;");
                file.WriteLine("using Microsoft.Cloud.Engineering.RdfeExtension.Operations;");
                file.WriteLine("");
                file.WriteLine("namespace Microsoft.Cloud.Engineering.RdfeExtension.RdfeExtension_UTest.Operations." + operationGroup + "// TODO: change here.");
                file.WriteLine("{");
                file.WriteLine("\t[TestClass]");
                file.WriteLine("\tpublic class " + operation + "Tests : RdfeExtensionTestBase");
                file.WriteLine("\t{");
                file.WriteLine("\t\t[ClassInitialize()]");
                file.WriteLine("\t\tpublic static void ClassInitialize(TestContext context)");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tClassInit(context);");
                file.WriteLine("\t\t}");
                file.WriteLine("");
                file.WriteLine("\t\t[TestMethod]");
                file.WriteLine("\t\tpublic void Test_" + operation + "Operation()");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tIAcisSMEOperation operation = new " + operation + "Operation(s_acisExtension.m_SmeRdfeHelper);");
                file.WriteLine("\t\t\ts_acisExtension.m_SmeRdfeHelper.CallingOperation = operation;");
                file.WriteLine("\t\t\t// Get the expected claims required from https://acis.engineering.core.windows.net/authInfo.aspx");
                file.WriteLine("\t\t\tIEnumerable<AcisUserClaim> expectedClaims = new[] {AcisSMESecurityGroup.PlatformServiceAdministrator, AcisSMESecurityGroup.ClientPlatformServiceAdministrator(\"RDFE\")}; // TODO: change here");
                file.WriteLine("\t\t\tHelper.VerifyClaims(expectedClaims, operation.ClaimsRequired);");
                foreach (Parameter param in parameterList)
                {
                    file.WriteLine("\t\t\t" + param.ParamVariableType + " " + param.ParamName + " = ; // TODO");
                }
                file.WriteLine("\t\t\tobject[] invocationParameterValues = new object[]");
                file.WriteLine("\t\t\t{");
                foreach (Parameter param in parameterList)
                {
                    file.WriteLine("\t\t\t\t" + param.ParamName + ",");
                }
                file.WriteLine("\t\t\t\t/*approver*/\"amdha, ferrys\", // TODO: check if this parameter is needed");
                file.WriteLine("\t\t\t\t/*approverLink*/\"https://www.microsoft.com/\",// TODO: check if this parameter is needed");
                file.WriteLine("\t\t\t\t/*endpoint*/s_acisExtension.m_Endpoint,");
                file.WriteLine("\t\t\t\t/*updater*/new MockAcisSMEOperationProgressUpdater(),");
                file.WriteLine("\t\t\t\t/*context*/OperationContext.Current");
                file.WriteLine("\t\t\t};");
                file.WriteLine("");
                file.WriteLine("\t\t\t// May need to construct the expected result. Sample:");
                file.WriteLine("\t\t\t//ConfigurationSettingList configurationSettingList = new ConfigurationSettingList();");
                file.WriteLine("\t\t\t//configurationSettingList.Add(ConfigurationSettingObject());");
                file.WriteLine("");
                file.WriteLine("\t\t\t// May need to inject the mock delegate. Sample");
                file.WriteLine("\t\t\t// Inject the mock delegate.");
                file.WriteLine("\t\t\t//s_acisExtension.m_SmeRdfeHelper.MockOperationStub = objectArray => new MockAsyncResult(configurationSettingList); ");
                file.WriteLine("");
                file.WriteLine("\t\t\tIAcisSMEOperationResponse response = TestHelpers.RunOperation(operation, invocationParameterValues);");
                file.WriteLine("");
                file.WriteLine("\t\t\t// May need to compare the result. Sample:");
                file.WriteLine("\t\t\t//ConfigurationSettingFormatter formatter = new ConfigurationSettingFormatter(s_acisExtension.m_Endpoint);");
                file.WriteLine("\t\t\t//Assert.IsTrue(response.SuccessResult == SuccessResult.Success);");
                file.WriteLine("\t\t\t//Assert.AreEqual(formatter.FormatConfigurationSettingsDisplay(configurationSettingList), response.Result.ToString());");
                file.WriteLine("\t\t\t//StringAssert.Contains(response.Result.ToString(), \"Extension successfully registered.\");");
                file.WriteLine("\t\t}");
                file.WriteLine("\t}");
                file.WriteLine("}");
            }
        }

        private static void GenerateHelperFunction(string helperFunctionName, List<Parameter> parameterList)
        {
            using (
                System.IO.StreamWriter file =
                    new System.IO.StreamWriter(
                        helperFunctionName + ".txt", true))
            {
                file.WriteLine("/// <summary>");
                file.WriteLine("/// TODO");
                file.WriteLine("/// </summary>");
                StringBuilder sb = new StringBuilder();
                foreach (Parameter param in parameterList)
                {
                    file.WriteLine("/// <param name=\"" + param.ParamName +"\">" + param.ParamDisplayName + "</param>");
                    sb.Append(param.ParamVariableType + " " + param.ParamName + ", ");
                }
                file.WriteLine("/// <returns>IAcisSMEOperationResponse</returns>");
                int sbLen = sb.Length;
                sb.Remove(sbLen - 2, 2);
                file.WriteLine("public IAcisSMEOperationResponse " + helperFunctionName + "(" + sb + ")");
                file.WriteLine("{");
                file.WriteLine("\t// TODO: verify the parameters. Delete these two line after verify.");
                sb = new StringBuilder();
                foreach (Parameter param in parameterList)
                {
                    sb.Append("+ \"" + param.ParamName + ":{\"" + " + " + param.ParamName + ".GetType() " + "+ \", \" +" + param.ParamName + "+ \"}; \""); // TODO change here
                }
                sb.Remove(0, 1);
                file.WriteLine("\tthrow new ArgumentException(" + sb + "); // TODO: delete this line after verification.");
                file.WriteLine("\treturn this.ExecuteAdministrationOperation( // TODO: Check here: Administration or Management or others.");
                file.WriteLine("\t\tadmin =>");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\treturn null; //TODO");
                file.WriteLine("\t\t\t},");
                file.WriteLine("\t\terr => string.Format(\"Unable to due to {0}.\", err)); // TODO: Refine the error message.");
                file.WriteLine("}");
            }
        }

        private static void GenerateOperationGroupCode(string operationGroup, string operationGroupDisplayName)
        {
            using (
                System.IO.StreamWriter file =
                    new System.IO.StreamWriter(
                        operationGroup + "OperationGroup.cs", true))
            {
                file.WriteLine("namespace Microsoft.Cloud.Engineering.RdfeExtension");
                file.WriteLine("{");
                file.WriteLine("\tusing Microsoft.WindowsAzure.Wapd.Acis.Contracts;");
                file.WriteLine();
                file.WriteLine("\tpublic class " + operationGroup + "OperationGroup : AcisSMEOperationGroup");
                file.WriteLine("\t{");
                file.WriteLine("\t\tpublic override string Name");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tget { return \"" + operationGroupDisplayName + "\"; }");
                file.WriteLine("\t\t}");
                file.WriteLine("\t}");
                file.WriteLine("}");
            }
        }

        private static void GenerateOperationCode(string operation, string operationDisplayName, string operationType,
            string operationGroup,
            string helperFunctionName, List<Parameter> parameterList, bool isApproverParameterNeeded,
            bool isApproverLinkParameterNeeded)
        {
            using (
                System.IO.StreamWriter file =
                    new System.IO.StreamWriter(
                        operation + "Operation.cs", true))
            {
                file.WriteLine("namespace Microsoft.Cloud.Engineering.RdfeExtension");
                file.WriteLine("{");
                file.WriteLine("\tusing System.Collections.Generic;");
                file.WriteLine("\tusing WindowsAzure.Wapd.Acis.Contracts;");
                file.WriteLine("\tusing OperationGroups;");
                file.WriteLine("\tusing Parameters;");
                file.WriteLine();
                file.WriteLine("\t/// <summary>");
                file.WriteLine("\t/// " + operationDisplayName + " operation.");
                file.WriteLine("\t/// </summary>");
                file.WriteLine("\tpublic class " + operation + "Operation : " + operationType);
                file.WriteLine("\t{");
                file.WriteLine("\t\t/// <summary>");
                file.WriteLine("\t\t/// Gets the operation name that is displayed to the user in the UI.");
                file.WriteLine("\t\t/// </summary>");
                file.WriteLine("\t\tpublic override string OperationName");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tget { return \"" + operationDisplayName +"\"; }");
                file.WriteLine("\t\t}");
                file.WriteLine();
                file.WriteLine("\t\t/// <summary>");
                file.WriteLine("\t\t/// Gets the operation group this operation is part of.");
                file.WriteLine("\t\t/// </summary>");
                file.WriteLine("\t\tpublic override IAcisSMEOperationGroup OperationGroup");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tget { return new " + operationGroup + "Group(); }");
                file.WriteLine("\t\t}");
                file.WriteLine();
                StringBuilder sb = new StringBuilder();
                StringBuilder callersb = new StringBuilder();
                for (int i = 0; i < parameterList.Count; i++)
                {
                    sb.Append(parameterList[i].ParamVariableType)
                        .Append(" ")
                        .Append(parameterList[i].ParamName)
                        .Append(", ");
                    callersb.Append(parameterList[i].ParamName + ", ");
                }
                if (isApproverParameterNeeded)
                {
                    sb.Append("string approver, ");
                }
                if (isApproverLinkParameterNeeded)
                {
                    sb.Append("string approverLink, ");
                }
                sb.Append(
                    "IAcisSMEEndpoint endpoint, IAcisSMEOperationProgressUpdater updater, OperationExecutionContext context");
                file.WriteLine("\t\tpublic IAcisSMEOperationResponse " + helperFunctionName + "(" + sb + ")");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tSmeRdfeHelper helper = new SmeRdfeHelper(endpoint, updater, this, context);");
                int callerSbLength = callersb.Length;
                callersb.Remove(callerSbLength-2, 2);
                file.WriteLine("\t\t\treturn helper." + helperFunctionName + "(" + callersb + ");");
                file.WriteLine("\t\t}");
                file.WriteLine();
                file.WriteLine("\t\t/// <summary>");
                file.WriteLine("\t\t/// Gets the list of parameters that apply to this operation.");
                file.WriteLine("\t\t/// </summary>");
                file.WriteLine("\t\tpublic override IEnumerable<IAcisSMEParameterRef> Parameters");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tget");
                file.WriteLine("\t\t\t{");
                file.WriteLine("\t\t\t\treturn new[]");
                file.WriteLine("\t\t\t\t\t{");
                for (int i = 0; i < parameterList.Count; i++)
                {
                    sb = new StringBuilder();
                    sb.Append("\t\t\t\t\t\tParamRefFromParam.Get<" + parameterList[i].ParamFileName +
                                       "Param>()");
                    if (i < parameterList.Count - 1 || isApproverParameterNeeded || isApproverLinkParameterNeeded)
                    {
                        sb.Append(", ");
                    }
                    file.WriteLine(sb);
                }
                if (isApproverParameterNeeded)
                {
                    file.WriteLine("\t\t\t\t\t\tAcisWellKnownParameters.Get(ParameterName.Approver),");
                    file.WriteLine("\t\t\t\t\t\tAcisWellKnownParameters.Get(ParameterName.ApproverLink)");
                }
                file.WriteLine("\t\t\t\t\t};");
                file.WriteLine("\t\t\t}");
                file.WriteLine("\t\t}");
                file.WriteLine("\t}");
                file.WriteLine("}");
            }
        }

        static void GenerateParameterCode(Parameter parameter)
        {
            using (
                System.IO.StreamWriter file =
                    new System.IO.StreamWriter(
                        parameter.ParamFileName + "Param.cs", true))
            {
                file.WriteLine("namespace Microsoft.Cloud.Engineering.RdfeExtension.Parameters");
                file.WriteLine("{");
                file.WriteLine("\tusing WindowsAzure.Wapd.Acis.Contracts;");
                file.WriteLine();
                file.WriteLine("\tpublic class " + parameter.ParamFileName + "Param : " + parameter.ParamType);
                file.WriteLine("\t{");
                file.WriteLine("\t\t///<summary>");
                file.WriteLine("\t\t/// Gets the short-name or ID form used to identify this parameter.  This name is surfaced in command-line interfaces.");
                file.WriteLine("\t\t/// This value must be all lower-case alphanumeric, with no spaces or punctuation.");
                file.WriteLine("\t\t///</summary>");
                file.WriteLine("\t\tpublic override string Key");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tget { return \"sme" + parameter.ParamFileName.ToLowerInvariant() + "\"; }");
                file.WriteLine("\t\t}");
                file.WriteLine();
                file.WriteLine("\t\t///<summary>");
                file.WriteLine("\t\t///  Full parameter name, displayed in the ACIS UI.");
                file.WriteLine("\t\t///</summary>");
                file.WriteLine("\t\tpublic override string Name");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tget { return \"" + parameter.ParamDisplayName + "\"; }");
                file.WriteLine("\t\t}");
                file.WriteLine();
                file.WriteLine("\t\t///<summary>");
                file.WriteLine("\t\t/// Gets the help text - HTML supported; displayed in the UI when the user clicks the help button.");
                file.WriteLine("\t\t///</summary>");
                file.WriteLine("\t\tpublic override string HelpText");
                file.WriteLine("\t\t{");
                file.WriteLine("\t\t\tget { return"  + "; } //TODO"); 
                file.WriteLine("\t\t}");
                file.WriteLine("\t}");
                file.WriteLine("}");
            }
        }

        private static void ParseInputFile(string inputFile, ref string operation, ref string operationDisplayName,
            ref string operationType, ref string operationGroup,
            ref string operationGroupDisplayName, ref bool isApproverParameterNeeded,
            ref bool isApproverLinkParameterNeeded, ref string helperFunctionName,
            ref List<Parameter> parameterList)
        {
            int counter = 0;
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(inputFile))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        //Console.WriteLine(line);
                        counter++;
                        string[] words = line.Split(':');
                        if (words.Length == 0) continue;
                        switch (words[0])
                        {
                            case "Operation":
                                if (!CheckLine(words, 2)) return;
                                operation = words[1];
                                break;
                            case "OperationDisplayName":
                                if (!CheckLine(words, 2)) return;
                                operationDisplayName = words[1];
                                break;
                            case "OperationType":
                                if (!CheckLine(words, 2)) return;
                                operationType = words[1];
                                break;
                            case "OperationGroup":
                                if (!CheckLine(words, 2)) return;
                                operationGroup = words[1];
                                break;
                            case "OperationGroupDisplayName":
                                if (!CheckLine(words, 2)) return;
                                operationGroupDisplayName = words[1];
                                break;
                            case "ApproverParameterNeeded":
                                if (!CheckLine(words, 2)) return;
                                if (words[1].Equals("Y", StringComparison.OrdinalIgnoreCase))
                                    isApproverParameterNeeded = true;
                                break;
                            case "ApproverLinkParameterNeeded":
                                if (!CheckLine(words, 2)) return;
                                if (words[1].Equals("Y", StringComparison.OrdinalIgnoreCase))
                                    isApproverLinkParameterNeeded = true;
                                break;
                            case "HelperFunctionName":
                                if (!CheckLine(words, 2)) return;
                                helperFunctionName = words[1];
                                break;
                            case "Parameter":
                                if (!CheckLine(words, 6)) return;
                                Parameter pa = new Parameter
                                {
                                    ParamVariableType = words[1],
                                    ParamName = words[2],
                                    ParamDisplayName = words[3],
                                    ParamFileName = words[4],
                                    ParamType = words[5]
                                };
                                parameterList.Add(pa);
                                break;
                            default:
                                Console.WriteLine("Invalid input: " + words[0]);
                                break;
                        }
                    }
                }
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("Error reading file. Message = {0}", e.Message);
            }
            finally
            {
                //Console.WriteLine("There were {0} lines.", counter);
            }
        }

        

        static bool CheckLine(string[] words, int numParam)
        {
            if (words.Length < numParam)
            {
                Console.WriteLine(words[0] + " cannot be empty.");
                return false;
            }
            return true;
        }
    }
}
