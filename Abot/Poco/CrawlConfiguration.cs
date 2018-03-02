using Abot.Core;
using Abot.Logic;
using Abot.Support;
using System;
using System.Collections.Generic;

namespace Abot.Poco
{
    /// <summary>
    /// Crawl:爬行
    /// 爬虫相关的配置项
    /// </summary>
    [Serializable]
    public class CrawlConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public CrawlConfiguration()
        {
            MaxConcurrentThreads = 10;
            //
            UserAgentString = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
            RobotsDotTextUserAgentString = "abot";
            MaxPagesToCrawl = 1000;
            DownloadableContentTypes = "text/html";
            ConfigurationExtensions = new Dictionary<string, string>();
            MaxRobotsDotTextCrawlDelayInSeconds = 5;
            HttpRequestMaxAutoRedirects = 7;
            IsHttpRequestAutoRedirectsEnabled = true;
            MaxCrawlDepth = 100;
            HttpServicePointConnectionLimit = 200;
            HttpRequestTimeoutInSeconds = 15;
            IsSslCertificateValidationEnabled = true;
            //默认不使用代理
            useAgent = false;
            //默认不使用仿真浏览器
            simulation = false;
        }

        #region crawlBehavior

        /// <summary>
        /// Max concurrent threads to use for http requests
        /// 最大的链接线程并发Http请求数
        /// 默认值为10
        /// </summary>
        public int MaxConcurrentThreads { get; set; }

        /// <summary>
        /// Maximum number of pages to crawl. 
        /// If zero, this setting has no effect
        /// 最大的爬行网页数量。默认10000
        /// 如果为0则无限制
        /// 
        /// </summary>
        public int MaxPagesToCrawl { get; set; }

        /// <summary>
        /// Maximum number of pages to crawl per domain
        /// If zero, this setting has no effect.
        /// 单页面最大的爬页面量，如果为0就没有限制
        /// </summary>
        public int MaxPagesToCrawlPerDomain { get; set; }

        /// <summary>
        /// Maximum size of page. If the page size is above this value, it will not be downloaded or processed
        /// 最大的页面大小，如果查过这个大小，则会下载或者被自动处理掉
        /// If zero, this setting has no effect.
        /// </summary>
        public int MaxPageSizeInBytes { get; set; }

        /// <summary>
        /// The user agent string to use for http requests
        /// User Agent中文名为用户代理，简称 UA，它是一个特殊字符串头
        /// ，使得服务器能够识别客户使用的操作系统及版本、CPU 类型、浏览器及版本、浏览器渲染引擎、浏览器语言、浏览器插件等。
        /// </summary>
        public string UserAgentString { get; set; }

        /// <summary>
        /// Maximum seconds before the crawl times out and stops. 
        /// If zero, this setting has no effect.
        /// 最大超时时间
        /// </summary>
        public int CrawlTimeoutSeconds { get; set; }

        /// <summary>
        /// Dictionary that stores additional keyvalue pairs that can be accessed throught the crawl pipeline
        /// 存储爬行过的URL地址
        /// </summary>
        public Dictionary<string, string> ConfigurationExtensions { get; set; }

        /// <summary>
        /// Whether Uris should be crawled more than once. This is not common and should be false for most scenarios
        /// 设置URI是否需要爬行多次
        /// </summary>
        public bool IsUriRecrawlingEnabled { get; set; }

        /// <summary>
        /// Whether pages external to the root uri should be crawled
        /// 是否可以爬拓展链接，根节点以外的链接
        /// external：扩展
        /// </summary>
        public bool IsExternalPageCrawlingEnabled { get; set; }

        /// <summary>
        /// Whether pages external to the root uri should have their links crawled. NOTE: IsExternalPageCrawlEnabled must be true for this setting to have any effect
        /// 前置条件：IsExternalPageCrawlEnabled 必须为true
        /// 配置扩展链接的子链接是否被爬行
        /// </summary>
        public bool IsExternalPageLinksCrawlingEnabled { get; set; }

