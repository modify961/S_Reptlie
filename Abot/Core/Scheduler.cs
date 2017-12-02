using Abot.Poco;
using System;
using System.Collections.Generic;

namespace Abot.Core
{
    /// <summary>
    /// Handles managing the priority of what pages need to be crawled
    /// 用来处理需要被爬行页面的优先级
    /// </summary>
    public interface IScheduler : IDisposable
    {
        /// <summary>
        /// Count of remaining items that are currently scheduled
        /// 等待爬行的数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Schedules the param to be crawled
        /// 添加一个待处理的页面
        /// </summary>
        void Add(PageToCrawl page);

        /// <summary>
        /// Schedules the param to be crawled
        /// 添加一列需要处理的页面
        /// </summary>
        void Add(IEnumerable<PageToCrawl> pages);

        /// <summary>
        /// Gets the next page to crawl
        /// 获取下一个待处理的链接
        /// </summary>
        PageToCrawl GetNext();

        /// <summary>
        /// Clear all currently scheduled pages
        /// </summary>
        void Clear();

        /// <summary>
        /// Add the Url to the list of crawled Url without scheduling it to be crawled.
        /// </summary>
        /// <param name="uri"></param>
        void AddKnownUri(Uri uri);

        /// <summary>
        /// Returns whether or not the specified Uri was already scheduled to be crawled or simply added to the
        /// list of known Uris.
        /// </summary>
        bool IsUriKnown(Uri uri);
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Scheduler : IScheduler
    {
        ICrawledUrlRepository _crawledUrlRepo;
        IPagesToCrawlRepository _pagesToCrawlRepo;
        bool _allowUriRecrawling;
        /// <summary>
        /// 
        /// </summary>
        public Scheduler()
            :this(false, null, null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowUriRecrawling"></param>
        /// <param name="crawledUrlRepo"></param>
        /// <param name="pagesToCrawlRepo"></param>
        public Scheduler(bool allowUriRecrawling, ICrawledUrlRepository crawledUrlRepo, IPagesToCrawlRepository pagesToCrawlRepo)
        {
            _allowUriRecrawling = allowUriRecrawling;
            _crawledUrlRepo = crawledUrlRepo ?? new CompactCrawledUrlRepository();
            _pagesToCrawlRepo = pagesToCrawlRepo ?? new FifoPagesToCrawlRepository();
        }
        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get { return _pagesToCrawlRepo.Count(); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        public void Add(PageToCrawl page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            if (_allowUriRecrawling || page.IsRetry)
            {
                _pagesToCrawlRepo.Add(page);
            }
            else
            {
                if (_crawledUrlRepo.AddIfNew(page.Uri))
                    _pagesToCrawlRepo.Add(page);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pages"></param>
        public void Add(IEnumerable<PageToCrawl> pages)
        {
            if (pages == null)
                throw new ArgumentNullException("pages");

            foreach (PageToCrawl page in pages)
                Add(page);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PageToCrawl GetNext()
        {
            return _pagesToCrawlRepo.GetNext();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            _pagesToCrawlRepo.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        public void AddKnownUri(Uri uri)
        {
            _crawledUrlRepo.AddIfNew(uri);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public bool IsUriKnown(Uri uri)
        {
            return _crawledUrlRepo.Contains(uri);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_crawledUrlRepo != null)
            {
                _crawledUrlRepo.Dispose();
            }
            if (_pagesToCrawlRepo != null)
            {
                _pagesToCrawlRepo.Dispose();
            }
        }
    }
}
