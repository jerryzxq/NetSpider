var logon_1 = "您目前的控件不支持启用保管箱功能，是否重新下载并安装保管箱控件？";
var verify_1 = "验证码错误";
var verify_2 = "请输入4位验证码!";
var logonFailNumDesc = "检查登录次数非法!";
var userNameMsg1 = "用户名不能为空!";
var userNameMsg2 = "用户名长度超长!";
var logonPwdMsg1 = "登录密码";
var securyCtrlMsg1 = "系统检测到安全控件被破坏,您需要重新下载安装！点“确定”后重新下载。";
var selectCertMsg = "请选择证书!";
var safeBoxPwd_1 = "请输入保管箱密码!";
var safeBoxPwd_2 = "保管箱密码长度小于6，请重新输入!";
var safeBoxPwd_3 = "保管箱密码长度大于12，请重新输入!";
var sessionKeyMsg = "您机器上的安全控件可能不正确，请重新下载安装。";
var certList_1 = "请选择中信银行证书";
var certList_2 = "选择其它登录方式";
var certType_1 = "一代key证书";
var certType_3 = "文件证书";
var certType_4 = "二代KEY证书";
var checkFlag;
function getCertCustUserIdMsg(msg) {
    // document.getElementById("showMsg").style.display="none";
    // document.getElementById("showInput").style.display="";
    var resMsgArray = msg.split("#");
    if (resMsgArray[0] == 'error') {
        alert("错误：" + resMsgArray[1]);
    } else if (resMsgArray[0] == 'succ') {
        if (resMsgArray[1] == '1') {
            certCustomerNum = "1";
            document.formTmp.logonNo.value = resMsgArray[2];
            getCstLogonNum(resMsgArray[2], "0");
        } else {
            //多条记录，则要显示出手工输入div
            // certCustomerNum = "M";
            // document.getElementById("certUserId").style.display="none"; //证书用户层隐藏
            // document.getElementById("noCertUserId").style.display="";	//无证书用户层展示
            // document.formTmp.logonNoCert.focus();		
        }
    }
}
//登录页面加载时，需要检查用户是否已经安装网银伴侣。如果未安装则提示用户下载安装
function CheckSecurity() {

    if (!!window.ActiveXObject || "ActiveXObject" in window) {
        if (judgeBrowserVersion() == false) {
            alert("IE浏览器版本过低，建议使用IE7及以上版本！");
            jQuery("#logonMethod").hide();
            return false;
        }
        if (isActiveXSupported() == false) {
            alert("您的IE设置不支持ActiveX，请重新配置IE！");
            controlPwdDisplay(false);
            return false;
        }
    } else {
        if (judgeBrowserVersion() == false) {
            //alert("IE浏览器版本过低，建议使用IE7及以上版本！");
            return false;
        }
    }

    var divObj = document.getElementById("helpmateobjdiv");
    var HelpMateObjectId = PBank.message['HelpMateObjectId'];
    divObj.innerHTML = HelpMateObjectId;
    var helpmate = PBank.message['HelpMateDown'];//生产环境的地址
    var sFile = '';

    try {
        //取得网银伴侣版本
        sFile = GetHelpMateVersions();
        if (sFile == 0 || sFile == -1) {//如果客户安装过网银伴侣，但又删除了网银伴侣。
            //if(confirm("您没有安装网银伴侣，是否进入网银伴侣下载程序？")){
            //location.href=helpmate;
            document.getElementById("type2").style.display = ""; //隐藏证书列表
            document.getElementById("logonLoading").style.display = "none";
            document.formLogon.logonType.value = "2";//无证书登录
            controlPwdDisplay(false);
            //setLogonType('2');
            checkFlag = 0;
            return false;
            //}else{
            //	return SetUpInittool();
            //}
        } else if (sFile != PBank.message['HelpMateVer']) { //客户安装了网银伴侣，但是版本不是最新版本
            /**if(confirm("您的网银伴侣版本较低，是否进行更新？")){
				location.href=helpmate;
				document.getElementById("type2").style.display=""; //隐藏证书列表
				document.getElementById("logonLoading").style.display="none";
				document.formLogon.logonType.value="2" ;//无证书登录
				controlPwdDisplay(false); //true
				//setLogonType('2');
				return false;
			}else{
				return SetUpInittool();
			}
				//location.href=helpmate;
				document.getElementById("type2").style.display=""; //隐藏证书列表
				document.getElementById("logonLoading").style.display="none";
				document.formLogon.logonType.value="2" ;//无证书登录
				controlPwdDisplay(false); //true
				setLogonType('2');
				return false;
		}*/
            document.getElementById("type2").style.display = ""; //隐藏证书列表
            document.getElementById("logonLoading").style.display = "none";
            document.formLogon.logonType.value = "2";//无证书登录
            controlPwdDisplay(false); //true
            //setLogonType('2');
            checkFlag = 1;
            return false;
        }
        else {
            try {
                var objVersion = GetVersions();
                if (objVersion == null || objVersion == '' || objVersion == '1,4,0,0') {
                    checkFlag = 2;
                    document.getElementById("type2").style.display = ""; //隐藏证书列表
                    document.getElementById("logonLoading").style.display = "none";
                    document.formLogon.logonType.value = "2";//无证书登录
                    controlPwdDisplay(false); //true
                    setLogonType('2');
                    return false;
                }

            } catch (e) {
                checkFlag = 0;
            }

            return SetUpInittool();
        }
    } catch (e) {//如果客户没有安装过网银伴侣。
        //console.log(e);
        //if(confirm("您没有安装网银伴侣，是否进入网银伴侣下载程序？")){
        //location.href=helpmate;
        checkFlag = 0;
        document.getElementById("type2").style.display = ""; //隐藏证书列表
        document.getElementById("logonLoading").style.display = "none";
        document.formLogon.logonType.value = "2";//无证书登录
        // controlPwdDisplay(false);
        setLogonType('2');
        return false;

    }


}

