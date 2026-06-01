// ============================================================================
// ServiceHelper.cs - Windows 服务帮助类
// ============================================================================
// 负责 Windows 服务的查询、停止和删除操作
//
// 什么是 Windows 服务？
// Windows 服务是在后台运行的程序，不需要用户登录就能运行
// 例如：杀毒软件、打印机服务、网络服务等
//
// Autodesk 相关服务：
//   - AdskLicensingService: 许可证管理服务
//   - AdskLicensingAgent: 许可证代理服务
//   - Autodesk Desktop App: 桌面应用程序服务
//
// 学习要点：
//   1. ServiceController 类用于控制 Windows 服务
//   2. 服务有多种状态：Running(运行中)、Stopped(已停止)等
//   3. 删除服务需要使用 sc.exe 命令行工具
// ============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace AutoCADUninstaller.Services
{
    /// <summary>
    /// 服务信息 - 存储单个服务的状态
    /// </summary>
    public class ServiceInfo
    {
        /// <summary>服务内部名称</summary>
        public string Name { get; set; }

        /// <summary>服务显示名称</summary>
        public string DisplayName { get; set; }

        /// <summary>服务当前状态</summary>
        public string Status { get; set; }

        /// <summary>是否正在运行</summary>
        public bool IsRunning { get; set; }

        /// <summary>是否被选中</summary>
        public bool IsSelected { get; set; }
    }

    /// <summary>
    /// 服务帮助类 - 静态类
    /// 提供 Windows 服务的管理功能
    /// </summary>
    public static class ServiceHelper
    {
        // ====================================================================
        // Autodesk 相关服务名称列表
        // 这些是需要检测和清理的服务
        // ====================================================================

        private static readonly string[] AutodeskServiceNames = {
            "AdskLicensingService",              // Autodesk 许可证服务
            "AdskLicensingAgent",                // Autodesk 许可证代理
            "Autodesk Desktop App",              // Autodesk 桌面应用
            "Autodesk Desktop Licensing Service", // 桌面许可证服务
            "Autodesk AM Service",               // AM 服务
            "Autodesk AM Static Service"          // AM 静态服务
        };

        // ====================================================================
        // 查询操作
        // ====================================================================

        /// <summary>
        /// 获取所有 Autodesk 相关服务
        /// </summary>
        /// <returns>服务信息列表</returns>
        public static List<ServiceInfo> GetAutodeskServices()
        {
            var services = new List<ServiceInfo>();

            try
            {
                // 获取系统中所有已安装的服务
                ServiceController[] allServices = ServiceController.GetServices();

                foreach (ServiceController service in allServices)
                {
                    // 检查是否是 Autodesk 相关服务
                    if (IsAutodeskService(service.ServiceName))
                    {
                        services.Add(new ServiceInfo
                        {
                            Name = service.ServiceName,
                            DisplayName = service.DisplayName,
                            Status = service.Status.ToString(),
                            IsRunning = service.Status == ServiceControllerStatus.Running,
                            IsSelected = service.Status == ServiceControllerStatus.Running
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取服务列表失败: {ex.Message}");
            }

            return services;
        }

        /// <summary>
        /// 检查是否是 Autodesk 相关服务
        /// </summary>
        private static bool IsAutodeskService(string serviceName)
        {
            foreach (string autodeskName in AutodeskServiceNames)
            {
                // 使用 Contains 进行模糊匹配（服务名可能有变体）
                if (serviceName.IndexOf(autodeskName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        // ====================================================================
        // 停止操作
        // ====================================================================

        /// <summary>
        /// 停止所有 Autodesk 服务
        /// 使用 async/await 异步执行，避免界面卡顿
        /// </summary>
        /// <param name="progress">进度回调，参数是当前正在处理的服务名</param>
        /// <returns>成功停止的服务数量</returns>
        public static async Task<int> StopAutodeskServicesAsync(
            IProgress<string> progress = null)
        {
            int stoppedCount = 0;

            foreach (string serviceName in AutodeskServiceNames)
            {
                try
                {
                    progress?.Report($"正在停止服务: {serviceName}...");

                    ServiceController service = new ServiceController(serviceName);

                    // 检查服务是否存在且正在运行
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();

                        // 等待服务停止（最多等待 30 秒）
                        // WaitForStatus 是阻塞操作，所以在 Task.Run 中执行
                        await Task.Run(() =>
                        {
                            service.WaitForStatus(
                                ServiceControllerStatus.Stopped,
                                TimeSpan.FromSeconds(30));
                        });

                        stoppedCount++;
                        progress?.Report($"✓ 服务 {serviceName} 已停止");
                    }
                    else if (service.Status == ServiceControllerStatus.Paused)
                    {
                        // 如果服务是暂停状态，先继续再停止
                        service.Continue();
                        await Task.Delay(1000);
                        service.Stop();
                        stoppedCount++;
                        progress?.Report($"✓ 服务 {serviceName} 已停止");
                    }
                    else
                    {
                        progress?.Report($"- 服务 {serviceName} 未在运行");
                    }
                }
                catch (InvalidOperationException)
                {
                    // 服务不存在
                    progress?.Report($"- 服务 {serviceName} 不存在");
                }
                catch (Exception ex)
                {
                    progress?.Report($"✗ 停止服务 {serviceName} 失败: {ex.Message}");
                }
            }

            return stoppedCount;
        }

        // ====================================================================
        // 删除操作
        // ====================================================================

        /// <summary>
        /// 删除 Windows 服务
        /// 使用 sc.exe 命令行工具删除服务
        /// 注意：需要管理员权限
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>是否删除成功</returns>
        public static bool DeleteService(string serviceName)
        {
            try
            {
                // 使用 sc delete 命令删除服务
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"delete \"{serviceName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit(10000); // 等待最多 10 秒
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"删除服务失败 {serviceName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 批量删除 Autodesk 服务
        /// </summary>
        /// <param name="progress">进度回调</param>
        /// <returns>成功删除的服务数量</returns>
        public static async Task<int> DeleteAutodeskServicesAsync(
            IProgress<string> progress = null)
        {
            int deletedCount = 0;

            foreach (string serviceName in AutodeskServiceNames)
            {
                try
                {
                    progress?.Report($"正在删除服务: {serviceName}...");

                    bool result = await Task.Run(() => DeleteService(serviceName));

                    if (result)
                    {
                        deletedCount++;
                        progress?.Report($"✓ 服务 {serviceName} 已删除");
                    }
                    else
                    {
                        progress?.Report($"- 服务 {serviceName} 删除失败或不存在");
                    }
                }
                catch (Exception ex)
                {
                    progress?.Report($"✗ 删除服务 {serviceName} 失败: {ex.Message}");
                }
            }

            return deletedCount;
        }
    }
}
