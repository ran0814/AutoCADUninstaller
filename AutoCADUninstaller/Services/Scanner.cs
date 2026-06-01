// ============================================================================
// Scanner.cs - 扫描服务（完整版 - 对照 Autobox 逆向分析）
// ============================================================================
// 完全对照 Autobox 的多维度检测方法：
//
// 检测维度：
//   1. HKLM\SOFTWARE\Autodesk\AutoCAD\R{version}              → 产品注册表
//   2. HKCU\Software\Autodesk\AutoCAD\R{version}              → 用户级注册表
//   3. Uninstall\{GUID} 读取 DisplayName                       → 卸载信息
//   4. Classes\Installer\Products\{GUID}                       → MSI 安装器
//   5. Installer\UserData\{SID}\{ProductCode}                  → 安装器用户数据
//   6. HKCR\AutoCAD.Drawing.{version}                          → 文件关联
//   7. DLL 文件检测 (accore.dll, acdb*.dll, acge*.dll)         → 文件系统
//   8. HKLM\SOFTWARE\WOW6432Node\Autodesk                     → 32位兼容
//   9. AppDataLow\Software\Autodesk                            → 低权限数据
//  10. 文件系统扫描 (Program Files, ProgramData, AppData)       → 文件残留
//  11. 桌面/开始菜单快捷方式                                    → 快捷方式残留
// ============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoCADUninstaller.Models;

namespace AutoCADUninstaller.Services
{
    /// <summary>
    /// 扫描结果汇总
    /// </summary>
    public class ScanResult
    {
        public List<ResidueItem> AllResidues { get; set; } = new List<ResidueItem>();

        public int ServiceCount => AllResidues.Count(r => r.Type == ResidueType.Service);
        public int ProcessCount => AllResidues.Count(r => r.Type == ResidueType.Process);
        public int FileCount => AllResidues.Count(r => r.Type == ResidueType.File);
        public int RegistryCount => AllResidues.Count(r => r.Type == ResidueType.Registry);
        public int ShortcutCount => AllResidues.Count(r => r.Type == ResidueType.Shortcut);
        public int UninstallCount => AllResidues.Count(r => r.Type == ResidueType.UninstallEntry);
        public int TotalCount => AllResidues.Count;

        public long TotalSize => AllResidues
            .Where(r => r.Type == ResidueType.File)
            .Sum(r => r.Size);

        public string TotalSizeDisplay
        {
            get
            {
                long size = TotalSize;
                if (size >= 1024L * 1024 * 1024)
                    return $"{size / (1024.0 * 1024 * 1024):F1} GB";
                if (size >= 1024L * 1024)
                    return $"{size / (1024.0 * 1024):F1} MB";
                if (size >= 1024)
                    return $"{size / 1024.0:F1} KB";
                return $"{size} B";
            }
        }
    }

    /// <summary>
    /// 扫描服务 - 对照 Autobox 的完整扫描逻辑
    /// </summary>
    public class Scanner
    {
        // ====================================================================
        // 检测已安装的 AutoCAD（多维度）
        // ====================================================================

