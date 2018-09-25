var pwdFormatMsg = new Array(18);

pwdFormatMsg[0] = new Array("100", "只允许字母");
pwdFormatMsg[1] = new Array("010", "只允许数字");
pwdFormatMsg[2] = new Array("001", "只允许特殊符");
pwdFormatMsg[3] = new Array("110", "允许字母或数字");
pwdFormatMsg[4] = new Array("101", "允许字母或特殊符");
pwdFormatMsg[5] = new Array("011", "允许数字或特殊符");
pwdFormatMsg[6] = new Array("000", "不允许输入任何字符");
pwdFormatMsg[7] = new Array("111", "允许输入任何字符");
pwdFormatMsg[8] = new Array("220", "必须字母和数字");
pwdFormatMsg[9] = new Array("202", "必须字母和特殊符");
pwdFormatMsg[10] = new Array("022", "必须数字和特殊符");
pwdFormatMsg[11] = new Array("222", "必须包含三种类型的字符");
pwdFormatMsg[12] = new Array("221", "须包含字母和数字，允许特殊符");
pwdFormatMsg[13] = new Array("212", "须包含字母和特殊符，允许数字");
pwdFormatMsg[14] = new Array("122", "须包含数字和特殊符，允许字母");
pwdFormatMsg[15] = new Array("221|212|122", "必须同时包含两种或以上类型的字符");
pwdFormatMsg[16] = new Array("221|212", "必须包含字母，且同时包含数字、字母、符号中任意两种以上字符");
pwdFormatMsg[17] = new Array("020", "必须是数字");

var version_now = "2.1.0.0";//现有版本

var passwordNum = 0;
var randomCode = "";//随机码
var randomCodeFlag = "0";//随机码请求标志0:未请求, 1:已请求.

//根据规则代码获取规则描述信息                             
function getpwdFormatMsg(ruleStr) {
    for (var i = 0; i < pwdFormatMsg.length; i++) {
        if (ruleStr == pwdFormatMsg[i][0]) {
            return pwdFormatMsg[i][1];
        }
    }
    return "";
}
function XSetFocus(item) {
    document.getElementById(item).focus();
}
function setPasswordFocus(event, item) {
    if (event.keyCode == 9) {
        if (!Prototype.Browser.IE) {
            document.getElementById(item).focus();
        }
    }
}

