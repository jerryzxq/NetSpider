/******************************************************
//*功能描述:生成提交控件 IE使用
//*Author: Mazg
//*参数说明
//   basePath:路径
//		
***************************************************** */
Util.safeControlTypes = Util.safeControlTypes || {};
Util.safeControlUniqueID = Util.safeControlUniqueID || {};
function createSafeCommitControl(id) {
    id = id ? id : "safeCommit";
    if (Util.isMacOSRequest()) {
        createSafeCommitControl_mac(id);
    } else if (Util.safeInput && Util.oldSafeInput && Util.isIE()) {
        createSafeCommitControl_old(id);
    } else if (Util.isIE()) {
        createSafeCommitControl_new(id);
    }
    Util.mask = true;
}

function createSafeCommitControl_mac(id) {
    document.write('<embed name="' + id + '" id="' + id + '" width="1" height="1" ip="currentip" ip2="firstip" ip3="secondip" mac="currentmac" mac2="firstmac" mac3="secondmac" type="application/x-bcmsubmit-plugin"></embed>');
}

function createSafeCommitControl_old(id) {
    var apiName = Util.isIE64() ? "CLSID:EC8A3953-D2E9-404c-825A-AACCBC30B99E" : "CLSID:C391E12A-EAF1-45F1-8425-6E513C0D553C";
    var ocx = Util.contextPath + "ocx/x32.cab#version=1,0,0,1";
    if (Util.isWOW64()) {
        ocx = Util.contextPath + "ocx/x6432.cab#version=1,0,0,1";
    } else if (Util.is64()) {
        ocx = Util.contextPath + "ocx/x64.cab#version=1,0,0,1";
    }
    document.write('<object  classid="' + apiName + '" id="' + id + '"' + ' codebase=' + ocx + ' style="position:absolute" width=1 height=1></object>');
}

function createSafeCommitControl_new(id) {
    var apiName = "CLSID:08C1E20D-C14E-4F05-8A7E-A1794DAB456C";
    var ocx = Util.contextPath + "ocx/Bocomx32New.cab#version=3,0,0,0";
    if (Util.isWOW64()) {
        ocx = Util.contextPath + "ocx/Bocomx6432New.cab#version=3,0,0,0";
    } else if (Util.is64()) {
        ocx = Util.contextPath + "ocx/Bocomx64New.cab#version=3,0,0,0";
    }
    var codebase = Util.getSpecialOCX(ocx);
    document.write('<object  classid="' + apiName + '" id="' + id + '"' + codebase + ' style="position:absolute" width=1 height=1></object>');
}

/******************************************************
//*功能描述:生成密码控件
//*Author: Mazg
//*参数说明：
//	id: 控件id
//  fieldName: 密码域在form内的字段ID（名称同）
//	basePath:路径
//  pwdMaxSize: 密码最大长度
//  pwdMinSize: 密码最小长度
//	pWidth:显示宽度
//	pHeight:显示高度
//  uniqueID：随机码密文
//  tabIndex: tab序号（仅针对控件排序）
//  rule:密码规则
//	
***************************************************** */
function createSafeControl(id, fieldName, uniqueID, pwdMaxSize, pwdMinSize, pHeight, pWidth, tabIndex, rule, editType) {
    if (Util.isMacOSRequest()) {
        createSafeControl_mac(id, fieldName, uniqueID, pwdMaxSize, pwdMinSize, pHeight, pWidth, tabIndex, rule, editType);
    } else if (Util.safeInput && Util.oldSafeInput && Util.isIE()) {
        createSafeControl_old(id, fieldName, uniqueID, pwdMaxSize, pwdMinSize, pHeight, pWidth, tabIndex, rule, editType);
    } else {
        createSafeControl_new(id, fieldName, uniqueID, pwdMaxSize, pwdMinSize, pHeight, pWidth, tabIndex, rule, editType);
    }

}

