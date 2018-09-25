function getEncrypt(randnum) {
    if ($("userPassword").GetPL() == 0) {
        return "";//请输入密码
    }
	
    var pwdStr = GetPwd($("userPassword"));//$("userPassword").GetRT();
    if (!checkPwdObject_login($("userPassword"), "密码", document.logonForm.userPassword, document.logonForm.isPass)) {
        return "";//密码规则不符
    }
	
    //$("userPassword").Type = "A";
    var pwdStrA = GetPwd($("userPassword"));//$("userPassword").GetRT();
    pwdStrA = pwdStrA.substring(0, pwdStrA.length - 4);

    document.logonForm.isCreditCard.value = "1";//0信用卡，1非信用卡
    //若为账号则检查是否是信用卡
    if (logonType == "0") {
        var rule = judgeAccRole($("loginId").value);
        if (rule.cardOwner == "0" && (rule.cardType == "1" || rule.cardType == "2")) {
            isCreditCard = "0";
            document.logonForm.isCreditCard.value = "0";
        }
    }
    document.logonForm.loginId.value = $("loginId").value;
    document.logonForm.accountNo.value = $("loginId").value;
    document.logonForm.logonType.value = logonType;
    document.logonForm.accPassword.value = pwdStrA;
    //document.logonForm.checkCode.value = $("captcha").value;
    processClientInfo("userPassword", "logonForm");//密码控件
    try {
        if (!(param == "" || param == null || param == "null")) {
            if (param == 'creditCard') { //信用卡版本登录
                document.logonForm.actionFlag.value = "01";
                document.logonForm.isCreditVersion.value = "01";
            } else {
                var logonValue = $("loginId").value;
                //3月份官方网站快捷入口改造
                new Ajax.Request("getMenuID.do?",
                        {
                            method: "post", parameters: { menuName: param }, asynchronous: false, onComplete: getMenuIDCallBack
                        }
                        );
                if (menuId) {
                    allParam = Object.extend({ 'accountNo': logonValue }, allParam);
                    document.logonForm.content.value = Object.toJSON(Object.extend({ 'bsncode': menuId }, allParam));
                }

            }
        }
    } catch (e) {
    }
    var loginId = document.logonForm.loginId.value;
    var accountNo = document.logonForm.accountNo.value;
    var accPassword = document.logonForm.accPassword.value;
    var userPassword=document.logonForm.userPassword.value;
    var isCreditCard = document.logonForm.isCreditCard.value;
    var actionFlag = document.logonForm.actionFlag.value;
    var isCreditVersion = document.logonForm.isCreditVersion.value;
    var content = document.logonForm.content.value;

    var isPass = document.logonForm.isPass.value;
    var clientIP = document.logonForm.clientIP.value;
    var clientMacAdress = document.logonForm.clientMacAdress.value;
    var clientMainboardNo = document.logonForm.clientMainboardNo.value;
    var clientHarddiskNo = document.logonForm.clientHarddiskNo.value;
    var hrefValue = document.logonForm.hrefValue.value;
    var version = document.logonForm.version.value;

    randomCodeFlag = 0;//每次执行后重置

    return "{'loginId': '" + loginId + "','accountNo': '" + accountNo + "','accPassword': '" + userPassword + "','userPassword': '" + accPassword + "'," +
            "'logonType': '" + logonType + "','isCreditCard': '" + isCreditCard + "','actionFlag': '" + actionFlag + "','version': '" + version + "'," +
            "'isCreditVersion': '" + isCreditVersion + "','content': '" + content + "','isPass': '" + isPass + "','clientIP': '" + clientIP + "'," +
            "'clientMacAdress': '" + clientMacAdress + "','clientMainboardNo': '" + clientMainboardNo + "','clientHarddiskNo': '" + clientHarddiskNo + "','hrefValue': '" + hrefValue + "'}";
}
