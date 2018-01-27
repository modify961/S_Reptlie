using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Threading;
using System.Timers;
using Abot.Core;
using Abot.Poco;
using Abot.Util;
using log4net;
using Timer = System.Timers.Timer;

namespace Abot.Crawler
{
    /// <summary>
    /// 当类及其业务逻辑中引用某些托管和非托管资源，就需要实现IDisposable接口，实现对这些资源对象的垃圾回收。
    /// </summary>
    public interface IWebCrawler : IDisposable
    {
        /// <summary>
        /// Synchronous event that is fired before a page is crawled.
        /// </summary>
        event EventHandler<PageCrawlStartingArgs> PageCrawlStarting;

        /// <summary>
        /// Synchronous event that is fired when an individual page has been crawled.
        /// </summary>
        event EventHandler<PageCrawlCompletedArgs> PageCrawlCompleted;

        /// <summary>
        /// Synchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowed;

        /// <summary>
        /// Synchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowed;

        /// <summary>
        /// Asynchronous event that is fired before a page is crawled.
        /// </summary>
        event EventHandler<PageCrawlStartingArgs> PageCrawlStartingAsync;

        /// <summary>
        /// Asynchronous event that is fired when an individual page has been crawled.
        /// </summary>
        event EventHandler<PageCrawlCompletedArgs> PageCrawlCompletedAsync;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowedAsync;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowedAsync;

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page should be crawled or not
        /// 判断一个页面是否需要爬取
        /// </summary>
        void ShouldCrawlPage(Func<PageToCrawl, CrawlContext, CrawlDecision> decisionMaker);

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether the page's content should be dowloaded
        /// </summary>
        /// <param name="shouldDownloadPageContent"></param>
        void ShouldDownloadPageContent(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker);

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page's links should be crawled or not
        /// </summary>
        /// <param name="shouldCrawlPageLinksDelegate"></param>
        void ShouldCrawlPageLinks(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker);

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a cerain link on a page should be scheduled to be crawled
        /// </summary>
        void ShouldScheduleLink(Func<Uri, CrawledPage, CrawlContext, bool> decisionMaker);

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page should be recrawled
        /// </summary>
        void ShouldRecrawlPage(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker);

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether the 1st uri param is considered an internal uri to the second uri param
        /// </summary>
        /// <param name="decisionMaker delegate"></param>
        void IsInternalUri(Func<Uri, Uri, bool> decisionMaker);

        /// <summary>
        /// Begins a crawl using the uri param
        /// </summary>
        CrawlResult Crawl(Uri uri);

        /// <summary>
        /// Begins a crawl using the uri param, and can be cancelled using the CancellationToken
        /// </summary>
        CrawlResult Crawl(Uri uri, CancellationTokenSource tokenSource);

        /// <summary>
        /// Dynamic object that can hold any value that needs to be available in the crawl context
        /// </summary>
        dynamic CrawlBag { get; set; }
    }

    [Serializable]
    public abstract class WebCrawler : IWebCrawler
    {
        static ILog _logger = LogManager.GetLogger("AbotLogger");
        /// <summary>
        /// 
        /// </summary>
        protected bool _crawlComplete = false;
        protected bool _crawlStopReported = false;
        /// <summary>
        /// 
        /// </summary>
        protected bool _crawlCancellationReported = false;
        /// <summary>
        /// 
        /// </summary>
        protected bool _maxPagesToCrawlLimitReachedOrScheduled = false;
        protected Timer _timeoutTimer;
        protected CrawlResult _crawlResult = null;
        protected CrawlContext _crawlContext;
        protected IThreadManager _threadManager;
        /// <summary>
        /// 等待被获取爬行的URI管理器
        /// </summary>
        protected IScheduler _scheduler;
        /// <summary>
        /// 页面请求基类
        /// </summary>
        protected IPageRequester _pageRequester;
        /// <summary>
        /// 用于处理爬取业内的URI地址
        /// </summary>
        protected IHyperLinkParser _hyperLinkParser;
        /// <summary>
        ///  用于判断页面是否会被爬行，以及是否获取页面原生内容和抓取页面内的链接
        /// </summary>
        protected ICrawlDecisionMaker _crawlDecisionMaker;
        protected IMemoryManager _memoryManager;
        /// <summary>
        /// 自定义函数，用来判断是否需要爬行网页
        /// </summary>
        protected Func<PageToCrawl, CrawlContext, CrawlDecision> _shouldCrawlPageDecisionMaker;
        protected Func<CrawledPage, CrawlContext, CrawlDecision> _shouldDownloadPageContentDecisionMaker;
        /// <summary>
        /// 
        /// </summary>
        protected Func<CrawledPage, CrawlContext, CrawlDecision> _shouldCrawlPageLinksDecisionMaker;
        protected Func<CrawledPage, CrawlContext, CrawlDecision> _shouldRecrawlPageDecisionMaker;
        protected Func<Uri, CrawledPage, CrawlContext, bool> _shouldScheduleLinkDecisionMaker;
        protected Func<Uri, Uri, bool> _isInternalDecisionMaker = (uriInQuestion, rootUri) => uriInQuestion.Authority == rootUri.Authority;


        /// <summary>
        /// Dynamic object that can hold any value that needs to be available in the crawl context
        /// </summary>
        public dynamic CrawlBag { get; set; }

        #region Constructors

