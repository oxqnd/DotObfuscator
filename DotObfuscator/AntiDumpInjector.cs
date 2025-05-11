using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DotObfuscator_Framework
{
    /// <summary>
    /// 프로세스가 메모리 덤프 당하지 않도록 간단한 더미 코드를 Main에 삽입합니다.
    /// </summary>
    public static class AntiDumpInjector
    {
        public static void Apply(ModuleDefinition module)
        {
            var main = module.EntryPoint;
            if (main == null || !main.HasBody) return;

            var il = main.Body.GetILProcessor();
            var first = main.Body.Instructions[0];

            // 간단한 더미 문자열 삽입
            il.InsertBefore(first, il.Create(OpCodes.Ldstr, "AntiDumpEnabled"));
            il.InsertBefore(first, il.Create(OpCodes.Pop));
        }
    }
}
