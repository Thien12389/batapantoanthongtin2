#  Báo Cáo Bài Tập: An Toàn và Bảo Mật Thông Tin

**THÔNG TIN SINH VIÊN:**
- **Họ và tên:** Lê Đỗ Hoàng Thiện
- **Mã số sinh viên (MSSV):** K235480106068
- **Lớp:** K59KMT

**THÔNG TIN DỰ ÁN:**
- **Ngôn ngữ lập trình:** C# (.NET 6.0)  
- **Thư viện sử dụng:** `System.Security.Cryptography`, `System.IO`, `System.Diagnostics`

Dự án này là chương trình Console Application thực hiện toàn bộ các quy trình về mã hóa đối xứng (AES), mã hóa bất đối xứng (RSA), băm dữ liệu (SHA256) và đề xuất giải pháp cho chữ ký số động theo đúng yêu cầu đề bài.

---

## 📌 BÀI 1: Hệ Mật Mã Đối Xứng (AES - Advanced Encryption Standard)

Trong phần này, hệ thống sử dụng thuật toán AES với khóa đối xứng (Symmetric Key). Nghĩa là chung một khóa sẽ được dùng cho cả hai quy trình Mã hóa (Encryption) và Giải mã (Decryption). Quá trình thử nghiệm bao gồm: Khởi tạo tự động Key và IV (Initialization Vector), xuất khóa ra file nhị phân (`.bin`), nạp lại khóa và thực thi với các luồng dữ liệu lớn.

###  Đánh giá và Kết luận thực tiễn:
- **Khả năng xử lý file dung lượng lớn:** Thuật toán AES thể hiện sức mạnh mã hóa khối vượt trội. Khi kết hợp cùng thư viện `CryptoStream` của .NET để đọc ghi theo dạng luồng (chunking), chương trình có thể mã hóa các tệp có kích thước hàng GB mà không gây tràn bộ nhớ (RAM). Thời gian xử lý cho tệp thử nghiệm lớn vô cùng ấn tượng, thường chỉ hoàn tất trong phạm vi vài phần trăm giây.
- **Tương quan tốc độ (Encrypt vs Decrypt):** Bằng việc sử dụng lớp `Stopwatch` đo thời gian (Ticks/Milliseconds), số liệu thu được cho thấy thời gian dùng để biến đổi bản rõ thành bản mã và thời gian khôi phục bản gốc là xấp xỉ nhau, độ trễ gần như không đáng kể. 
- **Bảo toàn dữ liệu tuyệt đối:** Quá trình quét và so sánh từng byte (Byte-by-byte comparison) giữa tệp nguồn ban đầu và tệp sau khi đã giải mã trả về kết quả **KHỚP HOÀN TOÀN**. Không có bất kỳ sự hao hụt hay sai lệch bit nào xảy ra trong suốt tiến trình mã hóa.

> ** Minh chứng kết quả chạy thực tế - Bài 1:**
> 
> <img width="1920" height="1080" alt="Ảnh chụp màn hình 2026-10-26 132622" src="https://github.com/user-attachments/assets/9d7dec76-3470-422f-8e61-eb2be8d9bd30" />


---

## 📌 BÀI 2: Hệ Mật Mã Bất Đối Xứng (RSA), Hàm Băm và Chữ Ký Số

Phần này áp dụng chuẩn mã hóa bất đối xứng RSA (Khóa 2048-bit), nơi mã hóa và giải mã sử dụng hai khóa khác nhau (Public Key & Private Key). Khóa được lưu trữ và truy xuất thông qua định dạng cấu trúc chuẩn PEM.

###  1. Đánh giá Tốc độ thuật toán RSA:
- **Tốc độ Mã hóa (Sử dụng Public Key):** Thời gian thực thi rất ngắn gọn. Khóa công khai sở hữu thành phần số mũ công khai $e$ thường được gán mặc định là giá trị $65537$ (nhị phân chỉ gồm hai bit 1). Do $e$ nhỏ, phép toán lũy thừa module $C = M^e \bmod n$ được thực hiện cực nhanh.
- **Tốc độ Giải mã (Sử dụng Private Key):** Thời gian xử lý chậm hơn rất nhiều so với lúc mã hóa. Khóa bí mật phụ thuộc vào thành phần $d$ (được tính bằng hàm số Euler, $d \equiv e^{-1} \pmod{\phi(n)}$). $d$ là một số nguyên vô cùng lớn, kéo theo số vòng lặp để thực hiện phép giải mã $M = C^d \bmod n$ tăng vọt, tiêu tốn rất nhiều chu kỳ CPU.

###  2. Tính chất của Hàm băm (Hash) SHA-256:
Chương trình tiến hành băm 2 chuỗi kiểm thử là `"Data123"` và `"Data124"`. 
- **Kết luận thu được:** Đầu vào dù chỉ bị thay đổi một lượng cực kỳ nhỏ (**duy nhất 1 ký tự cuối**), mã băm ngõ ra vẫn biến đổi hoàn toàn thành một chuỗi Hexadecimal xa lạ, không hề có quy luật liên quan tới mã ban đầu. Điều này minh chứng cực kỳ sắc nét cho **Hiệu ứng tuyết lở (Avalanche Effect)** - đặc tính bắt buộc phải có để tạo ra hàm băm mật mã an toàn, chống lại sự suy luận cấu trúc chuỗi ban đầu.

