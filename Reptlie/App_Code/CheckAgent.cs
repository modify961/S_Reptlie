using Abot.Crawler;
using Abot.Logic;
using Abot.Logic.enums;
using Abot.Poco;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// checkAgent 的摘要说明
/// </summary>
public class CheckAgent:IJob
{
    public IAbotProceed iAbotProceed = null;
    /// <summary>
    /// 执行检测进程程序
    /// </summary>
    /// <param name="context"></param>
    public void Execute(IJobExecutionContext context)
    {
        AbotFactory abotFactory = new AbotFactory();
        AbotContext abotContext = new AbotContext()
        {
            abotTypeEnum = AbotTypeEnum.AGENT
        };
        iAbotProceed = abotFactory.execute(abotContext);
        var crawler = GetManuallyConfiguredWebCrawler();
        var result = crawler.Crawl(iAbotProceed.obtainFeedUrl());
    }
    public IWebCrawler GetManuallyConfiguredWebCrawler()
    {
        CrawlConfiguration config = new CrawlConfiguration();
        config.CrawlTimeoutSeconds = 0;
        config.DownloadableContentTypes = "text/html, text/plain";
        config.IsExternalPageCrawlingEnabled = false;
        config.IsExternalPageLinksCrawlingEnabled = false;
        config.IsRespectRobotsDotTextEnabled = false;
        config.IsUriRecrawlingEnabled = false;
        //System.Environment.ProcessorCount：获取当前计算机上的处理器数。
        //config.MaxConcurrentThreads = System.Environment.ProcessorCount;
        config.MaxConcurrentThreads = 1;
        config.MaxPagesToCrawl = 1000;
        config.MaxPagesToCrawlPerDomain = 0;
        config.MinCrawlDelayPerDomainMilliSeconds = 1000;
        config.IsHttpRequestAutomaticDecompressionEnabled = true;
        //调用WebCrawler
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
    public void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
    {
    }
    public void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e)
    {
    }
    public void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
    {
    }
    public void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
    {
        iAbotProceed.crawler_ProcessPageCrawlCompletedAsync(sender, e);
    }
    /// <summary>
    /// 如果是Feed页面或者分页或者详细页面才需要爬取
    /// </summary>
    private CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
    {
        return iAbotProceed.ShouldCrawlPage(pageToCrawl, context);
    }
    /// <summary>
    /// 如果是Feed页面或者分页或者详细页面才需要爬取
    /// </summary>
    private CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
    {
        return iAbotProceed.ShouldDownloadPageContent(pageToCrawl, crawlContext);
    }
    private CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
    {
        return iAbotProceed.ShouldDownloadPageContent(crawledPage, crawlContext);
    }
}