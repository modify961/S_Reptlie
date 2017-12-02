using Abot.Poco;
using System;

namespace Abot.Crawler
{
    /// <summary>
    /// 爬虫自定义事件
    /// </summary>
    [Serializable]
    public class CrawlArgs : EventArgs
    {
        /// <summary>
        /// 爬虫配置上下文
        /// </summary>
        public CrawlContext CrawlContext { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="crawlContext"></param>
        public CrawlArgs(CrawlContext crawlContext)
        {
            if (crawlContext == null)
                throw new ArgumentNullException("crawlContext");

            CrawlContext = crawlContext;
        }
    }
}
