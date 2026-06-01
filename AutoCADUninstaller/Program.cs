// ============================================================================
// Program.cs - 程序入口点
// ============================================================================
// 这是程序的入口文件，负责：
//   1. 检查管理员权限
//   2. 启用视觉样式
//   3. 创建并显示主窗口
//
// Windows Forms 程序的启动流程：
//   ApplicationConfiguration.Initialize() -> .NET 8 配置初始化
//   Application.Run(new MainForm())       -> 创建主窗口并开始消息循环
//
// 学习要点：
//   1. Main 方法是 C# 程序的入口点
//   2. Application 类管理 WinForms 程序的生命周期
//   3. 消息循环 (Message Loop) 是 Windows 程序的核心机制
// ============================================================================

using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace AutoCADUninstaller
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            // ================================================================
            // 1. 检查管理员权限
            // ================================================================
            // 注册表操作和系统服务管理需要管理员权限
            if (!IsRunningAsAdmin())
            {
                MessageBox.Show(
                    "本工具需要管理员权限才能运行！\n\n" +
                    "请右键点击程序，选择\"以管理员身份运行\"。",
                    "需要管理员权限",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // ================================================================
            // 2. .NET 8 WinForms 初始化
            // ================================================================
            // ApplicationConfiguration.Initialize() 是 .NET 8 新增的简化写法
            // 它等价于旧版的 EnableVisualStyles() + SetCompatibleTextRenderingDefault()
            ApplicationConfiguration.Initialize();

            // ================================================================
            // 3. 设置异常处理
            // ================================================================
            // 全局异常处理 - 防止程序意外崩溃
            Application.ThreadException += (sender, args) =>
            {
                MessageBox.Show(
                    $"程序发生错误：\n{args.Exception.Message}\n\n" +
                    "建议关闭程序后以管理员身份重新运行。",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show(
                    $"程序发生严重错误：\n{ex?.Message}\n\n" +
                    "程序将关闭。",
                    "严重错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            // ================================================================
            // 4. 启动主窗口
            // ================================================================
            // Application.Run() 会创建主窗口并开始 Windows 消息循环
            // 消息循环会持续运行，处理用户的鼠标、键盘等输入
            // 直到用户关闭主窗口，Application.Run() 才会返回
            Application.Run(new MainForm());
        }

        /// <summary>
        /// 检查当前程序是否以管理员身份运行
        /// </summary>
        /// <returns>是否是管理员权限</returns>
        private static bool IsRunningAsAdmin()
        {
            try
            {
                // 获取当前 Windows 身份
                WindowsIdentity identity = WindowsIdentity.GetCurrent();

                // 创建 Windows 主体
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                // 检查是否是管理员角色
                // WindowsBuiltInRole.Administrator 表示管理员角色
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                // 如果检查失败，假设有管理员权限
                // （宁可运行也不要因为检查失败而阻止用户）
                return true;
            }
        }
    }
}
