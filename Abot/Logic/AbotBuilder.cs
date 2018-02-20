using Abot.Crawler;
using Abot.Poco;
using Abot.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Abot.Logic
{
    /// <summary>
    /// 构造爬虫程序
    /// </summary>
    public class AbotBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        private IAbotProceed _abotproceed;
        /// <summary>
        /// 爬取配置上下文
        /// </summary>
        private AbotContext _abotContext;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="abotProceed">爬取进程</param>
        /// <param name="abotContext">配置参数</param>
        public AbotBuilder(IAbotProceed abotProceed, AbotContext abotContext) {
            _abotproceed = abotProceed;
            _abotContext = abotContext;
        }
        /// <summary>
        /// 执行程序
        /// </summary>
        public void exe() {
            Thread thread = new Thread(new ThreadStart(createThead));
            thread.Start();
        }
        /// <summary>
        /// 开始执行爬取任务
        /// </summary>
        private void createThead()
        {
            var crawler = GetManuallyConfiguredWebCrawler();
            var result = crawler.Crawl(_abotproceed.obtainFeedUrl());
        }
        /// <summary>
        /// 配置参数
        /// </summary>
        /// <returns></returns>
        private IWebCrawler GetManuallyConfiguredWebCrawler()
        {
            CrawlConfiguration config = new CrawlConfiguration();
            config.CrawlTimeoutSeconds = 0;
            config.DownloadableContentTypes = "text/html, text/plain";
            config.IsExternalPageCrawlingEnabled = false;
            config.IsExternalPageLinksCrawlingEnabled = false;
            config.IsRespectRobotsDotTextEnabled = false;
            config.IsUriRecrawlingEnabled = false;
            //System.Environment.ProcessorCount：获取当前计算机上的处理器数。
            if(_abotContext.threadNum==0)
                config.MaxConcurrentThreads = System.Environment.ProcessorCount;
            else
                config.MaxConcurrentThreads = _abotContext.threadNum;
            config.MaxPagesToCrawl = 5000;
            config.MaxPagesToCrawlPerDomain = 0;
            config.MinCrawlDelayPerDomainMilliSeconds = 1000;
            config.minNeed = _abotContext.minNeed == 0 ? 5 : _abotContext.minNeed;
            config.maxNeed = _abotContext.minNeed == 0 ? (config.minNeed+5) : _abotContext.maxNeed;
            //初始化代理IP池操作类
            config.useAgent = false;
            if (_abotContext.useAgent)
            {
                config.useAgent = true;
                config.agentHelp = new AgentHelp();
            }
            config.simulation = _abotContext.simulation;
            config.requestHeader = _abotContext.requestHeader;
            //初始化执行类
            var crawler = new PoliteWebCrawler(config, null, null, null, null, null, null, null, null);

            crawler.ShouldCrawlPage(ShouldCrawlPage);
            crawler.ShouldDownloadPageContent(ShouldDownloadPageContent);
            crawler.ShouldCrawlPageLinks(ShouldCrawlPageLinks);
            crawler.PageCrawlStartingAsync += crawler_ProcessPageCrawlStarting;
            //爬取页面后的回调函数
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompletedAsync;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            return crawler;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            _abotproceed.crawler_ProcessPageCrawlStarting(sender, e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e)
        {
            _abotproceed.crawler_PageCrawlDisallowed(sender, e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            _abotproceed.crawler_PageLinksCrawlDisallowed(sender, e);
        }
        /// <summary>
        /// 程序获取到页面内容后，对页面内容解析的接口方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
        {
            _abotproceed.crawler_ProcessPageCrawlCompletedAsync(sender, e);
        }

        /// <summary>
        /// 判断实现需要获取页面
        /// </summary>
        private CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
        {
            return _abotproceed.ShouldCrawlPage(pageToCrawl, context);
        }
        /// <summary>
        /// 判断是否需要下载页面内容
        /// </summary>
        private CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            return _abotproceed.ShouldDownloadPageContent(pageToCrawl, crawlContext);
        }
        /// <summary>
        /// 判断是否需要解析页面内的URL链接，作为后继爬取的地址
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        private CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            return _abotproceed.ShouldCrawlPageLinks(crawledPage, crawlContext);
        }
    }
}
