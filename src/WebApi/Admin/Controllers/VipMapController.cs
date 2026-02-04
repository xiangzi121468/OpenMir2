using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VipModule;
using VipModule.Models;

namespace WebApi.Admin.Controllers
{
    /// <summary>
    /// VIP地图管理控制器
    /// </summary>
    [ApiController]
    [Route("api/admin/vipmap")]
    [Authorize]
    public class VipMapController : AdminBaseController
    {
        private readonly VipMapService _vipMapService;
        private readonly ILogger<VipMapController> _logger;

        public VipMapController(VipMapService vipMapService, ILogger<VipMapController> logger)
        {
            _vipMapService = vipMapService;
            _logger = logger;
        }

        #region VIP地图管理

        /// <summary>
        /// 获取VIP地图列表
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetVipMapList()
        {
            try
            {
                var maps = await _vipMapService.GetAllVipMapsAsync();
                return Ok(new { success = true, data = maps });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取VIP地图列表失败");
                return Ok(new { success = false, message = "获取列表失败" });
            }
        }

        /// <summary>
        /// 获取VIP地图详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVipMap(int id)
        {
            try
            {
                var map = await _vipMapService.GetVipMapByIdAsync(id);
                if (map == null)
                    return Ok(new { success = false, message = "VIP地图不存在" });

                return Ok(new { success = true, data = map });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取VIP地图详情失败: {id}");
                return Ok(new { success = false, message = "获取详情失败" });
            }
        }

        /// <summary>
        /// 保存VIP地图
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> SaveVipMap([FromBody] VipMapSaveRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MapId) || string.IsNullOrEmpty(request.MapName))
                    return Ok(new { success = false, message = "地图ID和名称不能为空" });

                var result = await _vipMapService.SaveVipMapAsync(request);
                if (result)
                {
                    _logger.LogInformation($"VIP地图保存成功: {request.MapName}, 操作人: {GetCurrentUserName()}");
                    return Ok(new { success = true, message = "保存成功" });
                }
                return Ok(new { success = false, message = "保存失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存VIP地图失败");
                return Ok(new { success = false, message = "保存失败" });
            }
        }

        /// <summary>
        /// 删除VIP地图
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVipMap(int id)
        {
            try
            {
                var result = await _vipMapService.DeleteVipMapAsync(id);
                if (result)
                {
                    _logger.LogInformation($"VIP地图已删除: {id}, 操作人: {GetCurrentUserName()}");
                    return Ok(new { success = true, message = "删除成功" });
                }
                return Ok(new { success = false, message = "删除失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除VIP地图失败: {id}");
                return Ok(new { success = false, message = "删除失败" });
            }
        }

        #endregion

        #region VIP地图怪物管理

        /// <summary>
        /// 保存VIP地图怪物
        /// </summary>
        [HttpPost("monster/save")]
        public async Task<IActionResult> SaveVipMapMonster([FromBody] VipMapMonsterSaveRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MonsterName))
                    return Ok(new { success = false, message = "怪物名称不能为空" });

                var result = await _vipMapService.SaveVipMapMonsterAsync(request);
                if (result)
                    return Ok(new { success = true, message = "保存成功" });
                return Ok(new { success = false, message = "保存失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存VIP地图怪物失败");
                return Ok(new { success = false, message = "保存失败" });
            }
        }

        /// <summary>
        /// 删除VIP地图怪物
        /// </summary>
        [HttpDelete("monster/{id}")]
        public async Task<IActionResult> DeleteVipMapMonster(int id)
        {
            try
            {
                var result = await _vipMapService.DeleteVipMapMonsterAsync(id);
                if (result)
                    return Ok(new { success = true, message = "删除成功" });
                return Ok(new { success = false, message = "删除失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除VIP地图怪物失败: {id}");
                return Ok(new { success = false, message = "删除失败" });
            }
        }

        #endregion

        #region VIP等级配置

        /// <summary>
        /// 获取VIP等级配置
        /// </summary>
        [HttpGet("levels")]
        public IActionResult GetVipLevels()
        {
            return Ok(new { success = true, data = VipMapService.VipLevels });
        }

        #endregion

        #region NPC脚本生成

        /// <summary>
        /// 生成VIP地图NPC脚本
        /// </summary>
        [HttpGet("{id}/script")]
        public async Task<IActionResult> GenerateNpcScript(int id)
        {
            try
            {
                var script = await _vipMapService.GenerateNpcScriptAsync(id);
                if (string.IsNullOrEmpty(script))
                    return Ok(new { success = false, message = "地图不存在" });

                return Ok(new { success = true, data = script });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"生成NPC脚本失败: {id}");
                return Ok(new { success = false, message = "生成脚本失败" });
            }
        }

        #endregion

        private string GetCurrentUserName()
        {
            return User.Identity?.Name ?? "admin";
        }
    }
}
