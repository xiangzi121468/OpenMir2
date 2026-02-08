# OpenMir2 客户端编译指南

## 一、环境要求

### 1.1 必需软件

| 软件 | 版本 | 说明 |
|------|------|------|
| **Delphi** | 10.3 Rio 或更高 | 推荐 Delphi 11 Alexandria |
| **Windows** | 10/11 | 仅支持Windows |

> **注意**：Delphi是商业软件，需要购买许可证。也可以使用免费的 [Delphi Community Edition](https://www.embarcadero.com/products/delphi/starter)（限个人使用）。

### 1.2 项目依赖控件

客户端使用了多个第三方控件，需要先安装：

| 控件 | 目录 | 安装方式 |
|------|------|----------|
| **DevExpress VCL** | Components/DevExpressVCL | 运行 DxAutoInstaller.exe |
| **Raize Components** | Components/Raize | 打开 一键安装.groupproj |
| **OverbyteICS** | Components/OverbyteIcsV8 | 打开 OverbyteIcsV8.groupproj |
| **EmbeddedWB** | Components/EmbeddedWB | 安装 EmbeddedWebBrowser.dpk |
| **uFormDesigner** | Components/uFormDesinger | 安装 uFormDesigner.dpk |
| **DXSceneUI** | Source/SceneUI | 安装 DXSceneUIComponents.dpk |
| **cnvcl** | Components/cnvcl | 可选，按需安装 |
| **TMS Component Pack** | Components/TMS Component Pack | 可选 |

---

## 二、安装控件

### 2.1 安装 DevExpress VCL

```
1. 进入 MirClient/Components/DevExpressVCL/
2. 运行 DxAutoInstaller.exe
3. 选择 Delphi 版本
4. 点击 Install
5. 等待安装完成
```

### 2.2 安装 Raize Components

```
1. 打开 Delphi IDE
2. 菜单: File -> Open Project
3. 选择 MirClient/Components/Raize/一键安装.groupproj
4. 右键项目组 -> Build All
5. 右键每个 .dpk -> Install
```

### 2.3 安装 OverbyteICS

```
1. 打开 Delphi IDE
2. 菜单: File -> Open Project
3. 选择 MirClient/Components/OverbyteIcsV8/OverbyteIcsV8.groupproj
4. 右键 OverbyteIcsD*.dpk -> Build
5. 右键 OverbyteIcsDXe*.dpk -> Install
```

### 2.4 安装 EmbeddedWB

```
1. 打开 Delphi IDE
2. 菜单: File -> Open
3. 选择 MirClient/Components/EmbeddedWB/Source/EmbeddedWebBrowser.dpk
4. 右键项目 -> Build
5. 右键项目 -> Install
```

### 2.5 安装 uFormDesigner

```
1. 打开 Delphi IDE
2. 菜单: File -> Open
3. 选择 MirClient/Components/uFormDesinger/uFormDesigner.dpk
4. 右键项目 -> Build
5. 右键项目 -> Install
```

### 2.6 安装 DXSceneUI

```
1. 打开 Delphi IDE
2. 菜单: File -> Open
3. 选择 MirClient/Source/SceneUI/DXSceneUIComponents.dpk
4. 右键项目 -> Build
5. 右键项目 -> Install
```

---

## 三、编译客户端

### 3.1 打开项目

```
1. 打开 Delphi IDE
2. 菜单: File -> Open Project
3. 选择 MirClient/Source/MirClient/MirClinet.dproj
```

### 3.2 配置项目

```
1. 菜单: Project -> Options
2. 选择 Building -> Delphi Compiler
3. Target: Windows 32-bit (必须是32位)
4. Build Configuration: Release
```

### 3.3 设置搜索路径

如果编译报错找不到单元，需要添加搜索路径：

```
1. Project -> Options -> Delphi Compiler -> Search Path
2. 添加以下路径（相对于项目目录）:
   - ..\Common
   - ..\SceneUI
   - ..\SceneUI\PXL\Source
   - ..\..\Components\cnvcl\Source\Common
   - ..\..\Components\cnvcl\Source\Graphics
```

### 3.4 编译

```
1. 菜单: Project -> Build MirClinet (Shift+F9)
2. 等待编译完成
3. 输出文件: MirClient/Source/MirClient/Win32/Release/MirClinet.exe
```

---

## 四、配置客户端

### 4.1 创建客户端目录结构

```
GameClient/
├── MirClinet.exe          (编译输出)
├── Data/                   (资源文件)
│   ├── AccessoryItems.data
│   ├── Mon41.data - Mon46.data
│   ├── NewHum.data
│   └── ...
├── Map/                    (地图文件)
├── Sound/                  (音效文件)
├── Wav/                    (背景音乐)
├── !setup.txt              (服务器配置)
└── Mir2.ini                (客户端配置)
```

### 4.2 配置服务器地址

创建或编辑 `!setup.txt`:

```ini
[Server]
Title=传奇世界
ServerName=OpenMir2
ServerAddr=127.0.0.1
ServerPort=7000
```

### 4.3 复制资源文件

```batch
:: 从项目复制资源
xcopy /E /I MirClient\Data GameClient\Data
```

---

## 五、常见问题

### Q1: 编译报错 "Unit xxx not found"

**解决方法**：添加对应控件的源码路径到 Search Path

```
Project -> Options -> Delphi Compiler -> Search Path
添加缺失单元所在的目录
```

### Q2: 编译报错 "Undeclared identifier"

**解决方法**：确保所有依赖控件已正确安装

```
1. 检查控件是否安装成功（Component 菜单应有对应控件）
2. 重新安装对应控件
3. 重启 Delphi IDE
```

### Q3: 运行时报错 "DLL not found"

**解决方法**：复制必需的DLL到客户端目录

```
常见需要的DLL:
- bass.dll (音频)
- d3dx9_43.dll (DirectX)
```

### Q4: 资源文件格式错误

**解决方法**：确保Data目录下的.data文件完整

```
检查以下文件是否存在:
- Data/AccessoryItems.data
- Data/Mon41.data - Mon46.data
- Data/NewHum.data
- Data/WeaponEffect100.data
```

### Q5: 无法连接服务器

**检查项**：
1. 服务器是否启动
2. !setup.txt 中的IP和端口是否正确
3. 防火墙是否阻止连接

---

## 六、发布客户端

### 6.1 必需文件清单

```
GameClient/
├── MirClinet.exe           ✓ 主程序
├── Data/                   ✓ 资源目录
├── Map/                    ✓ 地图文件
├── Sound/                  ✓ 音效文件
├── Wav/                    ✓ 背景音乐
├── !setup.txt              ✓ 服务器配置
├── bass.dll                ✓ 音频库
├── d3dx9_43.dll            可选 (DirectX)
└── 其他资源文件
```

### 6.2 打包发布

```batch
:: 使用7-Zip打包
7z a -t7z GameClient.7z GameClient\* -mx=9

:: 或使用Inno Setup制作安装程序
```

---

## 七、开发调试

### 7.1 Debug模式

```
1. Project -> Options -> Building -> Delphi Compiler
2. Build Configuration: Debug
3. 勾选 Debug Information
4. 勾选 Local Symbols
```

### 7.2 本地测试

```pascal
// 在 MirClinet.dpr 中取消注释以下代码，可本地测试
if ParamStr(1) = '' then
begin
  ClientParamStr := '...加密的测试参数...';
end
```

### 7.3 日志输出

```pascal
// 添加控制台输出
{$DEFINE CONSOLE}
// 然后使用 WriteLn() 输出调试信息
```

---

## 八、项目结构

```
MirClient/
├── Source/
│   ├── MirClient/          ← 主程序
│   │   ├── MirClinet.dpr   ← 项目入口
│   │   ├── MirClinet.dproj ← 项目配置
│   │   ├── ClMain.pas      ← 主窗口
│   │   ├── PlayScn.pas     ← 游戏场景
│   │   ├── Actor.pas       ← 角色系统
│   │   └── ...
│   ├── Common/             ← 公共单元
│   ├── SceneUI/            ← 场景UI引擎
│   └── Plug-in/            ← 插件
├── Components/             ← 第三方控件
│   ├── DevExpressVCL/
│   ├── Raize/
│   ├── cnvcl/
│   └── ...
└── Data/                   ← 资源文件
```

---

## 九、快速参考

```
# 控件安装顺序
1. DevExpress VCL  (DxAutoInstaller.exe)
2. Raize           (一键安装.groupproj)
3. OverbyteICS     (OverbyteIcsV8.groupproj)
4. EmbeddedWB      (EmbeddedWebBrowser.dpk)
5. uFormDesigner   (uFormDesigner.dpk)
6. DXSceneUI       (DXSceneUIComponents.dpk)

# 编译命令
Project -> Build MirClinet (Shift+F9)

# 输出位置
MirClient/Source/MirClient/Win32/Release/MirClinet.exe
```

---

**版本**: v1.0  
**更新日期**: 2026-02-08
