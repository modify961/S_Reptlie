using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abot.Logic.model
{
    /// <summary>
    /// 什么值得买实体类
    /// </summary>
    [Serializable]
    public class ZhidemaiModel
    {
        /// <summary>
        /// 品牌
        /// </summary>
        public string brand { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public string price { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string origin { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string target { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string pictiue { get; set; }
        /// <summary>
        /// 直达URL
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createDate { get; set; }
    }
}
