function getEncrypt(GenShell_ClientNo)
{
    var ClientNo = GenShell_ClientNo;

    //if (!ExtraPwdHide) {
    //    var extraPwd = UniLoginForm.ExtraPwd.value;
    //    var ExtraPwd = extraPwd;
    //}

    //if (!BranchNoHide) {
    //    var BranchNo = UniLoginForm.BranchNo.value;
    //}
    var CreditCardVersion = g_CreditCardVersion;

    document.getElementById("UniLoginUser_Ctrl").info = GenShell_ClientNo;
    var AccountNo = document.getElementById("UniLoginUser_Ctrl").IValue;

    document.getElementById("UniLoginPwd_Ctrl").info = GenShell_ClientNo;
    var Password = document.getElementById("UniLoginPwd_Ctrl").IValue;

    document.getElementById("UniLoginPwd_Ctrl").Lic = GenShell_ClientNo;
    var HardStamp = document.getElementById("UniLoginPwd_Ctrl").Lic;

    var Licex = "";
    try {
        var sLicex_UniLoginPwd_Ctrl = document.getElementById("UniLoginPwd_Ctrl").Licex;
        if (sLicex_UniLoginPwd_Ctrl != null && sLicex_UniLoginPwd_Ctrl != 'undefined') {
            Licex = sLicex_UniLoginPwd_Ctrl;
        }
    }
    catch (e)
    { }

    var encrypt = "{'ClientNo': '" + ClientNo + "', 'CreditCardVersion': '" + CreditCardVersion + "', 'AccountNo': '" + AccountNo + "', 'Password': '" + Password + "', 'HardStamp': '" + HardStamp + "', 'Licex': '" + Licex + "'}";

    addElement(encrypt);
}

var hdid = "hd_encrypt_string";
function addElement(value) {
    var ele = document.createElement("input");
    ele.id = hdid;
    ele.type = "hidden";
    ele.value = value;
    document.body.appendChild(ele);
}
function removeElement() {
    var ele = document.getElementById(hdid);
    if (ele) {
        document.body.removeChild(ele);
    }
}