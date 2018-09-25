function getEncrypt(randomKey)
{
    //$.ajax({
    //    cache: false,
    //    type: "post",
    //    async: false,
    //    url: basepath + "userReg.do?method=getSrandNum&num=" + Math.random(),
    //    dataType: "text",
    //    success: function (result)
    //    {
    //        pgeditor.pwdSetSk(result);//给控件设置随机因子
    //    }
    //});

    pgeditor.pwdSetSk(randomKey);//给控件设置随机因子

    var pwdResult = pgeditor.pwdResultRSA();//获取密码AES+RSA密文
    var machineNetwork = pgeditor.machineNetwork();//获取网卡信息密文
    var machineDisk = pgeditor.machineDisk();//获取硬盘信息密文
    var machineCPU = pgeditor.machineCPU();//获取CPU信息密文
    $("#password").val(pwdResult);//将密码密文赋值给表单
    $("#pass").val(pwdResult);
    //alert(pwdResult);
    return pwdResult;
}

function getPasswordPosition()
{
    var ele = document.getElementsByClassName("inputBox position_re")[0]
    return generatePositionJson(ele);
}
