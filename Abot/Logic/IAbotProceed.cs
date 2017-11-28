using Abot.Crawler;
using Abot.Poco;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abot.Logic
{
    /// <summary>
    /// Abot爬行公共接口
    /// </summary>
    public interface IAbotProceed
    {
        /// <summary>
        /// crawler_ProcessPageCrawlStarting:在页面被爬行之前执行的处理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e);
        /// <summary>
        /// 当 ICrawlDecisionMaker.ShouldCrawl 属性为false时被执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e);
        /// <summary>
        /// 当 ICrawlDecisionMaker.ShouldCrawl 属性为false时被执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e);
        /// <summary>
        /// 页面被抓取之后的处理方法
        /// PS：用于实现匹配逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e);
        /// <summary>
        /// 在请求页面之前运行。用来判断这个页面是否需要被Crawl
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context);
        /// <summary>
        /// 判断一个页面是否需要被下载以及Crawl
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext);
        /// <summary>
        /// 判断一个链接是否需要被Crawl
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext);
    }
}
