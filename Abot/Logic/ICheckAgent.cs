using Abot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abot.Logic
{
    /// <summary>
    /// 判断代理IP是否可以
    /// </summary>
    public interface ICheckAgent
    {
        /// <summary>
        ///判断代理IP是否可用 
        /// </summary>
        /// <param name="agenter"></param>
        /// <returns></returns>
        bool agentCheck(Agenter agenter);
    }
}
