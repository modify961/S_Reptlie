using Abot.Poco;
using log4net;
using System;
using System.CodeDom;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using log4net.Core;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Text.RegularExpressions;

namespace Abot.Core
{
    /// <summary>
    /// 页面请求处理接口
    /// </summary>
    public interface IPageRequester : IDisposable
    {
        /// <summary>
        /// 发送一个请求，并下载页面内容
        /// </summary>
        CrawledPage MakeRequest(Uri uri);

        /// <summary>
        /// Make an http web request to the url and download its content based on the param func decision
        /// 根据自定义处理函数来下载制定的内容
        /// </summary>
        CrawledPage MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent);

    }
    /// <summary>
    /// 页面请求
    /// </summary>
    [Serializable]
    public class PageRequester : IPageRequester
    {
        static ILog _logger = LogManager.GetLogger("AbotLogger");
        /// <summary>
        /// 爬取配置项
        /// </summary>
        protected CrawlConfiguration _config;
        /// <summary>
        /// 
        /// </summary>
        protected IWebContentExtractor _extractor;
        /// <summary>
        /// 
        /// </summary>
        protected CookieContainer _cookieContainer = new CookieContainer();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public PageRequester(CrawlConfiguration config)
            : this(config, null)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="contentExtractor"></param>
        public PageRequester(CrawlConfiguration config, IWebContentExtractor contentExtractor)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _config = config;

            if (_config.HttpServicePointConnectionLimit > 0)
                ServicePointManager.DefaultConnectionLimit = _config.HttpServicePointConnectionLimit;

            if (!_config.IsSslCertificateValidationEnabled)
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true;

            _extractor = contentExtractor ?? new WebContentExtractor();
        }

        /// <summary>
        ///发送请求并下载页面内容
        /// </summary>
        public virtual CrawledPage MakeRequest(Uri uri)
        {
            return MakeRequest(uri, (x) => new CrawlDecision { Allow = true });
        }

        /// <summary>
        /// 发送Http请求并根据shouldDownloadContent来判断是否需要下载内容
        /// uri 站点
        /// shouldDownloadContent：判断是否下载网页内容
        /// </summary>
        public virtual CrawledPage MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            CrawledPage crawledPage = new CrawledPage(uri);

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = BuildRequestObject(uri);
                /******************
                 * 使用代理IP
                 * ****************/
                if (_config.useAgent)
                {
                    Agenter agenter = _config.agentHelp.next(_config.icheckAgent);
                    if (agenter != null)
                    {
                        WebProxy proxy = new WebProxy(agenter.ip, agenter.port);
                        request.Proxy = proxy;
                    }
                }
                /*****************代理结束***********************/
                crawledPage.RequestStarted = DateTime.Now;
                response = (HttpWebResponse)request.GetResponse();
                ProcessResponseObject(response);
            }
            catch (WebException e)
            {
                crawledPage.WebException = e;

                if (e.Response != null)
                    response = (HttpWebResponse)e.Response;

                _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                _logger.Debug(e);
            }
            finally
            {
                try
                {
                    crawledPage.HttpWebRequest = request;
                    crawledPage.RequestCompleted = DateTime.Now;
                    if (response != null)
                    {
                        crawledPage.HttpWebResponse = new HttpWebResponseWrapper(response);
                        CrawlDecision shouldDownloadContentDecision = shouldDownloadContent(crawledPage);
                        if (shouldDownloadContentDecision.Allow)
                        {
                            crawledPage.DownloadContentStarted = DateTime.Now;
                            crawledPage.Content = _extractor.GetContent(response);
                            crawledPage.DownloadContentCompleted = DateTime.Now;
                        }
                        else
                        {
                            _logger.DebugFormat("Links on page [{0}] not crawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldDownloadContentDecision.Reason);
                        }

                        response.Close();//Should already be closed by _extractor but just being safe
                    }
                }
                catch (Exception e)
                {
                    _logger.DebugFormat("Error occurred finalizing requesting url [{0}]", uri.AbsoluteUri);
                    _logger.Debug(e);
                }
            }

            return crawledPage;
        }
        /// <summary>
        /// 配置请求数据
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected virtual HttpWebRequest BuildRequestObject(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = _config.IsHttpRequestAutoRedirectsEnabled;
            request.UserAgent = _config.UserAgentString;
            request.Accept = "*/*";

            if (_config.HttpRequestMaxAutoRedirects > 0)
                request.MaximumAutomaticRedirections = _config.HttpRequestMaxAutoRedirects;

            if (_config.IsHttpRequestAutomaticDecompressionEnabled)
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (_config.HttpRequestTimeoutInSeconds > 0)
                request.Timeout = _config.HttpRequestTimeoutInSeconds * 1000;

            if (_config.IsSendingCookiesEnabled)
                request.CookieContainer = _cookieContainer;
            if (_config.IsAlwaysLogin)
            {
                string credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_config.LoginUser + ":" + _config.LoginPassword));
                request.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            }

            return request;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        protected virtual void ProcessResponseObject(HttpWebResponse response)
        {
            if (response != null && _config.IsSendingCookiesEnabled)
            {
                CookieCollection cookies = response.Cookies;
                _cookieContainer.Add(cookies);
            }
        }
        /// <summary>
        /// 取消站点
        /// </summary>
        public void Dispose()
        {
            if (_extractor != null)
            {
                _extractor.Dispose();
            }
            _cookieContainer = null;
            _config = null;
        }
    }
    /// <summary>
    ///模拟浏览器发送请求，应对网站的反爬虫设置
    /// </summary>
    [Serializable]
    public class ImitateRequester : IPageRequester
    {
        static ILog _logger = LogManager.GetLogger("AbotLogger");
        /// <summary>
        /// 爬取配置项
        /// </summary>
        protected CrawlConfiguration _config;
        /// <summary>
        /// 
        /// </summary>
        protected IWebContentExtractor _extractor;
        /// <summary>
        /// 
        /// </summary>
        protected CookieContainer _cookieContainer = new CookieContainer();
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
        /// 静态构造函数
        /// </summary>
        static ImitateRequester()
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
        /// 
        /// </summary>
        /// <param name="config"></param>
        public ImitateRequester(CrawlConfiguration config)
            : this(config, null)
        {
            cookiedictionary = new Dictionary<string, Cookie>();
            this.trackcookie = _config.cacheCookie;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="contentExtractor"></param>
        public ImitateRequester(CrawlConfiguration config, IWebContentExtractor contentExtractor, bool trackcookies = false)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _config = config;

            if (_config.HttpServicePointConnectionLimit > 0)
                ServicePointManager.DefaultConnectionLimit = _config.HttpServicePointConnectionLimit;

            if (!_config.IsSslCertificateValidationEnabled)
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, certificate, chain, sslPolicyErrors) => true;

            _extractor = contentExtractor ?? new WebContentExtractor();

            cookiedictionary = new Dictionary<string, Cookie>();
            this.trackcookie = trackcookies;
        }

        /// <summary>
        ///发送请求并下载页面内容
        /// </summary>
        public virtual CrawledPage MakeRequest(Uri uri)
        {
            return MakeRequest(uri, (x) => new CrawlDecision { Allow = true });
        }

        /// <summary>
        /// 发送Http请求并根据shouldDownloadContent来判断是否需要下载内容
        /// uri 站点
        /// shouldDownloadContent：判断是否下载网页内容
        /// </summary>
        public virtual CrawledPage MakeRequest(Uri uri, Func<CrawledPage, CrawlDecision> shouldDownloadContent)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            CrawledPage crawledPage = new CrawledPage(uri);


            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                System.GC.Collect();
                request = BuildRequestObject(uri);
                //request = (HttpWebRequest)WebRequest.Create(uri);
                if (!String.IsNullOrEmpty(_config.requestHeader))
                    fillHeaders(request, _config.requestHeader);
                /******************
                 * 使用代理IP
                 * ****************/
                if (_config.useAgent)
                {
                    Agenter agenter = _config.agentHelp.next(_config.icheckAgent);
                    if (agenter != null)
                    {
                        WebProxy proxy = new WebProxy(agenter.ip, agenter.port);
                        request.Proxy = proxy;
                    }
                }
                /*****************代理结束***********************/

                crawledPage.RequestStarted = DateTime.Now;
                response = (HttpWebResponse)request.GetResponse();
                //获取缓存
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

                ProcessResponseObject(response);
            }
            catch (WebException e)
            {
                crawledPage.WebException = e;

                if (e.Response != null)
                    response = (HttpWebResponse)e.Response;

                _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.DebugFormat("Error occurred requesting url [{0}]", uri.AbsoluteUri);
                _logger.Debug(e);
            }
            finally
            {
                try
                {
                    crawledPage.HttpWebRequest = request;
                    crawledPage.RequestCompleted = DateTime.Now;
                    if (response != null)
                    {
                        crawledPage.HttpWebResponse = new HttpWebResponseWrapper(response);
                        CrawlDecision shouldDownloadContentDecision = shouldDownloadContent(crawledPage);
                        if (shouldDownloadContentDecision.Allow)
                        {
                            crawledPage.DownloadContentStarted = DateTime.Now;
                            crawledPage.Content = getResponseBody(response, uri);
                            crawledPage.DownloadContentCompleted = DateTime.Now;
                        }
                        else
                        {
                            _logger.DebugFormat("Links on page [{0}] not crawled, [{1}]", crawledPage.Uri.AbsoluteUri, shouldDownloadContentDecision.Reason);
                        }

                        response.Close();
                    }
                }
                catch (Exception e)
                {
                    _logger.DebugFormat("Error occurred finalizing requesting url [{0}]", uri.AbsoluteUri);
                    _logger.Debug(e);
                }
            }

            return crawledPage;
        }
        /// <summary>
        /// 跟踪cookies
        /// </summary>
        /// <param name="cookies"></param>
        private void trackCookies(CookieCollection cookies)
        {
            if (!trackcookie)
                return;
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
        /// 配置请求数据
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected virtual HttpWebRequest BuildRequestObject(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = _config.IsHttpRequestAutoRedirectsEnabled;
            request.UserAgent = _config.UserAgentString;
            request.Accept = "*/*";

            if (_config.HttpRequestMaxAutoRedirects > 0)
                request.MaximumAutomaticRedirections = _config.HttpRequestMaxAutoRedirects;

            if (_config.IsHttpRequestAutomaticDecompressionEnabled)
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (_config.HttpRequestTimeoutInSeconds > 0)
                request.Timeout = _config.HttpRequestTimeoutInSeconds * 1000;

            if (_config.IsSendingCookiesEnabled)
                request.CookieContainer = _cookieContainer;
            if (_config.IsAlwaysLogin)
            {
                string credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_config.LoginUser + ":" + _config.LoginPassword));
                request.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            }
            request.Timeout = 20 * 1000;
            request.ReadWriteTimeout = 20 * 1000;
            return request;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        protected virtual void ProcessResponseObject(HttpWebResponse response)
        {
            if (response != null && _config.IsSendingCookiesEnabled)
            {
                CookieCollection cookies = response.Cookies;
                _cookieContainer.Add(cookies);
            }
        }
        /// <summary>
        /// 取消站点
        /// </summary>
        public void Dispose()
        {
            if (_extractor != null)
            {
                _extractor.Dispose();
            }
            _cookieContainer = null;
            _config = null;
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
        private PageContent getResponseBody(HttpWebResponse response, Uri uri)
        {
            PageContent pageContent = new PageContent();
            Encoding defaultEncode = Encoding.UTF8;
            pageContent.Charset = "UTF8";
            string contentType = response.ContentType;
            if (contentType != null)
            {
                if (contentType.ToLower().Contains("gb2312"))
                {
                    pageContent.Charset = "GB2312";
                    defaultEncode = Encoding.GetEncoding("gb2312");
                }
                else if (contentType.ToLower().Contains("gbk"))
                {
                    pageContent.Charset = "GBK";
                    defaultEncode = Encoding.GetEncoding("gbk");
                }
                else if (contentType.ToLower().Contains("zh-cn"))
                {
                    pageContent.Charset = "zh-cn";
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
            pageContent.Encoding = defaultEncode;
            pageContent.Text = responseBody;
            Regex _shopregex = new Regex("^http://www.dianping.com/shop/\\d+$", RegexOptions.Compiled);
            //强制增加评论页面
            if (_shopregex.IsMatch(uri.AbsoluteUri)) {
                pageContent.Text = pageContent.Text.Replace("</body>", "<a href='"+uri.AbsoluteUri + "/review_all' /> </body>");
            }
            return pageContent;
        }
    }
}