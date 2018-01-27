using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abot.Logic.model
{
    /// <summary>
    /// 职位信息
    /// </summary>
    [Serializable]
    public class JobInfo
    {
        /// <summary>
        /// 职位名称
        /// </summary>
        public string name { set; get; }
        /// <summary>
        /// 职位福利
        /// </summary>
        public string mate { set; get; }
        /// <summary>
        /// 公司
        /// </summary>
        public string company { set; get; }
        /// <summary>
        /// 工资
        /// </summary>
        public string pay { set; get; }
        /// <summary>
        /// 地点
        /// </summary>
        public string address { set; get; }
        /// <summary>
        /// 发布日期
        /// </summary>
        public string publicDate { set; get; }
        /// <summary>
        /// 工作性质
        /// </summary>
        public string workType { set; get; }
        /// <summary>
        /// 经验要求
        /// </summary>
        public string expe { set; get; }
        /// <summary>
        /// 学历
        /// </summary>
        public string rec { set; get; }
        /// <summary>
        /// 招聘人数
        /// </summary>
        public string num { set; get; }
        /// <summary>
        ///职位类别
        /// </summary>
        public string jobType { set; get; }
        /// <summary>
        ///职位描述
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        ///工作位置
        /// </summary>
        public string workAddress { set; get; }
        /// <summary>
        ///公司人数
        /// </summary>
        public string companyNum { set; get; }
        /// <summary>
        ///公司性质
        /// </summary>
        public string companyType { set; get; }
        /// <summary>
        ///公司行业
        /// </summary>
        public string companyTrde { set; get; }
    }
}
