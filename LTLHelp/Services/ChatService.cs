using LTLHelp.Models;

namespace LTLHelp.Services
{
    public class ChatService
    {
        private readonly LtlhelpContext _db;
        private readonly GeminiService _gemini;

        public ChatService(LtlhelpContext db, GeminiService gemini)
        {
            _db = db;
            _gemini = gemini;
        }

        public async Task<string> ProcessMessage(string message)
        {
            message = message.ToLower();

            try
            {
                if (message.Contains("chiến dịch"))
                {
                    var campaigns = _db.Campaigns
                        .Where(c => c.Status != null && c.Status.Contains("diễn"))
                        .Select(c => c.Title)
                        .Take(5)
                        .ToList();

                    return campaigns.Any()
                        ? "Các chiến dịch đang diễn ra:\n- " + string.Join("\n- ", campaigns)
                        : "Hiện chưa có chiến dịch nào đang hoạt động.";
                }


               
                if (message.Contains("tổng tiền")
                    || message.Contains("tổng số tiền")
                    || message.Contains("đã gây quỹ")
                    || message.Contains("quyên góp được"))
                {
                    var totalAmount = _db.Donations
                        .Where(d => d.Amount > 0)
                        .Sum(d => d.Amount);

                    return $"Tổng số tiền đã quyên góp được đến hiện tại là: {totalAmount:N0} VNĐ.";
                }


                if (message.Contains("danh mục"))
                {
                    var categories = _db.CampaignCategories.Select(c => c.Name).ToList();
                    return "Danh mục chiến dịch:\n- " + string.Join("\n- ", categories);
                }

                if (message.Contains("bài viết"))
                {
                    var posts = _db.BlogPosts
                        .OrderByDescending(p => p.BlogPostId)
                        .Take(3)
                        .Select(p => p.Title)
                        .ToList();

                    return "Bài viết mới:\n- " + string.Join("\n- ", posts);
                }
            
                if (message.Contains("người đã ủng hộ")
                    || message.Contains("danh sách ủng hộ")
                    || message.Contains("nhà tài trợ"))
                {
                    var donors = _db.Donations
                        .OrderByDescending(d => d.DonationId)
                        .Take(5)
                        .Select(d => new
                        {
                            Name = d.DonorName,
                            Amount = d.Amount
                        })
                        .ToList();

                    if (!donors.Any())
                        return "Hiện chưa có lượt ủng hộ nào.";

                    var result = "Danh sách người đã ủng hộ gần đây:\n";

                    foreach (var d in donors)
                    {
                        result += $"- {d.Name}: {d.Amount:N0} VNĐ\n";
                    }

                    return result;
                }

                var now = DateTime.Now;
                var weatherDescription = "Hôm nay thời tiết tại Nghệ An là 23 độ";
                return await _gemini.AskGemini($@"
                    Bạn là chatbot của website gây quỹ từ thiện LTLHelp.

                    Ngữ cảnh hệ thống:
                    - Thời gian hiện tại: {now:HH:mm}
                    - Ngày hiện tại: {now:dd/MM/yyyy}
                    - Múi giờ: Việt Nam (GMT+7)
- Thời tiết hiện tại : {weatherDescription}
                    Quy tắc:
                    - Trả lời tự nhiên như chat thông thường
                    - Nếu người dùng hỏi ngày, giờ thì trả lời dựa vào thời gian hệ thống
                    - Nếu hỏi linh tinh thì trả lời như chatbot bình thường
                    - Nếu hỏi về gây quỹ thì trả lời đúng ngữ cảnh website
                    - Không nói lan man, không giải thích kỹ thuật
                    - Nếu người dùng muốn quyên góp số tiền nào đó thì hãy hướng dẫn họ cách để quyên góp
                    -Bạn vẫn có thể trả lời bình thường như các chat AI khác 
                    -Lời văn trả lời thân thiện.
                    - Nếu người dùng hỏi về thời tiết thế nào hãy trả lời dựa theo thời tiết của hệ thống

                    Người dùng hỏi:
                    {message}
                    ");

            }
            catch (Exception ex)
            {
                return $"❌ Lỗi hệ thống: {ex.Message}";
            }

        }
    }
}
