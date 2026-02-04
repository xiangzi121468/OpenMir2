using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using ShopModule;
using ShopModule.Models;

namespace WebApi.Controller
{
    /// <summary>
    /// 商城API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController : ControllerBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        /// <summary>
        /// 获取商品分类
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _shopService.GetCategoriesAsync();
                return Ok(new { code = 0, data = categories });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取分类失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        /// <summary>
        /// 获取分类商品
        /// </summary>
        [HttpGet("items")]
        public async Task<IActionResult> GetItems([FromQuery] int categoryId)
        {
            try
            {
                var items = await _shopService.GetItemsByCategoryAsync(categoryId);
                return Ok(new { code = 0, data = items });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取商品失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        /// <summary>
        /// 获取热销商品
        /// </summary>
        [HttpGet("hot")]
        public async Task<IActionResult> GetHotItems([FromQuery] int limit = 10)
        {
            try
            {
                var items = await _shopService.GetHotItemsAsync(limit);
                return Ok(new { code = 0, data = items });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取热销商品失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        /// <summary>
        /// 获取新品
        /// </summary>
        [HttpGet("new")]
        public async Task<IActionResult> GetNewItems([FromQuery] int limit = 10)
        {
            try
            {
                var items = await _shopService.GetNewItemsAsync(limit);
                return Ok(new { code = 0, data = items });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取新品失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        /// <summary>
        /// 搜索商品
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchItems([FromQuery] string keyword)
        {
            try
            {
                var items = await _shopService.SearchItemsAsync(keyword);
                return Ok(new { code = 0, data = items });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "搜索商品失败");
                return Ok(new { code = -1, msg = "搜索失败" });
            }
        }

        /// <summary>
        /// 获取商品详情
        /// </summary>
        [HttpGet("item/{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            try
            {
                var item = await _shopService.GetItemAsync(id);
                if (item == null)
                    return Ok(new { code = -1, msg = "商品不存在" });

                return Ok(new { code = 0, data = item });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取商品详情失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        /// <summary>
        /// 获取用户订单
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] string accountId, [FromQuery] int page = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                    return Ok(new { code = -1, msg = "参数错误" });

                var orders = await _shopService.GetUserOrdersAsync(accountId, page);
                return Ok(new { code = 0, data = orders });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取订单失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        #region VIP

        /// <summary>
        /// 获取VIP等级配置
        /// </summary>
        [HttpGet("vip/levels")]
        public async Task<IActionResult> GetVipLevels()
        {
            try
            {
                var levels = await _shopService.GetVipLevelsAsync();
                return Ok(new { code = 0, data = levels });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取VIP配置失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        /// <summary>
        /// 获取用户VIP信息
        /// </summary>
        [HttpGet("vip/info")]
        public async Task<IActionResult> GetUserVip([FromQuery] string accountId)
        {
            try
            {
                if (string.IsNullOrEmpty(accountId))
                    return Ok(new { code = -1, msg = "参数错误" });

                var vip = await _shopService.GetUserVipAsync(accountId);
                return Ok(new { code = 0, data = vip });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取VIP信息失败");
                return Ok(new { code = -1, msg = "获取失败" });
            }
        }

        #endregion

        #region 礼包码

        /// <summary>
        /// 检查礼包码
        /// </summary>
        [HttpGet("giftcode/check")]
        public async Task<IActionResult> CheckGiftCode([FromQuery] string code, [FromQuery] string accountId)
        {
            try
            {
                var (valid, message) = await _shopService.CheckGiftCodeAsync(code, accountId);
                return Ok(new { code = valid ? 0 : -1, msg = message });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "检查礼包码失败");
                return Ok(new { code = -1, msg = "检查失败" });
            }
        }

        #endregion
    }
}
