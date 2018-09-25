function getEncrypt(strSplit) {
    var strs = strSplit.split(","); //字符分割
    var loginName = strs[0];
    var ts = strs[1];
    var vForm = document.getElementById('con2');

    var password = getIBSPassword("powerpass1", ts, vForm.name + "_EEE", "密码至少需要六位")
    if (password == null)
        return "";
    vForm.Password.value = password;

    var mfm = getMFMInput("powerutil1", ts, vForm.name + "_EEE", "获取机器码失败")
    if (mfm == null)
        return "";
    vForm.MachineCode.value = mfm;

    var data = {
        '_locale': vForm._locale.value,
        'LoginType': vForm.LoginType.value,
        'PreLoginMode': vForm.PreLoginMode.value,
        'Password': vForm.Password.value,
        '_lang': vForm._lang.value,
        'LoginId': loginName,
        'PasswordLogin3': vForm.PasswordLogin3.value,
        //'_vTokenName': vForm._vTokenName.value,//验证码手动输入
        'MachineCode': vForm.MachineCode.value
    }

    return JSON.stringify(data); //将json对象转换成json对符串
}
