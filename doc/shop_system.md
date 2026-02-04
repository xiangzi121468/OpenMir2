# 元宝商城系统

## 系统概述

完整的游戏内元宝商城系统，包括：
- 商品管理与购买
- 充值系统
- 礼包码兑换
- VIP等级系统

## 数据库结构

### 核心表

| 表名 | 说明 |
|------|------|
| shop_categories | 商品分类 |
| shop_items | 商城商品 |
| shop_orders | 购买订单 |
| recharge_records | 充值记录 |
| recharge_packages | 充值档位 |
| gift_codes | 礼包码 |
| gift_code_records | 礼包码使用记录 |
| vip_levels | VIP等级配置 |
| user_vip | 用户VIP信息 |

### 初始化数据库

```bash
mysql -u root -p mir2_data < sql/shop_system.sql
```

## 模块结构

```
src/Modules/ShopModule/
├── Models/
│   └── ShopModels.cs          # 数据模型
├── Repository/
│   ├── IShopRepository.cs     # 仓储接口
│   └── MySqlShopRepository.cs # MySQL实现
├── IShopService.cs            # 服务接口
├── ShopService.cs             # 服务实现
├── ShopScriptProcessor.cs     # 脚本处理器
└── ShopModule.csproj          # 项目文件
```

## API接口

### 商城API

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/shop/categories | GET | 获取商品分类 |
| /api/shop/items?categoryId=1 | GET | 获取分类商品 |
| /api/shop/hot | GET | 获取热销商品 |
| /api/shop/new | GET | 获取新品 |
| /api/shop/search?keyword=xx | GET | 搜索商品 |
| /api/shop/item/{id} | GET | 商品详情 |
| /api/shop/orders?accountId=xx | GET | 用户订单 |

### 充值API

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/pay/packages | GET | 充值档位列表 |
| /api/pay/create | POST | 创建充值订单 |
| /api/pay/notify/alipay | POST | 支付宝回调 |
| /api/pay/notify/wechat | POST | 微信回调 |
| /api/pay/records?accountId=xx | GET | 充值记录 |
| /api/pay/isfirst?accountId=xx | GET | 是否首充 |

### VIP API

| 接口 | 方法 | 说明 |
|------|------|------|
| /api/shop/vip/levels | GET | VIP等级配置 |
| /api/shop/vip/info?accountId=xx | GET | 用户VIP信息 |

## GM命令

| 命令 | 权限 | 说明 |
|------|------|------|
| @SendGold 角色名 数量 | 10 | 发放元宝 |
| @QueryGold [角色名] | 6 | 查询元宝 |
| @SendGoldAll 数量 | 10 | 全服发放 |
| @SetVip 角色名 等级 | 10 | 设置VIP |
| @GenGiftCode 名称 元宝 数量 | 10 | 生成礼包码 |

## NPC脚本命令

### 条件命令

```
; 检查元宝
CHECKGAMEGOLD > 100

; 检查VIP等级
CHECKVIP >= 3

; 检查是否首充
ISFIRSTRECHARGE
```

### 执行命令

```
; 扣除元宝
GAMEGOLD - 100

; 增加元宝
GAMEGOLD + 50

; 打开商城 (需客户端支持)
OPENSHOP 1

; 打开充值 (需客户端支持)
OPENRECHARGE
```

## VIP等级配置

| 等级 | 累计充值 | 经验加成 | 掉落加成 | 商城折扣 |
|:----:|:--------:|:--------:|:--------:|:--------:|
| VIP1 | 100 | +5% | +2% | 98折 |
| VIP2 | 500 | +10% | +5% | 95折 |
| VIP3 | 1000 | +15% | +8% | 92折 |
| VIP4 | 3000 | +20% | +10% | 90折 |
| VIP5 | 5000 | +25% | +12% | 88折 |
| VIP6 | 10000 | +30% | +15% | 85折 |
| VIP7 | 20000 | +35% | +18% | 82折 |
| VIP8 | 50000 | +40% | +20% | 80折 |
| VIP9 | 100000 | +50% | +25% | 75折 |
| VIP10 | 200000 | +60% | +30% | 70折 |

## 充值档位

| 金额(元) | 基础元宝 | 首充赠送 | 总计(首充) |
|:--------:|:--------:|:--------:|:----------:|
| 6 | 60 | 6 | 66 |
| 30 | 300 | 50 | 350 |
| 98 | 980 | 200 | 1180 |
| 198 | 1980 | 500 | 2480 |
| 328 | 3280 | 1000 | 4280 |
| 648 | 6480 | 2500 | 8980 |

## 礼包码格式

```json
{
  "code": "WELCOME2024",
  "name": "新手礼包",
  "gameGold": 100,
  "gamePoint": 0,
  "items": "[{\"name\":\"金创药(大量)\",\"count\":50}]"
}
```

## 集成步骤

### 1. 导入数据库

```bash
mysql -u root -p mir2_data < sql/shop_system.sql
```

### 2. 注册服务

在 `Program.cs` 或 DI 容器中注册：

```csharp
services.AddSingleton<IShopRepository>(sp => 
    new MySqlShopRepository(connectionString));
services.AddSingleton<IShopService, ShopService>();
services.AddSingleton<ShopScriptProcessor>();
```

### 3. 配置NPC

将 `商城管理员-0.txt` 放到 `Envir/Market_Def/` 目录

### 4. 添加NPC到地图

在 `MerChant.txt` 中添加：
```
商城管理员 3 330 330 0 0 0 0
```

## 支付对接

### 支付宝

1. 创建订单后，使用返回的订单信息跳转到支付宝支付页
2. 配置异步通知地址: `https://yourserver.com/api/pay/notify/alipay`
3. 验证签名后处理回调

### 微信支付

1. 创建订单后，使用返回的订单信息调起微信支付
2. 配置异步通知地址: `https://yourserver.com/api/pay/notify/wechat`
3. 验证签名后处理回调

## 注意事项

1. 生产环境需要配置真实的支付密钥
2. 支付回调需要验证签名
3. 元宝发放要做好幂等处理
4. 建议添加订单超时自动取消
5. VIP等级加成需要在游戏逻辑中应用
