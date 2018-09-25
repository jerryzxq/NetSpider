function getEncrypt(timeStamp) {
    //document.getElementById("powerpass_ie").focus();
    var encryptObj = "";
    var macInfo = password.GetMachineCode('powerpass_ie', timeStamp);
    $('#userNameForm input[name="MachineCode"]').val(macInfo.machineCode);
    $('#userNameForm input[name="MachineInfo"]').val(macInfo.machineInfo);

    if (password.DealWithPwd("userNameForm", timeStamp, 'powerpass_ie_dyn_Msg'))
        encryptObj = getFormData($('#userNameForm'));

    var str = JSON.stringify(encryptObj);
    addElement(str);
}
function getFormData($form) {
    var unindexed_array = $form.serializeArray();
    var indexed_array = {};
    $.map(unindexed_array, function (n, i) {
        indexed_array[n['name']] = n['value'];
    });
    return indexed_array;
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
