using AppraisalModule.Models;
using MySqlConnector;
using NLog;

namespace AppraisalModule
{
    /// <summary>
    /// 装备鉴定服务
    /// </summary>
    public class AppraisalService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionString;
        private readonly Random _random = new();

        // 缓存属性配置
        private List<AppraisalAttribute>? _attributeCache;
        private DateTime _cacheTime = DateTime.MinValue;

        public AppraisalService(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region 鉴定属性管理

        public async Task<List<AppraisalAttribute>> GetAllAttributesAsync()
        {
            // 使用缓存
            if (_attributeCache != null && (DateTime.Now - _cacheTime).TotalMinutes < 5)
                return _attributeCache;

            var attrs = new List<AppraisalAttribute>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT * FROM appraisal_attributes WHERE is_enabled = 1 ORDER BY scroll_level, probability DESC", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    attrs.Add(new AppraisalAttribute
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                        EffectType = reader.GetString("effect_type"),
                        EffectValue = reader.GetInt32("effect_value"),
                        MaxLevel = reader.GetInt32("max_level"),
                        ScrollLevel = reader.GetInt32("scroll_level"),
                        Probability = reader.GetInt32("probability"),
                        IsRare = reader.GetBoolean("is_rare"),
                        Broadcast = reader.GetBoolean("broadcast"),
                        IsEnabled = reader.GetBoolean("is_enabled")
                    });
                }

                _attributeCache = attrs;
                _cacheTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取鉴定属性失败");
            }
            return attrs;
        }

        public async Task<bool> SaveAttributeAsync(AppraisalAttributeSaveRequest request)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql;
                if (request.Id.HasValue && request.Id > 0)
                {
                    sql = @"UPDATE appraisal_attributes SET 
                        name = @name, description = @desc, effect_type = @effect, max_level = @maxLvl,
                        scroll_level = @scrollLvl, probability = @prob, is_rare = @rare, broadcast = @broadcast, is_enabled = @enabled
                        WHERE id = @id";
                }
                else
                {
                    sql = @"INSERT INTO appraisal_attributes 
                        (name, description, effect_type, max_level, scroll_level, probability, is_rare, broadcast, is_enabled)
                        VALUES (@name, @desc, @effect, @maxLvl, @scrollLvl, @prob, @rare, @broadcast, @enabled)";
                }

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", request.Name);
                cmd.Parameters.AddWithValue("@desc", request.Description ?? "");
                cmd.Parameters.AddWithValue("@effect", request.EffectType);
                cmd.Parameters.AddWithValue("@maxLvl", request.MaxLevel);
                cmd.Parameters.AddWithValue("@scrollLvl", request.ScrollLevel);
                cmd.Parameters.AddWithValue("@prob", request.Probability);
                cmd.Parameters.AddWithValue("@rare", request.IsRare);
                cmd.Parameters.AddWithValue("@broadcast", request.Broadcast);
                cmd.Parameters.AddWithValue("@enabled", request.IsEnabled);

                if (request.Id.HasValue && request.Id > 0)
                    cmd.Parameters.AddWithValue("@id", request.Id.Value);

                await cmd.ExecuteNonQueryAsync();
                _attributeCache = null; // 清除缓存
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存鉴定属性失败");
                return false;
            }
        }

        #endregion

        #region 卷轴商品管理

        public async Task<List<ScrollItem>> GetAllScrollItemsAsync()
        {
            var items = new List<ScrollItem>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand("SELECT * FROM scroll_items WHERE is_enabled = 1 ORDER BY sort_order, id", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    items.Add(new ScrollItem
                    {
                        Id = reader.GetInt32("id"),
                        ItemName = reader.GetString("item_name"),
                        DisplayName = reader.GetString("display_name"),
                        ScrollLevel = reader.GetInt32("scroll_level"),
                        PriceSingle = reader.GetInt32("price_single"),
                        PriceBatch = reader.GetInt32("price_batch"),
                        BatchCount = reader.GetInt32("batch_count"),
                        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                        Icon = reader.IsDBNull(reader.GetOrdinal("icon")) ? null : reader.GetString("icon"),
                        SortOrder = reader.GetInt32("sort_order"),
                        IsEnabled = reader.GetBoolean("is_enabled")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取卷轴商品失败");
            }
            return items;
        }

        public async Task<bool> SaveScrollItemAsync(ScrollItemSaveRequest request)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                string sql;
                if (request.Id.HasValue && request.Id > 0)
                {
                    sql = @"UPDATE scroll_items SET 
                        item_name = @item, display_name = @display, scroll_level = @lvl,
                        price_single = @price1, price_batch = @price10, batch_count = @batch,
                        description = @desc, sort_order = @sort, is_enabled = @enabled
                        WHERE id = @id";
                }
                else
                {
                    sql = @"INSERT INTO scroll_items 
                        (item_name, display_name, scroll_level, price_single, price_batch, batch_count, description, sort_order, is_enabled)
                        VALUES (@item, @display, @lvl, @price1, @price10, @batch, @desc, @sort, @enabled)";
                }

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@item", request.ItemName);
                cmd.Parameters.AddWithValue("@display", request.DisplayName);
                cmd.Parameters.AddWithValue("@lvl", request.ScrollLevel);
                cmd.Parameters.AddWithValue("@price1", request.PriceSingle);
                cmd.Parameters.AddWithValue("@price10", request.PriceBatch);
                cmd.Parameters.AddWithValue("@batch", request.BatchCount);
                cmd.Parameters.AddWithValue("@desc", request.Description ?? "");
                cmd.Parameters.AddWithValue("@sort", request.SortOrder);
                cmd.Parameters.AddWithValue("@enabled", request.IsEnabled);

                if (request.Id.HasValue && request.Id > 0)
                    cmd.Parameters.AddWithValue("@id", request.Id.Value);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存卷轴商品失败");
                return false;
            }
        }

        #endregion

        #region 鉴定逻辑

        /// <summary>
        /// 执行装备鉴定
        /// </summary>
        public async Task<AppraisalResult> AppraiseItemAsync(AppraisalRequest request)
        {
            try
            {
                // 获取卷轴等级
                var scrollLevel = GetScrollLevel(request.ScrollName);

                // 获取可用属性
                var allAttributes = await GetAllAttributesAsync();
                var availableAttributes = allAttributes
                    .Where(a => a.ScrollLevel <= scrollLevel && a.IsEnabled)
                    .ToList();

                if (availableAttributes.Count == 0)
                {
                    return new AppraisalResult
                    {
                        Success = false,
                        Message = "没有可用的鉴定属性"
                    };
                }

                // 计算成功率 (使用幸运符提升10%)
                var baseSuccessRate = 80;
                if (request.UseLuckyCharm)
                    baseSuccessRate = 90;

                // 判断是否成功
                if (_random.Next(100) >= baseSuccessRate)
                {
                    await LogAppraisalAsync(request, null, 0, false);
                    return new AppraisalResult
                    {
                        Success = false,
                        Message = "鉴定失败，装备属性未改变"
                    };
                }

                // 随机选择属性 (基于概率权重)
                var selectedAttr = SelectRandomAttribute(availableAttributes);
                if (selectedAttr == null)
                {
                    return new AppraisalResult
                    {
                        Success = false,
                        Message = "鉴定失败"
                    };
                }

                // 随机属性等级 (1 ~ maxLevel)
                var attrLevel = _random.Next(1, selectedAttr.MaxLevel + 1);

                // 记录日志
                await LogAppraisalAsync(request, selectedAttr.Name, attrLevel, true);

                return new AppraisalResult
                {
                    Success = true,
                    AttributeName = selectedAttr.Name,
                    AttributeLevel = attrLevel,
                    EffectType = selectedAttr.EffectType,
                    ShouldBroadcast = selectedAttr.Broadcast && selectedAttr.IsRare,
                    Message = $"恭喜！成功鉴定出 {selectedAttr.Name}+{attrLevel}"
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "鉴定执行失败");
                return new AppraisalResult
                {
                    Success = false,
                    Message = "鉴定过程发生错误"
                };
            }
        }

        /// <summary>
        /// 根据概率权重随机选择属性
        /// </summary>
        private AppraisalAttribute? SelectRandomAttribute(List<AppraisalAttribute> attributes)
        {
            var totalWeight = attributes.Sum(a => a.Probability);
            var randomValue = _random.Next(totalWeight);

            var currentWeight = 0;
            foreach (var attr in attributes)
            {
                currentWeight += attr.Probability;
                if (randomValue < currentWeight)
                    return attr;
            }

            return attributes.LastOrDefault();
        }

        /// <summary>
        /// 获取卷轴等级
        /// </summary>
        private int GetScrollLevel(string scrollName)
        {
            return scrollName switch
            {
                "一级卷轴" => 1,
                "二级卷轴" => 2,
                "三级卷轴" => 3,
                "神秘卷轴" => _random.Next(1, 4), // 随机1-3级
                _ => 1
            };
        }

        /// <summary>
        /// 记录鉴定日志
        /// </summary>
        private async Task LogAppraisalAsync(AppraisalRequest request, string? attrName, int level, bool success)
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"INSERT INTO appraisal_logs 
                    (account_id, char_name, item_name, scroll_name, result_attribute, result_level, is_success)
                    VALUES (@account, @char, @item, @scroll, @attr, @level, @success)", conn);

                cmd.Parameters.AddWithValue("@account", request.AccountId);
                cmd.Parameters.AddWithValue("@char", request.CharName);
                cmd.Parameters.AddWithValue("@item", request.ItemName);
                cmd.Parameters.AddWithValue("@scroll", request.ScrollName);
                cmd.Parameters.AddWithValue("@attr", attrName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@level", level);
                cmd.Parameters.AddWithValue("@success", success);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "记录鉴定日志失败");
            }
        }

        #endregion

        #region 鉴定统计

        /// <summary>
        /// 获取最近鉴定成功记录 (用于公告展示)
        /// </summary>
        public async Task<List<AppraisalLog>> GetRecentSuccessLogsAsync(int limit = 10)
        {
            var logs = new List<AppraisalLog>();
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"SELECT * FROM appraisal_logs 
                    WHERE is_success = 1 AND result_attribute IS NOT NULL
                    ORDER BY created_at DESC LIMIT @limit", conn);
                cmd.Parameters.AddWithValue("@limit", limit);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    logs.Add(new AppraisalLog
                    {
                        Id = reader.GetInt64("id"),
                        AccountId = reader.GetString("account_id"),
                        CharName = reader.GetString("char_name"),
                        ItemName = reader.GetString("item_name"),
                        ScrollName = reader.IsDBNull(reader.GetOrdinal("scroll_name")) ? null : reader.GetString("scroll_name"),
                        ResultAttribute = reader.IsDBNull(reader.GetOrdinal("result_attribute")) ? null : reader.GetString("result_attribute"),
                        ResultLevel = reader.GetInt32("result_level"),
                        IsSuccess = reader.GetBoolean("is_success"),
                        CreatedAt = reader.GetDateTime("created_at")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取鉴定记录失败");
            }
            return logs;
        }

        /// <summary>
        /// 获取鉴定统计
        /// </summary>
        public async Task<(int TotalCount, int SuccessCount, int RareCount)> GetAppraisalStatsAsync()
        {
            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                var cmd = new MySqlCommand(@"SELECT 
                    COUNT(*) as total,
                    SUM(CASE WHEN is_success = 1 THEN 1 ELSE 0 END) as success,
                    SUM(CASE WHEN is_broadcast = 1 THEN 1 ELSE 0 END) as rare
                    FROM appraisal_logs WHERE DATE(created_at) = CURDATE()", conn);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return (
                        reader.GetInt32("total"),
                        reader.GetInt32("success"),
                        reader.GetInt32("rare")
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取鉴定统计失败");
            }
            return (0, 0, 0);
        }

        #endregion

        #region 生成NPC脚本

        /// <summary>
        /// 自动生成卷轴商店NPC脚本
        /// </summary>
        public async Task<string> GenerateScrollShopScriptAsync()
        {
            var items = await GetAllScrollItemsAsync();
            var attrs = await GetAllAttributesAsync();

            var attrDesc = string.Join("、", attrs.Where(a => a.IsRare).Take(6).Select(a => a.Name));
            var script = $@"; ============================================
; 神秘卷轴商人 NPC脚本 (自动生成)
; ============================================

[@main]
#IF
#ACT
OPENMERCHANTBIGDLG 650 480 0 1,1,1,1
BIGHINTSHOW  <神秘属性可解读出→><FCOLOR=249>{attrDesc}</FCOLOR>
BIGHINTSHOW  <FCOLOR=250>鉴定装备</FCOLOR>  <FCOLOR=249>神秘解读</FCOLOR>  <FCOLOR=250>金刚石购卷</FCOLOR>
BIGHINTSHOW  ";

            foreach (var item in items.Where(i => i.ScrollLevel > 0))
            {
                script += $@"
BIGHINTSHOW  <{item.DisplayName}>  <FCOLOR=249>{item.PriceSingle}元宝购买1个/@购买{item.ItemName}1>      <{item.DisplayName}>  <FCOLOR=249>{item.PriceBatch}元宝购买{item.BatchCount}个/@购买{item.ItemName}{item.BatchCount}>";
            }

            script += @"
BIGHINTSHOW  
BIGHINTSHOW                  <FCOLOR=250>鉴定属性转移/@属性转移</FCOLOR>                    <FCOLOR=250>鉴定最新通知/@最新通知</FCOLOR>";

            // 生成购买脚本
            foreach (var item in items.Where(i => i.ScrollLevel > 0))
            {
                script += $@"

[@购买{item.ItemName}1]
#IF
CHECKGAMEGOLD >= {item.PriceSingle}
CHECKBAGSIZE 1
#ACT
GAMEGOLD - {item.PriceSingle}
GIVE {item.ItemName} 1
SENDMSG 5 恭喜 <$USERNAME> 购买了1个{item.DisplayName}！
GOTO @main
#ELSEACT
MESSAGEBOX 元宝不足或背包已满！购买1个{item.DisplayName}需要{item.PriceSingle}元宝。

[@购买{item.ItemName}{item.BatchCount}]
#IF
CHECKGAMEGOLD >= {item.PriceBatch}
CHECKBAGSIZE 1
#ACT
GAMEGOLD - {item.PriceBatch}
GIVE {item.ItemName} {item.BatchCount}
SENDMSG 5 恭喜 <$USERNAME> 批量购买了{item.BatchCount}个{item.DisplayName}！
GOTO @main
#ELSEACT
MESSAGEBOX 元宝不足或背包已满！批量购买{item.BatchCount}个{item.DisplayName}需要{item.PriceBatch}元宝。";
            }

            return script;
        }

        #endregion
    }
}
