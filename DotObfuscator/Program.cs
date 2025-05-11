using System;
using System.IO;

namespace DotObfuscator_Framework
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== .NET Obfuscator ===\n");

            if (args.Length != 1 || !File.Exists(args[0]))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("▶ 사용법: DotObfuscator.exe <드래그할_대상_파일>\n");
                Console.ResetColor();
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"[+] 입력 파일: {args[0]}");

            try
            {
                Obfuscator.Run(args[0]);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ 예외 발생: " + ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("\n[엔터]를 눌러 종료합니다...");
            Console.ReadLine();
        }
    }
}