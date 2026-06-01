// ============================================================================
// PreviewCleanForm.cs - 清理预览对话框
// ============================================================================
// 显示将要删除的所有项目，提供"干运行"模式预览
// ============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AutoCADUninstaller.Models;
using AutoCADUninstaller.Themes;

namespace AutoCADUninstaller.Forms
{
    /// <summary>
    /// 清理预览对话框
    /// </summary>
    public class PreviewCleanForm : Form
    {
        private List<ResidueItem> _residuesToDelete;
        public bool UserConfirmed { get; private set; }

        public PreviewCleanForm(List<ResidueItem> residues)
        {
            _residuesToDelete = residues;
            UserConfirmed = false;
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "确认清理 - 预览将删除的项目";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            ControlHelper.ApplyThemeToForm(this);

            var colors = ThemeManager.CurrentColors;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            this.Controls.Add(mainPanel);

            var lblWarning = new Label
            {
                Text = "⚠️ 以下项目将被删除。请仔细检查后再确认。",
                AutoSize = false,
                Height = 30,
                Dock = DockStyle.Top,
                ForeColor = colors.Warning,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            mainPanel.Controls.Add(lblWarning);

            var statPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = colors.BackgroundSecondary,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(statPanel);

            CreateStatistics(statPanel, colors);

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = colors.BackgroundPrimary,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 35,
                RowHeadersVisible = false,
                ReadOnly = true,
                GridColor = colors.Divider,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = colors.BackgroundPrimary,
                    ForeColor = colors.TextPrimary,
                    Font = new Font("Microsoft YaHei UI", 9F)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = colors.BackgroundSecondary,
                    ForeColor = colors.TextPrimary,
                    Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
                }
            };

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "类型", Name = "Type", Width = 80 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "项目", Name = "Item", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "大小", Name = "Size", Width = 100 });

            foreach (var residue in _residuesToDelete)
            {
                dgv.Rows.Add(residue.TypeName, residue.Name, residue.SizeDisplay);
            }

            mainPanel.Controls.Add(dgv);

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = colors.BackgroundSecondary,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(buttonPanel);

            var btnCancel = new Button
            {
                Text = "❌ 取消",
                Width = 100,
                Dock = DockStyle.Right,
                Margin = new Padding(5, 0, 0, 0),
                DialogResult = DialogResult.Cancel
            };
            ControlHelper.SetButtonDefaultStyle(btnCancel);
            btnCancel.Click += (s, e) => { UserConfirmed = false; this.Close(); };
            buttonPanel.Controls.Add(btnCancel);

            var btnConfirm = new Button
            {
                Text = "✓ 确认删除",
                Width = 100,
                Dock = DockStyle.Right,
                Margin = new Padding(0, 0, 5, 0),
                DialogResult = DialogResult.OK
            };
            ControlHelper.SetButtonDangerStyle(btnConfirm);
            btnConfirm.Click += (s, e) => { UserConfirmed = true; this.Close(); };
            buttonPanel.Controls.Add(btnConfirm);

            this.CancelButton = btnCancel;
            this.AcceptButton = btnConfirm;
        }

        private void CreateStatistics(Panel statPanel, ThemeColors colors)
        {
            var stats = new Dictionary<string, int>
            {
                { "服务", _residuesToDelete.Count(r => r.Type == ResidueType.Service) },
                { "进程", _residuesToDelete.Count(r => r.Type == ResidueType.Process) },
                { "文件", _residuesToDelete.Count(r => r.Type == ResidueType.File) },
                { "注册表", _residuesToDelete.Count(r => r.Type == ResidueType.Registry) },
                { "快捷方式", _residuesToDelete.Count(r => r.Type == ResidueType.Shortcut) },
                { "卸载信息", _residuesToDelete.Count(r => r.Type == ResidueType.UninstallEntry) }
            };

            int x = 10;
            foreach (var stat in stats.Where(s => s.Value > 0))
            {
                var lbl = new Label
                {
                    Text = $"{stat.Key}: {stat.Value} 项",
                    Location = new Point(x, 10),
                    AutoSize = true,
                    ForeColor = colors.TextPrimary,
                    Font = new Font("Microsoft YaHei UI", 9F)
                };
                statPanel.Controls.Add(lbl);
                x += 150;
            }

            var totalSize = _residuesToDelete.Sum(r => r.Size);

            var lblTotal = new Label
            {
                Text = $"总计: {_residuesToDelete.Count} 项 ({FormatBytes(totalSize)})",
                Location = new Point(10, 35),
                AutoSize = true,
                ForeColor = colors.AccentSecondary,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
            };
            statPanel.Controls.Add(lblTotal);
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
