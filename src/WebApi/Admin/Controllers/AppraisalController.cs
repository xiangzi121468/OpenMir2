using AppraisalModule;
using AppraisalModule.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// 装备鉴定系统管理控制器
    /// </summary>
    [ApiController]
    [Route("api/admin/appraisal")]
    [Authorize]
    public class AppraisalController : AdminBaseController
    {
        private readonly AppraisalService _appraisalService;
        private readonly ILogger<AppraisalController> _logger;

        public AppraisalController(AppraisalService appraisalService, ILogger<AppraisalController> logger)
        {
            _appraisalService = appraisalService;
            _logger = logger;
        }

        #region 鉴定属性管理

        /// <summary>
        /// 获取所有鉴定属性
        /// </summary>
        [HttpGet("attributes")]
        public async Task<IActionResult> GetAllAttributes()
        {
            try
            {
                var attrs = await _appraisalService.GetAllAttributesAsync();
                return Ok(new { success = true, data = attrs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取鉴定属性失败");
                return Ok(new { success = false, message = "获取失败" });
            }
        }

        /// <summary>
        /// 保存鉴定属性
        /// </summary>
        [HttpPost("attribute/save")]
        public async Task<IActionResult> SaveAttribute([FromBody] AppraisalAttributeSaveRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                    return Ok(new { success = false, message = "属性名称不能为空" });

                var result = await _appraisalService.SaveAttributeAsync(request);
                if (result)
                    return Ok(new { success = true, message = "保存成功" });
                return Ok(new { success = false, message = "保存失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存鉴定属性失败");
                return Ok(new { success = false, message = "保存失败" });
            }
        }

        #endregion

        #region 卷轴商品管理

        /// <summary>
        /// 获取所有卷轴商品
        /// </summary>
        [HttpGet("scrolls")]
        public async Task<IActionResult> GetAllScrollItems()
        {
            try
            {
                var items = await _appraisalService.GetAllScrollItemsAsync();
                return Ok(new { success = true, data = items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取卷轴商品失败");
                return Ok(new { success = false, message = "获取失败" });
            }
        }

        /// <summary>
        /// 保存卷轴商品
        /// </summary>
        [HttpPost("scroll/save")]
        public async Task<IActionResult> SaveScrollItem([FromBody] ScrollItemSaveRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ItemName))
                    return Ok(new { success = false, message = "物品名称不能为空" });

                var result = await _appraisalService.SaveScrollItemAsync(request);
                if (result)
                    return Ok(new { success = true, message = "保存成功" });
                return Ok(new { success = false, message = "保存失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存卷轴商品失败");
                return Ok(new { success = false, message = "保存失败" });
            }
        }

        #endregion

        #region 鉴定记录

        /// <summary>
        /// 获取最近鉴定成功记录
        /// </summary>
        [HttpGet("logs/recent")]
        public async Task<IActionResult> GetRecentLogs([FromQuery] int limit = 20)
        {
            try
            {
                var logs = await _appraisalService.GetRecentSuccessLogsAsync(limit);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取鉴定记录失败");
                return Ok(new { success = false, message = "获取失败" });
            }
        }

        /// <summary>
        /// 获取鉴定统计
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var (total, success, rare) = await _appraisalService.GetAppraisalStatsAsync();
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalCount = total,
                        successCount = success,
                        rareCount = rare,
                        successRate = total > 0 ? (success * 100.0 / total).ToString("F1") + "%" : "0%"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取鉴定统计失败");
                return Ok(new { success = false, message = "获取失败" });
            }
        }

        #endregion

        #region NPC脚本生成

        /// <summary>
        /// 生成卷轴商店NPC脚本
        /// </summary>
        [HttpGet("generate-script")]
        public async Task<IActionResult> GenerateScript()
        {
            try
            {
                var script = await _appraisalService.GenerateScrollShopScriptAsync();
                return Ok(new { success = true, data = script });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成脚本失败");
                return Ok(new { success = false, message = "生成失败" });
            }
        }

        #endregion

        #region 效果类型

        /// <summary>
        /// 获取效果类型列表
        /// </summary>
        [HttpGet("effect-types")]
        public IActionResult GetEffectTypes()
        {
            var types = new[]
            {
                new { value = "rebirth", label = "重生", description = "死亡后原地复活" },
                new { value = "block", label = "格挡", description = "格挡物理伤害" },
                new { value = "paralysis", label = "物理麻痹", description = "物理攻击麻痹敌人" },
                new { value = "magic_paralysis", label = "魔法麻痹", description = "魔法攻击麻痹敌人" },
                new { value = "tao_paralysis", label = "道术麻痹", description = "道术攻击麻痹敌人" },
                new { value = "detect", label = "探测", description = "探测隐身玩家" },
                new { value = "teleport", label = "传送", description = "随机传送躲避" },
                new { value = "lifesteal", label = "吸血", description = "吸取敌人生命" },
                new { value = "reflect", label = "反弹", description = "反弹物理伤害" },
                new { value = "lucky", label = "幸运", description = "提升暴击几率" },
                new { value = "magic_shield", label = "魔盾", description = "减少魔法伤害" },
                new { value = "speed", label = "速度", description = "提升移动速度" }
            };

            return Ok(new { success = true, data = types });
        }

        #endregion
    }
}
