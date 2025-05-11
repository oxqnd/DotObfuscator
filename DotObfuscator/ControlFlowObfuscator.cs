using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace DotObfuscator_Framework
{
    /// <summary>
    /// 첫 줄에 무작위 switch 분기를 삽입해 흐름을 혼란시키는 안전한 흐름 난독화
    /// </summary>
    public static class ControlFlowObfuscator
    {
        public static void Apply(ModuleDefinition module)
        {
            var rnd = new Random();

            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods.Where(m =>
                    m.HasBody &&
                    m.Body.Instructions.Count > 10 &&
                    m != module.EntryPoint))
                {
                    var il = method.Body.GetILProcessor();
                    var first = method.Body.Instructions[0];

                    // switch에 들어갈 더미 label
                    var switchTarget = il.Create(OpCodes.Nop);
                    il.InsertBefore(first, switchTarget);

                    // 난수 + switch + br
                    int r = rnd.Next(0, 1);
                    var ldc = il.Create(OpCodes.Ldc_I4, r);
                    var sw = il.Create(OpCodes.Switch, new[] { switchTarget });
                    var br = il.Create(OpCodes.Br, first);

                    il.InsertBefore(switchTarget, br);
                    il.InsertBefore(br, sw);
                    il.InsertBefore(sw, ldc);
                }
            }
        }
    }
}
