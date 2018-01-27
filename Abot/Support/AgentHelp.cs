using Abot.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Abot.Support
{
    /// <summary>
    /// 获取代理基类
    /// </summary>
    public class AgentHelp
    {
        /// <summary>
        /// 代理数据
        /// </summary>
        List<Agenter> _agenter = null;
        /// <summary>
        /// 当前索引
        /// </summary>
        int index;
        /// <summary>
        /// 构造函数
        /// </summary>
        public AgentHelp() {
            index = 0;
            _agenter = new List<Agenter>();
            obtainAgent();
        }
        /// <summary>
        /// 获取下一个代理IP
        /// </summary>
        /// <returns></returns>
        public Agenter next() {
            index++;
            if (index < _agenter.Count)
                return _agenter[index];
            index = 0;
            obtainAgent();
            if(_agenter!=null&& _agenter.Count>0) 
                return _agenter[index];
            return null;
        }
        /// <summary>
        /// 获取当前可用的代理IP
        /// </summary>
        /// <returns></returns>
        private void obtainAgent() {
            Uri address = new Uri("http://47.96.146.22:7500/AgentIpService/obtainAll");
            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = 0;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                _agenter = JsonConvert.DeserializeObject<List<Agenter>>(reader.ReadToEnd().Replace("\0", ""), new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            }
        }
    }
}
