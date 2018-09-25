function getEncrypt(strSplit) {
    var strs = strSplit.split(","); //字符分割 
    var loginName = strs[0];//trim(document.getElementById('nameText').value);
    var checkCode = strs[1];
    document.logonForm.customerAlias.value = loginName;
    var params = 'customerAlias=' + loginName + '&BankNo=' + BankNo + '&checkCode=' + checkCode;
    params = params + '&bankName=' + BLink.getBankName();

    var retParams = "";
    new Ajax.Request(
        "loginUserInfoCheck.do",
        {
            method: "post",
            parameters: params,
            asynchronous: false,
            onFailure: showError,
            onComplete: function (response) {
                retParams = GetLoginParams(response);
            }
        }
    );

    return retParams;
}

function GetLoginParams(response) {
    var contextDataPos = response.responseXML.childNodes.length - 1;
    var contextData = new KeyedCollectionClass(response.responseXML.childNodes[contextDataPos]);
    var ts;
    var errorMessage = contextData.getValueAt("hostErrorMessage");
    if (errorMessage == null || errorMessage == '') {
        ts = contextData.getValueAt("logonTime");
    } else {
        //alert(errorMessage);
        return "";
    }
    var password = getIBSInput("powerpass", ts, "", "");
    var macJson = getMFMInput("isecurityutil", ts, "", "");
    if (password == null) {
        return "";
    }
    if (isEmpty(password)) {
        return "";
    }
    var actionFlag = contextData.getValueAt("actionFlag");
    if (actionFlag == 1) {
        //alert("您的用户名或密码错误！请重新输入！");
        return "";
    } else if (actionFlag == 2) {
        //alert("网银未开通！请稍候再试");
        return "";
    } else if (actionFlag == 3) {
        //alert("验证码错误！");
        return "";
    } else if (actionFlag == 5) {
        var errorTip = contextData.getValueAt("errorTip");
        //alert(errorTip);
        return "";
    } else {
        document.getElementById('BankNo').value = BankNo;
        document.getElementById('bankName').value = BLink.getBankName();
    }

    document.logonForm.password.value = password;
    document.logonForm.macJson.value = macJson;
    document.logonForm.pwdType.value = pwdType;

    var logonType = document.logonForm.logonType.value;
    var customerAlias = document.logonForm.customerAlias.value;
    //var password = document.logonForm.password.value;     //已定义
    var logonLanguage = document.logonForm.logonLanguage.value;
    var customerType = document.logonForm.customerType.value;
    //var BankNo = document.logonForm.BankNo.value;         //已定义
    var bankName = document.logonForm.bankName.value;
    //var checkCode = document.logonForm.checkCode.value;   //验证码手动输入
    var mobileCheckCode = document.logonForm.mobileCheckCode.value;
    var login_flag = document.logonForm.login_flag.value;
    //var pwdType = document.logonForm.pwdType.value;       //已定义
    //var macJson = document.logonForm.macJson.value;       //已定义

    return "{'logonType': '" + logonType + "','customerAlias': '" + customerAlias + "','password': '" + password + "'," +
            "'logonLanguage': '" + logonLanguage + "','customerType': '" + customerType + "','BankNo': '" + BankNo + "','bankName': '" + bankName + "'," +
            "'mobileCheckCode': '" + mobileCheckCode + "','login_flag': '" + login_flag + "','pwdType': '" + pwdType + "','macJson': '" + macJson + "'}";
}
