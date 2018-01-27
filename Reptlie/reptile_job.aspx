<%@ Page Language="C#" AutoEventWireup="true" CodeFile="reptile_job.aspx.cs" Inherits="job" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txt_header" TextMode="MultiLine"  Width="200" Height="200" runat="server"></asp:TextBox>
            <asp:Button ID="btn_start" runat="server" Text="开始爬取" OnClick="btn_start_Click" />
        </div>
    </form>
</body>
</html>