        static WebCrawler()
        {
            //This is a workaround for dealing with periods in urls (http://stackoverflow.com/questions/856885/httpwebrequest-to-url-with-dot-at-the-end)
            //Will not be needed when this project is upgraded to 4.5
            MethodInfo getSyntax = typeof(UriParser).GetMethod("GetSyntax", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo flagsField = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            if (getSyntax != null && flagsField != null)
            {
                foreach (string scheme in new[] { "http", "https" })
                {
                    UriParser parser = (UriParser)getSyntax.Invoke(null, new object[] { scheme });
                    if (parser != null)
                    {
                        int flagsValue = (int)flagsField.GetValue(parser);
                        // Clear the CanonicalizeAsFilePath attribute
                        if ((flagsValue & 0x1000000) != 0)
                            flagsField.SetValue(parser, flagsValue & ~0x1000000);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a crawler instance with the default settings and implementations.
        /// </summary>
        public WebCrawler()
            : this(null, null, null, null, null, null, null)
        {
        }
        /// <summary>
        /// Creates a crawler instance with the default settings and implementations.
        /// </summary>
        public WebCrawler(CrawlConfiguration crawlConfiguration)
            : this(crawlConfiguration, null, null, null, null, null, null)
        {
        }
        /// <summary>
        /// Creates a crawler instance with custom settings or implementation. Passing in null for all params is the equivalent of the empty constructor.
        /// </summary>
        /// <param name="threadManager">Distributes http requests over multiple threads</param>
        /// <param name="scheduler">Decides what link should be crawled next</param>
        /// <param name="pageRequester">Makes the raw http requests</param>
        /// <param name="hyperLinkParser">Parses a crawled page for it's hyperlinks</param>
        /// <param name="crawlDecisionMaker">Decides whether or not to crawl a page or that page's links</param>
        /// <param name="crawlConfiguration">Configurable crawl values</param>
        /// <param name="memoryManager">Checks the memory usage of the host process</param>
        public WebCrawler(
            CrawlConfiguration crawlConfiguration,
            ICrawlDecisionMaker crawlDecisionMaker,
            IThreadManager threadManager,
            IScheduler scheduler,
            IPageRequester pageRequester,
            IHyperLinkParser hyperLinkParser,
            IMemoryManager memoryManager)
        {
            _crawlContext = new CrawlContext();
            _crawlContext.CrawlConfiguration = crawlConfiguration ?? GetCrawlConfigurationFromConfigFile();
            CrawlBag = _crawlContext.CrawlBag;

            _threadManager = threadManager ?? new TaskThreadManager(_crawlContext.CrawlConfiguration.MaxConcurrentThreads > 0 ? _crawlContext.CrawlConfiguration.MaxConcurrentThreads : Environment.ProcessorCount);
            _scheduler = scheduler ?? new Scheduler(_crawlContext.CrawlConfiguration.IsUriRecrawlingEnabled, null, null);
            //判断是否需要仿真浏览器
            if (crawlConfiguration != null && crawlConfiguration.simulation)
                _pageRequester = pageRequester ?? new ImitateRequester(_crawlContext.CrawlConfiguration);
            else
                _pageRequester = pageRequester ?? new PageRequester(_crawlContext.CrawlConfiguration);
            //
            _crawlDecisionMaker = crawlDecisionMaker ?? new CrawlDecisionMaker();

            if (_crawlContext.CrawlConfiguration.MaxMemoryUsageInMb > 0
                || _crawlContext.CrawlConfiguration.MinAvailableMemoryRequiredInMb > 0)
                _memoryManager = memoryManager ?? new MemoryManager(new CachedMemoryMonitor(new GcMemoryMonitor(), _crawlContext.CrawlConfiguration.MaxMemoryUsageCacheTimeInSeconds));

            _hyperLinkParser = hyperLinkParser ?? new HapHyperLinkParser(_crawlContext.CrawlConfiguration, null);

            _crawlContext.Scheduler = _scheduler;
        }

        #endregion Constructors

        /// <summary>
        /// Begins a synchronous crawl using the uri param, subscribe to events to process data as it becomes available
        /// 根据URI开始抓取，并且在数据符合规则时调用自定义的处理函数：
        /// </summary>
        public virtual CrawlResult Crawl(Uri uri)
        {
            return Crawl(uri, null);
        }

        /// <summary>
        /// Begins a synchronous crawl using the uri param, subscribe to events to process data as it becomes available
        /// 根据URI开始抓取，并且在数据符合规则时调用自定义的处理函       
        /// 整体调用流程如下：
        /// 1：构建一个CrawlResult类，用于存储爬行抓取的结果
        /// 2：调用ShouldSchedulePageLink判断链接是否需要添加进爬行URI队列
        ///     2.1根据被爬行页面的信息判断是否需要被爬行，判断步骤如下
        ///         1：调用_crawlDecisionMaker（ICrawlDecisionMaker）的ShouldCrawlPage来判断是否爬取页面
        ///         2：调用_shouldCrawlPageDecisionMaker自定义扩展接口，判断是否爬取页面
        ///         3：如果不允许
        ///             3.1：调用FirePageCrawlDisallowedEventAsync()，运行自定义接口
        ///             3.2：调用FirePageCrawlDisallowedEvent 
        ///         4：调用SignalCrawlStopIfNeeded
        ///         5：返回
        /// 3：调用 VerifyRequiredAvailableMemory 处理内存相关
        /// </summary>
        public virtual CrawlResult Crawl(Uri uri, CancellationTokenSource cancellationTokenSource)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            //设置根节点
            _crawlContext.RootUri = _crawlContext.OriginalRootUri = uri;

            if (cancellationTokenSource != null)
                _crawlContext.CancellationTokenSource = cancellationTokenSource;
            //1、构建一个CrawlResult类，用于存储爬行抓取的结果
            _crawlResult = new CrawlResult();
            _crawlResult.RootUri = _crawlContext.RootUri;
            _crawlResult.CrawlContext = _crawlContext;
            _crawlComplete = false;

            //_logger.InfoFormat("开始爬行站点 [{0}]", uri.AbsoluteUri);
            _logger.InfoFormat("About to crawl site [{0}]", uri.AbsoluteUri);
            //打印配置项
            PrintConfigValues(_crawlContext.CrawlConfiguration);
            //内存管理
            if (_memoryManager != null)
            {
                //运行前获取内存使用情况
                _crawlContext.MemoryUsageBeforeCrawlInMb = _memoryManager.GetCurrentUsageInMb();
                _logger.InfoFormat("Starting memory usage for site [{0}] is [{1}mb]", uri.AbsoluteUri, _crawlContext.MemoryUsageBeforeCrawlInMb);
            }

            _crawlContext.CrawlStartDate = DateTime.Now;
            Stopwatch timer = Stopwatch.StartNew();

            if (_crawlContext.CrawlConfiguration.CrawlTimeoutSeconds > 0)
            {
                _timeoutTimer = new Timer(_crawlContext.CrawlConfiguration.CrawlTimeoutSeconds * 1000);
                _timeoutTimer.Elapsed += HandleCrawlTimeout;
                _timeoutTimer.Start();
            }

            try
            {
                //初始化被爬行的跟页面的数据
                PageToCrawl rootPage = new PageToCrawl(uri)
                {
                    ParentUri = uri,
                    IsInternal = true,
                    IsRoot = true
                };
                //
                if (ShouldSchedulePageLink(rootPage))
                    _scheduler.Add(rootPage);

                VerifyRequiredAvailableMemory();
                //爬行站点
                CrawlSite();
            }
            catch (Exception e)
            {
                _crawlResult.ErrorException = e;
                _logger.FatalFormat("An error occurred while crawling site [{0}]", uri);
                _logger.Fatal(e);
            }
            finally
            {
                if (_threadManager != null)
                    _threadManager.Dispose();
            }

            if (_timeoutTimer != null)
                _timeoutTimer.Stop();

            timer.Stop();

            if (_memoryManager != null)
            {
                _crawlContext.MemoryUsageAfterCrawlInMb = _memoryManager.GetCurrentUsageInMb();
                _logger.InfoFormat("Ending memory usage for site [{0}] is [{1}mb]", uri.AbsoluteUri, _crawlContext.MemoryUsageAfterCrawlInMb);
            }

            _crawlResult.Elapsed = timer.Elapsed;
            _logger.InfoFormat("Crawl complete for site [{0}]: Crawled [{1}] pages in [{2}]", _crawlResult.RootUri.AbsoluteUri, _crawlResult.CrawlContext.CrawledCount, _crawlResult.Elapsed);

            return _crawlResult;
        }

        #region Synchronous Events

        /// <summary>
        /// Synchronous event that is fired before a page is crawled.
        /// </summary>
        public event EventHandler<PageCrawlStartingArgs> PageCrawlStarting;

        /// <summary>
        /// Synchronous event that is fired when an individual page has been crawled.
        /// </summary>
        public event EventHandler<PageCrawlCompletedArgs> PageCrawlCompleted;

        /// <summary>
        /// Synchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// </summary>
        public event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowed;

        /// <summary>
        /// Synchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// </summary>
        public event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowed;
        /// <summary>
        /// 页面开始爬行的前时间
        /// </summary>
        /// <param name="pageToCrawl"></param>
        protected virtual void FirePageCrawlStartingEvent(PageToCrawl pageToCrawl)
        {
            try
            {
                EventHandler<PageCrawlStartingArgs> threadSafeEvent = PageCrawlStarting;
                if (threadSafeEvent != null)
                    threadSafeEvent(this, new PageCrawlStartingArgs(_crawlContext, pageToCrawl));
            }
            catch (Exception e)
            {
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlStarting event for url:" + pageToCrawl.Uri.AbsoluteUri);
                _logger.Error(e);
            }
        }

        protected virtual void FirePageCrawlCompletedEvent(CrawledPage crawledPage)
        {
            try
            {
                EventHandler<PageCrawlCompletedArgs> threadSafeEvent = PageCrawlCompleted;
                if (threadSafeEvent != null)
                    threadSafeEvent(this, new PageCrawlCompletedArgs(_crawlContext, crawledPage));
            }
            catch (Exception e)
            {
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlCompleted event for url:" + crawledPage.Uri.AbsoluteUri);
                _logger.Error(e);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="reason"></param>
        protected virtual void FirePageCrawlDisallowedEvent(PageToCrawl pageToCrawl, string reason)
        {
            try
            {
                EventHandler<PageCrawlDisallowedArgs> threadSafeEvent = PageCrawlDisallowed;
                if (threadSafeEvent != null)
                    threadSafeEvent(this, new PageCrawlDisallowedArgs(_crawlContext, pageToCrawl, reason));
            }
            catch (Exception e)
            {
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlDisallowed event for url:" + pageToCrawl.Uri.AbsoluteUri);
                _logger.Error(e);
            }
        }

        protected virtual void FirePageLinksCrawlDisallowedEvent(CrawledPage crawledPage, string reason)
        {
            try
            {
                EventHandler<PageLinksCrawlDisallowedArgs> threadSafeEvent = PageLinksCrawlDisallowed;
                if (threadSafeEvent != null)
                    threadSafeEvent(this, new PageLinksCrawlDisallowedArgs(_crawlContext, crawledPage, reason));
            }
            catch (Exception e)
            {
                _logger.Error("An unhandled exception was thrown by a subscriber of the PageLinksCrawlDisallowed event for url:" + crawledPage.Uri.AbsoluteUri);
                _logger.Error(e);
            }
        }

        #endregion

        #region Asynchronous Events

        /// <summary>
        /// Asynchronous event that is fired before a page is crawled.
        /// 自定义事件：在页面被爬行之前执行
        /// </summary>
        public event EventHandler<PageCrawlStartingArgs> PageCrawlStartingAsync;

        /// <summary>
        /// Asynchronous event that is fired when an individual page has been crawled.
        /// 自定义事件：页面被爬行之后
        /// </summary>
        public event EventHandler<PageCrawlCompletedArgs> PageCrawlCompletedAsync;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawl impl returned false. This means the page or its links were not crawled.
        /// 当 ICrawlDecisionMaker.ShouldCrawl 属性为false时被执行
        /// 自定义ICrawlDecisionMaker决定不再爬取的方法事件
        /// </summary>
        public event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowedAsync;

        /// <summary>
        /// Asynchronous event that is fired when the ICrawlDecisionMaker.ShouldCrawlLinks impl returned false. This means the page's links were not crawled.
        /// 当 ICrawlDecisionMaker.ShouldCrawl 属性为false时被执行
        /// </summary>
        public event EventHandler<PageLinksCrawlDisallowedArgs> PageLinksCrawlDisallowedAsync;
        /// <summary>
        /// 调用页面开始爬取前的自定义接口
        /// </summary>
        /// <param name="pageToCrawl"></param>
        protected virtual void FirePageCrawlStartingEventAsync(PageToCrawl pageToCrawl)
        {
            EventHandler<PageCrawlStartingArgs> threadSafeEvent = PageCrawlStartingAsync;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlStartingArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, new PageCrawlStartingArgs(_crawlContext, pageToCrawl), null, null);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawledPage"></param>
        protected virtual void FirePageCrawlCompletedEventAsync(CrawledPage crawledPage)
        {
            EventHandler<PageCrawlCompletedArgs> threadSafeEvent = PageCrawlCompletedAsync;

            if (threadSafeEvent == null)
                return;

            if (_scheduler.Count == 0)
            {
                //Must be fired synchronously to avoid main thread exiting before completion of event handler for first or last page crawled
                try
                {
                    threadSafeEvent(this, new PageCrawlCompletedArgs(_crawlContext, crawledPage));
                }
                catch (Exception e)
                {
                    _logger.Error("An unhandled exception was thrown by a subscriber of the PageCrawlCompleted event for url:" + crawledPage.Uri.AbsoluteUri);
                    _logger.Error(e);
                }
            }
            else
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlCompletedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, new PageCrawlCompletedArgs(_crawlContext, crawledPage), null, null);
                }
            }
        }
        /// <summary>
        /// ？？
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="reason"></param>
        protected virtual void FirePageCrawlDisallowedEventAsync(PageToCrawl pageToCrawl, string reason)
        {
            EventHandler<PageCrawlDisallowedArgs> threadSafeEvent = PageCrawlDisallowedAsync;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageCrawlDisallowedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, new PageCrawlDisallowedArgs(_crawlContext, pageToCrawl, reason), null, null);
                }
            }
        }

