using Abot.Core;
using Abot.Logic.model;
using Abot.Poco;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using HtmlAgilityPack;
using RabbitMQHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class requestPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Label1.Text = AgentSingleton.getInstance.count().ToString();
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        HttpWebRequest request = null;
        HttpWebResponse response = null;
        try
        {
            System.GC.Collect();
            string heads = @"Accept:text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
Accept-Encoding:gzip, deflate
Accept-Language:zh-CN,zh;q=0.8
Connection:keep-alive
Cookie:JSSearchModel=0; LastCity%5Fid=702; LastCity=%e6%b5%8e%e5%8d%97; hiddenEpinDiv=none; bdshare_firstime=1516711737715; dywez=95841923.1517032021.6.5.dywecsr=other|dyweccn=121113803|dywecmd=cnt|dywectr=%E6%99%BA%E8%81%94%E6%8B%9B%E8%81%98; _jzqy=1.1516711211.1517032021.1.jzqsr=baidu|jzqct=%E6%99%BA%E8%81%94%E6%8B%9B%E8%81%98.-; _jzqckmp=1; LastSearchHistory=%7b%22Id%22%3a%22f93c061a-a1e2-4d07-9c87-a50e266dc74f%22%2c%22Name%22%3a%22%e6%b5%8e%e5%8d%97+%2b+%e8%bd%af%e4%bb%b6%2f%e4%ba%92%e8%81%94%e7%bd%91%e5%bc%80%e5%8f%91%2f%e7%b3%bb%e7%bb%9f%e9%9b%86%e6%88%90%22%2c%22SearchUrl%22%3a%22http%3a%2f%2fsou.zhaopin.com%2fjobs%2fsearchresult.ashx%3fbj%3d160000%26jl%3d%25e6%25b5%258e%25e5%258d%2597%26p%3d1%26isadv%3d0%22%2c%22SaveTime%22%3a%22%5c%2fDate(1517032021929%2b0800)%5c%2f%22%7d; dywea=95841923.2981705312848688600.1516711211.1516881176.1517032021.6; dyweb=95841923.13.8.1517032104572; __utma=269921210.1641372652.1516711211.1516881176.1517032021.5; __utmb=269921210.13.9.1517032106314; __utmz=269921210.1517032021.5.4.utmcsr=other|utmccn=121113803|utmcmd=cnt|utmctr=%E6%99%BA%E8%81%94%E6%8B%9B%E8%81%98; Hm_lvt_38ba284938d5eddca645bb5e02a02006=1516881182,1516881186,1517032021,1517032025; _qzja=1.1659610844.1516711252419.1516878513898.1517032113100.1517032118688.1517032149606..0.0.33.4; _qzjb=1.1517032113100.3.0.0.0; _qzjto=3.1.0; _jzqa=1.99071692743118240.1516711211.1516881176.1517032021.6; _jzqb=1.8.10.1517032021.1; urlfrom2=121126445; adfcid2=none; adfbid2=0
Host:jobs.zhaopin.com
Upgrade-Insecure-Requests:1
User-Agent:Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";

          string url = "http://jobs.zhaopin.com/506152584250028.htm";
            //string url = "http://www.dianping.com/";

            HttpWebClient s = new HttpWebClient(true);
            string sw = s.http(url, "POST", heads,"",  Encoding.UTF8);

            //request = BuildRequestObject(new Uri(@"https://www.jianke.com/jibing/gaishu/264713"));
            //request = BuildRequestObject(new Uri(@"http://www.ip181.com/"));
            //response = (HttpWebResponse)request.GetResponse();
            //IWebContentExtractor _extractor = new WebContentExtractor();
            //PageContent pageContent = _extractor.GetContent(response);
            HtmlParser htmlParser = new HtmlParser();
            IHtmlDocument document = htmlParser.Parse(sw);
            JobInfo jobInfo = new JobInfo();
            //获取店名
            var workName = document.QuerySelector(".inner-left h1");
            if (workName != null)
                jobInfo.name = workName.TextContent;
            //获取评级
            var welfare_tab = document.QuerySelectorAll(".welfare-tab-box span");
            jobInfo.mate = "";
            foreach (var item in welfare_tab)
            {
                jobInfo.mate = jobInfo.mate + item.TextContent;
            }

            //获取评论总数
            var reviewCount = document.QuerySelectorAll(".terminalpage-left strong");
            if (reviewCount != null)
            {
                if (reviewCount.Count() > 7)
                {
                    jobInfo.pay = reviewCount[0].TextContent;
                    jobInfo.address = reviewCount[1].TextContent;
                    DateTime pub = DateTime.Now;
                    if (DateTime.TryParse(reviewCount[2].TextContent, out pub)) {
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
            var avgPriceTitle = document.QuerySelector(".tab-inner-cont");
            if (avgPriceTitle != null)
                jobInfo.remark = avgPriceTitle.TextContent;
            //获取综合评分 口味、环境、服务
            var comment_score = document.QuerySelector(".company-name-t a");
            if (comment_score != null)
            {
                jobInfo.company = comment_score.TextContent;

            }
            var company_box = document.QuerySelectorAll(".company-box strong");
            if (company_box != null)
            {
                if (company_box.Count() > 3)
                {
                    jobInfo.companyNum = company_box[0].TextContent;
                    jobInfo.companyType = company_box[1].TextContent;
                    jobInfo.companyTrde = company_box[2].TextContent;
                    if (company_box[3].TextContent.IndexOf("http") != -1) {
                        jobInfo.url = company_box[3].TextContent;
                    }
                }
            }


            //ShopInfo shopInfo = new ShopInfo();
            //DiscussInfo discussInfo = new DiscussInfo();
            //var content = document.QuerySelectorAll(".reviews-tags a");
            //shopInfo.discussKey = "";
            //if (content != null)
            //{
            //    foreach (var item in content)
            //    {
            //        shopInfo.discussKey = shopInfo.discussKey + "," + item.InnerHtml;
            //    }
            //}
            ////获取评论分类数量，差评、中评、好评
            //var count = document.QuerySelectorAll(".filters .count");
            //if (count != null && count.Count() >= 4)
            //{
            //    shopInfo.highOpinion = regNum(count[1].InnerHtml);
            //    shopInfo.middleOpinion = regNum(count[2].InnerHtml);
            //    shopInfo.badOpinion = regNum(count[3].InnerHtml);
            //}

            //var table = document.QuerySelector(".reviews-items").FirstElementChild;
            //var linkDom = table.Children;


            //StringBuilder stringBuilder = new StringBuilder();
            //foreach (var cq in linkDom)
            //{

            //    //获取用户ID
            //    var dper_photo_aside = cq.QuerySelector(".dper-photo-aside");
            //    if (dper_photo_aside != null)
            //        discussInfo.userId = dper_photo_aside.GetAttribute("data-user-id");
            //    //获取用户帐号名
            //    var dper_info = cq.QuerySelector(".dper-info a");
            //    if (dper_info != null)
            //        discussInfo.userName =regTrim( dper_info.InnerHtml);

            //    var review_rank = cq.QuerySelector(".review-rank");
            //    if (review_rank != null)
            //    {
            //        //星级
            //        var span = review_rank.QuerySelector("span");
            //        if (span != null)
            //            discussInfo.level = span.GetAttribute("class");

            //        //获取综合评分 口味、环境、服务
            //        var comment_score = review_rank.QuerySelectorAll(".item");
            //        if (comment_score != null && comment_score.Count() >= 3)
            //        {
            //            discussInfo.taste = regTrim(comment_score[0].InnerHtml);
            //            discussInfo.environment = regTrim(comment_score[1].InnerHtml);
            //            discussInfo.serve = regTrim(comment_score[2].InnerHtml);
            //            if(comment_score.Count()==4)
            //                discussInfo.consume = regNum(comment_score[3].InnerHtml);
            //        }
            //    }
            //    //喜欢的菜
            //    var review_recommend = cq.QuerySelectorAll(".review-recommend a");
            //    discussInfo.favoriteDishes = "";
            //    foreach (var item in review_recommend)
            //    {
            //        discussInfo.favoriteDishes = discussInfo.favoriteDishes + "," + item.InnerHtml;
            //    }
            //}
        }
        catch (Exception ex)
        {
        }
        finally
        {
            if (response != null)
            {
                response.Close();
                response.Dispose();
            }
            request.Abort();
        }
    }
    /// <summary>
    /// 替换掉字符串中的非数字
    /// </summary>
    /// <param name="value"></param>
    private string regNum(string value) {
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
    protected void Button2_Click(object sender, EventArgs e)
    {
        HttpWebRequest request = null;
        HttpWebResponse response = null;
        try
        {
            request = BuildRequestObject(new Uri(@"http://www.ip181.com/"));
            WebProxy webProxy = new WebProxy("112.114.95.156", 8118);
            request.Proxy = webProxy;
            response = (HttpWebResponse)request.GetResponse();
            HttpStatusCode code=response.StatusCode;
        }
        catch (Exception ex)
        {
        }

    }
    protected HttpWebRequest BuildRequestObject(Uri uri)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        request.AllowAutoRedirect = false;
        request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";
        request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        request.Method = "GET";
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        request.Timeout = 5 * 1000;
        return request;
    }
}