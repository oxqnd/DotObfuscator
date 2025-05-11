using Mono.Cecil;
using System.Linq;

namespace DotObfuscator_Framework
{
    public static class AntiDecompiler
    {
        public static void Apply(ModuleDefinition module)
        {
            var type = module.Types.FirstOrDefault(t => !t.IsSpecialName && t.Name != "<Module>");
            if (type == null) return;

            var attrType = new TypeReference("System", "ObsoleteAttribute", module, module.TypeSystem.CoreLibrary);
            var ctor = new MethodReference(".ctor", module.TypeSystem.Void, attrType) { HasThis = true };
            var attr = new CustomAttribute(ctor);
            type.CustomAttributes.Add(attr);
        }
    }
}
