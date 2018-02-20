using Abot.Crawler;
using Abot.Logic;
using Abot.Logic.check;
using Abot.Logic.enums;
using Abot.Logic.reptlie;
using Abot.Poco;
using Abot.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class job : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btn_start_Click(object sender, EventArgs e)
    {
        AbotContext abotContext = new AbotContext();
        abotContext.requestHeader = txt_header.Text;
        abotContext.rootUrl = txt_URl.Text;
        abotContext.threadNum = 0;
        abotContext.useAgent = false;
        abotContext.simulation = true;
        IAbotProceed abotProceed = new ZhiLian(abotContext);
        AbotBuilder abotBuilder = new AbotBuilder(abotProceed, abotContext);
        abotBuilder.exe();

    }
    
}