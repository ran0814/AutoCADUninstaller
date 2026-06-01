// ============================================================================
// MainForm.cs - 主窗体逻辑
// ============================================================================
// 这是程序的主窗口，包含所有的用户交互逻辑
//
// WinForms 编程基础：
//   1. 事件驱动：按钮点击、文本变化等会触发事件
//   2. 控件属性：通过属性面板或代码设置控件外观
//   3. 异步更新：长时间操作需要用 async/await 避免界面卡顿
//
// 本文件的职责：
//   - 初始化产品列表
//   - 处理扫描按钮点击
//   - 处理清理按钮点击
//   - 显示扫描结果和清理日志
//   - 更新进度条和状态栏
// ============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoCADUninstaller.Models;
using AutoCADUninstaller.Services;

namespace AutoCADUninstaller
{
    public partial class MainForm : Form
    {
        // ====================================================================
        // 成员变量
        // ====================================================================

        /// <summary>扫描服务实例</summary>
        private readonly Scanner _scanner = new Scanner();

        /// <summary>清理服务实例</summary>
        private readonly Cleaner _cleaner = new Cleaner();

        /// <summary>已检测到的产品列表</summary>
        private List<ProductInfo> _installedProducts = new List<ProductInfo>();

        /// <summary>扫描结果</summary>
        private ScanResult _scanResult = null;

        // ====================================================================
        // 构造函数
        // ====================================================================

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        /// <summary>
        /// 初始化 UI 状态
        /// </summary>
        private void InitializeUI()
        {
            // 设置初始状态
            btnClean.Enabled = false;
            progressBar.Visible = false;

            // 初始化日志
            AppendLog("═══════════════════════════════════════════");
            AppendLog("   AutoCAD 卸载清理工具 v1.0");
            AppendLog("   基于 Autobox 逆向分析开发");
            AppendLog("═══════════════════════════════════════════");
            AppendLog("");
            AppendLog("使用说明:");
            AppendLog("  1. 点击 [开始扫描] 检测系统中的 AutoCAD");
            AppendLog("  2. 在上方列表中选择要清理的项目");
            AppendLog("  3. 点击 [开始清理] 执行清理操作");
            AppendLog("");
            AppendLog("注意: 本工具需要管理员权限运行");
            AppendLog("═══════════════════════════════════════════");
            AppendLog("");
        }

        // ====================================================================
        // 事件处理 - 扫描
        // ====================================================================

        /// <summary>
        /// 扫描按钮点击事件
        /// 检测已安装的 AutoCAD 并扫描残留物
        /// </summary>
        private async void btnScan_Click(object sender, EventArgs e)
        {
            // 禁用按钮，防止重复点击
            SetButtonsEnabled(false);
            ClearLog();
            progressBar.Visible = true;
            progressBar.Value = 0;
            progressBar.Maximum = 100;

            try
            {
                // 创建进度报告器
                // 这允许后台任务更新 UI
                var progress = new Progress<string>(message =>
                {
                    AppendLog(message);
                    UpdateProgress(20); // 扫描阶段占 20%
                });

                // --- 第1步：检测已安装产品 ---
                UpdateStatus("正在检测已安装的 AutoCAD...");
                AppendLog("🔍 开始检测已安装的 AutoCAD...");
                AppendLog("");

                await Task.Run(() =>
                {
                    _installedProducts = _scanner.DetectInstalledProducts();
                });

                // 显示检测到的产品
                DisplayProducts(_installedProducts);

                if (_installedProducts.Count == 0)
                {
                    AppendLog("未检测到已安装的 AutoCAD");
                    AppendLog("");
                }
                else
                {
                    AppendLog($"检测到 {_installedProducts.Count} 个已安装的 AutoCAD 版本");
                    AppendLog("");
                }

                UpdateProgress(30);

                // --- 第2步：扫描残留物 ---
                UpdateStatus("正在扫描残留物...");
                AppendLog("🔍 开始扫描残留物...");
                AppendLog("───────────────────────────────────────");

                _scanResult = await _scanner.ScanAllResiduesAsync(progress);

                UpdateProgress(100);

                // 显示扫描结果摘要
                AppendLog("");
                AppendLog("═══════════════════════════════════════════");
                AppendLog("  扫描结果摘要");
                AppendLog("═══════════════════════════════════════════");
                AppendLog($"  服务残留:     {_scanResult.ServiceCount} 项");
                AppendLog($"  进程残留:     {_scanResult.ProcessCount} 项");
                AppendLog($"  文件残留:     {_scanResult.FileCount} 项 ({_scanResult.TotalSizeDisplay})");
                AppendLog($"  注册表残留:   {_scanResult.RegistryCount} 项");
                AppendLog($"  快捷方式残留: {_scanResult.ShortcutCount} 项");
                AppendLog($"  卸载信息残留: {_scanResult.UninstallCount} 项");
                AppendLog("───────────────────────────────────────");
                AppendLog($"  总计: {_scanResult.TotalCount} 项残留物");
                AppendLog("═══════════════════════════════════════════");

                // 启用清理按钮
                if (_scanResult.TotalCount > 0)
                {
                    btnClean.Enabled = true;
                    UpdateStatus($"扫描完成 - 发现 {_scanResult.TotalCount} 项残留物，可以开始清理");
                }
                else
                {
                    UpdateStatus("扫描完成 - 未发现残留物，系统很干净！");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"");
                AppendLog($"✗ 扫描出错: {ex.Message}");
                UpdateStatus("扫描出错");
            }
            finally
            {
                SetButtonsEnabled(true);
                progressBar.Visible = false;
            }
        }

        // ====================================================================
        // 事件处理 - 清理
        // ====================================================================

