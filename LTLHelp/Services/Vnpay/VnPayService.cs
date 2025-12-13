using LTLHelp.Libraries;
using LTLHelp.Models.Vnpay;
using Microsoft.Extensions.Logging;

namespace LTLHelp.Services.Vnpay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayService> _logger;

        public VnPayService(IConfiguration configuration, ILogger<VnPayService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context, string orderId)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"] ?? "SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var pay = new VnPayLibrary();

            // Đọc PaymentCallBack:ReturnUrl từ appsettings.json
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];
            
            // Nếu cấu hình null hoặc rỗng, tự động tạo ReturnUrl từ HttpContext.Request
            // Cho phép localhost (phù hợp cho demo)
            if (string.IsNullOrWhiteSpace(urlCallBack))
            {
                var request = context.Request;
                var scheme = request.Scheme; // http hoặc https
                var host = request.Host.Value; // localhost:7045 hoặc domain thực tế
                urlCallBack = $"{scheme}://{host}/Payment/PaymentCallback";
                _logger.LogInformation("PaymentCallBack:ReturnUrl not configured, auto-generated from HttpContext: {ReturnUrl}", urlCallBack);
            }
            else
            {
                urlCallBack = urlCallBack.Trim();
                _logger.LogInformation("PaymentCallBack:ReturnUrl from appsettings.json: {ReturnUrl}", urlCallBack);
            }

            // vnp_Amount: Phải = số tiền * 100, là số nguyên, không dấu . hoặc ,
            var amountInVnd = (long)(model.Amount * 100);
            var vnpAmount = amountInVnd.ToString();

            // vnp_TxnRef: Format DonationId_yyyyMMddHHmmss (đã được truyền vào từ controller)
            // orderId đã được format đúng ở controller

            // vnp_CreateDate: Format yyyyMMddHHmmss
            var vnpCreateDate = timeNow.ToString("yyyyMMddHHmmss");

            // vnp_IpAddr: Lấy từ HttpContext, fallback "127.0.0.1"
            var vnpIpAddr = pay.GetIpAddress(context);
            if (string.IsNullOrEmpty(vnpIpAddr))
            {
                vnpIpAddr = "127.0.0.1";
            }

            // vnp_OrderInfo: Không được rỗng, format "Quyen gop chien dich {CampaignId}"
            var vnpOrderInfo = model.OrderDescription;
            if (string.IsNullOrWhiteSpace(vnpOrderInfo))
            {
                vnpOrderInfo = $"Quyen gop chien dich {model.Name}";
            }
            // Đảm bảo không rỗng
            if (string.IsNullOrWhiteSpace(vnpOrderInfo))
            {
                vnpOrderInfo = "Quyen gop tu thien";
            }

            // Thêm các field bắt buộc
            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"] ?? "2.1.0");
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"] ?? "pay");
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"] ?? "");
            pay.AddRequestData("vnp_Amount", vnpAmount);
            pay.AddRequestData("vnp_CreateDate", vnpCreateDate);
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"] ?? "VND");
            pay.AddRequestData("vnp_IpAddr", vnpIpAddr);
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"] ?? "vn");
            pay.AddRequestData("vnp_OrderInfo", vnpOrderInfo);
            pay.AddRequestData("vnp_OrderType", model.OrderType ?? "other");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", orderId);

            // Log dữ liệu trước khi tạo URL
            _logger.LogInformation("=== VNPAY PAYMENT REQUEST DATA ===");
            _logger.LogInformation("vnp_Amount: {Amount}", vnpAmount);
            _logger.LogInformation("vnp_TxnRef: {TxnRef}", orderId);
            _logger.LogInformation("vnp_ReturnUrl: {ReturnUrl}", urlCallBack);
            _logger.LogInformation("vnp_IpAddr: {IpAddr}", vnpIpAddr);
            _logger.LogInformation("vnp_CreateDate: {CreateDate}", vnpCreateDate);
            _logger.LogInformation("vnp_OrderInfo: {OrderInfo}", vnpOrderInfo);
            _logger.LogInformation("vnp_CurrCode: {CurrCode}", _configuration["Vnpay:CurrCode"] ?? "VND");
            _logger.LogInformation("vnp_Command: {Command}", _configuration["Vnpay:Command"] ?? "pay");
            _logger.LogInformation("vnp_Version: {Version}", _configuration["Vnpay:Version"] ?? "2.1.0");

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", _configuration["Vnpay:HashSecret"] ?? "");

            // Log full query string trước khi redirect
            _logger.LogInformation("=== FULL VNPAY PAYMENT URL ===");
            _logger.LogInformation("Payment URL: {PaymentUrl}", paymentUrl);

            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"] ?? "");

            return response;
        }

    }
}
