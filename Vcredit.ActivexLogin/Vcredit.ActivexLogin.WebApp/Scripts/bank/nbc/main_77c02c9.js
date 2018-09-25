;
/*!/src/app/main/bannerloginController.js*/
angular.module("bannerLogin", ["ngRoute"]).config(["$routeProvider",
function (e) {
    e.when("/login/:paramsid?", {
        template: '<div ng-controller="bannerloginController" class="login bannerlogin"><div class="login_boxinline" style="border-radius: 10px;"><div class="loginNotice" ><div class="loginSwitcher loginSwitcherLogo" ng-class="{\'loginSwitcherLogo\': (!QRCode.isQRShow)}" ng-click="QRCode.switcher()"></div><div class="swictherArrow"></div><div class="swictherArrowBlank"></div></div></div><div class="loginSwitcherTitle">{{QRCode.switcherTitle}}</div></div><div class = "innerbox" ng-show="!QRCode.isQRShow" style="padding: 15px;"><form name="mainForm" ><ul><li class = "upspace"></li><li class="loginFormInput clearfix"><div class="clearfix"><s class="people"></s><input type="text" tabindex="1" name="userName" class="input-text login-input-text " ng-model="userName" placeholder="账号 /登录名 /身份证 /手机号"    ng-blur="getCheckCodeFlag();" ng-disabled="userNameDisabled" autocapitalize="off" autocomplete="off"/><div class="cross" ng-show="isCrossShow" ng-click="cleanUserName();"></div></div><p ng-class="{loginerror: isCheckCode,loginuser: !isCheckCode}"><em ng-show="userNameIserror">请输入用户名</em></p></li><li class="loginFormInput clearfix"><div class="clearfix"><s  class="peoplelock"></s><div passwordctrl check-ereg="regPwd" max-length="maxLength" class="passwordctrl " pass-operator="passOperator" enter-callback="enterClick();"></div></div><p class="errorMessage" ng-class="{loginerror: isCheckCode,loginuser: !isCheckCode}"><em ng-show="passwordIserror">请正确输入密码</em></p></li><li ng-if="isCheckCode" ><div class = "codeControl"><input class="codeinput input-text login-input-text" tabindex="3" type="text" name="verifyImage" maxlength="4" ng-model="$parent.verifyImageCode"    ng-keydown="enterClick($event);" placeholder="不区分大小写" /><img class="verifyImageCode pointer" title="看不清？点击更换" ng-src="{{verifyImageUrl}}" ng-click="updateVerifyImage();"/></div><p class= "loginerror"><em ng-show="verifyImagerequired">请输入图形验证码</em><em ng-show="verifyImageIserror">请正确输入图形验证码</em></p></li><li><a ng-click="login()" class="btn1 aButton" id="loginBtn2">登&nbsp;&nbsp;录</a><p class="loginerror"><em ng-show="isError" ng-bind="errMsg"></em></p></li><li class="control-group common_show" ><div class="controls clearfix" style="padding-right: 30px;"><span class="nb-assist fr"><div style="float:right" ></div></span></div></li></ul></form><p class=" online-account"><a ng-click="jumpRepass()" >设置密码</a>&nbsp;|&nbsp;<a href="#/HB00900" class="red">在线开户</a></p></div><div ng-init="QRCode.initQRcode();" ng-if="QRCode.isQRShow" class="scanCode"><div class="qrCodeContainer"><div class="qrCodeContent" ng-show="!QRCode.qrloading"><img ng-src="{{QRCode.qrCodeUrl}}" class="qrCodePic"/><div class="qrCodeContentContainer" ng-if="QRCode.mask.isMask"><div class="qrCode-backdrop"></div><div class="qrCodeCoreContent"><p class="qrCode-tooltip-content" ng-class="{\'qrCode-tooltip-icon-ok\': (!QRCode.mask.isError),\'qrCode-tooltip-icon-error\': (QRCode.mask.isError)}"></p><p>{{ (isError && errMsg) ? errMsg : QRCode.mask.showMessage}}</p></div></div></div><br/><br/></div><div class="passwordMFM"><div passwordctrl pass-operator="$parent.passOperatorMFM" class="mypasswordctrl"/></div></div></div><div cert-info on-init="initCertInfo($operator)"></div></div>',
        controller: "bannerloginController"
    })
}]).controller("bannerloginController", ["$scope", "$log", "$httpPlus", "$routeHelper", "$utb", "$loginFlow", "$message", "$location", "$timeout", "$interval",
function (e, r, s, o, i, a, n, t, c, l) {
    function d() {
        _ = I(),
        e.passOperator = null,
        e.enterClick = h,
        e.login = w,
        e.verifyImageUrl = i.getUrl("VerifyImage") + "&flag=" + (new Date).getTime(),
        e.updateVerifyImage = v,
        e.isCheckCode = !1,
        e.getCheckCodeFlag = Q,
        e.regPwd = "^[A-Za-z0-9!#$%&()*+,-./:;<=>?@[\\]^_{|}~]{6,12}$",
        e.maxLength = "12",
        e.QRCode = {},
        e.QRCode.initQRcode = f,
        e.QRCode.switcher = m,
        //e.QRCode.switcherNotice = "点此二维码登录",
        e.QRCode.switcherTitle = "账密登录",
        e.initCertInfo = g,
        e.userNameDisabled = !1,
        e.isCrossShow = !1,
        e.cleanUserName = p
    }
    function g(r) {
        try {
            r.getCertSN().then(function (r) {
                M = r,
                M && s.post("PB08001_queryName.do", {
                    certID: M
                }).success(function (r) {
                    if ("0000" == r.ec) {
                        var s = r.cd.customerId,
                        o = r.cd.userName;
                        10 == s.length && o && o.length > 0 && (e.userName = o, e.userNameDisabled = !0, N = !0, Q(), "0" == r.cd.flag && (e.isCrossShow = !0))
                    }
                })
            })
        } catch (o) { }
    }
    function p() {
        var r = function () {
            e.userName = "",
            e.userNameDisabled = !1,
            N = !1,
            M = "",
            e.isCrossShow = !1
        };
        n.confirm("您将关闭UK便捷登录服务，如需重新开启，登录后请至安全设置-登录方式管理处进行设置。",
        function () {
            s.post("updateUKSetting.do", {
                certID: M
            }),
            r()
        })
    }
    function m() {
        e.QRCode.isQRShow = !e.QRCode.isQRShow,
        e.QRCode.isQRShow ? (e.QRCode.switcherNotice = "点此账密登录", e.QRCode.switcherTitle = "扫码登录") : (u(), e.QRCode.switcherNotice = "点此扫码登录", e.QRCode.switcherTitle = "账密登录")
    }
    function u() {
        var r = f;
        r._timerHandler && l.cancel(r._timerHandler),
        r._timerHandler2 && c.cancel(r._timerHandler2),
        e.QRCode.qrCodeUrl = "",
        e.isError = !1,
        e.errMsg = ""
    }
    function f() {
        var r = $("div.pwdClass").text(),
        o = "",
        a = arguments.callee,
        t = null;
        e.QRCode.qrloading = !0,
        e.QRCode.mask = {
            isMask: !1,
            isError: !1,
            showMessage: ""
        },
        e.isError = !1,
        e.errMsg = "",
        i.getSrandNum(function () {
            s.post("generateToken.do", {
                actionName: "tokenLogin"
            }).success(function (s) {
                if ("0000" == s.ec) {
                    if (o = s.cd.accessToken, r.indexOf("安装控件") > 0) return n.alert("尊敬的客户：请您先安装密码控件。"),
                    d();
                    e.QRCode.qrCodeUrl = i.getUrl("generateQRCode.do", {
                        accessToken: s.cd.accessToken
                    }),
                    e.QRCode.qrloading = !1,
                    t && t()
                } else n.alert(s.em)
            })
        }),
        t = function () {
            a._timerHandler && l.cancel(a._timerHandler),
            a._timerHandler2 && c.cancel(a._timerHandler2),
            a._timerHandler = l(function () {
                var r = e.passOperatorMFM ? e.passOperatorMFM.getControlParams() : {};
                r.accessToken = o,
                s.post("tokenLogin.do", r).success(function (r) {
                    if ("0000" == r.ec) {
                        var s = r.cd.tokenState;
                        "1" == s ? e.QRCode.mask = {
                            isMask: !0,
                            isError: !1,
                            showMessage: "扫描成功，待授权"
                        } : "2" == s ? (e.QRCode.mask = {
                            isMask: !0,
                            isError: !1,
                            showMessage: "授权成功，登录中..."
                        },
                        a._timerHandler && l.cancel(a._timerHandler), C(o)) : "3" == s && (e.QRCode.mask = {
                            isMask: !0,
                            isError: !0,
                            showMessage: "授权拒绝"
                        },
                        a._timerHandler && l.cancel(a._timerHandler), a._timerHandler2 = c(function () {
                            f()
                        },
                        2e3))
                    } else a._timerHandler && l.cancel(a._timerHandler),
                    f()
                })
            },
            1500)
        }
    }
    function C(r) {
        var s, o = {};
        o.accountNo = r,
        o.accountTransferType = "1",
        o.isShowLogin = "",
        i.getSrandNum(function () {
            s = e.passOperatorMFM ? e.passOperatorMFM.getControlParams() : {},
            o = $.extend(o, s),
            k(o)
        })
    }
    function v() {
        e.verifyImageUrl = i.getUrl("VerifyImage") + "&flag=" + (new Date).getTime()
    }
    function h(e) {
        if (e) {
            var r = e.keyCode || e.which;
            if ("13" != r) return
        }
        w()
    }
    function w() {
        e.isError = !1,
        e.userNameIserror = !1,
        e.verifyImagerequired = !1,
        e.verifyImageIserror = !1,
        e.passwordIserror = !1,
        e.errMsg = "";
        var r = e.userName;
        if (N && (r = M), !r) return void (e.userNameIserror = !0);
        if (!e.passOperator.checkValid()) return void (e.passwordIserror = !0);
        //if (e.isCheckCode) {
        //    var s = /[a-zA-Z0-9]{4}$/;
        //    if (!e.verifyImageCode) return void (e.verifyImagerequired = !0);
        //    if (!s.test(e.verifyImageCode)) return void (e.verifyImageIserror = !0)
        //}
        e.mainForm.checkValid() && (x || (x = !0, $("#loginBtn2").text("登录中...").attr("disabled", "disabled"), i.getSrandNum(k)))
    }
    function k(r) {
        var o, i = {};
        i.accountNo = e.userName,
        i.password = e.passOperator.getValue(),
        e.isCheckCode && (i.checkCode = e.verifyImageCode),
        o = e.passOperator.getControlParams(),
        angular.extend(i, o),
        i.isShowLogin = "",
        i.logonVersion = "2",
        N && (i.accountNo = M, i.isShowLogin = "01"),
        angular.extend(i, r || {}),
        s.post("/bank/HBsignIn?token=" + strToken, i).success(function (r) {
            var o = r.em;
            if (x = !1, "0000" == r.ec) {
                var i = r.cd.session_voucher,
                n = r.cd.certID;
                if ("6" == r.cd.retValue) {
                    if (!("329" != i && "343" != i && "360" != i && "328" != i || n && "nbcb" == n.substring(0, 4))) {
                        var t = r.cd.session_customerId;
                        return void s.post("HBclearSecurity.do", {
                            accNo: t
                        }).success(function () {
                            a.switchToMybank("", {},
                            !0, r.cd.sessionId)
                        })
                    }
                    b(r.cd.sessionId, r.cd)
                } else y(r.cd)
            } else "1211" == r.ec ? e.passOperator.clear() : "1964" == r.ec ? (e.passOperator.clear(), o = "密码已锁，您可通过网银、手机银行或拨打95574重新设置密码。") : "8888" == r.ec ? (e.userName = "", e.passOperator.clear(), e.verifyImageCode = "", o = "您的登录名或密码输入错误，请核对后重新输入。") : "C003" == r.ec || "EBPB5000" == r.ec || "EBPB0131" == r.ec ? (e.userName = "", e.passOperator.clear(), e.verifyImageCode = "", o = "您的登录名或密码输入错误，请核对后重新输入。") : "C004" == r.ec ? (e.userName = "", e.passOperator.clear(), e.verifyImageCode = "", o = "您未设置相应的查询密码， 请拨打宁波银行24小时客服热线设置。") : "EBPB0851" == r.ec && (e.verifyImageCode = "", o = "您输入的验证码不正确"),
            Q(),
            e.isError = !0,
            e.errMsg = o,
            $("#loginBtn2").text("登录").removeAttr("disabled")
        })
    }
    function I() {
        for (var e = {},
        r = window.location.search.substring(1).split("&"), s = r.length, o = 0; s > o; o++) {
            var i = r[o].split("=");
            e[i[0]] = decodeURIComponent(i[1])
        }
        return e
    }
    function b(r, s) {
        e.userName = "",
        e.verifyImageCode = "",
        a.login(r,
        function () {
            a.loginDispatch(_)
        },
        null, {
            pre: R,
            result: s
        })
    }
    function y(r) {
        R(r) && (e.userName = "", e.verifyImageCode = "", a.login(r.sessionId,
        function () {
            a.loginDispatch(_)
        }))
    }
    function R(r, s, o) {
        var i = r.sessionId,
        t = r.session_eAccFlag;
        return "11" == r.customerType ? void n.pop({
            title: "登录跳转提示",
            src: "app/login/logincredit.tpl.html",
            entityClassName: "logincreditPop",
            onClose: function () {
                a.ebankDispatch(_, r.sessionId)
            }
        }) : "3" != t ? void a.ebankDispatch(_, i) : "01" == r.customerType && "1" != r.mobileVerify ? (e.passOperator.clear(), Q(), e.isError = !0, void (e.errMsg = "您尚未开通短信认证，请登录我行网银进行签约。")) : (s && s(o), !0)
    }
    function Q() {
        var r = e.userName;
        e.userName && (N && (r = M), s.post("/bank/HBcheckCodeFlag?token=" + strToken, {
            accountNo: r
        }).success(function (r) {
            if ("0000" === r.ec) {
                var s = r.cd.flag,
                o = r.cd.logonFlag;
                s && "1" == s || o && "1" == o ? (v(), e.isCheckCode = !0) : e.isCheckCode = !1
            }
        }))
    }
    var x = !1,
    N = !1,
    M = null,
    _ = {};
    d(),
    e.jumpRepass = function () {
        o.jump("/EXT00006", {})
    }
}]);;
/*!/src/app/main/contentController.js*/
angular.module("main", ["ngRoute"]).config(["$routeProvider",
function (s) {
    s.when("/main/:paramsid?", {
        template: '<div class="banner1"><div class="banner1_pic" style="display:block;" ng-class="{content_banner:($index == adverIndex),current:($index == adverIndex)}" ng-repeat="advertiseInfo in advertiseList" ng-style="{\'background\':\'url({{advertiseInfo.adverUrl}}) no-repeat center center\',\'z-index\':(50 - $index)}"><a ng-href="{{advertiseInfo.jumpUrl}}" ng-style="{\'cursor\':(advertiseInfo.jumpFlag == \'2\' ? \'default\' : \'pointer\')}" target="{{advertiseInfo.jumpFlag == \'1\' ? \'_blank\' : \'\'}}"><div class="w1200 banner1_info"></div></a></div><div class="w1200 banner_index" ng-if="!isLogin()"><div ng-controller="bannerloginController" class="login bannerlogin"><div class="login_boxinline" style="border-radius: 10px;"><div class="loginNotice" ><div class="loginSwitcherTitle">{{QRCode.switcherTitle}}</div></div><div class = "innerbox" ng-show="!QRCode.isQRShow" style="padding: 15px;"><form name="mainForm" ><ul><li class = "upspace"></li><li class="loginFormInput clearfix"><div class="clearfix"><s class="people"></s><input type="text" tabindex="1" name="userName" class="input-text login-input-text " ng-model="userName" placeholder="账号 /登录名 /身份证 /手机号"    ng-blur="getCheckCodeFlag();" ng-disabled="userNameDisabled" autocapitalize="off" autocomplete="off"/><div class="cross" ng-show="isCrossShow" ng-click="cleanUserName();"></div></div><p ng-class="{loginerror: isCheckCode,loginuser: !isCheckCode}"><em ng-show="userNameIserror">请输入用户名</em></p></li><li class="loginFormInput clearfix"><div class="clearfix"><s  class="peoplelock"></s><div passwordctrl check-ereg="regPwd" max-length="maxLength" class="passwordctrl " pass-operator="passOperator" enter-callback="enterClick();"></div></div><p class="errorMessage" ng-class="{loginerror: isCheckCode,loginuser: !isCheckCode}"><em ng-show="passwordIserror">请正确输入密码</em></p></li><li ng-if="isCheckCode" ><div class = "codeControl"><input class="codeinput input-text login-input-text" tabindex="3" type="text" name="verifyImage" maxlength="4" ng-model="$parent.verifyImageCode"    ng-keydown="enterClick($event);" placeholder="不区分大小写" /><img class="verifyImageCode pointer" title="看不清？点击更换" ng-src="{{verifyImageUrl}}" ng-click="updateVerifyImage();"/></div><p class= "loginerror"><em ng-show="verifyImagerequired">请输入图形验证码</em><em ng-show="verifyImageIserror">请正确输入图形验证码</em></p></li><li><a ng-click="login()" class="btn1 aButton" id="loginBtn2">登&nbsp;&nbsp;录</a><p class="loginerror"><em ng-show="isError" ng-bind="errMsg"></em></p></li><li class="control-group common_show" ><div class="controls clearfix" style="padding-right: 30px;"></div></span></div></li></ul></form><p class=" online-account"></p></div><div ng-init="QRCode.initQRcode();" ng-if="QRCode.isQRShow" class="scanCode"><div class="qrCodeContainer"><div class="qrCodeContent" ng-show="!QRCode.qrloading"><img ng-src="{{QRCode.qrCodeUrl}}" class="qrCodePic"/><div class="qrCodeContentContainer" ng-if="QRCode.mask.isMask"><div class="qrCode-backdrop"></div><div class="qrCodeCoreContent"><p class="qrCode-tooltip-content" ng-class="{\'qrCode-tooltip-icon-ok\': (!QRCode.mask.isError),\'qrCode-tooltip-icon-error\': (QRCode.mask.isError)}"></p><p>{{ (isError && errMsg) ? errMsg : QRCode.mask.showMessage}}</p></div></div></div><br/><br/></div><div class="passwordMFM"><div passwordctrl pass-operator="$parent.passOperatorMFM" class="mypasswordctrl"/></div></div></div><div cert-info on-init="initCertInfo($operator)"></div></div></div></div>',
        controller: "mainController"
    })
}]).controller("mainController", ["$scope", "$log", "$httpPlus", "$routeHelper", "$interval", "$utb", "$message", "$route", "$location", "$globalData",
function (s, n, a, e, i, t, l, o, d, r) {
    function c() {
        var n = e.getUrlParams();
        s.channelId = n.channelId,
        s.setAllMouseHover = R,
        s.getClassName = B,
        p(),
        f(),
        h(),
        j(),
        q()
    }
    function p() {
        //a.getCache("HB00000_bannerList.do", {
        //    queryType: "0"
        //}).then(function (n) {
        //    var a = n.data;
        //    if ("0000" == a.ec) {
        //        for (var e = a.cd,
        //        i = e.iBannerList,
        //        l = 0; l < i.length; l++) i[l].adverUrl = 1 == i[l].flowFlag ? i[l].zimageUrl : t.getUrl("HB00000_bannerImage.do", {
        //            iconId: i[l].iconFlag
        //        }),
        //        i[l].jumpUrl = i[l].bannerUrl,
        //        i[l].jumpFlag = i[l].bannerUrl ? "#" == i[l].bannerUrl.charAt(0) ? "0" : "1" : "2";
        //        s.advertiseList = i,
        //        _()
        //    }
        //}),
        s.advertiseList = [],
        s.selectAdver = v,
        s.adverIndex = 0,
        s.login = g
    }
    function g() {
        t.login(function (s) {
            2 == s && ("/HB00900" == d.path() || 0 == d.path().indexOf("/EXT00006") || 0 == d.path().indexOf("/error") || 0 == d.path().indexOf("/sessionTimeout") ? e.jump("/main") : o.reload())
        })
    }
    function _() {
        U && i.cancel(U),
        U = i(function () {
            s.adverIndex >= s.advertiseList.length - 1 ? s.adverIndex = 0 : s.adverIndex++
        },
        4e3)
    }
    function v(n) {
        s.adverIndex = n,
        _()
    }
    function f() {
        s.announceIndex = 0,
        s.announceLen = z ? z.length : 0,
        s.announcement = z,
        s.isLogin = m,
        u()
    }
    function m() {
        return t.isLogin()
    }
    function u() {
        Q && i.cancel(Q),
        Q = i(function () {
            s.announceIndex >= z.length - 1 ? s.announceIndex = 0 : s.announceIndex++
        },
        5e3)
    }
    function h() {
        var n, e = "HB_recommendedProds.do";
        //m() ? (e = "HB_recommendedProdsSession.do", n = "post") : (e = "HB_recommendedProds.do", n = "getCache2"),
        //a[n](e, {
        //    areaCode: "",
        //    recommendedId: "a"
        //}).success(function (s) {
        //    if ("0000" == s.ec) for (var n = s.cd.iRecommFundPBListInfo,
        //    a = n.length,
        //    e = 0; a > e; e++) {
        //        switch (n[e].fundType) {
        //            case "0":
        //                n[e].fundType = "普通型";
        //                break;
        //            case "1":
        //                n[e].fundType = "股票型基金";
        //                break;
        //            case "2":
        //                n[e].fundType = "债券型基金";
        //                break;
        //            case "3":
        //                n[e].fundType = "混合型基金";
        //                break;
        //            case "4":
        //                n[e].fundType = "货币型基金";
        //                break;
        //            case "5":
        //                n[e].fundType = "QDII型基金";
        //                break;
        //            case "9":
        //                n[e].fundType = "组合产品"
        //        }
        //        if (null != n[e].RANK3Y && "" != n[e].RANK3Y) {
        //            for (var i = n[e].RANK3Y.split("★").length, t = [], l = 0; i - 1 > l; l++) t[l] = l;
        //            n[e].RANK3Y = t,
        //            n[e].havestar = ""
        //        } else n[e].RANK3Y = [],
        //        n[e].havestar = "无评级";
        //        n[e].SYL_6Y < 0 ? (n[e].act = 0 == e ? "big_num jiang" : "big_num sml_num jiang", n[e].pctact = "percent jiang") : (n[e].act = 0 == e ? "big_num" : "big_num sml_num", n[e].SYL_6Y && 0 != n[e].SYL_6Y || (n[e].SYL_6Y = "--"), n[e].pctact = "percent"),
        //        G[1].iDataList[e] = {
        //            name: n[e].fundName,
        //            profit: n[e].SYL_6Y,
        //            jinzhi: n[e].unitNetValue,
        //            level: n[e].RANK3Y,
        //            fundType: n[e].fundType,
        //            fundCode: n[e].fundCode,
        //            act: n[e].act,
        //            pctact: n[e].pctact,
        //            havestar: n[e].havestar
        //        },
        //        G[1].iDataList[e].prodinfoJ = n[e]
        //    }
        //});
        var i = r.getAppContext("branch").value;
        //a[n]("hallbank_loadFinanceNew.do", {
        //    areaCode: i
        //}).success(function (n) {
        //    if ("0000" == n.ec) {
        //        var a = n.cd.isSaleUpForFixed;
        //        s.iFinanceList = n.cd.iNewFinancialProducts,
        //        L(s.iFinanceList, a);
        //        var e = "1" == a.charAt(0) ? !0 : !1,
        //        i = "1" == a.charAt(1) ? !0 : !1,
        //        l = "1" == a.charAt(2) ? !0 : !1,
        //        o = "o";
        //        t.getAdviceInfo(o).then(function (n) {
        //            for (var t = n.iRecommFinancialProducts,
        //            o = 0; o < t.length; o++) "o1" != t[o].recommendedId || e ? "o2" != t[o].recommendedId || i ? "o3" != t[o].recommendedId || l || (s.iFinanceList[2] = t[o]) : s.iFinanceList[1] = t[o] : s.iFinanceList[0] = t[o];
        //            L(s.iFinanceList, a, "1", x(n.iProdUseLimits, "prodId"))
        //        })
        //    }
        //}),
        N(),
        b(),
        s.ZQModule = G,
        s.iFinanceList = [],
        s.selectModule = k,
        s.purcaseFinance = H,
        s.quiklyApply = M,
        s.purcaseFund = P,
        s.onchartInit = y,
        s.purcaseDeposit = C,
        s.purcaseDirect = I,
        T(),
        w()
    }
    function I(s) {
        e.jump("/HB00501_directDetail", {
            productNo: s.dsInvestProdno
        })
    }
    function b() {
        var n = {
            currPage: 1,
            turnPageShowNum: "4",
            dsLowAnnualYield: "",
            dsUpAnnualYield: "",
            dsLowInveCycleLimit: "",
            dsUpInveCycleLimit: "",
            dsLowAmtLoanAllow: "",
            dsUpAmtLoanAllow: "",
            dsProjectName: ""
        };
        //a.getCache2("PB03301_investMarket.do", n).success(function (n) {
        //    if ("0000" == n.ec) for (var a = s.iInvestProdList = n.cd.iInvestProdList,
        //    e = a.length,
        //    i = 0; e > i; i++) a[i].s_dsExpectIncome = 100 * parseFloat(a[i].dsExpectIncome) + "",
        //    a[i].s_dsRemainAmt = parseFloat(a[i].dsRemainAmt) / 1e4 + "",
        //    a[i].s_dsFinanceProgress = 100 * parseFloat(a[i].dsFinanceProgress) + "",
        //    a[i].s_edOver = "A" != a[i].dsProjStatus ? "B" == a[i].dsProjStatus ? 2 : 1 : 0
        //})
    }
    function w() { }
    function x(s, n) {
        var a, e = {};
        for (a = 0; s && a < s.length; a++) e[s[a][n]] = s[a];
        return e
    }
    function C() {
        t.isLogin() ? e.jump("/HB00301_confirm") : t.login(function (s) {
            2 == s && e.jump("/HB00301_confirm")
        })
    }
    function y(s) {
        Y = s
    }
    function T() {
        t.getContextParams("DEP_PERIODTYPE", !0, !0).then(function () {
            a.getCache("HB00301_getDepositRateList.do", {
                areaCode: K
            }).success(function (s) {
                if ("0000" == s.ec) {
                    var n = $(s.cd.iRateList);
                    Y.init({
                        labels: F(Z),
                        datasets: [{
                            fillColor: "#00B4FF",
                            strokeColor: "#00B4FF",
                            data: n["00000"]
                        },
                        {
                            fillColor: "#FFD800",
                            strokeColor: "#FFD800",
                            data: n[K]
                        }]
                    },
                    {
                        scaleLineColor: "#FFFFFF",
                        scaleLineWidth: 0,
                        scaleShowLabels: !1,
                        scaleFontSize: 18,
                        scaleFontColor: "#666",
                        barStrokeWidth: 0,
                        barValueSpacing: 20,
                        barDatasetSpacing: 0,
                        barShowStroke: !0,
                        scaleShowLabels: !1,
                        scaleShowGridLines: !1,
                        multiTooltipTemplate: "<%= value + '%' %>"
                    })
                }
            })
        })
    }
    function F() {
        for (var s = [], n = "", a = 0; a < Z.length; a++) n = t.getContext("DEP_PERIODTYPE", Z[a]),
        n = n.replace("定期", ""),
        s.push(n);
        return s
    }
    function $(s) {
        for (var n, a, e, i = {},
        t = function (s) {
            for (var n = 0; n < Z.length; n++) if (s == Z[n]) return n;
            return -1
        },
        l = 0; l < s.length; l++) n = s[l],
        (a = i[n.branchId]) || (a = i[n.branchId] = []),
        (e = t(n.saveTypeId)) > -1 && (a[e] = n.saveRate);
        return i
    }
    function L(s, n, a, e) {
        for (var i, l = 0; s && l < s.length; l++) if (s[l]) {
            i = s[l];
            var o = i.PfirstAmt,
            d = o,
            c = parseInt(o, 10);
            if (1e4 > c) d = c + "元";
            else {
                var p = c / 1e4,
                g = c % 1e4;
                0 >= g ? d = p + "万" : (c = parseFloat(c) / parseFloat(1e4), d = c + "万")
            }
            var _ = i.TemplateCode,
            v = i.prodId,
            f = i.InterestDays,
            m = (i.IpoStartDate || "").substring(4).replace(/(\d{2})(\d{2})/g, "$1/$2"),
            u = (i.IpoEndDate || "").substring(4).replace(/(\d{2})(\d{2})/g, "$1/$2"),
            h = i.IncomeDate || "",
            I = i.EndDate || "";
            "1102" == _ && null != i.IncomeEndDate && (I = i.IncomeEndDate);
            var b = (h || "").substring(4).replace(/(\d{2})(\d{2})/g, "$1/$2"),
            w = (I || "").substring(4).replace(/(\d{2})(\d{2})/g, "$1/$2"),
            x = "",
            C = 0;
            "1102" == _ ? x = m + "-" + u : "1402" == _ ? (x = "每天", f = i.CycleDays) : "1407" == _ ? (x = "每天", f = "自选") : "1201" == _ || "9901" == _ || "9801" == _ || "9905" == _ ? (x = "--", f = "9801" == _ ? i.CycleDays : "活期", C = 1) : "9101" == _ ? "1001" == v ? (x = "每个工作日", f = "--") : (x = b + "至" + w, f = "--") : x = b + "至" + w,
            i.s_prodName = i.prodName,
            i.s_modelComment = (i.ModelComment || "").replace(/\%/g, ""),
            i.s_firstAmt = d,
            i.s_ipo = x,
            i.s_InterestDays = f,
            i.s_showFlag = C,
            i.s_danweiFlag = 0,
            ("9901" == _ || "9801" == _ || "9905" == _) && (i.s_danweiFlag = 1, i.s_modelComment = i.Nav),
            "0" == n.charAt(l) && "1" == a ? (i.s_lastEd = parseInt(e[i.prodId].totUseLimit, 10) / 1e4, i.s_percent = 100 * (1 - parseFloat(e[i.prodId].totUseLimit) / parseFloat(e[i.prodId].totLimit)) + "%") : (i.s_lastEd = parseInt(i.surplusUseLimit, 10) / 1e4, i.s_percent = 100 * (1 - parseFloat(i.surplusUseLimit) / parseFloat(i.totLimit)) + "%"),
            i.s_edOver = 0 == i.s_lastEd;
            var y = i.prodName,
            T = 0;
            if (-1 != y.indexOf("新客户专属") && 0 == l) {
                T = 1,
                i.s_new = T;
                var F = i.s_lastEd + "",
                $ = F.replace(/[\D.]/g, "").length;
                2 >= $ ? ($ = "1", i.numCnt = $) : $ > 2 && 4 >= $ ? ($ = "2", i.numCnt = $) : ($ = "3", i.numCnt = $)
            } else i.s_new = T;
            var L = i.IpoStartDate,
            A = i.OpenTime,
            $ = A.replace(/[\D]/g, "").length;
            5 == $ && (A = "0" + A);
            var k = A.substring(0, 2) + ":",
            H = A.substring(2, 4) + ":",
            M = A.substring(4, 6);
            if (A = k + H + M, "1102" == _) {
                var N = r.getSystemTime().getTime(),
                P = t.parseDate(L.substring(0, 8) + " " + A, "yyyyMMdd hh:mm:ss").getTime(),
                j = (P - N) / 1e3;
                i.leftTime = j
            }
            i.dateOne = (new Date).getTime(),
            D(i)
        }
    }
    function D(s) {
        U && i.cancel(U),
        A(s),
        O = i(function () {
            var n = (new Date).getTime(),
            a = (n - s.dateOne) / 1e3;
            A(s, a)
        },
        1e3)
    }
    function A(s, n) {
        var a = s.leftTime - n;
        if (!(120 >= a && a > 0)) return void (s.timeTwo = !1);
        s.timeTwo = !0,
        s.minutes = Math.floor(a / 60);
        var e = Math.floor(a % 60);
        s.seconds = 10 > e ? "0" + e : e
    }
    function k(n) {
        s.zqActive = n
    }
    function H(n) {
        return "1700" == n.TemplateCode ? void l.alert("暂不支持时时鑫类型产品") : "01" != n.currencyType ? void l.alert("网厅2.0暂不支持外币理财购买") : "NZ" != s.channelId && n.prodName.indexOf("直销专属") > -1 ? void l.alert("暂不支持直销专属类型产品") : void e.jump("/HB00101_buyConfirm", {
            prodId: n.prodId
        })
    }
    function M(s) {
        "0" == s ? e.jump("/HB00201_taxCreditDesc") : l.alert("此功能暂不开放")
    }
    function N() { }
    function P(s) {
        e.jump("/HB01104_Des", s.prodinfoJ)
    }
    function j() {
        s.queryDetailInfo = S;
        var n = "b";
        t.getAdviceInfo(n).then(function (n) {
            for (var a = n.iRecommGoldenInfoNew,
            e = [], i = 0; i < a.length; i++) {
                var l = a[i].goldPic,
                o = E(l, "GLP")[0] || {};
                a[i].imgUrl = 1 == a[i].flowFlag ? o.picUrl : t.getUrl("HB00000_bannerImage.do", {
                    iconId: o.picUrl
                }),
                a[i].proname = a[i].productName,
                a[i].proprice = a[i].singlePrice,
                a[i].canSellNum = t.nvl(a[i].sellNum, "99999"),
                "b1" == a[i].recommendedId ? e[0] = a[i] : "b2" == a[i].recommendedId ? e[1] = a[i] : "b3" == a[i].recommendedId && (e[2] = a[i])
            }
            s.HQModule = e
        })
    }
    function q() {
        var n = [];
        //n[0] = {
        //    name: "税务贷",
        //    imgUrl: "assets/nbcbEdit/otherMoney/taxCredit123.png"
        //},
        //n[1] = {
        //    name: "信用卡申请",
        //    imgUrl: "assets/nbcbEdit/otherMoney/bannerCredit.png"
        //},
        s.JQModule = n
    }
    function R(n, a, e) {
        s[n + "_" + a] = e
    }
    function B(n, a) {
        return !!s[n + "_" + a]
    }
    function S() { }
    function E(s, n) {
        if (!s) return [];
        for (var a = s.split("|"), e = [], i = 0; i < a.length; i++) {
            var t = a[i],
            l = {};
            if (t) {
                var o = t.split("@")[0],
                d = t.split("@")[1],
                r = t.split("@")[2];
                n == r && (l.picUrl = o, l.picSeq = d, l.picType = r, e.push(l))
            }
        }
        return e
    }
    var U, Q, O, Y, z = [{
        content: '哈喽！<span class="notice_time">就在刚刚</span><span class="notice_add">来自宁波的</span><span class="notice_peo">翁&nbsp;**</span>  买了咱们的理财 -- <span class="notice_pro">天利鑫20000号</span><span class="notice_price">100,000元</span>'
    }],
    G = [{
        title: "理财产品",
        allTitle: "更多理财产品",
        allUrl: "/HB00101",
        iDataList: [{
            name: "惠添利2299号",
            profit: "6.4",
            startAmount: "50000",
            raiseDate: "07/13-10/16",
            raisePeriod: "74",
            remainAmount: "500004",
            percent: "60%"
        }]
    }],
    K = r.getAppContext("branch").key;
    c();
    var Z = ["003201", "006201", "012201", "024201", "036201", "060201"];
    s.orderGoldConfirm = function (s) {
        var n = {
            goldInfo: s,
            goldId: s.goldId
        };
        e.jump("/HB00601_goldDetail", n)
    }
}]);;
/*!/src/app/main/developeController.js*/
angular.module("develope", ["ngRoute"]).config(["$routeProvider",
function (e) {
    e.when("/develope", {
        template: '<div style="width:100%;height:200px;font-size:40px;text-align:center;padding-top: 100px;"><span>更多功能，正在开发中......</span></div>',
        controller: "developeController"
    })
}]).controller("developeController", ["$scope", "$log",
function () { }]);;
/*!/src/app/main/headController.js*/
angular.module("head", []).controller("headController", ["$scope", "$log", "$httpPlus", "$routeHelper", "$utb", "$loginFlow", "$message", "$route", "$globalData", "$pinyin", "$location", "$routeParams", "$iss",
function (n, e, i, a, t, u, o, r, s, c, l, m, h) {
    function f() {
        M(),
        n.activeBranch = "宁波",
        s.setAppContext("branch", {
            key: "t0000",
            value: n.activeBranch
        }),
        n.isActive = !1,
        t.getContextParams("HALLBANK_SPECIAL_REGION", !0, !0).then(function (e) {
            var i = [];
            n.branchList = e.HALLBANK_SPECIAL_REGION;
            for (var a = 0; a < n.branchList.length; a++) {
                var t = angular.extend({},
                n.branchList[a]);
                t.pinyin = c.getFullChars(t.value),
                i.push(t)
            }
            n.branchList = i,
            L()
        }),
        //i.post("getUserLocationInfo.do").success(function (n) {
        //    if ("0000" == n.ec) {
        //        var e = JSON.parse(n.cd.locationInfo);
        //        0 == e.code && e.data.city && (w = e.data.city, L())
        //    }
        //}),
        n.selectBranch = H,
        n.selectMenu = g,
        n.setMouseHover = v,
        n.mouseHoverIndex = -1,
        n.isMouseHover = !1,
        n.jumpMenu = d,
        //n.userImg = "/assets/app/userCenter/userPhoto_5db5fa7.png",
        n.logoClick = p,
        n.jumpPage = A
    }
    function p() {
        A("/main")
    }
    function v(e, i) {
        i ? (n.isMouseHover = !0, n.mouseHoverIndex = e) : (n.isMouseHover = !1, n.mouseHoverIndex = -1)
    }
    function g(e, i) {
        n.activeIndex = e,
        n.iSubMenuList = i ? i.iMenuList : null,
        n.subActiveIndex = 0
    }
    function d(n, e) {
        n && A(n, e)
    }
    function H(e) {
        n.activeBranch = e.value,
        B(e)
    }
    function L() {
        var e = n.branchList,
        i = n.activeBranch;
        if (P++, 2 == P) for (var a = 0; a < e.length; a++) if (w.indexOf(e[a].value) > -1) {
            n.activeBranch = e[a].value,
            i != e[a].value && B(e[a]);
            break
        }
    }
    function B(n) {
        s.setAppContext("branch", n),
        r.reload()
    }
    function M() {
        //n.isLogin = C,
        //n.isShowLogin = $,
        //n.quit = x,
        //n.login = b,
        //n.iMenuList = k,
        //u.on("login.post",
        //function () {
        //    n.iMenuList = O
        //}).on("quit.post",
        //function () {
        //    n.iMenuList = k
        //}),
        //n.switchToFive = I,
        //n.isShowSwitch = _
    }
    function b() {
        h.addEvent("login", "login", "login"),
        t.login(function (n) {
            2 == n && ("/HB00900" == l.path() || 0 == l.path().indexOf("/EXT00006") || 0 == l.path().indexOf("/error") || 0 == l.path().indexOf("/sessionTimeout") ? a.jump("/main") : r.reload())
        },
        {
            loginFlag: "0"
        })
    }
    function x() {
        u.quit(),
        A("/main")
    }
    function C() {
        return t.isLogin()
    }
    function $() {
        return "/marginDesc" == l.path() ? !0 : C()
    }
    function I() {
        u.switchToMybank("", {})
    }
    function _() {
        return C() && "/perbank" == l.path() ? !0 : !1
    }
    function A(n, e) {
        e = e || {},
        e._source = "TJ001",
        a.jump(n, e)
    }
    var w, k = [{
        name: "我的",
        url: "/perbank"
    },
    {
        name: "首页",
        url: "/main"
    }],
    O = [{
        name: "我的",
        url: "/perbank"
    },
    {
        name: "服务",
        url: "/appoint"
    },
    {
        name: "花钱",
        iMenuList: [{
            name: "黄金",
            url: "/HB00600"
        },
        {
            name: "购物车",
            url: "/HB00601_goldCar"
        }]
    },
    {
        name: "首页",
        url: "/main"
    }],
    P = 0;
    f()
}]);;
/*!/src/app/main/mainModule.js*/
angular.module("mainModule", ["head", "main", "nbcb", "develope", "bannerLogin", "tips"]);;
/*!/src/app/main/nbcbController.js*/
angular.module("nbcb", ["ngRoute"]).controller("nbcbController", ["$scope", "$log", "$httpPlus", "$routeHelper", "$loginFlow", "$utb", "$globalData", "$appConfig", "$message", "$rootScope", "$routeParams",
function (n, o, e, t, a, i, r, l, d, c) {
    function m() {
        p(),
        w(),
        n.addBookMark = f,
        c.recordOperationTime = u;
        try {
            document.domain = l.domain
        } catch (e) {
            o.debug(e)
        }
        var a = t.getUrlParams();
        "NZ" == a.channelId && (l.channel = "ZD")
    }
    function u() {
        r.setOperationTime((new Date).getTime())
    }
    function p() {
        var n, o = window.name || "",
        e = "",
        t = "";
        if (window.name = null, n = o.match(/^NBCB@([0-9a-zA-Z]+)$/), e = o.match(/^NBCB@(.+)@(.+)$/), n) t = n[1],
        a.login(t);
        else if (e) {
            var i = e[1],
            l = {},
            d = e[2];
            d = d.replace(/([=&])/g, "@").split("@");
            for (var c = 0; c < d.length; c += 2) l[decodeURIComponent(d[c])] = decodeURIComponent(d[c + 1]);
            r.setAppContext(i, l)
        }
        s()
    }
    function s() {
        var n = {},
        o = window.location.href.split("?")[1] || "";
        o = o.split("/")[0] || "";
        for (var e = o.split("&"), t = e.length, a = 0; t > a; a++) {
            var i = e[a].split("=");
            n[i[0]] = decodeURIComponent(i[1])
        }
        r.setAppContext("initNBCBRootParams", n)
    }
    function w() {
        window.onbeforeunload = function () { },
        window.onunload = function () {
            i.isLogin() && a.quit(!0)
        }
    }
    function f() {
        var n = "宁波银行个人网上银行",
        o = "https://e.nbcb.com.cn/",
        e = -1 != navigator.userAgent.toLowerCase().indexOf("mac") ? "Command/Cmd" : "CTRL";
        document.all ? window.external.addFavorite(o, n) : window.sidebar ? window.sidebar.addPanel(n, o, "") : d.alert("您可以尝试通过快捷键 " + e + " + D 加入到收藏夹")
    }
    m()
}]);;
/*!/src/app/main/tipsController.js*/
angular.module("tips", []).controller("tipsController", ["$scope", "$appConfig", "$loginFlow",
function (n, o, i) {
    function t() {
        n.showTip = !0,
        n.hasTips = !0,
        i.on("login.post",
        function () {
            n.hasTips = !1
        }).on("quit.post",
        function () {
            n.hasTips = !0
        })
    }
    t(),
    n.hideTip = function (o) {
        n.showTip = o
    },
    n.doTips = function (n) {
        1 == n && window.open("http://www.nbcb.com.cn/electronic_banking/downloadCenter/"),
        2 == n && window.open(o.mybankUrl + "?start=000002"),
        3 == n && window.open(o.mybankUrl + "?start=000001")
    }
}]);