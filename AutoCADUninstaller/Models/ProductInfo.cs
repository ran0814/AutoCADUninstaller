// ============================================================================
// ProductInfo.cs - AutoCAD 产品信息模型
// ============================================================================
// 这个类用于存储检测到的 AutoCAD 安装信息
// 包括版本号、安装路径、注册表键等
// ============================================================================

using System;
using System.Collections.Generic;

namespace AutoCADUninstaller.Models
{
    /// <summary>
    /// AutoCAD 产品信息
    /// 存储从注册表中读取的安装信息
    /// </summary>
    public class ProductInfo
    {
        // ====================================================================
        // 基本信息
        // ====================================================================

        /// <summary>
        /// 产品名称，例如 "AutoCAD 2024"
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 版本号，例如 "R28.0"
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 产品代码 (GUID)，用于调用标准卸载程序
        /// 例如 "{57540000-0000-0000-0000-000000000000}"
        /// </summary>
        public string ProductCode { get; set; }

        // ====================================================================
        // 路径信息
        // ====================================================================

        /// <summary>
        /// 安装路径
        /// 例如 "C:\Program Files\Autodesk\AutoCAD 2024"
        /// </summary>
        public string InstallPath { get; set; }

        /// <summary>
        /// 注册表键路径
        /// 例如 "SOFTWARE\Autodesk\AutoCAD\R28.0"
        /// </summary>
        public string RegistryKeyPath { get; set; }

        // ====================================================================
        // 状态信息
        // ====================================================================

        /// <summary>
        /// 是否已安装
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// 是否被用户选中（用于清理操作）
        /// </summary>
        public bool IsSelected { get; set; }

        // ====================================================================
        // 版本映射表（静态数据）
        // ====================================================================

        /// <summary>
        /// AutoCAD 内部版本号到年份的映射表
        /// 这些是 Autodesk 内部使用的版本号
        /// </summary>
        private static readonly Dictionary<string, string> VersionMapping = new Dictionary<string, string>
        {
            { "R16.0", "2004" },
            { "R16.1", "2005" },
            { "R16.2", "2006" },
            { "R17.0", "2007" },
            { "R17.1", "2008" },
            { "R17.2", "2009" },
            { "R18.0", "2010" },
            { "R18.1", "2011" },
            { "R18.2", "2012" },
            { "R19.0", "2013" },
            { "R19.1", "2014" },
            { "R20.0", "2015" },
            { "R20.1", "2016" },
            { "R21.0", "2017" },
            { "R22.0", "2018" },
            { "R23.0", "2019" },
            { "R24.0", "2020" },
            { "R25.0", "2021" },
            { "R26.0", "2022" },
            { "R27.0", "2023" },
            { "R28.0", "2024" },
            { "R29.0", "2025" },
            { "R30.0", "2026" },
            { "R31.0", "2027" }
        };

        // ====================================================================
        // 方法
        // ====================================================================

        /// <summary>
        /// 从内部版本号获取年份
        /// 例如 "R28.0" -> "2024"
        /// </summary>
        public static string GetYearFromVersion(string internalVersion)
        {
            if (VersionMapping.ContainsKey(internalVersion))
            {
                return VersionMapping[internalVersion];
            }
            return "Unknown";
        }

        /// <summary>
        /// 获取显示名称
        /// 例如 "AutoCAD 2024 (R28.0)"
        /// </summary>
        public string DisplayName
        {
            get
            {
                string year = GetYearFromVersion(Version);
                return $"AutoCAD {year} ({Version})";
            }
        }

        /// <summary>
        /// 获取支持的所有版本列表
        /// </summary>
        public static List<string> GetAllSupportedVersions()
        {
            return new List<string>(VersionMapping.Keys);
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
