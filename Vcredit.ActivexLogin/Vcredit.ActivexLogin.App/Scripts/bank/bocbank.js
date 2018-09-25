
//function initPasswordInput(RandomKey_S) {
//    $("#div_password_79445").sec({ mode: 3, RandomKey_S: RandomKey_S });
//    $("#div_password_79445").sec("Version");
//}

//function getEncrypt() {
//    var isDebitCard = false;
//    //var a = {
//    //    loginName: isDebitCard ? $("#txt_50514_741010").val() : $("#txt_50514_740882").val(),
//    //    validationChar: isDebitCard ? $("#txt_captcha_741012").val() : $("#txt_captcha_740885").val(),
//    //    activ: isDebitCard ? $("#div_password_79445").sec("Version") : $("#txt_50531_740884").sec("Version"),
//    //    state: isDebitCard ? $("#div_password_79445").sec("State") : $("#txt_50531_740884").sec("State")
//    //};
//    //isDebitCard
//    //?
//    //    (a.atmPassword = $("#div_password_79445").sec("Value"),
//    //    a.atmPassword_RC = $("#div_password_79445").sec("RandomKey_C"))
//    //:
//    //    (a.phoneBankPassword = $("#txt_50531_740884").sec("Value"),
//    //    a.phoneBankPassword_RC = $("#txt_50531_740884").sec("RandomKey_C"))

//    var a = {
//        loginName: "",
//        validationChar: "",
//        activ: $("#div_password_79445").sec("Version"),
//        state: $("#div_password_79445").sec("State")
//    };
//    a.atmPassword = $("#div_password_79445").sec("Value");
//    a.atmPassword_RC = $("#div_password_79445").sec("RandomKey_C");
//    a.phoneBankPassword = $("#div_password_79445").sec("Value");
//    a.phoneBankPassword_RC = $("#div_password_79445").sec("RandomKey_C");

//    var str = JSON.stringify(a);
//    addElement(str);
//}

//var hdid = "hd_encrypt_string";
//function addElement(value) {
//    var ele = document.createElement("input");
//    ele.id = hdid;
//    ele.type = "hidden";
//    ele.value = value;
//    document.body.appendChild(ele);
//}
//function removeElement() {
//    var ele = document.getElementById(hdid);
//    if (ele) {
//        document.body.removeChild(ele);
//    }
//}
