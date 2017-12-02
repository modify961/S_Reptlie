using log4net;
using System;
using System.Diagnostics;

namespace Abot.Util
{
    /// <summary>
    /// 内存使用检测接口
    /// </summary>
    public interface IMemoryMonitor : IDisposable
    {
        /// <summary>
        /// 获取当前内存使用情况（单位：MB）
        /// </summary>
        /// <returns></returns>
        int GetCurrentUsageInMb();
    }
    /// <summary>
    /// 内存使用检测
    /// </summary>
    [Serializable]
    public class GcMemoryMonitor : IMemoryMonitor
    {
        static ILog _logger = LogManager.GetLogger("AbotLogger");
        /// <summary>
        /// 获取当前内存使用情况（单位：MB）
        /// </summary>
        /// <returns></returns>
        public virtual int GetCurrentUsageInMb()
        {
            Stopwatch timer = Stopwatch.StartNew();
            int currentUsageInMb = Convert.ToInt32(GC.GetTotalMemory(false) / (1024 * 1024));
            timer.Stop();

            _logger.DebugFormat("GC reporting [{0}mb] currently thought to be allocated, took [{1}] millisecs", currentUsageInMb, timer.ElapsedMilliseconds);

            return currentUsageInMb;       
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            //do nothing
        }
    }
}
