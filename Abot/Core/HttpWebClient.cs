using Abot.Logic.check;
using Abot.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Abot.Core
{
    /// <summary>
    /// 模拟浏览器请求站点，
    /// </summary>
    public  class HttpWebClient
    {
        /// <summary>
        /// 静态变量。存储表头
        /// </summary>
        private static List<string> _headers = null;
        /// <summary>
        /// 是否跟踪cookies
        /// </summary>
        private bool trackcookie;
        /// <summary>
        /// cookies 字典,
        /// </summary>
        private Dictionary<String, Cookie> cookiedictionary;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="trackcookies">是否跟踪缓存，默认为false</param>
        public HttpWebClient(bool trackcookies = false)
        {
            cookiedictionary = new Dictionary<string, Cookie>();
            this.trackcookie = trackcookies;
        }
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static HttpWebClient()
        {
            _headers = new List<string>();
            _headers.Add("Host");
            _headers.Add("Connection");
            _headers.Add("User-Agent");
            _headers.Add("Referer");
            _headers.Add("Range");
            _headers.Add("Content-Type");
            _headers.Add("Content-Length");
            _headers.Add("Expect");
            _headers.Add("Proxy-Connection");
            _headers.Add("If-Modified-Since");
            _headers.Add("Keep-alive");
            _headers.Add("Accept");
            ServicePointManager.DefaultConnectionLimit = 1000;//最大连接数
        }
        /// <summary>
        /// http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method">POST,GET</param>
        /// <param name="headers">http的头部,直接拷贝谷歌请求的头部即可</param>
        /// <param name="content">content,每个key,value 都要UrlEncode才行</param>
        /// <param name="contentEncode">content的编码</param>
        /// <returns></returns>
        public string http(string url, string method, string headers, string content, Encoding contentEncode)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            
            //request.Method = method;
            if (method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
            {
                request.MaximumAutomaticRedirections = 100;
                request.AllowAutoRedirect = false;
            }

            fillHeaders(request, headers);
            /******************
            * 使用代理IP
            * ****************/
            //AgentHelp agentHelp = new AgentHelp();
            //Agenter agenter = agentHelp.next(new ZhiLianCheck());
            //if (agenter != null)
            //{
            //    WebProxy proxy = new WebProxy(agenter.ip, agenter.port);
            //    request.Proxy = proxy;
            //}
            /*****************代理结束***********************/
            #region 添加Post 参数  
            if (contentEncode == null)
            {
                contentEncode = Encoding.UTF8;
            }
            if (!string.IsNullOrWhiteSpace(content))
            {
                byte[] data = contentEncode.GetBytes(content);
                request.ContentLength = data.Length;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
            }
            #endregion

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                CookieCollection cc = new CookieCollection();
                string cookieString = response.Headers[HttpResponseHeader.SetCookie];
                if (!string.IsNullOrWhiteSpace(cookieString))
                {
                    var spilit = cookieString.Split(';');
                    foreach (string item in spilit)
                    {
                        var kv = item.Split('=');
                        if (kv.Length == 2)
                            cc.Add(new Cookie(kv[0].Trim(), kv[1].Trim()));
                    }
                }
                trackCookies(cc);
            }
            catch (Exception ex)
            {
                return "";
            }

            string result = getResponseBody(response);
            return result;
        }
        /// <summary>
        /// 跟踪cookies
        /// </summary>
        /// <param name="cookies"></param>
        private void trackCookies(CookieCollection cookies)
        {
            if (!trackcookie) return;
            if (cookies == null) return;
            foreach (Cookie c in cookies)
            {
                if (cookiedictionary.ContainsKey(c.Name))
                {
                    cookiedictionary[c.Name] = c;
                }
                else
                {
                    cookiedictionary.Add(c.Name, c);
                }
            }

        }

        /// <summary>
        /// 格式cookies
        /// </summary>
        /// <param name="cookies"></param>
        private string getCookieStr()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, Cookie> item in cookiedictionary)
            {
                if (!item.Value.Expired)
                {
                    if (sb.Length == 0)
                    {
                        sb.Append(item.Key).Append("=").Append(item.Value.Value);
                    }
                    else
                    {
                        sb.Append("; ").Append(item.Key).Append(" = ").Append(item.Value.Value);
                    }
                }
            }
            return sb.ToString();

        }

        /// <summary>
        /// 填充头
        /// </summary>
        /// <param name="request"></param>
        /// <param name="headers"></param>
        private void fillHeaders(HttpWebRequest request, string headers, bool isPrint = false)
        {
            if (request == null) return;
            if (string.IsNullOrWhiteSpace(headers)) return;
            string[] hsplit = headers.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in hsplit)
            {
                string[] kv = item.Split(':');
                string key = kv[0].Trim();
                string value = string.Join(":", kv.Skip(1)).Trim();
                if (!_headers.Contains(key))
                {
                    request.Headers.Add(key, value);
                }
                else
                {
                    #region  设置http头
                    switch (key)
                    {

                        case "Accept":
                            {
                                request.Accept = value;
                                break;
                            }
                        case "Host":
                            {
                                request.Host = value;
                                break;
                            }
                        case "Connection":
                            {
                                request.KeepAlive = (value == "keep-alive" ? true : false);
                                break;
                            }
                        case "Content-Type":
                            {
                                request.ContentType = value;
                                break;
                            }

                        case "User-Agent":
                            {
                                request.UserAgent = value;
                                break;
                            }
                        case "Referer":
                            {
                                request.Referer = value;
                                break;
                            }

                        case "Content-Length":
                            {
                                request.ContentLength = Convert.ToInt64(value);
                                break;
                            }
                        case "Expect":
                            {
                                request.Expect = value;
                                break;
                            }
                        case "If-Modified-Since":
                            {
                                request.IfModifiedSince = Convert.ToDateTime(value);
                                break;
                            }
                        default:
                            break;
                    }
                    #endregion
                }
            }
            CookieCollection cc = new CookieCollection();
            string cookieString = request.Headers[HttpRequestHeader.Cookie];
            if (!string.IsNullOrWhiteSpace(cookieString))
            {
                var spilit = cookieString.Split(';');
                foreach (string item in spilit)
                {
                    var kv = item.Split('=');
                    if (kv.Length == 2)
                        cc.Add(new Cookie(kv[0].Trim(), kv[1].Trim()));
                }
            }
            trackCookies(cc);
            if (!trackcookie)
            {
                request.Headers[HttpRequestHeader.Cookie] = "";
            }
            else
            {
                request.Headers[HttpRequestHeader.Cookie] = getCookieStr();
            }

        }
        /// <summary>
        /// 返回body内容
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string getResponseBody(HttpWebResponse response)
        {
            Encoding defaultEncode = Encoding.UTF8;
            string contentType = response.ContentType;
            if (contentType != null)
            {
                if (contentType.ToLower().Contains("gb2312"))
                {
                    defaultEncode = Encoding.GetEncoding("gb2312");
                }
                else if (contentType.ToLower().Contains("gbk"))
                {
                    defaultEncode = Encoding.GetEncoding("gbk");
                }
                else if (contentType.ToLower().Contains("zh-cn"))
                {
                    defaultEncode = Encoding.GetEncoding("zh-cn");
                }
            }

            string responseBody = string.Empty;
            if (response.ContentEncoding.ToLower().Contains("gzip"))
            {
                using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        responseBody = reader.ReadToEnd();
                    }
                }
            }
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
            {
                using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                {
                    using (StreamReader reader = new StreamReader(stream, defaultEncode))
                    {
                        responseBody = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, defaultEncode))
                    {
                        responseBody = reader.ReadToEnd();
                    }
                }
            }
            return responseBody;
        }
       
    }
}
