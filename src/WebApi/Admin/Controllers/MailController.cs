using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MailModule;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// 邮件管理API
    /// </summary>
    [Route("api/admin/mail")]
    public class MailController : AdminBaseController
    {
        private readonly IMailService _mailService;

        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }

        /// <summary>
        /// 获取邮件列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetMails(
            [FromQuery] string? search,
            [FromQuery] string? mailType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _mailService.GetMailsAsync(search, mailType, status, page, pageSize);
            return Ok(new { code = 0, data = new { items, total, page, pageSize } });
        }

        /// <summary>
        /// 创建邮件
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateMail([FromBody] CreateMailRequest request)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Content))
            {
                return Ok(new { code = -1, msg = "标题和内容不能为空" });
            }

            if (request.ReceiverType == "single" && string.IsNullOrEmpty(request.ReceiverName))
            {
                return Ok(new { code = -1, msg = "单人发送需要指定接收者" });
            }

            var mailId = await _mailService.CreateMailAsync(request, AdminName);
            if (mailId > 0)
            {
                await LogAction("mail", "create", $"创建邮件: {request.Title}");
                return Ok(new { code = 0, msg = "创建成功", data = new { mailId } });
            }

            return Ok(new { code = -1, msg = "创建失败" });
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        [HttpPost("send/{id}")]
        public async Task<IActionResult> SendMail(int id)
        {
            var result = await _mailService.SendMailAsync(id);
            if (result)
            {
                await LogAction("mail", "send", $"发送邮件: ID={id}");
                return Ok(new { code = 0, msg = "发送成功" });
            }

            return Ok(new { code = -1, msg = "发送失败，邮件可能已发送或不存在" });
        }

        /// <summary>
        /// 删除邮件
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMail(int id)
        {
            if (!IsAdmin)
            {
                return Ok(new { code = -1, msg = "权限不足" });
            }

            var result = await _mailService.DeleteMailAsync(id);
            if (result)
            {
                await LogAction("mail", "delete", $"删除邮件: ID={id}");
                return Ok(new { code = 0, msg = "删除成功" });
            }

            return Ok(new { code = -1, msg = "删除失败" });
        }

        /// <summary>
        /// 获取邮件模板
        /// </summary>
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _mailService.GetTemplatesAsync();
            return Ok(new { code = 0, data = templates });
        }

        /// <summary>
        /// 快速发送单人邮件
        /// </summary>
        [HttpPost("quick-send")]
        public async Task<IActionResult> QuickSend([FromBody] QuickSendRequest request)
        {
            if (string.IsNullOrEmpty(request.CharName))
            {
                return Ok(new { code = -1, msg = "角色名不能为空" });
            }

            var mailRequest = new CreateMailRequest
            {
                ReceiverType = "single",
                ReceiverName = request.CharName,
                Title = request.Title ?? "系统邮件",
                Content = request.Content ?? "这是一封系统邮件",
                Gold = request.Gold,
                GameGold = request.GameGold,
                Attachments = request.Items,
                MailType = "gm",
                SendNow = true
            };

            var mailId = await _mailService.CreateMailAsync(mailRequest, AdminName);
            if (mailId > 0)
            {
                await LogAction("mail", "quick_send", $"快速发送邮件给 {request.CharName}: Gold={request.Gold}, GameGold={request.GameGold}");
                return Ok(new { code = 0, msg = "发送成功" });
            }

            return Ok(new { code = -1, msg = "发送失败" });
        }

        /// <summary>
        /// 全服发送邮件
        /// </summary>
        [HttpPost("broadcast")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastMailRequest request)
        {
            if (!IsAdmin)
            {
                return Ok(new { code = -1, msg = "权限不足" });
            }

            var mailRequest = new CreateMailRequest
            {
                ReceiverType = "all",
                Title = request.Title,
                Content = request.Content,
                Gold = request.Gold,
                GameGold = request.GameGold,
                Attachments = request.Items,
                MailType = request.MailType ?? "system",
                SendNow = true
            };

            var mailId = await _mailService.CreateMailAsync(mailRequest, AdminName);
            if (mailId > 0)
            {
                await LogAction("mail", "broadcast", $"全服发送邮件: {request.Title}");
                return Ok(new { code = 0, msg = "发送成功" });
            }

            return Ok(new { code = -1, msg = "发送失败" });
        }
    }

    public class QuickSendRequest
    {
        public string CharName { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public List<MailAttachment>? Items { get; set; }
    }

    public class BroadcastMailRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int Gold { get; set; }
        public int GameGold { get; set; }
        public string? MailType { get; set; }
        public List<MailAttachment>? Items { get; set; }
    }
}
