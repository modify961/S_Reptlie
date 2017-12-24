using Abot.Core;
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
            for (int i = 1; i <= 10; i++)
            {
                request = BuildRequestObject(new Uri(@"http://www.xicidaili.com/nn/" + i.ToString()));
                WebProxy proxy = new WebProxy("113.122.14.225", 808);
                request.Proxy = proxy;
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
                        if (nodeList[5].TextContent.ToLower().IndexOf("http") != -1)
                        {
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
                            int port = 0;
                            if (int.TryParse(nodeList[2].TextContent, out port))
                            {
                                //AgentSingleton.getInstance.add();
                                MQSend _mqsend = new MQSend();
                                _mqsend.send("angentIp", new Agenter()
                                {
                                    ip = nodeList[1].TextContent,
                                    port = port,
                                    type = nodeList[5].TextContent.ToLower(),
                                    survibal = dateOfNum,
                                    usable = true
                                }.ToString());
                            }
                        }
                    }
                }
                int count = AgentSingleton.getInstance.count();
            }

        }
        catch (Exception ex)
        {
        }

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
        request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko"; ;
        request.Accept = "*/*";
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        request.Timeout = 5 * 1000;
        return request;
    }
}