function downLoad() {
    //location.href='download\helpmate\HelpmateSetup.exe';
    //jQuery("#linkBtn").html("请刷新浏览器重新登录");
    jQuery("#linkBtn").hide();
    jQuery("#linkBtn2").show();


}
//控件安装程序   
function SetUpInittool() {
    //密码控件安装判断

    controlPwdDisplay(true);
    return true;
}
function GetCertificates() {
    jQuery(function () {
        try {
            try {
                jQuery("#certListUl").find("li").remove();
            } catch (e) {
                alert(e);
            }
            var ary;
            var length = 0;
            var obj;
            ary = returnAry();
            length = returnLength();
            obj = returnObj();
            //<!--控件根据系统加密标识，只加载相应算法证书 begin-->
            obj.CryptFlag = encryptSysFlag;
            //<!--控件根据系统加密标识，只加载相应算法证书 end-->

            var selectitem = "<li><a href='javascript:;' selectid='-1'>----" + certList_1 + "----</a></li>";
            jQuery("#certListUl").append(selectitem);

            var indexFileCn = 0;//文件证书数量
            var indexKeyCn = 0;//KEY证书数量
            var indexKeyCn2 = 0;//二代KEY证书数量
            var indexDllCn = 0;//控件证书数量
            var indexCn = 0;//证书总数量

            for (var i = 0; i <= length; i++) {
                var showText = ary[i].split('|')[1];//证书主题
                var type = ary[i].split('|')[2];//证书类型
                var showTextArray = showText.split(',');
                var CA_ID = showText;
                var CN = CA_ID;//取出DN	
                var temp_CN = "";
                if ((CN.split('@')).length == 4) {
                    temp_CN = CN.split('@')[(CN.split('@')).length - 2];
                }
                //如果为2，则是老证书
                if ((CN.split('@')).length == 2) {
                    var temp_CN_tmp = CN.split('@')[(CN.split('@')).length - 2];
                    //去掉cn=
                    var temp_CN_tmp_tmp = temp_CN_tmp.split("cn=")[1];
                    //去掉数字
                    var temp;
                    for (m = 0; m < temp_CN_tmp_tmp.length; m++) {
                        var tempChar = temp_CN_tmp_tmp.substring(m, m + 1);
                        if (!(isInteger(tempChar))) {
                            temp = m;
                            break;
                        }
                    }
                    var temp1 = temp_CN_tmp_tmp.length;
                    temp_CN = temp_CN_tmp_tmp.substring(temp, temp1);
                }
                //<!--新增二代key证书分支 begin -->
                if (type == "4" || type == "5") {//二代key证书
                    indexKeyCn2 = indexKeyCn2 + 1;
                    indexCn = indexCn + 1;
                    var valueStr = "3" + "," + CN;
                    temp_CN = temp_CN.split('&$')[0];
                    selectitem = "<li><a href='javascript:;' selectid=\'"
                              + ary[i].split('|')[0] + "," + valueStr + "," + type
                             + "\'>" + temp_CN + "(" + certType_4 + indexKeyCn2 + ")"
                             + "</a></li>";
                    jQuery("#certListUl").append(selectitem);
                }
                    //<!--新增二代key证书分支 end -->
                else if (type == "3") {//控件证书
                    indexFileCn = indexFileCn + 1;
                    indexCn = indexCn + 1;
                    var valueStr = "2" + "," + CN;
                    selectitem = "<li><a href='javascript:;' selectid=\'"
                              + ary[i].split('|')[0] + "," + valueStr + "," + type
                             + "\'>" + temp_CN + "(" + certType_3 + indexFileCn + ")"
                             + "</a></li>";
                    jQuery("#certListUl").append(selectitem);
                } else if (type == "0") {//文件证书
                    indexFileCn = indexFileCn + 1;
                    indexCn = indexCn + 1;
                    var valueStr = "0" + "," + CN;
                    selectitem = "<li><a href='javascript:;' selectid=\'"
                              + ary[i].split('|')[0] + "," + valueStr + "," + type
                             + "\'>" + temp_CN + "(" + certType_3 + indexFileCn + ")"
                             + "</a></li>";
                    jQuery("#certListUl").append(selectitem);
                } else if (type == "1" || type == "2") {//一代key证书
                    indexKeyCn = indexKeyCn + 1;
                    indexCn = indexCn + 1;
                    var valueStr = "1" + "," + CN;
                    selectitem = "<li><a href='javascript:;' selectid=\'"
                              + ary[i].split('|')[0] + "," + valueStr + "," + type
                             + "\'>" + temp_CN + "(" + certType_1 + indexKeyCn + ")"
                             + "</a></li>";
                    jQuery("#certListUl").append(selectitem);
                    if (type == "1") {
                        ftFlg = "1";//包含飞天key
                    } else {
                        gdFlg = "1";//包含捷德key
                    }
                    installFlg = "1";
                }
            }

            //无证书登录选择项
            selectitem = "<li><a href='javascript:;' selectid='-2'>" + certList_2 + "</a></li>";
            jQuery("#certListUl").append(selectitem);

            if (indexCn > 0) {
                document.getElementById("type2").style.display = ""; //显示选择证书列表
                document.getElementById("logonLoading").style.display = "none";
                setLogonType('0');

                //判断客户key驱动是否为最新版本。
                if (keyVerCheck()) {
                    if (indexCn == 1) {
                        setDefault("certListCite", "certListUl", "certList", 1);
                        getUserName();
                    }
                }

            } else {
                document.getElementById("type2").style.display = "none"; //隐藏证书列表
                setLogonType('2');
            }


        } catch (e) {
            document.getElementById("type2").style.display = ""; //隐藏证书列表
            document.getElementById("logonLoading").style.display = "none";
            var firstitem = "<li><a href='javascript:;' selectid='-1'>----" + certList_1 + "----</a></li>";
            jQuery("#certListUl").append(firstitem);
            setLogonType('2');
        }
        jQuery.divselect("#certListDiv", "#certList");
    })
    jQuery("#logonPwdIdText").show();
    jQuery(".logonPwdId_li").css("background", "#fff");
    jQuery("#ocxEdit_pge").show();
}

function createSessionKey() {

    var guardID = getGuardID();

    //给安全增强控件设置国密商密标识
    guardID.SetCipherMode(encryptSysFlag);

    var clientRandom = "";
    try {
        clientRandom = guardID.GetInfo();
    } catch (e) {
        alert(sessionKeyMsg);
        downLoadCNCBGuard();
        return -1;
    }
    document.sessionKeyForm.clientRandom.value = clientRandom;
    var linkurl = sessionLinkUrl;
    //var sessionKeyForm = jQuery("#sessionKeyForm > input");
    var msg = doSend5(document.sessionKeyForm, linkurl);//var msg = doSend4(document.sessionKeyForm,linkurl);

    if (msg.indexOf("serverRandom:") != -1) {
        var sr = msg.split(":")[1];
        var guardID = getGuardID();
        guardID.SetInfo(sr);
    }
    return 0;
}

function downLoadCNCBGuard() {
    var winDsp = 'width=' + (screen.availWidth - 24) + ',height=' + (screen.availHeight - 50) + ',toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' + 'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
    window.open("security_show.htm", "secObject", winDsp);
}

