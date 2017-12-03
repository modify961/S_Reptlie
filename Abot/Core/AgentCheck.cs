using System;
using System.Collections.Generic;
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
                request = BuildRequestObject(new Uri(@"http://www.dianping.com/jinan/food"));
                WebProxy proxy = new WebProxy(agenter.ip, agenter.port);
                request.Proxy = proxy;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
                return true;
            }
            catch (Exception ex)
            {
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
            return request;
        }
    }
    
}
