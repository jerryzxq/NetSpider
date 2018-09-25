function getEncrypt(randnum) {
    try {
        //无证书登录
        if (document.formLogon.logonType.value == "2") {
            var noCertLogon = trim(document.formTmp.logonNoCert.value);
            if (noCertLogon == '') {
                showLogonLoading();
                document.formTmp.logonNoCert.focus();
                return "";
            }
            if (noCertLogon.length > 100) {
                showLogonLoading();
                document.formTmp.logonNoCert.focus();
                return "";
            }
            document.formLogon.logonNo.value = noCertLogon;
            document.form2Logon.logonNo.value = noCertLogon;
            document.formLogon.browserFlag.value = browserFlag;
            document.form2Logon.browserFlag.value = browserFlag;
        } else {//有证书登录，要区分是不是手工输入
            //如果为1，则为自动调出
            if (certCustomerNum == "1") {
                document.formLogon.logonNo.value = document.formTmp.logonNo.value;
                document.keyLogonForm.logonNo.value = document.formTmp.logonNo.value;
                document.form2Logon.logonNo.value = document.formTmp.logonNo.value;
            }
            //如果为 M ，则为手工输入
            if (certCustomerNum == "M") {
                var noCertLogon = trim(document.formTmp.logonNoCert.value);
                if (noCertLogon == '') {
                    showLogonLoading();
                    document.formTmp.logonNoCert.focus();
                    return;
                }
                if (noCertLogon.length > 100) {
                    showLogonLoading();
                    document.formTmp.logonNoCert.focus();
                    return;
                }
                document.formLogon.logonNo.value = noCertLogon;
                document.keyLogonForm.logonNo.value = noCertLogon;
                document.form2Logon.logonNo.value = noCertLogon;
            }
        }
        if (!checkLogonPinBlockByName(pgeditor, logonPwdMsg1)) {
            showLogonLoading();
            return;
        }

        //判断mac操作系统以外进行sessionkey加密
        var userComputerId = "";
        if (browserFlag == '2') {
            document.formLogon.password.value = pgeditor.pwdResult();
            document.keyLogonForm.password.value = pgeditor.pwdResult();
            document.form2Logon.password.value = pgeditor.pwdResult();
            userComputerId = pgeditor.pwdResultCPU();//获取机器码
        } else {
            createSessionKey();
            var guardID = getGuardID();
            document.formLogon.password.value = guardID.EncryptData(pgeditor.pwdResult());
            document.keyLogonForm.password.value = guardID.EncryptData(pgeditor.pwdResult());
            document.form2Logon.password.value = guardID.EncryptData(pgeditor.pwdResult());

            userComputerId = guardID.GetUserID();
        }
        if (userComputerId == "NF") {
            downLoadCNCBGuard();
            throw e;
        }
        document.formLogon.computerId.value = userComputerId;
        document.keyLogonForm.computerId.value = userComputerId;
        document.form2Logon.computerId.value = userComputerId;

        //无证书登陆
        if (document.formLogon.logonType.value == "2") {
            var noCertLogon = trim(document.formTmp.logonNoCert.value);
            if (noCertLogon.length == 18) {
                document.formLogon.customerCtfType.value = "1";
                document.form2Logon.customerCtfType.value = "1";
            } else if (noCertLogon.length == 16 || noCertLogon.length == 19) {
                document.formLogon.customerCtfType.value = "2";
                document.form2Logon.customerCtfType.value = "2";
            } else {
                document.formLogon.customerCtfType.value = "0";
                document.form2Logon.customerCtfType.value = "0";
            }
            document.formLogon.session_certSubType.value = '';
            document.form2Logon.session_certSubType.value = '';
            var obj_verifyId = document.getElementById('verifyId');
            if (obj_verifyId.style.display == '') {
                if (document.formTmp.verifyCode.value == '' || document.formTmp.verifyCode.value.length != 4) {
                    showLogonLoading();
                    document.formTmp.verifyCode.focus();
                    return;
                }
                document.formLogon.verifyCode.value = document.formTmp.verifyCode.value;
                //验证校验码
                executeAjaxCommand(checkVeriyUrl, ('verifyCode=' + document.formTmp.verifyCode.value), 'disposeMsg');
                //验证校验码
            } else {
                //登录事件采集
                analize("login", "ptb_usernameland_ld", document.form2Logon.logonNo.value);
                //document.form2Logon.submit();
                setTimeout(function () {
                    jQuery("#logonLoading1").attr("src", logonLoadingUrl);
                }, 20);
            }

        }

    } catch (e) {
    }


    var opFlag = document.form2Logon.opFlag.value;
    var logonType = document.form2Logon.logonType.value;
    var customerCtfType = document.form2Logon.customerCtfType.value;
    var logonNo = document.form2Logon.logonNo.value;
    var password = document.form2Logon.password.value;
    //var verifyCode = document.form2Logon.verifyCode.value;//验证码手动输入
    var session_userRemoteIP = document.form2Logon.session_userRemoteIP.value;
    var session_userRemoteMAC = document.form2Logon.session_userRemoteMAC.value;
    var session_certSubType = document.form2Logon.session_certSubType.value;
    var session_tagFlag = document.form2Logon.session_tagFlag.value;
    var session_certHashcode = document.form2Logon.session_certHashcode.value;
    var logon_param = document.form2Logon.logon_param.value;
    var computerId = document.form2Logon.computerId.value;
    var htd_param = document.form2Logon.htd_param.value;
    var ca_id = document.form2Logon.ca_id.value;
    var logonFlag = document.formLogon.logonFlag.value; //formLogon验证码登录&form2Logon无验证码登录
    //var browserFlag = document.form2Logon.browserFlag.value;//全局变量已定义
    var language = document.form2Logon.language.value;
    var EMP_SID = document.form2Logon.EMP_SID.value;
    return "{'opFlag': '" + opFlag + "','logonType': '" + logonType + "','customerCtfType': '" + customerCtfType + "','logonNo': '" + logonNo + "'," +
            "'password': '" + password + "','session_userRemoteIP': '" + session_userRemoteIP + "','session_userRemoteMAC': '" + session_userRemoteMAC + "'," +
            "'session_certSubType': '" + session_certSubType + "','session_tagFlag': '" + session_tagFlag + "','session_certHashcode': '" + session_certHashcode + "'," +
            "'logon_param': '" + logon_param + "','computerId': '" + computerId + "','htd_param': '" + htd_param + "','ca_id': '" + ca_id + "'," +
            "'logonFlag': '" + logonFlag + "','browserFlag': '" + browserFlag + "','language': '" + language + "','EMP_SID': '" + EMP_SID + "'}";

}
