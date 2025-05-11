using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Text;

namespace DotObfuscator_Framework
{
    public static class StringEncryptor
    {
        public static void Apply(ModuleDefinition module)
        {
            var decryptMethod = module.ImportReference(typeof(Utils.Decryptor).GetMethod("DecryptString"));

            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods.Where(m => m.HasBody))
                {
                    var il = method.Body.GetILProcessor();
                    var instructions = method.Body.Instructions;

                    for (int i = 0; i < instructions.Count; i++)
                    {
                        if (instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            string original = (string)instructions[i].Operand;
                            string encrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(original));
                            instructions[i].Operand = encrypted;
                            il.InsertAfter(instructions[i], il.Create(OpCodes.Call, decryptMethod));
                        }
                    }
                }
            }
        }
    }
}