//生成密码控件
//tdObj:密码输入框所在表格的td对象,pwdObjId:密码控件对象id
var currentNavigator;
//createPwdObjectNew("userPassword","165","29","8","12","E","221|212",null,"captcha",true);
function createPwdObjectNew(pwdObjId, pwdObjWidth, pwdObjHeight, minLen, maxLen, typeStr, ruleStr, pwdHiddenName, nextElemId, flag, delay) {
    var _delay = delay || 100;
    var x = navigator;
    currentNavigator = navigator;
    var res = new Array(2);
    var appVersion_ = x.appVersion;
    if (x.cpuClass != 'x86') {
        res = choiceCab(1);//64位浏览器
    } else {
        if (appVersion_.indexOf("WOW64;") == '-1') {
            res = choiceCab(2);//32位浏览器，32系统
        } else {
            res = choiceCab(3);//32位浏览器，64系统
        }
    }
    classid = res[0];
    codebase = res[1];
    var activeName;
    if (navigator.userAgent.indexOf("Firefox") >= 0) {
        activeName = getActiveName(1);//集中管理,方便改
    } else if (navigator.userAgent.indexOf("Safari") >= 0 && navigator.userAgent.indexOf("Macintosh") >= 0 && navigator.userAgent.indexOf("Chrome") < 0) {
        activeName = getActiveName(2);
    } else if (navigator.userAgent.indexOf("Chrome") >= 0) {
        activeName = getActiveName(3);
    }

    var str = "<div style='width: " + pwdObjWidth + ";height:" + pwdObjHeight + "' ><div id='downloadAddress_" + passwordNum + "'><a id='downloadAddressURL' style='cursor: pointer;text-decoration: underline;' href='#' onclick='javasctipt:getDowloadAddress()'>点击下载安装安全控件</a></div><div  id='passwordInputArea_" + passwordNum + "'>"

    //IE浏览器		navigator.userAgent.indexOf("rv:11.0") > 0说明这个是IE11
    if (Prototype.Browser.IE || navigator.userAgent.indexOf("rv:11.0") > 0) {
        //补充:win7(NT 6.1), win8(NT 6.2), win8.1(NT 6.3)
        if (navigator.userAgent.indexOf("Windows NT 6.2") > 0 || navigator.userAgent.indexOf("Windows NT 6.3") > 0 || navigator.userAgent.indexOf("rv:11.0") > 0) {
            //是win8或者是IE11
            str += "<object  id=\"" + pwdObjId + "\" classid=\"" + classid + "\" codebase=\"cab_win81/" + codebase + "\#version=2,3,0,0\" width='0' height='0'> <param name=\"MinLength\" value=\"" + minLen + "\" >  <param name=\"MaxLength\" value=\"" + maxLen + "\" > <param name=\"Type\" value=\"" + typeStr + "\" > <param name=\"Rule\" value=\"" + ruleStr + "\" >  </object>";
        } else {
            //非win8,且不是IE11
            str += "<object  id=\"" + pwdObjId + "\" classid=\"" + classid + "\" codebase=\"cab_win81/" + codebase + "\#version=2,1,0,0\" width='0' height='0'> <param name=\"MinLength\" value=\"" + minLen + "\" >  <param name=\"MaxLength\" value=\"" + maxLen + "\" > <param name=\"Type\" value=\"" + typeStr + "\" > <param name=\"Rule\" value=\"" + ruleStr + "\" >  </object>";
        }
    } else {
        //其他浏览器
        str += "<object  id=\"" + pwdObjId + "\"  type='" + activeName + "'style=\"background-color:white\"   width='0' height='0'> <param name=\"NextElemId\" value=\"" + nextElemId + "\" >  <param name=\"MinLength\" value=\"" + minLen + "\" > <param name=\"MaxLength\" value=\"" + maxLen + "\" > <param name=\"Type\" value=\"" + typeStr + "\" >  <param name=\"Rule\" value=\"" + ruleStr + "\" >  </object>";
    }

    if (pwdHiddenName != "" && pwdHiddenName != null) str += "<input type=\"hidden\" id=\"" + pwdHiddenName + "\"/>";
    str += "</div></div>";
    document.write(str);//加载密码控件

    //加载密码控件(2014-06-09 屏蔽页面源码,屏蔽F12控制台,屏蔽另存为页面)
    createPwdObjectNewForCode();

    passwordNum++;
    //密码控件加载失败，则显示显示下载链接，IE浏览器支持cab包自动安装，所以不显示
    var num = passwordNum - 1;
    $("passwordInputArea_" + num).show();
    $("downloadAddress_" + num).hide();
    var pwdObj = $(pwdObjId);
    if (Prototype.Browser.IE || (flag == null || flag == false) || navigator.userAgent.indexOf("rv:11.0") > 0) {
        $("downloadAddress_" + num).hide();
        $("passwordInputArea_" + num).show();
        pwdObj.writeAttribute({ width: pwdObjWidth + 'px' });
        pwdObj.writeAttribute({ height: pwdObjHeight + 'px' });
    } else {
        var pwdObj = $(pwdObjId).remove();
        setTimeout(function () {
            document.body.appendChild(pwdObj);
            setTimeout(function () {
                try {
                    $(pwdObjId).GetPL();
                    var verSionflag = checkPasswordVersion(pwdObjId);
                    var pwdObj = $(pwdObjId).remove();
                    pwdObj.writeAttribute({ width: pwdObjWidth + 'px' });
                    pwdObj.writeAttribute({ height: pwdObjHeight + 'px' });
                    $("passwordInputArea_" + num).appendChild(pwdObj);
                    $("passwordInputArea_" + num).show();
                    if (verSionflag == '1') {
                        $("downloadAddress_" + num).show();
                        $("passwordInputArea_" + num).hide();
                    }
                } catch (e) {
                    $("downloadAddress_" + num).show();
                    $("passwordInputArea_" + num).hide();
                }
            }, 100);
        }, _delay);
    }
}

