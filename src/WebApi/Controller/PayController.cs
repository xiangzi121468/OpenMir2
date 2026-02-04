using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using ShopModule;
using ShopModule.Models;

namespace WebApi.Controller
{
    /// <summary>
    /// 充值API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PayController : ControllerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IShopService _shopService;
        private readonly IConfiguration _configuration;

        // 支付配置
        private string AlipayPublicKey => _configuration["Pay:Alipay:PublicKey"] ?? "";
        private string AlipayAppId => _configuration["Pay:Alipay:AppId"] ?? "";
        private string WechatApiKey => _configuration["Pay:Wechat:ApiKey"] ?? "";
        private string WechatMchId => _configuration["Pay:Wechat:MchId"] ?? "";
        private bool VerifySignEnabled => _configuration.GetValue<bool>("Pay:VerifySign", true);

        public PayController(IShopService shopService, IConfiguration configuration)
        {
            _shopService = shopService;
            _configuration = configuration;
        }

        #region 签名验证

        /// <summary>
        /// 验证支付宝签名
        /// </summary>
        private bool VerifyAlipaySign(AlipayNotifyRequest request, IFormCollection form)
        {
            if (!VerifySignEnabled) return true;
            if (string.IsNullOrEmpty(AlipayPublicKey))
            {
                Logger.Warn("支付宝公钥未配置，跳过签名验证");
                return true;
            }

            try
            {
                // 获取所有参数并排序
                var sortedParams = form
                    .Where(x => x.Key != "sign" && x.Key != "sign_type" && !string.IsNullOrEmpty(x.Value))
                    .OrderBy(x => x.Key)
                    .Select(x => $"{x.Key}={x.Value}")
                    .ToList();

                string signContent = string.Join("&", sortedParams);
                string sign = form["sign"];

                // RSA2验签 (SHA256WithRSA)
                using var rsa = RSA.Create();
                rsa.ImportFromPem(AlipayPublicKey);

                byte[] dataBytes = Encoding.UTF8.GetBytes(signContent);
                byte[] signBytes = Convert.FromBase64String(sign);

                bool verified = rsa.VerifyData(dataBytes, signBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                
                if (!verified)
                {
                    Logger.Warn($"支付宝签名验证失败: {request.out_trade_no}");
                }
                return verified;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "支付宝签名验证异常");
                return false;
            }
        }

        /// <summary>
        /// 验证微信支付签名
        /// </summary>
        private bool VerifyWechatSign(WechatNotifyRequest request, string xmlBody)
        {
            if (!VerifySignEnabled) return true;
            if (string.IsNullOrEmpty(WechatApiKey))
            {
                Logger.Warn("微信支付密钥未配置，跳过签名验证");
                return true;
            }

            try
            {
                // 解析XML获取sign和其他参数
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(xmlBody);

                var signNode = doc.SelectSingleNode("//sign");
                if (signNode == null) return false;
                string receivedSign = signNode.InnerText;

                // 构建签名字符串
                var sortedParams = new SortedDictionary<string, string>();
                foreach (System.Xml.XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node.Name != "sign" && !string.IsNullOrEmpty(node.InnerText))
                    {
                        sortedParams[node.Name] = node.InnerText;
                    }
                }

                var signBuilder = new StringBuilder();
                foreach (var param in sortedParams)
                {
                    signBuilder.Append($"{param.Key}={param.Value}&");
                }
                signBuilder.Append($"key={WechatApiKey}");

                // MD5签名
                using var md5 = MD5.Create();
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(signBuilder.ToString()));
                string computedSign = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();

