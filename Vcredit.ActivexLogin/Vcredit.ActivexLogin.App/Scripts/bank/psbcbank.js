function getEncrypt(strSplit) {
    var strs = strSplit.split(","); //字符分割
    var loginName = strs[0];
    var ts = strs[1];
    var password = getIBSPassword("powerpass", ts, "EEE", "密码至少需要六位")
    if (password == null)
        return "";
    document.forms[0].Password.value = password;

    var data = {
        'CSIISignature': document.forms[0].CSIISignature.value,
        '_locale': document.forms[0]._locale.value,
        'TransId': document.forms[0].TransId.value,
        'PayeeAcName': document.forms[0].PayeeAcName.value,
        'PayeeAcAddress': document.forms[0].PayeeAcAddress.value,
        'PayeePostCode': document.forms[0].PayeePostCode.value,
        'PayerPostCode': document.forms[0].PayerPostCode.value,
        'Amount': document.forms[0].Amount.value,
        'Postscript': document.forms[0].Postscript.value,
        'RetType': document.forms[0].RetType.value,
        'PayeeAcNo': document.forms[0].PayeeAcNo.value,
        'RetType': document.forms[0].RetType.value,
        'RemitSeq': document.forms[0].RemitSeq.value,
        'PlanDeptId': document.forms[0].PlanDeptId.value,
        'ProvinceCode': document.forms[0].ProvinceCode.value,
        'CityCode': document.forms[0].CityCode.value,
        'DistrictCode': document.forms[0].DistrictCode.value,
        'PayPassType': document.forms[0].PayPassType.value,
        'dotranscode': document.forms[0].dotranscode.value,
        'UserId': loginName,
        'Password': document.forms[0].Password.value,
        //'_vTokenName': document.forms[0]._vTokenName.value,//验证码手动输入
        'button': document.forms[0].button.value
    }

    return JSON.stringify(data); //将json对象转换成json对符串
}