function verify() {
    var code = document.formTmp.verifyCode.value;
    if (code.length == 0) {
        document.getElementById('verifyImg').style.display = 'none';
    }
    if (code.length == 4) {
        executeAjaxCommand(checkVeriyUrl, ('verifyCode=' + code), 'disposeVerifyCode');
    }
}
//异步查询操作完成标志(登录失败次数) 
var reqFlag = true;
function getCstLogonNum(logonno, logonType) {
    if (!reqFlag) { return; }
    reqFlag = false;
    document.getElementById('logonButton').disabled = true;
    // var computerId = "";
    // executeAjaxCommand('<ctp:jspURL jspFileName="getCstLogonFailNum.do"/>',('logonNo='+logonno+'&computerId='+computerId),'disposeLogonNum');
    if (logonType == '2') {
        if (trim(logonno) == '') { //页面输入账号、证件号、用户名方式登录，输入为空，则直接返回
            return;
        }
        document.checkLogonFailNumForm.logonType.value = '2';
        document.checkLogonFailNumForm.logonNo.value = trim(logonno);
    } else {
        document.checkLogonFailNumForm.logonType.value = '0';
        document.checkLogonFailNumForm.logonNo.value = document.formLogon.ca_id.value;
    }
    GetMACAddress();
    doSendNew(document.checkLogonFailNumForm, getFailNumUrl, 'disposeLogonNum');
    setTimeout("reqFlag = true;", 100);
}
function disposeLogonNum(msg) {
    if (msg != 'error' && trim(msg) != '') {
        var logonCtrlFailNum = trim(msg).split("|")[0];
        var isCheck = trim(msg).split("|")[1];
    }
    if (msg != 'error' && logonCtrlFailNum == '0' && trim(msg) != '' && isCheck == '0') {
        document.getElementById("verifyId").style.display = "none";
        document.getElementById("verifyIdTxt").style.display = "none";
    } else {
        jQuery(".alertlogin ul li").css("padding", "10px 0px");
        document.getElementById("pinImg").className = "imgVerityCode";
        document.getElementById("verifyId").style.display = "";
        document.getElementById("verifyIdTxt").style.display = "";
    }
    document.getElementById('logonButton').disabled = false;
}
// 生成二维码
function showQrCodeFrist() {
    logonNewFlag = "1";
    if (logonNewFlag == "1" && hasqrcode == "0" && canQryCodeFlag == "1") {
        // IP 生成二维码
        //document.formQrCodeGet.macCode.value = "<%=request.getRemoteAddr()%>";
        doSend(document.formQrCodeGet, checkQrUrl, 'showQrCodeSec');
        //showQrCodeSec('');
    }
}
// 回调函数
function showQrCodeSec(msg) {
    if (trim(msg) == "error") {
        //alert("二维码生成失败，请重新刷新页面！");
        document.getElementById('QrCodeImage').style.display = "none";
        document.getElementById('QrCodeImageEorr2').style.display = "";
        document.getElementById('QrCodeImageEorr1').style.display = "none";
        document.getElementById('QrCodeImageEorr').style.display = "none";
        document.getElementById('QrCodeImageSucc').style.display = "none";
        jQuery("#qrcodeErrMsg").text("获取二维码失败，请点击图片刷新");
        hasqrcode = "0";
        canQryCodeFlag = "1";
        return;
    } else if (trim(msg) == "more") {
        //alert("二维码生成失败，请重新刷新页面！");
        document.getElementById('QrCodeImage').style.display = "none";
        document.getElementById('QrCodeImageEorr2').style.display = "";
        document.getElementById('QrCodeImageEorr1').style.display = "none";
        document.getElementById('QrCodeImageEorr').style.display = "none";
        document.getElementById('QrCodeImageSucc').style.display = "none";
        jQuery("#qrcodeErrMsg").text("获取二维码次数过多");
        hasqrcode = "0";
        canQryCodeFlag = "1";
        return;
    } else if (trim(msg) == "black") {
        //alert("二维码生成失败，请重新刷新页面！");
        document.getElementById('QrCodeImage').style.display = "none";
        document.getElementById('QrCodeImageEorr2').style.display = "";
        document.getElementById('QrCodeImageEorr1').style.display = "none";
        document.getElementById('QrCodeImageEorr').style.display = "none";
        document.getElementById('QrCodeImageSucc').style.display = "none";
        jQuery("#qrcodeErrMsg").text("当前IP已经被加入黑名单");
        hasqrcode = "0";
        canQryCodeFlag = "1";
        return;
    } else {// 二维码生成成功
        // 返回msg中包括retUrl,qrCode
        // 获取msg中的qrCode 
        //var StringQrCode = msg.split("|");
        // 二维码唯一标识
        //requestQrCode = StringQrCode[1];
        //requestQrCode = requestQrCode.replace(/\+/g,"%2B");
        // 截取msg的retUrl
        //var lnterm = msg.substring(0,msg.indexOf("|"));
        var nowDate = new Date();
        transTime = nowDate.toLocaleTimeString()
        var img = document.getElementById("pinImg1");
        var imgSrc = getQrUrl;
        //img.src = imgSrc+"&cmd=verifyPin&retUrl="+ lnterm +"&qrCode="+requestQrCode+"&session_userRemoteIP=<%=request.getRemoteAddr()%>";
        img.src = imgSrc + "&cmd=verifyPin&time=" + transTime;
        img.className = "imgVerityQrCode";
        hasqrcode = "1";
        canQryCodeFlag = "0";
        document.getElementById('QrCodeImage').style.display = "";
        document.getElementById('QrCodeImageEorr2').style.display = "none";
        document.getElementById('QrCodeImageEorr1').style.display = "none";
        document.getElementById('QrCodeImageEorr').style.display = "none";
        document.getElementById('QrCodeImageSucc').style.display = "none";
        jQuery("#qrcodeErrMsg").text("");
        //imgSrcCount+=1;

    }
    if (logonNewFlag == "1" && hasqrcode == "1" && canQryCodeFlag == "0") {
        canQryCodeFlag = "1";
        // 每3秒 发送一次生成二维码请求
        TimerStart = window.setInterval("sendToTask();", 3000);
    }
}
// 轮询 
function sendToTask() {
    doSend(document.formQrCodeQuery, "queryQrCode.do", 'showQryStatus');
    TimerStop = window.clearInterval(TimerStart);
}
// 回调轮询
function showQryStatus(msg) {
    // 轮询失败
    if (msg == "error") {
        //alert("轮询失败，请重新刷新页面！");
        document.getElementById('QrCodeImage').style.display = "none";
        document.getElementById('QrCodeImageEorr2').style.display = "";
        document.getElementById('QrCodeImageEorr1').style.display = "none";
        document.getElementById('QrCodeImageEorr').style.display = "none";
        document.getElementById('QrCodeImageSucc').style.display = "none";
        jQuery("#qrcodeErrMsg").text("获取二维码信息出错，请点击图片刷新");
        hasqrcode = "0";
        canQryCodeFlag = "1";
        TimerStop = window.clearInterval(TimerStart);
        return;
    } else if (trim(msg) == "diff") {
        document.getElementById('QrCodeImage').style.display = "none";
        document.getElementById('QrCodeImageEorr2').style.display = "";
        document.getElementById('QrCodeImageEorr1').style.display = "none";
        document.getElementById('QrCodeImageEorr').style.display = "none";
        document.getElementById('QrCodeImageSucc').style.display = "none";
        jQuery("#qrcodeErrMsg").text("电脑IP变化,请点击图片刷新");
        hasqrcode = "0";
        canQryCodeFlag = "1";
        TimerStop = window.clearInterval(TimerStart);
        return;
    } else if (trim(msg) == "timeOut") {
        document.getElementById('QrCodeImage').style.display = "none";
        document.getElementById('QrCodeImageEorr2').style.display = "none";
        document.getElementById('QrCodeImageEorr1').style.display = "";
        document.getElementById('QrCodeImageEorr').style.display = "none";
        document.getElementById('QrCodeImageSucc').style.display = "none";
        jQuery("#qrcodeErrMsg").text("二维码失效,请点击图片刷新");
        hasqrcode = "0";
        canQryCodeFlag = "1";
        TimerStop = window.clearInterval(TimerStart);
        return;
    } else {
        //var remsg = msg.split("|");
        // 轮询成功 1次结果回来之后canQryCodeFlag 改为0 
        canQryCodeFlag = "0";
        // 二维码状态
        var qrStatus = msg;
        if (qrStatus == 2) { // 二维码状态，已确认
            // 停止定时发送
            TimerStop = window.clearInterval(TimerStart);
            jQuery("#qrcodeErrMsg").text("二维码已确认");
            // 走登录流程
            //doSend(document.formLogonQrCode,'<ctp:jspURL jspFileName="signIn.do"/>');
            //document.formQrCodeLogon.logonNo.value=remsg[1];
            //document.formQrCodeLogon.qrCode.value=requestQrCode;
            document.formQrCodeLogon.submit();
            canQryCodeFlag = "1";
            //登录失败，请刷新后重试
        } else if (qrStatus == 0 || qrStatus == 1) {
            // 状态为0时 等待3秒继续轮询，为1时等待3秒继续轮询，将BP返回的客户号保存到session中
            TimerStart = window.setInterval("sendToTask();", 3000);
            if (qrStatus == 1) {
                document.getElementById('QrCodeImage').style.display = "none";
                document.getElementById('QrCodeImageEorr').style.display = "none";
                document.getElementById('QrCodeImageEorr1').style.display = "none";
                document.getElementById('QrCodeImageEorr2').style.display = "none";
                document.getElementById('QrCodeImageSucc').style.display = "";
            }
        }
        // 二维码状态3，已登录状态时
        if (qrStatus == 3) {
            // 停止定时发送
            TimerStop = window.clearInterval(TimerStart);
            hasqrcode = "0";
            jQuery("#qrcodeErrMsg").text("二维码已登录");
            canQryCodeFlag = "1";
            document.getElementById('QrCodeImage').style.display = "none";
            document.getElementById('QrCodeImageEorr2').style.display = "none";
            document.getElementById('QrCodeImageEorr1').style.display = "none";
            document.getElementById('QrCodeImageEorr').style.display = "none";
            document.getElementById('QrCodeImageSucc').style.display = "";
        } else if (qrStatus == 4) {	// 二维码状态失效登录失败
            // 停止定时发送
            TimerStop = window.clearInterval(TimerStart);
            document.getElementById('QrCodeImage').style.display = "none";
            document.getElementById('QrCodeImageEorr').style.display = "";
            document.getElementById('QrCodeImageEorr2').style.display = "none";
            document.getElementById('QrCodeImageEorr1').style.display = "none";
            document.getElementById('QrCodeImageSucc').style.display = "none";
            jQuery("#qrcodeErrMsg").text("登录失败,请点击图片刷新");
            hasqrcode = "0";
            canQryCodeFlag = "1";
            return;
        }// 返回后再次开始轮询
        //TimerStart = window.setInterval("sendToTask();",3000);
    }
}
//增加登录页广告图片显示及轮播效果开始  addBy zhouxinyong



var _liLength;
var _olLength;
var liWidth;
var flag = 0;
var start;

function triggerClick() {
    var lis = jQuery(".loginBanner ol li");
    lis.eq(flag).trigger("click");
    if (flag == _liLength) {
        flag = 0;
    }
}

function initTurn() {
    start = setInterval(triggerClick, 3000);
}

jQuery(function () {
    jQuery(".loginBanner ol li").live('click', function () {
        var index = jQuery(this).index();
        var w = jQuery(".loginBanner ul li").width();
        jQuery(this).addClass("onlogin").siblings().removeClass("onlogin");
        jQuery(".loginBanner ul").stop().animate({ "marginLeft": -w * index + "px" }, 500);
        clearInterval(start);
        flag = index + 1;
        initTurn();

    });
    triggerClick();
    //jQuery("#cardPicUl li").hover(function(){
    // clearInterval(start);
    //},function(){
    //	initTurn();
    //})
    jQuery("#cardPicUl li").live({
        mouseenter: function () {
            clearInterval(start);
        },
        mouseleave: function () {
            initTurn();
        }
    });


    doSend(document.debitCardForm, debitCardForm_url, 'getDebitcardResult');


})

function getDebitcardResult(msg) {
    document.getElementById("loginBanner").innerHTML = jQuery.trim(msg);
    parent.sizeChange();
    _liLength = jQuery("#cardPicUl").find("li").length;
    _olLength = jQuery("#cardPicOl").find("li").length;
    liWidth = jQuery(".loginBanner ul li").outerWidth();
    jQuery("#cardPicUl").css("width", (_liLength * liWidth + 100) + "px");
    //banner轮播
    initTurn();

}

//增加登录页广告图片显示及轮播效果结束 addBy zhouxinyong

