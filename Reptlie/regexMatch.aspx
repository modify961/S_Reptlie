<%@ Page Language="C#" AutoEventWireup="true" CodeFile="regexMatch.aspx.cs" Inherits="regexMatch" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>

<body>
    <form id="form1" runat="server">
        <div>
            正则：<asp:textbox ID="txt_regx" runat="server"></asp:textbox>   <br />
            数据：<asp:textbox ID="txt_date" runat="server"></asp:textbox>   <br />
            <asp:Button ID="Button1" runat="server" Text="开始匹配" OnClick="Button1_Click" />
            <br /> 结果：<asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
        </div>
    </form>
</body>
</html>
