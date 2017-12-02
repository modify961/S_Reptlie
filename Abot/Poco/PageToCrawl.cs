using System;
using System.Dynamic;

namespace Abot.Poco
{
    /// <summary>
    /// 被爬行的页面信息
    /// </summary>
    [Serializable]
    public class PageToCrawl
    {
        /// <summary>
        ///系列化需要
        /// </summary>
        public PageToCrawl()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uri"></param>
        public PageToCrawl(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            Uri = uri;
            /*
             *ExpandoObject:表示一个对象，该对象包含可在运行时动态添加和移除的成员。
             */
            PageBag = new ExpandoObject();
        }

        /// <summary>
        /// 页面URI
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The parent uri of the page
        /// 父页面的URI
        /// </summary>
        public Uri ParentUri { get; set; }

        /// <summary>
        /// Whether http requests had to be retried more than once. 
        /// This could be due to throttling or politeness.
        /// 当前http请求是否被请多次获取，
        /// </summary>
        public bool IsRetry { get; set; }

        /// <summary>
        /// The time in seconds that the server sent to wait before retrying.
        /// 再请求发送以后多少秒如果没有回复的话，就再次发送请求
        /// </summary>
        public double? RetryAfter { get; set; }

        /// <summary>
        /// The number of times the http request was be retried.
        /// 被重复请求的次数
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// The datetime that the last http request was made. Will be null unless retries are enabled.
        /// 最后一次Http请求发送的时间。有可能为null
        /// </summary>
        public DateTime? LastRequest { get; set; }

        /// <summary>
        /// Whether the page is the root uri of the crawl
        /// 当前页面是不是跟页面
        /// </summary>
        public bool IsRoot { get; set; }

        /// <summary>
        /// Whether the page is internal to the root uri of the crawl
        /// 是否为站内页面
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// The depth from the root of the crawl. If this page is the homepage this value will be zero, if this page was found on the homepage this value will be 1 and so on.
        /// 页面层级。如果是根页面则为0，如果从根页面发现的链接则为1.。。。
        /// </summary>
        public int CrawlDepth { get; set; }

        /// <summary>
        /// Can store values of any type. Useful for adding custom values to the CrawledPage dynamically from event subscriber code
        /// 扩展参数，用来添加自定义动态from event代码
        /// </summary>
        public dynamic PageBag { get; set; }

        /// <summary>
        /// The uri that this page was redirected from. If null then it was not part of the redirect chain
        /// 重定向以后的表单，如果为空表示：
        /// </summary>
        public CrawledPage RedirectedFrom { get; set; }

        /// <summary>
        /// The position in the redirect chain. The first redirect is position 1, the next one is 2 and so on.
        /// 重定向层级
        /// </summary>
        public int RedirectPosition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Uri.AbsoluteUri;
        }
    }
}