//看不清验证码
var imgSrcCount = 0;
function reGetIMG(param) {
    if (param == '1') {
        document.getElementById('verifyImg').style.display = 'none';
    }
    var img = document.getElementById("pinImg");
    var imgSrc = dynPassUrl;
    img.src = imgSrc + "&tt=" + imgSrcCount;
    img.className = "imgVerityCode";
    imgSrcCount += 1;
    document.formTmp.verifyCode.value = "";
    document.formTmp.verifyCode.focus();
}

String.prototype.trim = function () {
    return this.replace(/(^\s*)|(\s*$)/g, "");
}
// var logonFailFlag = false;
function logon() {
    jQuery("#logonButton").addClass("loadingImgsSm");
    jQuery("#logonLoading1").show();
    jQuery("#logonButton").attr("disabled", true);
    try {
        //无证书登录
        if (document.formLogon.logonType.value == "2") {
            var noCertLogon = trim(document.formTmp.logonNoCert.value);
            if (noCertLogon == '') {
                showLogonLoading();
                alert(userNameMsg1);
                document.formTmp.logonNoCert.focus();
                return;
            }
            if (noCertLogon.length > 100) {
                showLogonLoading();
                alert(userNameMsg2);
                document.formTmp.logonNoCert.focus();
                return;
            }
            document.formLogon.logonNo.value = noCertLogon;
            document.form2Logon.logonNo.value = noCertLogon;
            document.formLogon.browserFlag.value = browserFlag;
            document.form2Logon.browserFlag.value = browserFlag;
            //if(logonFailFlag == false){
            //	getCstLogonNum(document.formTmp.logonNoCert.value);
            //}
        } else {//有证书登录，要区分是不是手工输入
            //如果为1，则为自动调出
            if (certCustomerNum == "1") {
                document.formLogon.logonNo.value = document.formTmp.logonNo.value;
                document.keyLogonForm.logonNo.value = document.formTmp.logonNo.value;
                document.form2Logon.logonNo.value = document.formTmp.logonNo.value;
                //if(logonFailFlag == false){
                //	getCstLogonNum(document.formTmp.logonNo.value);
                //}
            }
            //如果为 M ，则为手工输入
            if (certCustomerNum == "M") {
                var noCertLogon = trim(document.formTmp.logonNoCert.value);
                if (noCertLogon == '') {
                    showLogonLoading();
                    alert(userNameMsg1);
                    document.formTmp.logonNoCert.focus();
                    return;
                }
                if (noCertLogon.length > 100) {
                    showLogonLoading();
                    alert(userNameMsg2);
                    document.formTmp.logonNoCert.focus();
                    return;
                }
                document.formLogon.logonNo.value = noCertLogon;
                document.keyLogonForm.logonNo.value = noCertLogon;
                document.form2Logon.logonNo.value = noCertLogon;
                //if(logonFailFlag == false){
                //	getCstLogonNum(document.formTmp.logonNoCert.value);
                //}
            }
        }
        if (!checkLogonPinBlockByName(pgeditor, logonPwdMsg1)) {
            showLogonLoading();
            return;
        }

        //判断mac操作系统以外进行sessionkey加密
        var userComputerId = "";
        if (browserFlag == '2') {
            document.formLogon.password.value = pgeditor.pwdResult();
            document.keyLogonForm.password.value = pgeditor.pwdResult();
            document.form2Logon.password.value = pgeditor.pwdResult();
            userComputerId = pgeditor.pwdResultCPU();//获取机器码
        } else {

            createSessionKey();

            var guardID = getGuardID();
            document.formLogon.password.value = guardID.EncryptData(pgeditor.pwdResult());
            document.keyLogonForm.password.value = guardID.EncryptData(pgeditor.pwdResult());
            document.form2Logon.password.value = guardID.EncryptData(pgeditor.pwdResult());

            userComputerId = guardID.GetUserID();
        }
        if (userComputerId == "NF") {
            alert(securyCtrlMsg1);
            downLoadCNCBGuard();
            throw e;
        }
        document.formLogon.computerId.value = userComputerId;
        document.keyLogonForm.computerId.value = userComputerId;
        document.form2Logon.computerId.value = userComputerId;
        //无证书登陆
        if (document.formLogon.logonType.value == "2") {
            var noCertLogon = trim(document.formTmp.logonNoCert.value);
            if (noCertLogon.length == 18) {
                document.formLogon.customerCtfType.value = "1";
                document.form2Logon.customerCtfType.value = "1";
            } else if (noCertLogon.length == 16 || noCertLogon.length == 19) {
                document.formLogon.customerCtfType.value = "2";
                document.form2Logon.customerCtfType.value = "2";
            } else {
                document.formLogon.customerCtfType.value = "0";
                document.form2Logon.customerCtfType.value = "0";
            }
            document.formLogon.session_certSubType.value = '';
            document.form2Logon.session_certSubType.value = '';
            var obj_verifyId = document.getElementById('verifyId');
            if (obj_verifyId.style.display == '') {
                if (document.formTmp.verifyCode.value == '' || document.formTmp.verifyCode.value.length != 4) {
                    showLogonLoading();
                    alert(verify_2);
                    document.formTmp.verifyCode.focus();
                    return;
                }
                document.formLogon.verifyCode.value = document.formTmp.verifyCode.value;
                //验证校验码
                executeAjaxCommand(checkVeriyUrl, ('verifyCode=' + document.formTmp.verifyCode.value), 'disposeMsg');
                //验证校验码
            } else {
                //登录事件采集
                analize("login", "ptb_usernameland_ld", document.form2Logon.logonNo.value);
                document.form2Logon.submit();
                setTimeout(function () {
                    jQuery("#logonLoading1").attr("src", logonLoadingUrl);
                }, 20);
            }

        } else {
            if (document.formTmp.certList.value == "-1") {
                document.formTmp.certList.focus();
                showLogonLoading();
                alert(selectCertMsg);
                return;
            }
            var certStr = document.formTmp.certList.value;
            var certStrArray = certStr.split(",");
            //文件证书
            if (certStrArray[1] == "0") {
                document.formLogon.customerCtfType.value = "0";
                document.formLogon.logonType.value = "0";
                document.form2Logon.customerCtfType.value = "0";
                document.form2Logon.logonType.value = "0";
                var obj = returnObj();
                obj.SetSelectedCertificate(certStrArray[0]);
                obj.CryptInit();
                document.formLogon.session_certSubType.value = '0';
                document.form2Logon.session_certSubType.value = '0';
                //20091102增加证书哈希值开始
                document.formLogon.session_certHashcode.value = certStrArray[0];
                document.form2Logon.session_certHashcode.value = certStrArray[0];
                //20091102增加证书哈希值结束
                var obj_verifyId = document.getElementById('verifyId');
                if (obj_verifyId.style.display == '') {
                    if (document.formTmp.verifyCode.value == '' || document.formTmp.verifyCode.value.length != 4) {
                        showLogonLoading();
                        alert(verify_2);
                        document.formTmp.verifyCode.focus();
                        return;
                    }
                    document.formLogon.verifyCode.value = document.formTmp.verifyCode.value;
                    //验证校验码
                    executeAjaxCommand(checkVeriyUrl, ('verifyCode=' + document.formTmp.verifyCode.value), 'disposeMsgFileCert');
                    //验证校验码
                } else {
                    //登录事件采集
                    analize("login", "ptb_usernameland_ld", document.form2Logon.logonNo.value);
                    document.form2Logon.submit();
                    setTimeout(function () {
                        jQuery("#logonLoading1").attr("src", logonLoadingUrl);
                    }, 20);
                }

            }
            //控件证书
            if (certStrArray[1] == "2") {
                try {
                    var obj2 = "";
                    obj2 = new ActiveXObject("CITICSCP.MidInterface.1");
                    var pinDn = certStrArray[2] + ',' + certStrArray[3] + ',' + certStrArray[4] + ',' + certStrArray[5] + ',' + certStrArray[6];

                    var dllObj = document.getElementById("EPCSP");
                    obj2 = document.getElementById("oEdit2");

                    if (obj2.value == '') {
                        showLogonLoading();
                        alert(safeBoxPwd_1);
                        obj2.CreateFocus();
                        return;
                    }
                    if (obj2.value.length < 6) {
                        showLogonLoading();
                        alert(safeBoxPwd_2);
                        obj2.CreateFocus();
                        return;
                    }
                    if (obj2.value.length > 12) {
                        showLogonLoading();
                        alert(safeBoxPwd_3);
                        obj2.CreateFocus();
                        return;
                    }
                    var obj110 = returnObj();

                    obj110.SetSelectedCertificate(certStrArray[0]);
                    obj110.CryptInit();
                    document.formLogon.customerCtfType.value = "0";
                    document.formLogon.logonType.value = "0";
                    document.formLogon.session_certSubType.value = '3';
                    document.form2Logon.customerCtfType.value = "0";
                    document.form2Logon.logonType.value = "0";
                    document.form2Logon.session_certSubType.value = '0';
                    //20091102增加证书哈希值开始
                    document.formLogon.session_certHashcode.value = certStrArray[0];
                    document.form2Logon.session_certHashcode.value = certStrArray[0];
                    //20091102增加证书哈希值结束
                    var obj_verifyId = document.getElementById('verifyId');
                    if (obj_verifyId.style.display == '') {
                        if (document.formTmp.verifyCode.value == '' || document.formTmp.verifyCode.value.length != 4) {
                            showLogonLoading();
                            alert(verify_2);
                            document.formTmp.verifyCode.focus();
                            return;
                        }
                        document.formLogon.verifyCode.value = document.formTmp.verifyCode.value;
                        //验证校验码
                        executeAjaxCommand(checkVeriyUrl, ('verifyCode=' + document.formTmp.verifyCode.value), 'disposeMsgFileCert');
                        //验证校验码
                    } else {
                        //登录事件采集
                        analize("login", "ptb_usernameland_ld", document.form2Logon.logonNo.value);
                        document.form2Logon.submit();
                        setTimeout(function () {
                            jQuery("#logonLoading1").attr("src", logonLoadingUrl);
                        }, 20);
                    }

                } catch (e) {
                    if (confirm(logon_1)) {
                        var winDsp = 'width=' + (screen.availWidth - 24) + ',height=' + (screen.availHeight - 50) + ',toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
                        'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
                        window.open("/perbank6/pwd_show.htm", "secObject", winDsp);
                    }
                }
            }
            //一代KEY证书
            if (certStrArray[1] == "1") {
                document.keyLogonForm.customerCtfType.value = "0";
                document.keyLogonForm.logonType.value = "1";
                /**obj2 = document.getElementById("oEdit2");
                if(obj2.value=='' || obj2.value.length<6 || obj2.value.length>12){
                    showLogonLoading();
                    alert("请输入USBKEY密码!");
                    obj2.CreateFocus();	
                    return;
                }*/
                var obj = returnObj();
                //验证pin码
                obj.SetSelectedCertificate(certStrArray[0]);
                obj.CryptInit();
                var csp = certStrArray[certStrArray.length - 1];//取得证书类型  0：文件证书 1：飞天诚信的USBKEY证书 2：捷德的USBKEY证书 3:控件证书 4:握奇二代USBKEY证书 5：天地融二代USBKEY证书 
                document.keyLogonForm.session_certSubType.value = csp;
                //20091102增加证书哈希值开始
                document.formLogon.session_certHashcode.value = certStrArray[0];
                //20091102增加证书哈希值结束
                //验证校验码
                var obj_verifyId = document.getElementById('verifyId');
                if (obj_verifyId.style.display == '') {
                    if (document.formTmp.verifyCode.value == '' || document.formTmp.verifyCode.value.length != 4) {
                        showLogonLoading();
                        alert(verify_2);
                        document.formTmp.verifyCode.focus();
                        return;
                    }
                    document.keyLogonForm.verifyCode.value = document.formTmp.verifyCode.value;
                    //验证校验码
                    executeAjaxCommand(checkVeriyUrl, ('verifyCode=' + document.formTmp.verifyCode.value), 'disposeMsgKeyCert');
                    //验证校验码
                } else {
                    //登录事件采集
                    analize("login", "ptb_usbkeyland_ld", document.form2Logon.ca_id.value);
                    document.keyLogonForm.submit();
                    setTimeout(function () {
                        jQuery("#logonLoading1").attr("src", logonLoadingUrl);
                    }, 20);
                }

            }
            //二代KEY证书
            if (certStrArray[1] == "3") {
                document.keyLogonForm.customerCtfType.value = "0";
                document.keyLogonForm.logonType.value = "1";
                var csp = certStrArray[certStrArray.length - 1];//取得证书类型  0：文件证书 1：飞天诚信的USBKEY证书 2：捷德的USBKEY证书 3:控件证书 4:握奇二代USBKEY证书 5：天地融二代USBKEY证书 
                var obj = returnObj();

                obj.SetSelectedCertificate(certStrArray[0]);
                obj.CryptInit();
                document.keyLogonForm.session_certSubType.value = csp;
                //20091102增加证书哈希值开始
                document.keyLogonForm.session_certHashcode.value = certStrArray[0];
                //20091102增加证书哈希值结束
                var obj_verifyId = document.getElementById('verifyId');
                if (obj_verifyId.style.display == '') {
                    if (document.formTmp.verifyCode.value == '' || document.formTmp.verifyCode.value.length != 4) {
                        showLogonLoading();
                        alert(verify_2);
                        document.formTmp.verifyCode.focus();
                        return;
                    }
                    document.keyLogonForm.verifyCode.value = document.formTmp.verifyCode.value;
                    //验证校验码
                    executeAjaxCommand(checkVeriyUrl, ('verifyCode=' + document.formTmp.verifyCode.value), 'disposeMsgKeyCert');
                    //验证校验码
                } else {
                    //登录事件采集
                    analize("login", "ptb_usbkeyland_ld", document.form2Logon.ca_id.value);
                    document.keyLogonForm.submit();
                    setTimeout(function () {
                        jQuery("#logonLoading1").attr("src", logonLoadingUrl);
                    }, 20);
                }
            }
        }

    } catch (e) {
    }


} //end logon
function showLogonLoading() {
    jQuery("#logonLoading1").hide();
    jQuery("#logonButton").removeClass("loadingImgsSm");
    jQuery("#logonButton").attr("disabled", false);

}
//公告显示区

