using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abot.Crawler;
using Abot.Poco;

namespace Abot.Logic.reptlie
{
    /// <summary>
    /// 爬取抽线类
    /// </summary>
    public  class AbstractAgent : IAbotProceed
    {
        /// <summary>
        /// 种子Url
        /// </summary>
        public readonly Uri _rooturl = null;
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        protected AbotContext _abotcontext;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="abotContext"></param>
        protected AbstractAgent(AbotContext abotContext) {
            _abotcontext = abotContext;
            _rooturl = new Uri(abotContext.rootUrl);
        }
        /// <summary>
        /// 返回根URL
        /// </summary>
        /// <returns></returns>
        public Uri obtainFeedUrl()
        {
            return _rooturl;
        }
        /// <summary>
        /// 页面不允许被爬取的判断接口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e)
        {
             
        }
        /// <summary>
        /// 页面链接不允许被爬取的判断接口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
             
        }
        /// <summary>
        /// 页面内容获取完成后的自定义处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
        {
             
        }
        /// <summary>
        /// 开始爬取的接口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
             
        }

        /// <summary>
        /// 判断一个页面是否需要被获取
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
        {
            return null;
        }
        /// <summary>
        /// 通过链接判断页面内容需要不需要下载
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public virtual CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            return null;
        }
        /// <summary>
        /// 判断是否需要下载页面的内容
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public virtual CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            return null;
        }
    }
}
