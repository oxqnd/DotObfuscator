using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Text;

namespace DotObfuscator_Framework
{
    public static class MethodEncryptor
    {
        public static void Apply(ModuleDefinition module)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (method == null || !method.HasBody || method.Body?.Instructions == null)
                        continue;

                    if (method.IsConstructor || method.Body.Instructions.Count <= 10)
                        continue;

                    ILProcessor proc;
                    try
                    {
                        proc = method.Body.GetILProcessor();
                    }
                    catch
                    {
                        Console.WriteLine($"[!] ILProcessor 생성 실패: {method.FullName}");
                        continue;
                    }

                    try
                    {
                        Console.WriteLine($"[DEBUG] 암호화 중: {method.FullName}");
                        var il = method.Body.Instructions;
                        var sb = new StringBuilder();

                        foreach (var instr in il)
                            sb.Append(instr.OpCode.Code).Append(";");

                        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
                        method.Body.Instructions.Clear();
                        proc.Append(proc.Create(OpCodes.Ldstr, "[EncryptedMethod:" + encoded + "]"));
                        proc.Append(proc.Create(OpCodes.Pop));
                        proc.Append(proc.Create(OpCodes.Ret));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[!] 메서드 암호화 실패: {method?.FullName} - {ex.Message}");
                    }
                }
            }
        }
    }
}
