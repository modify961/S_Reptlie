using Abot.Core;
using Abot.Logic.model;
using Abot.Poco;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using Quartz;
using RabbitMQHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

/// <summary>
/// zhidemai 的摘要说明
/// </summary>
public class Zhidemai : IJob
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void Execute(IJobExecutionContext context)
    {
        try
        {
            string heads = @"Accept:text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
Accept-Encoding:gzip, deflate, br
Accept-Language:zh-CN,zh;q=0.9
Cache-Control:max-age=0
Connection:keep-alive
Cookie:__ckguid=T4b5JVGHKqUQ7vRFBux5kK5; __jsluid=acfe0cb47bfacc18e4fa3f7d0918ed12; Hm_lvt_9b7ac3d38f30fe89ff0b8a0546904e58=1519997475; Hm_lpvt_9b7ac3d38f30fe89ff0b8a0546904e58=1519997475; zdm_qd=%7B%7D; amvid=1a1dce82c45301a158da417add7a5c8e; _ga=GA1.2.296996008.1519997476; _gid=GA1.2.2000214778.1519997476; PHPSESSID=ca5f45cce054f7376f41dd0032e5b707; device_id=7948580061519997476602536d609ed09f99907ccf0bfb5c4c3b27854; _gat_UA-27058866-1=1
Host:www.smzdm.com
Upgrade-Insecure-Requests:1
User-Agent:Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36";

            string url = "https://www.smzdm.com/fenlei/muyingyongpin/h1c4s0f77t0p1/#feed-main";
            HttpWebClient s = new HttpWebClient(true);
            string sw = s.http(url, "POST", heads, "", Encoding.UTF8);
            HtmlParser htmlParser = new HtmlParser();
            IHtmlDocument document = htmlParser.Parse(sw);
            var ul = document.QuerySelector("#feed-main-list");
            var liDom = ul.QuerySelectorAll(".feed-row-wide");
            StringBuilder stringBuilder = new StringBuilder();
            List<ZhidemaiModel> list = new List<ZhidemaiModel>();
            foreach (var cq in liDom)
            {
                ZhidemaiModel zhidemaiModel = new ZhidemaiModel();
                //获取商品发布时间
                var upDate = cq.QuerySelector(".feed-block-extras");
                if (upDate != null)
                {
                    string origin = upDate.TextContent;
                    if (upDate.TextContent.IndexOf("分钟") == -1)
                    {
                        break;
                    }
                    zhidemaiModel.createDate = DateTime.Now;
                }
                //获取商品名称
                var d_title = cq.QuerySelector(".feed-block-title a");
                if (d_title != null)
                {
                    string title = d_title.GetAttribute("onclick");
                    if (!string.IsNullOrEmpty(title))
                    {
                        var titleList = title.Split(',');
                        if (titleList.Length == 4)
                        {
                            zhidemaiModel.remark = titleList[2];
                        }
                    }
                }
                //获取商品的出处
                var from = cq.QuerySelector(".feed-block-extras a");
                if (from != null)
                {
                    zhidemaiModel.origin = from.TextContent;
                }
                //获取价格
                var d_price = cq.QuerySelector(".z-highlight");
                if (d_price != null)
                {
                    zhidemaiModel.price = d_price.TextContent;
                }

                //商品的直达连接
                var gourl = cq.QuerySelector(".feed-link-btn a");
                if (gourl != null)
                {
                    zhidemaiModel.url = gourl.GetAttribute("href");
                }
                var targets = cq.QuerySelectorAll(".feed-block-tags a");
                List<string> target = new List<string>();
                foreach (var item in targets)
                {
                    target.Add(item.TextContent);
                }
                zhidemaiModel.target = string.Join(",", target.ToArray());
                var pic = cq.QuerySelector(".z-feed-img img");
                if (pic != null)
                {
                    var prcUrl = pic.GetAttribute("src");
                    int lastIndex = prcUrl.LastIndexOf('.');
                    string type = prcUrl.Substring(lastIndex + 1, prcUrl.Length - 1 - lastIndex);
                    string guid = Guid.NewGuid().ToString();
                    string picName = string.Format("{0}.{1}", guid, type);
                    string path = "C:\\web\\akidImg\\" + picName;
                    if (downLoadImg(path, prcUrl))
                    {
                        zhidemaiModel.pictiue = picName;
                    }
                }
                list.Add(zhidemaiModel);
            }
            MQSend _mqsend = new MQSend();
            _mqsend.send("zidm", JsonConvert.SerializeObject(list));
        }
        catch (Exception ex)
        {
        }
    }
    /// <summary>
    /// 从图片地址下载图片到本地磁盘
    /// </summary>
    /// <param name="ToLocalPath">图片本地磁盘地址</param>
    /// <param name="Url">图片网址</param>
    /// <returns></returns>
    private bool downLoadImg(string path, string url)
    {
        bool val = false;
        HttpWebRequest request = null;
        WebResponse response = null;
        Stream stream = null;
        try
        {
            request = (HttpWebRequest)WebRequest.Create(url);
            response = request.GetResponse();
            stream = response.GetResponseStream();
            if (!response.ContentType.ToLower().StartsWith("text/"))
            {
                val = saveImg(response, path);

            }
        }
        catch (Exception err)
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
        return val;
    }
    /// <summary>
    ///将二进制文件保存到硬盘
    /// </summary>
    /// <param name="response">The response used to save the file</param>
    // 将二进制文件保存到磁盘
    private bool saveImg(WebResponse response, string FileName)
    {
        bool val = true;
        byte[] buffer = new byte[1024];
        try
        {
            if (File.Exists(FileName))
                File.Delete(FileName);
            Stream outStream = System.IO.File.Create(FileName);
            Stream inStream = response.GetResponseStream();
            int l;
            do
            {
                l = inStream.Read(buffer, 0, buffer.Length);
                if (l > 0)
                    outStream.Write(buffer, 0, l);
            }
            while (l > 0);
            outStream.Close();
            inStream.Close();
        }
        catch
        {
            val = false;
        }
        return val;
    }
}