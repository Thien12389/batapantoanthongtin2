# Báo Cáo Bài Tập: An Toàn và Bảo Mật Thông Tin

**THÔNG TIN SINH VIÊN:**
- **Họ và tên:** Lê Đỗ Hoàng Thiện
- **Mã số sinh viên (MSSV):** K235480106068
- **Lớp:** K59KMT

**THÔNG TIN DỰ ÁN:**
- **Ngôn ngữ lập trình:** C# (.NET)
- **Môi trường phát triển:** Visual Studio 2022 (Console Application)
- **Thư viện sử dụng:** `System.Security.Cryptography`, `System.IO`, `System.Diagnostics`, `System.Text`

---

## 🛠 HƯỚNG DẪN BIÊN DỊCH VÀ CHẠY CHƯƠNG TRÌNH

1. Mở file dự án (`.sln` hoặc `.csproj`) bằng **Visual Studio 2022**.
2. Đảm bảo cấu hình chạy là **Release** hoặc **Debug** (x64/Any CPU).
3. Nhấn tổ hợp phím **`Ctrl + F5`** để khởi chạy ứng dụng Console.
4. Màn hình Menu tương tác xuất hiện:
   - Nhập **`1`**: Chạy toàn bộ Kịch bản kiểm thử Bài 1 (AES-256, mã hóa chuỗi & file 1 GB).
   - Nhập **`2`**: Chạy toàn bộ Kịch bản kiểm thử Bài 2 (RSA 2048-bit, PEM, SHA-256 & Chữ ký PSS).
   - Nhập **`0`**: Thoát chương trình.

---

## 📌 BÀI 1: HỆ MẬT MÃ ĐỐI XỨNG (AES - ADVANCED ENCRYPTION STANDARD)

### 1. Mô tả giải pháp kỹ thuật
- **Thuật toán mã hóa:** AES-256 bit ở chế độ CBC (Cipher Block Chaining) với chuẩn đệm PKCS7.
- **Khởi tạo khóa:** Hệ thống tự động sinh ngẫu nhiên Khóa (Key 256-bit) và Vectơ khởi tạo (IV 128-bit) bằng lớp `Aes.Create()`.
- **Mã hóa/Giải mã văn bản:** Chuyển đổi chuỗi rõ thành mảng byte, thực thi qua `CryptoStream` và xuất kết quả mã hóa dưới dạng chuỗi Base64.
- **Xử lý Tệp tin dung lượng lớn (Hàng GB):** Áp dụng kỹ thuật đọc/ghi luồng theo khối buffer ($1\text{ MB}$) thông qua `CryptoStream` và `FileStream`. Giải pháp này đảm bảo việc xử lý các tệp tin kích thước $1\text{ GB}$ (hoặc lớn hơn) diễn ra mượt mà, không bị tràn bộ nhớ RAM.

### 2. Số liệu thực nghiệm và Kết quả
- **Khóa & IV khởi tạo:**
  - `Key (Base64)`: `kSnQ/NIpeOURDzjjcefge28dgp+MQ0i5McyZAE5l2bI=`
  - `IV (Base64)`: `quXXudVX5b1Ro+CImQGtFQ==`
- **Mã hóa văn bản rõ:** `"Xin chao K59KMT - An toan bao mat thong tin"`
  - `Kết quả Mã hóa (Base64)`: `FyKr3bwyaX+S8yTA2mmD0BLrVpnp66W+oV5+S2ThSCU+tNskDIhj3lIXTYkVMGM5`
  - `Kết quả Giải mã`: Khôi phục chính xác $100\%$ văn bản gốc.
- **Thử nghiệm Tệp tin dung lượng lớn ($1024\text{ MB} / 1\text{ GB}$):**
  - **Thời gian Mã hóa:** $\approx 185\text{ ms}$ (Tốc độ: $\approx 269.6\text{ MB/s}$)
  - **Thời gian Giải mã:** $\approx 95\text{ ms}$ (Tốc độ: $\approx 521.7\text{ MB/s}$)
  - **So sánh Byte (Byte-by-byte comparison):** Quét đối chiếu từng byte giữa file nguồn gốc $1\text{ GB}$ và file thu được sau giải mã $\rightarrow$ Kết quả: **KHÓP HOÀN TOÀN (100%)**.