function createSafeControl_mac(id, fieldName, uniqueID, pwdMaxSize, pwdMinSize, pHeight, pWidth, tabIndex, rule, editType) {
    var idCell = id + "_cell";
    if (!rule) {
        rule = '10111';
    }
    Util.safeControlUniqueID[id] = uniqueID;
    document.write("<div class='safeInput' id='" + idCell + "'>");
    document.write("<input type='text' style='width:1px;height:1px;border:none;' onfocus='this.blur();this.nextSibling.focus();'  tabindex='" + tabIndex + "'/>");

    document.write('<embed id="' + id + '" name="' + id + '"  width="' + pWidth + '" height="' + pHeight + '" MinLength="' + pwdMinSize + '" MaxLength="' + pwdMaxSize + '" EditName="' + fieldName + '" Rule="' + rule + '" EditType="' + editType + '" UniqueID="' + uniqueID + '" type="application/x-bcmxedit-plugin" class="safeControl" onkeydown="if(event.keyCode==9)this.nextSibling.focus();"></embed>');

    document.write("<input type='text' style='width:1px;height:1px;border:none;' onfocus='this.style.outline=0;this.blur();$(\"#input_captcha\").focus();' tabindex='" + tabIndex + "'/>");
    document.write("</div>");
    Util.safeInit[id] = function () { };

    setTimeout(function () {
        var npApiObject = getSafeControl(id);
        var isShowMask = false;
        var replaceMask = function () {
            $("#" + id).hide();
            var exePath = Util.contextPath + 'download/BocomEdit.2.0.dmg';
            $("#" + idCell).append('<a href="' + exePath + '" style="display:inline-block;zoom: 1;*display: inline;height:' + pHeight + 'px;width:' + pWidth + 'px;padding-left:2px;text-decoration:underline;color:red;cursor:hand;background-color:#fff;line-height:30px;" >请点击此处升级安全控件</a>');
            return;
        };
        try {
            isShowMask = npApiObject.IsAvailable() && npApiObject.GetCtrlVer() == '1.0.0.0';
        } catch (e) {
            setTimeout(function () {
                try {
                    isShowMask = npApiObject.IsAvailable() && npApiObject.GetCtrlVer() == '1.0.0.0';
                } catch (e) { }
                if (isShowMask) {
                    replaceMask();
                }
            }, 500);
        }
        if (isShowMask) {
            replaceMask();
        }
    }, 500);
}

