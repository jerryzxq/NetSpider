function getEncrypt(rand) {
    sRand = rand;

    var certno, password, zjh
    if (flag == 1) {
        form1.seccertno.SetRecverCert(sCert);
        form1.seccertno.SetRandomSeed(sRand);
        certno = form1.seccertno.GetValue();

        form1.secpassword.SetRecverCert(sCert);
        form1.secpassword.SetRandomSeed(sRand);
        password = form1.secpassword.GetValue();
    }
    else {
        form1.seczjh.SetRecverCert(sCert);
        form1.seczjh.SetRandomSeed(sRand);
        zjh = form1.seczjh.GetValue();

        form1.secpassword.SetRecverCert(sCert);
        form1.secpassword.SetRandomSeed(sRand);
        password = form1.secpassword.GetValue();
    }

    return '{ "certno": "' + certno + '", "password": "' + password + '", "zjh": "' + zjh + '"}';
}

function changLoginType(type) {
    $("input[name='radiobutton']:eq(" + (parseInt(type) - 1) + ")").trigger("click")
    // changeCert(type);
}