using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DotObfuscator_Framework.Utils
{
    public static class PayloadEmbedder
    {
        public static void Embed(string payloadPath, string nativeLoaderPath)
        {
            if (!File.Exists(payloadPath))
            {
                Console.WriteLine($"❌ 대상 파일을 찾을 수 없습니다: {payloadPath}");
                return;
            }

            byte[] payload = File.ReadAllBytes(payloadPath);

            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.GenerateKey();
                aes.GenerateIV();

                byte[] encrypted;
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(payload, 0, payload.Length);
                    cs.FlushFinalBlock();
                    encrypted = ms.ToArray();
                }

                // 배열을 C# 코드 문자열로 변환
                string FormatByteArray(byte[] data)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("new byte[] { ");
                    sb.Append(string.Join(", ", data.Select(b => "0x" + b.ToString("X2"))));
                    sb.Append(" };\n");
                    return sb.ToString();
                }

                string encryptedCode = FormatByteArray(encrypted);
                string keyCode = FormatByteArray(aes.Key);
                string ivCode = FormatByteArray(aes.IV);

                // NativeLoader.cs 내용 읽기
                string code = File.ReadAllText(nativeLoaderPath);

                // 바이트 배열 삽입 지점 찾기 및 교체
                code = ReplaceBetween(code, "byte[] encryptedPayload =", encryptedCode);
                code = ReplaceBetween(code, "byte[] key =", keyCode);
                code = ReplaceBetween(code, "byte[] iv =", ivCode);

                File.WriteAllText(nativeLoaderPath, code);
                Console.WriteLine("✅ NativeLoader.cs에 payload가 삽입되었습니다.");
            }
        }

        private static string ReplaceBetween(string source, string startMarker, string newArray)
        {
            int startIdx = source.IndexOf(startMarker);
            if (startIdx == -1) return source;

            int firstBrace = source.IndexOf('{', startIdx);
            int lastBrace = source.IndexOf("};", firstBrace);
            if (firstBrace == -1 || lastBrace == -1) return source;

            string before = source.Substring(0, startIdx);
            string after = source.Substring(lastBrace + 2);
            return before + startMarker + " " + newArray + after;
        }
    }
}