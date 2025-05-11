using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Reflection;

namespace DotObfuscator_Framework
{
    public static class AntiTamper
    {
        public static void Apply(ModuleDefinition module, string originalPath)
        {
            // SHA256 해시 계산
            var hash = ComputeFileHash(originalPath);

            var main = module.EntryPoint;
            if (main == null || !main.HasBody) return;

            var il = main.Body.GetILProcessor();
            var first = main.Body.Instructions[0];

            // 복호화 메서드 import (null 방어 + BindingFlags 명시)
            var getExecMethod = typeof(System.Reflection.Assembly)
                .GetMethod("GetExecutingAssembly", BindingFlags.Public | BindingFlags.Static);

            var locationPropInfo = typeof(System.Reflection.Assembly).GetProperty("Location");
            var readAllBytesMethod = typeof(File).GetMethod("ReadAllBytes", new[] { typeof(string) });
            var sha256CtorInfo = typeof(SHA256Managed).GetConstructor(Type.EmptyTypes);
            var sha256ComputeMethod = typeof(SHA256).GetMethod("ComputeHash", new[] { typeof(byte[]) });
            var sequenceEqualMethod = typeof(Enumerable).GetMethods()
                .FirstOrDefault(m => m.Name == "SequenceEqual" &&
                                     m.GetParameters().Length == 2 &&
                                     m.GetParameters()[0].ParameterType == typeof(byte[]));

            if (getExecMethod == null || locationPropInfo == null || readAllBytesMethod == null ||
                sha256CtorInfo == null || sha256ComputeMethod == null || sequenceEqualMethod == null)
            {
                Console.WriteLine("[!] AntiTamper: 메서드 참조 중 null 발생. 중단합니다.");
                return;
            }

            var getExecutingAssembly = module.ImportReference(getExecMethod);
            var locationProp = module.ImportReference(locationPropInfo.GetGetMethod());
            var readAllBytes = module.ImportReference(readAllBytesMethod);
            var sha256Ctor = module.ImportReference(sha256CtorInfo);
            var sha256Compute = module.ImportReference(sha256ComputeMethod);
            var sequenceEqual = module.ImportReference(sequenceEqualMethod);

            // 해시 바이트 배열 삽입
            var hashField = new FieldDefinition("__tamper_hash", Mono.Cecil.FieldAttributes.Private | Mono.Cecil.FieldAttributes.Static, module.ImportReference(typeof(byte[])));
            module.Types.First().Fields.Add(hashField);

            var cctor = module.Types.First().Methods.FirstOrDefault(m => m.Name == ".cctor");
            if (cctor == null)
            {
                cctor = new MethodDefinition(".cctor", Mono.Cecil.MethodAttributes.Static | Mono.Cecil.MethodAttributes.Private | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName, module.TypeSystem.Void);
                module.Types.First().Methods.Add(cctor);
                cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            var cil = cctor.Body.GetILProcessor();
            cil.Body.Instructions.Insert(0, cil.Create(OpCodes.Ldc_I4, hash.Length));
            cil.Body.Instructions.Insert(1, cil.Create(OpCodes.Newarr, module.TypeSystem.Byte));

            for (int i = 0; i < hash.Length; i++)
            {
                cil.Body.Instructions.Insert(2 + i * 2, cil.Create(OpCodes.Dup));
                cil.Body.Instructions.Insert(3 + i * 2, cil.Create(OpCodes.Ldc_I4, i));
                cil.Body.Instructions.Insert(4 + i * 2, cil.Create(OpCodes.Ldc_I4, hash[i]));
                cil.Body.Instructions.Insert(5 + i * 2, cil.Create(OpCodes.Stelem_I1));
            }

            cil.Body.Instructions.Insert(2 + hash.Length * 2, cil.Create(OpCodes.Stsfld, hashField));

            // EntryPoint 앞에 SHA256 검증 삽입
            il.InsertBefore(first, il.Create(OpCodes.Call, getExecutingAssembly));
            il.InsertBefore(first, il.Create(OpCodes.Callvirt, locationProp));
            il.InsertBefore(first, il.Create(OpCodes.Call, readAllBytes));
            il.InsertBefore(first, il.Create(OpCodes.Newobj, sha256Ctor));
            il.InsertBefore(first, il.Create(OpCodes.Callvirt, sha256Compute));
            il.InsertBefore(first, il.Create(OpCodes.Ldsfld, hashField));
            il.InsertBefore(first, il.Create(OpCodes.Call, sequenceEqual));

            var continueLabel = il.Create(OpCodes.Nop);
            il.InsertBefore(first, il.Create(OpCodes.Brtrue_S, continueLabel));
            il.InsertBefore(first, il.Create(OpCodes.Ldstr, "파일 변조 감지됨"));
            il.InsertBefore(first, il.Create(OpCodes.Call, module.ImportReference(typeof(Environment).GetMethod("FailFast", new[] { typeof(string) }))));
            il.InsertBefore(first, continueLabel);
        }

        private static byte[] ComputeFileHash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha256 = SHA256.Create())
                return sha256.ComputeHash(stream);
        }
    }
}