        /// <summary>
        /// 清理按钮点击事件
        /// 执行清理操作
        /// </summary>
        private async void btnClean_Click(object sender, EventArgs e)
        {
            if (_scanResult == null || _scanResult.TotalCount == 0)
            {
                MessageBox.Show("请先扫描残留物！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 确认对话框
            DialogResult confirm = MessageBox.Show(
                $"确定要清理 {_scanResult.TotalCount} 项残留物吗？\n\n" +
                "此操作将：\n" +
                "  - 终止 Autodesk 相关进程\n" +
                "  - 停止并删除 Autodesk 服务\n" +
                "  - 删除残留文件和目录\n" +
                "  - 清理注册表键值\n\n" +
                "建议在清理前关闭所有 AutoCAD 相关程序。",
                "确认清理",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            // 禁用按钮
            SetButtonsEnabled(false);
            btnClean.Enabled = false;
            progressBar.Visible = true;
            progressBar.Value = 0;
            progressBar.Maximum = 100;

            try
            {
                var progress = new Progress<string>(message =>
                {
                    AppendLog(message);
                    // 简单的进度递增
                    if (progressBar.Value < progressBar.Maximum)
                    {
                        progressBar.Value = Math.Min(
                            progressBar.Value + 2,
                            progressBar.Maximum);
                    }
                });

                UpdateStatus("正在清理...");
                AppendLog("");
                AppendLog("🚀 开始清理操作...");
                AppendLog("");

                // 执行清理
                CleanResult result = await _cleaner.CleanAllAsync(
                    _scanResult.AllResidues, progress);

                // 显示结果
                AppendLog("");
                AppendLog("═══════════════════════════════════════════");
                AppendLog("  清理结果");
                AppendLog("═══════════════════════════════════════════");
                AppendLog($"  终止进程:   {result.ProcessesKilled}");
                AppendLog($"  停止服务:   {result.ServicesStopped}");
                AppendLog($"  删除文件:   {result.FilesDeleted}");
                AppendLog($"  清理注册表: {result.RegistryKeysDeleted}");
                AppendLog($"  释放空间:   {result.SpaceFreedDisplay}");
                AppendLog("═══════════════════════════════════════════");

                if (result.Errors.Count > 0)
                {
                    AppendLog($"");
                    AppendLog($"⚠ 有 {result.Errors.Count} 个操作失败:");
                    foreach (string error in result.Errors)
                    {
                        AppendLog($"  - {error}");
                    }
                }

                UpdateStatus("清理完成！");

                // 显示完成对话框
                MessageBox.Show(
                    $"清理完成！\n\n" +
                    $"终止进程: {result.ProcessesKilled}\n" +
                    $"停止服务: {result.ServicesStopped}\n" +
                    $"删除文件: {result.FilesDeleted}\n" +
                    $"释放空间: {result.SpaceFreedDisplay}\n\n" +
                    "建议重启计算机以完成清理。",
                    "完成",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppendLog($"");
                AppendLog($"✗ 清理出错: {ex.Message}");
                UpdateStatus("清理出错");
            }
            finally
            {
                SetButtonsEnabled(true);
                progressBar.Visible = false;
            }
        }

        // ====================================================================
        // 事件处理 - 其他按钮
        // ====================================================================

        /// <summary>
        /// 全选按钮
        /// </summary>
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                row.Cells["colSelect"].Value = true;
            }
        }

        /// <summary>
        /// 反选按钮
        /// </summary>
        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                bool currentValue = Convert.ToBoolean(row.Cells["colSelect"].Value);
                row.Cells["colSelect"].Value = !currentValue;
            }
        }

        /// <summary>
        /// 退出按钮
        /// </summary>
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ====================================================================
        // UI 辅助方法
        // ====================================================================

        /// <summary>
        /// 显示检测到的产品列表
        /// </summary>
        private void DisplayProducts(List<ProductInfo> products)
        {
            dgvProducts.Rows.Clear();

            foreach (var product in products)
            {
                int rowIndex = dgvProducts.Rows.Add(
                    product.IsSelected,
                    product.ProductName,
                    product.Version,
                    product.InstallPath ?? "(路径未知)");

                // 设置行的 Tag 属性，存储产品信息
                dgvProducts.Rows[rowIndex].Tag = product;
            }
        }

        /// <summary>
        /// 追加日志文本
        /// </summary>
        private void AppendLog(string message)
        {
            if (this.InvokeRequired)
            {
                // 如果在后台线程，需要通过 Invoke 回到 UI 线程
                this.Invoke(new Action<string>(AppendLog), message);
                return;
            }

            txtLog.AppendText(message + Environment.NewLine);

            // 自动滚动到底部
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        private void ClearLog()
        {
            txtLog.Clear();
        }

        /// <summary>
        /// 更新状态栏文本
        /// </summary>
        private void UpdateStatus(string text)
        {
            lblStatus.Text = text;
        }

        /// <summary>
        /// 更新进度条
        /// </summary>
        private void UpdateProgress(int value)
        {
            if (progressBar.Value < value)
            {
                progressBar.Value = Math.Min(value, progressBar.Maximum);
            }
        }

        /// <summary>
        /// 设置按钮启用/禁用状态
        /// </summary>
        private void SetButtonsEnabled(bool enabled)
        {
            btnScan.Enabled = enabled;
            btnSelectAll.Enabled = enabled;
            btnSelectNone.Enabled = enabled;

            // 清理按钮的启用状态取决于是否有扫描结果
            if (enabled && _scanResult != null && _scanResult.TotalCount > 0)
            {
                btnClean.Enabled = true;
            }
            else if (!enabled)
            {
                btnClean.Enabled = false;
            }
        }
    }
}