                bool verified = computedSign == receivedSign.ToUpper();
                if (!verified)
                {
                    Logger.Warn($"微信支付签名验证失败: {request.out_trade_no}");
                }
                return verified;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "微信支付签名验证异常");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// 获取充值档位列表
        /// </summary>
        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            try
            {
                var packages = await _shopService.GetRechargePackagesAsync();
                return Ok(new { code = 0, data = packages });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取充值档位失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        /// <summary>
        /// 创建充值订单
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.AccountId) || request.PackageId <= 0)
                    return Ok(new { code = -1, msg = "参数错误" });

                string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                var record = await _shopService.CreateRechargeOrderAsync(
                    request.AccountId,
                    request.CharName,
                    request.PackageId,
                    request.PayChannel ?? "alipay",
                    clientIp
                );

                if (record == null)
                    return Ok(new { code = -1, msg = "创建订单失败" });

                // 返回订单信息，前端跳转支付
                return Ok(new
                {
                    code = 0,
                    data = new
                    {
                        orderNo = record.OrderNo,
                        amount = record.Amount,
                        gameGold = record.TotalGold,
                        payChannel = record.PayChannel
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "创建充值订单失败");
                return Ok(new { code = -1, msg = "创建订单失败" });
            }
        }

        /// <summary>
        /// 支付宝异步通知
        /// </summary>
        [HttpPost("notify/alipay")]
        public async Task<IActionResult> AlipayNotify([FromForm] AlipayNotifyRequest request)
        {
            try
            {
                Logger.Info($"收到支付宝回调: {request.out_trade_no}, 状态:{request.trade_status}");

                // 验证签名
                if (!VerifyAlipaySign(request, Request.Form))
                {
                    Logger.Warn($"支付宝回调签名验证失败: {request.out_trade_no}");
                    return Content("fail");
                }

                // 验证AppId
                var appId = Request.Form["app_id"].ToString();
                if (!string.IsNullOrEmpty(AlipayAppId) && appId != AlipayAppId)
                {
                    Logger.Warn($"支付宝AppId不匹配: {appId} != {AlipayAppId}");
                    return Content("fail");
                }

                if (request.trade_status == "TRADE_SUCCESS" || request.trade_status == "TRADE_FINISHED")
                {
                    bool success = await _shopService.HandlePayCallbackAsync(
                        request.out_trade_no,
                        request.trade_no,
                        true
                    );

                    if (success)
                    {
                        // 发放元宝
                        await _shopService.DeliverRechargeAsync(request.out_trade_no);
                        Logger.Info($"支付宝订单处理成功: {request.out_trade_no}");
                    }
                }

                return Content("success");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "支付宝回调处理失败");
                return Content("fail");
            }
        }

        /// <summary>
        /// 微信支付异步通知 (XML格式)
        /// </summary>
        [HttpPost("notify/wechat")]
        [Consumes("application/xml", "text/xml")]
        public async Task<IActionResult> WechatNotify()
        {
            try
            {
                // 读取原始XML
                using var reader = new System.IO.StreamReader(Request.Body, Encoding.UTF8);
                string xmlBody = await reader.ReadToEndAsync();
                Logger.Info($"收到微信支付回调: {xmlBody.Length} bytes");

                // 解析XML
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(xmlBody);

                var request = new WechatNotifyRequest
                {
                    return_code = doc.SelectSingleNode("//return_code")?.InnerText,
                    result_code = doc.SelectSingleNode("//result_code")?.InnerText,
                    out_trade_no = doc.SelectSingleNode("//out_trade_no")?.InnerText,
                    transaction_id = doc.SelectSingleNode("//transaction_id")?.InnerText,
                    total_fee = int.TryParse(doc.SelectSingleNode("//total_fee")?.InnerText, out var fee) ? fee : 0
                };

                Logger.Info($"微信支付订单: {request.out_trade_no}, 结果: {request.result_code}");

                // 验证签名
                if (!VerifyWechatSign(request, xmlBody))
                {
                    Logger.Warn($"微信支付签名验证失败: {request.out_trade_no}");
                    return Content("<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[签名验证失败]]></return_msg></xml>", "application/xml");
                }

                // 验证商户号
                var mchId = doc.SelectSingleNode("//mch_id")?.InnerText;
                if (!string.IsNullOrEmpty(WechatMchId) && mchId != WechatMchId)
                {
                    Logger.Warn($"微信商户号不匹配: {mchId} != {WechatMchId}");
                    return Content("<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[商户号不匹配]]></return_msg></xml>", "application/xml");
                }

                if (request.return_code == "SUCCESS" && request.result_code == "SUCCESS")
                {
                    bool success = await _shopService.HandlePayCallbackAsync(
                        request.out_trade_no,
                        request.transaction_id,
                        true
                    );

                    if (success)
                    {
                        await _shopService.DeliverRechargeAsync(request.out_trade_no);
                        Logger.Info($"微信支付订单处理成功: {request.out_trade_no}");
                    }
                }

                return Content("<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>", "application/xml");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "微信支付回调处理失败");
                return Content("<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[系统错误]]></return_msg></xml>", "application/xml");
            }
        }

        /// <summary>
        /// 查询充值记录
        /// </summary>
        [HttpGet("records")]
        public async Task<IActionResult> GetRecords([FromQuery] string accountId, [FromQuery] int page = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                    return Ok(new { code = -1, msg = "参数错误" });

                var records = await _shopService.GetRechargeRecordsAsync(accountId, page);
                return Ok(new { code = 0, data = records });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "查询充值记录失败");
                return Ok(new { code = -1, msg = "查询失败" });
            }
        }

        /// <summary>
        /// 检查是否首充
        /// </summary>
        [HttpGet("isfirst")]
        public async Task<IActionResult> IsFirstRecharge([FromQuery] string accountId)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                    return Ok(new { code = -1, msg = "参数错误" });

                bool isFirst = await _shopService.IsFirstRechargeAsync(accountId);
                return Ok(new { code = 0, data = isFirst });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "检查首充失败");
                return Ok(new { code = -1, msg = "查询失败" });
            }
        }

        /// <summary>
        /// 模拟支付成功(测试用)
        /// </summary>
        [HttpPost("test/success")]
        public async Task<IActionResult> TestPaySuccess([FromBody] TestPayRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.OrderNo))
                    return Ok(new { code = -1, msg = "参数错误" });

                // 仅开发环境可用
#if DEBUG
                bool success = await _shopService.HandlePayCallbackAsync(
                    request.OrderNo,
                    $"TEST_{DateTime.Now:yyyyMMddHHmmss}",
                    true
                );

                if (success)
                {
                    await _shopService.DeliverRechargeAsync(request.OrderNo);
                    return Ok(new { code = 0, msg = "模拟支付成功" });
                }
#endif
                return Ok(new { code = -1, msg = "操作失败" });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "模拟支付失败");
                return Ok(new { code = -1, msg = "操作失败" });
            }
        }
    }

    #region 请求模型

    public class CreateOrderRequest
    {
        public string AccountId { get; set; }
        public string CharName { get; set; }
        public int PackageId { get; set; }
        public string PayChannel { get; set; }
    }

    public class AlipayNotifyRequest
    {
        public string out_trade_no { get; set; }
        public string trade_no { get; set; }
        public string trade_status { get; set; }
        public string total_amount { get; set; }
        public string sign { get; set; }
    }

    public class WechatNotifyRequest
    {
        public string return_code { get; set; }
        public string result_code { get; set; }
        public string out_trade_no { get; set; }
        public string transaction_id { get; set; }
        public int total_fee { get; set; }
    }

    public class TestPayRequest
    {
        public string OrderNo { get; set; }
    }

    #endregion
}
