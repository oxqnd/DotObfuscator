using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace DotObfuscator_Framework
{
    /// <summary>
    /// 디버거가 감지되면 경고 메시지를 출력하고 종료는 하지 않음 (안전)
    /// </summary>
    public static class AntiDebugInjector
    {
        public static void Apply(ModuleDefinition module)
        {
            var main = module.EntryPoint;
            if (main == null || !main.HasBody) return;

            var il = main.Body.GetILProcessor();
            var first = main.Body.Instructions[0];

            var isAttached = module.ImportReference(typeof(System.Diagnostics.Debugger).GetProperty("IsAttached").GetGetMethod());
            var writeLine = module.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

            var afterCheck = il.Create(OpCodes.Nop);

            il.InsertBefore(first, il.Create(OpCodes.Call, isAttached));
            il.InsertBefore(first, il.Create(OpCodes.Brfalse_S, afterCheck));
            il.InsertBefore(first, il.Create(OpCodes.Ldstr, "[AntiDebug] 디버거가 감지되었습니다."));
            il.InsertBefore(first, il.Create(OpCodes.Call, writeLine));
            il.InsertBefore(first, afterCheck);
        }
    }
}
