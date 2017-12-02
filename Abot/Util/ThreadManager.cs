using log4net;
using System;
using System.Threading;

namespace Abot.Util
{
    /// <summary>
    /// Handles the multithreading implementation details
    /// </summary>
    public interface IThreadManager : IDisposable
    {
        /// <summary>
        /// 最大线程数
        /// </summary>
        int MaxThreads { get; set; }

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread
        /// perform：执行
        /// 为Action单独开启一个线程
        /// </summary>
        /// <param name="action">The action to perform</param>
        void DoWork(Action action);

        /// <summary>
        /// Whether there are running threads
        /// 是否有在运行的线程
        /// </summary>
        bool HasRunningThreads();

        /// <summary>
        /// Abort all running threads
        /// 终止所有的线程
        /// </summary>
        void AbortAll();
    }

    [Serializable]
    public abstract class ThreadManager : IThreadManager
    {
        /// <summary>
        /// 日志
        /// </summary>
        protected static ILog _logger = LogManager.GetLogger("AbotLogger");
        /// <summary>
        /// 是否调用了AbortAll函数
        /// </summary>
        protected bool _abortAllCalled = false;
        /// <summary>
        /// 当前运行的线程数
        /// </summary>
        protected int _numberOfRunningThreads = 0;
        /// <summary>
        /// ManualResetEvent：用来控制当前线程是挂起状态还是运行状态
        /// ManualResetEvent构造函数：（true有信号／flase无信号）：构造以后对象将保持原来的状态不变，直到它的Reset()或者Set()方法被调用；
        /// Reset（）：设置为无信号状态，Set():设置为有信号状态
        /// WaitOne：在无信号状态下，可以使当前线程挂起；直到调用了Set()方法，该线程才被激活。
        /// 
        /// </summary>
        protected ManualResetEvent _resetEvent = new ManualResetEvent(true);
        /// <summary>
        /// 
        /// </summary>
        protected Object _locker = new Object();
        /// <summary>
        /// 是否调用了Dispose函数
        /// </summary>
        protected bool _isDisplosed = false;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxThreads">最大线程数：1到100之间</param>
        public ThreadManager(int maxThreads)
        {
            if ((maxThreads > 100) || (maxThreads < 1))
                throw new ArgumentException("线程数必须在1到100之间");

            MaxThreads = maxThreads;
        }

        /// <summary>
        /// 允许的最大线程数
        /// </summary>
        public int MaxThreads
        {
            get;
            set;
        }

        /// <summary>
        /// Will perform the action asynchrously on a seperate thread
        /// 为Action单独开启一个线程
        /// </summary>
        public virtual void DoWork(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            if (_abortAllCalled)
                throw new InvalidOperationException("Cannot call DoWork() after AbortAll() or Dispose() have been called.");
            //如果未执行注销函数及运行的最大线程大于1
            if (!_isDisplosed && MaxThreads > 1)
            {
                //挂起当前线程
                _resetEvent.WaitOne();
                //线程锁
                lock (_locker)
                {
                    _numberOfRunningThreads++;
                    //如果在运行的线程数大于等于最大线程数，调用Reset将线程设置为无信号状态
                    if (!_isDisplosed && _numberOfRunningThreads >= MaxThreads)
                        _resetEvent.Reset();

                    _logger.DebugFormat("Starting another thread, increasing running threads to [{0}].", _numberOfRunningThreads);
                }
                RunActionOnDedicatedThread(action);
            }
            else
            {
                RunAction(action, false);
            }
        }
        /// <summary>
        /// 停止所有线程
        /// </summary>
        public virtual void AbortAll()
        {
            _abortAllCalled = true;
            _numberOfRunningThreads = 0;
        }
        /// <summary>
        /// 注销
        /// </summary>
        public virtual void Dispose()
        {
            AbortAll();
            _resetEvent.Dispose();
            _isDisplosed = true;
        }
        /// <summary>
        /// 是否有在运行的线程
        /// </summary>
        /// <returns></returns>
        public virtual bool HasRunningThreads()
        {
            return _numberOfRunningThreads > 0;
        }
        /// <summary>
        /// 运行线程
        /// </summary>
        /// <param name="action"></param>
        /// <param name="decrementRunningThreadCountOnCompletion"></param>
        protected virtual void RunAction(Action action, bool decrementRunningThreadCountOnCompletion = true)
        {
            try
            {
                action.Invoke();
                _logger.Debug("线程启动成功.");
            }
            catch (OperationCanceledException oce)
            {
                _logger.DebugFormat("取消线程.");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("运行当前Action时发生错误.");
                _logger.Error(e);
            }
            finally
            {
                if (decrementRunningThreadCountOnCompletion)
                {
                    lock (_locker)
                    {
                        _numberOfRunningThreads--;
                        _logger.DebugFormat("[{0}] 个线程正在运行.", _numberOfRunningThreads);
                        if (!_isDisplosed && _numberOfRunningThreads < MaxThreads)
                            _resetEvent.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Runs the action on a seperate thread
        /// 为Action单独运行一个线程
        /// </summary>
        protected abstract void RunActionOnDedicatedThread(Action action);
    }
}
