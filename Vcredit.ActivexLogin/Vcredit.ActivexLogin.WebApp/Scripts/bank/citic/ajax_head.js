function newXMLHttpRequest() {
    var xmlreq = false;
    if (window.XMLHttpRequest) {
        xmlreq = new XMLHttpRequest();
        if (xmlreq.overrideMimeType) {//设置MiME类别
            xmlreq.overrideMimeType('text/xml');
        }
    } else if (window.ActiveXObject) {
        try {
            xmlreq = new ActiveXObject("Msxml2.XMLHTTP");
        } catch (e1) {
            try {
                xmlreq = new ActiveXObject("Microsoft.XMLHTTP");
            } catch (e2) { }
        }
    }
    return xmlreq;
}
//处理返回信息 
//xmlHttp返回值,
//method:方法名 方法必须带一个参数如doRecive(xNode);
function handleAjaxResult(req, method) {
    return function () {
        if (req.readyState == 4) {
            if (req.status == 200) {
                // 将载有响应信息的XML传递到处理函数
                if (window.ActiveXObject) {
                    var xmlDom = new ActiveXObject("Microsoft.XMLDOM");
                }
                else {
                    if (document.implementation && document.implementation.createDocument) {
                        var xmlDom = document.implementation.createDocument("", "", null);
                    }
                }
                //处理非法字符返回信息
                var returnText = req.responseText;
                if (returnText.indexOf('EP00090') != -1)
                    alert("交易信息中存在非法字符！");
                else
                    eval(method + "(req.responseText);");
                return;
            } else {
                try {
                    console.log("HTTP error: " + req.status);
                } catch (e) { }
                //alert("HTTP error: "+req.status);
            }
        }
    }
}

//执行客户端Ajax命令
//url 数据post地址
//postData 发送的数据包
//handleMethod　处理返回的方法
function executeAjaxCommand(url, postData, handleMethod) {
    parent.urlFrom = "";
    var req = newXMLHttpRequest();
    req.onreadystatechange = handleAjaxResult(req, handleMethod);
    req.open("POST", url, true);
    req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
    req.send(postData);
}

//执行客户端Ajax命令
//url 数据post地址
//postData 发送的数据包
//handleMethod　处理返回的方法
//实现页面提交同步
function executeAjaxCommand2(url, postData, handleMethod) {
    parent.urlFrom = "";
    var req = newXMLHttpRequest();
    req.onreadystatechange = handleAjaxResult(req, handleMethod);
    req.open("POST", url, false);
    req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
    req.send(postData);
}

//执行客户端Ajax命令
//url 数据post地址
//postData 发送的数据包
//handleMethod　处理返回的方法
//以get方式处理文件
function executeAjaxCommand3(url, postData, handleMethod) {
    parent.urlFrom = "";
    var req = newXMLHttpRequest();
    req.onreadystatechange = handleAjaxResult(req, handleMethod);
    req.open("GET", url, true);
    req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
    req.send(postData);
}

//执行客户端Ajax命令
//url 数据post地址
//postData 发送的数据包
//handleMethod　处理返回的方法
//实现页面提交同步
function executeAjaxCommand4(url, postData) {
    parent.urlFrom = "";
    var req = newXMLHttpRequest();
    req.open("POST", url, false);
    req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
    req.send(postData);
    if (req.readyState == 4) {
        if (req.status == 200) {
            // 将载有响应信息的XML传递到处理函数
            if (window.ActiveXObject) {
                var xmlDom = new ActiveXObject("Microsoft.XMLDOM");
            }
            else {
                if (document.implementation && document.implementation.createDocument) {
                    var xmlDom = document.implementation.createDocument("", "", null);
                }
            }
            //var objXMLDoc=new ActiveXObject("Microsoft.XMLDOM");
            var returnText = req.responseText;
            if (returnText.indexOf('EP00090') != -1) {
                //alert("交易信息中存在非法字符！");
                return "";
            } else
                return req.responseText;

        } else {

            //alert("HTTP error: " + req.status);
            return "";
        }
    } else {
        return "";
    }
}

function getNode(strXML, strName) {
    strNameLength = strName.length;
    var startPOS = strXML.indexOf("<" + strName + ">");
    if (startPOS == -1) {
        return "";
    }
    var endPOS = strXML.indexOf("</" + strName + ">", startPOS);
    if (endPOS == -1)
        return strXML.substring(startPOS + strNameLength + 2);
    else
        return strXML.substring(startPOS + strNameLength + 2, endPOS);
}

