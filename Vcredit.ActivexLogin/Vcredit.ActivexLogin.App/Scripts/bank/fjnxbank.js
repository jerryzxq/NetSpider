function getEncrypt(randnum) {
    var password = getIBSPassword("powerpass", randnum, "EEE", "密码为空或格式不正确")
    if (password == null)
        return "";
    document.forms[0].Password.value = password;

    var CSIISignature = document.forms[0].CSIISignature.value;
    var _locale = document.forms[0]._locale.value;
    var BankId = document.forms[0].BankId.value;
    var LoginType = document.forms[0].LoginType.value;
    var StartLogType = document.forms[0].StartLogType.value;
    var UserId = document.forms[0].UserId.value;
    //var _vTokenName = document.forms[0]._vTokenName.value; //验证码手动输入

    return "{'CSIISignature': '" + CSIISignature + "','_locale': '" + _locale + "','BankId': '" + BankId + "','LoginType': '" + LoginType + "'," +
            "'StartLogType': '" + StartLogType + "','UserId': '" + UserId + "','password': '" + password + "'}";
}
