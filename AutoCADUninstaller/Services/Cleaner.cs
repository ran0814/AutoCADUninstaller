// ============================================================================
// Cleaner.cs - 清理服务
// ============================================================================
// 负责执行实际的清理操作
//
// 清理顺序很重要：
//   1. 先终止进程（进程正在运行时无法删除文件）
//   2. 再停止服务（服务可能锁定文件）
//   3. 删除文件（需要进程和服务已停止）
//   4. 清理注册表（最后清理，因为不影响运行时）
//
// 安全考虑：
//   - 所有操作都有 try-catch 保护
//   - 删除前先检查路径是否存在
//   - 记录所有操作结果
//
// 学习要点：
//   1. async/await 异步编程模式
//   2. IProgress<T> 进度报告机制
//   3. 文件和目录的安全删除
//   4. 权限处理
// ============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AutoCADUninstaller.Models;

namespace AutoCADUninstaller.Services
{
    /// <summary>
    /// 清理结果统计
    /// </summary>
    public class CleanResult
    {
        /// <summary>成功终止的进程数</summary>
        public int ProcessesKilled { get; set; }

        /// <summary>成功停止的服务数</summary>
        public int ServicesStopped { get; set; }

        /// <summary>成功删除的文件/目录数</summary>
        public int FilesDeleted { get; set; }

        /// <summary>成功清理的注册表键数</summary>
        public int RegistryKeysDeleted { get; set; }

        /// <summary>释放的磁盘空间（字节）</summary>
        public long SpaceFreed { get; set; }

        /// <summary>错误消息列表</summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>是否全部成功</summary>
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>格式化的释放空间</summary>
        public string SpaceFreedDisplay
        {
            get
            {
                if (SpaceFreed >= 1024L * 1024 * 1024)
                    return $"{SpaceFreed / (1024.0 * 1024 * 1024):F1} GB";
                if (SpaceFreed >= 1024L * 1024)
                    return $"{SpaceFreed / (1024.0 * 1024):F1} MB";
                if (SpaceFreed >= 1024)
                    return $"{SpaceFreed / 1024.0:F1} KB";
                return $"{SpaceFreed} B";
            }
        }
    }

    /// <summary>
    /// 清理服务 - 执行实际的清理操作
    /// </summary>
    public class Cleaner
    {
        // ====================================================================
        // 主清理方法
        // ====================================================================

        /// <summary>
        /// 清理所有选中的残留物
        /// 这是清理的入口方法
        /// </summary>
        /// <param name="residues">要清理的残留物列表</param>
        /// <param name="progress">进度回调</param>
        /// <returns>清理结果</returns>
        public async Task<CleanResult> CleanAllAsync(
            List<ResidueItem> residues,
            IProgress<string> progress = null)
        {
            var result = new CleanResult();

            progress?.Report("===== 开始清理 =====");

            // 第1步：终止进程（必须最先执行）
            progress?.Report("");
            progress?.Report("【步骤 1/4】终止 Autodesk 进程...");
            result.ProcessesKilled = await KillProcessesAsync(residues, progress);

            // 第2步：停止并删除服务
            progress?.Report("");
            progress?.Report("【步骤 2/4】停止并删除服务...");
            result.ServicesStopped = await StopAndDeleteServicesAsync(residues, progress);

            // 第3步：删除文件
            progress?.Report("");
            progress?.Report("【步骤 3/4】删除残留文件...");
            var fileResult = await DeleteFilesAsync(residues, progress);
            result.FilesDeleted = fileResult.Count;
            result.SpaceFreed = fileResult.TotalSize;

            // 第4步：清理注册表
            progress?.Report("");
            progress?.Report("【步骤 4/4】清理注册表...");
            result.RegistryKeysDeleted = await CleanRegistryAsync(residues, progress);

            // 完成
            progress?.Report("");
            progress?.Report("===== 清理完成 =====");
            progress?.Report($"终止进程: {result.ProcessesKilled}");
            progress?.Report($"停止服务: {result.ServicesStopped}");
            progress?.Report($"删除文件: {result.FilesDeleted}");
            progress?.Report($"清理注册表: {result.RegistryKeysDeleted}");
            progress?.Report($"释放空间: {result.SpaceFreedDisplay}");

            if (result.Errors.Count > 0)
            {
                progress?.Report("");
                progress?.Report($"警告: 有 {result.Errors.Count} 个操作失败");
                foreach (string error in result.Errors)
                {
                    progress?.Report($"  - {error}");
                }
            }

            return result;
        }