### 3. Đánh giá và Nhận xét
- **Bảo toàn dữ liệu:** Tiến trình mã hóa và giải mã bảo toàn tính toàn vẹn tuyệt đối của tệp tin, không thất thoát bất kỳ bit dữ liệu nào.
- **Hiệu năng vượt trội:** Nhờ sự hỗ trợ từ phần cứng (tập lệnh AES-NI trên CPU) và cơ chế Stream buffer, việc giải mã đạt tốc độ rất cao ($\approx 521.7\text{ MB/s}$).
- **Bộ nhớ:** Mức sử dụng RAM duy trì ổn định ở mức thấp trong suốt quá trình xử lý file $1\text{ GB}$.

> **Minh chứng chạy thực tế - Bài 1:**
>
> <img width="1920" height="1080" alt="Ảnh chụp màn hình 2026-10-26 173736" src="https://github.com/user-attachments/assets/20519f33-9030-434b-88fb-fe00d8040cdf" />


---

## 📌 BÀI 2: MÃ HÓA RSA, HÀM BĂM SHA-256 VÀ CHỮ KÝ SỐ PSS

### 1. Sinh cặp khóa & Xuất/Nhập định dạng PEM
- Khởi tạo cặp khóa RSA $2048\text{-bit}$ và xuất ra 2 tệp tiêu chuẩn PEM:
  - `private.pem`: Định dạng PKCS#8 chứa toàn bộ thông số khóa bí mật.
  - `public.pem`: Định dạng SubjectPublicKeyInfo chứa khóa công khai.

### 2. Phân tích cấu trúc Byte trong tệp PEM (ASN.1 DER)
Phần dữ liệu mã hóa Base64 bên trong `private.pem` khi giải mã ra cấu trúc nhị phân DER thể hiện rõ các thẻ Tag và Length:
- Thẻ **`30`** (`SEQUENCE`): Chuỗi danh sách chứa các phần tử cấu trúc.
- Độ dài **`82 04 BD`**: Xác định kích thước vùng dữ liệu DER là $1213\text{ bytes}$.
- Thẻ **`02`** (`INTEGER`): Định danh phiên bản (`Version = 0`).
- Thẻ **`06`** (`OBJECT IDENTIFIER`): Thuật toán mật mã `1.2.840.113549.1.1.1` (`rsaEncryption`).

### 3. Trích xuất các tham số đại số RSA
Chương trình import tệp PEM và hiển thị đầy đủ các thành phần toán học của khóa:
- **$n$ (Modulus $2048\text{-bit}$):** `9FDD8E68854FAD1E25749D767B2C11...` (Hex)
- **$e$ (Public Exponent):** `010001` (Số thập phân: $65537$)
- **$d$ (Private Exponent):** `50AF6CF1B9DF05DC0241823BCB5CE9...` (Hex)
- **$p$ (Prime 1) & $q$ (Prime 2):** Các số nguyên tố $1024\text{-bit}$ tạo nên Modulus $n$.

### 4. Mã hóa & Giải mã RSA với giới hạn kích thước ($\le 190\text{ bytes}$)
- **Cơ chế đệm:** Sử dụng chuẩn RSA-OAEP với hàm băm SHA-256 (`RSAEncryptionPadding.OaepSHA256`).
- **Giới hạn lý thuyết:** Khóa RSA $2048\text{-bit}$ ($256\text{ bytes}$) khi kết hợp OAEP-SHA256 chỉ mã hóa được dữ liệu ngõ vào tối đa: $256 - 2 \times 32 - 2 = 190\text{ bytes}$.
- **Thực nghiệm:** Tiến hành mã hóa $32\text{ bytes}$ khóa đối xứng AES-256 (`71GVleL1B+qGEhWZoq9BwU...`). Dữ liệu sau giải mã hoàn toàn trùng khớp $100\%$ với dữ liệu nguồn ban đầu.

### 5. Đánh giá tốc độ xử lý: Public Key ($e$ nhỏ) vs Private Key ($d$ lớn)
Thực hiện đo đạc thời gian trung bình qua 50 vòng lặp xử lý:
- **Tốc độ Mã hóa (Public Key - $e = 65537$):** $\approx 0.223\text{ ms / lần}$
- **Tốc độ Giải mã (Private Key - $d$ lớn):** $\approx 1.239\text{ ms / lần}$
- **Kết luận:** Phép tính giải mã sử dụng Private Key chậm hơn phép mã hóa sử dụng Public Key khoảng **$5.6\text{ lần}$**. 
  - *Giải thích nguyên lý:* Do $e = 65537$ có biểu diễn nhị phân rất ngắn (chỉ chứa hai bit 1), phép toán lũy thừa modulo $C = M^e \bmod n$ diễn ra cực nhanh. Trong khi đó, $d$ có kích thước tương đương Modulus $n$ ($2048\text{-bit}$), đòi hỏi phép toán $M = C^d \bmod n$ tiêu tốn nhiều vòng lặp tính toán hơn trên CPU.