var gongGaoWinDsp = 'width=800,height=400,toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
				 'resizable=yes,location=no,left=100,top=100,screenx=' + 0 + ',screeny=' + 0;


var logonNewFlag = "0"; // 登录标志(0：用户名密码，1：二维码登录)
var hasqrcode = "0"; // 二维码是否生成标志(0：需要生成二维码，1：已经生成)
var canQryCodeFlag = "1"; // 轮询标志(0：需要轮询，1：不能轮询)
var requestQrCode; // bp返回的二维码标识
// 加密标识 0-国密 1-商密 add by zcc 20140107



//原脚本
var helpUrl = '';
function getHelpUrl() {
    return helpUrl;
}

function setHelpUrl() {
    helpUrl = "";
}

var certCustomerNum = "1";
//没有安装控件，需要显示密码无法输入
function showPwdCanNotUse() {
    if (checkFlag == 0 || checkFlag == 2) {
        document.getElementById("pwd_show_ID2").style.display = "";
        document.getElementById("ziti1").style.display = "";
        document.getElementById("pwdLi").style.display = "none";
    }
    if (checkFlag == 1) {
        document.getElementById("pwd_show_ID2").style.display = "";
        jQuery("#linkBtn").html("您的网银伴侣版本较低，请点击进行更新");
        document.getElementById("ziti1").style.display = "";
        document.getElementById("pwdLi").style.display = "none";
    }

    jQuery("#logonButton").attr("disabled", true);
}

