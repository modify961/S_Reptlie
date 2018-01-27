using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abot.Crawler;
using Abot.Poco;
using System.Text.RegularExpressions;

namespace Abot.Logic.reptlie
{
    public class Jibing : IAbotProceed
    {
        /// <summary>
        /// 种子Url；
        /// </summary>
        public readonly Uri _searchurl = new Uri(@"https://www.jianke.com/jibing/zimu/l");

        /// <summary>
        ///匹配疾病信息
        /// </summary>
        public Regex _jibinggex = new Regex("^https://www.jianke.com/jibing/zimu/[L-Zl-z]$", RegexOptions.Compiled);
        /// <summary>
        ///匹配疾病信息
        /// </summary>
        public Regex _reviewregex = new Regex("^https://www.jianke.com/jibing/gaishu/\\d+$", RegexOptions.Compiled);
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        private AbotContext _abotcontext;
        /// <summary>
        /// 构造函数
        /// </summary>
        public Jibing(AbotContext abotContext)
        {
            _abotcontext = abotContext;
        }
        /// <summary>
        /// 返回种子URL
        /// </summary>
        /// <returns></returns>
        public Uri obtainFeedUrl()
        {
            return _searchurl;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void crawler_PageCrawlDisallowed(object sender, PageCrawlStartingArgs e)
        {
            
        }

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
            try
            {
                System.IO.File.AppendAllText("C:\\data\\bingzheng\\ip.txt", e.CrawledPage.Uri.AbsoluteUri + "\r\n");
                //如果店铺信息
                if (_reviewregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
                {
                   
                    string str = "";
                    //获取店名
                    var workName = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".f16");
                    if (workName != null)
                        str = workName.InnerHtml.Replace(" 的症状：", "") + ";";
                    //获取评级
                    var welfare_tab = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".h1_sm");
                    if (welfare_tab != null)
                        str = str + welfare_tab.InnerHtml.Replace("别名：", "") + ",";
                    str = str + ";";
                    var content = e.CrawledPage.AngleSharpHtmlDocument.QuerySelectorAll(".tips_list .clearfix");
                    if (content != null)
                    {
                        foreach (var item in content)
                        {
                            if (item.InnerHtml.IndexOf("典型症状") != -1)
                            {
                                var qc = item.QuerySelectorAll("a");
                                foreach (var q in qc)
                                {
                                    str = str + q.InnerHtml + ",";
                                }
                                str = str + ";";
                            }

                            if (item.InnerHtml.IndexOf("就诊科室") != -1)
                            {

                                var qc = item.QuerySelectorAll("span");
                                foreach (var q in qc)
                                {
                                    str = str + q.InnerHtml + ",";
                                }
                                str = str + ";";
                            }
                        }
                    }
                    string name = DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString();
                    System.IO.File.AppendAllText("C:\\data\\bingzheng\\" + name + ".txt", str+"\r\n");
                }
            }
            catch (Exception es)
            {
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
        /// 
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _searchurl == pageToCrawl.Uri
             || _jibinggex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
             || _reviewregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
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
        /// 
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (!crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "只爬取网站内部的地址" };
            if (crawledPage.IsRoot || crawledPage.IsRetry || crawledPage.Uri == _searchurl
                || _jibinggex.IsMatch(crawledPage.Uri.AbsoluteUri)
                || _reviewregex.IsMatch(crawledPage.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "只爬取网站内部的地址" };
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
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _searchurl == pageToCrawl.Uri
              || _jibinggex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
              || _reviewregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
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
