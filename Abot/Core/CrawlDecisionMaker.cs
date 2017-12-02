using System;
using System.Collections.Generic;
using System.Linq;
using Abot.Poco;
using System.Net;

namespace Abot.Core
{
    /// <summary>
    /// Determines what pages should be crawled, whether the raw content should be downloaded and if the links on a page should be crawled
    /// 用于判断页面是否会被爬行，以及是否获取页面原生内容和抓取页面内的链接
    /// </summary>
    public interface ICrawlDecisionMaker
    {
        /// <summary>
        /// Decides whether the page should be crawled
        /// 判断是否爬取页面
        /// </summary>
        CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext);

        /// <summary>
        /// Decides whether the page's links should be crawled
        /// 判断页面内的链接是否需要被爬行
        /// </summary>
        CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext);

        /// <summary>
        /// Decides whether the page's content should be dowloaded
        /// 判断页面的内容是否需要被下载
        /// </summary>
        CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext);

        /// <summary>
        /// Decides whether the page should be re-crawled
        /// 判断页面是否需要被重新爬取
        /// </summary>
        CrawlDecision ShouldRecrawlPage(CrawledPage crawledPage, CrawlContext crawlContext);
    }
    /// <summary>
    /// 用于判断页面是否会被爬行，以及是否获取页面原生内容和抓取页面内的链接
    /// </summary>
    [Serializable]
    public class CrawlDecisionMaker : ICrawlDecisionMaker
    {
        /// <summary>
        /// 判断页面是否爬取页面
        /// </summary>
        /// <param name="pageToCrawl"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public virtual CrawlDecision ShouldCrawlPage(PageToCrawl pageToCrawl, CrawlContext crawlContext)
        {
            if(pageToCrawl == null)
                return new CrawlDecision { Allow = false, Reason = "Null page to crawl" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };

            if (pageToCrawl.RedirectedFrom != null && pageToCrawl.RedirectPosition > crawlContext.CrawlConfiguration.HttpRequestMaxAutoRedirects)
                return new CrawlDecision { Allow = false, Reason = string.Format("HttpRequestMaxAutoRedirects limit of [{0}] has been reached", crawlContext.CrawlConfiguration.HttpRequestMaxAutoRedirects) };

            if(pageToCrawl.CrawlDepth > crawlContext.CrawlConfiguration.MaxCrawlDepth)
                return new CrawlDecision { Allow = false, Reason = "Crawl depth is above max" };

            if (!pageToCrawl.Uri.Scheme.StartsWith("http"))
                return new CrawlDecision { Allow = false, Reason = "Scheme does not begin with http" };

            //TODO Do we want to ignore redirect chains (ie.. do not treat them as seperate page crawls)?
            if (!pageToCrawl.IsRetry &&
                crawlContext.CrawlConfiguration.MaxPagesToCrawl > 0 &&
                crawlContext.CrawledCount + crawlContext.Scheduler.Count + 1 > crawlContext.CrawlConfiguration.MaxPagesToCrawl)
            {
                return new CrawlDecision { Allow = false, Reason = string.Format("MaxPagesToCrawl limit of [{0}] has been reached", crawlContext.CrawlConfiguration.MaxPagesToCrawl) };
            }

            int pagesCrawledInThisDomain = 0;
            if (!pageToCrawl.IsRetry &&
                crawlContext.CrawlConfiguration.MaxPagesToCrawlPerDomain > 0 &&
                crawlContext.CrawlCountByDomain.TryGetValue(pageToCrawl.Uri.Authority, out pagesCrawledInThisDomain) &&
                pagesCrawledInThisDomain > 0)
            {
                if (pagesCrawledInThisDomain >= crawlContext.CrawlConfiguration.MaxPagesToCrawlPerDomain)
                    return new CrawlDecision { Allow = false, Reason = string.Format("MaxPagesToCrawlPerDomain limit of [{0}] has been reached for domain [{1}]", crawlContext.CrawlConfiguration.MaxPagesToCrawlPerDomain, pageToCrawl.Uri.Authority) };
            }

            if(!crawlContext.CrawlConfiguration.IsExternalPageCrawlingEnabled && !pageToCrawl.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "Link is external" };

            return new CrawlDecision { Allow = true };
        }
        /// <summary>
        /// 判断页面内的链接是否需要被爬行
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public virtual CrawlDecision ShouldCrawlPageLinks(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision{Allow = false, Reason = "Null crawled page"};

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };

            if(string.IsNullOrWhiteSpace(crawledPage.Content.Text))
                return new CrawlDecision { Allow = false, Reason = "Page has no content" };

            if (!crawlContext.CrawlConfiguration.IsExternalPageLinksCrawlingEnabled && !crawledPage.IsInternal)
                return new CrawlDecision { Allow = false, Reason = "Link is external" };

            if (crawledPage.CrawlDepth >= crawlContext.CrawlConfiguration.MaxCrawlDepth)
                return new CrawlDecision { Allow = false, Reason = "Crawl depth is above max" };

            return new CrawlDecision{Allow = true};
        }
        /// <summary>
        /// 判断页面的内容是否需要被下载
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public virtual CrawlDecision ShouldDownloadPageContent(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawled page" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };            

            if (crawledPage.HttpWebResponse == null)
                return new CrawlDecision { Allow = false, Reason = "Null HttpWebResponse" };
            
            if (crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                return new CrawlDecision { Allow = false, Reason = "HttpStatusCode is not 200" };
            
            string pageContentType = crawledPage.HttpWebResponse.ContentType.ToLower().Trim();
            bool isDownloadable = false;
            List<string> cleanDownloadableContentTypes = crawlContext.CrawlConfiguration.DownloadableContentTypes
                .Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            foreach (string downloadableContentType in cleanDownloadableContentTypes)
            {
                if (pageContentType.Contains(downloadableContentType.ToLower().Trim()))
                {
                    isDownloadable = true;
                    break;
                }
            }
            if (!isDownloadable)
                return new CrawlDecision { Allow = false, Reason = "Content type is not any of the following: " + string.Join(",", cleanDownloadableContentTypes) };

            if (crawlContext.CrawlConfiguration.MaxPageSizeInBytes > 0 && crawledPage.HttpWebResponse.ContentLength > crawlContext.CrawlConfiguration.MaxPageSizeInBytes)
                return new CrawlDecision { Allow = false, Reason = string.Format("Page size of [{0}] bytes is above the max allowable of [{1}] bytes", crawledPage.HttpWebResponse.ContentLength, crawlContext.CrawlConfiguration.MaxPageSizeInBytes) };

            return new CrawlDecision { Allow = true };            
        }
        /// <summary>
        /// 判断页面是否需要被重新爬取
        /// </summary>
        /// <param name="crawledPage"></param>
        /// <param name="crawlContext"></param>
        /// <returns></returns>
        public virtual CrawlDecision ShouldRecrawlPage(CrawledPage crawledPage, CrawlContext crawlContext)
        {
            if (crawledPage == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawled page" };

            if (crawlContext == null)
                return new CrawlDecision { Allow = false, Reason = "Null crawl context" };

            if (crawledPage.WebException == null)
                return new CrawlDecision { Allow = false, Reason = "WebException did not occur"};
           
            if (crawlContext.CrawlConfiguration.MaxRetryCount < 1)
                return new CrawlDecision { Allow = false, Reason = "MaxRetryCount is less than 1"};

            if (crawledPage.RetryCount >= crawlContext.CrawlConfiguration.MaxRetryCount)
                return new CrawlDecision {Allow = false, Reason = "MaxRetryCount has been reached"};

            return new CrawlDecision { Allow = true };
        }
    }
}
