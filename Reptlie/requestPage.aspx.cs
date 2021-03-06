﻿using Abot.Core;
using Abot.Logic.model;
using Abot.Poco;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RabbitMQHelper;
using System;
using System.Collections.Generic;
using System.IO;
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
            string heads = @"Accept:text / html,application / xhtml + xml,application / xml; q = 0.9,image / webp,image / apng,*/*;q=0.8
Accept-Encoding:gzip, deflate, br
Accept-Language:zh-CN,zh;q=0.9
Cache-Control:max-age=0
Connection:keep-alive
Cookie:__ckguid=wpX55KBSWp3feVVF4HRGEb6; __jsluid=c62c82c627cacd4d8c832e85725b1a56; PHPSESSID=c26a856292ca6dbfa3bf2d124b6e58a0; device_id=18646516791519304501291711b882c270003741763b0c3fc6fc0ff579; smzdm_user_view=FD22EEE35F4BF3A5ADD5D3FC3BA06791; smzdm_user_source=D38480FA8E45FC33BEC783F01C3604F7; wt3_sid=%3B999768690672041; wt3_eid=%3B999768690672041%7C2151930462700918218%232151930463700133257; Hm_lvt_9b7ac3d38f30fe89ff0b8a0546904e58=1519304593,1519305201; zdm_qd=%7B%22referrer%22%3A%22https%3A%2F%2Fwww.baidu.com%2Flink%3Furl%3DzCzPI2enuj5YTjqH8gM1Kcbiz4Aatpyf1HTdMHFczPpOb1ahOAy8wsJx8RL4ppgh%26wd%3D%26eqid%3Df3e4899b00080928000000035a8ec189%22%7D; amvid=44d7f4fadc2a059dedf69ffbdb91f2bc; Hm_lpvt_9b7ac3d38f30fe89ff0b8a0546904e58=1519305262
Host:www.smzdm.com
Upgrade-Insecure-Requests:1
User-Agent:Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";

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
                    if (upDate.TextContent.IndexOf("小时") == -1)
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
            Response.Write(JsonConvert.SerializeObject(list));
        }
        catch (Exception ex)
        {
            Response.Write(ex.ToString());
        }
        finally
        {
            if (response != null)
            {
                response.Close();
                response.Dispose();
            }
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
            HttpStatusCode code = response.StatusCode;
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