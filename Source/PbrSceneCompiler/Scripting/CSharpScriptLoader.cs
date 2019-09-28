using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PbrSceneCompiler.Scripting
{
    class CSharpScriptLoader
    {
        public static T Load<T>(string filename, string className)
        {
            /*
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var options = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" });
            options.GenerateExecutable = true;
            options.GenerateInMemory = true;
            CompilerResults results = provider.CompileAssemblyFromSource(options,
                @"using System;
                class Program1 {
                  public static void Main(string[] args) {
                    Console.WriteLine(""Hello World"");
                  }
                }");
            results.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText));
            */

            return default;
        }
    }
}
