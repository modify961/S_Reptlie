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
Accept-Encoding:gzip, deflate, br
Accept-Language:zh-CN,zh;q=0.8
Cache-Control:max-age=0
Cookie:Hm_lvt_aeced876570452e65008ee68cd836a07=1516880358; Hm_lpvt_aeced876570452e65008ee68cd836a07=1516880918; bd_uid=c24f33b5-5311-a135-0daf-f23219c6-aaaa; bigeater_AB_index=B; bigeater_session_id=7aa7-050b-2387-4251-57e7-dd97-ec68-4361; a101uname=%E4%B8%AD%E5%9B%BD%E8%AE%BF%E5%AE%A2; a101usercookie=085ab3b9-c0c5-4f75-9b79-264faed42d09; a101uid=085ab3b9-c0c5-4f75-9b79-264faed42d09; a101Isinvite=1; a101sid=7ec7f1da-573a-4b80-9167-82e51f5a4e69; Hm_lvt_ef169fbeadffa8cf7db5f5ace40c6c8f=1516880403; Hm_lpvt_ef169fbeadffa8cf7db5f5ace40c6c8f=1516880403; SourceInof2014=AAEAAAD/////AQAAAAAAAAAMAgAAAD5VdGlsaXR5LCBWZXJzaW9uPTEuMS4xLjEsIEN1bHR1cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbAUBAAAAGVV0aWxpdHkuU291cmNlS2V5V29yZE1vZGUKAAAACl9Tb3VyY2VVcmwJX1Zpc2l0VXJsC19Eb21haW5OYW1lB19Vc2VySXAJX1VzZXJUeXBlDV9TZWFyY2hFbmdpbmUIX0tleVdvcmQFX1N0aWQFX0dHaWQNX0xhc3RNb2RpZmllZAEBAQEAAQEBAQAIDQIAAAAGAwAAABdodHRwczovL3d3dy5qaWFua2UuY29tLwYEAAAAKGh0dHBzOi8vd3d3LmppYW5rZS5jb20vaGVscC9zaXRlbWFwLmh0bWwGBQAAAAsuamlhbmtlLmNvbQYGAAAADDE3Mi4xNi4yLjI0NgEAAAAGBwAAAAAJBwAAAAkHAAAACQcAAAClDdI5K2TViAs%3D; bfd_s=219645570.9552612.1516880403678; tmc=1.219645570.790494.1516880403681.1516880403681.1516880403681; tma=219645570.790494.1516880403681.1516880403681.1516880403681.1; tmd=1.219645570.790494.1516880403681.; bfd_g=acf8c8d6123c46730000bfaa000007d859ef1457; Hm_lvt_ce9691c89e2a82792aea2dcba46ea4f2=1516880358; Hm_lpvt_ce9691c89e2a82792aea2dcba46ea4f2=1516880918; bigeater_session_update_time=1516880918032
Referer:https://www.jianke.com/jibing/gaishu/264648
Upgrade-Insecure-Requests:1
User-Agent:Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36";

          string url = "https://www.jianke.com/jibing/gaishu/264713";
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
            string str = "";
            //获取店名
            var workName = document.QuerySelector(".f16");
            if (workName != null)
                str = workName.InnerHtml.Replace(" 的症状：", "")+";";
            //获取评级
            var welfare_tab = document.QuerySelector(".h1_sm");
            if (welfare_tab != null)
                str = str +  welfare_tab.InnerHtml.Replace("别名：", "") +",";
            str = str + ";";
            var content = document.QuerySelectorAll(".tips_list .clearfix");
            if (content != null)
            {
                foreach (var item in content)
                {
                    if (item.InnerHtml.IndexOf("典型症状") != -1)
                    {
                        var qc = item.QuerySelectorAll("a");
                        foreach (var q in qc) {
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