function createSafeControl_old(id, fieldName, uniqueID, pwdMaxSize, pwdMinSize, pHeight, pWidth, tabIndex, rule, editType) {
    var idCell = id + "_cell";
    Util.safeControlTypes[id] = "0";
    if (Util.isIE()) {
        var apiName = Util.isIE64() ? "CLSID:C2C81750-6310-4be7-B9E2-7B1C61D9E2C4" : "CLSID:C00E3FF3-1F6A-423D-B5E0-17C198EF65E4";
        if (editType == "100") {
            apiName = Util.isIE64() ? "CLSID:FC6E1BC7-61BD-4031-A90E-AA934EA77553" : "CLSID:E83313B3-B1B9-4DB8-9E3B-AD6037DE0679";
        }
        var ocx = Util.contextPath + "ocx/x32.cab#version=1,0,0,1";
        if (Util.isWOW64()) {
            ocx = Util.contextPath + "ocx/x6432.cab#version=1,0,0,1";
        } else if (Util.is64()) {
            ocx = Util.contextPath + "ocx/x64.cab#version=1,0,0,1";
        }
        document.write('<object classid="' + apiName + '" id="' + id + '"' + ' codebase=' + ocx + ' width=' + pWidth + ' height=' + pHeight + ' tabindex="' + tabIndex + '" class="safeControl">');
        document.write('<param  name="EditName" value="' + fieldName + '">');
        document.write('<param  name="MaxLength" value="' + pwdMaxSize + '">');
        document.write('<param  name="MinLength" value="' + pwdMinSize + '">');
        if (rule != null) {
            document.write('<param  name="Rule" value="' + rule + '">');
        } else {
            document.write('<param  name="Rule" value="10111">');
        }
        document.write('<param  name="EditType" value="' + editType + '">');
        document.write('<param  name="UniqueID" value="' + uniqueID + '">');
        document.write('</object>');
        Util.safeInit[id] = function () { }
    } else {
        var version = "1.0.0.7"; // 安全控件版本号 每次更新安全控件版本的时候，同步更新该变量
        var apiName = "npOEdit";
        if (editType == "100") {
            apiName = "npXEdit";
        }
        document.write("<div class='safeInput' id='" + idCell + "'>");
        document.write("<input type='text' style='width:1px;height:1px;border:none;' onfocus='this.blur();this.nextSibling.setfocus();'  tabindex='" + tabIndex + "'/>");
        if (window.openex) {
            // 客户端
            document.write("<embed id='" + id + "' type='application/" + apiName + "." + version + "' style='display: inline; width: " + pWidth + "px; height: " + pHeight + "px;border:none;' class='safeControl'></embed>");
        } else {
            // 浏览器
            document.write("<embed id='" + id + "' type='application/" + apiName + "' style='display: inline; width: " + pWidth + "px; height: " + pHeight + "px;border:none;' class='safeControl'></embed>");
        }

        document.write("<input type='text' style='width:1px;height:1px;border:none;' onfocus='this.style.outline=0' tabindex='" + tabIndex + "'/>");
        document.write("</div>");
        Util.safeInit[id] = function () {
            initSafeControl(id, fieldName, editType, pwdMaxSize, pwdMinSize, uniqueID, rule);
        }
        try {
            Util.safeInit[id].call();
        } catch (e) {

        }
    }
    document.write('<input  name="' + fieldName + '" type="hidden" value=""/>');
    var npApiObject = getSafeControl(id);
    var isAvailable = false;
    try {
        if (Util.isIE()) {
            npApiObject.isAvailable();
            isAvailable = true;
        } else {
            isAvailable = !!npApiObject.isAvailable;
        }
    } catch (e) {
        isAvailable = false;
    }
    if (!isAvailable) {
        $("#" + id).hide();
        $("#" + idCell).hide();
        document.write('<a href="' + Util.contextPath + 'download/BocomSetupInput.exe" style="display:inline-block;zoom: 1;*display: inline;height:' + pHeight + 'px;width:' + pWidth + 'px;padding-left:2px;text-decoration:underline;color:red;cursor:hand;background-color:#fff;line-height:30px;" >请点击此处升级安全控件</a>');
        return;
    }
}

