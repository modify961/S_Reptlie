using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Abot.Core
{
    /// <summary>
    /// 用于检查代理IP和端口是否有效
    /// </summary>
    public static class AgentCheck 
    {
        /// <summary>
        /// 检查IP和端口是否有效
        /// </summary>
        /// <param name="agenter"></param>
        /// <returns>有效为true，无效为 false</returns>
        public static bool agentCheck(Agenter agenter)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                System.GC.Collect();
                request = BuildRequestObject(new Uri(@"http://ip.chinaz.com/"));
                WebProxy proxy = new WebProxy(agenter.ip, agenter.port);
                request.Proxy = proxy;
                response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());//获取应答流
                    string content = reader.ReadToEnd();
                    if (content.IndexOf("search-write-left w442 pr") != -1)
                    {
                        response.Close();
                        response.Dispose();
                        request.Abort();
                        return true;
                    }
                    response.Close();
                    response.Dispose();
                    request.Abort();
                    return false;
                }
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
                request.Abort();
                return false;
            }
            catch (Exception ex)
            {
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                }
                request.Abort();
                return false;
            }
        }
        static HttpWebRequest BuildRequestObject(Uri uri)
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
    
}
