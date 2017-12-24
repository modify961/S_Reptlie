using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abot.Crawler;
using Abot.Poco;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Abot.Core;
using RabbitMQHelper;

namespace Abot.Logic.News
{
    /// <summary>
    /// 获取代理类
    /// </summary>
    public class AbotAgent : IAbotProceed
    {
        /// <summary>
        /// 种子Url
        /// </summary>
        public readonly Uri _feedurl = new Uri(@"http://www.xicidaili.com/nn/1");

        /// <summary>
        ///匹配所有济南的饭店页面
        /// </summary>
        public Regex _pageurlregex = new Regex("^http://www.xicidaili.com/nn/\\d+$", RegexOptions.Compiled);
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        private AbotContext _abotcontext;
        private MQSend _mqsend = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        public AbotAgent(AbotContext abotContext)
        {
            _mqsend = new MQSend();
            _abotcontext = abotContext;
        }
        /// <summary>
        /// 返回种子URL
        /// </summary>
        /// <returns></returns>
        public Uri obtainFeedUrl()
        {
            return _feedurl;
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
            if (_pageurlregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
            {
                try
                {
                    var table = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector("#ip_list");
                    var linkDom = table.QuerySelectorAll(".odd");
                    StringBuilder stringBuilder = new StringBuilder();
                    String date = "";
                    foreach (var cq in linkDom)
                    {
                        IHtmlCollection<IElement> nodeList = cq.QuerySelectorAll("td");
                        if (nodeList.Length > 9)
                        {
                            if (nodeList[5].TextContent.ToLower().IndexOf("http") != -1)
                            {
                                date = nodeList[8].TextContent;
                                int dateOfNum = 0;
                                if (date != null && date.IndexOf("天") != -1)
                                {
                                    int.TryParse(date.Replace("天", ""), out dateOfNum);
                                    dateOfNum = (dateOfNum * 24 * 60);
                                }
                                else if (date != null && date.IndexOf("小时") != -1)
                                {
                                    int.TryParse(date.Replace("小时", ""), out dateOfNum);
                                    dateOfNum = dateOfNum * 60;
                                }
                                else if (date != null && date.IndexOf("分钟") != -1)
                                {
                                    int.TryParse(date.Replace("分钟", ""), out dateOfNum);
                                }
                                int port = 0;
                                if (int.TryParse(nodeList[2].TextContent, out port))
                                {
                                    Agenter agent = new Agenter()
                                    {
                                        ip = nodeList[1].TextContent,
                                        port = port,
                                        type = nodeList[5].TextContent.ToLower(),
                                        survibal = dateOfNum,
                                        usable = true
                                    };
                                    _mqsend.send("angentIp", agent.ToString());
                                }
                            }
                        }

                    }
                }
                catch (Exception es) {
                }
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
        /// 
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _feedurl == pageToCrawl.Uri
              || _pageurlregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
              )
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "Not match uri" };
            }
        }
        /// <summary>
        /// 判断链接是否需要爬取
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (!crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "只爬取网站内部的地址" };

            if (crawledPage.IsRoot || crawledPage.IsRetry || crawledPage.Uri == _feedurl
                || _pageurlregex.IsMatch(crawledPage.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "只爬取网站内部的地址" };
            }
        }
        /// <summary>
        /// 通过链接判断页面内容需要不需要下载
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public CrawlDecision ShouldDownloadPageContent(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _feedurl == pageToCrawl.Uri
             || _pageurlregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
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