        protected virtual void FirePageLinksCrawlDisallowedEventAsync(CrawledPage crawledPage, string reason)
        {
            EventHandler<PageLinksCrawlDisallowedArgs> threadSafeEvent = PageLinksCrawlDisallowedAsync;
            if (threadSafeEvent != null)
            {
                //Fire each subscribers delegate async
                foreach (EventHandler<PageLinksCrawlDisallowedArgs> del in threadSafeEvent.GetInvocationList())
                {
                    del.BeginInvoke(this, new PageLinksCrawlDisallowedArgs(_crawlContext, crawledPage, reason), null, null);
                }
            }
        }

        #endregion


        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page should be crawled or not
        /// 自定义处理函数，用来决定一个页面是否被爬行
        /// Func：委托
        /// 入参为：PageToCrawl，CrawlContext
        /// 出参：CrawlDecision
        /// </summary>
        public void ShouldCrawlPage(Func<PageToCrawl, CrawlContext, CrawlDecision> decisionMaker)
        {
            _shouldCrawlPageDecisionMaker = decisionMaker;
        }

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether the page's content should be dowloaded
        /// 自定义处理函数，用来决定一个页面是否被下载爬行
        /// </summary>
        /// <param name="decisionMaker"></param>
        public void ShouldDownloadPageContent(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker)
        {
            _shouldDownloadPageContentDecisionMaker = decisionMaker;
        }

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page's links should be crawled or not
        /// 自定义处理函数，用来是否要将页面内的link爬取出来
        /// </summary>
        /// <param name="shouldCrawlPageLinksDelegate"></param>
        public void ShouldCrawlPageLinks(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker)
        {
            _shouldCrawlPageLinksDecisionMaker = decisionMaker;
        }

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a cerain link on a page should be scheduled to be crawled
        /// </summary>
        public void ShouldScheduleLink(Func<Uri, CrawledPage, CrawlContext, bool> decisionMaker)
        {
            _shouldScheduleLinkDecisionMaker = decisionMaker;
        }

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether a page should be recrawled or not
        /// </summary>
        public void ShouldRecrawlPage(Func<CrawledPage, CrawlContext, CrawlDecision> decisionMaker)
        {
            _shouldRecrawlPageDecisionMaker = decisionMaker;
        }