function createSafeControl_new(id, fieldName, uniqueID, pwdMaxSize, pwdMinSize, pHeight, pWidth, tabIndex, rule, editType) {
    var idCell = id + "_cell";
    Util.safeControlTypes[id] = "0";
    Util.safeControlUniqueID[id] = uniqueID;
    if (Util.isIE()) {
        var apiName = "CLSID:5D497C6A-B6C5-483A-AC6B-A02CE3D5A733";
        var ocx = Util.contextPath + "ocx/Bocomx32New.cab#version=3,0,0,0";
        if (Util.isWOW64()) {
            ocx = Util.contextPath + "ocx/Bocomx6432New.cab#version=3,0,0,0";
        } else if (Util.is64()) {
            ocx = Util.contextPath + "ocx/Bocomx64New.cab#version=3,0,0,0";
        }
        var codebase = Util.getSpecialOCX(ocx);
        document.write('<object classid="' + apiName + '" id="' + id + '"' + codebase + ' width=' + pWidth + ' height=' + pHeight + ' tabindex="' + tabIndex + '" class="safeControl">');
        document.write('<param  name="EditName" value="' + fieldName + '">');
        document.write('<param  name="MaxLength" value="' + pwdMaxSize + '">');
        document.write('<param  name="MinLength" value="' + pwdMinSize + '">');
        if (rule != null) {
            document.write('<param  name="Rule" value="' + rule + '">');
        } else {
            document.write('<param  name="Rule" value="10111">');
        }
        document.write('<param  name="EditType" value="' + editType + '">');
        document.write('<param  name="UniqueID" value="' + uniqueID + '">');
        document.write('</object>');
        Util.safeInit[id] = function () { }
    } else {
        var version = "1.0.0.7"; // 安全控件版本号 每次更新安全控件版本的时候，同步更新该变量
        var apiName = "npOEdit";
        if (editType == "100") {
            apiName = "npXEdit";
        }
        document.write("<div class='safeInput' id='" + idCell + "'>");
        document.write("<input type='text' style='width:1px;height:1px;border:none;' onfocus='this.blur();this.nextSibling.setfocus();'  tabindex='" + tabIndex + "'/>");
        if (window.openex) {
            // 客户端
            document.write("<embed id='" + id + "' type='application/" + apiName + "." + version + "' style='display: inline; width: " + pWidth + "px; height: " + pHeight + "px;border:none;' class='safeControl'></embed>");
        } else {
            // 浏览器
            document.write("<embed id='" + id + "' type='application/" + apiName + "' style='display: inline; width: " + pWidth + "px; height: " + pHeight + "px;border:none;' class='safeControl'></embed>");
        }

        document.write("<input type='text' style='width:1px;height:1px;border:none;' onfocus='this.style.outline=0' tabindex='" + tabIndex + "'/>");
        document.write("</div>");
        Util.safeInit[id] = function () {
            initSafeControl(id, fieldName, editType, pwdMaxSize, pwdMinSize, uniqueID, rule);
        }
        try {
            Util.safeInit[id].call();
        } catch (e) {

        }
    }
    document.write('<input  name="' + fieldName + '" type="hidden" value=""/>');
    var npApiObject = getSafeControl(id);
    var isAvailable = false;
    try {
        if (Util.isIE()) {
            npApiObject.isAvailable();
            isAvailable = true;
        } else {
            isAvailable = !!npApiObject.isAvailable;
        }
    } catch (e) {
        isAvailable = false;
    }
    if (!isAvailable) {
        $("#" + id).hide();
        $("#" + idCell).hide();
        var exePath = Util.contextPath + 'download/BocomSetupInput3.0.exe';
        document.write('<a href="' + exePath + '" style="display:inline-block;zoom: 1;*display: inline;height:' + pHeight + 'px;width:' + pWidth + 'px;padding-left:2px;text-decoration:underline;color:red;cursor:hand;background-color:#fff;line-height:30px;" >请点击此处升级安全控件</a>');
        return;
    }
}
/******************************************************
//*功能描述:初始化控件
//*Author: Mazg
//*参数说明：
//	id: 控件id
//	fieldName:数据域
//	maxLen:最大长度
//	minLen:最小长度
//	uniqueID:随机码密文	
//  rule:密码规则 
***************************************************** */

function initSafeControl(id, fieldName, editType, maxLen, minLen, uniqueID, rule) {
    var version = "1.0.0.7"; // 安全控件版本号 每次更新安全控件版本的时候，同步更新该变量
    var npApiObject = getSafeControl(id);
    if (!(rule != null))
        rule = "10111";
    if (npApiObject.checkVersion !== undefined && npApiObject.checkVersion() == version) {
        npApiObject.init(uniqueID, fieldName, rule, editType, maxLen, minLen, navigator.userAgent.toLowerCase());
    }
    var errCode = npApiObject.GetErrorCode();
    if (errCode != null && errCode != 0) {
        errorSafeControl(errCode);
    }

}
function getSafeControl(objectId) {
    return $('#' + objectId).get(0);
}

