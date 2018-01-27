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
    /// 爬取大众点评数据
    /// </summary>
    public class AbotDZDP : IAbotProceed
    {
        /// <summary>
        /// 种子Url；济南美食频道URL
        /// </summary>
        public readonly Uri _foodurl = new Uri(@"http://www.dianping.com/jinan/food");

        /// <summary>
        ///饭店信息匹配页面
        /// </summary>
        public Regex _shopregex = new Regex("^http://www.dianping.com/shop/\\d+$", RegexOptions.Compiled);
        /// <summary>
        ///评论首页匹配项
        /// </summary>
        public Regex _reviewregex = new Regex("^http://www.dianping.com/shop/\\d+/review_all$", RegexOptions.Compiled);
        /// <summary>
        ///评论分页匹配项
        /// </summary>
        public Regex _reviewpageregex = new Regex("^http://www.dianping.com/shop/\\d+/review_all/p\\d+$", RegexOptions.Compiled);
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        private AbotContext _abotcontext;
        /// <summary>
        /// 构造函数
        /// </summary>
        public AbotDZDP(AbotContext abotContext)
        {
            _abotcontext = abotContext;
        }
        /// <summary>
        /// 返回种子URL
        /// </summary>
        /// <returns></returns>
        public Uri obtainFeedUrl()
        {
            return _foodurl;
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
                    ShopInfo shopInfo = new ShopInfo();
                    shopInfo.shopId = regNum(e.CrawledPage.Uri.AbsoluteUri).Replace(".","");
                    //获取店名
                    var shop_name = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".breadcrumb span");
                    if (shop_name != null)
                        shopInfo.shopName = shop_name.InnerHtml;
                    //获取评级
                    var mid_rank_stars = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".mid-rank-stars");
                    if (mid_rank_stars != null)
                        shopInfo.shopGrade = mid_rank_stars.GetAttribute("class");
                    //获取评论总数
                    var reviewCount = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector("#reviewCount");
                    if (reviewCount != null)
                        shopInfo.shopDiscuss = regNum(reviewCount.InnerHtml);
                    //获取平均价格
                    var avgPriceTitle = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector("#avgPriceTitle");
                    if (avgPriceTitle != null)
                        shopInfo.shopConsume = regNum(avgPriceTitle.InnerHtml);
                    //获取综合评分 口味、环境、服务
                    var comment_score = e.CrawledPage.AngleSharpHtmlDocument.QuerySelectorAll("#comment_score span");
                    if (comment_score != null && comment_score.Count() == 3)
                    {
                        shopInfo.shopTaste = regNum(comment_score[0].InnerHtml);
                        shopInfo.shopEnvironment = regNum(comment_score[1].InnerHtml);
                        shopInfo.shopServe = regNum(comment_score[2].InnerHtml);
                    }
                    //获取地址 
                    var street_address = e.CrawledPage.AngleSharpHtmlDocument.QuerySelectorAll("[itemprop = 'street-address']");
                    if (street_address != null && street_address.Length == 2)
                        shopInfo.shopAddress = street_address[1].GetAttribute("title");
                    //获取电话
                    var tel = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector("[itemprop='tel']");
                    if (tel != null)
                        shopInfo.shopTel = tel.InnerHtml;
                    if (!string.IsNullOrEmpty(shopInfo.shopId))
                    {
                        MQSend _mqsend = new MQSend();
                        _mqsend.send("shops", shopInfo.ToString());
                    }

                }
                //如果是评论首页或者分页
                if (_reviewregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri)
                    || _reviewpageregex.IsMatch(e.CrawledPage.Uri.AbsoluteUri))
                {
                    ShopInfo shopInfo = new ShopInfo();
                    List<DiscussInfo> discussInfos = new List<DiscussInfo>();
                    var content = e.CrawledPage.AngleSharpHtmlDocument.QuerySelectorAll(".reviews-tags a");
                    shopInfo.discussKey = "";
                    if (content != null)
                    {
                        foreach (var item in content)
                        {
                            shopInfo.discussKey = shopInfo.discussKey + "," + item.InnerHtml;
                        }
                    }
                    //获取评论分类数量，差评、中评、好评
                    var count = e.CrawledPage.AngleSharpHtmlDocument.QuerySelectorAll(".filters .count");
                    if (count != null && count.Count() >= 4)
                    {
                        shopInfo.highOpinion = regNum(count[1].InnerHtml);
                        shopInfo.middleOpinion = regNum(count[2].InnerHtml);
                        shopInfo.badOpinion = regNum(count[3].InnerHtml);
                    }
                    MQSend _mqsend = new MQSend();
                    _mqsend.send("shops", shopInfo.ToString());
                    var table = e.CrawledPage.AngleSharpHtmlDocument.QuerySelector(".reviews-items").FirstElementChild;
                    var linkDom = table.Children;


                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var cq in linkDom)
                    {
                        DiscussInfo discussInfo = new DiscussInfo();
                        //获取用户ID
                        var dper_photo_aside = cq.QuerySelector(".dper-photo-aside");
                        if (dper_photo_aside != null)
                            discussInfo.userId = dper_photo_aside.GetAttribute("data-user-id");
                        //获取用户帐号名
                        var dper_info = cq.QuerySelector(".dper-info a");
                        if (dper_info != null)
                            discussInfo.userName = regTrim(dper_info.InnerHtml);

                        var review_rank = cq.QuerySelector(".review-rank");
                        if (review_rank != null)
                        {
                            //星级
                            var span = review_rank.QuerySelector("span");
                            if (span != null)
                                discussInfo.level = span.GetAttribute("class");

                            //获取综合评分 口味、环境、服务
                            var comment_score = review_rank.QuerySelectorAll(".item");
                            if (comment_score != null && comment_score.Count() >= 3)
                            {
                                discussInfo.taste = regTrim(comment_score[0].InnerHtml);
                                discussInfo.environment = regTrim(comment_score[1].InnerHtml);
                                discussInfo.serve = regTrim(comment_score[2].InnerHtml);
                                if (comment_score.Count() == 4)
                                    discussInfo.consume = regNum(comment_score[3].InnerHtml);
                            }
                        }
                        //喜欢的菜
                        var review_recommend = cq.QuerySelectorAll(".review-recommend a");
                        discussInfo.favoriteDishes = "";
                        foreach (var item in review_recommend)
                        {
                            discussInfo.favoriteDishes = discussInfo.favoriteDishes + "," + item.InnerHtml;
                        }
                        if(!string.IsNullOrEmpty(discussInfo.userId))
                            discussInfos.Add(discussInfo);
                    }
                    if(discussInfos.Count>0)
                        _mqsend.send("reviews", JsonConvert.SerializeObject(discussInfos));
                }
            }
            catch (Exception es)
            {
            }
        }
        /// <summary>
        /// 替换掉字符串中的非数字
        /// </summary>
        /// <param name="value"></param>
        private string regNum(string value)
        {
            return Regex.Replace(value, @"[^\d^.]*", "");
        }
        /// <summary>
        /// 替换掉字符串中的空格
        /// </summary>
        /// <param name="value"></param>
        private string regTrim(string value)
        {
            return Regex.Replace(value, @"\s", "");
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
              || _shopregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
              || _reviewregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
              || _reviewpageregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
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
        /// 判断是否需要获取页面内的链接地址
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (!crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "只爬取网站内部的地址" };
            if (crawledPage.IsRoot || crawledPage.IsRetry || crawledPage.Uri == _foodurl
                || crawledPage.Uri.AbsoluteUri.IndexOf(@"www.dianping.com/shop") != -1)
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
            if (pageToCrawl.IsRoot || pageToCrawl.IsRetry || _foodurl == pageToCrawl.Uri
              || _shopregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
              || _reviewregex.IsMatch(pageToCrawl.Uri.AbsoluteUri)
              || _reviewpageregex.IsMatch(pageToCrawl.Uri.AbsoluteUri))
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
