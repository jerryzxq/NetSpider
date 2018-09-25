
function getEncrypt(timestamp) {
    var LoginPasswordObj = getPassInput4Login('LoginPassword', timestamp, 'EEE', '密码输入错误：');
    if (LoginPasswordObj == null) return;
    var LoginPassword = LoginPasswordObj.LoginPassword;

    addElement(LoginPassword);
}

var hdid = "hd_encrypt_string";
function addElement(value) {
    var ele = document.createElement("input");
    ele.id = hdid;
    ele.type = "hidden";
    ele.value = value;
    document.body.appendChild(ele);
}
function removeElement() {
    var ele = document.getElementById(hdid);
    if (ele) {
        document.body.removeChild(ele);
    }
}