//Mac系统检查密码控件版本
function checkPasswordVersion(pwdObjId) {
    var verSionflag = '0';
    //判断Mac系统的密码控件是否为最新版本
    if (navigator.userAgent.indexOf("Macintosh") >= 0) {
        var version = $(pwdObjId).GetV();
        var versionArr = version.split(".");
        //若版本为2.3，则提示客户升级
        if (versionArr[0] == '2' && versionArr[1] == '3') {
            verSionflag = '1';
            showAlert("您的密码控件版本过旧，请升级为最新版！");
        }
    }
    return verSionflag;
}


function fetchRandomCode(response) {
    if (response.responseText.indexOf('<?xml') < 0) {
        alert('通讯失败，请重试');
        return;
    }
    var contextDataPos = response.responseXML.childNodes.length - 1;
    var contextData = new Liana.datatype.KeyedCollection(response.responseXML.childNodes[contextDataPos]);
    randomCode = contextData.getValueAt("flag");
}

//清空密码控件                   
function clearPwdObject(pwdObj) {
    try {
        pwdObj.Clear();
    } catch (e) {
    }
}

//校验密码控件
function checkPwdObject_login(pwdObj1, pwdObjTitle1, pwdHiddenObj, pwdIsPassObj) {
    // 得到密码加密后的密文
    var szres = GetPwd(pwdObj1);
    var len = szres.length - 4;
    pwdHiddenObj.value = szres.substring(0, len);
    // 密码长度校验代码
    var checkLen = szres.substring(len, len + 2);
    // 密码规则校验代码
    var checkRule = szres.substring(len + 2, len + 4);

    if (pwdIsPassObj) {
        pwdIsPassObj.value = "1";
        if (checkLen != "01") {
            pwdIsPassObj.value = "0";//长度不对要修改
        }
        if (checkRule != "01") {
            pwdIsPassObj.value = "0";//纯数字则要修改
        }
        if (doGetCSRule(pwdObj1, $("loginId").value) != "0") {
            pwdIsPassObj.value = "0";//符合简单密码规则要修改
        }
    }
    return true;
}

function checkPwdObject1(pwdObj1, pwdObjTitle1, pwdHiddenObj, ruleStr, lenMsg, userName) {
    var ruleMsg = "";

    if (ruleStr != "") {
        ruleMsg = getpwdFormatMsg(ruleStr);
    }

    // 得到密码加密后的密文
    var szres = GetPwd(pwdObj1);
    var len = szres.length - 4;

    // 密码长度校验代码
    var checkLen = szres.substring(len, len + 2);

    // 密码规则校验代码
    var checkRule = szres.substring(len + 2, len + 4);
    if (checkLen != "01" && lenMsg != "") {
        pwdAlert(pwdObjTitle1 + "长度为" + lenMsg + ",请重新输入");
        clearPwdObject(pwdObj1);
        return false;
    }
    if (checkRule != "01" && ruleStr != "") {
        pwdAlert(pwdObjTitle1 + ruleMsg + ",请重新输入");
        clearPwdObject(pwdObj1);
        return false;
    }
    if (userName) {
        var simple = doGetCSRule(pwdObj1, userName);
        if (simple != "0") {
            clearPwdObject(pwdObj1);
            pwdAlert(getSimpleMsg(simple));
            return false;
        }
    }

    pwdHiddenObj.value = szres.substring(0, len);//密码校验通过，则将加密后的密文赋值给pwdHiddenObj隐藏域                        
    return true;
}

