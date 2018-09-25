function getEncrypt(randnum) {
    //var oparams = new Array(
    //        new Pair("isNew", isNew)
    //    );
    //postData2SRVWithCallback("mcryptKey.do", PEGetPostData(oparams), function (success, message) {
    //if (success) {
    // pgeditor.pwdSetSk(message);
    //var ret=Login();
    //addElement(ret);
    // }
    //});

    //以上异步回调改为同步请求
    var retParams = "";
    $.ajax({
        url: 'mcryptKey.do',
        data: { 'isNew': isNew },
        async: false,
        type: "POST",
        success: function (result) {
            pgeditor.pwdSetSk(result);
            retParams = GetLoginParams();
        }
    });
    return retParams;
}

function GetLoginParams() {
    if (document.getElementById("UserAgent")) {
        document.getElementById("UserAgent").value = navigator.userAgent;
    }
    if (document.getElementById("BrowserName")) {
        document.getElementById("BrowserName").value = navigator.appName;
    }
    document.getElementById('EEE').style.display = "none";
    PEWriteFingerIdObject("MFM");
    var macaddress = pgeditor.machineNetwork();
    if (macaddress == null || macaddress == "") {
        macaddress = "0000";
    }


    var password = pgeditor.pwdResult();
    if (password == null || password == "") {
        document.getElementById('EEE').style.display = "";
        document.getElementById('EEE').innerHTML = "密码不能为空";
        _$("_ocx_password").focus();
        return "";
    }

    if (pgeditor.pwdValid() == 1) {
        document.getElementById('EEE').style.display = "";
        document.getElementById('EEE').innerHTML = "输入密码不合法";
        _$("_ocx_password").focus();
        return "";
    }


    if (pgeditor.pwdLength() < 6) {
        document.getElementById('EEE').style.display = "";
        document.getElementById('EEE').innerHTML = "登录密码至少需要六位";
        _$("_ocx_password").focus();
        return "";
    }

    document.getElementById('LoginPassword').value = password;
    document.getElementById('LoginMacAddress').value = macaddress;
    //document.forms[0].LoginPassword.value = password;
    //document.forms[0].LoginMacAddress.value=macaddress;

    var _viewReferer = document.forms[0]._viewReferer.value;
    var _locale = document.forms[0]._locale.value;
    var BankId = document.forms[0].BankId.value;
    var LoginType = document.forms[0].LoginType.value;
    var MFM = document.forms[0].MFM.value;
    var BrowserName = document.forms[0].BrowserName.value;
    var UserAgent = document.forms[0].UserAgent.value;
    var UserId = document.forms[0].UserId.value;
    var LoginPassword = document.forms[0].LoginPassword.value;
    var LoginMacAddress = document.forms[0].LoginMacAddress.value;
    var Submit = document.forms[0].Submit.value;

    //var _vTokenName = document.forms[0]._vTokenName.value;    //验证码手动输入
    return "{'_viewReferer': '" + _viewReferer + "','_locale': '" + _locale + "','BankId': '" + BankId + "','LoginType': '" + LoginType + "'," +
            "'MFM': '" + MFM + "','BrowserName': '" + BrowserName + "','UserAgent': '" + UserAgent + "','UserId': '" + UserId + "'," +
            "'LoginPassword': '" + LoginPassword + "','LoginMacAddress': '" + LoginMacAddress + "','Submit': '" + Submit + "'}";
}