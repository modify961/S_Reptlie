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
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;

namespace Abot.Logic.reptlie
{
    /// <summary>
    /// 
    /// </summary>
    public class Qbai : AbstractAgent
    {
        /// <summary>
        ///是否为糗百文字内容
        /// </summary>
        private Regex _contentregex = new Regex("^https://www.qiushibaike.com/article/\\d+", RegexOptions.Compiled);
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="abotContext"></param>
        public Qbai(AbotContext abotContext) : base(abotContext)
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
                //如果店铺信息
                if (_contentregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
                {
                    QbaiInfo qbaiInfo = new QbaiInfo();
                    qbaiInfo.id = e.CrawledPage.Uri.AbsoluteUri;
                    //获取笑话主题内容
                    var info = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector("#single-next-link");
                    if (info != null)
                        qbaiInfo.info = info.TextContent;
                    if (string.IsNullOrEmpty(qbaiInfo.info))
                        return;
                    //获取笑话点赞数
                    var num = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".stats-vote i");
                    if (num != null)
                    {
                        int nums = 0;
                        int.TryParse(num.TextContent, out nums);
                        //好笑数小于500的，不获取
                        if (nums < 500)
                            return;
                        qbaiInfo.silmeNum = nums;
                    }
                    //获取作者名字
                    var name = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".author img");
                    if (name != null)
                    {
                        qbaiInfo.from = name.Attributes["alt"].Value;
                    }
                    MQSend _mqsend = new MQSend();
                    _mqsend.send("qbais", JsonConvert.SerializeObject(qbaiInfo));
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
