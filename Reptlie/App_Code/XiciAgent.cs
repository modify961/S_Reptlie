using Abot.Core;
using Abot.Poco;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Quartz;
using RabbitMQHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

/// <summary>
/// XiciAgent 的摘要说明
/// </summary>
public class XiciAgent:IJob
{
    public void Execute(IJobExecutionContext context)
    {
        HttpWebRequest request = null;
        HttpWebResponse response = null;
        try
        {
            System.GC.Collect();
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
                    if (nodeList[5].TextContent.ToLower().IndexOf("http") != -1)
                    {
                        int port = 0;
                        if (int.TryParse(nodeList[2].TextContent, out port))
                        {
                            MQSend _mqsend = new MQSend();
                            _mqsend.send("angentIp", new Agenter()
                            {
                                ip = nodeList[1].TextContent,
                                port = port,
                                anonymous="高匿",
                                type = nodeList[5].TextContent.ToLower(),
                                survibal = 0,
                                usable = true
                            }.ToString());
                        }
                    }
                }
            }
            

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
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    protected HttpWebRequest BuildRequestObject(Uri uri)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        request.AllowAutoRedirect = false;
        request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko"; ;
        request.Accept = "*/*";
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        request.Timeout = 5 * 1000;
        request.KeepAlive = false;
        request.ReadWriteTimeout = 5 * 1000;
        return request;
    }
}