        /// <summary>
        /// Whether or not url named anchors or hashbangs are considered part of the url. If false, they will be ignored. If true, they will be considered part of the url.
        /// </summary>
        public bool IsRespectUrlNamedAnchorOrHashbangEnabled { get; set; }

        /// <summary>
        /// A comma seperated string that has content types that should have their page content downloaded.
        /// For each page, the content type is checked to see if it contains any of the values defined here.
        /// 配置包含哪些content-type的页面可以被下载，系统会检查每个页面的content-type，如果包含在这个字符串里面
        /// 则会下载
        /// 多个content-type 需要用,隔开"text/html, text/plain";
        /// </summary>
        public string DownloadableContentTypes { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of concurrent connections allowed by a System.Net.ServicePoint. The system default is 2. This means that only 2 concurrent http connections can be open to the same host.
        /// If zero, this setting has no effect.
        /// </summary>
        public int HttpServicePointConnectionLimit { get; set; }

        /// <summary>
        /// Gets or sets the time-out value in milliseconds for the System.Net.HttpWebRequest.GetResponse() and System.Net.HttpWebRequest.GetRequestStream() methods.
        /// If zero, this setting has no effect.
        /// </summary>
        public int HttpRequestTimeoutInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of redirects that the request follows.
        /// If zero, this setting has no effect.
        /// </summary>
        public int HttpRequestMaxAutoRedirects { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the request should follow redirection
        /// </summary>
        public bool IsHttpRequestAutoRedirectsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates gzip and deflate will be automatically accepted and decompressed
        /// </summary>
        public bool IsHttpRequestAutomaticDecompressionEnabled { get; set; }

        /// <summary>
        /// Whether the cookies should be set and resent with every request
        /// </summary>
        public bool IsSendingCookiesEnabled { get; set; }

        /// <summary>
        /// Whether or not to validate the server SSL certificate. If true, the default validation will be made.
        /// If false, the certificate validation is bypassed. This setting is useful to crawl sites with an
        /// invalid or expired SSL certificate.
        /// </summary>
        public bool IsSslCertificateValidationEnabled { get; set; }

        /// <summary>
        /// Uses closest mulitple of 16 to the value set.
        /// If there is not at least this much memory available before starting a crawl, 
        /// throws InsufficientMemoryException.
        /// If zero, this setting has no effect.
        /// 
        /// </summary>
        /// <exception cref="http://msdn.microsoft.com/en-us/library/system.insufficientmemoryexception.aspx">InsufficientMemoryException</exception>
        public int MinAvailableMemoryRequiredInMb { get; set; }

        /// <summary>
        /// The max amout of memory to allow the process to use. 
        /// If this limit is exceeded the crawler will stop prematurely.
        /// If zero, this setting has no effect.
        /// </summary>
        public int MaxMemoryUsageInMb { get; set; }

        /// <summary>
        /// The max amount of time before refreshing the value used to determine the amount of memory being used by the process that hosts the crawler instance.
        /// This value has no effect if MaxMemoryUsageInMb is zero.
        /// </summary>
        public int MaxMemoryUsageCacheTimeInSeconds { get; set; }

        /// <summary>
        /// Maximum levels below root page to crawl. If value is 0, the homepage will be crawled but none of its links will be crawled. If the level is 1, the homepage and its links will be crawled but none of the links links will be crawled.
        /// </summary>
        public int MaxCrawlDepth { get; set; }

        /// <summary>
        /// Maximum links to crawl per page.
        /// If value is zero, this setting has no effect.
        /// </summary>
        public int MaxLinksPerPage { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the crawler should parse the page's links even if a CrawlDecision (like CrawlDecisionMaker.ShouldCrawlPageLinks()) determines that those links will not be crawled.
        /// </summary>
        public bool IsForcedLinkParsingEnabled { get; set; }

        /// <summary>
        /// The max number of retries for a url if a web exception is encountered. If the value is 0, no retries will be made
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// The minimum delay between a failed http request and the next retry
        /// </summary>
        public int MinRetryDelayInMilliseconds { get; set; }

        #endregion

        #region politeness

        /// <summary>
        /// Whether the crawler should retrieve and respect the robots.txt file.
        /// 爬虫是否应该获取并遵守robots.txt的约定
        /// robots.txt：爬虫协议（网络爬虫排除标准”（Robots Exclusion Protocol），
        /// 网站通过Robots协议告诉搜索引擎哪些页面可以抓取，哪些页面不能抓取。
        /// </summary>
        public bool IsRespectRobotsDotTextEnabled { get; set; }

        /// <summary>
        /// Whether the crawler should ignore links on pages that have a <meta name="robots" content="nofollow" /> tag
        /// </summary>
        public bool IsRespectMetaRobotsNoFollowEnabled { get; set; }

        /// <summary>
        /// Whether the crawler should ignore links on pages that have an http X-Robots-Tag header of nofollow
        /// </summary>
        public bool IsRespectHttpXRobotsTagHeaderNoFollowEnabled { get; set; }

        /// <summary>
        /// Whether the crawler should ignore links that have a <a href="whatever" rel="nofollow">...
        /// </summary>
        public bool IsRespectAnchorRelNoFollowEnabled { get; set; }

        /// <summary>
        /// If true, will ignore the robots.txt file if it disallows crawling the root uri.
        /// </summary>
        public bool IsIgnoreRobotsDotTextIfRootDisallowedEnabled { get; set; }

        /// <summary>
        /// The user agent string to use when checking robots.txt file for specific directives.  Some examples of other crawler's user agent values are "googlebot", "slurp" etc...
        /// </summary>
        public string RobotsDotTextUserAgentString { get; set; }

        /// <summary>
        /// The number of milliseconds to wait in between http requests to the same domain.
        /// 每爬一个页面等多少毫秒
        /// </summary>
        public int MinCrawlDelayPerDomainMilliSeconds { get; set; }

        /// <summary>
        /// The maximum numer of seconds to respect in the robots.txt "Crawl-delay: X" directive. 
        /// IsRespectRobotsDotTextEnabled must be true for this value to be used.
        /// If zero, will use whatever the robots.txt crawl delay requests no matter how high the value is.
        /// </summary>
        public int MaxRobotsDotTextCrawlDelayInSeconds { get; set; }

        #endregion

        #region Authorization

        /// <summary>
        /// Defines whatewer each request shold be autorized via login 
        /// </summary>
        public bool IsAlwaysLogin { get; set; }
        /// <summary>
        /// The user name to be used for autorization 
        /// </summary>
        public string LoginUser { get; set; }
        /// <summary>
        /// The password to be used for autorization 
        /// </summary>
        public string LoginPassword { get; set; }

        #endregion
        #region 扩展参数
        /// <summary>
        /// 代理IP操作类
        /// </summary>
        public AgentHelp agentHelp { get; set; }
        /// <summary>
        /// 请求头，从谷歌浏览器获取即可
        /// </summary>
        public string requestHeader { get; set; }
        /// <summary>
        /// 是否使用代理IP
        /// </summary>
        public bool useAgent { get; set; }
        /// <summary>
        /// 是否使用仿真浏览器。当为false时requestHeader失效
        /// </summary>
        public bool simulation { get; set; }
        /// <summary>
        /// 检查代理是否可用类
        /// </summary>
        public ICheckAgent icheckAgent { get; set; }
        /// <summary>
        /// 线程间隔时间最小值
        /// </summary>
        public int minNeed { set; get; }
        /// <summary>
        /// 线程间隔时间最小值
        /// </summary>
        public int maxNeed { set; get; }
        /// <summary>
        /// 是否记住以及存储缓存
        /// </summary>
        public bool cacheCookie { set; get; }
        #endregion
    }
}
