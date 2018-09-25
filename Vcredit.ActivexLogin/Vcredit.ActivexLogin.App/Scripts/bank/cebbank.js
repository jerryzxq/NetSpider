function getEncrypt(randnum) {
    acquireInfo();
    var ran = randnum;
    if (ran != null && ran != "") {
        var random = parseFloat(ran) + 1;
        document.forms[0].ran.value = random;
    } else {
        document.forms[0].ran.value = 0;
    }

    var _viewReferer = document.forms[0]._viewReferer.value;
    var _locale = document.forms[0]._locale.value;
    var rand = document.forms[0].ran.value;
    var TransName = document.forms[0].TransName.value;
    var Plain = document.forms[0].Plain.value;
    var Signature = document.forms[0].Signature.value;
    var MerName = document.forms[0].MerName.value;
    var TransType = document.forms[0].TransType.value;
    var OperationNo = document.forms[0].OperationNo.value;
    var MerDCFlag = document.forms[0].MerDCFlag.value;
    var checkloginflag = document.forms[0].checkloginflag.value;
    var version = document.forms[0].version.value;
    var _tokenName = document.forms[0]._tokenName.value;
    var LoginName = document.forms[0].LoginName.value;
    var Password = document.forms[0].Password.value;
    var Password_RC = document.forms[0].Password_RC.value;
    //var TestCode = document.forms[0].TestCode.value;    //验证码手动输入
    return "{'_viewReferer': '" + _viewReferer + "','_locale': '" + _locale + "','ran': '" + rand + "','TransName': '" + TransName + "'," +
            "'Plain': '" + Plain + "','Signature': '" + Signature + "','MerName': '" + MerName + "','TransType': '" + TransType + "'," +
            "'OperationNo': '" + OperationNo + "','MerDCFlag': '" + MerDCFlag + "','checkloginflag': '" + checkloginflag + "','version': '" + version + "'," +
            "'_tokenName': '" + _tokenName + "','LoginName': '" + LoginName + "','Password': '" + Password + "','Password_RC': '" + Password_RC + "'}";
}
