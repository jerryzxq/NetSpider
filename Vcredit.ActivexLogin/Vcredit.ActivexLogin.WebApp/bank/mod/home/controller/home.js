define("text!mod/home/template/home.html", [], function () {
    return '<body onkeydown="enterLogin()">\r\n<div class="login-bar">\r\n  <div class="w930 ma c">\r\n    <div class="left">\r\n    	<div id="adPaly"></div>\r\n      	<!--<iframe src="https://www.pingan.com/adms/area.ctrl?AREAID=QY15102119454421" style="width:440px;height:380px;border:0px" scrolling="no" frameborder="no"></iframe>-->\r\n    	<!--<iframe src="../../mod/home/common/ad/index.html" style="width:440px;height:380px;border:0px" scrolling="no" frameborder="no"></iframe>-->  \r\n    </div>\r\n    <div class="right fr">\r\n      <h2>个人网银登录</h2>\r\n      <div class="login-box m_t5">\r\n        <ul class="login-ul">\r\n          <li class="c">\r\n            <div class="fl">\r\n              <div class="c">\r\n                <input class="input2 fl " id="userName" type="text" placeholder="一账通用户名/身份证号" tabindex="1">\r\n                <a class="linktwo m_l20 fl" id="forgetUsername" otype="button" otitle="登录页-忘记用户名" target="new"  href="https://www.pingan.com.cn/pinganone/pa/retrieveVerify.do?isPS=">忘记用户名？</a>\r\n              </div>\r\n              <div class="error_info2 vh" id="userNameInsure">\r\n                <i class="icon49"></i>\r\n                <span id="userNameError">请您输入一账通用户名/身份证</span>\r\n              </div>\r\n            </div>\r\n          </li>\r\n          <li class="c">\r\n            <div class="c">\r\n              <div class="por fl bor_2" id="passWord"></div>\r\n              <!--<a class="linktwo m_l20" id="forgetPassword" otype="button" otitle="登录页-忘记密码" target="new"  href="https://www.pingan.com.cn/pinganone/pa/paResetIndex.screen ">忘记密码？</a>-->\r\n            	<a class="linktwo m_l20" id="forgetPassword" otype="button" otitle="登录页-忘记密码" target="new"  href="#findPwd/findPassword/index">忘记密码？</a>\r\n            </div>\r\n            <div class="error_info2 vh" id="passWordInsure"><i class="icon49"></i>\r\n              <span id="pwdError"></span>\r\n            </div>\r\n          </li>\r\n          <li class="c" id="insureVerifyCode" style="display:none">\r\n            <div class="c">\r\n              <input class="input14 fl " id="verifyCode" type="text" maxlength="4" placeholder="验证码">\r\n              <div class="loading_img check_box dsib m_l20 fl" id="vCodeImgParent"></div>\r\n              <i class="check-code m_l20 fl"></i> \r\n              <a class="linktwo m_l10"  id="changeCode_Btn">换一张</a>\r\n            </div>\r\n          <div class="error_info2 vh" id="codeVerify">\r\n            <i class="icon49"></i>\r\n            <span id="verifyError"></span>\r\n          </div>\r\n          </li> \r\n          <li class="c">\r\n            <a class="btn-login fl" id="login_btn" otype="button" otitle="登录页-登录">登录</a>\r\n            <a class="btn5 m_l20" id="register" otype="button" otitle="登录页-注册" target="new" href="https://bank.pingan.com.cn/ibp/portal/register/bankNewRegisterStepOne.do">注册</a>\r\n          </li>\r\n          <li class="c" id="finalInsure" style="visibility:hidden">\r\n            <div class="error_info2 m_t5 por" id="errorInfo">\r\n              <i class="icon49 poa"></i>\r\n              <span class="lh18 m_l25  m_r22 fl" id="errorLoginMsg"></span>\r\n            </div>\r\n          </li>\r\n          <li class="c">\r\n            <div class="c5">\r\n							请持UKey安全工具用户，安装<a class="c8" href="http://bank.pingan.com/xiazaizhongxin.shtml" target="_blank">UKey管理工具</a>，建议使用<a class="c8" href="http://download.pingan.com.cn/bank/dengluzhushou0724.ZIP">网银助手</a>。\r\n            	<div class="newUserTip">温馨提示：因平安银行登录体系升级，通过“平安口袋银行” APP  V3.0.0及以上版本注册的用户，登录账号暂不支持登录个人网银（V3.0.0以下版本的注册用户不受影响），如需使用个人网银，请先点击上方的“注册”按钮进行注册，给您带来的不便，深表歉意！</div>\r\n            </div>\r\n          </li>\r\n        </ul>\r\n      </div>\r\n    </div>\r\n  </div>\r\n</div>\r\n<div class="w930 ma c">\r\n  <p class="tac c5 bor_t_s h54">\r\n    <!-- <span>UKey控件版本：20150212</span> -->\r\n    <%if(getLocalObjVersion()){%>\r\n    <span class="m_l15">密码控件版本：<%=getLocalObjVersion()%></span>\r\n    <%}%>\r\n    <span class="m_l15">登录设备：<%=utils.sysBrowserCheck.detectOS()%></span>\r\n    \r\n    <span class="m_l15">登录浏览器：<%=utils.sysBrowserCheck.detectBrowser()%></span>\r\n  </p>\r\n</div>\r\n</body>';
}), define("mod/home/model/home", ["require", "exports", "module"], function (e, t, n) {
    n.exports = Model.extend({}, {
        login: function (e) {
            App.sendRequest("login", {
                data: e.param, success: function (t) {
                    App.storage.get("logoutFlag") && App.storage.remove("logoutFlag"), t.responseBody.sensitiveInfoStatus == "0" ? App.secrityFlag = !1 : App.secrityFlag = !0, e.success && e.success(t);
                }, error: function (t) {
                    e.error && e.error(t);
                }
            });
        }, noLoginAgain: function (e) {
            App.commonView.getConfigValues(), App.sendRequest("noLoginAgain", {
                silent: !0, data: e.param, success: function (t) {
                    t.responseBody.sensitiveInfoStatus == "0" ? App.secrityFlag = !1 : App.secrityFlag = !0, e.success && e.success(t);
                }, error: function (t) {
                    e.error && e.error(t);
                }
            });
        }, ssoLogin: function (e) {
            App.commonView.getConfigValues(), App.sendRequest("ssoLogin", {
                silent: !0, data: e.param, success: function (t) {
                    loginModel = new Object, loginModel.bankType = t.responseBody.bankType, loginModel.username = t.responseBody.username, loginModel.preferredTool = t.responseBody.preferredTool, loginModel.openTools = t.responseBody.openTools, loginModel.certVerifyCode = t.responseBody.certVerifyCode, loginModel.partyNo = t.responseBody.partyNo, loginModel.loginName = t.responseBody.loginName, App.storage.set("loginModel", loginModel), t.responseBody.sensitiveInfoStatus == "0" ? App.secrityFlag = !1 : App.secrityFlag = !0, e.success && e.success(t);
                }, error: function (t) {
                    e.error && e.error(t);
                }
            });
        }, showVerifyCode: function (e) {
            App.sendRequest("showVerifyCode", {
                data: e.param, success: function (t) {
                    e.success && e.success(t);
                }, error: function (t) {
                    e.error && e.error(t);
                }
            });
        }, verifyCert: function (e) {
            App.sendRequest("verifyCert", {
                data: e.param, success: function (t) {
                    e.success && e.success(t);
                }, error: function (t) {
                    e.error && e.error(t);
                }
            });
        }, initDataFromNewLogin: function (e) {
            $.ajax({
                url: config.urls.get("initLoginData"), data: { cacheFlag: 0, channelId: "netbank-pc", resource: "PC" }, type: "post", dataType: "json", cache: !1, success: function (t) {
                    t.responseCode == "000000" ? (t.responseBody.sensitiveInfoStatus == "0" ? App.secrityFlag = !1 : App.secrityFlag = !0, e.success && e.success(t)) : e.error && e.error(t);
                }, error: function (t) {
                    e.error && e.error(t);
                }
            });
        }
    });
}), define("text!mod/home/template/loginOtp.html", [], function () {
    return '\r\n\r\n<div class="open-box" id="openBox">\r\n  <div class="input_list  open-box-demo">\r\n    <p class="c4 f2 open-tip" style="background: #fff7d4;padding-left: 5px">\r\n为了保障您的账户资金安全，请先验证手机动态密码。<a class="linktwo f1" id="changToUkey" style="display:none">使用数字证书登录&gt;</a></p>\r\n    <ul>\r\n      <li class="c m_t29 m_b14">\r\n        <p><span class="star">*</span>动态密码：</p>\r\n        <div class="fl" id="login_otp_container">\r\n          \r\n        </div>\r\n        \r\n      </li>\r\n      <li class="c">\r\n        <p></p>\r\n        <div class="fl"><a class="btn-login" id="loginOtp_submit">确 定</a></div>\r\n      </li>\r\n    </ul>\r\n  </div>\r\n  <div class="bor_t_s open-box-b bgc6">\r\n      <p class="c3">温馨提示：</p>\r\n      <ul>\r\n        <li>更换接收网银动态密码的手机号码，请前往<a class="linktwo f1" target="new" href="http://bank.pingan.com/geren/fuwuwangdian/map.shtml">平安银行网点</a>办理。</li>\r\n        <li style="display:none">可以进入“个人中心-XXX”取消登录安全验证。</li>\r\n      </ul>\r\n  </div>\r\n</div>\r\n\r\n\r\n';
}), define("text!mod/home/template/loginCert.html", [], function () {
    return '<body>\r\n<div class="open-box">\r\n  <div class="input_list  open-box-demo">\r\n    <p class="c4 f2 open-tip">为了保障您的账户资金安全，请先验证数字证书。<a class="linktwo f1" id="changeToOtp"  style="display:none">使用手机动态密码登录&gt;</a></p>\r\n    <ul>\r\n      <li class="c m_t29">\r\n        <p>数字证书：</p>\r\n         <div class="fl" id="certField">\r\n         <!-- <div class="part_text c">请插入您的USBKey，点击“下一步”进行验证 </div>\r\n         <div class=" c5">确保已安装最新<a href="http://bank.pingan.com/download/driver.shtml" target="_blank" class="c3">USBKey驱动 </a>，<br />如有出现异常，请查看<a href="http://download.pingan.com.cn/driver/sdbcontrol.doc" target="_blank" class="c3">常见问题解答</a>。</div>\r\n         <div id="errorMsg" class="error_info2" style="display:none"><i class="icon49"></i><span></span></div> -->\r\n         </div>\r\n      </li>\r\n      <li class="c">\r\n        <p></p>\r\n        <div class="fl"><a id="checkCert" class="btn-login">下一步</a></div>\r\n      </li>\r\n    </ul>\r\n  </div>\r\n  <div class="bor_t_s open-box-b bgc6">\r\n      <p class="c3">温馨提示：</p>\r\n      <ul>\r\n        <li>更换接收网银动态密码的手机号码，请前往<a class="linktwo f1" target="new" href="https://bank.pingan.com/geren/fuwuwangdian/map.shtml">平安银行网点</a>办理。</li>\r\n        <li style="display:none">可以进入“个人中心-XXX”取消登录安全验证。</li>\r\n      </ul>\r\n  </div>\r\n</div>\r\n</body>\r\n\r\n';
}), define("mod/home/view/loginCert", ["require", "exports", "module", "text!../template/loginCert.html", "../model/home", "./loginOtp"], function (e, t, n) {
    var r = e("text!../template/loginCert.html"), i = e("../model/home");
    n.exports = App.commonView.Dialog.extend({
        template: _.template(r), appearance: {
            title: "安全验证", open: function (e, t) {
                $(".ui-dialog-titlebar-close").hide();
            }
        }, events: { "click #checkCert": "checkCert", "click #changeToOtp": "_changeToOtp" }, initialize: function (e) {
            this.controller = e.controller, this.loginResponse = e.loginResponse;
        }, onRender: function () {
            var e = this;
            utils.webTrends.businessStepTalkingData({ paramData: "个人网银登录", businessStep: "登录-安全验证" });
            var t = this.loginResponse.openTools;
            t == "4" && this.$("#changeToOtp").show(), this.cert = App.commonView.creatOtpOrCertControll({ toolType: "2" }), this.$("#certField").html(this.cert.el);
        }, checkCert: function () {
            var e = this, t = { certVcode: this.options.vCode, partyNo: this.options.partyNo, loginName: this.options.loginName }, n = this.cert.checkCertResult(t);
            if (n.result) {
                var r = { signData: n.sign, busiData: n.data, usbkeySerial: n.serialNo };
                i.verifyCert({
                    param: r, success: function () {
                        e._successCallback();
                    }, error: function (e) {
                        utils.dialog.toast(e.errMsg || e.ret_msg);
                    }
                });
            }
        }, _successCallback: function () {
            this.close(), this.controller.indexRequest({ loginResponse: this.loginResponse });
        }, _errorCallBack: function () {
            alert("error");
        }, _changeToOtp: function () {
            var t = e("./loginOtp"), n = this, r = new t({ controller: n.controller, loginResponse: n.loginResponse });
            r.render(), this.close();
        }
    });
}), define("mod/home/view/loginOtp", ["require", "exports", "module", "text!../template/loginOtp.html", "component/otp/otpView", "./home", "../controller/home", "./loginCert"], function (e, t, n) {
    var r = e("text!../template/loginOtp.html"), i = e("component/otp/otpView"), s = e("./home"), o = e("../controller/home");
    n.exports = App.commonView.Dialog.extend({
        template: _.template(r), appearance: {
            title: "安全验证", open: function (e, t) {
                $(".ui-dialog-titlebar-close").hide();
            }
        }, events: { "click #loginOtp_submit": "submitOtp", successCallback: "_successCallback", errorCallback: "_errorCallback", "click #changToUkey": "_changeToUkey" }, initialize: function (e) {
            this.controller = e.controller, this.loginResponse = e.loginResponse;
        }, onRender: function () {
            utils.webTrends.businessStepTalkingData({ paramData: "个人网银登录", businessStep: "登录-安全验证" }), this.otp = new i({ autoSendOtp: !0, otpType: "67" }), this.$("#login_otp_container").html(this.otp.$el);
            var e = this.loginResponse.openTools;
            e == "4" && this.$("#changToUkey").show(), _.delay(_.bind(this.triggerEnter, this), 100);
        }, triggerEnter: function () {
            var e = this;
            this.$("#otp_password").focus(), this.$("#otp_password").on("keydown", function (t) {
                t.keyCode === 13 && e.submitOtp();
            });
        }, submitOtp: function () {
            this.otp.showErrorMsg() ? this.otp.submit() : this.otp.showErrorMsg();
        }, _successCallback: function (e) {
            this.close(), this.controller.indexRequest({ loginResponse: this.loginResponse });
        }, _errorCallback: function (e, t) {
            this.otp.showErrorMsg(t.errMsg);
        }, _changeToUkey: function () {
            var t = e("./loginCert"), n = this, r = new t({ type: "login", vCode: n.loginResponse.certVerifyCode, partyNo: n.loginResponse.partyNo, loginName: n.loginResponse.loginName, controller: n.controller, loginResponse: n.loginResponse });
            r.render(), n.close();
        }
    });
}), define("text!component/ad/ad.html", [], function () {
    return '<!--<script src="http://www.pingan.com/adng/admsTool.js"></script>-->\n<div class="pa_banner">\n	<div id="ty_picScroll">\n		<div id="ty_tabBtns" class="ty_tabBtns_1">\n			<p>\n				\n			</p>\n		</div>\n		<div id="ty_tabInfo" class="ty_tabInfo">\n			<ul id="ty_content" class="ty_content">\n				\n			</ul>\n		</div>\n	</div>\n</div>\n\n';
}), define("component/ad/ad", ["require", "exports", "module", "text!./ad.html"], function (e, t, n) {
    var r = e("text!./ad.html"), i = n.exports = ItemView.extend({
        template: _.template(r), jsonRowDate: {
            channelId: "", areaId: "", areaName: "", areaWidth: "", areaHeight: "", areaDesc: "", advertId: "", adName: "", adLink: "", adUrl: "", materialDesc: "", materialId: "", clientgroupId: "", init: function (e, t) {
                var n = this, r = e.data[t];
                n.channelId = r.channelId, n.areaId = r.areaId, n.areaName = r.areaName, n.areaWidth = r.areaWidth + "px", n.areaHeight = r.areaHeight + "px", n.areaDesc = r.areaDesc, n.advertId = r.advertId, n.adName = r.adName, n.adLink = r.adLink, n.adUrl = r.adUrl, n.materialDesc = r.materialDesc, n.materialId = r.materialId, n.clientgroupId = r.clientgroupId;
            }
        }, initialize: function (e) {
            var t = this;
            e.dom.html(t.template()), t.getAdJson(e);
        }, render: function (e) {
            var t = this;
            e.dom.html(t.template()), t.getAdJson(e);
        }, getAdJson: function (e) {
            function n(n) {
                function r(e) {
                    t.jsonRowDate.init(n, e);
                    var r = '<li class="ty_pic" id="' + t.jsonRowDate.areaId + '"><a  href="' + t.jsonRowDate.adLink + '" target="_blank" >' + '<img src="' + t.jsonRowDate.adUrl + '" width="' + t.jsonRowDate.areaWidth + '" height="' + t.jsonRowDate.areaHeight + '" adDesc="' + t.jsonRowDate.materialDesc + '" adLink="' + t.jsonRowDate.adLink + '" areaid="' + t.jsonRowDate.areaId + '" adName="' + t.jsonRowDate.adName + '" material_id="' + t.jsonRowDate.materialId + '" advertid="' + t.jsonRowDate.advertId + '" clientGroupId="' + t.jsonRowDate.clientgroupId + '"/></a></li>';
                    return r;
                }
                var i = 0, s = n.data.length;
                for (; i < s; i++)
                    t.jsonRowDate.init(n, i), t.jsonRowDate.areaId && $("#ty_content").append(r(i));
                t.setAdStyle(e);
            }
            var t = this;
            App.sendRequest("ad", {
                domain: "ad", type: "GET", dataType: "jsonp", cache: !1, data: { areaId: e.adAry.toString() }, success: function (t) {
                    var r = t.data.length;
                    n(t);
                    var i = new s({ box: "#ty_picScroll", dots: "#ty_tabBtns", inner: "#ty_content", msg: !1, auto: r <= 1 ? !1 : !0, speed: 4e3, width: e.styles.width, height: e.styles.height, dotsType: e.dotsType });
                }, error: function (e) {
                }
            });
        }, setAdStyle: function (e) {
            var t = e.styles, n = e.adAry.length, r = n * 18;
            $(".pa_banner").css({ width: t.width, height: t.height }), $("#ty_tabInfo").css({ height: t.height }), $(".ty_pic").css({ width: t.width, height: t.height, left: t.width }).eq(0).css({ left: "0px" }), e.dotsType == "1" ? ($("#ty_tabBtns").removeClass("ty_tabBtns_2"), $("#ty_tabBtns").addClass("ty_tabBtns_1"), $("#ty_tabBtns").css({ width: r + "px", marginLeft: -(r / 2) + "px", display: n <= 1 ? "none" : "block" })) : ($("#ty_tabBtns").removeClass("ty_tabBtns_1"), $("#ty_tabBtns").addClass("ty_tabBtns_2"));
        }
    }), s = function (e) {
        var t = this, n = {}, r = "", i = !0;
        t.init = function () {
            n = { box: $(e.box), inner: $(e.inner), dots: $(e.dots), msg: e.msg == undefined ? !0 : e.msg, auto: e.auto == undefined ? !0 : e.auto, speed: e.speed || 5e3, page: 0, width: e.width, height: e.height, dotsType: e.dotsType }, this.pages = n.inner.find("li").length, this.createDots(), n.auto && (clean = setInterval(this.auto, n.speed)), n.msg || n.box.find(".ty_picMsg").hide();
        }, t.createDots = function () {
            var e = "";
            for (var r = 0; r < this.pages; r++)
                r === 0 ? e = '<span class="ty_current pointers spanStyle_' + n.dotsType + '" data-index=' + r + ">●</span>" : e += '<span class="pointers spanStyle_' + n.dotsType + '" data-index=' + r + ">●</span>";
            n.dots.html(e), t.clickPointer();
        }, t.clickPointer = function () {
            n.box.on("click", ".pointers", function () {
                if (!i)
                    return;
                var e = n.dots.find("span").index(this);
                n.page != e && (clearInterval(clean), t.picScroll(e), clean = setInterval(t.auto, n.speed));
            });
        }, t.picScroll = function (e) {
            var t = n.dots.find("span"), r = n.inner.find(".ty_pic");
            i = !1, r.stop(!1, !0), t.removeClass("ty_current").eq(e).addClass("ty_current"), r.eq(e).animate({ left: "0px" }, 800), r.eq(n.page).animate({ left: "-" + n.width }, 800, function () {
                r.eq(n.page).css({ left: n.width }), n.page = e, i = !0;
            });
        }, t.auto = function () {
            var e = n.page + 1;
            e > t.pages - 1 && (e = 0), t.picScroll(e);
        }, t.init();
    };
});
var psaJSEncExportsYwaq = {};
(function (e) {
    function t(e, t, n) {
        null != e && ("number" == typeof e ? this.fromNumber(e, t, n) : null == t && "string" != typeof e ? this.fromString(e, 256) : this.fromString(e, t));
    }
    function n() {
        return new t(null);
    }
    function r(e, t, n, r, i, s) {
        for (; 0 <= --s;) {
            var o = t * this[e++] + n[r] + i;
            i = Math.floor(o / 67108864), n[r++] = o & 67108863;
        }
        return i;
    }
    function i(e, t, n, r, i, s) {
        var o = t & 32767;
        for (t >>= 15; 0 <= --s;) {
            var u = this[e] & 32767, a = this[e++] >> 15, f = t * u + a * o, u = o * u + ((f & 32767) << 15) + n[r] + (i & 1073741823);
            i = (u >>> 30) + (f >>> 15) + t * a + (i >>> 30), n[r++] = u & 1073741823;
        }
        return i;
    }
    function s(e, t, n, r, i, s) {
        var o = t & 16383;
        for (t >>= 14; 0 <= --s;) {
            var u = this[e] & 16383, a = this[e++] >> 14, f = t * u + a * o, u = o * u + ((f & 16383) << 14) + n[r] + i;
            i = (u >> 28) + (f >> 14) + t * a, n[r++] = u & 268435455;
        }
        return i;
    }
    function o(e) {
        return "0123456789abcdefghijklmnopqrstuvwxyz".charAt(e);
    }
    function u(e, t) {
        var n = T[e.charCodeAt(t)];
        return null == n ? -1 : n;
    }
    function a(e) {
        var t = n();
        return t.fromInt(e), t;
    }
    function f(e) {
        var t = 1, n;
        return 0 != (n = e >>> 16) && (e = n, t += 16), 0 != (n = e >> 8) && (e = n, t += 8), 0 != (n = e >> 4) && (e = n, t += 4), 0 != (n = e >> 2) && (e = n, t += 2), 0 != e >> 1 && (t += 1), t;
    }
    function l(e) {
        this.m = e;
    }
    function c(e) {
        this.m = e, this.mp = e.invDigit(), this.mpl = this.mp & 32767, this.mph = this.mp >> 15, this.um = (1 << e.DB - 15) - 1, this.mt2 = 2 * e.t;
    }
    function h(e, t) {
        return e & t;
    }
    function p(e, t) {
        return e | t;
    }
    function d(e, t) {
        return e ^ t;
    }
    function v(e, t) {
        return e & ~t;
    }
    function m() {
    }
    function g(e) {
        return e;
    }
    function y(e) {
        this.r2 = n(), this.q3 = n(), t.ONE.dlShiftTo(2 * e.t, this.r2), this.mu = this.r2.divide(e), this.m = e;
    }
    function b() {
        this.j = this.i = 0, this.S = [];
    }
    function w() {
    }
    function E(e, n) {
        return new t(e, n);
    }
    function S() {
        this.n = null, this.e = 0, this.coeff = this.dmq1 = this.dmp1 = this.q = this.p = this.d = null;
    }
    var x;
    "Microsoft Internet Explorer" == navigator.appName ? (t.prototype.am = i, x = 30) : "Netscape" != navigator.appName ? (t.prototype.am = r, x = 26) : (t.prototype.am = s, x = 28), t.prototype.DB = x, t.prototype.DM = (1 << x) - 1, t.prototype.DV = 1 << x, t.prototype.FV = Math.pow(2, 52), t.prototype.F1 = 52 - x, t.prototype.F2 = 2 * x - 52;
    var T = [], N;
    x = 48;
    for (N = 0; 9 >= N; ++N)
        T[x++] = N;
    x = 97;
    for (N = 10; 36 > N; ++N)
        T[x++] = N;
    x = 65;
    for (N = 10; 36 > N; ++N)
        T[x++] = N;
    l.prototype.convert = function (e) {
        return 0 > e.s || 0 <= e.compareTo(this.m) ? e.mod(this.m) : e;
    }, l.prototype.revert = function (e) {
        return e;
    }, l.prototype.reduce = function (e) {
        e.divRemTo(this.m, null, e);
    }, l.prototype.mulTo = function (e, t, n) {
        e.multiplyTo(t, n), this.reduce(n);
    }, l.prototype.sqrTo = function (e, t) {
        e.squareTo(t), this.reduce(t);
    }, c.prototype.convert = function (e) {
        var r = n();
        return e.abs().dlShiftTo(this.m.t, r), r.divRemTo(this.m, null, r), 0 > e.s && 0 < r.compareTo(t.ZERO) && this.m.subTo(r, r), r;
    }, c.prototype.revert = function (e) {
        var t = n();
        return e.copyTo(t), this.reduce(t), t;
    }, c.prototype.reduce = function (e) {
        for (; e.t <= this.mt2;)
            e[e.t++] = 0;
        for (var t = 0; t < this.m.t; ++t) {
            var n = e[t] & 32767, r = n * this.mpl + ((n * this.mph + (e[t] >> 15) * this.mpl & this.um) << 15) & e.DM, n = t + this.m.t;
            for (e[n] += this.m.am(0, r, e, t, 0, this.m.t) ; e[n] >= e.DV;)
                e[n] -= e.DV, e[++n]++;
        }
        e.clamp(), e.drShiftTo(this.m.t, e), 0 <= e.compareTo(this.m) && e.subTo(this.m, e);
    }, c.prototype.mulTo = function (e, t, n) {
        e.multiplyTo(t, n), this.reduce(n);
    }, c.prototype.sqrTo = function (e, t) {
        e.squareTo(t), this.reduce(t);
    }, t.prototype.copyTo = function (e) {
        for (var t = this.t - 1; 0 <= t; --t)
            e[t] = this[t];
        e.t = this.t, e.s = this.s;
    }, t.prototype.fromInt = function (e) {
        this.t = 1, this.s = 0 > e ? -1 : 0, 0 < e ? this[0] = e : -1 > e ? this[0] = e + this.DV : this.t = 0;
    }, t.prototype.fromString = function (e, n) {
        var r;
        if (16 == n)
            r = 4;
        else if (8 == n)
            r = 3;
        else if (256 == n)
            r = 8;
        else if (2 == n)
            r = 1;
        else if (32 == n)
            r = 5;
        else {
            if (4 != n) {
                this.fromRadix(e, n);
                return;
            }
            r = 2;
        }
        this.s = this.t = 0;
        for (var i = e.length, s = !1, o = 0; 0 <= --i;) {
            var a = 8 == r ? e[i] & 255 : u(e, i);
            0 > a ? "-" == e.charAt(i) && (s = !0) : (s = !1, 0 == o ? this[this.t++] = a : o + r > this.DB ? (this[this.t - 1] |= (a & (1 << this.DB - o) - 1) << o, this[this.t++] = a >> this.DB - o) : this[this.t - 1] |= a << o, o += r, o >= this.DB && (o -= this.DB));
        }
        8 == r && 0 != (e[0] & 128) && (this.s = -1, 0 < o && (this[this.t - 1] |= (1 << this.DB - o) - 1 << o)), this.clamp(), s && t.ZERO.subTo(this, this);
    }, t.prototype.clamp = function () {
        for (var e = this.s & this.DM; 0 < this.t && this[this.t - 1] == e;)
            --this.t;
    }, t.prototype.dlShiftTo = function (e, t) {
        var n;
        for (n = this.t - 1; 0 <= n; --n)
            t[n + e] = this[n];
        for (n = e - 1; 0 <= n; --n)
            t[n] = 0;
        t.t = this.t + e, t.s = this.s;
    }, t.prototype.drShiftTo = function (e, t) {
        for (var n = e; n < this.t; ++n)
            t[n - e] = this[n];
        t.t = Math.max(this.t - e, 0), t.s = this.s;
    }, t.prototype.lShiftTo = function (e, t) {
        var n = e % this.DB, r = this.DB - n, i = (1 << r) - 1, s = Math.floor(e / this.DB), o = this.s << n & this.DM, u;
        for (u = this.t - 1; 0 <= u; --u)
            t[u + s + 1] = this[u] >> r | o, o = (this[u] & i) << n;
        for (u = s - 1; 0 <= u; --u)
            t[u] = 0;
        t[s] = o, t.t = this.t + s + 1, t.s = this.s, t.clamp();
    }, t.prototype.rShiftTo = function (e, t) {
        t.s = this.s;
        var n = Math.floor(e / this.DB);
        if (n >= this.t)
            t.t = 0;
        else {
            var r = e % this.DB, i = this.DB - r, s = (1 << r) - 1;
            t[0] = this[n] >> r;
            for (var o = n + 1; o < this.t; ++o)
                t[o - n - 1] |= (this[o] & s) << i, t[o - n] = this[o] >> r;
            0 < r && (t[this.t - n - 1] |= (this.s & s) << i), t.t = this.t - n, t.clamp();
        }
    }, t.prototype.subTo = function (e, t) {
        for (var n = 0, r = 0, i = Math.min(e.t, this.t) ; n < i;)
            r += this[n] - e[n], t[n++] = r & this.DM, r >>= this.DB;
        if (e.t < this.t) {
            for (r -= e.s; n < this.t;)
                r += this[n], t[n++] = r & this.DM, r >>= this.DB;
            r += this.s;
        } else {
            for (r += this.s; n < e.t;)
                r -= e[n], t[n++] = r & this.DM, r >>= this.DB;
            r -= e.s;
        }
        t.s = 0 > r ? -1 : 0, -1 > r ? t[n++] = this.DV + r : 0 < r && (t[n++] = r), t.t = n, t.clamp();
    }, t.prototype.multiplyTo = function (e, n) {
        var r = this.abs(), i = e.abs(), s = r.t;
        for (n.t = s + i.t; 0 <= --s;)
            n[s] = 0;
        for (s = 0; s < i.t; ++s)
            n[s + r.t] = r.am(0, i[s], n, s, 0, r.t);
        n.s = 0, n.clamp(), this.s != e.s && t.ZERO.subTo(n, n);
    }, t.prototype.squareTo = function (e) {
        for (var t = this.abs(), n = e.t = 2 * t.t; 0 <= --n;)
            e[n] = 0;
        for (n = 0; n < t.t - 1; ++n) {
            var r = t.am(n, t[n], e, 2 * n, 0, 1);
            (e[n + t.t] += t.am(n + 1, 2 * t[n], e, 2 * n + 1, r, t.t - n - 1)) >= t.DV && (e[n + t.t] -= t.DV, e[n + t.t + 1] = 1);
        }
        0 < e.t && (e[e.t - 1] += t.am(n, t[n], e, 2 * n, 0, 1)), e.s = 0, e.clamp();
    }, t.prototype.divRemTo = function (e, r, i) {
        var s = e.abs();
        if (!(0 >= s.t)) {
            var o = this.abs();
            if (o.t < s.t)
                null != r && r.fromInt(0), null != i && this.copyTo(i);
            else {
                null == i && (i = n());
                var u = n(), a = this.s;
                e = e.s;
                var l = this.DB - f(s[s.t - 1]);
                0 < l ? (s.lShiftTo(l, u), o.lShiftTo(l, i)) : (s.copyTo(u), o.copyTo(i)), s = u.t, o = u[s - 1];
                if (0 != o) {
                    var c = o * (1 << this.F1) + (1 < s ? u[s - 2] >> this.F2 : 0), h = this.FV / c, c = (1 << this.F1) / c, p = 1 << this.F2, d = i.t, v = d - s, m = null == r ? n() : r;
                    u.dlShiftTo(v, m), 0 <= i.compareTo(m) && (i[i.t++] = 1, i.subTo(m, i)), t.ONE.dlShiftTo(s, m);
                    for (m.subTo(u, u) ; u.t < s;)
                        u[u.t++] = 0;
                    for (; 0 <= --v;) {
                        var g = i[--d] == o ? this.DM : Math.floor(i[d] * h + (i[d - 1] + p) * c);
                        if ((i[d] += u.am(0, g, i, v, 0, s)) < g)
                            for (u.dlShiftTo(v, m), i.subTo(m, i) ; i[d] < --g;)
                                i.subTo(m, i);
                    }
                    null != r && (i.drShiftTo(s, r), a != e && t.ZERO.subTo(r, r)), i.t = s, i.clamp(), 0 < l && i.rShiftTo(l, i), 0 > a && t.ZERO.subTo(i, i);
                }
            }
        }
    }, t.prototype.invDigit = function () {
        if (1 > this.t)
            return 0;
        var e = this[0];
        if (0 == (e & 1))
            return 0;
        var t = e & 3, t = t * (2 - (e & 15) * t) & 15, t = t * (2 - (e & 255) * t) & 255, t = t * (2 - ((e & 65535) * t & 65535)) & 65535, t = t * (2 - e * t % this.DV) % this.DV;
        return 0 < t ? this.DV - t : -t;
    }, t.prototype.isEven = function () {
        return 0 == (0 < this.t ? this[0] & 1 : this.s);
    }, t.prototype.exp = function (e, r) {
        if (4294967295 < e || 1 > e)
            return t.ONE;
        var i = n(), s = n(), o = r.convert(this), u = f(e) - 1;
        for (o.copyTo(i) ; 0 <= --u;)
            if (r.sqrTo(i, s), 0 < (e & 1 << u))
                r.mulTo(s, o, i);
            else
                var a = i, i = s, s = a;
        return r.revert(i);
    }, t.prototype.toString = function (e) {
        if (0 > this.s)
            return "-" + this.negate().toString(e);
        if (16 == e)
            e = 4;
        else if (8 == e)
            e = 3;
        else if (2 == e)
            e = 1;
        else if (32 == e)
            e = 5;
        else {
            if (4 != e)
                return this.toRadix(e);
            e = 2;
        }
        var t = (1 << e) - 1, n, r = !1, i = "", s = this.t, u = this.DB - s * this.DB % e;
        if (0 < s--)
            for (u < this.DB && 0 < (n = this[s] >> u) && (r = !0, i = o(n)) ; 0 <= s;)
                u < e ? (n = (this[s] & (1 << u) - 1) << e - u, n |= this[--s] >> (u += this.DB - e)) : (n = this[s] >> (u -= e) & t, 0 >= u && (u += this.DB, --s)), 0 < n && (r = !0), r && (i += o(n));
        return r ? i : "0";
    }, t.prototype.negate = function () {
        var e = n();
        return t.ZERO.subTo(this, e), e;
    }, t.prototype.abs = function () {
        return 0 > this.s ? this.negate() : this;
    }, t.prototype.compareTo = function (e) {
        var t = this.s - e.s;
        if (0 != t)
            return t;
        var n = this.t, t = n - e.t;
        if (0 != t)
            return 0 > this.s ? -t : t;
        for (; 0 <= --n;)
            if (0 != (t = this[n] - e[n]))
                return t;
        return 0;
    }, t.prototype.bitLength = function () {
        return 0 >= this.t ? 0 : this.DB * (this.t - 1) + f(this[this.t - 1] ^ this.s & this.DM);
    }, t.prototype.mod = function (e) {
        var r = n();
        return this.abs().divRemTo(e, null, r), 0 > this.s && 0 < r.compareTo(t.ZERO) && e.subTo(r, r), r;
    }, t.prototype.modPowInt = function (e, t) {
        var n;
        return n = 256 > e || t.isEven() ? new l(t) : new c(t), this.exp(e, n);
    }, t.ZERO = a(0), t.ONE = a(1), m.prototype.convert = g, m.prototype.revert = g, m.prototype.mulTo = function (e, t, n) {
        e.multiplyTo(t, n);
    }, m.prototype.sqrTo = function (e, t) {
        e.squareTo(t);
    }, y.prototype.convert = function (e) {
        if (0 > e.s || e.t > 2 * this.m.t)
            return e.mod(this.m);
        if (0 > e.compareTo(this.m))
            return e;
        var t = n();
        return e.copyTo(t), this.reduce(t), t;
    }, y.prototype.revert = function (e) {
        return e;
    }, y.prototype.reduce = function (e) {
        e.drShiftTo(this.m.t - 1, this.r2), e.t > this.m.t + 1 && (e.t = this.m.t + 1, e.clamp()), this.mu.multiplyUpperTo(this.r2, this.m.t + 1, this.q3);
        for (this.m.multiplyLowerTo(this.q3, this.m.t + 1, this.r2) ; 0 > e.compareTo(this.r2) ;)
            e.dAddOffset(1, this.m.t + 1);
        for (e.subTo(this.r2, e) ; 0 <= e.compareTo(this.m) ;)
            e.subTo(this.m, e);
    }, y.prototype.mulTo = function (e, t, n) {
        e.multiplyTo(t, n), this.reduce(n);
    }, y.prototype.sqrTo = function (e, t) {
        e.squareTo(t), this.reduce(t);
    };
    var C = [2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997], k = 67108864 / C[C.length - 1];
    t.prototype.chunkSize = function (e) {
        return Math.floor(Math.LN2 * this.DB / Math.log(e));
    }, t.prototype.toRadix = function (e) {
        null == e && (e = 10);
        if (0 == this.signum() || 2 > e || 36 < e)
            return "0";
        var t = this.chunkSize(e), t = Math.pow(e, t), r = a(t), i = n(), s = n(), o = "";
        for (this.divRemTo(r, i, s) ; 0 < i.signum() ;)
            o = (t + s.intValue()).toString(e).substr(1) + o, i.divRemTo(r, i, s);
        return s.intValue().toString(e) + o;
    }, t.prototype.fromRadix = function (e, n) {
        this.fromInt(0), null == n && (n = 10);
        for (var r = this.chunkSize(n), i = Math.pow(n, r), s = !1, o = 0, a = 0, f = 0; f < e.length; ++f) {
            var l = u(e, f);
            0 > l ? "-" == e.charAt(f) && 0 == this.signum() && (s = !0) : (a = n * a + l, ++o >= r && (this.dMultiply(i), this.dAddOffset(a, 0), a = o = 0));
        }
        0 < o && (this.dMultiply(Math.pow(n, o)), this.dAddOffset(a, 0)), s && t.ZERO.subTo(this, this);
    }, t.prototype.fromNumber = function (e, n, r) {
        if ("number" == typeof n)
            if (2 > e)
                this.fromInt(1);
            else
                for (this.fromNumber(e, r), this.testBit(e - 1) || this.bitwiseTo(t.ONE.shiftLeft(e - 1), p, this), this.isEven() && this.dAddOffset(1, 0) ; !this.isProbablePrime(n) ;)
                    this.dAddOffset(2, 0), this.bitLength() > e && this.subTo(t.ONE.shiftLeft(e - 1), this);
        else {
            r = [];
            var i = e & 7;
            r.length = (e >> 3) + 1, n.nextBytes(r), r[0] = 0 < i ? r[0] & (1 << i) - 1 : 0, this.fromString(r, 256);
        }
    }, t.prototype.bitwiseTo = function (e, t, n) {
        var r, i, s = Math.min(e.t, this.t);
        for (r = 0; r < s; ++r)
            n[r] = t(this[r], e[r]);
        if (e.t < this.t) {
            i = e.s & this.DM;
            for (r = s; r < this.t; ++r)
                n[r] = t(this[r], i);
            n.t = this.t;
        } else {
            i = this.s & this.DM;
            for (r = s; r < e.t; ++r)
                n[r] = t(i, e[r]);
            n.t = e.t;
        }
        n.s = t(this.s, e.s), n.clamp();
    }, t.prototype.changeBit = function (e, n) {
        var r = t.ONE.shiftLeft(e);
        return this.bitwiseTo(r, n, r), r;
    }, t.prototype.addTo = function (e, t) {
        for (var n = 0, r = 0, i = Math.min(e.t, this.t) ; n < i;)
            r += this[n] + e[n], t[n++] = r & this.DM, r >>= this.DB;
        if (e.t < this.t) {
            for (r += e.s; n < this.t;)
                r += this[n], t[n++] = r & this.DM, r >>= this.DB;
            r += this.s;
        } else {
            for (r += this.s; n < e.t;)
                r += e[n], t[n++] = r & this.DM, r >>= this.DB;
            r += e.s;
        }
        t.s = 0 > r ? -1 : 0, 0 < r ? t[n++] = r : -1 > r && (t[n++] = this.DV + r), t.t = n, t.clamp();
    }, t.prototype.dMultiply = function (e) {
        this[this.t] = this.am(0, e - 1, this, 0, 0, this.t), ++this.t, this.clamp();
    }, t.prototype.dAddOffset = function (e, t) {
        if (0 != e) {
            for (; this.t <= t;)
                this[this.t++] = 0;
            for (this[t] += e; this[t] >= this.DV;)
                this[t] -= this.DV, ++t >= this.t && (this[this.t++] = 0), ++this[t];
        }
    }, t.prototype.multiplyLowerTo = function (e, t, n) {
        var r = Math.min(this.t + e.t, t);
        n.s = 0;
        for (n.t = r; 0 < r;)
            n[--r] = 0;
        var i;
        for (i = n.t - this.t; r < i; ++r)
            n[r + this.t] = this.am(0, e[r], n, r, 0, this.t);
        for (i = Math.min(e.t, t) ; r < i; ++r)
            this.am(0, e[r], n, r, 0, t - r);
        n.clamp();
    }, t.prototype.multiplyUpperTo = function (e, t, n) {
        --t;
        var r = n.t = this.t + e.t - t;
        for (n.s = 0; 0 <= --r;)
            n[r] = 0;
        for (r = Math.max(t - this.t, 0) ; r < e.t; ++r)
            n[this.t + r - t] = this.am(t - r, e[r], n, 0, 0, this.t + r - t);
        n.clamp(), n.drShiftTo(1, n);
    }, t.prototype.modInt = function (e) {
        if (0 >= e)
            return 0;
        var t = this.DV % e, n = 0 > this.s ? e - 1 : 0;
        if (0 < this.t)
            if (0 == t)
                n = this[0] % e;
            else
                for (var r = this.t - 1; 0 <= r; --r)
                    n = (t * n + this[r]) % e;
        return n;
    }, t.prototype.millerRabin = function (e) {
        var r = this.subtract(t.ONE), i = r.getLowestSetBit();
        if (0 >= i)
            return !1;
        var s = r.shiftRight(i);
        e = e + 1 >> 1, e > C.length && (e = C.length);
        for (var o = n(), u = 0; u < e; ++u) {
            o.fromInt(C[Math.floor(Math.random() * C.length)]);
            var a = o.modPow(s, this);
            if (0 != a.compareTo(t.ONE) && 0 != a.compareTo(r)) {
                for (var f = 1; f++ < i && 0 != a.compareTo(r) ;)
                    if (a = a.modPowInt(2, this), 0 == a.compareTo(t.ONE))
                        return !1;
                if (0 != a.compareTo(r))
                    return !1;
            }
        }
        return !0;
    }, t.prototype.clone = function () {
        var e = n();
        return this.copyTo(e), e;
    }, t.prototype.intValue = function () {
        if (0 > this.s) {
            if (1 == this.t)
                return this[0] - this.DV;
            if (0 == this.t)
                return -1;
        } else {
            if (1 == this.t)
                return this[0];
            if (0 == this.t)
                return 0;
        }
        return (this[1] & (1 << 32 - this.DB) - 1) << this.DB | this[0];
    }, t.prototype.byteValue = function () {
        return 0 == this.t ? this.s : this[0] << 24 >> 24;
    }, t.prototype.shortValue = function () {
        return 0 == this.t ? this.s : this[0] << 16 >> 16;
    }, t.prototype.signum = function () {
        return 0 > this.s ? -1 : 0 >= this.t || 1 == this.t && 0 >= this[0] ? 0 : 1;
    }, t.prototype.toByteArray = function () {
        var e = this.t, t = [];
        t[0] = this.s;
        var n = this.DB - e * this.DB % 8, r, i = 0;
        if (0 < e--)
            for (n < this.DB && (r = this[e] >> n) != (this.s & this.DM) >> n && (t[i++] = r | this.s << this.DB - n) ; 0 <= e;)
                if (8 > n ? (r = (this[e] & (1 << n) - 1) << 8 - n, r |= this[--e] >> (n += this.DB - 8)) : (r = this[e] >> (n -= 8) & 255, 0 >= n && (n += this.DB, --e)), 0 != (r & 128) && (r |= -256), 0 == i && (this.s & 128) != (r & 128) && ++i, 0 < i || r != this.s)
                    t[i++] = r;
        return t;
    }, t.prototype.equals = function (e) {
        return 0 == this.compareTo(e);
    }, t.prototype.min = function (e) {
        return 0 > this.compareTo(e) ? this : e;
    }, t.prototype.max = function (e) {
        return 0 < this.compareTo(e) ? this : e;
    }, t.prototype.and = function (e) {
        var t = n();
        return this.bitwiseTo(e, h, t), t;
    }, t.prototype.or = function (e) {
        var t = n();
        return this.bitwiseTo(e, p, t), t;
    }, t.prototype.xor = function (e) {
        var t = n();
        return this.bitwiseTo(e, d, t), t;
    }, t.prototype.andNot = function (e) {
        var t = n();
        return this.bitwiseTo(e, v, t), t;
    }, t.prototype.not = function () {
        for (var e = n(), t = 0; t < this.t; ++t)
            e[t] = this.DM & ~this[t];
        return e.t = this.t, e.s = ~this.s, e;
    }, t.prototype.shiftLeft = function (e) {
        var t = n();
        return 0 > e ? this.rShiftTo(-e, t) : this.lShiftTo(e, t), t;
    }, t.prototype.shiftRight = function (e) {
        var t = n();
        return 0 > e ? this.lShiftTo(-e, t) : this.rShiftTo(e, t), t;
    }, t.prototype.getLowestSetBit = function () {
        for (var e = 0; e < this.t; ++e)
            if (0 != this[e]) {
                var t = e * this.DB;
                e = this[e];
                if (0 == e)
                    e = -1;
                else {
                    var n = 0;
                    0 == (e & 65535) && (e >>= 16, n += 16), 0 == (e & 255) && (e >>= 8, n += 8), 0 == (e & 15) && (e >>= 4, n += 4), 0 == (e & 3) && (e >>= 2, n += 2), 0 == (e & 1) && ++n, e = n;
                }
                return t + e;
            }
        return 0 > this.s ? this.t * this.DB : -1;
    }, t.prototype.bitCount = function () {
        for (var e = 0, t = this.s & this.DM, n = 0; n < this.t; ++n) {
            for (var r = this[n] ^ t, i = 0; 0 != r;)
                r &= r - 1, ++i;
            e += i;
        }
        return e;
    }, t.prototype.testBit = function (e) {
        var t = Math.floor(e / this.DB);
        return t >= this.t ? 0 != this.s : 0 != (this[t] & 1 << e % this.DB);
    }, t.prototype.setBit = function (e) {
        return this.changeBit(e, p);
    }, t.prototype.clearBit = function (e) {
        return this.changeBit(e, v);
    }, t.prototype.flipBit = function (e) {
        return this.changeBit(e, d);
    }, t.prototype.add = function (e) {
        var t = n();
        return this.addTo(e, t), t;
    }, t.prototype.subtract = function (e) {
        var t = n();
        return this.subTo(e, t), t;
    }, t.prototype.multiply = function (e) {
        var t = n();
        return this.multiplyTo(e, t), t;
    }, t.prototype.divide = function (e) {
        var t = n();
        return this.divRemTo(e, t, null), t;
    }, t.prototype.remainder = function (e) {
        var t = n();
        return this.divRemTo(e, null, t), t;
    }, t.prototype.divideAndRemainder = function (e) {
        var t = n(), r = n();
        return this.divRemTo(e, t, r), [t, r];
    }, t.prototype.modPow = function (e, t) {
        var r = e.bitLength(), i, s = a(1), o;
        if (0 >= r)
            return s;
        i = 18 > r ? 1 : 48 > r ? 3 : 144 > r ? 4 : 768 > r ? 5 : 6, o = 8 > r ? new l(t) : t.isEven() ? new y(t) : new c(t);
        var u = [], h = 3, p = i - 1, d = (1 << i) - 1;
        u[1] = o.convert(this);
        if (1 < i)
            for (r = n(), o.sqrTo(u[1], r) ; h <= d;)
                u[h] = n(), o.mulTo(r, u[h - 2], u[h]), h += 2;
        for (var v = e.t - 1, m, g = !0, b = n(), r = f(e[v]) - 1; 0 <= v;) {
            r >= p ? m = e[v] >> r - p & d : (m = (e[v] & (1 << r + 1) - 1) << p - r, 0 < v && (m |= e[v - 1] >> this.DB + r - p));
            for (h = i; 0 == (m & 1) ;)
                m >>= 1, --h;
            0 > (r -= h) && (r += this.DB, --v);
            if (g)
                u[m].copyTo(s), g = !1;
            else {
                for (; 1 < h;)
                    o.sqrTo(s, b), o.sqrTo(b, s), h -= 2;
                0 < h ? o.sqrTo(s, b) : (h = s, s = b, b = h), o.mulTo(b, u[m], s);
            }
            for (; 0 <= v && 0 == (e[v] & 1 << r) ;)
                o.sqrTo(s, b), h = s, s = b, b = h, 0 > --r && (r = this.DB - 1, --v);
        }
        return o.revert(s);
    }, t.prototype.modInverse = function (e) {
        var n = e.isEven();
        if (this.isEven() && n || 0 == e.signum())
            return t.ZERO;
        for (var r = e.clone(), i = this.clone(), s = a(1), o = a(0), u = a(0), f = a(1) ; 0 != r.signum() ;) {
            for (; r.isEven() ;)
                r.rShiftTo(1, r), n ? (s.isEven() && o.isEven() || (s.addTo(this, s), o.subTo(e, o)), s.rShiftTo(1, s)) : o.isEven() || o.subTo(e, o), o.rShiftTo(1, o);
            for (; i.isEven() ;)
                i.rShiftTo(1, i), n ? (u.isEven() && f.isEven() || (u.addTo(this, u), f.subTo(e, f)), u.rShiftTo(1, u)) : f.isEven() || f.subTo(e, f), f.rShiftTo(1, f);
            0 <= r.compareTo(i) ? (r.subTo(i, r), n && s.subTo(u, s), o.subTo(f, o)) : (i.subTo(r, i), n && u.subTo(s, u), f.subTo(o, f));
        }
        return 0 != i.compareTo(t.ONE) ? t.ZERO : 0 <= f.compareTo(e) ? f.subtract(e) : 0 > f.signum() ? (f.addTo(e, f), 0 > f.signum() ? f.add(e) : f) : f;
    }, t.prototype.pow = function (e) {
        return this.exp(e, new m);
    }, t.prototype.gcd = function (e) {
        var t = 0 > this.s ? this.negate() : this.clone();
        e = 0 > e.s ? e.negate() : e.clone();
        if (0 > t.compareTo(e)) {
            var n = t, t = e;
            e = n;
        }
        var n = t.getLowestSetBit(), r = e.getLowestSetBit();
        if (0 > r)
            return t;
        n < r && (r = n), 0 < r && (t.rShiftTo(r, t), e.rShiftTo(r, e));
        for (; 0 < t.signum() ;)
            0 < (n = t.getLowestSetBit()) && t.rShiftTo(n, t), 0 < (n = e.getLowestSetBit()) && e.rShiftTo(n, e), 0 <= t.compareTo(e) ? (t.subTo(e, t), t.rShiftTo(1, t)) : (e.subTo(t, e), e.rShiftTo(1, e));
        return 0 < r && e.lShiftTo(r, e), e;
    }, t.prototype.isProbablePrime = function (e) {
        var t, n = this.abs();
        if (1 == n.t && n[0] <= C[C.length - 1]) {
            for (t = 0; t < C.length; ++t)
                if (n[0] == C[t])
                    return !0;
            return !1;
        }
        if (n.isEven())
            return !1;
        for (t = 1; t < C.length;) {
            for (var r = C[t], i = t + 1; i < C.length && r < k;)
                r *= C[i++];
            for (r = n.modInt(r) ; t < i;)
                if (0 == r % C[t++])
                    return !1;
        }
        return n.millerRabin(e);
    }, t.prototype.square = function () {
        var e = n();
        return this.squareTo(e), e;
    }, b.prototype.init = function (e) {
        var t, n, r;
        for (t = 0; 256 > t; ++t)
            this.S[t] = t;
        for (t = n = 0; 256 > t; ++t)
            n = n + this.S[t] + e[t % e.length] & 255, r = this.S[t], this.S[t] = this.S[n], this.S[n] = r;
        this.j = this.i = 0;
    }, b.prototype.next = function () {
        var e;
        return this.i = this.i + 1 & 255, this.j = this.j + this.S[this.i] & 255, e = this.S[this.i], this.S[this.i] = this.S[this.j], this.S[this.j] = e, this.S[e + this.S[this.i] & 255];
    };
    var L, A, O;
    if (null == A && (A = [], O = 0, window.crypto && window.crypto.getRandomValues))
        for (N = new Uint32Array(256), window.crypto.getRandomValues(N), x = 0; x < N.length; ++x)
            A[O++] = N[x] & 255;
    w.prototype.nextBytes = function (e) {
        var t;
        for (t = 0; t < e.length; ++t) {
            var n = t, r;
            if (null == L) {
                for (L = new b; 256 > O;)
                    r = Math.floor(65536 * Math.random()), A[O++] = r & 255;
                L.init(A);
                for (O = 0; O < A.length; ++O)
                    A[O] = 0;
                O = 0;
            }
            r = L.next(), e[n] = r;
        }
    }, S.prototype.doPublic = function (e) {
        return e.modPowInt(this.e, this.n);
    }, S.prototype.setPublic = function (e, t) {
        null != e && null != t && 0 < e.length && 0 < t.length ? (this.n = E(e, 16), this.e = parseInt(t, 16)) : console.error("Invalid RSA public key");
    }, S.prototype.encrypt = function (e) {
        var n;
        n = this.n.bitLength() + 7 >> 3;
        if (n < e.length + 11)
            console.error("Message too long for RSA"), n = null;
        else {
            for (var r = [], i = e.length - 1; 0 <= i && 0 < n;) {
                var s = e.charCodeAt(i--);
                128 > s ? r[--n] = s : 127 < s && 2048 > s ? (r[--n] = s & 63 | 128, r[--n] = s >> 6 | 192) : (r[--n] = s & 63 | 128, r[--n] = s >> 6 & 63 | 128, r[--n] = s >> 12 | 224);
            }
            r[--n] = 0, e = new w;
            for (i = []; 2 < n;) {
                for (i[0] = 0; 0 == i[0];)
                    e.nextBytes(i);
                r[--n] = i[0];
            }
            r[--n] = 2, r[--n] = 0, n = new t(r);
        }
        return null == n ? null : (n = this.doPublic(n), null == n ? null : (n = n.toString(16), 0 == (n.length & 1) ? n : "0" + n));
    }, S.prototype.doPrivate = function (e) {
        if (null == this.p || null == this.q)
            return e.modPow(this.d, this.n);
        var t = e.mod(this.p).modPow(this.dmp1, this.p);
        for (e = e.mod(this.q).modPow(this.dmq1, this.q) ; 0 > t.compareTo(e) ;)
            t = t.add(this.p);
        return t.subtract(e).multiply(this.coeff).mod(this.p).multiply(this.q).add(e);
    }, S.prototype.setPrivate = function (e, t, n) {
        null != e && null != t && 0 < e.length && 0 < t.length ? (this.n = E(e, 16), this.e = parseInt(t, 16), this.d = E(n, 16)) : console.error("Invalid RSA private key");
    }, S.prototype.setPrivateEx = function (e, t, n, r, i, s, o, u) {
        null != e && null != t && 0 < e.length && 0 < t.length ? (this.n = E(e, 16), this.e = parseInt(t, 16), this.d = E(n, 16), this.p = E(r, 16), this.q = E(i, 16), this.dmp1 = E(s, 16), this.dmq1 = E(o, 16), this.coeff = E(u, 16)) : console.error("Invalid RSA private key");
    }, S.prototype.generate = function (e, n) {
        var r = new w, i = e >> 1;
        this.e = parseInt(n, 16);
        for (var s = new t(n, 16) ; ;) {
            for (; this.p = new t(e - i, 1, r), 0 != this.p.subtract(t.ONE).gcd(s).compareTo(t.ONE) || !this.p.isProbablePrime(10) ;)
                ;
            for (; this.q = new t(i, 1, r), 0 != this.q.subtract(t.ONE).gcd(s).compareTo(t.ONE) || !this.q.isProbablePrime(10) ;)
                ;
            if (0 >= this.p.compareTo(this.q)) {
                var o = this.p;
                this.p = this.q, this.q = o;
            }
            var o = this.p.subtract(t.ONE), u = this.q.subtract(t.ONE), a = o.multiply(u);
            if (0 == a.gcd(s).compareTo(t.ONE)) {
                this.n = this.p.multiply(this.q), this.d = s.modInverse(a), this.dmp1 = this.d.mod(o), this.dmq1 = this.d.mod(u), this.coeff = this.q.modInverse(this.p);
                break;
            }
        }
    }, function () {
        S.prototype.generateAsync = function (e, r, i) {
            var s = new w, o = e >> 1;
            this.e = parseInt(r, 16);
            var u = new t(r, 16), a = this, f = function () {
                var r = function () {
                    if (0 >= a.p.compareTo(a.q)) {
                        var e = a.p;
                        a.p = a.q, a.q = e;
                    }
                    var e = a.p.subtract(t.ONE), n = a.q.subtract(t.ONE), r = e.multiply(n);
                    0 == r.gcd(u).compareTo(t.ONE) ? (a.n = a.p.multiply(a.q), a.d = u.modInverse(r), a.dmp1 = a.d.mod(e), a.dmq1 = a.d.mod(n), a.coeff = a.q.modInverse(a.p), setTimeout(function () {
                        i();
                    }, 0)) : setTimeout(f, 0);
                }, l = function () {
                    a.q = n(), a.q.fromNumberAsync(o, 1, s, function () {
                        a.q.subtract(t.ONE).gcda(u, function (e) {
                            0 == e.compareTo(t.ONE) && a.q.isProbablePrime(10) ? setTimeout(r, 0) : setTimeout(l, 0);
                        });
                    });
                }, c = function () {
                    a.p = n(), a.p.fromNumberAsync(e - o, 1, s, function () {
                        a.p.subtract(t.ONE).gcda(u, function (e) {
                            0 == e.compareTo(t.ONE) && a.p.isProbablePrime(10) ? setTimeout(l, 0) : setTimeout(c, 0);
                        });
                    });
                };
                setTimeout(c, 0);
            };
            setTimeout(f, 0);
        }, t.prototype.gcda = function (e, t) {
            var n = 0 > this.s ? this.negate() : this.clone(), r = 0 > e.s ? e.negate() : e.clone();
            if (0 > n.compareTo(r))
                var i = n, n = r, r = i;
            var s = n.getLowestSetBit(), o = r.getLowestSetBit();
            if (0 > o)
                t(n);
            else {
                s < o && (o = s), 0 < o && (n.rShiftTo(o, n), r.rShiftTo(o, r));
                var u = function () {
                    0 < (s = n.getLowestSetBit()) && n.rShiftTo(s, n), 0 < (s = r.getLowestSetBit()) && r.rShiftTo(s, r), 0 <= n.compareTo(r) ? (n.subTo(r, n), n.rShiftTo(1, n)) : (r.subTo(n, r), r.rShiftTo(1, r)), 0 < n.signum() ? setTimeout(u, 0) : (0 < o && r.lShiftTo(o, r), setTimeout(function () {
                        t(r);
                    }, 0));
                };
                setTimeout(u, 10);
            }
        }, t.prototype.fromNumberAsync = function (e, n, r, i) {
            if ("number" == typeof n)
                if (2 > e)
                    this.fromInt(1);
                else {
                    this.fromNumber(e, r), this.testBit(e - 1) || this.bitwiseTo(t.ONE.shiftLeft(e - 1), p, this), this.isEven() && this.dAddOffset(1, 0);
                    var s = this, o = function () {
                        s.dAddOffset(2, 0), s.bitLength() > e && s.subTo(t.ONE.shiftLeft(e - 1), s), s.isProbablePrime(n) ? setTimeout(function () {
                            i();
                        }, 0) : setTimeout(o, 0);
                    };
                    setTimeout(o, 0);
                }
            else {
                r = [];
                var u = e & 7;
                r.length = (e >> 3) + 1, n.nextBytes(r), r[0] = 0 < u ? r[0] & (1 << u) - 1 : 0, this.fromString(r, 256);
            }
        };
    }(), function (e) {
        var t = {}, n;
        t.decode = function (t) {
            var r;
            if (n === e) {
                var i = "0123456789ABCDEF";
                n = [];
                for (r = 0; 16 > r; ++r)
                    n[i.charAt(r)] = r;
                i = i.toLowerCase();
                for (r = 10; 16 > r; ++r)
                    n[i.charAt(r)] = r;
                for (r = 0; 8 > r; ++r)
                    n[" \f\n\r	 \u2028\u2029".charAt(r)] = -1;
            }
            var i = [], s = 0, o = 0;
            for (r = 0; r < t.length; ++r) {
                var u = t.charAt(r);
                if ("=" == u)
                    break;
                u = n[u];
                if (-1 != u) {
                    if (u === e)
                        throw "Illegal character at offset " + r;
                    s |= u, 2 <= ++o ? (i[i.length] = s, o = s = 0) : s <<= 4;
                }
            }
            if (o)
                throw "Hex encoding incomplete: 4 bits missing";
            return i;
        }, window.HexSe = t;
    }(), function (e) {
        var t = {}, n;
        t.decode = function (t) {
            var r;
            if (n === e) {
                n = [];
                for (r = 0; 64 > r; ++r)
                    n["ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(r)] = r;
                for (r = 0; 9 > r; ++r)
                    n["= \f\n\r	 \u2028\u2029".charAt(r)] = -1;
            }
            var i = [], s = 0, o = 0;
            for (r = 0; r < t.length; ++r) {
                var u = t.charAt(r);
                if ("=" == u)
                    break;
                u = n[u];
                if (-1 != u) {
                    if (u === e)
                        throw "Illegal character at offset " + r;
                    s |= u, 4 <= ++o ? (i[i.length] = s >> 16, i[i.length] = s >> 8 & 255, i[i.length] = s & 255, o = s = 0) : s <<= 6;
                }
            }
            switch (o) {
                case 1:
                    throw "Base64 encoding incomplete: at least 2 bits missing";
                case 2:
                    i[i.length] = s >> 10;
                    break;
                case 3:
                    i[i.length] = s >> 16, i[i.length] = s >> 8 & 255;
            }
            return i;
        }, t.re = /-----BEGIN [^-]+-----([A-Za-z0-9+\/=\s]+)-----END [^-]+-----|begin-base64[^\n]+\n([A-Za-z0-9+\/=\s]+)====/, t.unarmor = function (e) {
            var n = t.re.exec(e);
            if (n)
                if (n[1])
                    e = n[1];
                else {
                    if (!n[2])
                        throw "RegExp out of sync";
                    e = n[2];
                }
            return t.decode(e);
        }, window.Base64Se = t;
    }(), function (e) {
        function t(e, n) {
            e instanceof t ? (this.enc = e.enc, this.pos = e.pos) : (this.enc = e, this.pos = n);
        }
        function n(e, t, n, r, i) {
            this.stream = e, this.header = t, this.length = n, this.tag = r, this.sub = i;
        }
        t.prototype.get = function (t) {
            t === e && (t = this.pos++);
            if (t >= this.enc.length)
                throw "Requesting byte offset " + t + " on a stream of length " + this.enc.length;
            return this.enc[t];
        }, t.prototype.hexDigits = "0123456789ABCDEF", t.prototype.hexByte = function (e) {
            return this.hexDigits.charAt(e >> 4 & 15) + this.hexDigits.charAt(e & 15);
        }, t.prototype.hexDump = function (e, t, n) {
            for (var r = ""; e < t; ++e)
                if (r += this.hexByte(this.get(e)), !0 !== n)
                    switch (e & 15) {
                        case 7:
                            r += "  ";
                            break;
                        case 15:
                            r += "\n";
                            break;
                        default:
                            r += " ";
                    }
            return r;
        }, t.prototype.parseStringISO = function (e, t) {
            for (var n = "", r = e; r < t; ++r)
                n += String.fromCharCode(this.get(r));
            return n;
        }, t.prototype.parseStringUTF = function (e, t) {
            for (var n = "", r = e; r < t;)
                var i = this.get(r++), n = 128 > i ? n + String.fromCharCode(i) : 191 < i && 224 > i ? n + String.fromCharCode((i & 31) << 6 | this.get(r++) & 63) : n + String.fromCharCode((i & 15) << 12 | (this.get(r++) & 63) << 6 | this.get(r++) & 63);
            return n;
        }, t.prototype.parseStringBMP = function (e, t) {
            for (var n = "", r = e; r < t; r += 2)
                var i = this.get(r), s = this.get(r + 1), n = n + String.fromCharCode((i << 8) + s);
            return n;
        }, t.prototype.reTime = /^((?:1[89]|2\d)?\d\d)(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])([01]\d|2[0-3])(?:([0-5]\d)(?:([0-5]\d)(?:[.,](\d{1,3}))?)?)?(Z|[-+](?:[0]\d|1[0-2])([0-5]\d)?)?$/, t.prototype.parseTime = function (e, t) {
            var n = this.parseStringISO(e, t), r = this.reTime.exec(n);
            return r ? (n = r[1] + "-" + r[2] + "-" + r[3] + " " + r[4], r[5] && (n += ":" + r[5], r[6] && (n += ":" + r[6], r[7] && (n += "." + r[7]))), r[8] && (n += " UTC", "Z" != r[8] && (n += r[8], r[9] && (n += ":" + r[9]))), n) : "Unrecognized time: " + n;
        }, t.prototype.parseInteger = function (e, t) {
            var n = t - e;
            if (4 < n) {
                var n = n << 3, r = this.get(e);
                if (0 === r)
                    n -= 8;
                else
                    for (; 128 > r;)
                        r <<= 1, --n;
                return "(" + n + " bit)";
            }
            n = 0;
            for (r = e; r < t; ++r)
                n = n << 8 | this.get(r);
            return n;
        }, t.prototype.parseBitString = function (e, t) {
            var n = this.get(e), r = (t - e - 1 << 3) - n, i = "(" + r + " bit)";
            if (20 >= r)
                for (var s = n, i = i + " ", n = t - 1; n > e; --n) {
                    for (r = this.get(n) ; 8 > s; ++s)
                        i += r >> s & 1 ? "1" : "0";
                    s = 0;
                }
            return i;
        }, t.prototype.parseOctetString = function (e, t) {
            var n = t - e, r = "(" + n + " byte) ";
            100 < n && (t = e + 100);
            for (var i = e; i < t; ++i)
                r += this.hexByte(this.get(i));
            return 100 < n && (r += "…"), r;
        }, t.prototype.parseOID = function (e, t) {
            for (var n = "", r = 0, i = 0, s = e; s < t; ++s) {
                var o = this.get(s), r = r << 7 | o & 127, i = i + 7;
                o & 128 || ("" === n ? (n = 80 > r ? 40 > r ? 0 : 1 : 2, n = n + "." + (r - 40 * n)) : n += "." + (31 <= i ? "bigint" : r), r = i = 0);
            }
            return n;
        }, n.prototype.typeName = function () {
            if (this.tag === e)
                return "unknown";
            var t = this.tag & 31;
            switch (this.tag >> 6) {
                case 0:
                    switch (t) {
                        case 0:
                            return "EOC";
                        case 1:
                            return "BOOLEAN";
                        case 2:
                            return "INTEGER";
                        case 3:
                            return "BIT_STRING";
                        case 4:
                            return "OCTET_STRING";
                        case 5:
                            return "NULL";
                        case 6:
                            return "OBJECT_IDENTIFIER";
                        case 7:
                            return "ObjectDescriptor";
                        case 8:
                            return "EXTERNAL";
                        case 9:
                            return "REAL";
                        case 10:
                            return "ENUMERATED";
                        case 11:
                            return "EMBEDDED_PDV";
                        case 12:
                            return "UTF8String";
                        case 16:
                            return "SEQUENCE";
                        case 17:
                            return "SET";
                        case 18:
                            return "NumericString";
                        case 19:
                            return "PrintableString";
                        case 20:
                            return "TeletexString";
                        case 21:
                            return "VideotexString";
                        case 22:
                            return "IA5String";
                        case 23:
                            return "UTCTime";
                        case 24:
                            return "GeneralizedTime";
                        case 25:
                            return "GraphicString";
                        case 26:
                            return "VisibleString";
                        case 27:
                            return "GeneralString";
                        case 28:
                            return "UniversalString";
                        case 30:
                            return "BMPString";
                        default:
                            return "Universal_" + t.toString(16);
                    }
                    ;
                case 1:
                    return "Application_" + t.toString(16);
                case 2:
                    return "[" + t + "]";
                case 3:
                    return "Private_" + t.toString(16);
            }
        }, n.prototype.reSeemsASCII = /^[ -~]+$/, n.prototype.content = function () {
            if (this.tag === e)
                return null;
            var t = this.tag >> 6, n = this.tag & 31, r = this.posContent(), i = Math.abs(this.length);
            if (0 !== t)
                return null !== this.sub ? "(" + this.sub.length + " elem)" : (t = this.stream.parseStringISO(r, r + Math.min(i, 100)), this.reSeemsASCII.test(t) ? t.substring(0, 200) + (200 < t.length ? "…" : "") : this.stream.parseOctetString(r, r + i));
            switch (n) {
                case 1:
                    return 0 === this.stream.get(r) ? "false" : "true";
                case 2:
                    return this.stream.parseInteger(r, r + i);
                case 3:
                    return this.sub ? "(" + this.sub.length + " elem)" : this.stream.parseBitString(r, r + i);
                case 4:
                    return this.sub ? "(" + this.sub.length + " elem)" : this.stream.parseOctetString(r, r + i);
                case 6:
                    return this.stream.parseOID(r, r + i);
                case 16:
                case 17:
                    return "(" + this.sub.length + " elem)";
                case 12:
                    return this.stream.parseStringUTF(r, r + i);
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 26:
                    return this.stream.parseStringISO(r, r + i);
                case 30:
                    return this.stream.parseStringBMP(r, r + i);
                case 23:
                case 24:
                    return this.stream.parseTime(r, r + i);
            }
            return null;
        }, n.prototype.toString = function () {
            return this.typeName() + "@" + this.stream.pos + "[header:" + this.header + ",length:" + this.length + ",sub:" + (null === this.sub ? "null" : this.sub.length) + "]";
        }, n.prototype.toPrettyString = function (t) {
            t === e && (t = "");
            var n = t + this.typeName() + " @" + this.stream.pos;
            0 <= this.length && (n += "+"), n += this.length, this.tag & 32 ? n += " (constructed)" : 3 != this.tag && 4 != this.tag || null === this.sub || (n += " (encapsulates)"), n += "\n";
            if (null !== this.sub) {
                t += "  ";
                for (var r = 0, i = this.sub.length; r < i; ++r)
                    n += this.sub[r].toPrettyString(t);
            }
            return n;
        }, n.prototype.posStart = function () {
            return this.stream.pos;
        }, n.prototype.posContent = function () {
            return this.stream.pos + this.header;
        }, n.prototype.posEnd = function () {
            return this.stream.pos + this.header + Math.abs(this.length);
        }, n.prototype.toHexString = function (e) {
            return this.stream.hexDump(this.posStart(), this.posEnd(), !0);
        }, n.decodeLength = function (e) {
            var t = e.get(), n = t & 127;
            if (n == t)
                return n;
            if (3 < n)
                throw "Length over 24 bits not supported at position " + (e.pos - 1);
            if (0 === n)
                return -1;
            for (var r = t = 0; r < n; ++r)
                t = t << 8 | e.get();
            return t;
        }, n.hasContent = function (e, r, i) {
            if (e & 32)
                return !0;
            if (3 > e || 4 < e)
                return !1;
            var s = new t(i);
            3 == e && s.get();
            if (s.get() >> 6 & 1)
                return !1;
            try {
                var o = n.decodeLength(s);
                return s.pos - i.pos + o == r;
            } catch (u) {
                return !1;
            }
        }, n.decode = function (e) {
            e instanceof t || (e = new t(e, 0));
            var r = new t(e), i = e.get(), s = n.decodeLength(e), o = e.pos - r.pos, u = null;
            if (n.hasContent(i, s, e)) {
                var a = e.pos;
                3 == i && e.get(), u = [];
                if (0 <= s) {
                    for (var f = a + s; e.pos < f;)
                        u[u.length] = n.decode(e);
                    if (e.pos != f)
                        throw "Content size is not correct for container starting at offset " + a;
                } else
                    try {
                        for (; ;) {
                            f = n.decode(e);
                            if (0 === f.tag)
                                break;
                            u[u.length] = f;
                        }
                        s = a - e.pos;
                    } catch (l) {
                        throw "Exception while decoding undefined length content: " + l;
                    }
            } else
                e.pos += s;
            return new n(r, o, s, i, u);
        }, window.ASNSe1 = n;
    }(), ASNSe1.prototype.getHexStringValue = function () {
        return this.toHexString().substr(2 * this.header, 2 * this.length);
    }, S.prototype.parseKey = function (e) {
        try {
            var t = 0, n = 0, r = /^\s*(?:[0-9A-Fa-f][0-9A-Fa-f]\s*)+$/.test(e) ? HexSe.decode(e) : Base64Se.unarmor(e), i = ASNSe1.decode(r);
            3 === i.sub.length && (i = i.sub[2].sub[0]);
            if (9 === i.sub.length) {
                t = i.sub[1].getHexStringValue(), this.n = E(t, 16), n = i.sub[2].getHexStringValue(), this.e = parseInt(n, 16);
                var s = i.sub[3].getHexStringValue();
                this.d = E(s, 16);
                var o = i.sub[4].getHexStringValue();
                this.p = E(o, 16);
                var u = i.sub[5].getHexStringValue();
                this.q = E(u, 16);
                var a = i.sub[6].getHexStringValue();
                this.dmp1 = E(a, 16);
                var f = i.sub[7].getHexStringValue();
                this.dmq1 = E(f, 16);
                var l = i.sub[8].getHexStringValue();
                this.coeff = E(l, 16);
            } else {
                if (2 !== i.sub.length)
                    return !1;
                var c = i.sub[1].sub[0], t = c.sub[0].getHexStringValue();
                this.n = E(t, 16), n = c.sub[1].getHexStringValue(), this.e = parseInt(n, 16);
            }
            return !0;
        } catch (h) {
            return !1;
        }
    }, S.prototype.getPublicBaseKey = function () {
        try {
            var e = { array: [new KJUR.asn1.DERObjectIdentifier({ oid: "1.2.840.113549.1.1.1" }), new KJUR.asn1.DERNull] }, t = new KJUR.asn1.DERSequence(e), e = { array: [new KJUR.asn1.DERInteger({ bigint: this.n }), new KJUR.asn1.DERInteger({ "int": this.e })] }, e = { hex: "00" + (new KJUR.asn1.DERSequence(e)).getEncodedHex() }, n = new KJUR.asn1.DERBitString(e), e = { array: [t, n] };
            return (new KJUR.asn1.DERSequence(e)).getEncodedHex();
        } catch (r) {
        }
    }, S.prototype.getPublicBaseKeyB64 = function () {
        var e = this.getPublicBaseKey(), t, n, r = "";
        for (t = 0; t + 3 <= e.length; t += 3)
            n = parseInt(e.substring(t, t + 3), 16), r += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(n >> 6) + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(n & 63);
        t + 1 == e.length ? (n = parseInt(e.substring(t, t + 1), 16), r += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(n << 2)) : t + 2 == e.length && (n = parseInt(e.substring(t, t + 2), 16), r += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(n >> 2) + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt((n & 3) << 4));
        for (; 0 < (r.length & 3) ;)
            r += "=";
        return r;
    }, S.prototype.wordwrap = function (e, t) {
        return t = t || 64, e ? e.match(RegExp("(.{1," + t + "})( +|$\n?)|(.{1," + t + "})", "g")).join("\n") : e;
    }, S.prototype.getPublicKey = function () {
        var e;
        return e = "-----BEGIN PUBLIC KEY-----\n" + (this.wordwrap(this.getPublicBaseKeyB64()) + "\n"), e + "-----END PUBLIC KEY-----";
    }, S.prototype.hasPublicKeyProperty = function (e) {
        return e = e || {}, e.hasOwnProperty("n") && e.hasOwnProperty("e");
    }, S.prototype.hasPrivateKeyProperty = function (e) {
        return e = e || {}, e.hasOwnProperty("n") && e.hasOwnProperty("e") && e.hasOwnProperty("d") && e.hasOwnProperty("p") && e.hasOwnProperty("q") && e.hasOwnProperty("dmp1") && e.hasOwnProperty("dmq1") && e.hasOwnProperty("coeff");
    }, S.prototype.parsePropertiesFrom = function (e) {
        this.n = e.n, this.e = e.e, e.hasOwnProperty("d") && (this.d = e.d, this.p = e.p, this.q = e.q, this.dmp1 = e.dmp1, this.dmq1 = e.dmq1, this.coeff = e.coeff);
    };
    var M = function (e) {
        S.call(this), e && ("string" == typeof e ? this.parseKey(e) : (this.hasPrivateKeyProperty(e) || this.hasPublicKeyProperty(e)) && this.parsePropertiesFrom(e));
    };
    M.prototype = new S, M.prototype.constructor = M, x = function (e) {
        e = e || {}, this.default_key_size = parseInt(e.default_key_size) || 1024, this.default_public_exponent = e.default_public_exponent || "010001", this.log = e.log || !1, this.key = null;
    }, x.prototype.setKey = function (e) {
        this.log && this.key && console.warn("A key was already set, overriding existing."), this.key = new M(e);
    }, x.prototype.setPrivateKey = function (e) {
        this.setKey(e);
    }, x.prototype.setPublicKey = function (e) {
        this.setKey(e);
    }, x.prototype.encrypt = function (e) {
        try {
            return this.getKey().encrypt(e);
        } catch (t) {
            return !1;
        }
    }, x.prototype.getKey = function (e) {
        if (!this.key) {
            this.key = new M;
            if (e && "[object Function]" === {}.toString.call(e)) {
                this.key.generateAsync(this.default_key_size, this.default_public_exponent, e);
                return;
            }
            this.key.generate(this.default_key_size, this.default_public_exponent);
        }
        return this.key;
    }, x.prototype.getPrivateKey = function () {
        return this.getKey().getPrivateKey();
    }, x.prototype.getPrivateKeyB64 = function () {
        return this.getKey().getPrivateBaseKeyB64();
    }, x.prototype.getPublicKey = function () {
        return this.getKey().getPublicKey();
    }, x.prototype.getPublicKeyB64 = function () {
        return this.getKey().getPublicBaseKeyB64();
    }, e.JSEncrypt = x;
})(psaJSEncExportsYwaq);
var psaRSAUtilsYwaq = psaJSEncExportsYwaq.JSEncrypt;
(function (e, t) {
    "undefined" == typeof e.psaCryptoJSywaq && (e.psaCryptoJSywaq = {}), e.psaCryptoJSywaq = t();
})(window, function () {
    var e = e || function (e, t) {
        var n = {}, r = n.lib = {}, i = r.Base = function () {
            function e() {
            }
            return {
                extend: function (t) {
                    e.prototype = this;
                    var n = new e;
                    return t && n.mixIn(t), n.hasOwnProperty("init") || (n.init = function () {
                        n.$super.init.apply(this, arguments);
                    }), n.init.prototype = n, n.$super = this, n;
                }, create: function () {
                    var e = this.extend();
                    return e.init.apply(e, arguments), e;
                }, init: function () {
                }, mixIn: function (e) {
                    for (var t in e)
                        e.hasOwnProperty(t) && (this[t] = e[t]);
                    e.hasOwnProperty("toString") && (this.toString = e.toString);
                }, clone: function () {
                    return this.init.prototype.extend(this);
                }
            };
        }(), s = r.WordArray = i.extend({
            init: function (e, n) {
                e = this.words = e || [], this.sigBytes = n != t ? n : 4 * e.length;
            }, toString: function (e) {
                return (e || u).stringify(this);
            }, concat: function (e) {
                var t = this.words, n = e.words, r = this.sigBytes;
                e = e.sigBytes, this.clamp();
                if (r % 4)
                    for (var i = 0; i < e; i++)
                        t[r + i >>> 2] |= (n[i >>> 2] >>> 24 - i % 4 * 8 & 255) << 24 - (r + i) % 4 * 8;
                else
                    for (i = 0; i < e; i += 4)
                        t[r + i >>> 2] = n[i >>> 2];
                return this.sigBytes += e, this;
            }, clamp: function () {
                var t = this.words, n = this.sigBytes;
                t[n >>> 2] &= 4294967295 << 32 - n % 4 * 8, t.length = e.ceil(n / 4);
            }, clone: function () {
                var e = i.clone.call(this);
                return e.words = this.words.slice(0), e;
            }, random: function (t) {
                for (var n = [], r = function (t) {
                    var n = 987654321;
                    return function () {
                        n = 36969 * (n & 65535) + (n >> 16) & 4294967295, t = 18e3 * (t & 65535) + (t >> 16) & 4294967295;
                        var r = (n << 16) + t & 4294967295, r = r / 4294967296 + .5;
                        return r * (.5 < e.random() ? 1 : -1);
                };
                }, i = 0, o; i < t; i += 4) {
                    var u = r(4294967296 * (o || e.random()));
                    o = 987654071 * u(), n.push(4294967296 * u() | 0);
                }
                return new s.init(n, t);
            }
        }), o = n.enc = {}, u = o.Hex = {
            stringify: function (e) {
                var t = e.words;
                e = e.sigBytes;
                for (var n = [], r = 0; r < e; r++) {
                    var i = t[r >>> 2] >>> 24 - r % 4 * 8 & 255;
                    n.push((i >>> 4).toString(16)), n.push((i & 15).toString(16));
                }
                return n.join("");
            }, parse: function (e) {
                for (var t = e.length, n = [], r = 0; r < t; r += 2)
                    n[r >>> 3] |= parseInt(e.substr(r, 2), 16) << 24 - r % 8 * 4;
                return new s.init(n, t / 2);
            }
        }, a = o.Latin1 = {
            stringify: function (e) {
                var t = e.words;
                e = e.sigBytes;
                for (var n = [], r = 0; r < e; r++)
                    n.push(String.fromCharCode(t[r >>> 2] >>> 24 - r % 4 * 8 & 255));
                return n.join("");
            }, parse: function (e) {
                for (var t = e.length, n = [], r = 0; r < t; r++)
                    n[r >>> 2] |= (e.charCodeAt(r) & 255) << 24 - r % 4 * 8;
                return new s.init(n, t);
            }
        }, f = o.Utf8 = {
            stringify: function (e) {
                try {
                    return decodeURIComponent(escape(a.stringify(e)));
                } catch (t) {
                    throw Error("Malformed UTF-8 data");
                }
            }, parse: function (e) {
                return a.parse(unescape(encodeURIComponent(e)));
            }
        };
        return r.BufferedBlockAlgorithm = i.extend({
            reset: function () {
                this._data = new s.init, this._nDataBytes = 0;
            }, _append: function (e) {
                "string" == typeof e && (e = f.parse(e)), this._data.concat(e), this._nDataBytes += e.sigBytes;
            }, _process: function (t) {
                var n = this._data, r = n.words, i = n.sigBytes, o = this.blockSize, u = i / (4 * o), u = t ? e.ceil(u) : e.max((u | 0) - this._minBufferSize, 0);
                t = u * o, i = e.min(4 * t, i);
                if (t) {
                    for (var a = 0; a < t; a += o)
                        this._doProcessBlock(r, a);
                    a = r.splice(0, t), n.sigBytes -= i;
                }
                return new s.init(a, i);
            }, clone: function () {
                var e = i.clone.call(this);
                return e._data = this._data.clone(), e;
            }, _minBufferSize: 0
        }), n.algo = {}, n;
    }(Math);
    return function (e) {
        (function () {
            function t(t) {
                for (var n = e.sqrt(t), r = 2; r <= n; r++)
                    if (!(t % r))
                        return !1;
                return !0;
            }
            function n(e) {
                return 4294967296 * (e - (e | 0)) | 0;
            }
            for (var r = 2, i = 0; 64 > i;)
                t(r) && (8 > i && n(e.pow(r, .5)), n(e.pow(r, 1 / 3)), i++), r++;
        })();
    }(Math), e.lib.Cipher || function (t) {
        var n = e;
        t = n.lib;
        var r = t.Base, i = t.WordArray, s = t.BufferedBlockAlgorithm, o = n.enc.Base64, u = t.Cipher = s.extend({
            cfg: r.extend(), createEncryptor: function (e, t) {
                return this.create(this._ENC_XFORM_MODE, e, t);
            }, init: function (e, t, n) {
                this.cfg = this.cfg.extend(n), this._xformMode = e, this._key = t, this.reset();
            }, reset: function () {
                s.reset.call(this), this._doReset();
            }, process: function (e) {
                return this._append(e), this._process();
            }, finalize: function (e) {
                return e && this._append(e), this._doFinalize();
            }, keySize: 4, ivSize: 4, _createHelper: function () {
                return function (e) {
                    return {
                        encrypt: function (t, n, r) {
                            var i;
                            return i = "string" == typeof n ? PasswordBasedCipher : f, i.encrypt(e, t, n, r);
                        }
                    };
                };
            }()
        });
        n.mode = {}, t.BlockCipherMode = r.extend({
            createEncryptor: function (e, t) {
                return this.Encryptor.create(e, t);
            }, init: function (e, t) {
                this._cipher = e, this._iv = t;
            }
        }), (n.pad = {}).Pkcs7 = {
            pad: function (e, t) {
                for (var n = 4 * t, n = n - e.sigBytes % n, r = n << 24 | n << 16 | n << 8 | n, s = [], o = 0; o < n; o += 4)
                    s.push(r);
                n = i.create(s, n), e.concat(n);
            }
        }, t.BlockCipher = u.extend({
            reset: function () {
                u.reset.call(this);
                var e = this.cfg, t = e.iv, e = e.mode;
                if (this._xformMode == this._ENC_XFORM_MODE)
                    var n = e.createEncryptor;
                else
                    n = e.createDecryptor, this._minBufferSize = 1;
                this._mode = n.call(e, this, t && t.words);
            }, _doProcessBlock: function (e, t) {
                this._mode.processBlock(e, t);
            }, _doFinalize: function () {
                var e = this.cfg.padding;
                if (this._xformMode == this._ENC_XFORM_MODE) {
                    e.pad(this._data, this.blockSize);
                    var t = this._process(!0);
                } else
                    t = this._process(!0), e.unpad(t);
                return t;
            }, blockSize: 4
        });
        var a = t.CipherParams = r.extend({
            init: function (e) {
                this.mixIn(e);
            }, toString: function (e) {
                return (e || this.formatter).stringify(this);
            }
        }), n = (n.format = {}).OpenSSL = {
            stringify: function (e) {
                var t = e.ciphertext;
                return e = e.salt, (e ? i.create([1398893684, 1701076831]).concat(e).concat(t) : t).toString(o);
            }
        }, f = t.SerializableCipher = r.extend({
            cfg: r.extend({ format: n }), encrypt: function (e, t, n, r) {
                r = this.cfg.extend(r);
                var i = e.createEncryptor(n, r);
                return t = i.finalize(t), i = i.cfg, a.create({ ciphertext: t, key: n, iv: i.iv, algorithm: e, mode: i.mode, padding: i.padding, blockSize: e.blockSize, formatter: r.format });
            }
        });
    }(), e.mode.ECB = function () {
        var t = e.lib.BlockCipherMode.extend();
        return t.Encryptor = t.extend({
            processBlock: function (e, t) {
                this._cipher.encryptBlock(e, t);
            }
        }), t;
    }(), function () {
        var t = e, n = t.lib.BlockCipher, r = t.algo, i = [], s = [], o = [], u = [], a = [], f = [], l = [], c = [], h = [];
        (function () {
            for (var e = [], t = 0; 256 > t; t++)
                e[t] = 128 > t ? t << 1 : t << 1 ^ 283;
            for (var n = 0, r = 0, t = 0; 256 > t; t++) {
                var p = r ^ r << 1 ^ r << 2 ^ r << 3 ^ r << 4, p = p >>> 8 ^ p & 255 ^ 99;
                i[n] = p;
                var d = e[n], v = e[d], m = e[v], g = 257 * e[p] ^ 16843008 * p;
                s[n] = g << 24 | g >>> 8, o[n] = g << 16 | g >>> 16, u[n] = g << 8 | g >>> 24, a[n] = g, g = 16843009 * m ^ 65537 * v ^ 257 * d ^ 16843008 * n, f[p] = g << 24 | g >>> 8, l[p] = g << 16 | g >>> 16, c[p] = g << 8 | g >>> 24, h[p] = g, n ? (n = d ^ e[e[e[m ^ d]]], r ^= e[e[r]]) : n = r = 1;
            }
        })();
        var p = [0, 1, 2, 4, 8, 16, 32, 64, 128, 27, 54], r = r.AES = n.extend({
            _doReset: function () {
                for (var e = this._key, t = e.words, n = e.sigBytes / 4, e = 4 * ((this._nRounds = n + 6) + 1), r = this._keySchedule = [], s = 0; s < e; s++)
                    if (s < n)
                        r[s] = t[s];
                    else {
                        var o = r[s - 1];
                        s % n ? 6 < n && 4 == s % n && (o = i[o >>> 24] << 24 | i[o >>> 16 & 255] << 16 | i[o >>> 8 & 255] << 8 | i[o & 255]) : (o = o << 8 | o >>> 24, o = i[o >>> 24] << 24 | i[o >>> 16 & 255] << 16 | i[o >>> 8 & 255] << 8 | i[o & 255], o ^= p[s / n | 0] << 24), r[s] = r[s - n] ^ o;
                    }
                t = this._invKeySchedule = [];
                for (n = 0; n < e; n++)
                    s = e - n, o = n % 4 ? r[s] : r[s - 4], t[n] = 4 > n || 4 >= s ? o : f[i[o >>> 24]] ^ l[i[o >>> 16 & 255]] ^ c[i[o >>> 8 & 255]] ^ h[i[o & 255]];
            }, encryptBlock: function (e, t) {
                this._doCryptBlock(e, t, this._keySchedule, s, o, u, a, i);
            }, _doCryptBlock: function (e, t, n, r, i, s, o, u) {
                for (var a = this._nRounds, f = e[t] ^ n[0], l = e[t + 1] ^ n[1], c = e[t + 2] ^ n[2], h = e[t + 3] ^ n[3], p = 4, d = 1; d < a; d++)
                    var v = r[f >>> 24] ^ i[l >>> 16 & 255] ^ s[c >>> 8 & 255] ^ o[h & 255] ^ n[p++], m = r[l >>> 24] ^ i[c >>> 16 & 255] ^ s[h >>> 8 & 255] ^ o[f & 255] ^ n[p++], g = r[c >>> 24] ^ i[h >>> 16 & 255] ^ s[f >>> 8 & 255] ^ o[l & 255] ^ n[p++], h = r[h >>> 24] ^ i[f >>> 16 & 255] ^ s[l >>> 8 & 255] ^ o[c & 255] ^ n[p++], f = v, l = m, c = g;
                v = (u[f >>> 24] << 24 | u[l >>> 16 & 255] << 16 | u[c >>> 8 & 255] << 8 | u[h & 255]) ^ n[p++], m = (u[l >>> 24] << 24 | u[c >>> 16 & 255] << 16 | u[h >>> 8 & 255] << 8 | u[f & 255]) ^ n[p++], g = (u[c >>> 24] << 24 | u[h >>> 16 & 255] << 16 | u[f >>> 8 & 255] << 8 | u[l & 255]) ^ n[p++], h = (u[h >>> 24] << 24 | u[f >>> 16 & 255] << 16 | u[l >>> 8 & 255] << 8 | u[c & 255]) ^ n[p++], e[t] = v, e[t + 1] = m, e[t + 2] = g, e[t + 3] = h;
            }, keySize: 8
        });
        t.AES = n._createHelper(r);
    }(), e;
}), function (e) {
    function t(e) {
        return "undefined" == typeof e ? "" : e;
    }
    function n() {
        var e = document.createElement("canvas"), t = null;
        try {
            t = e.getContext("webgl") || e.getContext("experimental-webgl");
        } catch (n) {
        }
        return t || (t = null), t;
    }
    function r(e) {
        var t, r = function (e) {
            return t.clearColor(0, 0, 0, 1), t.enable(t.DEPTH_TEST), t.depthFunc(t.LEQUAL), t.clear(t.COLOR_BUFFER_BIT | t.DEPTH_BUFFER_BIT), "[" + e[0] + ", " + e[1] + "]";
        }, i = function (e) {
            var t, n = e.getExtension("EXT_texture_filter_anisotropic") || e.getExtension("WEBKIT_EXT_texture_filter_anisotropic") || e.getExtension("MOZ_EXT_texture_filter_anisotropic");
            return n ? (t = e.getParameter(n.MAX_TEXTURE_MAX_ANISOTROPY_EXT), 0 === t && (t = 2), t) : null;
        };
        if (t = n()) {
            var s = t.createBuffer();
            t.bindBuffer(t.ARRAY_BUFFER, s);
            var o = new Float32Array([-0.2, -0.9, 0, .4, -0.26, 0, 0, .732134444, 0]);
            t.bufferData(t.ARRAY_BUFFER, o, t.STATIC_DRAW), s.itemSize = 3, s.numItems = 3;
            var o = t.createProgram(), a = t.createShader(t.VERTEX_SHADER);
            t.shaderSource(a, "attribute vec2 attrVertex;varying vec2 varyinTexCoordinate;uniform vec2 uniformOffset;void main(){varyinTexCoordinate=attrVertex+uniformOffset;gl_Position=vec4(attrVertex,0,1);}"), t.compileShader(a);
            var f = t.createShader(t.FRAGMENT_SHADER);
            t.shaderSource(f, "precision mediump float;varying vec2 varyinTexCoordinate;void main() {gl_FragColor=vec4(varyinTexCoordinate,0,1);}"), t.compileShader(f), t.attachShader(o, a), t.attachShader(o, f), t.linkProgram(o), t.useProgram(o), o.vertexPosAttrib = t.getAttribLocation(o, "attrVertex"), o.offsetUniform = t.getUniformLocation(o, "uniformOffset"), t.enableVertexAttribArray(o.vertexPosArray), t.vertexAttribPointer(o.vertexPosAttrib, s.itemSize, t.FLOAT, !1, 0, 0), t.uniform2f(o.offsetUniform, 1, 1), t.drawArrays(t.TRIANGLE_STRIP, 0, s.numItems), null != t.canvas && (u.webgl_id = b(t.canvas.toDataURL())), e.max_ani = i(t), e.max_cub_txt = t.getParameter(t.MAX_CUBE_MAP_TEXTURE_SIZE), e.max_fra_unif = t.getParameter(t.MAX_FRAGMENT_UNIFORM_VECTORS), e.max_rend_buf = t.getParameter(t.MAX_RENDERBUFFER_SIZE), e.max_txt_img = t.getParameter(t.MAX_TEXTURE_IMAGE_UNITS), e.max_txt_size = t.getParameter(t.MAX_TEXTURE_SIZE), e.max_var_vect = t.getParameter(t.MAX_VARYING_VECTORS), e.max_ver_att = t.getParameter(t.MAX_VERTEX_ATTRIBS), e.max_ver_txt_img = t.getParameter(t.MAX_VERTEX_TEXTURE_IMAGE_UNITS), e.max_ver_unif_vect = t.getParameter(t.MAX_VERTEX_UNIFORM_VECTORS), e.max_view_dims = r(t.getParameter(t.MAX_VIEWPORT_DIMS)), e.renderer = t.getParameter(t.RENDERER), e.shading_l_v = t.getParameter(t.SHADING_LANGUAGE_VERSION), e.vendor = t.getParameter(t.VENDOR), e.version = t.getParameter(t.VERSION), t.getShaderPrecisionFormat && (e.ver_h_f = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.HIGH_FLOAT).precision, e.ver_h_f_min = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.HIGH_FLOAT).rangeMin, e.ver_h_f_max = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.HIGH_FLOAT).rangeMax, e.ver_m_f = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.MEDIUM_FLOAT).precision, e.ver_m_f_min = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.MEDIUM_FLOAT).rangeMin, e.ver_m_f_max = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.MEDIUM_FLOAT).rangeMax, e.ver_l_f = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.LOW_FLOAT).precision, e.ver_l_f_min = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.LOW_FLOAT).rangeMin, e.ver_l_f_max = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.LOW_FLOAT).rangeMax, e.fra_h_f = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.HIGH_FLOAT).precision, e.fra_h_f_min = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.HIGH_FLOAT).rangeMin, e.fra_h_f_max = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.HIGH_FLOAT).rangeMax, e.fra_m_f = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.MEDIUM_FLOAT).precision, e.fra_m_f_min = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.MEDIUM_FLOAT).rangeMin, e.fra_m_f_max = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.MEDIUM_FLOAT).rangeMax, e.fra_l_f = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.LOW_FLOAT).precision, e.fra_l_f_min = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.LOW_FLOAT).rangeMin, e.fra_l_f_max = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.LOW_FLOAT).rangeMax, e.ver_h_i = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.HIGH_INT).precision, e.ver_h_i_min = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.HIGH_INT).rangeMin, e.ver_h_i_max = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.HIGH_INT).rangeMax, e.ver_m_i = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.MEDIUM_INT).precision, e.ver_m_i_min = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.MEDIUM_INT).rangeMin, e.ver_m_i_max = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.MEDIUM_INT).rangeMax, e.ver_l_i = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.LOW_INT).precision, e.ver_l_i_min = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.LOW_INT).rangeMin, e.ver_l_i_max = t.getShaderPrecisionFormat(t.VERTEX_SHADER, t.LOW_INT).rangeMax, e.fra_h_i = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.HIGH_INT).precision, e.fra_h_i_min = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.HIGH_INT).rangeMin, e.fra_h_i_max = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.HIGH_INT).rangeMax, e.fra_m_i = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.MEDIUM_INT).precision, e.fra_m_i_min = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.MEDIUM_INT).rangeMin, e.fra_m_i_max = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.MEDIUM_INT).rangeMax, e.fra_l_i = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.LOW_INT).precision, e.fra_l_i_min = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.LOW_INT).rangeMin, e.fra_l_i_max = t.getShaderPrecisionFormat(t.FRAGMENT_SHADER, t.LOW_INT).rangeMax);
        }
    }
    function i(e) {
        return H.lastIndex = 0, H.test(e) ? '"' + e.replace(H, function (e) {
            var t = meta[e];
            return "string" == typeof t ? t : "\\u" + ("0000" + e.charCodeAt(0).toString(16)).slice(-4);
        }) + '"' : '"' + e + '"';
    }
    function s(e, t) {
        var n, r, o, u, a = _, f, l = t[e];
        l && "object" == typeof l && "function" == typeof l.toJSON && (l = l.toJSON(e)), "function" == typeof P && (l = P.call(t, e, l));
        switch (typeof l) {
            case "string":
                return i(l);
            case "number":
                return isFinite(l) ? String(l) : "null";
            case "boolean":
            case "null":
                return String(l);
            case "object":
                if (!l)
                    return "null";
                _ += D, f = [];
                if ("[object Array]" === Object.prototype.toString.apply(l)) {
                    u = l.length;
                    for (n = 0; n < u; n += 1)
                        f[n] = s(n, l) || "null";
                    return o = 0 === f.length ? "[]" : _ ? "[\n" + _ + f.join(",\n" + _) + "\n" + a + "]" : "[" + f.join(",") + "]", _ = a, o;
                }
                if (P && "object" == typeof P)
                    for (u = P.length, n = 0; n < u; n += 1)
                        "string" == typeof P[n] && (r = P[n], (o = s(r, l)) && f.push(i(r) + (_ ? ": " : ":") + o));
                else
                    for (r in l)
                        Object.prototype.hasOwnProperty.call(l, r) && (o = s(r, l)) && f.push(i(r) + (_ ? ": " : ":") + o);
                return o = 0 === f.length ? "{}" : _ ? "{\n" + _ + f.join(",\n" + _) + "\n" + a + "}" : "{" + f.join(",") + "}", _ = a, o;
        }
    }
    if ("undefined" == typeof e.Security)
        var o = e.Security = {};
    o.eventUserName = "userName", o.eventPassWd = "pwd", o.eventCheckCode = "checkCode", o.eventFinish = "finish", o.version = "build_2.4_20161121_1618";
    var u = {}, a = 0, f = "", l = [0, 0], c = 0, h = 0, p = 0, d = 0, v = !1, m = function () {
        var e = {};
        return e[o.eventUserName] = { mouseScreenPosition: "", textClickTime: 0, textIsCopy: "false", textIsTap: "false", textKeyCount: 0 }, e[o.eventPassWd] = { mouseScreenPosition: "", textClickTime: 0, textIsCopy: "false", textIsTap: "false", textKeyCount: 0 }, e[o.eventCheckCode] = { mouseScreenPosition: "", textClickTime: 0, textIsCopy: "false", textIsTap: "false", textKeyCount: 0 }, e;
    }(), g = (new Date).getTime(), y = !0;
    u.sdk_version = "web-2.4", v = window.navigator.userAgent.indexOf("Mac OS") ? !0 : !1;
    var b = function (e) {
        function t(e, t) {
            var n, r, i, s, o;
            return i = e & 2147483648, s = t & 2147483648, n = e & 1073741824, r = t & 1073741824, o = (e & 1073741823) + (t & 1073741823), n & r ? o ^ 2147483648 ^ i ^ s : n | r ? o & 1073741824 ? o ^ 3221225472 ^ i ^ s : o ^ 1073741824 ^ i ^ s : o ^ i ^ s;
        }
        function n(e, n, r, i, s, o, u) {
            return e = t(e, t(t(n & r | ~n & i, s), u)), t(e << o | e >>> 32 - o, n);
        }
        function r(e, n, r, i, s, o, u) {
            return e = t(e, t(t(n & i | r & ~i, s), u)), t(e << o | e >>> 32 - o, n);
        }
        function i(e, n, r, i, s, o, u) {
            return e = t(e, t(t(n ^ r ^ i, s), u)), t(e << o | e >>> 32 - o, n);
        }
        function s(e, n, r, i, s, o, u) {
            return e = t(e, t(t(r ^ (n | ~i), s), u)), t(e << o | e >>> 32 - o, n);
        }
        function o(e) {
            var t = "", n = "", r;
            for (r = 0; 3 >= r; r++)
                n = e >>> 8 * r & 255, n = "0" + n.toString(16), t += n.substr(n.length - 2, 2);
            return t;
        }
        var u = [], a, f, l, c, h, p, d, v;
        e = function (e) {
            e = e.replace(/\r\n/g, "\n");
            for (var t = "", n = 0; n < e.length; n++) {
                var r = e.charCodeAt(n);
                128 > r ? t += String.fromCharCode(r) : (127 < r && 2048 > r ? t += String.fromCharCode(r >> 6 | 192) : (t += String.fromCharCode(r >> 12 | 224), t += String.fromCharCode(r >> 6 & 63 | 128)), t += String.fromCharCode(r & 63 | 128));
            }
            return t;
        }(e), u = function (e) {
            var t, n = e.length;
            t = n + 8;
            for (var r = 16 * ((t - t % 64) / 64 + 1), i = Array(r - 1), s = 0, o = 0; o < n;)
                t = (o - o % 4) / 4, s = o % 4 * 8, i[t] |= e.charCodeAt(o) << s, o++;
            return t = (o - o % 4) / 4, i[t] |= 128 << o % 4 * 8, i[r - 2] = n << 3, i[r - 1] = n >>> 29, i;
        }(e), h = 1732584193, p = 4023233417, d = 2562383102, v = 271733878;
        for (e = 0; e < u.length; e += 16)
            a = h, f = p, l = d, c = v, h = n(h, p, d, v, u[e + 0], 7, 3614090360), v = n(v, h, p, d, u[e + 1], 12, 3905402710), d = n(d, v, h, p, u[e + 2], 17, 606105819), p = n(p, d, v, h, u[e + 3], 22, 3250441966), h = n(h, p, d, v, u[e + 4], 7, 4118548399), v = n(v, h, p, d, u[e + 5], 12, 1200080426), d = n(d, v, h, p, u[e + 6], 17, 2821735955), p = n(p, d, v, h, u[e + 7], 22, 4249261313), h = n(h, p, d, v, u[e + 8], 7, 1770035416), v = n(v, h, p, d, u[e + 9], 12, 2336552879), d = n(d, v, h, p, u[e + 10], 17, 4294925233), p = n(p, d, v, h, u[e + 11], 22, 2304563134), h = n(h, p, d, v, u[e + 12], 7, 1804603682), v = n(v, h, p, d, u[e + 13], 12, 4254626195), d = n(d, v, h, p, u[e + 14], 17, 2792965006), p = n(p, d, v, h, u[e + 15], 22, 1236535329), h = r(h, p, d, v, u[e + 1], 5, 4129170786), v = r(v, h, p, d, u[e + 6], 9, 3225465664), d = r(d, v, h, p, u[e + 11], 14, 643717713), p = r(p, d, v, h, u[e + 0], 20, 3921069994), h = r(h, p, d, v, u[e + 5], 5, 3593408605), v = r(v, h, p, d, u[e + 10], 9, 38016083), d = r(d, v, h, p, u[e + 15], 14, 3634488961), p = r(p, d, v, h, u[e + 4], 20, 3889429448), h = r(h, p, d, v, u[e + 9], 5, 568446438), v = r(v, h, p, d, u[e + 14], 9, 3275163606), d = r(d, v, h, p, u[e + 3], 14, 4107603335), p = r(p, d, v, h, u[e + 8], 20, 1163531501), h = r(h, p, d, v, u[e + 13], 5, 2850285829), v = r(v, h, p, d, u[e + 2], 9, 4243563512), d = r(d, v, h, p, u[e + 7], 14, 1735328473), p = r(p, d, v, h, u[e + 12], 20, 2368359562), h = i(h, p, d, v, u[e + 5], 4, 4294588738), v = i(v, h, p, d, u[e + 8], 11, 2272392833), d = i(d, v, h, p, u[e + 11], 16, 1839030562), p = i(p, d, v, h, u[e + 14], 23, 4259657740), h = i(h, p, d, v, u[e + 1], 4, 2763975236), v = i(v, h, p, d, u[e + 4], 11, 1272893353), d = i(d, v, h, p, u[e + 7], 16, 4139469664), p = i(p, d, v, h, u[e + 10], 23, 3200236656), h = i(h, p, d, v, u[e + 13], 4, 681279174), v = i(v, h, p, d, u[e + 0], 11, 3936430074), d = i(d, v, h, p, u[e + 3], 16, 3572445317), p = i(p, d, v, h, u[e + 6], 23, 76029189), h = i(h, p, d, v, u[e + 9], 4, 3654602809), v = i(v, h, p, d, u[e + 12], 11, 3873151461), d = i(d, v, h, p, u[e + 15], 16, 530742520), p = i(p, d, v, h, u[e + 2], 23, 3299628645), h = s(h, p, d, v, u[e + 0], 6, 4096336452), v = s(v, h, p, d, u[e + 7], 10, 1126891415), d = s(d, v, h, p, u[e + 14], 15, 2878612391), p = s(p, d, v, h, u[e + 5], 21, 4237533241), h = s(h, p, d, v, u[e + 12], 6, 1700485571), v = s(v, h, p, d, u[e + 3], 10, 2399980690), d = s(d, v, h, p, u[e + 10], 15, 4293915773), p = s(p, d, v, h, u[e + 1], 21, 2240044497), h = s(h, p, d, v, u[e + 8], 6, 1873313359), v = s(v, h, p, d, u[e + 15], 10, 4264355552), d = s(d, v, h, p, u[e + 6], 15, 2734768916), p = s(p, d, v, h, u[e + 13], 21, 1309151649), h = s(h, p, d, v, u[e + 4], 6, 4149444226), v = s(v, h, p, d, u[e + 11], 10, 3174756917), d = s(d, v, h, p, u[e + 2], 15, 718787259), p = s(p, d, v, h, u[e + 9], 21, 3951481745), h = t(h, a), p = t(p, f), d = t(d, l), v = t(v, c);
        return (o(h) + o(p) + o(d) + o(v)).toLowerCase();
    };
    try {
        var w = window.navigator;
        w && (u.appVersion = t(w.appVersion), u.appCodeName = t(w.appCodeName), u.appName = t(w.appName), u.platform = t(w.platform), u.userAgent = t(w.userAgent), u.cpuClass = t(w.cpuClass), u.browserLanguage = t(w.browserLanguage), u.systemLanguage = t(w.systemLanguage), u.userLanguage = t(w.userLanguage), u.language = t(w.language), u.product = t(w.product), u.productSub = t(w.productSub), u.vendor = t(w.vendor), u.javaEnabled = t(w.javaEnabled()), u.doNotTrack = w.doNotTrack ? navigator.doNotTrack : ""), u.maxTouchPoints = function () {
            var e = 0;
            return "undefined" != typeof window.navigator.maxTouchPoints ? e = window.navigator.maxTouchPoints : "undefined" != typeof window.navigator.msMaxTouchPoints && (e = window.navigator.msMaxTouchPoints), e;
        }(), u.TouchEvent = function () {
            var e = !1;
            try {
                e = "ontouchend" in document ? !0 : !1;
            } catch (t) {
            }
            return e;
        }(), u.cookiedsEnabled = function () {
            var e;
            return e = window.navigator.cookieEnabled ? !0 : !1, "undefined" != typeof navigator.cookieEnabled || e || (document.cookie = "testCookie", e = -1 !== document.cookie.indexOf("testCookie") ? !0 : !1), e;
        }();
    } catch (E) {
    }
    try {
        var S = window.screen;
        S && (u.screenWH = S.width + ":" + S.height, u.availableWH = S.availWidth + ":" + S.availHeight, u.colorDepth = S.colorDepth, u.pixelDepth = S.pixelDepth);
        var x = document.documentElement || document.body;
        x && (u.windowPosition = t(window.screenLeft) + ":" + t(window.screenTop), u.windowOuterWH = t(window.outerWidth) + ":" + t(window.outerHeight), u.windowInnerWH = x.clientWidth + ":" + x.clientHeight);
    } catch (E) {
    }
    try {
        var T = new Date;
        if (T) {
            var N = T.getTimezoneOffset();
            N && (u.timeZone = N / 60 * -1), u.currentTime = function () {
                var e = T.getFullYear(), t = T.getMonth() + 1;
                10 > t && (t = "0" + t);
                var n = T.getDate();
                10 > n && (n = "0" + n);
                var r = T.getHours();
                10 > r && (r = "0" + r);
                var i = T.getMinutes();
                10 > i && (i = "0" + i);
                var s = T.getSeconds();
                return 10 > s && (s = "0" + s), e + "-" + t + "-" + n + " " + r + ":" + i + ":" + s;
            }();
        }
    } catch (E) {
    }
    u.navigatorPluginList = function () {
        var e = function (e) {
            for (var t = !1, n = 0, n = 0; n < this.hashtable.length; n += 1)
                if (n === e && null !== this.hashtable[n]) {
                    t = !0;
                    break;
                }
            return t;
        }, t = function (e) {
            return this.hashtable[e];
        }, n = function () {
            var e = [], t = 0;
            for (t in this.hashtable)
                null !== this.hashtable[t] && e.push(t);
            return e;
        }, r = function (e, t) {
            if (null === e || null === t)
                throw "NullPointerException {" + e + "},{" + t + "}";
            this.hashtable[e] = t;
        }, i = function () {
            var e = 0, t = 0;
            for (t in this.hashtable)
                null !== this.hashtable[t] && (e += 1);
            return e;
        }, s = function () {
            this.containsKey = e, this.get = t, this.keys = n, this.put = r, this.size = i, this.hashtable = [];
        };
        u.hasTLS = /^[Hh][Tt][Tt][Pp][Ss]/.test(window.location.protocol) ? !0 : !1;
        var o = function (e) {
            var t = "";
            try {
                return t = document.body.getComponentVersion("{" + e + "}", "ComponentID"), null !== t ? t : !1;
            } catch (n) {
            }
        };
        return function () {
            var e, t, n, r, i, u = [];
            try {
                e = new s, e.put("7790769C-0471-11D2-AF11-00C04FA35D02", "AddressBook"), e.put("47F67D00-9E55-11D1-BAEF-00C04FC2D130", "AolArtFormat"), e.put("76C19B38-F0C8-11CF-87CC-0020AFEECF20", "ArabicDS"), e.put("76C19B34-F0C8-11CF-87CC-0020AFEECF20", "ChineseSDS"), e.put("76C19B33-F0C8-11CF-87CC-0020AFEECF20", "ChineseTDS"), e.put("238F6F83-B8B4-11CF-8771-00A024541EE3", "CitrixICA"), e.put("283807B5-2C60-11D0-A31D-00AA00B92C03", "DirectAnim"), e.put("44BBA848-CC51-11CF-AAFA-00AA00B6015C", "DirectShow"), e.put("9381D8F2-0288-11D0-9501-00AA00B911A5", "DynHTML"), e.put("4F216970-C90C-11D1-B5C7-0000F8051515", "DynHTML4Java"), e.put("D27CDB6E-AE6D-11CF-96B8-444553540000", "Flash"), e.put("76C19B36-F0C8-11CF-87CC-0020AFEECF20", "HebrewDS"), e.put("630B1DA0-B465-11D1-9948-00C04F98BBC9", "IEBrwEnh"), e.put("08B0E5C0-4FCB-11CF-AAA5-00401C608555", "IEClass4Java"), e.put("45EA75A0-A269-11D1-B5BF-0000F8051515", "IEHelp"), e.put("DE5AED00-A4BF-11D1-9948-00C04F98BBC9", "IEHelpEng"), e.put("89820200-ECBD-11CF-8B85-00AA005B4383", "IE5WebBrw"), e.put("5A8D6EE0-3E18-11D0-821E-444553540000", "InetConnectionWiz"), e.put("76C19B30-F0C8-11CF-87CC-0020AFEECF20", "JapaneseDS"), e.put("76C19B31-F0C8-11CF-87CC-0020AFEECF20", "KoreanDS"), e.put("76C19B50-F0C8-11CF-87CC-0020AFEECF20", "LanguageAS"), e.put("08B0E5C0-4FCB-11CF-AAA5-00401C608500", "MsftVM"), e.put("5945C046-LE7D-LLDL-BC44-00C04FD912BE", "MSNMessengerSrv"), e.put("44BBA842-CC51-11CF-AAFA-00AA00B6015B", "NetMeetingNT"), e.put("3AF36230-A269-11D1-B5BF-0000F8051515", "OfflineBrwPack"), e.put("44BBA840-CC51-11CF-AAFA-00AA00B6015C", "OutlookExpress"), e.put("76C19B32-F0C8-11CF-87CC-0020AFEECF20", "PanEuropeanDS"), e.put("4063BE15-3B08-470D-A0D5-B37161CFFD69", "QuickTime"), e.put("DE4AF3B0-F4D4-11D3-B41A-0050DA2E6C21", "QuickTimeCheck"), e.put("3049C3E9-B461-4BC5-8870-4C09146192CA", "RealPlayer"), e.put("2A202491-F00D-11CF-87CC-0020AFEECF20", "ShockwaveDir"), e.put("3E01D8E0-A72B-4C9F-99BD-8A6E7B97A48D", "Skype"), e.put("CC2A9BA0-3BDD-11D0-821E-444553540000", "TaskScheduler"), e.put("76C19B35-F0C8-11CF-87CC-0020AFEECF20", "ThaiDS"), e.put("3BF42070-B3B1-11D1-B5C5-0000F8051515", "Uniscribe"), e.put("4F645220-306D-11D2-995D-00C04F98BBC9", "VBScripting"), e.put("76C19B37-F0C8-11CF-87CC-0020AFEECF20", "VietnameseDS"), e.put("10072CEC-8CC1-11D1-986E-00A0C955B42F", "VML"), e.put("90E2BA2E-DD1B-4CDE-9134-7A8B86D33CA7", "WebEx"), e.put("73FA19D0-2D75-11D2-995D-00C04F98BBC9", "WebFolders"), e.put("89820200-ECBD-11CF-8B85-00AA005B4340", "WinDesktopUpdateNT"), e.put("9030D464-4C02-4ABF-8ECC-5164760863C6", "WinLive"), e.put("6BF52A52-394A-11D3-B153-00C04F79FAA6", "WinMediaPlayer"), e.put("22D6F312-B0F6-11D0-94AB-0080C74C7E95", "WinMediaPlayerTrad");
                if (0 < navigator.plugins.length)
                    for (r = 0; r < navigator.plugins.length; r += 1)
                        u.push(navigator.plugins[r].name);
                else if (0 < navigator.mimeTypes.length)
                    for (i = navigator.mimeTypes, r = 0; r < i.length; r += 1)
                        u.push(i[r].description);
                else {
                    if (Object.getOwnPropertyDescriptor && Object.getOwnPropertyDescriptor(window, "ActiveXObject") || "ActiveXObject" in window)
                        u = this.map("AcroPDF.PDF;Adodb.Stream;AgControl.AgControl;DevalVRXCtrl.DevalVRXCtrl.1;MacromediaFlashPaper.MacromediaFlashPaper;Msxml2.DOMDocument;Msxml2.XMLHTTP;PDF.PdfCtrl;QuickTime.QuickTime;QuickTimeCheckObject.QuickTimeCheck.1;RealPlayer;RealPlayer.RealPlayer(tm) ActiveX Control (32-bit);RealVideo.RealVideo(tm) ActiveX Control (32-bit);Scripting.Dictionary;SWCtl.SWCtl;Shell.UIHelper;ShockwaveFlash.ShockwaveFlash;Skype.Detection;TDCCtl.TDCCtl;WMPlayer.OCX;rmocx.RealPlayer G2 Control;rmocx.RealPlayer G2 Control.1".split(";"), function (e) {
                            try {
                                return new ActiveXObject(e), e;
                            } catch (t) {
                                return null;
                            }
                        });
                    this.document.body.addBehavior("#default#clientCaps"), t = e.keys();
                    for (r = 0; r < e.size() ; r += 1)
                        o(t[r]), (n = e.get(t[r])) && u.push(n);
                }
                return u;
            } catch (a) {
            }
        }();
    }();
    try {
        u.naPluginListHash = b(u.navigatorPluginList.join(""));
    } catch (E) {
    }
    var C = function () {
        var e = function (e, t, n) {
            e = e.toString(16).toUpperCase();
            for (var r = [], i = "", s = 0, s = 0; s < e.length; s++)
                r.push(e.substring(s, s + 1));
            for (s = Math.floor(t / 4) ; s <= Math.floor(n / 4) ; s++)
                i = r[s] && "" != r[s] ? i + r[s] : i + "0";
            return i;
        }, t = function (e) {
            return Math.floor(Math.random() * (e + 1));
        };
        return function () {
            var n = new Date(1582, 10, 15, 0, 0, 0, 0), r = (new Date).getTime() - n.getTime(), n = e(r, 0, 31), i = e(r, 32, 47), r = e(r, 48, 59) + "2", s = e(t(4095), 0, 7), o = e(t(4095), 0, 7), u = e(t(8191), 0, 7) + e(t(8191), 8, 15) + e(t(8191), 0, 7) + e(t(8191), 8, 15) + e(t(8191), 0, 15);
            return n + i + r + s + o + u;
        }();
    }(), k = function (e) {
        var t = 65535, n;
        try {
            for (var r = 0; r < e.length; r++)
                for (var i = e.charCodeAt(r), s = 0; 8 > s; s++) {
                    var o = 1 == (i >> 7 - s & 1), u = 1 == (t >> 15 & 1), t = t << 1;
                    u ^ o && (t ^= 4129);
                }
            n = (t & 65535).toString(16);
            for (var a = 4 - n.length; 0 < a;)
                n = "0" + n, a--;
            return n;
        } catch (f) {
        }
    };
    try {
        var L = { deKey: "pcSysTxt", timeKey: "timeTxt", deviceValue: C + k(C), timeValue: Math.floor((new Date).getTime() / 1e3) };
    } catch (E) {
    }
    (function (e) {
        var t = e.deKey, n = e.timeKey, r = e.deviceValue;
        e = e.timeValue;
        var i = {}, s = function (e, t) {
            var n, r, i, s, o, u = "", a = "";
            try {
                document.cookie.length && (n = document.cookie.indexOf(e + "="), -1 != n && (n = n + e.length + 1, r = document.cookie.indexOf(";", n), -1 == r && (r = document.cookie.length), o = document.cookie.substring(n, r)), i = document.cookie.indexOf(t + "="), -1 != i && (i = i + t.length + 1, s = document.cookie.indexOf(";", i), -1 == s && (s = document.cookie.length), a = document.cookie.substring(i, s)));
                if (o) {
                    var f = o.substring(o.length - 4), l = o.substring(0, o.length - 4);
                    f == k(l) && (u = l);
                }
                return { deviceKey: u, timeKey: a };
            } catch (c) {
            }
        }, o = function (e, t, n, r) {
            try {
                window.navigator.cookieEnabled && (e = e + "=" + t, e += ";expires=" + (new Date((new Date).getTime() + 31536e4)).toGMTString() + "; path=/", document.cookie = e, n = n + "=" + r, n += ";expires=" + (new Date((new Date).getTime() + 31536e4)).toGMTString() + "; path=/", document.cookie = n);
            } catch (i) {
            }
        }, a = function (e, t) {
            var n = "", r = "", i = "";
            try {
                var s = window.sessionStorage;
                if (s && s.length)
                    var o = s.getItem(e), r = s.getItem(t) || "", i = o || "";
                if (i) {
                    var u = i.substring(i.length - 4), a = i.substring(0, i.length - 4);
                    u == k(a) && (n = a);
                }
                return { deviceKey: n, timeKey: r };
            } catch (f) {
            }
        }, f = function (e, t, n, r) {
            try {
                var i = window.sessionStorage;
                i && (i.setItem(e, t), i.setItem(n, r));
            } catch (s) {
            }
        }, l = function (e, t) {
            var n = "", r = "", i = "";
            try {
                var s = window.localStorage;
                s && (i = s[e] || "", r = s[t] || "");
                if (i) {
                    var o = i.substring(i.length - 4), u = i.substring(0, i.length - 4);
                    o == k(u) && (n = u);
                }
                return { deviceKey: n, timeKey: r };
            } catch (a) {
            }
        }, c = function (e, t, n, r) {
            try {
                var i = window.localStorage;
                i && (i[e] = t, i[n] = r);
            } catch (s) {
            }
        }, h = "", p = "", d = "", v = "", m = "", g = "";
        try {
            h = s(t, n).deviceKey, p = a(t, n).deviceKey, d = l(t, n).deviceKey, v = s(t, n).timeKey, m = a(t, n).timeKey, g = l(t, n).timeKey;
        } catch (y) {
        }
        s = h || p || d || "" || "", a = v || m || g || "" || "";
        if (s && a) {
            i.deviceKey = s, i.timeKey = a;
            try {
                if (!h || !v) {
                    var b = s + k(s);
                    o(t, b, n, a);
                }
                if (!p || !m) {
                    var w = s + k(s);
                    f(t, w, n, a);
                }
                if (!d || !g) {
                    var E = s + k(s);
                    c(t, E, n, a);
                }
            } catch (y) {
            }
            try {
                k(s);
            } catch (y) {
            }
            try {
                k(s);
            } catch (y) {
            }
        } else {
            try {
                o(t, r, n, e), f(t, r, n, e), c(t, r, n, e);
            } catch (y) {
            }
            try {
                i.deviceKey = r.substring(0, r.length - 4), i.timeKey = e;
            } catch (y) {
            }
        }
        return u.token = u.deviceId = i.deviceKey || "", u.token_ctime = i.timeKey || "", i;
    })(L), u.canvasID = function () {
        var e = null, t = null, n = null;
        try {
            return e = document.createElement("canvas"), t = e.getContext("2d"), t.textBaseline = "top", t.font = "14px 'Arial'", t.textBaseline = "alphabetic", t.fillStyle = "#f60", t.fillRect(125, 1, 62, 20), t.fillStyle = "#069", t.fillText("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ`~1!2@3#4$5%6^7&8*9(0)-_=+[{]}|;:',<.>/?", 2, 15), t.fillStyle = "rgba(102, 204, 0, 0.7)", t.fillText("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ`~1!2@3#4$5%6^7&8*9(0)-_=+[{]}|;:',<.>/?", 4, 17), n = b(e.toDataURL());
        } catch (r) {
        }
    }(), u.webgl = function () {
        var e = { max_ani: 0, max_cub_txt: 0, max_fra_unif: 0, max_rend_buf: 0, max_txt_img: 0, max_txt_size: 0, max_var_vect: 0, max_ver_att: 0, max_ver_txt_img: 0, max_ver_unif_vect: 0, max_view_dims: "", renderer: "", shading_l_v: "", vendor: "", version: "", ver_h_f: 0, ver_h_f_min: 0, ver_h_f_max: 0, ver_m_f: 0, ver_m_f_min: 0, ver_m_f_max: 0, ver_l_f: 0, ver_l_f_min: 0, ver_l_f_max: 0, fra_h_f: 0, fra_h_f_min: 0, fra_h_f_max: 0, fra_m_f: 0, fra_m_f_min: 0, fra_m_f_max: 0, fra_l_f: 0, fra_l_f_min: 0, fra_l_f_max: 0, ver_h_i: 0, ver_h_i_min: 0, ver_h_i_max: 0, ver_m_i: 0, ver_m_i_min: 0, ver_m_i_max: 0, ver_l_i: 0, ver_l_i_min: 0, ver_l_i_max: 0, fra_h_i: 0, fra_h_i_min: 0, fra_h_i_max: 0, fra_m_i: 0, fra_m_i_max: 0, fra_m_i_min: 0, fra_l_i: 0, fra_l_i_min: 0, fra_l_i_max: 0 }, t, n = document.createElement("canvas");
        if (n.getContext && n.getContext("2d")) {
            n = document.createElement("canvas");
            try {
                t = n.getContext && (n.getContext("webgl") || n.getContext("experimental-webgl"));
            } catch (i) {
                t = !1;
            }
            t = !!window.WebGLRenderingContext && !!t;
        } else
            t = !1;
        if (!t)
            return e;
        try {
            r(e);
        } catch (i) {
        }
        return e;
    }(), u.hasAdBlock = function () {
        var e = document.createElement("div");
        e.innerHTML = "&nbsp;", e.className = "adsbox";
        var t = !1;
        try {
            document.body.appendChild(e), t = 0 === document.getElementsByClassName("adsbox")[0].offsetHeight, document.body.removeChild(e);
        } catch (n) {
            t = !1;
        }
        return t;
    }();
    var A = function (e) {
        0 != e.length % 2 && (e = "0" + e);
        for (var t = 0, n = e.length / 2, r = [], i = 0; i < n; i++) {
            for (var s = e.substr(t, 2), s = e.substr(t, 2), o = parseInt(s, 16).toString(2), u = 8 - o.length; 0 < u;)
                o = "0" + o, u--;
            u = 0, u = 1 == o[0] ? -((parseInt(s, 16) ^ 255) + 1) : parseInt(s, 16), r.push(u), t += 2;
        }
        return r;
    }, O = function (e) {
        for (var t = [], n = 0; 4 > n; n++) {
            for (var r = (e & 255).toString(2), i = 8 - r.length; 0 < i;)
                r = "0" + r, i--;
            i = 0, i = 1 == r[0] ? -((parseInt(r, 2) ^ 255) + 1) : parseInt(r, 2), t.unshift(i), e >>= 8;
        }
        return t;
    }, M = function (e) {
        var t = C.substring(0, 16);
        try {
            try {
                var n = psaCryptoJSywaq.enc.Utf8.parse(t), r = psaCryptoJSywaq.AES.encrypt(e, n, { mode: psaCryptoJSywaq.mode.ECB, padding: psaCryptoJSywaq.pad.Pkcs7 }).ciphertext.toString();
            } catch (i) {
                return "";
            }
            var s = A(r);
            try {
                var o = new psaRSAUtilsYwaq;
                o.setPublicKey("MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQChrjU60beCTmSQ84T0B3AYAg8y +9xh3oK3Ay9JvD+GnXAzffa7poSQ4xYYvO96yqG+K8sDSDI13IjKiP/V36rONGO94U/X59EqXDhgyRLM14hsyiUAtzEstf0W/+uP1KALP8lTEM0O6MwhC/zx+Ju921FO3tJIZtwn8S8E12EPzQIDAQAB");
                var u = o.encrypt(t).toString(16);
                if ("false" == u)
                    return "";
            } catch (i) {
                return "";
            }
            var a = A(u), f = O(a.length);
            e = [], e = e.concat(f).concat(a).concat(s);
            var l;
            e:
                {
                    var s = e, a = 65535, c;
                    null == s && (a &= 65535, c = a.toString(16));
                    try {
                        for (f = 0; f < s.length; f++)
                            for (var h = s[f], t = 0; 8 > t; t++)
                                n = 1 == (h >> 7 - t & 1), r = 1 == (a >> 15 & 1), a <<= 1, r ^ n && (a ^= 4129);
                        c = (a & 65535).toString(16);
                        for (var p = 4 - c.length; 0 < p;)
                            c = "0" + c, p--;
                        l = c;
                        break e;
                    } catch (i) {
                    }
                    l = void 0;
                }
            var d = A(l), v = O(e.length);
            l = [];
            var d = l = l.concat(v).concat(d).concat(e), v = "", m = d.length;
            l = 0;
            for (var g, y, b; l < m;) {
                g = d[l++] & 255;
                if (l == m) {
                    v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(g >>> 2), v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt((g & 3) << 4), v += "==";
                    break;
                }
                y = d[l++] & 255;
                if (l == m) {
                    v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(g >>> 2), v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt((g & 3) << 4 | (y & 240) >>> 4), v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt((y & 15) << 2), v += "=";
                    break;
                }
                b = d[l++] & 255, v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(g >>> 2), v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt((g & 3) << 4 | (y & 240) >>> 4), v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt((y & 15) << 2 | (b & 192) >>> 6), v += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".charAt(b & 63);
            }
        } catch (i) {
            return "";
        }
        return v;
    };
    (function () {
        try {
            void 0 != document.addEventListener && document.addEventListener("mousemove", function (e) {
                try {
                    l[0] = Math.round(e.clientX), l[1] = Math.round(e.clientY);
                } catch (t) {
                }
            });
        } catch (e) {
        }
    })(), function () {
        try {
            void 0 != document.addEventListener && document.addEventListener("keydown", function (e) {
                c++;
                try {
                    v ? e.metaKey && 86 == e.keyCode ? (p++, "" != f && (m[f].textIsCopy = "true")) : 9 == e.keyCode ? (d++, "" != f && (m[f].textIsTap = "true")) : h++ : e.ctrlKey && 86 == e.keyCode ? (p++, "" != f && (m[f].textIsCopy = "true")) : 9 == e.keyCode ? (d++, "" != f && (m[f].textIsTap = "true")) : h++;
                } catch (t) {
                }
            });
        } catch (e) {
        }
    }();
    var _, D, P, H = /[\\"\u0000-\u001f\u007f-\u009f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, B = function (e, t, n, r, i) {
        var o, a = "", f = (new Date).getTime();
        try {
            u.intervalTime = f - g, u.template = e || "", u.eventType = t || "", u.clientNo = n || "", u.cellno = r || "", u.clientIp = i || "", u.eventMsg = m, u.totalKeyCount = c, u.totalCopyCount = p, u.totalTapCount = d, "undefined" == typeof JSON ? (e = u, D = _ = "", P = void 0, o = s("", { "": e })) : o = JSON.stringify(u), a = M(o);
        } catch (l) {
        }
        return a;
    };
    o.registerValue = function (e, t, n, r) {
        return B(e, "register", t, n, r);
    }, o.loginValue = function (e, t, n, r) {
        return B(e, "login", t, n, r);
    }, o.getDeviceAllInfo = function (e, t, n, r) {
        return B(e, "dev_all_msg", t, n, r);
    }, o.getDeviceAllInfo = function () {
        return B("", "dev_all_msg", "", "", "");
    }, o.setEventMsg = function (e) {
        if (y) {
            var t = (new Date).getTime();
            u.firstInputTime = t - g, g = t, y = !1;
        }
        try {
            if (void 0 == e || "" == e)
                f = "";
            else if (e == o.eventFinish) {
                if ("" != f) {
                    var n = (new Date).getTime() - a;
                    m[f].textClickTime = n, m[f].textKeyCount = h;
                }
                f = "";
            } else
                a = (new Date).getTime(), f = e, m[f].mouseScreenPosition = l[0] + ":" + l[1];
            h = 0;
        } catch (r) {
        }
    };
}(window), define("psaWebAccountJS.min", function () {
}), define("mod/home/view/home", ["require", "exports", "module", "text!../template/home.html", "../model/home", "./loginOtp", "./loginCert", "../../../component/ad/ad", "psaWebAccountJS.min"], function (e, t, n) {
    var r = e("text!../template/home.html"), i = e("../model/home"), s = e("./loginOtp"), o = e("./loginCert"), u = e("../../../component/ad/ad");
    e("psaWebAccountJS.min"), n.exports = ItemView.extend({
        template: _.template(r), events: { "click #login_btn": "loginSubmit", "focus #userName": "focus_userName", "focus #passWord": "focus_passWord", "click #vCodeImgParent": "changeCode", "click #changeCode_Btn": "changeCode", "focus #verifyCode": "focus_verifyCode", "click #forgetUsername": "_forgetUsername", "click #forgetPassword": "_forgetPassword", "click #register": "_register", "keydown #verifyCode": "submitEnter" }, ui: { forgetUsername: "#forgetUsername", forgetPassword: "#forgetPassword" }, initialize: function (e) {
            this.controller = e.controller, this.pwdInput = App.commonView.getPasswordControll({ width: 258, keyType: "normal", minLength: 8, maxLength: 16, accepts: "[:graph:]+" });
        }, onRender: function () {
            document.title = "平安银行个人网银", utils.webTrends.businessStepTalkingData({ paramData: "个人网银登录", businessStep: "登录-起始页" }), this.$("#passWord").prepend(this.pwdInput.render().el), this._showVerifyCode(), $("#serviceContainer").empty(), this.$("input").placeholder();
            var e = { dom: this.$("#adPaly"), adAry: ["QY16111814565438", "QY16111814570439", "QY16111814570940", "QY16111814571441", "QY16111814571842", "QY16111814572343"], styles: { width: "440px", height: "380px" }, dotsType: "1" }, t = new u(e);
        }, onShow: function () {
            setTimeout(_.bind(this._findNameFlag, this), 300);
        }, _findNameFlag: function () {
            var e = this;
            App.commonView.getConfigValues({
                success: function (t) {
                    t.user_info_find_new_channel == "on" && (e.ui.forgetUsername.prop("href", "#findUserName/findName/index"), e.ui.forgetUsername.prop("target", "_self"), e.ui.forgetPassword.prop("href", "#findPwd/findPassword/index"), e.ui.forgetPassword.prop("target", "_self"));
                }
            });
        }, createImg: function () {
            var e = $('<img style="width:80px;height:36px;boder:0px" class="cur_po">');
            e[0].onload = function () {
                e.show(), $("#vCodeImgParent").empty().append(e), $("#vCodeImgParent").removeClass("loading_img");
            }, this.changeCode(e);
        }, changeCode: function (e) {
            var t = e.attr ? e : $("#vCodeImgParent").children();
            t.hide(), t.prop("src", ""), t.parent().addClass("loading_img"), t.prop("src", config.urls.get("vCode") + "?" + Date.parse(new Date));
        }, _forgetUsername: function () {
            utils.webTrends.talkingDataSucc({ paramData: "登录页-忘记用户名" });
        }, _forgetPassword: function () {
            utils.webTrends.talkingDataSucc({ paramData: "登录页-忘记密码" });
        }, _register: function () {
            utils.webTrends.talkingDataSucc({ paramData: "登录页-注册" });
        }, _showVerifyCode: function () {
            var e = this;
            i.showVerifyCode({
                param: {}, success: function (t) {
                    var n = t.responseBody.vcodeShowFlag;
                    n = "1";
                    n == "1" ? (e.$("#finalInsure").css("visibility", "hidden"), e.$("#insureVerifyCode").css("display", "block"), e.$("#verifyCode").val(""), e.createImg()) : n == "0" && (e.$("#finalInsure").css("visibility", "hidden"), e.$("#insureVerifyCode").css("display", "none"));
                }, error: function (e) {
                }
            });
        }, loginSubmit: function () {
            utils.webTrends.talkingDataSucc({ paramData: "登录页-登陆" });
            var e = this, t = $("#userName").val(), n = this.pwdInput.getPasswordInput().password, r = { userName: t, passWord: n };
            t = "test";
            if (!t)
            {
                $("#userName").addClass("focus-red"), $("#userNameInsure").removeClass("vh"), $("#userNameError").text("请输入一账通用户名/身份证号");
            }
            else if (!this.pwdInput.getPasswordInput().success)
                $("#passWordInsure").removeClass("vh"), $("#passWord").addClass("focus-red"), $("#pwdError").text(this.pwdInput.getPasswordInput().msg);
            else if ($("#insureVerifyCode").css("display") === "block") {
                var i = $("#verifyCode").val();
                //i ? (r.inputCode = i, e.SubmitInsureWithVcode(r)) : ($("#verifyCode").addClass("focus-red"), $("#codeVerify").removeClass("vh"), $("#verifyError").text("请输入验证码"));
                r.inputCode = i;
                e.SubmitInsureWithVcode(r);
            }
            else
                e.SubmitInsure(r);
        }, submitEnter: function (e) {
            var t = e.keyCode;
            13 === t && this.loginSubmit();
        }, focus_userName: function () {
            $("#userName").removeClass("focus-red"), $("#userNameInsure").addClass("vh"), $("#finalInsure").css("visibility", "hidden");
        }, focus_passWord: function () {
            $("#passWord").removeClass("focus-red"), $("#passWordInsure").addClass("vh"), $("#finalInsure").css("visibility", "hidden");
        }, focus_verifyCode: function () {
            $("#verifyCode").removeClass("focus-red"), $("#codeVerify").addClass("vh"), $("#finalInsure").css("visibility", "hidden");
        }, SubmitInsure: function (e) {
            var t = this, n = navigator.userAgent, r = navigator.userAgent.toLowerCase(), s = n.indexOf("Android") > -1 || n.indexOf("Adr") > -1, o = /(iPhone|iPad|iPod|iOS)/i.test(navigator.userAgent), u = r.match(/windows mobile/i) == "windows mobile", a = "001", f = "001";
            s ? f = "002" : o ? f = "003" : u && (f = "004");
            var l = Security.getDeviceAllInfo();
            i.login({
                param: { userId: e.userName, rsaPin: e.passWord, main_channel_type: a, branch_channel_type: f, cdata: l }, success: function (e) {
                    t.loginSuccess(e);
                }, error: function (e) {
                    e.errCode == "3000" ? t.informPage() : (t.$("#finalInsure").css("visibility", "visible"), t.$("#errorLoginMsg").text(e.errMsg), t.$("#insureVerifyCode").css("display", "block"), t.$("#verifyCode").val(""), t.createImg());
                }
            });
        }, SubmitInsureWithVcode: function (e) {
            var t = this, n = navigator.userAgent, r = navigator.userAgent.toLowerCase(), s = n.indexOf("Android") > -1 || n.indexOf("Adr") > -1, o = /(iPhone|iPad|iPod|iOS)/i.test(navigator.userAgent), u = r.match(/windows mobile/i) == "windows mobile", a = "001", f = "001";
            s ? f = "002" : o ? f = "003" : u && (f = "004");
            var l = Security.getDeviceAllInfo();
            i.login({
                param: { userId: e.userName, rsaPin: e.passWord, vCode: e.inputCode, main_channel_type: a, branch_channel_type: f, cdata: l }, success: function (e) {
                    t.loginSuccess(e);
                }, error: function (e) {
                    e.errCode == "104" || e.errCode == "105" || e.errCode == "110" ? (t.$("#codeVerify").removeClass("vh"), t.$("#verifyError").text(e.errMsg)) : e.errCode == "3000" ? t.informPage() : (t.$("#finalInsure").css("visibility", "visible"), t.$("#errorLoginMsg").text(e.errMsg)), t.$("#verifyCode").val(""), t.createImg();
                }
            });
        }, loginSuccess: function (e) {
            var t = this, n = e.responseBody, r = n.bankType, i = n.userType, u = n.preferredTool, a = n.otpFlag, f = n.openTools;
            App.storage.set("telePhone", n.telephoneNum);
            if (i == "1" || i == "2") {
                if (f == "1" || r == "0" || a == "0" || u == "3")
                    t.controller.indexRequest({ loginResponse: n });
                else if (f != "1" && r == "1" && a != "0")
                    if (f != "3" && u == "1") {
                        var l = new s({ controller: t.controller, loginResponse: n });
                        l.render();
                    } else if (f != "2" && u == "2") {
                        var c = new o({ type: "login", vCode: n.certVerifyCode, partyNo: n.partyNo, loginName: n.loginName, controller: t.controller, loginResponse: n });
                        c.render();
                    }
            } else
                i == "0" ? t.controller.indexRequest({ loginResponse: n }) : this.informPage();
        }, informPage: function () {
            var e = "抱歉，您还没有平安银行借记卡，暂时无法使用个人网银。";
            e += "已有平安银行借记卡？请先<a target='new' style='color:blue' ", e += "href='https://bank.pingan.com.cn/ibp/portal/register", e += "/bankNewRegisterStepOne.do'>注册个人网银</a>后再使用。", utils.dialog.alert(e);
        }
    });
}), define("text!mod/home/template/relogin.html", [], function () {
    return '<div class="tac m_t50">\r\n    <div class="ma relogin_icon"></div>\r\n</div>\r\n\r\n<p class="tac f2 m_t20">您已安全退出个人网银</p>\r\n\r\n<div class="tac m_t32">\r\n    <a class="btn7 m_r15" href="http://bank.pingan.com/index.shtml">回到首页</a>\r\n    <a class="btn7 m_r15" style="background-color: #3786d0" href="#home/home/index">重新登录</a>\r\n</div>\r\n<p class="tac m_t90">\r\n<div id="adPaly" style="margin: 0 auto;position: relative;left: 50%;margin-left: -338px;"></div>\r\n<!--<iframe src="https://www.pingan.com/adms/area.ctrl?AREAID=QY15111212343755" frameborder="0" scrolling="no" width="677px" height="212px"></iframe>-->\r\n</p>';
}), define("mod/home/view/relogin", ["require", "exports", "module", "text!../template/relogin.html", "../../../component/ad/ad"], function (e, t, n) {
    var r = e("text!../template/relogin.html"), i = e("../../../component/ad/ad");
    n.exports = ItemView.extend({
        template: _.template(r), initialize: function (e) {
            setTimeout(function () {
                var e = { dom: this.$("#adPaly"), adAry: ["QY16112315555550", "QY16112315560351", "QY16112315560952"], styles: { width: "677px", height: "212px" }, dotsType: "2" }, t = new i(e);
            }, 500);
        }
    });
}), function (e) {
    var t = function (e, t) {
        return e << t | e >>> 32 - t;
    }, n = function (e, t) {
        var n, r, i, s, o;
        return i = e & 2147483648, s = t & 2147483648, n = e & 1073741824, r = t & 1073741824, o = (e & 1073741823) + (t & 1073741823), n & r ? o ^ 2147483648 ^ i ^ s : n | r ? o & 1073741824 ? o ^ 3221225472 ^ i ^ s : o ^ 1073741824 ^ i ^ s : o ^ i ^ s;
    }, r = function (e, t, n) {
        return e & t | ~e & n;
    }, i = function (e, t, n) {
        return e & n | t & ~n;
    }, s = function (e, t, n) {
        return e ^ t ^ n;
    }, o = function (e, t, n) {
        return t ^ (e | ~n);
    }, u = function (e, i, s, o, u, a, f) {
        return e = n(e, n(n(r(i, s, o), u), f)), n(t(e, a), i);
    }, a = function (e, r, s, o, u, a, f) {
        return e = n(e, n(n(i(r, s, o), u), f)), n(t(e, a), r);
    }, f = function (e, r, i, o, u, a, f) {
        return e = n(e, n(n(s(r, i, o), u), f)), n(t(e, a), r);
    }, l = function (e, r, i, s, u, a, f) {
        return e = n(e, n(n(o(r, i, s), u), f)), n(t(e, a), r);
    }, c = function (e) {
        var t, n = e.length, r = n + 8, i = (r - r % 64) / 64, s = (i + 1) * 16, o = Array(s - 1), u = 0, a = 0;
        while (a < n)
            t = (a - a % 4) / 4, u = a % 4 * 8, o[t] = o[t] | e.charCodeAt(a) << u, a++;
        return t = (a - a % 4) / 4, u = a % 4 * 8, o[t] = o[t] | 128 << u, o[s - 2] = n << 3, o[s - 1] = n >>> 29, o;
    }, h = function (e) {
        var t = "", n = "", r, i;
        for (i = 0; i <= 3; i++)
            r = e >>> i * 8 & 255, n = "0" + r.toString(16), t += n.substr(n.length - 2, 2);
        return t;
    }, p = function (e) {
        e = e.replace(/\x0d\x0a/g, "\n");
        var t = "";
        for (var n = 0; n < e.length; n++) {
            var r = e.charCodeAt(n);
            r < 128 ? t += String.fromCharCode(r) : r > 127 && r < 2048 ? (t += String.fromCharCode(r >> 6 | 192), t += String.fromCharCode(r & 63 | 128)) : (t += String.fromCharCode(r >> 12 | 224), t += String.fromCharCode(r >> 6 & 63 | 128), t += String.fromCharCode(r & 63 | 128));
        }
        return t;
    };
    e.extend({
        md5: function (e) {
            var t = Array(), r, i, s, o, d, v, m, g, y, b = 7, w = 12, E = 17, S = 22, x = 5, T = 9, N = 14, C = 20, k = 4, L = 11, A = 16, O = 23, M = 6, _ = 10, D = 15, P = 21;
            e = p(e), t = c(e), v = 1732584193, m = 4023233417, g = 2562383102, y = 271733878;
            for (r = 0; r < t.length; r += 16)
                i = v, s = m, o = g, d = y, v = u(v, m, g, y, t[r + 0], b, 3614090360), y = u(y, v, m, g, t[r + 1], w, 3905402710), g = u(g, y, v, m, t[r + 2], E, 606105819), m = u(m, g, y, v, t[r + 3], S, 3250441966), v = u(v, m, g, y, t[r + 4], b, 4118548399), y = u(y, v, m, g, t[r + 5], w, 1200080426), g = u(g, y, v, m, t[r + 6], E, 2821735955), m = u(m, g, y, v, t[r + 7], S, 4249261313), v = u(v, m, g, y, t[r + 8], b, 1770035416), y = u(y, v, m, g, t[r + 9], w, 2336552879), g = u(g, y, v, m, t[r + 10], E, 4294925233), m = u(m, g, y, v, t[r + 11], S, 2304563134), v = u(v, m, g, y, t[r + 12], b, 1804603682), y = u(y, v, m, g, t[r + 13], w, 4254626195), g = u(g, y, v, m, t[r + 14], E, 2792965006), m = u(m, g, y, v, t[r + 15], S, 1236535329), v = a(v, m, g, y, t[r + 1], x, 4129170786), y = a(y, v, m, g, t[r + 6], T, 3225465664), g = a(g, y, v, m, t[r + 11], N, 643717713), m = a(m, g, y, v, t[r + 0], C, 3921069994), v = a(v, m, g, y, t[r + 5], x, 3593408605), y = a(y, v, m, g, t[r + 10], T, 38016083), g = a(g, y, v, m, t[r + 15], N, 3634488961), m = a(m, g, y, v, t[r + 4], C, 3889429448), v = a(v, m, g, y, t[r + 9], x, 568446438), y = a(y, v, m, g, t[r + 14], T, 3275163606), g = a(g, y, v, m, t[r + 3], N, 4107603335), m = a(m, g, y, v, t[r + 8], C, 1163531501), v = a(v, m, g, y, t[r + 13], x, 2850285829), y = a(y, v, m, g, t[r + 2], T, 4243563512), g = a(g, y, v, m, t[r + 7], N, 1735328473), m = a(m, g, y, v, t[r + 12], C, 2368359562), v = f(v, m, g, y, t[r + 5], k, 4294588738), y = f(y, v, m, g, t[r + 8], L, 2272392833), g = f(g, y, v, m, t[r + 11], A, 1839030562), m = f(m, g, y, v, t[r + 14], O, 4259657740), v = f(v, m, g, y, t[r + 1], k, 2763975236), y = f(y, v, m, g, t[r + 4], L, 1272893353), g = f(g, y, v, m, t[r + 7], A, 4139469664), m = f(m, g, y, v, t[r + 10], O, 3200236656), v = f(v, m, g, y, t[r + 13], k, 681279174), y = f(y, v, m, g, t[r + 0], L, 3936430074), g = f(g, y, v, m, t[r + 3], A, 3572445317), m = f(m, g, y, v, t[r + 6], O, 76029189), v = f(v, m, g, y, t[r + 9], k, 3654602809), y = f(y, v, m, g, t[r + 12], L, 3873151461), g = f(g, y, v, m, t[r + 15], A, 530742520), m = f(m, g, y, v, t[r + 2], O, 3299628645), v = l(v, m, g, y, t[r + 0], M, 4096336452), y = l(y, v, m, g, t[r + 7], _, 1126891415), g = l(g, y, v, m, t[r + 14], D, 2878612391), m = l(m, g, y, v, t[r + 5], P, 4237533241), v = l(v, m, g, y, t[r + 12], M, 1700485571), y = l(y, v, m, g, t[r + 3], _, 2399980690), g = l(g, y, v, m, t[r + 10], D, 4293915773), m = l(m, g, y, v, t[r + 1], P, 2240044497), v = l(v, m, g, y, t[r + 8], M, 1873313359), y = l(y, v, m, g, t[r + 15], _, 4264355552), g = l(g, y, v, m, t[r + 6], D, 2734768916), m = l(m, g, y, v, t[r + 13], P, 1309151649), v = l(v, m, g, y, t[r + 4], M, 4149444226), y = l(y, v, m, g, t[r + 11], _, 3174756917), g = l(g, y, v, m, t[r + 2], D, 718787259), m = l(m, g, y, v, t[r + 9], P, 3951481745), v = n(v, i), m = n(m, s), g = n(g, o), y = n(y, d);
            var H = h(v) + h(m) + h(g) + h(y);
            return H.toLowerCase();
        }
    });
}(jQuery), define("mod/home/common/jquery.md5", function () {
}), define("text!mod/home/template/noLoginAgain.html", [], function () {
    return '<div class="w930 ma c">\r\n    <p class="tac">\r\n        <img src="images/tzpic.png"></p>\r\n    <div id="newBankTz_container">\r\n        <p class="tz_ts tac p_b60 p_t20">\r\n            <span class="tzts">新网银跳转中</span>\r\n            <span class="tz-icon">\r\n                <img src="images/tz-icon.gif"></span>\r\n        </p>\r\n    </div>\r\n</div>';
}), define("text!mod/home/template/noLoginErrorHtml.html", [], function () {
    return '<div class="sucessLayout c">\r\n    <div class="c">\r\n        <%if("400"!=code){%>\r\n        <div class="icon65 fl m_l195" style="margin-left:360px;"></div>\r\n        <div class="fl m_l30">\r\n            <p class="f7 p_t10 tz-c1">跳转失败</p>\r\n        </div>\r\n        <%}else{%>\r\n        <div class="c">\r\n           <div class="icon65 fl" style=" margin-left:268px;"></div>\r\n           <div class="fl m_l30"><p class="f7 p_t10 tz-c1 c3">登录超时，请重新登录</p></div>\r\n         </div>\r\n         <%}%>\r\n    </div>\r\n    <div class="btn_box p_t30 p_b40">\r\n        <a class="btn-login m_r40" href="<%=config.values.old_bank_link%>">返回旧网银</a>\r\n        <a class="btn-login" href="#home/home/index">进入新网银</a>\r\n    </div>\r\n</div>\r\n';
}), define("mod/home/view/noLoginAgain", ["require", "exports", "module", "text!../template/noLoginAgain.html", "text!../template/noLoginErrorHtml.html", "../model/home", "./home"], function (e, t, n) {
    var r = e("text!../template/noLoginAgain.html"), i = e("text!../template/noLoginErrorHtml.html"), s = e("../model/home"), o = e("./home"), u = ItemView.extend({ template: _.template(i) });
    n.exports = Layout.extend({
        template: _.template(r), regions: { noLoginResultContainer: "#newBankTz_container" }, initialize: function () {
            var e = App.header.$el, t = App.footer.$el;
            e && App.header.$el.hide(), t && App.footer.$el.hide();
        }, onBeforeClose: function () {
            var e = App.header.$el, t = App.footer.$el;
            e && App.header.$el.show(), t && App.footer.$el.show();
        }, onShow: function () {
            var e = this.options.controller, t = this, n = this.options.toaFlag ? "ssoLogin" : "noLoginAgain", r, i = navigator.userAgent, a = navigator.userAgent.toLowerCase(), f = i.indexOf("Android") > -1 || i.indexOf("Adr") > -1, l = /(iPhone|iPad|iPod|iOS)/i.test(navigator.userAgent), c = a.match(/windows mobile/i) == "windows mobile", h = "001", p = "001";
            f ? p = "002" : l ? p = "003" : c && (p = "004");
            var d = { main_channel_type: h, branch_channel_type: p };
            _.delay(function () {
                s[n]({
                    param: d, success: function (t) {
                        r = t.responseBody.isVarifyLoginPassed;
                        if (r === "false") {
                            (new o({ controller: e })).loginSuccess(t);
                            return;
                        }
                        e.indexRequest({ loginResponse: t.responseBody });
                    }, error: function (e) {
                        if (t.options.jumpFlag) {
                            var n = t.options.backUrl;
                            n = encodeURIComponent(n), App.storage.set("notInFlag", !0), App.navigate("home/home/index/" + +(new Date) + "?returnURL=" + n, !0);
                            return;
                        }
                        t.noLoginResultContainer.show(new u({ model: new Model({ code: e.errCode }) })), App.service.$el && App.service.$el.empty();
                    }
                });
            }, 20);
        }
    });
}), define("mod/home/controller/home", ["require", "exports", "module", "../view/home", "../view/loginOtp", "../../common/view/header", "../../common/view/footer", "../model/home", "../view/relogin", "../common/jquery.md5", "../view/noLoginAgain", "../view/noLoginAgain"], function (e, t, n) {
    var r = e("../view/home"), i = e("../view/loginOtp"), s = e("../../common/view/header"), o = e("../../common/view/footer"), u = e("../model/home"), a = e("../view/relogin");
    e("../common/jquery.md5"), n.exports = Controller.extend({
        actions: { index: "index", "index/:param": "index", directToIndex: "directToIndex", relogin: "relogin", toaToIndex: "toaToIndex" }, authorize: function () {
            return !0;
        }, index: function (e, t) {
            var n = this;
            t = typeof e == "string" ? t : e, this.returnURL = t ? t.returnURL : "index/indexInfo/index", t && !App.storage.get("notInFlag") ? this.directToIndex(t) : (App.header.show(new s({ headerType: "portal" })), App.container.show(new r({ controller: this })), App.footer.show(new o), App.storage.remove("notInFlag"));
        }, relogin: function () {
            App.header.show(new s({ headerType: "portal" })), App.container.show(new a), App.footer.show(new o), App.service.$el && App.service.$el.empty();
        }, directToIndex: function (t) {
            this.returnURL = t ? t.returnURL : "index/indexInfo/index";
            var n = !!t, r = e("../view/noLoginAgain"), i = this;
            App.container.show(new r({ controller: this, jumpFlag: n, backUrl: this.returnURL }));
        }, toaToIndex: function (t) {
            this.returnURL = t ? t.returnURL : "index/indexInfo/index";
            var n = !!t, r = e("../view/noLoginAgain"), i = this;
            App.container.show(new r({ controller: this, jumpFlag: n, backUrl: this.returnURL, toaFlag: !0 }));
        }, indexRequest: function (e) {
            var t = this, n = e.loginResponse, r = this.returnURL;
            if (r && r.indexOf("transfer/payeeMgt") != -1) {
                var i = +(new Date);
                r = "transfer/payeeMgt/index?t=" + i;
            }
            var s = n.userType, o = {};
            if (s == "0") {
                r = "#creditCard/creditcard/index";
                var a = [], f = [], l = _.findWhere(n.menuInfo.levelOne, { menu_code: "ibpcred000000" });
                a.push(l), _.each(n.menuInfo.levelTwo, function (e) {
                    e.menu_parent_code == "ibpcred000000" && f.push(e);
                }), o = { levelOne: a, levelTwo: f };
            } else
                o = { levelOne: n.menuInfo.levelOne, levelOneSub: n.menuInfo.levelOneSub, levelTwo: n.menuInfo.levelTwo, levelThree: n.menuInfo.levelThree };
            App.storage.set("LoginMenuObj", o), u = new Object, u.userType = n.userType, u.bankType = n.bankType, u.currServiceTime = n.currServiceTime ? n.currServiceTime.replace(/\-/g, "/") : "", u.lastLogonSuccessDateStr = n.lastLogonSuccessDateStr, u.username = n.username, u.cardcode = n.cardcode, u.cardNo = n.cardNo, u.rankCode = n.rankCode, u.preferredTool = n.preferredTool, u.openTools = n.openTools, u.sysLimit = n.sysLimit, u.crossBankTransferLimit = n.crossBankTransferLimit, u.sameBankTransferLimit = n.sameBankTransferLimit, u.emailAddress = n.emailAddress, u.imageUrl = n.imageUrl, u.partyNo = n.partyNo, u.sex = n.sex, u.bankUserName = n.bankUserName, u.certVerifyCode = n.certVerifyCode, u.loginName = n.loginName, u.cardType = n.cardType, u.cardTypeCode = n.cardTypeCode, u.token = n.token, store.set("bankUserName", n.bankUserName), App.storage.set("telePhone", n.telephoneNum), App.storage.set("CUSTOMERNO", $.md5(n.partyNo)), App.storage.set("loginModel", u), App.storage.set("isUserLoggedIn", !0), App.navigate(r, !0), $("body").removeClass("body1"), domainUrl = domainUrl.replace("http:", "https:");
        }
    });
});
