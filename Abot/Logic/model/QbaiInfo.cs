using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abot.Logic.model
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class QbaiInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string info { get; set; }
        /// <summary>
        /// 好笑数
        /// </summary>
        public int silmeNum { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string froms { get; set; }
    }
}
