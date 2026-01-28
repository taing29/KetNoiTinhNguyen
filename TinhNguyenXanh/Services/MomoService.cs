using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TinhNguyenXanh.Services
{
    public class MomoService
    {
        public static async Task<string> CreatePaymentAsync(string orderInfo, string orderId, string amount, IConfiguration config)
        {
            string endpoint = config["Momo:MomoApiUrl"];
            string partnerCode = config["Momo:PartnerCode"];
            string accessKey = config["Momo:AccessKey"];
            string secretKey = config["Momo:SecretKey"];
            string notifyUrl = config["Momo:NotifyUrl"];
            string returnUrl = config["Momo:ReturnUrl"];

            string requestId = Guid.NewGuid().ToString();
            string extraData = "";

            // 1. SỬA LẠI: requestType="captureWallet" (Để hiện QR)
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + notifyUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + returnUrl +
                "&requestId=" + requestId +
                "&requestType=captureWallet"; // <--- Đã đổi về captureWallet

            string signature = ComputeHmacSha256(rawHash, secretKey);

            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "partnerName", "Tinh Nguyen Xanh" },
                { "storeId", "MomoTestStore" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", returnUrl },
                { "ipnUrl", notifyUrl },
                { "lang", "vi" },
                { "extraData", extraData },
                { "requestType", "captureWallet" }, // <--- Đã đổi về captureWallet
                { "signature", signature }
            };

            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync(endpoint, new StringContent(message.ToString(), Encoding.UTF8, "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var jsonResponse = JObject.Parse(responseContent);
                    if (jsonResponse["payUrl"] != null)
                    {
                        return jsonResponse["payUrl"].ToString();
                    }
                    else
                    {
                        return "LỖI MOMO: " + jsonResponse["message"]?.ToString();
                    }
                }
                catch
                {
                    return "Lỗi xử lý phản hồi: " + responseContent;
                }
            }
        }

        private static string ComputeHmacSha256(string message, string secretKey)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                string hex = BitConverter.ToString(hashmessage);
                return hex.Replace("-", "").ToLower();
            }
        }
    }
}