/******************************************************
//*功能描述:校验控件输入合法性
//*参数说明：
//	objectId: 控件ID
//	objectName：控件名称
***************************************************** */
function verifySafeControl(objectId, objectName) {
    if (Util.isMacOSRequest()) {
        verifySafeControl_mac(objectId, objectName);
    } else {
        verifySafeControl_new(objectId, objectName);
    }
}
function verifySafeControl_mac(objectId, objectName) {
    var safeControl = getSafeControl(objectId);
    var isValid = true;
    try {
        isValid = safeControl.IsValid();
    } catch (e) {
        throw new Error(Util.getMessage(Util.Message.tip.mountControl));
    }
    if (!isValid) {
        safeControl.focus();
        var errcode = 0;
        try {
            errcode = safeControl.GetErrorCode();
        } catch (e) {
            throw new Error(Util.getMessage(Util.Message.tip.mountControl));
        }
        errorSafeControl(errcode, objectName);
    }
}
function verifySafeControl_old(objectId, objectName) {
    var safeControl = getSafeControl(objectId);
    if (safeControl.isValid()) {
        safeControl.focus();
        errorSafeControl(8, fields[key]);
    }
}
function verifySafeControl_new(objectId, objectName) {
    var cryptObject = getSafeControl(objectId);
    var valid = true;
    if (Util.isIE()) {
        try {
            valid = cryptObject.isValid();
        } catch (e) {
            throw new Error(Util.getMessage(Util.Message.tip.mountControl));
        }
    } else {
        try {
            valid = cryptObject.isValid;
        } catch (e) {
            throw new Error(Util.getMessage(Util.Message.tip.mountControl));
        }
    }
    if (!valid) {
        cryptObject.focus();
        var errcode = 0;
        try {
            errcode = cryptObject.GetErrorCode();
        } catch (e) {
            throw new Error(Util.getMessage(Util.Message.tip.mountControl));
        }
        errorSafeControl(errcode, objectName);
    }
}
/******************************************************
//*功能描述:密码控件提交(单个)
//*Author: lanxb
//*参数说明：
//  fields: 提交控件键值对集合
//  submitFormID: 包含密码控件的form的ID
//	
***************************************************** */
var xPEBSClickBoolean = true;
function submitSafeControl(fields, submitFormID, id) {
    id = id ? id : "safeCommit";
    if (Util.isMacOSRequest()) {
        submitSafeControl_mac(fields, submitFormID, id);
    } else if (Util.safeInput && Util.oldSafeInput && Util.isIE()) {
        submitSafeControl_old(fields, submitFormID, id);
    } else {
        submitSafeControl_new(fields, submitFormID, id);
    }
}

function submitSafeControl_mac(fields, submitFormID, id) {
    var submitForm = document.forms[submitFormID];
    try {
        var safeCommit = getSafeControl(id);
        safeCommit.Reset();
        for (var key in fields) {
            var safeControl = getSafeControl(key);
            try {
                var uniqueId = Util.safeControlUniqueID[key];
                var safeInputDeviceInfo = safeControl.GetDeviceInfo(uniqueId);
                if (safeInputDeviceInfo) {
                    $(submitForm).children("input[name='safeInputDeviceInfo']").remove();
                    $(submitForm).append('<input type="hidden" name="safeInputDeviceInfo"/>');
                    $(submitForm).children("[name='safeInputDeviceInfo']").val(safeInputDeviceInfo);
                }
            } catch (e) {
                // do nothing
            }
            $(submitForm).children("input[name=" + key + "]").remove();
            if (!safeControl.CommitSM4(1, safeCommit)) {
                safeControl.focus();
                errorSafeControl(safeControl.GetErrorCode(), fields[key]);
            }
        }
        if (!submitForm.target) {
            if (!xPEBSClickBoolean) {
                return;
            }
            $(window.lastBaseClkBtn).attr("disabled", true).parent().addClass("gray");
            xPEBSClickBoolean = false;
        }
        if (!safeCommit.Submit(submitForm)) {
            errorSafeControl(safeCommit.GetErrorCode());
        }
    } catch (e) {
        Util.error(e.message);
        throw e;
    }
}

function submitSafeControl_old(fields, submitFormID, id) {
    alert("submitSafeControl_old");
    var submitForm = document.forms[submitFormID];
    try {
        if (Util.isIE()) {
            var safeCommit = getSafeControl(id);
            safeCommit.reset();
            //IE提交
            for (var key in fields) {
                var safeControl = getSafeControl(key);
                alert("safeControl" + safeControl.CypherText);
                $(submitForm).children("input[name=" + key + "]").remove();
                if (!safeControl.commit(safeCommit)) {
                    safeControl.focus();
                    errorSafeControl(safeControl.GetErrorCode(), fields[key]);
                }
            }
            if (!submitForm.target) {
                if (!xPEBSClickBoolean) {
                    return;
                }
                $(window.lastBaseClkBtn).attr("disabled", true).parent().addClass("gray");
                xPEBSClickBoolean = false;
            }
            if (!safeCommit.submit(submitForm)) {
                errorSafeControl(safeCommit.GetErrorCode());
            }
        } else {
            //非IE
            for (var key in fields) {
                var safeControl = getSafeControl(key);
                if (!safeControl.isValid) {
                    safeControl.focus();
                    errorSafeControl(safeControl.GetErrorCode(), fields[key]);
                }
                submitForm[key].value = safeControl.CypherText;
            }
            if (!submitForm.target) {
                if (xPEBSClickBoolean) {
                    xPEBSClickBoolean = false;
                    $(window.lastBaseClkBtn).attr("disabled", true).parent().addClass("gray");
                } else {
                    return;
                }
            }
            submitForm.submit();
        }
    } catch (e) {
        Util.error(e.message);
        throw e;
    }
}