        /// <summary>
        /// Synchronous method that registers a delegate to be called to determine whether the 1st uri param is considered an internal uri to the second uri param
        /// </summary>
        /// <param name="decisionMaker delegate"></param>     
        public void IsInternalUri(Func<Uri, Uri, bool> decisionMaker)
        {
            _isInternalDecisionMaker = decisionMaker;
        }

        private CrawlConfiguration GetCrawlConfigurationFromConfigFile()
        {
            AbotConfigurationSectionHandler configFromFile = AbotConfigurationSectionHandler.LoadFromXml();

            if (configFromFile == null)
                throw new InvalidOperationException("abot config section was NOT found");

            _logger.DebugFormat("abot config section was found");
            return configFromFile.Convert();
        }
        /// <summary>
        /// 爬取站点，操作步骤如下：
        /// 1：
        /// </summary>
        protected virtual void CrawlSite()
        {
            //是否爬取完毕
            while (!_crawlComplete)
            {
                //爬行前检查
                RunPreWorkChecks();

                if (_scheduler.Count > 0)
                {
                    _threadManager.DoWork(() => ProcessPage(_scheduler.GetNext()));
                }
                //如果没有在运行的线程，且待爬取的页面为0
                else if (!_threadManager.HasRunningThreads())
                {
                    _crawlComplete = true;
                }
                else
                {
                    //如果待爬取的链接为零但还有在运行的线程，则等待2.5秒后继续循环
                    _logger.DebugFormat("Waiting for links to be scheduled...");
                }
                Random rd = new Random();
                int num=rd.Next(5, 10);
                //临时处理每隔10秒开始一个线程
                Thread.Sleep(num*1000);
            }
        }
        /// <summary>
        /// 验证占用内存是否超出规定大小
        /// </summary>
        protected virtual void VerifyRequiredAvailableMemory()
        {
            if (_crawlContext.CrawlConfiguration.MinAvailableMemoryRequiredInMb < 1)
                return;

            if (!_memoryManager.IsSpaceAvailable(_crawlContext.CrawlConfiguration.MinAvailableMemoryRequiredInMb))
                throw new InsufficientMemoryException(string.Format("Process does not have the configured [{0}mb] of available memory to crawl site [{1}]. This is configurable through the minAvailableMemoryRequiredInMb in app.conf or CrawlConfiguration.MinAvailableMemoryRequiredInMb.", _crawlContext.CrawlConfiguration.MinAvailableMemoryRequiredInMb, _crawlContext.RootUri));
        }
        /// <summary>
        /// 爬取前检查 通过1、2 步判断是否需要中断爬取，3、4步执行
        /// 1：CheckMemoryUsage  
        /// 2：CheckForCancellationRequest
        /// 3：CheckForHardStopRequest  强制中断爬取 
        /// 4：CheckForStopRequest  中断爬取
        /// </summary>
        protected virtual void RunPreWorkChecks()
        {
            CheckMemoryUsage();
            CheckForCancellationRequest();
            CheckForHardStopRequest();
            CheckForStopRequest();
        }
        /// <summary>
        /// 检查内存使用情况
        /// </summary>
        protected virtual void CheckMemoryUsage()
        {
            if (_memoryManager == null
                || _crawlContext.IsCrawlHardStopRequested
                || _crawlContext.CrawlConfiguration.MaxMemoryUsageInMb < 1)
                return;

            int currentMemoryUsage = _memoryManager.GetCurrentUsageInMb();
            if (_logger.IsDebugEnabled)
                _logger.DebugFormat("Current memory usage for site [{0}] is [{1}mb]", _crawlContext.RootUri, currentMemoryUsage);

            if (currentMemoryUsage > _crawlContext.CrawlConfiguration.MaxMemoryUsageInMb)
            {
                _memoryManager.Dispose();
                _memoryManager = null;

                string message = string.Format("Process is using [{0}mb] of memory which is above the max configured of [{1}mb] for site [{2}]. This is configurable through the maxMemoryUsageInMb in app.conf or CrawlConfiguration.MaxMemoryUsageInMb.", currentMemoryUsage, _crawlContext.CrawlConfiguration.MaxMemoryUsageInMb, _crawlContext.RootUri);
                _crawlResult.ErrorException = new InsufficientMemoryException(message);

                _logger.Fatal(_crawlResult.ErrorException);
                _crawlContext.IsCrawlHardStopRequested = true;
            }
        }
        /// <summary>
        /// CancellationTokenSource：
        /// </summary>
        protected virtual void CheckForCancellationRequest()
        {
            if (_crawlContext.CancellationTokenSource.IsCancellationRequested)
            {
                if (!_crawlCancellationReported)
                {
                    string message = string.Format("Crawl cancellation requested for site [{0}]!", _crawlContext.RootUri);
                    _logger.Fatal(message);
                    _crawlResult.ErrorException = new OperationCanceledException(message, _crawlContext.CancellationTokenSource.Token);
                    _crawlContext.IsCrawlHardStopRequested = true;
                    _crawlCancellationReported = true;
                }
            }
        }
        /// <summary>
        /// 强制停止
        /// </summary>
        protected virtual void CheckForHardStopRequest()
        {
            if (_crawlContext.IsCrawlHardStopRequested)
            {
                if (!_crawlStopReported)
                {
                    _logger.InfoFormat("Hard crawl stop requested for site [{0}]!", _crawlContext.RootUri);
                    _crawlStopReported = true;
                }

                _scheduler.Clear();
                _threadManager.AbortAll();
                _scheduler.Clear();//to be sure nothing was scheduled since first call to clear()

                //Set all events to null so no more events are fired
                PageCrawlStarting = null;
                PageCrawlCompleted = null;
                PageCrawlDisallowed = null;
                PageLinksCrawlDisallowed = null;
                PageCrawlStartingAsync = null;
                PageCrawlCompletedAsync = null;
                PageCrawlDisallowedAsync = null;
                PageLinksCrawlDisallowedAsync = null;
            }
        }
        /// <summary>
        /// 停止
        /// </summary>
        protected virtual void CheckForStopRequest()
        {
            if (_crawlContext.IsCrawlStopRequested)
            {
                if (!_crawlStopReported)
                {
                    _logger.InfoFormat("Crawl stop requested for site [{0}]!", _crawlContext.RootUri);
                    _crawlStopReported = true;
                }
                _scheduler.Clear();
            }
        }

