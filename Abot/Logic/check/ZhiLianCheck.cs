using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abot.Core;
using System.Net;
using System.IO;

namespace Abot.Logic.check
{
    /// <summary>
    /// 判断智联招聘的代理能不能用
    /// </summary>
    public class ZhiLianCheck : ICheckAgent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="agenter"></param>
        /// <returns></returns>
        public bool agentCheck(Agenter agenter)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                System.GC.Collect();
                request = BuildRequestObject(new Uri(@"https://www.zhaopin.com/"));
                WebProxy proxy = new WebProxy(agenter.ip, agenter.port);
                request.Proxy = proxy;
                response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());//获取应答流
                    string content = reader.ReadToEnd();
                    if (content.IndexOf("招聘_求职_找工作_上智联招聘人才网") != -1)
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
            request.Timeout = 20 * 1000;
            request.KeepAlive = false;
            request.ReadWriteTimeout = 20 * 1000;
            return request;
        }
    }
}
