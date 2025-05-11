using Mono.Cecil;
using System;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;

namespace DotObfuscator_Framework
{
    /// <summary>
    /// .resources 내 문자열 값을 Base64로 암호화하고 리소스를 다시 삽입합니다.
    /// </summary>
    public static class ResourceEncryptor
    {
        public static void Apply(ModuleDefinition module)
        {
            var embeddedResources = module.Resources
                .OfType<EmbeddedResource>()
                .ToList();

            foreach (var res in embeddedResources)
            {
                var oldStream = res.GetResourceStream();
                using (var reader = new ResourceReader(oldStream))
                {
                    var mem = new MemoryStream();
                    using (var writer = new ResourceWriter(mem))
                    {
                        var dict = reader.Cast<System.Collections.DictionaryEntry>();
                        foreach (var entry in dict)
                        {
                            if (entry.Value is string str)
                            {
                                var encrypted = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
                                writer.AddResource((string)entry.Key, encrypted);
                            }
                            else
                            {
                                writer.AddResource((string)entry.Key, entry.Value);
                            }
                        }

                        writer.Generate();
                        var newBytes = mem.ToArray();
                        var newResource = new EmbeddedResource(
                            res.Name,
                            res.Attributes,
                            new MemoryStream(newBytes)
                        );

                        module.Resources.Remove(res);
                        module.Resources.Add(newResource);
                    }
                }
            }
        }
    }
}