function submitSafeControl_new(fields, submitFormID, id) {
    var submitForm = document.forms[submitFormID];
    try {
        if (Util.isIE()) {
            var safeCommit = getSafeControl(id);
            safeCommit.reset();
            //IE提交
            for (var key in fields) {
                var safeControl = getSafeControl(key);
                try {
                    var uniqueId = Util.safeControlUniqueID[key];
                    var safeInputDeviceInfo = safeControl.GetDeviceInfo(uniqueId);
                    if (safeInputDeviceInfo) {
                        $(submitForm).children("input[name='safeInputDeviceInfo']").remove();
                        $(submitForm).append('<input type="hidden" name="safeInputDeviceInfo"/>');
                        $(submitForm).children("[name='safeInputDeviceInfo']").val(safeInputDeviceInfo);
                    }
                } catch (e) {
                    // do nothing
                }
                $(submitForm).children("input[name=" + key + "]").remove();
                if (!safeControl.CommitSM4(1, safeCommit)) {
                    safeControl.focus();
                    errorSafeControl(safeControl.GetErrorCode(), fields[key]);
                }
            }
            if (!submitForm.target) {
                if (!xPEBSClickBoolean) {
                    return;
                }
                $(window.lastBaseClkBtn).attr("disabled", true).parent().addClass("gray");
                xPEBSClickBoolean = false;
            }
            if (!safeCommit.submit(submitForm)) {
                errorSafeControl(safeCommit.GetErrorCode());
            }
        } else {
            //非IE
            for (var key in fields) {
                var safeControl = getSafeControl(key);
                if (!safeControl.isValid) {
                    safeControl.focus();
                    errorSafeControl(safeControl.GetErrorCode(), fields[key]);
                }
                submitForm[key].value = safeControl.CypherText;
            }
            if (!submitForm.target) {
                if (xPEBSClickBoolean) {
                    xPEBSClickBoolean = false;
                    $(window.lastBaseClkBtn).attr("disabled", true).parent().addClass("gray");
                } else {
                    return;
                }
            }
            submitForm.submit();
        }
    } catch (e) {
        Util.error(e.message);
        throw e;
    }
}

function commitSafeControl(fields, submitForm, id) {
    id = id ? id : "safeCommit";
    if (Util.isMacOSRequest()) {
        commitSafeControl_mac(fields, submitForm, id);
    } else if (Util.safeInput && Util.oldSafeInput && Util.isIE()) {
        commitSafeControl_old(fields, submitForm, id);
    } else {
        commitSafeControl_new(fields, submitForm, id);
    }
}

function commitSafeControl_mac(fields, submitForm, id) {
    try {
        var safeCommit = getSafeControl(id);
        safeCommit.reset();
        //IE提交
        for (var key in fields) {
            var safeControl = getSafeControl(key);
            $(submitForm).children("input[name=" + key + "]").remove();
            if (!safeControl.CommitSM4(1, safeCommit)) {
                safeControl.focus();
                errorSafeControl(safeControl.GetErrorCode(), fields[key]);
            }
        }
    } catch (e) {
        Util.error(e.message);
    }
}