//控制首页焦点位置
function controlFocus() {
    var obj_verifyId = document.getElementById('verifyId');
    if (obj_verifyId.style.display == '') {
        document.formTmp.verifyCode.focus();
    }
}
var firstFlg = "";
//获取证书的用户名
function getUserName() {
    pgeditor.pwdclear();
    var certStr = document.formTmp.certList.value;
    if (certStr == "-1") {
        document.formTmp.logonNo.value = "";
        document.getElementById("certUserId").style.display = "none";
        document.getElementById("noCertUserId").style.display = "none";
        if (firstFlg == "1") {
            ftFlg = "1";
        } else if (firstFlg == "2") {
            gdFlg = "1";
        }
        errShow();
        return;
    }
    certShow();
    if (certStr == "-2") {
        document.formTmp.logonNo.value = "";
        document.formTmp.logonNoCert.value = "";
        document.formLogon.logonType.value = "2";
        document.form2Logon.logonType.value = "2";
        document.getElementById("noCertUserId").style.display = "";
        document.getElementById("certUserId").style.display = "none";
        initValidate();
        return;
    }

    document.getElementById("type2").style.display = "";
    document.getElementById("logonLoading").style.display = "none";
    document.formTmp.password.value = "";
    document.formTmp.logonNo.value = "";
    document.formTmp.logonNoCert.value = "";
    document.formTmp.verifyCode.value = "";
    document.getElementById("certUserId").style.display = "none";
    document.getElementById("noCertUserId").style.display = "none";
    document.formLogon.logonType.value = "";
    document.form2Logon.logonType.value = "";
    var certStrArray = certStr.split(",");
    //以0,开头，则为文件证书
    if (certStrArray[1] == "0") {
        document.formLogon.logonType.value = "0";
        document.form2Logon.logonType.value = "0";
        //查询用户名
        // document.getElementById("showInput").style.display="";
        // document.getElementById("showMsg").style.display="";
        userIdForm.logonType.value = "0";
        userIdForm.ca_id.value = certStrArray[2] + ',' + certStrArray[3] + ',' + certStrArray[4] + ',' + certStrArray[5] + ',' + certStrArray[6];
        userIdForm.certSubType.value = certStrArray[7];
        document.formLogon.ca_id.value = userIdForm.ca_id.value;
        document.keyLogonForm.ca_id.value = userIdForm.ca_id.value;
        document.form2Logon.ca_id.value = userIdForm.ca_id.value;
        // userIdForm.submit();
        getCstLogonNum(userIdForm.ca_id.value, "0");
    }

    //以1,开头，则为一代key证书
    if (certStrArray[1] == "1") {
        if (firstFlg == "1") {
            ftFlg = "1";
        } else if (firstFlg == "2") {
            gdFlg = "1";
        }
        if (ftFlg == "1" || gdFlg == "1") {
            errShow();
            return;
        }
        document.keyLogonForm.logonType.value = "1";
        //查询用户名
        // document.getElementById("showMsg").style.display="";
        // document.getElementById("showInput").style.display="none";
        userIdForm.logonType.value = "1";
        userIdForm.ca_id.value = certStrArray[2] + ',' + certStrArray[3] + ',' + certStrArray[4] + ',' + certStrArray[5] + ',' + certStrArray[6];
        userIdForm.certSubType.value = certStrArray[7];
        document.formLogon.ca_id.value = userIdForm.ca_id.value;
        document.keyLogonForm.ca_id.value = userIdForm.ca_id.value;
        document.form2Logon.ca_id.value = userIdForm.ca_id.value;
        // userIdForm.submit();
        getCstLogonNum(userIdForm.ca_id.value, "0");
    }
    //<!-- 新增二代key分支 begin-->
    //以3,开头，则为二代key证书
    if (certStrArray[1] == "3") {
        document.keyLogonForm.logonType.value = "1";
        //查询用户名
        // document.getElementById("showMsg").style.display="";//正在查询用户名...
        // document.getElementById("showInput").style.display="none";//用户名输入框
        userIdForm.logonType.value = "1";//key证书（包括一代key和二代key）
        userIdForm.ca_id.value = certStrArray[2] + ',' + certStrArray[3] + ',' + certStrArray[4] + ',' + certStrArray[5] + ',' + certStrArray[6];
        userIdForm.certSubType.value = certStrArray[7];
        document.formLogon.ca_id.value = userIdForm.ca_id.value;
        document.keyLogonForm.ca_id.value = userIdForm.ca_id.value;
        document.form2Logon.ca_id.value = userIdForm.ca_id.value;
        // userIdForm.submit();
        getCstLogonNum(userIdForm.ca_id.value, "0");
    }
    //<!-- 新增二代key分支 end-->
    //以2,开头，则为控件证书
    if (certStrArray[1] == "2") {
        document.formLogon.logonType.value = "0";
        document.form2Logon.logonType.value = "0";
        //查询用户名
        // document.getElementById("showMsg").style.display="block";
        // document.getElementById("showInput").style.display="none";
        try {
            var obj2 = "";
            obj2 = new ActiveXObject("CITICSCP.MidInterface.1");
            var tempObjHTML = tempObjHTML + '<OBJECT classid="clsid:4BFE8042-A780-452F-AF17-9A424BE31B02" name="EPCSP" width="0" height="0" id="EPCSP" ></OBJECT>';
            document.getElementById("objectDivSafe").innerHTML = tempObjHTML;
        } catch (e) {
            if (confirm(logon_1)) {
                var winDsp = 'width=' + (screen.availWidth - 24) + ',height=' + (screen.availHeight - 50) + ',toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
                'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
                window.open("/perbank6/pwd_show.htm", "secObject", winDsp);
            }
        }
        userIdForm.logonType.value = "0";
        userIdForm.ca_id.value = certStrArray[2] + ',' + certStrArray[3] + ',' + certStrArray[4] + ',' + certStrArray[5] + ',' + certStrArray[6];
        userIdForm.certSubType.value = certStrArray[7];
        document.formLogon.ca_id.value = userIdForm.ca_id.value;
        document.keyLogonForm.ca_id.value = userIdForm.ca_id.value;
        document.form2Logon.ca_id.value = userIdForm.ca_id.value;
        //userIdForm.submit();
        getCstLogonNum(userIdForm.ca_id.value, "0");
    }
}

function disposeMsg(msg) {
    if (msg == 'error') {
        alert(verify_1);
        reGetIMG();
        document.formTmp.verifyCode.focus();
        document.formTmp.verifyCode.value = "";
    } else if (msg == 'succ') {
        document.formLogon.submit();
        setTimeout(function () { jQuery("#logonLoading1").attr("src", logonLoadingUrl); }, 20);
    } else if (msg.indexOf('长时间没有操作') != -1) {
        alert("图形验证码超时，请重新输入！");
        reGetIMG();
        document.formTmp.verifyCode.focus();
        document.formTmp.verifyCode.value = "";
    }
}

function disposeMsgFileCert(msg) {
    if (msg == 'error') {
        alert(verify_1);
        document.formTmp.verifyCode.focus();
        document.formTmp.verifyCode.value = "";
        reGetIMG();
    } else if (msg == 'succ') {
        document.formLogon.submit();
        setTimeout(function () { jQuery("#logonLoading1").attr("src", logonLoadingUrl); }, 20);
    } else if (msg.indexOf('长时间没有操作') != -1) {
        alert("图形验证码超时，请重新输入！");
        document.formTmp.verifyCode.focus();
        document.formTmp.verifyCode.value = "";
        reGetIMG();
    }
}
function disposeMsgKeyCert(msg) {
    if (msg == 'error') {
        alert(verify_1);
        document.formTmp.verifyCode.focus();
        document.formTmp.verifyCode.value = "";
        reGetIMG();
    } else if (msg == 'succ') {
        document.keyLogonForm.submit();
        setTimeout(function () { jQuery("#logonLoading1").attr("src", logonLoadingUrl); }, 20);
    } else if (msg.indexOf('长时间没有操作') != -1) {
        alert("图形验证码超时，请重新输入！");
        document.formTmp.verifyCode.focus();
        document.formTmp.verifyCode.value = "";
        reGetIMG();
    }
}
function setLogonType(type) {
    try {
        var logontype = type;
        if (logontype == "0") {//有证书登录，则要显示证书列表
            document.getElementById("type2").style.display = "";
            document.getElementById("logonLoading").style.display = "none";
            setDefault("certListCite", "certListUl", "certList", 0);
            document.formTmp.password.value = "";
            document.formTmp.logonNo.value = "";
            document.formTmp.logonNoCert.value = "";
            document.formTmp.verifyCode.value = "";
            document.getElementById("noCertUserId").style.display = "none";
            document.formLogon.logonType.value = "";
            document.form2Logon.logonType.value = "";
            var certListUl = getSelectList("certListUl");
            if (certListUl.length == 2) {
                setDefault("certListCite", "certListUl", "certList", 1);
                getUserName();
            }
            document.getElementById("oEdit2").Clear();
        } else {//无证书登录，显示登录方式选择
            ftFlg = "1";
            if (keyVerCheck()) {
                document.getElementById("type2").style.display = "none";
                document.getElementById("logonLoading").style.display = "none";
                document.getElementById("noCertUserId").style.display = "";
                document.formTmp.password.value = "";
                document.formTmp.logonNo.value = "";
                document.formTmp.logonNoCert.value = "";
                document.formTmp.verifyCode.value = "";
                document.formLogon.logonType.value = "2";
                document.form2Logon.logonType.value = "2";
                initValidate();
                document.getElementById("oEdit2").Clear();
            } else {
                document.getElementById("noCertUserId").style.display = "";
                document.getElementById("logonLoading").style.display = "none";
            }
        }
    } catch (e) {
        document.getElementById("logonLoading").style.display = "none";
    }
}

function isActiveXRegistered(ctlname) {
    try {
        var comActiveX = new ActiveXObject(ctlname);
    } catch (e) {
        return false;
    }
    return true;
}

