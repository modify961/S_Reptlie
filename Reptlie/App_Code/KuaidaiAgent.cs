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
/// KuaidaiAgent 的摘要说明
/// </summary>
public class KuaidaiAgent : IJob
{
    public void Execute(IJobExecutionContext context)
    {
        HttpWebRequest request = null;
        HttpWebResponse response = null;
        try
        {
            System.GC.Collect();
            request = BuildRequestObject(new Uri(@"https://www.kuaidaili.com/free/inha/"));
            response = (HttpWebResponse)request.GetResponse();
            IWebContentExtractor _extractor = new WebContentExtractor();
            PageContent pageContent = _extractor.GetContent(response);
            HtmlParser htmlParser = new HtmlParser();
            IHtmlDocument document = htmlParser.Parse(pageContent.Text);
            var table = document.QuerySelector("#list");
            var linkDom = table.QuerySelectorAll("tbody tr");
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var cq in linkDom)
            {
                IHtmlCollection<IElement> nodeList = cq.QuerySelectorAll("td");
                if (nodeList.Length > 5)
                {
                    if (nodeList[3].TextContent.ToLower().IndexOf("http") != -1)
                    {
                        int port = 0;
                        if (int.TryParse(nodeList[1].TextContent, out port))
                        {
                            MQSend _mqsend = new MQSend();
                            Agenter agent = new Agenter()
                            {
                                ip = nodeList[0].TextContent,
                                port = port,
                                type = nodeList[3].TextContent.ToLower(),
                                anonymous = "高匿",
                                survibal = 0,
                                usable = true
                            };
                            _mqsend.send("angentIp", agent.ToString());
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