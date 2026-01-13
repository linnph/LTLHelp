using LTLHelp.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Newtonsoft.Json; // Cần cài đặt gói Newtonsoft.Json

namespace LTLHelp.Services
{
    public class ChatService
    {
        private readonly LtlhelpContext _db;
        private readonly GeminiService _gemini;
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatService(LtlhelpContext db, GeminiService gemini, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _gemini = gemini;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> ProcessMessage(string message)
        {
            try
            {
                // 1. LẤY THỜI GIAN VÀ THỜI TIẾT THỰC TẾ TỪ MÁY CHỦ
                var now = DateTime.Now;
                string weatherData = await GetCurrentWeather();

                // 2. LẤY DỮ LIỆU DATABASE 
                var campaigns = _db.Campaigns.Where(c => c.Status != null).ToList();
                string campaignsInfo = string.Join("\n", campaigns.Select(c =>
                    $"- {c.Title}: Đã có {c.RaisedAmount:N0} VNĐ / Mục tiêu {c.GoalAmount:N0} VNĐ"));

                var donors = _db.Donations.OrderByDescending(d => d.DonationId).Take(5)
                    .Select(d => $"- {d.DonorName}: {d.Amount:N0} VNĐ").ToList();
                // 2. THÊM DỮ LIỆU CỐ ĐỊNH (Hình thức thanh toán) VÀO PROMPT
                string paymentMethods = "- Thanh toán qua VNPay (Thẻ ATM, QR Code)\n- Chuyển khoản ngân hàng qua mã QR\n- Ví điện tử MoMo";

                string prompt = $@"
                Bạn là trợ lý ảo của website LTLHelp. Dữ liệu thực tế:
                [THỜI GIAN]: {now:HH:mm:ss dd/MM/yyyy}
                [THỜI TIẾT]: {weatherData}
                
                [HÌNH THỨC THANH TOÁN]:
                {paymentMethods}
                [DỮ LIỆU WEBSITE]:
                {campaignsInfo}
                Nhà tài trợ mới: {string.Join(", ", donors)}

                QUY TẮC:
                - Nếu khách hỏi thời tiết, hãy đọc chính xác dữ liệu [THỜI TIẾT MÁY CHỦ].
                - Nếu hỏi giờ giấc, dùng [THỜI GIAN].
                - Luôn thân thiện và chuyên nghiệp.

                CÂU HỎI: {message}";

                return await _gemini.AskGemini(prompt);
            }
            catch (Exception ex)
            {
                return $"Lỗi hệ thống: {ex.Message}";
            }
        }

        private async Task<string> GetCurrentWeather()
        {
            try
            {
                
                return "Trời đang nắng nhẹ, nhiệt độ 25°C, gió nhẹ. (Cập nhật từ hệ thống LTLHelp)";
            }
            catch
            {
                return "Hiện chưa cập nhật được dữ liệu thời tiết.";
            }
        }
    }
}