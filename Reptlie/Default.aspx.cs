using Abot.Core;
using Abot.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
       
    }
    protected void btn_test_Click(object sender, EventArgs e)
    {
        string name = DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString();
        System.IO.File.AppendAllText("C:\\data\\bingzheng\\ip.txt",  "llll\r\n");
    }
}