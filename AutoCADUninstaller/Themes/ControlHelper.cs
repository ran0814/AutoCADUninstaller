// ============================================================================
// ControlHelper.cs - 控件样式辅助类
// ============================================================================
// 提供便捷的方法来应用主题样式到各种 WinForms 控件
// ============================================================================

using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoCADUninstaller.Themes
{
    /// <summary>
    /// 控件样式辅助类
    /// </summary>
    public static class ControlHelper
    {
        /// <summary>
        /// 应用主题到整个窗体
        /// </summary>
        public static void ApplyThemeToForm(Form form)
        {
            var colors = ThemeManager.CurrentColors;
            form.BackColor = colors.BackgroundPrimary;
            form.ForeColor = colors.TextPrimary;

            // 递归应用到所有子控件
            ApplyThemeToControls(form.Controls, colors);
        }

        /// <summary>
        /// 递归应用主题到所有子控件
        /// </summary>
        private static void ApplyThemeToControls(Control.ControlCollection controls, ThemeColors colors)
        {
            foreach (Control control in controls)
            {
                ApplyThemeToControl(control, colors);

                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls, colors);
                }
            }
        }

        /// <summary>
        /// 应用主题到单个控件
        /// </summary>
        private static void ApplyThemeToControl(Control control, ThemeColors colors)
        {
            control.ForeColor = colors.TextPrimary;

            switch (control)
            {
                case Form form:
                    form.BackColor = colors.BackgroundPrimary;
                    break;

                case Panel panel:
                    panel.BackColor = colors.BackgroundSecondary;
                    break;

                case Label label:
                    label.BackColor = colors.BackgroundSecondary;
                    break;

                case TextBox textBox:
                    textBox.BackColor = colors.BackgroundTertiary;
                    textBox.ForeColor = colors.TextPrimary;
                    break;

                case DataGridView dgv:
                    dgv.BackgroundColor = colors.BackgroundPrimary;
                    dgv.GridColor = colors.Divider;
                    dgv.DefaultCellStyle.BackColor = colors.BackgroundPrimary;
                    dgv.DefaultCellStyle.ForeColor = colors.TextPrimary;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = colors.BackgroundSecondary;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = colors.TextPrimary;
                    break;

                case StatusStrip statusStrip:
                    statusStrip.BackColor = colors.BackgroundSecondary;
                    break;
            }
        }

        /// <summary>
        /// 设置按钮为主操作样式（扫描）
        /// </summary>
        public static void SetButtonPrimaryStyle(Button button)
        {
            var colors = ThemeManager.CurrentColors;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = colors.AccentPrimary;
            button.BackColor = colors.AccentPrimary;
            button.ForeColor = Color.White;
            button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 设置按钮为危险操作样式（清理）
        /// </summary>
        public static void SetButtonDangerStyle(Button button)
        {
            var colors = ThemeManager.CurrentColors;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = colors.AccentSecondary;
            button.BackColor = colors.AccentSecondary;
            button.ForeColor = Color.White;
            button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 设置按钮为次操作样式（全选、反选等）
        /// </summary>
        public static void SetButtonSecondaryStyle(Button button)
        {
            var colors = ThemeManager.CurrentColors;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = colors.Border;
            button.BackColor = colors.ButtonEnabled;
            button.ForeColor = colors.TextPrimary;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 设置按钮为默��样式（退出等）
        /// </summary>
        public static void SetButtonDefaultStyle(Button button)
        {
            var colors = ThemeManager.CurrentColors;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = colors.Border;
            button.BackColor = colors.BackgroundSecondary;
            button.ForeColor = colors.TextPrimary;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 设置标题标签样式
        /// </summary>
        public static void SetTitleLabelStyle(Label label, Color accentColor)
        {
            var colors = ThemeManager.CurrentColors;
            label.BackColor = colors.BackgroundSecondary;
            label.ForeColor = accentColor;
            label.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
        }
    }
}
