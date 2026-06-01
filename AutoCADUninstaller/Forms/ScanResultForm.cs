// ============================================================================
// ScanResultForm.cs - 扫描结果详细展示对话框
// ============================================================================
// 以表格形式展示扫描结果的详细信息，支持按类型筛选和导出
// ============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AutoCADUninstaller.Models;
using AutoCADUninstaller.Services;
using AutoCADUninstaller.Themes;

namespace AutoCADUninstaller.Forms
{
    /// <summary>
    /// 扫描结果详细展示对话框
    /// </summary>
    public class ScanResultForm : Form
    {
        private ScanResult _scanResult;
        private List<ResidueItem> _allResidues;

        public ScanResultForm(ScanResult scanResult)
        {
            _scanResult = scanResult;
            _allResidues = scanResult.AllResidues;
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "扫描���果详情";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // 应用主题
            ControlHelper.ApplyThemeToForm(this);

            // 创建主面板
            var mainPanel = new Panel { Dock = DockStyle.Fill };
            this.Controls.Add(mainPanel);

            // 创建标签页控件
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.CurrentColors.BackgroundPrimary
            };
            mainPanel.Controls.Add(tabControl);

            // 创建各个标签页
            CreateServiceTab(tabControl);
            CreateProcessTab(tabControl);
            CreateFileTab(tabControl);
            CreateRegistryTab(tabControl);
            CreateShortcutTab(tabControl);
            CreateUninstallTab(tabControl);
            CreateSummaryTab(tabControl);

            // 创建按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = ThemeManager.CurrentColors.BackgroundSecondary,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(buttonPanel);

            var btnExport = new Button
            {
                Text = "📊 导出报告",
                Width = 100,
                Dock = DockStyle.Left,
                Margin = new Padding(5)
            };
            ControlHelper.SetButtonSecondaryStyle(btnExport);
            btnExport.Click += (s, e) => ExportReport();
            buttonPanel.Controls.Add(btnExport);

            var btnClose = new Button
            {
                Text = "关闭",
                Width = 80,
                Dock = DockStyle.Right,
                Margin = new Padding(5)
            };
            ControlHelper.SetButtonDefaultStyle(btnClose);
            btnClose.Click += (s, e) => this.Close();
            buttonPanel.Controls.Add(btnClose);
        }

        private void CreateServiceTab(TabControl tabControl)
        {
            var tab = new TabPage { Text = $"🔧 服务残留 ({_scanResult.ServiceCount})" };
            var dgv = CreateDataGridView(_scanResult.AllResidues.Where(r => r.Type == ResidueType.Service).ToList());
            tab.Controls.Add(dgv);
            tabControl.TabPages.Add(tab);
        }

        private void CreateProcessTab(TabControl tabControl)
        {
            var tab = new TabPage { Text = $"⚙️ 进程残��� ({_scanResult.ProcessCount})" };
            var dgv = CreateDataGridView(_scanResult.AllResidues.Where(r => r.Type == ResidueType.Process).ToList());
            tab.Controls.Add(dgv);
            tabControl.TabPages.Add(tab);
        }

        private void CreateFileTab(TabControl tabControl)
        {
            var tab = new TabPage { Text = $"📁 文件残留 ({_scanResult.FileCount})" };
            var dgv = CreateDataGridView(_scanResult.AllResidues.Where(r => r.Type == ResidueType.File).ToList());
            tab.Controls.Add(dgv);
            tabControl.TabPages.Add(tab);
        }

        private void CreateRegistryTab(TabControl tabControl)
        {
            var tab = new TabPage { Text = $"📚 注册表残留 ({_scanResult.RegistryCount})" };
            var dgv = CreateDataGridView(_scanResult.AllResidues.Where(r => r.Type == ResidueType.Registry).ToList());
            tab.Controls.Add(dgv);
            tabControl.TabPages.Add(tab);
        }

        private void CreateShortcutTab(TabControl tabControl)
        {
            var tab = new TabPage { Text = $"🔗 快捷方式残留 ({_scanResult.ShortcutCount})" };
            var dgv = CreateDataGridView(_scanResult.AllResidues.Where(r => r.Type == ResidueType.Shortcut).ToList());
            tab.Controls.Add(dgv);
            tabControl.TabPages.Add(tab);
        }

        private void CreateUninstallTab(TabControl tabControl)
        {
            var tab = new TabPage { Text = $"📦 卸载信息残留 ({_scanResult.UninstallCount})" };
            var dgv = CreateDataGridView(_scanResult.AllResidues.Where(r => r.Type == ResidueType.UninstallEntry).ToList());
            tab.Controls.Add(dgv);
            tabControl.TabPages.Add(tab);
        }

        private void CreateSummaryTab(TabControl tabControl)
        {
            var tab = new TabPage { Text = "📈 摘要统计" };
            var summaryPanel = new Panel { Dock = DockStyle.Fill };

            var colors = ThemeManager.CurrentColors;

            var summary = $@"
═══════════════════════════════════════════
  扫描结果摘要
═══════════════════════════════════════════

  扫描时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}
  
  残留物统计：
  ├─ 服务残留：        {_scanResult.ServiceCount:D3} 项
  ├─ 进程残留：        {_scanResult.ProcessCount:D3} 项
  ├─ 文件残留：        {_scanResult.FileCount:D3} 项
  ├─ 注册表残留：      {_scanResult.RegistryCount:D3} 项
  ├─ 快捷方式残留：    {_scanResult.ShortcutCount:D3} 项
  ├─ 卸载信息残留：    {_scanResult.UninstallCount:D3} 项
  └─ 总计：            {_scanResult.TotalCount:D3} 项

  文件大小：{_scanResult.TotalSizeDisplay}
  
═══════════════════════════════════════════
";

            var lblSummary = new Label
            {
                Text = summary,
                Dock = DockStyle.Fill,
                Font = new Font("Cascadia Code", 10F),
                Padding = new Padding(15),
                BackColor = colors.BackgroundTertiary,
                ForeColor = colors.TextPrimary
            };

            summaryPanel.Controls.Add(lblSummary);
            tab.Controls.Add(summaryPanel);
            tabControl.TabPages.Add(tab);
        }

        private DataGridView CreateDataGridView(List<ResidueItem> residues)
        {
            var colors = ThemeManager.CurrentColors;

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
                GridColor = colors.Divider
            };

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "路径/键值", Name = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "详情", Name = "Details", Width = 200 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "大小", Name = "Size", Width = 100 });

            foreach (var residue in residues)
            {
                dgv.Rows.Add(residue.Name, residue.Note ?? "", residue.SizeDisplay);
            }

            dgv.DefaultCellStyle.BackColor = colors.BackgroundPrimary;
            dgv.DefaultCellStyle.ForeColor = colors.TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = colors.BackgroundSecondary;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = colors.TextPrimary;

            return dgv;
        }

        private void ExportReport()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "文本文件 (*.txt)|*.txt|CSV 文件 (*.csv)|*.csv",
                FileName = $"ScanReport_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("报告已导出！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
