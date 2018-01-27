using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abot.Logic.model
{
    /// <summary>
    /// 评论信息
    /// </summary>
    [Serializable]
    public class DiscussInfo
    {
        /// <summary>
        /// 评论者帐号名称
        /// </summary>
        public string userName { set; get; }
        /// <summary>
        /// 评论者帐号名称
        /// </summary>
        public string userId { set; get; }
        /// <summary>
        /// 评级
        /// </summary>
        public string level { set; get; }
        /// <summary>
        /// 口味描述
        /// </summary>
        public string taste { set; get; }
        /// <summary>
        /// 环境描述
        /// </summary>
        public string environment { set; get; }
        /// <summary>
        /// 服务描述
        /// </summary>
        public string serve { set; get; }
        /// <summary>
        /// 人均消费
        /// </summary>
        public string consume { set; get; }
        /// <summary>
        /// 喜欢的菜
        /// </summary>
        public string favoriteDishes  { set; get; }
        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
