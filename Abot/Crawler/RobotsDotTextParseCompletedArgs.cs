using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abot.Poco;
using Robots;

namespace Abot.Crawler
{
    /// <summary>
    /// Class which hold robot txt data after successful parsing
    /// 处理完Robots.txt后调用的自定义处理事件
    /// </summary>
    public class RobotsDotTextParseCompletedArgs : CrawlArgs
    {
        /// <summary>
        /// Robots.txt 实体
        /// </summary>
        public IRobots Robots { get; set; }

        /// <summary>
        /// Contructor to be used to create an object which will path arugments when robots txt is parsed
        /// 将解析好的Robots.txt转换为实体对象
        /// </summary>
        /// <param name="crawlContext"></param>
        /// <param name="robots"></param>
        public RobotsDotTextParseCompletedArgs(CrawlContext crawlContext, IRobots robots) : base(crawlContext)
        {
            Robots = robots;
        }
    }
}