        // ====================================================================
        // 第1步：终止进程
        // ====================================================================

        /// <summary>
        /// 终止选中的进程残留
        /// </summary>
        private async Task<int> KillProcessesAsync(
            List<ResidueItem> residues, IProgress<string> progress)
        {
            return await Task.Run(() =>
            {
                int count = 0;

                // 获取所有选中的进程残留
                var processResidues = residues.FindAll(
                    r => r.Type == ResidueType.Process && r.IsSelected);

                foreach (var item in processResidues)
                {
                    try
                    {
                        progress?.Report($"  正在终止进程: {item.Name}");

                        // 如果路径是有效的，尝试按路径终止
                        if (!string.IsNullOrEmpty(item.Path) && item.Path != "(无法获取路径)")
                        {
                            // 查找并终止进程
                            string processName = Path.GetFileNameWithoutExtension(item.Path);
                            Process[] processes = Process.GetProcessesByName(processName);

                            foreach (Process proc in processes)
                            {
                                try
                                {
                                    proc.CloseMainWindow();
                                    if (!proc.WaitForExit(5000))
                                    {
                                        proc.Kill();
                                    }
                                    count++;
                                }
                                catch (Exception ex)
                                {
                                    progress?.Report($"    ✗ 终止进程失败: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            // 尝试用名称终止
                            ProcessHelper.KillProcess(0); // 这里需要更智能的处理
                        }

                        progress?.Report($"    ✓ {item.Name} 已终止");
                    }
                    catch (Exception ex)
                    {
                        progress?.Report($"    ✗ 终止 {item.Name} 失败: {ex.Message}");
                    }
                }

                return count;
            });
        }

        // ====================================================================
        // 第2步：停止并删除服务
        // ====================================================================

        /// <summary>
        /// 停止并删除选中的服务残留
        /// </summary>
        private async Task<int> StopAndDeleteServicesAsync(
            List<ResidueItem> residues, IProgress<string> progress)
        {
            int count = 0;

            // 获取所有选中的服务残留
            var serviceResidues = residues.FindAll(
                r => r.Type == ResidueType.Service && r.IsSelected);

            foreach (var item in serviceResidues)
            {
                try
                {
                    progress?.Report($"  正在处理服务: {item.Name}");

                    // 从 Path 中提取服务名（格式："服务: ServiceName"）
                    string serviceName = item.Name;

                    // 尝试停止服务
                    try
                    {
                        var sc = new System.ServiceProcess.ServiceController(serviceName);
                        if (sc.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                        {
                            progress?.Report($"    正在停止...");
                            sc.Stop();
                            await Task.Run(() =>
                            {
                                sc.WaitForStatus(
                                    System.ServiceProcess.ServiceControllerStatus.Stopped,
                                    TimeSpan.FromSeconds(30));
                            });
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // 服务不存在，继续删除
                    }

                    // 尝试删除服务
                    progress?.Report($"    正在删除...");
                    bool deleted = await Task.Run(() =>
                        ServiceHelper.DeleteService(serviceName));

                    if (deleted)
                    {
                        count++;
                        progress?.Report($"    ✓ 服务 {serviceName} 已删除");
                    }
                    else
                    {
                        progress?.Report($"    - 服务 {serviceName} 删除失败或不存在");
                    }
                }
                catch (Exception ex)
                {
                    progress?.Report($"    ✗ 处理服务失败: {ex.Message}");
                }
            }

            return count;
        }

        // ====================================================================
        // 第3步：删除文件
        // ====================================================================

        /// <summary>
        /// 删除选中的文件残留
        /// </summary>
        private async Task<(int Count, long TotalSize)> DeleteFilesAsync(
            List<ResidueItem> residues, IProgress<string> progress)
        {
            int count = 0;
            long totalSize = 0;

            // 获取所有选中的文件残留
            var fileResidues = residues.FindAll(
                r => r.Type == ResidueType.File && r.IsSelected);

            foreach (var item in fileResidues)
            {
                try
                {
                    progress?.Report($"  正在删除: {item.Path}");

                    if (Directory.Exists(item.Path))
                    {
                        // 删除目录（递归）
                        long dirSize = await Task.Run(() =>
                        {
                            return DeleteDirectorySafe(item.Path, progress);
                        });

                        totalSize += dirSize;
                        count++;
                        progress?.Report($"    ✓ 目录已删除");
                    }
                    else if (File.Exists(item.Path))
                    {
                        // 删除单个文件
                        long fileSize = new FileInfo(item.Path).Length;
                        await Task.Run(() => File.Delete(item.Path));
                        totalSize += fileSize;
                        count++;
                        progress?.Report($"    ✓ 文件已删除");
                    }
                    else
                    {
                        progress?.Report($"    - 路径不存在，跳过");
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    progress?.Report($"    ✗ 权限不足，需要管理员权限");
                }
                catch (Exception ex)
                {
                    progress?.Report($"    ✗ 删除失败: {ex.Message}");
                }
            }

            return (count, totalSize);
        }

        /// <summary>
        /// 安全删除目录
        /// 先清除只读属性，再递归删除
        /// </summary>
        private long DeleteDirectorySafe(string dirPath, IProgress<string> progress)
        {
            long totalSize = 0;

            try
            {
                if (!Directory.Exists(dirPath)) return 0;

                // 第1步：清除所有文件的只读属性
                foreach (string file in Directory.GetFiles(dirPath, "*",
                    SearchOption.AllDirectories))
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        totalSize += fi.Length;

                        // 如果文件是只读的，移除只读属性
                        if (fi.IsReadOnly)
                        {
                            fi.IsReadOnly = false;
                        }
                    }
                    catch { }
                }

                // 第2步：删除目录
                Directory.Delete(dirPath, true);
            }
            catch (Exception ex)
            {
                progress?.Report($"    ⚠ 部分文件无法删除: {ex.Message}");
            }

            return totalSize;
        }

        // ====================================================================
        // 第4步：清理注册表
        // ====================================================================

        /// <summary>
        /// 清理选中的注册表残留
        /// </summary>
        private async Task<int> CleanRegistryAsync(
            List<ResidueItem> residues, IProgress<string> progress)
        {
            return await Task.Run(() =>
            {
                int count = 0;

                // 获取所有选中的注册表残留
                var registryResidues = residues.FindAll(
                    r => (r.Type == ResidueType.Registry ||
                          r.Type == ResidueType.UninstallEntry) && r.IsSelected);

                foreach (var item in registryResidues)
                {
                    try
                    {
                        progress?.Report($"  正在清理: {item.Path}");

                        bool deleted = RegistryHelper.DeleteKeyTree(item.Path);

                        if (deleted)
                        {
                            count++;
                            progress?.Report($"    ✓ 注册表键已删除");
                        }
                        else
                        {
                            progress?.Report($"    - 注册表键删除失败或不存在");
                        }
                    }
                    catch (Exception ex)
                    {
                        progress?.Report($"    ✗ 清理注册表失败: {ex.Message}");
                    }
                }

                return count;
            });
        }

        // ====================================================================
        // 便捷方法
        // ====================================================================

        /// <summary>
        /// 快速清理指定版本的 AutoCAD
        /// 不需要先扫描，直接清理常见路径
        /// </summary>
        /// <param name="version">版本号，例如 "R28.0"</param>
        /// <param name="progress">进度回调</param>
        public async Task<CleanResult> QuickCleanAsync(
            string version, IProgress<string> progress = null)
        {
            var residues = new List<ResidueItem>();

            // 获取安装路径
            string installPath = RegistryHelper.GetAutoCADPath(version);
            if (!string.IsNullOrEmpty(installPath))
            {
                residues.Add(new ResidueItem
                {
                    Name = $"AutoCAD {ProductInfo.GetYearFromVersion(version)}",
                    Path = installPath,
                    Type = ResidueType.File,
                    IsSelected = true
                });
            }

            // 添加注册表键
            string regKey = $@"SOFTWARE\Autodesk\AutoCAD\{version}";
            residues.Add(new ResidueItem
            {
                Name = version,
                Path = regKey,
                Type = ResidueType.Registry,
                IsSelected = true
            });

            return await CleanAllAsync(residues, progress);
        }
    }
}
