using Abot.Crawler;
using Abot.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Abot.Logic.reptlie
{
    /// <summary>
    /// 
    /// </summary>
    public class WeiBo : AbstractAgent
    {
        /// <summary>
        ///
        /// </summary>
        private Regex _contentregex = new Regex("https://weibo.com/p/\\d+/follow?relate=fans&from=100505&wvr=6&mod=headfans&current=fans#place", RegexOptions.Compiled);
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="abotContext"></param>
        public WeiBo(AbotContext abotContext) : base(abotContext)
        {
        }
        /// <summary>
        /// 页面内容获取完成后的自定义处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
        {
            try
            {
                if (_contentregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
                {
                   
                }
            }
            catch (Exception es)
            {
            }
        }
        /// <summary>
        /// 根据URL判断页面是否需要爬取
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _rooturl == pageToCrawl.Uri
              || _contentregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "Not match uri" };
            }
        }
        /// <summary>
        /// 根据链接判断页面的链接是否需要爬取
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public override CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (!crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "只爬取网站内部的地址" };
            if (crawledPage.IsRoot || crawledPage.IsRetry || crawledPage.Uri == _rooturl
                || _contentregex.IsMatch(crawledPage.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "只爬取网站内部的地址" };
            }
        }
        /// <summary>
        /// 根据链接判断是否需要下载页面内容
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public override CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _rooturl == pageToCrawl.Uri
              || _contentregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
            {
                return new CrawlDecision
                {
                    Allow = true
                };
            }

            return new CrawlDecision { Allow = false, Reason = "Not match uri" };
        }
    }
}
