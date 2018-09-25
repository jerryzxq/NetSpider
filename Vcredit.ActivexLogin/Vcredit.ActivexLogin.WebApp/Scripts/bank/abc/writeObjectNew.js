/*
**旧版本仅有Windows系统下控件，新版本包括Windows系统下控件、插件，Mac系统下插件
*/
(function (window) {
    var WIN_NEW_32_ACTIVEX_VERSION = 0x10160726; //新版本，Windows系统下 32位控件版本号
    var WIN_NEW_64_ACTIVEX_VERSION = 0x10160726; //新版本，Windows系统下 64位控件版本号
    var WIN_PLUGIN_VERSION = 0x10160726; //Windows系统下插件版本号
    var MAC_PLUGIN_VERSION = 0x10170224; //Mac系统下插件版本号
    //var WIN_SETUP_PATH = "/InfoSec/SafeInputABC_x86.exe"; //Windows安装程序下载路径
    //var WIN_64_SETUP_PATH = "/InfoSec/SafeInputABC_x64.exe";
    //var MAC_SETUP_PATH = "/InfoSec/ABCSafeSoftKeyboardInputPlugin.pkg"; //Mac系统下插件安装程序下载路径

    var WIN_SETUP_PATH = "http://ebhelper.abchina.com/ebhelper/Data/PowerEnterABC32.exe"; //Windows安装程序下载路径
    var WIN_64_SETUP_PATH = "http://ebhelper.abchina.com/ebhelper/Data/PowerEnterABC64.exe";
    var MAC_SETUP_PATH = "http://ebhelper.abchina.com/ebhelper/Data/PowerEnterABC.pkg"; //Mac系统下插件安装程序下载路径
    var DEFAULT_SETUP_PATH = "http://ebhelper.abchina.com/ebhelper/Data/PowerEnterABC.exe"; //32和64位整合版本

    var LocalObjVersion = 0; //已经安装的控件或插件的版本号，使用getLocalObjVersion获取

    //新版本控件的clasid和名称
    var NewPassCtrlClsid = "clsid:E5752D5E-5BFB-4d04-AEFB-3601B8BF2D23";
    var NewCtlName = "ABCSAFEINPUTAPP.ABCSafeInputAppCtrl.1";

    //插件信息
    var MIME = "application/npABCSafeInputApp";
    var PluginDescription = "npABCsafeinputapp";

    var OsType = "Win32"; //操作系统类型，isRegisterediSecurity函数设置
    var IsIE = true; //是否为IE浏览器，仅在OsType为Win32或Win64时此值才可能为真，isRegisterediSecurity函数设置
    var IsInstalled = false; //浏览器是IE且控件安装时为真，或浏览器为非IE浏览器且插件已安装时为真，isRegisterediSecurity函数设置
    var IsNewCtlInstalled = false; //控件是否安装标志，仅在IsIE为true是此值才可能为真，isRegisterediSecurity函数设置
    var IsPluginInstalled = false; //插件是否安装标志，IsIE为false时此值才可能为真，isRegisterediSecurity函数设置
    var IsNewest = false; //IsInstalled为真且版本为最新时为真，setPEXSetupUrl函数设置

    //控件或插件公钥
    //为了保证安全，Windows版本和Mac版本要使用不同的密钥
    var _newpk = "JuqKOTAgjv2RMvccUQqrV0OLPbwn0wTnmOzK8qDqdOt1ANnP8w5jEBXHc3KOjCUJOAoi4edCtqugnc+FfELM6g==";
    var _newmacpk = "fPIjtLh7+Tow+rI6GhxcNw/t/a6mnWiW81EShAiz1I5F8SiGC9JaUSV27jUTmMyZCSbSC0DBTRndA+VWcoIyhQ==";

    var divId = "PowerEnterDiv";
    var objId = "powerpass_ie";
    var ComSpanId = "Msg";
    var cspName = "请插入K宝";
    var CspValue;
    var hasFocused = false;

    var EventReturn = function () { }

    //Tab事件，定位到powerpass_ie的父节点的下一个兄弟节点
    var EventTab = function () {
    }

    //Blur事件，校验密码是否符合要求
    var EventBlur = function (id, errorSpanId) {
        var obj = PEGetElement(id);
        var errorId = id + "Msg";
        if (errorSpanId != "undefined" && errorSpanId != "") {
            errorId = errorSpanId;
        }
        if (obj.hasFocusOn()) {
            hasFocused = true;    //表示控件是否获得过焦点，获得过焦点之后，才触发事件，否则不理会
            return;   //光标离开了元素，但是此时控件还有焦点，说明此事件是web触发，不予理会
        }
        if (!hasFocused) {
            return;   //当前还未被触发过
        }
        var reslt = obj.verify();
        if (reslt == 0) {
            PEGetElement(errorId).innerHTML = "";
        } else if (reslt < 0 && reslt != -4) {
            var error;
            if (reslt == -1) {
                error = "密码内容不能为空";
            } else if (reslt == -2) {
                error = "密码输入长度不足";
            } else if (reslt == -3) {
                error = "密码输入内容不合法";
            } else if (reslt == -4) {
                error = "输入为简单密码";
            } else {
                error = obj.lastError();
            }
            PEGetElement(errorId).innerHTML = error;
            return false;
        }
        return true;
    }

    //控件默认属性
    //注意：为了保持一致，lang和languages最好都设置下。旧安全控件使用的是lang属性，由于在IE中与DOM的重名，新控件中改名为languages
    var defaults = {
        "width": 150,
        "height": 32,
        "maxLength": 6,
        "minLength": 6,
        "maskChar": "*",
        "backColor": "#FFFFFF",
        "textColor": "#0000FF",
        "borderColor": "#cccccc",
        "accepts": "^[0-9]*$",
        "caption": "中国农业银行",
        "captionColor": "#87011f",
        "captionFont": "Microsoft YaHei",
        "captionSize": 0,
        "captionBold": "true",
        "lang": "zh_CN",
        "languages": "zh_CN",
        "softKeyboard": "true",
        "softkbdType": 1,
        "promptText": " 请输入密码",
        "wmode": "transparent",
        "textFont": "weight:20 width:5 height:20 CharSet:0 OutputPrecision:0 ClipPrecision:0 OutputQuality:0 FontName:微软雅黑"
    };

    //K宝登陆页面专用样式
    var defaultsLogin = {
        "width": 262,
        "height": 36,
        "maxLength": 8,
        "minLength": 8,
        "maskChar": "*",
        "backColor": "#FFFFFF",
        "textColor": "#0000FF",
        "borderColor": "#cccccc",
        "accepts": "[A-Za-z0-9-]{1,30}",
        "caption": "中国农业银行",
        "captionColor": "#87011f",
        "captionFont": "Arial",
        "captionSize": 0,
        "captionBold": "true",
        "lang": "zh_CN",
        "languages": "zh_CN",
        "softKeyboard": "true",
        "softkbdType": 0,
        "promptText": " 请输入密码",
        "wmode": "transparent",
        "textFont": "weight:20 width:5 height:20 CharSet:0 OutputPrecision:0 ClipPrecision:0 OutputQuality:0 FontName:微软雅黑"
    };
    //一代key是否插入标识
    var hFlag = false;
    //一代key驱动名称列表
    var cspNameList = new Array(
        "EnterSafe ePassAuto CSP For ABChina v1.0",
        "Tendyron OnKey 193I3B CSP ABC v1.0",
        "ABC SAFE CSP v3.2",
        "ComyiSafe CSP V3.0",
        "ZGHD Cryptographic Service Provider v1.0")

    function powerConfig(args) {

        for (var p in args)
            if (args[p] != null) defaults[p] = args[p];
        return defaults;
    }
    function powerConfigLogin(args) {

        for (var p in args)
            if (args[p] != null) defaultsLogin[p] = args[p];
        return defaultsLogin;
    }


    //获取浏览器类型，可能的返回值有IE、Chrome、Safari、Firefox、unknown

    function getBrowserType() {
        var ua = navigator.userAgent;
        if (navigator.appName == "Microsoft Internet Explorer" || ua.indexOf("Trident") > -1) {
            return "IE";
        } else if (ua.indexOf("Chrome") > -1) {
            return "Chrome";
        } else if (ua.indexOf("Safari") > -1) {
            return "Safari";
        } else if (ua.indexOf("Firefox") > -1) {
            return "Firefox";
        } else {
            return "unknown";
        }
    }

    //获取操作系统类型，可能的返回值有Win32、Win64、Mac、Linux、unknown

    function getOsType() {
        var plat = navigator.platform;
        if ((plat == "Win32") || (plat == "Windows")) {
            return "Win32";
        } else if (plat == "Win64") {
            return "Win64";
        } else if ((plat == "Mac68K") || (plat == "MacPPC") || (plat == "Macintosh") || (plat == "MacIntel")) {
            return "Mac";
        } else if (String(plat).indexOf("Linux") > -1) {
            return "Linux";
        } else {
            return "unknown";
        }
    }

    //该函数应首先被调用，以设定全局参数OsType、IsIE、IsNewCtlInstalled、IsPluginInstalled、IsInstalled
    //为了兼容原有调用方式，该函数同时将IsInstalled值返回

    function isRegisterediSecurity() {
        OsType = getOsType();

        if (getBrowserType() == "IE") {
            IsIE = true;
            try {
                new ActiveXObject(NewCtlName);
                IsNewCtlInstalled = true;
                IsInstalled = true;
            } catch (e) {
                IsNewCtlInstalled = false;
                IsInstalled = false;
            }
        } else {
            IsIE = false;

            var powerEnterPlugin = navigator.plugins[PluginDescription];
            if (powerEnterPlugin == null) {
                IsPluginInstalled = false;
                IsInstalled = false;
            } else {
                IsPluginInstalled = true;
                IsInstalled = true;
            }
        }

        return IsInstalled;
    }

    function writePluginObject(oid, clsid, cfg, errorSpanId) {

        document.write('<object id="' + oid + '" wmode="transparent" type="' + clsid
        + '" width="' + cfg.width + '" height="' + cfg.height
        + '" style="z-index:inherit;vertical-align:middle;width:' + cfg.width + 'px;height:' + cfg.height + 'px" onblur = "password.EventBlur(\'' + oid + '\',\'' + errorSpanId + '\')">');
        for (var name in cfg)
            document.write('<param name="' + name + '" value="' + cfg[name] + '">');
        document.write('</object><span id = ' + oid + "Msg" + ' class = "pwdError"></span>');
    }

    //为非IE浏览器绘制一个隐藏的输入框, 宽度和高度都设成0
    function writeHiddenPluginObject(oid, clsid, cfg) {
        document.write('<div style="height:0px;"><object id="' + oid + '" type="' + clsid
        + '"width="0px" height="0px" style="width:0px; height:0px;">');
        document.write('</object></div>');
    }

    function writePluginObjectForHtml(divId, oid, clsid, cfg) {
        var htmlString = '<object id="' + oid + '"  wmode="transparent" type="' + clsid
        + '" width="' + cfg.width + '" height="' + cfg.height
        + '" style="z-index:inherit;vertical-align:middle;width:' + cfg.width + 'px;height:' + cfg.height + 'px">';
        for (var name in cfg)
            htmlString += '<param name="' + name + '" value="' + cfg[name] + '">';
        htmlString += '</object><span id = "pwdMsg" class = "pwdError"></span>';

        document.getElementById(divId).innerHTML = htmlString;
    }

    function writeObject(oid, clsid, cfg, errorSpanId) {
        document.write('<object id="' + oid + '" classid="' + clsid
          + '" style="z-index:inherit;vertical-align:middle;" width="' + cfg.width + '" height="' + cfg.height + '" onblur = "password.EventBlur(\'' + oid + '\',\'' + errorSpanId + '\')">');
        for (var name in cfg)
            document.write('<param name="' + name + '" value="' + cfg[name] + '">');
        document.write('</object><span id = ' + oid + "Msg" + ' class = "pwdError"></span>');
    }

    //为IE绘制隐藏输入框

    function writeHiddenObject(oid, clsid, cfg) {
        document.write('<div style="display:none;"><object id="' + oid + '" classid="' + clsid
            + '" width="0" height="0" style="display : none">');
        document.write('</object></div>');
    }

    //该方法用于解决页面嵌套以及load的问题

    function writeObjectForHtml(divId, oid, clsid, cfg) {
        var htmlString = '<object id="' + oid + '" classid="' + clsid
          + '" style="z-index:inherit;vertical-align:middle;" width="' + cfg.width + '" height="' + cfg.height + '">';
        for (var name in cfg)
            htmlString += '<param name="' + name + '" value="' + cfg[name] + '">';
        htmlString += '</object><span id = ' + oid + "Msg" + ' class = "pwdError"></span>';

        document.getElementById(divId).innerHTML = htmlString;
    }


    //专门供K宝登陆页面使用
    function writePassObjectLogin(oid, cfg) {
        if (!oid || typeof (oid) != "string") {
            alert("writePassObj Failed: oid are required!");
        } else {
            setPEXSetupUrl(oid);
            if (IsNewest) {
                if (IsIE) {
                    writeObject(oid, NewPassCtrlClsid, powerConfigLogin(cfg));
                    if (navigator.userAgent.indexOf("MSIE") > -1) {
                        document.getElementById(oid).attachEvent("EventTab", EventTab, true);
                    } else {
                        document.getElementById(oid).addEventListener("EventTab", EventTab, true);
                    }
                } else {
                    writePluginObject(oid, MIME, powerConfigLogin(cfg));
                    document.getElementById(oid).addEventListener("EventTab", EventTab, true);
                }
            }
        }
    }

    //该函数调用前需保证已完整了最新的控件/插件，该工作由setPEXSetupUrl函数完成

    function writePassObjectOrg(oid, cfg, errorSpanId) {
        if (!oid || typeof (oid) != "string") {
            alert("writePassObj Failed: oid are required!");
        } else {
            setPEXSetupUrl(oid);
            if (IsNewest) {
                if (IsIE) {
                    writeObject(oid, NewPassCtrlClsid, powerConfig(cfg), errorSpanId);
                    if (navigator.userAgent.indexOf("MSIE") > -1) {
                        document.getElementById(oid).attachEvent("EventTab", EventTab, true);
                    } else {
                        document.getElementById(oid).addEventListener("EventTab", EventTab, true);
                    }
                } else {
                    writePluginObject(oid, MIME, powerConfig(cfg), errorSpanId);
                    document.getElementById(oid).addEventListener("EventTab", EventTab, true);
                }
            }
        }
    }

    //对于网银内大部分交易，都采取默认样式，且名称强制定为powerpass_ie

    function writePassObject(random) {
        setPEXSetupUrl(objId);
        if (IsNewest) {
            if (IsIE) {
                writeObject(objId, NewPassCtrlClsid, powerConfig({ "randomText": random }));
                if (navigator.userAgent.indexOf("MSIE") > -1) {
                    document.getElementById(objId).attachEvent("EventTab", EventTab, true);
                    document.getElementById(objId).attachEvent("EventReturn", EventReturn, true);

                } else {
                    document.getElementById(objId).addEventListener("EventTab", EventTab, true);
                    document.getElementById(objId).addEventListener("EventReturn", EventReturn, true);
                }
            } else {
                writePluginObject(objId, MIME, powerConfig({ "randomText": random }));
                document.getElementById(objId).addEventListener("EventTab", EventTab, true);
                document.getElementById(objId).addEventListener("EventReturn", EventReturn, true);
            }
        }

    }

    //多密码框时，采用这个方法，传密码框序号即可，如：writeMultiPassObject(0, random)

    function writeMultiPassObject(oid, random) {
        setPEXSetupUrl(objId + oid);
        if (IsNewest) {
            if (IsIE) {
                writeObject(objId + oid, NewPassCtrlClsid, powerConfig({ "randomText": random }));
            } else {
                writePluginObject(objId + oid, MIME, powerConfig({ "randomText": random }));
            }
        }
    }

    //多密码框，同时又需要自己定制样式

    function writeMultiPassObjectOrg(oid, cfg) {
        setPEXSetupUrl(objId + oid);
        if (IsNewest) {
            if (IsIE) {
                writeObject(objId + oid, NewPassCtrlClsid, powerConfig(cfg));
            } else {
                writePluginObject(objId + oid, MIME, powerConfig(cfg));
            }
        }
    }

    //该接口不判断控件/插件是否安装，直接写脚本，如果要调用控件/插件的函数，需保证已经安装有控件/插件

    function writeUtilObject(oid, cfg) {
        if (!oid || typeof (oid) != "string") {
            alert("writePassObj Failed: oid are required!");
        } else {
            if (IsIE) {
                writeHiddenObject(oid, NewPassCtrlClsid, powerConfig(cfg));
            } else {
                writeHiddenPluginObject(oid, MIME, powerConfig(cfg));
            }
        }
    }

    function writePassObjectHtml(random) {
        //如果传入第二个参数，则第二个参数作为控件所在的层的id

        if (writePassObjectHtml.arguments[1] != null) {
            divId = writePassObjectHtml.arguments[1];
        }

        //如果传入第三个参数，则第三个参数作为控件名称
        if (writePassObjectHtml.arguments[2] != null) {
            objId = writePassObjectHtml.arguments[2];
        }

        var configDetail = {};
        //如果传入第四个参数，第四个参数位具体配置
        if (writePassObjectHtml.arguments[3] != null) {
            configDetail = writePassObjectHtml.arguments[3];
        }
        configDetail.randomText = random;
        setPEXSetupUrl(objId);
        if (IsNewest) {
            if (IsIE) {
                writeObjectForHtml(divId, objId, NewPassCtrlClsid, powerConfig(configDetail));
            } else {
                writePluginObjectForHtml(divId, objId, MIME, powerConfig(configDetail));
            }
        }

    }

    //calcfactor参数并未用到，为了保持与原来函数的一致，继续保留此参数

    function getPassInput(id, ts, spanId, massage, calcfactor) {
        try {
            var bOk = false;
            var powerobj = document.getElementById(id);
            powerobj.setTimestamp(ts);
            if (IsIE) {
                bOk = powerobj.publicKeyBlob(_newpk); //设置新控件Windows公钥
            } else {
                if (OsType == "Mac") {
                    bOk = powerobj.publicKeyBlob(_newmacpk); //设置新控件Mac公钥
                } else {
                    bOk = powerobj.publicKeyBlob(_newpk); //设置新控件Windows公钥
                }
            }

            if (!bOk) {
                PEGetElement(spanId).innerHTML = powerobj.lastError();
                return null;
            }

            var nresult = powerobj.verify();
            //为了兼容旧控件，此处不对弱PIN码做处理，后台必须增加弱PIN码的检测与处理
            if (nresult < 0 && nresult != -4) {
                var error;
                if (nresult == -1) {
                    error = "密码内容不能为空";
                } else if (nresult == -2) {
                    error = "密码输入长度不足";
                } else if (nresult == -3) {
                    error = "密码输入内容不合法";
                } else if (nresult == -4) {
                    error = "输入为简单密码";
                } else {
                    error = powerobj.lastError();
                }
                PEGetElement(spanId).innerHTML = error;
                PEGetElement(id).focus();
                return null;
            }

            var value = powerobj.getPinValue();
            if (value == "") {
                PEGetElement(spanId).innerHTML = powerobj.lastError();
                PEGetElement(id).focus();
                return null;
            } else {
                return value;
            }
        } catch (e) {
            PEGetElement(spanId).innerHTML = "控件尚未安装，请下载并安装控件！";
            return null;
        }
        return null;
    }

    //比较两次输入的密码是否一致

    function comparePWD(id, firstPWD, spanId, message) {
        try {
            var pwdobj = PEGetElement(id);
            if (null != firstPWD) {
                var flag = pwdobj.comparePWD(firstPWD);
                var err = pwdobj.lastError();
                if (('成功' == err) || ("Success" == err)) {
                    if (!flag) {
                        PEGetElement(spanId).innerHTML += "</br>密码比较：密码不相同";
                        return false;
                    } else {
                        PEGetElement(spanId).innerHTML += "</br>密码比较：密码相同";
                        return true;
                    }
                } else {
                    PEGetElement(spanId).innerHTML += "</br>密码比较：失败" + " lastError:" + err;
                }
            } else {
                return false;
            }

        } catch (e) {
            PEGetElement("Err_Info").innerHTML += e.toString();
            return false;
        }

    }

    //比较两个密码是否相同，返回true of false，转入两个密码控件的ID和时间戳

    function ComparePassword(objId1, objId2, ts) {
        var pwdobj = PEGetElement(objId1);
        var password1 = getPassInput(objId1, ts, objId1 + ComSpanId, "", null);
        var password2 = getPassInput(objId2, ts, objId2 + ComSpanId, "", null);
        return pwdobj.comparePWD(password2);
    }

    //获取机器码
    function getMachineCode(objName, ts) {
        try {
            var bOk = false;

            var powerobj = document.getElementById(objName);
            try {
                powerobj.setTimestamp(ts);
            } catch (ex) { }

            if (IsIE) {
                bOk = powerobj.publicKeyBlob(_newpk); //设置新控件Windows公钥
            } else {
                if (OsType == "Mac") {
                    bOk = powerobj.publicKeyBlob(_newmacpk); //设置新控件Mac公钥
                } else {
                    bOk = powerobj.publicKeyBlob(_newpk); //设置新控件Windows公钥
                }
            }
            if (!bOk) {

                return null;
            }

            var machineCode = powerobj.getMachineCode();
            var machineInfo = powerobj.getClientInfo();

            return {
                machineCode: machineCode,
                machineInfo: machineInfo
            };

        } catch (e) {
        }
        return null;
    }

    function PEGetElement(id) {
        return window.document.getElementById(id);
    }

    //控件未安装及不是最新的时候给出提示
    //oid参数并未用到，为了保持与之前接口的一致性，继续保留此参数

    function setPEXSetupUrl(oid) {
        //var winPath = window.document.location.href;
        //var tmpPath = winPath.substring(0, winPath.lastIndexOf('/'));
        //if (tmpPath.indexOf("NetBank/zh_CN") < 0)
        //    tmpPath = tmpPath + "/NetBank/zh_CN";
        //var DownloadPath = tmpPath + getDownLoadPath();

        var DownloadPath = getDownLoadPath();

        if (!IsInstalled) {
            if ((OsType == "Win32") || (OsType == "Win64") || (OsType == "Mac")) {
                document.write('<a href="' + DownloadPath + '" class="download_install" target="_blank">点击此处下载控件</a><span id = ' + oid + "Msg" + ' class = "pwdError"></span>');
            } else {
                document.write('<a href="#" class="download_install">暂不支持此系统</a><span id = ' + oid + "Msg" + ' class = "pwdError"></span>');
            }

            IsNewest = false;
        } else {
            var LocalObjVersion = getLocalObjVersion();

            var ObjVersion = getObjVersion();
            if (LocalObjVersion < ObjVersion) {
                document.write('<a href="' + DownloadPath + '" class="download_install" target="_blank">点击此处更新控件</a><span id = ' + oid + "Msg" + ' class = "pwdError"></span>');
                IsNewest = false;
            } else {
                IsNewest = true;
            }
        }
    }

    //貌似没什么用

    function removeUtilObject(oid) {
        var myUtilObject = document.getElementById(oid);
        if (myUtilObject != null)
            myUtilObject.parentNode.removeChild(myUtilObject);
    }

    //获取控件或插件的安装文件路径，如果操作系统不是可识别的，则默认返回Win32版本的安装文件路径

    function getDownLoadPath() {
        if (OsType == "Win32")
            return WIN_SETUP_PATH;		    	//Windows32 OS
        else if (OsType == "Win64")
            return WIN_64_SETUP_PATH;			//Windows64 OS
        else if (OsType == "Mac")
            return MAC_SETUP_PATH; //MAC OS

        //如果都识别不出来，默认返回Win32的版本
        return WIN_SETUP_PATH;
    }

    //调用该函数前，要保证IsInstalled为true，目前仅在setPEXSetupUrl中调用，调用前已经得到保证

    function getObjVersion() {
        var ObjVersion = "";

        if (OsType == "Win32") {
            if (IsIE) {
                ObjVersion = WIN_NEW_32_ACTIVEX_VERSION; //Windows系统下32位新控件版本
            } else {
                ObjVersion = WIN_PLUGIN_VERSION; //Windows系统下插件版本
            }
        } else if (OsType == "Win64") {
            if (IsIE) {
                ObjVersion = WIN_NEW_64_ACTIVEX_VERSION; //Windows系统下64位新控件版本
            } else {
                ObjVersion = WIN_PLUGIN_VERSION; //Windows系统下插件版本
            }
        } else if (OsType == "Mac") {
            ObjVersion = MAC_PLUGIN_VERSION; //Mac系统下插件版本
        }

        return ObjVersion;
    }

    var IsInistall = isRegisterediSecurity();
    writeUtilObject("versionObj");

    function getLocalObjVersion() {
        if (LocalObjVersion == 0) {
            //        var pwdobj =  PEGetElement("versionObj");
            //        if (null != pwdobj)
            //        {
            //            LocalObjVersion = pwdobj.getVersion();
            //        }
            // 为了解决mac系统中safari浏览器多tab页加载问题，临时解决方案
            try {
                var tmpObj = PEGetElement("versionObj");
                LocalObjVersion = tmpObj.getVersion();
            } catch (e) {
                LocalObjVersion = getObjVersion();
            }
        }
        return LocalObjVersion;
    }

    function DealWithPwd(formName, ts) {

        var password;

        var messageSpanId = "powerpass_ie" + ComSpanId;
        //如果传入的第三个参数不为空，那么就将错误信息显示在该参数名的innerHtml里
        var arg3 = DealWithPwd.arguments[2];
        if (arg3 != null) { messageSpanId = arg3; }

        //强制规定，密码域名字用powerpass_ie，密码错误信息存放域为errorInf，方便日后修改
        password = getPassInput("powerpass_ie", ts, messageSpanId, "", null);

        if (password == null || password == "undefined") {

            return false;
        }

        //为了公用同一解密步骤，需要将密文拼装上送，pwdFieldKeys为键集，pwdField为值集 其中{CSTPWD,newAcctPswd1}表示需要将解密后的值同时赋给CSTPWD和newAcctPswd1域
        //先将各种卡类型情况下共用的字段拼接
        var pwdFieldKeys = "fromAcctPswd";
        var pwdField = password;

        document.getElementById(formName).pwdFieldKeys.value = pwdFieldKeys;
        document.getElementById(formName).pwdField.value = pwdField;
        document.getElementById(formName).plattype.value = OsType;
        return true;

    }

    //为基金单独写的方法，可以传入待处理的obj名称

    function DealWithPwdFund(formName, objName, ts) {

        var password;

        //强制规定，密码域名字用powerpass_ie，密码错误信息存放域为errorInf，方便日后修改
        password = getPassInput(objName, ts, objName + ComSpanId, "", null);
        if (password == null || password == "undefined") {

            return false;
        }

        //为了公用同一解密步骤，需要将密文拼装上送，pwdFieldKeys为键集，pwdField为值集 其中{CSTPWD,newAcctPswd1}表示需要将解密后的值同时赋给CSTPWD和newAcctPswd1域
        //先将各种卡类型情况下共用的字段拼接
        var pwdFieldKeys = "fromAcctPswd";
        var pwdField = password;

        document.getElementById(formName).pwdFieldKeys.value = pwdFieldKeys;
        document.getElementById(formName).pwdField.value = pwdField;
        document.getElementById(formName).plattype.value = OsType;
        return true;

    }


    function DealWithPwds(formName, ts, pwdKeys) {
        var password;
        var pwdValues = "";

        //强制规定，密码域名字用powerpass_ie，针对多密码输入页面，使用powerpass_ie0，powerpass_ie1，powerpass_ie2……密码错误信息存放域为errorInf，方便日后修改
        var keyCollection = pwdKeys.split("@");
        var count = keyCollection.length;

        for (i = 0; i < count; i++) {
            var powerpassName = "powerpass_ie" + i;


            password = getPassInput(powerpassName, ts, powerpassName + ComSpanId, "", null);

            if (password == null || password == "undefined") {
                return false;
            }

            pwdValues = pwdValues + "@" + password;

            //先将各种卡类型情况下共用的字段拼接
        }

        pwdValues = pwdValues.substring(1, pwdValues.length);
        document.getElementById(formName).pwdFieldKeys.value = pwdKeys;
        document.getElementById(formName).pwdField.value = pwdValues;
        document.getElementById(formName).plattype.value = OsType;

        return true;
    }
    function getCSPListLogin(successCallback) {
        hFlag = false;
        var pwdObj = PEGetElement("powerpass_ie1");
        if (pwdObj != null) {
            var lists = pwdObj.getCspList();
            var csparray = lists.split("|");
            //cspName is here
            CspValue = csparray[0];
            if (CspValue != null && CspValue != "") {
                PEGetElement("cspName").innerHTML = "K宝已插入";
                successCallback != undefined && successCallback && successCallback();
                if (csparray.length > 1) {
                    PEGetElement("PowerEnterDiv").style.display = "none";
                    PEGetElement("m-keyShow").style.display = "none";
                    PEGetElement("m-firstKeyShow").style.display = "";
                    PEGetElement("cspName_first").innerHTML = "检测到多个K宝，请<a href='###' onclick = '$.directLogon();'>点击这里</a>进行登录";
                    PEGetElement("m-KbinsertLogon").className = 'm-Kbinsertimg-have';
                    PEGetElement("m-kbbtn").disabled = true;
                    return;
                } else {
                    PEGetElement("m-kbbtn").disabled = false;
                    for (var item in cspNameList) {
                        if (cspNameList[item] == CspValue) {
                            hFlag = true;
                            break;
                        }
                    }
                }
                if (hFlag) {
                    PEGetElement("PowerEnterDiv").style.display = "none";
                    PEGetElement("m-keyShow").style.display = "none";
                    PEGetElement("m-firstKeyShow").style.display = "";
                    PEGetElement("cspName_first").innerHTML = "K宝已插入，请点击下方按钮进行登录";
                    PEGetElement("m-KbinsertLogon-first").className = 'm-Kbinsertimg-first-have';
                } else {
                    PEGetElement("cspName").innerHTML = "K宝已插入";
                    PEGetElement("PowerEnterDiv").style.display = "";
                    PEGetElement("m-keyShow").style.display = "";
                    PEGetElement("m-firstKeyShow").style.display = "none";
                    PEGetElement("m-KbinsertLogon").className = 'm-Kbinsertimg-have';
                }

            }
            else {
                PEGetElement("cspName").innerHTML = "请插入K宝";
                PEGetElement("PowerEnterDiv").style.display = "";
                PEGetElement("m-keyShow").style.display = "";
                PEGetElement("m-firstKeyShow").style.display = "none";
                PEGetElement("m-KbinsertLogon").className = 'm-Kbinsertimg';
                PEGetElement("m-KbinsertLogon-first").className = 'm-Kbinsertimg-first-have';
            }
        }

    }

    function verifyCSPPin() {
        if (hFlag) {
            return true;
        } else {
            try {
                var pwdobj = document.getElementById("powerpass_ie1");
                if (null == pwdobj) {
                    return false;
                }
                var csp = document.getElementById("cspName").innerHTML;
                if (("" == csp) || (null == csp) || (csp == "请插入K宝")) {
                    PEGetElement("powerpass_ie1Msg").innerHTML = "未检测到K宝！请插入K宝或<a href='http://www.abchina.com/cn/EBanking/Safety/Securitytools/ukey/USBKey/SecondgenerationUSBKey/' target='_blank'>更新驱动</a>";
                    return false;
                }

                //if (!EventBlur("powerpass_ie1")) { return false;}
                if (pwdobj.getPinLength() == 8) {
                    var ret = pwdobj.verifyCspPin(CspValue, "");
                    var err = pwdobj.lastError();

                    if (ret > 0) {
                        //ret的值就是密码还可以重试的次数
                        PEGetElement("powerpass_ie1Msg").innerHTML = "K宝" + err + "，您还可以重试" + ret + "次";
                        return false;
                    }
                    else {
                        if (err.indexOf("成功") >= 0 || err.indexOf("success") >= 0) {
                            return true;
                        } else if (err.indexOf('密码不正确') >= 0) {
                            PEGetElement("powerpass_ie1Msg").innerHTML = "K宝已锁定，请您前往网点补办证书";
                            return false;
                        } else {
                            //老驱动时，返回的信息为"示设置公钥"，星龙基为"USBKey未连接"
                            PEGetElement("powerpass_ie1Msg").innerHTML = "<a href='http://www.abchina.com/cn/EBanking/Safety/Securitytools/ukey/USBKey/SecondgenerationUSBKey/' target='_blank'>请下载最新版的K宝驱动</a>";
                            return false;
                        }

                    }
                }
                else {
                    PEGetElement("powerpass_ie1Msg").innerHTML = "请输入8位K宝密码";
                    return false;
                }

            }
            catch (e) {
                PEGetElement("powerpass_ie1Msg").innerHTML = "未知错误";
            }
            return false;
        }
    }

    window.password = {

        EventBlur: EventBlur,
        CspValue: CspValue,
        //默认绘制密码框
        WritePassObject: writePassObject,
        //默认绘制多密码框
        WriteMultiPassObject: writeMultiPassObject,
        //绘制密码框，可以自己将样式作为参数传入，参数的最后需要传入random
        WritePassObjectOrg: writePassObjectOrg,

        //绘制多密码框，可以自己将样式作为参数传入，参数的最后需要传入random
        WriteMultiPassObjectOrg: writeMultiPassObjectOrg,

        //HTML页面嵌套时，使用此方法
        WritePassObjectHtml: writePassObjectHtml,

        //专门供K宝登陆页使用
        WritePassObjectLogin: writePassObjectLogin,

        //WriteObjectForMultiHtml: writeObjectForMultiHtml,
        //提交时处理单一密码控件
        DealWithPwd: DealWithPwd,
        ////提交时处理多个密码控件
        DealWithPwds: DealWithPwds,

        //为基金写的方法，可以传入待处理的密码控件名称
        DealWithPwdFund: DealWithPwdFund,

        //比较两个密码是否相同
        ComparePassword: ComparePassword,

        //证书登陆时，获取CSP列表
        GetCSPListLogin: getCSPListLogin,

        //证书登陆时，校验pin
        VerifyCSPPin: verifyCSPPin,

        //获取用户机器码信息
        GetMachineCode: getMachineCode,
        //获取平台信息
        GetOsType: getOsType,
        //是否安装驱动
        IsInstalled: IsInstalled
    };
}(window));