using Abot.Poco;
using System;

namespace Abot.Crawler
{
    /// <summary>
    /// 爬行被驳回的自定义事件
    /// </summary>
    [Serializable]
    public class PageCrawlDisallowedArgs: PageCrawlStartingArgs
    {
        /// <summary>
        /// 不允许的原因
        /// </summary>
        public string DisallowedReason { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlContext"></param>
        /// <param name="pageToCrawl"></param>
        /// <param name="disallowedReason"></param>
        public PageCrawlDisallowedArgs(CrawlContext crawlContext, PageToCrawl pageToCrawl, string disallowedReason)
            : base(crawlContext, pageToCrawl)
        {
            if (string.IsNullOrWhiteSpace(disallowedReason))
                throw new ArgumentNullException("disallowedReason");

            DisallowedReason = disallowedReason;
        }
    }
}
