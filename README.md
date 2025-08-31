
- Giám sát hoạt động hệ thống
  - Theo dõi bàn phím/chuột (Low-Level hook, có thể bật/tắt trong Cài đặ
  - Theo dõi cửa sổ đang hoạt động (tên tiến trình/tiêu đề)
  - Ghi nhận tiến trình khởi động/thoát (ProcessStart/ProcessStop)
  - Ghi nhận cắm/rút thiết bị USB
- **Protection Features (Mới)**
  - Phát hiện từ ngữ không phù hợp (tiếng Việt/Anh) với BadWordsDetector
  - Kiểm tra an toàn URL/website với UrlSafetyChecker
  - Giám sát âm thanh với AudioMonitor (yêu cầu FFmpeg)
  - Enhanced Hook Manager cho phân tích real-time
- Chính sách & Khung giờ yên lặng (Quiet Hours)
  - Cấu hình QuietHoursStart/QuietHoursEnd (HH:mm); hỗ trợ nhiều khung g
(AdditionalQuietWindows)
  - Danh sách chặn (BlockedProcesses) và danh sách cho phép trong Quiet 
(AllowedProcessesDuringQuietHours)
  - Quy tắc nâng cao theo ngày/giờ (PolicyRules) cho phép/chặn theo lịch
  - Thực thi mềm với cảnh báo/đếm ngược; chống spam cảnh báo
- Ghi log & lưu trữ
  - Ghi sự kiện dạng JSON Lines theo ngày (events-YYYYMMDD.jsonl)
  - Dọn dẹp log tự động theo số ngày (LogRetentionDays) và giới hạn dung
(LogMaxSizeMB)
  - Tự động tải lại cấu hình khi file thay đổi (debounce)