# Hệ thống Trích xuất Dữ liệu Âm thanh - Hikvision Acoustic Imaging Camera

Dự án này được thiết kế riêng cho dòng Camera Hình ảnh Âm thanh mạng Hikvision (Model: **DS-QAAI264G1-P**), phục vụ mục đích nghiên cứu và theo dõi các chỉ số tiếng ồn (Decibel) và tần số (Frequency) theo thời gian thực.

## 1. Quá trình Nghiên cứu & Phân tích (SDK vs ISAPI)

### Thách thức với SDK truyền thống:
Trong quá trình đọc tài liệu `HCNetSDK.h` và thử nghiệm code C++, chúng tôi nhận thấy:
*   Mã sự kiện âm thanh tiêu chuẩn (`COMM_ALARM_AUDIOEXCEPTION` - `0x1150`) chỉ trả về các chỉ số nguyên đơn giản.
*   Dữ liệu từ SDK không cung cấp độ chính xác phần thập phân và thiếu hoàn toàn chỉ số **Tần số (Hz)** vốn là đặc trưng quan trọng nhất của dòng camera Acoustic.

### Giải pháp tối ưu: ISAPI Alert Stream
Sau khi quét các giao thức hỗ trợ của camera, chúng tôi phát hiện ra luồng dữ liệu **ISAPI** (`/ISAPI/Event/notification/alertStream`) là nguồn cung cấp dữ liệu đầy đủ nhất.
*   **Dữ liệu thô**: Camera trả về định dạng XML liên tục qua HTTP Stream.
*   **Chỉ số bóc tách**: `audioDecibel` (độ chính xác cao) và `frequency` (tần số nguồn âm).

## 2. Giải thuật trong `acoustic_final.py`

File `acoustic_final.py` không chỉ đơn thuần là nhận dữ liệu mà còn chứa các logic xử lý thông minh để đảm bảo dữ liệu "sạch":

1.  **Cơ chế Ghép đôi (Smart Pairing)**: 
    *   Camera gửi dữ liệu rời rạc (gói Decibel đến trước, gói Tần số đến sau). 
    *   Script sử dụng bộ nhớ đệm (Buffer) để đợi và ghép hai gói tin này lại thành một dòng duy nhất trong CSV dựa trên mốc thời gian.
2.  **Bộ lọc nghiêm ngặt (Strict Filter)**:
    *   Loại bỏ hoàn toàn các gói tin "rác" như thông báo trạng thái video, gói tin giữ kết nối (heartbeat).
    *   Chỉ lưu vào CSV khi có đầy đủ cả 2 chỉ số `audioDecibel` và `frequency`.
3.  **Định dạng thời gian**: 
    *   Tự động chuyển đổi thời gian từ chuẩn máy tính sang định dạng dễ đọc: `Ngày-Tháng-Năm Giờ:Phút:Giây`.

## 3. Hướng dẫn sử dụng

### Yêu cầu hệ thống:
*   Python 3.x
*   Thư viện `requests`: `pip install requests`

### Cách chạy:
1.  Đảm bảo máy tính kết nối được IP Camera: `192.168.10.153`.
2.  Mở Terminal/Command Prompt tại thư mục dự án.
3.  Chạy lệnh: 
    ```bash
    python acoustic_final.py
    ```

## 4. Kết quả đầu ra
Dữ liệu được lưu tại file **`camera_metrics.csv`** với cấu trúc:
*   **dateTime**: Thời gian đo (khớp với OSD trên màn hình camera).
*   **audioDecibel**: Cường độ âm thanh thực tế (dB).
*   **frequency**: Tần số đặc trưng của nguồn âm (Hz).

---
*Dự án được hỗ trợ thực hiện bởi Antigravity AI.*
