using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace Venus.Calculate {
    [Serializable]
    public class CodeDomCalculator : MarshalByRefObject {
        private readonly string expression;
        private readonly CSharpCodeProvider codeProvider;

        public CodeDomCalculator(string expression) {
            codeProvider = new CSharpCodeProvider();
            this.expression = expression;
        }

        private static CompilerParameters CreateCompilerParameters() {
            var compilerParams = new CompilerParameters
                                     {
                                         CompilerOptions = "/target:library /optimize",
                                         GenerateExecutable = false,
                                         GenerateInMemory = true,
                                         IncludeDebugInformation = false
                                     };
            compilerParams.ReferencedAssemblies.Add("System.dll");
            return compilerParams;
        }

        private CompilerResults CompileAssembly(string source) {
            CompilerParameters parms = CreateCompilerParameters();
            return codeProvider.CompileAssemblyFromSource(parms, source);
        }

        public string DoIt() {
            string source = BuildClass();
            CompilerResults results = CompileAssembly(source);
            if (results == null || results.Errors.Count != 0 || results.CompiledAssembly == null) return "";
            return RunCode(results);
        }

        private static string RunCode(CompilerResults results) {
            Assembly executingAssembly = results.CompiledAssembly;
            object assemblyInstance = executingAssembly.CreateInstance("ExpressionEvaluator.Calculator");
            return assemblyInstance.GetType().GetMethod("Calculate").Invoke(assemblyInstance, new object[] {}).ToString();
        }

        private string BuildClass() {
            var source = new StringBuilder();
            var sw = new StringWriter(source);
            var options = new CodeGeneratorOptions();
            var myNamespace = new CodeNamespace("ExpressionEvaluator");
            myNamespace.Imports.Add(new CodeNamespaceImport("System"));
            var classDeclaration = new CodeTypeDeclaration {IsClass = true, Name = "Calculator", Attributes = MemberAttributes.Public};
            var myMethod = new CodeMemberMethod
                               {Name = "Calculate", ReturnType = new CodeTypeReference(typeof (double)), Attributes = MemberAttributes.Public};
            myMethod.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression(expression)));
            classDeclaration.Members.Add(myMethod);
            myNamespace.Types.Add(classDeclaration);
            codeProvider.GenerateCodeFromNamespace(myNamespace, sw, options);
            sw.Flush();
            sw.Close();
            return source.ToString();
        }
    }
}