        protected virtual void HandleCrawlTimeout(object sender, ElapsedEventArgs e)
        {
            Timer elapsedTimer = sender as Timer;
            if (elapsedTimer != null)
                elapsedTimer.Stop();

            _logger.InfoFormat("Crawl timeout of [{0}] seconds has been reached for [{1}]", _crawlContext.CrawlConfiguration.CrawlTimeoutSeconds, _crawlContext.RootUri);
            _crawlContext.IsCrawlHardStopRequested = true;
        }

        //protected virtual async Task ProcessPage(PageToCrawl pageToCrawl)
        /// <summary>
        /// 爬取处理
        /// 1：ThrowIfCancellationRequested 检查是否需要取消Task并抛出异常
        /// </summary>
        /// <param name="pageToCrawl"></param>
        protected virtual void ProcessPage(PageToCrawl pageToCrawl)
        {
            try
            {
                if (pageToCrawl == null)
                    return;
                //检查是否需要取消Task并抛出异常
                ThrowIfCancellationRequested();
                //暂时不知道干嘛的
                AddPageToContext(pageToCrawl);

                //CrawledPage crawledPage = await CrawlThePage(pageToCrawl);
                //爬取页面
                CrawledPage crawledPage = CrawlThePage(pageToCrawl);

                // Validate the root uri in case of a redirection.
                //判断是否重定向
                if (crawledPage.IsRoot)
                    ValidateRootUriForRedirection(crawledPage);
                //如果是重定向，则处理
                if (IsRedirect(crawledPage) && !_crawlContext.CrawlConfiguration.IsHttpRequestAutoRedirectsEnabled)
                    ProcessRedirect(crawledPage);
                //判断页面大小是否超出限制
                if (PageSizeIsAboveMax(crawledPage))
                    return;
                //
                ThrowIfCancellationRequested();

                bool shouldCrawlPageLinks = ShouldCrawlPageLinks(crawledPage);
                //获取页面内的URI地址
                if (shouldCrawlPageLinks || _crawlContext.CrawlConfiguration.IsForcedLinkParsingEnabled)
                    ParsePageLinks(crawledPage);

                ThrowIfCancellationRequested();
                //将页面内获取的地址存放至待爬取队列
                if (shouldCrawlPageLinks)
                    SchedulePageLinks(crawledPage);

                ThrowIfCancellationRequested();
                //调用自定义爬取完成接口
                FirePageCrawlCompletedEventAsync(crawledPage);
                FirePageCrawlCompletedEvent(crawledPage);

                if (ShouldRecrawlPage(crawledPage))
                {
                    crawledPage.IsRetry = true;
                    _scheduler.Add(crawledPage);
                }
            }
            catch (OperationCanceledException oce)
            {
                _logger.DebugFormat("Thread cancelled while crawling/processing page [{0}]", pageToCrawl.Uri);
                throw;
            }
            catch (Exception e)
            {
                _crawlResult.ErrorException = e;
                _logger.FatalFormat("Error occurred during processing of page [{0}]", pageToCrawl.Uri);
                _logger.Fatal(e);

                _crawlContext.IsCrawlHardStopRequested = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawledPage"></param>
        protected virtual void ProcessRedirect(CrawledPage crawledPage)
        {
            if (crawledPage.RedirectPosition >= 20)
                _logger.WarnFormat("Page [{0}] is part of a chain of 20 or more consecutive redirects, redirects for this chain will now be aborted.", crawledPage.Uri);

            try
            {
                var uri = ExtractRedirectUri(crawledPage);

                PageToCrawl page = new PageToCrawl(uri);
                page.ParentUri = crawledPage.ParentUri;
                page.CrawlDepth = crawledPage.CrawlDepth;
                page.IsInternal = IsInternalUri(uri);
                page.IsRoot = false;
                page.RedirectedFrom = crawledPage;
                page.RedirectPosition = crawledPage.RedirectPosition + 1;

                crawledPage.RedirectedTo = page;
                _logger.DebugFormat("Page [{0}] is requesting that it be redirect to [{1}]", crawledPage.Uri, crawledPage.RedirectedTo.Uri);

                if (ShouldSchedulePageLink(page))
                {
                    _logger.InfoFormat("Page [{0}] will be redirect to [{1}]", crawledPage.Uri, crawledPage.RedirectedTo.Uri);
                    _scheduler.Add(page);
                }
            }
            catch { }
        }

        protected virtual bool IsInternalUri(Uri uri)
        {
            return _isInternalDecisionMaker(uri, _crawlContext.RootUri) ||
                _isInternalDecisionMaker(uri, _crawlContext.OriginalRootUri);
        }

        protected virtual bool IsRedirect(CrawledPage crawledPage)
        {
            bool isRedirect = false;
            if (crawledPage.HttpWebResponse != null)
            {
                isRedirect = (_crawlContext.CrawlConfiguration.IsHttpRequestAutoRedirectsEnabled &&
                    crawledPage.HttpWebResponse.ResponseUri != null &&
                    crawledPage.HttpWebResponse.ResponseUri.AbsoluteUri != crawledPage.Uri.AbsoluteUri) ||
                    (!_crawlContext.CrawlConfiguration.IsHttpRequestAutoRedirectsEnabled &&
                    (int)crawledPage.HttpWebResponse.StatusCode >= 300 &&
                    (int)crawledPage.HttpWebResponse.StatusCode <= 399);
            }
            return isRedirect;
        }
        /// <summary>
        /// 检查是否需要取消Task并抛出异常
        /// </summary>
        protected virtual void ThrowIfCancellationRequested()
        {
            if (_crawlContext.CancellationTokenSource != null && _crawlContext.CancellationTokenSource.IsCancellationRequested)
                _crawlContext.CancellationTokenSource.Token.ThrowIfCancellationRequested();
        }
        /// <summary>
        /// 页面大小是否超出大小
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <returns></returns>
        protected virtual bool PageSizeIsAboveMax(CrawledPage crawledPage)
        {
            bool isAboveMax = false;
            if (_crawlContext.CrawlConfiguration.MaxPageSizeInBytes > 0 &&
                crawledPage.Content.Bytes != null &&
                crawledPage.Content.Bytes.Length > _crawlContext.CrawlConfiguration.MaxPageSizeInBytes)
            {
                isAboveMax = true;
                _logger.InfoFormat("Page [{0}] has a page size of [{1}] bytes which is above the [{2}] byte max, no further processing will occur for this page", crawledPage.Uri, crawledPage.Content.Bytes.Length, _crawlContext.CrawlConfiguration.MaxPageSizeInBytes);
            }
            return isAboveMax;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <returns></returns>
        protected virtual bool ShouldCrawlPageLinks(CrawledPage crawledPage)
        {
            CrawlDecision shouldCrawlPageLinksDecision = _crawlDecisionMaker.ShouldCrawlPageLinks(crawledPage, _crawlContext);
            if (shouldCrawlPageLinksDecision.Allow)
                shouldCrawlPageLinksDecision = (_shouldCrawlPageLinksDecisionMaker != null) ? _shouldCrawlPageLinksDecisionMaker.Invoke(crawledPage, _crawlContext) : new CrawlDecision { Allow = true };

            if (!shouldCrawlPageLinksDecision.Allow)
            {
                _logger.DebugFormat("Links on page [{0}] not crawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldCrawlPageLinksDecision.Reason);
                FirePageLinksCrawlDisallowedEventAsync(crawledPage, shouldCrawlPageLinksDecision.Reason);
                FirePageLinksCrawlDisallowedEvent(crawledPage, shouldCrawlPageLinksDecision.Reason);
            }

            SignalCrawlStopIfNeeded(shouldCrawlPageLinksDecision);
            return shouldCrawlPageLinksDecision.Allow;
        }
        /// <summary>
        /// 根据被爬行页面的信息判断是否需要被爬行，判断步骤如下
        /// 1：调用_crawlDecisionMaker（ICrawlDecisionMaker）的ShouldCrawlPage来判断是否爬取页面
        /// 2：调用_shouldCrawlPageDecisionMaker自定义扩展接口，判断是否爬取页面
        /// 3：如果不允许
        ///     3.1：调用FirePageCrawlDisallowedEventAsync()，运行自定义接口
        ///     3.2：调用FirePageCrawlDisallowedEvent 
        /// 4：调用SignalCrawlStopIfNeeded
        /// 5：返回
        /// </summary>
        /// <param name="pageToCrawl">被爬行页面的信息</param>
        /// <returns></returns>
        protected virtual bool ShouldCrawlPage(PageToCrawl pageToCrawl)
        {

            if (_maxPagesToCrawlLimitReachedOrScheduled)
                return false;
            CrawlDecision shouldCrawlPageDecision = _crawlDecisionMaker.ShouldCrawlPage(pageToCrawl, _crawlContext);
            if (!shouldCrawlPageDecision.Allow &&
                shouldCrawlPageDecision.Reason.Contains("MaxPagesToCrawl limit of"))
            {
                _maxPagesToCrawlLimitReachedOrScheduled = true;
                _logger.Info("MaxPagesToCrawlLimit has been reached or scheduled. No more pages will be scheduled.");
                return false;
            }

            if (shouldCrawlPageDecision.Allow)
                shouldCrawlPageDecision = (_shouldCrawlPageDecisionMaker != null) ? _shouldCrawlPageDecisionMaker.Invoke(pageToCrawl, _crawlContext) : new CrawlDecision { Allow = true };

            if (!shouldCrawlPageDecision.Allow)
            {
                _logger.DebugFormat("Page [{0}] not crawled, [{1}]", pageToCrawl.Uri.AbsoluteUri, shouldCrawlPageDecision.Reason);
                FirePageCrawlDisallowedEventAsync(pageToCrawl, shouldCrawlPageDecision.Reason);
                FirePageCrawlDisallowedEvent(pageToCrawl, shouldCrawlPageDecision.Reason);
            }

            SignalCrawlStopIfNeeded(shouldCrawlPageDecision);
            return shouldCrawlPageDecision.Allow;
        }

        protected virtual bool ShouldRecrawlPage(CrawledPage crawledPage)
        {
            //TODO No unit tests cover these lines
            CrawlDecision shouldRecrawlPageDecision = _crawlDecisionMaker.ShouldRecrawlPage(crawledPage, _crawlContext);
            if (shouldRecrawlPageDecision.Allow)
                shouldRecrawlPageDecision = (_shouldRecrawlPageDecisionMaker != null) ? _shouldRecrawlPageDecisionMaker.Invoke(crawledPage, _crawlContext) : new CrawlDecision { Allow = true };

            if (!shouldRecrawlPageDecision.Allow)
            {
                _logger.DebugFormat("Page [{0}] not recrawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldRecrawlPageDecision.Reason);
            }
            else
            {
                // Look for the Retry-After header in the response.
                crawledPage.RetryAfter = null;
                if (crawledPage.HttpWebResponse != null &&
                    crawledPage.HttpWebResponse.Headers != null)
                {
                    string value = crawledPage.HttpWebResponse.GetResponseHeader("Retry-After");
                    if (!String.IsNullOrEmpty(value))
                    {
                        // Try to convert to DateTime first, then in double.
                        DateTime date;
                        double seconds;
                        if (crawledPage.LastRequest.HasValue && DateTime.TryParse(value, out date))
                        {
                            crawledPage.RetryAfter = (date - crawledPage.LastRequest.Value).TotalSeconds;
                        }
                        else if (double.TryParse(value, out seconds))
                        {
                            crawledPage.RetryAfter = seconds;
                        }
                    }
                }
            }

            SignalCrawlStopIfNeeded(shouldRecrawlPageDecision);
            return shouldRecrawlPageDecision.Allow;
        }

        //protected virtual async Task<CrawledPage> CrawlThePage(PageToCrawl pageToCrawl)
        /// <summary>
        /// 爬取页面
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <returns></returns>
        protected virtual CrawledPage CrawlThePage(PageToCrawl pageToCrawl)
        {
            _logger.DebugFormat("About to crawl page [{0}]", pageToCrawl.Uri.AbsoluteUri);
            //调取页面开始爬行前的自定义响应时间
            FirePageCrawlStartingEventAsync(pageToCrawl);
            //取页面开始爬行前事件
            FirePageCrawlStartingEvent(pageToCrawl);
            //站点是否被多次爬取
            if (pageToCrawl.IsRetry)
            {
                //线程休眠。
                WaitMinimumRetryDelay(pageToCrawl);
            }

            pageToCrawl.LastRequest = DateTime.Now;
            //发送http请求获取页面
            //ShouldDownloadPageContent  自定义接口，用于判断页面是否需要被下载
            CrawledPage crawledPage = _pageRequester.MakeRequest(pageToCrawl.Uri, ShouldDownloadPageContent);
            //CrawledPage crawledPage = await _pageRequester.MakeRequestAsync(pageToCrawl.Uri, ShouldDownloadPageContent);

            Map(pageToCrawl, crawledPage);

            if (crawledPage.HttpWebResponse == null)
                _logger.InfoFormat("Page crawl complete, Status:[NA] Url:[{0}] Elapsed:[{1}] Parent:[{2}] Retry:[{3}]", crawledPage.Uri.AbsoluteUri, crawledPage.Elapsed, crawledPage.ParentUri, crawledPage.RetryCount);
            else
                _logger.InfoFormat("Page crawl complete, Status:[{0}] Url:[{1}] Elapsed:[{2}] Parent:[{3}] Retry:[{4}]", Convert.ToInt32(crawledPage.HttpWebResponse.StatusCode), crawledPage.Uri.AbsoluteUri, crawledPage.Elapsed, crawledPage.ParentUri, crawledPage.RetryCount);

            return crawledPage;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        protected void Map(PageToCrawl src, CrawledPage dest)
        {
            dest.Uri = src.Uri;
            dest.ParentUri = src.ParentUri;
            dest.IsRetry = src.IsRetry;
            dest.RetryAfter = src.RetryAfter;
            dest.RetryCount = src.RetryCount;
            dest.LastRequest = src.LastRequest;
            dest.IsRoot = src.IsRoot;
            dest.IsInternal = src.IsInternal;
            dest.PageBag = CombinePageBags(src.PageBag, dest.PageBag);
            dest.CrawlDepth = src.CrawlDepth;
            dest.RedirectedFrom = src.RedirectedFrom;
            dest.RedirectPosition = src.RedirectPosition;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageToCrawlBag"></param>
        /// <param name="crawledPageBag"></param>
        /// <returns></returns>
        protected virtual dynamic CombinePageBags(dynamic pageToCrawlBag, dynamic crawledPageBag)
        {
            IDictionary<string, object> combinedBag = new ExpandoObject();
            var pageToCrawlBagDict = pageToCrawlBag as IDictionary<string, object>;
            var crawledPageBagDict = crawledPageBag as IDictionary<string, object>;

            foreach (KeyValuePair<string, object> entry in pageToCrawlBagDict)
                combinedBag[entry.Key] = entry.Value;
            foreach (KeyValuePair<string, object> entry in crawledPageBagDict)
                combinedBag[entry.Key] = entry.Value;

            return combinedBag;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageToCrawl"></param>
        protected virtual void AddPageToContext(PageToCrawl pageToCrawl)
        {
            if (pageToCrawl.IsRetry)
            {
                pageToCrawl.RetryCount++;
                return;
            }

            int domainCount = 0;
            Interlocked.Increment(ref _crawlContext.CrawledCount);
            _crawlContext.CrawlCountByDomain.AddOrUpdate(pageToCrawl.Uri.Authority, 1, (key, oldValue) => oldValue + 1);
        }
        /// <summary>
        /// 爬取页面内的URI地址
        /// </summary>
        /// <param name="crawledPage"></param>
        protected virtual void ParsePageLinks(CrawledPage crawledPage)
        {
            crawledPage.ParsedLinks = _hyperLinkParser.GetLinks(crawledPage);
        }
        /// <summary>
        /// 将从页面内爬出来的URI加入到待爬取的队列_scheduler
        /// </summary>
        /// <param name="crawledPage"></param>
        protected virtual void SchedulePageLinks(CrawledPage crawledPage)
        {
            int linksToCrawl = 0;
            foreach (Uri uri in crawledPage.ParsedLinks)
            {
                // First validate that the link was not already visited or added to the list of pages to visit, so we don't
                // make the same validation and fire the same events twice.
                if (!_scheduler.IsUriKnown(uri) &&
                    (_shouldScheduleLinkDecisionMaker == null || _shouldScheduleLinkDecisionMaker.Invoke(uri, crawledPage, _crawlContext)))
                {
                    try //Added due to a bug in the Uri class related to this (http://stackoverflow.com/questions/2814951/system-uriformatexception-invalid-uri-the-hostname-could-not-be-parsed)
                    {
                        PageToCrawl page = new PageToCrawl(uri);
                        page.ParentUri = crawledPage.Uri;
                        page.CrawlDepth = crawledPage.CrawlDepth + 1;
                        page.IsInternal = IsInternalUri(uri);
                        page.IsRoot = false;

                        if (ShouldSchedulePageLink(page))
                        {
                            _scheduler.Add(page);
                            linksToCrawl++;
                        }

                        if (!ShouldScheduleMorePageLink(linksToCrawl))
                        {
                            _logger.InfoFormat("MaxLinksPerPage has been reached. No more links will be scheduled for current page [{0}].", crawledPage.Uri);
                            break;
                        }
                    }
                    catch { }
                }

                // Add this link to the list of known Urls so validations are not duplicated in the future.
                _scheduler.AddKnownUri(uri);
            }
        }
        /// <summary>
        /// 判断URI是否可以被添加到等待获取的链接：判断条件
        /// (page.IsInternal || _crawlContext.CrawlConfiguration.IsExternalPageCrawlingEnabled):是否为站内页面或者是否允许爬行站外节点
        /// 调用 ShouldCrawlPage  函数
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        protected virtual bool ShouldSchedulePageLink(PageToCrawl page)
        {
            if ((page.IsInternal || _crawlContext.CrawlConfiguration.IsExternalPageCrawlingEnabled) && (ShouldCrawlPage(page)))
                return true;

            return false;
        }

        protected virtual bool ShouldScheduleMorePageLink(int linksAdded)
        {
            return _crawlContext.CrawlConfiguration.MaxLinksPerPage == 0 || _crawlContext.CrawlConfiguration.MaxLinksPerPage > linksAdded;
        }
        /// <summary>
        /// 是否需要下载页面内容
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <returns></returns>
        protected virtual CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage)
        {
            CrawlDecision decision = _crawlDecisionMaker.ShouldDownloadPageContent(crawledPage, _crawlContext);
            if (decision.Allow)
                decision = (_shouldDownloadPageContentDecisionMaker != null) ? _shouldDownloadPageContentDecisionMaker.Invoke(crawledPage, _crawlContext) : new CrawlDecision { Allow = true };

            SignalCrawlStopIfNeeded(decision);
            return decision;
        }

        protected virtual void PrintConfigValues(CrawlConfiguration config)
        {
            _logger.Info("Configuration Values:");

            string indentString = new string(' ', 2);
            string abotVersion = Assembly.GetAssembly(this.GetType()).GetName().Version.ToString();
            _logger.InfoFormat("{0}Abot Version: {1}", indentString, abotVersion);
            foreach (PropertyInfo property in config.GetType().GetProperties())
            {
                if (property.Name != "ConfigurationExtensions")
                    _logger.InfoFormat("{0}{1}: {2}", indentString, property.Name, property.GetValue(config, null));
            }

            foreach (string key in config.ConfigurationExtensions.Keys)
            {
                _logger.InfoFormat("{0}{1}: {2}", indentString, key, config.ConfigurationExtensions[key]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="decision"></param>
        protected virtual void SignalCrawlStopIfNeeded(CrawlDecision decision)
        {
            if (decision.ShouldHardStopCrawl)
            {
                _logger.InfoFormat("Decision marked crawl [Hard Stop] for site [{0}], [{1}]", _crawlContext.RootUri, decision.Reason);
                _crawlContext.IsCrawlHardStopRequested = decision.ShouldHardStopCrawl;
            }
            else if (decision.ShouldStopCrawl)
            {
                _logger.InfoFormat("Decision marked crawl [Stop] for site [{0}], [{1}]", _crawlContext.RootUri, decision.Reason);
                _crawlContext.IsCrawlStopRequested = decision.ShouldStopCrawl;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageToCrawl"></param>
        protected virtual void WaitMinimumRetryDelay(PageToCrawl pageToCrawl)
        {
            //TODO No unit tests cover these lines
            if (pageToCrawl.LastRequest == null)
            {
                _logger.WarnFormat("pageToCrawl.LastRequest value is null for Url:{0}. Cannot retry without this value.", pageToCrawl.Uri.AbsoluteUri);
                return;
            }

            double milliSinceLastRequest = (DateTime.Now - pageToCrawl.LastRequest.Value).TotalMilliseconds;
            double milliToWait;
            if (pageToCrawl.RetryAfter.HasValue)
            {
                // Use the time to wait provided by the server instead of the config, if any.
                milliToWait = pageToCrawl.RetryAfter.Value * 1000 - milliSinceLastRequest;
            }
            else
            {
                if (!(milliSinceLastRequest < _crawlContext.CrawlConfiguration.MinRetryDelayInMilliseconds)) return;
                milliToWait = _crawlContext.CrawlConfiguration.MinRetryDelayInMilliseconds - milliSinceLastRequest;
            }

            _logger.InfoFormat("Waiting [{0}] milliseconds before retrying Url:[{1}] LastRequest:[{2}] SoonestNextRequest:[{3}]",
                milliToWait,
                pageToCrawl.Uri.AbsoluteUri,
                pageToCrawl.LastRequest,
                pageToCrawl.LastRequest.Value.AddMilliseconds(_crawlContext.CrawlConfiguration.MinRetryDelayInMilliseconds));

            //TODO Cannot use RateLimiter since it currently cannot handle dynamic sleep times so using Thread.Sleep in the meantime
            if (milliToWait > 0)
                Thread.Sleep(TimeSpan.FromMilliseconds(milliToWait));
        }

        /// <summary>
        /// Validate that the Root page was not redirected. If the root page is redirected, we assume that the root uri
        /// should be changed to the uri where it was redirected.
        /// </summary>
        protected virtual void ValidateRootUriForRedirection(CrawledPage crawledRootPage)
        {
            if (!crawledRootPage.IsRoot)
            {
                throw new ArgumentException("The crawled page must be the root page to be validated for redirection.");
            }

            if (IsRedirect(crawledRootPage))
            {
                _crawlContext.RootUri = ExtractRedirectUri(crawledRootPage);
                _logger.InfoFormat("The root URI [{0}] was redirected to [{1}]. [{1}] is the new root.",
                    _crawlContext.OriginalRootUri,
                    _crawlContext.RootUri);
            }
        }

        /// <summary>
        /// Retrieve the URI where the specified crawled page was redirected.
        /// </summary>
        /// <remarks>
        /// If HTTP auto redirections is disabled, this value is stored in the 'Location' header of the response.
        /// If auto redirections is enabled, this value is stored in the response's ResponseUri property.
        /// </remarks>
        protected virtual Uri ExtractRedirectUri(CrawledPage crawledPage)
        {
            Uri locationUri;
            if (_crawlContext.CrawlConfiguration.IsHttpRequestAutoRedirectsEnabled)
            {
                // For auto redirects, look for the response uri.
                locationUri = crawledPage.HttpWebResponse.ResponseUri;
            }
            else
            {
                // For manual redirects, we need to look for the location header.
                var location = crawledPage.HttpWebResponse.Headers["Location"];

                // Check if the location is absolute. If not, create an absolute uri.
                if (!Uri.TryCreate(location, UriKind.Absolute, out locationUri))
                {
                    Uri baseUri = new Uri(crawledPage.Uri.GetLeftPart(UriPartial.Authority));
                    locationUri = new Uri(baseUri, location);
                }
            }
            return locationUri;
        }

        public virtual void Dispose()
        {
            if (_threadManager != null)
            {
                _threadManager.Dispose();
            }
            if (_scheduler != null)
            {
                _scheduler.Dispose();
            }
            if (_pageRequester != null)
            {
                _pageRequester.Dispose();
            }
            if (_memoryManager != null)
            {
                _memoryManager.Dispose();
            }
        }
    }
}