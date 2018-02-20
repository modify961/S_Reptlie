using Abot.Logic.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abot.Logic
{
    public class AbotFactory
    {
        /// <summary>
        /// IAbotProceed：根据不同类型初始化不同的功能项
        /// </summary>
        private IAbotProceed _iabotproceed;
        /// <summary>
        /// 爬行配置的上下文
        /// </summary>
        private AbotContext _abotcontext;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="abotContext"></param>
        /// <returns></returns>
        public IAbotProceed execute(AbotContext abotContext) {
            _abotcontext = abotContext;
            switch (_abotcontext.abotTypeEnum) {
                default:
                    break;
            }
            return _iabotproceed;
        }
    }
}