function commitSafeControl_old(fields, submitForm, id) {
    try {
        if (Util.isIE()) {
            var safeCommit = getSafeControl(id);
            safeCommit.reset();
            //IE提交
            for (var key in fields) {
                var safeControl = getSafeControl(key);
                $(submitForm).children("input[name=" + key + "]").remove();
                if (!safeControl.commit(safeCommit)) {
                    safeControl.focus();
                    errorSafeControl(safeControl.GetErrorCode(), fields[key]);
                }
            }
        } else {
            //非IE
            for (var key in fields) {
                var safeControl = getSafeControl(key);
                if (!safeControl.isValid) {
                    safeControl.focus();
                    errorSafeControl(safeControl.GetErrorCode(), fields[key]);
                }
                submitForm[key].value = safeControl.CypherText;
            }
        }
    } catch (e) {
        Util.error(e.message);
    }
}
function commitSafeControl_new(fields, submitForm, id) {
    try {
        if (Util.isIE()) {
            var safeCommit = getSafeControl(id);
            safeCommit.reset();
            //IE提交
            for (var key in fields) {
                var safeControl = getSafeControl(key);
                $(submitForm).children("input[name=" + key + "]").remove();
                if (!safeControl.CommitSM4(1, safeCommit)) {
                    safeControl.focus();
                    errorSafeControl(safeControl.GetErrorCode(), fields[key]);
                }
            }
        } else {
            //非IE
            for (var key in fields) {
                var safeControl = getSafeControl(key);
                if (!safeControl.isValid) {
                    safeControl.focus();
                    errorSafeControl(safeControl.GetErrorCode(), fields[key]);
                }
                submitForm[key].value = safeControl.CypherText;
            }
        }
    } catch (e) {
        Util.error(e.message);
    }
}
/******************************************************
//*功能描述:翻译错误码信息
//*参数说明：
//	errCode: 错误码
//	name：控件名称
***************************************************** */
function errorSafeControl(errCode, name) {
    name = name ? name : Util.getMessage(Util.Message.tip.password);
    if (errCode == 1) {
        throw new Error(Util.getMessage(Util.Message.tip.notURL, [name]));
    } else if (errCode == 2) {
        throw new Error(Util.getMessage(Util.Message.tip.debugAndOpen, [name]));
    } else if (errCode == 3) {
        throw new Error(Util.getMessage(Util.Message.tip.runException, [name]));
    } else if (errCode == 4) {
        throw new Error(Util.getMessage(Util.Message.tip.runException, [name]));
    } else if (errCode == 5) {
        throw new Error(Util.getMessage(Util.Message.tip.startException, [name]));
    } else if (errCode == 6) {
        throw new Error(Util.getMessage(Util.Message.tip.startException, [name]));
    } else if (errCode == 7) {
        throw new Error(Util.getMessage(Util.Message.tip.startException, [name]));
    } else if (errCode == 8) {
        throw new Error(Util.getMessage(Util.Message.tip.inputRight, [name]));
    } else if (errCode == 9) {
        throw new Error(Util.getMessage(Util.Message.tip.runException, [name]));
    } else if (errCode == 10) {
        throw new Error(Util.getMessage(Util.Message.tip.runException, [name]));
    } else if (errCode == 11) {
        throw new Error(Util.getMessage(Util.Message.tip.runException, [name]));
    } else if (errCode == 12) {
        throw new Error(Util.getMessage(Util.Message.tip.runAndRefresh, [name]));
    } else {
        throw new Error(Util.getMessage(Util.Message.tip.inputRight, [name]));
    }
}

/******************************************************
//*功能描述:页面重置、清空（包括IE浏览器交易密码）
//*参数说明：
//	passwId: 交易密码id
//	formName：需要清空的表单名称
***************************************************** */
function resetForm(passwId, formName) {
    /**
	 * 清除密码
	 */
    if (Util.isMacOSRequest()) {
        if ($.isArray(passwId)) {
            $.each(passwId, function (index, item) {
                getSafeControl(item).Clear();
            });
        } else {
            getSafeControl(passwId).Clear();
        }
    } else if (Util.isIE()) {
        if ($.isArray(passwId)) {
            $.each(passwId, function (index, item) {
                document[item].Clear();
            });
        } else {
            document[passwId].Clear();
        }
    } else {
        if ($.isArray(passwId)) {
            $.each(passwId, function (index, item) {
                document[item].clear();
            });
        } else {
            document[passwId].clear();
        }
    }
    // 表单重置
    if (formName) {
        document.forms[formName].reset();
    }
}