function isActiveXSupported() {

    //xmlhttp对象
    var kXmlHttp = null;

    //非微软IE支持的xmlhttp对象
    try {
        if (typeof XMLHttpRequest != "undefined") {
            kXmlHttp = new XMLHttpRequest();
            return true;
        }
    } catch (e) { }

    //微软IE支持的xmlhttp对象
    var aVersionhs = ["MSXML2.XMLHttp.5.0",
                        "MSXML2.XMLHttp.4.0",
                        "MSXML2.XMLHttp.3.0",
                        "MSXML2.XMLHttp",
                        "Microsoft.XMLHttp"];
    for (var i = 0; i < aVersionhs.length; i++) {
        try {
            kXmlHttp = new ActiveXObject(aVersionhs[i]);
            return true;
        } catch (e) { }
    }
    return false;
}

function creatPwd() {
    pgeditor = createLogonPinBlock("logonPwdId", "ocxEdit", "beforeLogon()", "");
    encryptKeyExchange(pgeditor, createMcryptKeyUrl);
}
function certShow() {
    document.getElementById("linkBtn3").style.display = "none";
    document.getElementById("linkBtn4").style.display = "none";
    document.getElementById("linkBtn5").style.display = "none";
    document.getElementById("pwd_show_ID2").style.display = "none";
    document.getElementById("linkBtn").style.display = "";
    document.getElementById("ziti1").style.display = "none";
    document.getElementById("pwdLi").style.display = "";
    document.getElementById("logonButton").disabled = false;
}
function errShow() {
    document.getElementById("logonLoading").style.display = "none";
    if (browserFlag == "2") {
        document.getElementById("linkBtn4").style.display = "";
        document.getElementById("pwd_show_ID2").style.display = "";
        document.getElementById("linkBtn").style.display = "none";
        document.getElementById("ziti1").style.display = "";
        document.getElementById("pwdLi").style.display = "none";
        document.getElementById("logonButton").disabled = true;
    } else {
        if (ftFlg == "1") {
            firstFlg = "1";
            document.getElementById("linkBtn3").style.display = "";
            document.getElementById("linkBtn5").style.display = "none";
        } else if (gdFlg == "1") {
            firstFlg = "2";
            document.getElementById("linkBtn5").style.display = "";
            document.getElementById("linkBtn3").style.display = "none";
        }
        document.getElementById("pwd_show_ID2").style.display = "";
        document.getElementById("linkBtn").style.display = "none";
        document.getElementById("ziti1").style.display = "";
        document.getElementById("pwdLi").style.display = "none";
        document.getElementById("logonButton").disabled = true;
    }
    //初始化信息
    ftFlg = "0";
    gdFlg = "0";
    installFlg = "1";
}

function keyVerCheck() {

    try {
        if (browserFlag == "0") {

            var obj_npft_citic = document.getElementById("embed4");//飞天
            var obj_npgd_citic = document.getElementById("embed5");//捷德

            if (ftFlg == "1") {
                var npft_version = obj_npft_citic.FT_GetVer();
            } else if (gdFlg == "1") {
                var npgd_version = obj_npgd_citic.GD_GetVer();
            }
        } else if (browserFlag == "1") {

            var obj_Init_Tool_FT = document.getElementById("Init_Tool_FT");
            var obj_Init_Tool_GD = document.getElementById("Init_Tool_GD");

            if (ftFlg == "1") {
                var ft_Version = obj_Init_Tool_FT.GetVer();
            } else if (gdFlg == "1") {
                var gd_Version = obj_Init_Tool_GD.GetVer();
            }

        } else if (browserFlag == "2") {
            try {
                objCert = document.getElementById("osDetecter");
                var ios_ft_version = "";
                if (ftFlg == "1") {
                    ios_ft_version = objCert.YTEC_Get_DriveVersion();
                }
                var ver = ios_ft_version.split('.')[2];
                if ((ver - '0') == 0) {
                    errShow(); //错误提示信息
                    return false;
                }
            } catch (e) {
                errShow(); //错误提示信息
                return false;
            }
        }
        //初始化信息
        ftFlg = "0";
        gdFlg = "0";
        return true;
    } catch (e) {
        errShow(); //错误提示信息
        return false;
    }
}

//控制密码控件显示
function controlPwdDisplay(isDiplay) {
    var logonPwdObj = document.getElementById("logonPwdId");
    var objectDivObj = document.getElementById("objectDivId");
    var tempObjHTML = '';
    if ((navigator.userAgent.indexOf("MSIE") > 0) || (navigator.userAgent.indexOf("Trident") > 0)) {
        var certId = document.getElementById("certId");
        certId.innerHTML = PBank.message['objectid'];
    }
    if (isDiplay) {
        /*if((navigator.userAgent.indexOf("MSIE")>0)||(navigator.userAgent.indexOf("Trident")>0)){
            var certId=document.getElementById("certId");
            certId.innerHTML=PBank.message['objectid'];
        }*/
        creatPwd();
    } else {
        document.getElementById("pwd_show_ID2").style.display = "";
        jQuery("#logonButton").attr("disabled", true);
        // logonPwdObj.innerHTML='<input type="password" class="inputOut" size="22" readonly id="oEdit1">';
        // objectDivObj.innerHTML='';
    }
}
function beforeLogon() {
    var obj_verifyId = document.getElementById('verifyId');
    if (obj_verifyId.style.display == 'none' && jQuery("#logonButton").prop("disabled") == false) {
        logon();
    }
}
//判断IE浏览器版本
function judgeBrowserVersion() {
    var user_agent = navigator.userAgent.toLowerCase();
    if (user_agent.indexOf("msie") > 0) {
        //是否是IE浏览器 
        var regStr_ie = /msie [\d.]+;/gi;
        var versionStr = user_agent.match(regStr_ie).toString().replace(/[^0-9.]/ig, "");
        var mainVersion = versionStr.split(".")[0];
        if (Number(mainVersion) < 7) {
            return false;
        } else {
            return true;
        }
    } else if (user_agent.indexOf("chrome") > 0) {
        var regStr_chrome = /chrome\/[\d.]+/gi;
        var versionStr = user_agent.match(regStr_chrome).toString().replace(/[^0-9.]/ig, "");
        console.log(versionStr);
        var mainVersion = versionStr.split(".")[0];
        console.log(mainVersion);
        if (Number(mainVersion) >= 42) {
            alert("暂不支持chrome42及以上版本浏览器使用！");
            //IE6.0 
            return false;
        } else {
            return true;
        }
    }
}


function GetMACAddress() {
    try {
        var mac;
        mac = GetEigenvalue();
        if (mac.length > 99) {
            mac = mac.substring(0, 99);
        }

        document.formLogon.session_userRemoteMAC.value = mac;
        document.keyLogonForm.session_userRemoteMAC.value = mac;
        document.form2Logon.session_userRemoteMAC.value = mac;
        document.onlineUnifyRegister.session_userRemoteMAC.value = mac;
        document.find_pwd_qryInput_req.session_userRemoteMAC.value = mac;
        document.checkLogonFailNumForm.session_userRemoteMAC.value = mac;
    } catch (e) {
        document.formLogon.session_userRemoteMAC.value = "";
        document.keyLogonForm.session_userRemoteMAC.value = "";
        document.form2Logon.session_userRemoteMAC.value = "";
        document.onlineUnifyRegister.session_userRemoteMAC.value = "";
        document.find_pwd_qryInput_req.session_userRemoteMAC.value = "";
        document.checkLogonFailNumForm.session_userRemoteMAC.value = "";
    }
}

function GetStoreType(dn) {
    var tp;
    tp = GetStoreTypes(dn);
    if (tp == '1')      //文件证书
        return "0";
    else if (tp == '2') //移动证书
        return "1";
    else            //未知类型，请检查证书是否正确安装
        return "3";
}

function getHelp() {
    var winDsp = 'width=' + (screen.availWidth - 24) + ',height=' + (screen.availHeight - 50) + ',toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
                 'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
    helpWin = window.open("/perbank6/5.0helpcenter/HelpCenter.html", "help", winDsp);
    helpWin.focus();
}

function getSafe() {
    var winDsp = 'width=480,height=380,toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
                 'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
    helpWin = window.open("/perbank6/safe.html", "safe", winDsp);
    helpWin.focus();
}

function getFiveStar() {
    var winDsp = 'width=480,height=380,toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
                 'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
    helpWin1 = window.open("/perbank6/fiveStar.html", "fiveStar", winDsp);
    helpWin1.focus();
}

function getFirstLogon() {
    var winDsp = 'width=' + (screen.availWidth - 24) + ',height=' + (screen.availHeight - 50) + ',toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
                 'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
    helpWin = window.open("/perbank6/firstLogon.htm", "", winDsp);
    helpWin.focus();
}