function cutNode(strXML, strName) {
    strNameLength = strName.length;
    var startPOS = strXML.indexOf("<" + strName + ">");
    if (startPOS == -1) {
        return "";
    }
    var endPOS = strXML.indexOf("</" + strName + ">", startPOS);
    if (endPOS == -1)
        return strXML.substring(0, startPOS + strNameLength + 2);
    else
        return strXML.substring(0, startPOS) + strXML.substring(endPOS + strNameLength + 3, strXML.length);
}

function getNodes(strXML, strName) {
    strNameLength = strName.length;
    strLength = strXML.length;
    var varArray = new Array();
    var varArrayIdx = 0;
    var startPOS = 0;
    var endPOS;
    while ((startPOS = strXML.indexOf("<" + strName + ">", startPOS)) != -1) {
        endPOS = strXML.indexOf("</" + strName + ">", startPOS);
        if (endPOS == -1) {
            varArray[varArrayId++] = strXML.substring(startPOS + strNameLength + 2);
            return varArray;
        }
        else
            varArray[varArrayIdx++] = strXML.substring(startPOS + strNameLength + 2, endPOS);
        startPOS = endPOS + strNameLength + 1;
        if (startPOS >= strLength)
            break;
    }
    return varArray;
}
function listdisplay(str) {
    document.getElementById(str).style.display = '';
}

//发送 --  [新网银已重写为：doSendNew(formTag,linkurl,func)] 【老网银方法，待删除】
function doSend(formTag, linkurl, func) {
    /*var msg="";
    　var reciver=""; 
    　var postData="";
      var inputTag = formTag.all.tags("input");
      for (i=0;i<inputTag.length;i++){
          if (postData=="")
             postData = inputTag[i].getAttribute("name") + "=" + inputTag[i].getAttribute("value");
          else
             postData = postData + "&" + inputTag[i].getAttribute("name") + "=" + inputTag[i].getAttribute("value");
      }
      postData= encodeURI(postData);	
      postData= encodeURI(postData);
      executeAjaxCommand(linkurl,postData,func);
      */
    doSendNew(formTag, linkurl, func);
}

//发送  --  [重写替换doSend(formTag,linkurl,func)]  【新网银方法】
function doSendNew(formTag, linkurl, func) {
    var postData = jQuery(formTag).serialize();

    linkurl = encodeURI(linkurl);
    linkurl = encodeURI(linkurl);
    executeAjaxCommand(linkurl, postData, func);
}

//发送
function doSend3(formTag, linkurl, func) {
    var postData = jQuery(formTag).serialize();

    linkurl = encodeURI(linkurl);
    linkurl = encodeURI(linkurl);
    executeAjaxCommand(linkurl, postData, func);
}


//发送,同步
function doSend2(formTag, linkurl, func) {
    var postData = jQuery(formTag).serialize();

    linkurl = encodeURI(linkurl);
    linkurl = encodeURI(linkurl);
    executeAjaxCommand2(linkurl, postData, func);
}

//发送,同步,并对数据进行encodeURIComponent处理 --  [新网银已重写为：doSend4New(formTag,linkurl)] 【老网银方法，待删除】
function doSend4(formTag, linkurl) {
    var postData = jQuery(formTag).serialize();

    linkurl = encodeURI(linkurl);
    linkurl = encodeURI(linkurl);
    return executeAjaxCommand4(linkurl, postData);
}

//发送,同步,并对数据进行encodeURIComponent处理  -- [重写替换doSend(formTag,linkurl,func)]  【新网银方法】
function doSend5(formTag, linkurl) {
    var postData = jQuery(formTag).serialize();

    linkurl = encodeURI(linkurl);
    linkurl = encodeURI(linkurl);
    return executeAjaxCommand4(linkurl, postData);
}
//执行ajax执行异步加载的页面中JS
function execAjaxLoadScript(msg) {
    var regDetectJs = /<script(.|\n)*?>(.|\n|\r|\n)*?<\/script>/ig;
    var jsContained = msg.match(regDetectJs);
    if (jsContained) {
        var regGetJs = /<script(.|\n)*?>((.|\n|\r|\n)*)?<\/script>/im;
        var jsNums = jsContained.length;
        for (var i = 0; i < jsNums; i++) {
            var jsSection = jsContained[i].match(regGetJs);
            if (jsSection[2]) {
                if (window.execScript) {
                    window.execScript(jsSection[2]);
                } else {
                    window.eval(jsSection[2]);
                }
            }
        }
    }
}