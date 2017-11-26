using Logic.enums;
using System;
using System.Collections.Generic;
using System.Text;
using Logic.News;

namespace Logic
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
        public IAbotProceed execute(AbotContext abotContext) {
            _abotcontext = abotContext;
            switch (_abotcontext.abotTypeEnum) {
                case AbotTypeEnum.NEWS:
                    _iabotproceed = new AbotNews(_abotcontext);
                    break;
                default:
                    break;
            }
            return _iabotproceed;
        }
    }
}
