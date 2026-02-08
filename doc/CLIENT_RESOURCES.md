# OpenMir2 客户端资源获取指南

## 一、资源下载站点

### 1.1 LOMCN 官方资源站（推荐）

**主站点**: https://www.mirfiles.com/resources/mir2/

这是传奇开发社区最全面的资源站，包含：

| 目录 | 大小 | 说明 |
|------|------|------|
| [clients/](https://www.mirfiles.com/resources/mir2/clients/) | ~315GB | 各版本完整客户端 |
| [Data/](https://www.mirfiles.com/resources/mir2/Data/) | ~30GB | 客户端资源文件 |
| [Maps/](https://www.mirfiles.com/resources/mir2/Maps/) | ~6.3GB | 地图文件 |
| [Tools/](https://www.mirfiles.com/resources/mir2/Tools/) | ~365MB | 开发工具 |
| [crystal/](https://www.mirfiles.com/resources/mir2/crystal/) | ~30GB | Crystal客户端资源 |

### 1.2 直接下载链接

**完整客户端下载**:
- http://mirfiles.game-server.cc/

| 版本 | 大小 | 适用 |
|------|------|------|
| Mir 2 Client 1.4 | 281 MB | 早期版本 |
| Mir 2 Client 1.9 | 445 MB | 经典版本 |
| Mir 2 Client 2.3 | 531 MB | 推荐版本 |
| Crystal Client | ~1GB | 最新功能 |

---

## 二、推荐下载方案

### 方案A：下载完整客户端（最简单）

```
1. 访问 https://www.mirfiles.com/resources/mir2/clients/
2. 下载 "Mir2 Client 2.3" 或更高版本
3. 解压后即可使用
```

### 方案B：只下载资源文件

如果你只需要资源文件（Data/Map/Sound），可以单独下载：

```
1. Data文件: https://www.mirfiles.com/resources/mir2/Data/
2. Map文件:  https://www.mirfiles.com/resources/mir2/Maps/
```

---

## 三、资源整合步骤

### 3.1 下载资源

```powershell
# 创建下载目录
mkdir D:\MirResources

# 使用下载工具下载（推荐使用 IDM 或 FDM）
# 下载以下文件到 D:\MirResources:
# - 完整客户端 或 Data目录内容
```

### 3.2 复制资源到项目

```powershell
# 假设你下载的客户端解压到 D:\MirResources\Mir2Client

# 复制 Data 资源
xcopy /E /I "D:\MirResources\Mir2Client\Data" "D:\aiwork\OpenMir2\MirClient\Data"

# 复制 Map 资源
xcopy /E /I "D:\MirResources\Mir2Client\Map" "D:\aiwork\OpenMir2\MirClient\Map"

# 复制 Sound 资源
xcopy /E /I "D:\MirResources\Mir2Client\Sound" "D:\aiwork\OpenMir2\MirClient\Sound"

# 复制 Wav 资源
xcopy /E /I "D:\MirResources\Mir2Client\Wav" "D:\aiwork\OpenMir2\MirClient\Wav"
```

### 3.3 保留新增资源

注意：复制时保留我们新增的资源文件：

```
保留的新增资源（不要覆盖）:
- Data/AccessoryItems.data   (新配饰)
- Data/Mon41-46.data         (新怪物)
- Data/NewHum.data           (新衣服)
- Data/WeaponEffect100.data  (新武器特效)
- Data/ArmorEffect.data      (衣服特效)
```

---

## 四、盛大官方客户端

### 4.1 官方下载

**官网**: https://mir2.sdo.com/

下载地址: https://f.sdo.com/mir2

| 版本 | 大小 | 说明 |
|------|------|------|
| 客户端下载器 | 124 MB | 自动下载完整客户端 |
| 完整客户端 | ~8.86 GB | 包含所有资源 |

### 4.2 使用官方资源

```powershell
# 下载并安装官方客户端到默认目录
# 通常是 C:\Program Files\盛大游戏\热血传奇

# 复制资源
xcopy /E /I "C:\Program Files\盛大游戏\热血传奇\Data" "D:\aiwork\OpenMir2\MirClient\Data"
xcopy /E /I "C:\Program Files\盛大游戏\热血传奇\Map" "D:\aiwork\OpenMir2\MirClient\Map"
```

---

## 五、必需资源文件清单

下载后请确保以下文件存在：

### 5.1 核心界面资源
```
Data/
├── Prguse.wil 或 Prguse.data    (主界面)
├── Prguse2.wil 或 Prguse2.data  (主界面2)
├── Prguse3.wil 或 Prguse3.data  (主界面3)
├── ChrSel.wil 或 ChrSel.data    (角色选择)
└── ui1.wil 或 ui1.data          (UI界面)
```

### 5.2 人物资源
```
Data/
├── Hum.wil 或 Hum.data          (人物1)
├── Hum2.wil 或 Hum2.data        (人物2)
├── Hair.wil 或 Hair.data        (头发)
├── Weapon.wil 或 Weapon.data    (武器1)
└── Weapon2.wil 或 Weapon2.data  (武器2)
```

### 5.3 怪物资源
```
Data/
├── Mon1.wil - Mon40.wil         (基础怪物)
└── Mon41.data - Mon46.data      (新增怪物，已有)
```

### 5.4 魔法/物品资源
```
Data/
├── Magic.wil - Magic10.wil      (魔法效果)
├── Items.wil 或 Items.data      (物品图标)
└── DnItems.wil 或 DnItems.data  (地面物品)
```

### 5.5 地图资源
```
Data/
├── Tiles.wil 或 Tiles.data      (地图瓦片)
├── SmTiles.wil 或 SmTiles.data  (小地图)
└── Objects.wil 或 Objects.data  (地图对象)

Map/
└── *.map                        (地图数据文件)
```

---

## 六、资源格式说明

传奇客户端资源有两种格式：

| 格式 | 说明 | 兼容性 |
|------|------|--------|
| `.wil` | 传统格式 | 所有版本 |
| `.data` | 新格式（加密） | 新版客户端 |

你的客户端代码（`WIL.pas`）应该同时支持两种格式。

---

## 七、下载工具推荐

由于资源文件较大，建议使用专业下载工具：

| 工具 | 说明 |
|------|------|
| **IDM** (Internet Download Manager) | 多线程下载，续传支持 |
| **FDM** (Free Download Manager) | 免费，支持多线程 |
| **aria2** | 命令行工具，支持断点续传 |

### aria2 下载示例

```bash
# 下载完整客户端
aria2c -x 16 -s 16 "https://www.mirfiles.com/resources/mir2/clients/Mir2_Client_2.3.zip"
```

---

## 八、常见问题

### Q1: 下载速度慢
mirfiles.com 服务器在国外，建议：
- 使用VPN加速
- 选择非高峰时段下载
- 使用多线程下载工具

### Q2: 资源格式不匹配
如果下载的是 .wil 格式，而代码需要 .data 格式：
- 使用 WIL 编辑器转换
- 或修改代码同时支持两种格式

### Q3: 部分资源缺失
可以从多个来源组合资源：
- LOMCN 资源站
- 盛大官方客户端
- 其他开源项目

---

## 九、快速开始

```powershell
# 1. 下载完整客户端（推荐 2.3 版本）
# 访问: https://www.mirfiles.com/resources/mir2/clients/

# 2. 解压到临时目录
# D:\MirResources\Mir2Client\

# 3. 复制资源到项目（保留新增资源）
robocopy "D:\MirResources\Mir2Client\Data" "D:\aiwork\OpenMir2\MirClient\Data" /E /XF AccessoryItems.data Mon41.data Mon42.data Mon43.data Mon44.data Mon45.data Mon46.data NewHum.data WeaponEffect100.data

# 4. 复制地图
xcopy /E /I "D:\MirResources\Mir2Client\Map" "D:\aiwork\OpenMir2\MirClient\Map"

# 5. 复制音效
xcopy /E /I "D:\MirResources\Mir2Client\Sound" "D:\aiwork\OpenMir2\MirClient\Sound"
xcopy /E /I "D:\MirResources\Mir2Client\Wav" "D:\aiwork\OpenMir2\MirClient\Wav"
```

---

## 十、参考链接

- **LOMCN 社区**: https://www.lomcn.net/
- **资源下载站**: https://www.mirfiles.com/resources/mir2/
- **客户端下载**: http://mirfiles.game-server.cc/
- **盛大官网**: https://mir2.sdo.com/
- **GitHub MirClient**: https://github.com/mirbeta/MirClient

---

**版本**: v1.0  
**更新日期**: 2026-02-08
