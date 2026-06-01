// ============================================================================
// MainForm.Designer.cs - 主窗体设计器自动生成代码
// ============================================================================
// 这个文件由 Visual Studio 的窗体设计器自动生成
// 所有控件的创建和布局代码都在这里
//
// 注意：不要手动修改这个文件，除非你很清楚自己在做什么
// 所有 UI 调整应该在设计器中进行
// ============================================================================

namespace AutoCADUninstaller
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // ====================================================================
            // 主面板
            // ====================================================================
            this.mainPanel = new System.Windows.Forms.Panel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();

            // ====================================================================
            // 上半部分 - 已安装产品列表
            // ====================================================================
            this.lblInstalledTitle = new System.Windows.Forms.Label();
            this.dgvProducts = new System.Windows.Forms.DataGridView();
            this.colSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colProductName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPath = new System.Windows.Forms.DataGridViewTextBoxColumn();

            // ====================================================================
            // 按钮区域
            // ====================================================================
            this.btnPanel = new System.Windows.Forms.Panel();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnClean = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnSelectNone = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();

            // ====================================================================
            // 下半部分 - 扫描结果
            // ====================================================================
            this.lblResultTitle = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();

            // ====================================================================
            // 底部状态栏
            // ====================================================================
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();

            // 开始初始化
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).BeginInit();
            this.btnPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();

            // ====================================================================
            // 分割容器（上下分割）
            // ====================================================================
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.SplitterDistance = 250;
            this.splitContainer.Size = new System.Drawing.Size(784, 561);
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;

            // --- 上半部分面板 ---
            this.splitContainer.Panel1.Controls.Add(this.dgvProducts);
            this.splitContainer.Panel1.Controls.Add(this.lblInstalledTitle);

            // --- 下半部分面板 ---
            this.splitContainer.Panel2.Controls.Add(this.txtLog);
            this.splitContainer.Panel2.Controls.Add(this.lblResultTitle);
            this.splitContainer.Panel2.Controls.Add(this.btnPanel);

            // ====================================================================
            // 标题标签 - 已安装产品
            // ====================================================================
            this.lblInstalledTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInstalledTitle.Font = new System.Drawing.Font(
                "Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblInstalledTitle.ForeColor = System.Drawing.Color.FromArgb(30, 60, 120);
            this.lblInstalledTitle.Location = new System.Drawing.Point(0, 0);
            this.lblInstalledTitle.Name = "lblInstalledTitle";
            this.lblInstalledTitle.Size = new System.Drawing.Size(784, 30);
            this.lblInstalledTitle.Text = "  📦 已安装的 AutoCAD";
            this.lblInstalledTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblInstalledTitle.BackColor = System.Drawing.Color.FromArgb(240, 244, 250);

            // ====================================================================
            // 产品列表 DataGridView
            // ====================================================================
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AllowUserToDeleteRows = false;
            this.dgvProducts.BackgroundColor = System.Drawing.Color.White;
            this.dgvProducts.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvProducts.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dgvProducts.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgvProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvProducts.ColumnHeadersHeight = 35;
            this.dgvProducts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colSelect, this.colProductName, this.colVersion, this.colPath
            });
            this.dgvProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProducts.GridColor = System.Drawing.Color.FromArgb(230, 235, 245);
            this.dgvProducts.Location = new System.Drawing.Point(0, 30);
            this.dgvProducts.Name = "dgvProducts";
            this.dgvProducts.RowHeadersVisible = false;
            this.dgvProducts.RowTemplate.Height = 30;
            this.dgvProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProducts.Size = new System.Drawing.Size(784, 220);
            this.dgvProducts.StandardTab = true;
            this.dgvProducts.AllowUserToResizeRows = false;

            // 列设置
            this.colSelect.HeaderText = "选中";
            this.colSelect.Name = "colSelect";
            this.colSelect.Width = 50;
            this.colSelect.FlatStyle = System.Windows.Forms.FlatStyle.Standard;

            this.colProductName.HeaderText = "产品名称";
            this.colProductName.Name = "colProductName";
            this.colProductName.ReadOnly = true;
            this.colProductName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colProductName.Width = 180;

            this.colVersion.HeaderText = "版本";
            this.colVersion.Name = "colVersion";
            this.colVersion.ReadOnly = true;
            this.colVersion.Width = 100;

            this.colPath.HeaderText = "安装路径";
            this.colPath.Name = "colPath";
            this.colPath.ReadOnly = true;
            this.colPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;

            // ====================================================================
            // 按钮面板
            // ====================================================================
            this.btnPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnPanel.Height = 50;
            this.btnPanel.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.btnPanel.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);

            // --- 扫描按钮 ---
            this.btnScan.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnScan.Width = 120;
            this.btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScan.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(40, 120, 200);
            this.btnScan.FlatAppearance.BorderSize = 1;
            this.btnScan.BackColor = System.Drawing.Color.FromArgb(40, 120, 200);
            this.btnScan.ForeColor = System.Drawing.Color.White;
            this.btnScan.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F,
                System.Drawing.FontStyle.Bold);
            this.btnScan.Text = "🔍 开始扫描";
            this.btnScan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScan.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);

            // --- 全选按钮 ---
            this.btnSelectAll.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSelectAll.Width = 80;
            this.btnSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(150, 160, 180);
            this.btnSelectAll.FlatAppearance.BorderSize = 1;
            this.btnSelectAll.BackColor = System.Drawing.Color.White;
            this.btnSelectAll.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.btnSelectAll.Text = "全选";
            this.btnSelectAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectAll.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);

            // --- 反选按钮 ---
            this.btnSelectNone.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnSelectNone.Width = 80;
            this.btnSelectNone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelectNone.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(150, 160, 180);
            this.btnSelectNone.FlatAppearance.BorderSize = 1;
            this.btnSelectNone.BackColor = System.Drawing.Color.White;
            this.btnSelectNone.ForeColor = System.Drawing.Color.FromArgb(60, 60, 60);
            this.btnSelectNone.Text = "反选";
            this.btnSelectNone.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSelectNone.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.btnSelectNone.Click += new System.EventHandler(this.btnSelectNone_Click);

            // --- 清理按钮 ---
            this.btnClean.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClean.Width = 120;
            this.btnClean.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClean.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 60, 60);
            this.btnClean.FlatAppearance.BorderSize = 1;
            this.btnClean.BackColor = System.Drawing.Color.FromArgb(200, 60, 60);
            this.btnClean.ForeColor = System.Drawing.Color.White;
            this.btnClean.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F,
                System.Drawing.FontStyle.Bold);
            this.btnClean.Text = "🗑 开始清理";
            this.btnClean.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClean.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnClean.Click += new System.EventHandler(this.btnClean_Click);

            // --- 退出按钮 ---
            this.btnExit.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnExit.Width = 80;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 180, 180);
            this.btnExit.FlatAppearance.BorderSize = 1;
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.btnExit.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            this.btnExit.Text = "退出";
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);

            // 按钮添加到面板（注意顺序影响 Dock 布局）
            this.btnPanel.Controls.Add(this.btnSelectNone);
            this.btnPanel.Controls.Add(this.btnSelectAll);
            this.btnPanel.Controls.Add(this.btnScan);
            this.btnPanel.Controls.Add(this.btnExit);
            this.btnPanel.Controls.Add(this.btnClean);

            // ====================================================================
            // 结果标签
            // ====================================================================
            this.lblResultTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblResultTitle.Font = new System.Drawing.Font(
                "Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblResultTitle.ForeColor = System.Drawing.Color.FromArgb(30, 120, 60);
            this.lblResultTitle.Location = new System.Drawing.Point(0, 50);
            this.lblResultTitle.Name = "lblResultTitle";
            this.lblResultTitle.Size = new System.Drawing.Size(784, 25);
            this.lblResultTitle.Text = "  📋 扫描结果 / 清理日志";
            this.lblResultTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblResultTitle.BackColor = System.Drawing.Color.FromArgb(240, 250, 240);

            // ====================================================================
            // 日志文本框
            // ====================================================================
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Cascadia Code", 9.5F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(200, 220, 200);
            this.txtLog.Location = new System.Drawing.Point(0, 75);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(784, 486);
            this.txtLog.WordWrap = false;

            // ====================================================================
            // 状态栏
            // ====================================================================
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(240, 243, 248);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.lblStatus, this.progressBar
            });

            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(600, 17);
            this.lblStatus.Text = "就绪 - 请先扫描检测 AutoCAD 安装情况";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(180, 16);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.Visible = false;

            // ====================================================================
            // 主窗体
            // ====================================================================
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);
            this.ClientSize = new System.Drawing.Size(784, 581);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.MinimumSize = new System.Drawing.Size(600, 450);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoCAD 卸载清理工具 v1.0";

            // 恢复布局
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProducts)).EndInit();
            this.btnPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // ====================================================================
        // 控件声明
        // ====================================================================

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.SplitContainer splitContainer;

        // 上半部分
        private System.Windows.Forms.Label lblInstalledTitle;
        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPath;

        // 按钮
        private System.Windows.Forms.Panel btnPanel;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnClean;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnSelectNone;
        private System.Windows.Forms.Button btnExit;

        // 下半部分
        private System.Windows.Forms.Label lblResultTitle;
        private System.Windows.Forms.TextBox txtLog;

        // 状态栏
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
    }
}
