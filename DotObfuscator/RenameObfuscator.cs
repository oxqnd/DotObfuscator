using Mono.Cecil;
using System;
using System.Linq;

namespace DotObfuscator_Framework
{
    public static class RenameObfuscator
    {
        private static Random rnd = new Random();

        public static void Apply(ModuleDefinition module)
        {
            foreach (var type in module.Types)
            {
                if (!type.IsSpecialName && !type.Name.StartsWith("<"))
                    type.Name = Gen();

                foreach (var method in type.Methods)
                {
                    if (!method.IsConstructor && !method.IsSpecialName && !method.Name.StartsWith("<"))
                        method.Name = Gen();
                }

                foreach (var field in type.Fields)
                {
                    if (!field.IsSpecialName)
                        field.Name = Gen();
                }
            }
        }

        private static string Gen() =>
            "_" + string.Concat(Enumerable.Range(0, 8).Select(_ => (char)rnd.Next('a', 'z' + 1)));
    }
}
