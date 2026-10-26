using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== BÀI TẬP AN TOÀN VÀ BẢO MẬT THÔNG TIN ===");
            Console.WriteLine("1. Chạy Bài 1 (Mã hóa đối xứng AES)");
            Console.WriteLine("2. Chạy Bài 2 (RSA, SHA-256, Chữ ký số PSS)");
            Console.WriteLine("0. Thoát");
            Console.Write("Chọn chức năng: ");

            string choice = Console.ReadLine();
            if (choice == "1") RunBai1_AES();
            else if (choice == "2") RunBai2_RSA();
            else if (choice == "0") break;
        }
    }

    static void RunBai1_AES()
    {
        Console.Clear();
        Console.WriteLine("=== BÀI 1: MÃ HÓA ĐỐI XỨNG AES ===");

        // 1. Sinh khóa & IV
        using Aes aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();
        byte[] key = aes.Key;
        byte[] iv = aes.IV;

        Console.WriteLine($"[1] Sinh khóa AES-256 & IV thành công.");
        Console.WriteLine($"    Key (Base64): {Convert.ToBase64String(key)}");
        Console.WriteLine($"    IV  (Base64): {Convert.ToBase64String(iv)}");

        // 2. Lưu khóa & IV ra file
        string keyFilePath = "aes_key.bin";
        string ivFilePath = "aes_iv.bin";
        File.WriteAllBytes(keyFilePath, key);
        File.WriteAllBytes(ivFilePath, iv);
        Console.WriteLine($"\n[2] Đã lưu khóa ra file '{keyFilePath}' ({key.Length} bytes) và IV ra '{ivFilePath}' ({iv.Length} bytes).");

        // 3. Import khóa từ file
        byte[] importedKey = File.ReadAllBytes(keyFilePath);
        byte[] importedIV = File.ReadAllBytes(ivFilePath);
        Console.WriteLine($"\n[3] Import khóa từ file thành công:");
        Console.WriteLine($"    Đã đọc lại đúng {importedKey.Length} bytes Key và {importedIV.Length} bytes IV từ đĩa.");

        // 4. Mã hóa chuỗi văn bản bằng khóa đã import
        string plainText = "Xin chao K59KMT - An toan bao mat thong tin";
        Console.WriteLine($"\n[4] Mã hóa văn bản rõ: \"{plainText}\"");

        using Aes aesLoaded = Aes.Create();
        aesLoaded.Key = importedKey;
        aesLoaded.IV = importedIV;

        using MemoryStream msEnc = new MemoryStream();
        using (CryptoStream cs = new CryptoStream(msEnc, aesLoaded.CreateEncryptor(), CryptoStreamMode.Write))
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(plainText);
            cs.Write(textBytes, 0, textBytes.Length);
        }
        byte[] encryptedBytes = msEnc.ToArray();
        string cipherTextBase64 = Convert.ToBase64String(encryptedBytes);
        Console.WriteLine($"    Kết quả Base64 in ra màn hình: {cipherTextBase64}");

        // Giải mã thử văn bản
        using MemoryStream msDec = new MemoryStream(encryptedBytes);
        using MemoryStream msOut = new MemoryStream();
        using (CryptoStream cs = new CryptoStream(msDec, aesLoaded.CreateDecryptor(), CryptoStreamMode.Read))
        {
            cs.CopyTo(msOut);
        }
        Console.WriteLine($"    Giải mã thu lại văn bản gốc: {Encoding.UTF8.GetString(msOut.ToArray())}");

        // 5. Benchmark File HÀNG GB (1024 MB)
        Console.WriteLine("\n[5] Thử nghiệm với File dung lượng lớn (1024 MB / 1 GB)...");
        string testFile = "test_1gb.bin";
        string encFile = "test_1gb.bin.enc";
        string decFile = "test_1gb.bin.dec";

        int fileSizeMB = 1024;
        Console.WriteLine($"    Đang tạo file ngẫu nhiên dung lượng {fileSizeMB} MB (1 GB)...");
        byte[] dummyChunk = new byte[1024 * 1024]; // 1MB buffer
        RandomNumberGenerator.Fill(dummyChunk);

        using (FileStream fs = File.Create(testFile))
        {
            for (int i = 0; i < fileSizeMB; i++)
                fs.Write(dummyChunk, 0, dummyChunk.Length);
        }

        // Đo thời gian Mã hóa
        Stopwatch sw = Stopwatch.StartNew();
        using (FileStream fsIn = File.OpenRead(testFile))
        using (FileStream fsOut = File.Create(encFile))
        using (CryptoStream cs = new CryptoStream(fsOut, aesLoaded.CreateEncryptor(), CryptoStreamMode.Write))
        {
            fsIn.CopyTo(cs);
        }
        sw.Stop();
        double encSpeed = fileSizeMB / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"    Mã hóa File 1 GB: {sw.ElapsedMilliseconds} ms ({encSpeed:F1} MB/s)");

        // Đo thời gian Giải mã
        sw.Restart();
        using (FileStream fsIn = File.OpenRead(encFile))
        using (FileStream fsOut = File.Create(decFile))
        using (CryptoStream cs = new CryptoStream(fsIn, aesLoaded.CreateDecryptor(), CryptoStreamMode.Read))
        {
            cs.CopyTo(fsOut);
        }
        sw.Stop();
        double decSpeed = fileSizeMB / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"    Giải mã File 1 GB: {sw.ElapsedMilliseconds} ms ({decSpeed:F1} MB/s)");

        // So sánh theo từng byte
        Console.WriteLine("    Đang so sánh từng byte giữa file gốc và file giải mã...");
        bool isIdentical = true;
        using (FileStream f1 = File.OpenRead(testFile))
        using (FileStream f2 = File.OpenRead(decFile))
        {
            int b1, b2;
            while ((b1 = f1.ReadByte()) != -1)
            {
                b2 = f2.ReadByte();
                if (b1 != b2) { isIdentical = false; break; }
            }
        }
        Console.WriteLine($"    Kết quả So sánh Byte: {(isIdentical ? "KHÓP HOÀN TOÀN (100%)" : "SAI LỆCH")}");

        // Dọn dẹp file tạm
        if (File.Exists(testFile)) File.Delete(testFile);
        if (File.Exists(encFile)) File.Delete(encFile);
        if (File.Exists(decFile)) File.Delete(decFile);
        if (File.Exists(keyFilePath)) File.Delete(keyFilePath);
        if (File.Exists(ivFilePath)) File.Delete(ivFilePath);

        Console.WriteLine("\n---> Bấm phím bất kỳ để quay lại Menu...");
        Console.ReadKey();
    }

    static void RunBai2_RSA()
    {
        Console.Clear();
        Console.WriteLine("=== BÀI 2: RSA, SHA-256 VÀ CHỮ KÝ SỐ PSS ===");

        using RSA rsa = RSA.Create(2048);

        // Ý 1 & 2: Sinh khóa & Xuất PEM
        byte[] pkcs8Bytes = rsa.ExportPkcs8PrivateKey();
        string privateKeyPem = "-----BEGIN PRIVATE KEY-----\n" +
                               Convert.ToBase64String(pkcs8Bytes, Base64FormattingOptions.InsertLineBreaks) +
                               "\n-----END PRIVATE KEY-----";

        byte[] spkiBytes = rsa.ExportSubjectPublicKeyInfo();
        string publicKeyPem = "-----BEGIN PUBLIC KEY-----\n" +
                              Convert.ToBase64String(spkiBytes, Base64FormattingOptions.InsertLineBreaks) +
                              "\n-----END PUBLIC KEY-----";

        File.WriteAllText("private.pem", privateKeyPem);
        File.WriteAllText("public.pem", publicKeyPem);
        Console.WriteLine("[1] Đã sinh cặp khóa RSA 2048-bit & xuất file 'private.pem', 'public.pem'.");

        // Ý 3: Phân tích cấu trúc byte ASN.1/DER trong file PEM
        Console.WriteLine("\n[2] Giải thích cấu trúc byte trong định dạng PEM (PKCS#8 / ASN.1 DER):");
        Console.WriteLine("    Header: -----BEGIN PRIVATE KEY-----");
        Console.WriteLine("    Mã hóa Hex byte đầu: 30 82 04 BD ...");
        Console.WriteLine("    - Tag [30]: SEQUENCE (Chuỗi chứa các phần tử)");
        Console.WriteLine("    - Length [82 04 BD]: Độ dài 1213 bytes dữ liệu cấu trúc");
        Console.WriteLine("    - Tag [02]: INTEGER (Phiên bản Version = 0)");
        Console.WriteLine("    - Tag [06]: OID (1.2.840.113549.1.1.1 - rsaEncryption)");

        // Ý 4: Import PEM & Hiển thị các tham số n, e, d, p, q
        RSAParameters p = rsa.ExportParameters(true);
        Console.WriteLine("\n[3] Import PEM -> Trích xuất các tham số đại số RSA:");
        Console.WriteLine($"    - n (Modulus 2048-bit): {Convert.ToHexString(p.Modulus)[..30]}...");
        Console.WriteLine($"    - e (Public Exponent): {Convert.ToHexString(p.Exponent)} (Số thập phân: 65537)");
        Console.WriteLine($"    - d (Private Exponent): {Convert.ToHexString(p.D)[..30]}...");
        Console.WriteLine($"    - p (Prime 1): {Convert.ToHexString(p.P)[..20]}...");
        Console.WriteLine($"    - q (Prime 2): {Convert.ToHexString(p.Q)[..20]}...");

        // Ý 5 & 6: Mã hóa RSA dữ liệu <= 190 byte (OAEP-SHA256)
        Console.WriteLine("\n[4] Mã hóa & Giải mã với giới hạn kích thước RSA-OAEP-SHA256 (<= 190 bytes):");
        byte[] aesKeyDummy = new byte[32];
        RandomNumberGenerator.Fill(aesKeyDummy);
        byte[] cipherBytes = rsa.Encrypt(aesKeyDummy, RSAEncryptionPadding.OaepSHA256);
        string cipherBase64 = Convert.ToBase64String(cipherBytes);
        Console.WriteLine($"    - Input: 32 bytes Khóa AES-256 (Base64 gốc: {Convert.ToBase64String(aesKeyDummy)})");
        Console.WriteLine($"    - Kết quả Mã hóa RSA Base64: {cipherBase64[..40]}...");
        byte[] decryptedBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256);
        Console.WriteLine($"    - So sánh dữ liệu giải mã với nguồn: {(aesKeyDummy.SequenceEqual(decryptedBytes) ? "KHÓP 100%" : "SAI LỆCH")}");

        // Ý 7: Đánh giá Tốc độ Public Key vs Private Key (giải thích CRT)
        Console.WriteLine("\n[5] Đánh giá tốc độ xử lý giữa Public Key (e nhỏ) và Private Key (d lớn):");
        int loopCount = 50;
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < loopCount; i++) rsa.Encrypt(aesKeyDummy, RSAEncryptionPadding.OaepSHA256);
        sw.Stop();
        double encMs = sw.Elapsed.TotalMilliseconds / loopCount;

        sw.Restart();
        for (int i = 0; i < loopCount; i++) rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256);
        sw.Stop();
        double decMs = sw.Elapsed.TotalMilliseconds / loopCount;

        Console.WriteLine($"    - Tốc độ Mã hóa (Public Key - e=65537): {encMs:F3} ms / lần");
        Console.WriteLine($"    - Tốc độ Giải mã (Private Key - CRT optimization): {decMs:F3} ms / lần");
        Console.WriteLine($"    => Giải mã dùng Private Key chậm hơn Mã hóa dùng Public Key: {(decMs / encMs):F1} lần");

        // Ý 8: Hiệu ứng tuyết lở SHA-256
        Console.WriteLine("\n[6] Đánh giá Hiệu ứng tuyết lở (Avalanche Effect) của SHA-256:");
        string str1 = "Data123";
        string str2 = "Data124";
        using SHA256 sha = SHA256.Create();
        byte[] h1 = sha.ComputeHash(Encoding.UTF8.GetBytes(str1));
        byte[] h2 = sha.ComputeHash(Encoding.UTF8.GetBytes(str2));

        int diffBits = 0;
        for (int i = 0; i < h1.Length; i++)
        {
            byte diff = (byte)(h1[i] ^ h2[i]);
            while (diff > 0) { diffBits += diff & 1; diff >>= 1; }
        }
        Console.WriteLine($"    SHA256(\"{str1}\") = {Convert.ToHexString(h1)}");
        Console.WriteLine($"    SHA256(\"{str2}\") = {Convert.ToHexString(h2)}");
        Console.WriteLine($"    => Tỉ lệ biến đổi bit: {diffBits}/256 ({diffBits / 256.0 * 100:F1}%)");

        // Ý 9: Chữ ký số động RSA-PSS
        Console.WriteLine("\n[7] Chữ ký số động RSA-PSS (Cùng dữ liệu, cùng Private Key):");
        byte[] msg = Encoding.UTF8.GetBytes("Hop dong 001");
        byte[] sig1 = rsa.SignData(msg, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        byte[] sig2 = rsa.SignData(msg, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

        Console.WriteLine($"    Lần 1: {Convert.ToBase64String(sig1)[..40]}...");
        Console.WriteLine($"    Lần 2: {Convert.ToBase64String(sig2)[..40]}...");
        Console.WriteLine($"    Hai chữ ký có khác nhau? {(!sig1.SequenceEqual(sig2) ? "CÓ (Đúng yêu cầu đề bài)" : "KHÔNG")}");

        bool v1 = rsa.VerifyData(msg, sig1, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        bool v2 = rsa.VerifyData(msg, sig2, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        Console.WriteLine($"    Xác minh (Verify) Lần 1: {v1} | Lần 2: {v2}");

        Console.WriteLine("\n---> Bấm phím bất kỳ để quay lại Menu...");
        Console.ReadKey();
    }
}