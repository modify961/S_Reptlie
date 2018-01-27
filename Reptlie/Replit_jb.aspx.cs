using Abot.Crawler;
using Abot.Logic;
using Abot.Logic.enums;
using Abot.Logic.reptlie;
using Abot.Poco;
using Abot.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Replit_jb : System.Web.UI.Page
{
    private Jibing jibing;
    protected void Page_Load(object sender, EventArgs e)
    {

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
        config.MaxConcurrentThreads = System.Environment.ProcessorCount;
        //config.MaxConcurrentThreads = 1;
        config.MaxPagesToCrawl = 50000;
        config.MaxPagesToCrawlPerDomain = 0;
        config.MinCrawlDelayPerDomainMilliSeconds = 1000;
        //初始化代理IP池操作类
        config.agentHelp = new AgentHelp();
        config.useAgent = false;
        config.simulation = false;
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
    protected void Button1_Click(object sender, EventArgs e)
    {
        jibing = new Jibing(new AbotContext()
        {
            abotTypeEnum = AbotTypeEnum.DZDP
        });
        var crawler = GetManuallyConfiguredWebCrawler();
        var result = crawler.Crawl(jibing.obtainFeedUrl());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
    {
        jibing.crawler_ProcessPageCrawlStarting(sender, e);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e)
    {
        jibing.crawler_PageCrawlDisallowed(sender, e);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
    {
        jibing.crawler_PageLinksCrawlDisallowed(sender, e);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
    {
        jibing.crawler_ProcessPageCrawlCompletedAsync(sender, e);
    }

    /// <summary>
    /// 如果是Feed页面或者分页或者详细页面才需要爬取
    /// </summary>
    private CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
    {
        return jibing.ShouldCrawlPage(pageToCrawl, context);
    }
    /// <summary>
    /// 如果是Feed页面或者分页或者详细页面才需要爬取
    /// </summary>
    private CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
    {
        return jibing.ShouldDownloadPageContent(pageToCrawl, crawlContext);
    }
    private CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
    {
        return jibing.ShouldCrawlPageLinks(crawledPage, crawlContext);
    }
}