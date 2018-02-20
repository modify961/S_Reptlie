<%@ Page Language="C#" AutoEventWireup="true" CodeFile="reptile_qb.aspx.cs" Inherits="reptile_qb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>糗百数据</title>
    <style type="text/css">
        .td_right {
            text-align: right
        }

        .input_len {
            width: 98%
        }
        .font_red {
            color:red
        }
        td {
            padding:3px;
            margin:3px
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table>
                <tr>
                    <td></td>
                    <td style="text-align: center">
                        <asp:Label ID="lb_Error" runat="server" Text="" CssClass="font_red"  ></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td class="td_right">初始URL：</td>
                    <td>
                        <asp:TextBox ID="txt_url" runat="server" CssClass="input_len"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class="td_right">时间间隔：</td>
                    <td>
                        <asp:TextBox ID="txt_min" runat="server" style=" width:40%"></asp:TextBox>~~
                        <asp:TextBox ID="txt_max" runat="server" style=" width:40%"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class="td_right">线程数：</td>
                    <td>
                        <asp:DropDownList ID="dp_threadNum" runat="server" CssClass="input_len">
                            <asp:ListItem Value="0" Selected="True">0</asp:ListItem>
                            <asp:ListItem Value="1">1</asp:ListItem>
                            <asp:ListItem Value="2">2</asp:ListItem>
                            <asp:ListItem Value="3">3</asp:ListItem>
                            <asp:ListItem Value="4">4</asp:ListItem>
                            <asp:ListItem Value="5">5</asp:ListItem>
                            <asp:ListItem Value="6">6</asp:ListItem>
                            <asp:ListItem Value="7">7</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td class="td_right">是否使用代理：</td>
                    <td>
                        <asp:DropDownList ID="dp_useAgent" runat="server" CssClass="input_len">
                            <asp:ListItem Value="0" Selected="True">否</asp:ListItem>
                            <asp:ListItem Value="1">是</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td class="td_right">是否仿真：</td>
                    <td>
                        <asp:DropDownList ID="dp_simulation" runat="server" CssClass="input_len">
                            <asp:ListItem Value="0" Selected="True">否</asp:ListItem>
                            <asp:ListItem Value="1">是</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td class="td_right">仿真表头：</td>
                    <td>
                        <asp:TextBox ID="txt_requestHeader" TextMode="MultiLine" Width="400" Height="200" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2" style="text-align: center; padding-top:10px">
                        <asp:Button ID="btn_start" runat="server" Text="开始爬取" OnClick="btn_start_Click" />
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