function checkPwdObject2(pwdObj1, pwdObjTitle1, pwdObj2, pwdObjTitle2, pwdHiddenObj1, pwdHiddenObj2, ruleStr, lenMsg, userName) {
    var ruleMsg = "";

    if (ruleStr != "") {
        ruleMsg = getpwdFormatMsg(ruleStr);
    }

    // 得到密码加密后的密文
    var szres1 = GetPwd(pwdObj1);
    var len1 = szres1.length - 4;
    var checkLen1 = szres1.substring(len1, len1 + 2);
    var checkRule1 = szres1.substring(len1 + 2, len1 + 4);

    if (checkLen1 != "01" && lenMsg != "") {
        pwdAlert(pwdObjTitle1 + "长度为" + lenMsg + ",请重新输入"); clearPwdObject(pwdObj1); clearPwdObject(pwdObj2); return false;
    }

    if (checkRule1 != "01" && ruleStr != "") {
        pwdAlert(pwdObjTitle1 + ruleMsg + ",请重新输入"); clearPwdObject(pwdObj1); clearPwdObject(pwdObj2); return false;
    }

    // 得到密码加密后的密文 
    var szres2 = GetPwd(pwdObj2);

    if (!pwdObj1.IsEqual(pwdObj2)) {
        pwdAlert(pwdObjTitle1 + "与" + pwdObjTitle2 + "不一致!");
        clearPwdObject(pwdObj1);
        clearPwdObject(pwdObj2);
        return false;
    }

    if (userName) {
        var simple = doGetCSRule(pwdObj1, userName);
        if (simple != "0") {
            clearPwdObject(pwdObj1);
            clearPwdObject(pwdObj2);
            pwdAlert(getSimpleMsg(simple));
            return false;
        }
    }

    // 密码校验通过，则将加密后的密文赋值给pwdHiddenObj隐藏域
    pwdHiddenObj1.value = szres2.substring(0, len1);
    // 密码校验通过，则将加密后的密文赋值给pwdHiddenObj隐藏域
    pwdHiddenObj2.value = szres2.substring(0, len1);
    return true;
}

function checkPwdObject3(pwdObj1, pwdObjTitle1, pwdObj2, pwdObjTitle2, pwdObj3, pwdObjTitle3, pwdHiddenObj1, pwdHiddenObj2, pwdHiddenObj3, ruleStr, lenMsg, userName) {
    // 验证规则提示
    var ruleMsg = "";
    if (ruleStr != "") {
        ruleMsg = getpwdFormatMsg(ruleStr);
    }

    // 得到密码加密后的密文 
    var szres1 = GetPwd(pwdObj1);
    var len1 = szres1.length - 4;
    var checkLen1 = szres1.substring(len1, len1 + 2);
    var checkRule1 = szres1.substring(len1 + 2, len1 + 4);

    /*
	if(checkRule1!="01" && ruleStr!=""){
		pwdAlert(pwdObjTitle1+ruleMsg+",请重新输入");
		clearPwdObject(pwdObj1);
		return false;
	}
	*/

    // 得到密码加密后的密文
    var szres2 = GetPwd(pwdObj2);
    var len2 = szres2.length - 4;
    var checkLen2 = szres2.substring(len2, len2 + 2);
    var checkRule2 = szres2.substring(len2 + 2, len2 + 4);

    if (checkLen2 != "01" && lenMsg != "") {
        pwdAlert(pwdObjTitle2 + "长度为" + lenMsg + ",请重新输入");
        clearPwdObject(pwdObj2);
        clearPwdObject(pwdObj3);
        return false;
    }

    if (checkRule2 != "01" && ruleStr != "") {
        pwdAlert(pwdObjTitle2 + ruleMsg + ",请重新输入");
        clearPwdObject(pwdObj2);
        clearPwdObject(pwdObj3);
        return false;
    }

    // 得到密码加密后的密文
    var szres3 = GetPwd(pwdObj3);
    if (!pwdObj2.IsEqual(pwdObj3)) {
        pwdAlert(pwdObjTitle2 + "与" + pwdObjTitle3 + "不一致!");
        clearPwdObject(pwdObj2);
        clearPwdObject(pwdObj3);
        return false;
    }

    if (userName) {
        var simple = doGetCSRule(pwdObj2, userName);
        if (simple != "0") {
            clearPwdObject(pwdObj2);
            clearPwdObject(pwdObj3);
            pwdAlert(getSimpleMsg(simple));
            return false;
        }
    }

    // 密码校验通过，则将加密后的密文赋值给pwdHiddenObj1隐藏域
    pwdHiddenObj1.value = szres1.substring(0, len1);
    // 密码校验通过，则将加密后的密文赋值给pwdHiddenObj2隐藏域
    pwdHiddenObj2.value = szres2.substring(0, len2);
    // 密码校验通过，则将加密后的密文赋值给pwdHiddenObj3隐藏域
    pwdHiddenObj3.value = szres2.substring(0, len2);

    return true;
}

