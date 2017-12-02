using Abot.Poco;
using System;

namespace Abot.Crawler
{
    /// <summary>
    /// 链接不允许被爬行的自定义事件
    /// </summary>
    [Serializable]
    public class PageLinksCrawlDisallowedArgs : PageCrawlCompletedArgs
    {
        /// <summary>
        /// 不允许的原因
        /// </summary>
        public string DisallowedReason { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawlContext"></param>
        /// <param name="crawledPage"></param>
        /// <param name="disallowedReason"></param>
        public PageLinksCrawlDisallowedArgs(CrawlContext crawlContext, CrawledPage crawledPage, string disallowedReason)
            : base(crawlContext, crawledPage)
        {
            if (string.IsNullOrWhiteSpace(disallowedReason))
                throw new ArgumentNullException("disallowedReason");

            DisallowedReason = disallowedReason;
        }
    }
}
