using Abot.Crawler;
using Abot.Poco;
using CsQuery.HtmlParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class home : System.Web.UI.Page
{
    /// <summary>
    /// 种子Url
    /// </summary>
    public static readonly Uri FeedUrl = new Uri(@"http://news.cnblogs.com/");

    /// <summary>
    ///匹配新闻详细页面的正则
    /// </summary>
    public static Regex NewsUrlRegex = new Regex("^https://news.cnblogs.com/n/\\d+$", RegexOptions.Compiled);

    /// <summary>
    /// 匹配分页正则
    /// </summary>
    public static Regex NewsPageRegex = new Regex("^https://news.cnblogs.com/n/page/\\d+/$", RegexOptions.Compiled);

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    public static IWebCrawler GetManuallyConfiguredWebCrawler()
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
        config.MaxPagesToCrawl = 1000;
        config.MaxPagesToCrawlPerDomain = 0;
        config.MinCrawlDelayPerDomainMilliSeconds = 1000;

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
        var crawler = GetManuallyConfiguredWebCrawler();
        var result = crawler.Crawl(FeedUrl);
        Label1.Text = "结束";
    }
    public static void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
    {
    }
    public static void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e)
    {
    }
    public static void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
    {
    }
    public static void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
    {
        //判断是否是新闻详细页面
        if (NewsUrlRegex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
        {
            //获取信息标题和发表的时间
            var csTitle = e.CrawledPage.CsQueryDocument.Select("#news_title");
            var linkDom = csTitle.FirstElement().FirstChild;

            var newsInfo = e.CrawledPage.CsQueryDocument.Select("#news_info");
            var dateString = newsInfo.Select(".time", newsInfo);

            //判断是不是今天发表的
            if (IsPublishToday(dateString.Text()))
            {
                var str = (e.CrawledPage.Uri.AbsoluteUri + "\t" + HtmlData.HtmlDecode(linkDom.InnerText) + "\r\n");
                System.IO.File.AppendAllText("D:\\fake.txt", str);
            }
        }
    }
    /// <summary>
    /// "发布于 2016-05-09 11:25" => true
    /// </summary>
    public static bool IsPublishToday(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }

        const string prefix = "发布于";
        int index = str.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            str = str.Substring(prefix.Length).Trim();
        }

        DateTime date;
        return DateTime.TryParse(str, out date) && date.Date.Equals(DateTime.Today);
    }
    /// <summary>
    /// 如果是Feed页面或者分页或者详细页面才需要爬取
    /// </summary>
    private static CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
    {
        if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || FeedUrl == pageToCrawl.Uri
            || NewsPageRegex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
            || NewsUrlRegex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
        {
            return new CrawlDecision { Allow = true };
        }
        else
        {
            return new CrawlDecision { Allow = false, Reason = "Not match uri" };
        }
    }
    /// <summary>
    /// 如果是Feed页面或者分页或者详细页面才需要爬取
    /// </summary>
    private static CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
    {
        if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || FeedUrl == pageToCrawl.Uri
            || NewsPageRegex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
            || NewsUrlRegex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
        {
            return new CrawlDecision
            {
                Allow = true
            };
        }

        return new CrawlDecision { Allow = false, Reason = "Not match uri" };
    }
    private static CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
    {
        if (!crawledPage.IsInternal)
            return new CrawlDecision { Allow = false, Reason = "We dont crawl links of external pages" };

        if (crawledPage.IsRoot || crawledPage.IsRetry || crawledPage.Uri == FeedUrl
            || NewsPageRegex.IsMatch(crawledPage.Uri.AbsoluteUri))
        {
            return new CrawlDecision { Allow = true };
        }
        else
        {
            return new CrawlDecision { Allow = false, Reason = "We only crawl links of pagination pages" };
        }
    }
}