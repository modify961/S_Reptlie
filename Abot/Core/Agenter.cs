using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abot.Core
{
    /// <summary>
    /// 存放代理类
    /// </summary>
    [Serializable]
    public class Agenter
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public String ip { set; get; }
        /// <summary>
        /// 端口号
        /// </summary>
        public int port { set; get; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public String type { set; get; }
        /// <summary>
        /// 存活时间（分钟）
        /// </summary>
        public int survibal { set; get; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool usable { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ip + "," + this.port+","+ this.type;
        }
    }
}
