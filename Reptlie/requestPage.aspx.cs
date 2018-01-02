﻿using Abot.Core;
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
            request = BuildRequestObject(new Uri(@"http://www.ip181.com/"));
            response = (HttpWebResponse)request.GetResponse();
            IWebContentExtractor _extractor = new WebContentExtractor();
            PageContent pageContent = _extractor.GetContent(response);
            HtmlParser htmlParser = new HtmlParser();
            IHtmlDocument document = htmlParser.Parse(pageContent.Text);
            var table = document.QuerySelector("table");
            var linkDom = table.QuerySelectorAll("tr");
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
                                survibal = 0,
                                usable = true
                            };
                            //_mqsend.send("angentIp", agent.ToString());
                        }
                    }
                }
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