//初始化密码值
function InitValue(objName, length, length2) {
    obj = document.getElementById(objName);
    obj.MaxLength = length;
    obj.Border = false;
    obj.PasswordIntensityMinLength = length2;
    obj.PasswordIntensityRegularExpression = "\\W*((\\d+\\W*[a-z|A-Z]+)|([a-z|A-Z]+\\W*\\d+))\\W*";
}

function showMessage() {
    var winDsp = 'width=400,height=200,toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
                 'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
    messageWin = window.open("/perbank6/showMessage.html", "", winDsp);
    messageWin.focus();
}

function showBoxInfo() {
    var winDsp = 'width=' + (screen.availWidth - 24) + ',height=' + (screen.availHeight - 50) + ',toolbar=no,menubar=no,titlebar=yes,scrollbars=yes,status=yes,' +
    'resizable=yes,location=no,left=' + 5 + ',top=' + 5 + ',screenx=' + 0 + ',screeny=' + 0;
    secWin = window.open("5.0helpcenter/flile_certmanage.html", "secObject", winDsp);
    secWin.focus();
}

function disposeVerifyCode(msg) {
    if (msg == 'error') {
        showLogonLoading();
        document.getElementById('verifyImg').src = "images/default/stop_button.gif";
        document.getElementById('verifyImg').style.display = 'inline';
        alert(verify_1);
        document.formTmp.verifyCode.focus();
        document.formTmp.verifyCode.value = "";
        reGetIMG();
    } else if (msg == 'succ') {
        document.getElementById('verifyImg').src = "images/default/start_button.gif";
        document.getElementById('verifyImg').style.display = 'inline';
    } else if (msg.indexOf('长时间没有操作') != -1) {
        showLogonLoading();
        document.getElementById('verifyImg').src = "images/default/stop_button.gif";
        document.getElementById('verifyImg').style.display = 'inline';
        alert("图形验证码超时，请重新输入！");
        document.formTmp.verifyCode.focus();
        document.formTmp.verifyCode.value = "";
        reGetIMG();
    }
}
function preOnload() {
    setTimeout(function () { preOnload1() }, 1000);
    //  jQuery("#gonggao").load("/perbank6/htmlFile/sListPage.html");
}

function macDownload() {
    jQuery("#linkBtn").hide();
    jQuery("#linkBtn2").show();
}


function showPwdNotUse() {
    document.getElementById("pwd_show_ID2").style.display = "";
    jQuery("#linkBtn").html("您未安装Mac版网银控件，请点击前往下载中心下载");
    jQuery("#linkBtn").attr("href", "#");
    jQuery("#linkBtn").attr("onclick", "javascript:helpUrl='downloadCenter.html';getHelp();macDownload();");
    jQuery("#logonButton").attr("disabled", true);
}


//判断是否安装了mac版密码控件
function checkOS() {
    try {
        var plugin = navigator.plugins['CITICEdit'].description;

        if (plugin.indexOf(":") > 0) {
            arr = plugin.split(":");
            var pge_version = arr[1];
            return true;
        } else {
            var pge_version = "";
            return false;
        }
    } catch (e) {
        return false;
    }


}

//清除pin码缓存
function clearPinCache() {
    try {
        SZPAPlutoCtrl = document.getElementById('embed4');
        cache = false;
        SZPAPlutoCtrl.FT_CachePin(cache);
    } catch (e) {

    }

}

function CheckKeyDriver() {

    //飞天	
    clearPinCache();
    if (browserFlag == "0") { //非IE
        try {
            ftFlg = "1";
            var myFTocx = document.getElementById("embed4");//飞天
            var retFT = myFTocx.FT_GetVer();
            ftFlg = "0";
            /*gdFlg = "1";
            var myGDocx = document.getElementById("embed5");//捷德
            var retGD = myGDocx.GD_GetVer();
            gdFlg = "0";*/
        } catch (e) {
            errShow();
            return false;
        }
    } else if (browserFlag == "1") {//IE
        try {
            ftFlg = "1";
            var myFTocx = document.getElementById("Init_Tool_FT");
            var retFT = myFTocx.GetVer();
            ftFlg = "0";
            /*gdFlg = "1";
            var myGDocx = document.getElementById("Init_Tool_GD");
            var retGD = myGDocx.GetVer();
            gdFlg = "0";*/
        } catch (e) {
            errShow();
            return false;
        }

    } else if (browserFlag == "2") { //MAC
        try {
            ftFlg = "1";
            objCert = document.getElementById("osDetecter");
            var ios_ft_version = objCert.YTEC_Get_DriveVersion();
            var ver = ios_ft_version.split('.')[2];
            if ((ver - '0') == 0) {
                document.getElementById("logonLoading").style.display = "none";
                errShow(); //错误提示信息
                return false;
            } else {
                ftFlg = "0";
            }
        } catch (e) {
            errShow(); //错误提示信息
            return false;
        }
    }
    ftFlg = "0";
    gdFlg = "0";
}


//登陆页预加载
function preOnload1() {
    try {
        if (browserFlag != "2") {
            if (CheckSecurity()) {
                GetCertificates(); //获取文件证书
                if (installFlg == "0") {
                    CheckKeyDriver();
                }
                GetMACAddress();   //获取MAC地址
            } else {
                showPwdCanNotUse();
            }
        } else {
            if (checkOS()) {
                document.getElementById("type2").style.display = ""; //隐藏证书列表
                document.getElementById("logonLoading").style.display = "none";
                document.formLogon.logonType.value = "2";//无证书登录
                controlPwdDisplay(true);
                setLogonType('2');
                jQuery("#logonPwdIdText").show();
                jQuery(".logonPwdId_li").css("background", "#fff");
                jQuery("#ocxEdit").css("border", "0");
                jQuery("#ocxEdit").show();
            } else {
                document.getElementById("type2").style.display = ""; //隐藏证书列表
                document.getElementById("logonLoading").style.display = "none";
                document.formLogon.logonType.value = "2";//无证书登录
                controlPwdDisplay(false);
                setLogonType('2');
                showPwdNotUse();

            }

        }
        var logonFailNum = logonCtrlFailNums;
        if (logonFailNum != '' && logonFailNum != '0') {
            jQuery(".alertlogin ul li").css("padding", "10px 0px");
            document.getElementById("pinImg").className = "imgVerityCode";
            document.getElementById("verifyId").style.display = "";
            document.getElementById("verifyIdTxt").style.display = "";
        }
        //ie7下隐藏查询按钮上面空行
        if (navigator.userAgent.indexOf("MSIE") >= 0) {
            var version = navigator.appVersion.split(";");
            var trimVersion = version[1].replace(/(^\s*)|(\s*$)/g, "");
            if (navigator.appName == "Microsoft Internet Explorer" && trimVersion == "MSIE 7.0") {
                jQuery("#verifyIdTxt").hide();
            }
        }
    } catch (e) { };
}

//多语言切换
function switchLang(lang) {
    var reqDo = "signIn.do?language=" + lang;
    window.open(reqDo, "perbank3", "");
}
function showComperFirst() {
    logonNewFlag = "0";
    hasqrcode = "0";
    canQryCodeFlag = "1";
}

// 使用帮助
/*
function getHelp() {
    document.getElementById('QrCodeImage').style.display = "none";
    document.getElementById('QrCodeImageSucc').style.display = "none";
    document.getElementById('QrCodeImageEorr').style.display = "none";
    document.getElementById('QrCodeImageHelp').style.display = "";
}*/
window.onresize = function () {
    try {
        if (document.documentElement.clientHeight > 802) {
            jQuery(".main").css("height", document.documentElement.clientHeight - 452 - 110);
        } else {
            jQuery(".main").css("height", "240px");
        }
    } catch (e) { }
}

jQuery(function () {
    try {
        if (document.documentElement.clientHeight > 802) {
            jQuery(".main").css("height", document.documentElement.clientHeight - 452 - 110);
        } else {
            jQuery(".main").css("height", "240px");
        }
    } catch (e) { }
});

function noticeLoaded() {
    var isIE7 = !!navigator.userAgent.match(/MSIE 7.0/);
    var isIE8 = !!navigator.userAgent.match(/MSIE 8.0/);
    var subWeb = document.frames ? document.frames["mainframe"].document : document.getElementById("mainframe").contentDocument;
    jQuery(subWeb.body).find("ul li").css({ "padding": "0px", "line-height": "40px", "clear": "both" });
    if (isIE7 || isIE8) {
        jQuery(subWeb.body).css("background-color", "#eff0f2");
        jQuery(subWeb.body).find("ul").css("margin-left", "0px");
    }
}
