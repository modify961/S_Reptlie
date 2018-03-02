using Abot.Logic.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abot.Logic
{
    /// <summary>
    /// 上下文
    /// </summary>
    public class AbotContext
    {
        /// <summary>
        /// 类型
        /// </summary>
        public AbotTypeEnum abotTypeEnum { set; get; }
        /// <summary>
        /// 初始URL
        /// </summary>
        public string rootUrl { set; get; }
        /// <summary>
        /// 线程数
        /// 0表示使用核心数作为线程数
        /// </summary>
        public int threadNum { set; get; }
        /// <summary>
        /// 是否使用代理
        /// </summary>
        public bool useAgent { set; get; }
        /// <summary>
        /// 是否仿真
        /// </summary>
        public bool simulation { set; get; }
        /// <summary>
        /// 仿真表头
        /// </summary>
        public string requestHeader { set; get; }
        /// <summary>
        /// 帐号
        /// </summary>
        public string account { set; get; }
        /// <summary>
        /// 密码
        /// </summary>
        public string password { set; get; }
        /// <summary>
        /// 线程间隔时间最小值
        /// </summary>
        public int minNeed { set; get; }
        /// <summary>
        /// 线程间隔时间最小值
        /// </summary>
        public int maxNeed { set; get; }
        /// <summary>
        /// 是否记住以及使用缓存
        /// </summary>
        public bool cacheCookie { set; get; }
    }
}
