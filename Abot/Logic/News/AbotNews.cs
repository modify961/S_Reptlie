﻿using System;
using System.Collections.Generic;
using System.Text;
using Abot.Crawler;
using Abot.Poco;
using System.Text.RegularExpressions;
using CsQuery.HtmlParser;

namespace Abot.Logic.News
{
    /// <summary>
    /// 
    /// </summary>
    public class AbotNews : IAbotProceed
    {
        /// <summary>
        /// 种子Url
        /// </summary>
        private  readonly Uri FeedUrl = new Uri(@"http://news.cnblogs.com/");

        /// <summary>
        ///匹配新闻详细页面的正则
        /// </summary>
        private Regex NewsUrlRegex = new Regex("^https://news.cnblogs.com/n/\\d+$", RegexOptions.Compiled);

        /// <summary>
        /// 匹配分页正则
        /// </summary>
        private Regex NewsPageRegex = new Regex("^https://news.cnblogs.com/n/page/\\d+/$", RegexOptions.Compiled);
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        private AbotContext _abotcontext;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="abotContext"></param>
        public AbotNews(AbotContext abotContext)
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
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
        }
        /// <summary>
        /// 返回种子链接
        /// </summary>
        /// <returns></returns>
        public Uri obtainFeedUrl()
        {
            return FeedUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
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
