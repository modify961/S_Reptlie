using RabbitMQHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class regexMatch : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        MQSend mqSend = new MQSend();
        MQReceive mqReceive = new MQReceive();
        //mqSend.send("hello", "123");
        mqReceive.Receive();
    }

    protected void Button1_Click(object sender, EventArgs e)
    { 
        Regex obj = new Regex("^http://www.xicidaili.com/nn/\\d+$", RegexOptions.Compiled);
        if (obj.IsMatch(txt_date.Text))
        {
            Label1.Text = "匹配";
        }
        else {
            Label1.Text = "不匹配";
        }
    }
}