function getVersion(pwdObj1) {//获得版本
    try {
        var version_new = pwdObj1.GetV();
        if (version_now != version_new) { return false; } else { return true; }
    } catch (e) {
        return false;
    }
}

function doGetCSRule(pwdObj1, userName) {//简单密码
    var res = pwdObj1.CSRule(userName);
    var simpleRes = '0';
    if (res != '0000000000') {
        if (res.substring(5, 6) != '0') {//判断登录名密码是否一致
            simpleRes = '1';
        } else if (res.substring(6, 9) != '000') {
            simpleRes = '2';
        } else if (res.substring(9, 10) != '0') {
            simpleRes = '3';
        }
    }
    return simpleRes;
}

function getSimpleMsg(simpleValue) {//密码提示
    if (simpleValue == '1') {
        return "您设置的网银密码与登录名相同，为了您的账户安全，请重新输入";
    } else if (simpleValue == '2') {
        return "您设置的网银密码与登录名有连续的相同部分，为了您的账户安全，请重新输入";
    } else if (simpleValue == '3') {
        return "您设置的网银密码存在连续或相同的数字或字母，为了您的账户安全，请重新输入";
    }
}

//根据浏览器和操作系统选择不同的cab
function choiceCab(flag) {
    var res = new Array();
    if (flag == '1') {
        res[0] = 'CLSID:9FF156EE-F5C1-4FBD-A567-9F0BD113A072';
        res[1] = 'CgbEditx64.cab';
    } else if (flag == '2') {
        res[0] = 'CLSID:5157896D-FCA4-40C8-BFCF-34CD3BAEE25A';
        res[1] = 'CgbEditx86.cab';
    } else if (flag == '3') {
        res[0] = 'CLSID:5157896D-FCA4-40C8-BFCF-34CD3BAEE25A';
        res[1] = 'CgbEditx64x86.cab';
    }
    return res;
}

function pwdAlert(msg) {
    try {
        showAlert(msg);
    } catch (e) {
        alert(msg);
    }
}
/**
 * 
 * 获取用户密码控件信息
 * 
 * 使用例子:参数为密码控件ID和表单名
 * processClientInfo("passwordId","formName");
 * 
 **/
