<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MobileAuth.aspx.cs" Inherits="Vcredit.WeiXin.RestService.MobileAuth" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        身份证号：<asp:TextBox ID="tb_identityNo" runat="server"></asp:TextBox>
        <br />
        <br />
        &nbsp;手机号：<asp:TextBox ID="tb_mobile" runat="server"></asp:TextBox>
        <br />
        <br />
        <asp:Button ID="btn_update" runat="server" OnClick="btn_update_Click" Text="修 改" />
    
        <br />
        <br />
        <asp:Label ID="lbl_sm" runat="server" ForeColor="#FF3300"></asp:Label>
    
    </div>
    </form>
</body>
</html>
