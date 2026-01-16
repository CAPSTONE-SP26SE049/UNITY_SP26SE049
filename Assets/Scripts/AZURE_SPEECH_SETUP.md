# Hướng Dẫn Setup Azure Speech Services

## Bước 1: Đăng ký Azure Speech Services

1. Truy cập: https://azure.microsoft.com/services/cognitive-services/speech-services/
2. Đăng nhập vào Azure Portal (hoặc tạo tài khoản miễn phí)
3. Tạo một **Speech** resource:
   - Vào Azure Portal → Create a resource
   - Tìm "Speech" → Chọn "Speech Services"
   - Chọn Subscription và Resource Group
   - Chọn Region: **Southeast Asia** (gần Việt Nam nhất)
   - Chọn Pricing tier: **Free F0** (có 5 giờ/tháng miễn phí)
   - Tạo resource

## Bước 2: Lấy Subscription Key và Region

1. Sau khi tạo resource, vào **Keys and Endpoint**
2. Copy **KEY 1** (Subscription Key)
3. Ghi nhớ **Location/Region** (ví dụ: `southeastasia`)

## Bước 3: Cấu hình trong Unity

### Cách 1: Điền trực tiếp trong Inspector

1. Trong Unity, tìm GameObject **AzureSpeechRecognitionService** trong Hierarchy
2. Trong Inspector, điền:
   - **Subscription Key**: Dán key bạn đã copy
   - **Region**: `southeastasia` (hoặc region bạn đã chọn)
   - **Language**: `vi-VN` (tiếng Việt)

### Cách 2: Sử dụng Script để lưu vào PlayerPrefs

```csharp
// Trong code hoặc console, chạy:
AzureSpeechRecognitionService service = FindObjectOfType<AzureSpeechRecognitionService>();
service.SaveSubscriptionKey("YOUR_SUBSCRIPTION_KEY_HERE");
```

## Bước 4: Test

1. Chạy game và test Speaking Question
2. Bấm "Bắt đầu ghi âm"
3. Nói câu/từ theo yêu cầu
4. Bấm "Dừng ghi âm"
5. Hệ thống sẽ gửi audio đến Azure và nhận diện giọng nói

## Lưu ý

- **Free Tier**: 5 giờ nhận diện/tháng miễn phí
- **Region**: Nên chọn `southeastasia` để có độ trễ thấp nhất
- **Language**: `vi-VN` cho tiếng Việt
- **Sample Rate**: Azure yêu cầu 16000 Hz (đã tự động convert)

## Troubleshooting

### Lỗi "Subscription Key chưa được cấu hình"
- Kiểm tra lại đã điền Subscription Key trong Inspector chưa
- Hoặc sử dụng `SaveSubscriptionKey()` để lưu vào PlayerPrefs

### Lỗi "Không thể lấy access token"
- Kiểm tra Subscription Key có đúng không
- Kiểm tra Region có đúng không
- Kiểm tra kết nối internet

### Nhận diện không chính xác
- Nói rõ ràng, không quá nhanh
- Đảm bảo microphone chất lượng tốt
- Kiểm tra môi trường không có tiếng ồn

## Tài liệu tham khảo

- Azure Speech Services: https://azure.microsoft.com/services/cognitive-services/speech-services/
- API Documentation: https://docs.microsoft.com/azure/cognitive-services/speech-service/
- Pricing: https://azure.microsoft.com/pricing/details/cognitive-services/speech-services/

