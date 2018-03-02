using Abot.Logic;
using Abot.Logic.reptlie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class reptile_wb : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btn_start_Click(object sender, EventArgs e)
    {
        lb_Error.Text = "";
        if (!string.IsNullOrEmpty(txt_url.Text))
        {
            if (dp_simulation.SelectedValue == "1" && string.IsNullOrEmpty(txt_requestHeader.Text))
            {
                lb_Error.Text = "仿真环境下，仿真表头不得为空！";
                return;
            }
            AbotContext abotContext = new AbotContext();
            if (!string.IsNullOrEmpty(txt_min.Text))
            {
                int min = 0;
                int.TryParse(txt_min.Text, out min);
                abotContext.minNeed = min;
            }
            if (!string.IsNullOrEmpty(txt_max.Text))
            {
                int max = 0;
                int.TryParse(txt_max.Text, out max);
                abotContext.maxNeed = max;
            }
            abotContext.rootUrl = txt_url.Text;
            if (!string.IsNullOrEmpty(dp_threadNum.SelectedValue))
            {
                int threadNum = 0;
                int.TryParse(dp_threadNum.SelectedValue, out threadNum);
                abotContext.threadNum = threadNum;
            }
            abotContext.useAgent = dp_useAgent.SelectedValue == "0" ? false : true;
            abotContext.simulation = dp_simulation.SelectedValue == "0" ? false : true;
            abotContext.cacheCookie = dp_cacheCookie.SelectedValue == "0" ? false : true;
            if (abotContext.simulation)
                abotContext.requestHeader = txt_requestHeader.Text;
            IAbotProceed abotProceed = new WeiBo(abotContext);
            AbotBuilder abotBuilder = new AbotBuilder(abotProceed, abotContext);
            abotBuilder.exe();
        }
        else
        {
            lb_Error.Text = "种子URL不能为空";
        }
    }
}