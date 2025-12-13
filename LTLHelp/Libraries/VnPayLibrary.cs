using LTLHelp.Models.Vnpay;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LTLHelp.Libraries
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VnPayLibrary();
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }
            // vnp_TxnRef có thể là số hoặc format DonationId_yyyyMMddHHmmss
            var vnpTxnRef = vnPay.GetResponseData("vnp_TxnRef");
            var vnPayTranId = Convert.ToInt64(vnPay.GetResponseData("vnp_TransactionNo"));
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnpSecureHash =
                collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value; //hash của dữ liệu trả về
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
            var checkSignature =
                vnPay.ValidateSignature(vnpSecureHash, hashSecret); //check Signature
            if (!checkSignature)
            {
                // Log warning nhưng vẫn trả về OrderId để có thể xử lý
                // Trong môi trường demo/sandbox, signature validation có thể không chính xác
                return new PaymentResponseModel()
                {
                    Success = false,
                    OrderId = vnpTxnRef, // Vẫn trả về OrderId để có thể xử lý
                    VnPayResponseCode = vnpResponseCode
                };
            }
            return new PaymentResponseModel()
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = vnpTxnRef, // Giữ nguyên format (có thể là số hoặc DonationId_yyyyMMddHHmmss)
                PaymentId = vnPayTranId.ToString(),
                TransactionId = vnPayTranId.ToString(),
                Token = vnpSecureHash,
                VnPayResponseCode = vnpResponseCode
            };
        }

        public string GetIpAddress(HttpContext context)
        {
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null)
                    {
                        var ipAddress = remoteIpAddress.ToString();
                        // Đảm bảo không trả về null hoặc rỗng
                        if (!string.IsNullOrEmpty(ipAddress))
                        {
                            return ipAddress;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fallback to default IP
            }

            // Fallback mặc định nếu không lấy được IP
            return "127.0.0.1";
        }

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            // Chỉ thêm các field vnp_* và loại bỏ các field rỗng
            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value) && kv.Key.StartsWith("vnp_")))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();

            // Tạo signData bằng cách loại bỏ dấu & cuối cùng
            var signData = querystring;
            if (signData.Length > 0 && signData.EndsWith("&"))
            {
                signData = signData.Remove(signData.Length - 1, 1);
            }

            // Tạo secure hash từ signData (KHÔNG bao gồm vnp_SecureHash)
            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            
            // Tạo URL cuối cùng với vnp_SecureHash
            baseUrl += "?" + querystring + "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }

            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }

            foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }

    }
}
public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}
