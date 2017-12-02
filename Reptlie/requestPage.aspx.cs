using Abot.Core;
using Abot.Poco;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class requestPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        HttpWebRequest request = null;
        HttpWebResponse response = null;
        try
        {
            request = BuildRequestObject(new Uri(@"http://www.xicidaili.com/nn/1"));
            response = (HttpWebResponse)request.GetResponse();
            IWebContentExtractor _extractor = new WebContentExtractor();
            PageContent pageContent = _extractor.GetContent(response);
            HtmlParser htmlParser = new HtmlParser();
            IHtmlDocument document = htmlParser.Parse(pageContent.Text);
            var table = document.QuerySelector("#ip_list");
            var linkDom = table.QuerySelectorAll(".odd");
            StringBuilder stringBuilder = new StringBuilder();
            String date = "";
            foreach (var cq in linkDom)
            {
                IHtmlCollection<IElement> nodeList = cq.QuerySelectorAll("td");
                if (nodeList.Length > 9)
                {
                    stringBuilder.Clear();
                    date = "";
                    stringBuilder.Append(nodeList[1].TextContent);
                    stringBuilder.Append(",");
                    stringBuilder.Append(nodeList[2].TextContent);
                    stringBuilder.Append(",");
                    stringBuilder.Append(nodeList[5].TextContent);
                    stringBuilder.Append(",");
                    stringBuilder.Append(nodeList[8].TextContent);
                    stringBuilder.Append(",");
                    date = nodeList[8].TextContent;
                    int dateOfNum = 0;
                    if (date != null && date.IndexOf("天") != -1)
                    {
                        int.TryParse(date.Replace("天", ""), out dateOfNum);
                        date = (dateOfNum * 24 * 60).ToString();
                    }
                    else if (date != null && date.IndexOf("小时") != -1)
                    {
                        int.TryParse(date.Replace("小时", ""), out dateOfNum);
                        date = (dateOfNum * 60).ToString();
                    }
                    else if (date != null && date.IndexOf("分钟") != -1)
                    {
                        date = date.Replace("分钟", "");
                    }
                    stringBuilder.Append(date);
                    stringBuilder.Append("\r\n");
                    System.IO.File.AppendAllText("D:\\fake.txt", stringBuilder.ToString());
                }
            }
        }
        catch (Exception ex)
        {
        }

    }
    protected HttpWebRequest BuildRequestObject(Uri uri)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        request.AllowAutoRedirect = false;
        request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko"; ;
        request.Accept = "*/*";
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        request.Timeout = 60 * 1000;
        return request;
    }
}