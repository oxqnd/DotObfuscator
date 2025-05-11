using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace DotObfuscator_Framework
{
    public static class DummyClassInjector
    {
        public static void Inject(ModuleDefinition module)
        {
            for (int i = 0; i < 30; i++)
            {
                var dummyType = new Mono.Cecil.TypeDefinition(
                    $"FakeNamespace{i}",
                    $"FakeClass{i}",
                    Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class,
                    module.TypeSystem.Object
                );

                for (int j = 0; j < 5; j++)
                {
                    var method = new Mono.Cecil.MethodDefinition(
                        $"DoNothing{j}",
                        Mono.Cecil.MethodAttributes.Public,
                        module.TypeSystem.Void
                    );

                    var il = method.Body.GetILProcessor();
                    il.Append(il.Create(OpCodes.Ret));
                    dummyType.Methods.Add(method);
                }

                module.Types.Add(dummyType);
            }
        }
    }
}