function processClientInfo(passwordId, formId) {
    try {
        var formObject = document.forms[formId];

        try {
            var clientIP = $(passwordId).GetClientInfo(1);
            var clientMacAdress = $(passwordId).GetClientInfo(2);
            var clientMainboardNo = $(passwordId).GetClientInfo(3);
            var clientHarddiskNo = $(passwordId).GetClientInfo(4);

            if (!formObject.clientIP) {
                var ipinput = document.createElement('input'); ipinput.name = 'clientIP'; ipinput.type = 'hidden';
                formObject.appendChild(ipinput);
            }
            if (!formObject.clientMacAdress) {
                var macinput = document.createElement('input'); macinput.name = 'clientMacAdress'; macinput.type = 'hidden';
                formObject.appendChild(macinput);
            }
            if (!formObject.clientMainboardNo) {
                var boardinput = document.createElement('input'); boardinput.name = 'clientMainboardNo'; boardinput.type = 'hidden';
                formObject.appendChild(boardinput);
            }
            if (!formObject.clientHarddiskNo) {
                var diskinput = document.createElement('input'); diskinput.name = 'clientHarddiskNo'; diskinput.type = 'hidden';
                formObject.appendChild(diskinput);
            }

            formObject.clientIP.value = clientIP.substr(0, clientIP.length - 4);
            formObject.clientMacAdress.value = clientMacAdress.substr(0, clientMacAdress.length - 4);
            formObject.clientMainboardNo.value = clientMainboardNo.substr(0, clientMainboardNo.length - 4);
            formObject.clientHarddiskNo.value = clientHarddiskNo.substr(0, clientHarddiskNo.length - 4);
        } catch (e) {
            formObject.clientIP.value = "";
            formObject.clientMacAdress.value = "";
            formObject.clientMainboardNo.value = "";
            formObject.clientHarddiskNo.value = "";
        }
    } catch (e) { }
    return;
}

/**
 * 
 * 获取用户密码控件信息ajax
 * 
 * 使用例子:参数为密码控件ID
 * processClientInfoAjax("passwordId");
 * 
 **/
function processClientInfoAjax(passwordId) {
    var params = { clientIP: "", clientMacAdress: "", clientMainboardNo: "", clientHarddiskNo: "" };
    try {
        var clientIP = $(passwordId).GetClientInfo(1);
        var clientMacAdress = $(passwordId).GetClientInfo(2);
        var clientMainboardNo = $(passwordId).GetClientInfo(3);
        var clientHarddiskNo = $(passwordId).GetClientInfo(4);
        params.clientIP = clientIP.substr(0, clientIP.length - 4);
        params.clientMacAdress = clientMacAdress.substr(0, clientMacAdress.length - 4);
        params.clientMainboardNo = clientMainboardNo.substr(0, clientMainboardNo.length - 4);
        params.clientHarddiskNo = clientHarddiskNo.substr(0, clientHarddiskNo.length - 4);
    } catch (e) { }
    return params;
}

/**
 * 
 * 获取密码2048或者1024位密码
 * 
 * @param pwdObj
 * @param randomCode
 */
function GetPwd(pwdObj) {
    if (randomCodeFlag == '0') {
        randomCodeFlag = '1';
        new Ajax.Request("/Bank/getPwdRandomCode?token=" + document.getElementById("current_token").value, {
            asynchronous: false, method: "post", parameters: {},
            onComplete: function (response) {
                if (response.responseText.indexOf('<?xml') < 0) {
                    alert('通讯失败，请重试');
                }
                var contextDataPos = response.responseXML.childNodes.length - 1;
                var contextData = new Liana.datatype.KeyedCollection(response.responseXML.childNodes[contextDataPos]);
                randomCode = contextData.getValueAt("flag");
                alert(randomCode);
            }
        }
		);
    }
    var password = "";
    var version = pwdObj.GetV();
    var versionArr = version.split(".");
    //新增版本号
    //if(((version == "2.3.0.0")||(version == "2.3.3.0")) && randomCode !=null && randomCode.length>=6){
    try {
        if (((versionArr[1] == "3") || (versionArr[1] == "4") || (versionArr[1] == "5")) && randomCode != null && randomCode.length >= 6) {
            password = pwdObj.GetRT2048(randomCode);
        } else {
            password = pwdObj.GetRT();
        }
    } catch (e) {
        password = pwdObj.GetRT();
    }
    return password;

}

