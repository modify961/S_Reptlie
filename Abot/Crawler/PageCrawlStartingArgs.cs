using Abot.Poco;
using System;

namespace Abot.Crawler
{
    /// <summary>
    /// 开始爬行的自定义事件
    /// </summary>
    [Serializable]
    public class PageCrawlStartingArgs : CrawlArgs
    {
        /// <summary>
        /// 爬行的页面
        /// </summary>
        public PageToCrawl PageToCrawl { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlContext"></param>
        /// <param name="pageToCrawl"></param>
        public PageCrawlStartingArgs(CrawlContext crawlContext, PageToCrawl pageToCrawl)
            : base(crawlContext)
        {
            if (pageToCrawl == null)
                throw new ArgumentNullException("pageToCrawl");

            PageToCrawl = pageToCrawl;
        }
    }
}
