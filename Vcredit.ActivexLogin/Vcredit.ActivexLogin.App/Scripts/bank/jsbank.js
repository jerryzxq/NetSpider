function getEncrypt(account) {

    $.ajax({
        url: "SrandNum",
        type: "GET",
        async: false,
        cache: false,
        success: function (srand_num) {
            pgeditor.pwdSetSk(srand_num);
        }
    });

    var pass = pgeditor.pwdResult();
    var name = account;//$('#nameText').val();
    //var checkCode = $('#checkCode').val();    //验证码手动输入
    var data = {
        "logonId": name,
        "password": pass,
        "channel": "1101",
        "currentBusinessCode": "PB0000",
        "userName": name,
        //"checkCode": checkCode                //验证码手动输入
        "EMP_SID": $.lily.CONFIG_SESSION_ID,
        "responseFormat": "JSON"
    }
    return JSON.stringify(data); //将json对象转换成json对符串
}
