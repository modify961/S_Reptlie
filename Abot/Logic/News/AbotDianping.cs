using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abot.Crawler;
using Abot.Poco;
using System.Text.RegularExpressions;
using CsQuery.HtmlParser;

namespace Abot.Logic.News
{
    /// <summary>
    /// 大众点评数据抓取类
    /// </summary>
    public class AbotDianping : IAbotProceed
    {

        /// <summary>
        /// 种子Url
        /// </summary>
        public readonly Uri _feedurl = new Uri(@"http://www.dianping.com/jinan/food");

        /// <summary>
        ///匹配所有济南的饭店页面
        /// </summary>
        public Regex _shopurlregex = new Regex("^http://www.dianping.com/shop/\\d+/$", RegexOptions.Compiled);

        /// <summary>
        ///匹配饭店评论里的全部评论链接
        /// </summary>
        public Regex _reviewurlregex = new Regex("^http://www.dianping.com/jinan/shop/\\d+/review_all$", RegexOptions.Compiled);
        
        /// <summary>
        ///匹配分页链接
        /// </summary>
        public Regex _reviewpageregex = new Regex("^http://www.dianping.com/jinan/shop/\\d+/review_all/p\\d+$", RegexOptions.Compiled);
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        private AbotContext _abotcontext;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="abotContext"></param>
        public AbotDianping(AbotContext abotContext)
        {
            _abotcontext = abotContext;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
        {
            //判断是否为全部评论的首页或者其他分页
            if (_reviewurlregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri)|| _reviewpageregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
            {
                //获取信息标题和发表的时间
                var csTitle = e.CrawledPage.CsQueryDocument.Select(".revitew-title");
                var linkDom = csTitle.FirstElement().FirstChild;

                //判断是不是今天发表的
                var str = (e.CrawledPage.Uri.AbsoluteUri + "\t" + HtmlData.HtmlDecode(linkDom.InnerText) + "\r\n");
                System.IO.File.AppendAllText("D:\\fake.txt", str);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取种子节点
        /// </summary>
        /// <returns></returns>
        public Uri obtainFeedUrl()
        {
            return _feedurl;
        }

        /// <summary>
        /// 判断一个页面是否需要被爬取页面内部的url
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _feedurl == pageToCrawl.Uri
             || _shopurlregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
             || _reviewurlregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
             || _reviewurlregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "Not match uri" };
            }
        }
        /// <summary>
        /// 根据链接判断是否需要进行爬取
        /// 主要用于判断链接是否为网站内的链接，确保不会跳转到其他站点。如果不需要限制则直接返回true即可
        /// crawledPage.IsInternal：是否是站内页面
        /// 此处指爬取 店面链接
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (!crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "只爬取大众网内部的链接" };

            if (crawledPage.IsRoot || crawledPage.IsRetry || crawledPage.Uri == _feedurl
                || _shopurlregex.IsMatch(crawledPage.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "只爬取大众网的店铺链接" };
            }
        }
        /// <summary>
        /// 根据链接判断链接是不是指向饭店\更多评论\评论分页的页面,符合规则 则会下载页面内容，
        /// pageToCrawl.IsRoot：是否为跟页面
        /// pageToCrawl.IsRetry：当前http请求是否被请求多次
        /// pageToCrawl.Uri：页面URL
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _feedurl == pageToCrawl.Uri
             || _shopurlregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
             || _reviewurlregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
             || _reviewurlregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
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
