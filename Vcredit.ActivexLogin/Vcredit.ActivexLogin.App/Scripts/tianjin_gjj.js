function getEncrypt(randnum) {
    //var result 
    //$.ajax({
    //    url: "/dzyw-grwt/getRandnum.do?" + Math.random(),
    //    type: "POST",
    //    async: false,
    //    success: function (randnum) {
    //        pgeditor.pwdSetSk(randnum);
    //        var pwdResult2 = pgeditor.pwdSm2Result();
    //        var a = pgeditor.ma();
    //        var b = pgeditor.mb();
    //        var c = pgeditor.mc();
    //        var d = pgeditor.hardwareinfo();

    //        result = '{"cxmm": "' + pwdResult2 + '", "a": "' + a + '", "b": "' + b + '", "c": "' + c + '", "d": "' + d + '"}';
    //    },
    //    error: function () {
            
    //    }
    //});

    //return result;

    pgeditor.pwdSetSk(randnum);
    var pwdResult2 = pgeditor.pwdSm2Result();
    var a = pgeditor.ma();
    var b = pgeditor.mb();
    var c = pgeditor.mc();
    var d = pgeditor.hardwareinfo();

    var result = '{"cxmm": "' + pwdResult2 + '", "a": "' + a + '", "b": "' + b + '", "c": "' + c + '", "d": "' + d + '"}';
    return result;
}