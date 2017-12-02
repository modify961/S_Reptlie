
using System;

namespace Abot.Poco
{
    /// <summary>
    /// 用于存储 裁决是否可以被爬取的数据
    /// </summary>
    [Serializable]
    public class CrawlDecision
    {
        /// <summary>
        /// 
        /// </summary>
        public CrawlDecision()
        {
            Reason = "";
        }

        /// <summary>
        /// Whether to allow the crawl decision
        /// 是否被允许爬取
        /// </summary>
        public bool Allow { get; set; }

        /// <summary>
        /// The reason the crawl decision was NOT allowed
        /// 不被运行爬行的原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Whether the crawl should be stopped. Will clear all scheduled pages but will allow any threads that are currently crawling to complete.
        /// 用于判断爬行是否需要停止：如果为true
        /// 则会清空scheduled pages（排队的页面）；但是允许已经运行的线程继续获取
        /// </summary>
        public bool ShouldStopCrawl { get; set; }

        /// <summary>
        /// Whether the crawl should be "hard stopped". Will clear all scheduled pages and cancel any threads that are currently crawling.
        /// 是否强制停止
        ///  则会清空scheduled pages（排队的页面）已经运行的线程也会被强行停止
        /// </summary>
        public bool ShouldHardStopCrawl { get; set; }
    }
}