        /// <summary>
        /// 多维度检测已安装的 AutoCAD
        /// 对照 Autobox: DetailedCheckInstalled() = CheckInstalledByDll() || ByRegistry()
        /// </summary>
        public List<ProductInfo> DetectInstalledProducts()
        {
            var products = new List<ProductInfo>();
            var detectedVersions = new HashSet<string>(); // 去重

            // === 维度1: HKLM 注册表 ===
            List<string> hklmVersions = RegistryHelper.GetInstalledVersions();
            foreach (string version in hklmVersions)
            {
                if (detectedVersions.Add(version))
                {
                    string path = RegistryHelper.GetAutoCADPath(version);
                    products.Add(CreateProductInfo(version, path, "HKLM 注册表"));
                }
            }

            // === 维度2: HKCU 用户级注册表 ===
            List<string> hkcuVersions = RegistryHelper.GetUserLevelVersions();
            foreach (string version in hkcuVersions)
            {
                if (detectedVersions.Add(version))
                {
                    products.Add(CreateProductInfo(version, null, "HKCU 用户注册表"));
                }
            }

            // === 维度3: 卸载信息（读取 DisplayName）===
            List<UninstallEntryInfo> uninstallEntries = RegistryHelper.GetUninstallEntries();
            foreach (var entry in uninstallEntries)
            {
                // 尝试从 DisplayName 中提取版本
                string extractedVersion = ExtractVersionFromDisplayName(entry.DisplayName);
                if (!string.IsNullOrEmpty(extractedVersion) && detectedVersions.Add(extractedVersion))
                {
                    products.Add(new ProductInfo
                    {
                        ProductName = entry.DisplayName,
                        Version = extractedVersion,
                        InstallPath = entry.InstallLocation,
                        RegistryKeyPath = entry.KeyPath,
                        IsInstalled = true,
                        IsSelected = true
                    });
                }
                else if (!string.IsNullOrEmpty(entry.DisplayName) &&
                         entry.DisplayName.IndexOf("AutoCAD", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // 找到 AutoCAD 相关卸载条目但无法提取版本
                    products.Add(new ProductInfo
                    {
                        ProductName = entry.DisplayName,
                        Version = "Unknown",
                        InstallPath = entry.InstallLocation,
                        RegistryKeyPath = entry.KeyPath,
                        IsInstalled = true,
                        IsSelected = true
                    });
                }
            }

            // === 维度4: DLL 文件检测（Autobox: CheckInstalledByDll）===
            List<string> dllDetectedPaths = DetectByDllFiles();
            foreach (string dllPath in dllDetectedPaths)
            {
                // 尝试从路径中提取版本
                string version = ExtractVersionFromPath(dllPath);
                if (!string.IsNullOrEmpty(version) && detectedVersions.Add(version))
                {
                    products.Add(CreateProductInfo(version, Path.GetDirectoryName(dllPath), "DLL 文件检测"));
                }
            }

            return products;
        }

        /// <summary>
        /// 创建产品信息对象
        /// </summary>
        private ProductInfo CreateProductInfo(string version, string installPath, string source)
        {
            string year = ProductInfo.GetYearFromVersion(version);
            string displayName = year != "Unknown"
                ? $"AutoCAD {year}"
                : $"AutoCAD ({version})";

            return new ProductInfo
            {
                ProductName = displayName,
                Version = version,
                InstallPath = installPath,
                RegistryKeyPath = $@"HKLM\SOFTWARE\Autodesk\AutoCAD\{version}",
                IsInstalled = true,
                IsSelected = true
            };
        }

        // ====================================================================
        // DLL 文件检测（Autobox: CheckInstalledByDll）
        // ====================================================================

        /// <summary>
        /// 通过检测关键 DLL 文件来判断 AutoCAD 是否安装
        /// Autobox 会检查 accore.dll, acdb*.dll, acge*.dll 等
        /// </summary>
        private List<string> DetectByDllFiles()
        {
            var foundPaths = new List<string>();

            // Autobox 检测的关键 DLL
            string[] criticalDlls = {
                "accore.dll",       // AutoCAD 核心库
                "acdb*.dll",        // AutoCAD 数据库
                "acge*.dll",        // AutoCAD 几何引擎
                "acmgd.dll",        // AutoCAD Managed
                "AdWindows.dll",    // AutoCAD 窗口
            };

            // 常见安装路径
            string[] searchRoots = {
                @"C:\Program Files\Autodesk",
                @"C:\Program Files (x86)\Autodesk",
                @"C:\ProgramData\Autodesk"
            };

            foreach (string root in searchRoots)
            {
                if (!Directory.Exists(root)) continue;

                try
                {
                    // 搜索 acad.exe（最直接的证据）
                    string[] exeFiles = Directory.GetFiles(root, "acad.exe",
                        SearchOption.AllDirectories);
                    foreach (string exe in exeFiles)
                    {
                        foundPaths.Add(exe);
                    }

                    // 搜索关键 DLL
                    foreach (string dllPattern in criticalDlls)
                    {
                        try
                        {
                            string[] dllFiles = Directory.GetFiles(root, dllPattern,
                                SearchOption.AllDirectories);
                            foreach (string dll in dllFiles)
                            {
                                string dir = Path.GetDirectoryName(dll);
                                if (!foundPaths.Contains(dir))
                                {
                                    foundPaths.Add(dir);
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            // 也检查用户的 AppData
            try
            {
                string appData = Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData);
                string localAppData = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);

                string[] userPaths = {
                    Path.Combine(appData, "Autodesk"),
                    Path.Combine(localAppData, "Autodesk")
                };

                foreach (string userPath in userPaths)
                {
                    if (Directory.Exists(userPath))
                    {
                        try
                        {
                            string[] files = Directory.GetFiles(userPath, "*.dll",
                                SearchOption.AllDirectories);
                            foreach (string dll in files)
                            {
                                string fileName = Path.GetFileName(dll).ToLower();
                                if (fileName.StartsWith("ac") || fileName.StartsWith("ad"))
                                {
                                    string dir = Path.GetDirectoryName(dll);
                                    if (!foundPaths.Contains(dir))
                                    {
                                        foundPaths.Add(dir);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return foundPaths;
        }

        // ====================================================================
        // 残留物扫描（完整版）
        // ====================================================================

        /// <summary>
        /// 扫描所有类型的残留物 - 对照 Autobox DetectAllResidues()
        /// </summary>
        public async Task<ScanResult> ScanAllResiduesAsync(IProgress<string> progress = null)
        {
            var result = new ScanResult();

            // 1. 服务残留
            progress?.Report("正在扫描服务残留...");
            result.AllResidues.AddRange(await ScanServiceResiduesAsync());

            // 2. 进程残留
            progress?.Report("正在扫描进程残留...");
            result.AllResidues.AddRange(ScanProcessResidues());

            // 3. 文件残留
            progress?.Report("正在扫描文件残留...");
            result.AllResidues.AddRange(await ScanFileResiduesAsync(progress));

            // 4. 注册表残留（增强版 - 使用 GetAllAutodeskRegistryEntries）
            progress?.Report("正在扫描注册表残留（多维度）...");
            result.AllResidues.AddRange(ScanRegistryResiduesEnhanced(progress));

            // 5. 快捷方式残留
            progress?.Report("正在扫描快捷方式残留...");
            result.AllResidues.AddRange(ScanShortcutResidues());

            // 6. 卸载信息残留（增强版 - 读取 DisplayName）
            progress?.Report("正在扫描卸载信息残留...");
            result.AllResidues.AddRange(ScanUninstallResiduesEnhanced());

            progress?.Report($"扫描完成，共发现 {result.TotalCount} 项残留物");

            return result;
        }

        // ====================================================================
        // 各类型的扫描实现
        // ====================================================================

        private async Task<List<ResidueItem>> ScanServiceResiduesAsync()
        {
            return await Task.Run(() =>
            {
                var residues = new List<ResidueItem>();
                var services = ServiceHelper.GetAutodeskServices();
                foreach (var service in services)
                {
                    residues.Add(new ResidueItem
                    {
                        Name = service.DisplayName,
                        Path = $"服务: {service.Name}",
                        Type = ResidueType.Service,
                        ProductName = "Autodesk",
                        IsSelected = true,
                        Risk = RiskLevel.Medium,
                        Note = $"状态: {service.Status}"
                    });
                }
                return residues;
            });
        }

        private List<ResidueItem> ScanProcessResidues()
        {
            var residues = new List<ResidueItem>();
            var processes = ProcessHelper.GetAutodeskProcesses();
            foreach (var proc in processes)
            {
                residues.Add(new ResidueItem
                {
                    Name = proc.Name,
                    Path = proc.FilePath,
                    Type = ResidueType.Process,
                    ProductName = "Autodesk",
                    IsSelected = true,
                    Risk = RiskLevel.Low,
                    Note = $"PID: {proc.Id}, 内存: {proc.MemoryUsage}"
                });
            }
            return residues;
        }

        private async Task<List<ResidueItem>> ScanFileResiduesAsync(IProgress<string> progress = null)
        {
            return await Task.Run(() =>
            {
                var residues = new List<ResidueItem>();

                // Autobox 检测的文件系统路径
                string[] possiblePaths = {
                    @"C:\Program Files\Autodesk",
                    @"C:\Program Files (x86)\Autodesk",
                    @"C:\ProgramData\Autodesk",
                    Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData), "Autodesk"),
                    Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData), "Autodesk"),
                    Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.CommonApplicationData), "Autodesk"),
                    // Autodesk 桌面应用路径
                    @"C:\Program Files\Autodesk\Autodesk Desktop App",
                    @"C:\Program Files (x86)\Autodesk\Autodesk Desktop App"
                };

                foreach (string basePath in possiblePaths)
                {
                    try
                    {
                        if (Directory.Exists(basePath))
                        {
                            progress?.Report($"  扫描目录: {basePath}");
                            long dirSize = CalculateDirectorySize(basePath);

                            residues.Add(new ResidueItem
                            {
                                Name = Path.GetFileName(basePath),
                                Path = basePath,
                                Type = ResidueType.File,
                                Size = dirSize,
                                ProductName = "Autodesk",
                                IsSelected = true,
                                Risk = dirSize > 1024L * 1024 * 1024 ?
                                    RiskLevel.High : RiskLevel.Medium,
                                Note = $"包含子目录和文件"
                            });
                        }
                    }
                    catch { }
                }

                // 桌面快捷方式
                string desktopPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Desktop);
                ScanShortcutsInDirectory(desktopPath, residues, "桌面");

                // 公共桌面
                string publicDesktop = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs");
                if (Directory.Exists(publicDesktop))
                {
                    ScanShortcutsInDirectory(publicDesktop, residues, "开始菜单(公共)");
                }

                return residues;
            });
        }

        /// <summary>
        /// 增强版注册表残留扫描 - 使用 GetAllAutodeskRegistryEntries
        /// 对照 Autobox 的 DetectRegistryResidues()
        /// </summary>
        private List<ResidueItem> ScanRegistryResiduesEnhanced(IProgress<string> progress = null)
        {
            var residues = new List<ResidueItem>();

            // 使用增强版方法获取所有 Autodesk 注册表条目
            var entries = RegistryHelper.GetAllAutodeskRegistryEntries();

            foreach (var entry in entries)
            {
                residues.Add(new ResidueItem
                {
                    Name = entry.KeyName,
                    Path = entry.FullPath,
                    Type = ResidueType.Registry,
                    ProductName = "Autodesk",
                    IsSelected = true,
                    Risk = RiskLevel.Low,
                    Note = $"值: {entry.Value ?? "(空)"} | 根键: {entry.RootHive}"
                });
            }

            return residues;
        }

        private List<ResidueItem> ScanShortcutResidues()
        {
            var residues = new List<ResidueItem>();

            string startMenuPath = Environment.GetFolderPath(
                Environment.SpecialFolder.StartMenu);

            try
            {
                if (Directory.Exists(startMenuPath))
                {
                    string programsPath = Path.Combine(startMenuPath, "Programs");
                    if (Directory.Exists(programsPath))
                    {
                        ScanShortcutsInDirectory(programsPath, residues, "开始菜单");
                    }
                }
            }
            catch { }

            return residues;
        }

        /// <summary>
        /// 增强版卸载信息扫描 - 读取 DisplayName、Publisher 等值
        /// </summary>
        private List<ResidueItem> ScanUninstallResiduesEnhanced()
        {
            var residues = new List<ResidueItem>();

            var uninstallEntries = RegistryHelper.GetUninstallEntries();

            foreach (var entry in uninstallEntries)
            {
                residues.Add(new ResidueItem
                {
                    Name = entry.DisplayName,
                    Path = entry.KeyPath,
                    Type = ResidueType.UninstallEntry,
                    ProductName = "Autodesk",
                    IsSelected = true,
                    Risk = RiskLevel.Low,
                    Note = $"发布者: {entry.Publisher} | 版本: {entry.DisplayVersion} | 路径: {entry.InstallLocation}"
                });
            }

            return residues;
        }

        // ====================================================================
        // 辅助方法
        // ====================================================================

        /// <summary>
        /// 在指定目录中扫描 Autodesk 相关快捷方式
        /// </summary>
        private void ScanShortcutsInDirectory(string dirPath, List<ResidueItem> residues, string source)
        {
            try
            {
                if (!Directory.Exists(dirPath)) return;

                string[] files = Directory.GetFiles(dirPath, "*.lnk", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (name.IndexOf("AutoCAD", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Autodesk", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        residues.Add(new ResidueItem
                        {
                            Name = name,
                            Path = file,
                            Type = ResidueType.Shortcut,
                            ProductName = "Autodesk",
                            IsSelected = true,
                            Risk = RiskLevel.Low,
                            Note = $"来源: {source}"
                        });
                    }
                }
            }
            catch { }
        }

        private long CalculateDirectorySize(string dirPath)
        {
            long size = 0;
            try
            {
                foreach (string file in Directory.GetFiles(dirPath))
                {
                    try { size += new FileInfo(file).Length; } catch { }
                }
                foreach (string dir in Directory.GetDirectories(dirPath))
                {
                    size += CalculateDirectorySize(dir);
                }
            }
            catch { }
            return size;
        }

        /// <summary>
        /// 从路径中提取 AutoCAD 版本号
        /// 例如: "C:\...\AutoCAD 2024\..." -> "R28.0"
        /// </summary>
        private string ExtractVersionFromPath(string path)
        {
            // 尝试匹配路径中的年份
            for (int year = 2027; year >= 2004; year--)
            {
                if (path.Contains(year.ToString()))
                {
                    // 查找对应的内部版本号
                    foreach (var kvp in ProductInfo.GetAllSupportedVersions()
                        .Select(v => new { Version = v, Year = ProductInfo.GetYearFromVersion(v) }))
                    {
                        if (kvp.Year == year.ToString())
                            return kvp.Version;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 从 DisplayName 中提取版本号
        /// 例如: "AutoCAD 2024 - English" -> "R28.0"
        /// </summary>
        private string ExtractVersionFromDisplayName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName)) return null;

            // 尝试匹配 "AutoCAD 2024" 中的年份
            for (int year = 2027; year >= 2004; year--)
            {
                if (displayName.Contains(year.ToString()))
                {
                    foreach (var version in ProductInfo.GetAllSupportedVersions())
                    {
                        if (ProductInfo.GetYearFromVersion(version) == year.ToString())
                            return version;
                    }
                }
            }
            return null;
        }
    }
}
