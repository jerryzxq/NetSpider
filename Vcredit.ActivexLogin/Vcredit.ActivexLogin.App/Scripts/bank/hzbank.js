function getEncrypt(account) {
    document.getElementById('customerIdText').value = account;
    var customerId = trim(document.getElementById('customerIdText').value);
    //获取密文
    var pwdResult = GetPassLogon(input_password);
    if (pwdResult == null || pwdResult == "") {
        return "";
    }
    //获取设备信息
    var machineNetwork = input_password.machineNetwork();//获取网卡信息
    var machineDisk = input_password.machineDisk();//获取硬盘信息
    var machineCPU = input_password.machineCPU();//获取CPU信息

    document.logonForm.session_randomNum.value = randomNum;

    document.logonForm.session_machineNetwork.value = machineNetwork;
    document.logonForm.session_machineDisk.value = machineDisk;
    document.logonForm.session_machineCPU.value = machineCPU;

    //document.logonForm.checkCode.value = checkCode;   //验证码手动输入
    document.logonForm.customerId.value = customerId;
    document.logonForm.password.value = pwdResult;
    //保存客户端信息
    try {
        document.logonForm.clientInfo.value = clientInfoX.GetClientInfo();
        document.logonForm.pageFlag.value = "1";
    } catch (ei) {
        var errorDesc = "";
        try {
            errorDesc = clientInfoX.GetLastErrorDesc();
            errorDesc = errorDesc.replace(/\n/g, "");
            document.logonForm.clientInfo.value = "error-" + "客户端信息控件获取信息异常" + errorDesc;
        } catch (ec) {
            document.logonForm.clientInfo.value = "error-" + "未成功安装或者已禁用客户端信息控件";
        }
    }
    //获得登录浏览器版本和操作系统
    document.logonForm.broser.value = getBroser();
    document.logonForm.sys.value = getSys();
    document.logonForm.info.value = getBroserSysInfo();
    document.logonForm.pwdKey.value = pwdKey;


    var netType = document.logonForm.netType.value;
    var logonLanguage = document.logonForm.logonLanguage.value;
    var userType = document.logonForm.userType.value;
    var simCertDN = document.logonForm.simCertDN.value;
    var password = document.logonForm.password.value;
    //var customerId = document.logonForm.customerId.value;     //已定义
    var sn = document.logonForm.sn.value;
    var dn = document.logonForm.dn.value;
    var userRemoteIP = document.logonForm.userRemoteIP.value;
    var session_userRemoteIP = document.logonForm.session_userRemoteIP.value;
    var endDate = document.logonForm.endDate.value;
    var sequenceNumber = document.logonForm.sequenceNumber.value;
    //var checkCode = document.logonForm.checkCode.value;     //验证码手动输入
    var clientInfo = document.logonForm.clientInfo.value;
    var b2cFlag = document.logonForm.b2cFlag.value;
    var pageFlag = document.logonForm.pageFlag.value;
    var broser = document.logonForm.broser.value;
    var sys = document.logonForm.sys.value;
    var info = document.logonForm.info.value;
    var IPAddress = document.logonForm.IPAddress.value;
    var MACAddress = document.logonForm.MACAddress.value;
    var session_randomNum = document.logonForm.session_randomNum.value;
    //var pwdKey = document.logonForm.pwdKey.value;         //已定义
    var session_machineNetwork = document.logonForm.session_machineNetwork.value;
    var session_machineDisk = document.logonForm.session_machineDisk.value;
    var session_machineCPU = document.logonForm.session_machineCPU.value;

    var ret = "{'netType': '" + netType + "','logonLanguage': '" + logonLanguage + "','userType': '" + userType + "'," +
            "'simCertDN': '" + simCertDN + "','password': '" + password + "','customerId': '" + customerId + "','sn': '" + sn + "'," +
            "'dn': '" + dn + "','userRemoteIP': '" + userRemoteIP + "','session_userRemoteIP': '" + session_userRemoteIP + "'," +
            "'clientInfo': '" + clientInfo + "','b2cFlag': '" + b2cFlag + "','pageFlag': '" + pageFlag + "','broser': '" + broser + "'," +
            "'endDate': '" + endDate + "','sequenceNumber': '" + sequenceNumber + "','sys': '" + sys + "','info': '" + info + "'," +
            "'IPAddress': '" + IPAddress + "','MACAddress': '" + MACAddress + "','session_randomNum': '" + session_randomNum + "'," +
            "'pwdKey': '" + pwdKey + "','session_machineNetwork': '" + session_machineNetwork + "'," +
            "'session_machineDisk': '" + session_machineDisk + "','session_machineCPU': '" + session_machineCPU + "'}";

    clearPwdkey();

    return ret;
}