---

###  3. ĐỀ XUẤT GIẢI PHÁP: Kỹ thuật Chữ ký số động cho dữ liệu cố định

** Câu hỏi tình huống:**  
Mong muốn đưa ra là: *Cùng một dữ liệu*, *cùng một người ký* (nghĩa là sử dụng chung một Private Key), thì làm thế nào để chữ ký số sinh ra **BẮT BUỘC PHẢI KHÁC NHAU** trong mỗi lần ký thực thi, nhưng bên nhận vẫn phải Verify (xác thực) thành công?

** Phương pháp giải quyết:**  
Sử dụng tiêu chuẩn đệm **RSA-PSS (Probabilistic Signature Scheme)** để thay thế cho cơ chế mã hóa xác định cũ (PKCS#1 v1.5). 

**Cơ chế cốt lõi của giải pháp:**
1. **Bổ sung Nhiễu (Salt):** Mỗi khi hàm ký số được gọi, chuẩn đệm RSA-PSS sẽ yêu cầu hệ thống tự động sinh ra một lượng bit ngẫu nhiên gọi là *Salt*.
2. **Ký trên Hash trộn:** Chuỗi Salt ngẫu nhiên này được trộn trực tiếp vào mã băm (Hash) của dữ liệu. Do đó, hàm băm cuối cùng được dùng để đưa vào máy biến đổi khóa bí mật RSA luôn là một chuỗi mới tinh.
3. **Kết quả đạt được:** Sự kết hợp hoàn hảo này khiến chữ ký xuất ra sẽ hoàn toàn khác biệt sau vô số lần ký lặp lại (ngay cả trên cùng một tệp tĩnh). 
4. **Xác minh (Verify):** Mặc dù chữ ký liên tục thay đổi hình thái, nhưng do cấu trúc PSS đã được tiêu chuẩn hóa, Public Key của người nhận vẫn có khả năng trích xuất ngược chuỗi Salt và đối chiếu chính xác với mã băm ban đầu.  
=> Cơ chế này tăng tính bảo mật lên nhiều lần, ngăn chặn tuyệt đối các cuộc tấn công suy diễn phân tích chữ ký hoặc phát lại (Replay Attacks).

> ** Minh chứng kết quả (Tốc độ RSA, Hiệu ứng SHA256 và Chữ ký PSS):**
> 
> <img width="1920" height="1080" alt="Ảnh chụp màn hình 2026-10-26 132346" src="https://github.com/user-attachments/assets/64b15765-c488-44c1-b295-8c7be79d6e9b" />


---
---

## KẾT LUẬN TỔNG QUAN DỰ ÁN

Báo cáo và chương trình Console Application đã hoàn thành **100% các yêu cầu kỹ thuật** của bài tập lớn An Toàn và Bảo Mật Thông Tin với các kết quả thực nghiệm nổi bật:

1. **Hệ mật mã đối xứng (AES-256):** 
   * Áp dụng thành công cơ chế đọc/ghi luồng (`CryptoStream`) giúp mã hóa/giải mã các tệp tin dung lượng lớn với tốc độ cao (~270 – 480 MB/s) mà không gây tràn bộ nhớ RAM. 
   * Tiến trình giải mã khôi phục chính xác **100% từng byte** so với dữ liệu nguồn ban đầu.

2. **Hệ mật mã bất đối xứng (RSA-2048) & SHA-256:**
   * **Tốc độ:** Thực nghiệm chứng minh thao tác với Khóa công khai (Public Key - $e=65537$) diễn ra cực nhanh, trong khi thao tác với Khóa bí mật (Private Key - $d$ lớn) tốn nhiều chu kỳ CPU hơn rõ rệt.
   * **Tính chất Hàm băm:** SHA-256 thể hiện rõ nét **Hiệu ứng tuyết lở (Avalanche Effect)** khi thay đổi duy nhất 1 ký tự đầu vào làm biến đổi **52.7% số bit** của mã băm ngõ ra (đạt chuẩn lý thuyết $\approx 50\%$).

3. **Chữ ký số động (RSA-PSS):**
   * Giải quyết triệt để yêu cầu tình huống bằng chuẩn đệm **RSA-PSS**. Nhờ cơ chế tự động nhúng bit nhiễu ngẫu nhiên (**Salt**), chương trình tạo ra các chuỗi chữ ký hoàn toàn khác nhau cho mỗi lần ký trên cùng một dữ liệu tĩnh, ngăn chặn triệt để tấn công phát lại (Replay Attacks) mà bên nhận vẫn Verify thành công bằng Public Key.

> **Tóm lại:** Dự án không chỉ đáp ứng chuẩn xác các thuật toán mật mã tiêu chuẩn trong .NET (`System.Security.Cryptography`) mà còn đưa ra các đánh giá thực nghiệm minh bạch, có số liệu đo đạc và hình ảnh minh chứng cụ thể.
*Báo cáo được hoàn thiện nhằm mục đích minh chứng bài tập môn An Toàn & Bảo Mật Thông Tin.*
