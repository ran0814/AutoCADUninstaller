// ============================================================================
// ResidueItem.cs - 残留物信息模型
// ============================================================================
// 这个类用于存储扫描到的残留物信息
// 包括文件残留、注册表残留、服务残留等
// ============================================================================

using System;

namespace AutoCADUninstaller.Models
{
    /// <summary>
    /// 残留物类型枚举
    /// 定义了所有可能的残留物类型
    /// </summary>
    public enum ResidueType
    {
        /// <summary>服务残留 - Windows 服务未被卸载</summary>
        Service,

        /// <summary>进程残留 - 相关进程仍在运行</summary>
        Process,

        /// <summary>文件残留 - 安装目录、用户数据等文件</summary>
        File,

        /// <summary>注册表残留 - 注册表键值未被清理</summary>
        Registry,

        /// <summary>快捷方式残留 - 桌面/开始菜单快捷方式</summary>
        Shortcut,

        /// <summary>卸载信息残留 - 卸载注册表键</summary>
        UninstallEntry
    }

    /// <summary>
    /// 残留物风险等级
    /// 帮助用户判断清理的优先级
    /// </summary>
    public enum RiskLevel
    {
        /// <summary>低风险 - 清理后对系统无影响</summary>
        Low,

        /// <summary>中风险 - 可能影响其他 Autodesk 产品</summary>
        Medium,

        /// <summary>高风险 - 清理后可能影响系统稳定性</summary>
        High
    }

    /// <summary>
    /// 残留物信息
    /// 存储单个残留物的所有相关信息
    /// </summary>
    public class ResidueItem
    {
        // ====================================================================
        // 基本信息
        // ====================================================================

        /// <summary>
        /// 残留物名称
        /// 例如 "AdskLicensingService" 或 "C:\Program Files\Autodesk"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 完整路径（文件路径或注册表路径）
        /// 例如 "HKLM\SOFTWARE\Autodesk" 或 "C:\Program Files\Autodesk"
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 残留物类型
        /// </summary>
        public ResidueType Type { get; set; }

        /// <summary>
        /// 关联的产品名称
        /// 例如 "AutoCAD 2024"
        /// </summary>
        public string ProductName { get; set; }

        // ====================================================================
        // 大小和状态
        // ====================================================================

        /// <summary>
        /// 文件大小（字节），仅对文件类型有效
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 风险等级
        /// </summary>
        public RiskLevel Risk { get; set; }

        /// <summary>
        /// 是否被用户选中（用于清理操作）
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否可以安全删除
        /// 某些系统文件不能直接删除
        /// </summary>
        public bool CanDelete { get; set; } = true;

        /// <summary>
        /// 备注信息
        /// 用于显示额外的说明
        /// </summary>
        public string Note { get; set; }

        // ====================================================================
        // 显示属性
        // ====================================================================

        /// <summary>
        /// 获取类型的中文显示名称
        /// </summary>
        public string TypeName
        {
            get
            {
                switch (Type)
                {
                    case ResidueType.Service: return "服务";
                    case ResidueType.Process: return "进程";
                    case ResidueType.File: return "文件";
                    case ResidueType.Registry: return "注册表";
                    case ResidueType.Shortcut: return "快捷方式";
                    case ResidueType.UninstallEntry: return "卸载信息";
                    default: return "未知";
                }
            }
        }

        /// <summary>
        /// 获取风险等级的中文显示名称
        /// </summary>
        public string RiskName
        {
            get
            {
                switch (Risk)
                {
                    case RiskLevel.Low: return "低";
                    case RiskLevel.Medium: return "中";
                    case RiskLevel.High: return "高";
                    default: return "未知";
                }
            }
        }

        /// <summary>
        /// 获取格式化的大小显示
        /// 例如 "2.3 GB" 或 "15.2 MB"
        /// </summary>
        public string SizeDisplay
        {
            get
            {
                if (Size <= 0) return "-";

                if (Size >= 1024L * 1024 * 1024)
                    return $"{Size / (1024.0 * 1024 * 1024):F1} GB";
                if (Size >= 1024L * 1024)
                    return $"{Size / (1024.0 * 1024):F1} MB";
                if (Size >= 1024)
                    return $"{Size / 1024.0:F1} KB";

                return $"{Size} B";
            }
        }

        /// <summary>
        /// 获取用于显示的完整描述
        /// </summary>
        public string DisplayName
        {
            get
            {
                string size = Size > 0 ? $" ({SizeDisplay})" : "";
                return $"[{TypeName}] {Name}{size}";
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
