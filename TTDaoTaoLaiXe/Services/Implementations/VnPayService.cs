
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TTDaoTaoLaiXe.Services.Interfaces;
using System.Net;

namespace TTDaoTaoLaiXe.Services.Implementations
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string LayCauHinh(string key)
        {
            return _configuration[$"VnPay:{key}"]
                ?? throw new InvalidOperationException(
                    $"Thiếu cấu hình VnPay:{key}.");
        }



        private static string TaoChuKy(
            SortedDictionary<string, string> thamSo,
            string hashSecret)
        {
            var duLieu = string.Join("&",
                thamSo
                    .Where(x => !string.IsNullOrEmpty(x.Value))
                    .Select(x =>
                        $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

            using var hmac = new HMACSHA512(
                Encoding.UTF8.GetBytes(hashSecret));

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(duLieu));

            return Convert.ToHexString(hash).ToLowerInvariant();
        }


        public string TaoUrlThanhToan(
            string maThanhToan,
            decimal soTien,
            string noiDung,
            string ipAddress)
        {
            if (soTien <= 0 || soTien != decimal.Truncate(soTien))
                throw new ArgumentException(
                    "Số tiền phải là số nguyên dương.", nameof(soTien));

            var hashSecret = LayCauHinh("HashSecret");
            var tmnCode = LayCauHinh("TmnCode");
            var paymentUrl = LayCauHinh("PaymentUrl");
            var returnUrl = LayCauHinh("ReturnUrl");

            var now = DateTime.Now;

            var thamSo = new SortedDictionary<string, string>
            {
                ["vnp_Version"] = LayCauHinh("Version"),
                ["vnp_Command"] = LayCauHinh("Command"),
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = (soTien * 100m)
                    .ToString("0", CultureInfo.InvariantCulture),
                ["vnp_CurrCode"] = LayCauHinh("CurrCode"),
                ["vnp_TxnRef"] = maThanhToan,
                ["vnp_OrderInfo"] = noiDung,
                ["vnp_OrderType"] = LayCauHinh("OrderType"),
                ["vnp_Locale"] = LayCauHinh("Locale"),
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_CreateDate"] = now.ToString(
                    "yyyyMMddHHmmss", CultureInfo.InvariantCulture),
                ["vnp_ExpireDate"] = now.AddMinutes(15).ToString(
                    "yyyyMMddHHmmss", CultureInfo.InvariantCulture)
            };


            var chuKy = TaoChuKy(thamSo, hashSecret);

            var query = string.Join("&",
                thamSo.Select(x =>
                    $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

            return $"{paymentUrl}?{query}&vnp_SecureHash={chuKy}";

        }

        public bool XacMinhChuKy(
            IDictionary<string, string> thamSo)
        {
            if (!thamSo.TryGetValue(
                    "vnp_SecureHash", out var chuKyNhanDuoc) ||
                string.IsNullOrWhiteSpace(chuKyNhanDuoc))
            {
                return false;
            }

            var duLieu = new SortedDictionary<string, string>(
                StringComparer.Ordinal);

            foreach (var item in thamSo)
            {
                if (item.Key.Equals(
                        "vnp_SecureHash",
                        StringComparison.OrdinalIgnoreCase) ||
                    item.Key.Equals(
                        "vnp_SecureHashType",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                duLieu[item.Key] = item.Value;
            }

            var chuKyTinhDuoc = TaoChuKy(
                duLieu, LayCauHinh("HashSecret"));

            try
            {
                var hashNhan = Convert.FromHexString(chuKyNhanDuoc);
                var hashTinh = Convert.FromHexString(chuKyTinhDuoc);

                return CryptographicOperations.FixedTimeEquals(
                    hashNhan, hashTinh);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
