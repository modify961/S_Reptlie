using System;
using System.Collections.Generic;
using System.Text;
using Abot.Crawler;
using Abot.Poco;
using System.Text.RegularExpressions;
using CsQuery.HtmlParser;

namespace Abot.Logic.News
{
    public class AbotNews : IAbotProceed
    {
        /// <summary>
        /// 种子Url
        /// </summary>
        public  readonly Uri FeedUrl = new Uri(@"http://news.cnblogs.com/");

        /// <summary>
        ///匹配新闻详细页面的正则
        /// </summary>
        public  Regex NewsUrlRegex = new Regex("^https://news.cnblogs.com/n/\\d+$", RegexOptions.Compiled);

        /// <summary>
        /// 匹配分页正则
        /// </summary>
        public  Regex NewsPageRegex = new Regex("^https://news.cnblogs.com/n/page/\\d+/$", RegexOptions.Compiled);
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        private AbotContext _abotcontext;
        public AbotNews(AbotContext abotContext)
        {
            _abotcontext = abotContext;
        }
        public void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e)
        {
            
        }

        public void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            
        }

        public void crawler_ProcessPageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
        {
            //判断是否是新闻详细页面
            if (NewsUrlRegex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
            {
                //获取信息标题和发表的时间
                var csTitle = e.CrawledPage.CsQueryDocument.Select("#news_title");
                var linkDom = csTitle.FirstElement().FirstChild;

                var newsInfo = e.CrawledPage.CsQueryDocument.Select("#news_info");
                var dateString = newsInfo.Select(".time", newsInfo);

                //判断是不是今天发表的
                var str = (e.CrawledPage.Uri.AbsoluteUri + "\t" + HtmlData.HtmlDecode(linkDom.InnerText) + "\r\n");
                System.IO.File.AppendAllText("D:\\fake.txt", str);
            }
        }

        public void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
        }

        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || FeedUrl == pageToCrawl.Uri
            || NewsPageRegex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
            || NewsUrlRegex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "Not match uri" };
            }
        }

        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (!crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "We dont crawl links of external pages" };

            if (crawledPage.IsRoot || crawledPage.IsRetry || crawledPage.Uri == FeedUrl
                || NewsPageRegex.IsMatch(crawledPage.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "We only crawl links of pagination pages" };
            }
        }

        public CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || FeedUrl == pageToCrawl.Uri
            || NewsPageRegex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
            || NewsUrlRegex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
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