//生成屏蔽页面源码的密码控件
function createPwdObjectNewForCode() {
    //方便关闭
    if ("true" != Liana.Constants.IS_CLOSE_SRCCODE) {
        return;
    }

    var resTemp = new Array(2);
    if (navigator.cpuClass != 'x86') {
        resTemp = choiceCab(1);//64位浏览器
    } else {
        if (navigator.appVersion.indexOf("WOW64;") == '-1') {
            resTemp = choiceCab(2);//32位浏览器，32系统
        } else {
            resTemp = choiceCab(3);//32位浏览器，64系统
        }
    }
    classidTemp = resTemp[0];
    //加载密码控件(2014-06-09 屏蔽页面源码,屏蔽F12控制台,屏蔽另存为页面)

    var activeNameTemp;
    if (navigator.userAgent.indexOf("Firefox") >= 0) {
        //火狐
        activeNameTemp = getActiveName(1);
    } else if (navigator.userAgent.indexOf("Safari") >= 0 && navigator.userAgent.indexOf("Macintosh") >= 0 && navigator.userAgent.indexOf("Chrome") < 0) {
        //其他(IE,火狐,谷歌之外的)  
        activeNameTemp = getActiveName(2);
    } else if (navigator.userAgent.indexOf("Chrome") >= 0) {
        //谷歌  
        activeNameTemp = getActiveName(3);
    }

    var strHide;
    //IE浏览器		navigator.userAgent.indexOf("rv:11.0") > 0说明这个是IE11
    if (Prototype.Browser.IE || navigator.userAgent.indexOf("rv:11.0") > 0) {
        //新密码控件的属性IsProtectSource(1:启用保护, 0:不启用), UserAgent把那个
        //为了兼容旧版本的密码控件,所以把从MinLength到IsPassword这5个属性都加上
        strHide = "<div  disabled='disabled' readonly='readonly' onfocus='this.blur()' tabindex='-1' style='position:absolute;top:-100px;'><object disabled='disabled' readonly='readonly' onfocus='this.blur()' tabindex='-1'  id=\"gdbedit\" classid=\"" + classidTemp + "\" width=0 height=0> <param name=\"IsProtectSource\" value=\"1\" > <param name=\"UserAgent\" value=\"" + navigator.userAgent + "\"> <param name=\"MinLength\" value=\"5\" > <param name=\"MaxLength\" value=\"3\" > <param name=\"Type\" value=\"E\" > <param name=\"Rule\" value=\"111|001||\" > <param name=\"IsPassword\" value=\"0\" > </object></div>";
        document.write(strHide);
    } else {
        //火狐和谷歌
        strHide = "<div disabled='disabled' readonly='readonly' onfocus='this.blur()' tabindex='-1' style='position:absolute;top:-100px;'><object disabled='disabled' readonly='readonly' onfocus='this.blur()' tabindex='-1' id=\"gdbedit\" type=\"" + activeNameTemp + "\" width='0' height='0'> <param name=\"IsProtectSource\" value=\"1\" > <param name=\"UserAgent\" value=\"" + navigator.userAgent + "\"> <param name=\"MinLength\" value=\"5\" > <param name=\"MaxLength\" value=\"3\" > <param name=\"Type\" value=\"E\" > <param name=\"Rule\" value=\"111|001||\" > <param name=\"IsPassword\" value=\"0\" > </object></div>";
        document.write(strHide);
    }
}

//集中管理,方便改
function getActiveName(flag) {
    if (flag == 1) {
        //火狐
        return "application/x-CgbEditFirefox-plugin";
    } else if (flag == 2) {
        //其他(IE,火狐,谷歌之外的)
        return "application/x-CgbEditMac-plugin";
    } else if (flag == 3) {
        //谷歌
        return "application/x-CgbEditChrome-plugin";
    }
}