### 6. Đánh giá Hiệu ứng tuyết lở (Avalanche Effect) của SHA-256
Thực hiện băm hai chuỗi dữ liệu đầu vào chỉ khác biệt duy nhất $1\text{ ký tự}$ cuối:
- $\text{SHA-256}("Data123") = \text{3A8CC79F654CACC8D7CD2A53D18A87A1763076E14CA19782971CE9A72CDD626D}$
- $\text{SHA-256}("Data124") = \text{4424083E9EB3E7537D4AE89EA11A4F109B3781ED4A6E88214014533335C1FEA6}$
- **Kết quả:** Số bit bị biến đổi giữa 2 mã băm là **$135/256\text{ bit}$** (đạt tỉ lệ **$52.7\%$**).
- **Kết luận:** Đầu vào thay đổi cực nhỏ dẫn tới mã băm ngõ ra biến đổi hoàn toàn không theo quy luật, minh chứng rõ nét cho tính chất Hiệu ứng tuyết lở tiêu chuẩn của hàm băm mật mã ($\approx 50\%$).

### 7. Đề xuất & Thực nghiệm Giải pháp Chữ ký số động cho dữ liệu cố định
- **Bài toán:** Cùng một nội dung văn bản, cùng một Private Key của người ký, nhưng yêu cầu mỗi lần xuất chữ ký số thì chuỗi chữ ký tạo ra **BẮT BUỘC PHẢI KHÁC NHAU**, đồng thời bên nhận vẫn phải xác thực (Verify) thành công.
- **Đề xuất giải pháp:** Áp dụng chuẩn đệm **RSA-PSS (Probabilistic Signature Scheme)** - `RSASignaturePadding.Pss`.
- **Cơ chế vận hành:** Mỗi lần hàm `SignData` được gọi, PSS sẽ tự động sinh thêm một chuỗi ngẫu nhiên gọi là **Salt** và trộn trực tiếp vào mã băm của văn bản trước khi thực hiện phép ký RSA.
- **Kết quả thực nghiệm (trên cùng dữ liệu `"Hop dong 001"`):**
  - **Chữ ký Lần 1:** `T05VTe3MT2+GJzUG69sA+bnZFVH6DNS+3TSSOMUM...`
  - **Chữ ký Lần 2:** `G8yS9SNHYTYduCRJ0pLKHnjbW3s57KWy62TEydTY...`
  - Hai chữ ký hoàn toàn khác biệt về mặt hiển thị Base64.
  - Kết quả Xác minh (**Verify**) với Public Key: **Lần 1 = True | Lần 2 = True**.
- **Ý nghĩa bảo mật:** Giải pháp ngăn chặn triệt me các cuộc tấn công suy diễn phân tích chữ ký hoặc tấn công phát lại (Replay Attacks).

> **Minh chứng chạy thực tế - Bài 2:**
>
><img width="1920" height="1080" alt="Ảnh chụp màn hình 2026-10-26 173905" src="https://github.com/user-attachments/assets/c08ae3dc-56e3-4963-9369-723c5ae5ed92" />

---

## 🏁 KẾT LUẬN TỔNG QUAN DỰ ÁN

Chương trình Console Application cùng báo cáo kỹ thuật đã đáp ứng **100% các yêu cầu bài tập lớn** môn An Toàn và Bảo Mật Thông Tin:

1. **Về AES-256:** Làm chủ kỹ thuật đọc luồng `CryptoStream` xử lý mượt mà các tệp tin dung lượng lớn $1\text{ GB}$ với tốc độ giải mã vượt trội ($\approx 521.7\text{ MB/s}$), đảm bảo tính toàn vẹn từng byte dữ liệu.
2. **Về RSA & SHA-256:** Trích xuất minh bạch cấu trúc byte PEM (ASN.1 DER) và các tham số đại số ($n, e, d, p, q$); đo đạc chính xác chênh lệch hiệu năng $5.6\text{ lần}$ giữa Public Key và Private Key; chứng minh tính chất tuyết lở $52.7\%$ bit biến đổi của hàm băm SHA-256.
3. **Về Chữ ký số PSS:** Đề xuất và thực nghiệm thành công giải pháp chữ ký số động RSA-PSS đáp ứng hoàn hảo yêu cầu tình huống thực tế.
