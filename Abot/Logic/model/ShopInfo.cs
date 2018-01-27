using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abot.Logic.model
{
    /// <summary>
    /// 大众点评店家信息
    /// </summary>
    [Serializable]
    public class ShopInfo
    {
        /// <summary>
        /// 店家ID
        /// </summary>
        public string shopId { set; get; }
        /// <summary>
        /// 店家名称
        /// </summary>
        public string shopName { set; get; }
        /// <summary>
        /// 店家地址
        /// </summary>
        public string shopAddress { set; get; }
        /// <summary>
        /// 电话
        /// </summary>
        public string shopTel { set; get; }
        /// <summary>
        /// 店家评级
        /// </summary>
        public string shopGrade { set; get; }
        /// <summary>
        /// 店家人均消费
        /// </summary>
        public string shopConsume { set; get; }
        /// <summary>
        /// 口味评分
        /// </summary>
        public string shopTaste { set; get; }
        /// <summary>
        /// 环境评分
        /// </summary>
        public string shopEnvironment { set; get; }
        /// <summary>
        /// 服务评分
        /// </summary>
        public string shopServe { set; get; }
        /// <summary>
        /// 评论数
        /// </summary>
        public string shopDiscuss { set; get; }
        /// <summary>
        /// 好评数量
        /// </summary>
        public string highOpinion { set; get; }
        /// <summary>
        /// 中评数量
        /// </summary>
        public string middleOpinion { set; get; }
        /// <summary>
        /// 差评数量
        /// </summary>
        public string badOpinion { set; get; }
        /// <summary>
        /// 评语
        /// </summary>
        public string discussKey { set; get; }
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
