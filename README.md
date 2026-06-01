# AutoCAD 卸载清理工具 v1.0

AutoCAD 残留物扫描与一键清理工具，支持 AutoCAD 2004 ~ 2027 全版本。

## 功能特性

- **多维度产品检测**：通过注册表、DLL 文件、卸载信息等多种方式检测已安装的 AutoCAD
- **6 类残留物扫描**：服务、进程、文件、注册表、快捷方式、卸载信息
- **4 步清理流程**：终止进程 → 停止服务 → 删除文件 → 清理注册表
- **图形化界面**：WinForms 界面，一键扫描、一键清理

## 技术栈

- **语言**：C#
- **框架**：.NET 9 / Windows Forms
- **平台**：Windows 10/11 (x64)
- **需要管理员权限运行**

## 项目结构

```
AutoCADUninstaller/
├── Models/
│   ├── ProductInfo.cs          # 产品信息模型
│   └── ResidueItem.cs          # 残留物信息模型
├── Services/
│   ├── RegistryHelper.cs       # 注册表读写操作
│   ├── ServiceHelper.cs        # Windows 服务控制
│   ├── ProcessHelper.cs        # 进程管理
│   ├── Scanner.cs              # 扫描服务（多维度检测）
│   └── Cleaner.cs              # 清理服务（4步清理）
├── MainForm.cs                 # 主窗体逻辑
├── MainForm.Designer.cs        # 主窗体 UI 布局
├── Program.cs                  # 程序入口（管理员检查）
└── app.manifest                # UAC 管理员权限清单
```

## 检测方式

| 检测维度 | 说明 |
|----------|------|
| HKLM 注册表 | `HKLM\SOFTWARE\Autodesk\AutoCAD\R{version}` |
| HKCU 注册表 | `HKCU\Software\Autodesk\AutoCAD\R{version}` |
| HKCR 文件关联 | `AutoCAD.Drawing.{version}` |
| 卸载信息 | `Uninstall\{GUID}` 读取 DisplayName/Publisher |
| MSI 安装器 | `Classes\Installer\Products\{GUID}` |
| 安装器数据 | `Installer\UserData\{SID}\{ProductCode}` |
| DLL 文件检测 | accore.dll、acdb*.dll、acge*.dll |
| WOW64 兼容 | 32位程序在64位系统上的注册信息 |

## 使用方法

### 前置要求

- Windows 10/11 (x64)
- [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)（精简版需要）

### 方式一：精简版（单文件，320KB）

下载 `AutoCADUninstaller.exe`，双击运行即可。需要电脑已安装 .NET 9 Runtime。

### 方式二：完整版（自包含，107MB）

下载整个 `publish_output` 文件夹，双击 `AutoCADUninstaller.exe` 运行。无需安装 .NET。

### 方式三：从源码编译

```bash
git clone https://github.com/ran0814/AutoCADUninstaller.git
cd AutoCADUninstaller
dotnet build -c Release
```

## 使用截图

```
┌─────────────────────────────────────────────────────┐
│  AutoCAD 卸载清理工具 v1.0                           │
├─────────────────────────────────────────────────────┤
│  已安装的 AutoCAD:                                   │
│  ☑ AutoCAD 2024 (R28.0)                            │
│  ☑ AutoCAD 2025 (R29.0)                            │
│                                                     │
│  [开始扫描]  [开始清理]  [退出]                      │
├─────────────────────────────────────────────────────┤
│  扫描结果:                                          │
│  ├─ 服务残留: 2 项                                  │
│  ├─ 文件残留: 15 项 (2.3 GB)                       │
│  └─ 注册表残留: 8 项                                │
├─────────────────────────────────────────────────────┤
│  [████████████░░░░░░░░] 60%  正在清理...            │
└─────────────────────────────────────────────────────┘
```

## 学习价值

本项目涵盖 Windows 系统编程的核心知识点：

| 技术 | 应用场景 |
|------|----------|
| **注册表操作** | 检测安装状态、清理残留键值 |
| **进程管理** | 终止 AutoCAD 相关进程 |
| **服务控制** | 停止/删除 Autodesk Windows 服务 |
| **文件系统操作** | 清理安装目录和用户数据 |
| **异步编程** | async/await 避免界面卡顿 |
| **WinForms UI** | DataGridView、进度条、日志显示 |

## 注意事项

- 本工具需要**管理员权限**运行（删除系统文件和注册表需要提权）
- 清理前建议关闭所有 AutoCAD 相关程序
- 清理完成后建议重启计算机
- 本工具仅供学习交流使用

## 许可证

MIT License
