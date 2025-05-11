using Mono.Cecil;
using DotObfuscator_Framework.Utils;
using System;
using System.IO;

namespace DotObfuscator_Framework
{
    public static class Obfuscator
    {
        public static void Run(string inputPath)
        {
            if (!File.Exists(inputPath))
            {
                Console.WriteLine("❌ 입력 파일이 존재하지 않습니다.");
                return;
            }

            string outputPath = "obfuscated_output.exe";

            // 1. 어셈블리 로드
            var assembly = AssemblyDefinition.ReadAssembly(inputPath, new ReaderParameters { ReadWrite = false });
            var module = assembly.MainModule;

            // 2. 난독화 패스 적용
            RenameObfuscator.Apply(module);
            StringEncryptor.Apply(module);
            ControlFlowObfuscator.Apply(module);

            //  3. AntiDebug, AntiTamper 등 필요한 보호 기능 추가 가능
            AntiDebugInjector.Apply(module);
            AntiDumpInjector.Apply(module);
            ResourceEncryptor.Apply(module);
            AntiDecompiler.Apply(module);
            // 4. 결과 저장
            assembly.Write(outputPath);

            Console.WriteLine($"✅ 난독화 완료: {outputPath}");

            // 5. 실행 테스트
            Console.WriteLine("▶ 결과 실행 중...");
            try
            {
                System.Diagnostics.Process.Start(outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ 실행 실패: " + ex.Message);
            }
        }
    }
}