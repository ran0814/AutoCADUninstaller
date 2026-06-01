// ============================================================================
// ProcessHelper.cs - 进程管理帮助类
// ============================================================================
// 负责进程的查询、终止操作
//
// 什么是进程？
// 进程是正在运行的程序的实例
// 例如：当您打开 AutoCAD，系统会创建一个 acad.exe 进程
//
// Autodesk 相关进程：
//   - acad.exe: AutoCAD 主程序
//   - AcCoreConsole.exe: AutoCAD 核心控制台
//   - AdskLicensingService.exe: 许可证服务进程
//
// 学习要点：
//   1. Process 类是 .NET 提供的进程管理类
//   2. Kill() 强制终止进程（类似任务管理器的"结束任务"）
//   3. CloseMainWindow() 优雅关闭（发送关闭消息）
// ============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AutoCADUninstaller.Services
{
    /// <summary>
    /// 进程信息 - 存储单个进程的状态
    /// </summary>
    public class ProcessInfo
    {
        /// <summary>进程名称（不含扩展名）</summary>
        public string Name { get; set; }

        /// <summary>进程 ID</summary>
        public int Id { get; set; }

        /// <summary>进程完整路径</summary>
        public string FilePath { get; set; }

        /// <summary>进程内存使用量</summary>
        public string MemoryUsage { get; set; }

        /// <summary>是否被选中</summary>
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// 进程管理帮助类 - 静态类
    /// 提供进程的查询和终止功能
    /// </summary>
    public static class ProcessHelper
    {
        // ====================================================================
        // Autodesk 相关进程名称
        // 这些是需要检测和终止的进程
        // 注意：这里不需要 .exe 扩展名
        // ====================================================================

        private static readonly string[] AutodeskProcessNames = {
            "acad",                 // AutoCAD 主程序
            "AcCoreConsole",        // AutoCAD 核心控制台
            "AcEventSync",          // AutoCAD 事件同步
            "AdskLicensingService", // 许可证服务
            "AdskLicensingAgent",   // 许可证代理
            "AutodeskDesktopApp"    // 桌面应用
        };

        // ====================================================================
        // 查询操作
        // ====================================================================

        /// <summary>
        /// 获取所有 Autodesk 相关进程
        /// </summary>
        /// <returns>进程信息列表</returns>
        public static List<ProcessInfo> GetAutodeskProcesses()
        {
            var processes = new List<ProcessInfo>();

            foreach (string processName in AutodeskProcessNames)
            {
                try
                {
                    // 获取指定名称的所有进程
                    // 一个程序可以运行多个实例（多个进程）
                    Process[] foundProcesses = Process.GetProcessesByName(processName);

                    foreach (Process proc in foundProcesses)
                    {
                        try
                        {
                            string filePath = "";
                            try
                            {
                                // 获取进程的完整路径
                                // 注意：如果进程已退出，这会抛出异常
                                filePath = proc.MainModule.FileName;
                            }
                            catch
                            {
                                // 无法获取路径（可能是系统进程）
                                filePath = "(无法获取路径)";
                            }

                            processes.Add(new ProcessInfo
                            {
                                Name = proc.ProcessName,
                                Id = proc.Id,
                                FilePath = filePath,
                                MemoryUsage = FormatMemorySize(proc.WorkingSet64),
                                IsSelected = true
                            });
                        }
                        catch
                        {
                            // 进程已退出或无法访问
                        }
                    }
                }
                catch
                {
                    // 进程名不存在
                }
            }

            return processes;
        }

        /// <summary>
        /// 检查是否有 Autodesk 进程正在运行
        /// </summary>
        /// <returns>是否在运行</returns>
        public static bool IsAnyAutodeskProcessRunning()
        {
            foreach (string processName in AutodeskProcessNames)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        // ====================================================================
        // 终止操作
        // ====================================================================

        /// <summary>
        /// 终止 Autodesk 相关进程
        /// 先尝试优雅关闭，超时后强制终止
        /// </summary>
        /// <param name="progress">进度回调</param>
        /// <returns>成功终止的进程数量</returns>
        public static int KillAutodeskProcesses(IProgress<string> progress = null)
        {
            int killedCount = 0;

            foreach (string processName in AutodeskProcessNames)
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(processName);

                    foreach (Process proc in processes)
                    {
                        try
                        {
                            progress?.Report($"正在终止进程: {processName} (PID: {proc.Id})");

                            // 方法1：尝试优雅关闭（发送 WM_CLOSE 消息）
                            // 这样程序有机会保存数据
                            proc.CloseMainWindow();

                            // 等待 5 秒看进程是否自行退出
                            bool exited = proc.WaitForExit(5000);

                            if (!exited)
                            {
                                // 方法2：超时了，强制终止
                                progress?.Report($"  进程未响应，强制终止...");
                                proc.Kill();

                                // 等待进程真正退出
                                proc.WaitForExit(3000);
                            }

                            killedCount++;
                            progress?.Report($"✓ 进程 {processName} 已终止");
                        }
                        catch (Exception ex)
                        {
                            progress?.Report($"✗ 终止进程 {processName} 失败: {ex.Message}");
                        }
                    }
                }
                catch
                {
                    // 获取进程失败
                }
            }

            return killedCount;
        }

        /// <summary>
        /// 强制终止指定进程（按 PID）
        /// </summary>
        /// <param name="processId">进程 ID</param>
        /// <returns>是否成功</returns>
        public static bool KillProcess(int processId)
        {
            try
            {
                Process proc = Process.GetProcessById(processId);
                proc.Kill();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ====================================================================
        // 辅助方法
        // ====================================================================

        /// <summary>
        /// 格式化内存大小显示
        /// 例如：1024 -> "1.0 KB"，1048576 -> "1.0 MB"
        /// </summary>
        private static string FormatMemorySize(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
            if (bytes >= 1024L * 1024)
                return $"{bytes / (1024.0 * 1024):F1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024.0:F1} KB";

            return $"{bytes} B";
        }
    }
}
