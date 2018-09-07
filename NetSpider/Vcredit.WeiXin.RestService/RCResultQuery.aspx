<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RCResultQuery.aspx.cs" Inherits="Vcredit.WeiXin.RestService.RCResultQuery" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table>
                <tr>
                    <td>
                        <label style="width: 100px;">输入参数：</label></td>
                </tr>
                <tr>
                    <td><asp:Literal runat="server" ID="ltl_params"></asp:Literal></td>
                </tr>
                <tr>
                    <td>
                        <label>结果：</label></td>
                </tr>
                <tr>
                    <td>
                        <asp:Literal runat="server" ID="ltl_result"></asp:Literal>
                        <div id="txtResult">
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
