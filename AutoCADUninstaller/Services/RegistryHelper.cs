// ============================================================================
// RegistryHelper.cs - 注册表帮助类（完整版）
// ============================================================================
// 负责所有 Windows 注册表的读写操作
// 完全对照 Autobox 逆向分析的所有注册表路径
//
// 注册表是什么？
// Windows 注册表是一个分层数据库，存储系统和应用程序的配置信息
// 可以理解为 Windows 的"设置中心"
//
// 主要根键：
//   HKLM (HKEY_LOCAL_MACHINE) - 本机配置，所有用户共享
//   HKCU (HKEY_CURRENT_USER)  - 当前用户配置
//   HKCR (HKEY_CLASSES_ROOT)   - 文件关联和 COM 对象
//
// Autobox 使用了多维度检测：
//   1. HKLM\SOFTWARE\Autodesk\AutoCAD\R{version}      → 产品安装信息
//   2. HKCU\Software\Autodesk\AutoCAD\R{version}      → 用户级配置
//   3. HKCR\AutoCAD.Drawing.{version}                  → 文件关联 (.dwg)
//   4. HKLM\...\Uninstall\{GUID}                       → 卸载信息
//   5. HKLM\...\Installer\Products\{GUID}              → MSI 安装器
//   6. HKLM\...\Installer\UserData\{GUID}              → 安装器用户数据
//   7. HKLM\SOFTWARE\WOW6432Node\Autodesk              → 32位兼容路径
//   8. HKLM\SOFTWARE\AppDataLow\Software\Autodesk      → AppDataLow 数据
// ============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace AutoCADUninstaller.Services
{
    /// <summary>
    /// 注册表帮助类 - 静态类
    /// 提供注册表的读取、搜索和删除功能
    /// </summary>
    public static class RegistryHelper
    {
        // ====================================================================
        // 注册表路径常量 - 对照 Autobox 逆向分析
        // ====================================================================

        /// <summary>AutoCAD 主安装信息 (HKLM)</summary>
        private const string AutoCADRootKey = @"SOFTWARE\Autodesk\AutoCAD";

        /// <summary>AutoCAD 用户级配置 (HKCU)</summary>
        private const string AutoCADUserRootKey = @"Software\Autodesk\AutoCAD";

        /// <summary>Autodesk 产品根目录 (HKLM)</summary>
        private const string AutodeskRootKey = @"SOFTWARE\Autodesk";

        /// <summary>Autodesk 32位兼容路径 (HKLM)</summary>
        private const string AutodeskRootKeyWOW64 = @"SOFTWARE\WOW6432Node\Autodesk";

        /// <summary>Windows 卸载程序注册表路径 (HKLM)</summary>
        private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        /// <summary>Windows 卸载程序 WOW64 路径 (32位程序在64位系统上)</summary>
        private const string UninstallKeyWOW64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

        /// <summary>Installer 产品注册表路径</summary>
        private const string InstallerProductsKey = @"SOFTWARE\Classes\Installer\Products";

        /// <summary>Installer 功能注册表路径</summary>
        private const string InstallerFeaturesKey = @"SOFTWARE\Classes\Installer\Features";

        /// <summary>Installer 用户数据路径</summary>
        private const string InstallerUserDataKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData";

        /// <summary>AppDataLow 路径</summary>
        private const string AppDataLowKey = @"SOFTWARE\AppDataLow\Software\Autodesk";

        // ====================================================================
        // 版本号到年份的映射（与 ProductInfo 共用）
        // ====================================================================

        private static readonly Dictionary<string, string> VersionMapping = new Dictionary<string, string>
        {
            { "R16.0", "2004" }, { "R16.1", "2005" }, { "R16.2", "2006" },
            { "R17.0", "2007" }, { "R17.1", "2008" }, { "R17.2", "2009" },
            { "R18.0", "2010" }, { "R18.1", "2011" }, { "R18.2", "2012" },
            { "R19.0", "2013" }, { "R19.1", "2014" },
            { "R20.0", "2015" }, { "R20.1", "2016" },
            { "R21.0", "2017" }, { "R22.0", "2018" }, { "R23.0", "2019" },
            { "R24.0", "2020" }, { "R25.0", "2021" }, { "R26.0", "2022" },
            { "R27.0", "2023" }, { "R28.0", "2024" }, { "R29.0", "2025" },
            { "R30.0", "2026" }, { "R31.0", "2027" }
        };

        // ====================================================================
        // 方式1: 通过 HKLM 检测已安装版本（原有）
        // ====================================================================

        /// <summary>
        /// 获取 AutoCAD 安装路径（从 HKLM 注册表）
        /// </summary>
        public static string GetAutoCADPath(string version)
        {
            try
            {
                string keyPath = $@"{AutoCADRootKey}\{version}";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                    {
                        string path = key.GetValue("AcadLocation") as string;
                        if (!string.IsNullOrEmpty(path)) return path;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取注册表失败: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// 获取 HKLM 中所有已安装的 AutoCAD 版本
        /// </summary>
        public static List<string> GetInstalledVersions()
        {
            var versions = new List<string>();

            try
            {
                using (RegistryKey rootKey = Registry.LocalMachine.OpenSubKey(AutoCADRootKey))
                {
                    if (rootKey != null)
                    {
                        foreach (string subKeyName in rootKey.GetSubKeyNames())
                        {
                            if (subKeyName.StartsWith("R") && subKeyName.Contains("."))
                            {
                                versions.Add(subKeyName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取版本列表失败: {ex.Message}");
            }

            return versions;
        }

        // ====================================================================
        // 方式2: 通过 HKCU 检测用户级安装信息（Autobox 方式）
        // ====================================================================

        /// <summary>
        /// 获取 HKCU 中的 AutoCAD 用户级安装版本
        /// 某些 AutoCAD 安装只在当前用户注册表中留下信息
        /// </summary>
        public static List<string> GetUserLevelVersions()
        {
            var versions = new List<string>();

            try
            {
                using (RegistryKey rootKey = Registry.CurrentUser.OpenSubKey(AutoCADUserRootKey))
                {
                    if (rootKey != null)
                    {
                        foreach (string subKeyName in rootKey.GetSubKeyNames())
                        {
                            if (subKeyName.StartsWith("R") && subKeyName.Contains("."))
                            {
                                versions.Add(subKeyName);
                            }
                        }
                    }
                }
            }
            catch { }

            return versions;
        }

        // ====================================================================
        // 方式3: 通过 HKCR 检测文件关联（Autobox 方式）
        // ====================================================================

        /// <summary>
        /// 检测 AutoCAD 文件关联注册表键
        /// AutoCAD 会在 HKCR 中注册 .dwg 文件关联
        /// 例如: AutoCAD.Drawing.24 表示 AutoCAD 2018
        /// </summary>
        public static List<RegistryResidueInfo> GetFileAssociationKeys()
        {
            var results = new List<RegistryResidueInfo>();

            // AutoCAD 文件关联键格式: AutoCAD.Drawing.{内部版本号}
            // 版本号对应关系见 VersionMapping
            string[] drawingPatterns = {
                "AutoCAD.Drawing.16",   // 2004
                "AutoCAD.Drawing.17",   // 2007
                "AutoCAD.Drawing.18",   // 2010
                "AutoCAD.Drawing.19",   // 2013
                "AutoCAD.Drawing.20",   // 2015
                "AutoCAD.Drawing.21",   // 2016
                "AutoCAD.Drawing.22",   // 2017
                "AutoCAD.Drawing.23",   // 2018
                "AutoCAD.Drawing.24",   // 2019
                "AutoCAD.Drawing.25",   // 2020
                "AutoCAD.Drawing.26",   // 2021
            };

            foreach (string pattern in drawingPatterns)
            {
                try
                {
                    // 检查 HKCR 中是否存在
                    using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(pattern))
                    {
                        if (key != null)
                        {
                            results.Add(new RegistryResidueInfo
                            {
                                FullPath = $@"HKEY_CLASSES_ROOT\{pattern}",
                                KeyName = pattern,
                                RootHive = "HKCR",
                                Value = key.GetValue("") as string
                            });
                        }
                    }
                }
                catch { }
            }

            // 也搜索包含 AutoCAD 或 Autodesk 的 HKCR 键
            try
            {
                SearchHKCRForAutodesk(results);
            }
            catch { }

            return results;
        }

        /// <summary>
        /// 在 HKCR 中搜索所有 Autodesk 相关键
        /// </summary>
        private static void SearchHKCRForAutodesk(List<RegistryResidueInfo> results)
        {
            string[] searchNames = { "AutoCAD", "Autodesk" };

            foreach (string name in searchNames)
            {
                try
                {
                    // 搜索 Applications 子键
                    using (RegistryKey appsKey = Registry.ClassesRoot.OpenSubKey("Applications"))
                    {
                        if (appsKey != null)
                        {
                            foreach (string subKeyName in appsKey.GetSubKeyNames())
                            {
                                if (subKeyName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    string fullPath = $@"HKEY_CLASSES_ROOT\Applications\{subKeyName}";
                                    if (!results.Exists(r => r.FullPath == fullPath))
                                    {
                                        results.Add(new RegistryResidueInfo
                                        {
                                            FullPath = fullPath,
                                            KeyName = subKeyName,
                                            RootHive = "HKCR"
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }

                // 搜索 PROGID
                try
                {
                    using (RegistryKey progIdKey = Registry.ClassesRoot.OpenSubKey($"{name}.Drawing"))
                    {
                        if (progIdKey != null)
                        {
                            foreach (string subKeyName in progIdKey.GetSubKeyNames())
                            {
                                string fullPath = $@"HKEY_CLASSES_ROOT\{name}.Drawing.{subKeyName}";
                                if (!results.Exists(r => r.FullPath == fullPath))
                                {
                                    results.Add(new RegistryResidueInfo
                                    {
                                        FullPath = fullPath,
                                        KeyName = $"{name}.Drawing.{subKeyName}",
                                        RootHive = "HKCR"
                                    });
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        // ====================================================================
        // 方式4: 通过 Uninstall 键检测（增强版 - 读取 DisplayName）
        // ====================================================================

        /// <summary>
        /// 获取所有 Autodesk 相关的卸载信息
        /// 对照 Autobox: 读取 DisplayName、InstallLocation 等值
        /// </summary>
        public static List<UninstallEntryInfo> GetUninstallEntries()
        {
            var entries = new List<UninstallEntryInfo>();

            // 同时搜索两个路径
            string[] uninstallPaths = {
                UninstallKey,
                UninstallKeyWOW64
            };

            foreach (string basePath in uninstallPaths)
            {
                try
                {
                    using (RegistryKey rootKey = Registry.LocalMachine.OpenSubKey(basePath))
                    {
                        if (rootKey == null) continue;

                        foreach (string subKeyName in rootKey.GetSubKeyNames())
                        {
                            try
                            {
                                using (RegistryKey subKey = rootKey.OpenSubKey(subKeyName))
                                {
                                    if (subKey == null) continue;

                                    // 读取 DisplayName 判断是否是 Autodesk 产品
                                    string displayName = subKey.GetValue("DisplayName") as string ?? "";
                                    string publisher = subKey.GetValue("Publisher") as string ?? "";
                                    string installLocation = subKey.GetValue("InstallLocation") as string ?? "";

                                    bool isAutodesk =
                                        displayName.IndexOf("Autodesk", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        displayName.IndexOf("AutoCAD", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        publisher.IndexOf("Autodesk", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        installLocation.IndexOf("Autodesk", StringComparison.OrdinalIgnoreCase) >= 0;

                                    if (isAutodesk)
                                    {
                                        entries.Add(new UninstallEntryInfo
                                        {
                                            KeyPath = $@"{basePath}\{subKeyName}",
                                            KeyName = subKeyName,
                                            DisplayName = displayName,
                                            Publisher = publisher,
                                            InstallLocation = installLocation,
                                            DisplayVersion = subKey.GetValue("DisplayVersion") as string ?? ""
                                        });
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }

            return entries;
        }

        // ====================================================================
        // 方式5: 通过 Installer\Products 检测（Autobox 方式）
        // ====================================================================

        /// <summary>
        /// 搜索 MSI 安装器产品中与 Autodesk 相关的条目
        /// Autobox 会搜索 HKLM\SOFTWARE\Classes\Installer\Products 下的所有键
        /// </summary>
        public static List<RegistryResidueInfo> GetInstallerProducts()
        {
            var results = new List<RegistryResidueInfo>();

            try
            {
                using (RegistryKey rootKey = Registry.LocalMachine.OpenSubKey(InstallerProductsKey))
                {
                    if (rootKey != null)
                    {
                        foreach (string subKeyName in rootKey.GetSubKeyNames())
                        {
                            try
                            {
                                using (RegistryKey subKey = rootKey.OpenSubKey(subKeyName))
                                {
                                    if (subKey == null) continue;

                                    // 读取 ProductName 值
                                    string productName = subKey.GetValue("ProductName") as string ?? "";

                                    if (productName.IndexOf("Autodesk", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        productName.IndexOf("AutoCAD", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        results.Add(new RegistryResidueInfo
                                        {
                                            FullPath = $@"{InstallerProductsKey}\{subKeyName}",
                                            KeyName = subKeyName,
                                            RootHive = "HKLM",
                                            Value = productName
                                        });
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }

            return results;
        }

        // ====================================================================
        // 方式6: 通过 Installer\UserData 检测（Autobox 方式）
        // ====================================================================

        /// <summary>
        /// 搜索 Installer\UserData 中的 Autodesk 产品数据
        /// </summary>
        public static List<RegistryResidueInfo> GetInstallerUserData()
        {
            var results = new List<RegistryResidueInfo>();

            try
            {
                using (RegistryKey rootKey = Registry.LocalMachine.OpenSubKey(InstallerUserDataKey))
                {
                    if (rootKey != null)
                    {
                        foreach (string subKeyName in rootKey.GetSubKeyNames())
                        {
                            // 子键名是 SID（用户安全标识符），递归搜索
                            string userPath = $@"{InstallerUserDataKey}\{subKeyName}";

                            try
                            {
                                using (RegistryKey userKey = Registry.LocalMachine.OpenSubKey(userPath))
                                {
                                    if (userKey == null) continue;

                                    foreach (string productName in userKey.GetSubKeyNames())
                                    {
                                        if (productName.IndexOf("Autodesk", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                            productName.IndexOf("AutoCAD", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            results.Add(new RegistryResidueInfo
                                            {
                                                FullPath = $@"{userPath}\{productName}",
                                                KeyName = productName,
                                                RootHive = "HKLM",
                                                Value = "Installer UserData"
                                            });
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }

            return results;
        }

        // ====================================================================
        // 方式7: 搜索 WOW6432Node（32位兼容）
        // ====================================================================

        /// <summary>
        /// 搜索 WOW6432Node 中的 Autodesk 注册表信息
        /// 64位系统上的32位程序会注册在这里
        /// </summary>
        public static List<RegistryResidueInfo> GetWOW64Entries()
        {
            var results = new List<RegistryResidueInfo>();

            // 搜索 WOW6432Node\Autodesk
            results.AddRange(SearchRegistryTree(
                Registry.LocalMachine,
                AutodeskRootKeyWOW64,
                "Autodesk", 3));

            return results;
        }

        // ====================================================================
        // 综合方法: 搜索所有 Autodesk 相关注册表路径
        // ====================================================================

        /// <summary>
        /// 获取所有 Autodesk 相关的注册表路径（综合扫描）
        /// 完全对照 Autobox 的 DetectRegistryResidues()
        /// </summary>
        public static List<RegistryResidueInfo> GetAllAutodeskRegistryEntries()
        {
            var allEntries = new List<RegistryResidueInfo>();

            // 1. HKLM\SOFTWARE\Autodesk 全树
            allEntries.AddRange(SearchRegistryTree(
                Registry.LocalMachine, AutodeskRootKey, "Autodesk", 3));

            // 2. HKLM\SOFTWARE\WOW6432Node\Autodesk
            allEntries.AddRange(GetWOW64Entries());

            // 3. HKCU Software\Autodesk
            allEntries.AddRange(SearchRegistryTree(
                Registry.CurrentUser, AutoCADUserRootKey, "Autodesk", 2));

            // 4. HKLM AppDataLow
            allEntries.AddRange(SearchRegistryTree(
                Registry.LocalMachine, AppDataLowKey, "Autodesk", 2));

            // 5. 文件关联
            allEntries.AddRange(GetFileAssociationKeys());

            // 6. Installer\Products
            allEntries.AddRange(GetInstallerProducts());

            // 7. Installer\UserData
            allEntries.AddRange(GetInstallerUserData());

            // 8. 卸载信息
            var uninstallEntries = GetUninstallEntries();
            foreach (var entry in uninstallEntries)
            {
                allEntries.Add(new RegistryResidueInfo
                {
                    FullPath = entry.KeyPath,
                    KeyName = entry.DisplayName,
                    RootHive = "HKLM",
                    Value = entry.DisplayName
                });
            }

            return allEntries;
        }

        // ====================================================================
        // 搜索辅助方法
        // ====================================================================

        /// <summary>
        /// 递归搜索注册表树
        /// </summary>
        /// <param name="rootKey">根键（LocalMachine 或 CurrentUser）</param>
        /// <param name="startPath">搜索起始路径</param>
        /// <param name="matchText">匹配文本</param>
        /// <param name="maxDepth">最大递归深度</param>
        public static List<RegistryResidueInfo> SearchRegistryTree(
            RegistryKey rootKey, string startPath, string matchText, int maxDepth)
        {
            var results = new List<RegistryResidueInfo>();
            SearchRegistryTreeRecursive(rootKey, startPath, matchText, results, 0, maxDepth);
            return results;
        }

        private static void SearchRegistryTreeRecursive(
            RegistryKey rootKey, string keyPath, string matchText,
            List<RegistryResidueInfo> results, int currentDepth, int maxDepth)
        {
            if (currentDepth >= maxDepth) return;

            try
            {
                using (RegistryKey key = rootKey.OpenSubKey(keyPath))
                {
                    if (key == null) return;

                    // 检查当前键是否匹配
                    bool matched = key.Name.IndexOf(matchText, StringComparison.OrdinalIgnoreCase) >= 0;

                    // 也检查默认值
                    string defaultValue = key.GetValue("") as string ?? "";
                    if (defaultValue.IndexOf(matchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matched = true;
                    }

                    if (matched)
                    {
                        results.Add(new RegistryResidueInfo
                        {
                            FullPath = key.Name,
                            KeyName = key.Name.Substring(key.Name.LastIndexOf('\\') + 1),
                            RootHive = rootKey.Name.Contains("LMACHINE") ? "HKLM" : "HKCU",
                            Value = defaultValue
                        });
                    }

                    // 递归子键
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        string subKeyPath = $@"{keyPath}\{subKeyName}";
                        SearchRegistryTreeRecursive(rootKey, subKeyPath, matchText,
                            results, currentDepth + 1, maxDepth);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 搜索包含特定文本的注册表键（兼容旧接口）
        /// </summary>
        public static List<string> SearchKeys(string rootPath, string searchText)
        {
            var results = new List<string>();
            var entries = SearchRegistryTree(Registry.LocalMachine, rootPath, searchText, 3);
            foreach (var entry in entries)
            {
                results.Add(entry.FullPath);
            }
            return results;
        }

        /// <summary>
        /// 收集指定路径下的所有叶键
        /// </summary>
        public static List<string> CollectLeafKeys(string rootPath)
        {
            var leafKeys = new List<string>();
            CollectLeafKeysRecursive(Registry.LocalMachine, rootPath, leafKeys);
            return leafKeys;
        }

        private static void CollectLeafKeysRecursive(RegistryKey rootKey, string keyPath, List<string> leafKeys)
        {
            try
            {
                using (RegistryKey key = rootKey.OpenSubKey(keyPath))
                {
                    if (key == null) return;

                    string[] subKeyNames = key.GetSubKeyNames();
                    if (subKeyNames.Length == 0)
                    {
                        leafKeys.Add(keyPath);
                    }
                    else
                    {
                        foreach (string subKeyName in subKeyNames)
                        {
                            CollectLeafKeysRecursive(rootKey, $@"{keyPath}\{subKeyName}", leafKeys);
                        }
                    }
                }
            }
            catch { }
        }

        // ====================================================================
        // 删除操作
        // ====================================================================

        /// <summary>
        /// 删除注册表键（连同所有子键）
        /// </summary>
        public static bool DeleteKeyTree(string keyPath)
        {
            try
            {
                // 尝试从 HKLM 删除
                Registry.LocalMachine.DeleteSubKeyTree(keyPath, false);
                return true;
            }
            catch
            {
                // 尝试从 HKCU 删除
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(keyPath, false);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"删除注册表键失败 {keyPath}: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 删除卸载注册表键
        /// </summary>
        public static bool DeleteUninstallKey(string productCode)
        {
            bool deleted = false;
            if (DeleteKeyTree($@"{UninstallKey}\{productCode}")) deleted = true;
            if (DeleteKeyTree($@"{UninstallKeyWOW64}\{productCode}")) deleted = true;
            return deleted;
        }

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        public static bool KeyExists(string keyPath)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    // ====================================================================
    // 辅助数据类
    // ====================================================================

    /// <summary>
    /// 注册表残留信息
    /// </summary>
    public class RegistryResidueInfo
    {
        /// <summary>完整路径</summary>
        public string FullPath { get; set; }

        /// <summary>键名</summary>
        public string KeyName { get; set; }

        /// <summary>根键 (HKLM/HKCU/HKCR)</summary>
        public string RootHive { get; set; }

        /// <summary>默认值</summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// 卸载条目信息
    /// </summary>
    public class UninstallEntryInfo
    {
        /// <summary>注册表键路径</summary>
        public string KeyPath { get; set; }

        /// <summary>键名 (GUID)</summary>
        public string KeyName { get; set; }

        /// <summary>显示名称</summary>
        public string DisplayName { get; set; }

        /// <summary>发布者</summary>
        public string Publisher { get; set; }

        /// <summary>安装位置</summary>
        public string InstallLocation { get; set; }

        /// <summary>显示版本</summary>
        public string DisplayVersion { get; set; }
    }
}
