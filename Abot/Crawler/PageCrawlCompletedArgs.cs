using Abot.Poco;
using System;

namespace Abot.Crawler
{
    /// <summary>
    ///爬行结束的自定义事件
    /// </summary>
    [Serializable]
    public class PageCrawlCompletedArgs : CrawlArgs
    {
        /// <summary>
        /// 爬行过的页面
        /// </summary>
        public CrawledPage CrawledPage { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlContext"></param>
        /// <param name="crawledPage"></param>
        public PageCrawlCompletedArgs(CrawlContext crawlContext, CrawledPage crawledPage)
            : base(crawlContext)
        {
            if (crawledPage == null)
                throw new ArgumentNullException("crawledPage");

            CrawledPage = crawledPage;
        }
    }
}
