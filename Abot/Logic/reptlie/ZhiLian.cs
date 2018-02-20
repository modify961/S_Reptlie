using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abot.Crawler;
using Abot.Poco;
using System.Text.RegularExpressions;
using Abot.Logic.model;
using RabbitMQHelper;
using Newtonsoft.Json;

namespace Abot.Logic.reptlie
{
    /// <summary>
    /// 智联招聘爬取功能
    /// </summary>
    public class ZhiLian : IAbotProceed
    {
        /// <summary>
        /// 种子Url；济南美食频道URL
        /// </summary>
        public readonly Uri _foodurl = null;

        /// <summary>
        ///职位页面
        /// </summary>
        public Regex _shopregex = new Regex("^http://jobs.zhaopin.com/\\d+.htm", RegexOptions.Compiled);
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        private AbotContext _abotcontext;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Uri obtainFeedUrl()
        {
            return _foodurl;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public ZhiLian(AbotContext abotContext)
        {
            _abotcontext = abotContext;
            _foodurl=new Uri(abotContext.rootUrl);
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
            try
            {
                //如果店铺信息
                if (_shopregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
                {
                    JobInfo jobInfo = new JobInfo();
                    //获取店名
                    var workName = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".inner-left h1");
                    if (workName != null)
                        jobInfo.name = workName.TextContent;
                    if (string.IsNullOrEmpty(jobInfo.name))
                        return;
                    //获取评级
                    var welfare_tab = e.CrawledPage.AngleSharpHtmlDocument.QuerySelectorAll(".welfare-tab-box span");
                    jobInfo.mate = "";
                    foreach (var item in welfare_tab)
                    {
                        jobInfo.mate = jobInfo.mate + item.TextContent;
                    }

                    //获取评论总数
                    var reviewCount = e.CrawledPage.AngleSharpHtmlDocument.QuerySelectorAll(".terminalpage-left strong");
                    if (reviewCount != null)
                    {
                        if (reviewCount.Count() > 7)
                        {
                            jobInfo.pay = reviewCount[0].TextContent;
                            jobInfo.address = reviewCount[1].TextContent;
                            DateTime pub = DateTime.Now;
                            if (DateTime.TryParse(reviewCount[2].TextContent, out pub))
                            {
                                jobInfo.publicDate = pub.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            jobInfo.workType = reviewCount[3].TextContent;
                            jobInfo.expe = reviewCount[4].TextContent;
                            jobInfo.rec = reviewCount[5].TextContent;
                            jobInfo.num = reviewCount[6].TextContent;
                            jobInfo.jobType = reviewCount[7].TextContent;
                        }
                    }
                    //获取平均价格
                    var avgPriceTitle = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".tab-inner-cont");
                    if (avgPriceTitle != null)
                        jobInfo.remark = avgPriceTitle.TextContent;
                    //获取综合评分 口味、环境、服务
                    var comment_score = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".company-name-t a");
                    if (comment_score != null)
                    {
                        jobInfo.company = comment_score.TextContent;

                    }
                    var company_box = e.CrawledPage.AngleSharpHtmlDocument.QuerySelectorAll(".company-box strong");
                    if (company_box != null)
                    {
                        if (company_box.Count() > 3)
                        {
                            jobInfo.companyNum = company_box[0].TextContent;
                            jobInfo.companyType = company_box[1].TextContent;
                            jobInfo.companyTrde = company_box[2].TextContent;
                            if (company_box[3].TextContent.IndexOf("http") != -1)
                            {
                                jobInfo.url = company_box[3].TextContent;
                            }
                        }
                    }
                    
                    MQSend _mqsend = new MQSend();
                    _mqsend.send("jobs", JsonConvert.SerializeObject(jobInfo));
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
        /// 判断一个页面是否需要被获取
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext context)
        {
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _foodurl == pageToCrawl.Uri
              || _shopregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
            {
                return new CrawlDecision { Allow = true };
            }
            else
            {
                return new CrawlDecision { Allow = false, Reason = "Not match uri" };
            }
        }
        /// <summary>
        /// 通过链接判断页面内容需要不需要下载
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (!crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "只爬取网站内部的地址" };
            if (crawledPage.IsRoot || crawledPage.IsRetry || crawledPage.Uri == _foodurl
                || _shopregex.IsMatch(crawledPage.Uri.AbsoluteUri))
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
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _foodurl == pageToCrawl.Uri
              || _shopregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
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
