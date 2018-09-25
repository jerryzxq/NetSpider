function getEncrypt(strSplit) {
    var strs = strSplit.split(","); //字符分割
    var loginName = strs[0];
    var keycode = strs[1];

    //var clearSignatureCode = "";
    var loginWay = $("#loginWay").val();
    //clearSignatureCode = clearSignatureCode + loginWay;

    try {
        document.getElementById("customerMacAddr").value = document.all.ipmacObject.getClientMac();
    } catch (e) {
    }

    var alias = loginName;//document.getElementById("alias").value;
    var obj = document.getElementsByName("idType");
    var idType = obj[0].options[obj[0].selectedIndex].value;
    var idNo = document.getElementById("idNo").value;

    if (loginWay == "" || loginWay == null) {
        //alert('登录方式必填！');
        return "";
    }
    if (loginWay == "2") {
        if (alias == "" || alias == null) {
            //alert('用户名必填！');
            return "";
        }
        //clearSignatureCode = clearSignatureCode + alias;
    }
    if (loginWay == "3") {

        if (idType == "CHOOSE") {
            //alert('请选择证件类型！');
            return "";
        }
        if (idNo == "" || idNo == null) {
            alert('证件号码必填！');
            return "";
        }
        //clearSignatureCode = clearSignatureCode + idType + idNo;
    }
    //if ("pwdError" == "") {
    //    var validateNo = document.getElementById("validateNo").value;
    //    if (validateNo == "" || validateNo == null) {
    //        alert('验证码必填！');
    //        return "";
    //    }
    //    clearSignatureCode = clearSignatureCode + validateNo;
    //}

    var pwd = document.getElementById("passwd");
    pwd.SetPasswordEncryptionKey(keycode, 0, 0);
    document.getElementById("realpass").value = pwd.value;
    var loginVersion = document.getElementById("loginVersion").value;


    //var loginVersion = document.logonForm.loginVersion.value; //已定义
    var actionType = document.logonForm.actionType.value;
    var realpass = document.logonForm.realpass.value;
    var dn = document.logonForm.dn.value;
    var sn = document.logonForm.sn.value;
    var signHardId = document.logonForm.signHardId.value;
    var signPublicKey = document.logonForm.signPublicKey.value;
    var signEncryptCode = document.logonForm.signEncryptCode.value;
    var signClearCode = document.logonForm.signClearCode.value;
    var signCertInfo = document.logonForm.signCertInfo.value;
    var customerMacAddr = document.logonForm.customerMacAddr.value;
    //var loginWay = document.logonForm.loginWay.value;     //已定义
    var idNoVal = document.logonForm.idNoVal.value;
    var idTypeVal = document.logonForm.idTypeVal.value;
    var _operator = document.logonForm.operator.value;
    var paraValue = document.logonForm.paraValue.value;
    var signCSPName = document.logonForm.signCSPName.value;
    var keySN = document.logonForm.keySN.value;
    var CFCAVersion = document.logonForm.CFCAVersion.value;
    //var idType = document.logonForm.idType.value;         //已定义
    //var alias = document.logonForm.alias.value;           //已定义
    //var idNo = document.logonForm.idNo.value;             //已定义
    var qy_mima = document.logonForm.qy_mima.value;
    //var validateNo = document.logonForm.validateNo.value; //验证码手动输入
    var qy_sut = document.logonForm.qy_sut.value;

    var ret = "{'loginVersion': '" + loginVersion + "','actionType': '" + actionType + "','realpass': '" + realpass + "'," +
            "'dn': '" + dn + "','sn': '" + sn + "','signHardId': '" + signHardId + "','signPublicKey': '" + signPublicKey + "'," +
            "'signEncryptCode': '" + signEncryptCode + "','signClearCode': '" + signClearCode + "'," +
            "'signCertInfo': '" + signCertInfo + "','customerMacAddr': '" + customerMacAddr + "','loginWay': '" + loginWay + "'," +
            "'idNoVal': '" + idNoVal + "','idTypeVal': '" + idTypeVal + "','_operator': '" + _operator + "'," +
            "'paraValue': '" + paraValue + "','signCSPName': '" + signCSPName + "','keySN': '" + keySN + "'," +
            "'CFCAVersion': '" + CFCAVersion + "','idType': '" + idType + "','alias': '" + alias + "'," +
            "'idNo': '" + idNo + "','qy_mima': '" + qy_mima + "','qy_sut': '" + qy_sut + "'}";

    return ret;
}
