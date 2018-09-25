function getEncrypt(randnum) {

    pgeditorChar.pwdSetSk(randnum);

    var formData = {
        PwdResult: pgeditorChar.pwdResult(),
        CspName: null,
        BankId: '9999',
        LoginType: 'C',
        _locale: 'zh_CN',
        //UserId: $model.UserId,
        //_vTokenName: $model._vTokenName,
        _UserDN: null,
        _asii: pgeditorChar.pwdLength(),
        _targetPath: getParameter('targetPath')
    };
    return JSON.stringify(formData); //可以将json对象转换成json对符串
}
