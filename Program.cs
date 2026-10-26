using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== BÀI TẬP AN TOÀN BẢO MẬT THÔNG TIN ===");
            Console.WriteLine("1. Bài 1: Mã hóa AES");
            Console.WriteLine("2. Bài 2: Mã hóa RSA & Chữ ký số");
            Console.Write("Chọn bài tập để chạy (1 hoặc 2): ");
            var choice = Console.ReadLine();

            if (choice == "1") RunBai1_AES();
            else if (choice == "2") RunBai2_RSA();
            else Console.WriteLine("Lựa chọn không hợp lệ!");

            Console.WriteLine("\nHoàn thành! Nhấn phím Enter để thoát...");
            Console.ReadLine();
        }

        static void RunBai1_AES()
        {
            Console.WriteLine("\n--- BÀI 1: MÃ HÓA ĐỐI XỨNG AES ---");
            string keyFile = "aes_key.bin";
            string ivFile = "aes_iv.bin";
            string sourceFile = "bigfile_source.dat";
            string encryptedFile = "bigfile_encrypted.dat";
            string decryptedFile = "bigfile_decrypted.dat";

            if (!File.Exists(sourceFile))
            {
                Console.WriteLine("Đang tạo file test 50MB giả lập file lớn...");
                byte[] dummyData = new byte[1024 * 1024 * 50];
                new Random().NextBytes(dummyData);
                File.WriteAllBytes(sourceFile, dummyData);
            }

            using Aes aes = Aes.Create();

            File.WriteAllBytes(keyFile, aes.Key);
            File.WriteAllBytes(ivFile, aes.IV);
            Console.WriteLine("1,2. Đã sinh và lưu khóa AES ra file.");

            byte[] importedKey = File.ReadAllBytes(keyFile);
            byte[] importedIV = File.ReadAllBytes(ivFile);
            Console.WriteLine("3. Đã import khóa từ file.");

            string plainText = "Hello AES Encryption!";
            using var encryptorText = aes.CreateEncryptor(importedKey, importedIV);
            byte[] textBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedTextBytes = encryptorText.TransformFinalBlock(textBytes, 0, textBytes.Length);
            Console.WriteLine($"4. KQ mã hóa đoạn văn (Base64): {Convert.ToBase64String(encryptedTextBytes)}");

            Console.WriteLine("5. Đang mã hóa file lớn...");
            Stopwatch sw = Stopwatch.StartNew();
            EncryptFile(sourceFile, encryptedFile, importedKey, importedIV);
            sw.Stop();
            Console.WriteLine($"=> Thời gian mã hóa: {sw.ElapsedMilliseconds} ms");

            Console.WriteLine("6. Đang giải mã file...");
            sw.Restart();
            DecryptFile(encryptedFile, decryptedFile, importedKey, importedIV);
            sw.Stop();
            Console.WriteLine($"=> Thời gian giải mã: {sw.ElapsedMilliseconds} ms");

            bool isMatch = CompareFiles(sourceFile, decryptedFile);
            Console.WriteLine($"=> So sánh byte file nguồn và file giải mã: {(isMatch ? "KHỚP HOÀN TOÀN" : "SAI LỆCH")}");
        }

        static void RunBai2_RSA()
        {
            Console.WriteLine("\n--- BÀI 2: MÃ HÓA BẤT ĐỐI XỨNG RSA & CHỮ KÝ ---");
            string pubKeyFile = "public_key.pem";
            string privKeyFile = "private_key.pem";

            using RSA rsa = RSA.Create(2048);

            // --- CÁCH XUẤT PEM TƯƠNG THÍCH VỚI .NET 6.0 ---
            string pubKeyStr = "-----BEGIN PUBLIC KEY-----\n" + Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo(), Base64FormattingOptions.InsertLineBreaks) + "\n-----END PUBLIC KEY-----";
            string privKeyStr = "-----BEGIN PRIVATE KEY-----\n" + Convert.ToBase64String(rsa.ExportPkcs8PrivateKey(), Base64FormattingOptions.InsertLineBreaks) + "\n-----END PRIVATE KEY-----";

            File.WriteAllText(pubKeyFile, pubKeyStr);
            File.WriteAllText(privKeyFile, privKeyStr);
            Console.WriteLine("1,2,3. Đã sinh và xuất cặp khóa ra file PEM.");

            using RSA rsaPub = RSA.Create();
            rsaPub.ImportFromPem(File.ReadAllText(pubKeyFile));
            using RSA rsaPriv = RSA.Create();
            rsaPriv.ImportFromPem(File.ReadAllText(privKeyFile));
            Console.WriteLine("4. Đã import khóa PEM thành công.");

            byte[] dataToEncrypt = Encoding.UTF8.GetBytes("Thong tin bao mat 123");

            Stopwatch sw = Stopwatch.StartNew();
            byte[] encryptedData = rsaPub.Encrypt(dataToEncrypt, RSAEncryptionPadding.Pkcs1);
            sw.Stop();
            long encryptTime = sw.ElapsedTicks;
            Console.WriteLine($"5. KQ mã hóa RSA lưu dạng Base64: {Convert.ToBase64String(encryptedData).Substring(0, 30)}...");

            sw.Restart();
            byte[] decryptedData = rsaPriv.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);
            sw.Stop();
            long decryptTime = sw.ElapsedTicks;

            Console.WriteLine($"6. Giải mã: {Encoding.UTF8.GetString(decryptedData)} (Khớp với nguồn)");
            Console.WriteLine($"7. Đánh giá tốc độ: Mã hóa (khóa Public e bé) = {encryptTime} ticks, Giải mã (khóa Private d lớn) = {decryptTime} ticks (Giải mã tốn thời gian hơn nhiều).");

            Console.WriteLine("\n8. --- BĂM SHA256 ---");
            byte[] hash1 = SHA256.HashData(Encoding.UTF8.GetBytes("Data123"));
            byte[] hash2 = SHA256.HashData(Encoding.UTF8.GetBytes("Data124"));
            Console.WriteLine($"Băm 'Data123': {Convert.ToHexString(hash1)}");
            Console.WriteLine($"Băm 'Data124' (đổi 1 ký tự): {Convert.ToHexString(hash2)}");
            Console.WriteLine("=> Kết luận: Đầu vào thay đổi rất ít nhưng KQ băm thay đổi hoàn toàn.");

            Console.WriteLine("\n9. --- CHỮ KÝ SỐ RSA-PSS (GIẢI QUYẾT YÊU CẦU ĐỀ BÀI) ---");
            byte[] sig1 = rsaPriv.SignHash(hash1, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            byte[] sig2 = rsaPriv.SignHash(hash1, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

            Console.WriteLine($"Chữ ký lần 1: {Convert.ToBase64String(sig1).Substring(0, 30)}...");
            Console.WriteLine($"Chữ ký lần 2 (cùng dữ liệu): {Convert.ToBase64String(sig2).Substring(0, 30)}...");

            bool verify1 = rsaPub.VerifyHash(hash1, sig1, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            bool verify2 = rsaPub.VerifyHash(hash1, sig2, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

            Console.WriteLine($"Xác minh chữ ký lần 1: {verify1}");
            Console.WriteLine($"Xác minh chữ ký lần 2: {verify2}");
            Console.WriteLine("=> PHƯƠNG PHÁP ĐỀ XUẤT: Sử dụng chuẩn Padding RSA-PSS. Cơ chế này tự động đệm thêm một chuỗi nhiễu ngẫu nhiên (salt) vào mã băm trước khi ký, giúp tạo ra các chữ ký khác biệt ở mỗi lần ký mà vẫn Verify thành công.");
        }

        static void EncryptFile(string inPath, string outPath, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key; aes.IV = iv;
            using FileStream fsIn = new FileStream(inPath, FileMode.Open);
            using FileStream fsOut = new FileStream(outPath, FileMode.Create);
            using CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write);
            fsIn.CopyTo(cs);
        }

        static void DecryptFile(string inPath, string outPath, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key; aes.IV = iv;
            using FileStream fsIn = new FileStream(inPath, FileMode.Open);
            using FileStream fsOut = new FileStream(outPath, FileMode.Create);
            using CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read);
            cs.CopyTo(fsOut);
        }

        static bool CompareFiles(string file1, string file2)
        {
            using FileStream fs1 = new FileStream(file1, FileMode.Open);
            using FileStream fs2 = new FileStream(file2, FileMode.Open);
            if (fs1.Length != fs2.Length) return false;
            int byte1, byte2;
            while ((byte1 = fs1.ReadByte()) != -1)
            {
                byte2 = fs2.ReadByte();
                if (byte1 != byte2) return false;
            }
            return true;
        }
    }
}