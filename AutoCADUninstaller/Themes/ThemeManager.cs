// ============================================================================
// ThemeManager.cs - 主题管理系统
// ============================================================================
// 支持浅色和深色主题的切换，集中管理应用程序的颜色方案
// ============================================================================

using System;
using System.Drawing;

namespace AutoCADUninstaller.Themes
{
    /// <summary>
    /// 主题类型枚举
    /// </summary>
    public enum ThemeType
    {
        Light,  // 浅色主题
        Dark    // 深色主题
    }

    /// <summary>
    /// 主题颜色方案
    /// </summary>
    public class ThemeColors
    {
        // 背景色
        public Color BackgroundPrimary { get; set; }
        public Color BackgroundSecondary { get; set; }
        public Color BackgroundTertiary { get; set; }

        // 前景色（文本）
        public Color TextPrimary { get; set; }
        public Color TextSecondary { get; set; }

        // 强调色
        public Color AccentPrimary { get; set; }      // 主操作（扫描）
        public Color AccentSecondary { get; set; }    // 次操作（清理）
        public Color AccentDanger { get; set; }       // 危险操作（删除）

        // 状态色
        public Color Success { get; set; }    // 成功
        public Color Warning { get; set; }    // 警告
        public Color Error { get; set; }      // 错误
        public Color Info { get; set; }       // 信息

        // 边框和分割线
        public Color Border { get; set; }
        public Color Divider { get; set; }

        // 按钮状态
        public Color ButtonEnabled { get; set; }
        public Color ButtonDisabled { get; set; }
        public Color ButtonHover { get; set; }
        public Color ButtonPressed { get; set; }
    }

    /// <summary>
    /// 主题管理器
    /// </summary>
    public static class ThemeManager
    {
        private static ThemeType _currentTheme = ThemeType.Light;
        private static ThemeColors _lightTheme;
        private static ThemeColors _darkTheme;

        static ThemeManager()
        {
            InitializeThemes();
        }

        /// <summary>
        /// 初始化主题
        /// </summary>
        private static void InitializeThemes()
        {
            _lightTheme = new ThemeColors
            {
                // 浅色主题 - 明亮干净
                BackgroundPrimary = Color.FromArgb(255, 255, 255),
                BackgroundSecondary = Color.FromArgb(245, 248, 252),
                BackgroundTertiary = Color.FromArgb(240, 244, 250),

                TextPrimary = Color.FromArgb(30, 30, 30),
                TextSecondary = Color.FromArgb(100, 100, 100),

                AccentPrimary = Color.FromArgb(40, 120, 200),    // 蓝色 - 扫描
                AccentSecondary = Color.FromArgb(200, 60, 60),   // 红色 - 清理
                AccentDanger = Color.FromArgb(220, 53, 69),

                Success = Color.FromArgb(40, 167, 69),    // 绿色
                Warning = Color.FromArgb(255, 193, 7),    // 黄色
                Error = Color.FromArgb(220, 53, 69),      // 深红
                Info = Color.FromArgb(23, 162, 184),      // 青色

                Border = Color.FromArgb(200, 210, 225),
                Divider = Color.FromArgb(230, 235, 245),

                ButtonEnabled = Color.FromArgb(240, 240, 240),
                ButtonDisabled = Color.FromArgb(220, 220, 220),
                ButtonHover = Color.FromArgb(230, 230, 230),
                ButtonPressed = Color.FromArgb(200, 200, 200)
            };

            _darkTheme = new ThemeColors
            {
                // 深色主题 - 护眼暗色
                BackgroundPrimary = Color.FromArgb(45, 45, 48),
                BackgroundSecondary = Color.FromArgb(37, 37, 38),
                BackgroundTertiary = Color.FromArgb(30, 30, 30),

                TextPrimary = Color.FromArgb(230, 230, 230),
                TextSecondary = Color.FromArgb(160, 160, 160),

                AccentPrimary = Color.FromArgb(100, 180, 255),    // 亮蓝
                AccentSecondary = Color.FromArgb(255, 120, 120),  // 亮红
                AccentDanger = Color.FromArgb(255, 100, 100),

                Success = Color.FromArgb(100, 210, 130),   // 亮绿
                Warning = Color.FromArgb(255, 200, 50),    // 亮黄
                Error = Color.FromArgb(255, 100, 100),     // 亮红
                Info = Color.FromArgb(80, 200, 220),       // 亮青

                Border = Color.FromArgb(80, 80, 80),
                Divider = Color.FromArgb(60, 60, 60),

                ButtonEnabled = Color.FromArgb(65, 65, 65),
                ButtonDisabled = Color.FromArgb(50, 50, 50),
                ButtonHover = Color.FromArgb(75, 75, 75),
                ButtonPressed = Color.FromArgb(85, 85, 85)
            };
        }

        /// <summary>
        /// 获取当前主题
        /// </summary>
        public static ThemeType CurrentTheme => _currentTheme;

        /// <summary>
        /// 获取当前主题的颜色方案
        /// </summary>
        public static ThemeColors CurrentColors =>
            _currentTheme == ThemeType.Light ? _lightTheme : _darkTheme;

        /// <summary>
        /// 切换主题
        /// </summary>
        public static void SwitchTheme(ThemeType theme)
        {
            _currentTheme = theme;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// 主题变更事件
        /// </summary>
        public static event EventHandler ThemeChanged;

        /// <summary>
        /// 获取浅色主题颜色
        /// </summary>
        public static ThemeColors GetLightTheme() => _lightTheme;

        /// <summary>
        /// 获取深色主题颜色
        /// </summary>
        public static ThemeColors GetDarkTheme() => _darkTheme;
    }
}
