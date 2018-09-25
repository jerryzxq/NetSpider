;
angular.module("nosessionSms", []).directive("nosessionsms", ["$log", "$interval", "$httpPlus", "$message", function (e, s, t, n) {
        return { restrict: "A", template: '<label>短信验证码：</label><input type="text" name="smsCode" ng-model="inputModel" ng-disabled="inputDisabled" class="input-text" maxlength="6" placeholder="{{smsPlaceHolder}}" validator="require|dypassword"/><p class="message-tip"><span class="message-tip-span" ng-if="smsResultMsg!=\'\'">{{smsResultMsg}}，{{secondValue}}s后可</span><a class="message-tip-a" ng-click="getSMCode()" ng-class="{\'a-disabled\':btnDisabled}">{{smsBtnText}}<span class="color-black">{{smsBtnText2}}</span></a></p><!--<div ng-if="showVerifyCode"><label>图片验证码：</label><input type="text" ng-model="$parent.verifyCode" class="input-text input-verifycode" /><img class="img-verifycode" verifycode="common" verify-operator="$parent.verifyCodeOp" height="40" /></div> --><span class="erro-info">{{errorMsg}}</span>', scope: { inputModel: "=", getParams: "&", preCondition: "&", extriVerifyCallback: "&" }, link: function (o) {
                function i(e, t) {
                    o.smsResultMsg = e, o.smsBtnText = "重发验证码", o.smsBtnText2 = "", o.secondValue = t;
                    var n = s(function () {
                        o.secondValue--, 0 == o.secondValue && (s.cancel(n), o.btnDisabled = !1, o.smsResultMsg = "");
                    }, 1e3);
                }
                o.smsPlaceHolder = "", o.smsBtnText = "发送", o.smsBtnText2 = "短信验证码", o.smsResultMsg = "", o.secondValue = -1, o.btnDisabled = !1, o.inputDisabled = !0, o.showVerifyCode = !1, o.errorMsg = "", o.getSMCode = function () {
                    if (!o.btnDisabled) {
                        if (o.preCondition && angular.isFunction(o.preCondition()) && !o.preCondition()())
                            return !1;
                        var s = o.showVerifyCode ? "sendMessageNoSessionAddCode.do" : "sendMessageNoSession.do", a = o.getParams()();
                        o.showVerifyCode && (a.checkCode = o.verifyCode), o.securitySmsCode = "", o.btnDisabled = !0, o.smsBtnText = "短信发送中...", o.smsBtnText2 = "", t.post(s, a).success(function (s) {
                            if (o.showVerifyCode = !1, o.errorMsg = "", "0000" === s.ec) {
                                var t = s.cd.sequence;
                                "0001" == t ? (o.showVerifyCode = !0, o.smsBtnText = "发送", o.smsBtnText2 = "短信验证码", o.btnDisabled = !1, n.pop({ title: "输入图片验证码", src: "app/error/extraVerifycode.tpl.html", className: "extraVerifyContainer", params: { msg: "您已多次获取短信，请校验图片验证码" }, onClose: function (e, s) {
                                        "2" == e ? (o.verifyCode = s.verifyCode, o.getSMCode()) : o.showVerifyCode = !1;
                                    } })) : "0002" == t ? (o.errorMsg = "短信验证超出当天次数", o.smsBtnText = "", o.smsBtnText2 = "") : "0003" == t ? i("发送失败", 10) : "0004" == t ? i("手机号不存在", 10) : "0005" == t ? i("手机号有误", 10) : "0006" == t ? (o.errorMsg = "系统正在繁忙中，请稍候再试", i("发送失败", 10)) : (e.debug(s.cd.mobileMsg), o.extriVerifyCallback && o.verifyCode && o.extriVerifyCallback(), o.smsPlaceHolder = "交易序号" + t, o.inputDisabled = !1, i("发送成功", 60));
                            } else
                                "EBPB0851" == s.ec ? (o.errorMsg = "您输入的图片验证码不正确", i("发送失败", 10)) : i("发送失败", 10);
                        });
                    }
                };
            } };
    }]);
;
angular.module("animations", ["ngAnimate", "commonAnimation", "mainAnimation", "fundAnimation", "goldAnimation"]);
;
angular.module("fundAnimation", []).animation(".fund_compare", [function () {
        return { addClass: function (a) {
                var n = a.inheritedData("$scope"), i = n.$index;
                $(".prod_compare").eq(i).append("<a style='display:block;position:absolute;opacity=0.5;z-index:100'>对比</a>");
                var o = $(".prod_compare").eq(i).find("a");
                $(o[0]).css($(o[0]).position()).animate({ top: "-=" + (50 + 155 * i) + "px", left: "-=319px" }, 500, function () {
                    $(this).animate({ top: "-=44px" }, 800, function () {
                        $(this).remove();
                    });
                });
            } };
    }]);
;
angular.module("goldAnimation", []).animation(".gold_addcar", [function () {
        var a = function (a) {
            var e = (a.inheritedData("$scope"), angular.element("<i style='position:absolute;opacity=0.5;z-index:100' class='addcar'></i>"));
            a.eq(0).append(e);
            var t = angular.element("#addcarTag").eq(0).offset(), n = angular.element("#goldCarTopDiv").eq(0).offset(), o = t.top - n.top - 60, i = n.left - t.left;
            e.css(e.position()).animate({ top: "-=" + o + "px", left: "+=" + i + "px" }, 500, function () {
                e.animate({ top: "-=44px" }, 800, function () {
                    e.remove();
                });
            });
        };
        return { addClass: a, removeClass: a };
    }]);
;
angular.module("mainAnimation", []).animation(".content_banner", [function () {
        return { addClass: function (n, a, e) {
                n.fadeIn(1500, e);
            }, removeClass: function (n, a, e) {
                n.fadeOut(1e3, e);
            } };
    }]).animation(".content_announce", [function () {
        return { addClass: function (n) {
                var a = n.inheritedData("$scope"), e = a.announceIndex, t = (a.announceLen, 1500);
                if (0 == e)
                    n.hide(), n.parent().css({ top: "0px" }), n.fadeIn(1500);
                else {
                    var i = n.parent().css("top");
                    i = i.indexOf("px") ? i.substring(0, i.indexOf("px")) : i, n.parent().animate({ top: i - 80 + "px" }, t);
                }
            } };
    }]);
;
angular.module("autoPadding", []).directive("autoPadding", ["$globalData", "$httpPlus", "$utb", "$log", "$interval", "$compile", function (i, t, a, n, o, l) {
        return { restrict: "EAC", link: function (i, t) {
                var a, n = t, d = $('<ul class="ng-hide autoPaddingContainer" ng-show="_isShow" doc-click="_clearInput();"><li class="gold-morethan" ng-show="_isLoadAutoFill"><span class="more-box"><i></i></span></li><li class="item" ng-repeat="options in _dataList" ng-click="_itemTab(options)">{{options.title}}</li></ul>'), s = d, e = n.val(), c = !1;
                n.after(d);
                var u = l(d);
                $(s[0]).css({ "margin-top": $(n[0]).height() }), $(s[0]).width($(n[0]).width() - 2), $(s[0]).find(".gold-morethan").width($(n[0]).width() - 2), o(function () {
                    var t, o = n.val();
                    o && e != o ? (c = !1, i._isLoadAutoFill = !0, i._isShow = !0, t = a = i.getPaddingData(o).then(function (n) {
                        i._isLoadAutoFill = !1, t === a && (n.length > 0 ? i._dataList = n : (i._dataList = [], i._isShow = !1));
                    }), e = o) : o || (e = "", i._dataList = [], i._isShow = !1, c = !1);
                }, 600), i._itemTab = function (t) {
                    i._isShow = !1, n.val(""), c = !0, i.paddingItemClick && i.paddingItemClick(t);
                }, i._clearInput = function () {
                    c || (e = "", i.paddingClearInput && i.paddingClearInput());
                }, u(i);
            } };
    }]);
;
angular.module("cardInput", []).directive("cardInput", ["$filter", function (e) {
        return { restrict: "A", require: "ngModel", link: function (n, t, r, u) {
                var a = e("card"), i = e("uncard");
                t.on("keypress", function (e) {
                    var n, r;
                    return e.ctrlKey || e.shiftKey || e.altKey ? void 0 : (n = e.keyCode || e.which, r = t.val().length, n >= 48 && 57 >= n || 8 == n || n >= 65 && 90 >= n || n >= 97 && 122 >= n ? !0 : (window.event ? window.event.returnValue = !1 : e.preventDefault(), !1));
                }), t.bind("keyup", function () {
                    var e = this.value;
                    u.$viewValue !== e && (n.$root.$$phase ? u.$setViewValue(e) : n.$apply(function () {
                        u.$setViewValue(e);
                    }));
                }), t.on("blur", function () {
                    t.val(a(t.val()));
                }), t.on("focus", function () {
                    t.val(i(t.val()));
                }), u.$parsers.push(function (e) {
                    return i(e);
                }), u.$formatters.push(function (e) {
                    return a(e);
                });
            } };
    }]);
;
angular.module("certInfo", []).directive("certInfo", ["$filter", "$q", "$httpPlus", function (e, t, i) {
        var n = 0;
        return { restrict: "A", scope: { onInit: "&" }, link: function (e, d) {
                function c() {
                    var e = "";
                    return f || (e = o ? '<div style="width:0px;height:0px" id="ftActiveDiv"><OBJECT classid="clsid:39DAD358-310A-48E6-9EBE-9B4C8260A626" id="ftActive2000" name="ftActive2000" height="0" width="0"></OBJECT><OBJECT classid="clsid:4C4A9EAE-4C40-43F5-8AFB-AE0B0B94B0FF" id="ftActive2001" name="ftActive2001" height="0" width="0"></OBJECT><OBJECT classid="clsid:A9E8A51E-236F-4390-9CE2-2FCD9819C7B0" id="ftActive3000" name="ftActive3000" height="0" width="0"></OBJECT></div>' : '<div id="ftActiveDiv"><embed type="application/npFT2000APIForNBCBank" id="ftActive2000" name="ftActive2000" style="width:0;height:0" > </embed><embed type="application/npFTAudio3000APIForNBCBank" id="ftActive3000" name="ftActive3000" style="width:0;height:0" > </embed><embed type="application/npFTAPIForNBCBank" id="ftActive2001" name="ftActive2001" style="width:0;height:0" > </embed></div>', d.html(e), f = !0), d.find(o ? "OBJECT" : "EMBED");
                }
                function r() {
                    for (var e = c(), t = 0, i = 0; i < e.length && !(t > 0); i++)
                        try  {
                            t = e[i].ftGetCount();
                        } catch (n) {
                            t = 0;
                        }
                    return t;
                }
                function a() {
                    var e = r(), c = "GLOBAL_MW_CSP_CERTIVICATE_OBJECT_LOGIN_" + (new Date).getTime() + "_" + n++, a = new Messenger(c), o = t.defer(), f = "", v = function () {
                        return 0 != e || a.env.edge || a.env.isMac ? (f = a.certInfo("C = CN"), f || (f = a.certInfo("C = cn")), (!f || f.indexOf("@") < 0 && 0 != f.indexOf("nbcb")) && (f = ""), void o.resolve(f)) : (f = "", void o.resolve(f));
                    };
                    return a.env.edge ? i.post("getEdgeRandomSignNoSession.do", {}).success(function (e) {
                        if ("0000" == e.ec) {
                            var t = e.cd.pgeRZRand, i = e.cd.pgeRZData;
                            d.after(angular.element(a.createTag(t, i))), v();
                        }
                    }) : (d.after(angular.element(a.createTag())), v()), o.promise;
                }
                var o = window.navigator.userAgent.indexOf("MSIE") > -1 || window.navigator.userAgent.indexOf("Trident") > -1, f = !1, v = { getCertSN: a };
                e.onInit({ $operator: v });
            } };
    }]);
;
angular.module("chart", []).directive("chart", ["$log", "$window", function (t, i) {
        return { restrict: "A", scope: { onInit: "&" }, link: function (t, e, s) {
                function n(t, e) {
                    switch (e = e || {}, i.G_vmlCanvasManager && (e.animation = !1), s.chart) {
                        case "line":
                            r = l.Line(t, e);
                            break;
                        case "bar":
                            r = l.Bar(t, e);
                            break;
                        case "radar":
                            r = l.Radar(t, e);
                            break;
                        case "pie":
                            r = l.Pie(t, e);
                            break;
                        case "doughnut":
                            r = l.Doughnut(t, e);
                            break;
                        case "polar":
                            r = l.Polar(t, e);
                    }
                }
                function o() {
                    null != r && r.destroy();
                }
                var a = angular.element("<canvas>");
                a[0].width = s.width, a[0].height = s.height, e.append(a), i.G_vmlCanvasManager && i.G_vmlCanvasManager.initElement(a[0]);
                var h = a[0].getContext("2d"), l = new Chart(h), r = null;
                angular.extend(Chart.defaults.global, { tooltipFontFamily: "'Microsoft Yahei', '微软雅黑', 'Lucida Grande', 'Lucida Sans Unicode', 'Verdana', 'Arial', 'Helvetica Neue', 'Helvetica', sans-serif" }), t.onInit({ $operator: { init: n, clear: o } });
            } };
    }]), function () {
    "use strict";
    var t = this, i = t.Chart, e = function (t) {
        this.canvas = t.canvas, this.ctx = t;
        var i = function (t, i) {
            return t["offset" + i] ? t["offset" + i] : document.defaultView.getComputedStyle(t).getPropertyValue(i);
        }, e = this.width = i(t.canvas, "Width"), n = this.height = i(t.canvas, "Height");
        t.canvas.width = e, t.canvas.height = n;
        var e = this.width = t.canvas.width, n = this.height = t.canvas.height;
        return this.aspectRatio = this.width / this.height, s.retinaScale(this), this;
    };
    e.defaults = { global: { animation: !0, animationSteps: 60, animationEasing: "easeOutQuart", showScale: !0, scaleOverride: !1, scaleSteps: null, scaleStepWidth: null, scaleStartValue: null, scaleLineColor: "rgba(0,0,0,.1)", scaleLineWidth: 1, scaleShowLabels: !0, scaleLabel: "<%=value%>", scaleIntegersOnly: !0, scaleBeginAtZero: !1, scaleFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif", scaleFontSize: 12, scaleFontStyle: "normal", scaleFontColor: "#666", responsive: !1, maintainAspectRatio: !0, showTooltips: !0, customTooltips: !1, tooltipEvents: ["mousemove", "touchstart", "touchmove", "mouseout"], tooltipFillColor: "rgba(0,0,0,0.8)", tooltipFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif", tooltipFontSize: 14, tooltipFontStyle: "normal", tooltipFontColor: "#fff", tooltipTitleFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif", tooltipTitleFontSize: 14, tooltipTitleFontStyle: "bold", tooltipTitleFontColor: "#fff", tooltipYPadding: 6, tooltipXPadding: 6, tooltipCaretSize: 8, tooltipCornerRadius: 6, tooltipXOffset: 10, tooltipTemplate: "<%if (label){%><%=label%>: <%}%><%= value %>", multiTooltipTemplate: "<%= value %>", multiTooltipKeyBackground: "#fff", onAnimationProgress: function () {
            }, onAnimationComplete: function () {
            } } }, e.types = {};
    var s = e.helpers = {}, n = s.each = function (t, i, e) {
        var s = Array.prototype.slice.call(arguments, 3);
        if (t)
            if (t.length === +t.length) {
                var n;
                for (n = 0; n < t.length; n++)
                    i.apply(e, [t[n], n].concat(s));
            } else
                for (var o in t)
                    i.apply(e, [t[o], o].concat(s));
    }, o = s.clone = function (t) {
        var i = {};
        return n(t, function (e, s) {
            t.hasOwnProperty(s) && (i[s] = e);
        }), i;
    }, a = s.extend = function (t) {
        return n(Array.prototype.slice.call(arguments, 1), function (i) {
            n(i, function (e, s) {
                i.hasOwnProperty(s) && (t[s] = e);
            });
        }), t;
    }, h = s.merge = function () {
        var t = Array.prototype.slice.call(arguments, 0);
        return t.unshift({}), a.apply(null, t);
    }, l = s.indexOf = function (t, i) {
        if (Array.prototype.indexOf)
            return t.indexOf(i);
        for (var e = 0; e < t.length; e++)
            if (t[e] === i)
                return e;
        return -1;
    }, r = (s.where = function (t, i) {
        var e = [];
        return s.each(t, function (t) {
            i(t) && e.push(t);
        }), e;
    }, s.findNextWhere = function (t, i, e) {
        e || (e = -1);
        for (var s = e + 1; s < t.length; s++) {
            var n = t[s];
            if (i(n))
                return n;
        }
    }, s.findPreviousWhere = function (t, i, e) {
        e || (e = t.length);
        for (var s = e - 1; s >= 0; s--) {
            var n = t[s];
            if (i(n))
                return n;
        }
    }, s.inherits = function (t) {
        var i = this, e = t && t.hasOwnProperty("constructor") ? t.constructor : function () {
            return i.apply(this, arguments);
        }, s = function () {
            this.constructor = e;
        };
        return s.prototype = i.prototype, e.prototype = new s, e.extend = r, t && a(e.prototype, t), e.__super__ = i.prototype, e;
    }), c = s.noop = function () {
    }, u = s.uid = function () {
        var t = 0;
        return function () {
            return "chart-" + t++;
        };
    }(), d = s.warn = function (t) {
        window.console && "function" == typeof window.console.warn && console.warn(t);
    }, p = s.amd = "function" == typeof define && define.amd, f = s.isNumber = function (t) {
        return !isNaN(parseFloat(t)) && isFinite(t);
    }, g = s.max = function (t) {
        return Math.max.apply(Math, t);
    }, m = s.min = function (t) {
        return Math.min.apply(Math, t);
    }, v = (s.cap = function (t, i, e) {
        if (f(i)) {
            if (t > i)
                return i;
        } else if (f(e) && e > t)
            return e;
        return t;
    }, s.getDecimalPlaces = function (t) {
        return t % 1 !== 0 && f(t) ? t.toString().split(".")[1].length : 0;
    }), S = s.radians = function (t) {
        return t * (Math.PI / 180);
    }, x = (s.getAngleFromPoint = function (t, i) {
        var e = i.x - t.x, s = i.y - t.y, n = Math.sqrt(e * e + s * s), o = 2 * Math.PI + Math.atan2(s, e);
        return 0 > e && 0 > s && (o += 2 * Math.PI), { angle: o, distance: n };
    }, s.aliasPixel = function (t) {
        return t % 2 === 0 ? 0 : .5;
    }), y = (s.splineCurve = function (t, i, e, s) {
        var n = Math.sqrt(Math.pow(i.x - t.x, 2) + Math.pow(i.y - t.y, 2)), o = Math.sqrt(Math.pow(e.x - i.x, 2) + Math.pow(e.y - i.y, 2)), a = s * n / (n + o), h = s * o / (n + o);
        return { inner: { x: i.x - a * (e.x - t.x), y: i.y - a * (e.y - t.y) }, outer: { x: i.x + h * (e.x - t.x), y: i.y + h * (e.y - t.y) } };
    }, s.calculateOrderOfMagnitude = function (t) {
        return Math.floor(Math.log(t) / Math.LN10);
    }), w = (s.calculateScaleRange = function (t, i, e, s, n) {
        var o = 2, a = Math.floor(i / (1.5 * e)), h = o >= a, l = g(t), r = m(t);
        l === r && (l += .5, r >= .5 && !s ? r -= .5 : l += .5);
        for (var c = Math.abs(l - r), u = y(c), d = Math.ceil(l / (1 * Math.pow(10, u))) * Math.pow(10, u), p = s ? 0 : Math.floor(r / (1 * Math.pow(10, u))) * Math.pow(10, u), f = d - p, v = Math.pow(10, u), S = Math.round(f / v); (S > a || a > 2 * S) && !h;)
            if (S > a)
                v *= 2, S = Math.round(f / v), S % 1 !== 0 && (h = !0);
            else if (n && u >= 0) {
                if (v / 2 % 1 !== 0)
                    break;
                v /= 2, S = Math.round(f / v);
            } else
                v /= 2, S = Math.round(f / v);
        return h && (S = o, v = f / S), { steps: S, stepValue: v, min: p, max: p + S * v };
    }, s.template = function (t, i) {
        function e(t, i) {
            var e = /\W/.test(t) ? new Function("obj", "var p=[],print=function(){p.push.apply(p,arguments);};with(obj){p.push('" + t.replace(/[\r\t\n]/g, " ").split("<%").join("	").replace(/((^|%>)[^\t]*)'/g, "$1\r").replace(/\t=(.*?)%>/g, "',$1,'").split("	").join("');").split("%>").join("p.push('").split("\r").join("\\'") + "');}return p.join('');") : s[t] = s[t];
            return i ? e(i) : e;
        }
        if (t instanceof Function)
            return t(i);
        var s = {};
        return e(t, i);
    }), C = (s.generateLabels = function (t, i, e, s) {
        var o = new Array(i);
        return labelTemplateString && n(o, function (i, n) {
            o[n] = w(t, { value: e + s * (n + 1) });
        }), o;
    }, s.easingEffects = { linear: function (t) {
            return t;
        }, easeInQuad: function (t) {
            return t * t;
        }, easeOutQuad: function (t) {
            return -1 * t * (t - 2);
        }, easeInOutQuad: function (t) {
            return (t /= .5) < 1 ? .5 * t * t : -0.5 * (--t * (t - 2) - 1);
        }, easeInCubic: function (t) {
            return t * t * t;
        }, easeOutCubic: function (t) {
            return 1 * ((t = t / 1 - 1) * t * t + 1);
        }, easeInOutCubic: function (t) {
            return (t /= .5) < 1 ? .5 * t * t * t : .5 * ((t -= 2) * t * t + 2);
        }, easeInQuart: function (t) {
            return t * t * t * t;
        }, easeOutQuart: function (t) {
            return -1 * ((t = t / 1 - 1) * t * t * t - 1);
        }, easeInOutQuart: function (t) {
            return (t /= .5) < 1 ? .5 * t * t * t * t : -0.5 * ((t -= 2) * t * t * t - 2);
        }, easeInQuint: function (t) {
            return 1 * (t /= 1) * t * t * t * t;
        }, easeOutQuint: function (t) {
            return 1 * ((t = t / 1 - 1) * t * t * t * t + 1);
        }, easeInOutQuint: function (t) {
            return (t /= .5) < 1 ? .5 * t * t * t * t * t : .5 * ((t -= 2) * t * t * t * t + 2);
        }, easeInSine: function (t) {
            return -1 * Math.cos(t / 1 * (Math.PI / 2)) + 1;
        }, easeOutSine: function (t) {
            return 1 * Math.sin(t / 1 * (Math.PI / 2));
        }, easeInOutSine: function (t) {
            return -0.5 * (Math.cos(Math.PI * t / 1) - 1);
        }, easeInExpo: function (t) {
            return 0 === t ? 1 : 1 * Math.pow(2, 10 * (t / 1 - 1));
        }, easeOutExpo: function (t) {
            return 1 === t ? 1 : 1 * (-Math.pow(2, -10 * t / 1) + 1);
        }, easeInOutExpo: function (t) {
            return 0 === t ? 0 : 1 === t ? 1 : (t /= .5) < 1 ? .5 * Math.pow(2, 10 * (t - 1)) : .5 * (-Math.pow(2, -10 * --t) + 2);
        }, easeInCirc: function (t) {
            return t >= 1 ? t : -1 * (Math.sqrt(1 - (t /= 1) * t) - 1);
        }, easeOutCirc: function (t) {
            return 1 * Math.sqrt(1 - (t = t / 1 - 1) * t);
        }, easeInOutCirc: function (t) {
            return (t /= .5) < 1 ? -0.5 * (Math.sqrt(1 - t * t) - 1) : .5 * (Math.sqrt(1 - (t -= 2) * t) + 1);
        }, easeInElastic: function (t) {
            var i = 1.70158, e = 0, s = 1;
            return 0 === t ? 0 : 1 == (t /= 1) ? 1 : (e || (e = .3), s < Math.abs(1) ? (s = 1, i = e / 4) : i = e / (2 * Math.PI) * Math.asin(1 / s), -(s * Math.pow(2, 10 * (t -= 1)) * Math.sin(2 * (1 * t - i) * Math.PI / e)));
        }, easeOutElastic: function (t) {
            var i = 1.70158, e = 0, s = 1;
            return 0 === t ? 0 : 1 == (t /= 1) ? 1 : (e || (e = .3), s < Math.abs(1) ? (s = 1, i = e / 4) : i = e / (2 * Math.PI) * Math.asin(1 / s), s * Math.pow(2, -10 * t) * Math.sin(2 * (1 * t - i) * Math.PI / e) + 1);
        }, easeInOutElastic: function (t) {
            var i = 1.70158, e = 0, s = 1;
            return 0 === t ? 0 : 2 == (t /= .5) ? 1 : (e || (e = .3 * 1.5), s < Math.abs(1) ? (s = 1, i = e / 4) : i = e / (2 * Math.PI) * Math.asin(1 / s), 1 > t ? -.5 * s * Math.pow(2, 10 * (t -= 1)) * Math.sin(2 * (1 * t - i) * Math.PI / e) : s * Math.pow(2, -10 * (t -= 1)) * Math.sin(2 * (1 * t - i) * Math.PI / e) * .5 + 1);
        }, easeInBack: function (t) {
            var i = 1.70158;
            return 1 * (t /= 1) * t * ((i + 1) * t - i);
        }, easeOutBack: function (t) {
            var i = 1.70158;
            return 1 * ((t = t / 1 - 1) * t * ((i + 1) * t + i) + 1);
        }, easeInOutBack: function (t) {
            var i = 1.70158;
            return (t /= .5) < 1 ? .5 * t * t * (((i *= 1.525) + 1) * t - i) : .5 * ((t -= 2) * t * (((i *= 1.525) + 1) * t + i) + 2);
        }, easeInBounce: function (t) {
            return 1 - C.easeOutBounce(1 - t);
        }, easeOutBounce: function (t) {
            return (t /= 1) < 1 / 2.75 ? 7.5625 * t * t : 2 / 2.75 > t ? 1 * (7.5625 * (t -= 1.5 / 2.75) * t + .75) : 2.5 / 2.75 > t ? 1 * (7.5625 * (t -= 2.25 / 2.75) * t + .9375) : 1 * (7.5625 * (t -= 2.625 / 2.75) * t + .984375);
        }, easeInOutBounce: function (t) {
            return .5 > t ? .5 * C.easeInBounce(2 * t) : .5 * C.easeOutBounce(2 * t - 1) + .5;
        } }), b = s.requestAnimFrame = function () {
        return window.requestAnimationFrame || window.webkitRequestAnimationFrame || window.mozRequestAnimationFrame || window.oRequestAnimationFrame || window.msRequestAnimationFrame || function (t) {
            return window.setTimeout(t, 1e3 / 60);
        };
    }(), P = s.cancelAnimFrame = function () {
        return window.cancelAnimationFrame || window.webkitCancelAnimationFrame || window.mozCancelAnimationFrame || window.oCancelAnimationFrame || window.msCancelAnimationFrame || function (t) {
            return window.clearTimeout(t, 1e3 / 60);
        };
    }(), L = (s.animationLoop = function (t, i, e, s, n, o) {
        var a = 0, h = C[e] || C.linear, l = function () {
            a++;
            var e = a / i, r = h(e);
            t.call(o, r, e, a), s.call(o, r, e), i > a ? o.animationFrame = b(l) : n.apply(o);
        };
        b(l);
    }, s.getRelativePosition = function (t) {
        var i, e, s = t.originalEvent || t, n = t.currentTarget || t.srcElement, o = n.getBoundingClientRect();
        return s.touches ? (i = s.touches[0].clientX - o.left, e = s.touches[0].clientY - o.top) : (i = s.clientX - o.left, e = s.clientY - o.top), { x: i, y: e };
    }, s.addEvent = function (t, i, e) {
        t.addEventListener ? t.addEventListener(i, e) : t.attachEvent ? t.attachEvent("on" + i, e) : t["on" + i] = e;
    }), k = s.removeEvent = function (t, i, e) {
        t.removeEventListener ? t.removeEventListener(i, e, !1) : t.detachEvent ? t.detachEvent("on" + i, e) : t["on" + i] = c;
    }, F = (s.bindEvents = function (t, i, e) {
        t.events || (t.events = {}), n(i, function (i) {
            t.events[i] = function () {
                e.apply(t, arguments);
            }, L(t.chart.canvas, i, t.events[i]);
        });
    }, s.unbindEvents = function (t, i) {
        n(i, function (i, e) {
            k(t.chart.canvas, e, i);
        });
    }), R = s.getMaximumWidth = function (t) {
        var i = t.parentNode;
        return i.clientWidth;
    }, A = s.getMaximumHeight = function (t) {
        var i = t.parentNode;
        return i.clientHeight;
    }, T = (s.getMaximumSize = s.getMaximumWidth, s.retinaScale = function (t) {
        var i = t.ctx, e = t.canvas.width, s = t.canvas.height;
        window.devicePixelRatio && (i.canvas.style.width = e + "px", i.canvas.style.height = s + "px", i.canvas.height = s * window.devicePixelRatio, i.canvas.width = e * window.devicePixelRatio, i.scale(window.devicePixelRatio, window.devicePixelRatio));
    }), M = s.clear = function (t) {
        t.ctx.clearRect(0, 0, t.width, t.height);
    }, W = s.fontString = function (t, i, e) {
        return i + " " + t + "px " + e;
    }, z = s.longestText = function (t, i, e) {
        t.font = i;
        var s = 0;
        return n(e, function (i) {
            var e = t.measureText(i).width;
            s = e > s ? e : s;
        }), s;
    }, B = s.drawRoundedRectangle = function (t, i, e, s, n, o) {
        t.beginPath(), t.moveTo(i + o, e), t.lineTo(i + s - o, e), t.quadraticCurveTo(i + s, e, i + s, e + o), t.lineTo(i + s, e + n - o), t.quadraticCurveTo(i + s, e + n, i + s - o, e + n), t.lineTo(i + o, e + n), t.quadraticCurveTo(i, e + n, i, e + n - o), t.lineTo(i, e + o), t.quadraticCurveTo(i, e, i + o, e), t.closePath();
    };
    e.instances = {}, e.Type = function (t, i, s) {
        this.options = i, this.chart = s, this.id = u(), e.instances[this.id] = this, i.responsive && this.resize(), this.initialize.call(this, t);
    }, a(e.Type.prototype, { initialize: function () {
            return this;
        }, clear: function () {
            return M(this.chart), this;
        }, stop: function () {
            return P(this.animationFrame), this;
        }, resize: function (t) {
            this.stop();
            var i = this.chart.canvas, e = R(this.chart.canvas), s = this.options.maintainAspectRatio ? e / this.chart.aspectRatio : A(this.chart.canvas);
            return i.width = this.chart.width = e, i.height = this.chart.height = s, T(this.chart), "function" == typeof t && t.apply(this, Array.prototype.slice.call(arguments, 1)), this;
        }, reflow: c, render: function (t) {
            return t && this.reflow(), this.options.animation && !t ? s.animationLoop(this.draw, this.options.animationSteps, this.options.animationEasing, this.options.onAnimationProgress, this.options.onAnimationComplete, this) : (this.draw(), this.options.onAnimationComplete.call(this)), this;
        }, generateLegend: function () {
            return w(this.options.legendTemplate, this);
        }, destroy: function () {
            this.clear(), F(this, this.events);
            var t = this.chart.canvas;
            t.width = this.chart.width, t.height = this.chart.height, t.style.removeProperty ? (t.style.removeProperty("width"), t.style.removeProperty("height")) : (t.style.removeAttribute("width"), t.style.removeAttribute("height")), delete e.instances[this.id];
        }, showTooltip: function (t, i) {
            "undefined" == typeof this.activeElements && (this.activeElements = []);
            var o = function (t) {
                var i = !1;
                return t.length !== this.activeElements.length ? i = !0 : (n(t, function (t, e) {
                    t !== this.activeElements[e] && (i = !0);
                }, this), i);
            }.call(this, t);
            if (o || i) {
                if (this.activeElements = t, this.draw(), this.options.customTooltips && this.options.customTooltips(!1), t.length > 0)
                    if (this.datasets && this.datasets.length > 1) {
                        for (var a, h, r = this.datasets.length - 1; r >= 0 && (a = this.datasets[r].points || this.datasets[r].bars || this.datasets[r].segments, h = l(a, t[0]), -1 === h); r--)
                            ;
                        var c = [], u = [], d = function () {
                            var t, i, e, n, o, a = [], l = [], r = [];
                            return s.each(this.datasets, function (i) {
                                t = i.points || i.bars || i.segments, t[h] && t[h].hasValue() && a.push(t[h]);
                            }), s.each(a, function (t) {
                                l.push(t.x), r.push(t.y), c.push(s.template(this.options.multiTooltipTemplate, t)), u.push({ fill: t._saved.fillColor || t.fillColor, stroke: t._saved.strokeColor || t.strokeColor });
                            }, this), o = m(r), e = g(r), n = m(l), i = g(l), { x: n > this.chart.width / 2 ? n : i, y: (o + e) / 2 };
                        }.call(this, h);
                        new e.MultiTooltip({ x: d.x, y: d.y, xPadding: this.options.tooltipXPadding, yPadding: this.options.tooltipYPadding, xOffset: this.options.tooltipXOffset, fillColor: this.options.tooltipFillColor, textColor: this.options.tooltipFontColor, fontFamily: this.options.tooltipFontFamily, fontStyle: this.options.tooltipFontStyle, fontSize: this.options.tooltipFontSize, titleTextColor: this.options.tooltipTitleFontColor, titleFontFamily: this.options.tooltipTitleFontFamily, titleFontStyle: this.options.tooltipTitleFontStyle, titleFontSize: this.options.tooltipTitleFontSize, cornerRadius: this.options.tooltipCornerRadius, labels: c, legendColors: u, legendColorBackground: this.options.multiTooltipKeyBackground, title: "" == t[0].label ? t[0].customLabel : t[0].label, chart: this.chart, ctx: this.chart.ctx, custom: this.options.customTooltips }).draw();
                    } else
                        n(t, function (t) {
                            var i = t.tooltipPosition();
                            new e.Tooltip({ x: Math.round(i.x), y: Math.round(i.y), xPadding: this.options.tooltipXPadding, yPadding: this.options.tooltipYPadding, fillColor: this.options.tooltipFillColor, textColor: this.options.tooltipFontColor, fontFamily: this.options.tooltipFontFamily, fontStyle: this.options.tooltipFontStyle, fontSize: this.options.tooltipFontSize, caretHeight: this.options.tooltipCaretSize, cornerRadius: this.options.tooltipCornerRadius, text: w(this.options.tooltipTemplate, t), chart: this.chart, custom: this.options.customTooltips }).draw();
                        }, this);
                return this;
            }
        }, toBase64Image: function () {
            return this.chart.canvas.toDataURL.apply(this.chart.canvas, arguments);
        } }), e.Type.extend = function (t) {
        var i = this, s = function () {
            return i.apply(this, arguments);
        };
        if (s.prototype = o(i.prototype), a(s.prototype, t), s.extend = e.Type.extend, t.name || i.prototype.name) {
            var n = t.name || i.prototype.name, l = e.defaults[i.prototype.name] ? o(e.defaults[i.prototype.name]) : {};
            e.defaults[n] = a(l, t.defaults), e.types[n] = s, e.prototype[n] = function (t, i) {
                var o = h(e.defaults.global, e.defaults[n], i || {});
                return new s(t, o, this);
            };
        } else
            d("Name not provided for this chart, so it hasn't been registered");
        return i;
    }, e.Element = function (t) {
        a(this, t), this.initialize.apply(this, arguments), this.save();
    }, a(e.Element.prototype, { initialize: function () {
        }, restore: function (t) {
            return t ? n(t, function (t) {
                this[t] = this._saved[t];
            }, this) : a(this, this._saved), this;
        }, save: function () {
            return this._saved = o(this), delete this._saved._saved, this;
        }, update: function (t) {
            return n(t, function (t, i) {
                this._saved[i] = this[i], this[i] = t;
            }, this), this;
        }, transition: function (t, i) {
            return n(t, function (t, e) {
                this[e] = (t - this._saved[e]) * i + this._saved[e];
            }, this), this;
        }, tooltipPosition: function () {
            return { x: this.x, y: this.y };
        }, hasValue: function () {
            return f(this.value);
        } }), e.Element.extend = r, e.Point = e.Element.extend({ display: !0, inRange: function (t, i) {
            var e = this.hitDetectionRadius + this.radius;
            return Math.pow(t - this.x, 2) + Math.pow(i - this.y, 2) < Math.pow(e, 2);
        }, draw: function () {
            if (this.display) {
                var t = this.ctx;
                t.beginPath(), t.arc(this.x, this.y, this.radius, 0, 2 * Math.PI), t.closePath(), t.strokeStyle = this.strokeColor, t.lineWidth = this.strokeWidth, t.fillStyle = this.fillColor, t.fill(), t.stroke();
            }
        } }), e.Arc = e.Element.extend({ inRange: function (t, i) {
            var e = s.getAngleFromPoint(this, { x: t, y: i }), n = e.angle >= this.startAngle && e.angle <= this.endAngle, o = e.distance >= this.innerRadius && e.distance <= this.outerRadius;
            return n && o;
        }, tooltipPosition: function () {
            var t = this.startAngle + (this.endAngle - this.startAngle) / 2, i = (this.outerRadius - this.innerRadius) / 2 + this.innerRadius;
            return { x: this.x + Math.cos(t) * i, y: this.y + Math.sin(t) * i };
        }, draw: function (t) {
            var i = this.ctx;
            i.beginPath(), i.arc(this.x, this.y, this.outerRadius, this.startAngle, this.endAngle), i.arc(this.x, this.y, this.innerRadius, this.endAngle, this.startAngle, !0), i.closePath(), i.strokeStyle = this.strokeColor, i.lineWidth = this.strokeWidth, i.fillStyle = this.fillColor, i.fill(), i.lineJoin = "bevel", this.showStroke && i.stroke();
        } }), e.Rectangle = e.Element.extend({ draw: function () {
            var t = this.ctx, i = this.width / 2, e = this.x - i, s = this.x + i, n = this.base - (this.base - this.y), o = this.strokeWidth / 2;
            this.showStroke && (e += o, s -= o, n += o), t.beginPath(), t.fillStyle = this.fillColor, t.strokeStyle = this.strokeColor, t.lineWidth = this.strokeWidth, t.moveTo(e, this.base), t.lineTo(e, n), t.lineTo(s, n), t.lineTo(s, this.base), t.fill(), this.showStroke && t.stroke();
        }, height: function () {
            return this.base - this.y;
        }, inRange: function (t, i) {
            return t >= this.x - this.width / 2 && t <= this.x + this.width / 2 && i >= this.y && i <= this.base;
        } }), e.Tooltip = e.Element.extend({ draw: function () {
            var t = this.chart.ctx;
            t.font = W(this.fontSize, this.fontStyle, this.fontFamily), this.xAlign = "center", this.yAlign = "above";
            var i = this.caretPadding = 2, e = t.measureText(this.text).width + 2 * this.xPadding, s = this.fontSize + 2 * this.yPadding, n = s + this.caretHeight + i;
            this.x + e / 2 > this.chart.width ? this.xAlign = "left" : this.x - e / 2 < 0 && (this.xAlign = "right"), this.y - n < 0 && (this.yAlign = "below");
            var o = this.x - e / 2, a = this.y - n;
            if (t.fillStyle = this.fillColor, this.custom)
                this.custom(this);
            else {
                switch (this.yAlign) {
                    case "above":
                        t.beginPath(), t.moveTo(this.x, this.y - i), t.lineTo(this.x + this.caretHeight, this.y - (i + this.caretHeight)), t.lineTo(this.x - this.caretHeight, this.y - (i + this.caretHeight)), t.closePath(), t.fill();
                        break;
                    case "below":
                        a = this.y + i + this.caretHeight, t.beginPath(), t.moveTo(this.x, this.y + i), t.lineTo(this.x + this.caretHeight, this.y + i + this.caretHeight), t.lineTo(this.x - this.caretHeight, this.y + i + this.caretHeight), t.closePath(), t.fill();
                }
                switch (this.xAlign) {
                    case "left":
                        o = this.x - e + (this.cornerRadius + this.caretHeight);
                        break;
                    case "right":
                        o = this.x - (this.cornerRadius + this.caretHeight);
                }
                B(t, o, a, e, s, this.cornerRadius), t.fill(), t.fillStyle = this.textColor, t.textAlign = "center", t.textBaseline = "middle", t.fillText(this.text, o + e / 2, a + s / 2);
            }
        } }), e.MultiTooltip = e.Element.extend({ initialize: function () {
            this.font = W(this.fontSize, this.fontStyle, this.fontFamily), this.titleFont = W(this.titleFontSize, this.titleFontStyle, this.titleFontFamily), this.height = this.labels.length * this.fontSize + (this.labels.length - 1) * (this.fontSize / 2) + 2 * this.yPadding + 1.5 * this.titleFontSize, this.ctx.font = this.titleFont;
            var t = this.ctx.measureText(this.title).width, i = z(this.ctx, this.font, this.labels) + this.fontSize + 3, e = g([i, t]);
            this.width = e + 2 * this.xPadding;
            var s = this.height / 2;
            this.y - s < 0 ? this.y = s : this.y + s > this.chart.height && (this.y = this.chart.height - s), this.x > this.chart.width / 2 ? this.x -= this.xOffset + this.width : this.x += this.xOffset;
        }, getLineHeight: function (t) {
            var i = this.y - this.height / 2 + this.yPadding, e = t - 1;
            return 0 === t ? i + this.titleFontSize / 2 : i + (1.5 * this.fontSize * e + this.fontSize / 2) + 1.5 * this.titleFontSize;
        }, draw: function () {
            if (this.custom)
                this.custom(this);
            else {
                B(this.ctx, this.x, this.y - this.height / 2, this.width, this.height, this.cornerRadius);
                var t = this.ctx;
                t.fillStyle = this.fillColor, t.fill(), t.closePath(), t.textAlign = "left", t.textBaseline = "middle", t.fillStyle = this.titleTextColor, t.font = this.titleFont, t.fillText(this.title, this.x + this.xPadding, this.getLineHeight(0)), t.font = this.font, s.each(this.labels, function (i, e) {
                    t.fillStyle = this.textColor, t.fillText(i, this.x + this.xPadding + this.fontSize + 3, this.getLineHeight(e + 1)), t.fillStyle = this.legendColorBackground, t.fillRect(this.x + this.xPadding, this.getLineHeight(e + 1) - this.fontSize / 2, this.fontSize, this.fontSize), t.fillStyle = this.legendColors[e].fill, t.fillRect(this.x + this.xPadding, this.getLineHeight(e + 1) - this.fontSize / 2, this.fontSize, this.fontSize);
                }, this);
            }
        } }), e.Scale = e.Element.extend({ initialize: function () {
            this.fit();
        }, buildYLabels: function () {
            this.yLabels = [];
            for (var t = v(this.stepValue), i = 0; i <= this.steps; i++)
                this.yLabels.push(w(this.templateString, { value: (this.min + i * this.stepValue).toFixed(t) }));
            this.yLabelWidth = this.display && this.showLabels ? z(this.ctx, this.font, this.yLabels) : 0;
        }, addXLabel: function (t) {
            this.xLabels.push(t), this.valuesCount++, this.fit();
        }, removeXLabel: function () {
            this.xLabels.shift(), this.valuesCount--, this.fit();
        }, fit: function () {
            this.startPoint = this.display ? this.fontSize : 0, this.endPoint = this.display ? this.height - 1.5 * this.fontSize - 5 : this.height, this.startPoint += this.padding, this.endPoint -= this.padding;
            var t, i = this.endPoint - this.startPoint;
            for (this.calculateYRange(i), this.buildYLabels(), this.calculateXLabelRotation(); i > this.endPoint - this.startPoint;)
                i = this.endPoint - this.startPoint, t = this.yLabelWidth, this.calculateYRange(i), this.buildYLabels(), t < this.yLabelWidth && this.calculateXLabelRotation();
        }, calculateXLabelRotation: function () {
            this.ctx.font = this.font;
            var t, i, e = this.ctx.measureText(this.xLabels[0]).width, s = this.ctx.measureText(this.xLabels[this.xLabels.length - 1]).width;
            if (this.xScalePaddingRight = s / 2 + 3, this.xScalePaddingLeft = e / 2 > this.yLabelWidth + 10 ? e / 2 : this.yLabelWidth + 10, this.xLabelRotation = 0, this.display) {
                var n, o = z(this.ctx, this.font, this.xLabels);
                this.xLabelWidth = o;
                for (var a = Math.floor(this.calculateX(1) - this.calculateX(0)) - 6; this.xLabelWidth > a && 0 === this.xLabelRotation || this.xLabelWidth > a && this.xLabelRotation <= 90 && this.xLabelRotation > 0;)
                    n = Math.cos(S(this.xLabelRotation)), t = n * e, i = n * s, t + this.fontSize / 2 > this.yLabelWidth + 8 && (this.xScalePaddingLeft = t + this.fontSize / 2), this.xScalePaddingRight = this.fontSize / 2, this.xLabelRotation++, this.xLabelWidth = n * o;
                this.xLabelRotation > 0 && (this.endPoint -= Math.sin(S(this.xLabelRotation)) * o + 3);
            } else
                this.xLabelWidth = 0, this.xScalePaddingRight = this.padding, this.xScalePaddingLeft = this.padding;
        }, calculateYRange: c, drawingArea: function () {
            return this.startPoint - this.endPoint;
        }, calculateY: function (t) {
            var i = this.drawingArea() / (this.min - this.max);
            return this.endPoint - i * (t - this.min);
        }, calculateX: function (t) {
            var i = (this.xLabelRotation > 0, this.width - (this.xScalePaddingLeft + this.xScalePaddingRight)), e = i / Math.max(this.valuesCount - (this.offsetGridLines ? 0 : 1), 1), s = e * t + this.xScalePaddingLeft;
            return this.offsetGridLines && (s += e / 2), Math.round(s);
        }, update: function (t) {
            s.extend(this, t), this.fit();
        }, draw: function () {
            var t = this.ctx, i = (this.endPoint - this.startPoint) / this.steps, e = Math.round(this.xScalePaddingLeft);
            this.display && (t.fillStyle = this.textColor, t.font = this.font, n(this.yLabels, function (n, o) {
                var a = this.endPoint - i * o, h = Math.round(a), l = this.showHorizontalLines;
                t.textAlign = "right", t.textBaseline = "middle", this.showLabels && t.fillText(n, e - 10, a), 0 !== o || l || (l = !0), l && t.beginPath(), o > 0 ? (t.lineWidth = this.gridLineWidth, t.strokeStyle = this.gridLineColor) : (t.lineWidth = this.lineWidth, t.strokeStyle = this.lineColor), h += s.aliasPixel(t.lineWidth), l && (t.moveTo(e, h), t.lineTo(this.width, h), t.stroke(), t.closePath()), t.lineWidth = this.lineWidth, t.strokeStyle = this.lineColor, t.beginPath(), t.moveTo(e - 5, h), t.lineTo(e, h), t.stroke(), t.closePath();
            }, this), n(this.xLabels, function (i, e) {
                var s = this.calculateX(e) + x(this.lineWidth), n = this.calculateX(e - (this.offsetGridLines ? .5 : 0)) + x(this.lineWidth), o = this.xLabelRotation > 0, a = this.showVerticalLines;
                0 !== e || a || (a = !0), a && t.beginPath(), e > 0 ? (t.lineWidth = this.gridLineWidth, t.strokeStyle = this.gridLineColor) : (t.lineWidth = this.lineWidth, t.strokeStyle = this.lineColor), a && (t.moveTo(n, this.endPoint), t.lineTo(n, this.startPoint - 3), t.stroke(), t.closePath()), t.lineWidth = this.lineWidth, t.strokeStyle = this.lineColor, t.beginPath(), t.moveTo(n, this.endPoint), t.lineTo(n, this.endPoint + 5), t.stroke(), t.closePath(), t.save(), t.translate(s, o ? this.endPoint + 12 : this.endPoint + 8), t.rotate(-1 * S(this.xLabelRotation)), t.font = this.font, t.textAlign = o ? "right" : "center", t.textBaseline = o ? "middle" : "top", t.fillText(i, 0, 0), t.restore();
            }, this));
        } }), e.RadialScale = e.Element.extend({ initialize: function () {
            this.size = m([this.height, this.width]), this.drawingArea = this.display ? this.size / 2 - (this.fontSize / 2 + this.backdropPaddingY) : this.size / 2;
        }, calculateCenterOffset: function (t) {
            var i = this.drawingArea / (this.max - this.min);
            return (t - this.min) * i;
        }, update: function () {
            this.lineArc ? this.drawingArea = this.display ? this.size / 2 - (this.fontSize / 2 + this.backdropPaddingY) : this.size / 2 : this.setScaleSize(), this.buildYLabels();
        }, buildYLabels: function () {
            this.yLabels = [];
            for (var t = v(this.stepValue), i = 0; i <= this.steps; i++)
                this.yLabels.push(w(this.templateString, { value: (this.min + i * this.stepValue).toFixed(t) }));
        }, getCircumference: function () {
            return 2 * Math.PI / this.valuesCount;
        }, setScaleSize: function () {
            var t, i, e, s, n, o, a, h, l, r, c, u, d = m([this.height / 2 - this.pointLabelFontSize - 5, this.width / 2]), p = this.width, g = 0;
            for (this.ctx.font = W(this.pointLabelFontSize, this.pointLabelFontStyle, this.pointLabelFontFamily), i = 0; i < this.valuesCount; i++)
                t = this.getPointPosition(i, d), e = this.ctx.measureText(w(this.templateString, { value: this.labels[i] })).width + 5, 0 === i || i === this.valuesCount / 2 ? (s = e / 2, t.x + s > p && (p = t.x + s, n = i), t.x - s < g && (g = t.x - s, a = i)) : i < this.valuesCount / 2 ? t.x + e > p && (p = t.x + e, n = i) : i > this.valuesCount / 2 && t.x - e < g && (g = t.x - e, a = i);
            l = g, r = Math.ceil(p - this.width), o = this.getIndexAngle(n), h = this.getIndexAngle(a), c = r / Math.sin(o + Math.PI / 2), u = l / Math.sin(h + Math.PI / 2), c = f(c) ? c : 0, u = f(u) ? u : 0, this.drawingArea = d - (u + c) / 2, this.setCenterPoint(u, c);
        }, setCenterPoint: function (t, i) {
            var e = this.width - i - this.drawingArea, s = t + this.drawingArea;
            this.xCenter = (s + e) / 2, this.yCenter = this.height / 2;
        }, getIndexAngle: function (t) {
            var i = 2 * Math.PI / this.valuesCount;
            return t * i - Math.PI / 2;
        }, getPointPosition: function (t, i) {
            var e = this.getIndexAngle(t);
            return { x: Math.cos(e) * i + this.xCenter, y: Math.sin(e) * i + this.yCenter };
        }, draw: function () {
            if (this.display) {
                var t = this.ctx;
                if (n(this.yLabels, function (i, e) {
                    if (e > 0) {
                        var s, n = e * (this.drawingArea / this.steps), o = this.yCenter - n;
                        if (this.lineWidth > 0)
                            if (t.strokeStyle = this.lineColor, t.lineWidth = this.lineWidth, this.lineArc)
                                t.beginPath(), t.arc(this.xCenter, this.yCenter, n, 0, 2 * Math.PI), t.closePath(), t.stroke();
                            else {
                                t.beginPath();
                                for (var a = 0; a < this.valuesCount; a++)
                                    s = this.getPointPosition(a, this.calculateCenterOffset(this.min + e * this.stepValue)), 0 === a ? t.moveTo(s.x, s.y) : t.lineTo(s.x, s.y);
                                t.closePath(), t.stroke();
                            }
                        if (this.showLabels) {
                            if (t.font = W(this.fontSize, this.fontStyle, this.fontFamily), this.showLabelBackdrop) {
                                var h = t.measureText(i).width;
                                t.fillStyle = this.backdropColor, t.fillRect(this.xCenter - h / 2 - this.backdropPaddingX, o - this.fontSize / 2 - this.backdropPaddingY, h + 2 * this.backdropPaddingX, this.fontSize + 2 * this.backdropPaddingY);
                            }
                            t.textAlign = "center", t.textBaseline = "middle", t.fillStyle = this.fontColor, t.fillText(i, this.xCenter, o);
                        }
                    }
                }, this), !this.lineArc) {
                    t.lineWidth = this.angleLineWidth, t.strokeStyle = this.angleLineColor;
                    for (var i = this.valuesCount - 1; i >= 0; i--) {
                        if (this.angleLineWidth > 0) {
                            var e = this.getPointPosition(i, this.calculateCenterOffset(this.max));
                            t.beginPath(), t.moveTo(this.xCenter, this.yCenter), t.lineTo(e.x, e.y), t.stroke(), t.closePath();
                        }
                        var s = this.getPointPosition(i, this.calculateCenterOffset(this.max) + 5);
                        t.font = W(this.pointLabelFontSize, this.pointLabelFontStyle, this.pointLabelFontFamily), t.fillStyle = this.pointLabelFontColor;
                        var o = this.labels.length, a = this.labels.length / 2, h = a / 2, l = h > i || i > o - h, r = i === h || i === o - h;
                        t.textAlign = 0 === i ? "center" : i === a ? "center" : a > i ? "left" : "right", t.textBaseline = r ? "middle" : l ? "bottom" : "top", t.fillText(this.labels[i], s.x, s.y);
                    }
                }
            }
        } }), s.addEvent(window, "resize", function () {
        var t;
        return function () {
            clearTimeout(t), t = setTimeout(function () {
                n(e.instances, function (t) {
                    t.options.responsive && t.resize(t.render, !0);
                });
            }, 50);
        };
    }()), p ? define(function () {
        return e;
    }) : "object" == typeof module && module.exports && (module.exports = e), t.Chart = e, e.noConflict = function () {
        return t.Chart = i, e;
    };
}.call(this), function () {
    "use strict";
    var t = this, i = t.Chart, e = i.helpers, s = { scaleBeginAtZero: !0, scaleShowGridLines: !0, scaleGridLineColor: "rgba(0,0,0,.05)", scaleGridLineWidth: 1, scaleShowHorizontalLines: !0, scaleShowVerticalLines: !0, barShowStroke: !0, barStrokeWidth: 2, barValueSpacing: 5, barDatasetSpacing: 1, legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<datasets.length; i++){%><li><span style="background-color:<%=datasets[i].fillColor%>"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>' };
    i.Type.extend({
        name: "Bar", defaults: s, initialize: function (t) {
            var s = this.options;
            this.ScaleClass = i.Scale.extend({ offsetGridLines: !0, calculateBarX: function (t, i, e) {
                    var n = this.calculateBaseWidth(), o = this.calculateX(e) - n / 2, a = this.calculateBarWidth(t);
                    return o + a * i + i * s.barDatasetSpacing + a / 2;
                }, calculateBaseWidth: function () {
                    return this.calculateX(1) - this.calculateX(0) - 2 * s.barValueSpacing;
                }, calculateBarWidth: function (t) {
                    var i = this.calculateBaseWidth() - (t - 1) * s.barDatasetSpacing;
                    return i / t;
                } }), this.datasets = [], this.options.showTooltips && e.bindEvents(this, this.options.tooltipEvents, function (t) {
                var i = "mouseout" !== t.type ? this.getBarsAtEvent(t) : [];
                this.eachBars(function (t) {
                    t.restore(["fillColor", "strokeColor"]);
                }), e.each(i, function (t) {
                    t.fillColor = t.highlightFill, t.strokeColor = t.highlightStroke;
                }), this.showTooltip(i);
            }), this.BarClass = i.Rectangle.extend({ strokeWidth: this.options.barStrokeWidth, showStroke: this.options.barShowStroke, ctx: this.chart.ctx }), e.each(t.datasets, function (i) {
                var s = { label: i.label || null, fillColor: i.fillColor, strokeColor: i.strokeColor, bars: [] };
                this.datasets.push(s), e.each(i.data, function (e, n) {
                    s.bars.push(new this.BarClass({ value: e, label: t.labels[n], customLabel: t.customLabels ? t.customLabels[n] : "", datasetLabel: i.label, strokeColor: i.strokeColor, fillColor: i.data[n] < 0 && i.negativeColor ? i.negativeColor : i.fillColor, highlightFill: i.highlightFill || i.fillColor, highlightStroke: i.highlightStroke || i.strokeColor }));
                }, this);
            }, this), this.buildScale(t.labels), this.BarClass.prototype.base = this.scale.endPoint, this.eachBars(function (t, i, s) {
                e.extend(t, { width: this.scale.calculateBarWidth(this.datasets.length), x: this.scale.calculateBarX(this.datasets.length, s, i), y: this.scale.endPoint }), t.save();
            }, this), this.render();
        }, update: function () {
            this.scale.update(), e.each(this.activeElements, function (t) {
                t.restore(["fillColor", "strokeColor"]);
            }), this.eachBars(function (t) {
                t.save();
            }), this.render();
        }, eachBars: function (t) {
            e.each(this.datasets, function (i, s) {
                e.each(i.bars, t, this, s);
            }, this);
        }, getBarsAtEvent: function (t) {
            for (var i, s = [], n = e.getRelativePosition(t), o = function (t) {
                s.push(t.bars[i]);
            }, a = 0; a < this.datasets.length; a++)
                for (i = 0; i < this.datasets[a].bars.length; i++)
                    if (this.datasets[a].bars[i].inRange(n.x, n.y))
                        return e.each(this.datasets, o), s;
            return s;
        }, buildScale: function (t) {
            var i = this, s = function () {
                var t = [];
                return i.eachBars(function (i) {
                    t.push(i.value);
                }), t;
            }, n = { templateString: this.options.scaleLabel, height: this.chart.height, width: this.chart.width, ctx: this.chart.ctx, textColor: this.options.scaleFontColor, fontSize: this.options.scaleFontSize, fontStyle: this.options.scaleFontStyle, fontFamily: this.options.scaleFontFamily, valuesCount: t.length, beginAtZero: this.options.scaleBeginAtZero, integersOnly: this.options.scaleIntegersOnly, calculateYRange: function (t) {
                    var i = e.calculateScaleRange(s(), t, this.fontSize, this.beginAtZero, this.integersOnly);
                    e.extend(this, i);
                }, xLabels: t, font: e.fontString(this.options.scaleFontSize, this.options.scaleFontStyle, this.options.scaleFontFamily), lineWidth: this.options.scaleLineWidth, lineColor: this.options.scaleLineColor, showHorizontalLines: this.options.scaleShowHorizontalLines, showVerticalLines: this.options.scaleShowVerticalLines, gridLineWidth: this.options.scaleShowGridLines ? this.options.scaleGridLineWidth : 0, gridLineColor: this.options.scaleShowGridLines ? this.options.scaleGridLineColor : "rgba(0,0,0,0)", padding: this.options.showScale ? 0 : this.options.barShowStroke ? this.options.barStrokeWidth : 0, showLabels: this.options.scaleShowLabels, display: this.options.showScale };
            this.options.scaleOverride && e.extend(n, { calculateYRange: e.noop, steps: this.options.scaleSteps, stepValue: this.options.scaleStepWidth, min: this.options.scaleStartValue, max: this.options.scaleStartValue + this.options.scaleSteps * this.options.scaleStepWidth }), this.scale = new this.ScaleClass(n);
        }, addData: function (t, i) {
            e.each(t, function (t, e) {
                this.datasets[e].bars.push(new this.BarClass({ value: t, label: i, x: this.scale.calculateBarX(this.datasets.length, e, this.scale.valuesCount + 1), y: this.scale.endPoint, width: this.scale.calculateBarWidth(this.datasets.length), base: this.scale.endPoint, strokeColor: this.datasets[e].strokeColor, fillColor: this.datasets[e].fillColor }));
            }, this), this.scale.addXLabel(i), this.update();
        }, removeData: function () {
            this.scale.removeXLabel(), e.each(this.datasets, function (t) {
                t.bars.shift();
            }, this), this.update();
        }, reflow: function () {
            e.extend(this.BarClass.prototype, { y: this.scale.endPoint, base: this.scale.endPoint });
            var t = e.extend({ height: this.chart.height, width: this.chart.width });
            this.scale.update(t);
        }, draw: function (t) {
            var i = t || 1;
            this.clear();
            this.chart.ctx;
            this.scale.draw(i), e.each(this.datasets, function (t, s) {
                e.each(t.bars, function (t, e) {
                    t.hasValue() && (t.base = this.scale.endPoint, t.transition({ x: this.scale.calculateBarX(this.datasets.length, s, e), y: this.scale.calculateY(t.value), width: this.scale.calculateBarWidth(this.datasets.length) }, i).draw());
                }, this);
            }, this);
        } });
}.call(this), function () {
    "use strict";
    var t = this, i = t.Chart, e = i.helpers, s = { segmentShowStroke: !0, segmentStrokeColor: "#fff", segmentStrokeWidth: 2, percentageInnerCutout: 50, animationSteps: 100, animationEasing: "easeOutBounce", animateRotate: !0, animateScale: !1, legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<segments.length; i++){%><li><span style="background-color:<%=segments[i].fillColor%>"></span><%if(segments[i].label){%><%=segments[i].label%><%}%></li><%}%></ul>' };
    i.Type.extend({ name: "Doughnut", defaults: s, initialize: function (t) {
            this.segments = [], this.outerRadius = (e.min([this.chart.width, this.chart.height]) - this.options.segmentStrokeWidth / 2) / 2, this.SegmentArc = i.Arc.extend({ ctx: this.chart.ctx, x: this.chart.width / 2, y: this.chart.height / 2 }), this.options.showTooltips && e.bindEvents(this, this.options.tooltipEvents, function (t) {
                var i = "mouseout" !== t.type ? this.getSegmentsAtEvent(t) : [];
                e.each(this.segments, function (t) {
                    t.restore(["fillColor"]);
                }), e.each(i, function (t) {
                    t.fillColor = t.highlightColor;
                }), this.showTooltip(i);
            });
            var s = !!window.ActiveXObject, n = s && !window.XMLHttpRequest, o = s && !!document.documentMode, a = s && !n && !o;
            if (n || a || o) {
                var h = 0;
                e.each(t, function (t) {
                    h += parseFloat(t.value), t.value = parseFloat(t.value) + 1e-5;
                }, this);
            }
            this.calculateTotal(t), e.each(t, function (t, i) {
                this.addData(t, i, !0);
            }, this), (n || a || o) && 0 == h || this.render();
        }, getSegmentsAtEvent: function (t) {
            var i = [], s = e.getRelativePosition(t);
            return e.each(this.segments, function (t) {
                t.inRange(s.x, s.y) && i.push(t);
            }, this), i;
        }, addData: function (t, i, e) {
            var s = i || this.segments.length;
            this.segments.splice(s, 0, new this.SegmentArc({ value: t.value, outerRadius: this.options.animateScale ? 0 : this.outerRadius, innerRadius: this.options.animateScale ? 0 : this.outerRadius / 100 * this.options.percentageInnerCutout, fillColor: t.color, highlightColor: t.highlight || t.color, showStroke: this.options.segmentShowStroke, strokeWidth: this.options.segmentStrokeWidth, strokeColor: this.options.segmentStrokeColor, startAngle: 1.5 * Math.PI, circumference: this.options.animateRotate ? 0 : this.calculateCircumference(t.value), label: t.label })), e || (this.reflow(), this.update());
        }, calculateCircumference: function (t) {
            return 2 * Math.PI * (Math.abs(t) / this.total);
        }, calculateTotal: function (t) {
            this.total = 0, e.each(t, function (t) {
                this.total += Math.abs(t.value);
            }, this);
        }, update: function () {
            this.calculateTotal(this.segments), e.each(this.activeElements, function (t) {
                t.restore(["fillColor"]);
            }), e.each(this.segments, function (t) {
                t.save();
            }), this.render();
        }, removeData: function (t) {
            var i = e.isNumber(t) ? t : this.segments.length - 1;
            this.segments.splice(i, 1), this.reflow(), this.update();
        }, reflow: function () {
            e.extend(this.SegmentArc.prototype, { x: this.chart.width / 2, y: this.chart.height / 2 }), this.outerRadius = (e.min([this.chart.width, this.chart.height]) - this.options.segmentStrokeWidth / 2) / 2, e.each(this.segments, function (t) {
                t.update({ outerRadius: this.outerRadius, innerRadius: this.outerRadius / 100 * this.options.percentageInnerCutout });
            }, this);
        }, draw: function (t) {
            var i = t ? t : 1;
            this.clear(), e.each(this.segments, function (t, e) {
                t.transition({ circumference: this.calculateCircumference(t.value), outerRadius: this.outerRadius, innerRadius: this.outerRadius / 100 * this.options.percentageInnerCutout }, i), t.endAngle = t.startAngle + t.circumference, t.draw(), 0 === e && (t.startAngle = 1.5 * Math.PI), e < this.segments.length - 1 && (this.segments[e + 1].startAngle = t.endAngle);
            }, this);
        } }), i.types.Doughnut.extend({ name: "Pie", defaults: e.merge(s, { percentageInnerCutout: 0 }) });
}.call(this), function () {
    "use strict";
    var t = this, i = t.Chart, e = i.helpers, s = { scaleShowGridLines: !0, scaleGridLineColor: "rgba(0,0,0,.05)", scaleGridLineWidth: 1, scaleShowHorizontalLines: !0, scaleShowVerticalLines: !0, bezierCurve: !0, bezierCurveTension: .4, pointDot: !0, pointDotRadius: 4, pointDotStrokeWidth: 1, pointHitDetectionRadius: 20, datasetStroke: !0, datasetStrokeWidth: 2, datasetFill: !0, legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<datasets.length; i++){%><li><span style="background-color:<%=datasets[i].strokeColor%>"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>' };
    i.Type.extend({ name: "Line", defaults: s, initialize: function (t) {
            this.PointClass = i.Point.extend({ strokeWidth: this.options.pointDotStrokeWidth, radius: this.options.pointDotRadius, display: this.options.pointDot, hitDetectionRadius: this.options.pointHitDetectionRadius, ctx: this.chart.ctx, inRange: function (t) {
                    return Math.pow(t - this.x, 2) < Math.pow(this.radius + this.hitDetectionRadius, 2);
                } }), this.datasets = [], this.options.showTooltips && e.bindEvents(this, this.options.tooltipEvents, function (t) {
                var i = "mouseout" !== t.type ? this.getPointsAtEvent(t) : [];
                this.eachPoints(function (t) {
                    t.restore(["fillColor", "strokeColor"]);
                }), e.each(i, function (t) {
                    t.fillColor = t.highlightFill, t.strokeColor = t.highlightStroke;
                }), this.showTooltip(i);
            }), e.each(t.datasets, function (i) {
                var s = { label: i.label || null, fillColor: i.fillColor, strokeColor: i.strokeColor, pointColor: i.pointColor, pointStrokeColor: i.pointStrokeColor, points: [] };
                this.datasets.push(s), e.each(i.data, function (e, n) {
                    s.points.push(new this.PointClass({ value: e, label: t.labels[n], datasetLabel: i.label, customLabel: t.customLabels[n], strokeColor: i.pointStrokeColor, fillColor: i.pointColor, highlightFill: i.pointHighlightFill || i.pointColor, highlightStroke: i.pointHighlightStroke || i.pointStrokeColor }));
                }, this), this.buildScale(t.labels), this.eachPoints(function (t, i) {
                    e.extend(t, { x: this.scale.calculateX(i), y: this.scale.endPoint }), t.save();
                }, this);
            }, this), this.render();
        }, update: function () {
            this.scale.update(), e.each(this.activeElements, function (t) {
                t.restore(["fillColor", "strokeColor"]);
            }), this.eachPoints(function (t) {
                t.save();
            }), this.render();
        }, eachPoints: function (t) {
            e.each(this.datasets, function (i) {
                e.each(i.points, t, this);
            }, this);
        }, getPointsAtEvent: function (t) {
            var i = [], s = e.getRelativePosition(t);
            return e.each(this.datasets, function (t) {
                e.each(t.points, function (t) {
                    t.inRange(s.x, s.y) && i.push(t);
                });
            }, this), i;
        }, buildScale: function (t) {
            var s = this, n = function () {
                var t = [];
                return s.eachPoints(function (i) {
                    t.push(i.value);
                }), t;
            }, o = { templateString: this.options.scaleLabel, height: this.chart.height, width: this.chart.width, ctx: this.chart.ctx, textColor: this.options.scaleFontColor, fontSize: this.options.scaleFontSize, fontStyle: this.options.scaleFontStyle, fontFamily: this.options.scaleFontFamily, valuesCount: t.length, beginAtZero: this.options.scaleBeginAtZero, integersOnly: this.options.scaleIntegersOnly, calculateYRange: function (t) {
                    var i = e.calculateScaleRange(n(), t, this.fontSize, this.beginAtZero, this.integersOnly);
                    e.extend(this, i);
                }, xLabels: t, font: e.fontString(this.options.scaleFontSize, this.options.scaleFontStyle, this.options.scaleFontFamily), lineWidth: this.options.scaleLineWidth, lineColor: this.options.scaleLineColor, showHorizontalLines: this.options.scaleShowHorizontalLines, showVerticalLines: this.options.scaleShowVerticalLines, gridLineWidth: this.options.scaleShowGridLines ? this.options.scaleGridLineWidth : 0, gridLineColor: this.options.scaleShowGridLines ? this.options.scaleGridLineColor : "rgba(0,0,0,0)", padding: this.options.showScale ? 0 : this.options.pointDotRadius + this.options.pointDotStrokeWidth, showLabels: this.options.scaleShowLabels, display: this.options.showScale };
            this.options.scaleOverride && e.extend(o, { calculateYRange: e.noop, steps: this.options.scaleSteps, stepValue: this.options.scaleStepWidth, min: this.options.scaleStartValue, max: this.options.scaleStartValue + this.options.scaleSteps * this.options.scaleStepWidth }), this.scale = new i.Scale(o);
        }, addData: function (t, i) {
            e.each(t, function (t, e) {
                this.datasets[e].points.push(new this.PointClass({ value: t, label: i, x: this.scale.calculateX(this.scale.valuesCount + 1), y: this.scale.endPoint, strokeColor: this.datasets[e].pointStrokeColor, fillColor: this.datasets[e].pointColor }));
            }, this), this.scale.addXLabel(i), this.update();
        }, removeData: function () {
            this.scale.removeXLabel(), e.each(this.datasets, function (t) {
                t.points.shift();
            }, this), this.update();
        }, reflow: function () {
            var t = e.extend({ height: this.chart.height, width: this.chart.width });
            this.scale.update(t);
        }, draw: function (t) {
            var i = t || 1;
            this.clear();
            var s = this.chart.ctx, n = function (t) {
                return null !== t.value;
            }, o = function (t, i, s) {
                return e.findNextWhere(i, n, s) || t;
            }, a = function (t, i, s) {
                return e.findPreviousWhere(i, n, s) || t;
            };
            this.scale.draw(i), e.each(this.datasets, function (t) {
                var h = e.where(t.points, n);
                e.each(t.points, function (t, e) {
                    t.hasValue() && t.transition({ y: this.scale.calculateY(t.value), x: this.scale.calculateX(e) }, i);
                }, this), this.options.bezierCurve && e.each(h, function (t, i) {
                    var s = i > 0 && i < h.length - 1 ? this.options.bezierCurveTension : 0;
                    t.controlPoints = e.splineCurve(a(t, h, i), t, o(t, h, i), s), t.controlPoints.outer.y > this.scale.endPoint ? t.controlPoints.outer.y = this.scale.endPoint : t.controlPoints.outer.y < this.scale.startPoint && (t.controlPoints.outer.y = this.scale.startPoint), t.controlPoints.inner.y > this.scale.endPoint ? t.controlPoints.inner.y = this.scale.endPoint : t.controlPoints.inner.y < this.scale.startPoint && (t.controlPoints.inner.y = this.scale.startPoint);
                }, this), s.lineWidth = this.options.datasetStrokeWidth, s.strokeStyle = t.strokeColor, s.beginPath(), e.each(h, function (t, i) {
                    if (0 === i)
                        s.moveTo(t.x, t.y);
                    else if (this.options.bezierCurve) {
                        var e = a(t, h, i);
                        s.bezierCurveTo(e.controlPoints.outer.x, e.controlPoints.outer.y, t.controlPoints.inner.x, t.controlPoints.inner.y, t.x, t.y);
                    } else
                        s.lineTo(t.x, t.y);
                }, this), s.stroke(), this.options.datasetFill && h.length > 0 && (s.lineTo(h[h.length - 1].x, this.scale.endPoint), s.lineTo(h[0].x, this.scale.endPoint), s.fillStyle = t.fillColor, s.closePath(), s.fill()), e.each(h, function (t) {
                    t.draw();
                });
            }, this);
        } });
}.call(this), function () {
    "use strict";
    var t = this, i = t.Chart, e = i.helpers, s = { scaleShowLabelBackdrop: !0, scaleBackdropColor: "rgba(255,255,255,0.75)", scaleBeginAtZero: !0, scaleBackdropPaddingY: 2, scaleBackdropPaddingX: 2, scaleShowLine: !0, segmentShowStroke: !0, segmentStrokeColor: "#fff", segmentStrokeWidth: 2, animationSteps: 100, animationEasing: "easeOutBounce", animateRotate: !0, animateScale: !1, legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<segments.length; i++){%><li><span style="background-color:<%=segments[i].fillColor%>"></span><%if(segments[i].label){%><%=segments[i].label%><%}%></li><%}%></ul>' };
    i.Type.extend({ name: "PolarArea", defaults: s, initialize: function (t) {
            this.segments = [], this.SegmentArc = i.Arc.extend({ showStroke: this.options.segmentShowStroke, strokeWidth: this.options.segmentStrokeWidth, strokeColor: this.options.segmentStrokeColor, ctx: this.chart.ctx, innerRadius: 0, x: this.chart.width / 2, y: this.chart.height / 2 }), this.scale = new i.RadialScale({ display: this.options.showScale, fontStyle: this.options.scaleFontStyle, fontSize: this.options.scaleFontSize, fontFamily: this.options.scaleFontFamily, fontColor: this.options.scaleFontColor, showLabels: this.options.scaleShowLabels, showLabelBackdrop: this.options.scaleShowLabelBackdrop, backdropColor: this.options.scaleBackdropColor, backdropPaddingY: this.options.scaleBackdropPaddingY, backdropPaddingX: this.options.scaleBackdropPaddingX, lineWidth: this.options.scaleShowLine ? this.options.scaleLineWidth : 0, lineColor: this.options.scaleLineColor, lineArc: !0, width: this.chart.width, height: this.chart.height, xCenter: this.chart.width / 2, yCenter: this.chart.height / 2, ctx: this.chart.ctx, templateString: this.options.scaleLabel, valuesCount: t.length }), this.updateScaleRange(t), this.scale.update(), e.each(t, function (t, i) {
                this.addData(t, i, !0);
            }, this), this.options.showTooltips && e.bindEvents(this, this.options.tooltipEvents, function (t) {
                var i = "mouseout" !== t.type ? this.getSegmentsAtEvent(t) : [];
                e.each(this.segments, function (t) {
                    t.restore(["fillColor"]);
                }), e.each(i, function (t) {
                    t.fillColor = t.highlightColor;
                }), this.showTooltip(i);
            }), this.render();
        }, getSegmentsAtEvent: function (t) {
            var i = [], s = e.getRelativePosition(t);
            return e.each(this.segments, function (t) {
                t.inRange(s.x, s.y) && i.push(t);
            }, this), i;
        }, addData: function (t, i, e) {
            var s = i || this.segments.length;
            this.segments.splice(s, 0, new this.SegmentArc({ fillColor: t.color, highlightColor: t.highlight || t.color, label: t.label, value: t.value, outerRadius: this.options.animateScale ? 0 : this.scale.calculateCenterOffset(t.value), circumference: this.options.animateRotate ? 0 : this.scale.getCircumference(), startAngle: 1.5 * Math.PI })), e || (this.reflow(), this.update());
        }, removeData: function (t) {
            var i = e.isNumber(t) ? t : this.segments.length - 1;
            this.segments.splice(i, 1), this.reflow(), this.update();
        }, calculateTotal: function (t) {
            this.total = 0, e.each(t, function (t) {
                this.total += t.value;
            }, this), this.scale.valuesCount = this.segments.length;
        }, updateScaleRange: function (t) {
            var i = [];
            e.each(t, function (t) {
                i.push(t.value);
            });
            var s = this.options.scaleOverride ? { steps: this.options.scaleSteps, stepValue: this.options.scaleStepWidth, min: this.options.scaleStartValue, max: this.options.scaleStartValue + this.options.scaleSteps * this.options.scaleStepWidth } : e.calculateScaleRange(i, e.min([this.chart.width, this.chart.height]) / 2, this.options.scaleFontSize, this.options.scaleBeginAtZero, this.options.scaleIntegersOnly);
            e.extend(this.scale, s, { size: e.min([this.chart.width, this.chart.height]), xCenter: this.chart.width / 2, yCenter: this.chart.height / 2 });
        }, update: function () {
            this.calculateTotal(this.segments), e.each(this.segments, function (t) {
                t.save();
            }), this.reflow(), this.render();
        }, reflow: function () {
            e.extend(this.SegmentArc.prototype, { x: this.chart.width / 2, y: this.chart.height / 2 }), this.updateScaleRange(this.segments), this.scale.update(), e.extend(this.scale, { xCenter: this.chart.width / 2, yCenter: this.chart.height / 2 }), e.each(this.segments, function (t) {
                t.update({ outerRadius: this.scale.calculateCenterOffset(t.value) });
            }, this);
        }, draw: function (t) {
            var i = t || 1;
            this.clear(), e.each(this.segments, function (t, e) {
                t.transition({ circumference: this.scale.getCircumference(), outerRadius: this.scale.calculateCenterOffset(t.value) }, i), t.endAngle = t.startAngle + t.circumference, 0 === e && (t.startAngle = 1.5 * Math.PI), e < this.segments.length - 1 && (this.segments[e + 1].startAngle = t.endAngle), t.draw();
            }, this), this.scale.draw();
        } });
}.call(this), function () {
    "use strict";
    var t = this, i = t.Chart, e = i.helpers;
    i.Type.extend({ name: "Radar", defaults: { scaleShowLine: !0, angleShowLineOut: !0, scaleShowLabels: !1, scaleBeginAtZero: !0, angleLineColor: "rgba(0,0,0,.1)", angleLineWidth: 1, pointLabelFontFamily: "'Arial'", pointLabelFontStyle: "normal", pointLabelFontSize: 10, pointLabelFontColor: "#666", pointDot: !0, pointDotRadius: 3, pointDotStrokeWidth: 1, pointHitDetectionRadius: 20, datasetStroke: !0, datasetStrokeWidth: 2, datasetFill: !0, legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<datasets.length; i++){%><li><span style="background-color:<%=datasets[i].strokeColor%>"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>' }, initialize: function (t) {
            this.PointClass = i.Point.extend({ strokeWidth: this.options.pointDotStrokeWidth, radius: this.options.pointDotRadius, display: this.options.pointDot, hitDetectionRadius: this.options.pointHitDetectionRadius, ctx: this.chart.ctx }), this.datasets = [], this.buildScale(t), this.options.showTooltips && e.bindEvents(this, this.options.tooltipEvents, function (t) {
                var i = "mouseout" !== t.type ? this.getPointsAtEvent(t) : [];
                this.eachPoints(function (t) {
                    t.restore(["fillColor", "strokeColor"]);
                }), e.each(i, function (t) {
                    t.fillColor = t.highlightFill, t.strokeColor = t.highlightStroke;
                }), this.showTooltip(i);
            }), e.each(t.datasets, function (i) {
                var s = { label: i.label || null, fillColor: i.fillColor, strokeColor: i.strokeColor, pointColor: i.pointColor, pointStrokeColor: i.pointStrokeColor, points: [] };
                this.datasets.push(s), e.each(i.data, function (e, n) {
                    var o;
                    this.scale.animation || (o = this.scale.getPointPosition(n, this.scale.calculateCenterOffset(e))), s.points.push(new this.PointClass({ value: e, label: t.labels[n], datasetLabel: i.label, x: this.options.animation ? this.scale.xCenter : o.x, y: this.options.animation ? this.scale.yCenter : o.y, strokeColor: i.pointStrokeColor, fillColor: i.pointColor, highlightFill: i.pointHighlightFill || i.pointColor, highlightStroke: i.pointHighlightStroke || i.pointStrokeColor }));
                }, this);
            }, this), this.render();
        }, eachPoints: function (t) {
            e.each(this.datasets, function (i) {
                e.each(i.points, t, this);
            }, this);
        }, getPointsAtEvent: function (t) {
            var i = e.getRelativePosition(t), s = e.getAngleFromPoint({ x: this.scale.xCenter, y: this.scale.yCenter }, i), n = 2 * Math.PI / this.scale.valuesCount, o = Math.round((s.angle - 1.5 * Math.PI) / n), a = [];
            return (o >= this.scale.valuesCount || 0 > o) && (o = 0), s.distance <= this.scale.drawingArea && e.each(this.datasets, function (t) {
                a.push(t.points[o]);
            }), a;
        }, buildScale: function (t) {
            this.scale = new i.RadialScale({ display: this.options.showScale, fontStyle: this.options.scaleFontStyle, fontSize: this.options.scaleFontSize, fontFamily: this.options.scaleFontFamily, fontColor: this.options.scaleFontColor, showLabels: this.options.scaleShowLabels, showLabelBackdrop: this.options.scaleShowLabelBackdrop, backdropColor: this.options.scaleBackdropColor, backdropPaddingY: this.options.scaleBackdropPaddingY, backdropPaddingX: this.options.scaleBackdropPaddingX, lineWidth: this.options.scaleShowLine ? this.options.scaleLineWidth : 0, lineColor: this.options.scaleLineColor, angleLineColor: this.options.angleLineColor, angleLineWidth: this.options.angleShowLineOut ? this.options.angleLineWidth : 0, pointLabelFontColor: this.options.pointLabelFontColor, pointLabelFontSize: this.options.pointLabelFontSize, pointLabelFontFamily: this.options.pointLabelFontFamily, pointLabelFontStyle: this.options.pointLabelFontStyle, height: this.chart.height, width: this.chart.width, xCenter: this.chart.width / 2, yCenter: this.chart.height / 2, ctx: this.chart.ctx, templateString: this.options.scaleLabel, labels: t.labels, valuesCount: t.datasets[0].data.length }), this.scale.setScaleSize(), this.updateScaleRange(t.datasets), this.scale.buildYLabels();
        }, updateScaleRange: function (t) {
            var i = function () {
                var i = [];
                return e.each(t, function (t) {
                    t.data ? i = i.concat(t.data) : e.each(t.points, function (t) {
                        i.push(t.value);
                    });
                }), i;
            }(), s = this.options.scaleOverride ? { steps: this.options.scaleSteps, stepValue: this.options.scaleStepWidth, min: this.options.scaleStartValue, max: this.options.scaleStartValue + this.options.scaleSteps * this.options.scaleStepWidth } : e.calculateScaleRange(i, e.min([this.chart.width, this.chart.height]) / 2, this.options.scaleFontSize, this.options.scaleBeginAtZero, this.options.scaleIntegersOnly);
            e.extend(this.scale, s);
        }, addData: function (t, i) {
            this.scale.valuesCount++, e.each(t, function (t, e) {
                var s = this.scale.getPointPosition(this.scale.valuesCount, this.scale.calculateCenterOffset(t));
                this.datasets[e].points.push(new this.PointClass({ value: t, label: i, x: s.x, y: s.y, strokeColor: this.datasets[e].pointStrokeColor, fillColor: this.datasets[e].pointColor }));
            }, this), this.scale.labels.push(i), this.reflow(), this.update();
        }, removeData: function () {
            this.scale.valuesCount--, this.scale.labels.shift(), e.each(this.datasets, function (t) {
                t.points.shift();
            }, this), this.reflow(), this.update();
        }, update: function () {
            this.eachPoints(function (t) {
                t.save();
            }), this.reflow(), this.render();
        }, reflow: function () {
            e.extend(this.scale, { width: this.chart.width, height: this.chart.height, size: e.min([this.chart.width, this.chart.height]), xCenter: this.chart.width / 2, yCenter: this.chart.height / 2 }), this.updateScaleRange(this.datasets), this.scale.setScaleSize(), this.scale.buildYLabels();
        }, draw: function (t) {
            var i = t || 1, s = this.chart.ctx;
            this.clear(), this.scale.draw(), e.each(this.datasets, function (t) {
                e.each(t.points, function (t, e) {
                    t.hasValue() && t.transition(this.scale.getPointPosition(e, this.scale.calculateCenterOffset(t.value)), i);
                }, this), s.lineWidth = this.options.datasetStrokeWidth, s.strokeStyle = t.strokeColor, s.beginPath(), e.each(t.points, function (t, i) {
                    0 === i ? s.moveTo(t.x, t.y) : s.lineTo(t.x, t.y);
                }, this), s.closePath(), s.stroke(), s.fillStyle = t.fillColor, s.fill(), e.each(t.points, function (t) {
                    t.hasValue() && t.draw();
                });
            }, this);
        } });
}.call(this);
;
angular.module("combox", []).directive("combox", ["$log", function () {
        return { restrict: "A", template: '<div class="combox-input" ng-click="optionsToggle()"><div ng-class="{\'defaultState\':nonSelected}" title="{{selectTitle}} {{selectSubtitle}}">{{selectTitle}}&nbsp;<span ng-if="selectSubtitle" class="{{optionsData.config.subtitleStyle}}">{{selectSubtitle}}</span></div></div><dl ng-show="showOptions"><dd ng-repeat="option in optionsData.data" ng-click="selectChange(option)" title="{{option.title}}">{{option.title}}&nbsp;<span ng-if="option.subtitle" class="{{optionsData.config.subtitleStyle}}">{{option.subtitle}}</span></dd></dl>', scope: { formObject: "=", optionsData: "=", defaultValue: "=", onChange: "&" }, link: function (t, e, o) {
                function n(o) {
                    t.showOptions = o, o ? e.addClass("comboxPosition") : e.removeClass("comboxPosition");
                }
                var i = !1;
                t.selectTitle = o.selectPlaceholder || "请选择", t.nonSelected = !0, t.$watch("optionsData", function () {
                    i = !1;
                    var n = t.optionsData ? t.optionsData.data : [];
                    if (t.defaultValue)
                        for (var l = 0; l < n.length; l++) {
                            var a = n[l];
                            if (a.value == t.defaultValue) {
                                t.selectChange(a), i = !0;
                                break;
                            }
                        }
                    i || (t.selectChange({ title: "", value: "" }, !1), t.formObject.$setPristine(), t.selectTitle = o.selectPlaceholder || "请选择", t.nonSelected = !0), t.optionsData && t.optionsData.config && t.optionsData.config.width && (e.find(".combox-input").width(t.optionsData.config.width), e.find("dl").width(t.optionsData.config.width + 22), e.find(".combox-input").css("background-position", t.optionsData.config.width));
                }), t.selectChange = function (e, o) {
                    t.selectTitle = e.title, t.selectSubtitle = e.subtitle, t.formObject.$setViewValue(e.value), o !== !1 && t.onChange({ selectedOption: e }), n(!1), t.nonSelected = !1;
                }, t.optionsToggle = function () {
                    n(!t.showOptions);
                }, e.on("mouseleave", function () {
                    n(!1), t.$digest();
                });
                var l = e.find("dl")[0];
                /firefox/.test(window.navigator.userAgent.toLowerCase()) ? l.addEventListener("DOMMouseScroll", function (t) {
                    l.scrollTop += t.detail > 0 ? 60 : -60, t.preventDefault();
                }, !1) : l.onmousewheel = function (t) {
                    return t = t || window.event, l.scrollTop += t.wheelDelta > 0 ? -60 : 60, t.returnValue = !1, !1;
                };
            } };
    }]);
;
angular.module("customInput", []).directive("customInput", ["$log", "$parse", function (A, M) {
        return { restrict: "A", require: "ngModel", link: function (A, I, B) {
                function Q(A, M) {
                    M ? (A.attr("src", D), A.addClass("custom-checked")) : (A.attr("src", R), A.removeClass("custom-checked"));
                }
                function E(A, M) {
                    B.value == M ? A.attr("src", c) : A.attr("src", w);
                }
                var D = "data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMraHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjMtYzAxMSA2Ni4xNDU2NjEsIDIwMTIvMDIvMDYtMTQ6NTY6MjcgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzYgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjFEOUQ2NEJFM0MwQjExRTVBNzcyQzY2QUMyNDhCRTI0IiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjFEOUQ2NEJGM0MwQjExRTVBNzcyQzY2QUMyNDhCRTI0Ij4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6MUQ5RDY0QkMzQzBCMTFFNUE3NzJDNjZBQzI0OEJFMjQiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6MUQ5RDY0QkQzQzBCMTFFNUE3NzJDNjZBQzI0OEJFMjQiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAgICAgICAgICAgIDAwMDAwMDAwMDAQEBAQEBAQIBAQICAgECAgMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwP/wAARCAAUABQDAREAAhEBAxEB/8QAkAAAAgMBAAAAAAAAAAAAAAAABwgDBAUJAQABBQEBAAAAAAAAAAAAAAAGAwQFBwgCChAAAAQDBwMDBQEAAAAAAAAAAgMEBQEGBxESExQVFhcAIQgiIxhjhJRlRigRAAECBAQEAwQLAQAAAAAAAAECAxESBAUAIRMGMRQVB2EWFyIyQmPwgZGhwZJDgyVFJwj/2gAMAwEAAhEDEQA/AL81TVMU8TE8zbNry4TBMcwOCl0d3d0Umq1q5arNEccaaccIY4wiMcbobboA2QhCEIQh1iCpqaisqF1VUtTlQtRKlExJJx6yrDYbNtezU239v0zNJZqRlLTLLSQhCEIACQEgAcBmeJOZiTjbp5TGoNWX46WKbSi9zm/J2l0fVDYxpBK1BDQzJRq3Fcd3CWUSUWGAAXhQEceYWSXAZxhYBLUFurro+aa3tLefCSohIiQlIiT9OJIAiSAYzeG+No9v7Um+b0uFLbbSuoaYS6+sJSp15QQ2hPEkkmJgIIQlbiyltC1Jm5VqLxzxHvF/443DunaGoqdG1vKZLN5TEw7uD3w7MPE9d2/3666nX9P6Xqr6fqTyRMs0IRh+HCOfHCXkLZ3nL1C6dSec+T5Xm9NOtoTzyTwjx+L3pfZjLlgieNlVpDpXPK0yqlNGOqNM5zYlUmTyyOCIgyYG1ic1CU42YJGdTBFHMk2M56UBxBpZhUTQhEXiFCEE4p/t+50VsrSbnToqbc8gtuJIEwSSPabV8K0wiCCI8IjIgP707B3XvzazSNh3uqsW9rbVJrKF5tahTuPtJUBT1zQBD1K8lSkOJUlQSSFyOBKmlslWnyApDSGn7x46eFix6HKE4BCrq9XB7IGgnuphCsIz0UiozIomxWzSYxI1WWVFgITxWnYobkChnmLCC73y1WqhXYNoFfKu5v1CsnHgeDYyBS2kGBEBMY5QKiul+2naPuH3D3dTd4/+lm6YbitxktNkZUF0NtUghK65YndQ9WPrTqNKK3NFGmqYrS0im50dAONj4IFVeOuRZy4l1/jjcDntDc+V1rRM0bks3k/ZswbLlvuYdl/129P7nyHPu9L1OnzmSeE0scow+7w454Edh+cfJ1t9QOU85co3zfLTaOtKJ5J8+PvfDNGX2YYH/TDBdhgv86/HP+9+R/IX63ZuydJ/Iw859xmPpdTv8B0D9fr+v4aenL9vH64+GKi/2P1k/qfRnpHzOc53V/LGT9vT+Zj/2Q==", R = "data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMraHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjMtYzAxMSA2Ni4xNDU2NjEsIDIwMTIvMDIvMDYtMTQ6NTY6MjcgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzYgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOkNBRURFN0U1M0MwQzExRTVCNzFGQzUxRDVCRjM0MzdFIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOkNBRURFN0U2M0MwQzExRTVCNzFGQzUxRDVCRjM0MzdFIj4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6Q0FFREU3RTMzQzBDMTFFNUI3MUZDNTFENUJGMzQzN0UiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6Q0FFREU3RTQzQzBDMTFFNUI3MUZDNTFENUJGMzQzN0UiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAgICAgICAgICAgIDAwMDAwMDAwMDAQEBAQEBAQIBAQICAgECAgMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwP/wAARCAAUABQDAREAAhEBAxEB/8QArwAAAwEBAAAAAAAAAAAAAAAABgcICQoBAAIDAAAAAAAAAAAAAAAAAAYIBAUHEAAAAgcFBAQPAAAAAAAAAAAGFwADFAUWBxgBAhITCAQkFWURIiOXhSbWR7coOFh4qMgpSWmJEQAAAQgEBAchAQAAAAAAAAAEAAERAhIDExUUBQYWIwcXGCFBIjJjlSdigtIzk7MkNERkdISUpMTUJUVVZYW1NlZ2psY3R2eHCChI/9oADAMBAAIRAxEAPwDsX0+afJETEkRJmYcw5MysmCPJgytAQ7GYzHYBCwxFAiFIxCzrEb+ebzfwjdbyem0W7Q9HmttVq7VuVs6rCqVXbiu5du2BtRVFUg+pAg8eEDPxr8M6ePHjx0o8XWXeKKrrHOsuY6x0nOdBkoMbQMgxjGJoMbuN3GrY/GraSx9j7S17VFlKor0eBBgwI8UDChwoMU9DuHbtwHeu3SrLp2qZZYyrTxZLx4dZdZZY7gpM0re7Pp+7mpc+TaWt17M+DgPU7rgCzvOAx8eu1rtuKx7YKQCtlzXbTvBIaIals9ya4RsRXmkaJcRLAmTDHTC3UZmVkat6y2ntUFZaAvrIYLuSS2kwGTQY0aE3D1ms0kMtapDWiTEX7tlmrZYZmNyr36kc4jLzOWSyY0anJpPLWjEixYWAbgYMq/0meytpn+H6TXo5DaFVl/RmrugQ/GlCXf8AYD58W29rq484iCoFL0sjLP38qP8AP36jEBvqX5C7KJuP8Hflz4eIBlZXeXIKp5pZIaG3STJ6mkaJXMaqA4mLnxXxQuys+DemXLat5zUg1ZfWXuZDLZJDVo9JjRoKMG3C1GsQjTQhrVJIst3mq3yrPLBfvKtTXs4kcrlkzbPTqNMeSkUqLETgorcDAwyPvuo/r9+YxJ26X4i66IT/AIO+7nu8SE9Yyonza15l1zmmClvjPebiNDwrxPlqUfd+f8z32o+9UOhtcW4dv7exlrG43kd79ZqU52G8154O1qJZ0rRufS//2Q==", c = "data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMraHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjMtYzAxMSA2Ni4xNDU2NjEsIDIwMTIvMDIvMDYtMTQ6NTY6MjcgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzYgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOkNGOTYyNTlBM0MwRTExRTU4ODMwRDlGNzNDMDVFN0FGIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOkNGOTYyNTlCM0MwRTExRTU4ODMwRDlGNzNDMDVFN0FGIj4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6Q0Y5NjI1OTgzQzBFMTFFNTg4MzBEOUY3M0MwNUU3QUYiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6Q0Y5NjI1OTkzQzBFMTFFNTg4MzBEOUY3M0MwNUU3QUYiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAgICAgICAgICAgIDAwMDAwMDAwMDAQEBAQEBAQIBAQICAgECAgMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwP/wAARCAAQABADAREAAhEBAxEB/8QAXwABAQEAAAAAAAAAAAAAAAAAAAEKAQEAAwAAAAAAAAAAAAAAAAAAAQcJEAACAwEBAQAAAAAAAAAAAAAEBQMGBwIIAREAAgMAAwEBAAAAAAAAAAAAAgMBBAURBgcSIf/aAAwDAQACEQMRAD8A09+i/ReWUjK6dv2/U4zaJ9pLal4ZhpTXqDN1ubwfRjkr58lOHMrjV41rZi1ixYsVrUwQxryCDzGJHJJ3VW/v5lPNVubipuTckprV5LhUK/JEiGYkCIgkDMzAyEj+A4GJmdEPHPHO+dn75peS+S6S+tK60tQbe2KvrRZol9A5CHAQWFIVYCzXr169mqlqas2rUnYMAF509F5XeMsuO/YDTjMXnxYtUXuWGit+583ZZuR9JOdPUSQGAOtq3iquBsmK5ivWqjCzFXQJ3MgkkcnDA38y5mt3MNU0ypyM2a8FyqVTzJEIxEAJCEGYGIAREEgfIzEw9j8c751fved5L61or7Krsy2hibZKgdFeiPyCUPcZHZahtg61exXsWbSVJtRaqyFgDAnovzpll3yum4Dv1yLxafFi2omG7kWq+z5uyzef6OClQvnRxIdcVPFVcDWrmK5iyVGFmKuTgepBJJI+G/gZtzMVh7jZpzTkorWJHlUqniBEimYASEIATAjAiIPsORmYh457H3vrHfNL1ryXNX2VXZVqPbxAb86K9Efo3PQkBOw1DbB2bFexXrWkqTamragLAAZTzp50yyj5ZcMBwG4mbQTtBigbctyFUdj5wrzgbogNyjROgpzK4zetK6WyXrVy9k0MEMa9HHdRiRxx9sDAzKea3Dw2zcK5IxZsQPCoVHMEIlEyElISYgAmZCR/Z8DERM+x+x987R3vO9a9azl9aT1pbSxMQnQWi3RL5NL3pMQsKQqwFexYsWK1VLU1Yq1YOwZmP//Z", w = "data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMraHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjMtYzAxMSA2Ni4xNDU2NjEsIDIwMTIvMDIvMDYtMTQ6NTY6MjcgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzYgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOkRDQUFGODU2M0MwRTExRTU5NTMwRjRCMEY2RDJCMTA5IiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOkRDQUFGODU3M0MwRTExRTU5NTMwRjRCMEY2RDJCMTA5Ij4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6RENBQUY4NTQzQzBFMTFFNTk1MzBGNEIwRjZEMkIxMDkiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6RENBQUY4NTUzQzBFMTFFNTk1MzBGNEIwRjZEMkIxMDkiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAgICAgICAgICAgIDAwMDAwMDAwMDAQEBAQEBAQIBAQICAgECAgMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwP/wAARCAAQABADAREAAhEBAxEB/8QAYgABAQEAAAAAAAAAAAAAAAAAAAEKAQEAAwAAAAAAAAAAAAAAAAAAAQYIEAACAwEBAQAAAAAAAAAAAAAEBgMFBwIBCBEAAgMAAgIDAQAAAAAAAAAAAgMBBAURBxIGITIjE//aAAwDAQACEQMRAD8A2gIKCu/Rq7XbBsFdE70TxFNd5znN3ORYZwu5xYEdTJhpyZN1wvM7ezr3AtocfaCmThTmdhh9xDR++zVajRRvoHV1RhyHR5KUXMqBUz+cyv6mZjwZEcFIyXgEwMfOg/bPbNjpvYd13124svVyyFOjopEV6NjRWPjcALkc2K1StYltZCKrUg4EjZsibjiFn5BXfnNdsNgyCujSKJHihu9GzmkIJr84Ys3r5/JnM0JLh77XVluWF7sm0APqxQ5zZw+AzO5BpPPYV6jXwEFq5QwlKY8mqGZhRqif0mF/UDAeTEggZKRgDmRn4ep+2bHcmwnrrsRxamrqFKc7RcIs0a+iweKYHcmIsWalmxC6r0WWuBIOKzXEHBMMIL8u/OS7XY/sFjCj0SRFNSZzo13CRX5uxZxXkdQpgRrnPz2vLDesL3YtWcBaFBzmzh9mB8SjSe+QqN5GAgcrVKEoTHippcwo1RP5xLPqDAHgCE5GSkZMIkZ+Htnqex3JsO7E66SWpq6hC7RzkyLNGvosHyuGFMeLFmpZsQ2yh9ZTgSDhrWSBwRLI/Py79GL1hj+P2EbvRu8Y9Loui0o5R+crmcnkcxOQYTnFxwvMrezL8ZNWABWFFzhTmcGGcRjR+eTL15G+gsrKKHJdHi1oxMqBUz+kQz6mwx5ARCSkZKDOIGPmfU/U9jpvYT2J2ImczUy5J2fnuIV6NjRAeaZnTmZsVqlaxK7L32VKBwJKtWI3HP8AP//Z", m = B.type;
                if (m)
                    switch (m) {
                        case "checkbox":
                            var Y = $('<img class="custom-checkbox" />'), b = M(B.ngModel);
                            Q(Y, b(A) || !1), I.css("display", "none").before(Y), Y.on("click", function () {
                                var A = Y.hasClass("custom-checked");
                                Q(Y, !A), I.click();
                            });
                            break;
                        case "radio":
                            var g = $('<img class="custom-radio" />');
                            I.css("display", "none").before(g);
                            var l = M(B.ngModel);
                            A.$watch(B.ngModel, function (A) {
                                E(g, A);
                            });
                            for (var N = B.ngModel.split("."), z = A, G = 0; G < N.length; G++)
                                if ("$parent" == N[G] && (z = z.$parent, !z)) {
                                    z = A;
                                    break;
                                }
                            g.on("click", function () {
                                l.assign(A, I.val()), z.$digest();
                            });
                    }
            } };
    }]);
;
angular.module("datepicker", []).directive("datepicker", ["$log", function () {
        return { restrict: "A", scope: { dateOperator: "=", onChange: "&" }, link: function (e, t, i) {
                var s = { minView: 2, autoclose: 1 }, a = angular.extend(s, i, { language: "zh-CN" });
                t.datetimepicker(a), t.on("changeDate", function (t) {
                    e.$apply(function () {
                        e.onChange({ $event: t, $date: t.date });
                    });
                }), i.$observe("startDate", function (e) {
                    t.datetimepicker("setStartDate", e);
                }), i.$observe("endDate", function (e) {
                    t.datetimepicker("setEndDate", e);
                });
            } };
    }]), !function (e) {
    function t() {
        return new Date(Date.UTC.apply(Date, arguments));
    }
    "indexOf" in Array.prototype || (Array.prototype.indexOf = function (e, t) {
        void 0 === t && (t = 0), 0 > t && (t += this.length), 0 > t && (t = 0);
        for (var i = this.length; i > t; t++)
            if (t in this && this[t] === e)
                return t;
        return -1;
    });
    var i = function (t, i) {
        var s = this;
        this.element = e(t), this.container = i.container || "body", this.language = i.language || this.element.data("date-language") || "en", this.language = this.language in a ? this.language : this.language.split("-")[0], this.language = this.language in a ? this.language : "en", this.isRTL = a[this.language].rtl || !1, this.formatType = i.formatType || this.element.data("format-type") || "standard", this.format = n.parseFormat(i.format || this.element.data("date-format") || a[this.language].format || n.getDefaultFormat(this.formatType, "input"), this.formatType), this.isInline = !1, this.isVisible = !1, this.isInput = this.element.is("input"), this.fontAwesome = i.fontAwesome || this.element.data("font-awesome") || !1, this.bootcssVer = i.bootcssVer || (this.isInput ? this.element.is(".form-control") ? 3 : 2 : this.bootcssVer = this.element.is(".input-group") ? 3 : 2), this.component = this.element.is(".date") ? 3 == this.bootcssVer ? this.element.find(".input-group-addon .glyphicon-th, .input-group-addon .glyphicon-time, .input-group-addon .glyphicon-calendar, .input-group-addon .glyphicon-calendar, .input-group-addon .fa-calendar, .input-group-addon .fa-clock-o").parent() : this.element.find(".add-on .icon-th, .add-on .icon-time, .add-on .icon-calendar .fa-calendar .fa-clock-o").parent() : !1, this.componentReset = this.element.is(".date") ? 3 == this.bootcssVer ? this.element.find(".input-group-addon .glyphicon-remove, .input-group-addon .fa-times").parent() : this.element.find(".add-on .icon-remove, .add-on .fa-times").parent() : !1, this.hasInput = this.component && this.element.find("input").length, this.component && 0 === this.component.length && (this.component = !1), this.linkField = i.linkField || this.element.data("link-field") || !1, this.linkFormat = n.parseFormat(i.linkFormat || this.element.data("link-format") || n.getDefaultFormat(this.formatType, "link"), this.formatType), this.minuteStep = i.minuteStep || this.element.data("minute-step") || 5, this.pickerPosition = i.pickerPosition || this.element.data("picker-position") || "bottom-right", this.showMeridian = i.showMeridian || this.element.data("show-meridian") || !1, this.initialDate = i.initialDate || new Date, this.zIndex = i.zIndex || this.element.data("z-index") || void 0, this.icons = { leftArrow: this.fontAwesome ? "fa-arrow-left" : 3 === this.bootcssVer ? "glyphicon-arrow-left" : "icon-arrow-left", rightArrow: this.fontAwesome ? "fa-arrow-right" : 3 === this.bootcssVer ? "glyphicon-arrow-right" : "icon-arrow-right" }, this.icontype = this.fontAwesome ? "fa" : "glyphicon", this._attachEvents(), this.formatViewType = "datetime", "formatViewType" in i ? this.formatViewType = i.formatViewType : "formatViewType" in this.element.data() && (this.formatViewType = this.element.data("formatViewType")), this.minView = 0, "minView" in i ? this.minView = i.minView : "minView" in this.element.data() && (this.minView = this.element.data("min-view")), this.minView = n.convertViewMode(this.minView), this.maxView = n.modes.length - 1, "maxView" in i ? this.maxView = i.maxView : "maxView" in this.element.data() && (this.maxView = this.element.data("max-view")), this.maxView = n.convertViewMode(this.maxView), this.wheelViewModeNavigation = !1, "wheelViewModeNavigation" in i ? this.wheelViewModeNavigation = i.wheelViewModeNavigation : "wheelViewModeNavigation" in this.element.data() && (this.wheelViewModeNavigation = this.element.data("view-mode-wheel-navigation")), this.wheelViewModeNavigationInverseDirection = !1, "wheelViewModeNavigationInverseDirection" in i ? this.wheelViewModeNavigationInverseDirection = i.wheelViewModeNavigationInverseDirection : "wheelViewModeNavigationInverseDirection" in this.element.data() && (this.wheelViewModeNavigationInverseDirection = this.element.data("view-mode-wheel-navigation-inverse-dir")), this.wheelViewModeNavigationDelay = 100, "wheelViewModeNavigationDelay" in i ? this.wheelViewModeNavigationDelay = i.wheelViewModeNavigationDelay : "wheelViewModeNavigationDelay" in this.element.data() && (this.wheelViewModeNavigationDelay = this.element.data("view-mode-wheel-navigation-delay")), this.startViewMode = 2, "startView" in i ? this.startViewMode = i.startView : "startView" in this.element.data() && (this.startViewMode = this.element.data("start-view")), this.startViewMode = n.convertViewMode(this.startViewMode), this.viewMode = this.startViewMode, this.viewSelect = this.minView, "viewSelect" in i ? this.viewSelect = i.viewSelect : "viewSelect" in this.element.data() && (this.viewSelect = this.element.data("view-select")), this.viewSelect = n.convertViewMode(this.viewSelect), this.forceParse = !0, "forceParse" in i ? this.forceParse = i.forceParse : "dateForceParse" in this.element.data() && (this.forceParse = this.element.data("date-force-parse"));
        for (var h = 3 === this.bootcssVer ? n.templateV3 : n.template; -1 !== h.indexOf("{iconType}");)
            h = h.replace("{iconType}", this.icontype);
        for (; -1 !== h.indexOf("{leftArrow}");)
            h = h.replace("{leftArrow}", this.icons.leftArrow);
        for (; -1 !== h.indexOf("{rightArrow}");)
            h = h.replace("{rightArrow}", this.icons.rightArrow);
        if (this.picker = e(h).appendTo(this.isInline ? this.element : this.container).on({ click: e.proxy(this.click, this), mousedown: e.proxy(this.mousedown, this) }), this.wheelViewModeNavigation && e.fn.mousewheel && this.picker.on({ mousewheel: e.proxy(this.mousewheel, this) }), this.picker.addClass(this.isInline ? "datetimepicker-inline" : "datetimepicker-dropdown-" + this.pickerPosition + " dropdown-menu"), this.isRTL) {
            this.picker.addClass("datetimepicker-rtl");
            var o = 3 === this.bootcssVer ? ".prev span, .next span" : ".prev i, .next i";
            this.picker.find(o).toggleClass(this.icons.leftArrow + " " + this.icons.rightArrow);
        }
        e(document).on("mousedown", function (t) {
            0 === e(t.target).closest(".datetimepicker").length && s.hide();
        }), this.autoclose = !1, "autoclose" in i ? this.autoclose = i.autoclose : "dateAutoclose" in this.element.data() && (this.autoclose = this.element.data("date-autoclose")), this.keyboardNavigation = !0, "keyboardNavigation" in i ? this.keyboardNavigation = i.keyboardNavigation : "dateKeyboardNavigation" in this.element.data() && (this.keyboardNavigation = this.element.data("date-keyboard-navigation")), this.todayBtn = i.todayBtn || this.element.data("date-today-btn") || !1, this.todayHighlight = i.todayHighlight || this.element.data("date-today-highlight") || !1, this.weekStart = (i.weekStart || this.element.data("date-weekstart") || a[this.language].weekStart || 0) % 7, this.weekEnd = (this.weekStart + 6) % 7, this.startDate = -1 / 0, this.endDate = 1 / 0, this.daysOfWeekDisabled = [], this.setStartDate(i.startDate || this.element.data("date-startdate")), this.setEndDate(i.endDate || this.element.data("date-enddate")), this.setDaysOfWeekDisabled(i.daysOfWeekDisabled || this.element.data("date-days-of-week-disabled")), this.setMinutesDisabled(i.minutesDisabled || this.element.data("date-minute-disabled")), this.setHoursDisabled(i.hoursDisabled || this.element.data("date-hour-disabled")), this.fillDow(), this.fillMonths(), this.update(), this.showMode(), this.isInline && this.show();
    };
    i.prototype = { constructor: i, _events: [], _attachEvents: function () {
            this._detachEvents(), this.isInput ? this._events = [[this.element, { focus: e.proxy(this.show, this), keyup: e.proxy(this.update, this), keydown: e.proxy(this.keydown, this) }]] : this.component && this.hasInput ? (this._events = [[this.element.find("input"), { focus: e.proxy(this.show, this), keyup: e.proxy(this.update, this), keydown: e.proxy(this.keydown, this) }], [this.component, { click: e.proxy(this.show, this) }]], this.componentReset && this._events.push([this.componentReset, { click: e.proxy(this.reset, this) }])) : this.element.is("div") ? this.isInline = !0 : this._events = [[this.element, { click: e.proxy(this.show, this) }]];
            for (var t, i, s = 0; s < this._events.length; s++)
                t = this._events[s][0], i = this._events[s][1], t.on(i);
        }, _detachEvents: function () {
            for (var e, t, i = 0; i < this._events.length; i++)
                e = this._events[i][0], t = this._events[i][1], e.off(t);
            this._events = [];
        }, show: function (t) {
            this.picker.show(), this.height = this.component ? this.component.outerHeight() : this.element.outerHeight(), this.forceParse && this.update(), this.place(), e(window).on("resize", e.proxy(this.place, this)), t && (t.stopPropagation(), t.preventDefault()), this.isVisible = !0, this.element.trigger({ type: "show", date: this.date });
        }, hide: function () {
            this.isVisible && (this.isInline || (this.picker.hide(), e(window).off("resize", this.place), this.viewMode = this.startViewMode, this.showMode(), this.isInput || e(document).off("mousedown", this.hide), this.forceParse && (this.isInput && this.element.val() || this.hasInput && this.element.find("input").val()) && this.setValue(), this.isVisible = !1, this.element.trigger({ type: "hide", date: this.date })));
        }, remove: function () {
            this._detachEvents(), this.picker.remove(), delete this.picker, delete this.element.data().datetimepicker;
        }, getDate: function () {
            var e = this.getUTCDate();
            return new Date(e.getTime() + 6e4 * e.getTimezoneOffset());
        }, getUTCDate: function () {
            return this.date;
        }, setDate: function (e) {
            this.setUTCDate(new Date(e.getTime() - 6e4 * e.getTimezoneOffset()));
        }, setUTCDate: function (e) {
            e >= this.startDate && e <= this.endDate ? (this.date = e, this.setValue(), this.viewDate = this.date, this.fill()) : this.element.trigger({ type: "outOfRange", date: e, startDate: this.startDate, endDate: this.endDate });
        }, setFormat: function (e) {
            this.format = n.parseFormat(e, this.formatType);
            var t;
            this.isInput ? t = this.element : this.component && (t = this.element.find("input")), t && t.val() && this.setValue();
        }, setValue: function () {
            var t = this.getFormattedDate();
            this.isInput ? this.element.val(t) : (this.component && this.element.find("input").val(t), this.element.data("date", t)), this.linkField && e("#" + this.linkField).val(this.getFormattedDate(this.linkFormat));
        }, getFormattedDate: function (e) {
            return void 0 == e && (e = this.format), n.formatDate(this.date, e, this.language, this.formatType);
        }, setStartDate: function (e) {
            this.startDate = e || -1 / 0, this.startDate !== -1 / 0 && (this.startDate = n.parseDate(this.startDate, this.format, this.language, this.formatType)), this.update(), this.updateNavArrows();
        }, setEndDate: function (e) {
            this.endDate = e || 1 / 0, 1 / 0 !== this.endDate && (this.endDate = n.parseDate(this.endDate, this.format, this.language, this.formatType)), this.update(), this.updateNavArrows();
        }, setDaysOfWeekDisabled: function (t) {
            this.daysOfWeekDisabled = t || [], e.isArray(this.daysOfWeekDisabled) || (this.daysOfWeekDisabled = this.daysOfWeekDisabled.split(/,\s*/)), this.daysOfWeekDisabled = e.map(this.daysOfWeekDisabled, function (e) {
                return parseInt(e, 10);
            }), this.update(), this.updateNavArrows();
        }, setMinutesDisabled: function (t) {
            this.minutesDisabled = t || [], e.isArray(this.minutesDisabled) || (this.minutesDisabled = this.minutesDisabled.split(/,\s*/)), this.minutesDisabled = e.map(this.minutesDisabled, function (e) {
                return parseInt(e, 10);
            }), this.update(), this.updateNavArrows();
        }, setHoursDisabled: function (t) {
            this.hoursDisabled = t || [], e.isArray(this.hoursDisabled) || (this.hoursDisabled = this.hoursDisabled.split(/,\s*/)), this.hoursDisabled = e.map(this.hoursDisabled, function (e) {
                return parseInt(e, 10);
            }), this.update(), this.updateNavArrows();
        }, place: function () {
            if (!this.isInline) {
                if (!this.zIndex) {
                    var t = 0;
                    e("div").each(function () {
                        var i = parseInt(e(this).css("zIndex"), 10);
                        i > t && (t = i);
                    }), this.zIndex = t + 10;
                }
                var i, s, a, n;
                n = this.container instanceof e ? this.container.offset() : e(this.container).offset(), this.component ? (i = this.component.offset(), a = i.left, ("bottom-left" == this.pickerPosition || "top-left" == this.pickerPosition) && (a += this.component.outerWidth() - this.picker.outerWidth())) : (i = this.element.offset(), a = i.left), a + 220 > document.body.clientWidth && (a = document.body.clientWidth - 220), s = "top-left" == this.pickerPosition || "top-right" == this.pickerPosition ? i.top - this.picker.outerHeight() : i.top + this.height, s -= n.top, a -= n.left, this.picker.css({ top: s, left: a, zIndex: this.zIndex });
            }
        }, update: function () {
            var e, t = !1;
            arguments && arguments.length && ("string" == typeof arguments[0] || arguments[0] instanceof Date) ? (e = arguments[0], t = !0) : (e = (this.isInput ? this.element.val() : this.element.find("input").val()) || this.element.data("date") || this.initialDate, ("string" == typeof e || e instanceof String) && (e = e.replace(/^\s+|\s+$/g, ""))), e || (e = new Date, t = !1), this.date = n.parseDate(e, this.format, this.language, this.formatType), t && this.setValue(), this.viewDate = new Date(this.date < this.startDate ? this.startDate : this.date > this.endDate ? this.endDate : this.date), this.fill();
        }, fillDow: function () {
            for (var e = this.weekStart, t = "<tr>"; e < this.weekStart + 7;)
                t += '<th class="dow">' + a[this.language].daysMin[e++ % 7] + "</th>";
            t += "</tr>", this.picker.find(".datetimepicker-days thead").append(t);
        }, fillMonths: function () {
            for (var e = "", t = 0; 12 > t;)
                e += '<span class="month">' + a[this.language].monthsShort[t++] + "</span>";
            this.picker.find(".datetimepicker-months td").html(e);
        }, fill: function () {
            if (null != this.date && null != this.viewDate) {
                var i = new Date(this.viewDate), s = i.getUTCFullYear(), h = i.getUTCMonth(), o = i.getUTCDate(), r = i.getUTCHours(), d = i.getUTCMinutes(), l = this.startDate !== -1 / 0 ? this.startDate.getUTCFullYear() : -1 / 0, c = this.startDate !== -1 / 0 ? this.startDate.getUTCMonth() + 1 : -1 / 0, u = 1 / 0 !== this.endDate ? this.endDate.getUTCFullYear() : 1 / 0, m = 1 / 0 !== this.endDate ? this.endDate.getUTCMonth() + 1 : 1 / 0, p = new t(this.date.getUTCFullYear(), this.date.getUTCMonth(), this.date.getUTCDate()).valueOf(), v = new Date;
                if (this.picker.find(".datetimepicker-days thead th:eq(1)").text(a[this.language].months[h] + " " + s), "time" == this.formatViewType) {
                    var g = this.getFormattedDate();
                    this.picker.find(".datetimepicker-hours thead th:eq(1)").text(g), this.picker.find(".datetimepicker-minutes thead th:eq(1)").text(g);
                } else
                    this.picker.find(".datetimepicker-hours thead th:eq(1)").text(o + " " + a[this.language].months[h] + " " + s), this.picker.find(".datetimepicker-minutes thead th:eq(1)").text(o + " " + a[this.language].months[h] + " " + s);
                this.picker.find("tfoot th.today").text(a[this.language].today).toggle(this.todayBtn !== !1), this.updateNavArrows(), this.fillMonths();
                var f = t(s, h - 1, 28, 0, 0, 0, 0), w = n.getDaysInMonth(f.getUTCFullYear(), f.getUTCMonth());
                f.setUTCDate(w), f.setUTCDate(w - (f.getUTCDay() - this.weekStart + 7) % 7);
                var D = new Date(f);
                D.setUTCDate(D.getUTCDate() + 42), D = D.valueOf();
                for (var y, T = []; f.valueOf() < D;)
                    f.getUTCDay() == this.weekStart && T.push("<tr>"), y = "", f.getUTCFullYear() < s || f.getUTCFullYear() == s && f.getUTCMonth() < h ? y += " old" : (f.getUTCFullYear() > s || f.getUTCFullYear() == s && f.getUTCMonth() > h) && (y += " new"), this.todayHighlight && f.getUTCFullYear() == v.getFullYear() && f.getUTCMonth() == v.getMonth() && f.getUTCDate() == v.getDate() && (y += " today"), f.valueOf() == p && (y += " active"), (f.valueOf() + 864e5 <= this.startDate || f.valueOf() > this.endDate || -1 !== e.inArray(f.getUTCDay(), this.daysOfWeekDisabled)) && (y += " disabled"), T.push('<td class="day' + y + '">' + f.getUTCDate() + "</td>"), f.getUTCDay() == this.weekEnd && T.push("</tr>"), f.setUTCDate(f.getUTCDate() + 1);
                this.picker.find(".datetimepicker-days tbody").empty().append(T.join("")), T = [];
                for (var M = "", C = "", k = "", b = this.hoursDisabled || [], U = 0; 24 > U; U++)
                    if (-1 === b.indexOf(U)) {
                        var V = t(s, h, o, U);
                        y = "", V.valueOf() + 36e5 <= this.startDate || V.valueOf() > this.endDate ? y += " disabled" : r == U && (y += " active"), this.showMeridian && 2 == a[this.language].meridiem.length ? (C = 12 > U ? a[this.language].meridiem[0] : a[this.language].meridiem[1], C != k && ("" != k && T.push("</fieldset>"), T.push('<fieldset class="hour"><legend>' + C.toUpperCase() + "</legend>")), k = C, M = U % 12 ? U % 12 : 12, T.push('<span class="hour' + y + " hour_" + (12 > U ? "am" : "pm") + '">' + M + "</span>"), 23 == U && T.push("</fieldset>")) : (M = U + ":00", T.push('<span class="hour' + y + '">' + M + "</span>"));
                    }
                this.picker.find(".datetimepicker-hours td").html(T.join("")), T = [], M = "", C = "", k = "";
                for (var x = this.minutesDisabled || [], U = 0; 60 > U; U += this.minuteStep)
                    if (-1 === x.indexOf(U)) {
                        var V = t(s, h, o, r, U, 0);
                        y = "", V.valueOf() < this.startDate || V.valueOf() > this.endDate ? y += " disabled" : Math.floor(d / this.minuteStep) == Math.floor(U / this.minuteStep) && (y += " active"), this.showMeridian && 2 == a[this.language].meridiem.length ? (C = 12 > r ? a[this.language].meridiem[0] : a[this.language].meridiem[1], C != k && ("" != k && T.push("</fieldset>"), T.push('<fieldset class="minute"><legend>' + C.toUpperCase() + "</legend>")), k = C, M = r % 12 ? r % 12 : 12, T.push('<span class="minute' + y + '">' + M + ":" + (10 > U ? "0" + U : U) + "</span>"), 59 == U && T.push("</fieldset>")) : (M = U + ":00", T.push('<span class="minute' + y + '">' + r + ":" + (10 > U ? "0" + U : U) + "</span>"));
                    }
                this.picker.find(".datetimepicker-minutes td").html(T.join(""));
                var S = this.date.getUTCFullYear(), F = this.picker.find(".datetimepicker-months").find("th:eq(1)").text(s).end().find("span").removeClass("active");
                if (S == s) {
                    var H = F.length - 12;
                    F.eq(this.date.getUTCMonth() + H).addClass("active");
                }
                (l > s || s > u) && F.addClass("disabled"), s == l && F.slice(0, c - 1).addClass("disabled"), s == u && F.slice(m).addClass("disabled"), T = "", s = 10 * parseInt(s / 10, 10);
                var N = this.picker.find(".datetimepicker-years").find("th:eq(1)").text(s + "-" + (s + 9)).end().find("td");
                s -= 1;
                for (var U = -1; 11 > U; U++)
                    T += '<span class="year' + (-1 == U || 10 == U ? " old" : "") + (S == s ? " active" : "") + (l > s || s > u ? " disabled" : "") + '">' + s + "</span>", s += 1;
                N.html(T), this.place();
            }
        }, updateNavArrows: function () {
            var e = new Date(this.viewDate), t = e.getUTCFullYear(), i = e.getUTCMonth(), s = e.getUTCDate(), a = e.getUTCHours();
            switch (this.viewMode) {
                case 0:
                    this.picker.find(".prev").css(this.startDate !== -1 / 0 && t <= this.startDate.getUTCFullYear() && i <= this.startDate.getUTCMonth() && s <= this.startDate.getUTCDate() && a <= this.startDate.getUTCHours() ? { visibility: "hidden" } : { visibility: "visible" }), this.picker.find(".next").css(1 / 0 !== this.endDate && t >= this.endDate.getUTCFullYear() && i >= this.endDate.getUTCMonth() && s >= this.endDate.getUTCDate() && a >= this.endDate.getUTCHours() ? { visibility: "hidden" } : { visibility: "visible" });
                    break;
                case 1:
                    this.picker.find(".prev").css(this.startDate !== -1 / 0 && t <= this.startDate.getUTCFullYear() && i <= this.startDate.getUTCMonth() && s <= this.startDate.getUTCDate() ? { visibility: "hidden" } : { visibility: "visible" }), this.picker.find(".next").css(1 / 0 !== this.endDate && t >= this.endDate.getUTCFullYear() && i >= this.endDate.getUTCMonth() && s >= this.endDate.getUTCDate() ? { visibility: "hidden" } : { visibility: "visible" });
                    break;
                case 2:
                    this.picker.find(".prev").css(this.startDate !== -1 / 0 && t <= this.startDate.getUTCFullYear() && i <= this.startDate.getUTCMonth() ? { visibility: "hidden" } : { visibility: "visible" }), this.picker.find(".next").css(1 / 0 !== this.endDate && t >= this.endDate.getUTCFullYear() && i >= this.endDate.getUTCMonth() ? { visibility: "hidden" } : { visibility: "visible" });
                    break;
                case 3:
                case 4:
                    this.picker.find(".prev").css(this.startDate !== -1 / 0 && t <= this.startDate.getUTCFullYear() ? { visibility: "hidden" } : { visibility: "visible" }), this.picker.find(".next").css(1 / 0 !== this.endDate && t >= this.endDate.getUTCFullYear() ? { visibility: "hidden" } : { visibility: "visible" });
            }
        }, mousewheel: function (t) {
            if (t.preventDefault(), t.stopPropagation(), !this.wheelPause) {
                this.wheelPause = !0;
                var i = t.originalEvent, s = i.wheelDelta, a = s > 0 ? 1 : 0 === s ? 0 : -1;
                this.wheelViewModeNavigationInverseDirection && (a = -a), this.showMode(a), setTimeout(e.proxy(function () {
                    this.wheelPause = !1;
                }, this), this.wheelViewModeNavigationDelay);
            }
        }, click: function (i) {
            i.stopPropagation(), i.preventDefault();
            var s = e(i.target).closest("span, td, th, legend");
            if (s.is("." + this.icontype) && (s = e(s).parent().closest("span, td, th, legend")), 1 == s.length) {
                if (s.is(".disabled"))
                    return void this.element.trigger({ type: "outOfRange", date: this.viewDate, startDate: this.startDate, endDate: this.endDate });
                switch (s[0].nodeName.toLowerCase()) {
                    case "th":
                        switch (s[0].className) {
                            case "switch":
                                this.showMode(1);
                                break;
                            case "prev":
                            case "next":
                                var a = n.modes[this.viewMode].navStep * ("prev" == s[0].className ? -1 : 1);
                                switch (this.viewMode) {
                                    case 0:
                                        this.viewDate = this.moveHour(this.viewDate, a);
                                        break;
                                    case 1:
                                        this.viewDate = this.moveDate(this.viewDate, a);
                                        break;
                                    case 2:
                                        this.viewDate = this.moveMonth(this.viewDate, a);
                                        break;
                                    case 3:
                                    case 4:
                                        this.viewDate = this.moveYear(this.viewDate, a);
                                }
                                this.fill(), this.element.trigger({ type: s[0].className + ":" + this.convertViewModeText(this.viewMode), date: this.viewDate, startDate: this.startDate, endDate: this.endDate });
                                break;
                            case "today":
                                var h = new Date;
                                h = t(h.getFullYear(), h.getMonth(), h.getDate(), h.getHours(), h.getMinutes(), h.getSeconds(), 0), h < this.startDate ? h = this.startDate : h > this.endDate && (h = this.endDate), this.viewMode = this.startViewMode, this.showMode(0), this._setDate(h), this.fill(), this.autoclose && this.hide();
                        }
                        break;
                    case "span":
                        if (!s.is(".disabled")) {
                            var o = this.viewDate.getUTCFullYear(), r = this.viewDate.getUTCMonth(), d = this.viewDate.getUTCDate(), l = this.viewDate.getUTCHours(), c = this.viewDate.getUTCMinutes(), u = this.viewDate.getUTCSeconds();
                            if (s.is(".month") ? (this.viewDate.setUTCDate(1), r = s.parent().find("span").index(s), d = this.viewDate.getUTCDate(), this.viewDate.setUTCMonth(r), this.element.trigger({ type: "changeMonth", date: this.viewDate }), this.viewSelect >= 3 && this._setDate(t(o, r, d, l, c, u, 0))) : s.is(".year") ? (this.viewDate.setUTCDate(1), o = parseInt(s.text(), 10) || 0, this.viewDate.setUTCFullYear(o), this.element.trigger({ type: "changeYear", date: this.viewDate }), this.viewSelect >= 4 && this._setDate(t(o, r, d, l, c, u, 0))) : s.is(".hour") ? (l = parseInt(s.text(), 10) || 0, (s.hasClass("hour_am") || s.hasClass("hour_pm")) && (12 == l && s.hasClass("hour_am") ? l = 0 : 12 != l && s.hasClass("hour_pm") && (l += 12)), this.viewDate.setUTCHours(l), this.element.trigger({ type: "changeHour", date: this.viewDate }), this.viewSelect >= 1 && this._setDate(t(o, r, d, l, c, u, 0))) : s.is(".minute") && (c = parseInt(s.text().substr(s.text().indexOf(":") + 1), 10) || 0, this.viewDate.setUTCMinutes(c), this.element.trigger({ type: "changeMinute", date: this.viewDate }), this.viewSelect >= 0 && this._setDate(t(o, r, d, l, c, u, 0))), 0 != this.viewMode) {
                                var m = this.viewMode;
                                this.showMode(-1), this.fill(), m == this.viewMode && this.autoclose && this.hide();
                            } else
                                this.fill(), this.autoclose && this.hide();
                        }
                        break;
                    case "td":
                        if (s.is(".day") && !s.is(".disabled")) {
                            var d = parseInt(s.text(), 10) || 1, o = this.viewDate.getUTCFullYear(), r = this.viewDate.getUTCMonth(), l = this.viewDate.getUTCHours(), c = this.viewDate.getUTCMinutes(), u = this.viewDate.getUTCSeconds();
                            s.is(".old") ? 0 === r ? (r = 11, o -= 1) : r -= 1 : s.is(".new") && (11 == r ? (r = 0, o += 1) : r += 1), this.viewDate.setUTCFullYear(o), this.viewDate.setUTCMonth(r, d), this.element.trigger({ type: "changeDay", date: this.viewDate }), this.viewSelect >= 2 && this._setDate(t(o, r, d, l, c, u, 0));
                        }
                        var m = this.viewMode;
                        this.showMode(-1), this.fill(), m == this.viewMode && this.autoclose && this.hide();
                }
            }
        }, _setDate: function (e, t) {
            t && "date" != t || (this.date = e), t && "view" != t || (this.viewDate = e), this.fill(), this.setValue();
            var i;
            this.isInput ? i = this.element : this.component && (i = this.element.find("input")), i && (i.change(), this.autoclose && (!t || "date" == t)), this.element.trigger({ type: "changeDate", date: this.date }), null == e && (this.date = this.viewDate);
        }, moveMinute: function (e, t) {
            if (!t)
                return e;
            var i = new Date(e.valueOf());
            return i.setUTCMinutes(i.getUTCMinutes() + t * this.minuteStep), i;
        }, moveHour: function (e, t) {
            if (!t)
                return e;
            var i = new Date(e.valueOf());
            return i.setUTCHours(i.getUTCHours() + t), i;
        }, moveDate: function (e, t) {
            if (!t)
                return e;
            var i = new Date(e.valueOf());
            return i.setUTCDate(i.getUTCDate() + t), i;
        }, moveMonth: function (e, t) {
            if (!t)
                return e;
            var i, s, a = new Date(e.valueOf()), n = a.getUTCDate(), h = a.getUTCMonth(), o = Math.abs(t);
            if (t = t > 0 ? 1 : -1, 1 == o)
                s = -1 == t ? function () {
                    return a.getUTCMonth() == h;
                } : function () {
                    return a.getUTCMonth() != i;
                }, i = h + t, a.setUTCMonth(i), (0 > i || i > 11) && (i = (i + 12) % 12);
            else {
                for (var r = 0; o > r; r++)
                    a = this.moveMonth(a, t);
                i = a.getUTCMonth(), a.setUTCDate(n), s = function () {
                    return i != a.getUTCMonth();
                };
            }
            for (; s();)
                a.setUTCDate(--n), a.setUTCMonth(i);
            return a;
        }, moveYear: function (e, t) {
            return this.moveMonth(e, 12 * t);
        }, dateWithinRange: function (e) {
            return e >= this.startDate && e <= this.endDate;
        }, keydown: function (e) {
            if (this.picker.is(":not(:visible)"))
                return void (27 == e.keyCode && this.show());
            var t, i, s, a = !1;
            switch (e.keyCode) {
                case 27:
                    this.hide(), e.preventDefault();
                    break;
                case 37:
                case 39:
                    if (!this.keyboardNavigation)
                        break;
                    t = 37 == e.keyCode ? -1 : 1, viewMode = this.viewMode, e.ctrlKey ? viewMode += 2 : e.shiftKey && (viewMode += 1), 4 == viewMode ? (i = this.moveYear(this.date, t), s = this.moveYear(this.viewDate, t)) : 3 == viewMode ? (i = this.moveMonth(this.date, t), s = this.moveMonth(this.viewDate, t)) : 2 == viewMode ? (i = this.moveDate(this.date, t), s = this.moveDate(this.viewDate, t)) : 1 == viewMode ? (i = this.moveHour(this.date, t), s = this.moveHour(this.viewDate, t)) : 0 == viewMode && (i = this.moveMinute(this.date, t), s = this.moveMinute(this.viewDate, t)), this.dateWithinRange(i) && (this.date = i, this.viewDate = s, this.setValue(), this.update(), e.preventDefault(), a = !0);
                    break;
                case 38:
                case 40:
                    if (!this.keyboardNavigation)
                        break;
                    t = 38 == e.keyCode ? -1 : 1, viewMode = this.viewMode, e.ctrlKey ? viewMode += 2 : e.shiftKey && (viewMode += 1), 4 == viewMode ? (i = this.moveYear(this.date, t), s = this.moveYear(this.viewDate, t)) : 3 == viewMode ? (i = this.moveMonth(this.date, t), s = this.moveMonth(this.viewDate, t)) : 2 == viewMode ? (i = this.moveDate(this.date, 7 * t), s = this.moveDate(this.viewDate, 7 * t)) : 1 == viewMode ? this.showMeridian ? (i = this.moveHour(this.date, 6 * t), s = this.moveHour(this.viewDate, 6 * t)) : (i = this.moveHour(this.date, 4 * t), s = this.moveHour(this.viewDate, 4 * t)) : 0 == viewMode && (i = this.moveMinute(this.date, 4 * t), s = this.moveMinute(this.viewDate, 4 * t)), this.dateWithinRange(i) && (this.date = i, this.viewDate = s, this.setValue(), this.update(), e.preventDefault(), a = !0);
                    break;
                case 13:
                    if (0 != this.viewMode) {
                        var n = this.viewMode;
                        this.showMode(-1), this.fill(), n == this.viewMode && this.autoclose && this.hide();
                    } else
                        this.fill(), this.autoclose && this.hide();
                    e.preventDefault();
                    break;
                case 9:
                    this.hide();
            }
            if (a) {
                var h;
                this.isInput ? h = this.element : this.component && (h = this.element.find("input")), h && h.change(), this.element.trigger({ type: "changeDate", date: this.date });
            }
        }, showMode: function (e) {
            if (e) {
                var t = Math.max(0, Math.min(n.modes.length - 1, this.viewMode + e));
                t >= this.minView && t <= this.maxView && (this.element.trigger({ type: "changeMode", date: this.viewDate, oldViewMode: this.viewMode, newViewMode: t }), this.viewMode = t);
            }
            this.picker.find(">div").hide().filter(".datetimepicker-" + n.modes[this.viewMode].clsName).css("display", "block"), this.updateNavArrows();
        }, reset: function () {
            this._setDate(null, "date");
        }, convertViewModeText: function (e) {
            switch (e) {
                case 4:
                    return "decade";
                case 3:
                    return "year";
                case 2:
                    return "month";
                case 1:
                    return "day";
                case 0:
                    return "hour";
            }
        } };
    var s = e.fn.datetimepicker;
    e.fn.datetimepicker = function (t) {
        var s = Array.apply(null, arguments);
        s.shift();
        var a;
        return this.each(function () {
            var n = e(this), h = n.data("datetimepicker"), o = "object" == typeof t && t;
            return h || n.data("datetimepicker", h = new i(this, e.extend({}, e.fn.datetimepicker.defaults, o))), "string" == typeof t && "function" == typeof h[t] && (a = h[t].apply(h, s), void 0 !== a) ? !1 : void 0;
        }), void 0 !== a ? a : this;
    }, e.fn.datetimepicker.defaults = {}, e.fn.datetimepicker.Constructor = i;
    var a = e.fn.datetimepicker.dates = { en: { days: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"], daysShort: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"], daysMin: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su"], months: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"], monthsShort: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"], meridiem: ["am", "pm"], suffix: ["st", "nd", "rd", "th"], today: "Today" } }, n = {
        modes: [{ clsName: "minutes", navFnc: "Hours", navStep: 1 }, { clsName: "hours", navFnc: "Date", navStep: 1 }, { clsName: "days", navFnc: "Month", navStep: 1 }, { clsName: "months", navFnc: "FullYear", navStep: 1 }, { clsName: "years", navFnc: "FullYear", navStep: 10 }], isLeapYear: function (e) {
            return e % 4 === 0 && e % 100 !== 0 || e % 400 === 0;
        }, getDaysInMonth: function (e, t) {
            return [31, n.isLeapYear(e) ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][t];
        }, getDefaultFormat: function (e, t) {
            if ("standard" == e)
                return "input" == t ? "yyyy-mm-dd hh:ii" : "yyyy-mm-dd hh:ii:ss";
            if ("php" == e)
                return "input" == t ? "Y-m-d H:i" : "Y-m-d H:i:s";
            throw new Error("Invalid format type.");
        }, validParts: function (e) {
            if ("standard" == e)
                return /hh?|HH?|p|P|ii?|ss?|dd?|DD?|mm?|MM?|yy(?:yy)?/g;
            if ("php" == e)
                return /[dDjlNwzFmMnStyYaABgGhHis]/g;
            throw new Error("Invalid format type.");
        }, nonpunctuation: /[^ -\/:-@\[-`{-~\t\n\rTZ]+/g, parseFormat: function (e, t) {
            var i = e.replace(this.validParts(t), "\x00").split("\x00"), s = e.match(this.validParts(t));
            if (!i || !i.length || !s || 0 == s.length)
                throw new Error("Invalid date format.");
            return { separators: i, parts: s };
        }, parseDate: function (s, n, h, o) {
            if (s instanceof Date) {
                var r = new Date(s.valueOf() - 6e4 * s.getTimezoneOffset());
                return r.setMilliseconds(0), r;
            }
            if (/^\d{4}\-\d{1,2}\-\d{1,2}$/.test(s) && (n = this.parseFormat("yyyy-mm-dd", o)), /^\d{4}\-\d{1,2}\-\d{1,2}[T ]\d{1,2}\:\d{1,2}$/.test(s) && (n = this.parseFormat("yyyy-mm-dd hh:ii", o)), /^\d{4}\-\d{1,2}\-\d{1,2}[T ]\d{1,2}\:\d{1,2}\:\d{1,2}[Z]{0,1}$/.test(s) && (n = this.parseFormat("yyyy-mm-dd hh:ii:ss", o)), /^[-+]\d+[dmwy]([\s,]+[-+]\d+[dmwy])*$/.test(s)) {
                var d, l, c = /([-+]\d+)([dmwy])/, u = s.match(/([-+]\d+)([dmwy])/g);
                s = new Date;
                for (var m = 0; m < u.length; m++)
                    switch (d = c.exec(u[m]), l = parseInt(d[1]), d[2]) {
                        case "d":
                            s.setUTCDate(s.getUTCDate() + l);
                            break;
                        case "m":
                            s = i.prototype.moveMonth.call(i.prototype, s, l);
                            break;
                        case "w":
                            s.setUTCDate(s.getUTCDate() + 7 * l);
                            break;
                        case "y":
                            s = i.prototype.moveYear.call(i.prototype, s, l);
                    }
                return t(s.getUTCFullYear(), s.getUTCMonth(), s.getUTCDate(), s.getUTCHours(), s.getUTCMinutes(), s.getUTCSeconds(), 0);
            }
            var p, v, d, u = s && s.toString().match(this.nonpunctuation) || [], s = new Date(0, 0, 0, 0, 0, 0, 0), g = {}, f = ["hh", "h", "ii", "i", "ss", "s", "yyyy", "yy", "M", "MM", "m", "mm", "D", "DD", "d", "dd", "H", "HH", "p", "P"], w = { hh: function (e, t) {
                    return e.setUTCHours(t);
                }, h: function (e, t) {
                    return e.setUTCHours(t);
                }, HH: function (e, t) {
                    return e.setUTCHours(12 == t ? 0 : t);
                }, H: function (e, t) {
                    return e.setUTCHours(12 == t ? 0 : t);
                }, ii: function (e, t) {
                    return e.setUTCMinutes(t);
                }, i: function (e, t) {
                    return e.setUTCMinutes(t);
                }, ss: function (e, t) {
                    return e.setUTCSeconds(t);
                }, s: function (e, t) {
                    return e.setUTCSeconds(t);
                }, yyyy: function (e, t) {
                    return e.setUTCFullYear(t);
                }, yy: function (e, t) {
                    return e.setUTCFullYear(2e3 + t);
                }, m: function (e, t) {
                    for (t -= 1; 0 > t;)
                        t += 12;
                    for (t %= 12, e.setUTCMonth(t); e.getUTCMonth() != t;) {
                        if (isNaN(e.getUTCMonth()))
                            return e;
                        e.setUTCDate(e.getUTCDate() - 1);
                    }
                    return e;
                }, d: function (e, t) {
                    return e.setUTCDate(t);
                }, p: function (e, t) {
                    return e.setUTCHours(1 == t ? e.getUTCHours() + 12 : e.getUTCHours());
                } };
            if (w.M = w.MM = w.mm = w.m, w.dd = w.d, w.P = w.p, s = t(s.getFullYear(), s.getMonth(), s.getDate(), s.getHours(), s.getMinutes(), s.getSeconds()), u.length == n.parts.length) {
                for (var m = 0, D = n.parts.length; D > m; m++) {
                    if (p = parseInt(u[m], 10), d = n.parts[m], isNaN(p))
                        switch (d) {
                            case "MM":
                                v = e(a[h].months).filter(function () {
                                    var e = this.slice(0, u[m].length), t = u[m].slice(0, e.length);
                                    return e == t;
                                }), p = e.inArray(v[0], a[h].months) + 1;
                                break;
                            case "M":
                                v = e(a[h].monthsShort).filter(function () {
                                    var e = this.slice(0, u[m].length), t = u[m].slice(0, e.length);
                                    return e.toLowerCase() == t.toLowerCase();
                                }), p = e.inArray(v[0], a[h].monthsShort) + 1;
                                break;
                            case "p":
                            case "P":
                                p = e.inArray(u[m].toLowerCase(), a[h].meridiem);
                        }
                    g[d] = p;
                }
                for (var y, m = 0; m < f.length; m++)
                    y = f[m], y in g && !isNaN(g[y]) && w[y](s, g[y]);
            }
            return s;
        }, formatDate: function (t, i, s, h) {
            if (null == t)
                return "";
            var o;
            if ("standard" == h)
                o = { yy: t.getUTCFullYear().toString().substring(2), yyyy: t.getUTCFullYear(), m: t.getUTCMonth() + 1, M: a[s].monthsShort[t.getUTCMonth()], MM: a[s].months[t.getUTCMonth()], d: t.getUTCDate(), D: a[s].daysShort[t.getUTCDay()], DD: a[s].days[t.getUTCDay()], p: 2 == a[s].meridiem.length ? a[s].meridiem[t.getUTCHours() < 12 ? 0 : 1] : "", h: t.getUTCHours(), i: t.getUTCMinutes(), s: t.getUTCSeconds() }, o.H = 2 == a[s].meridiem.length ? o.h % 12 == 0 ? 12 : o.h % 12 : o.h, o.HH = (o.H < 10 ? "0" : "") + o.H, o.P = o.p.toUpperCase(), o.hh = (o.h < 10 ? "0" : "") + o.h, o.ii = (o.i < 10 ? "0" : "") + o.i, o.ss = (o.s < 10 ? "0" : "") + o.s, o.dd = (o.d < 10 ? "0" : "") + o.d, o.mm = (o.m < 10 ? "0" : "") + o.m;
            else {
                if ("php" != h)
                    throw new Error("Invalid format type.");
                o = { y: t.getUTCFullYear().toString().substring(2), Y: t.getUTCFullYear(), F: a[s].months[t.getUTCMonth()], M: a[s].monthsShort[t.getUTCMonth()], n: t.getUTCMonth() + 1, t: n.getDaysInMonth(t.getUTCFullYear(), t.getUTCMonth()), j: t.getUTCDate(), l: a[s].days[t.getUTCDay()], D: a[s].daysShort[t.getUTCDay()], w: t.getUTCDay(), N: 0 == t.getUTCDay() ? 7 : t.getUTCDay(), S: t.getUTCDate() % 10 <= a[s].suffix.length ? a[s].suffix[t.getUTCDate() % 10 - 1] : "", a: 2 == a[s].meridiem.length ? a[s].meridiem[t.getUTCHours() < 12 ? 0 : 1] : "", g: t.getUTCHours() % 12 == 0 ? 12 : t.getUTCHours() % 12, G: t.getUTCHours(), i: t.getUTCMinutes(), s: t.getUTCSeconds() }, o.m = (o.n < 10 ? "0" : "") + o.n, o.d = (o.j < 10 ? "0" : "") + o.j, o.A = o.a.toString().toUpperCase(), o.h = (o.g < 10 ? "0" : "") + o.g, o.H = (o.G < 10 ? "0" : "") + o.G, o.i = (o.i < 10 ? "0" : "") + o.i, o.s = (o.s < 10 ? "0" : "") + o.s;
            }
            for (var t = [], r = e.extend([], i.separators), d = 0, l = i.parts.length; l > d; d++)
                r.length && t.push(r.shift()), t.push(o[i.parts[d]]);
            return r.length && t.push(r.shift()), t.join("");
        }, convertViewMode: function (e) {
            switch (e) {
                case 4:
                case "decade":
                    e = 4;
                    break;
                case 3:
                case "year":
                    e = 3;
                    break;
                case 2:
                case "month":
                    e = 2;
                    break;
                case 1:
                case "day":
                    e = 1;
                    break;
                case 0:
                case "hour":
                    e = 0;
            }
            return e;
        }, headTemplate: '<thead><tr><th class="prev"><i class="{iconType} {leftArrow}"/></th><th colspan="5" class="switch"></th><th class="next"><i class="{iconType} {rightArrow}"/></th></tr></thead>', headTemplateV3: '<thead><tr><th class="prev"><span class="{iconType} {leftArrow}"></span> </th><th colspan="5" class="switch"></th><th class="next"><span class="{iconType} {rightArrow}"></span> </th></tr></thead>', contTemplate: '<tbody><tr><td colspan="7"></td></tr></tbody>', footTemplate: '<tfoot><tr><th colspan="7" class="today"></th></tr></tfoot>' };
    n.template = '<div class="datetimepicker"><div class="datetimepicker-minutes"><table class=" table-condensed">' + n.headTemplate + n.contTemplate + n.footTemplate + '</table></div><div class="datetimepicker-hours"><table class=" table-condensed">' + n.headTemplate + n.contTemplate + n.footTemplate + '</table></div><div class="datetimepicker-days"><table class=" table-condensed">' + n.headTemplate + "<tbody></tbody>" + n.footTemplate + '</table></div><div class="datetimepicker-months"><table class="table-condensed">' + n.headTemplate + n.contTemplate + n.footTemplate + '</table></div><div class="datetimepicker-years"><table class="table-condensed">' + n.headTemplate + n.contTemplate + n.footTemplate + "</table></div></div>", n.templateV3 = '<div class="datetimepicker"><div class="datetimepicker-minutes"><table class=" table-condensed">' + n.headTemplateV3 + n.contTemplate + n.footTemplate + '</table></div><div class="datetimepicker-hours"><table class=" table-condensed">' + n.headTemplateV3 + n.contTemplate + n.footTemplate + '</table></div><div class="datetimepicker-days"><table class=" table-condensed">' + n.headTemplateV3 + "<tbody></tbody>" + n.footTemplate + '</table></div><div class="datetimepicker-months"><table class="table-condensed">' + n.headTemplateV3 + n.contTemplate + n.footTemplate + '</table></div><div class="datetimepicker-years"><table class="table-condensed">' + n.headTemplateV3 + n.contTemplate + n.footTemplate + "</table></div></div>", e.fn.datetimepicker.DPGlobal = n, e.fn.datetimepicker.noConflict = function () {
        return e.fn.datetimepicker = s, this;
    }, e(document).on("focus.datetimepicker.data-api click.datetimepicker.data-api", '[data-provide="datetimepicker"]', function (t) {
        var i = e(this);
        i.data("datetimepicker") || (t.preventDefault(), i.datetimepicker("show"));
    }), e(function () {
        e('[data-provide="datetimepicker-inline"]').datetimepicker();
    });
}(window.jQuery), function (e) {
    e.fn.datetimepicker.dates["zh-CN"] = { days: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六", "星期日"], daysShort: ["周日", "周一", "周二", "周三", "周四", "周五", "周六", "周日"], daysMin: ["日", "一", "二", "三", "四", "五", "六", "日"], months: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"], monthsShort: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"], today: "今天", suffix: [], meridiem: ["上午", "下午"] };
}(jQuery);
;
angular.module("directives", ["validator", "placeholder", "cardInput", "moneyInput", "pagination", "docClick", "password", "pop", "security", "combox", "customInput", "groupfilter", "chart", "echart", "verifycode", "nosessionSms", "waiting", "formValidate", "datepicker", "print", "disable", "numberInput", "certInfo", "autoPadding", "advertise", "productNavigator", "goldCar", "evoucher", "myVoucherList", "financialCalendar"]);
;
angular.module("disable", []).directive("ngDisabled", ["$log", function () {
        return { link: function (e, a, l) {
                var n = "elem_disabled";
                e.$watch(l.ngDisabled, function (e) {
                    e ? a.addClass(n) : a.removeClass(n);
                });
            } };
    }]);
;
angular.module("docClick", []).directive("docClick", ["$parse", "$rootScope", function (c, n) {
        return { restrict: "A", link: function (o, i, t) {
                var e = c(t.docClick), l = !0, r = function () {
                    l = !1;
                }, u = function () {
                    var c = function () {
                        e(o);
                    };
                    l ? n.$$phase ? o.$evalAsync(c) : o.$apply(c) : l = !0;
                };
                $(i).on("click", r), $(document).on("click", u), o.$on("$destroy", function () {
                    $(i).off("click", r), $(document).off("click", u);
                });
            } };
    }]);
;
angular.module("echart", []).directive("echart", ["$log", "$window", function () {
        return { restrict: "A", scope: { onInit: "&" }, link: function (t, e) {
                function i(t) {
                    n = echarts.init(e[0]), n.setOption(t, !0);
                }
                function r() {
                    null != n && n.clear();
                }
                var n = null;
                t.onInit({ $operator: { init: i, clear: r } });
            } };
    }]), function (t, e) {
    "function" == typeof define && define.amd ? define([], e) : "object" == typeof module && module.exports ? module.exports = e() : t.echarts = e();
}(this, function () {
    var t, e;
    !function () {
        function i(t, e) {
            if (!e)
                return t;
            if (0 === t.indexOf(".")) {
                var i = e.split("/"), r = t.split("/"), n = i.length - 1, a = r.length, o = 0, s = 0;
                t:
                for (var l = 0; a > l; l++)
                    switch (r[l]) {
                        case "..":
                            if (!(n > o))
                                break t;
                            o++, s++;
                            break;
                        case ".":
                            s++;
                            break;
                        default:
                            break t;
                    }
                return i.length = n - o, r = r.slice(s), i.concat(r).join("/");
            }
            return t;
        }
        function r(t) {
            function e(e, o) {
                if ("string" == typeof e) {
                    var s = r[e];
                    return s || (s = a(i(e, t)), r[e] = s), s;
                }
                e instanceof Array && (o = o || function () {
                }, o.apply(this, n(e, o, t)));
            }
            var r = {};
            return e;
        }
        function n(e, r, n) {
            for (var s = [], l = o[n], h = 0, u = Math.min(e.length, r.length); u > h; h++) {
                var c, f = i(e[h], n);
                switch (f) {
                    case "require":
                        c = l && l.require || t;
                        break;
                    case "exports":
                        c = l.exports;
                        break;
                    case "module":
                        c = l;
                        break;
                    default:
                        c = a(f);
                }
                s.push(c);
            }
            return s;
        }
        function a(t) {
            var e = o[t];
            if (!e)
                throw new Error("No " + t);
            if (!e.defined) {
                var i = e.factory, r = i.apply(this, n(e.deps || [], i, t));
                "undefined" != typeof r && (e.exports = r), e.defined = 1;
            }
            return e.exports;
        }
        var o = {};
        e = function (t, e, i) {
            o[t] = { id: t, deps: e, factory: i, defined: 0, exports: {}, require: r(t) };
        }, t = r("");
    }(), e("echarts/chart/pie", ["require", "zrender/core/util", "../echarts", "./pie/PieSeries", "./pie/PieView", "../action/createDataSelectAction", "../visual/dataColor", "./pie/pieLayout", "../processor/dataFilter"], function (t) {
        var e = t("zrender/core/util"), i = t("../echarts");
        t("./pie/PieSeries"), t("./pie/PieView"), t("../action/createDataSelectAction")("pie", [{ type: "pieToggleSelect", event: "pieselectchanged", method: "toggleSelected" }, { type: "pieSelect", event: "pieselected", method: "select" }, { type: "pieUnSelect", event: "pieunselected", method: "unSelect" }]), i.registerVisual(e.curry(t("../visual/dataColor"), "pie")), i.registerLayout(e.curry(t("./pie/pieLayout"), "pie")), i.registerProcessor(e.curry(t("../processor/dataFilter"), "pie"));
    }), e("echarts/component/tooltip", ["require", "./tooltip/TooltipModel", "./tooltip/TooltipView", "../echarts"], function (t) {
        t("./tooltip/TooltipModel"), t("./tooltip/TooltipView"), t("../echarts").registerAction({ type: "showTip", event: "showTip", update: "none" }, function () {
        }), t("../echarts").registerAction({ type: "hideTip", event: "hideTip", update: "none" }, function () {
        });
    }), e("zrender/vml/vml", ["require", "./graphic", "../zrender", "./Painter"], function (t) {
        t("./graphic"), t("../zrender").registerPainter("vml", t("./Painter"));
    }), e("echarts/scale/Time", ["require", "zrender/core/util", "../util/number", "../util/format", "./Interval"], function (t) {
        var e = t("zrender/core/util"), i = t("../util/number"), r = t("../util/format"), n = t("./Interval"), a = n.prototype, o = Math.ceil, s = Math.floor, l = 1e3, h = 60 * l, u = 60 * h, c = 24 * u, f = function (t, e, i, r) {
            for (; r > i;) {
                var n = i + r >>> 1;
                t[n][2] < e ? i = n + 1 : r = n;
            }
            return i;
        }, d = n.extend({ type: "time", getLabel: function (t) {
                var e = this._stepLvl, i = new Date(t);
                return r.formatTime(e[0], i);
            }, niceExtent: function (t, e, r) {
                var n = this._extent;
                if (n[0] === n[1] && (n[0] -= c, n[1] += c), n[1] === -1 / 0 && 1 / 0 === n[0]) {
                    var a = new Date;
                    n[1] = new Date(a.getFullYear(), a.getMonth(), a.getDate()), n[0] = n[1] - c;
                }
                this.niceTicks(t);
                var l = this._interval;
                e || (n[0] = i.round(s(n[0] / l) * l)), r || (n[1] = i.round(o(n[1] / l) * l));
            }, niceTicks: function (t) {
                t = t || 10;
                var e = this._extent, r = e[1] - e[0], n = r / t, a = p.length, l = f(p, n, 0, a), h = p[Math.min(l, a - 1)], u = h[2];
                if ("year" === h[0]) {
                    var c = r / u, d = i.nice(c / t, !0);
                    u *= d;
                }
                var g = [o(e[0] / u) * u, s(e[1] / u) * u];
                this._stepLvl = h, this._interval = u, this._niceExtent = g;
            }, parse: function (t) {
                return +i.parseDate(t);
            } });
        e.each(["contain", "normalize"], function (t) {
            d.prototype[t] = function (e) {
                return a[t].call(this, this.parse(e));
            };
        });
        var p = [["hh:mm:ss", 1, l], ["hh:mm:ss", 5, 5 * l], ["hh:mm:ss", 10, 10 * l], ["hh:mm:ss", 15, 15 * l], ["hh:mm:ss", 30, 30 * l], ["hh:mm\nMM-dd", 1, h], ["hh:mm\nMM-dd", 5, 5 * h], ["hh:mm\nMM-dd", 10, 10 * h], ["hh:mm\nMM-dd", 15, 15 * h], ["hh:mm\nMM-dd", 30, 30 * h], ["hh:mm\nMM-dd", 1, u], ["hh:mm\nMM-dd", 2, 2 * u], ["hh:mm\nMM-dd", 6, 6 * u], ["hh:mm\nMM-dd", 12, 12 * u], ["MM-dd\nyyyy", 1, c], ["week", 7, 7 * c], ["month", 1, 31 * c], ["quarter", 3, 380 * c / 4], ["half-year", 6, 380 * c / 2], ["year", 1, 380 * c]];
        return d.create = function () {
            return new d;
        }, d;
    }), e("echarts/echarts", ["require", "zrender/core/env", "./model/Global", "./ExtensionAPI", "./CoordinateSystem", "./model/OptionManager", "./model/Component", "./model/Series", "./view/Component", "./view/Chart", "./util/graphic", "zrender", "zrender/core/util", "zrender/tool/color", "zrender/mixin/Eventful", "zrender/core/timsort", "./loading/default", "./visual/seriesColor", "./preprocessor/backwardCompat", "./data/List", "./model/Model", "./util/number", "./util/format", "zrender/core/matrix", "zrender/core/vector"], function (t) {
        function e(t) {
            return function (e, i, r) {
                e = e && e.toLowerCase(), A.prototype[t].call(this, e, i, r);
            };
        }
        function i() {
            A.call(this);
        }
        function r(t, e, r) {
            function n(t, e) {
                return t.prio - e.prio;
            }
            r = r || {}, "string" == typeof e && (e = $[e]), this.id, this.group, this._dom = t, this._zr = S.init(t, { renderer: r.renderer || "canvas", devicePixelRatio: r.devicePixelRatio }), this._theme = C.clone(e), this._chartsViews = [], this._chartsMap = {}, this._componentsViews = [], this._componentsMap = {}, this._api = new y(this), this._coordSysMgr = new _, A.call(this), this._messageCenter = new i, this._initEvents(), this.resize = C.bind(this.resize, this), this._pendingActions = [], k(Z, n), k(X, n);
        }
        function n(t, e) {
            var i = this._model;
            i && i.eachComponent({ mainType: "series", query: e }, function (r) {
                var n = this._chartsMap[r.__viewId];
                n && n.__alive && n[t](r, i, this._api, e);
            }, this);
        }
        function a(t, e, i) {
            var r = this._api;
            L(this._componentsViews, function (n) {
                var a = n.__model;
                n[t](a, e, r, i), p(a, n);
            }, this), e.eachSeries(function (n) {
                var a = this._chartsMap[n.__viewId];
                a[t](n, e, r, i), p(n, a), d(n, a);
            }, this), f(this._zr, e);
        }
        function o(t, e) {
            for (var i = "component" === t, r = i ? this._componentsViews : this._chartsViews, n = i ? this._componentsMap : this._chartsMap, a = this._zr, o = 0; o < r.length; o++)
                r[o].__alive = !1;
            e[i ? "eachComponent" : "eachSeries"](function (t, o) {
                if (i) {
                    if ("series" === t)
                        return;
                } else
                    o = t;
                var s = o.id + "_" + o.type, l = n[s];
                if (!l) {
                    var h = b.parseClassType(o.type), u = i ? T.getClass(h.main, h.sub) : M.getClass(h.sub);
                    if (!u)
                        return;
                    l = new u, l.init(e, this._api), n[s] = l, r.push(l), a.add(l.group);
                }
                o.__viewId = s, l.__alive = !0, l.__id = s, l.__model = o;
            }, this);
            for (var o = 0; o < r.length;) {
                var s = r[o];
                s.__alive ? o++ : (a.remove(s.group), s.dispose(e, this._api), r.splice(o, 1), delete n[s.__id]);
            }
        }
        function s(t, e) {
            L(X, function (i) {
                i.func(t, e);
            });
        }
        function l(t) {
            var e = {};
            t.eachSeries(function (t) {
                var i = t.get("stack"), r = t.getData();
                if (i && "list" === r.type) {
                    var n = e[i];
                    n && (r.stackedOn = n), e[i] = r;
                }
            });
        }
        function h(t, e) {
            var i = this._api;
            L(Z, function (r) {
                r.isLayout && r.func(t, i, e);
            });
        }
        function u(t, e) {
            var i = this._api;
            t.clearColorPalette(), t.eachSeries(function (t) {
                t.clearColorPalette();
            }), L(Z, function (r) {
                r.func(t, i, e);
            });
        }
        function c(t, e) {
            var i = this._api;
            L(this._componentsViews, function (r) {
                var n = r.__model;
                r.render(n, t, i, e), p(n, r);
            }, this), L(this._chartsViews, function (t) {
                t.__alive = !1;
            }, this), t.eachSeries(function (r) {
                var n = this._chartsMap[r.__viewId];
                n.__alive = !0, n.render(r, t, i, e), n.group.silent = !!r.get("silent"), p(r, n), d(r, n);
            }, this), f(this._zr, t), L(this._chartsViews, function (e) {
                e.__alive || e.remove(t, i);
            }, this);
        }
        function f(t, e) {
            var i = t.storage, r = 0;
            i.traverse(function (t) {
                t.isGroup || r++;
            }), r > e.get("hoverLayerThreshold") && !v.node && i.traverse(function (t) {
                t.isGroup || (t.useHoverLayer = !0);
            });
        }
        function d(t, e) {
            var i = 0;
            e.group.traverse(function (t) {
                "group" === t.type || t.ignore || i++;
            });
            var r = +t.get("progressive"), n = i > t.get("progressiveThreshold") && r && !v.node;
            n && e.group.traverse(function (t) {
                t.isGroup || (t.progressive = n ? Math.floor(i++ / r) : -1, n && t.stopAnimation(!0));
            });
            var a = t.get("blendMode") || null;
            !v.canvasSupported && a && "source-over" !== a && console.warn("Only canvas support blendMode"), e.group.traverse(function (t) {
                t.isGroup || t.setStyle("blend", a);
            });
        }
        function p(t, e) {
            var i = t.get("z"), r = t.get("zlevel");
            e.group.traverse(function (t) {
                "group" !== t.type && (null != i && (t.z = i), null != r && (t.zlevel = r));
            });
        }
        function g(t) {
            function e(t, e) {
                for (var i = 0; i < t.length; i++) {
                    var r = t[i];
                    r[a] = e;
                }
            }
            var i = 0, r = 1, n = 2, a = "__connectUpdateStatus";
            C.each(U, function (o, s) {
                t._messageCenter.on(s, function (o) {
                    if (K[t.group] && t[a] !== i) {
                        var s = t.makeActionFromEvent(o), l = [];
                        for (var h in Q) {
                            var u = Q[h];
                            u !== t && u.group === t.group && l.push(u);
                        }
                        e(l, i), L(l, function (t) {
                            t[a] !== r && t.dispatchAction(s);
                        }), e(l, n);
                    }
                });
            });
        }
        var v = t("zrender/core/env"), m = t("./model/Global"), y = t("./ExtensionAPI"), _ = t("./CoordinateSystem"), x = t("./model/OptionManager"), b = t("./model/Component"), w = t("./model/Series"), T = t("./view/Component"), M = t("./view/Chart"), z = t("./util/graphic"), S = t("zrender"), C = t("zrender/core/util"), P = t("zrender/tool/color"), A = t("zrender/mixin/Eventful"), k = t("zrender/core/timsort"), L = C.each, I = 1e3, D = 5e3, E = 1e3, O = 2e3, R = 3e3, q = 4e3, B = 5e3, F = "__flag_in_main_process", N = "_hasGradientOrPatternBg";
        i.prototype.on = e("on"), i.prototype.off = e("off"), i.prototype.one = e("one"), C.mixin(i, A);
        var V = r.prototype;
        V.getDom = function () {
            return this._dom;
        }, V.getZr = function () {
            return this._zr;
        }, V.setOption = function (t, e, i) {
            if (C.assert(!this[F], "`setOption` should not be called during main process."), this[F] = !0, !this._model || e) {
                var r = new x(this._api), n = this._theme, a = this._model = new m(null, null, n, r);
                a.init(null, null, n, r);
            }
            this._model.setOption(t, Y), H.prepareAndUpdate.call(this), this[F] = !1, this._flushPendingActions(), !i && this._zr.refreshImmediately();
        }, V.setTheme = function () {
            console.log("ECharts#setTheme() is DEPRECATED in ECharts 3.0");
        }, V.getModel = function () {
            return this._model;
        }, V.getOption = function () {
            return this._model.getOption();
        }, V.getWidth = function () {
            return this._zr.getWidth();
        }, V.getHeight = function () {
            return this._zr.getHeight();
        }, V.getRenderedCanvas = function (t) {
            if (v.canvasSupported) {
                t = t || {}, t.pixelRatio = t.pixelRatio || 1, t.backgroundColor = t.backgroundColor || this._model.get("backgroundColor");
                var e = this._zr, i = e.storage.getDisplayList();
                return C.each(i, function (t) {
                    t.stopAnimation(!0);
                }), e.painter.getRenderedCanvas(t);
            }
        }, V.getDataURL = function (t) {
            t = t || {};
            var e = t.excludeComponents, i = this._model, r = [], n = this;
            L(e, function (t) {
                i.eachComponent({ mainType: t }, function (t) {
                    var e = n._componentsMap[t.__viewId];
                    e.group.ignore || (r.push(e), e.group.ignore = !0);
                });
            });
            var a = this.getRenderedCanvas(t).toDataURL("image/" + (t && t.type || "png"));
            return L(r, function (t) {
                t.group.ignore = !1;
            }), a;
        }, V.getConnectedDataURL = function (t) {
            if (v.canvasSupported) {
                var e = this.group, i = Math.min, r = Math.max, n = 1 / 0;
                if (K[e]) {
                    var a = n, o = n, s = -n, l = -n, h = [], u = t && t.pixelRatio || 1;
                    for (var c in Q) {
                        var f = Q[c];
                        if (f.group === e) {
                            var d = f.getRenderedCanvas(C.clone(t)), p = f.getDom().getBoundingClientRect();
                            a = i(p.left, a), o = i(p.top, o), s = r(p.right, s), l = r(p.bottom, l), h.push({ dom: d, left: p.left, top: p.top });
                        }
                    }
                    a *= u, o *= u, s *= u, l *= u;
                    var g = s - a, m = l - o, y = C.createCanvas();
                    y.width = g, y.height = m;
                    var _ = S.init(y);
                    return L(h, function (t) {
                        var e = new z.Image({ style: { x: t.left * u - a, y: t.top * u - o, image: t.dom } });
                        _.add(e);
                    }), _.refreshImmediately(), y.toDataURL("image/" + (t && t.type || "png"));
                }
                return this.getDataURL(t);
            }
        };
        var H = { update: function (t) {
                var e = this._model, i = this._api, r = this._coordSysMgr, n = this._zr;
                if (e) {
                    e.restoreData(), r.create(this._model, this._api), s.call(this, e, i), l.call(this, e), r.update(e, i), u.call(this, e, t), c.call(this, e, t);
                    var a = e.get("backgroundColor") || "transparent", o = n.painter;
                    if (o.isSingleCanvas && o.isSingleCanvas())
                        n.configLayer(0, { clearColor: a });
                    else {
                        if (!v.canvasSupported) {
                            var h = P.parse(a);
                            a = P.stringify(h, "rgb"), 0 === h[3] && (a = "transparent");
                        }
                        a.colorStops || a.image ? (n.configLayer(0, { clearColor: a }), this[N] = !0, this._dom.style.background = "transparent") : (this[N] && n.configLayer(0, { clearColor: null }), this[N] = !1, this._dom.style.background = a);
                    }
                }
            }, updateView: function (t) {
                var e = this._model;
                e && (e.eachSeries(function (t) {
                    t.getData().clearAllVisual();
                }), u.call(this, e, t), a.call(this, "updateView", e, t));
            }, updateVisual: function (t) {
                var e = this._model;
                e && (e.eachSeries(function (t) {
                    t.getData().clearAllVisual();
                }), u.call(this, e, t), a.call(this, "updateVisual", e, t));
            }, updateLayout: function (t) {
                var e = this._model;
                e && (h.call(this, e, t), a.call(this, "updateLayout", e, t));
            }, highlight: function (t) {
                n.call(this, "highlight", t);
            }, downplay: function (t) {
                n.call(this, "downplay", t);
            }, prepareAndUpdate: function (t) {
                var e = this._model;
                o.call(this, "component", e), o.call(this, "chart", e), H.update.call(this, t);
            } };
        V.resize = function () {
            C.assert(!this[F], "`resize` should not be called during main process."), this[F] = !0, this._zr.resize();
            var t = this._model && this._model.resetOption("media");
            H[t ? "prepareAndUpdate" : "update"].call(this), this._loadingFX && this._loadingFX.resize(), this[F] = !1, this._flushPendingActions();
        };
        var G = t("./loading/default");
        V.showLoading = function (t, e) {
            C.isObject(t) && (e = t, t = "default"), this.hideLoading();
            var i = G(this._api, e), r = this._zr;
            this._loadingFX = i, r.add(i);
        }, V.hideLoading = function () {
            this._loadingFX && this._zr.remove(this._loadingFX), this._loadingFX = null;
        }, V.makeActionFromEvent = function (t) {
            var e = C.extend({}, t);
            return e.type = U[t.type], e;
        }, V.dispatchAction = function (t, e) {
            var i = j[t.type];
            if (i) {
                var r = i.actionInfo, n = r.update || "update";
                if (this[F])
                    return void this._pendingActions.push(t);
                this[F] = !0;
                var a = [t], o = !1;
                t.batch && (o = !0, a = C.map(t.batch, function (e) {
                    return e = C.defaults(C.extend({}, e), t), e.batch = null, e;
                }));
                for (var s, l = [], h = "highlight" === t.type || "downplay" === t.type, u = 0; u < a.length; u++) {
                    var c = a[u];
                    s = i.action(c, this._model), s = s || C.extend({}, c), s.type = r.event || s.type, l.push(s), h && H[n].call(this, c);
                }
                "none" !== n && !h && H[n].call(this, t), s = o ? { type: r.event || t.type, batch: l } : l[0], this[F] = !1, !e && this._messageCenter.trigger(s.type, s), this._flushPendingActions();
            }
        }, V._flushPendingActions = function () {
            for (var t = this._pendingActions; t.length;) {
                var e = t.shift();
                this.dispatchAction(e);
            }
        }, V.on = e("on"), V.off = e("off"), V.one = e("one");
        var W = ["click", "dblclick", "mouseover", "mouseout", "mousedown", "mouseup", "globalout"];
        V._initEvents = function () {
            L(W, function (t) {
                this._zr.on(t, function (e) {
                    var i = this.getModel(), r = e.target;
                    if (r && null != r.dataIndex) {
                        var n = r.dataModel || i.getSeriesByIndex(r.seriesIndex), a = n && n.getDataParams(r.dataIndex, r.dataType) || {};
                        a.event = e, a.type = t, this.trigger(t, a);
                    } else
                        r && r.eventData && this.trigger(t, r.eventData);
                }, this);
            }, this), L(U, function (t, e) {
                this._messageCenter.on(e, function (t) {
                    this.trigger(e, t);
                }, this);
            }, this);
        }, V.isDisposed = function () {
            return this._disposed;
        }, V.clear = function () {
            this.setOption({}, !0);
        }, V.dispose = function () {
            if (this._disposed)
                return void console.warn("Instance " + this.id + " has been disposed");
            this._disposed = !0;
            var t = this._api, e = this._model;
            L(this._componentsViews, function (i) {
                i.dispose(e, t);
            }), L(this._chartsViews, function (i) {
                i.dispose(e, t);
            }), this._zr.dispose(), delete Q[this.id];
        }, C.mixin(r, A);
        var j = [], U = {}, X = [], Y = [], Z = [], $ = {}, Q = {}, K = {}, J = new Date - 0, te = new Date - 0, ee = "_echarts_instance_", ie = { version: "3.2.2", dependencies: { zrender: "3.1.2" } };
        ie.init = function (t, e, i) {
            if (S.version.replace(".", "") - 0 < ie.dependencies.zrender.replace(".", "") - 0)
                throw new Error("ZRender " + S.version + " is too old for ECharts " + ie.version + ". Current version need ZRender " + ie.dependencies.zrender + "+");
            if (!t)
                throw new Error("Initialize failed: invalid dom.");
            !C.isDom(t) || "CANVAS" === t.nodeName.toUpperCase() || t.clientWidth && t.clientHeight || console.warn("Can't get dom width or height");
            var n = new r(t, e, i);
            return n.id = "ec_" + J++, Q[n.id] = n, t.setAttribute && t.setAttribute(ee, n.id), g(n), n;
        }, ie.connect = function (t) {
            if (C.isArray(t)) {
                var e = t;
                t = null, C.each(e, function (e) {
                    null != e.group && (t = e.group);
                }), t = t || "g_" + te++, C.each(e, function (e) {
                    e.group = t;
                });
            }
            return K[t] = !0, t;
        }, ie.disConnect = function (t) {
            K[t] = !1;
        }, ie.dispose = function (t) {
            C.isDom(t) ? t = ie.getInstanceByDom(t) : "string" == typeof t && (t = Q[t]), t instanceof r && !t.isDisposed() && t.dispose();
        }, ie.getInstanceByDom = function (t) {
            var e = t.getAttribute(ee);
            return Q[e];
        }, ie.getInstanceById = function (t) {
            return Q[t];
        }, ie.registerTheme = function (t, e) {
            $[t] = e;
        }, ie.registerPreprocessor = function (t) {
            Y.push(t);
        }, ie.registerProcessor = function (t, e) {
            if ("function" == typeof t && (e = t, t = I), isNaN(t))
                throw new Error("Unkown processor priority");
            X.push({ prio: t, func: e });
        }, ie.registerAction = function (t, e, i) {
            "function" == typeof e && (i = e, e = "");
            var r = C.isObject(t) ? t.type : [t, t = { event: e }][0];
            t.event = (t.event || r).toLowerCase(), e = t.event, j[r] || (j[r] = { action: i, actionInfo: t }), U[e] = r;
        }, ie.registerCoordinateSystem = function (t, e) {
            _.register(t, e);
        }, ie.registerLayout = function (t, e) {
            if ("function" == typeof t && (e = t, t = E), isNaN(t))
                throw new Error("Unkown layout priority");
            Z.push({ prio: t, func: e, isLayout: !0 });
        }, ie.registerVisual = function (t, e) {
            if ("function" == typeof t && (e = t, t = R), isNaN(t))
                throw new Error("Unkown visual priority");
            Z.push({ prio: t, func: e });
        };
        var re = b.parseClassType;
        return ie.extendComponentModel = function (t, e) {
            var i = b;
            if (e) {
                var r = re(e);
                i = b.getClass(r.main, r.sub, !0);
            }
            return i.extend(t);
        }, ie.extendComponentView = function (t, e) {
            var i = T;
            if (e) {
                var r = re(e);
                i = T.getClass(r.main, r.sub, !0);
            }
            return i.extend(t);
        }, ie.extendSeriesModel = function (t, e) {
            var i = w;
            if (e) {
                e = "series." + e.replace("series.", "");
                var r = re(e);
                i = w.getClass(r.main, r.sub, !0);
            }
            return i.extend(t);
        }, ie.extendChartView = function (t, e) {
            var i = M;
            if (e) {
                e.replace("series.", "");
                var r = re(e);
                i = M.getClass(r.main, !0);
            }
            return M.extend(t);
        }, ie.setCanvasCreator = function (t) {
            C.createCanvas = t;
        }, ie.registerVisual(O, t("./visual/seriesColor")), ie.registerPreprocessor(t("./preprocessor/backwardCompat")), ie.registerAction({ type: "highlight", event: "highlight", update: "highlight" }, C.noop), ie.registerAction({ type: "downplay", event: "downplay", update: "downplay" }, C.noop), ie.List = t("./data/List"), ie.Model = t("./model/Model"), ie.graphic = t("./util/graphic"), ie.number = t("./util/number"), ie.format = t("./util/format"), ie.matrix = t("zrender/core/matrix"), ie.vector = t("zrender/core/vector"), ie.color = t("zrender/tool/color"), ie.util = {}, L(["map", "each", "filter", "indexOf", "inherits", "reduce", "filter", "bind", "curry", "isArray", "isString", "isObject", "isFunction", "extend", "defaults"], function (t) {
            ie.util[t] = C[t];
        }), ie.PRIORITY = { PROCESSOR: { FILTER: I, STATISTIC: D }, VISUAL: { LAYOUT: E, GLOBAL: O, CHART: R, COMPONENT: q, BRUSH: B } }, ie;
    }), e("echarts/scale/Log", ["require", "zrender/core/util", "./Scale", "../util/number", "./Interval"], function (t) {
        var e = t("zrender/core/util"), i = t("./Scale"), r = t("../util/number"), n = t("./Interval"), a = i.prototype, o = n.prototype, s = Math.floor, l = Math.ceil, h = Math.pow, u = 10, c = Math.log, f = i.extend({ type: "log", getTicks: function () {
                return e.map(o.getTicks.call(this), function (t) {
                    return r.round(h(u, t));
                });
            }, getLabel: o.getLabel, scale: function (t) {
                return t = a.scale.call(this, t), h(u, t);
            }, setExtent: function (t, e) {
                t = c(t) / c(u), e = c(e) / c(u), o.setExtent.call(this, t, e);
            }, getExtent: function () {
                var t = a.getExtent.call(this);
                return t[0] = h(u, t[0]), t[1] = h(u, t[1]), t;
            }, unionExtent: function (t) {
                t[0] = c(t[0]) / c(u), t[1] = c(t[1]) / c(u), a.unionExtent.call(this, t);
            }, niceTicks: function (t) {
                t = t || 10;
                var e = this._extent, i = e[1] - e[0];
                if (!(1 / 0 === i || 0 >= i)) {
                    var n = h(10, s(c(i / t) / Math.LN10)), a = t / i * n;
                    .5 >= a && (n *= 10);
                    var o = [r.round(l(e[0] / n) * n), r.round(s(e[1] / n) * n)];
                    this._interval = n, this._niceExtent = o;
                }
            }, niceExtent: o.niceExtent });
        return e.each(["contain", "normalize"], function (t) {
            f.prototype[t] = function (e) {
                return e = c(e) / c(u), a[t].call(this, e);
            };
        }), f.create = function () {
            return new f;
        }, f;
    }), e("echarts/chart/pie/PieSeries", ["require", "../../data/List", "zrender/core/util", "../../util/model", "../../data/helper/completeDimensions", "../../component/helper/selectableMixin", "../../echarts"], function (t) {
        "use strict";
        var e = t("../../data/List"), i = t("zrender/core/util"), r = t("../../util/model"), n = t("../../data/helper/completeDimensions"), a = t("../../component/helper/selectableMixin"), o = t("../../echarts").extendSeriesModel({ type: "series.pie", init: function (t) {
                o.superApply(this, "init", arguments), this.legendDataProvider = function () {
                    return this._dataBeforeProcessed;
                }, this.updateSelectedMap(t.data), this._defaultLabelLine(t);
            }, mergeOption: function (t) {
                o.superCall(this, "mergeOption", t), this.updateSelectedMap(this.option.data);
            }, getInitialData: function (t) {
                var i = n(["value"], t.data), r = new e(i, this);
                return r.initData(t.data), r;
            }, getDataParams: function (t) {
                var e = this._data, i = o.superCall(this, "getDataParams", t), r = e.getSum("value");
                return i.percent = r ? +(e.get("value", t) / r * 100).toFixed(2) : 0, i.$vars.push("percent"), i;
            }, _defaultLabelLine: function (t) {
                r.defaultEmphasis(t.labelLine, ["show"]);
                var e = t.labelLine.normal, i = t.labelLine.emphasis;
                e.show = e.show && t.label.normal.show, i.show = i.show && t.label.emphasis.show;
            }, defaultOption: { zlevel: 0, z: 2, legendHoverLink: !0, hoverAnimation: !0, center: ["50%", "50%"], radius: [0, "75%"], clockwise: !0, startAngle: 90, minAngle: 0, selectedOffset: 10, avoidLabelOverlap: !0, label: { normal: { rotate: !1, show: !0, position: "outer" }, emphasis: {} }, labelLine: { normal: { show: !0, length: 15, length2: 15, smooth: !1, lineStyle: { width: 1, type: "solid" } } }, itemStyle: { normal: { borderWidth: 1 }, emphasis: {} }, animationEasing: "cubicOut", data: [] } });
        return i.mixin(o, a), o;
    }), e("zrender/core/util", ["require"], function () {
        function t(e) {
            if ("object" == typeof e && null !== e) {
                var i = e;
                if (e instanceof Array) {
                    i = [];
                    for (var r = 0, n = e.length; n > r; r++)
                        i[r] = t(e[r]);
                } else if (!w(e) && !T(e)) {
                    i = {};
                    for (var a in e)
                        e.hasOwnProperty(a) && (i[a] = t(e[a]));
                }
                return i;
            }
            return e;
        }
        function e(i, r, n) {
            if (!b(r) || !b(i))
                return n ? t(r) : i;
            for (var a in r)
                if (r.hasOwnProperty(a)) {
                    var o = i[a], s = r[a];
                    !b(s) || !b(o) || y(s) || y(o) || T(s) || T(o) || w(s) || w(o) ? !n && a in i || (i[a] = t(r[a], !0)) : e(o, s, n);
                }
            return i;
        }
        function i(t, i) {
            for (var r = t[0], n = 1, a = t.length; a > n; n++)
                r = e(r, t[n], i);
            return r;
        }
        function r(t, e) {
            for (var i in e)
                e.hasOwnProperty(i) && (t[i] = e[i]);
            return t;
        }
        function n(t, e, i) {
            for (var r in e)
                e.hasOwnProperty(r) && (i ? null != e[r] : null == t[r]) && (t[r] = e[r]);
            return t;
        }
        function a() {
            return document.createElement("canvas");
        }
        function o() {
            return C || (C = R.createCanvas().getContext("2d")), C;
        }
        function s(t, e) {
            if (t) {
                if (t.indexOf)
                    return t.indexOf(e);
                for (var i = 0, r = t.length; r > i; i++)
                    if (t[i] === e)
                        return i;
            }
            return -1;
        }
        function l(t, e) {
            function i() {
            }
            var r = t.prototype;
            i.prototype = e.prototype, t.prototype = new i;
            for (var n in r)
                t.prototype[n] = r[n];
            t.prototype.constructor = t, t.superClass = e;
        }
        function h(t, e, i) {
            t = "prototype" in t ? t.prototype : t, e = "prototype" in e ? e.prototype : e, n(t, e, i);
        }
        function u(t) {
            return t ? "string" == typeof t ? !1 : "number" == typeof t.length : void 0;
        }
        function c(t, e, i) {
            if (t && e)
                if (t.forEach && t.forEach === L)
                    t.forEach(e, i);
                else if (t.length === +t.length)
                    for (var r = 0, n = t.length; n > r; r++)
                        e.call(i, t[r], r, t);
                else
                    for (var a in t)
                        t.hasOwnProperty(a) && e.call(i, t[a], a, t);
        }
        function f(t, e, i) {
            if (t && e) {
                if (t.map && t.map === E)
                    return t.map(e, i);
                for (var r = [], n = 0, a = t.length; a > n; n++)
                    r.push(e.call(i, t[n], n, t));
                return r;
            }
        }
        function d(t, e, i, r) {
            if (t && e) {
                if (t.reduce && t.reduce === O)
                    return t.reduce(e, i, r);
                for (var n = 0, a = t.length; a > n; n++)
                    i = e.call(r, i, t[n], n, t);
                return i;
            }
        }
        function p(t, e, i) {
            if (t && e) {
                if (t.filter && t.filter === I)
                    return t.filter(e, i);
                for (var r = [], n = 0, a = t.length; a > n; n++)
                    e.call(i, t[n], n, t) && r.push(t[n]);
                return r;
            }
        }
        function g(t, e, i) {
            if (t && e)
                for (var r = 0, n = t.length; n > r; r++)
                    if (e.call(i, t[r], r, t))
                        return t[r];
        }
        function v(t, e) {
            var i = D.call(arguments, 2);
            return function () {
                return t.apply(e, i.concat(D.call(arguments)));
            };
        }
        function m(t) {
            var e = D.call(arguments, 1);
            return function () {
                return t.apply(this, e.concat(D.call(arguments)));
            };
        }
        function y(t) {
            return "[object Array]" === A.call(t);
        }
        function _(t) {
            return "function" == typeof t;
        }
        function x(t) {
            return "[object String]" === A.call(t);
        }
        function b(t) {
            var e = typeof t;
            return "function" === e || !!t && "object" == e;
        }
        function w(t) {
            return !!P[A.call(t)];
        }
        function T(t) {
            return t && 1 === t.nodeType && "string" == typeof t.nodeName;
        }
        function M() {
            for (var t = 0, e = arguments.length; e > t; t++)
                if (null != arguments[t])
                    return arguments[t];
        }
        function z() {
            return Function.call.apply(D, arguments);
        }
        function S(t, e) {
            if (!t)
                throw new Error(e);
        }
        var C, P = { "[object Function]": 1, "[object RegExp]": 1, "[object Date]": 1, "[object Error]": 1, "[object CanvasGradient]": 1, "[object CanvasPattern]": 1, "[object Image]": 1 }, A = Object.prototype.toString, k = Array.prototype, L = k.forEach, I = k.filter, D = k.slice, E = k.map, O = k.reduce, R = { inherits: l, mixin: h, clone: t, merge: e, mergeAll: i, extend: r, defaults: n, getContext: o, createCanvas: a, indexOf: s, slice: z, find: g, isArrayLike: u, each: c, map: f, reduce: d, filter: p, bind: v, curry: m, isArray: y, isString: x, isObject: b, isFunction: _, isBuildInObject: w, isDom: T, retrieve: M, assert: S, noop: function () {
            } };
        return R;
    }), e("echarts/chart/pie/PieView", ["require", "../../util/graphic", "zrender/core/util", "../../view/Chart"], function (t) {
        function e(t, e, r, n) {
            var a = e.getData(), o = this.dataIndex, s = a.getName(o), l = e.get("selectedOffset");
            n.dispatchAction({ type: "pieToggleSelect", from: t, name: s, seriesId: e.id }), a.each(function (t) {
                i(a.getItemGraphicEl(t), a.getItemLayout(t), e.isSelected(a.getName(t)), l, r);
            });
        }
        function i(t, e, i, r, n) {
            var a = (e.startAngle + e.endAngle) / 2, o = Math.cos(a), s = Math.sin(a), l = i ? r : 0, h = [o * l, s * l];
            n ? t.animate().when(200, { position: h }).start("bounceOut") : t.attr("position", h);
        }
        function r(t, e) {
            function i() {
                o.ignore = o.hoverIgnore, s.ignore = s.hoverIgnore;
            }
            function r() {
                o.ignore = o.normalIgnore, s.ignore = s.normalIgnore;
            }
            a.Group.call(this);
            var n = new a.Sector({ z2: 2 }), o = new a.Polyline, s = new a.Text;
            this.add(n), this.add(o), this.add(s), this.updateData(t, e, !0), this.on("emphasis", i).on("normal", r).on("mouseover", i).on("mouseout", r);
        }
        function n(t, e, i, r, n) {
            var a = r.getModel("textStyle"), s = "inside" === n || "inner" === n;
            return { fill: a.getTextColor() || (s ? "#fff" : t.getItemVisual(e, "color")), opacity: t.getItemVisual(e, "opacity"), textFont: a.getFont(), text: o.retrieve(t.hostModel.getFormattedLabel(e, i), t.getName(e)) };
        }
        var a = t("../../util/graphic"), o = t("zrender/core/util"), s = r.prototype;
        s.updateData = function (t, e, r) {
            function n() {
                l.stopAnimation(!0), l.animateTo({ shape: { r: c.r + 10 } }, 300, "elasticOut");
            }
            function s() {
                l.stopAnimation(!0), l.animateTo({ shape: { r: c.r } }, 300, "elasticOut");
            }
            var l = this.childAt(0), h = t.hostModel, u = t.getItemModel(e), c = t.getItemLayout(e), f = o.extend({}, c);
            f.label = null, r ? (l.setShape(f), l.shape.endAngle = c.startAngle, a.updateProps(l, { shape: { endAngle: c.endAngle } }, h, e)) : a.updateProps(l, { shape: f }, h, e);
            var d = u.getModel("itemStyle"), p = t.getItemVisual(e, "color");
            l.useStyle(o.defaults({ lineJoin: "bevel", fill: p }, d.getModel("normal").getItemStyle())), l.hoverStyle = d.getModel("emphasis").getItemStyle(), i(this, t.getItemLayout(e), u.get("selected"), h.get("selectedOffset"), h.get("animation")), l.off("mouseover").off("mouseout").off("emphasis").off("normal"), u.get("hoverAnimation") && h.ifEnableAnimation() && l.on("mouseover", n).on("mouseout", s).on("emphasis", n).on("normal", s), this._updateLabel(t, e), a.setHoverStyle(this);
        }, s._updateLabel = function (t, e) {
            var i = this.childAt(1), r = this.childAt(2), o = t.hostModel, s = t.getItemModel(e), l = t.getItemLayout(e), h = l.label, u = t.getItemVisual(e, "color");
            a.updateProps(i, { shape: { points: h.linePoints || [[h.x, h.y], [h.x, h.y], [h.x, h.y]] } }, o, e), a.updateProps(r, { style: { x: h.x, y: h.y } }, o, e), r.attr({ style: { textVerticalAlign: h.verticalAlign, textAlign: h.textAlign, textFont: h.font }, rotation: h.rotation, origin: [h.x, h.y], z2: 10 });
            var c = s.getModel("label.normal"), f = s.getModel("label.emphasis"), d = s.getModel("labelLine.normal"), p = s.getModel("labelLine.emphasis"), g = c.get("position") || f.get("position");
            r.setStyle(n(t, e, "normal", c, g)), r.ignore = r.normalIgnore = !c.get("show"), r.hoverIgnore = !f.get("show"), i.ignore = i.normalIgnore = !d.get("show"), i.hoverIgnore = !p.get("show"), i.setStyle({ stroke: u, opacity: t.getItemVisual(e, "opacity") }), i.setStyle(d.getModel("lineStyle").getLineStyle()), r.hoverStyle = n(t, e, "emphasis", f, g), i.hoverStyle = p.getModel("lineStyle").getLineStyle();
            var v = d.get("smooth");
            v && v === !0 && (v = .4), i.setShape({ smooth: v });
        }, o.inherits(r, a.Group);
        var l = t("../../view/Chart").extend({ type: "pie", init: function () {
                var t = new a.Group;
                this._sectorGroup = t;
            }, render: function (t, i, n, a) {
                if (!a || a.from !== this.uid) {
                    var s = t.getData(), l = this._data, h = this.group, u = i.get("animation"), c = !l, f = o.curry(e, this.uid, t, u, n), d = t.get("selectedMode");
                    if (s.diff(l).add(function (t) {
                        var e = new r(s, t);
                        c && e.eachChild(function (t) {
                            t.stopAnimation(!0);
                        }), d && e.on("click", f), s.setItemGraphicEl(t, e), h.add(e);
                    }).update(function (t, e) {
                        var i = l.getItemGraphicEl(e);
                        i.updateData(s, t), i.off("click"), d && i.on("click", f), h.add(i), s.setItemGraphicEl(t, i);
                    }).remove(function (t) {
                        var e = l.getItemGraphicEl(t);
                        h.remove(e);
                    }).execute(), u && c && s.count() > 0) {
                        var p = s.getItemLayout(0), g = Math.max(n.getWidth(), n.getHeight()) / 2, v = o.bind(h.removeClipPath, h);
                        h.setClipPath(this._createClipPath(p.cx, p.cy, g, p.startAngle, p.clockwise, v, t));
                    }
                    this._data = s;
                }
            }, _createClipPath: function (t, e, i, r, n, o, s) {
                var l = new a.Sector({ shape: { cx: t, cy: e, r0: 0, r: i, startAngle: r, endAngle: r, clockwise: n } });
                return a.initProps(l, { shape: { endAngle: r + (n ? 1 : -1) * Math.PI * 2 } }, s, o), l;
            } });
        return l;
    }), e("echarts/visual/dataColor", ["require"], function () {
        return function (t, e) {
            var i = {};
            e.eachRawSeriesByType(t, function (t) {
                var r = t.getRawData(), n = {};
                if (!e.isSeriesFiltered(t)) {
                    var a = t.getData();
                    a.each(function (t) {
                        var e = a.getRawIndex(t);
                        n[e] = t;
                    }), r.each(function (e) {
                        var o = r.getItemModel(e), s = n[e], l = a.getItemVisual(s, "color", !0);
                        if (l)
                            r.setItemVisual(e, "color", l);
                        else {
                            var h = o.get("itemStyle.normal.color") || t.getColorFromPalette(r.getName(e), i);
                            r.setItemVisual(e, "color", h), a.setItemVisual(s, "color", h);
                        }
                    });
                }
            });
        };
    }), e("echarts/action/createDataSelectAction", ["require", "../echarts", "zrender/core/util"], function (t) {
        var e = t("../echarts"), i = t("zrender/core/util");
        return function (t, r) {
            i.each(r, function (i) {
                i.update = "updateView", e.registerAction(i, function (e, r) {
                    var n = {};
                    return r.eachComponent({ mainType: "series", subType: t, query: e }, function (t) {
                        t[i.method] && t[i.method](e.name);
                        var r = t.getData();
                        r.each(function (e) {
                            var i = r.getName(e);
                            n[i] = t.isSelected(i) || !1;
                        });
                    }), { name: e.name, selected: n };
                });
            });
        };
    }), e("echarts/component/tooltip/TooltipModel", ["require", "../../echarts"], function (t) {
        t("../../echarts").extendComponentModel({ type: "tooltip", defaultOption: { zlevel: 0, z: 8, show: !0, showContent: !0, trigger: "item", triggerOn: "mousemove", alwaysShowContent: !1, showDelay: 0, hideDelay: 100, transitionDuration: .4, enterable: !1, backgroundColor: "rgba(50,50,50,0.7)", borderColor: "#333", borderRadius: 4, borderWidth: 0, padding: 5, extraCssText: "", axisPointer: { type: "line", axis: "auto", animation: !0, animationDurationUpdate: 200, animationEasingUpdate: "exponentialOut", lineStyle: { color: "#555", width: 1, type: "solid" }, crossStyle: { color: "#555", width: 1, type: "dashed", textStyle: {} }, shadowStyle: { color: "rgba(150,150,150,0.3)" } }, textStyle: { color: "#fff", fontSize: 14 } } });
    }), e("echarts/processor/dataFilter", [], function () {
        return function (t, e) {
            var i = e.findComponents({ mainType: "legend" });
            i && i.length && e.eachSeriesByType(t, function (t) {
                var e = t.getData();
                e.filterSelf(function (t) {
                    for (var r = e.getName(t), n = 0; n < i.length; n++)
                        if (!i[n].isSelected(r))
                            return !1;
                    return !0;
                }, this);
            }, this);
        };
    }), e("echarts/chart/pie/pieLayout", ["require", "../../util/number", "./labelLayout", "zrender/core/util"], function (t) {
        var e = t("../../util/number"), i = e.parsePercent, r = t("./labelLayout"), n = t("zrender/core/util"), a = 2 * Math.PI, o = Math.PI / 180;
        return function (t, s, l) {
            s.eachSeriesByType(t, function (t) {
                var s = t.get("center"), h = t.get("radius");
                n.isArray(h) || (h = [0, h]), n.isArray(s) || (s = [s, s]);
                var u = l.getWidth(), c = l.getHeight(), f = Math.min(u, c), d = i(s[0], u), p = i(s[1], c), g = i(h[0], f / 2), v = i(h[1], f / 2), m = t.getData(), y = -t.get("startAngle") * o, _ = t.get("minAngle") * o, x = m.getSum("value"), b = Math.PI / (x || m.count()) * 2, w = t.get("clockwise"), T = t.get("roseType"), M = m.getDataExtent("value");
                M[0] = 0;
                var z = a, S = 0, C = y, P = w ? 1 : -1;
                if (m.each("value", function (t, i) {
                    var r;
                    r = "area" !== T ? 0 === x ? b : t * b : a / (m.count() || 1), _ > r ? (r = _, z -= _) : S += t;
                    var n = C + P * r;
                    m.setItemLayout(i, { angle: r, startAngle: C, endAngle: n, clockwise: w, cx: d, cy: p, r0: g, r: T ? e.linearMap(t, M, [g, v]) : v }), C = n;
                }, !0), a > z)
                    if (.001 >= z) {
                        var A = a / m.count();
                        m.each(function (t) {
                            var e = m.getItemLayout(t);
                            e.startAngle = y + P * t * A, e.endAngle = y + P * (t + 1) * A;
                        });
                    } else
                        b = z / S, C = y, m.each("value", function (t, e) {
                            var i = m.getItemLayout(e), r = i.angle === _ ? _ : t * b;
                            i.startAngle = C, i.endAngle = C + P * r, C += r;
                        });
                r(t, v, u, c);
            });
        };
    }), e("zrender/zrender", ["require", "./core/guid", "./core/env", "./Handler", "./Storage", "./animation/Animation", "./dom/HandlerProxy", "./Painter"], function (t) {
        function e(t) {
            delete u[t];
        }
        var i = t("./core/guid"), r = t("./core/env"), n = t("./Handler"), a = t("./Storage"), o = t("./animation/Animation"), s = t("./dom/HandlerProxy"), l = !r.canvasSupported, h = { canvas: t("./Painter") }, u = {}, c = {};
        c.version = "3.1.2", c.init = function (t, e) {
            var r = new f(i(), t, e);
            return u[r.id] = r, r;
        }, c.dispose = function (t) {
            if (t)
                t.dispose();
            else {
                for (var e in u)
                    u[e].dispose();
                u = {};
            }
            return c;
        }, c.getInstance = function (t) {
            return u[t];
        }, c.registerPainter = function (t, e) {
            h[t] = e;
        };
        var f = function (t, e, i) {
            i = i || {}, this.dom = e, this.id = t;
            var u = this, c = new a, f = i.renderer;
            if (l) {
                if (!h.vml)
                    throw new Error("You need to require 'zrender/vml/vml' to support IE8");
                f = "vml";
            } else
                f && h[f] || (f = "canvas");
            var d = new h[f](e, c, i);
            this.storage = c, this.painter = d;
            var p = r.node ? null : new s(d.getViewportRoot());
            this.handler = new n(c, d, p), this.animation = new o({ stage: { update: function () {
                        u._needsRefresh && u.refreshImmediately(), u._needsRefreshHover && u.refreshHoverImmediately();
                    } } }), this.animation.start(), this._needsRefresh;
            var g = c.delFromMap, v = c.addToMap;
            c.delFromMap = function (t) {
                var e = c.get(t);
                g.call(c, t), e && e.removeSelfFromZr(u);
            }, c.addToMap = function (t) {
                v.call(c, t), t.addSelfToZr(u);
            };
        };
        return f.prototype = { constructor: f, getId: function () {
                return this.id;
            }, add: function (t) {
                this.storage.addRoot(t), this._needsRefresh = !0;
            }, remove: function (t) {
                this.storage.delRoot(t), this._needsRefresh = !0;
            }, configLayer: function (t, e) {
                this.painter.configLayer(t, e), this._needsRefresh = !0;
            }, refreshImmediately: function () {
                this._needsRefresh = !1, this.painter.refresh(), this._needsRefresh = !1;
            }, refresh: function () {
                this._needsRefresh = !0;
            }, addHover: function (t, e) {
                this.painter.addHover && (this.painter.addHover(t, e), this.refreshHover());
            }, removeHover: function (t) {
                this.painter.removeHover && (this.painter.removeHover(t), this.refreshHover());
            }, clearHover: function () {
                this.painter.clearHover && (this.painter.clearHover(), this.refreshHover());
            }, refreshHover: function () {
                this._needsRefreshHover = !0;
            }, refreshHoverImmediately: function () {
                this._needsRefreshHover = !1, this.painter.refreshHover && this.painter.refreshHover();
            }, resize: function () {
                this.painter.resize(), this.handler.resize();
            }, clearAnimation: function () {
                this.animation.clear();
            }, getWidth: function () {
                return this.painter.getWidth();
            }, getHeight: function () {
                return this.painter.getHeight();
            }, pathToImage: function (t, e, r) {
                var n = i();
                return this.painter.pathToImage(n, t, e, r);
            }, setCursorStyle: function (t) {
                this.handler.setCursorStyle(t);
            }, on: function (t, e, i) {
                this.handler.on(t, e, i);
            }, off: function (t, e) {
                this.handler.off(t, e);
            }, trigger: function (t, e) {
                this.handler.trigger(t, e);
            }, clear: function () {
                this.storage.delRoot(), this.painter.clear();
            }, dispose: function () {
                this.animation.stop(), this.clear(), this.storage.dispose(), this.painter.dispose(), this.handler.dispose(), this.animation = this.storage = this.painter = this.handler = null, e(this.id);
            } }, c;
    }), e("zrender/vml/Painter", ["require", "../core/log", "./core"], function (t) {
        function e(t) {
            return parseInt(t, 10);
        }
        function i(t, e) {
            a.initVML(), this.root = t, this.storage = e;
            var i = document.createElement("div"), r = document.createElement("div");
            i.style.cssText = "display:inline-block;overflow:hidden;position:relative;width:300px;height:150px;", r.style.cssText = "position:absolute;left:0;top:0;", t.appendChild(i), this._vmlRoot = r, this._vmlViewport = i, this.resize();
            var n = e.delFromMap, o = e.addToMap;
            e.delFromMap = function (t) {
                var i = e.get(t);
                n.call(e, t), i && i.onRemove && i.onRemove(r);
            }, e.addToMap = function (t) {
                t.onAdd && t.onAdd(r), o.call(e, t);
            }, this._firstPaint = !0;
        }
        function r(t) {
            return function () {
                n('In IE8.0 VML mode painter not support method "' + t + '"');
            };
        }
        var n = t("../core/log"), a = t("./core");
        i.prototype = { constructor: i, getViewportRoot: function () {
                return this._vmlViewport;
            }, refresh: function () {
                var t = this.storage.getDisplayList(!0, !0);
                this._paintList(t);
            }, _paintList: function (t) {
                for (var e = this._vmlRoot, i = 0; i < t.length; i++) {
                    var r = t[i];
                    r.invisible || r.ignore ? (r.__alreadyNotVisible || r.onRemove(e), r.__alreadyNotVisible = !0) : (r.__alreadyNotVisible && r.onAdd(e), r.__alreadyNotVisible = !1, r.__dirty && (r.beforeBrush && r.beforeBrush(), (r.brushVML || r.brush).call(r, e), r.afterBrush && r.afterBrush())), r.__dirty = !1;
                }
                this._firstPaint && (this._vmlViewport.appendChild(e), this._firstPaint = !1);
            }, resize: function () {
                var t = this._getWidth(), e = this._getHeight();
                if (this._width != t && this._height != e) {
                    this._width = t, this._height = e;
                    var i = this._vmlViewport.style;
                    i.width = t + "px", i.height = e + "px";
                }
            }, dispose: function () {
                this.root.innerHTML = "", this._vmlRoot = this._vmlViewport = this.storage = null;
            }, getWidth: function () {
                return this._width;
            }, getHeight: function () {
                return this._height;
            }, clear: function () {
                this.root.removeChild(this.vmlViewport);
            }, _getWidth: function () {
                var t = this.root, i = t.currentStyle;
                return (t.clientWidth || e(i.width)) - e(i.paddingLeft) - e(i.paddingRight) | 0;
            }, _getHeight: function () {
                var t = this.root, i = t.currentStyle;
                return (t.clientHeight || e(i.height)) - e(i.paddingTop) - e(i.paddingBottom) | 0;
            } };
        for (var o = ["getLayer", "insertLayer", "eachLayer", "eachBuildinLayer", "eachOtherLayer", "getLayers", "modLayer", "delLayer", "clearLayer", "toDataURL", "pathToImage"], s = 0; s < o.length; s++) {
            var l = o[s];
            i.prototype[l] = r(l);
        }
        return i;
    }), e("zrender/vml/graphic", ["require", "../core/env", "../core/vector", "../core/BoundingRect", "../core/PathProxy", "../tool/color", "../contain/text", "../graphic/mixin/RectText", "../graphic/Displayable", "../graphic/Image", "../graphic/Text", "../graphic/Path", "../graphic/Gradient", "./core"], function (t) {
        if (!t("../core/env").canvasSupported) {
            var e = t("../core/vector"), i = t("../core/BoundingRect"), r = t("../core/PathProxy").CMD, n = t("../tool/color"), a = t("../contain/text"), o = t("../graphic/mixin/RectText"), s = t("../graphic/Displayable"), l = t("../graphic/Image"), h = t("../graphic/Text"), u = t("../graphic/Path"), c = t("../graphic/Gradient"), f = t("./core"), d = Math.round, p = Math.sqrt, g = Math.abs, v = Math.cos, m = Math.sin, y = Math.max, _ = e.applyTransform, x = ",", b = "progid:DXImageTransform.Microsoft", w = 21600, T = w / 2, M = 1e5, z = 1e3, S = function (t) {
                t.style.cssText = "position:absolute;left:0;top:0;width:1px;height:1px;", t.coordsize = w + "," + w, t.coordorigin = "0,0";
            }, C = function (t) {
                return String(t).replace(/&/g, "&amp;").replace(/"/g, "&quot;");
            }, P = function (t, e, i) {
                return "rgb(" + [t, e, i].join(",") + ")";
            }, A = function (t, e) {
                e && t && e.parentNode !== t && t.appendChild(e);
            }, k = function (t, e) {
                e && t && e.parentNode === t && t.removeChild(e);
            }, L = function (t, e, i) {
                return (parseFloat(t) || 0) * M + (parseFloat(e) || 0) * z + i;
            }, I = function (t, e) {
                return "string" == typeof t ? t.lastIndexOf("%") >= 0 ? parseFloat(t) / 100 * e : parseFloat(t) : t;
            }, D = function (t, e, i) {
                var r = n.parse(e);
                i = +i, isNaN(i) && (i = 1), r && (t.color = P(r[0], r[1], r[2]), t.opacity = i * r[3]);
            }, E = function (t) {
                var e = n.parse(t);
                return [P(e[0], e[1], e[2]), e[3]];
            }, O = function (t, e, i) {
                var r = e.fill;
                if (null != r)
                    if (r instanceof c) {
                        var n, a = 0, o = [0, 0], s = 0, l = 1, h = i.getBoundingRect(), u = h.width, f = h.height;
                        if ("linear" === r.type) {
                            n = "gradient";
                            var d = i.transform, p = [r.x * u, r.y * f], g = [r.x2 * u, r.y2 * f];
                            d && (_(p, p, d), _(g, g, d));
                            var v = g[0] - p[0], m = g[1] - p[1];
                            a = 180 * Math.atan2(v, m) / Math.PI, 0 > a && (a += 360), 1e-6 > a && (a = 0);
                        } else {
                            n = "gradientradial";
                            var p = [r.x * u, r.y * f], d = i.transform, x = i.scale, b = u, T = f;
                            o = [(p[0] - h.x) / b, (p[1] - h.y) / T], d && _(p, p, d), b /= x[0] * w, T /= x[1] * w;
                            var M = y(b, T);
                            s = 0 / M, l = 2 * r.r / M - s;
                        }
                        var z = r.colorStops.slice();
                        z.sort(function (t, e) {
                            return t.offset - e.offset;
                        });
                        for (var S = z.length, C = [], P = [], A = 0; S > A; A++) {
                            var k = z[A], L = E(k.color);
                            P.push(k.offset * l + s + " " + L[0]), (0 === A || A === S - 1) && C.push(L);
                        }
                        if (S >= 2) {
                            var I = C[0][0], O = C[1][0], R = C[0][1] * e.opacity, q = C[1][1] * e.opacity;
                            t.type = n, t.method = "none", t.focus = "100%", t.angle = a, t.color = I, t.color2 = O, t.colors = P.join(","), t.opacity = q, t.opacity2 = R;
                        }
                        "radial" === n && (t.focusposition = o.join(","));
                    } else
                        D(t, r, e.opacity);
            }, R = function (t, e) {
                null != e.lineDash && (t.dashstyle = e.lineDash.join(" ")), null == e.stroke || e.stroke instanceof c || D(t, e.stroke, e.opacity);
            }, q = function (t, e, i, r) {
                var n = "fill" == e, a = t.getElementsByTagName(e)[0];
                null != i[e] && "none" !== i[e] && (n || !n && i.lineWidth) ? (t[n ? "filled" : "stroked"] = "true", i[e] instanceof c && k(t, a), a || (a = f.createNode(e)), n ? O(a, i, r) : R(a, i), A(t, a)) : (t[n ? "filled" : "stroked"] = "false", k(t, a));
            }, B = [[], [], []], F = function (t, e) {
                var i, n, a, o, s, l, h = r.M, u = r.C, c = r.L, f = r.A, g = r.Q, y = [];
                for (o = 0; o < t.length;) {
                    switch (a = t[o++], n = "", i = 0, a) {
                        case h:
                            n = " m ", i = 1, s = t[o++], l = t[o++], B[0][0] = s, B[0][1] = l;
                            break;
                        case c:
                            n = " l ", i = 1, s = t[o++], l = t[o++], B[0][0] = s, B[0][1] = l;
                            break;
                        case g:
                        case u:
                            n = " c ", i = 3;
                            var b, M, z = t[o++], S = t[o++], C = t[o++], P = t[o++];
                            a === g ? (b = C, M = P, C = (C + 2 * z) / 3, P = (P + 2 * S) / 3, z = (s + 2 * z) / 3, S = (l + 2 * S) / 3) : (b = t[o++], M = t[o++]), B[0][0] = z, B[0][1] = S, B[1][0] = C, B[1][1] = P, B[2][0] = b, B[2][1] = M, s = b, l = M;
                            break;
                        case f:
                            var A = 0, k = 0, L = 1, I = 1, D = 0;
                            e && (A = e[4], k = e[5], L = p(e[0] * e[0] + e[1] * e[1]), I = p(e[2] * e[2] + e[3] * e[3]), D = Math.atan2(-e[1] / I, e[0] / L));
                            var E = t[o++], O = t[o++], R = t[o++], q = t[o++], F = t[o++] + D, N = t[o++] + F + D;
                            o++;
                            var V = t[o++], H = E + v(F) * R, G = O + m(F) * q, z = E + v(N) * R, S = O + m(N) * q, W = V ? " wa " : " at ";
                            Math.abs(H - z) < 1e-10 && (Math.abs(N - F) > .01 ? V && (H += 270 / w) : Math.abs(G - O) < 1e-10 ? V && E > H || !V && H > E ? S -= 270 / w : S += 270 / w : V && O > G || !V && G > O ? z += 270 / w : z -= 270 / w), y.push(W, d(((E - R) * L + A) * w - T), x, d(((O - q) * I + k) * w - T), x, d(((E + R) * L + A) * w - T), x, d(((O + q) * I + k) * w - T), x, d((H * L + A) * w - T), x, d((G * I + k) * w - T), x, d((z * L + A) * w - T), x, d((S * I + k) * w - T)), s = z, l = S;
                            break;
                        case r.R:
                            var j = B[0], U = B[1];
                            j[0] = t[o++], j[1] = t[o++], U[0] = j[0] + t[o++], U[1] = j[1] + t[o++], e && (_(j, j, e), _(U, U, e)), j[0] = d(j[0] * w - T), U[0] = d(U[0] * w - T), j[1] = d(j[1] * w - T), U[1] = d(U[1] * w - T), y.push(" m ", j[0], x, j[1], " l ", U[0], x, j[1], " l ", U[0], x, U[1], " l ", j[0], x, U[1]);
                            break;
                        case r.Z:
                            y.push(" x ");
                    }
                    if (i > 0) {
                        y.push(n);
                        for (var X = 0; i > X; X++) {
                            var Y = B[X];
                            e && _(Y, Y, e), y.push(d(Y[0] * w - T), x, d(Y[1] * w - T), i - 1 > X ? x : "");
                        }
                    }
                }
                return y.join("");
            };
            u.prototype.brushVML = function (t) {
                var e = this.style, i = this._vmlEl;
                i || (i = f.createNode("shape"), S(i), this._vmlEl = i), q(i, "fill", e, this), q(i, "stroke", e, this);
                var r = this.transform, n = null != r, a = i.getElementsByTagName("stroke")[0];
                if (a) {
                    var o = e.lineWidth;
                    if (n && !e.strokeNoScale) {
                        var s = r[0] * r[3] - r[1] * r[2];
                        o *= p(g(s));
                    }
                    a.weight = o + "px";
                }
                var l = this.path;
                this.__dirtyPath && (l.beginPath(), this.buildPath(l, this.shape), l.toStatic(), this.__dirtyPath = !1), i.path = F(l.data, this.transform), i.style.zIndex = L(this.zlevel, this.z, this.z2), A(t, i), e.text ? this.drawRectText(t, this.getBoundingRect()) : this.removeRectText(t);
            }, u.prototype.onRemove = function (t) {
                k(t, this._vmlEl), this.removeRectText(t);
            }, u.prototype.onAdd = function (t) {
                A(t, this._vmlEl), this.appendRectText(t);
            };
            var N = function (t) {
                return "object" == typeof t && t.tagName && "IMG" === t.tagName.toUpperCase();
            };
            l.prototype.brushVML = function (t) {
                var e, i, r = this.style, n = r.image;
                if (N(n)) {
                    var a = n.src;
                    if (a === this._imageSrc)
                        e = this._imageWidth, i = this._imageHeight;
                    else {
                        var o = n.runtimeStyle, s = o.width, l = o.height;
                        o.width = "auto", o.height = "auto", e = n.width, i = n.height, o.width = s, o.height = l, this._imageSrc = a, this._imageWidth = e, this._imageHeight = i;
                    }
                    n = a;
                } else
                    n === this._imageSrc && (e = this._imageWidth, i = this._imageHeight);
                if (n) {
                    var h = r.x || 0, u = r.y || 0, c = r.width, g = r.height, v = r.sWidth, m = r.sHeight, w = r.sx || 0, T = r.sy || 0, M = v && m, z = this._vmlEl;
                    z || (z = f.doc.createElement("div"), S(z), this._vmlEl = z);
                    var C, P = z.style, k = !1, I = 1, D = 1;
                    if (this.transform && (C = this.transform, I = p(C[0] * C[0] + C[1] * C[1]), D = p(C[2] * C[2] + C[3] * C[3]), k = C[1] || C[2]), k) {
                        var E = [h, u], O = [h + c, u], R = [h, u + g], q = [h + c, u + g];
                        _(E, E, C), _(O, O, C), _(R, R, C), _(q, q, C);
                        var B = y(E[0], O[0], R[0], q[0]), F = y(E[1], O[1], R[1], q[1]), V = [];
                        V.push("M11=", C[0] / I, x, "M12=", C[2] / D, x, "M21=", C[1] / I, x, "M22=", C[3] / D, x, "Dx=", d(h * I + C[4]), x, "Dy=", d(u * D + C[5])), P.padding = "0 " + d(B) + "px " + d(F) + "px 0", P.filter = b + ".Matrix(" + V.join("") + ", SizingMethod=clip)";
                    } else
                        C && (h = h * I + C[4], u = u * D + C[5]), P.filter = "", P.left = d(h) + "px", P.top = d(u) + "px";
                    var H = this._imageEl, G = this._cropEl;
                    H || (H = f.doc.createElement("div"), this._imageEl = H);
                    var W = H.style;
                    if (M) {
                        if (e && i)
                            W.width = d(I * e * c / v) + "px", W.height = d(D * i * g / m) + "px";
                        else {
                            var j = new Image, U = this;
                            j.onload = function () {
                                j.onload = null, e = j.width, i = j.height, W.width = d(I * e * c / v) + "px", W.height = d(D * i * g / m) + "px", U._imageWidth = e, U._imageHeight = i, U._imageSrc = n;
                            }, j.src = n;
                        }
                        G || (G = f.doc.createElement("div"), G.style.overflow = "hidden", this._cropEl = G);
                        var X = G.style;
                        X.width = d((c + w * c / v) * I), X.height = d((g + T * g / m) * D), X.filter = b + ".Matrix(Dx=" + -w * c / v * I + ",Dy=" + -T * g / m * D + ")", G.parentNode || z.appendChild(G), H.parentNode != G && G.appendChild(H);
                    } else
                        W.width = d(I * c) + "px", W.height = d(D * g) + "px", z.appendChild(H), G && G.parentNode && (z.removeChild(G), this._cropEl = null);
                    var Y = "", Z = r.opacity;
                    1 > Z && (Y += ".Alpha(opacity=" + d(100 * Z) + ") "), Y += b + ".AlphaImageLoader(src=" + n + ", SizingMethod=scale)", W.filter = Y, z.style.zIndex = L(this.zlevel, this.z, this.z2), A(t, z), r.text && this.drawRectText(t, this.getBoundingRect());
                }
            }, l.prototype.onRemove = function (t) {
                k(t, this._vmlEl), this._vmlEl = null, this._cropEl = null, this._imageEl = null, this.removeRectText(t);
            }, l.prototype.onAdd = function (t) {
                A(t, this._vmlEl), this.appendRectText(t);
            };
            var V, H = "normal", G = {}, W = 0, j = 100, U = document.createElement("div"), X = function (t) {
                var e = G[t];
                if (!e) {
                    W > j && (W = 0, G = {});
                    var i, r = U.style;
                    try  {
                        r.font = t, i = r.fontFamily.split(",")[0];
                    } catch (n) {
                    }
                    e = { style: r.fontStyle || H, variant: r.fontVariant || H, weight: r.fontWeight || H, size: 0 | parseFloat(r.fontSize || 12), family: i || "Microsoft YaHei" }, G[t] = e, W++;
                }
                return e;
            };
            a.measureText = function (t, e) {
                var i = f.doc;
                V || (V = i.createElement("div"), V.style.cssText = "position:absolute;top:-20000px;left:0;padding:0;margin:0;border:none;white-space:pre;", f.doc.body.appendChild(V));
                try  {
                    V.style.font = e;
                } catch (r) {
                }
                return V.innerHTML = "", V.appendChild(i.createTextNode(t)), { width: V.offsetWidth };
            };
            for (var Y = new i, Z = function (t, e, i, r) {
                var n = this.style, o = n.text;
                if (o) {
                    var s, l, h = n.textAlign, u = X(n.textFont), c = u.style + " " + u.variant + " " + u.weight + " " + u.size + 'px "' + u.family + '"', p = n.textBaseline, g = n.textVerticalAlign;
                    i = i || a.getBoundingRect(o, c, h, p);
                    var v = this.transform;
                    if (v && !r && (Y.copy(e), Y.applyTransform(v), e = Y), r)
                        s = e.x, l = e.y;
                    else {
                        var m = n.textPosition, y = n.textDistance;
                        if (m instanceof Array)
                            s = e.x + I(m[0], e.width), l = e.y + I(m[1], e.height), h = h || "left", p = p || "top";
                        else {
                            var b = a.adjustTextPositionOnRect(m, e, i, y);
                            s = b.x, l = b.y, h = h || b.textAlign, p = p || b.textBaseline;
                        }
                    }
                    if (g) {
                        switch (g) {
                            case "middle":
                                l -= i.height / 2;
                                break;
                            case "bottom":
                                l -= i.height;
                        }
                        p = "top";
                    }
                    var w = u.size;
                    switch (p) {
                        case "hanging":
                        case "top":
                            l += w / 1.75;
                            break;
                        case "middle":
                            break;
                        default:
                            l -= w / 2.25;
                    }
                    switch (h) {
                        case "left":
                            break;
                        case "center":
                            s -= i.width / 2;
                            break;
                        case "right":
                            s -= i.width;
                    }
                    var T, M, z, P = f.createNode, k = this._textVmlEl;
                    k ? (z = k.firstChild, T = z.nextSibling, M = T.nextSibling) : (k = P("line"), T = P("path"), M = P("textpath"), z = P("skew"), M.style["v-text-align"] = "left", S(k), T.textpathok = !0, M.on = !0, k.from = "0 0", k.to = "1000 0.05", A(k, z), A(k, T), A(k, M), this._textVmlEl = k);
                    var D = [s, l], E = k.style;
                    v && r ? (_(D, D, v), z.on = !0, z.matrix = v[0].toFixed(3) + x + v[2].toFixed(3) + x + v[1].toFixed(3) + x + v[3].toFixed(3) + ",0,0", z.offset = (d(D[0]) || 0) + "," + (d(D[1]) || 0), z.origin = "0 0", E.left = "0px", E.top = "0px") : (z.on = !1, E.left = d(s) + "px", E.top = d(l) + "px"), M.string = C(o);
                    try  {
                        M.style.font = c;
                    } catch (O) {
                    }
                    q(k, "fill", { fill: r ? n.fill : n.textFill, opacity: n.opacity }, this), q(k, "stroke", { stroke: r ? n.stroke : n.textStroke, opacity: n.opacity, lineDash: n.lineDash }, this), k.style.zIndex = L(this.zlevel, this.z, this.z2), A(t, k);
                }
            }, $ = function (t) {
                k(t, this._textVmlEl), this._textVmlEl = null;
            }, Q = function (t) {
                A(t, this._textVmlEl);
            }, K = [o, s, l, u, h], J = 0; J < K.length; J++) {
                var te = K[J].prototype;
                te.drawRectText = Z, te.removeRectText = $, te.appendRectText = Q;
            }
            h.prototype.brushVML = function (t) {
                var e = this.style;
                e.text ? this.drawRectText(t, { x: e.x || 0, y: e.y || 0, width: 0, height: 0 }, this.getBoundingRect(), !0) : this.removeRectText(t);
            }, h.prototype.onRemove = function (t) {
                this.removeRectText(t);
            }, h.prototype.onAdd = function (t) {
                this.appendRectText(t);
            };
        }
    }), e("echarts/util/number", ["require"], function () {
        function t(t) {
            return t.replace(/^\s+/, "").replace(/\s+$/, "");
        }
        var e = {}, i = 1e-4;
        return e.linearMap = function (t, e, i, r) {
            var n = e[1] - e[0], a = i[1] - i[0];
            if (0 === n)
                return 0 === a ? i[0] : (i[0] + i[1]) / 2;
            if (r)
                if (n > 0) {
                    if (t <= e[0])
                        return i[0];
                    if (t >= e[1])
                        return i[1];
                } else {
                    if (t >= e[0])
                        return i[0];
                    if (t <= e[1])
                        return i[1];
                }
            else {
                if (t === e[0])
                    return i[0];
                if (t === e[1])
                    return i[1];
            }
            return (t - e[0]) / n * a + i[0];
        }, e.parsePercent = function (e, i) {
            switch (e) {
                case "center":
                case "middle":
                    e = "50%";
                    break;
                case "left":
                case "top":
                    e = "0%";
                    break;
                case "right":
                case "bottom":
                    e = "100%";
            }
            return "string" == typeof e ? t(e).match(/%$/) ? parseFloat(e) / 100 * i : parseFloat(e) : null == e ? 0 / 0 : +e;
        }, e.round = function (t) {
            return +(+t).toFixed(10);
        }, e.asc = function (t) {
            return t.sort(function (t, e) {
                return t - e;
            }), t;
        }, e.getPrecision = function (t) {
            if (t = +t, isNaN(t))
                return 0;
            for (var e = 1, i = 0; Math.round(t * e) / e !== t;)
                e *= 10, i++;
            return i;
        }, e.getPixelPrecision = function (t, e) {
            var i = Math.log, r = Math.LN10, n = Math.floor(i(t[1] - t[0]) / r), a = Math.round(i(Math.abs(e[1] - e[0])) / r);
            return Math.max(-n + a, 0);
        }, e.MAX_SAFE_INTEGER = 9007199254740991, e.remRadian = function (t) {
            var e = 2 * Math.PI;
            return (t % e + e) % e;
        }, e.isRadianAroundZero = function (t) {
            return t > -i && i > t;
        }, e.parseDate = function (t) {
            return t instanceof Date ? t : new Date("string" == typeof t ? new Date(t.replace(/-/g, "/")) - new Date("1970/01/01") : Math.round(t));
        }, e.quantity = function (t) {
            return Math.pow(10, Math.floor(Math.log(t) / Math.LN10));
        }, e.nice = function (t, i) {
            var r, n = e.quantity(t), a = t / n;
            return r = i ? 1.5 > a ? 1 : 2.5 > a ? 2 : 4 > a ? 3 : 7 > a ? 5 : 10 : 1 > a ? 1 : 2 > a ? 2 : 3 > a ? 3 : 5 > a ? 5 : 10, r * n;
        }, e;
    }), e("echarts/util/format", ["require", "zrender/core/util", "./number", "zrender/contain/text"], function (t) {
        function e(t) {
            return isNaN(t) ? "-" : (t = (t + "").split("."), t[0].replace(/(\d{1,3})(?=(?:\d{3})+(?!\d))/g, "$1,") + (t.length > 1 ? "." + t[1] : ""));
        }
        function i(t) {
            return t.toLowerCase().replace(/-(.)/g, function (t, e) {
                return e.toUpperCase();
            });
        }
        function r(t) {
            var e = t.length;
            return "number" == typeof t ? [t, t, t, t] : 2 === e ? [t[0], t[1], t[0], t[1]] : 3 === e ? [t[0], t[1], t[2], t[1]] : t;
        }
        function n(t) {
            return String(t).replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#39;");
        }
        function a(t, e) {
            return "{" + t + (null == e ? "" : e) + "}";
        }
        function o(t, e) {
            h.isArray(e) || (e = [e]);
            var i = e.length;
            if (!i)
                return "";
            for (var r = e[0].$vars || [], n = 0; n < r.length; n++) {
                var o = f[n];
                t = t.replace(a(o), a(o, 0));
            }
            for (var s = 0; i > s; s++)
                for (var l = 0; l < r.length; l++)
                    t = t.replace(a(f[l], s), e[s][r[l]]);
            return t;
        }
        function s(t, e) {
            ("week" === t || "month" === t || "quarter" === t || "half-year" === t || "year" === t) && (t = "MM-dd\nyyyy");
            var i = u.parseDate(e), r = i.getFullYear(), n = i.getMonth() + 1, a = i.getDate(), o = i.getHours(), s = i.getMinutes(), h = i.getSeconds();
            return t = t.replace("MM", l(n)).toLowerCase().replace("yyyy", r).replace("yy", r % 100).replace("dd", l(a)).replace("d", a).replace("hh", l(o)).replace("h", o).replace("mm", l(s)).replace("m", s).replace("ss", l(h)).replace("s", h);
        }
        function l(t) {
            return 10 > t ? "0" + t : t;
        }
        var h = t("zrender/core/util"), u = t("./number"), c = t("zrender/contain/text"), f = ["a", "b", "c", "d", "e", "f", "g"];
        return { normalizeCssArray: r, addCommas: e, toCamelCase: i, encodeHTML: n, formatTpl: o, formatTime: s, truncateText: c.truncateText };
    }), e("echarts/scale/Interval", ["require", "../util/number", "../util/format", "./Scale"], function (t) {
        var e = t("../util/number"), i = t("../util/format"), r = t("./Scale"), n = Math.floor, a = Math.ceil, o = r.extend({ type: "interval", _interval: 0, setExtent: function (t, e) {
                var i = this._extent;
                isNaN(t) || (i[0] = parseFloat(t)), isNaN(e) || (i[1] = parseFloat(e));
            }, unionExtent: function (t) {
                var e = this._extent;
                t[0] < e[0] && (e[0] = t[0]), t[1] > e[1] && (e[1] = t[1]), o.prototype.setExtent.call(this, e[0], e[1]);
            }, getInterval: function () {
                return this._interval || this.niceTicks(), this._interval;
            }, setInterval: function (t) {
                this._interval = t, this._niceExtent = this._extent.slice();
            }, getTicks: function () {
                this._interval || this.niceTicks();
                var t = this._interval, i = this._extent, r = [], n = 1e4;
                if (t) {
                    var a = this._niceExtent;
                    i[0] < a[0] && r.push(i[0]);
                    for (var o = a[0]; o <= a[1];)
                        if (r.push(o), o = e.round(o + t), r.length > n)
                            return [];
                    i[1] > a[1] && r.push(i[1]);
                }
                return r;
            }, getTicksLabels: function () {
                for (var t = [], e = this.getTicks(), i = 0; i < e.length; i++)
                    t.push(this.getLabel(e[i]));
                return t;
            }, getLabel: function (t) {
                return i.addCommas(t);
            }, niceTicks: function (t) {
                t = t || 5;
                var i = this._extent, r = i[1] - i[0];
                if (isFinite(r)) {
                    0 > r && (r = -r, i.reverse());
                    var o = e.nice(r / t, !0), s = [e.round(a(i[0] / o) * o), e.round(n(i[1] / o) * o)];
                    this._interval = o, this._niceExtent = s;
                }
            }, niceExtent: function (t, i, r) {
                var o = this._extent;
                if (o[0] === o[1])
                    if (0 !== o[0]) {
                        var s = o[0];
                        r ? o[0] -= s / 2 : (o[1] += s / 2, o[0] -= s / 2);
                    } else
                        o[1] = 1;
                var l = o[1] - o[0];
                isFinite(l) || (o[0] = 0, o[1] = 1), this.niceTicks(t);
                var h = this._interval;
                i || (o[0] = e.round(n(o[0] / h) * h)), r || (o[1] = e.round(a(o[1] / h) * h));
            } });
        return o.create = function () {
            return new o;
        }, o;
    }), e("zrender/core/env", [], function () {
        function t(t) {
            var e = {}, i = {}, r = t.match(/Firefox\/([\d.]+)/), n = t.match(/MSIE\s([\d.]+)/) || t.match(/Trident\/.+?rv:(([\d.]+))/), a = t.match(/Edge\/([\d.]+)/);
            return r && (i.firefox = !0, i.version = r[1]), n && (i.ie = !0, i.version = n[1]), n && (i.ie = !0, i.version = n[1]), a && (i.edge = !0, i.version = a[1]), { browser: i, os: e, node: !1, canvasSupported: document.createElement("canvas").getContext ? !0 : !1, touchEventsSupported: "ontouchstart" in window && !i.ie && !i.edge, pointerEventsSupported: "onpointerdown" in window && (i.edge || i.ie && i.version >= 10) };
        }
        var e = {};
        return e = "undefined" == typeof navigator ? { browser: {}, os: {}, node: !0, canvasSupported: !0 } : t(navigator.userAgent);
    }), e("echarts/model/Global", ["require", "zrender/core/util", "../util/model", "./Model", "./Component", "./globalDefault", "./mixin/colorPalette"], function (t) {
        function e(t, e) {
            for (var i in e)
                y.hasClass(i) || ("object" == typeof e[i] ? t[i] = t[i] ? h.merge(t[i], e[i], !1) : h.clone(e[i]) : null == t[i] && (t[i] = e[i]));
        }
        function i(t) {
            t = t, this.option = {}, this.option[x] = 1, this._componentsMap = {}, this._seriesIndices = null, e(t, this._theme.option), h.merge(t, _, !1), this.mergeOption(t);
        }
        function r(t, e) {
            h.isArray(e) || (e = e ? [e] : []);
            var i = {};
            return f(e, function (e) {
                i[e] = (t[e] || []).slice();
            }), i;
        }
        function n(t, e) {
            var i = {};
            f(e, function (t) {
                var e = t.exist;
                e && (i[e.id] = t);
            }), f(e, function (e) {
                var r = e.option;
                if (h.assert(!r || null == r.id || !i[r.id] || i[r.id] === e, "id duplicates: " + (r && r.id)), r && null != r.id && (i[r.id] = e), m(r)) {
                    var n = a(t, r, e.exist);
                    e.keyInfo = { mainType: t, subType: n };
                }
            }), f(e, function (t) {
                var e = t.exist, r = t.option, n = t.keyInfo;
                if (m(r)) {
                    if (n.name = null != r.name ? r.name + "" : e ? e.name : " -", e)
                        n.id = e.id;
                    else if (null != r.id)
                        n.id = r.id + "";
                    else {
                        var a = 0;
                        do
                            n.id = " " + n.name + " " + a++; while(i[n.id]);
                    }
                    i[n.id] = t;
                }
            });
        }
        function a(t, e, i) {
            var r = e.type ? e.type : i ? i.subType : y.determineSubType(t, e);
            return r;
        }
        function o(t) {
            return p(t, function (t) {
                return t.componentIndex;
            }) || [];
        }
        function s(t, e) {
            return e.hasOwnProperty("subType") ? d(t, function (t) {
                return t.subType === e.subType;
            }) : t;
        }
        function l(t) {
            if (!t._seriesIndices)
                throw new Error("Series has not been initialized yet.");
        }
        var h = t("zrender/core/util"), u = t("../util/model"), c = t("./Model"), f = h.each, d = h.filter, p = h.map, g = h.isArray, v = h.indexOf, m = h.isObject, y = t("./Component"), _ = t("./globalDefault"), x = " _ec_inner", b = c.extend({ constructor: b, init: function (t, e, i, r) {
                i = i || {}, this.option = null, this._theme = new c(i), this._optionManager = r;
            }, setOption: function (t, e) {
                h.assert(!(x in t), "please use chart.getOption()"), this._optionManager.setOption(t, e), this.resetOption();
            }, resetOption: function (t) {
                var e = !1, r = this._optionManager;
                if (!t || "recreate" === t) {
                    var n = r.mountOption("recreate" === t);
                    this.option && "recreate" !== t ? (this.restoreData(), this.mergeOption(n)) : i.call(this, n), e = !0;
                }
                if (("timeline" === t || "media" === t) && this.restoreData(), !t || "recreate" === t || "timeline" === t) {
                    var a = r.getTimelineOption(this);
                    a && (this.mergeOption(a), e = !0);
                }
                if (!t || "recreate" === t || "media" === t) {
                    var o = r.getMediaOption(this, this._api);
                    o.length && f(o, function (t) {
                        this.mergeOption(t, e = !0);
                    }, this);
                }
                return e;
            }, mergeOption: function (t) {
                function e(e, s) {
                    var l = u.normalizeToArray(t[e]), c = u.mappingToExists(a[e], l);
                    n(e, c);
                    var d = r(a, s);
                    i[e] = [], a[e] = [], f(c, function (t, r) {
                        var n = t.exist, o = t.option;
                        if (h.assert(m(o) || n, "Empty component definition"), o) {
                            var s = y.getClass(e, t.keyInfo.subType, !0);
                            if (n && n instanceof s)
                                n.mergeOption(o, this), n.optionUpdated(o, !1);
                            else {
                                var l = h.extend({ dependentModels: d, componentIndex: r }, t.keyInfo);
                                n = new s(o, this, this, l), n.init(o, this, this, l), n.optionUpdated(null, !0);
                            }
                        } else
                            n.mergeOption({}, this), n.optionUpdated({}, !1);
                        a[e][r] = n, i[e][r] = n.option;
                    }, this), "series" === e && (this._seriesIndices = o(a.series));
                }
                var i = this.option, a = this._componentsMap, s = [];
                f(t, function (t, e) {
                    null != t && (y.hasClass(e) ? s.push(e) : i[e] = null == i[e] ? h.clone(t) : h.merge(i[e], t, !0));
                }), y.topologicalTravel(s, y.getAllClassMainTypes(), e, this);
            }, getOption: function () {
                var t = h.clone(this.option);
                return f(t, function (e, i) {
                    if (y.hasClass(i)) {
                        for (var e = u.normalizeToArray(e), r = e.length - 1; r >= 0; r--)
                            u.isIdInner(e[r]) && e.splice(r, 1);
                        t[i] = e;
                    }
                }), delete t[x], t;
            }, getTheme: function () {
                return this._theme;
            }, getComponent: function (t, e) {
                var i = this._componentsMap[t];
                return i ? i[e || 0] : void 0;
            }, queryComponents: function (t) {
                var e = t.mainType;
                if (!e)
                    return [];
                var i = t.index, r = t.id, n = t.name, a = this._componentsMap[e];
                if (!a || !a.length)
                    return [];
                var o;
                if (null != i)
                    g(i) || (i = [i]), o = d(p(i, function (t) {
                        return a[t];
                    }), function (t) {
                        return !!t;
                    });
                else if (null != r) {
                    var l = g(r);
                    o = d(a, function (t) {
                        return l && v(r, t.id) >= 0 || !l && t.id === r;
                    });
                } else if (null != n) {
                    var h = g(n);
                    o = d(a, function (t) {
                        return h && v(n, t.name) >= 0 || !h && t.name === n;
                    });
                }
                return s(o, t);
            }, findComponents: function (t) {
                function e(t) {
                    var e = n + "Index", i = n + "Id", r = n + "Name";
                    return t && (t.hasOwnProperty(e) || t.hasOwnProperty(i) || t.hasOwnProperty(r)) ? { mainType: n, index: t[e], id: t[i], name: t[r] } : null;
                }
                function i(e) {
                    return t.filter ? d(e, t.filter) : e;
                }
                var r = t.query, n = t.mainType, a = e(r), o = a ? this.queryComponents(a) : this._componentsMap[n];
                return i(s(o, t));
            }, eachComponent: function (t, e, i) {
                var r = this._componentsMap;
                if ("function" == typeof t)
                    i = e, e = t, f(r, function (t, r) {
                        f(t, function (t, n) {
                            e.call(i, r, t, n);
                        });
                    });
                else if (h.isString(t))
                    f(r[t], e, i);
                else if (m(t)) {
                    var n = this.findComponents(t);
                    f(n, e, i);
                }
            }, getSeriesByName: function (t) {
                var e = this._componentsMap.series;
                return d(e, function (e) {
                    return e.name === t;
                });
            }, getSeriesByIndex: function (t) {
                return this._componentsMap.series[t];
            }, getSeriesByType: function (t) {
                var e = this._componentsMap.series;
                return d(e, function (e) {
                    return e.subType === t;
                });
            }, getSeries: function () {
                return this._componentsMap.series.slice();
            }, eachSeries: function (t, e) {
                l(this), f(this._seriesIndices, function (i) {
                    var r = this._componentsMap.series[i];
                    t.call(e, r, i);
                }, this);
            }, eachRawSeries: function (t, e) {
                f(this._componentsMap.series, t, e);
            }, eachSeriesByType: function (t, e, i) {
                l(this), f(this._seriesIndices, function (r) {
                    var n = this._componentsMap.series[r];
                    n.subType === t && e.call(i, n, r);
                }, this);
            }, eachRawSeriesByType: function (t, e, i) {
                return f(this.getSeriesByType(t), e, i);
            }, isSeriesFiltered: function (t) {
                return l(this), h.indexOf(this._seriesIndices, t.componentIndex) < 0;
            }, filterSeries: function (t, e) {
                l(this);
                var i = d(this._componentsMap.series, t, e);
                this._seriesIndices = o(i);
            }, restoreData: function () {
                var t = this._componentsMap;
                this._seriesIndices = o(t.series);
                var e = [];
                f(t, function (t, i) {
                    e.push(i);
                }), y.topologicalTravel(e, y.getAllClassMainTypes(), function (e) {
                    f(t[e], function (t) {
                        t.restoreData();
                    });
                });
            } });
        return h.mixin(b, t("./mixin/colorPalette")), b;
    }), e("echarts/CoordinateSystem", ["require"], function () {
        "use strict";
        function t() {
            this._coordinateSystems = [];
        }
        var e = {};
        return t.prototype = { constructor: t, create: function (t, i) {
                var r = [];
                for (var n in e) {
                    var a = e[n].create(t, i);
                    a && (r = r.concat(a));
                }
                this._coordinateSystems = r;
            }, update: function (t, e) {
                for (var i = this._coordinateSystems, r = 0; r < i.length; r++)
                    i[r].update && i[r].update(t, e);
            } }, t.register = function (t, i) {
            e[t] = i;
        }, t.get = function (t) {
            return e[t];
        }, t;
    }), e("echarts/ExtensionAPI", ["require", "zrender/core/util"], function (t) {
        "use strict";
        function e(t) {
            i.each(r, function (e) {
                this[e] = i.bind(t[e], t);
            }, this);
        }
        var i = t("zrender/core/util"), r = ["getDom", "getZr", "getWidth", "getHeight", "dispatchAction", "isDisposed", "on", "off", "getDataURL", "getConnectedDataURL", "getModel", "getOption"];
        return e;
    }), e("echarts/model/OptionManager", ["require", "zrender/core/util", "../util/model", "./Component"], function (t) {
        function e(t) {
            this._api = t, this._timelineOptions = [], this._mediaList = [], this._mediaDefault, this._currentMediaIndices = [], this._optionBackup, this._newBaseOption;
        }
        function i(t, e, i) {
            var r, n, a = [], o = [], l = t.timeline;
            if (t.baseOption && (n = t.baseOption), (l || t.options) && (n = n || {}, a = (t.options || []).slice()), t.media) {
                n = n || {};
                var h = t.media;
                u(h, function (t) {
                    t && t.option && (t.query ? o.push(t) : r || (r = t));
                });
            }
            return n || (n = t), n.timeline || (n.timeline = l), u([n].concat(a).concat(s.map(o, function (t) {
                return t.option;
            })), function (t) {
                u(e, function (e) {
                    e(t, i);
                });
            }), { baseOption: n, timelineOptions: a, mediaDefault: r, mediaList: o };
        }
        function r(t, e, i) {
            var r = { width: e, height: i, aspectratio: e / i }, a = !0;
            return s.each(t, function (t, e) {
                var i = e.match(p);
                if (i && i[1] && i[2]) {
                    var o = i[1], s = i[2].toLowerCase();
                    n(r[s], t, o) || (a = !1);
                }
            }), a;
        }
        function n(t, e, i) {
            return "min" === i ? t >= e : "max" === i ? e >= t : t === e;
        }
        function a(t, e) {
            return t.join(",") === e.join(",");
        }
        function o(t, e) {
            e = e || {}, u(e, function (e, i) {
                if (null != e) {
                    var r = t[i];
                    if (h.hasClass(i)) {
                        e = l.normalizeToArray(e), r = l.normalizeToArray(r);
                        var n = l.mappingToExists(r, e);
                        t[i] = f(n, function (t) {
                            return t.option && t.exist ? d(t.exist, t.option, !0) : t.exist || t.option;
                        });
                    } else
                        t[i] = d(r, e, !0);
                }
            });
        }
        var s = t("zrender/core/util"), l = t("../util/model"), h = t("./Component"), u = s.each, c = s.clone, f = s.map, d = s.merge, p = /^(min|max)?(.+)$/;
        return e.prototype = { constructor: e, setOption: function (t, e) {
                t = c(t, !0);
                var r = this._optionBackup, n = i.call(this, t, e, !r);
                this._newBaseOption = n.baseOption, r ? (o(r.baseOption, n.baseOption), n.timelineOptions.length && (r.timelineOptions = n.timelineOptions), n.mediaList.length && (r.mediaList = n.mediaList), n.mediaDefault && (r.mediaDefault = n.mediaDefault)) : this._optionBackup = n;
            }, mountOption: function (t) {
                var e = this._optionBackup;
                return this._timelineOptions = f(e.timelineOptions, c), this._mediaList = f(e.mediaList, c), this._mediaDefault = c(e.mediaDefault), this._currentMediaIndices = [], c(t ? e.baseOption : this._newBaseOption);
            }, getTimelineOption: function (t) {
                var e, i = this._timelineOptions;
                if (i.length) {
                    var r = t.getComponent("timeline");
                    r && (e = c(i[r.getCurrentIndex()], !0));
                }
                return e;
            }, getMediaOption: function () {
                var t = this._api.getWidth(), e = this._api.getHeight(), i = this._mediaList, n = this._mediaDefault, o = [], s = [];
                if (!i.length && !n)
                    return s;
                for (var l = 0, h = i.length; h > l; l++)
                    r(i[l].query, t, e) && o.push(l);
                return !o.length && n && (o = [-1]), o.length && !a(o, this._currentMediaIndices) && (s = f(o, function (t) {
                    return c(-1 === t ? n.option : i[t].option);
                })), this._currentMediaIndices = o, s;
            } }, e;
    }), e("echarts/model/Component", ["require", "./Model", "zrender/core/util", "../util/component", "../util/clazz", "../util/layout", "./mixin/boxLayout"], function (t) {
        function e(t) {
            var e = [];
            return r.each(l.getClassesByMainType(t), function (t) {
                n.apply(e, t.prototype.dependencies || []);
            }), r.map(e, function (t) {
                return o.parseClassType(t).main;
            });
        }
        var i = t("./Model"), r = t("zrender/core/util"), n = Array.prototype.push, a = t("../util/component"), o = t("../util/clazz"), s = t("../util/layout"), l = i.extend({ type: "component", id: "", name: "", mainType: "", subType: "", componentIndex: 0, defaultOption: null, ecModel: null, dependentModels: [], uid: null, layoutMode: null, $constructor: function (t, e, n, o) {
                i.call(this, t, e, n, o), r.extend(this, o), this.uid = a.getUID("componentModel");
            }, init: function (t, e, i) {
                this.mergeDefaultAndTheme(t, i);
            }, mergeDefaultAndTheme: function (t, e) {
                var i = this.layoutMode, n = i ? s.getLayoutParams(t) : {}, a = e.getTheme();
                r.merge(t, a.get(this.mainType)), r.merge(t, this.getDefaultOption()), i && s.mergeLayoutParam(t, n, i);
            }, mergeOption: function (t) {
                r.merge(this.option, t, !0);
                var e = this.layoutMode;
                e && s.mergeLayoutParam(this.option, t, e);
            }, optionUpdated: function () {
            }, getDefaultOption: function () {
                if (!this.hasOwnProperty("__defaultOption")) {
                    for (var t = [], e = this.constructor; e;) {
                        var i = e.prototype.defaultOption;
                        i && t.push(i), e = e.superClass;
                    }
                    for (var n = {}, a = t.length - 1; a >= 0; a--)
                        n = r.merge(n, t[a], !0);
                    this.__defaultOption = n;
                }
                return this.__defaultOption;
            } });
        return o.enableClassManagement(l, { registerWhenExtend: !0 }), a.enableSubTypeDefaulter(l), a.enableTopologicalTravel(l, e), r.mixin(l, t("./mixin/boxLayout")), l;
    }), e("echarts/model/Series", ["require", "zrender/core/util", "../util/format", "../util/model", "./Component", "./mixin/colorPalette", "zrender/core/env"], function (t) {
        "use strict";
        var e = t("zrender/core/util"), i = t("../util/format"), r = t("../util/model"), n = t("./Component"), a = t("./mixin/colorPalette"), o = t("zrender/core/env"), s = i.encodeHTML, l = i.addCommas, h = n.extend({
            type: "series.__base__", seriesIndex: 0, coordinateSystem: null, defaultOption: null, legendDataProvider: null, visualColorAccessPath: "itemStyle.normal.color", init: function (t, e, i) {
                this.seriesIndex = this.componentIndex, this.mergeDefaultAndTheme(t, i), this._dataBeforeProcessed = this.getInitialData(t, i), this._data = this._dataBeforeProcessed.cloneShallow();
            }, mergeDefaultAndTheme: function (t, i) {
                e.merge(t, i.getTheme().get(this.subType)), e.merge(t, this.getDefaultOption()), r.defaultEmphasis(t.label, r.LABEL_OPTIONS), this.fillDataTextStyle(t.data);
            }, mergeOption: function (t, i) {
                t = e.merge(this.option, t, !0), this.fillDataTextStyle(t.data);
                var r = this.getInitialData(t, i);
                r && (this._data = r, this._dataBeforeProcessed = r.cloneShallow());
            }, fillDataTextStyle: function (t) {
                if (t)
                    for (var e = 0; e < t.length; e++)
                        t[e] && t[e].label && r.defaultEmphasis(t[e].label, r.LABEL_OPTIONS);
            }, getInitialData: function () {
            }, getData: function (t) {
                return null == t ? this._data : this._data.getLinkedData(t);
            }, setData: function (t) {
                this._data = t;
            }, getRawData: function () {
                return this._dataBeforeProcessed;
            }, coordDimToDataDim: function (t) {
                return [t];
            }, dataDimToCoordDim: function (t) {
                return t;
            }, getBaseAxis: function () {
                var t = this.coordinateSystem;
                return t && t.getBaseAxis && t.getBaseAxis();
            }, formatTooltip: function (t, r) {
                function n(t) {
                    return e.map(t, function (t, e) {
                        var n = a.getDimensionInfo(e), o = n && n.type;
                        return "ordinal" === o ? t : "time" === o ? r ? "" : i.formatTime("yyyy/mm/dd hh:mm:ss", t) : l(t);
                    }).filter(function (t) {
                        return !!t;
                    }).join(", ");
                }
                var a = this._data, o = this.getRawValue(t), h = e.isArray(o) ? n(o) : l(o), u = a.getName(t), c = a.getItemVisual(t, "color"), f = '<span style="display:inline-block;margin-right:5px;border-radius:10px;width:9px;height:9px;background-color:' + c + '"></span>', d = this.name;
                return " -" === d && (d = ""), r ? f + s(this.name) + " : " + h : (d && s(d) + "<br />") + f + (u ? s(u) + " : " + h : h);
            }, ifEnableAnimation: function () {
                if (o.node)
                    return !1;
                var t = this.getShallow("animation");
                return t && this.getData().count() > this.getShallow("animationThreshold") && (t = !1), t;
            }, restoreData: function () {
                this._data = this._dataBeforeProcessed.cloneShallow();
            }, getColorFromPalette: function (t, e) {
                var i = this.ecModel, r = a.getColorFromPalette.call(this, t, e);
                return r || (r = i.getColorFromPalette(t, e)), r;
            }, getAxisTooltipDataIndex: null });
        return e.mixin(h, r.dataFormatMixin), e.mixin(h, a), h;
    }), e("echarts/view/Component", ["require", "zrender/container/Group", "../util/component", "../util/clazz"], function (t) {
        var e = t("zrender/container/Group"), i = t("../util/component"), r = t("../util/clazz"), n = function () {
            this.group = new e, this.uid = i.getUID("viewComponent");
        };
        n.prototype = { constructor: n, init: function () {
            }, render: function () {
            }, dispose: function () {
            } };
        var a = n.prototype;
        return a.updateView = a.updateLayout = a.updateVisual = function () {
        }, r.enableClassExtend(n), r.enableClassManagement(n, { registerWhenExtend: !0 }), n;
    }), e("echarts/view/Chart", ["require", "zrender/container/Group", "../util/component", "../util/clazz"], function (t) {
        function e() {
            this.group = new n, this.uid = a.getUID("viewChart");
        }
        function i(t, e) {
            if (t && (t.trigger(e), "group" === t.type))
                for (var r = 0; r < t.childCount(); r++)
                    i(t.childAt(r), e);
        }
        function r(t, e, r) {
            var n = e && e.dataIndex, a = e && e.name;
            if (null != n)
                for (var o = n instanceof Array ? n : [n], s = 0, l = o.length; l > s; s++)
                    i(t.getItemGraphicEl(o[s]), r);
            else if (a)
                for (var h = a instanceof Array ? a : [a], s = 0, l = h.length; l > s; s++) {
                    var n = t.indexOfName(h[s]);
                    i(t.getItemGraphicEl(n), r);
                }
            else
                t.eachItemGraphicEl(function (t) {
                    i(t, r);
                });
        }
        var n = t("zrender/container/Group"), a = t("../util/component"), o = t("../util/clazz");
        e.prototype = { type: "chart", init: function () {
            }, render: function () {
            }, highlight: function (t, e, i, n) {
                r(t.getData(), n, "emphasis");
            }, downplay: function (t, e, i, n) {
                r(t.getData(), n, "normal");
            }, remove: function () {
                this.group.removeAll();
            }, dispose: function () {
            } };
        var s = e.prototype;
        return s.updateView = s.updateLayout = s.updateVisual = function (t, e, i, r) {
            this.render(t, e, i, r);
        }, o.enableClassExtend(e), o.enableClassManagement(e, { registerWhenExtend: !0 }), e;
    }), e("echarts/util/graphic", ["require", "zrender/core/util", "zrender/tool/path", "zrender/graphic/Path", "zrender/tool/color", "zrender/core/matrix", "zrender/core/vector", "zrender/graphic/Gradient", "zrender/container/Group", "zrender/graphic/Image", "zrender/graphic/Text", "zrender/graphic/shape/Circle", "zrender/graphic/shape/Sector", "zrender/graphic/shape/Ring", "zrender/graphic/shape/Polygon", "zrender/graphic/shape/Polyline", "zrender/graphic/shape/Rect", "zrender/graphic/shape/Line", "zrender/graphic/shape/BezierCurve", "zrender/graphic/shape/Arc", "zrender/graphic/CompoundPath", "zrender/graphic/LinearGradient", "zrender/graphic/RadialGradient", "zrender/core/BoundingRect"], function (t) {
        "use strict";
        function e(t) {
            return null != t && "none" != t;
        }
        function i(t) {
            return "string" == typeof t ? y.lift(t, -.1) : t;
        }
        function r(t) {
            if (t.__hoverStlDirty) {
                var r = t.style.stroke, n = t.style.fill, a = t.__hoverStl;
                a.fill = a.fill || (e(n) ? i(n) : null), a.stroke = a.stroke || (e(r) ? i(r) : null);
                var o = {};
                for (var s in a)
                    a.hasOwnProperty(s) && (o[s] = t.style[s]);
                t.__normalStl = o, t.__hoverStlDirty = !1;
            }
        }
        function n(t) {
            t.__isHover || (r(t), t.useHoverLayer ? t.__zr && t.__zr.addHover(t, t.__hoverStl) : (t.setStyle(t.__hoverStl), t.z2 += 1), t.__isHover = !0);
        }
        function a(t) {
            if (t.__isHover) {
                var e = t.__normalStl;
                t.useHoverLayer ? t.__zr && t.__zr.removeHover(t) : (e && t.setStyle(e), t.z2 -= 1), t.__isHover = !1;
            }
        }
        function o(t) {
            "group" === t.type ? t.traverse(function (t) {
                "group" !== t.type && n(t);
            }) : n(t);
        }
        function s(t) {
            "group" === t.type ? t.traverse(function (t) {
                "group" !== t.type && a(t);
            }) : a(t);
        }
        function l(t, e) {
            t.__hoverStl = t.hoverStyle || e || {}, t.__hoverStlDirty = !0, t.__isHover && r(t);
        }
        function h() {
            !this.__isEmphasis && o(this);
        }
        function u() {
            !this.__isEmphasis && s(this);
        }
        function c() {
            this.__isEmphasis = !0, o(this);
        }
        function f() {
            this.__isEmphasis = !1, s(this);
        }
        function d(t, e, i, r, n, a) {
            "function" == typeof n && (a = n, n = null);
            var o = r && (r.ifEnableAnimation ? r.ifEnableAnimation() : r.getShallow("animation"));
            if (o) {
                var s = t ? "Update" : "", l = r && r.getShallow("animationDuration" + s), h = r && r.getShallow("animationEasing" + s), u = r && r.getShallow("animationDelay" + s);
                "function" == typeof u && (u = u(n)), l > 0 ? e.animateTo(i, l, u || 0, h, a) : (e.attr(i), a && a());
            } else
                e.attr(i), a && a();
        }
        var p = t("zrender/core/util"), g = t("zrender/tool/path"), v = Math.round, m = t("zrender/graphic/Path"), y = t("zrender/tool/color"), _ = t("zrender/core/matrix"), x = t("zrender/core/vector"), b = (t("zrender/graphic/Gradient"), {});
        return b.Group = t("zrender/container/Group"), b.Image = t("zrender/graphic/Image"), b.Text = t("zrender/graphic/Text"), b.Circle = t("zrender/graphic/shape/Circle"), b.Sector = t("zrender/graphic/shape/Sector"), b.Ring = t("zrender/graphic/shape/Ring"), b.Polygon = t("zrender/graphic/shape/Polygon"), b.Polyline = t("zrender/graphic/shape/Polyline"), b.Rect = t("zrender/graphic/shape/Rect"), b.Line = t("zrender/graphic/shape/Line"), b.BezierCurve = t("zrender/graphic/shape/BezierCurve"), b.Arc = t("zrender/graphic/shape/Arc"), b.CompoundPath = t("zrender/graphic/CompoundPath"), b.LinearGradient = t("zrender/graphic/LinearGradient"), b.RadialGradient = t("zrender/graphic/RadialGradient"), b.BoundingRect = t("zrender/core/BoundingRect"), b.extendShape = function (t) {
            return m.extend(t);
        }, b.extendPath = function (t, e) {
            return g.extendFromString(t, e);
        }, b.makePath = function (t, e, i, r) {
            var n = g.createFromString(t, e), a = n.getBoundingRect();
            if (i) {
                var o = a.width / a.height;
                if ("center" === r) {
                    var s, l = i.height * o;
                    l <= i.width ? s = i.height : (l = i.width, s = l / o);
                    var h = i.x + i.width / 2, u = i.y + i.height / 2;
                    i.x = h - l / 2, i.y = u - s / 2, i.width = l, i.height = s;
                }
                this.resizePath(n, i);
            }
            return n;
        }, b.mergePath = g.mergePath, b.resizePath = function (t, e) {
            if (t.applyTransform) {
                var i = t.getBoundingRect(), r = i.calculateTransform(e);
                t.applyTransform(r);
            }
        }, b.subPixelOptimizeLine = function (t) {
            var e = b.subPixelOptimize, i = t.shape, r = t.style.lineWidth;
            return v(2 * i.x1) === v(2 * i.x2) && (i.x1 = i.x2 = e(i.x1, r, !0)), v(2 * i.y1) === v(2 * i.y2) && (i.y1 = i.y2 = e(i.y1, r, !0)), t;
        }, b.subPixelOptimizeRect = function (t) {
            var e = b.subPixelOptimize, i = t.shape, r = t.style.lineWidth, n = i.x, a = i.y, o = i.width, s = i.height;
            return i.x = e(i.x, r, !0), i.y = e(i.y, r, !0), i.width = Math.max(e(n + o, r, !1) - i.x, 0 === o ? 0 : 1), i.height = Math.max(e(a + s, r, !1) - i.y, 0 === s ? 0 : 1), t;
        }, b.subPixelOptimize = function (t, e, i) {
            var r = v(2 * t);
            return (r + v(e)) % 2 === 0 ? r / 2 : (r + (i ? 1 : -1)) / 2;
        }, b.setHoverStyle = function (t, e) {
            "group" === t.type ? t.traverse(function (t) {
                "group" !== t.type && l(t, e);
            }) : l(t, e), t.on("mouseover", h).on("mouseout", u), t.on("emphasis", c).on("normal", f);
        }, b.setText = function (t, e, i) {
            var r = e.getShallow("position") || "inside", n = r.indexOf("inside") >= 0 ? "white" : i, a = e.getModel("textStyle");
            p.extend(t, { textDistance: e.getShallow("distance") || 5, textFont: a.getFont(), textPosition: r, textFill: a.getTextColor() || n });
        }, b.updateProps = function (t, e, i, r, n) {
            d(!0, t, e, i, r, n);
        }, b.initProps = function (t, e, i, r, n) {
            d(!1, t, e, i, r, n);
        }, b.getTransform = function (t, e) {
            for (var i = _.identity([]); t && t !== e;)
                _.mul(i, t.getLocalTransform(), i), t = t.parent;
            return i;
        }, b.applyTransform = function (t, e, i) {
            return i && (e = _.invert([], e)), x.applyTransform([], t, e);
        }, b.transformDirection = function (t, e, i) {
            var r = 0 === e[4] || 0 === e[5] || 0 === e[0] ? 1 : Math.abs(2 * e[4] / e[0]), n = 0 === e[4] || 0 === e[5] || 0 === e[2] ? 1 : Math.abs(2 * e[4] / e[2]), a = ["left" === t ? -r : "right" === t ? r : 0, "top" === t ? -n : "bottom" === t ? n : 0];
            return a = b.applyTransform(a, e, i), Math.abs(a[0]) > Math.abs(a[1]) ? a[0] > 0 ? "right" : "left" : a[1] > 0 ? "bottom" : "top";
        }, b.groupTransition = function (t, e, i) {
            function r(t) {
                var e = {};
                return t.traverse(function (t) {
                    !t.isGroup && t.anid && (e[t.anid] = t);
                }), e;
            }
            function n(t) {
                var e = { position: x.clone(t.position), rotation: t.rotation };
                return t.shape && (e.shape = p.extend({}, t.shape)), e;
            }
            if (t && e) {
                var a = r(t);
                e.traverse(function (t) {
                    if (!t.isGroup && t.anid) {
                        var e = a[t.anid];
                        if (e) {
                            var r = n(t);
                            t.attr(n(e)), b.updateProps(t, r, i, t.dataIndex);
                        }
                    }
                });
            }
        }, b;
    }), e("zrender/tool/color", ["require"], function () {
        function t(t) {
            return t = Math.round(t), 0 > t ? 0 : t > 255 ? 255 : t;
        }
        function e(t) {
            return t = Math.round(t), 0 > t ? 0 : t > 360 ? 360 : t;
        }
        function i(t) {
            return 0 > t ? 0 : t > 1 ? 1 : t;
        }
        function r(e) {
            return t(e.length && "%" === e.charAt(e.length - 1) ? parseFloat(e) / 100 * 255 : parseInt(e, 10));
        }
        function n(t) {
            return i(t.length && "%" === t.charAt(t.length - 1) ? parseFloat(t) / 100 : parseFloat(t));
        }
        function a(t, e, i) {
            return 0 > i ? i += 1 : i > 1 && (i -= 1), 1 > 6 * i ? t + (e - t) * i * 6 : 1 > 2 * i ? e : 2 > 3 * i ? t + (e - t) * (2 / 3 - i) * 6 : t;
        }
        function o(t, e, i) {
            return t + (e - t) * i;
        }
        function s(t) {
            if (t) {
                t += "";
                var e = t.replace(/ /g, "").toLowerCase();
                if (e in m)
                    return m[e].slice();
                if ("#" !== e.charAt(0)) {
                    var i = e.indexOf("("), a = e.indexOf(")");
                    if (-1 !== i && a + 1 === e.length) {
                        var o = e.substr(0, i), s = e.substr(i + 1, a - (i + 1)).split(","), h = 1;
                        switch (o) {
                            case "rgba":
                                if (4 !== s.length)
                                    return;
                                h = n(s.pop());
                            case "rgb":
                                if (3 !== s.length)
                                    return;
                                return [r(s[0]), r(s[1]), r(s[2]), h];
                            case "hsla":
                                if (4 !== s.length)
                                    return;
                                return s[3] = n(s[3]), l(s);
                            case "hsl":
                                if (3 !== s.length)
                                    return;
                                return l(s);
                            default:
                                return;
                        }
                    }
                } else {
                    if (4 === e.length) {
                        var u = parseInt(e.substr(1), 16);
                        if (!(u >= 0 && 4095 >= u))
                            return;
                        return [(3840 & u) >> 4 | (3840 & u) >> 8, 240 & u | (240 & u) >> 4, 15 & u | (15 & u) << 4, 1];
                    }
                    if (7 === e.length) {
                        var u = parseInt(e.substr(1), 16);
                        if (!(u >= 0 && 16777215 >= u))
                            return;
                        return [(16711680 & u) >> 16, (65280 & u) >> 8, 255 & u, 1];
                    }
                }
            }
        }
        function l(e) {
            var i = (parseFloat(e[0]) % 360 + 360) % 360 / 360, r = n(e[1]), o = n(e[2]), s = .5 >= o ? o * (r + 1) : o + r - o * r, l = 2 * o - s, h = [t(255 * a(l, s, i + 1 / 3)), t(255 * a(l, s, i)), t(255 * a(l, s, i - 1 / 3))];
            return 4 === e.length && (h[3] = e[3]), h;
        }
        function h(t) {
            if (t) {
                var e, i, r = t[0] / 255, n = t[1] / 255, a = t[2] / 255, o = Math.min(r, n, a), s = Math.max(r, n, a), l = s - o, h = (s + o) / 2;
                if (0 === l)
                    e = 0, i = 0;
                else {
                    i = .5 > h ? l / (s + o) : l / (2 - s - o);
                    var u = ((s - r) / 6 + l / 2) / l, c = ((s - n) / 6 + l / 2) / l, f = ((s - a) / 6 + l / 2) / l;
                    r === s ? e = f - c : n === s ? e = 1 / 3 + u - f : a === s && (e = 2 / 3 + c - u), 0 > e && (e += 1), e > 1 && (e -= 1);
                }
                var d = [360 * e, i, h];
                return null != t[3] && d.push(t[3]), d;
            }
        }
        function u(t, e) {
            var i = s(t);
            if (i) {
                for (var r = 0; 3 > r; r++)
                    i[r] = 0 > e ? i[r] * (1 - e) | 0 : (255 - i[r]) * e + i[r] | 0;
                return v(i, 4 === i.length ? "rgba" : "rgb");
            }
        }
        function c(t) {
            var e = s(t);
            return e ? ((1 << 24) + (e[0] << 16) + (e[1] << 8) + +e[2]).toString(16).slice(1) : void 0;
        }
        function f(e, i, r) {
            if (i && i.length && e >= 0 && 1 >= e) {
                r = r || [0, 0, 0, 0];
                var n = e * (i.length - 1), a = Math.floor(n), s = Math.ceil(n), l = i[a], h = i[s], u = n - a;
                return r[0] = t(o(l[0], h[0], u)), r[1] = t(o(l[1], h[1], u)), r[2] = t(o(l[2], h[2], u)), r[3] = t(o(l[3], h[3], u)), r;
            }
        }
        function d(e, r, n) {
            if (r && r.length && e >= 0 && 1 >= e) {
                var a = e * (r.length - 1), l = Math.floor(a), h = Math.ceil(a), u = s(r[l]), c = s(r[h]), f = a - l, d = v([t(o(u[0], c[0], f)), t(o(u[1], c[1], f)), t(o(u[2], c[2], f)), i(o(u[3], c[3], f))], "rgba");
                return n ? { color: d, leftIndex: l, rightIndex: h, value: a } : d;
            }
        }
        function p(t, i, r, a) {
            return t = s(t), t ? (t = h(t), null != i && (t[0] = e(i)), null != r && (t[1] = n(r)), null != a && (t[2] = n(a)), v(l(t), "rgba")) : void 0;
        }
        function g(t, e) {
            return t = s(t), t && null != e ? (t[3] = i(e), v(t, "rgba")) : void 0;
        }
        function v(t, e) {
            var i = t[0] + "," + t[1] + "," + t[2];
            return ("rgba" === e || "hsva" === e || "hsla" === e) && (i += "," + t[3]), e + "(" + i + ")";
        }
        var m = { transparent: [0, 0, 0, 0], aliceblue: [240, 248, 255, 1], antiquewhite: [250, 235, 215, 1], aqua: [0, 255, 255, 1], aquamarine: [127, 255, 212, 1], azure: [240, 255, 255, 1], beige: [245, 245, 220, 1], bisque: [255, 228, 196, 1], black: [0, 0, 0, 1], blanchedalmond: [255, 235, 205, 1], blue: [0, 0, 255, 1], blueviolet: [138, 43, 226, 1], brown: [165, 42, 42, 1], burlywood: [222, 184, 135, 1], cadetblue: [95, 158, 160, 1], chartreuse: [127, 255, 0, 1], chocolate: [210, 105, 30, 1], coral: [255, 127, 80, 1], cornflowerblue: [100, 149, 237, 1], cornsilk: [255, 248, 220, 1], crimson: [220, 20, 60, 1], cyan: [0, 255, 255, 1], darkblue: [0, 0, 139, 1], darkcyan: [0, 139, 139, 1], darkgoldenrod: [184, 134, 11, 1], darkgray: [169, 169, 169, 1], darkgreen: [0, 100, 0, 1], darkgrey: [169, 169, 169, 1], darkkhaki: [189, 183, 107, 1], darkmagenta: [139, 0, 139, 1], darkolivegreen: [85, 107, 47, 1], darkorange: [255, 140, 0, 1], darkorchid: [153, 50, 204, 1], darkred: [139, 0, 0, 1], darksalmon: [233, 150, 122, 1], darkseagreen: [143, 188, 143, 1], darkslateblue: [72, 61, 139, 1], darkslategray: [47, 79, 79, 1], darkslategrey: [47, 79, 79, 1], darkturquoise: [0, 206, 209, 1], darkviolet: [148, 0, 211, 1], deeppink: [255, 20, 147, 1], deepskyblue: [0, 191, 255, 1], dimgray: [105, 105, 105, 1], dimgrey: [105, 105, 105, 1], dodgerblue: [30, 144, 255, 1], firebrick: [178, 34, 34, 1], floralwhite: [255, 250, 240, 1], forestgreen: [34, 139, 34, 1], fuchsia: [255, 0, 255, 1], gainsboro: [220, 220, 220, 1], ghostwhite: [248, 248, 255, 1], gold: [255, 215, 0, 1], goldenrod: [218, 165, 32, 1], gray: [128, 128, 128, 1], green: [0, 128, 0, 1], greenyellow: [173, 255, 47, 1], grey: [128, 128, 128, 1], honeydew: [240, 255, 240, 1], hotpink: [255, 105, 180, 1], indianred: [205, 92, 92, 1], indigo: [75, 0, 130, 1], ivory: [255, 255, 240, 1], khaki: [240, 230, 140, 1], lavender: [230, 230, 250, 1], lavenderblush: [255, 240, 245, 1], lawngreen: [124, 252, 0, 1], lemonchiffon: [255, 250, 205, 1], lightblue: [173, 216, 230, 1], lightcoral: [240, 128, 128, 1], lightcyan: [224, 255, 255, 1], lightgoldenrodyellow: [250, 250, 210, 1], lightgray: [211, 211, 211, 1], lightgreen: [144, 238, 144, 1], lightgrey: [211, 211, 211, 1], lightpink: [255, 182, 193, 1], lightsalmon: [255, 160, 122, 1], lightseagreen: [32, 178, 170, 1], lightskyblue: [135, 206, 250, 1], lightslategray: [119, 136, 153, 1], lightslategrey: [119, 136, 153, 1], lightsteelblue: [176, 196, 222, 1], lightyellow: [255, 255, 224, 1], lime: [0, 255, 0, 1], limegreen: [50, 205, 50, 1], linen: [250, 240, 230, 1], magenta: [255, 0, 255, 1], maroon: [128, 0, 0, 1], mediumaquamarine: [102, 205, 170, 1], mediumblue: [0, 0, 205, 1], mediumorchid: [186, 85, 211, 1], mediumpurple: [147, 112, 219, 1], mediumseagreen: [60, 179, 113, 1], mediumslateblue: [123, 104, 238, 1], mediumspringgreen: [0, 250, 154, 1], mediumturquoise: [72, 209, 204, 1], mediumvioletred: [199, 21, 133, 1], midnightblue: [25, 25, 112, 1], mintcream: [245, 255, 250, 1], mistyrose: [255, 228, 225, 1], moccasin: [255, 228, 181, 1], navajowhite: [255, 222, 173, 1], navy: [0, 0, 128, 1], oldlace: [253, 245, 230, 1], olive: [128, 128, 0, 1], olivedrab: [107, 142, 35, 1], orange: [255, 165, 0, 1], orangered: [255, 69, 0, 1], orchid: [218, 112, 214, 1], palegoldenrod: [238, 232, 170, 1], palegreen: [152, 251, 152, 1], paleturquoise: [175, 238, 238, 1], palevioletred: [219, 112, 147, 1], papayawhip: [255, 239, 213, 1], peachpuff: [255, 218, 185, 1], peru: [205, 133, 63, 1], pink: [255, 192, 203, 1], plum: [221, 160, 221, 1], powderblue: [176, 224, 230, 1], purple: [128, 0, 128, 1], red: [255, 0, 0, 1], rosybrown: [188, 143, 143, 1], royalblue: [65, 105, 225, 1], saddlebrown: [139, 69, 19, 1], salmon: [250, 128, 114, 1], sandybrown: [244, 164, 96, 1], seagreen: [46, 139, 87, 1], seashell: [255, 245, 238, 1], sienna: [160, 82, 45, 1], silver: [192, 192, 192, 1], skyblue: [135, 206, 235, 1], slateblue: [106, 90, 205, 1], slategray: [112, 128, 144, 1], slategrey: [112, 128, 144, 1], snow: [255, 250, 250, 1], springgreen: [0, 255, 127, 1], steelblue: [70, 130, 180, 1], tan: [210, 180, 140, 1], teal: [0, 128, 128, 1], thistle: [216, 191, 216, 1], tomato: [255, 99, 71, 1], turquoise: [64, 224, 208, 1], violet: [238, 130, 238, 1], wheat: [245, 222, 179, 1], white: [255, 255, 255, 1], whitesmoke: [245, 245, 245, 1], yellow: [255, 255, 0, 1], yellowgreen: [154, 205, 50, 1] };
        return { parse: s, lift: u, toHex: c, fastMapToColor: f, mapToColor: d, modifyHSL: p, modifyAlpha: g, stringify: v };
    }), e("zrender/mixin/Eventful", ["require"], function () {
        var t = Array.prototype.slice, e = function () {
            this._$handlers = {};
        };
        return e.prototype = { constructor: e, one: function (t, e, i) {
                var r = this._$handlers;
                if (!e || !t)
                    return this;
                r[t] || (r[t] = []);
                for (var n = 0; n < r[t].length; n++)
                    if (r[t][n].h === e)
                        return this;
                return r[t].push({ h: e, one: !0, ctx: i || this }), this;
            }, on: function (t, e, i) {
                var r = this._$handlers;
                if (!e || !t)
                    return this;
                r[t] || (r[t] = []);
                for (var n = 0; n < r[t].length; n++)
                    if (r[t][n].h === e)
                        return this;
                return r[t].push({ h: e, one: !1, ctx: i || this }), this;
            }, isSilent: function (t) {
                var e = this._$handlers;
                return e[t] && e[t].length;
            }, off: function (t, e) {
                var i = this._$handlers;
                if (!t)
                    return this._$handlers = {}, this;
                if (e) {
                    if (i[t]) {
                        for (var r = [], n = 0, a = i[t].length; a > n; n++)
                            i[t][n].h != e && r.push(i[t][n]);
                        i[t] = r;
                    }
                    i[t] && 0 === i[t].length && delete i[t];
                } else
                    delete i[t];
                return this;
            }, trigger: function (e) {
                if (this._$handlers[e]) {
                    var i = arguments, r = i.length;
                    r > 3 && (i = t.call(i, 1));
                    for (var n = this._$handlers[e], a = n.length, o = 0; a > o;) {
                        switch (r) {
                            case 1:
                                n[o].h.call(n[o].ctx);
                                break;
                            case 2:
                                n[o].h.call(n[o].ctx, i[1]);
                                break;
                            case 3:
                                n[o].h.call(n[o].ctx, i[1], i[2]);
                                break;
                            default:
                                n[o].h.apply(n[o].ctx, i);
                        }
                        n[o].one ? (n.splice(o, 1), a--) : o++;
                    }
                }
                return this;
            }, triggerWithContext: function (e) {
                if (this._$handlers[e]) {
                    var i = arguments, r = i.length;
                    r > 4 && (i = t.call(i, 1, i.length - 1));
                    for (var n = i[i.length - 1], a = this._$handlers[e], o = a.length, s = 0; o > s;) {
                        switch (r) {
                            case 1:
                                a[s].h.call(n);
                                break;
                            case 2:
                                a[s].h.call(n, i[1]);
                                break;
                            case 3:
                                a[s].h.call(n, i[1], i[2]);
                                break;
                            default:
                                a[s].h.apply(n, i);
                        }
                        a[s].one ? (a.splice(s, 1), o--) : s++;
                    }
                }
                return this;
            } }, e;
    }), e("echarts/loading/default", ["require", "../util/graphic", "zrender/core/util"], function (t) {
        var e = t("../util/graphic"), i = t("zrender/core/util"), r = Math.PI;
        return function (t, n) {
            n = n || {}, i.defaults(n, { text: "loading", color: "#c23531", textColor: "#000", maskColor: "rgba(255, 255, 255, 0.8)", zlevel: 0 });
            var a = new e.Rect({ style: { fill: n.maskColor }, zlevel: n.zlevel, z: 1e4 }), o = new e.Arc({ shape: { startAngle: -r / 2, endAngle: -r / 2 + .1, r: 10 }, style: { stroke: n.color, lineCap: "round", lineWidth: 5 }, zlevel: n.zlevel, z: 10001 }), s = new e.Rect({ style: { fill: "none", text: n.text, textPosition: "right", textDistance: 10, textFill: n.textColor }, zlevel: n.zlevel, z: 10001 });
            o.animateShape(!0).when(1e3, { endAngle: 3 * r / 2 }).start("circularInOut"), o.animateShape(!0).when(1e3, { startAngle: 3 * r / 2 }).delay(300).start("circularInOut");
            var l = new e.Group;
            return l.add(o), l.add(s), l.add(a), l.resize = function () {
                var e = t.getWidth() / 2, i = t.getHeight() / 2;
                o.setShape({ cx: e, cy: i });
                var r = o.shape.r;
                s.setShape({ x: e - r, y: i - r, width: 2 * r, height: 2 * r }), a.setShape({ x: 0, y: 0, width: t.getWidth(), height: t.getHeight() });
            }, l.resize(), l;
        };
    }), e("zrender/core/timsort", [], function () {
        function t(t) {
            for (var e = 0; t >= l;)
                e |= 1 & t, t >>= 1;
            return t + e;
        }
        function e(t, e, r, n) {
            var a = e + 1;
            if (a === r)
                return 1;
            if (n(t[a++], t[e]) < 0) {
                for (; r > a && n(t[a], t[a - 1]) < 0;)
                    a++;
                i(t, e, a);
            } else
                for (; r > a && n(t[a], t[a - 1]) >= 0;)
                    a++;
            return a - e;
        }
        function i(t, e, i) {
            for (i--; i > e;) {
                var r = t[e];
                t[e++] = t[i], t[i--] = r;
            }
        }
        function r(t, e, i, r, n) {
            for (r === e && r++; i > r; r++) {
                for (var a, o = t[r], s = e, l = r; l > s;)
                    a = s + l >>> 1, n(o, t[a]) < 0 ? l = a : s = a + 1;
                var h = r - s;
                switch (h) {
                    case 3:
                        t[s + 3] = t[s + 2];
                    case 2:
                        t[s + 2] = t[s + 1];
                    case 1:
                        t[s + 1] = t[s];
                        break;
                    default:
                        for (; h > 0;)
                            t[s + h] = t[s + h - 1], h--;
                }
                t[s] = o;
            }
        }
        function n(t, e, i, r, n, a) {
            var o = 0, s = 0, l = 1;
            if (a(t, e[i + n]) > 0) {
                for (s = r - n; s > l && a(t, e[i + n + l]) > 0;)
                    o = l, l = (l << 1) + 1, 0 >= l && (l = s);
                l > s && (l = s), o += n, l += n;
            } else {
                for (s = n + 1; s > l && a(t, e[i + n - l]) <= 0;)
                    o = l, l = (l << 1) + 1, 0 >= l && (l = s);
                l > s && (l = s);
                var h = o;
                o = n - l, l = n - h;
            }
            for (o++; l > o;) {
                var u = o + (l - o >>> 1);
                a(t, e[i + u]) > 0 ? o = u + 1 : l = u;
            }
            return l;
        }
        function a(t, e, i, r, n, a) {
            var o = 0, s = 0, l = 1;
            if (a(t, e[i + n]) < 0) {
                for (s = n + 1; s > l && a(t, e[i + n - l]) < 0;)
                    o = l, l = (l << 1) + 1, 0 >= l && (l = s);
                l > s && (l = s);
                var h = o;
                o = n - l, l = n - h;
            } else {
                for (s = r - n; s > l && a(t, e[i + n + l]) >= 0;)
                    o = l, l = (l << 1) + 1, 0 >= l && (l = s);
                l > s && (l = s), o += n, l += n;
            }
            for (o++; l > o;) {
                var u = o + (l - o >>> 1);
                a(t, e[i + u]) < 0 ? l = u : o = u + 1;
            }
            return l;
        }
        function o(t, e) {
            function i(t, e) {
                f[y] = t, d[y] = e, y += 1;
            }
            function r() {
                for (; y > 1;) {
                    var t = y - 2;
                    if (t >= 1 && d[t - 1] <= d[t] + d[t + 1] || t >= 2 && d[t - 2] <= d[t] + d[t - 1])
                        d[t - 1] < d[t + 1] && t--;
                    else if (d[t] > d[t + 1])
                        break;
                    s(t);
                }
            }
            function o() {
                for (; y > 1;) {
                    var t = y - 2;
                    t > 0 && d[t - 1] < d[t + 1] && t--, s(t);
                }
            }
            function s(i) {
                var r = f[i], o = d[i], s = f[i + 1], h = d[i + 1];
                d[i] = o + h, i === y - 3 && (f[i + 1] = f[i + 2], d[i + 1] = d[i + 2]), y--;
                var u = a(t[s], t, r, o, 0, e);
                r += u, o -= u, 0 !== o && (h = n(t[r + o - 1], t, s, h, h - 1, e), 0 !== h && (h >= o ? l(r, o, s, h) : c(r, o, s, h)));
            }
            function l(i, r, o, s) {
                var l = 0;
                for (l = 0; r > l; l++)
                    _[l] = t[i + l];
                var u = 0, c = o, f = i;
                if (t[f++] = t[c++], 0 !== --s) {
                    if (1 === r) {
                        for (l = 0; s > l; l++)
                            t[f + l] = t[c + l];
                        return void (t[f + s] = _[u]);
                    }
                    for (var d, g, v, m = p; ;) {
                        d = 0, g = 0, v = !1;
                        do
                            if (e(t[c], _[u]) < 0) {
                                if (t[f++] = t[c++], g++, d = 0, 0 === --s) {
                                    v = !0;
                                    break;
                                }
                            } else if (t[f++] = _[u++], d++, g = 0, 1 === --r) {
                                v = !0;
                                break;
                            } while(m > (d | g));
                        if (v)
                            break;
                        do {
                            if (d = a(t[c], _, u, r, 0, e), 0 !== d) {
                                for (l = 0; d > l; l++)
                                    t[f + l] = _[u + l];
                                if (f += d, u += d, r -= d, 1 >= r) {
                                    v = !0;
                                    break;
                                }
                            }
                            if (t[f++] = t[c++], 0 === --s) {
                                v = !0;
                                break;
                            }
                            if (g = n(_[u], t, c, s, 0, e), 0 !== g) {
                                for (l = 0; g > l; l++)
                                    t[f + l] = t[c + l];
                                if (f += g, c += g, s -= g, 0 === s) {
                                    v = !0;
                                    break;
                                }
                            }
                            if (t[f++] = _[u++], 1 === --r) {
                                v = !0;
                                break;
                            }
                            m--;
                        } while(d >= h || g >= h);
                        if (v)
                            break;
                        0 > m && (m = 0), m += 2;
                    }
                    if (p = m, 1 > p && (p = 1), 1 === r) {
                        for (l = 0; s > l; l++)
                            t[f + l] = t[c + l];
                        t[f + s] = _[u];
                    } else {
                        if (0 === r)
                            throw new Error;
                        for (l = 0; r > l; l++)
                            t[f + l] = _[u + l];
                    }
                } else
                    for (l = 0; r > l; l++)
                        t[f + l] = _[u + l];
            }
            function c(i, r, o, s) {
                var l = 0;
                for (l = 0; s > l; l++)
                    _[l] = t[o + l];
                var u = i + r - 1, c = s - 1, f = o + s - 1, d = 0, g = 0;
                if (t[f--] = t[u--], 0 !== --r) {
                    if (1 === s) {
                        for (f -= r, u -= r, g = f + 1, d = u + 1, l = r - 1; l >= 0; l--)
                            t[g + l] = t[d + l];
                        return void (t[f] = _[c]);
                    }
                    for (var v = p; ;) {
                        var m = 0, y = 0, x = !1;
                        do
                            if (e(_[c], t[u]) < 0) {
                                if (t[f--] = t[u--], m++, y = 0, 0 === --r) {
                                    x = !0;
                                    break;
                                }
                            } else if (t[f--] = _[c--], y++, m = 0, 1 === --s) {
                                x = !0;
                                break;
                            } while(v > (m | y));
                        if (x)
                            break;
                        do {
                            if (m = r - a(_[c], t, i, r, r - 1, e), 0 !== m) {
                                for (f -= m, u -= m, r -= m, g = f + 1, d = u + 1, l = m - 1; l >= 0; l--)
                                    t[g + l] = t[d + l];
                                if (0 === r) {
                                    x = !0;
                                    break;
                                }
                            }
                            if (t[f--] = _[c--], 1 === --s) {
                                x = !0;
                                break;
                            }
                            if (y = s - n(t[u], _, 0, s, s - 1, e), 0 !== y) {
                                for (f -= y, c -= y, s -= y, g = f + 1, d = c + 1, l = 0; y > l; l++)
                                    t[g + l] = _[d + l];
                                if (1 >= s) {
                                    x = !0;
                                    break;
                                }
                            }
                            if (t[f--] = t[u--], 0 === --r) {
                                x = !0;
                                break;
                            }
                            v--;
                        } while(m >= h || y >= h);
                        if (x)
                            break;
                        0 > v && (v = 0), v += 2;
                    }
                    if (p = v, 1 > p && (p = 1), 1 === s) {
                        for (f -= r, u -= r, g = f + 1, d = u + 1, l = r - 1; l >= 0; l--)
                            t[g + l] = t[d + l];
                        t[f] = _[c];
                    } else {
                        if (0 === s)
                            throw new Error;
                        for (d = f - (s - 1), l = 0; s > l; l++)
                            t[d + l] = _[l];
                    }
                } else
                    for (d = f - (s - 1), l = 0; s > l; l++)
                        t[d + l] = _[l];
            }
            var f, d, p = h, g = 0, v = u, m = 0, y = 0;
            g = t.length, 2 * u > g && (v = g >>> 1);
            var _ = [];
            m = 120 > g ? 5 : 1542 > g ? 10 : 119151 > g ? 19 : 40, f = [], d = [], this.mergeRuns = r, this.forceMergeRuns = o, this.pushRun = i;
        }
        function s(i, n, a, s) {
            a || (a = 0), s || (s = i.length);
            var h = s - a;
            if (!(2 > h)) {
                var u = 0;
                if (l > h)
                    return u = e(i, a, s, n), void r(i, a, s, a + u, n);
                var c = new o(i, n), f = t(h);
                do {
                    if (u = e(i, a, s, n), f > u) {
                        var d = h;
                        d > f && (d = f), r(i, a, a + d, a + u, n), u = d;
                    }
                    c.pushRun(a, u), c.mergeRuns(), h -= u, a += u;
                } while(0 !== h);
                c.forceMergeRuns();
            }
        }
        var l = 32, h = 7, u = 256;
        return s;
    }), e("echarts/preprocessor/backwardCompat", ["require", "zrender/core/util", "./helper/compatStyle"], function (t) {
        function e(t, e) {
            e = e.split(",");
            for (var i = t, r = 0; r < e.length && (i = i && i[e[r]], null != i); r++)
                ;
            return i;
        }
        function i(t, e, i, r) {
            e = e.split(",");
            for (var n, a = t, o = 0; o < e.length - 1; o++)
                n = e[o], null == a[n] && (a[n] = {}), a = a[n];
            (r || null == a[e[o]]) && (a[e[o]] = i);
        }
        function r(t) {
            h(o, function (e) {
                e[0] in t && !(e[1] in t) && (t[e[1]] = t[e[0]]);
            });
        }
        var n = t("zrender/core/util"), a = t("./helper/compatStyle"), o = [["x", "left"], ["y", "top"], ["x2", "right"], ["y2", "bottom"]], s = ["grid", "geo", "parallel", "legend", "toolbox", "title", "visualMap", "dataZoom", "timeline"], l = ["bar", "boxplot", "candlestick", "chord", "effectScatter", "funnel", "gauge", "lines", "graph", "heatmap", "line", "map", "parallel", "pie", "radar", "sankey", "scatter", "treemap"], h = n.each;
        return function (t) {
            h(t.series, function (t) {
                if (n.isObject(t)) {
                    var o = t.type;
                    if (a(t), ("pie" === o || "gauge" === o) && null != t.clockWise && (t.clockwise = t.clockWise), "gauge" === o) {
                        var s = e(t, "pointer.color");
                        null != s && i(t, "itemStyle.normal.color", s);
                    }
                    for (var h = 0; h < l.length; h++)
                        if (l[h] === t.type) {
                            r(t);
                            break;
                        }
                }
            }), t.dataRange && (t.visualMap = t.dataRange), h(s, function (e) {
                var i = t[e];
                i && (n.isArray(i) || (i = [i]), h(i, function (t) {
                    r(t);
                }));
            });
        };
    }), e("echarts/visual/seriesColor", ["require", "zrender/graphic/Gradient"], function (t) {
        var e = t("zrender/graphic/Gradient");
        return function (t) {
            function i(i) {
                var r = (i.visualColorAccessPath || "itemStyle.normal.color").split("."), n = i.getData(), a = i.get(r) || i.getColorFromPalette(i.get("name"));
                n.setVisual("color", a), t.isSeriesFiltered(i) || ("function" != typeof a || a instanceof e || n.each(function (t) {
                    n.setItemVisual(t, "color", a(i.getDataParams(t)));
                }), n.each(function (t) {
                    var e = n.getItemModel(t), i = e.get(r, !0);
                    null != i && n.setItemVisual(t, "color", i);
                }));
            }
            t.eachRawSeries(i);
        };
    }), e("echarts/model/Model", ["require", "zrender/core/util", "../util/clazz", "./mixin/lineStyle", "./mixin/areaStyle", "./mixin/textStyle", "./mixin/itemStyle"], function (t) {
        function e(t, e, i) {
            this.parentModel = e, this.ecModel = i, this.option = t;
        }
        var i = t("zrender/core/util"), r = t("../util/clazz");
        e.prototype = { constructor: e, init: null, mergeOption: function (t) {
                i.merge(this.option, t, !0);
            }, get: function (t, e) {
                if (!t)
                    return this.option;
                "string" == typeof t && (t = t.split("."));
                for (var i = this.option, r = this.parentModel, n = 0; n < t.length && (!t[n] || (i = i && "object" == typeof i ? i[t[n]] : null, null != i)); n++)
                    ;
                return null == i && r && !e && (i = r.get(t)), i;
            }, getShallow: function (t, e) {
                var i = this.option, r = i && i[t], n = this.parentModel;
                return null == r && n && !e && (r = n.getShallow(t)), r;
            }, getModel: function (t, i) {
                var r = this.get(t, !0), n = this.parentModel, a = new e(r, i || n && n.getModel(t), this.ecModel);
                return a;
            }, isEmpty: function () {
                return null == this.option;
            }, restoreData: function () {
            }, clone: function () {
                var t = this.constructor;
                return new t(i.clone(this.option));
            }, setReadOnly: function (t) {
                r.setReadOnly(this, t);
            } }, r.enableClassExtend(e);
        var n = i.mixin;
        return n(e, t("./mixin/lineStyle")), n(e, t("./mixin/areaStyle")), n(e, t("./mixin/textStyle")), n(e, t("./mixin/itemStyle")), e;
    }), e("echarts/data/List", ["require", "../model/Model", "./DataDiffer", "zrender/core/util", "../util/model"], function (t) {
        function e(t) {
            return u.isArray(t) || (t = [t]), t;
        }
        function i(t, e) {
            var i = t.dimensions, r = new g(u.map(i, t.getDimensionInfo, t), t.hostModel);
            p(r, t);
            for (var n = r._storage = {}, a = t._storage, o = 0; o < i.length; o++) {
                var s = i[o], l = a[s];
                n[s] = u.indexOf(e, s) >= 0 ? new l.constructor(a[s].length) : a[s];
            }
            return r;
        }
        var r = "undefined", n = "undefined" == typeof window ? global : window, a = typeof n.Float64Array === r ? Array : n.Float64Array, o = typeof n.Int32Array === r ? Array : n.Int32Array, s = { "float": a, "int": o, ordinal: Array, number: Array, time: Array }, l = t("../model/Model"), h = t("./DataDiffer"), u = t("zrender/core/util"), c = t("../util/model"), f = u.isObject, d = ["stackedOn", "hasItemOption", "_nameList", "_idList", "_rawData"], p = function (t, e) {
            u.each(d.concat(e.__wrappedMethods || []), function (i) {
                e.hasOwnProperty(i) && (t[i] = e[i]);
            }), t.__wrappedMethods = e.__wrappedMethods;
        }, g = function (t, e) {
            t = t || ["x", "y"];
            for (var i = {}, r = [], n = 0; n < t.length; n++) {
                var a, o = {};
                "string" == typeof t[n] ? (a = t[n], o = { name: a, stackable: !1, type: "number" }) : (o = t[n], a = o.name, o.type = o.type || "number"), r.push(a), i[a] = o;
            }
            this.dimensions = r, this._dimensionInfos = i, this.hostModel = e, this.dataType, this.indices = [], this._storage = {}, this._nameList = [], this._idList = [], this._optionModels = [], this.stackedOn = null, this._visual = {}, this._layout = {}, this._itemVisuals = [], this._itemLayouts = [], this._graphicEls = [], this._rawData, this._extent;
        }, v = g.prototype;
        v.type = "list", v.hasItemOption = !0, v.getDimension = function (t) {
            return isNaN(t) || (t = this.dimensions[t] || t), t;
        }, v.getDimensionInfo = function (t) {
            return u.clone(this._dimensionInfos[this.getDimension(t)]);
        }, v.initData = function (t, e, i) {
            if (t = t || [], !u.isArray(t))
                throw new Error("Invalid data.");
            this._rawData = t;
            var r = this._storage = {}, n = this.indices = [], a = this.dimensions, o = t.length, l = this._dimensionInfos, h = [], f = {};
            e = e || [];
            for (var d = 0; d < a.length; d++) {
                var p = l[a[d]], g = s[p.type];
                r[a[d]] = new g(o);
            }
            var v = this;
            i || (v.hasItemOption = !1), i = i || function (t, e, i, r) {
                var n = c.getDataItemValue(t);
                return c.isDataItemOption(t) && (v.hasItemOption = !0), c.converDataValue(n instanceof Array ? n[r] : n, l[e]);
            };
            for (var m = 0; m < t.length; m++) {
                for (var y = t[m], _ = 0; _ < a.length; _++) {
                    var x = a[_], b = r[x];
                    b[m] = i(y, x, m, _);
                }
                n.push(m);
            }
            for (var d = 0; d < t.length; d++) {
                e[d] || t[d] && null != t[d].name && (e[d] = t[d].name);
                var w = e[d] || "", T = t[d] && t[d].id;
                !T && w && (f[w] = f[w] || 0, T = w, f[w] > 0 && (T += "__ec__" + f[w]), f[w]++), T && (h[d] = T);
            }
            this._nameList = e, this._idList = h;
        }, v.count = function () {
            return this.indices.length;
        }, v.get = function (t, e, i) {
            var r = this._storage, n = this.indices[e];
            if (null == n)
                return 0 / 0;
            var a = r[t] && r[t][n];
            if (i) {
                var o = this._dimensionInfos[t];
                if (o && o.stackable)
                    for (var s = this.stackedOn; s;) {
                        var l = s.get(t, e);
                        (a >= 0 && l > 0 || 0 >= a && 0 > l) && (a += l), s = s.stackedOn;
                    }
            }
            return a;
        }, v.getValues = function (t, e, i) {
            var r = [];
            u.isArray(t) || (i = e, e = t, t = this.dimensions);
            for (var n = 0, a = t.length; a > n; n++)
                r.push(this.get(t[n], e, i));
            return r;
        }, v.hasValue = function (t) {
            for (var e = this.dimensions, i = this._dimensionInfos, r = 0, n = e.length; n > r; r++)
                if ("ordinal" !== i[e[r]].type && isNaN(this.get(e[r], t)))
                    return !1;
            return !0;
        }, v.getDataExtent = function (t, e) {
            t = this.getDimension(t);
            var i = this._storage[t], r = this.getDimensionInfo(t);
            e = r && r.stackable && e;
            var n, a = (this._extent || (this._extent = {}))[t + !!e];
            if (a)
                return a;
            if (i) {
                for (var o = 1 / 0, s = -1 / 0, l = 0, h = this.count(); h > l; l++)
                    n = this.get(t, l, e), o > n && (o = n), n > s && (s = n);
                return this._extent[t + !!e] = [o, s];
            }
            return [1 / 0, -1 / 0];
        }, v.getSum = function (t, e) {
            var i = this._storage[t], r = 0;
            if (i)
                for (var n = 0, a = this.count(); a > n; n++) {
                    var o = this.get(t, n, e);
                    isNaN(o) || (r += o);
                }
            return r;
        }, v.indexOf = function (t, e) {
            var i = this._storage, r = i[t], n = this.indices;
            if (r)
                for (var a = 0, o = n.length; o > a; a++) {
                    var s = n[a];
                    if (r[s] === e)
                        return a;
                }
            return -1;
        }, v.indexOfName = function (t) {
            for (var e = this.indices, i = this._nameList, r = 0, n = e.length; n > r; r++) {
                var a = e[r];
                if (i[a] === t)
                    return r;
            }
            return -1;
        }, v.indexOfRawIndex = function (t) {
            for (var e = this.indices, i = 0, r = e.length - 1; r >= i;) {
                var n = (i + r) / 2 | 0;
                if (e[n] < t)
                    i = n + 1;
                else {
                    if (!(e[n] > t))
                        return n;
                    r = n - 1;
                }
            }
            return -1;
        }, v.indexOfNearest = function (t, e, i, r) {
            var n = this._storage, a = n[t];
            null == r && (r = 1 / 0);
            var o = -1;
            if (a)
                for (var s = Number.MAX_VALUE, l = 0, h = this.count(); h > l; l++) {
                    var u = e - this.get(t, l, i), c = Math.abs(u);
                    r >= u && (s > c || c === s && u > 0) && (s = c, o = l);
                }
            return o;
        }, v.getRawIndex = function (t) {
            var e = this.indices[t];
            return null == e ? -1 : e;
        }, v.getRawDataItem = function (t) {
            return this._rawData[this.getRawIndex(t)];
        }, v.getName = function (t) {
            return this._nameList[this.indices[t]] || "";
        }, v.getId = function (t) {
            return this._idList[this.indices[t]] || this.getRawIndex(t) + "";
        }, v.each = function (t, i, r, n) {
            "function" == typeof t && (n = r, r = i, i = t, t = []), t = u.map(e(t), this.getDimension, this);
            var a = [], o = t.length, s = this.indices;
            n = n || this;
            for (var l = 0; l < s.length; l++)
                switch (o) {
                    case 0:
                        i.call(n, l);
                        break;
                    case 1:
                        i.call(n, this.get(t[0], l, r), l);
                        break;
                    case 2:
                        i.call(n, this.get(t[0], l, r), this.get(t[1], l, r), l);
                        break;
                    default:
                        for (var h = 0; o > h; h++)
                            a[h] = this.get(t[h], l, r);
                        a[h] = l, i.apply(n, a);
                }
        }, v.filterSelf = function (t, i, r, n) {
            "function" == typeof t && (n = r, r = i, i = t, t = []), t = u.map(e(t), this.getDimension, this);
            var a = [], o = [], s = t.length, l = this.indices;
            n = n || this;
            for (var h = 0; h < l.length; h++) {
                var c;
                if (1 === s)
                    c = i.call(n, this.get(t[0], h, r), h);
                else {
                    for (var f = 0; s > f; f++)
                        o[f] = this.get(t[f], h, r);
                    o[f] = h, c = i.apply(n, o);
                }
                c && a.push(l[h]);
            }
            return this.indices = a, this._extent = {}, this;
        }, v.mapArray = function (t, e, i, r) {
            "function" == typeof t && (r = i, i = e, e = t, t = []);
            var n = [];
            return this.each(t, function () {
                n.push(e && e.apply(this, arguments));
            }, i, r), n;
        }, v.map = function (t, r, n, a) {
            t = u.map(e(t), this.getDimension, this);
            var o = i(this, t), s = o.indices = this.indices, l = o._storage, h = [];
            return this.each(t, function () {
                var e = arguments[arguments.length - 1], i = r && r.apply(this, arguments);
                if (null != i) {
                    "number" == typeof i && (h[0] = i, i = h);
                    for (var n = 0; n < i.length; n++) {
                        var a = t[n], o = l[a], u = s[e];
                        o && (o[u] = i[n]);
                    }
                }
            }, n, a), o;
        }, v.downSample = function (t, e, r, n) {
            for (var a = i(this, [t]), o = this._storage, s = a._storage, l = this.indices, h = a.indices = [], u = [], c = [], f = Math.floor(1 / e), d = s[t], p = this.count(), g = 0; g < o[t].length; g++)
                s[t][g] = o[t][g];
            for (var g = 0; p > g; g += f) {
                f > p - g && (f = p - g, u.length = f);
                for (var v = 0; f > v; v++) {
                    var m = l[g + v];
                    u[v] = d[m], c[v] = m;
                }
                var y = r(u), m = c[n(u, y) || 0];
                d[m] = y, h.push(m);
            }
            return a;
        }, v.getItemModel = function (t) {
            var e = this.hostModel;
            return t = this.indices[t], new l(this._rawData[t], e, e && e.ecModel);
        }, v.diff = function (t) {
            var e = this._idList, i = t && t._idList;
            return new h(t ? t.indices : [], this.indices, function (t) {
                return i[t] || t + "";
            }, function (t) {
                return e[t] || t + "";
            });
        }, v.getVisual = function (t) {
            var e = this._visual;
            return e && e[t];
        }, v.setVisual = function (t, e) {
            if (f(t))
                for (var i in t)
                    t.hasOwnProperty(i) && this.setVisual(i, t[i]);
            else
                this._visual = this._visual || {}, this._visual[t] = e;
        }, v.setLayout = function (t, e) {
            if (f(t))
                for (var i in t)
                    t.hasOwnProperty(i) && this.setLayout(i, t[i]);
            else
                this._layout[t] = e;
        }, v.getLayout = function (t) {
            return this._layout[t];
        }, v.getItemLayout = function (t) {
            return this._itemLayouts[t];
        }, v.setItemLayout = function (t, e, i) {
            this._itemLayouts[t] = i ? u.extend(this._itemLayouts[t] || {}, e) : e;
        }, v.clearItemLayouts = function () {
            this._itemLayouts.length = 0;
        }, v.getItemVisual = function (t, e, i) {
            var r = this._itemVisuals[t], n = r && r[e];
            return null != n || i ? n : this.getVisual(e);
        }, v.setItemVisual = function (t, e, i) {
            var r = this._itemVisuals[t] || {};
            if (this._itemVisuals[t] = r, f(e))
                for (var n in e)
                    e.hasOwnProperty(n) && (r[n] = e[n]);
            else
                r[e] = i;
        }, v.clearAllVisual = function () {
            this._visual = {}, this._itemVisuals = [];
        };
        var m = function (t) {
            t.seriesIndex = this.seriesIndex, t.dataIndex = this.dataIndex, t.dataType = this.dataType;
        };
        return v.setItemGraphicEl = function (t, e) {
            var i = this.hostModel;
            e && (e.dataIndex = t, e.dataType = this.dataType, e.seriesIndex = i && i.seriesIndex, "group" === e.type && e.traverse(m, e)), this._graphicEls[t] = e;
        }, v.getItemGraphicEl = function (t) {
            return this._graphicEls[t];
        }, v.eachItemGraphicEl = function (t, e) {
            u.each(this._graphicEls, function (i, r) {
                i && t && t.call(e, i, r);
            });
        }, v.cloneShallow = function () {
            var t = u.map(this.dimensions, this.getDimensionInfo, this), e = new g(t, this.hostModel);
            return e._storage = this._storage, p(e, this), e.indices = this.indices.slice(), this._extent && (e._extent = u.extend({}, this._extent)), e;
        }, v.wrapMethod = function (t, e) {
            var i = this[t];
            "function" == typeof i && (this.__wrappedMethods = this.__wrappedMethods || [], this.__wrappedMethods.push(t), this[t] = function () {
                var t = i.apply(this, arguments);
                return e.apply(this, [t].concat(u.slice(arguments)));
            });
        }, v.TRANSFERABLE_METHODS = ["cloneShallow", "downSample", "map"], v.CHANGABLE_METHODS = ["filterSelf"], g;
    }), e("zrender/core/matrix", [], function () {
        var t = "undefined" == typeof Float32Array ? Array : Float32Array, e = { create: function () {
                var i = new t(6);
                return e.identity(i), i;
            }, identity: function (t) {
                return t[0] = 1, t[1] = 0, t[2] = 0, t[3] = 1, t[4] = 0, t[5] = 0, t;
            }, copy: function (t, e) {
                return t[0] = e[0], t[1] = e[1], t[2] = e[2], t[3] = e[3], t[4] = e[4], t[5] = e[5], t;
            }, mul: function (t, e, i) {
                var r = e[0] * i[0] + e[2] * i[1], n = e[1] * i[0] + e[3] * i[1], a = e[0] * i[2] + e[2] * i[3], o = e[1] * i[2] + e[3] * i[3], s = e[0] * i[4] + e[2] * i[5] + e[4], l = e[1] * i[4] + e[3] * i[5] + e[5];
                return t[0] = r, t[1] = n, t[2] = a, t[3] = o, t[4] = s, t[5] = l, t;
            }, translate: function (t, e, i) {
                return t[0] = e[0], t[1] = e[1], t[2] = e[2], t[3] = e[3], t[4] = e[4] + i[0], t[5] = e[5] + i[1], t;
            }, rotate: function (t, e, i) {
                var r = e[0], n = e[2], a = e[4], o = e[1], s = e[3], l = e[5], h = Math.sin(i), u = Math.cos(i);
                return t[0] = r * u + o * h, t[1] = -r * h + o * u, t[2] = n * u + s * h, t[3] = -n * h + u * s, t[4] = u * a + h * l, t[5] = u * l - h * a, t;
            }, scale: function (t, e, i) {
                var r = i[0], n = i[1];
                return t[0] = e[0] * r, t[1] = e[1] * n, t[2] = e[2] * r, t[3] = e[3] * n, t[4] = e[4] * r, t[5] = e[5] * n, t;
            }, invert: function (t, e) {
                var i = e[0], r = e[2], n = e[4], a = e[1], o = e[3], s = e[5], l = i * o - a * r;
                return l ? (l = 1 / l, t[0] = o * l, t[1] = -a * l, t[2] = -r * l, t[3] = i * l, t[4] = (r * s - o * n) * l, t[5] = (a * n - i * s) * l, t) : null;
            } };
        return e;
    }), e("zrender/core/BoundingRect", ["require", "./vector", "./matrix"], function (t) {
        "use strict";
        function e(t, e, i, r) {
            this.x = t, this.y = e, this.width = i, this.height = r;
        }
        var i = t("./vector"), r = t("./matrix"), n = i.applyTransform, a = Math.min, o = Math.abs, s = Math.max;
        return e.prototype = { constructor: e, union: function (t) {
                var e = a(t.x, this.x), i = a(t.y, this.y);
                this.width = s(t.x + t.width, this.x + this.width) - e, this.height = s(t.y + t.height, this.y + this.height) - i, this.x = e, this.y = i;
            }, applyTransform: function () {
                var t = [], e = [];
                return function (i) {
                    i && (t[0] = this.x, t[1] = this.y, e[0] = this.x + this.width, e[1] = this.y + this.height, n(t, t, i), n(e, e, i), this.x = a(t[0], e[0]), this.y = a(t[1], e[1]), this.width = o(e[0] - t[0]), this.height = o(e[1] - t[1]));
                };
            }(), calculateTransform: function (t) {
                var e = this, i = t.width / e.width, n = t.height / e.height, a = r.create();
                return r.translate(a, a, [-e.x, -e.y]), r.scale(a, a, [i, n]), r.translate(a, a, [t.x, t.y]), a;
            }, intersect: function (t) {
                var e = this, i = e.x, r = e.x + e.width, n = e.y, a = e.y + e.height, o = t.x, s = t.x + t.width, l = t.y, h = t.y + t.height;
                return !(o > r || i > s || l > a || n > h);
            }, contain: function (t, e) {
                var i = this;
                return t >= i.x && t <= i.x + i.width && e >= i.y && e <= i.y + i.height;
            }, clone: function () {
                return new e(this.x, this.y, this.width, this.height);
            }, copy: function (t) {
                this.x = t.x, this.y = t.y, this.width = t.width, this.height = t.height;
            } }, e;
    }), e("echarts/scale/Scale", ["require", "../util/clazz"], function (t) {
        function e() {
            this._extent = [1 / 0, -1 / 0], this._interval = 0, this.init && this.init.apply(this, arguments);
        }
        var i = t("../util/clazz"), r = e.prototype;
        return r.parse = function (t) {
            return t;
        }, r.contain = function (t) {
            var e = this._extent;
            return t >= e[0] && t <= e[1];
        }, r.normalize = function (t) {
            var e = this._extent;
            return e[1] === e[0] ? .5 : (t - e[0]) / (e[1] - e[0]);
        }, r.scale = function (t) {
            var e = this._extent;
            return t * (e[1] - e[0]) + e[0];
        }, r.unionExtent = function (t) {
            var e = this._extent;
            t[0] < e[0] && (e[0] = t[0]), t[1] > e[1] && (e[1] = t[1]);
        }, r.getExtent = function () {
            return this._extent.slice();
        }, r.setExtent = function (t, e) {
            var i = this._extent;
            isNaN(t) || (i[0] = t), isNaN(e) || (i[1] = e);
        }, r.getTicksLabels = function () {
            for (var t = [], e = this.getTicks(), i = 0; i < e.length; i++)
                t.push(this.getLabel(e[i]));
            return t;
        }, i.enableClassExtend(e), i.enableClassManagement(e, { registerWhenExtend: !0 }), e;
    }), e("zrender/core/vector", [], function () {
        var t = "undefined" == typeof Float32Array ? Array : Float32Array, e = { create: function (e, i) {
                var r = new t(2);
                return null == e && (e = 0), null == i && (i = 0), r[0] = e, r[1] = i, r;
            }, copy: function (t, e) {
                return t[0] = e[0], t[1] = e[1], t;
            }, clone: function (e) {
                var i = new t(2);
                return i[0] = e[0], i[1] = e[1], i;
            }, set: function (t, e, i) {
                return t[0] = e, t[1] = i, t;
            }, add: function (t, e, i) {
                return t[0] = e[0] + i[0], t[1] = e[1] + i[1], t;
            }, scaleAndAdd: function (t, e, i, r) {
                return t[0] = e[0] + i[0] * r, t[1] = e[1] + i[1] * r, t;
            }, sub: function (t, e, i) {
                return t[0] = e[0] - i[0], t[1] = e[1] - i[1], t;
            }, len: function (t) {
                return Math.sqrt(this.lenSquare(t));
            }, lenSquare: function (t) {
                return t[0] * t[0] + t[1] * t[1];
            }, mul: function (t, e, i) {
                return t[0] = e[0] * i[0], t[1] = e[1] * i[1], t;
            }, div: function (t, e, i) {
                return t[0] = e[0] / i[0], t[1] = e[1] / i[1], t;
            }, dot: function (t, e) {
                return t[0] * e[0] + t[1] * e[1];
            }, scale: function (t, e, i) {
                return t[0] = e[0] * i, t[1] = e[1] * i, t;
            }, normalize: function (t, i) {
                var r = e.len(i);
                return 0 === r ? (t[0] = 0, t[1] = 0) : (t[0] = i[0] / r, t[1] = i[1] / r), t;
            }, distance: function (t, e) {
                return Math.sqrt((t[0] - e[0]) * (t[0] - e[0]) + (t[1] - e[1]) * (t[1] - e[1]));
            }, distanceSquare: function (t, e) {
                return (t[0] - e[0]) * (t[0] - e[0]) + (t[1] - e[1]) * (t[1] - e[1]);
            }, negate: function (t, e) {
                return t[0] = -e[0], t[1] = -e[1], t;
            }, lerp: function (t, e, i, r) {
                return t[0] = e[0] + r * (i[0] - e[0]), t[1] = e[1] + r * (i[1] - e[1]), t;
            }, applyTransform: function (t, e, i) {
                var r = e[0], n = e[1];
                return t[0] = i[0] * r + i[2] * n + i[4], t[1] = i[1] * r + i[3] * n + i[5], t;
            }, min: function (t, e, i) {
                return t[0] = Math.min(e[0], i[0]), t[1] = Math.min(e[1], i[1]), t;
            }, max: function (t, e, i) {
                return t[0] = Math.max(e[0], i[0]), t[1] = Math.max(e[1], i[1]), t;
            } };
        return e.length = e.len, e.lengthSquare = e.lenSquare, e.dist = e.distance, e.distSquare = e.distanceSquare, e;
    }), e("zrender/core/PathProxy", ["require", "./curve", "./vector", "./bbox", "./BoundingRect", "../config"], function (t) {
        "use strict";
        var e = t("./curve"), i = t("./vector"), r = t("./bbox"), n = t("./BoundingRect"), a = t("../config").devicePixelRatio, o = { M: 1, L: 2, C: 3, Q: 4, A: 5, Z: 6, R: 7 }, s = [], l = [], h = [], u = [], c = Math.min, f = Math.max, d = Math.cos, p = Math.sin, g = Math.sqrt, v = Math.abs, m = "undefined" != typeof Float32Array, y = function () {
            this.data = [], this._len = 0, this._ctx = null, this._xi = 0, this._yi = 0, this._x0 = 0, this._y0 = 0, this._ux = 0, this._uy = 0;
        };
        return y.prototype = { constructor: y, _lineDash: null, _dashOffset: 0, _dashIdx: 0, _dashSum: 0, setScale: function (t, e) {
                this._ux = v(1 / a / t) || 0, this._uy = v(1 / a / e) || 0;
            }, getContext: function () {
                return this._ctx;
            }, beginPath: function (t) {
                return this._ctx = t, t && t.beginPath(), this._len = 0, this._lineDash && (this._lineDash = null, this._dashOffset = 0), this;
            }, moveTo: function (t, e) {
                return this.addData(o.M, t, e), this._ctx && this._ctx.moveTo(t, e), this._x0 = t, this._y0 = e, this._xi = t, this._yi = e, this;
            }, lineTo: function (t, e) {
                var i = v(t - this._xi) > this._ux || v(e - this._yi) > this._uy || this._len < 5;
                return this.addData(o.L, t, e), this._ctx && i && (this._needsDash() ? this._dashedLineTo(t, e) : this._ctx.lineTo(t, e)), i && (this._xi = t, this._yi = e), this;
            }, bezierCurveTo: function (t, e, i, r, n, a) {
                return this.addData(o.C, t, e, i, r, n, a), this._ctx && (this._needsDash() ? this._dashedBezierTo(t, e, i, r, n, a) : this._ctx.bezierCurveTo(t, e, i, r, n, a)), this._xi = n, this._yi = a, this;
            }, quadraticCurveTo: function (t, e, i, r) {
                return this.addData(o.Q, t, e, i, r), this._ctx && (this._needsDash() ? this._dashedQuadraticTo(t, e, i, r) : this._ctx.quadraticCurveTo(t, e, i, r)), this._xi = i, this._yi = r, this;
            }, arc: function (t, e, i, r, n, a) {
                return this.addData(o.A, t, e, i, i, r, n - r, 0, a ? 0 : 1), this._ctx && this._ctx.arc(t, e, i, r, n, a), this._xi = d(n) * i + t, this._xi = p(n) * i + t, this;
            }, arcTo: function (t, e, i, r, n) {
                return this._ctx && this._ctx.arcTo(t, e, i, r, n), this;
            }, rect: function (t, e, i, r) {
                return this._ctx && this._ctx.rect(t, e, i, r), this.addData(o.R, t, e, i, r), this;
            }, closePath: function () {
                this.addData(o.Z);
                var t = this._ctx, e = this._x0, i = this._y0;
                return t && (this._needsDash() && this._dashedLineTo(e, i), t.closePath()), this._xi = e, this._yi = i, this;
            }, fill: function (t) {
                t && t.fill(), this.toStatic();
            }, stroke: function (t) {
                t && t.stroke(), this.toStatic();
            }, setLineDash: function (t) {
                if (t instanceof Array) {
                    this._lineDash = t, this._dashIdx = 0;
                    for (var e = 0, i = 0; i < t.length; i++)
                        e += t[i];
                    this._dashSum = e;
                }
                return this;
            }, setLineDashOffset: function (t) {
                return this._dashOffset = t, this;
            }, len: function () {
                return this._len;
            }, setData: function (t) {
                var e = t.length;
                this.data && this.data.length == e || !m || (this.data = new Float32Array(e));
                for (var i = 0; e > i; i++)
                    this.data[i] = t[i];
                this._len = e;
            }, appendPath: function (t) {
                t instanceof Array || (t = [t]);
                for (var e = t.length, i = 0, r = this._len, n = 0; e > n; n++)
                    i += t[n].len();
                m && this.data instanceof Float32Array && (this.data = new Float32Array(r + i));
                for (var n = 0; e > n; n++)
                    for (var a = t[n].data, o = 0; o < a.length; o++)
                        this.data[r++] = a[o];
                this._len = r;
            }, addData: function (t) {
                var e = this.data;
                this._len + arguments.length > e.length && (this._expandData(), e = this.data);
                for (var i = 0; i < arguments.length; i++)
                    e[this._len++] = arguments[i];
                this._prevCmd = t;
            }, _expandData: function () {
                if (!(this.data instanceof Array)) {
                    for (var t = [], e = 0; e < this._len; e++)
                        t[e] = this.data[e];
                    this.data = t;
                }
            }, _needsDash: function () {
                return this._lineDash;
            }, _dashedLineTo: function (t, e) {
                var i, r, n = this._dashSum, a = this._dashOffset, o = this._lineDash, s = this._ctx, l = this._xi, h = this._yi, u = t - l, d = e - h, p = g(u * u + d * d), v = l, m = h, y = o.length;
                for (u /= p, d /= p, 0 > a && (a = n + a), a %= n, v -= a * u, m -= a * d; u > 0 && t >= v || 0 > u && v >= t || 0 == u && (d > 0 && e >= m || 0 > d && m >= e);)
                    r = this._dashIdx, i = o[r], v += u * i, m += d * i, this._dashIdx = (r + 1) % y, u > 0 && l > v || 0 > u && v > l || d > 0 && h > m || 0 > d && m > h || s[r % 2 ? "moveTo" : "lineTo"](u >= 0 ? c(v, t) : f(v, t), d >= 0 ? c(m, e) : f(m, e));
                u = v - t, d = m - e, this._dashOffset = -g(u * u + d * d);
            }, _dashedBezierTo: function (t, i, r, n, a, o) {
                var s, l, h, u, c, f = this._dashSum, d = this._dashOffset, p = this._lineDash, v = this._ctx, m = this._xi, y = this._yi, _ = e.cubicAt, x = 0, b = this._dashIdx, w = p.length, T = 0;
                for (0 > d && (d = f + d), d %= f, s = 0; 1 > s; s += .1)
                    l = _(m, t, r, a, s + .1) - _(m, t, r, a, s), h = _(y, i, n, o, s + .1) - _(y, i, n, o, s), x += g(l * l + h * h);
                for (; w > b && (T += p[b], !(T > d)); b++)
                    ;
                for (s = (T - d) / x; 1 >= s;)
                    u = _(m, t, r, a, s), c = _(y, i, n, o, s), b % 2 ? v.moveTo(u, c) : v.lineTo(u, c), s += p[b] / x, b = (b + 1) % w;
                b % 2 !== 0 && v.lineTo(a, o), l = a - u, h = o - c, this._dashOffset = -g(l * l + h * h);
            }, _dashedQuadraticTo: function (t, e, i, r) {
                var n = i, a = r;
                i = (i + 2 * t) / 3, r = (r + 2 * e) / 3, t = (this._xi + 2 * t) / 3, e = (this._yi + 2 * e) / 3, this._dashedBezierTo(t, e, i, r, n, a);
            }, toStatic: function () {
                var t = this.data;
                t instanceof Array && (t.length = this._len, m && (this.data = new Float32Array(t)));
            }, getBoundingRect: function () {
                s[0] = s[1] = h[0] = h[1] = Number.MAX_VALUE, l[0] = l[1] = u[0] = u[1] = -Number.MAX_VALUE;
                for (var t = this.data, e = 0, a = 0, c = 0, f = 0, g = 0; g < t.length;) {
                    var v = t[g++];
                    switch (1 == g && (e = t[g], a = t[g + 1], c = e, f = a), v) {
                        case o.M:
                            c = t[g++], f = t[g++], e = c, a = f, h[0] = c, h[1] = f, u[0] = c, u[1] = f;
                            break;
                        case o.L:
                            r.fromLine(e, a, t[g], t[g + 1], h, u), e = t[g++], a = t[g++];
                            break;
                        case o.C:
                            r.fromCubic(e, a, t[g++], t[g++], t[g++], t[g++], t[g], t[g + 1], h, u), e = t[g++], a = t[g++];
                            break;
                        case o.Q:
                            r.fromQuadratic(e, a, t[g++], t[g++], t[g], t[g + 1], h, u), e = t[g++], a = t[g++];
                            break;
                        case o.A:
                            var m = t[g++], y = t[g++], _ = t[g++], x = t[g++], b = t[g++], w = t[g++] + b, T = (t[g++], 1 - t[g++]);
                            1 == g && (c = d(b) * _ + m, f = p(b) * x + y), r.fromArc(m, y, _, x, b, w, T, h, u), e = d(w) * _ + m, a = p(w) * x + y;
                            break;
                        case o.R:
                            c = e = t[g++], f = a = t[g++];
                            var M = t[g++], z = t[g++];
                            r.fromLine(c, f, c + M, f + z, h, u);
                            break;
                        case o.Z:
                            e = c, a = f;
                    }
                    i.min(s, s, h), i.max(l, l, u);
                }
                return 0 === g && (s[0] = s[1] = l[0] = l[1] = 0), new n(s[0], s[1], l[0] - s[0], l[1] - s[1]);
            }, rebuildPath: function (t) {
                for (var e, i, r, n, a, s, l = this.data, h = this._ux, u = this._uy, c = this._len, f = 0; c > f;) {
                    var g = l[f++];
                    switch (1 == f && (r = l[f], n = l[f + 1], e = r, i = n), g) {
                        case o.M:
                            e = r = l[f++], i = n = l[f++], t.moveTo(r, n);
                            break;
                        case o.L:
                            a = l[f++], s = l[f++], (v(a - r) > h || v(s - n) > u || f === c - 1) && (t.lineTo(a, s), r = a, n = s);
                            break;
                        case o.C:
                            t.bezierCurveTo(l[f++], l[f++], l[f++], l[f++], l[f++], l[f++]), r = l[f - 2], n = l[f - 1];
                            break;
                        case o.Q:
                            t.quadraticCurveTo(l[f++], l[f++], l[f++], l[f++]), r = l[f - 2], n = l[f - 1];
                            break;
                        case o.A:
                            var m = l[f++], y = l[f++], _ = l[f++], x = l[f++], b = l[f++], w = l[f++], T = l[f++], M = l[f++], z = _ > x ? _ : x, S = _ > x ? 1 : _ / x, C = _ > x ? x / _ : 1, P = Math.abs(_ - x) > .001, A = b + w;
                            P ? (t.translate(m, y), t.rotate(T), t.scale(S, C), t.arc(0, 0, z, b, A, 1 - M), t.scale(1 / S, 1 / C), t.rotate(-T), t.translate(-m, -y)) : t.arc(m, y, z, b, A, 1 - M), 1 == f && (e = d(b) * _ + m, i = p(b) * x + y), r = d(A) * _ + m, n = p(A) * x + y;
                            break;
                        case o.R:
                            e = r = l[f], i = n = l[f + 1], t.rect(l[f++], l[f++], l[f++], l[f++]);
                            break;
                        case o.Z:
                            t.closePath(), r = e, n = i;
                    }
                }
            } }, y.CMD = o, y;
    }), e("zrender/contain/text", ["require", "../core/util", "../core/BoundingRect"], function (t) {
        function e(t, e) {
            var i = t + ":" + e;
            if (o[i])
                return o[i];
            for (var r = (t + "").split("\n"), n = 0, a = 0, h = r.length; h > a; a++)
                n = Math.max(f.measureText(r[a], e).width, n);
            return s > l && (s = 0, o = {}), s++, o[i] = n, n;
        }
        function i(t, i, r, n) {
            var a = ((t || "") + "").split("\n").length, o = e(t, i), s = e("国", i), l = a * s, h = new u(0, 0, o, l);
            switch (h.lineHeight = s, n) {
                case "bottom":
                case "alphabetic":
                    h.y -= s;
                    break;
                case "middle":
                    h.y -= s / 2;
            }
            switch (r) {
                case "end":
                case "right":
                    h.x -= h.width;
                    break;
                case "center":
                    h.x -= h.width / 2;
            }
            return h;
        }
        function r(t, e, i, r) {
            var n = e.x, a = e.y, o = e.height, s = e.width, l = i.height, h = o / 2 - l / 2, u = "left";
            switch (t) {
                case "left":
                    n -= r, a += h, u = "right";
                    break;
                case "right":
                    n += r + s, a += h, u = "left";
                    break;
                case "top":
                    n += s / 2, a -= r + l, u = "center";
                    break;
                case "bottom":
                    n += s / 2, a += o + r, u = "center";
                    break;
                case "inside":
                    n += s / 2, a += h, u = "center";
                    break;
                case "insideLeft":
                    n += r, a += h, u = "left";
                    break;
                case "insideRight":
                    n += s - r, a += h, u = "right";
                    break;
                case "insideTop":
                    n += s / 2, a += r, u = "center";
                    break;
                case "insideBottom":
                    n += s / 2, a += o - l - r, u = "center";
                    break;
                case "insideTopLeft":
                    n += r, a += r, u = "left";
                    break;
                case "insideTopRight":
                    n += s - r, a += r, u = "right";
                    break;
                case "insideBottomLeft":
                    n += r, a += o - l - r;
                    break;
                case "insideBottomRight":
                    n += s - r, a += o - l - r, u = "right";
            }
            return { x: n, y: a, textAlign: u, textBaseline: "top" };
        }
        function n(t, i, r, n, o) {
            if (!i)
                return "";
            o = o || {}, n = c(n, "...");
            for (var s = c(o.maxIterations, 2), l = c(o.minChar, 0), h = e("国", r), u = e("a", r), f = c(o.placeholder, ""), d = i = Math.max(0, i - 1), p = 0; l > p && d >= u; p++)
                d -= u;
            var g = e(n);
            g > d && (n = "", g = 0), d = i - g;
            for (var v = (t + "").split("\n"), p = 0, m = v.length; m > p; p++) {
                var y = v[p], _ = e(y, r);
                if (!(i >= _)) {
                    for (var x = 0; ; x++) {
                        if (d >= _ || x >= s) {
                            y += n;
                            break;
                        }
                        var b = 0 === x ? a(y, d, u, h) : _ > 0 ? Math.floor(y.length * d / _) : 0;
                        y = y.substr(0, b), _ = e(y, r);
                    }
                    "" === y && (y = f), v[p] = y;
                }
            }
            return v.join("\n");
        }
        function a(t, e, i, r) {
            for (var n = 0, a = 0, o = t.length; o > a && e > n; a++) {
                var s = t.charCodeAt(a);
                n += s >= 0 && 127 >= s ? i : r;
            }
            return a;
        }
        var o = {}, s = 0, l = 5e3, h = t("../core/util"), u = t("../core/BoundingRect"), c = h.retrieve, f = { getWidth: e, getBoundingRect: i, adjustTextPositionOnRect: r, truncateText: n, measureText: function (t, e) {
                var i = h.getContext();
                return i.font = e || "12px sans-serif", i.measureText(t);
            } };
        return f;
    }), e("zrender/graphic/mixin/RectText", ["require", "../../contain/text", "../../core/BoundingRect"], function (t) {
        function e(t, e) {
            return "string" == typeof t ? t.lastIndexOf("%") >= 0 ? parseFloat(t) / 100 * e : parseFloat(t) : t;
        }
        var i = t("../../contain/text"), r = t("../../core/BoundingRect"), n = new r, a = function () {
        };
        return a.prototype = { constructor: a, drawRectText: function (t, r, a) {
                var o = this.style, s = o.text;
                if (null != s && (s += ""), s) {
                    t.save();
                    var l, h, u = o.textPosition, c = o.textDistance, f = o.textAlign, d = o.textFont || o.font, p = o.textBaseline, g = o.textVerticalAlign;
                    a = a || i.getBoundingRect(s, d, f, p);
                    var v = this.transform;
                    if (v && (n.copy(r), n.applyTransform(v), r = n), u instanceof Array) {
                        if (l = r.x + e(u[0], r.width), h = r.y + e(u[1], r.height), f = f || "left", p = p || "top", g) {
                            switch (g) {
                                case "middle":
                                    h -= a.height / 2 - a.lineHeight / 2;
                                    break;
                                case "bottom":
                                    h -= a.height - a.lineHeight / 2;
                                    break;
                                default:
                                    h += a.lineHeight / 2;
                            }
                            p = "middle";
                        }
                    } else {
                        var m = i.adjustTextPositionOnRect(u, r, a, c);
                        l = m.x, h = m.y, f = f || m.textAlign, p = p || m.textBaseline;
                    }
                    t.textAlign = f || "left", t.textBaseline = p || "alphabetic";
                    var y = o.textFill, _ = o.textStroke;
                    y && (t.fillStyle = y), _ && (t.strokeStyle = _), t.font = d, t.shadowBlur = o.textShadowBlur, t.shadowColor = o.textShadowColor || "transparent", t.shadowOffsetX = o.textShadowOffsetX, t.shadowOffsetY = o.textShadowOffsetY;
                    for (var x = s.split("\n"), b = 0; b < x.length; b++)
                        y && t.fillText(x[b], l, h), _ && t.strokeText(x[b], l, h), h += a.lineHeight;
                    t.restore();
                }
            } }, a;
    }), e("zrender/graphic/Displayable", ["require", "../core/util", "./Style", "../Element", "./mixin/RectText"], function (t) {
        function e(t) {
            t = t || {}, n.call(this, t);
            for (var e in t)
                t.hasOwnProperty(e) && "style" !== e && (this[e] = t[e]);
            this.style = new r(t.style), this._rect = null, this.__clipPaths = [];
        }
        var i = t("../core/util"), r = t("./Style"), n = t("../Element"), a = t("./mixin/RectText");
        return e.prototype = { constructor: e, type: "displayable", __dirty: !0, invisible: !1, z: 0, z2: 0, zlevel: 0, draggable: !1, dragging: !1, silent: !1, culling: !1, cursor: "pointer", rectHover: !1, progressive: -1, beforeBrush: function () {
            }, afterBrush: function () {
            }, brush: function () {
            }, getBoundingRect: function () {
            }, contain: function (t, e) {
                return this.rectContain(t, e);
            }, traverse: function (t, e) {
                t.call(e, this);
            }, rectContain: function (t, e) {
                var i = this.transformCoordToLocal(t, e), r = this.getBoundingRect();
                return r.contain(i[0], i[1]);
            }, dirty: function () {
                this.__dirty = !0, this._rect = null, this.__zr && this.__zr.refresh();
            }, animateStyle: function (t) {
                return this.animate("style", t);
            }, attrKV: function (t, e) {
                "style" !== t ? n.prototype.attrKV.call(this, t, e) : this.style.set(e);
            }, setStyle: function (t, e) {
                return this.style.set(t, e), this.dirty(!1), this;
            }, useStyle: function (t) {
                return this.style = new r(t), this.dirty(!1), this;
            } }, i.inherits(e, n), i.mixin(e, a), e;
    }), e("zrender/graphic/Image", ["require", "./Displayable", "../core/BoundingRect", "../core/util", "../core/LRU"], function (t) {
        function e(t) {
            i.call(this, t);
        }
        var i = t("./Displayable"), r = t("../core/BoundingRect"), n = t("../core/util"), a = t("../core/LRU"), o = new a(50);
        return e.prototype = { constructor: e, type: "image", brush: function (t, e) {
                var i, r = this.style, n = r.image;
                if (r.bind(t, this, e), i = "string" == typeof n ? this._image : n, !i && n) {
                    var a = o.get(n);
                    if (!a)
                        return i = new Image, i.onload = function () {
                            i.onload = null;
                            for (var t = 0; t < a.pending.length; t++)
                                a.pending[t].dirty();
                        }, a = { image: i, pending: [this] }, i.src = n, o.put(n, a), void (this._image = i);
                    if (i = a.image, this._image = i, !i.width || !i.height)
                        return void a.pending.push(this);
                }
                if (i) {
                    var s = r.width || i.width, l = r.height || i.height, h = r.x || 0, u = r.y || 0;
                    if (!i.width || !i.height)
                        return;
                    if (this.setTransform(t), r.sWidth && r.sHeight) {
                        var c = r.sx || 0, f = r.sy || 0;
                        t.drawImage(i, c, f, r.sWidth, r.sHeight, h, u, s, l);
                    } else if (r.sx && r.sy) {
                        var c = r.sx, f = r.sy, d = s - c, p = l - f;
                        t.drawImage(i, c, f, d, p, h, u, s, l);
                    } else
                        t.drawImage(i, h, u, s, l);
                    null == r.width && (r.width = s), null == r.height && (r.height = l), this.restoreTransform(t), null != r.text && this.drawRectText(t, this.getBoundingRect());
                }
            }, getBoundingRect: function () {
                var t = this.style;
                return this._rect || (this._rect = new r(t.x || 0, t.y || 0, t.width || 0, t.height || 0)), this._rect;
            } }, n.inherits(e, i), e;
    }), e("zrender/graphic/Text", ["require", "./Displayable", "../core/util", "../contain/text"], function (t) {
        var e = t("./Displayable"), i = t("../core/util"), r = t("../contain/text"), n = function (t) {
            e.call(this, t);
        };
        return n.prototype = { constructor: n, type: "text", brush: function (t, e) {
                var i = this.style, n = i.x || 0, a = i.y || 0, o = i.text;
                if (null != o && (o += ""), i.bind(t, this, e), o) {
                    this.setTransform(t);
                    var s, l = i.textAlign, h = i.textFont || i.font;
                    if (i.textVerticalAlign) {
                        var u = r.getBoundingRect(o, h, i.textAlign, "top");
                        switch (s = "middle", i.textVerticalAlign) {
                            case "middle":
                                a -= u.height / 2 - u.lineHeight / 2;
                                break;
                            case "bottom":
                                a -= u.height - u.lineHeight / 2;
                                break;
                            default:
                                a += u.lineHeight / 2;
                        }
                    } else
                        s = i.textBaseline;
                    t.font = h, t.textAlign = l || "left", t.textAlign !== l && (t.textAlign = "left"), t.textBaseline = s || "alphabetic", t.textBaseline !== s && (t.textBaseline = "alphabetic");
                    for (var c = r.measureText("国", t.font).width, f = o.split("\n"), d = 0; d < f.length; d++)
                        i.hasFill() && t.fillText(f[d], n, a), i.hasStroke() && t.strokeText(f[d], n, a), a += c;
                    this.restoreTransform(t);
                }
            }, getBoundingRect: function () {
                if (!this._rect) {
                    var t = this.style, e = t.textVerticalAlign, i = r.getBoundingRect(t.text + "", t.textFont || t.font, t.textAlign, e ? "top" : t.textBaseline);
                    switch (e) {
                        case "middle":
                            i.y -= i.height / 2;
                            break;
                        case "bottom":
                            i.y -= i.height;
                    }
                    i.x += t.x || 0, i.y += t.y || 0, this._rect = i;
                }
                return this._rect;
            } }, i.inherits(n, e), n;
    }), e("zrender/graphic/Path", ["require", "./Displayable", "../core/util", "../core/PathProxy", "../contain/path", "./Pattern"], function (t) {
        function e(t) {
            i.call(this, t), this.path = new n;
        }
        var i = t("./Displayable"), r = t("../core/util"), n = t("../core/PathProxy"), a = t("../contain/path"), o = t("./Pattern"), s = o.prototype.getCanvasPattern, l = Math.abs;
        return e.prototype = { constructor: e, type: "path", __dirtyPath: !0, strokeContainThreshold: 5, brush: function (t, e) {
                var i = this.style, r = this.path, n = i.hasStroke(), a = i.hasFill(), o = i.fill, l = i.stroke, h = a && !!o.colorStops, u = n && !!l.colorStops, c = a && !!o.image, f = n && !!l.image;
                if (i.bind(t, this, e), this.setTransform(t), this.__dirty) {
                    var d = this.getBoundingRect();
                    h && (this._fillGradient = i.getGradient(t, o, d)), u && (this._strokeGradient = i.getGradient(t, l, d));
                }
                h ? t.fillStyle = this._fillGradient : c && (t.fillStyle = s.call(o, t)), u ? t.strokeStyle = this._strokeGradient : f && (t.strokeStyle = s.call(l, t));
                var p = i.lineDash, g = i.lineDashOffset, v = !!t.setLineDash, m = this.getGlobalScale();
                r.setScale(m[0], m[1]), this.__dirtyPath || p && !v && n ? (r = this.path.beginPath(t), p && !v && (r.setLineDash(p), r.setLineDashOffset(g)), this.buildPath(r, this.shape, !1), this.__dirtyPath = !1) : (t.beginPath(), this.path.rebuildPath(t)), a && r.fill(t), p && v && (t.setLineDash(p), t.lineDashOffset = g), n && r.stroke(t), p && v && t.setLineDash([]), this.restoreTransform(t), (i.text || 0 === i.text) && this.drawRectText(t, this.getBoundingRect());
            }, buildPath: function () {
            }, getBoundingRect: function () {
                var t = this._rect, e = this.style, i = !t;
                if (i) {
                    var r = this.path;
                    this.__dirtyPath && (r.beginPath(), this.buildPath(r, this.shape, !1)), t = r.getBoundingRect();
                }
                if (this._rect = t, e.hasStroke()) {
                    var n = this._rectWithStroke || (this._rectWithStroke = t.clone());
                    if (this.__dirty || i) {
                        n.copy(t);
                        var a = e.lineWidth, o = e.strokeNoScale ? this.getLineScale() : 1;
                        e.hasFill() || (a = Math.max(a, this.strokeContainThreshold || 4)), o > 1e-10 && (n.width += a / o, n.height += a / o, n.x -= a / o / 2, n.y -= a / o / 2);
                    }
                    return n;
                }
                return t;
            }, contain: function (t, e) {
                var i = this.transformCoordToLocal(t, e), r = this.getBoundingRect(), n = this.style;
                if (t = i[0], e = i[1], r.contain(t, e)) {
                    var o = this.path.data;
                    if (n.hasStroke()) {
                        var s = n.lineWidth, l = n.strokeNoScale ? this.getLineScale() : 1;
                        if (l > 1e-10 && (n.hasFill() || (s = Math.max(s, this.strokeContainThreshold)), a.containStroke(o, s / l, t, e)))
                            return !0;
                    }
                    if (n.hasFill())
                        return a.contain(o, t, e);
                }
                return !1;
            }, dirty: function (t) {
                null == t && (t = !0), t && (this.__dirtyPath = t, this._rect = null), this.__dirty = !0, this.__zr && this.__zr.refresh(), this.__clipTarget && this.__clipTarget.dirty();
            }, animateShape: function (t) {
                return this.animate("shape", t);
            }, attrKV: function (t, e) {
                "shape" === t ? (this.setShape(e), this.__dirtyPath = !0, this._rect = null) : i.prototype.attrKV.call(this, t, e);
            }, setShape: function (t, e) {
                var i = this.shape;
                if (i) {
                    if (r.isObject(t))
                        for (var n in t)
                            i[n] = t[n];
                    else
                        i[t] = e;
                    this.dirty(!0);
                }
                return this;
            }, getLineScale: function () {
                var t = this.transform;
                return t && l(t[0] - 1) > 1e-10 && l(t[3] - 1) > 1e-10 ? Math.sqrt(l(t[0] * t[3] - t[2] * t[1])) : 1;
            } }, e.extend = function (t) {
            var i = function (i) {
                e.call(this, i), t.style && this.style.extendFrom(t.style, !1);
                var r = t.shape;
                if (r) {
                    this.shape = this.shape || {};
                    var n = this.shape;
                    for (var a in r)
                        !n.hasOwnProperty(a) && r.hasOwnProperty(a) && (n[a] = r[a]);
                }
                t.init && t.init.call(this, i);
            };
            r.inherits(i, e);
            for (var n in t)
                "style" !== n && "shape" !== n && (i.prototype[n] = t[n]);
            return i;
        }, r.inherits(e, i), e;
    }), e("echarts/component/tooltip/TooltipView", ["require", "./TooltipContent", "../../util/graphic", "zrender/core/util", "../../util/format", "../../util/number", "zrender/core/env", "../../model/Model", "../../echarts"], function (t) {
        function e(t, e) {
            if (!t || !e)
                return !1;
            var i = d.round;
            return i(t[0]) === i(e[0]) && i(t[1]) === i(e[1]);
        }
        function i(t, e, i, r) {
            return { x1: t, y1: e, x2: i, y2: r };
        }
        function r(t, e, i, r) {
            return { x: t, y: e, width: i, height: r };
        }
        function n(t, e, i, r, n, a) {
            return { cx: t, cy: e, r0: i, r: r, startAngle: n, endAngle: a, clockwise: !0 };
        }
        function a(t, e, i, r, n) {
            var a = i.clientWidth, o = i.clientHeight, s = 20;
            return t + a + s > r ? t -= a + s : t += s, e + o + s > n ? e -= o + s : e += s, [t, e];
        }
        function o(t, e, i) {
            var r = i.clientWidth, n = i.clientHeight, a = 5, o = 0, s = 0, l = e.width, h = e.height;
            switch (t) {
                case "inside":
                    o = e.x + l / 2 - r / 2, s = e.y + h / 2 - n / 2;
                    break;
                case "top":
                    o = e.x + l / 2 - r / 2, s = e.y - n - a;
                    break;
                case "bottom":
                    o = e.x + l / 2 - r / 2, s = e.y + h + a;
                    break;
                case "left":
                    o = e.x - r - a, s = e.y + h / 2 - n / 2;
                    break;
                case "right":
                    o = e.x + l + a, s = e.y + h / 2 - n / 2;
            }
            return [o, s];
        }
        function s(t, e, i, r, n, s, l) {
            var h = l.getWidth(), u = l.getHeight(), f = s && s.getBoundingRect().clone();
            if (s && f.applyTransform(s.transform), "function" == typeof t && (t = t([e, i], n, r.el, f)), c.isArray(t))
                e = p(t[0], h), i = p(t[1], u);
            else if ("string" == typeof t && s) {
                var d = o(t, f, r.el);
                e = d[0], i = d[1];
            } else {
                var d = a(e, i, r.el, h, u);
                e = d[0], i = d[1];
            }
            r.moveTo(e, i);
        }
        function l(t) {
            var e = t.coordinateSystem, i = t.get("tooltip.trigger", !0);
            return !(!e || "cartesian2d" !== e.type && "polar" !== e.type && "singleAxis" !== e.type || "item" === i);
        }
        var h = t("./TooltipContent"), u = t("../../util/graphic"), c = t("zrender/core/util"), f = t("../../util/format"), d = t("../../util/number"), p = d.parsePercent, g = t("zrender/core/env"), v = t("../../model/Model");
        t("../../echarts").extendComponentView({
            type: "tooltip", _axisPointers: {}, init: function (t, e) {
                if (!g.node) {
                    var i = new h(e.getDom(), e);
                    this._tooltipContent = i, e.on("showTip", this._manuallyShowTip, this), e.on("hideTip", this._manuallyHideTip, this);
                }
            }, render: function (t, e, i) {
                if (!g.node) {
                    this.group.removeAll(), this._axisPointers = {}, this._tooltipModel = t, this._ecModel = e, this._api = i, this._lastHover = {};
                    var r = this._tooltipContent;
                    r.update(), r.enterable = t.get("enterable"), this._alwaysShowContent = t.get("alwaysShowContent"), this._seriesGroupByAxis = this._prepareAxisTriggerData(t, e);
                    var n = this._crossText;
                    if (n && this.group.add(n), null != this._lastX && null != this._lastY) {
                        var a = this;
                        clearTimeout(this._refreshUpdateTimeout), this._refreshUpdateTimeout = setTimeout(function () {
                            a._manuallyShowTip({ x: a._lastX, y: a._lastY });
                        });
                    }
                    var o = this._api.getZr();
                    o.off("click", this._tryShow), o.off("mousemove", this._mousemove), o.off("mouseout", this._hide), o.off("globalout", this._hide), "click" === t.get("triggerOn") ? o.on("click", this._tryShow, this) : (o.on("mousemove", this._mousemove, this), o.on("mouseout", this._hide, this), o.on("globalout", this._hide, this));
                }
            }, _mousemove: function (t) {
                var e = this._tooltipModel.get("showDelay"), i = this;
                clearTimeout(this._showTimeout), e > 0 ? this._showTimeout = setTimeout(function () {
                    i._tryShow(t);
                }, e) : this._tryShow(t);
            }, _manuallyShowTip: function (t) {
                if (t.from !== this.uid) {
                    var e = this._ecModel, i = t.seriesIndex, r = t.dataIndex, n = e.getSeriesByIndex(i), a = this._api;
                    if (null == t.x || null == t.y) {
                        if (n || e.eachSeries(function (t) {
                            l(t) && !n && (n = t);
                        }), n) {
                            var o = n.getData();
                            null == r && (r = o.indexOfName(t.name));
                            var s, h, u = o.getItemGraphicEl(r), f = n.coordinateSystem;
                            if (f && f.dataToPoint) {
                                var d = f.dataToPoint(o.getValues(c.map(f.dimensions, function (t) {
                                    return n.coordDimToDataDim(t)[0];
                                }), r, !0));
                                s = d && d[0], h = d && d[1];
                            } else if (u) {
                                var p = u.getBoundingRect().clone();
                                p.applyTransform(u.transform), s = p.x + p.width / 2, h = p.y + p.height / 2;
                            }
                            null != s && null != h && this._tryShow({ offsetX: s, offsetY: h, target: u, event: {} });
                        }
                    } else {
                        var u = a.getZr().handler.findHover(t.x, t.y);
                        this._tryShow({ offsetX: t.x, offsetY: t.y, target: u, event: {} });
                    }
                }
            }, _manuallyHideTip: function (t) {
                t.from !== this.uid && this._hide();
            }, _prepareAxisTriggerData: function (t, e) {
                var i = {};
                return e.eachSeries(function (t) {
                    if (l(t)) {
                        var e, r, n = t.coordinateSystem;
                        "cartesian2d" === n.type ? (e = n.getBaseAxis(), r = e.dim + e.index) : "singleAxis" === n.type ? (e = n.getAxis(), r = e.dim + e.type) : (e = n.getBaseAxis(), r = e.dim + n.name), i[r] = i[r] || { coordSys: [], series: [] }, i[r].coordSys.push(n), i[r].series.push(t);
                    }
                }, this), i;
            }, _tryShow: function (t) {
                var e = t.target, i = this._tooltipModel, r = i.get("trigger"), n = this._ecModel, a = this._api;
                if (i)
                    if (this._lastX = t.offsetX, this._lastY = t.offsetY, e && null != e.dataIndex) {
                        var o = e.dataModel || n.getSeriesByIndex(e.seriesIndex), s = e.dataIndex, l = o.getData().getItemModel(s);
                        "axis" === (l.get("tooltip.trigger") || r) ? this._showAxisTooltip(i, n, t) : (this._ticket = "", this._hideAxisPointer(), this._resetLastHover(), this._showItemTooltipContent(o, s, e.dataType, t)), a.dispatchAction({ type: "showTip", from: this.uid, dataIndex: e.dataIndex, seriesIndex: e.seriesIndex });
                    } else if (e && e.tooltip) {
                        var h = e.tooltip;
                        if ("string" == typeof h) {
                            var u = h;
                            h = { content: u, formatter: u };
                        }
                        var c = new v(h, i), f = c.get("content"), d = Math.random();
                        this._showTooltipContent(c, f, c.get("formatterParams") || {}, d, t.offsetX, t.offsetY, e, a);
                    } else
                        "item" === r ? this._hide() : this._showAxisTooltip(i, n, t), "cross" === i.get("axisPointer.type") && a.dispatchAction({ type: "showTip", from: this.uid, x: t.offsetX, y: t.offsetY });
            }, _showAxisTooltip: function (t, i, r) {
                var n = t.getModel("axisPointer"), a = n.get("type");
                if ("cross" === a) {
                    var o = r.target;
                    if (o && null != o.dataIndex) {
                        var s = i.getSeriesByIndex(o.seriesIndex), l = o.dataIndex;
                        this._showItemTooltipContent(s, l, o.dataType, r);
                    }
                }
                this._showAxisPointer();
                var h = !0;
                c.each(this._seriesGroupByAxis, function (t) {
                    var i = t.coordSys, o = i[0], s = [r.offsetX, r.offsetY];
                    if (!o.containPoint(s))
                        return void this._hideAxisPointer(o.name);
                    h = !1;
                    var l = o.dimensions, u = o.pointToData(s, !0);
                    s = o.dataToPoint(u);
                    var f = o.getBaseAxis(), d = n.get("axis");
                    "auto" === d && (d = f.dim);
                    var p = !1, g = this._lastHover;
                    if ("cross" === a)
                        e(g.data, u) && (p = !0), g.data = u;
                    else {
                        var v = c.indexOf(l, d);
                        g.data === u[v] && (p = !0), g.data = u[v];
                    }
                    "cartesian2d" !== o.type || p ? "polar" !== o.type || p ? "singleAxis" !== o.type || p || this._showSinglePointer(n, o, d, s) : this._showPolarPointer(n, o, d, s) : this._showCartesianPointer(n, o, d, s), "cross" !== a && this._dispatchAndShowSeriesTooltipContent(o, t.series, s, u, p);
                }, this), this._tooltipModel.get("show") || this._hideAxisPointer(), h && this._hide();
            }, _showCartesianPointer: function (t, e, n, a) {
                function o(r, n, a) {
                    var o = "x" === r ? i(n[0], a[0], n[0], a[1]) : i(a[0], n[1], a[1], n[1]), s = l._getPointerElement(e, t, r, o);
                    u.subPixelOptimizeLine({ shape: o, style: s.style }), f ? u.updateProps(s, { shape: o }, t) : s.attr({ shape: o });
                }
                function s(i, n, a) {
                    var o = e.getAxis(i), s = o.getBandWidth(), h = a[1] - a[0], c = "x" === i ? r(n[0] - s / 2, a[0], s, h) : r(a[0], n[1] - s / 2, h, s), d = l._getPointerElement(e, t, i, c);
                    f ? u.updateProps(d, { shape: c }, t) : d.attr({ shape: c });
                }
                var l = this, h = t.get("type"), c = e.getBaseAxis(), f = "cross" !== h && "category" === c.type && c.getBandWidth() > 20;
                if ("cross" === h)
                    o("x", a, e.getAxis("y").getGlobalExtent()), o("y", a, e.getAxis("x").getGlobalExtent()), this._updateCrossText(e, a, t);
                else {
                    var d = e.getAxis("x" === n ? "y" : "x"), p = d.getGlobalExtent();
                    "cartesian2d" === e.type && ("line" === h ? o : s)(n, a, p);
                }
            }, _showSinglePointer: function (t, e, r, n) {
                function a(r, n, a) {
                    var s = e.getAxis(), h = s.orient, c = "horizontal" === h ? i(n[0], a[0], n[0], a[1]) : i(a[0], n[1], a[1], n[1]), f = o._getPointerElement(e, t, r, c);
                    l ? u.updateProps(f, { shape: c }, t) : f.attr({ shape: c });
                }
                var o = this, s = t.get("type"), l = "cross" !== s && "category" === e.getBaseAxis().type, h = e.getRect(), c = [h.y, h.y + h.height];
                a(r, n, c);
            }, _showPolarPointer: function (t, e, r, a) {
                function o(r, n, a) {
                    var o, s = e.pointToCoord(n);
                    if ("angle" === r) {
                        var h = e.coordToPoint([a[0], s[1]]), c = e.coordToPoint([a[1], s[1]]);
                        o = i(h[0], h[1], c[0], c[1]);
                    } else
                        o = { cx: e.cx, cy: e.cy, r: s[0] };
                    var f = l._getPointerElement(e, t, r, o);
                    d ? u.updateProps(f, { shape: o }, t) : f.attr({ shape: o });
                }
                function s(i, r, a) {
                    var o, s = e.getAxis(i), h = s.getBandWidth(), c = e.pointToCoord(r), f = Math.PI / 180;
                    o = "angle" === i ? n(e.cx, e.cy, a[0], a[1], (-c[1] - h / 2) * f, (-c[1] + h / 2) * f) : n(e.cx, e.cy, c[0] - h / 2, c[0] + h / 2, 0, 2 * Math.PI);
                    var p = l._getPointerElement(e, t, i, o);
                    d ? u.updateProps(p, { shape: o }, t) : p.attr({ shape: o });
                }
                var l = this, h = t.get("type"), c = e.getAngleAxis(), f = e.getRadiusAxis(), d = "cross" !== h && "category" === e.getBaseAxis().type;
                if ("cross" === h)
                    o("angle", a, f.getExtent()), o("radius", a, c.getExtent()), this._updateCrossText(e, a, t);
                else {
                    var p = e.getAxis("radius" === r ? "angle" : "radius"), g = p.getExtent();
                    ("line" === h ? o : s)(r, a, g);
                }
            }, _updateCrossText: function (t, e, i) {
                var r = i.getModel("crossStyle"), n = r.getModel("textStyle"), a = this._tooltipModel, o = this._crossText;
                o || (o = this._crossText = new u.Text({ style: { textAlign: "left", textVerticalAlign: "bottom" } }), this.group.add(o));
                var s = t.pointToData(e), l = t.dimensions;
                s = c.map(s, function (e, i) {
                    var r = t.getAxis(l[i]);
                    return e = "category" === r.type || "time" === r.type ? r.scale.getLabel(e) : f.addCommas(e.toFixed(r.getPixelPrecision()));
                }), o.setStyle({ fill: n.getTextColor() || r.get("color"), textFont: n.getFont(), text: s.join(", "), x: e[0] + 5, y: e[1] - 5 }), o.z = a.get("z"), o.zlevel = a.get("zlevel");
            }, _getPointerElement: function (t, e, i, r) {
                var n = this._tooltipModel, a = n.get("z"), o = n.get("zlevel"), s = this._axisPointers, l = t.name;
                if (s[l] = s[l] || {}, s[l][i])
                    return s[l][i];
                var h = e.get("type"), c = e.getModel(h + "Style"), f = "shadow" === h, d = c[f ? "getAreaStyle" : "getLineStyle"](), p = "polar" === t.type ? f ? "Sector" : "radius" === i ? "Circle" : "Line" : f ? "Rect" : "Line";
                f ? d.stroke = null : d.fill = null;
                var g = s[l][i] = new u[p]({ style: d, z: a, zlevel: o, silent: !0, shape: r });
                return this.group.add(g), g;
            }, _dispatchAndShowSeriesTooltipContent: function (t, e, i, r, n) {
                var a = this._tooltipModel, o = t.getBaseAxis(), l = "x" === o.dim || "radius" === o.dim ? 0 : 1, h = c.map(e, function (t) {
                    return { seriesIndex: t.seriesIndex, dataIndex: t.getAxisTooltipDataIndex ? t.getAxisTooltipDataIndex(t.coordDimToDataDim(o.dim), r, o) : t.getData().indexOfNearest(t.coordDimToDataDim(o.dim)[0], r[l], !1, "category" === o.type ? .5 : null) };
                }), u = this._lastHover, f = this._api;
                if (u.payloadBatch && !n && f.dispatchAction({ type: "downplay", batch: u.payloadBatch }), n || (f.dispatchAction({ type: "highlight", batch: h }), u.payloadBatch = h), f.dispatchAction({ type: "showTip", dataIndex: h[0].dataIndex, seriesIndex: h[0].seriesIndex, from: this.uid }), o && a.get("showContent") && a.get("show")) {
                    var d = c.map(e, function (t, e) {
                        return t.getDataParams(h[e].dataIndex);
                    });
                    if (n)
                        s(a.get("position"), i[0], i[1], this._tooltipContent, d, null, f);
                    else {
                        var p = h[0].dataIndex, g = "time" === o.type ? o.scale.getLabel(r[l]) : e[0].getData().getName(p), v = (g ? g + "<br />" : "") + c.map(e, function (t, e) {
                            return t.formatTooltip(h[e].dataIndex, !0);
                        }).join("<br />"), m = "axis_" + t.name + "_" + p;
                        this._showTooltipContent(a, v, d, m, i[0], i[1], null, f);
                    }
                }
            }, _showItemTooltipContent: function (t, e, i, r) {
                var n = this._api, a = t.getData(i), o = a.getItemModel(e), s = o.get("tooltip", !0);
                if ("string" == typeof s) {
                    var l = s;
                    s = { formatter: l };
                }
                var h = this._tooltipModel, u = t.getModel("tooltip", h), c = new v(s, u, u.ecModel), f = t.getDataParams(e, i), d = t.formatTooltip(e, !1, i), p = "item_" + t.name + "_" + e;
                this._showTooltipContent(c, d, f, p, r.offsetX, r.offsetY, r.target, n);
            }, _showTooltipContent: function (t, e, i, r, n, a, o, l) {
                if (this._ticket = "", t.get("showContent") && t.get("show")) {
                    var h = this._tooltipContent, u = t.get("formatter"), c = t.get("position"), d = e;
                    if (u)
                        if ("string" == typeof u)
                            d = f.formatTpl(u, i);
                        else if ("function" == typeof u) {
                            var p = this, g = r, v = function (t, e) {
                                t === p._ticket && (h.setContent(e), s(c, n, a, h, i, o, l));
                            };
                            p._ticket = g, d = u(i, g, v);
                        }
                    h.show(t), h.setContent(d), s(c, n, a, h, i, o, l);
                }
            }, _showAxisPointer: function (t) {
                if (t) {
                    var e = this._axisPointers[t];
                    e && c.each(e, function (t) {
                        t.show();
                    });
                } else
                    this.group.eachChild(function (t) {
                        t.show();
                    }), this.group.show();
            }, _resetLastHover: function () {
                var t = this._lastHover;
                t.payloadBatch && this._api.dispatchAction({ type: "downplay", batch: t.payloadBatch }), this._lastHover = {};
            }, _hideAxisPointer: function (t) {
                if (t) {
                    var e = this._axisPointers[t];
                    e && c.each(e, function (t) {
                        t.hide();
                    });
                } else
                    this.group.children().length && this.group.hide();
            }, _hide: function () {
                clearTimeout(this._showTimeout), this._hideAxisPointer(), this._resetLastHover(), this._alwaysShowContent || this._tooltipContent.hideLater(this._tooltipModel.get("hideDelay")), this._api.dispatchAction({ type: "hideTip", from: this.uid }), this._lastX = this._lastY = null;
            }, dispose: function (t, e) {
                if (!g.node) {
                    var i = e.getZr();
                    this._tooltipContent.hide(), i.off("click", this._tryShow), i.off("mousemove", this._mousemove), i.off("mouseout", this._hide), i.off("globalout", this._hide), e.off("showTip", this._manuallyShowTip), e.off("hideTip", this._manuallyHideTip);
                }
            } });
    }), e("zrender/graphic/Gradient", ["require"], function () {
        var t = function (t) {
            this.colorStops = t || [];
        };
        return t.prototype = { constructor: t, addColorStop: function (t, e) {
                this.colorStops.push({ offset: t, color: e });
            } }, t;
    }), e("zrender/vml/core", ["require", "exports", "module", "../core/env"], function (t, e, i) {
        if (!t("../core/env").canvasSupported) {
            var r, n = "urn:schemas-microsoft-com:vml", a = window, o = a.document, s = !1;
            try  {
                !o.namespaces.zrvml && o.namespaces.add("zrvml", n), r = function (t) {
                    return o.createElement("<zrvml:" + t + ' class="zrvml">');
                };
            } catch (l) {
                r = function (t) {
                    return o.createElement("<" + t + ' xmlns="' + n + '" class="zrvml">');
                };
            }
            var h = function () {
                if (!s) {
                    s = !0;
                    var t = o.styleSheets;
                    t.length < 31 ? o.createStyleSheet().addRule(".zrvml", "behavior:url(#default#VML)") : t[0].addRule(".zrvml", "behavior:url(#default#VML)");
                }
            };
            i.exports = { doc: o, initVML: h, createNode: r };
        }
    }), e("echarts/util/model", ["require", "./format", "./number", "../model/Model", "zrender/core/util"], function (t) {
        var e = t("./format"), i = t("./number"), r = t("../model/Model"), n = t("zrender/core/util"), a = ["x", "y", "z", "radius", "angle"], o = {};
        return o.createNameEach = function (t, e) {
            t = t.slice();
            var i = n.map(t, o.capitalFirst);
            e = (e || []).slice();
            var r = n.map(e, o.capitalFirst);
            return function (a, o) {
                n.each(t, function (t, n) {
                    for (var s = { name: t, capital: i[n] }, l = 0; l < e.length; l++)
                        s[e[l]] = t + r[l];
                    a.call(o, s);
                });
            };
        }, o.capitalFirst = function (t) {
            return t ? t.charAt(0).toUpperCase() + t.substr(1) : t;
        }, o.eachAxisDim = o.createNameEach(a, ["axisIndex", "axis", "index"]), o.normalizeToArray = function (t) {
            return t instanceof Array ? t : null == t ? [] : [t];
        }, o.createLinkedNodesFinder = function (t, e, i) {
            function r(t, e) {
                return n.indexOf(e.nodes, t) >= 0;
            }
            function a(t, r) {
                var a = !1;
                return e(function (e) {
                    n.each(i(t, e) || [], function (t) {
                        r.records[e.name][t] && (a = !0);
                    });
                }), a;
            }
            function o(t, r) {
                r.nodes.push(t), e(function (e) {
                    n.each(i(t, e) || [], function (t) {
                        r.records[e.name][t] = !0;
                    });
                });
            }
            return function (i) {
                function n(t) {
                    !r(t, s) && a(t, s) && (o(t, s), l = !0);
                }
                var s = { nodes: [], records: {} };
                if (e(function (t) {
                    s.records[t.name] = {};
                }), !i)
                    return s;
                o(i, s);
                var l;
                do
                    l = !1, t(n); while(l);
                return s;
            };
        }, o.defaultEmphasis = function (t, e) {
            if (t) {
                var i = t.emphasis = t.emphasis || {}, r = t.normal = t.normal || {};
                n.each(e, function (t) {
                    var e = n.retrieve(i[t], r[t]);
                    null != e && (i[t] = e);
                });
            }
        }, o.LABEL_OPTIONS = ["position", "show", "textStyle", "distance", "formatter"], o.getDataItemValue = function (t) {
            return t && (null == t.value ? t : t.value);
        }, o.isDataItemOption = function (t) {
            return n.isObject(t) && !(t instanceof Array);
        }, o.converDataValue = function (t, e) {
            var r = e && e.type;
            return "ordinal" === r ? t : ("time" !== r || isFinite(t) || null == t || "-" === t || (t = +i.parseDate(t)), null == t || "" === t ? 0 / 0 : +t);
        }, o.createDataFormatModel = function (t, e) {
            var i = new r;
            return n.mixin(i, o.dataFormatMixin), i.seriesIndex = e.seriesIndex, i.name = e.name || "", i.mainType = e.mainType, i.subType = e.subType, i.getData = function () {
                return t;
            }, i;
        }, o.dataFormatMixin = { getDataParams: function (t, e) {
                var i = this.getData(e), r = this.seriesIndex, n = this.name, a = this.getRawValue(t, e), o = i.getRawIndex(t), s = i.getName(t, !0), l = i.getRawDataItem(t);
                return { componentType: this.mainType, componentSubType: this.subType, seriesType: "series" === this.mainType ? this.subType : null, seriesIndex: r, seriesName: n, name: s, dataIndex: o, data: l, dataType: e, value: a, color: i.getItemVisual(t, "color"), $vars: ["seriesName", "name", "value"] };
            }, getFormattedLabel: function (t, i, r, n) {
                i = i || "normal";
                var a = this.getData(r), o = a.getItemModel(t), s = this.getDataParams(t, r);
                null != n && s.value instanceof Array && (s.value = s.value[n]);
                var l = o.get(["label", i, "formatter"]);
                return "function" == typeof l ? (s.status = i, l(s)) : "string" == typeof l ? e.formatTpl(l, s) : void 0;
            }, getRawValue: function (t, e) {
                var i = this.getData(e), r = i.getRawDataItem(t);
                return null != r ? !n.isObject(r) || r instanceof Array ? r : r.value : void 0;
            }, formatTooltip: n.noop }, o.mappingToExists = function (t, e) {
            e = (e || []).slice();
            var i = n.map(t || [], function (t) {
                return { exist: t };
            });
            return n.each(e, function (t, r) {
                if (n.isObject(t))
                    for (var a = 0; a < i.length; a++) {
                        var s = i[a].exist;
                        if (!i[a].option && (null != t.id && s.id === t.id + "" || null != t.name && !o.isIdInner(t) && !o.isIdInner(s) && s.name === t.name + "")) {
                            i[a].option = t, e[r] = null;
                            break;
                        }
                    }
            }), n.each(e, function (t) {
                if (n.isObject(t)) {
                    for (var e = 0; e < i.length; e++) {
                        var r = i[e].exist;
                        if (!i[e].option && !o.isIdInner(r) && null == t.id) {
                            i[e].option = t;
                            break;
                        }
                    }
                    e >= i.length && i.push({ option: t });
                }
            }), i;
        }, o.isIdInner = function (t) {
            return n.isObject(t) && t.id && 0 === (t.id + "").indexOf(" _ec_ ");
        }, o.compressBatches = function (t, e) {
            function i(t, e, i) {
                for (var r = 0, n = t.length; n > r; r++)
                    for (var a = t[r].seriesId, s = o.normalizeToArray(t[r].dataIndex), l = i && i[a], h = 0, u = s.length; u > h; h++) {
                        var c = s[h];
                        l && l[c] ? l[c] = null : (e[a] || (e[a] = {}))[c] = 1;
                    }
            }
            function r(t, e) {
                var i = [];
                for (var n in t)
                    if (t.hasOwnProperty(n) && null != t[n])
                        if (e)
                            i.push(+n);
                        else {
                            var a = r(t[n], !0);
                            a.length && i.push({ seriesId: n, dataIndex: a });
                        }
                return i;
            }
            var n = {}, a = {};
            return i(t || [], n), i(e || [], a, n), [r(n), r(a)];
        }, o;
    }), e("echarts/model/globalDefault", [], function () {
        var t = "";
        return "undefined" != typeof navigator && (t = navigator.platform || ""), { color: ["#c23531", "#2f4554", "#61a0a8", "#d48265", "#91c7ae", "#749f83", "#ca8622", "#bda29a", "#6e7074", "#546570", "#c4ccd3"], grid: {}, textStyle: { fontFamily: t.match(/^Win/) ? "Microsoft YaHei" : "sans-serif", fontSize: 12, fontStyle: "normal", fontWeight: "normal" }, blendMode: null, animation: !0, animationDuration: 1e3, animationDurationUpdate: 300, animationEasing: "exponentialOut", animationEasingUpdate: "cubicOut", animationThreshold: 2e3, progressiveThreshold: 3e3, progressive: 400, hoverLayerThreshold: 3e3 };
    }), e("echarts/model/mixin/colorPalette", [], function () {
        return { clearColorPalette: function () {
                this._colorIdx = 0, this._colorNameMap = {};
            }, getColorFromPalette: function (t, e) {
                e = e || this;
                var i = e._colorIdx || 0, r = e._colorNameMap || (e._colorNameMap = {});
                if (r[t])
                    return r[t];
                var n = this.get("color", !0) || [];
                if (n.length) {
                    var a = n[i];
                    return t && (r[t] = a), e._colorIdx = (i + 1) % n.length, a;
                }
            } };
    }), e("zrender/core/curve", ["require", "./vector"], function (t) {
        "use strict";
        function e(t) {
            return t > -x && x > t;
        }
        function i(t) {
            return t > x || -x > t;
        }
        function r(t, e, i, r, n) {
            var a = 1 - n;
            return a * a * (a * t + 3 * n * e) + n * n * (n * r + 3 * a * i);
        }
        function n(t, e, i, r, n) {
            var a = 1 - n;
            return 3 * (((e - t) * a + 2 * (i - e) * n) * a + (r - i) * n * n);
        }
        function a(t, i, r, n, a, o) {
            var s = n + 3 * (i - r) - t, l = 3 * (r - 2 * i + t), h = 3 * (i - t), u = t - a, c = l * l - 3 * s * h, f = l * h - 9 * s * u, d = h * h - 3 * l * u, p = 0;
            if (e(c) && e(f))
                if (e(l))
                    o[0] = 0;
                else {
                    var g = -h / l;
                    g >= 0 && 1 >= g && (o[p++] = g);
                }
            else {
                var v = f * f - 4 * c * d;
                if (e(v)) {
                    var m = f / c, g = -l / s + m, x = -m / 2;
                    g >= 0 && 1 >= g && (o[p++] = g), x >= 0 && 1 >= x && (o[p++] = x);
                } else if (v > 0) {
                    var b = _(v), M = c * l + 1.5 * s * (-f + b), z = c * l + 1.5 * s * (-f - b);
                    M = 0 > M ? -y(-M, T) : y(M, T), z = 0 > z ? -y(-z, T) : y(z, T);
                    var g = (-l - (M + z)) / (3 * s);
                    g >= 0 && 1 >= g && (o[p++] = g);
                } else {
                    var S = (2 * c * l - 3 * s * f) / (2 * _(c * c * c)), C = Math.acos(S) / 3, P = _(c), A = Math.cos(C), g = (-l - 2 * P * A) / (3 * s), x = (-l + P * (A + w * Math.sin(C))) / (3 * s), k = (-l + P * (A - w * Math.sin(C))) / (3 * s);
                    g >= 0 && 1 >= g && (o[p++] = g), x >= 0 && 1 >= x && (o[p++] = x), k >= 0 && 1 >= k && (o[p++] = k);
                }
            }
            return p;
        }
        function o(t, r, n, a, o) {
            var s = 6 * n - 12 * r + 6 * t, l = 9 * r + 3 * a - 3 * t - 9 * n, h = 3 * r - 3 * t, u = 0;
            if (e(l)) {
                if (i(s)) {
                    var c = -h / s;
                    c >= 0 && 1 >= c && (o[u++] = c);
                }
            } else {
                var f = s * s - 4 * l * h;
                if (e(f))
                    o[0] = -s / (2 * l);
                else if (f > 0) {
                    var d = _(f), c = (-s + d) / (2 * l), p = (-s - d) / (2 * l);
                    c >= 0 && 1 >= c && (o[u++] = c), p >= 0 && 1 >= p && (o[u++] = p);
                }
            }
            return u;
        }
        function s(t, e, i, r, n, a) {
            var o = (e - t) * n + t, s = (i - e) * n + e, l = (r - i) * n + i, h = (s - o) * n + o, u = (l - s) * n + s, c = (u - h) * n + h;
            a[0] = t, a[1] = o, a[2] = h, a[3] = c, a[4] = c, a[5] = u, a[6] = l, a[7] = r;
        }
        function l(t, e, i, n, a, o, s, l, h, u, c) {
            var f, d, p, g, v, y = .005, x = 1 / 0;
            M[0] = h, M[1] = u;
            for (var w = 0; 1 > w; w += .05)
                z[0] = r(t, i, a, s, w), z[1] = r(e, n, o, l, w), g = m(M, z), x > g && (f = w, x = g);
            x = 1 / 0;
            for (var T = 0; 32 > T && !(b > y); T++)
                d = f - y, p = f + y, z[0] = r(t, i, a, s, d), z[1] = r(e, n, o, l, d), g = m(z, M), d >= 0 && x > g ? (f = d, x = g) : (S[0] = r(t, i, a, s, p), S[1] = r(e, n, o, l, p), v = m(S, M), 1 >= p && x > v ? (f = p, x = v) : y *= .5);
            return c && (c[0] = r(t, i, a, s, f), c[1] = r(e, n, o, l, f)), _(x);
        }
        function h(t, e, i, r) {
            var n = 1 - r;
            return n * (n * t + 2 * r * e) + r * r * i;
        }
        function u(t, e, i, r) {
            return 2 * ((1 - r) * (e - t) + r * (i - e));
        }
        function c(t, r, n, a, o) {
            var s = t - 2 * r + n, l = 2 * (r - t), h = t - a, u = 0;
            if (e(s)) {
                if (i(l)) {
                    var c = -h / l;
                    c >= 0 && 1 >= c && (o[u++] = c);
                }
            } else {
                var f = l * l - 4 * s * h;
                if (e(f)) {
                    var c = -l / (2 * s);
                    c >= 0 && 1 >= c && (o[u++] = c);
                } else if (f > 0) {
                    var d = _(f), c = (-l + d) / (2 * s), p = (-l - d) / (2 * s);
                    c >= 0 && 1 >= c && (o[u++] = c), p >= 0 && 1 >= p && (o[u++] = p);
                }
            }
            return u;
        }
        function f(t, e, i) {
            var r = t + i - 2 * e;
            return 0 === r ? .5 : (t - e) / r;
        }
        function d(t, e, i, r, n) {
            var a = (e - t) * r + t, o = (i - e) * r + e, s = (o - a) * r + a;
            n[0] = t, n[1] = a, n[2] = s, n[3] = s, n[4] = o, n[5] = i;
        }
        function p(t, e, i, r, n, a, o, s, l) {
            var u, c = .005, f = 1 / 0;
            M[0] = o, M[1] = s;
            for (var d = 0; 1 > d; d += .05) {
                z[0] = h(t, i, n, d), z[1] = h(e, r, a, d);
                var p = m(M, z);
                f > p && (u = d, f = p);
            }
            f = 1 / 0;
            for (var g = 0; 32 > g && !(b > c); g++) {
                var v = u - c, y = u + c;
                z[0] = h(t, i, n, v), z[1] = h(e, r, a, v);
                var p = m(z, M);
                if (v >= 0 && f > p)
                    u = v, f = p;
                else {
                    S[0] = h(t, i, n, y), S[1] = h(e, r, a, y);
                    var x = m(S, M);
                    1 >= y && f > x ? (u = y, f = x) : c *= .5;
                }
            }
            return l && (l[0] = h(t, i, n, u), l[1] = h(e, r, a, u)), _(f);
        }
        var g = t("./vector"), v = g.create, m = g.distSquare, y = Math.pow, _ = Math.sqrt, x = 1e-8, b = 1e-4, w = _(3), T = 1 / 3, M = v(), z = v(), S = v();
        return { cubicAt: r, cubicDerivativeAt: n, cubicRootAt: a, cubicExtrema: o, cubicSubdivide: s, cubicProjectPoint: l, quadraticAt: h, quadraticDerivativeAt: u, quadraticRootAt: c, quadraticExtremum: f, quadraticSubdivide: d, quadraticProjectPoint: p };
    }), e("echarts/util/clazz", ["require", "zrender/core/util"], function (t) {
        function e(t, e) {
            var i = r.slice(arguments, 2);
            return this.superClass.prototype[e].apply(t, i);
        }
        function i(t, e, i) {
            return this.superClass.prototype[e].apply(t, i);
        }
        var r = t("zrender/core/util"), n = {}, a = ".", o = "___EC__COMPONENT__CONTAINER___", s = n.parseClassType = function (t) {
            var e = { main: "", sub: "" };
            return t && (t = t.split(a), e.main = t[0] || "", e.sub = t[1] || ""), e;
        };
        return n.enableClassExtend = function (t) {
            t.$constructor = t, t.extend = function (t) {
                var n = this, a = function () {
                    t.$constructor ? t.$constructor.apply(this, arguments) : n.apply(this, arguments);
                };
                return r.extend(a.prototype, t), a.extend = this.extend, a.superCall = e, a.superApply = i, r.inherits(a, this), a.superClass = n, a;
            };
        }, n.enableClassManagement = function (t, e) {
            function i(t) {
                var e = n[t.main];
                return e && e[o] || (e = n[t.main] = {}, e[o] = !0), e;
            }
            e = e || {};
            var n = {};
            if (t.registerClass = function (t, e) {
                if (e)
                    if (e = s(e), e.sub) {
                        if (e.sub !== o) {
                            var r = i(e);
                            r[e.sub] = t;
                        }
                    } else
                        n[e.main] && console.warn(e.main + " exists."), n[e.main] = t;
                return t;
            }, t.getClass = function (t, e, i) {
                var r = n[t];
                if (r && r[o] && (r = e ? r[e] : null), i && !r)
                    throw new Error("Component " + t + "." + (e || "") + " not exists. Load it first.");
                return r;
            }, t.getClassesByMainType = function (t) {
                t = s(t);
                var e = [], i = n[t.main];
                return i && i[o] ? r.each(i, function (t, i) {
                    i !== o && e.push(t);
                }) : e.push(i), e;
            }, t.hasClass = function (t) {
                return t = s(t), !!n[t.main];
            }, t.getAllClassMainTypes = function () {
                var t = [];
                return r.each(n, function (e, i) {
                    t.push(i);
                }), t;
            }, t.hasSubTypes = function (t) {
                t = s(t);
                var e = n[t.main];
                return e && e[o];
            }, t.parseClassType = s, e.registerWhenExtend) {
                var a = t.extend;
                a && (t.extend = function (e) {
                    var i = a.call(this, e);
                    return t.registerClass(i, e.type);
                });
            }
            return t;
        }, n.setReadOnly = function () {
        }, n;
    }), e("zrender/core/bbox", ["require", "./vector", "./curve"], function (t) {
        var e = t("./vector"), i = t("./curve"), r = {}, n = Math.min, a = Math.max, o = Math.sin, s = Math.cos, l = e.create(), h = e.create(), u = e.create(), c = 2 * Math.PI;
        r.fromPoints = function (t, e, i) {
            if (0 !== t.length) {
                var r, o = t[0], s = o[0], l = o[0], h = o[1], u = o[1];
                for (r = 1; r < t.length; r++)
                    o = t[r], s = n(s, o[0]), l = a(l, o[0]), h = n(h, o[1]), u = a(u, o[1]);
                e[0] = s, e[1] = h, i[0] = l, i[1] = u;
            }
        }, r.fromLine = function (t, e, i, r, o, s) {
            o[0] = n(t, i), o[1] = n(e, r), s[0] = a(t, i), s[1] = a(e, r);
        };
        var f = [], d = [];
        return r.fromCubic = function (t, e, r, o, s, l, h, u, c, p) {
            var g, v = i.cubicExtrema, m = i.cubicAt, y = v(t, r, s, h, f);
            for (c[0] = 1 / 0, c[1] = 1 / 0, p[0] = -1 / 0, p[1] = -1 / 0, g = 0; y > g; g++) {
                var _ = m(t, r, s, h, f[g]);
                c[0] = n(_, c[0]), p[0] = a(_, p[0]);
            }
            for (y = v(e, o, l, u, d), g = 0; y > g; g++) {
                var x = m(e, o, l, u, d[g]);
                c[1] = n(x, c[1]), p[1] = a(x, p[1]);
            }
            c[0] = n(t, c[0]), p[0] = a(t, p[0]), c[0] = n(h, c[0]), p[0] = a(h, p[0]), c[1] = n(e, c[1]), p[1] = a(e, p[1]), c[1] = n(u, c[1]), p[1] = a(u, p[1]);
        }, r.fromQuadratic = function (t, e, r, o, s, l, h, u) {
            var c = i.quadraticExtremum, f = i.quadraticAt, d = a(n(c(t, r, s), 1), 0), p = a(n(c(e, o, l), 1), 0), g = f(t, r, s, d), v = f(e, o, l, p);
            h[0] = n(t, s, g), h[1] = n(e, l, v), u[0] = a(t, s, g), u[1] = a(e, l, v);
        }, r.fromArc = function (t, i, r, n, a, f, d, p, g) {
            var v = e.min, m = e.max, y = Math.abs(a - f);
            if (1e-4 > y % c && y > 1e-4)
                return p[0] = t - r, p[1] = i - n, g[0] = t + r, void (g[1] = i + n);
            if (l[0] = s(a) * r + t, l[1] = o(a) * n + i, h[0] = s(f) * r + t, h[1] = o(f) * n + i, v(p, l, h), m(g, l, h), a %= c, 0 > a && (a += c), f %= c, 0 > f && (f += c), a > f && !d ? f += c : f > a && d && (a += c), d) {
                var _ = f;
                f = a, a = _;
            }
            for (var x = 0; f > x; x += Math.PI / 2)
                x > a && (u[0] = s(x) * r + t, u[1] = o(x) * n + i, v(p, u, p), m(g, u, g));
        }, r;
    }), e("zrender/config", [], function () {
        var t = 1;
        "undefined" != typeof window && (t = Math.max(window.devicePixelRatio || 1, 1));
        var e = { debugMode: 0, devicePixelRatio: t };
        return e;
    }), e("echarts/model/mixin/lineStyle", ["require", "./makeStyleMapper"], function (t) {
        var e = t("./makeStyleMapper")([["lineWidth", "width"], ["stroke", "color"], ["opacity"], ["shadowBlur"], ["shadowOffsetX"], ["shadowOffsetY"], ["shadowColor"]]);
        return { getLineStyle: function (t) {
                var i = e.call(this, t), r = this.getLineDash();
                return r && (i.lineDash = r), i;
            }, getLineDash: function () {
                var t = this.get("type");
                return "solid" === t || null == t ? null : "dashed" === t ? [5, 5] : [1, 1];
            } };
    }), e("echarts/model/mixin/areaStyle", ["require", "./makeStyleMapper"], function (t) {
        return { getAreaStyle: t("./makeStyleMapper")([["fill", "color"], ["shadowBlur"], ["shadowOffsetX"], ["shadowOffsetY"], ["opacity"], ["shadowColor"]]) };
    }), e("echarts/model/mixin/textStyle", ["require", "zrender/contain/text"], function (t) {
        function e(t, e) {
            return t && t.getShallow(e);
        }
        var i = t("zrender/contain/text");
        return { getTextColor: function () {
                var t = this.ecModel;
                return this.getShallow("color") || t && t.get("textStyle.color");
            }, getFont: function () {
                var t = this.ecModel, i = t && t.getModel("textStyle");
                return [this.getShallow("fontStyle") || e(i, "fontStyle"), this.getShallow("fontWeight") || e(i, "fontWeight"), (this.getShallow("fontSize") || e(i, "fontSize") || 12) + "px", this.getShallow("fontFamily") || e(i, "fontFamily") || "sans-serif"].join(" ");
            }, getTextRect: function (t) {
                var e = this.get("textStyle") || {};
                return i.getBoundingRect(t, this.getFont(), e.align, e.baseline);
            }, truncateText: function (t, e, r, n) {
                return i.truncateText(t, e, this.getFont(), r, n);
            } };
    }), e("echarts/model/mixin/itemStyle", ["require", "./makeStyleMapper"], function (t) {
        var e = t("./makeStyleMapper")([["fill", "color"], ["stroke", "borderColor"], ["lineWidth", "borderWidth"], ["opacity"], ["shadowBlur"], ["shadowOffsetX"], ["shadowOffsetY"], ["shadowColor"]]);
        return { getItemStyle: function (t) {
                var i = e.call(this, t), r = this.getBorderLineDash();
                return r && (i.lineDash = r), i;
            }, getBorderLineDash: function () {
                var t = this.get("borderType");
                return "solid" === t || null == t ? null : "dashed" === t ? [5, 5] : [1, 1];
            } };
    }), e("zrender/Element", ["require", "./core/guid", "./mixin/Eventful", "./mixin/Transformable", "./mixin/Animatable", "./core/util"], function (t) {
        "use strict";
        var e = t("./core/guid"), i = t("./mixin/Eventful"), r = t("./mixin/Transformable"), n = t("./mixin/Animatable"), a = t("./core/util"), o = function (t) {
            r.call(this, t), i.call(this, t), n.call(this, t), this.id = t.id || e();
        };
        return o.prototype = { type: "element", name: "", __zr: null, ignore: !1, clipPath: null, drift: function (t, e) {
                switch (this.draggable) {
                    case "horizontal":
                        e = 0;
                        break;
                    case "vertical":
                        t = 0;
                }
                var i = this.transform;
                i || (i = this.transform = [1, 0, 0, 1, 0, 0]), i[4] += t, i[5] += e, this.decomposeTransform(), this.dirty(!1);
            }, beforeUpdate: function () {
            }, afterUpdate: function () {
            }, update: function () {
                this.updateTransform();
            }, traverse: function () {
            }, attrKV: function (t, e) {
                if ("position" === t || "scale" === t || "origin" === t) {
                    if (e) {
                        var i = this[t];
                        i || (i = this[t] = []), i[0] = e[0], i[1] = e[1];
                    }
                } else
                    this[t] = e;
            }, hide: function () {
                this.ignore = !0, this.__zr && this.__zr.refresh();
            }, show: function () {
                this.ignore = !1, this.__zr && this.__zr.refresh();
            }, attr: function (t, e) {
                if ("string" == typeof t)
                    this.attrKV(t, e);
                else if (a.isObject(t))
                    for (var i in t)
                        t.hasOwnProperty(i) && this.attrKV(i, t[i]);
                return this.dirty(!1), this;
            }, setClipPath: function (t) {
                var e = this.__zr;
                e && t.addSelfToZr(e), this.clipPath && this.clipPath !== t && this.removeClipPath(), this.clipPath = t, t.__zr = e, t.__clipTarget = this, this.dirty(!1);
            }, removeClipPath: function () {
                var t = this.clipPath;
                t && (t.__zr && t.removeSelfFromZr(t.__zr), t.__zr = null, t.__clipTarget = null, this.clipPath = null, this.dirty(!1));
            }, addSelfToZr: function (t) {
                this.__zr = t;
                var e = this.animators;
                if (e)
                    for (var i = 0; i < e.length; i++)
                        t.animation.addAnimator(e[i]);
                this.clipPath && this.clipPath.addSelfToZr(t);
            }, removeSelfFromZr: function (t) {
                this.__zr = null;
                var e = this.animators;
                if (e)
                    for (var i = 0; i < e.length; i++)
                        t.animation.removeAnimator(e[i]);
                this.clipPath && this.clipPath.removeSelfFromZr(t);
            } }, a.mixin(o, n), a.mixin(o, r), a.mixin(o, i), o;
    }), e("zrender/graphic/Style", ["require"], function () {
        function t(t, e, i) {
            var r = e.x, n = e.x2, a = e.y, o = e.y2;
            e.global || (r = r * i.width + i.x, n = n * i.width + i.x, a = a * i.height + i.y, o = o * i.height + i.y);
            var s = t.createLinearGradient(r, a, n, o);
            return s;
        }
        function e(t, e, i) {
            var r = i.width, n = i.height, a = Math.min(r, n), o = e.x, s = e.y, l = e.r;
            e.global || (o = o * r + i.x, s = s * n + i.y, l *= a);
            var h = t.createRadialGradient(o, s, 0, o, s, l);
            return h;
        }
        var i = [["shadowBlur", 0], ["shadowOffsetX", 0], ["shadowOffsetY", 0], ["shadowColor", "#000"], ["lineCap", "butt"], ["lineJoin", "miter"], ["miterLimit", 10]], r = function (t) {
            this.extendFrom(t);
        };
        r.prototype = { constructor: r, fill: "#000000", stroke: null, opacity: 1, lineDash: null, lineDashOffset: 0, shadowBlur: 0, shadowOffsetX: 0, shadowOffsetY: 0, lineWidth: 1, strokeNoScale: !1, text: null, textFill: "#000", textStroke: null, textPosition: "inside", textBaseline: null, textAlign: null, textVerticalAlign: null, textDistance: 5, textShadowBlur: 0, textShadowOffsetX: 0, textShadowOffsetY: 0, blend: null, bind: function (t, e, r) {
                for (var n = this, a = r && r.style, o = !a, s = 0; s < i.length; s++) {
                    var l = i[s], h = l[0];
                    (o || n[h] !== a[h]) && (t[h] = n[h] || l[1]);
                }
                if ((o || n.fill !== a.fill) && (t.fillStyle = n.fill), (o || n.stroke !== a.stroke) && (t.strokeStyle = n.stroke), (o || n.opacity !== a.opacity) && (t.globalAlpha = null == n.opacity ? 1 : n.opacity), (o || n.blend !== a.blend) && (t.globalCompositeOperation = n.blend || "source-over"), this.hasStroke()) {
                    var u = n.lineWidth;
                    t.lineWidth = u / (this.strokeNoScale && e && e.getLineScale ? e.getLineScale() : 1);
                }
            }, hasFill: function () {
                var t = this.fill;
                return null != t && "none" !== t;
            }, hasStroke: function () {
                var t = this.stroke;
                return null != t && "none" !== t && this.lineWidth > 0;
            }, extendFrom: function (t, e) {
                if (t) {
                    var i = this;
                    for (var r in t)
                        !t.hasOwnProperty(r) || !e && i.hasOwnProperty(r) || (i[r] = t[r]);
                }
            }, set: function (t, e) {
                "string" == typeof t ? this[t] = e : this.extendFrom(t, !0);
            }, clone: function () {
                var t = new this.constructor;
                return t.extendFrom(this, !0), t;
            }, getGradient: function (i, r, n) {
                for (var a = "radial" === r.type ? e : t, o = a(i, r, n), s = r.colorStops, l = 0; l < s.length; l++)
                    o.addColorStop(s[l].offset, s[l].color);
                return o;
            } };
        for (var n = r.prototype, a = 0; a < i.length; a++) {
            var o = i[a];
            o[0] in n || (n[o[0]] = o[1]);
        }
        return r.getGradient = n.getGradient, r;
    }), e("echarts/model/mixin/makeStyleMapper", ["require", "zrender/core/util"], function (t) {
        var e = t("zrender/core/util");
        return function (t) {
            for (var i = 0; i < t.length; i++)
                t[i][1] || (t[i][1] = t[i][0]);
            return function (i) {
                for (var r = {}, n = 0; n < t.length; n++) {
                    var a = t[n][1];
                    if (!(i && e.indexOf(i, a) >= 0)) {
                        var o = this.getShallow(a);
                        null != o && (r[t[n][0]] = o);
                    }
                }
                return r;
            };
        };
    }), e("zrender/mixin/Transformable", ["require", "../core/matrix", "../core/vector"], function (t) {
        "use strict";
        function e(t) {
            return t > a || -a > t;
        }
        var i = t("../core/matrix"), r = t("../core/vector"), n = i.identity, a = 5e-5, o = function (t) {
            t = t || {}, t.position || (this.position = [0, 0]), null == t.rotation && (this.rotation = 0), t.scale || (this.scale = [1, 1]), this.origin = this.origin || null;
        }, s = o.prototype;
        s.transform = null, s.needLocalTransform = function () {
            return e(this.rotation) || e(this.position[0]) || e(this.position[1]) || e(this.scale[0] - 1) || e(this.scale[1] - 1);
        }, s.updateTransform = function () {
            var t = this.parent, e = t && t.transform, r = this.needLocalTransform(), a = this.transform;
            return r || e ? (a = a || i.create(), r ? this.getLocalTransform(a) : n(a), e && (r ? i.mul(a, t.transform, a) : i.copy(a, t.transform)), this.transform = a, this.invTransform = this.invTransform || i.create(), void i.invert(this.invTransform, a)) : void (a && n(a));
        }, s.getLocalTransform = function (t) {
            t = t || [], n(t);
            var e = this.origin, r = this.scale, a = this.rotation, o = this.position;
            return e && (t[4] -= e[0], t[5] -= e[1]), i.scale(t, t, r), a && i.rotate(t, t, a), e && (t[4] += e[0], t[5] += e[1]), t[4] += o[0], t[5] += o[1], t;
        }, s.setTransform = function (t) {
            var e = this.transform;
            e && t.transform(e[0], e[1], e[2], e[3], e[4], e[5]);
        }, s.restoreTransform = function (t) {
            var e = this.invTransform;
            e && t.transform(e[0], e[1], e[2], e[3], e[4], e[5]);
        };
        var l = [];
        return s.decomposeTransform = function () {
            if (this.transform) {
                var t = this.parent, r = this.transform;
                t && t.transform && (i.mul(l, t.invTransform, r), r = l);
                var n = r[0] * r[0] + r[1] * r[1], a = r[2] * r[2] + r[3] * r[3], o = this.position, s = this.scale;
                e(n - 1) && (n = Math.sqrt(n)), e(a - 1) && (a = Math.sqrt(a)), r[0] < 0 && (n = -n), r[3] < 0 && (a = -a), o[0] = r[4], o[1] = r[5], s[0] = n, s[1] = a, this.rotation = Math.atan2(-r[1] / a, r[0] / n);
            }
        }, s.getGlobalScale = function () {
            var t = this.transform;
            if (!t)
                return [1, 1];
            var e = Math.sqrt(t[0] * t[0] + t[1] * t[1]), i = Math.sqrt(t[2] * t[2] + t[3] * t[3]);
            return t[0] < 0 && (e = -e), t[3] < 0 && (i = -i), [e, i];
        }, s.transformCoordToLocal = function (t, e) {
            var i = [t, e], n = this.invTransform;
            return n && r.applyTransform(i, i, n), i;
        }, s.transformCoordToGlobal = function (t, e) {
            var i = [t, e], n = this.transform;
            return n && r.applyTransform(i, i, n), i;
        }, o;
    }), e("zrender/mixin/Animatable", ["require", "../animation/Animator", "../core/util", "../core/log"], function (t) {
        "use strict";
        var e = t("../animation/Animator"), i = t("../core/util"), r = i.isString, n = i.isFunction, a = i.isObject, o = t("../core/log"), s = function () {
            this.animators = [];
        };
        return s.prototype = { constructor: s, animate: function (t, r) {
                var n, a = !1, s = this, l = this.__zr;
                if (t) {
                    var h = t.split("."), u = s;
                    a = "shape" === h[0];
                    for (var c = 0, f = h.length; f > c; c++)
                        u && (u = u[h[c]]);
                    u && (n = u);
                } else
                    n = s;
                if (!n)
                    return void o('Property "' + t + '" is not existed in element ' + s.id);
                var d = s.animators, p = new e(n, r);
                return p.during(function () {
                    s.dirty(a);
                }).done(function () {
                    d.splice(i.indexOf(d, p), 1);
                }), d.push(p), l && l.animation.addAnimator(p), p;
            }, stopAnimation: function (t) {
                for (var e = this.animators, i = e.length, r = 0; i > r; r++)
                    e[r].stop(t);
                return e.length = 0, this;
            }, animateTo: function (t, e, i, a, o) {
                function s() {
                    h--, h || o && o();
                }
                r(i) ? (o = a, a = i, i = 0) : n(a) ? (o = a, a = "linear", i = 0) : n(i) ? (o = i, i = 0) : n(e) ? (o = e, e = 500) : e || (e = 500), this.stopAnimation(), this._animateToShallow("", this, t, e, i, a, o);
                var l = this.animators.slice(), h = l.length;
                h || o && o();
                for (var u = 0; u < l.length; u++)
                    l[u].done(s).start(a);
            }, _animateToShallow: function (t, e, r, n, o) {
                var s = {}, l = 0;
                for (var h in r)
                    if (null != e[h])
                        a(r[h]) && !i.isArrayLike(r[h]) ? this._animateToShallow(t ? t + "." + h : h, e[h], r[h], n, o) : (s[h] = r[h], l++);
                    else if (null != r[h])
                        if (t) {
                            var u = {};
                            u[t] = {}, u[t][h] = r[h], this.attr(u);
                        } else
                            this.attr(h, r[h]);
                return l > 0 && this.animate(t, !1).when(null == n ? 500 : n, s).delay(o || 0), this;
            } }, s;
    }), e("zrender/core/guid", [], function () {
        var t = 2311;
        return function () {
            return t++;
        };
    }), e("echarts/util/component", ["require", "zrender/core/util", "./clazz"], function (t) {
        var e = t("zrender/core/util"), i = t("./clazz"), r = i.parseClassType, n = 0, a = {}, o = "_";
        return a.getUID = function (t) {
            return [t || "", n++, Math.random()].join(o);
        }, a.enableSubTypeDefaulter = function (t) {
            var e = {};
            return t.registerSubTypeDefaulter = function (t, i) {
                t = r(t), e[t.main] = i;
            }, t.determineSubType = function (i, n) {
                var a = n.type;
                if (!a) {
                    var o = r(i).main;
                    t.hasSubTypes(i) && e[o] && (a = e[o](n));
                }
                return a;
            }, t;
        }, a.enableTopologicalTravel = function (t, i) {
            function r(t) {
                var r = {}, o = [];
                return e.each(t, function (s) {
                    var l = n(r, s), h = l.originalDeps = i(s), u = a(h, t);
                    l.entryCount = u.length, 0 === l.entryCount && o.push(s), e.each(u, function (t) {
                        e.indexOf(l.predecessor, t) < 0 && l.predecessor.push(t);
                        var i = n(r, t);
                        e.indexOf(i.successor, t) < 0 && i.successor.push(s);
                    });
                }), { graph: r, noEntryList: o };
            }
            function n(t, e) {
                return t[e] || (t[e] = { predecessor: [], successor: [] }), t[e];
            }
            function a(t, i) {
                var r = [];
                return e.each(t, function (t) {
                    e.indexOf(i, t) >= 0 && r.push(t);
                }), r;
            }
            t.topologicalTravel = function (t, i, n, a) {
                function o(t) {
                    h[t].entryCount--, 0 === h[t].entryCount && u.push(t);
                }
                function s(t) {
                    c[t] = !0, o(t);
                }
                if (t.length) {
                    var l = r(i), h = l.graph, u = l.noEntryList, c = {};
                    for (e.each(t, function (t) {
                        c[t] = !0;
                    }); u.length;) {
                        var f = u.pop(), d = h[f], p = !!c[f];
                        p && (n.call(a, f, d.originalDeps.slice()), delete c[f]), e.each(d.successor, p ? s : o);
                    }
                    e.each(c, function () {
                        throw new Error("Circle dependency may exists");
                    });
                }
            };
        }, a;
    }), e("echarts/model/mixin/boxLayout", ["require"], function () {
        return { getBoxLayoutParams: function () {
                return { left: this.get("left"), top: this.get("top"), right: this.get("right"), bottom: this.get("bottom"), width: this.get("width"), height: this.get("height") };
            } };
    }), e("echarts/util/layout", ["require", "zrender/core/util", "zrender/core/BoundingRect", "./number", "./format"], function (t) {
        "use strict";
        function e(t, e, i, r, n) {
            var a = 0, o = 0;
            null == r && (r = 1 / 0), null == n && (n = 1 / 0);
            var s = 0;
            e.eachChild(function (l, h) {
                var u, c, f = l.position, d = l.getBoundingRect(), p = e.childAt(h + 1), g = p && p.getBoundingRect();
                if ("horizontal" === t) {
                    var v = d.width + (g ? -g.x + d.x : 0);
                    u = a + v, u > r || l.newline ? (a = 0, u = v, o += s + i, s = d.height) : s = Math.max(s, d.height);
                } else {
                    var m = d.height + (g ? -g.y + d.y : 0);
                    c = o + m, c > n || l.newline ? (a += s + i, o = 0, c = m, s = d.width) : s = Math.max(s, d.width);
                }
                l.newline || (f[0] = a, f[1] = o, "horizontal" === t ? a = u + i : o = c + i);
            });
        }
        var i = t("zrender/core/util"), r = t("zrender/core/BoundingRect"), n = t("./number"), a = t("./format"), o = n.parsePercent, s = i.each, l = {}, h = ["left", "right", "top", "bottom", "width", "height"];
        return l.box = e, l.vbox = i.curry(e, "vertical"), l.hbox = i.curry(e, "horizontal"), l.getAvailableSize = function (t, e, i) {
            var r = e.width, n = e.height, s = o(t.x, r), l = o(t.y, n), h = o(t.x2, r), u = o(t.y2, n);
            return (isNaN(s) || isNaN(parseFloat(t.x))) && (s = 0), (isNaN(h) || isNaN(parseFloat(t.x2))) && (h = r), (isNaN(l) || isNaN(parseFloat(t.y))) && (l = 0), (isNaN(u) || isNaN(parseFloat(t.y2))) && (u = n), i = a.normalizeCssArray(i || 0), { width: Math.max(h - s - i[1] - i[3], 0), height: Math.max(u - l - i[0] - i[2], 0) };
        }, l.getLayoutRect = function (t, e, i) {
            i = a.normalizeCssArray(i || 0);
            var n = e.width, s = e.height, l = o(t.left, n), h = o(t.top, s), u = o(t.right, n), c = o(t.bottom, s), f = o(t.width, n), d = o(t.height, s), p = i[2] + i[0], g = i[1] + i[3], v = t.aspect;
            switch (isNaN(f) && (f = n - u - g - l), isNaN(d) && (d = s - c - p - h), isNaN(f) && isNaN(d) && (v > n / s ? f = .8 * n : d = .8 * s), null != v && (isNaN(f) && (f = v * d), isNaN(d) && (d = f / v)), isNaN(l) && (l = n - u - f - g), isNaN(h) && (h = s - c - d - p), t.left || t.right) {
                case "center":
                    l = n / 2 - f / 2 - i[3];
                    break;
                case "right":
                    l = n - f - g;
            }
            switch (t.top || t.bottom) {
                case "middle":
                case "center":
                    h = s / 2 - d / 2 - i[0];
                    break;
                case "bottom":
                    h = s - d - p;
            }
            l = l || 0, h = h || 0, isNaN(f) && (f = n - l - (u || 0)), isNaN(d) && (d = s - h - (c || 0));
            var m = new r(l + i[3], h + i[0], f, d);
            return m.margin = i, m;
        }, l.positionGroup = function (t, e, r, n) {
            var a = t.getBoundingRect();
            e = i.extend(i.clone(e), { width: a.width, height: a.height }), e = l.getLayoutRect(e, r, n), t.attr("position", [e.x - a.x, e.y - a.y]);
        }, l.mergeLayoutParam = function (t, e, r) {
            function n(i) {
                var n = {}, l = 0, h = {}, u = 0, c = r.ignoreSize ? 1 : 2;
                if (s(i, function (e) {
                    h[e] = t[e];
                }), s(i, function (t) {
                    a(e, t) && (n[t] = h[t] = e[t]), o(n, t) && l++, o(h, t) && u++;
                }), u !== c && l) {
                    if (l >= c)
                        return n;
                    for (var f = 0; f < i.length; f++) {
                        var d = i[f];
                        if (!a(n, d) && a(t, d)) {
                            n[d] = t[d];
                            break;
                        }
                    }
                    return n;
                }
                return h;
            }
            function a(t, e) {
                return t.hasOwnProperty(e);
            }
            function o(t, e) {
                return null != t[e] && "auto" !== t[e];
            }
            function l(t, e, i) {
                s(t, function (t) {
                    e[t] = i[t];
                });
            }
            !i.isObject(r) && (r = {});
            var h = ["width", "left", "right"], u = ["height", "top", "bottom"], c = n(h), f = n(u);
            l(h, t, c), l(u, t, f);
        }, l.getLayoutParams = function (t) {
            return l.copyLayoutParams({}, t);
        }, l.copyLayoutParams = function (t, e) {
            return e && t && s(h, function (i) {
                e.hasOwnProperty(i) && (t[i] = e[i]);
            }), t;
        }, l;
    }), e("zrender/animation/Animator", ["require", "./Clip", "../tool/color", "../core/util"], function (t) {
        function e(t, e) {
            return t[e];
        }
        function i(t, e, i) {
            t[e] = i;
        }
        function r(t, e, i) {
            return (e - t) * i + t;
        }
        function n(t, e, i) {
            return i > .5 ? e : t;
        }
        function a(t, e, i, n, a) {
            var o = t.length;
            if (1 == a)
                for (var s = 0; o > s; s++)
                    n[s] = r(t[s], e[s], i);
            else
                for (var l = t[0].length, s = 0; o > s; s++)
                    for (var h = 0; l > h; h++)
                        n[s][h] = r(t[s][h], e[s][h], i);
        }
        function o(t, e, i) {
            var r = t.length, n = e.length;
            if (r !== n) {
                var a = r > n;
                if (a)
                    t.length = n;
                else
                    for (var o = r; n > o; o++)
                        t.push(1 === i ? e[o] : m.call(e[o]));
            }
            for (var s = t[0] && t[0].length, o = 0; o < t.length; o++)
                if (1 === i)
                    isNaN(t[o]) && (t[o] = e[o]);
                else
                    for (var l = 0; s > l; l++)
                        isNaN(t[o][l]) && (t[o][l] = e[o][l]);
        }
        function s(t, e, i) {
            if (t === e)
                return !0;
            var r = t.length;
            if (r !== e.length)
                return !1;
            if (1 === i) {
                for (var n = 0; r > n; n++)
                    if (t[n] !== e[n])
                        return !1;
            } else
                for (var a = t[0].length, n = 0; r > n; n++)
                    for (var o = 0; a > o; o++)
                        if (t[n][o] !== e[n][o])
                            return !1;
            return !0;
        }
        function l(t, e, i, r, n, a, o, s, l) {
            var u = t.length;
            if (1 == l)
                for (var c = 0; u > c; c++)
                    s[c] = h(t[c], e[c], i[c], r[c], n, a, o);
            else
                for (var f = t[0].length, c = 0; u > c; c++)
                    for (var d = 0; f > d; d++)
                        s[c][d] = h(t[c][d], e[c][d], i[c][d], r[c][d], n, a, o);
        }
        function h(t, e, i, r, n, a, o) {
            var s = .5 * (i - t), l = .5 * (r - e);
            return (2 * (e - i) + s + l) * o + (-3 * (e - i) - 2 * s - l) * a + s * n + e;
        }
        function u(t) {
            if (v(t)) {
                var e = t.length;
                if (v(t[0])) {
                    for (var i = [], r = 0; e > r; r++)
                        i.push(m.call(t[r]));
                    return i;
                }
                return m.call(t);
            }
            return t;
        }
        function c(t) {
            return t[0] = Math.floor(t[0]), t[1] = Math.floor(t[1]), t[2] = Math.floor(t[2]), "rgba(" + t.join(",") + ")";
        }
        function f(t, e, i, u, f) {
            var g = t._getter, m = t._setter, y = "spline" === e, _ = u.length;
            if (_) {
                var x, b = u[0].value, w = v(b), T = !1, M = !1, z = w && v(b[0]) ? 2 : 1;
                u.sort(function (t, e) {
                    return t.time - e.time;
                }), x = u[_ - 1].time;
                for (var S = [], C = [], P = u[0].value, A = !0, k = 0; _ > k; k++) {
                    S.push(u[k].time / x);
                    var L = u[k].value;
                    if (w && s(L, P, z) || !w && L === P || (A = !1), P = L, "string" == typeof L) {
                        var I = p.parse(L);
                        I ? (L = I, T = !0) : M = !0;
                    }
                    C.push(L);
                }
                if (!A) {
                    for (var D = C[_ - 1], k = 0; _ - 1 > k; k++)
                        w ? o(C[k], D, z) : !isNaN(C[k]) || isNaN(D) || M || T || (C[k] = D);
                    w && o(g(t._target, f), D, z);
                    var E, O, R, q, B, F, N = 0, V = 0;
                    if (T)
                        var H = [0, 0, 0, 0];
                    var G = function (t, e) {
                        var i;
                        if (V > e) {
                            for (E = Math.min(N + 1, _ - 1), i = E; i >= 0 && !(S[i] <= e); i--)
                                ;
                            i = Math.min(i, _ - 2);
                        } else {
                            for (i = N; _ > i && !(S[i] > e); i++)
                                ;
                            i = Math.min(i - 1, _ - 2);
                        }
                        N = i, V = e;
                        var o = S[i + 1] - S[i];
                        if (0 !== o)
                            if (O = (e - S[i]) / o, y)
                                if (q = C[i], R = C[0 === i ? i : i - 1], B = C[i > _ - 2 ? _ - 1 : i + 1], F = C[i > _ - 3 ? _ - 1 : i + 2], w)
                                    l(R, q, B, F, O, O * O, O * O * O, g(t, f), z);
                                else {
                                    var s;
                                    if (T)
                                        s = l(R, q, B, F, O, O * O, O * O * O, H, 1), s = c(H);
                                    else {
                                        if (M)
                                            return n(q, B, O);
                                        s = h(R, q, B, F, O, O * O, O * O * O);
                                    }
                                    m(t, f, s);
                                }
                            else if (w)
                                a(C[i], C[i + 1], O, g(t, f), z);
                            else {
                                var s;
                                if (T)
                                    a(C[i], C[i + 1], O, H, 1), s = c(H);
                                else {
                                    if (M)
                                        return n(C[i], C[i + 1], O);
                                    s = r(C[i], C[i + 1], O);
                                }
                                m(t, f, s);
                            }
                    }, W = new d({ target: t._target, life: x, loop: t._loop, delay: t._delay, onframe: G, ondestroy: i });
                    return e && "spline" !== e && (W.easing = e), W;
                }
            }
        }
        var d = t("./Clip"), p = t("../tool/color"), g = t("../core/util"), v = g.isArrayLike, m = Array.prototype.slice, y = function (t, r, n, a) {
            this._tracks = {}, this._target = t, this._loop = r || !1, this._getter = n || e, this._setter = a || i, this._clipCount = 0, this._delay = 0, this._doneList = [], this._onframeList = [], this._clipList = [];
        };
        return y.prototype = {
            when: function (t, e) {
                var i = this._tracks;
                for (var r in e) {
                    if (!i[r]) {
                        i[r] = [];
                        var n = this._getter(this._target, r);
                        if (null == n)
                            continue;
                        0 !== t && i[r].push({ time: 0, value: u(n) });
                    }
                    i[r].push({ time: t, value: e[r] });
                }
                return this;
            }, during: function (t) {
                return this._onframeList.push(t), this;
            }, _doneCallback: function () {
                this._tracks = {}, this._clipList.length = 0;
                for (var t = this._doneList, e = t.length, i = 0; e > i; i++)
                    t[i].call(this);
            }, start: function (t) {
                var e, i = this, r = 0, n = function () {
                    r--, r || i._doneCallback();
                };
                for (var a in this._tracks) {
                    var o = f(this, t, n, this._tracks[a], a);
                    o && (this._clipList.push(o), r++, this.animation && this.animation.addClip(o), e = o);
                }
                if (e) {
                    var s = e.onframe;
                    e.onframe = function (t, e) {
                        s(t, e);
                        for (var r = 0; r < i._onframeList.length; r++)
                            i._onframeList[r](t, e);
                    };
                }
                return r || this._doneCallback(), this;
            }, stop: function (t) {
                for (var e = this._clipList, i = this.animation, r = 0; r < e.length; r++) {
                    var n = e[r];
                    t && n.onframe(this._target, 1), i && i.removeClip(n);
                }
                e.length = 0;
            }, delay: function (t) {
                return this._delay = t, this;
            }, done: function (t) {
                return t && this._doneList.push(t), this;
            }, getClips: function () {
                return this._clipList;
            } }, y;
    }), e("zrender/container/Group", ["require", "../core/util", "../Element", "../core/BoundingRect"], function (t) {
        var e = t("../core/util"), i = t("../Element"), r = t("../core/BoundingRect"), n = function (t) {
            t = t || {}, i.call(this, t);
            for (var e in t)
                this[e] = t[e];
            this._children = [], this.__storage = null, this.__dirty = !0;
        };
        return n.prototype = { constructor: n, isGroup: !0, type: "group", silent: !1, children: function () {
                return this._children.slice();
            }, childAt: function (t) {
                return this._children[t];
            }, childOfName: function (t) {
                for (var e = this._children, i = 0; i < e.length; i++)
                    if (e[i].name === t)
                        return e[i];
            }, childCount: function () {
                return this._children.length;
            }, add: function (t) {
                return t && t !== this && t.parent !== this && (this._children.push(t), this._doAdd(t)), this;
            }, addBefore: function (t, e) {
                if (t && t !== this && t.parent !== this && e && e.parent === this) {
                    var i = this._children, r = i.indexOf(e);
                    r >= 0 && (i.splice(r, 0, t), this._doAdd(t));
                }
                return this;
            }, _doAdd: function (t) {
                t.parent && t.parent.remove(t), t.parent = this;
                var e = this.__storage, i = this.__zr;
                e && e !== t.__storage && (e.addToMap(t), t instanceof n && t.addChildrenToStorage(e)), i && i.refresh();
            }, remove: function (t) {
                var i = this.__zr, r = this.__storage, a = this._children, o = e.indexOf(a, t);
                return 0 > o ? this : (a.splice(o, 1), t.parent = null, r && (r.delFromMap(t.id), t instanceof n && t.delChildrenFromStorage(r)), i && i.refresh(), this);
            }, removeAll: function () {
                var t, e, i = this._children, r = this.__storage;
                for (e = 0; e < i.length; e++)
                    t = i[e], r && (r.delFromMap(t.id), t instanceof n && t.delChildrenFromStorage(r)), t.parent = null;
                return i.length = 0, this;
            }, eachChild: function (t, e) {
                for (var i = this._children, r = 0; r < i.length; r++) {
                    var n = i[r];
                    t.call(e, n, r);
                }
                return this;
            }, traverse: function (t, e) {
                for (var i = 0; i < this._children.length; i++) {
                    var r = this._children[i];
                    t.call(e, r), "group" === r.type && r.traverse(t, e);
                }
                return this;
            }, addChildrenToStorage: function (t) {
                for (var e = 0; e < this._children.length; e++) {
                    var i = this._children[e];
                    t.addToMap(i), i instanceof n && i.addChildrenToStorage(t);
                }
            }, delChildrenFromStorage: function (t) {
                for (var e = 0; e < this._children.length; e++) {
                    var i = this._children[e];
                    t.delFromMap(i.id), i instanceof n && i.delChildrenFromStorage(t);
                }
            }, dirty: function () {
                return this.__dirty = !0, this.__zr && this.__zr.refresh(), this;
            }, getBoundingRect: function (t) {
                for (var e = null, i = new r(0, 0, 0, 0), n = t || this._children, a = [], o = 0; o < n.length; o++) {
                    var s = n[o];
                    if (!s.ignore && !s.invisible) {
                        var l = s.getBoundingRect(), h = s.getLocalTransform(a);
                        h ? (i.copy(l), i.applyTransform(h), e = e || i.clone(), e.union(i)) : (e = e || l.clone(), e.union(l));
                    }
                }
                return e || i;
            } }, e.inherits(n, i), n;
    }), e("zrender/core/log", ["require", "../config"], function (t) {
        var e = t("../config");
        return function () {
            if (0 !== e.debugMode)
                if (1 == e.debugMode)
                    for (var t in arguments)
                        throw new Error(arguments[t]);
                else if (e.debugMode > 1)
                    for (var t in arguments)
                        console.log(arguments[t]);
        };
    }), e("zrender/animation/Clip", ["require", "./easing"], function (t) {
        function e(t) {
            this._target = t.target, this._life = t.life || 1e3, this._delay = t.delay || 0, this._initialized = !1, this.loop = null == t.loop ? !1 : t.loop, this.gap = t.gap || 0, this.easing = t.easing || "Linear", this.onframe = t.onframe, this.ondestroy = t.ondestroy, this.onrestart = t.onrestart;
        }
        var i = t("./easing");
        return e.prototype = { constructor: e, step: function (t) {
                this._initialized || (this._startTime = (new Date).getTime() + this._delay, this._initialized = !0);
                var e = (t - this._startTime) / this._life;
                if (!(0 > e)) {
                    e = Math.min(e, 1);
                    var r = this.easing, n = "string" == typeof r ? i[r] : r, a = "function" == typeof n ? n(e) : e;
                    return this.fire("frame", a), 1 == e ? this.loop ? (this.restart(), "restart") : (this._needsRemove = !0, "destroy") : null;
                }
            }, restart: function () {
                var t = (new Date).getTime(), e = (t - this._startTime) % this._life;
                this._startTime = (new Date).getTime() - e + this.gap, this._needsRemove = !1;
            }, fire: function (t, e) {
                t = "on" + t, this[t] && this[t](this._target, e);
            } }, e;
    }), e("zrender/animation/easing", [], function () {
        var t = { linear: function (t) {
                return t;
            }, quadraticIn: function (t) {
                return t * t;
            }, quadraticOut: function (t) {
                return t * (2 - t);
            }, quadraticInOut: function (t) {
                return (t *= 2) < 1 ? .5 * t * t : -.5 * (--t * (t - 2) - 1);
            }, cubicIn: function (t) {
                return t * t * t;
            }, cubicOut: function (t) {
                return --t * t * t + 1;
            }, cubicInOut: function (t) {
                return (t *= 2) < 1 ? .5 * t * t * t : .5 * ((t -= 2) * t * t + 2);
            }, quarticIn: function (t) {
                return t * t * t * t;
            }, quarticOut: function (t) {
                return 1 - --t * t * t * t;
            }, quarticInOut: function (t) {
                return (t *= 2) < 1 ? .5 * t * t * t * t : -.5 * ((t -= 2) * t * t * t - 2);
            }, quinticIn: function (t) {
                return t * t * t * t * t;
            }, quinticOut: function (t) {
                return --t * t * t * t * t + 1;
            }, quinticInOut: function (t) {
                return (t *= 2) < 1 ? .5 * t * t * t * t * t : .5 * ((t -= 2) * t * t * t * t + 2);
            }, sinusoidalIn: function (t) {
                return 1 - Math.cos(t * Math.PI / 2);
            }, sinusoidalOut: function (t) {
                return Math.sin(t * Math.PI / 2);
            }, sinusoidalInOut: function (t) {
                return .5 * (1 - Math.cos(Math.PI * t));
            }, exponentialIn: function (t) {
                return 0 === t ? 0 : Math.pow(1024, t - 1);
            }, exponentialOut: function (t) {
                return 1 === t ? 1 : 1 - Math.pow(2, -10 * t);
            }, exponentialInOut: function (t) {
                return 0 === t ? 0 : 1 === t ? 1 : (t *= 2) < 1 ? .5 * Math.pow(1024, t - 1) : .5 * (-Math.pow(2, -10 * (t - 1)) + 2);
            }, circularIn: function (t) {
                return 1 - Math.sqrt(1 - t * t);
            }, circularOut: function (t) {
                return Math.sqrt(1 - --t * t);
            }, circularInOut: function (t) {
                return (t *= 2) < 1 ? -.5 * (Math.sqrt(1 - t * t) - 1) : .5 * (Math.sqrt(1 - (t -= 2) * t) + 1);
            }, elasticIn: function (t) {
                var e, i = .1, r = .4;
                return 0 === t ? 0 : 1 === t ? 1 : (!i || 1 > i ? (i = 1, e = r / 4) : e = r * Math.asin(1 / i) / (2 * Math.PI), -(i * Math.pow(2, 10 * (t -= 1)) * Math.sin(2 * (t - e) * Math.PI / r)));
            }, elasticOut: function (t) {
                var e, i = .1, r = .4;
                return 0 === t ? 0 : 1 === t ? 1 : (!i || 1 > i ? (i = 1, e = r / 4) : e = r * Math.asin(1 / i) / (2 * Math.PI), i * Math.pow(2, -10 * t) * Math.sin(2 * (t - e) * Math.PI / r) + 1);
            }, elasticInOut: function (t) {
                var e, i = .1, r = .4;
                return 0 === t ? 0 : 1 === t ? 1 : (!i || 1 > i ? (i = 1, e = r / 4) : e = r * Math.asin(1 / i) / (2 * Math.PI), (t *= 2) < 1 ? -.5 * i * Math.pow(2, 10 * (t -= 1)) * Math.sin(2 * (t - e) * Math.PI / r) : i * Math.pow(2, -10 * (t -= 1)) * Math.sin(2 * (t - e) * Math.PI / r) * .5 + 1);
            }, backIn: function (t) {
                var e = 1.70158;
                return t * t * ((e + 1) * t - e);
            }, backOut: function (t) {
                var e = 1.70158;
                return --t * t * ((e + 1) * t + e) + 1;
            }, backInOut: function (t) {
                var e = 2.5949095;
                return (t *= 2) < 1 ? .5 * t * t * ((e + 1) * t - e) : .5 * ((t -= 2) * t * ((e + 1) * t + e) + 2);
            }, bounceIn: function (e) {
                return 1 - t.bounceOut(1 - e);
            }, bounceOut: function (t) {
                return 1 / 2.75 > t ? 7.5625 * t * t : 2 / 2.75 > t ? 7.5625 * (t -= 1.5 / 2.75) * t + .75 : 2.5 / 2.75 > t ? 7.5625 * (t -= 2.25 / 2.75) * t + .9375 : 7.5625 * (t -= 2.625 / 2.75) * t + .984375;
            }, bounceInOut: function (e) {
                return .5 > e ? .5 * t.bounceIn(2 * e) : .5 * t.bounceOut(2 * e - 1) + .5;
            } };
        return t;
    }), e("zrender/graphic/shape/Circle", ["require", "../Path"], function (t) {
        "use strict";
        return t("../Path").extend({ type: "circle", shape: { cx: 0, cy: 0, r: 0 }, buildPath: function (t, e, i) {
                i && t.moveTo(e.cx + e.r, e.cy), t.arc(e.cx, e.cy, e.r, 0, 2 * Math.PI, !0);
            } });
    }), e("zrender/graphic/shape/Sector", ["require", "../Path"], function (t) {
        return t("../Path").extend({ type: "sector", shape: { cx: 0, cy: 0, r0: 0, r: 0, startAngle: 0, endAngle: 2 * Math.PI, clockwise: !0 }, buildPath: function (t, e) {
                var i = e.cx, r = e.cy, n = Math.max(e.r0 || 0, 0), a = Math.max(e.r, 0), o = e.startAngle, s = e.endAngle, l = e.clockwise, h = Math.cos(o), u = Math.sin(o);
                t.moveTo(h * n + i, u * n + r), t.lineTo(h * a + i, u * a + r), t.arc(i, r, a, o, s, !l), t.lineTo(Math.cos(s) * n + i, Math.sin(s) * n + r), 0 !== n && t.arc(i, r, n, s, o, l), t.closePath();
            } });
    }), e("zrender/tool/path", ["require", "../graphic/Path", "../core/PathProxy", "./transformPath", "../core/matrix"], function (t) {
        function e(t, e, i, r, n, a, o, s, l, d, v) {
            var m = l * (f / 180), y = c(m) * (t - i) / 2 + u(m) * (e - r) / 2, _ = -1 * u(m) * (t - i) / 2 + c(m) * (e - r) / 2, x = y * y / (o * o) + _ * _ / (s * s);
            x > 1 && (o *= h(x), s *= h(x));
            var b = (n === a ? -1 : 1) * h((o * o * s * s - o * o * _ * _ - s * s * y * y) / (o * o * _ * _ + s * s * y * y)) || 0, w = b * o * _ / s, T = b * -s * y / o, M = (t + i) / 2 + c(m) * w - u(m) * T, z = (e + r) / 2 + u(m) * w + c(m) * T, S = g([1, 0], [(y - w) / o, (_ - T) / s]), C = [(y - w) / o, (_ - T) / s], P = [(-1 * y - w) / o, (-1 * _ - T) / s], A = g(C, P);
            p(C, P) <= -1 && (A = f), p(C, P) >= 1 && (A = 0), 0 === a && A > 0 && (A -= 2 * f), 1 === a && 0 > A && (A += 2 * f), v.addData(d, M, z, o, s, S, A, m, a);
        }
        function i(t) {
            if (!t)
                return [];
            var i, r = t.replace(/-/g, " -").replace(/  /g, " ").replace(/ /g, ",").replace(/,,/g, ",");
            for (i = 0; i < l.length; i++)
                r = r.replace(new RegExp(l[i], "g"), "|" + l[i]);
            var n, o = r.split("|"), s = 0, h = 0, u = new a, c = a.CMD;
            for (i = 1; i < o.length; i++) {
                var f, d = o[i], p = d.charAt(0), g = 0, v = d.slice(1).replace(/e,-/g, "e-").split(",");
                v.length > 0 && "" === v[0] && v.shift();
                for (var m = 0; m < v.length; m++)
                    v[m] = parseFloat(v[m]);
                for (; g < v.length && !isNaN(v[g]) && !isNaN(v[0]);) {
                    var y, _, x, b, w, T, M, z = s, S = h;
                    switch (p) {
                        case "l":
                            s += v[g++], h += v[g++], f = c.L, u.addData(f, s, h);
                            break;
                        case "L":
                            s = v[g++], h = v[g++], f = c.L, u.addData(f, s, h);
                            break;
                        case "m":
                            s += v[g++], h += v[g++], f = c.M, u.addData(f, s, h), p = "l";
                            break;
                        case "M":
                            s = v[g++], h = v[g++], f = c.M, u.addData(f, s, h), p = "L";
                            break;
                        case "h":
                            s += v[g++], f = c.L, u.addData(f, s, h);
                            break;
                        case "H":
                            s = v[g++], f = c.L, u.addData(f, s, h);
                            break;
                        case "v":
                            h += v[g++], f = c.L, u.addData(f, s, h);
                            break;
                        case "V":
                            h = v[g++], f = c.L, u.addData(f, s, h);
                            break;
                        case "C":
                            f = c.C, u.addData(f, v[g++], v[g++], v[g++], v[g++], v[g++], v[g++]), s = v[g - 2], h = v[g - 1];
                            break;
                        case "c":
                            f = c.C, u.addData(f, v[g++] + s, v[g++] + h, v[g++] + s, v[g++] + h, v[g++] + s, v[g++] + h), s += v[g - 2], h += v[g - 1];
                            break;
                        case "S":
                            y = s, _ = h;
                            var C = u.len(), P = u.data;
                            n === c.C && (y += s - P[C - 4], _ += h - P[C - 3]), f = c.C, z = v[g++], S = v[g++], s = v[g++], h = v[g++], u.addData(f, y, _, z, S, s, h);
                            break;
                        case "s":
                            y = s, _ = h;
                            var C = u.len(), P = u.data;
                            n === c.C && (y += s - P[C - 4], _ += h - P[C - 3]), f = c.C, z = s + v[g++], S = h + v[g++], s += v[g++], h += v[g++], u.addData(f, y, _, z, S, s, h);
                            break;
                        case "Q":
                            z = v[g++], S = v[g++], s = v[g++], h = v[g++], f = c.Q, u.addData(f, z, S, s, h);
                            break;
                        case "q":
                            z = v[g++] + s, S = v[g++] + h, s += v[g++], h += v[g++], f = c.Q, u.addData(f, z, S, s, h);
                            break;
                        case "T":
                            y = s, _ = h;
                            var C = u.len(), P = u.data;
                            n === c.Q && (y += s - P[C - 4], _ += h - P[C - 3]), s = v[g++], h = v[g++], f = c.Q, u.addData(f, y, _, s, h);
                            break;
                        case "t":
                            y = s, _ = h;
                            var C = u.len(), P = u.data;
                            n === c.Q && (y += s - P[C - 4], _ += h - P[C - 3]), s += v[g++], h += v[g++], f = c.Q, u.addData(f, y, _, s, h);
                            break;
                        case "A":
                            x = v[g++], b = v[g++], w = v[g++], T = v[g++], M = v[g++], z = s, S = h, s = v[g++], h = v[g++], f = c.A, e(z, S, s, h, T, M, x, b, w, f, u);
                            break;
                        case "a":
                            x = v[g++], b = v[g++], w = v[g++], T = v[g++], M = v[g++], z = s, S = h, s += v[g++], h += v[g++], f = c.A, e(z, S, s, h, T, M, x, b, w, f, u);
                    }
                }
                ("z" === p || "Z" === p) && (f = c.Z, u.addData(f)), n = f;
            }
            return u.toStatic(), u;
        }
        function r(t, e) {
            var r, n = i(t);
            return e = e || {}, e.buildPath = function (t) {
                t.setData(n.data), r && o(t, r);
                var e = t.getContext();
                e && t.rebuildPath(e);
            }, e.applyTransform = function (t) {
                r || (r = s.create()), s.mul(r, t, r);
            }, e;
        }
        var n = t("../graphic/Path"), a = t("../core/PathProxy"), o = t("./transformPath"), s = t("../core/matrix"), l = ["m", "M", "l", "L", "v", "V", "h", "H", "z", "Z", "c", "C", "q", "Q", "t", "T", "s", "S", "a", "A"], h = Math.sqrt, u = Math.sin, c = Math.cos, f = Math.PI, d = function (t) {
            return Math.sqrt(t[0] * t[0] + t[1] * t[1]);
        }, p = function (t, e) {
            return (t[0] * e[0] + t[1] * e[1]) / (d(t) * d(e));
        }, g = function (t, e) {
            return (t[0] * e[1] < t[1] * e[0] ? -1 : 1) * Math.acos(p(t, e));
        };
        return { createFromString: function (t, e) {
                return new n(r(t, e));
            }, extendFromString: function (t, e) {
                return n.extend(r(t, e));
            }, mergePath: function (t, e) {
                for (var i = [], r = t.length, a = 0; r > a; a++) {
                    var o = t[a];
                    o.__dirty && o.buildPath(o.path, o.shape, !0), i.push(o.path);
                }
                var s = new n(e);
                return s.buildPath = function (t) {
                    t.appendPath(i);
                    var e = t.getContext();
                    e && t.rebuildPath(e);
                }, s;
            } };
    }), e("zrender/graphic/shape/Ring", ["require", "../Path"], function (t) {
        return t("../Path").extend({ type: "ring", shape: { cx: 0, cy: 0, r: 0, r0: 0 }, buildPath: function (t, e) {
                var i = e.cx, r = e.cy, n = 2 * Math.PI;
                t.moveTo(i + e.r, r), t.arc(i, r, e.r, 0, n, !1), t.moveTo(i + e.r0, r), t.arc(i, r, e.r0, 0, n, !0);
            } });
    }), e("zrender/graphic/shape/Polygon", ["require", "../helper/poly", "../Path"], function (t) {
        var e = t("../helper/poly");
        return t("../Path").extend({ type: "polygon", shape: { points: null, smooth: !1, smoothConstraint: null }, buildPath: function (t, i) {
                e.buildPath(t, i, !0);
            } });
    }), e("zrender/graphic/shape/Polyline", ["require", "../helper/poly", "../Path"], function (t) {
        var e = t("../helper/poly");
        return t("../Path").extend({ type: "polyline", shape: { points: null, smooth: !1, smoothConstraint: null }, style: { stroke: "#000", fill: null }, buildPath: function (t, i) {
                e.buildPath(t, i, !1);
            } });
    }), e("zrender/graphic/shape/Rect", ["require", "../helper/roundRect", "../Path"], function (t) {
        var e = t("../helper/roundRect");
        return t("../Path").extend({ type: "rect", shape: { r: 0, x: 0, y: 0, width: 0, height: 0 }, buildPath: function (t, i) {
                var r = i.x, n = i.y, a = i.width, o = i.height;
                i.r ? e.buildPath(t, i) : t.rect(r, n, a, o), t.closePath();
            } });
    }), e("zrender/graphic/shape/Line", ["require", "../Path"], function (t) {
        return t("../Path").extend({ type: "line", shape: { x1: 0, y1: 0, x2: 0, y2: 0, percent: 1 }, style: { stroke: "#000", fill: null }, buildPath: function (t, e) {
                var i = e.x1, r = e.y1, n = e.x2, a = e.y2, o = e.percent;
                0 !== o && (t.moveTo(i, r), 1 > o && (n = i * (1 - o) + n * o, a = r * (1 - o) + a * o), t.lineTo(n, a));
            }, pointAt: function (t) {
                var e = this.shape;
                return [e.x1 * (1 - t) + e.x2 * t, e.y1 * (1 - t) + e.y2 * t];
            } });
    }), e("zrender/graphic/shape/BezierCurve", ["require", "../../core/curve", "../../core/vector", "../Path"], function (t) {
        "use strict";
        function e(t, e, i) {
            var r = t.cpx2, n = t.cpy2;
            return null === r || null === n ? [(i ? h : s)(t.x1, t.cpx1, t.cpx2, t.x2, e), (i ? h : s)(t.y1, t.cpy1, t.cpy2, t.y2, e)] : [(i ? l : o)(t.x1, t.cpx1, t.x2, e), (i ? l : o)(t.y1, t.cpy1, t.y2, e)];
        }
        var i = t("../../core/curve"), r = t("../../core/vector"), n = i.quadraticSubdivide, a = i.cubicSubdivide, o = i.quadraticAt, s = i.cubicAt, l = i.quadraticDerivativeAt, h = i.cubicDerivativeAt, u = [];
        return t("../Path").extend({ type: "bezier-curve", shape: { x1: 0, y1: 0, x2: 0, y2: 0, cpx1: 0, cpy1: 0, percent: 1 }, style: { stroke: "#000", fill: null }, buildPath: function (t, e) {
                var i = e.x1, r = e.y1, o = e.x2, s = e.y2, l = e.cpx1, h = e.cpy1, c = e.cpx2, f = e.cpy2, d = e.percent;
                0 !== d && (t.moveTo(i, r), null == c || null == f ? (1 > d && (n(i, l, o, d, u), l = u[1], o = u[2], n(r, h, s, d, u), h = u[1], s = u[2]), t.quadraticCurveTo(l, h, o, s)) : (1 > d && (a(i, l, c, o, d, u), l = u[1], c = u[2], o = u[3], a(r, h, f, s, d, u), h = u[1], f = u[2], s = u[3]), t.bezierCurveTo(l, h, c, f, o, s)));
            }, pointAt: function (t) {
                return e(this.shape, t, !1);
            }, tangentAt: function (t) {
                var i = e(this.shape, t, !0);
                return r.normalize(i, i);
            } });
    }), e("zrender/graphic/CompoundPath", ["require", "./Path"], function (t) {
        var e = t("./Path");
        return e.extend({ type: "compound", shape: { paths: null }, _updatePathDirty: function () {
                for (var t = this.__dirtyPath, e = this.shape.paths, i = 0; i < e.length; i++)
                    t = t || e[i].__dirtyPath;
                this.__dirtyPath = t, this.__dirty = this.__dirty || t;
            }, beforeBrush: function () {
                this._updatePathDirty();
                for (var t = this.shape.paths || [], e = this.getGlobalScale(), i = 0; i < t.length; i++)
                    t[i].path.setScale(e[0], e[1]);
            }, buildPath: function (t, e) {
                for (var i = e.paths || [], r = 0; r < i.length; r++)
                    i[r].buildPath(t, i[r].shape, !0);
            }, afterBrush: function () {
                for (var t = this.shape.paths, e = 0; e < t.length; e++)
                    t[e].__dirtyPath = !1;
            }, getBoundingRect: function () {
                return this._updatePathDirty(), e.prototype.getBoundingRect.call(this);
            } });
    }), e("zrender/graphic/shape/Arc", ["require", "../Path"], function (t) {
        return t("../Path").extend({ type: "arc", shape: { cx: 0, cy: 0, r: 0, startAngle: 0, endAngle: 2 * Math.PI, clockwise: !0 }, style: { stroke: "#000", fill: null }, buildPath: function (t, e) {
                var i = e.cx, r = e.cy, n = Math.max(e.r, 0), a = e.startAngle, o = e.endAngle, s = e.clockwise, l = Math.cos(a), h = Math.sin(a);
                t.moveTo(l * n + i, h * n + r), t.arc(i, r, n, a, o, !s);
            } });
    }), e("zrender/graphic/LinearGradient", ["require", "../core/util", "./Gradient"], function (t) {
        "use strict";
        var e = t("../core/util"), i = t("./Gradient"), r = function (t, e, r, n, a, o) {
            this.x = null == t ? 0 : t, this.y = null == e ? 0 : e, this.x2 = null == r ? 1 : r, this.y2 = null == n ? 0 : n, this.type = "linear", this.global = o || !1, i.call(this, a);
        };
        return r.prototype = { constructor: r }, e.inherits(r, i), r;
    }), e("zrender/graphic/RadialGradient", ["require", "../core/util", "./Gradient"], function (t) {
        "use strict";
        var e = t("../core/util"), i = t("./Gradient"), r = function (t, e, r, n, a) {
            this.x = null == t ? .5 : t, this.y = null == e ? .5 : e, this.r = null == r ? .5 : r, this.type = "radial", this.global = a || !1, i.call(this, n);
        };
        return r.prototype = { constructor: r }, e.inherits(r, i), r;
    }), e("zrender/core/LRU", ["require"], function () {
        var t = function () {
            this.head = null, this.tail = null, this._len = 0;
        }, e = t.prototype;
        e.insert = function (t) {
            var e = new i(t);
            return this.insertEntry(e), e;
        }, e.insertEntry = function (t) {
            this.head ? (this.tail.next = t, t.prev = this.tail, this.tail = t) : this.head = this.tail = t, this._len++;
        }, e.remove = function (t) {
            var e = t.prev, i = t.next;
            e ? e.next = i : this.head = i, i ? i.prev = e : this.tail = e, t.next = t.prev = null, this._len--;
        }, e.len = function () {
            return this._len;
        };
        var i = function (t) {
            this.value = t, this.next, this.prev;
        }, r = function (e) {
            this._list = new t, this._map = {}, this._maxSize = e || 10;
        }, n = r.prototype;
        return n.put = function (t, e) {
            var i = this._list, r = this._map;
            if (null == r[t]) {
                var n = i.len();
                if (n >= this._maxSize && n > 0) {
                    var a = i.head;
                    i.remove(a), delete r[a.key];
                }
                var o = i.insert(e);
                o.key = t, r[t] = o;
            }
        }, n.get = function (t) {
            var e = this._map[t], i = this._list;
            return null != e ? (e !== i.tail && (i.remove(e), i.insertEntry(e)), e.value) : void 0;
        }, n.clear = function () {
            this._list.clear(), this._map = {};
        }, r;
    }), e("zrender/tool/transformPath", ["require", "../core/PathProxy", "../core/vector"], function (t) {
        function e(t, e) {
            var r, l, h, u, c, f, d = t.data, p = i.M, g = i.C, v = i.L, m = i.R, y = i.A, _ = i.Q;
            for (h = 0, u = 0; h < d.length;) {
                switch (r = d[h++], u = h, l = 0, r) {
                    case p:
                        l = 1;
                        break;
                    case v:
                        l = 1;
                        break;
                    case g:
                        l = 3;
                        break;
                    case _:
                        l = 2;
                        break;
                    case y:
                        var x = e[4], b = e[5], w = o(e[0] * e[0] + e[1] * e[1]), T = o(e[2] * e[2] + e[3] * e[3]), M = s(-e[1] / T, e[0] / w);
                        d[h++] += x, d[h++] += b, d[h++] *= w, d[h++] *= T, d[h++] += M, d[h++] += M, h += 2, u = h;
                        break;
                    case m:
                        f[0] = d[h++], f[1] = d[h++], n(f, f, e), d[u++] = f[0], d[u++] = f[1], f[0] += d[h++], f[1] += d[h++], n(f, f, e), d[u++] = f[0], d[u++] = f[1];
                }
                for (c = 0; l > c; c++) {
                    var f = a[c];
                    f[0] = d[h++], f[1] = d[h++], n(f, f, e), d[u++] = f[0], d[u++] = f[1];
                }
            }
        }
        var i = t("../core/PathProxy").CMD, r = t("../core/vector"), n = r.applyTransform, a = [[], [], []], o = Math.sqrt, s = Math.atan2;
        return e;
    }), e("zrender/contain/path", ["require", "../core/PathProxy", "./line", "./cubic", "./quadratic", "./arc", "./util", "../core/curve", "./windingLine"], function (t) {
        "use strict";
        function e(t, e) {
            return Math.abs(t - e) < m;
        }
        function i() {
            var t = _[0];
            _[0] = _[1], _[1] = t;
        }
        function r(t, e, r, n, a, o, s, l, h, u) {
            if (u > e && u > n && u > o && u > l || e > u && n > u && o > u && l > u)
                return 0;
            var c = d.cubicRootAt(e, n, o, l, u, y);
            if (0 === c)
                return 0;
            for (var f, p, g = 0, v = -1, m = 0; c > m; m++) {
                var x = y[m], b = 0 === x || 1 === x ? .5 : 1, w = d.cubicAt(t, r, a, s, x);
                h > w || (0 > v && (v = d.cubicExtrema(e, n, o, l, _), _[1] < _[0] && v > 1 && i(), f = d.cubicAt(e, n, o, l, _[0]), v > 1 && (p = d.cubicAt(e, n, o, l, _[1]))), g += 2 == v ? x < _[0] ? e > f ? b : -b : x < _[1] ? f > p ? b : -b : p > l ? b : -b : x < _[0] ? e > f ? b : -b : f > l ? b : -b);
            }
            return g;
        }
        function n(t, e, i, r, n, a, o, s) {
            if (s > e && s > r && s > a || e > s && r > s && a > s)
                return 0;
            var l = d.quadraticRootAt(e, r, a, s, y);
            if (0 === l)
                return 0;
            var h = d.quadraticExtremum(e, r, a);
            if (h >= 0 && 1 >= h) {
                for (var u = 0, c = d.quadraticAt(e, r, a, h), f = 0; l > f; f++) {
                    var p = 0 === y[f] || 1 === y[f] ? .5 : 1, g = d.quadraticAt(t, i, n, y[f]);
                    o > g || (u += y[f] < h ? e > c ? p : -p : c > a ? p : -p);
                }
                return u;
            }
            var p = 0 === y[0] || 1 === y[0] ? .5 : 1, g = d.quadraticAt(t, i, n, y[0]);
            return o > g ? 0 : e > a ? p : -p;
        }
        function a(t, e, i, r, n, a, o, s) {
            if (s -= e, s > i || -i > s)
                return 0;
            var l = Math.sqrt(i * i - s * s);
            y[0] = -l, y[1] = l;
            var h = Math.abs(r - n);
            if (1e-4 > h)
                return 0;
            if (1e-4 > h % v) {
                r = 0, n = v;
                var u = a ? 1 : -1;
                return o >= y[0] + t && o <= y[1] + t ? u : 0;
            }
            if (a) {
                var l = r;
                r = f(n), n = f(l);
            } else
                r = f(r), n = f(n);
            r > n && (n += v);
            for (var c = 0, d = 0; 2 > d; d++) {
                var p = y[d];
                if (p + t > o) {
                    var g = Math.atan2(s, p), u = a ? 1 : -1;
                    0 > g && (g = v + g), (g >= r && n >= g || g + v >= r && n >= g + v) && (g > Math.PI / 2 && g < 1.5 * Math.PI && (u = -u), c += u);
                }
            }
            return c;
        }
        function o(t, i, o, l, f) {
            for (var d = 0, v = 0, m = 0, y = 0, _ = 0, x = 0; x < t.length;) {
                var b = t[x++];
                switch (b === s.M && x > 1 && (o || (d += p(v, m, y, _, l, f))), 1 == x && (v = t[x], m = t[x + 1], y = v, _ = m), b) {
                    case s.M:
                        y = t[x++], _ = t[x++], v = y, m = _;
                        break;
                    case s.L:
                        if (o) {
                            if (g(v, m, t[x], t[x + 1], i, l, f))
                                return !0;
                        } else
                            d += p(v, m, t[x], t[x + 1], l, f) || 0;
                        v = t[x++], m = t[x++];
                        break;
                    case s.C:
                        if (o) {
                            if (h.containStroke(v, m, t[x++], t[x++], t[x++], t[x++], t[x], t[x + 1], i, l, f))
                                return !0;
                        } else
                            d += r(v, m, t[x++], t[x++], t[x++], t[x++], t[x], t[x + 1], l, f) || 0;
                        v = t[x++], m = t[x++];
                        break;
                    case s.Q:
                        if (o) {
                            if (u.containStroke(v, m, t[x++], t[x++], t[x], t[x + 1], i, l, f))
                                return !0;
                        } else
                            d += n(v, m, t[x++], t[x++], t[x], t[x + 1], l, f) || 0;
                        v = t[x++], m = t[x++];
                        break;
                    case s.A:
                        var w = t[x++], T = t[x++], M = t[x++], z = t[x++], S = t[x++], C = t[x++], P = (t[x++], 1 - t[x++]), A = Math.cos(S) * M + w, k = Math.sin(S) * z + T;
                        x > 1 ? d += p(v, m, A, k, l, f) : (y = A, _ = k);
                        var L = (l - w) * z / M + w;
                        if (o) {
                            if (c.containStroke(w, T, z, S, S + C, P, i, L, f))
                                return !0;
                        } else
                            d += a(w, T, z, S, S + C, P, L, f);
                        v = Math.cos(S + C) * M + w, m = Math.sin(S + C) * z + T;
                        break;
                    case s.R:
                        y = v = t[x++], _ = m = t[x++];
                        var I = t[x++], D = t[x++], A = y + I, k = _ + D;
                        if (o) {
                            if (g(y, _, A, _, i, l, f) || g(A, _, A, k, i, l, f) || g(A, k, y, k, i, l, f) || g(y, k, y, _, i, l, f))
                                return !0;
                        } else
                            d += p(A, _, A, k, l, f), d += p(y, k, y, _, l, f);
                        break;
                    case s.Z:
                        if (o) {
                            if (g(v, m, y, _, i, l, f))
                                return !0;
                        } else
                            d += p(v, m, y, _, l, f);
                        v = y, m = _;
                }
            }
            return o || e(m, _) || (d += p(v, m, y, _, l, f) || 0), 0 !== d;
        }
        var s = t("../core/PathProxy").CMD, l = t("./line"), h = t("./cubic"), u = t("./quadratic"), c = t("./arc"), f = t("./util").normalizeRadian, d = t("../core/curve"), p = t("./windingLine"), g = l.containStroke, v = 2 * Math.PI, m = 1e-4, y = [-1, -1, -1], _ = [-1, -1];
        return { contain: function (t, e, i) {
                return o(t, 0, !1, e, i);
            }, containStroke: function (t, e, i, r) {
                return o(t, e, !0, i, r);
            } };
    }), e("zrender/graphic/Pattern", ["require"], function () {
        var t = function (t, e) {
            this.image = t, this.repeat = e, this.type = "pattern";
        };
        return t.prototype.getCanvasPattern = function (t) {
            return this._canvasPattern || (this._canvasPattern = t.createPattern(this.image, this.repeat));
        }, t;
    }), e("zrender/contain/line", [], function () {
        return { containStroke: function (t, e, i, r, n, a, o) {
                if (0 === n)
                    return !1;
                var s = n, l = 0, h = t;
                if (o > e + s && o > r + s || e - s > o && r - s > o || a > t + s && a > i + s || t - s > a && i - s > a)
                    return !1;
                if (t === i)
                    return Math.abs(a - t) <= s / 2;
                l = (e - r) / (t - i), h = (t * r - i * e) / (t - i);
                var u = l * a - o + h, c = u * u / (l * l + 1);
                return s / 2 * s / 2 >= c;
            } };
    }), e("zrender/contain/quadratic", ["require", "../core/curve"], function (t) {
        var e = t("../core/curve");
        return { containStroke: function (t, i, r, n, a, o, s, l, h) {
                if (0 === s)
                    return !1;
                var u = s;
                if (h > i + u && h > n + u && h > o + u || i - u > h && n - u > h && o - u > h || l > t + u && l > r + u && l > a + u || t - u > l && r - u > l && a - u > l)
                    return !1;
                var c = e.quadraticProjectPoint(t, i, r, n, a, o, l, h, null);
                return u / 2 >= c;
            } };
    }), e("zrender/contain/cubic", ["require", "../core/curve"], function (t) {
        var e = t("../core/curve");
        return { containStroke: function (t, i, r, n, a, o, s, l, h, u, c) {
                if (0 === h)
                    return !1;
                var f = h;
                if (c > i + f && c > n + f && c > o + f && c > l + f || i - f > c && n - f > c && o - f > c && l - f > c || u > t + f && u > r + f && u > a + f && u > s + f || t - f > u && r - f > u && a - f > u && s - f > u)
                    return !1;
                var d = e.cubicProjectPoint(t, i, r, n, a, o, s, l, u, c, null);
                return f / 2 >= d;
            } };
    }), e("zrender/contain/arc", ["require", "./util"], function (t) {
        var e = t("./util").normalizeRadian, i = 2 * Math.PI;
        return { containStroke: function (t, r, n, a, o, s, l, h, u) {
                if (0 === l)
                    return !1;
                var c = l;
                h -= t, u -= r;
                var f = Math.sqrt(h * h + u * u);
                if (f - c > n || n > f + c)
                    return !1;
                if (Math.abs(a - o) % i < 1e-4)
                    return !0;
                if (s) {
                    var d = a;
                    a = e(o), o = e(d);
                } else
                    a = e(a), o = e(o);
                a > o && (o += i);
                var p = Math.atan2(u, h);
                return 0 > p && (p += i), p >= a && o >= p || p + i >= a && o >= p + i;
            } };
    }), e("zrender/contain/util", ["require"], function () {
        var t = 2 * Math.PI;
        return { normalizeRadian: function (e) {
                return e %= t, 0 > e && (e += t), e;
            } };
    }), e("zrender/contain/windingLine", [], function () {
        return function (t, e, i, r, n, a) {
            if (a > e && a > r || e > a && r > a)
                return 0;
            if (r === e)
                return 0;
            var o = e > r ? 1 : -1, s = (a - e) / (r - e);
            (1 === s || 0 === s) && (o = e > r ? .5 : -.5);
            var l = s * (i - t) + t;
            return l > n ? o : 0;
        };
    }), e("zrender/Storage", ["require", "./core/util", "./core/env", "./container/Group", "./core/timsort"], function (t) {
        "use strict";
        function e(t, e) {
            return t.zlevel === e.zlevel ? t.z === e.z ? t.z2 - e.z2 : t.z - e.z : t.zlevel - e.zlevel;
        }
        var i = t("./core/util"), r = t("./core/env"), n = t("./container/Group"), a = t("./core/timsort"), o = function () {
            this._elements = {}, this._roots = [], this._displayList = [], this._displayListLen = 0;
        };
        return o.prototype = { constructor: o, traverse: function (t, e) {
                for (var i = 0; i < this._roots.length; i++)
                    this._roots[i].traverse(t, e);
            }, getDisplayList: function (t, e) {
                return e = e || !1, t && this.updateDisplayList(e), this._displayList;
            }, updateDisplayList: function (t) {
                this._displayListLen = 0;
                for (var i = this._roots, n = this._displayList, o = 0, s = i.length; s > o; o++)
                    this._updateAndAddDisplayable(i[o], null, t);
                n.length = this._displayListLen, r.canvasSupported && a(n, e);
            }, _updateAndAddDisplayable: function (t, e, i) {
                if (!t.ignore || i) {
                    t.beforeUpdate(), t.__dirty && t.update(), t.afterUpdate();
                    var r = t.clipPath;
                    if (r && (r.parent = t, r.updateTransform(), e ? (e = e.slice(), e.push(r)) : e = [r]), t.isGroup) {
                        for (var n = t._children, a = 0; a < n.length; a++) {
                            var o = n[a];
                            t.__dirty && (o.__dirty = !0), this._updateAndAddDisplayable(o, e, i);
                        }
                        t.__dirty = !1;
                    } else
                        t.__clipPaths = e, this._displayList[this._displayListLen++] = t;
                }
            }, addRoot: function (t) {
                this._elements[t.id] || (t instanceof n && t.addChildrenToStorage(this), this.addToMap(t), this._roots.push(t));
            }, delRoot: function (t) {
                if (null == t) {
                    for (var e = 0; e < this._roots.length; e++) {
                        var r = this._roots[e];
                        r instanceof n && r.delChildrenFromStorage(this);
                    }
                    return this._elements = {}, this._roots = [], this._displayList = [], void (this._displayListLen = 0);
                }
                if (t instanceof Array)
                    for (var e = 0, a = t.length; a > e; e++)
                        this.delRoot(t[e]);
                else {
                    var o;
                    o = "string" == typeof t ? this._elements[t] : t;
                    var s = i.indexOf(this._roots, o);
                    s >= 0 && (this.delFromMap(o.id), this._roots.splice(s, 1), o instanceof n && o.delChildrenFromStorage(this));
                }
            }, addToMap: function (t) {
                return t instanceof n && (t.__storage = this), t.dirty(), this._elements[t.id] = t, this;
            }, get: function (t) {
                return this._elements[t];
            }, delFromMap: function (t) {
                var e = this._elements, i = e[t];
                return i && (delete e[t], i instanceof n && (i.__storage = null)), this;
            }, dispose: function () {
                this._elements = this._renderList = this._roots = null;
            }, displayableSortFunc: e }, o;
    }), e("zrender/Painter", ["require", "./config", "./core/util", "./core/log", "./core/BoundingRect", "./core/timsort", "./Layer", "./animation/requestAnimationFrame", "./graphic/Image"], function (t) {
        "use strict";
        function e(t) {
            return parseInt(t, 10);
        }
        function i(t) {
            return t ? t.isBuildin ? !0 : "function" != typeof t.resize || "function" != typeof t.refresh ? !1 : !0 : !1;
        }
        function r(t) {
            t.__unusedCount++;
        }
        function n(t) {
            1 == t.__unusedCount && t.clear();
        }
        function a(t, e, i) {
            return m.copy(t.getBoundingRect()), t.transform && m.applyTransform(t.transform), y.width = e, y.height = i, !m.intersect(y);
        }
        function o(t, e) {
            if (t == e)
                return !1;
            if (!t || !e || t.length !== e.length)
                return !0;
            for (var i = 0; i < t.length; i++)
                if (t[i] !== e[i])
                    return !0;
        }
        function s(t, e) {
            for (var i = 0; i < t.length; i++) {
                var r, n = t[i];
                n.transform && (r = n.transform, e.transform(r[0], r[1], r[2], r[3], r[4], r[5]));
                var a = n.path;
                a.beginPath(e), n.buildPath(a, n.shape), e.clip(), n.transform && (r = n.invTransform, e.transform(r[0], r[1], r[2], r[3], r[4], r[5]));
            }
        }
        function l(t, e) {
            var i = document.createElement("div"), r = i.style;
            return r.position = "relative", r.overflow = "hidden", r.width = t + "px", r.height = e + "px", i;
        }
        var h = t("./config"), u = t("./core/util"), c = t("./core/log"), f = t("./core/BoundingRect"), d = t("./core/timsort"), p = t("./Layer"), g = t("./animation/requestAnimationFrame"), v = 5, m = new f(0, 0, 0, 0), y = new f(0, 0, 0, 0), _ = function (t, e, i) {
            var r = !t.nodeName || "CANVAS" === t.nodeName.toUpperCase();
            i = i || {}, this.dpr = i.devicePixelRatio || h.devicePixelRatio, this._singleCanvas = r, this.root = t;
            var n = t.style;
            n && (n["-webkit-tap-highlight-color"] = "transparent", n["-webkit-user-select"] = n["user-select"] = n["-webkit-touch-callout"] = "none", t.innerHTML = ""), this.storage = e;
            var a = this._zlevelList = [], o = this._layers = {};
            if (this._layerConfig = {}, r) {
                var s = t.width, u = t.height;
                this._width = s, this._height = u;
                var c = new p(t, this, 1);
                c.initContext(), o[0] = c, a.push(0);
            } else {
                this._width = this._getWidth(), this._height = this._getHeight();
                var f = this._domRoot = l(this._width, this._height);
                t.appendChild(f);
            }
            this.pathToImage = this._createPathToImage(), this._progressiveLayers = [], this._hoverlayer, this._hoverElements = [];
        };
        return _.prototype = {
            constructor: _, isSingleCanvas: function () {
                return this._singleCanvas;
            }, getViewportRoot: function () {
                return this._singleCanvas ? this._layers[0].dom : this._domRoot;
            }, refresh: function (t) {
                var e = this.storage.getDisplayList(!0), i = this._zlevelList;
                this._paintList(e, t);
                for (var r = 0; r < i.length; r++) {
                    var n = i[r], a = this._layers[n];
                    !a.isBuildin && a.refresh && a.refresh();
                }
                return this.refreshHover(), this._progressiveLayers.length && this._startProgessive(), this;
            }, addHover: function (t, e) {
                if (!t.__hoverMir) {
                    var i = new t.constructor({ style: t.style, shape: t.shape });
                    i.__from = t, t.__hoverMir = i, i.setStyle(e), this._hoverElements.push(i);
                }
            }, removeHover: function (t) {
                var e = t.__hoverMir, i = this._hoverElements, r = u.indexOf(i, e);
                r >= 0 && i.splice(r, 1), t.__hoverMir = null;
            }, clearHover: function () {
                for (var t = this._hoverElements, e = 0; e < t.length; e++) {
                    var i = t[e].__from;
                    i && (i.__hoverMir = null);
                }
                t.length = 0;
            }, refreshHover: function () {
                var t = this._hoverElements, e = t.length, i = this._hoverlayer;
                if (i && i.clear(), e) {
                    d(t, this.storage.displayableSortFunc), i || (i = this._hoverlayer = this.getLayer(1e5));
                    var r = {};
                    i.ctx.save();
                    for (var n = 0; e > n;) {
                        var a = t[n], o = a.__from;
                        o && o.__zr ? (n++, o.invisible || (a.transform = o.transform, a.invTransform = o.invTransform, a.__clipPaths = o.__clipPaths, this._doPaintEl(a, i, !0, r))) : (t.splice(n, 1), o.__hoverMir = null, e--);
                    }
                    i.ctx.restore();
                }
            }, _startProgessive: function () {
                function t() {
                    i === e._progressiveToken && e.storage && (e._doPaintList(e.storage.getDisplayList()), e._furtherProgressive ? (e._progress++, g(t)) : e._progressiveToken = -1);
                }
                var e = this;
                if (e._furtherProgressive) {
                    var i = e._progressiveToken = +new Date;
                    e._progress++, g(t);
                }
            }, _clearProgressive: function () {
                this._progressiveToken = -1, this._progress = 0, u.each(this._progressiveLayers, function (t) {
                    t.__dirty && t.clear();
                });
            }, _paintList: function (t, e) {
                null == e && (e = !1), this._updateLayerStatus(t), this._clearProgressive(), this.eachBuildinLayer(r), this._doPaintList(t, e), this.eachBuildinLayer(n);
            }, _doPaintList: function (t, e) {
                function i(t) {
                    a.save(), a.globalAlpha = 1, a.shadowBlur = 0, r.__dirty = !0, a.drawImage(t.dom, 0, 0, f, d), a.restore(), r.ctx.restore();
                }
                for (var r, n, a, o, s, l, h = 0, f = this._width, d = this._height, p = this._progress, g = 0, m = t.length; m > g; g++) {
                    var y = t[g], _ = this._singleCanvas ? 0 : y.zlevel, x = y.__frame;
                    if (0 > x && s && (i(s), s = null), n !== _ && (a && a.restore(), o = {}, n = _, r = this.getLayer(n), r.isBuildin || c("ZLevel " + n + " has been used by unkown layer " + r.id), a = r.ctx, a.save(), r.__unusedCount = 0, (r.__dirty || e) && r.clear()), r.__dirty || e) {
                        if (x >= 0) {
                            if (!s) {
                                if (s = this._progressiveLayers[Math.min(h++, v - 1)], s.ctx.save(), s.renderScope = {}, s && s.__progress > s.__maxProgress) {
                                    g = s.__nextIdxNotProg - 1;
                                    continue;
                                }
                                l = s.__progress, s.__dirty || (p = l), s.__progress = p + 1;
                            }
                            x === p && this._doPaintEl(y, s, !0, s.renderScope);
                        } else
                            this._doPaintEl(y, r, e, o);
                        y.__dirty = !1;
                    }
                }
                s && i(s), a && a.restore(), this._furtherProgressive = !1, u.each(this._progressiveLayers, function (t) {
                    t.__maxProgress >= t.__progress && (this._furtherProgressive = !0);
                }, this);
            }, _doPaintEl: function (t, e, i, r) {
                var n = e.ctx;
                if ((e.__dirty || i) && !t.invisible && 0 !== t.style.opacity && t.scale[0] && t.scale[1] && (!t.culling || !a(t, this._width, this._height))) {
                    var l = t.__clipPaths;
                    (r.prevClipLayer !== e || o(l, r.prevElClipPaths)) && (r.prevElClipPaths && (r.prevClipLayer.ctx.restore(), r.prevClipLayer = r.prevElClipPaths = null, r.prevEl = null), l && (n.save(), s(l, n), r.prevClipLayer = e, r.prevElClipPaths = l)), t.beforeBrush && t.beforeBrush(n), t.brush(n, r.prevEl || null), r.prevEl = t, t.afterBrush && t.afterBrush(n);
                }
            }, getLayer: function (t) {
                if (this._singleCanvas)
                    return this._layers[0];
                var e = this._layers[t];
                return e || (e = new p("zr_" + t, this, this.dpr), e.isBuildin = !0, this._layerConfig[t] && u.merge(e, this._layerConfig[t], !0), this.insertLayer(t, e), e.initContext()), e;
            }, insertLayer: function (t, e) {
                var r = this._layers, n = this._zlevelList, a = n.length, o = null, s = -1, l = this._domRoot;
                if (r[t])
                    return void c("ZLevel " + t + " has been used already");
                if (!i(e))
                    return void c("Layer of zlevel " + t + " is not valid");
                if (a > 0 && t > n[0]) {
                    for (s = 0; a - 1 > s && !(n[s] < t && n[s + 1] > t); s++)
                        ;
                    o = r[n[s]];
                }
                if (n.splice(s + 1, 0, t), o) {
                    var h = o.dom;
                    h.nextSibling ? l.insertBefore(e.dom, h.nextSibling) : l.appendChild(e.dom);
                } else
                    l.firstChild ? l.insertBefore(e.dom, l.firstChild) : l.appendChild(e.dom);
                r[t] = e;
            }, eachLayer: function (t, e) {
                var i, r, n = this._zlevelList;
                for (r = 0; r < n.length; r++)
                    i = n[r], t.call(e, this._layers[i], i);
            }, eachBuildinLayer: function (t, e) {
                var i, r, n, a = this._zlevelList;
                for (n = 0; n < a.length; n++)
                    r = a[n], i = this._layers[r], i.isBuildin && t.call(e, i, r);
            }, eachOtherLayer: function (t, e) {
                var i, r, n, a = this._zlevelList;
                for (n = 0; n < a.length; n++)
                    r = a[n], i = this._layers[r], i.isBuildin || t.call(e, i, r);
            }, getLayers: function () {
                return this._layers;
            }, _updateLayerStatus: function (t) {
                var e = this._layers, i = this._progressiveLayers, r = {}, n = {};
                this.eachBuildinLayer(function (t, e) {
                    r[e] = t.elCount, t.elCount = 0, t.__dirty = !1;
                }), u.each(i, function (t, e) {
                    n[e] = t.elCount, t.elCount = 0, t.__dirty = !1;
                });
                for (var a, o, s = 0, l = 0, h = 0, c = t.length; c > h; h++) {
                    var f = t[h], d = this._singleCanvas ? 0 : f.zlevel, g = e[d], m = f.progressive;
                    if (g && (g.elCount++, g.__dirty = g.__dirty || f.__dirty), m >= 0) {
                        o !== m && (o = m, l++);
                        var y = f.__frame = l - 1;
                        if (!a) {
                            var _ = Math.min(s, v - 1);
                            a = i[_], a || (a = i[_] = new p("progressive", this, this.dpr), a.initContext()), a.__maxProgress = 0;
                        }
                        a.__dirty = a.__dirty || f.__dirty, a.elCount++, a.__maxProgress = Math.max(a.__maxProgress, y), a.__maxProgress >= a.__progress && (g.__dirty = !0);
                    } else
                        f.__frame = -1, a && (a.__nextIdxNotProg = h, s++, a = null);
                }
                a && (s++, a.__nextIdxNotProg = h), this.eachBuildinLayer(function (t, e) {
                    r[e] !== t.elCount && (t.__dirty = !0);
                }), i.length = Math.min(s, v), u.each(i, function (t, e) {
                    n[e] !== t.elCount && (f.__dirty = !0), t.__dirty && (t.__progress = 0);
                });
            }, clear: function () {
                return this.eachBuildinLayer(this._clearLayer), this;
            }, _clearLayer: function (t) {
                t.clear();
            }, configLayer: function (t, e) {
                if (e) {
                    var i = this._layerConfig;
                    i[t] ? u.merge(i[t], e, !0) : i[t] = e;
                    var r = this._layers[t];
                    r && u.merge(r, i[t], !0);
                }
            }, delLayer: function (t) {
                var e = this._layers, i = this._zlevelList, r = e[t];
                r && (r.dom.parentNode.removeChild(r.dom), delete e[t], i.splice(u.indexOf(i, t), 1));
            }, resize: function (t, e) {
                var i = this._domRoot;
                if (i.style.display = "none", t = t || this._getWidth(), e = e || this._getHeight(), i.style.display = "", this._width != t || e != this._height) {
                    i.style.width = t + "px", i.style.height = e + "px";
                    for (var r in this._layers)
                        this._layers[r].resize(t, e);
                    this.refresh(!0);
                }
                return this._width = t, this._height = e, this;
            }, clearLayer: function (t) {
                var e = this._layers[t];
                e && e.clear();
            }, dispose: function () {
                this.root.innerHTML = "", this.root = this.storage = this._domRoot = this._layers = null;
            }, getRenderedCanvas: function (t) {
                if (t = t || {}, this._singleCanvas)
                    return this._layers[0].dom;
                var e = new p("image", this, t.pixelRatio || this.dpr);
                e.initContext(), e.clearColor = t.backgroundColor, e.clear();
                for (var i = this.storage.getDisplayList(!0), r = {}, n = 0; n < i.length; n++) {
                    var a = i[n];
                    this._doPaintEl(a, e, !0, r);
                }
                return e.dom;
            }, getWidth: function () {
                return this._width;
            }, getHeight: function () {
                return this._height;
            }, _getWidth: function () {
                var t = this.root, i = document.defaultView.getComputedStyle(t);
                return (t.clientWidth || e(i.width) || e(t.style.width)) - (e(i.paddingLeft) || 0) - (e(i.paddingRight) || 0) | 0;
            }, _getHeight: function () {
                var t = this.root, i = document.defaultView.getComputedStyle(t);
                return (t.clientHeight || e(i.height) || e(t.style.height)) - (e(i.paddingTop) || 0) - (e(i.paddingBottom) || 0) | 0;
            }, _pathToImage: function (e, i, r, n, a) {
                var o = document.createElement("canvas"), s = o.getContext("2d");
                o.width = r * a, o.height = n * a, s.clearRect(0, 0, r * a, n * a);
                var l = { position: i.position, rotation: i.rotation, scale: i.scale };
                i.position = [0, 0, 0], i.rotation = 0, i.scale = [1, 1], i && i.brush(s);
                var h = t("./graphic/Image"), u = new h({ id: e, style: { x: 0, y: 0, image: o } });
                return null != l.position && (u.position = i.position = l.position), null != l.rotation && (u.rotation = i.rotation = l.rotation), null != l.scale && (u.scale = i.scale = l.scale), u;
            }, _createPathToImage: function () {
                var t = this;
                return function (e, i, r, n) {
                    return t._pathToImage(e, i, r, n, t.dpr);
                };
            } }, _;
    }), e("zrender/dom/HandlerProxy", ["require", "../core/event", "../core/util", "../mixin/Eventful", "../core/env", "../core/GestureMgr"], function (t) {
        function e(t) {
            return "mousewheel" === t && u.browser.firefox ? "DOMMouseScroll" : t;
        }
        function i(t, e, i) {
            var r = t._gestureMgr;
            "start" === i && r.clear();
            var n = r.recognize(e, t.handler.findHover(e.zrX, e.zrY, null), t.dom);
            if ("end" === i && r.clear(), n) {
                var a = n.type;
                e.gestureEvent = a, t.handler.dispatchToElement(n.target, a, n.event);
            }
        }
        function r(t) {
            t._touching = !0, clearTimeout(t._touchTimer), t._touchTimer = setTimeout(function () {
                t._touching = !1;
            }, 700);
        }
        function n() {
            return u.touchEventsSupported;
        }
        function a(t) {
            function e(t, e) {
                return function () {
                    return e._touching ? void 0 : t.apply(e, arguments);
                };
            }
            for (var i = 0; i < m.length; i++) {
                var r = m[i];
                t._handlers[r] = l.bind(y[r], t);
            }
            for (var i = 0; i < v.length; i++) {
                var r = v[i];
                t._handlers[r] = e(y[r], t);
            }
        }
        function o(t) {
            function i(i, r) {
                l.each(i, function (i) {
                    f(t, e(i), r._handlers[i]);
                }, r);
            }
            h.call(this), this.dom = t, this._touching = !1, this._touchTimer, this._gestureMgr = new c, this._handlers = {}, a(this), n() && i(m, this), i(v, this);
        }
        var s = t("../core/event"), l = t("../core/util"), h = t("../mixin/Eventful"), u = t("../core/env"), c = t("../core/GestureMgr"), f = s.addEventListener, d = s.removeEventListener, p = s.normalizeEvent, g = 300, v = ["click", "dblclick", "mousewheel", "mouseout", "mouseup", "mousedown", "mousemove"], m = ["touchstart", "touchend", "touchmove"], y = { mousemove: function (t) {
                t = p(this.dom, t), this.trigger("mousemove", t);
            }, mouseout: function (t) {
                t = p(this.dom, t);
                var e = t.toElement || t.relatedTarget;
                if (e != this.dom)
                    for (; e && 9 != e.nodeType;) {
                        if (e === this.dom)
                            return;
                        e = e.parentNode;
                    }
                this.trigger("mouseout", t);
            }, touchstart: function (t) {
                t = p(this.dom, t), this._lastTouchMoment = new Date, i(this, t, "start"), y.mousemove.call(this, t), y.mousedown.call(this, t), r(this);
            }, touchmove: function (t) {
                t = p(this.dom, t), i(this, t, "change"), y.mousemove.call(this, t), r(this);
            }, touchend: function (t) {
                t = p(this.dom, t), i(this, t, "end"), y.mouseup.call(this, t), +new Date - this._lastTouchMoment < g && y.click.call(this, t), r(this);
            } };
        l.each(["click", "mousedown", "mouseup", "mousewheel", "dblclick"], function (t) {
            y[t] = function (e) {
                e = p(this.dom, e), this.trigger(t, e);
            };
        });
        var _ = o.prototype;
        return _.dispose = function () {
            for (var t = v.concat(m), i = 0; i < t.length; i++) {
                var r = t[i];
                d(this.dom, e(r), this._handlers[r]);
            }
        }, _.setCursor = function (t) {
            this.dom.style.cursor = t || "default";
        }, l.mixin(o, h), o;
    }), e("zrender/graphic/helper/poly", ["require", "./smoothSpline", "./smoothBezier"], function (t) {
        var e = t("./smoothSpline"), i = t("./smoothBezier");
        return { buildPath: function (t, r, n) {
                var a = r.points, o = r.smooth;
                if (a && a.length >= 2) {
                    if (o && "spline" !== o) {
                        var s = i(a, o, n, r.smoothConstraint);
                        t.moveTo(a[0][0], a[0][1]);
                        for (var l = a.length, h = 0; (n ? l : l - 1) > h; h++) {
                            var u = s[2 * h], c = s[2 * h + 1], f = a[(h + 1) % l];
                            t.bezierCurveTo(u[0], u[1], c[0], c[1], f[0], f[1]);
                        }
                    } else {
                        "spline" === o && (a = e(a, n)), t.moveTo(a[0][0], a[0][1]);
                        for (var h = 1, d = a.length; d > h; h++)
                            t.lineTo(a[h][0], a[h][1]);
                    }
                    n && t.closePath();
                }
            } };
    }), e("zrender/Handler", ["require", "./core/util", "./mixin/Draggable", "./mixin/Eventful"], function (t) {
        "use strict";
        function e(t, e, i) {
            return { type: t, event: i, target: e, cancelBubble: !1, offsetX: i.zrX, offsetY: i.zrY, gestureEvent: i.gestureEvent, pinchX: i.pinchX, pinchY: i.pinchY, pinchScale: i.pinchScale, wheelDelta: i.zrDelta };
        }
        function i() {
        }
        function r(t, e, i) {
            if (t[t.rectHover ? "rectContain" : "contain"](e, i)) {
                for (var r = t; r;) {
                    if (r.silent || r.clipPath && !r.clipPath.contain(e, i))
                        return !1;
                    r = r.parent;
                }
                return !0;
            }
            return !1;
        }
        var n = t("./core/util"), a = t("./mixin/Draggable"), o = t("./mixin/Eventful");
        i.prototype.dispose = function () {
        };
        var s = ["click", "dblclick", "mousewheel", "mouseout", "mouseup", "mousedown", "mousemove"], l = function (t, e, r) {
            o.call(this), this.storage = t, this.painter = e, r = r || new i, this.proxy = r, r.handler = this, this._hovered, this._lastTouchMoment, this._lastX, this._lastY, a.call(this), n.each(s, function (t) {
                r.on && r.on(t, this[t], this);
            }, this);
        };
        return l.prototype = { constructor: l, mousemove: function (t) {
                var e = t.zrX, i = t.zrY, r = this.findHover(e, i, null), n = this._hovered, a = this.proxy;
                this._hovered = r, a.setCursor && a.setCursor(r ? r.cursor : "default"), n && r !== n && n.__zr && this.dispatchToElement(n, "mouseout", t), this.dispatchToElement(r, "mousemove", t), r && r !== n && this.dispatchToElement(r, "mouseover", t);
            }, mouseout: function (t) {
                this.dispatchToElement(this._hovered, "mouseout", t), this.trigger("globalout", { event: t });
            }, resize: function () {
                this._hovered = null;
            }, dispatch: function (t, e) {
                var i = this[t];
                i && i.call(this, e);
            }, dispose: function () {
                this.proxy.dispose(), this.storage = this.proxy = this.painter = null;
            }, setCursorStyle: function (t) {
                var e = this.proxy;
                e.setCursor && e.setCursor(t);
            }, dispatchToElement: function (t, i, r) {
                for (var n = "on" + i, a = e(i, t, r), o = t; o && (o[n] && (a.cancelBubble = o[n].call(o, a)), o.trigger(i, a), o = o.parent, !a.cancelBubble);)
                    ;
                a.cancelBubble || (this.trigger(i, a), this.painter && this.painter.eachOtherLayer(function (t) {
                    "function" == typeof t[n] && t[n].call(t, a), t.trigger && t.trigger(i, a);
                }));
            }, findHover: function (t, e, i) {
                for (var n = this.storage.getDisplayList(), a = n.length - 1; a >= 0; a--)
                    if (!n[a].silent && n[a] !== i && !n[a].ignore && r(n[a], t, e))
                        return n[a];
            } }, n.each(["click", "mousedown", "mouseup", "mousewheel", "dblclick"], function (t) {
            l.prototype[t] = function (e) {
                var i = this.findHover(e.zrX, e.zrY, null);
                if ("mousedown" === t)
                    this._downel = i, this._upel = i;
                else if ("mosueup" === t)
                    this._upel = i;
                else if ("click" === t && this._downel !== this._upel)
                    return;
                this.dispatchToElement(i, t, e);
            };
        }), n.mixin(l, o), n.mixin(l, a), l;
    }), e("zrender/animation/Animation", ["require", "../core/util", "../core/event", "./requestAnimationFrame", "./Animator"], function (t) {
        "use strict";
        var e = t("../core/util"), i = t("../core/event").Dispatcher, r = t("./requestAnimationFrame"), n = t("./Animator"), a = function (t) {
            t = t || {}, this.stage = t.stage || {}, this.onframe = t.onframe || function () {
            }, this._clips = [], this._running = !1, this._time = 0, i.call(this);
        };
        return a.prototype = { constructor: a, addClip: function (t) {
                this._clips.push(t);
            }, addAnimator: function (t) {
                t.animation = this;
                for (var e = t.getClips(), i = 0; i < e.length; i++)
                    this.addClip(e[i]);
            }, removeClip: function (t) {
                var i = e.indexOf(this._clips, t);
                i >= 0 && this._clips.splice(i, 1);
            }, removeAnimator: function (t) {
                for (var e = t.getClips(), i = 0; i < e.length; i++)
                    this.removeClip(e[i]);
                t.animation = null;
            }, _update: function () {
                for (var t = (new Date).getTime(), e = t - this._time, i = this._clips, r = i.length, n = [], a = [], o = 0; r > o; o++) {
                    var s = i[o], l = s.step(t);
                    l && (n.push(l), a.push(s));
                }
                for (var o = 0; r > o;)
                    i[o]._needsRemove ? (i[o] = i[r - 1], i.pop(), r--) : o++;
                r = n.length;
                for (var o = 0; r > o; o++)
                    a[o].fire(n[o]);
                this._time = t, this.onframe(e), this.trigger("frame", e), this.stage.update && this.stage.update();
            }, start: function () {
                function t() {
                    e._running && (r(t), e._update());
                }
                var e = this;
                this._running = !0, this._time = (new Date).getTime(), r(t);
            }, stop: function () {
                this._running = !1;
            }, clear: function () {
                this._clips = [];
            }, animate: function (t, e) {
                e = e || {};
                var i = new n(t, e.loop, e.getter, e.setter);
                return i;
            } }, e.mixin(a, i), a;
    }), e("zrender/graphic/helper/smoothSpline", ["require", "../../core/vector"], function (t) {
        function e(t, e, i, r, n, a, o) {
            var s = .5 * (i - t), l = .5 * (r - e);
            return (2 * (e - i) + s + l) * o + (-3 * (e - i) - 2 * s - l) * a + s * n + e;
        }
        var i = t("../../core/vector");
        return function (t, r) {
            for (var n = t.length, a = [], o = 0, s = 1; n > s; s++)
                o += i.distance(t[s - 1], t[s]);
            var l = o / 2;
            l = n > l ? n : l;
            for (var s = 0; l > s; s++) {
                var h, u, c, f = s / (l - 1) * (r ? n : n - 1), d = Math.floor(f), p = f - d, g = t[d % n];
                r ? (h = t[(d - 1 + n) % n], u = t[(d + 1) % n], c = t[(d + 2) % n]) : (h = t[0 === d ? d : d - 1], u = t[d > n - 2 ? n - 1 : d + 1], c = t[d > n - 3 ? n - 1 : d + 2]);
                var v = p * p, m = p * v;
                a.push([e(h[0], g[0], u[0], c[0], p, v, m), e(h[1], g[1], u[1], c[1], p, v, m)]);
            }
            return a;
        };
    }), e("zrender/mixin/Draggable", ["require"], function () {
        function t() {
            this.on("mousedown", this._dragStart, this), this.on("mousemove", this._drag, this), this.on("mouseup", this._dragEnd, this), this.on("globalout", this._dragEnd, this);
        }
        return t.prototype = { constructor: t, _dragStart: function (t) {
                var e = t.target;
                e && e.draggable && (this._draggingTarget = e, e.dragging = !0, this._x = t.offsetX, this._y = t.offsetY, this.dispatchToElement(e, "dragstart", t.event));
            }, _drag: function (t) {
                var e = this._draggingTarget;
                if (e) {
                    var i = t.offsetX, r = t.offsetY, n = i - this._x, a = r - this._y;
                    this._x = i, this._y = r, e.drift(n, a, t), this.dispatchToElement(e, "drag", t.event);
                    var o = this.findHover(i, r, e), s = this._dropTarget;
                    this._dropTarget = o, e !== o && (s && o !== s && this.dispatchToElement(s, "dragleave", t.event), o && o !== s && this.dispatchToElement(o, "dragenter", t.event));
                }
            }, _dragEnd: function (t) {
                var e = this._draggingTarget;
                e && (e.dragging = !1), this.dispatchToElement(e, "dragend", t.event), this._dropTarget && this.dispatchToElement(this._dropTarget, "drop", t.event), this._draggingTarget = null, this._dropTarget = null;
            } }, t;
    }), e("zrender/graphic/helper/smoothBezier", ["require", "../../core/vector"], function (t) {
        var e = t("../../core/vector"), i = e.min, r = e.max, n = e.scale, a = e.distance, o = e.add;
        return function (t, s, l, h) {
            var u, c, f, d, p = [], g = [], v = [], m = [];
            if (h) {
                f = [1 / 0, 1 / 0], d = [-1 / 0, -1 / 0];
                for (var y = 0, _ = t.length; _ > y; y++)
                    i(f, f, t[y]), r(d, d, t[y]);
                i(f, f, h[0]), r(d, d, h[1]);
            }
            for (var y = 0, _ = t.length; _ > y; y++) {
                var x = t[y];
                if (l)
                    u = t[y ? y - 1 : _ - 1], c = t[(y + 1) % _];
                else {
                    if (0 === y || y === _ - 1) {
                        p.push(e.clone(t[y]));
                        continue;
                    }
                    u = t[y - 1], c = t[y + 1];
                }
                e.sub(g, c, u), n(g, g, s);
                var b = a(x, u), w = a(x, c), T = b + w;
                0 !== T && (b /= T, w /= T), n(v, g, -b), n(m, g, w);
                var M = o([], x, v), z = o([], x, m);
                h && (r(M, M, f), i(M, M, d), r(z, z, f), i(z, z, d)), p.push(M), p.push(z);
            }
            return l && p.push(p.shift()), p;
        };
    }), e("zrender/core/event", ["require", "../mixin/Eventful"], function (t) {
        "use strict";
        function e(t) {
            return t.getBoundingClientRect ? t.getBoundingClientRect() : { left: 0, top: 0 };
        }
        function i(t, i, r) {
            var n = e(t);
            return r = r || {}, r.zrX = i.clientX - n.left, r.zrY = i.clientY - n.top, r;
        }
        function r(t, e) {
            if (e = e || window.event, null != e.zrX)
                return e;
            var r = e.type, n = r && r.indexOf("touch") >= 0;
            if (n) {
                var a = "touchend" != r ? e.targetTouches[0] : e.changedTouches[0];
                a && i(t, a, e);
            } else
                i(t, e, e), e.zrDelta = e.wheelDelta ? e.wheelDelta / 120 : -(e.detail || 0) / 3;
            return e;
        }
        function n(t, e, i) {
            s ? t.addEventListener(e, i) : t.attachEvent("on" + e, i);
        }
        function a(t, e, i) {
            s ? t.removeEventListener(e, i) : t.detachEvent("on" + e, i);
        }
        var o = t("../mixin/Eventful"), s = "undefined" != typeof window && !!window.addEventListener, l = s ? function (t) {
            t.preventDefault(), t.stopPropagation(), t.cancelBubble = !0;
        } : function (t) {
            t.returnValue = !1, t.cancelBubble = !0;
        };
        return { clientToLocal: i, normalizeEvent: r, addEventListener: n, removeEventListener: a, stop: l, Dispatcher: o };
    }), e("zrender/animation/requestAnimationFrame", ["require"], function () {
        return "undefined" != typeof window && (window.requestAnimationFrame || window.msRequestAnimationFrame || window.mozRequestAnimationFrame || window.webkitRequestAnimationFrame) || function (t) {
            setTimeout(t, 16);
        };
    }), e("zrender/graphic/helper/roundRect", ["require"], function () {
        return { buildPath: function (t, e) {
                var i, r, n, a, o = e.x, s = e.y, l = e.width, h = e.height, u = e.r;
                0 > l && (o += l, l = -l), 0 > h && (s += h, h = -h), "number" == typeof u ? i = r = n = a = u : u instanceof Array ? 1 === u.length ? i = r = n = a = u[0] : 2 === u.length ? (i = n = u[0], r = a = u[1]) : 3 === u.length ? (i = u[0], r = a = u[1], n = u[2]) : (i = u[0], r = u[1], n = u[2], a = u[3]) : i = r = n = a = 0;
                var c;
                i + r > l && (c = i + r, i *= l / c, r *= l / c), n + a > l && (c = n + a, n *= l / c, a *= l / c), r + n > h && (c = r + n, r *= h / c, n *= h / c), i + a > h && (c = i + a, i *= h / c, a *= h / c), t.moveTo(o + i, s), t.lineTo(o + l - r, s), 0 !== r && t.quadraticCurveTo(o + l, s, o + l, s + r), t.lineTo(o + l, s + h - n), 0 !== n && t.quadraticCurveTo(o + l, s + h, o + l - n, s + h), t.lineTo(o + a, s + h), 0 !== a && t.quadraticCurveTo(o, s + h, o, s + h - a), t.lineTo(o, s + i), 0 !== i && t.quadraticCurveTo(o, s, o + i, s);
            } };
    }), e("zrender/core/GestureMgr", ["require", "./event"], function (t) {
        "use strict";
        function e(t) {
            var e = t[1][0] - t[0][0], i = t[1][1] - t[0][1];
            return Math.sqrt(e * e + i * i);
        }
        function i(t) {
            return [(t[0][0] + t[1][0]) / 2, (t[0][1] + t[1][1]) / 2];
        }
        var r = t("./event"), n = function () {
            this._track = [];
        };
        n.prototype = { constructor: n, recognize: function (t, e, i) {
                return this._doTrack(t, e, i), this._recognize(t);
            }, clear: function () {
                return this._track.length = 0, this;
            }, _doTrack: function (t, e, i) {
                var n = t.touches;
                if (n) {
                    for (var a = { points: [], touches: [], target: e, event: t }, o = 0, s = n.length; s > o; o++) {
                        var l = n[o], h = r.clientToLocal(i, l);
                        a.points.push([h.zrX, h.zrY]), a.touches.push(l);
                    }
                    this._track.push(a);
                }
            }, _recognize: function (t) {
                for (var e in a)
                    if (a.hasOwnProperty(e)) {
                        var i = a[e](this._track, t);
                        if (i)
                            return i;
                    }
            } };
        var a = { pinch: function (t, r) {
                var n = t.length;
                if (n) {
                    var a = (t[n - 1] || {}).points, o = (t[n - 2] || {}).points || a;
                    if (o && o.length > 1 && a && a.length > 1) {
                        var s = e(a) / e(o);
                        !isFinite(s) && (s = 1), r.pinchScale = s;
                        var l = i(a);
                        return r.pinchX = l[0], r.pinchY = l[1], { type: "pinch", target: t[0].target, event: r };
                    }
                }
            } };
        return n;
    }), e("zrender/Layer", ["require", "./core/util", "./config", "./graphic/Style", "./graphic/Pattern"], function (t) {
        function e() {
            return !1;
        }
        function i(t, e, i, r) {
            var n = document.createElement(e), a = i.getWidth(), o = i.getHeight(), s = n.style;
            return s.position = "absolute", s.left = 0, s.top = 0, s.width = a + "px", s.height = o + "px", n.width = a * r, n.height = o * r, n.setAttribute("data-zr-dom-id", t), n;
        }
        var r = t("./core/util"), n = t("./config"), a = t("./graphic/Style"), o = t("./graphic/Pattern"), s = function (t, a, o) {
            var s;
            o = o || n.devicePixelRatio, "string" == typeof t ? s = i(t, "canvas", a, o) : r.isObject(t) && (s = t, t = s.id), this.id = t, this.dom = s;
            var l = s.style;
            l && (s.onselectstart = e, l["-webkit-user-select"] = "none", l["user-select"] = "none", l["-webkit-touch-callout"] = "none", l["-webkit-tap-highlight-color"] = "rgba(0,0,0,0)"), this.domBack = null, this.ctxBack = null, this.painter = a, this.config = null, this.clearColor = 0, this.motionBlur = !1, this.lastFrameAlpha = .7, this.dpr = o;
        };
        return s.prototype = { constructor: s, elCount: 0, __dirty: !0, initContext: function () {
                this.ctx = this.dom.getContext("2d");
                var t = this.dpr;
                1 != t && this.ctx.scale(t, t);
            }, createBackBuffer: function () {
                var t = this.dpr;
                this.domBack = i("back-" + this.id, "canvas", this.painter, t), this.ctxBack = this.domBack.getContext("2d"), 1 != t && this.ctxBack.scale(t, t);
            }, resize: function (t, e) {
                var i = this.dpr, r = this.dom, n = r.style, a = this.domBack;
                n.width = t + "px", n.height = e + "px", r.width = t * i, r.height = e * i, 1 != i && this.ctx.scale(i, i), a && (a.width = t * i, a.height = e * i, 1 != i && this.ctxBack.scale(i, i));
            }, clear: function (t) {
                var e = this.dom, i = this.ctx, r = e.width, n = e.height, s = this.clearColor, l = this.motionBlur && !t, h = this.lastFrameAlpha, u = this.dpr;
                if (l && (this.domBack || this.createBackBuffer(), this.ctxBack.globalCompositeOperation = "copy", this.ctxBack.drawImage(e, 0, 0, r / u, n / u)), i.clearRect(0, 0, r / u, n / u), s) {
                    var c;
                    s.colorStops ? (c = s.__canvasGradient || a.getGradient(i, s, { x: 0, y: 0, width: r / u, height: n / u }), s.__canvasGradient = c) : s.image && (c = o.prototype.getCanvasPattern.call(s, i)), i.save(), i.fillStyle = c || s, i.fillRect(0, 0, r / u, n / u), i.restore();
                }
                if (l) {
                    var f = this.domBack;
                    i.save(), i.globalAlpha = h, i.drawImage(f, 0, 0, r / u, n / u), i.restore();
                }
            } }, s;
    }), e("echarts/preprocessor/helper/compatStyle", ["require", "zrender/core/util"], function (t) {
        function e(t) {
            var e = t && t.itemStyle;
            e && i.each(r, function (r) {
                var n = e.normal, a = e.emphasis;
                n && n[r] && (t[r] = t[r] || {}, t[r].normal ? i.merge(t[r].normal, n[r]) : t[r].normal = n[r], n[r] = null), a && a[r] && (t[r] = t[r] || {}, t[r].emphasis ? i.merge(t[r].emphasis, a[r]) : t[r].emphasis = a[r], a[r] = null);
            });
        }
        var i = t("zrender/core/util"), r = ["areaStyle", "lineStyle", "nodeStyle", "linkStyle", "chordStyle", "label", "labelLine"];
        return function (t) {
            if (t) {
                e(t), e(t.markPoint), e(t.markLine);
                var r = t.data;
                if (r) {
                    for (var n = 0; n < r.length; n++)
                        e(r[n]);
                    var a = t.markPoint;
                    if (a && a.data)
                        for (var o = a.data, n = 0; n < o.length; n++)
                            e(o[n]);
                    var s = t.markLine;
                    if (s && s.data)
                        for (var l = s.data, n = 0; n < l.length; n++)
                            i.isArray(l[n]) ? (e(l[n][0]), e(l[n][1])) : e(l[n]);
                }
            }
        };
    }), e("echarts/data/DataDiffer", ["require"], function () {
        "use strict";
        function t(t) {
            return t;
        }
        function e(e, i, r, n) {
            this._old = e, this._new = i, this._oldKeyGetter = r || t, this._newKeyGetter = n || t;
        }
        function i(t, e, i, r) {
            for (var n = 0; n < t.length; n++) {
                var a = r(t[n], n), o = e[a];
                null == o ? (i.push(a), e[a] = n) : (o.length || (e[a] = o = [o]), o.push(n));
            }
        }
        return e.prototype = { constructor: e, add: function (t) {
                return this._add = t, this;
            }, update: function (t) {
                return this._update = t, this;
            }, remove: function (t) {
                return this._remove = t, this;
            }, execute: function () {
                var t, e = this._old, r = this._new, n = this._oldKeyGetter, a = this._newKeyGetter, o = {}, s = {}, l = [], h = [];
                for (i(e, o, l, n), i(r, s, h, a), t = 0; t < e.length; t++) {
                    var u = l[t], c = s[u];
                    if (null != c) {
                        var f = c.length;
                        f ? (1 === f && (s[u] = null), c = c.unshift()) : s[u] = null, this._update && this._update(c, t);
                    } else
                        this._remove && this._remove(t);
                }
                for (var t = 0; t < h.length; t++) {
                    var u = h[t];
                    if (s.hasOwnProperty(u)) {
                        var c = s[u];
                        if (null == c)
                            continue;
                        if (c.length)
                            for (var d = 0, f = c.length; f > d; d++)
                                this._add && this._add(c[d]);
                        else
                            this._add && this._add(c);
                    }
                }
            } }, e;
    }), e("echarts/data/helper/completeDimensions", ["require", "zrender/core/util"], function (t) {
        function e(t, e, a, o) {
            if (!e)
                return t;
            var s = i(e[0]), l = r.isArray(s) && s.length || 1;
            a = a || [], o = o || "extra";
            for (var h = 0; l > h; h++)
                if (!t[h]) {
                    var u = a[h] || o + (h - a.length);
                    t[h] = n(e, h) ? { type: "ordinal", name: u } : u;
                }
            return t;
        }
        function i(t) {
            return r.isArray(t) ? t : r.isObject(t) ? t.value : t;
        }
        var r = t("zrender/core/util"), n = e.guessOrdinal = function (t, e) {
            for (var n = 0, a = t.length; a > n; n++) {
                var o = i(t[n]);
                if (!r.isArray(o))
                    return !1;
                var o = o[e];
                if (null != o && isFinite(o))
                    return !1;
                if (r.isString(o) && "-" !== o)
                    return !0;
            }
            return !1;
        };
        return e;
    }), e("echarts/component/helper/selectableMixin", ["require", "zrender/core/util"], function (t) {
        var e = t("zrender/core/util");
        return { updateSelectedMap: function (t) {
                this._selectTargetMap = e.reduce(t || [], function (t, e) {
                    return t[e.name] = e, t;
                }, {});
            }, select: function (t) {
                var i = this._selectTargetMap, r = i[t], n = this.get("selectedMode");
                "single" === n && e.each(i, function (t) {
                    t.selected = !1;
                }), r && (r.selected = !0);
            }, unSelect: function (t) {
                var e = this._selectTargetMap[t];
                e && (e.selected = !1);
            }, toggleSelected: function (t) {
                var e = this._selectTargetMap[t];
                return null != e ? (this[e.selected ? "unSelect" : "select"](t), e.selected) : void 0;
            }, isSelected: function (t) {
                var e = this._selectTargetMap[t];
                return e && e.selected;
            } };
    }), e("echarts/component/tooltip/TooltipContent", ["require", "zrender/core/util", "zrender/tool/color", "zrender/core/event", "../../util/format", "zrender/core/env"], function (t) {
        function e(t) {
            var e = "cubic-bezier(0.23, 1, 0.32, 1)", i = "left " + t + "s " + e + ",top " + t + "s " + e;
            return o.map(d, function (t) {
                return t + "transition:" + i;
            }).join(";");
        }
        function i(t) {
            var e = [], i = t.get("fontSize"), r = t.getTextColor();
            return r && e.push("color:" + r), e.push("font:" + t.getFont()), i && e.push("line-height:" + Math.round(3 * i / 2) + "px"), u(["decoration", "align"], function (i) {
                var r = t.get(i);
                r && e.push("text-" + i + ":" + r);
            }), e.join(";");
        }
        function r(t) {
            t = t;
            var r = [], n = t.get("transitionDuration"), a = t.get("backgroundColor"), o = t.getModel("textStyle"), l = t.get("padding");
            return n && r.push(e(n)), a && (f.canvasSupported ? r.push("background-Color:" + a) : (r.push("background-Color:#" + s.toHex(a)), r.push("filter:alpha(opacity=70)"))), u(["width", "color", "radius"], function (e) {
                var i = "border-" + e, n = c(i), a = t.get(n);
                null != a && r.push(i + ":" + a + ("color" === e ? "" : "px"));
            }), r.push(i(o)), null != l && r.push("padding:" + h.normalizeCssArray(l).join("px ") + "px"), r.join(";") + ";";
        }
        function n(t, e) {
            var i = document.createElement("div"), r = e.getZr();
            this.el = i, this._x = e.getWidth() / 2, this._y = e.getHeight() / 2, t.appendChild(i), this._container = t, this._show = !1, this._hideTimeout;
            var n = this;
            i.onmouseenter = function () {
                n.enterable && (clearTimeout(n._hideTimeout), n._show = !0), n._inContent = !0;
            }, i.onmousemove = function (e) {
                if (!n.enterable) {
                    var i = r.handler;
                    l.normalizeEvent(t, e), i.dispatch("mousemove", e);
                }
            }, i.onmouseleave = function () {
                n.enterable && n._show && n.hideLater(n._hideDelay), n._inContent = !1;
            }, a(i, t);
        }
        function a(t, e) {
            function i(t) {
                r(t.target) && t.preventDefault();
            }
            function r(i) {
                for (; i && i !== e;) {
                    if (i === t)
                        return !0;
                    i = i.parentNode;
                }
            }
            l.addEventListener(e, "touchstart", i), l.addEventListener(e, "touchmove", i), l.addEventListener(e, "touchend", i);
        }
        var o = t("zrender/core/util"), s = t("zrender/tool/color"), l = t("zrender/core/event"), h = t("../../util/format"), u = o.each, c = h.toCamelCase, f = t("zrender/core/env"), d = ["", "-webkit-", "-moz-", "-o-"], p = "position:absolute;display:block;border-style:solid;white-space:nowrap;z-index:9999999;";
        return n.prototype = { constructor: n, enterable: !0, update: function () {
                var t = this._container, e = t.currentStyle || document.defaultView.getComputedStyle(t), i = t.style;
                "absolute" !== i.position && "absolute" !== e.position && (i.position = "relative");
            }, show: function (t) {
                clearTimeout(this._hideTimeout);
                var e = this.el;
                e.style.cssText = p + r(t) + ";left:" + this._x + "px;top:" + this._y + "px;" + (t.get("extraCssText") || ""), e.style.display = e.innerHTML ? "block" : "none", this._show = !0;
            }, setContent: function (t) {
                var e = this.el;
                e.innerHTML = t, e.style.display = t ? "block" : "none";
            }, moveTo: function (t, e) {
                var i = this.el.style;
                i.left = t + "px", i.top = e + "px", this._x = t, this._y = e;
            }, hide: function () {
                this.el.style.display = "none", this._show = !1;
            }, hideLater: function (t) {
                !this._show || this._inContent && this.enterable || (t ? (this._hideDelay = t, this._show = !1, this._hideTimeout = setTimeout(o.bind(this.hide, this), t)) : this.hide());
            }, isShow: function () {
                return this._show;
            } }, n;
    }), e("echarts/chart/pie/labelLayout", ["require", "zrender/contain/text"], function (t) {
        "use strict";
        function e(t, e, i, r, n, a, o) {
            function s(e, i, r) {
                for (var n = e; i > n; n++)
                    if (t[n].y += r, n > e && i > n + 1 && t[n + 1].y > t[n].y + t[n].height)
                        return void l(n, r / 2);
                l(i - 1, r / 2);
            }
            function l(e, i) {
                for (var r = e; r >= 0 && (t[r].y -= i, !(r > 0 && t[r].y > t[r - 1].y + t[r - 1].height)); r--)
                    ;
            }
            function h(t, e, i, r, n, a) {
                for (var o = a > 0 ? e ? Number.MAX_VALUE : 0 : e ? Number.MAX_VALUE : 0, s = 0, l = t.length; l > s; s++)
                    if ("center" !== t[s].position) {
                        var h = Math.abs(t[s].y - r), u = t[s].len, c = t[s].len2, f = n + u > h ? Math.sqrt((n + u + c) * (n + u + c) - h * h) : Math.abs(t[s].x - i);
                        e && f >= o && (f = o - 10), !e && o >= f && (f = o + 10), t[s].x = i + f * a, o = f;
                    }
            }
            t.sort(function (t, e) {
                return t.y - e.y;
            });
            for (var u, c = 0, f = t.length, d = [], p = [], g = 0; f > g; g++)
                u = t[g].y - c, 0 > u && s(g, f, -u, n), c = t[g].y + t[g].height;
            0 > o - c && l(f - 1, c - o);
            for (var g = 0; f > g; g++)
                t[g].y >= i ? p.push(t[g]) : d.push(t[g]);
            h(d, !1, e, i, r, n), h(p, !0, e, i, r, n);
        }
        function i(t, i, r, n, a, o) {
            for (var s = [], l = [], h = 0; h < t.length; h++)
                t[h].x < i ? s.push(t[h]) : l.push(t[h]);
            e(l, i, r, n, 1, a, o), e(s, i, r, n, -1, a, o);
            for (var h = 0; h < t.length; h++) {
                var u = t[h].linePoints;
                if (u) {
                    var c = u[1][0] - u[2][0];
                    u[2][0] = t[h].x < i ? t[h].x + 3 : t[h].x - 3, u[1][1] = u[2][1] = t[h].y, u[1][0] = u[2][0] + c;
                }
            }
        }
        var r = t("zrender/contain/text");
        return function (t, e, n, a) {
            var o, s, l = t.getData(), h = [], u = !1;
            l.each(function (i) {
                var n, a, c, f, d = l.getItemLayout(i), p = l.getItemModel(i), g = p.getModel("label.normal"), v = g.get("position") || p.get("label.emphasis.position"), m = p.getModel("labelLine.normal"), y = m.get("length"), _ = m.get("length2"), x = (d.startAngle + d.endAngle) / 2, b = Math.cos(x), w = Math.sin(x);
                o = d.cx, s = d.cy;
                var T = "inside" === v || "inner" === v;
                if ("center" === v)
                    n = d.cx, a = d.cy, f = "center";
                else {
                    var M = (T ? (d.r + d.r0) / 2 * b : d.r * b) + o, z = (T ? (d.r + d.r0) / 2 * w : d.r * w) + s;
                    if (n = M + 3 * b, a = z + 3 * w, !T) {
                        var S = M + b * (y + e - d.r), C = z + w * (y + e - d.r), P = S + (0 > b ? -1 : 1) * _, A = C;
                        n = P + (0 > b ? -5 : 5), a = A, c = [[M, z], [S, C], [P, A]];
                    }
                    f = T ? "center" : b > 0 ? "left" : "right";
                }
                var k = g.getModel("textStyle").getFont(), L = g.get("rotate") ? 0 > b ? -x + Math.PI : -x : 0, I = t.getFormattedLabel(i, "normal") || l.getName(i), D = r.getBoundingRect(I, k, f, "top");
                u = !!L, d.label = { x: n, y: a, position: v, height: D.height, len: y, len2: _, linePoints: c, textAlign: f, verticalAlign: "middle", font: k, rotation: L }, T || h.push(d.label);
            }), !u && t.get("avoidLabelOverlap") && i(h, o, s, e, n, a);
        };
    }), e("zrender", ["zrender/zrender"], function (t) {
        return t;
    }), e("echarts", ["echarts/echarts"], function (t) {
        return t;
    });
    var i = t("echarts");
    return i.graphic = t("echarts/util/graphic"), i.number = t("echarts/util/number"), i.format = t("echarts/util/format"), t("echarts/chart/pie"), t("echarts/component/tooltip"), t("zrender/vml/vml"), i;
});
;
angular.module("financialCalendar", []).directive("financialCalendar", ["$globalData", "$httpPlus", "$utb", "$log", "$interval", "$filter", function (n, e, a, t, i, r) {
        return { restrict: "A", template: '<div class="date-list" ng-show="showCalendar"><div class="rili e-calendar-licai"><div id="calendar"></div></div></div>', scope: { onInit: "&", onChange: "&", changeMonth: "&" }, link: function (n, e) {
                function a(a) {
                    d = e.find("#calendar"), d.eCalendar({ today: a.today, weekDays: ["周日", "周一", "周二", "周三", "周四", "周五", "周六"], months: ["1月", "2月", "3月", "4月", "5月", "6月", "7月", "8月", "9月", "10月", "11月", "12月"], textArrows: { previous: "", previousClick: i, next: "", nextClick: i }, days: { listener: { click: t } }, events: a.events }), s = a.events, n.showCalendar = !0;
                }
                function t() {
                    var e = d.getDate();
                    if (null != e) {
                        var a = { endDate: e, edu: o(e) };
                        n.showCalendar = !1, n.onChange({ selectDateOption: a });
                    }
                }
                function i() {
                    var e = r("date")(d.getMonth(), "yyyyMM");
                    n.changeMonth({ monthOption: { nowMonth: e } });
                }
                function l(e) {
                    s = e, d.setEvents(e), d.renderEvents(), n.showCalendar = !0;
                }
                function o(n) {
                    for (var e = 0, a = 0; a < s.length; a++) {
                        var t = s[a].totUseLimit, i = s[a].datetime, i = r("date")(i, "yyyyMMdd"), l = r("date")(n, "yyyyMMdd");
                        l == i && (e = t);
                    }
                    return e;
                }
                var d = null, s = [];
                n.showCalendar = !1;
                var c = { initCalendar: a, reviewCalendar: l };
                n.onInit({ $operator: c });
            } };
    }]);
;
angular.module("advertise", []).directive("advertise", ["$utb", "$message", "$routeHelper", function (e, n, s) {
        function o(n) {
            return e.getAdviceInfo(n);
        }
        function a(e, n) {
            var s, o = {};
            for (s = 0; e && s < e.length; s++)
                o[e[s][n]] = e[s];
            return o;
        }
        function t(n) {
            var s, o, a, t = n.TemplateCode, r = (n.prodId, n.InterestDays, (n.IpoStartDate || "").substring(4).replace(/(\d{2})(\d{2})/g, "$1/$2")), i = (n.IpoEndDate || "").substring(4).replace(/(\d{2})(\d{2})/g, "$1/$2"), c = n.IncomeDate || "", d = n.EndDate || "", l = n.s_ProdUseLimits;
            s = 0 == l.totLimit || null == l.totLimit ? "0%" : 100 * (1 - (parseFloat(l.personUseLimit) + parseFloat(l.adjustUseLimit)) / parseFloat(l.totLimit)) + "%", o = parseInt(l.personUseLimit, 10) + parseInt(l.adjustUseLimit, 10), a = o / 1e4, n.s_usedPercent = s, n.s_sellUseLimitAmtChinese = a, n.s_ModelComment = (n.ModelComment || "").replace(/\%/g, ""), "1102" == t && null != n.IncomeEndDate && (d = n.IncomeEndDate);
            var p = (c || "").substring(4).replace(/(\d{2})(\d{2})/g, "$1/$2"), f = (d || "").substring(4).replace(/(\d{2})(\d{2})/g, "$1/$2");
            n.s_ipoDateFormat_show = "募集期限", "1102" == t ? (n.s_ipoDateFormat = r + "-" + i, n.s_incomeDateFormat = p + "-" + f, n.s_interestDaysFormat = n.InterestDays) : "1402" == t ? (n.s_ipoDateFormat = "每天", n.s_incomeDateFormat = "任意", n.s_interestDaysFormat = n.CycleDays) : "1407" == t ? (n.s_ipoDateFormat = "每天", n.s_incomeDateFormat = "自主选择", n.s_interestDaysFormat = "自选", n.s_interestDaysFormatFlag = !0, n.s_ipoDateFormat_show = "认购时间") : "9101" == t ? (n.s_ipoDateFormat = "每个工作日", n.s_incomeDateFormat = "任意", n.s_interestDaysFormat = "活期", n.s_interestDaysFormatFlag = !0) : "1201" == t ? (n.s_ipoDateFormat = "每个工作日", n.s_incomeDateFormat = "任意", n.s_interestDaysFormat = "活期", n.s_interestDaysFormatFlag = !0) : "9801" == t ? (n.s_ipoDateFormat = "每天", n.s_incomeDateFormat = n.CycleDays, n.s_interestDaysFormat = n.CycleDays) : "9901" == t || "9905" == t ? (n.s_ipoDateFormat = "每个工作日", n.s_incomeDateFormat = "任意", n.s_interestDaysFormat = "活期", n.s_interestDaysFormatFlag = !0) : (n.s_ipoDateFormat = "每个工作日", n.s_incomeDateFormat = "任意", n.s_interestDaysFormat = "18天");
            var u = n.IpoStartDate, g = n.OpenTime, _ = g.replace(/[\D]/g, "").length;
            5 == _ && (g = "0" + g);
            var I = g.substring(0, 2) + ":", v = g.substring(2, 4) + ":", h = g.substring(4, 6);
            if (g = I + v + h, "1102" == t) {
                var D = $globalData.getSystemTime().getTime(), F = e.parseDate(u.substring(0, 8) + " " + g, "yyyyMMdd hh:mm:ss").getTime(), y = (F - D) / 1e3;
                n.leftTime = y;
            }
            n.dateOne = (new Date).getTime(), m(n);
        }
        function m(e) {
            r(e), $interval(function () {
                var n = (new Date).getTime(), s = (n - e.dateOne) / 1e3;
                r(e, s);
            }, 1e3);
        }
        function r(e, n) {
            var s = e.leftTime - n;
            if (!(120 >= s && s > 0))
                return void (e.timeTwo = !1);
            e.timeTwo = !0, e.minutesRe = Math.floor(s / 60);
            var o = Math.floor(s % 60);
            e.secondsRe = 10 > o ? "0" + o : o;
        }
        function i(n) {
            e.isLogin() ? s.jump("/HB00301_confirm", { term: n.saveTypeId }) : e.login(function (e) {
                "2" == e && s.jump("/HB00301_confirm", { term: n.saveTypeId });
            });
        }
        function c(n) {
            e.isLogin() ? s.jump("/HB01104_buy", n) : e.login(function (e) {
                "2" == e && s.jump("/HB01104_buy", n);
            });
        }
        function d(e, n) {
            if (!e)
                return [];
            for (var s = e.split("|"), o = [], a = 0; a < s.length; a++) {
                var t = s[a], m = {};
                if (t) {
                    var r = t.split("@")[0], i = t.split("@")[1], c = t.split("@")[2];
                    n == c && (m.picUrl = r, m.picSeq = i, m.picType = c, o.push(m));
                }
            }
            return o;
        }
        function l(e) {
            s.jump("/HB00601_goldDetail", { goldId: e });
        }
        function p(e) {
            var n = [];
            if (null != e && "" != e)
                for (var s = e.split("★").length, o = 0; s - 1 > o; o++)
                    n[o] = o;
            return n;
        }
        return { restrict: "EA", template: '<div class="advertise-conduct-other" ng-if="iRecommendList.length > 0"><p class="advertise-look-other">您还可以看看</p><div class="advertise-other"><div class="advertise-other-list" ng-repeat="recommendInfo in iRecommendList"><div ng-if="recommendInfo.s_type == \'1\'" class="finance"><h3>{{recommendInfo.prodName}}</h3><div class="num-main" ng-show="recommendInfo.TemplateCode != 9901 && recommendInfo.TemplateCode != 9801 && recommendInfo.TemplateCode != 9905"><span class="big_num">{{recommendInfo.s_ModelComment}}</span><span class="percent">%</span><br>                                          预期年化收益</div><div class="num-main" ng-show="recommendInfo.TemplateCode == 9901 || recommendInfo.TemplateCode == 9801 && recommendInfo.TemplateCode == 9905"><span class="nav_num2">{{recommendInfo.Nav}}</span><br>                                         单位净值</div><div class="num-main"><span class="big_num" ng-class="{\'fontCon\': recommendInfo.s_interestDaysFormatFlag}">{{recommendInfo.s_interestDaysFormat}}</span><span class="unit" ng-show="recommendInfo.TemplateCode != 1201 && recommendInfo.TemplateCode != 9101 && recommendInfo.TemplateCode != 9901 && recommendInfo.TemplateCode != 1407 && recommendInfo.TemplateCode != 9905">&nbsp;天</span><br>               投资期限</div><p class="first">{{recommendInfo.s_ipoDateFormat_show}}<span>{{recommendInfo.s_ipoDateFormat}}</span></p><br><p>理财期限<span>{{recommendInfo.s_incomeDateFormat}}</span></p><div class="re_amount">地区可售额度<span class="placeholder12"></span><span class="unit" ng-show="recommendInfo.sellUseLimitAmtChinese != 0">￥{{recommendInfo.s_sellUseLimitAmtChinese | money }}万</span><span class="unit" ng-show="recommendInfo.sellUseLimitAmtChinese == 0">全部售完</span></div><div class="progress"><div class="progress_val" ng-style="{width : recommendInfo.s_usedPercent}"></div></div><a class="btn1" ng-show="recommendInfo.s_sellUseLimitAmtChinese != \'0\' && recommendInfo.timeTwo==true" ng-click="">{{recommendInfo.minutesRe}}:{{recommendInfo.secondsRe}}后开售</a><a class="btn1" ng-show="recommendInfo.timeTwo!=true" ng-click="recommendInfo.s_sellUseLimitAmtChinese != \'0\' ? recommendInfo.s_go(\'/HB00101_buyConfirm\', {\'prodId\': recommendInfo.prodId,\'prodName\': recommendInfo.prodName}): recommendInfo.s_alertPop(\'该产品目前已无可售额度。\')">立即购买</a></div><div ng-if="recommendInfo.s_type == \'2\'" class="fund"><h3>{{recommendInfo.fundName}}</h3><span class="big_num fu" ng-class="{zheng:(recommendInfo.s_SYL_6Y_TRUE)}">{{recommendInfo.SYL_6Y}}<span class="percent">%</span></span><span class="placeholder12"></span>近六月涨幅<div class="sl_right_l_money clearfix"><p>单位净值<span class="placeholder12"></span><span class="unit">{{recommendInfo.unitNetValue}}</span></p><p>            晨星评级<span class="placeholder12"></span><span ng-show="recommendInfo.s_levels.length == 0">无评级</span><span class="stars" ng-show="recommendInfo.s_levels.length > 0"></span></p><br/><p>基金类型<span class="placeholder12"></span><span class="unit">{{recommendInfo.fundType != \'0\' ? $utb.getContext("PB_FUND_TYPE",recommendInfo.fundType) : \'保本基金\' }}</span></p><p>基金代码<span class="placeholder12"></span><span class="unit">{{recommendInfo.fundCode}}</span></p></div><a class="btn1" ng-click="purcaseFund(recommendInfo);">立即购买</a></div><div ng-if="recommendInfo.s_type == \'3\'" class="metal"><a href="javascript:void(0)" class="pro"><img ng-src="{{recommendInfo.listPicUrl}}"/></a><h3>recommendInfo.productName</h3><div class="re_amount">recommendInfo.recommendDesc</div><div class="num-main"><span class="percent">￥</span><span class="big_num">{{recommendInfo.singlePrice | money}}</span></div><a class="btn1" ng-click="showGoldDetail(recommendInfo.goldId);">查看详情</a></div><div ng-if="recommendInfo.s_type == \'4\'" class="deposit"><h3>{{$utb.getContext("DEP_PERIODTYPE", recommendInfo.saveTypeId)}}</h3><span class="big_num">{{recommendInfo.saveRate}}</span><span class="percent">%</span><span class="placeholder12"></span>预期年化收益率<div class="sl_right_l_money clearfix"><p ng-show="!!recommendInfo.recommendedDesc">【推荐理由】:<span class="placeholder12"></span><span class="unit">{{recommendInfo.recommendedDesc}}</span></p></div><a class="btn1" ng-click="purcaseDeposit(recommendInfo);" ng-class="{\'reason\': !!recommendInfo.recommendedDesc}">立即购买</a></div></div></div></div>', scope: {}, link: function (m, r, f) {
                m.$utb = e, m.purcaseDeposit = i, m.purcaseFund = c, m.showGoldDetail = l, e.getContextParams("PB_FUND_TYPE").then(function () {
                    o(f.type).then(function (o) {
                        for (var r = o.iRecommGoldenInfoNew || [], i = 0; i < r.length; i++) {
                            var c = r[i].goldPic, l = d(c, "GLP")[0] || {};
                            r[i].listPicUrl = 1 == r[i].flowFlag ? l.picUrl : e.getUrl("HB00000_bannerImage.do", { iconId: l.picUrl });
                        }
                        var i, f, u, g, _, I = o.iRecommTypes, v = a(o.iRecommFinancialProducts, "recommendedId"), h = a(r, "recommendedId"), D = a(o.iRecommRateList, "recommendedId"), F = a(o.iProdUseLimits, "prodId"), y = a(o.iRecommFundPBListInfo, "recommendedId"), T = m.iRecommendList = [];
                        for (i = 0; I && i < I.length; i++)
                            if (f = I[i].recommProdType, u = I[i].recommendedId, _ = {}, _.s_type = f, "1" == f) {
                                if (!v[u])
                                    continue;
                                angular.extend(_, v[u]), _.s_ProdUseLimits = F[_.prodId] || {}, t(_), _.s_alertPop = function () {
                                    n.alert("该产品目前已无可售额度。");
                                }, _.s_go = function (e, o) {
                                    var a = s.getUrlParams(), t = a.channelId;
                                    return "NZ" != t && o.prodName.indexOf("直销专属") > -1 ? void n.alert("暂不支持直销专属类型产品") : void s.jump(e, o);
                                }, T.push(_);
                            } else if ("2" == f) {
                                if (!y[u])
                                    continue;
                                angular.extend(_, y[u]), g = [], _.s_levels = p(_.RANK3Y), _.s_SYL_6Y_TRUE = parseFloat(_.SYL_6Y) > 0 ? !0 : !1, T.push(_);
                            } else if ("3" == f) {
                                if (!h[u])
                                    continue;
                                angular.extend(_, h[u]), T.push(_);
                            } else if ("4" == f) {
                                if (!D[u])
                                    continue;
                                angular.extend(_, D[u]), T.push(_);
                            }
                    });
                });
            } };
    }]);
;
angular.module("evoucher", []).directive("evoucher", ["$utb", "$message", "$httpPlus", function (e, s, i) {
        function n(s, n, t, r, u) {
            var a = "PB00000_queryVoucherListNoSession.do";
            e.isLogin() && (a = "PB00000_queryVoucherList.do"), s.evoucherBodyInit = function () {
                $(n[0]).find(".evoucherBody").width(172 * s.iBusinessVoucherList.length);
            }, s.scrollHandler = function (e) {
                var s = $(n[0]).find(".evoucherContainer"), i = s.scrollLeft();
                s.animate("0" == e ? { scrollLeft: i - 172 > 0 ? i - 172 : 0 } : { scrollLeft: i + 172 });
            }, s.isUseVoucher = !1, i.post(a, r.params).success(function (e) {
                if ("0000" == e.ec) {
                    var i = c(e.cd.iVoucherConList, "voucherid") || {};
                    s.iBusinessVoucherList = o(e.cd.iBusinessVoucherList, i, r, u), 0 == s.iBusinessVoucherList.length ? n.css("display", "none") : n.css("display", "block"), s.onChange({ amount: s.iBusinessVoucherList[s.activeIndex] ? s.iBusinessVoucherList[s.activeIndex].amount : "0" }), s.isUseVoucher = !0;
                }
            });
        }
        function o(e, s, i, n) {
            var o, c = [], r = (i.conditions || [], e ? e.length : 0), u = 0, a = n ? n.amount : "";
            for (u = 0; r > u; u++)
                o = e[u], a && parseFloat(a) <= parseFloat(o.amount) || (t(o.useorienttype, s[o.voucherid] || [], i.conditions || []) && c.push(o), o.s_starttime = o.starttime.substring(0, 8).replace(/(\d{4})(\d{2})(\d{2})/, "$1.$2.$3"), o.s_endtime = o.endtime.substring(0, 8).replace(/(\d{4})(\d{2})(\d{2})/, "$1.$2.$3"));
            return c;
        }
        function t(e, s, i) {
            var n, o, t = s ? s.length : 0, c = !1, r = !0, u = !1, a = !1, l = 0;
            if ("0" == e)
                return !0;
            if (0 == t)
                return !1;
            for (l = 0; t > l; l++)
                if (n = s[l], o = n.contype, "00" == n.contype)
                    c = !0;
                else {
                    for (var d = (n.conflags || "").length, h = "", v = "", f = !1, g = !0, p = "", L = 0; d > L; L++) {
                        if (h = n.conflags.charAt(L), v = n["coninfo" + (L + 1)], p = i[parseInt(h, 10)] || "", "01" == o)
                            switch (h) {
                                case "3":
                                    u = !0, f = p.indexOf(v) > -1, f && (a = !0);
                                    break;
                                case "0":
                                case "1":
                                    f = p.indexOf(v) > -1;
                                    break;
                                case "2":
                                    f = parseFloat(p) >= parseFloat(v), r = f;
                            }
                        else if ("02" == o)
                            switch (h) {
                                case "0":
                                case "1":
                                    f = p.indexOf(v) > -1;
                                    break;
                                case "2":
                                    f = parseFloat(p) >= parseFloat(v);
                            }
                        g = g ? f : f = !1;
                    }
                    f && (c = !0);
                }
            return (!u || u && a) && r && c ? !0 : !1;
        }
        function c(e, s) {
            var i, n, o = {};
            for (n = 0; e && n < e.length; n++)
                (i = o[e[n][s]]) || (i = o[e[n][s]] = []), i.push(e[n]);
            return o;
        }
        function r(s) {
            var n = "PB00000_queryVoucherList.do";
            return !e.isLogin() && (n = "PB00000_queryVoucherListNoSession.do"), i.post(n, s).then(function (e) {
                return e.data;
            });
        }
        function u(s) {
            var n = "PB00000_queryAllVoucherList.do";
            return !e.isLogin() && (n = "PB00000_queryAllVoucherListNoSession.do"), i.post(n, s).then(function (e) {
                return e.data;
            });
        }
        return { restrict: "EA", template: '<li class="evoucher"><label><a class="evoucherLink">现金券抵扣（{{iBusinessVoucherList.length}}张）：</a></label><div class="clearfix fl evoucherPack"><div class="arrowLeft" ng-show="iBusinessVoucherList && iBusinessVoucherList.length > 3" ng-click="scrollHandler(\'0\')"></div><div class="evoucherContainer" ng-if="isUseVoucher"><div class="evoucherBody clearfix" ng-init="evoucherBodyInit()"><div ng-repeat="voucherInfo in iBusinessVoucherList" ng-click="selectVoucher($index);" ng-class="{\'voucherActive\':(activeIndex == $index)}" class="evoucherCard"><div class="voucherHeader"></div><div class="voucherBody clearfix"><div class="voucherLeft"><div class="maskAmount">{{voucherInfo.amount}}<span class="yuan">元</span></div></div><div class="voucherRight"><input type="radio" name="voucherSelect"  ng-model="$parent.$parent.activeIndex"   custom-input  value="{{$index}}"  validator="r_required"/></div></div><div class="voucherFooter"><p>有效期至{{voucherInfo.s_endtime}}</p></div></div><div ng-show="iBusinessVoucherList.length == 0" class="evoucherNull"><p>您暂无现金券</p></div></div></div><div class="arrowRight" ng-show="iBusinessVoucherList && iBusinessVoucherList.length > 3" ng-click="scrollHandler(\'1\')"></div></div></li>', scope: { onInit: "&", onChange: "&" }, link: function (e, s, i) {
                function o(o, t, r) {
                    c.params = o, c.conditions = t || [], n(e, s, i, c, r), e.activeIndex = 0;
                }
                function t() {
                    return { selectedEvoucherids: ((e.iBusinessVoucherList || [])[e.activeIndex] || {}).flowno || "" };
                }
                s.css("display", "none");
                var c = {};
                e.selectVoucher = function (s) {
                    e.activeIndex = e.activeIndex == s ? -1 : s, e.onChange({ amount: ((e.iBusinessVoucherList || [])[e.activeIndex] || {}).amount || "0" });
                }, e.selectLink = function () {
                    e.isUseVoucher && (e.activeIndex = -1), e.isUseVoucher = !e.isUseVoucher;
                }, e.onInit({ $operator: { init: o, getVoucherParams: t, getBusinessVoucherList: r, getAllVoucherList: u } });
            } };
    }]);
;
angular.module("goldCar", []).directive("goldCar", ["$utb", "$message", "$location", "$browser", "$httpPlus", function (o, a, t, n, r) {
        var i = "/HB00601_goldCar";
        return { restrict: "EA", template: '<div class="subnav-fiexd-right subnav-fiexd-gold" id="goldCarTopDiv"><span class="subnav-right"><a ng-href="#{{actionUrl}}" ng-class="{\'active\':isActive()}">购物车<span ng-show="totalNum > 0">({{totalNum}})</span></a></span></div>', scope: { onInit: "&" }, link: function (a) {
                function s() {
                    var t = 0;
                    if (o.isLogin()) {
                        var i = n.cookies().HB_carGoldInfo;
                        if (i) {
                            var s = angular.fromJson(i);
                            s = angular.fromJson(s);
                            for (var l = s.goldList, u = {}, c = "", e = 0; e < l.length; e++)
                                c += l[e].productNo + "@" + l[e].buyNum + "@" + l[e].kind + "|";
                            u.flag = "0", u.goldId = c, r.post("PB00601_importGoldCarInfo.do", u).success(function (o) {
                                "0000" == o.ec && (t = o.cd.totalNum, a.totalNum = t, n.cookies("HB_carGoldInfo"));
                            });
                        } else
                            r.post("PB00601_queryCarNum.do", { flag: "2" }).success(function (o) {
                                "0000" == o.ec && (t = o.cd.totalNum, a.totalNum = t);
                            });
                    } else {
                        var i = n.cookies().HB_carGoldInfo;
                        if (i) {
                            var s = angular.fromJson(i);
                            s = angular.fromJson(s), t = s.totalNum, a.totalNum = t;
                        }
                    }
                }
                var l = {};
                s(), a.actionUrl = i, a.isActive = function () {
                    var o = t.path();
                    return o.indexOf(i) > -1;
                }, l.updateNum = function (o) {
                    a.totalNum = o;
                }, a.onInit({ $operator: l });
            } };
    }]);
;
angular.module("myVoucherList", []).directive("myVoucherList", ["$utb", "$message", "$httpPlus", "$routeHelper", function (s, i, o, e) {
        function n(i, e, n, t) {
            var h = "PB00000_queryAllVoucherListNoSession.do";
            s.isLogin() && (h = "PB00000_queryAllVoucherList.do"), o.post(h, t.params).success(function (s) {
                if ("0000" == s.ec) {
                    var o, e = u(s.cd.iVoucherConList, "voucherid"), n = u(s.cd.iVoucherPrivList, "voucherid"), t = 0, h = s.cd.iBusinessVoucherList, d = h.length;
                    for (t = 0; d > t; t++)
                        o = h[t].voucherid, h[t].other_infos = { conditions: e[o] || [], privs: n[o] || [] }, h[t].s_moneylimitMsg = a(h[t]), h[t].s_conditionMsgs = r(h[t]), h[t].s_starttime = h[t].starttime.replace(/(\d{4})(\d{2})(\d{2})(.*)/, "$1.$2.$3"), h[t].s_endtime = h[t].endtime.replace(/(\d{4})(\d{2})(\d{2})(.*)/, "$1.$2.$3");
                    i.iVoucherList = h, i.isClosed = h.length > 3, i.goShop = c;
                }
            });
        }
        function c(s) {
            var i, o = (s.other_infos || {}).privs || [], n = 0, c = o.length;
            if (!(c > 1))
                for (n = 0; c > n; n++)
                    if (i = o[n], "005821" == i.bsncode)
                        return void e.jump("/HB00600");
        }
        function r(s) {
            var i = [], o = (s.other_infos || {}).conditions || [], e = ((s.other_infos || {}).privs || [], s.useorienttype);
            return "1" == e && (i = i.concat(t(o))), i;
        }
        function t(s) {
            s = s || [];
            var i, o, e, n, c = 0, r = s.length, t = [], a = [], u = {};
            for (c = 0; r > c; c++)
                if (i = s[c].contype, o = s[c].conflags, e = s[c].coninfo1, n = i + "-" + o + "-" + e, !u[n]) {
                    if ("00" == i)
                        return [];
                    "01" == i && ("0" == o ? t.push({ msg: "购买" + e + "时可用" }) : "1" == o ? t.push({ msg: "购买" + ("2" == e ? "投资金" : "工艺金") + "可用" }) : "3" == o && a.push(e)), u[n] = !0;
                }
            return a.length > 0 && t.push({ msg: "限购买" + a.join("、") + "公司提供的产品可用" }), t;
        }
        function a(s) {
            var i, o, e, n = (s.other_infos || {}).conditions || [], c = s.useorienttype, r = "";
            if ("1" == c)
                for (i = 0; i < n.length; i++) {
                    if (o = n[i].contype, e = n[i].conflags, "00" == o) {
                        r = "";
                        break;
                    }
                    "01" == o && "2" == e.charAt(0) && (r = n[i].coninfo1);
                }
            return r ? "满" + r + "元可用" : "";
        }
        function u(s, i) {
            var o, e, n = {};
            for (e = 0; s && e < s.length; e++)
                (o = n[s[e][i]]) || (o = n[s[e][i]] = []), o.push(s[e]);
            return n;
        }
        return { restrict: "EA", template: '<div class="myVoucherList clearfix"><div class="myVoucherHeader">我的现金券{{ (iVoucherList && iVoucherList.length > 0) ? (\'（\' + iVoucherList.length + \'张）\') : \'\'  }}</div><div class="myvoucherBody clearfix" ng-show="iVoucherList && iVoucherList.length > 0"><dl class="myVoucherListUl clearfix" ng-class="{\'threeMore\':(iVoucherList && iVoucherList.length > 3), \'fixHeight\':(!!isClosed)}" ng-show="iVoucherList && iVoucherList.length > 0"><dd class="myVoucherListLi" ng-repeat="voucherInfo in iVoucherList" ng-click="goShop(voucherInfo);"><div class="voucherHeader"></div><div class="voucherBody clearfix"><div class="voucherMaskLeft"><div class="maskAmount" ng-class="{\'maskMoneyHas\': (!!voucherInfo.s_moneylimitMsg)}">{{voucherInfo.amount}}<span class="yuan">元</span></div><div class="maskMoney">{{ voucherInfo.s_moneylimitMsg }}</div></div><div class="voucherMaskRight"><div class="maskName">{{voucherInfo.vouchername}}</div><div class="maskConditions"><p class="maskCon" ng-repeat="conMsg in voucherInfo.s_conditionMsgs">{{conMsg.msg}}</p><span class="maskBtn">立即使用</span></div></div></div><div class="voucherFooter"><div class="maskNotice"><span class="maskSign">不可叠加使用</span><span>{{voucherInfo.s_starttime}}~{{voucherInfo.s_endtime}}有效</span></div><div class="maskLogo"></div></div></dd></dl></div><div class="arrow" ng-show="(iVoucherList && iVoucherList.length > 3)" ng-class="{\'uparrow\': (!isClosed)}" ng-click="isClosed = !isClosed;"></div><div ng-show="iVoucherList && iVoucherList.length == 0"><div class="myVoucherNone" ></div><p class="myVoucherMsg">对不起，您没有现金券</p></div></div>', scope: { onInit: "&" }, link: function (s, i, o) {
                function e(e) {
                    c.params = e, n(s, i, o, c);
                }
                var c = {};
                s.onInit({ $operator: { init: e } });
            } };
    }]);
;
angular.module("productNavigator", []).directive("productNavigator", ["$utb", "$message", "$location", "$routeHelper", function (t, i, a, n) {
        var e = [{ hash: "HB00101", title: "理财" }, { hash: "HB01104", title: "基金" }, { hash: "HB00501", title: "直销" }, { hash: "HB00301", title: "存款" }];
        return { restrict: "EA", template: '<div class="subnav-fiexd"><div class="subnav"><a ng-repeat="actionInfo in actionList" ng-click="jumpPage(\'/\' + actionInfo.hash)" ng-class="{\'active\': isActive(actionInfo)}">{{actionInfo.title}}</a></div></div>', scope: {}, link: function (t) {
                t.actionList = e, t.jumpPage = function (t, i) {
                    i = i || {}, i._source = "TJ002", n.jump(t, i);
                }, t.isActive = function (t) {
                    var i = t.hash, n = a.path(), e = i.substr(0, 5);
                    return n.indexOf(e) > -1;
                };
            } };
    }]);
;
!function () {
    var r = ["$log", function () {
            return { restrict: "E", link: function (r, n, i) {
                    if (i.name) {
                        var o = r[i.name];
                        o.$showErrorMsg = !1, o.checkValid = function () {
                            return o.$showErrorMsg = !0, o.$valid;
                        }, o.reset = function () {
                            o.$showErrorMsg = !1;
                        }, o.config = function (r) {
                            for (var i in r)
                                n.attr(i, r[i]);
                        }, o.submit = function () {
                            n[0].submit();
                        };
                    }
                } };
        }];
    angular.module("formValidate", []).directive("form", r).directive("ngForm", r);
}();
;
angular.module("groupfilter", []).directive("groupfilter", ["$log", function () {
        return { restrict: "A", template: '<div class="group-name" ng-repeat="group in groups"><span>{{group.title}}</span><div class="group-container" ng-class="{\'container-expand\':containerExpanded}"><a ng-class="{\'group-selected\':group.defaultValue==filter.value}" ng-repeat="filter in group.filters" ng-click="selectFilter(group.type, filter.value, $parent.$index)">{{filter.title}}</a></div><i ng-class="{\'more\':!containerExpanded,\'less\':containerExpanded}" ng-show="hasMore(group)" ng-click="containerExpanded=!containerExpanded">{{containerExpanded?\'收起\':\'更多\'}}</i></div>', scope: { groups: "=", onSelectFilter: "&" }, link: function (e) {
                e.selectFilter = function (n, t, r) {
                    e.groups[r].defaultValue = t, e.onSelectFilter({ filter: { type: n, value: t } });
                }, e.hasMore = function (e) {
                    for (var n = e.filters, t = [], r = 0; r < n.length; r++)
                        t.push(n[r].title);
                    return t.join("").length > 30;
                };
            } };
    }]);
;
!function (e) {
    var t = function (t, a) {
        function n(e, t, a) {
            "undefined" == typeof a && (a = "0");
            for (var n, s = 0; t > s; s++)
                n += a;
            return (n + e).slice(-t);
        }
        function s() {
            "undefined" != typeof i.url && "" != i.url && e.ajax({ url: i.url, async: !1, success: function (e) {
                    i.events = e;
                } });
        }
        function d() {
            s();
            var t = new Date(p, u, 1).getDay(), a = new Date(p, u + 1, 0).getDate(), n = new Date(p, u + 1, 0).getDate() - t + 1, d = e("<div/>").addClass("c-grid clearfix"), g = e("<div/>").addClass("c-event-grid"), D = e("<div/>").addClass("c-event-body");
            g.append(e("<div/>").addClass("c-event-title c-pad-top").html(i.eventTitle)), g.append(D);
            var b = e("<div/>").addClass("c-header clearfix"), k = e("<div/>").addClass("c-next c-grid-title c-pad-top"), A = e("<div/>").addClass("c-month c-grid-title c-pad-top"), M = e("<div/>").addClass("c-previous c-grid-title c-pad-top");
            M.html(i.textArrows.previous), A.html(p + " 年  " + i.months[u]), k.html(i.textArrows.next), M.on("mouseover", m).on("mouseleave", h).on("click", w), k.on("mouseover", m).on("mouseleave", h).on("click", x), d.append(b), b.append(M).append(A).append(k);
            for (var F = 0; F < i.weekDays.length; F++) {
                var S = e("<div/>").addClass("c-week-day c-pad-top");
                S.html(i.weekDays[F]), d.append(S);
            }
            for (var j = 1, T = 1, F = 0; 42 > F; F++) {
                var Y = e("<div/>"), B = "";
                if (t > F)
                    Y.addClass("c-day-previous-month c-pad-top"), Y.html(n++);
                else if (a >= j) {
                    Y.addClass("c-day c-pad-top"), j == v && l == u && c == p && Y.addClass("c-today"), j == r && Y.addClass("c-day-selected"), B = '<div class="day-number">' + j + '</div><span class="no-edu">无额度</span>', Y.on("click", function () {
                        return function () {
                            r = "";
                        };
                    }(Y));
                    for (var E = 0; E < i.events.length; E++) {
                        var J = i.events[E].datetime;
                        if (J.getDate() == j && J.getMonth() == u && J.getFullYear() == p) {
                            Y.addClass("c-event").attr("data-event-day", J.getDate()), B = '<div class="day-number">' + j + '</div><span class="day-edu">' + i.events[E].edu + "</span>", Y.on("mouseover", C).on("mouseleave", y), Y.unbind("click").on("click", function (t) {
                                return function () {
                                    r = parseInt(t.attr("data-event-day"), 10), e(this).siblings().removeClass("c-day-selected"), e(this).addClass("c-day-selected"), o(A, D);
                                };
                            }(Y));
                            break;
                        }
                    }
                    Y.html(B), j++;
                } else
                    Y.addClass("c-day-next-month c-pad-top"), Y.html(T++);
                i.days.listener && Y.on(i.days.listener), d.append(Y);
            }
            o(A, D), e(f).addClass("calendar"), e(f).html(d);
        }
        function o(t, a) {
            t.html(p + " 年  " + i.months[u]);
            for (var s = e("<div/>").addClass("c-event-list"), d = 0; d < i.events.length; d++) {
                var o = i.events[d].datetime;
                if (i.events[d].isCondition ? i.events[d].isCondition() : o.getMonth() == u && o.getFullYear() == p) {
                    var r = i.events[d].formatDate || p + "/" + n(o.getMonth() + 1, 2) + "/" + n(o.getDate(), 2) + " " + n(o.getHours(), 2) + ":" + n(o.getMinutes(), 2), l = e("<div/>").addClass("c-event-item"), c = e("<div/>").addClass("title").html(r + "  " + i.events[d].title + "<br/>"), v = e("<div/>").addClass("description").html(i.events[d].description + "<br/>");
                    l.attr("data-event-day", o.getDate()), l.on("mouseover", g).on("mouseleave", D), i.events[d].listener && l.on(i.events[d].listener), l.append(c).append(v), s.append(l);
                }
            }
            (a || e(f).find(".c-event-body")).html(s);
        }
        var i = e.extend({}, e.fn.eCalendar.defaults, t), r = i.today.getDate(), l = i.today.getMonth(), c = i.today.getFullYear(), v = r, u = l, p = c, f = a, m = function () {
        }, h = function () {
        }, C = function () {
            e(this).addClass("c-day-selected");
        }, y = function () {
            e(this).removeClass("c-day-selected");
        }, g = function () {
            e(this).addClass("c-event-ho");
            var t = e(this).attr("data-event-day");
            e('div.c-event[data-event-day="' + t + '"]').addClass("c-event-over");
        }, D = function () {
            e(this).removeClass("c-event-ho");
            var t = e(this).attr("data-event-day");
            e('div.c-event[data-event-day="' + t + '"]').removeClass("c-event-over");
        }, x = function () {
            11 > u ? u++ : (u = 0, p++), r = "", i.textArrows.nextClick && i.textArrows.nextClick(), !i.textArrows.nextClick && d();
        }, w = function () {
            u > 0 ? u-- : (u = 11, p--), r = "", i.textArrows.previousClick && i.textArrows.previousClick(), !i.textArrows.previousClick && d();
        }, b = function () {
            r = i.today.getDate(), u = i.today.getMonth(), c = i.today.getFullYear(), d();
        }, k = function () {
            return "" == r ? null : new Date(p, u, r);
        }, A = function () {
            return new Date(p, u, 1);
        }, M = function () {
            return i.events;
        }, F = function (e) {
            i.events = e;
        };
        return e.fn.today = b, e.fn.getDate = k, e.fn.getEvents = M, e.fn.setEvents = F, e.fn.renderEvents = d, e.fn.getMonth = A, d();
    };
    e.fn.eCalendar = function (a) {
        return this.each(function () {
            return t(a, e(this));
        });
    }, e.fn.eCalendar.defaults = { weekDays: ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sab"], months: ["Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"], textArrows: { previous: "", previousClick: null, next: "", nextClick: null }, eventTitle: "事件", today: new Date, days: {}, url: "", events: [{ title: "Brasil x Croácia", description: "Abertura da copa do mundo 2014", datetime: new Date(2014, 6, 12, 17), formatDate: "", listener: {} }, { title: "Brasil x México", description: "Segundo jogo da seleção brasileira", datetime: new Date(2014, 6, 17, 16), formatDate: "", listener: {} }, { title: "Brasil x Camarões", description: "Terceiro jogo da seleção brasileira", datetime: new Date(2014, 6, 23, 16), formatDate: "", listener: {} }] };
}(jQuery);
;
angular.module("moneyInput", []).directive("moneyInput", ["$filter", function (n) {
        var e = n("money"), t = n("unmoney"), u = function (n) {
            var e = angular.element(this).attr("money-input");
            if (!n.ctrlKey) {
                var t = window.event ? n.keyCode : n.which;
                if (t >= 48 && 57 >= t || 0 == t || 8 == t) {
                    if ("0" != this.value || 0 == t || 8 == t)
                        return;
                    return i(n), !0;
                }
                if (e > 0 && 46 == t && -1 == this.value.indexOf("."))
                    return;
                i(n);
            }
        }, r = function (n) {
            var e = angular.element(this).attr("money-input");
            if (!n.ctrlKey) {
                var t = this.value.indexOf("."), u = this.value.length - (t + 1);
                0 == t ? this.value = "0." : t > 0 && u > e && (this.value = this.value.substr(0, this.value.length - (u - e)));
            }
        }, i = function (n) {
            window.event ? window.event.returnValue = !1 : n.preventDefault();
        }, a = function (n) {
            return i(n), !1;
        };
        return { restrict: "A", require: "ngModel", link: function (n, i, o, l) {
                o.moneyInput || (o.$set("moneyInput", "2"), i.attr("money-input", "2")), i.bind("keypress", u), i.bind("keyup", function () {
                    var e = r.apply(this, arguments), t = this.value;
                    return l.$viewValue !== t && (n.$root.$$phase ? l.$setViewValue(t) : n.$apply(function () {
                        l.$setViewValue(t);
                    })), e;
                }), i.on("paste", a), i.on("dragenter", a), i.on("blur", function () {
                    i.val(e(i.val(), o.moneyInput));
                }), i.on("focus", function () {
                    i.val(t(i.val()));
                }), l.$parsers.push(function (n) {
                    return t(e(n, o.moneyInput));
                }), l.$formatters.push(function (n) {
                    return e(n, o.moneyInput);
                });
            } };
    }]);
;
angular.module("commonAnimation", []);
;
angular.module("numberInput", []).directive("numberInput", ["$log", function () {
        return { restrict: "A", template: '<div class="number-input"><a class="reduce" ng-click="reduce()"></a><input type="text" ng-model="ngModel" readonly/><a class="add" ng-click="add()"></a></div>', scope: { ngModel: "=", onChange: "&" }, link: function (n) {
                n.ngModel = 1, n.add = function () {
                    n.ngModel++, n.onChange({ number: n.ngModel });
                }, n.reduce = function () {
                    n.ngModel > 1 && (n.ngModel--, n.onChange({ number: n.ngModel }));
                };
            } };
    }]);
;
angular.module("pagination", []).directive("pagination", ["$filter", function () {
        return { restrict: "A", template: '<ul class="pagination"><li class="long" ng-class="{disabled: noPrevious()}" ng-click="selectPrevious()">上一页</li><li ng-show="currentRange>1" ng-click="showPrevious()">...</li><li ng-repeat="page in pages" ng-class="{active: isActive(page)}" ng-click="selectPage(page)">{{page}}</li><li ng-show="numPages>currentRange+showNums-1" ng-click="showNext()">...</li><li class="long" ng-class="{disabled: noNext(),last:true}" ng-click="selectNext()">下一页</li></ul>', scope: { numPages: "=", currentPage: "=", pageRefresh: "=", onSelectPage: "&" }, link: function (e, n, t) {
                e.showNums = t.showNums ? parseInt(t.showNums, 10) : 5, e.$watch("numPages", function (n) {
                    e.currentRange = 1, e.currentPage > n && e.selectPage(1), e.getPageRange();
                }), e.$watch("pageRefresh", function () {
                    e.currentRange = Math.floor((e.currentPage - 1) / e.showNums) * e.showNums + 1, e.getPageRange();
                }), e.isActive = function (n) {
                    return e.currentPage === n;
                }, e.showPrevious = function () {
                    e.currentRange -= e.showNums, e.getPageRange();
                }, e.showNext = function () {
                    e.currentRange += e.showNums, e.getPageRange();
                }, e.getPageRange = function () {
                    e.pages = [];
                    for (var n = Math.min(e.currentRange + (e.showNums - 1), e.numPages), t = e.currentRange; n >= t; t++)
                        e.pages.push(t);
                }, e.selectPage = function (n) {
                    e.isActive(n) || (e.currentPage = n, e.onSelectPage({ page: n }));
                }, e.selectNext = function () {
                    e.noNext() || e.selectPage(e.currentPage + 1), e.currentRange = Math.floor((e.currentPage - 1) / e.showNums) * e.showNums + 1, e.getPageRange();
                }, e.selectPrevious = function () {
                    e.noPrevious() || e.selectPage(e.currentPage - 1), e.currentRange = Math.floor((e.currentPage - 1) / e.showNums) * e.showNums + 1, e.getPageRange();
                }, e.noNext = function () {
                    return e.currentPage >= e.numPages;
                }, e.noPrevious = function () {
                    return e.currentPage <= 1;
                };
            } };
    }]);
;
angular.module("password", []).directive("passwordctrl", ["$appConfig", "$globalData", "$httpPlus", function (t, e, s) {
        return { restrict: "A", template: '<span ng-show="controlFlag"></span><input ng-if="!controlFlag" ng-model="$parent.inputValue" type="password" maxlength="{{_maxLength}}" tabindex="{{_tabIndex}}" ng-keypress="doKeypress" />', scope: { passOperator: "=", checkEreg: "=", maxLength: "=", passwordClass: "=", tabIndex: "=", enterCallback: "&" }, link: function (i, n) {
                function a() {
                    return i.controlFlag ? c.pwdResult() : i.inputValue;
                }
                function r() {
                    if (i.controlFlag) {
                        var t = c.machineNetwork(), e = { isIE: !1, MFM: t, editType: 1, logonLanguage: "zh_CN" };
                        return e;
                    }
                    var e = { isIE: !1, MFM: "", editType: 4, logonLanguage: "zh_CN" };
                    return e;
                }
                function o() {
                    if (i.controlFlag)
                        return c.pwdLength();
                    var t = i.inputValue;
                    return t ? t.length : 0;
                }
                function g() {
                    var t = "";
                    return i.controlFlag && (t = c.machineNetwork()), t;
                }
                function d() {
                    var t = new RegExp(i._checkEreg), e = "";
                    if (i.controlFlag)
                        return 10 == c.osBrowser || 11 == c.osBrowser || (e = c.pwdResult()) ? 1 == c.pwdValid() ? !1 : !0 : t.test(e);
                    var s = i.inputValue;
                    return t.test(s);
                }
                function p() {
                    i.controlFlag ? c.pwdclear() : i.inputValue = "";
                }
                function h() {
                    return c.checkInstall();
                }
                function l(t, s) {
                    var i, n = { pwdId: "", pgePath: "./assets/nbcbEdit/", pgeId: "", pgeEdittype: 0, pgeEreg1: "[\\s\\S]*", pgeEreg2: "[\\d]{6,6}", pgeMaxlength: 6, pgeTabindex: 2, pgeClass: "pwdClass", pgeInstallClass: "pwdClass", pgeOnkeydown: "", tabCallback: "", pgeEnterDown: null, pgeTabDown: null };
                    angular.extend(n, s);
                    var a = e.getTimestamp(), r = "_password_container_" + a;
                    t.attr("id", r), n.pgeId = "_password_" + a, n.pgeOnkeydown = "PassGuardCtrl('" + r + "',0)", n.tabCallback = "PassGuardCtrl('" + r + "',1)";
                    var o = {};
                    o.pgeEnterDown = n.pgeEnterDown, o.pgeTabDown = n.pgeTabDown, t.data("event", o), window.top.PassGuardCtrl = function (t, e) {
                        var s = $(document.getElementById(t)), i = s.data("event");
                        i && (0 == e && i.pgeEnterDown && angular.isFunction(i.pgeEnterDown) ? i.pgeEnterDown() : 1 == e && i.pgeTabDown && angular.isFunction(i.pgeTabDown) ? i.pgeTabDown() : 2 == e && i.pgeRefresh && angular.isFunction(i.pgeRefresh) && i.pgeRefresh());
                    }, i = new pge(n), i && i.pwdResult && (i.oldPwdResult = i.pwdResult, i.pwdResult = function () {
                        return window.srandNum && i.pwdSetSk(window.srandNum), i.oldPwdResult();
                    }), i && i.machineNetwork && (i.oldMachineNetwork = i.machineNetwork, i.machineNetwork = function () {
                        return window.srandNum && i.pwdSetSk(window.srandNum), i.oldMachineNetwork();
                    });
                    var g = $(i.load());
                    return t.append(g), i.pgInitialize(), i;
                }
                i.controlFlag = t.passwordCtrl, i._checkEreg = i.checkEreg || "[\\d]{6}", i._maxLength = i.maxLength || 6, i._tabIndex = i.tabIndex || 2, i._passwordClass = i.passwordClass || "pwdClass";
                var c = null, u = null;
                if (i.controlFlag) {
                    u = n.find("span");
                    var w = e.checkOsBrowser();
                    10 == w || 11 == w ? s.post("getEdgeRandom.do", {}).success(function (t) {
                        "0000" == t.ec ? c = l(u, { pgeClass: i._passwordClass, pgeInstallClass: i._passwordClass, pgeEreg2: i._checkEreg, pgeMaxlength: parseInt(i._maxLength), pgeEnterDown: i.enterCallback, pgeTabindex: i._tabIndex, pgeWindowID: "password" + (new Date).getTime() + 1, pgeRZRandNum: t.cd.pgeRZRand, pgeRZDataB: t.cd.pgeRZData }) : window.console && console.log("通讯请求加密数据失败");
                    }) : c = l(u, { pgeClass: i._passwordClass, pgeInstallClass: i._passwordClass, pgeEreg2: i._checkEreg, pgeMaxlength: i._maxLength, pgeEnterDown: i.enterCallback, pgeTabindex: i._tabIndex });
                } else
                    i.inputValue = "", i.doKeypress = function (t) {
                        var e = t.keyCode || t.which;
                        13 == e && i.enterCallback && i.enterCallback();
                    };
                i.passOperator = { getValue: a, getLength: o, getControlParams: r, machineNetwork: g, checkValid: d, clear: p, checkInstall: h };
            } };
    }]);
var PGEdit_IE32_CLASSID = "0CBFD428-E975-4B41-A8C6-EB820FA5BDE6", PGEdit_IE32_CAB = "nbcbEdit.cab#version=1,0,0,1", PGEdit_IE32_EXE = "nbcbEdit.exe", PGEdit_IE32_VERSION = "1.0.0.1", PGEdit_IE64_CLASSID = "C969545D-FD1D-40F2-A943-C733FEB21B7A", PGEdit_IE64_CAB = "nbcbEditX64.cab#version=1,0,0,1", PGEdit_IE64_EXE = "nbcbEdit.exe", PGEdit_FF = "nbcbEdit.exe", PGEdit_FF_VERSION = "1.0.0.1", PGEdit_Linux32 = "", PGEdit_Linux64 = "", PGEdit_Linux_VERSION = "", PGEdit_MacOs = "nbcbEdit.pkg", PGEdit_MacOs_VERSION = "1.0.0.2", PGEdit_MacOs_Safari = "nbcbEdit.pkg", PGEdit_MacOs_Safari_VERSION = "1.0.0.2", PGEdit_Edge = "nbcbEditEdge.exe", PGEdit_Edge_VERSION = "1.0.0.1", PGEdit_Edge_Mac = "nbcbEditEdge.pkg", PGEdit_Edge_Mac_VERSION = "1.0.0.1", PGEdit_Update = "1", debugConsole = !0, urls = "https://windows10.microdone.cn:5186", port = 5186, CIJSON = { interfacetype: 0, data: { "switch": 3 } }, ICJSON = { interfacetype: 0, data: { "switch": 2 } }, INCJSON = { interfacetype: 1, data: {} }, OPJSON = { interfacetype: 0, data: { "switch": 0 } }, XTJSON = { interfacetype: 0, data: { "switch": 5 } }, CPJSON = { interfacetype: 0, data: { "switch": 1 } }, OUTJSON = { interfacetype: 2, data: {} }, CLPJSON = { interfacetype: 0, data: { "switch": 4 } }, interv, onceInterv = {}, iterArray = [], outs = {}, inFlag = {}, isInit = {}, license = "QVFxb3FjcDlEcFJVK3UxUmc2cEluQ0V4NkxnWllEODBNSEt6YVg5OFZXdDRDZDBFaEZONXI4VkI3QjRsb0VoRElXdWVkNXdaYlJVaVJyb0dJRjQ3d0dnMkZiS2sxQ1pLeFRLanR6MzF6YkdMbVY0YStVLytyakc3TnJnak5Zd0sxRU8rbzFRRXJOcE1EUUpIWE9qNHlrL1RtYVNPckdxYVZBaEkrcFduemQ0PXsiaWQiOjAsInR5cGUiOiJwcm9kdWN0IiwicGFja2FnZSI6WyIiXSwiYXBwbHluYW1lIjpbIjE5Mi4xNjguMS4xMTgiXSwicGxhdGZvcm0iOjR9", licenseMac = "SUxtVTdoVSt3RVd4WHlNUWMxWGxvS0VhZUF3V1BVS2N4VDEzaGp1M0x3WUlRVEtiaC9PaGlQQUlDOXB5UkNWUVV6clRiV3oxTWJHeFJyeEYwWE9YTVQ4S09vdkcxYVNzYWpSWkZaaElvekV5OTdGNktJbVBiMGpTelE5OGREVWFxYk5NWWZBSHZHeXJ1bnU4bDNSVmJrc0QwSmpqR2h2L0RMZ0xZWTI2SitRPXsiaWQiOjAsInR5cGUiOiJwcm9kdWN0IiwicGFja2FnZSI6WyIiXSwiYXBwbHluYW1lIjpbIjE5Mi4xNjguMS4xMTgiXSwicGxhdGZvcm0iOjh9";
navigator.userAgent.indexOf("MSIE") < 0 && navigator.plugins.refresh();
var pge = function (t) {
    this.settings = angular.extend({}, pge.defaults, t), this.init();
};
!function ($) {
    angular.extend(pge, {
        defaults: { pgePath: "./ocx/", pgeId: "", pgeEdittype: 0, pgeEreg1: "", pgeEreg2: "", pgeMaxlength: 12, pgeTabindex: 2, pgeClass: "ocx_style", pgeInstallClass: "ocx_style", pgeOnkeydown: "", pgeFontName: "", pgeFontSize: "", tabCallback: "", pgeBackColor: "", pgeForeColor: "", pgeOnblur: "", pgeOnfocus: "", pgeUrls: "https://windows10.microdone.cn", pgePort: 5186, pgeWindowID: "password" + (new Date).getTime() }, prototype: {
            init: function () {
                outs[this.settings.pgeWindowID] = { length: 0, version: 0, mac: "", hard: "", cpu: "", aes: "", valid: 1, hash: "", rsa: "", pin: "", sign: "", charNum: "", hardList: "" }, this.pgeDownText = "请点此安装控件", this.osBrowser = this.checkOsBrowser(), this.pgeVersion = this.getVersion(), this.isInstalled = this.checkInstall();
            }, checkOsBrowser: function () {
                var t, e = /chrome\/[\d.]+/gi;
                if ("Win32" == navigator.platform || "Windows" == navigator.platform)
                    if (navigator.userAgent.indexOf("MSIE") > 0 || navigator.userAgent.indexOf("msie") > 0 || navigator.userAgent.indexOf("Trident") > 0 || navigator.userAgent.indexOf("trident") > 0)
                        navigator.userAgent.indexOf("ARM") > 0 ? (t = 9, this.pgeditIEExe = "") : (t = 1, this.pgeditIEClassid = PGEdit_IE32_CLASSID, this.pgeditIECab = PGEdit_IE32_CAB, this.pgeditIEExe = PGEdit_IE32_EXE);
                    else if (navigator.userAgent.indexOf("Edge") > 0)
                        t = 10, this.pgeditFFExe = PGEdit_Edge;
                    else if (navigator.userAgent.indexOf("Chrome") > 0) {
                        var s = navigator.userAgent.match(e).toString();
                        s = parseInt(s.replace(/[^0-9.]/gi, "")), s >= 42 ? (t = 10, this.pgeditFFExe = PGEdit_Edge) : (t = 2, this.pgeditFFExe = PGEdit_FF);
                    } else
                        t = 2, this.pgeditFFExe = PGEdit_FF;
                else if ("Win64" == navigator.platform)
                    if (navigator.userAgent.indexOf("Windows NT 6.2") > 0 || navigator.userAgent.indexOf("windows nt 6.2") > 0)
                        t = 1, this.pgeditIEClassid = PGEdit_IE32_CLASSID, this.pgeditIECab = PGEdit_IE32_CAB, this.pgeditIEExe = PGEdit_IE32_EXE;
                    else if (navigator.userAgent.indexOf("MSIE") > 0 || navigator.userAgent.indexOf("msie") > 0 || navigator.userAgent.indexOf("Trident") > 0 || navigator.userAgent.indexOf("trident") > 0)
                        t = 3, this.pgeditIEClassid = PGEdit_IE64_CLASSID, this.pgeditIECab = PGEdit_IE64_CAB, this.pgeditIEExe = PGEdit_IE64_EXE;
                    else if (navigator.userAgent.indexOf("Edge") > 0)
                        t = 10, this.pgeditFFExe = PGEdit_Edge;
                    else if (navigator.userAgent.indexOf("Chrome") > 0) {
                        var s = navigator.userAgent.match(e).toString();
                        s = parseInt(s.replace(/[^0-9.]/gi, "")), s >= 42 ? (t = 10, this.pgeditFFExe = PGEdit_Edge) : (t = 2, this.pgeditFFExe = PGEdit_FF);
                    } else
                        t = 2, this.pgeditFFExe = PGEdit_FF;
                else if (navigator.userAgent.indexOf("Linux") > 0)
                    navigator.userAgent.indexOf("_64") > 0 ? (t = 4, this.pgeditFFExe = PGEdit_Linux64) : (t = 5, this.pgeditFFExe = PGEdit_Linux32), navigator.userAgent.indexOf("Android") > 0 && (t = 7);
                else if (navigator.userAgent.indexOf("Macintosh") > 0)
                    if (navigator.userAgent.indexOf("Safari") > 0 && (navigator.userAgent.indexOf("Version/5.1") > 0 || navigator.userAgent.indexOf("Version/5.2") > 0 || navigator.userAgent.indexOf("Version/6") > 0))
                        t = 8, this.pgeditFFExe = PGEdit_MacOs_Safari;
                    else if (navigator.userAgent.indexOf("Firefox") > 0 || navigator.userAgent.indexOf("Chrome") > 0) {
                        var s = navigator.userAgent.match(e);
                        null != s ? (s = s.toString(), s = parseInt(s.replace(/[^0-9.]/gi, "")), s >= 42 ? (t = 11, this.pgeditFFExe = PGEdit_Edge_Mac) : (t = 6, this.pgeditFFExe = PGEdit_MacOs)) : (t = 6, this.pgeditFFExe = PGEdit_MacOs);
                    } else
                        navigator.userAgent.indexOf("Opera") >= 0 && (navigator.userAgent.indexOf("Version/11.6") > 0 || navigator.userAgent.indexOf("Version/11.7") > 0) ? (t = 6, this.pgeditFFExe = PGEdit_MacOs) : navigator.userAgent.indexOf("Safari") >= 0 ? (t = 6, this.pgeditFFExe = PGEdit_MacOs) : (t = 0, this.pgeditFFExe = "");
                return t;
            }, getpgeHtml: function (callf) {
                var _this = this;
                if (1 == this.osBrowser || 3 == this.osBrowser) {
                    var pgeOcx = '<div id="' + this.settings.pgeId + '_pge" style="z-index: 1;display:none;margin-left:0px;width:135px !important; height:19px !important;"><OBJECT ID="' + this.settings.pgeId + '" CLASSID="CLSID:' + this.pgeditIEClassid + '" ';
                    return void 0 != this.settings.pgeOnkeydown && "" != this.settings.pgeOnkeydown && (pgeOcx += ' onkeydown="if(13==event.keyCode || 27==event.keyCode)' + this.settings.pgeOnkeydown + ';"'), void 0 != this.settings.pgeOnblur && "" != this.settings.pgeOnblur && (pgeOcx += ' onblur="' + this.settings.pgeOnblur + '"'), void 0 != this.settings.pgeOnfocus && "" != this.settings.pgeOnfocus && (pgeOcx += ' onfocus="' + this.settings.pgeOnfocus + '"'), void 0 != this.settings.pgeTabindex && "" != this.settings.pgeTabindex && (pgeOcx += ' tabindex="' + this.settings.pgeTabindex + '"'), void 0 != this.settings.pgeClass && "" != this.settings.pgeClass && (pgeOcx += ' class="' + this.settings.pgeClass + '"'), pgeOcx += ">", void 0 != this.settings.pgeEdittype && "" != this.settings.pgeEdittype && (pgeOcx += ' <param name="edittype" value="' + this.settings.pgeEdittype + '">'), pgeOcx += ' <param name="kbmode" value="1">', void 0 != this.settings.pgeMaxlength && "" != this.settings.pgeMaxlength && (pgeOcx += ' <param name="maxlength" value="' + this.settings.pgeMaxlength + '">'), void 0 != this.settings.pgeEreg1 && "" != this.settings.pgeEreg1 && (pgeOcx += ' <param name="input2" value="' + this.settings.pgeEreg1 + '">'), void 0 != this.settings.pgeEreg2 && "" != this.settings.pgeEreg2 && (pgeOcx += ' <param name="input3" value="' + this.settings.pgeEreg2 + '">'), void 0 != this.settings.pgeFontName && "" != this.settings.pgeFontName && (pgeOcx += '<param name="FontName" value="' + this.settings.pgeFontName + '">'), void 0 != this.settings.pgeFontSize && "" != this.settings.pgeFontSize && (pgeOcx += '<param name="FontSize" value="' + this.settings.pgeFontSize + '">'), pgeOcx += "</OBJECT></div>";
                }
                if (2 == this.osBrowser) {
                    var pgeOcx = '<embed ID="' + this.settings.pgeId + '"  maxlength="' + this.settings.pgeMaxlength + '" input_2="' + this.settings.pgeEreg1 + '" input_3="' + this.settings.pgeEreg2 + '" edittype="' + this.settings.pgeEdittype + '" type="application/nbcb" tabindex="' + this.settings.pgeTabindex + '" class="' + this.settings.pgeClass + '" ';
                    return void 0 != this.settings.pgeOnblur && "" != this.settings.pgeOnblur && (pgeOcx += ' onblur="' + this.settings.pgeOnblur + '"'), void 0 != this.settings.pgeOnfocus && "" != this.settings.pgeOnfocus && (pgeOcx += ' onfocus="' + this.settings.pgeOnfocus + '"'), void 0 != this.settings.pgeOnkeydown && "" != this.settings.pgeOnkeydown && (pgeOcx += ' input_1013="' + this.settings.pgeOnkeydown + '"'), void 0 != this.settings.tabCallback && "" != this.settings.tabCallback && (pgeOcx += ' input_1009="' + this.settings.tabCallback + '"'), void 0 != this.settings.pgeFontName && "" != this.settings.pgeFontName && (pgeOcx += ' FontName="' + this.settings.pgeFontName + '"'), void 0 != this.settings.pgeFontSize && "" != this.settings.pgeFontSize && (pgeOcx += " FontSize=" + Number(this.settings.pgeFontSize)), pgeOcx += " >";
                }
                if (6 == this.osBrowser)
                    return '<embed ID="' + this.settings.pgeId + '" input2="' + this.settings.pgeEreg1 + '" input3="' + this.settings.pgeEreg2 + '" input4="' + Number(this.settings.pgeMaxlength) + '" input0="' + Number(this.settings.pgeEdittype) + '" input11="3" type="application/nbcbEdit-safari-plugin" version="' + PGEdit_MacOs_VERSION + '" tabindex="' + this.settings.pgeTabindex + '" class="' + this.settings.pgeClass + '">';
                if (8 == this.osBrowser)
                    return '<embed ID="' + this.settings.pgeId + '" input2="' + this.settings.pgeEreg1 + '" input3="' + this.settings.pgeEreg2 + '" input4="' + Number(this.settings.pgeMaxlength) + '" input0="' + Number(this.settings.pgeEdittype) + '" input11="3" type="application/nbcbEdit-safari-plugin" version="' + PGEdit_MacOs_Safari_VERSION + '" tabindex="' + this.settings.pgeTabindex + '" class="' + this.settings.pgeClass + '">';
                if (10 == this.osBrowser || 11 == this.osBrowser) {
                    var obj = this, isInstalled = this.isInstalled;
                    if (isInstalled) {
                        debugConsole && console.log("是否安装" + isInstalled);
                        var id = obj.settings.pgeId, winId = obj.settings.pgeWindowID, $input = $('<input type="password" autocomplete="off" id="' + id + '" style="ime-mode:disabled" tabindex="2" class=" newpwdcontrol ocx_style "  value="" maxlength="' + obj.settings.pgeMaxlength + '"/>');
                        $input.focus(function () {
                            _this.openProt(winId, id), _this.setCX($input[0]);
                        }), $input.keydown(function () {
                            _this.setSX(window.event, obj.settings.pgeOnkeydown, $input[0]);
                        }), $input.click(function () {
                            _this.setCX($input[0]);
                        }), $input.blur(function () {
                            _this.closeProt(winId, id);
                        }), $("#" + id + "_down").parent().html($input);
                        var o = $input[0];
                        return 11 == obj.osBrowser && ($input.attr("type", "text"), o.onkeypress = function (t) {
                            var e = 0, s = t ? t : event;
                            e = s.which;
                            var i = String.fromCharCode(e), n = parseInt(obj.settings.pgeMaxlength);
                            if (this.value.length > n - 1)
                                return !1;
                            if (e >= 32 && 126 >= e) {
                                var a = obj.settings.pgeEreg1.replace("*", "");
                                return a = new RegExp(a), a.test(i) && (this.value += "*"), !1;
                            }
                            return !0;
                        }, o.onkeydown = function (e) {
                            var chrTyped, chrCode = 0, evt = e ? e : event;
                            chrCode = evt.which;
                            var x = String.fromCharCode(chrCode), maxlength = parseInt(obj.settings.pgeMaxlength);
                            return this.value.length > maxlength - 1 ? !1 : 13 != chrCode ? chrCode >= 37 && 40 >= chrCode ? !1 : !0 : (this.blur(), void eval("(" + obj.settings.pgeOnkeydown + ")"));
                        }), 10 == obj.osBrowser && (o.onkeypress = function () {
                            return inFlag[winId].flag;
                        }), obj.instControl(winId), o;
                    }
                    return '<div id="' + this.settings.pgeId + '_down" class="' + this.settings.pgeInstallClass + '" style="text-align:center;line-height:25px;"><a href="' + this.settings.pgePath + this.pgeditFFExe + '">' + this.pgeDownText + "</a></div>";
                }
                return '<div id="' + this.settings.pgeId + '_down" class="' + this.settings.pgeInstallClass + '" style="text-align:center;">暂不支持此浏览器</div>';
            }, getDownHtml: function () {
                return 1 == this.osBrowser || 3 == this.osBrowser ? '<div id="' + this.settings.pgeId + '_down" class="' + this.settings.pgeInstallClass + '" style="text-align:center;margin-left:0px;"><a href="' + this.settings.pgePath + this.pgeditIEExe + '">' + this.pgeDownText + "</a></div>" : 2 == this.osBrowser || 6 == this.osBrowser || 8 == this.osBrowser ? '<div id="' + this.settings.pgeId + '_down" class="' + this.settings.pgeInstallClass + '" style="text-align:center;margin-left:0px;line-height:23px;"><a href="' + this.settings.pgePath + this.pgeditFFExe + '">' + this.pgeDownText + "</a></div>" : '<div id="' + this.settings.pgeId + '_down" class="' + this.settings.pgeInstallClass + '" style="text-align:center;">暂不支持此浏览器</div>';
            }, load: function () {
                if (10 == this.osBrowser || 11 == this.osBrowser)
                    return this.getpgeHtml();
                if (this.isInstalled) {
                    if (2 == this.osBrowser) {
                        if (this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_FF_VERSION) && 1 == PGEdit_Update)
                            return this.setDownText(), this.getDownHtml();
                    } else if (6 == this.osBrowser) {
                        if (this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_MacOs_VERSION) && 1 == PGEdit_Update)
                            return this.setDownText(), this.getDownHtml();
                    } else if (8 == this.osBrowser && this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_MacOs_Safari_VERSION) && 1 == PGEdit_Update)
                        return this.setDownText(), this.getDownHtml();
                    return this.getpgeHtml();
                }
                return this.getDownHtml();
            }, needDown: function () {
                if (!this.isInstalled)
                    return !0;
                if (2 == this.osBrowser) {
                    if (this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_FF_VERSION) && 1 == PGEdit_Update)
                        return !0;
                } else if (6 == this.osBrowser) {
                    if (this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_MacOs_VERSION) && 1 == PGEdit_Update)
                        return !0;
                } else if (8 == this.osBrowser && this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_MacOs_Safari_VERSION) && 1 == PGEdit_Update)
                    return !0;
                return !1;
            }, isSupport: function () {
                return 1 == this.osBrowser || 3 == this.osBrowser || 2 == this.osBrowser || 6 == this.osBrowser || 8 == this.osBrowser ? !0 : !1;
            }, generate: function () {
                if (10 == this.osBrowser || 11 == this.osBrowser)
                    return document.write(this.getpgeHtml());
                if (2 == this.osBrowser) {
                    if (0 == this.isInstalled)
                        return document.write(this.getDownHtml());
                    if (this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_FF_VERSION) && 1 == PGEdit_Update)
                        return this.setDownText(), document.write(this.getDownHtml());
                } else if (6 == this.osBrowser) {
                    if (0 == this.isInstalled)
                        return document.write(this.getDownHtml());
                    if (this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_MacOs_VERSION) && 1 == PGEdit_Update)
                        return this.setDownText(), document.write(this.getDownHtml());
                } else if (8 == this.osBrowser) {
                    if (0 == this.isInstalled)
                        return document.write(this.getDownHtml());
                    if (this.getConvertVersion(this.pgeVersion) < this.getConvertVersion(PGEdit_MacOs_Safari_VERSION) && 1 == PGEdit_Update)
                        return this.setDownText(), document.write(this.getDownHtml());
                }
                return document.write(this.getpgeHtml());
            }, pwdclear: function () {
                if (this.isInstalled)
                    if (10 == this.osBrowser || 11 == this.osBrowser) {
                        var t = this.settings.pgeWindowID, e = this.settings.pgeId;
                        $("#" + e).val(""), CLPJSON.id = t;
                        var s = getEnStr(this.settings.pgeRZRandNum, CLPJSON), i = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: s };
                        this.pwdGetData(i);
                    } else {
                        var n = document.getElementById(this.settings.pgeId);
                        n.ClearSeCtrl();
                    }
            }, pwdSetSk: function (t) {
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser || 6 == this.osBrowser || 8 == this.osBrowser)
                            e.input1 = t;
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            e.input(1, t);
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var s = this.settings.pgeWindowID, i = { interfacetype: 1, data: {} };
                            i.id = s, i.data.reg1 = this.settings.pgeEreg1, i.data.aeskey = t;
                            var n = getEnStr(this.settings.pgeRZRandNum, i), a = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: n };
                            this.pwdGetData(a);
                        }
                    } catch (r) {
                        debugConsole && console.log(r);
                    }
            }, pwdResult: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser)
                            t = e.output1;
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            t = e.output(7);
                        else if (6 == this.osBrowser || 8 == this.osBrowser)
                            t = e.get_output1();
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            OUTJSON.id = this.settings.pgeWindowID, OUTJSON.data.datatype = 7, OUTJSON.data.encrypttype = 0;
                            var s = (OUTJSON.id, getEnStr(this.settings.pgeRZRandNum, OUTJSON)), i = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: s };
                            t = this.pwdGetData(i);
                        }
                    } catch (n) {
                        t = "";
                    }
                else
                    t = "";
                return t;
            }, machineNetwork: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser)
                            t = e.GetIPMacList();
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            t = e.output(9);
                        else if (6 == this.osBrowser || 8 == this.osBrowser)
                            t = e.get_output7(0);
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var s = this.settings.pgeWindowID;
                            OUTJSON.id = s, OUTJSON.data.datatype = 9, OUTJSON.data.encrypttype = 0;
                            var i = getEnStr(this.settings.pgeRZRandNum, OUTJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: i };
                            t = this.pwdGetData(n);
                        }
                    } catch (a) {
                        t = "";
                    }
                else
                    t = "";
                return t;
            }, machineDisk: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser)
                            t = e.GetNicPhAddr(1);
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            t = e.output(11);
                        else if (6 == this.osBrowser || 8 == this.osBrowser)
                            t = e.get_output7(2);
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var s = this.settings.pgeWindowID;
                            OUTJSON.id = s, OUTJSON.data.datatype = 11, OUTJSON.data.encrypttype = 0;
                            var i = getEnStr(this.settings.pgeRZRandNum, OUTJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: i };
                            t = this.pwdGetData(n);
                        }
                    } catch (a) {
                        t = "";
                    }
                else
                    t = "";
                return t;
            }, machineCPU: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser)
                            t = e.GetNicPhAddr(2);
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            t = e.output(10);
                        else if (6 == this.osBrowser || 8 == this.osBrowser)
                            t = e.get_output7(1);
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var s = this.settings.pgeWindowID;
                            OUTJSON.id = s, OUTJSON.data.datatype = 10, OUTJSON.data.encrypttype = 0;
                            var i = getEnStr(this.settings.pgeRZRandNum, OUTJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: i };
                            t = this.pwdGetData(n);
                        }
                    } catch (a) {
                        t = "";
                    }
                else
                    t = "";
                return t;
            }, pwdSimple: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser)
                            t = e.output44;
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            t = e.output(13);
                        else if (6 == this.osBrowser || 8 == this.osBrowser)
                            t = e.get_output10();
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var s = this.settings.pgeWindowID;
                            OUTJSON.id = s, OUTJSON.data.datatype = 13, OUTJSON.data.encrypttype = 1, datac = getEnStr(this.settings.pgeRZRandNum, OUTJSON), RZCIJSON = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: datac }, t = this.pwdGetData(RZCIJSON);
                        }
                    } catch (i) {
                        t = "";
                    }
                else
                    t = "";
                return t;
            }, pwdValid: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser)
                            e.output1 && (t = e.output5);
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            t = e.output(5);
                        else if (6 == this.osBrowser || 8 == this.osBrowser)
                            t = e.get_output5();
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var s = this.settings.pgeWindowID;
                            OUTJSON.id = s, OUTJSON.data.datatype = 5, OUTJSON.data.encrypttype = 0;
                            var i = getEnStr(this.settings.pgeRZRandNum, OUTJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: i };
                            t = this.pwdGetData(n, 1);
                        }
                    } catch (a) {
                        t = 1;
                    }
                else
                    t = 1;
                return t;
            }, pwdHash: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser)
                            t = e.output2;
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            t = e.output(2);
                        else if (6 == this.osBrowser || 8 == this.osBrowser)
                            t = e.get_output2();
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var s = this.settings.pgeWindowID;
                            OUTJSON.id = s, OUTJSON.data.datatype = 2, OUTJSON.data.encrypttype = 1;
                            var i = getEnStr(this.settings.pgeRZRandNum, OUTJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: i };
                            t = this.pwdGetData(n);
                        }
                    } catch (a) {
                        t = 0;
                    }
                else
                    t = 0;
                return t;
            }, pwdLength: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = document.getElementById(this.settings.pgeId);
                        if (1 == this.osBrowser || 3 == this.osBrowser)
                            t = e.output3;
                        else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser)
                            t = e.output(3);
                        else if (6 == this.osBrowser || 8 == this.osBrowser)
                            t = e.get_output3();
                        else if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var s = this.settings.pgeWindowID;
                            OUTJSON.id = s, OUTJSON.data.datatype = 3, OUTJSON.data.encrypttype = 0;
                            var i = getEnStr(this.settings.pgeRZRandNum, OUTJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: i };
                            t = this.pwdGetData(n);
                        }
                    } catch (a) {
                        t = 0;
                    }
                else
                    t = 0;
                return t;
            }, pwdStrength: function (t) {
                var e = 0;
                if (this.isInstalled)
                    try  {
                        var s = document.getElementById(this.settings.pgeId);
                        if (10 == this.osBrowser || 11 == this.osBrowser) {
                            var i = this.settings.pgeWindowID;
                            OUTJSON.id = i, OUTJSON.data.datatype = 3, OUTJSON.data.encrypttype = 0;
                            var n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: a }, a = getEnStr(this.settings.pgeRZRandNum, OUTJSON);
                            n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: a };
                            var r = this.pwdGetData(n);
                            OUTJSON.data.datatype = 4, OUTJSON.data.encrypttype = 2, a = getEnStr(this.settings.pgeRZRandNum, OUTJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: a };
                            var o = this.pwdGetData(n);
                            OUTJSON.data.datatype = 4, OUTJSON.data.encrypttype = 1, a = getEnStr(this.settings.pgeRZRandNum, OUTJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: a };
                             {
                                this.pwdGetData(n);
                            }
                            0 == r ? e = 0 : 1 == o || 6 > r ? e = 1 : 2 == o && r >= 6 ? e = 2 : 3 == o && r >= 6 && (e = 3), t && t(e);
                        } else {
                            if (1 == this.osBrowser || 3 == this.osBrowser) {
                                var r = s.output3, o = s.output4;
                                s.output54;
                            } else if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser) {
                                var r = s.output(3), o = s.output(4);
                                s.output(4, 1);
                            } else if (6 == this.osBrowser || 8 == this.osBrowser) {
                                var r = s.get_output3(), o = s.get_output4();
                                s.get_output16();
                            }
                            0 == r ? e = 0 : 1 == o || 6 > r ? e = 1 : 2 == o && r >= 6 ? e = 2 : 3 == o && r >= 6 && (e = 3), t && t(e);
                        }
                    } catch (g) {
                        e = "";
                    }
                else
                    e = 0;
                return e;
            }, checkInstall: function (t) {
                try  {
                    if (1 == this.osBrowser) {
                         {
                            new ActiveXObject("nbcbEdit.nbcbEditCtrl.1");
                        }
                        return !0;
                    }
                    if (2 == this.osBrowser || 4 == this.osBrowser || 5 == this.osBrowser || 6 == this.osBrowser || 8 == this.osBrowser) {
                        var e = new Array;
                        if (6 == this.osBrowser)
                            var s = navigator.plugins["nbcbEdit Safari"].description;
                        else if (8 == this.osBrowser)
                            var s = navigator.plugins["nbcbEdit Safari"].description;
                        else
                            var s = navigator.plugins.nbcbEdit.description;
                        if (s.indexOf(":") > 0) {
                            e = s.split(":");
                             {
                                e[1];
                            }
                        } else
                            ;
                        return !0;
                    }
                    if (3 == this.osBrowser) {
                         {
                            new ActiveXObject("nbcbEditX64.nbcbEditCtrl.1");
                        }
                        return !0;
                    }
                    if (10 == this.osBrowser || 11 == this.osBrowser) {
                        var i = !1;
                        CIJSON.id = this.settings.pgeWindowID;
                        var n = getEnStr(this.settings.pgeRZRandNum, CIJSON), a = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: n };
                        return jQuery.ajax({ timeout: 500, url: urls, type: "GET", async: !1, data: { jsoncallback: "jsoncallback", str: JSON.stringify(a) }, success: function () {
                                i = !0, t && t(!0);
                            }, error: function () {
                                i = !1, t && t(!1);
                            } }), i;
                    }
                } catch (r) {
                    return !1;
                }
            }, getConvertVersion: function (t) {
                try  {
                    if (void 0 == t || "" == t)
                        return 0;
                    var e = t.split("."), s = parseInt(1e3 * e[0]) + parseInt(100 * e[1]) + parseInt(10 * e[2]) + parseInt(e[3]);
                    return s;
                } catch (i) {
                    return 0;
                }
            }, getVersion: function () {
                try  {
                    if (1 == this.osBrowser || 3 == this.osBrowser)
                        var t = $("#" + this.settings.pgeId)[0], e = t.output35;
                    else if (2 == this.osBrowser || 6 == this.osBrowser || 8 == this.osBrowser) {
                        if (navigator.userAgent.indexOf("MSIE") < 0) {
                            var s = new Array;
                            if (6 == this.osBrowser)
                                var i = navigator.plugins["nbcbEdit Safari"].description;
                            else if (8 == this.osBrowser)
                                var i = navigator.plugins["nbcbEdit Safari"].description;
                            else
                                var i = navigator.plugins.nbcbEdit.description;
                            if (i.indexOf(":") > 0) {
                                s = i.split(":");
                                var e = s[1];
                            } else
                                var e = "";
                        }
                    } else if (10 == this.osBrowser || 11 == this.osBrowser) {
                        var n = this.settings.pgeWindowID;
                        OUTJSON.id = n, OUTJSON.data.datatype = 12, OUTJSON.data.encrypttype = 0;
                        var a = getEnStr(this.settings.pgeRZRandNum, OUTJSON), r = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: a };
                        e = this.pwdGetData(r);
                    }
                    return e;
                } catch (o) {
                    return "";
                }
            }, setColor: function () {
                var t = "";
                if (this.isInstalled)
                    try  {
                        var e = $("#" + this.settings.pgeId)[0];
                        void 0 != this.settings.pgeBackColor && "" != this.settings.pgeBackColor && (e.BackColor = this.settings.pgeBackColor), void 0 != this.settings.pgeForeColor && "" != this.settings.pgeForeColor && (e.ForeColor = this.settings.pgeForeColor);
                    } catch (s) {
                        t = "";
                    }
                else
                    t = "";
            }, setDownText: function () {
                void 0 != this.pgeVersion && "" != this.pgeVersion && (this.pgeDownText = "请点此升级控件");
            }, pgInitialize: function () {
                if (this.isInstalled) {
                    (1 == this.osBrowser || 3 == this.osBrowser) && ($("#" + this.settings.pgeId + "_pge").show(), this.getConvertVersion(this.getVersion()) < this.getConvertVersion(PGEdit_IE32_VERSION) && 1 == PGEdit_Update && (void 0 != this.getVersion() && "" != this.getVersion() && (this.pgeDownText = "请点此升级控件", $("#" + this.settings.pgeId + "_down").html("<a href='" + this.settings.pgePath + this.pgeditIEExe + "'>" + this.pgeDownText + "</a>")), $("#" + this.settings.pgeId + "_pge").hide(), $("#" + this.settings.pgeId + "_down").show()));
                    var t = $("#" + this.settings.pgeId)[0];
                    void 0 != this.settings.pgeBackColor && "" != this.settings.pgeBackColor && (t.BackColor = this.settings.pgeBackColor), void 0 != this.settings.pgeForeColor && "" != this.settings.pgeForeColor && (t.ForeColor = this.settings.pgeForeColor);
                } else
                    (1 == this.osBrowser || 3 == this.osBrowser) && $("#" + this.settings.pgeId + "_down").show();
            }, setSX: function (e, m, o) {
                var keynum;
                window.event ? keynum = e.keyCode : e.which && (keynum = e.which), 13 == keynum && (o.blur(), eval("(" + m + ")"));
            }, setCX: function (t) {
                var e = 0;
                if (document.selection) {
                    var s = document.selection.createRange();
                    s.moveStart("character", -t.value.length), e = s.text.length;
                } else
                    (t.selectionStart || "0" == t.selectionStart) && (e = t.selectionStart);
                var i = t.value.length;
                if (i >= e)
                    if (t.setSelectionRange)
                        setTimeout(function () {
                            t.setSelectionRange(i, i);
                        }, 1);
                    else if (t.createTextRange) {
                        var n = t.createTextRange();
                        n.collapse(!0), n.moveEnd("character", i), n.moveStart("character", i), n.select();
                    }
            }, instControl: function (t) {
                var e = this.settings.pgeId, s = this.settings.pgeInstallClass;
                ICJSON.id = t;
                var i = this, n = getEnStr(this.settings.pgeRZRandNum, ICJSON), a = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: n };
                jQuery.ajax({ url: urls, dataType: "jsonp", data: { str: JSON.stringify(a) }, contentType: "application/json;utf-8", jsonp: "jsoncallback", success: function (n) {
                        if (debugConsole && console.log("id:" + t), debugConsole && console.log("x.data:" + n.data + ",x.code:" + n.code), 0 == n.code)
                            debugConsole && console.info("实例化成功");
                        else {
                            if (6 == n.code)
                                return this.pgeDownText = "验签失败!", void $("#" + e).parent().html('<span id="' + e + '_down" class="' + s + '" style="text-align:center;display:block;" ><a id="winA" href="javascript:void(0);">' + this.pgeDownText + "</a></span>");
                            7 == n.code ? debugConsole && console.log("请点击'是'") : 8 == n.code ? debugConsole && console.log("请重新安装控件") : 9 == n.code || (debugConsole && console.info("实例化失败"), debugConsole && console.info("data:" + n.data));
                        }
                        i.initControl(t), i.getVersion();
                    }, error: function (t, e, s) {
                        debugConsole && console.log(s);
                    } }), inFlag[t] = { flag: !1 };
            }, initControl: function (t) {
                INCJSON.id = t, INCJSON.data.edittype = this.settings.pgeEdittype, INCJSON.data.maxlength = this.settings.pgeMaxlength, INCJSON.data.reg1 = this.settings.pgeEreg1, INCJSON.data.reg2 = this.settings.pgeEreg2, 10 == this.osBrowser ? INCJSON.data.lic = { liccode: license, url: "aHR0cDovLzE5Mi4xNjguMS4xMTg6ODA4Ny9EZW1vWF9BTExfQUVTL2xvZ2luLmpzcA==" } : 11 == this.osBrowser && (INCJSON.data.lic = { liccode: licenseMac, url: "aHR0cDovLzE5Mi4xNjguMS4xMTg6ODA4Ny9EZW1vWF9BTExfQUVTL2xvZ2luLmpzcA==" });
                var e = getEnStr(this.settings.pgeRZRandNum, INCJSON), s = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: e };
                jQuery.ajax({ url: urls, dataType: "jsonp", data: { str: JSON.stringify(s) }, contentType: "application/json;utf-8", jsonp: "jsoncallback", success: function (e) {
                        debugConsole && console.log("id:" + t), debugConsole && console.log("x.data:" + e.data + ",x.code:" + e.code), isInit[t] = !0, console.info(0 == e.code ? "设置参数成功" : "data:" + e.data);
                    }, error: function (t, e, s) {
                        debugConsole && console.log(s);
                    } }), onceInterv[t] = "";
            }, openProt: function (t, e) {
                var s = this;
                inFlag[t].flag = !1, OPJSON.id = t;
                var i = getEnStr(this.settings.pgeRZRandNum, OPJSON), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: i };
                if (jQuery.ajax({ url: urls, dataType: "jsonp", data: { str: JSON.stringify(n) }, contentType: "application/json;utf-8", jsonp: "jsoncallback", success: function (e) {
                        debugConsole && console.log("成功开启保护"), debugConsole && console.log("x.data:" + e.data + ",x.code:" + e.code), inFlag[t].flag = !0;
                    }, error: function (t, e, s) {
                        debugConsole && console.log(s);
                    } }), "string" == typeof onceInterv[t]) {
                    for (var a = 0; a < iterArray.length; a++)
                        window.clearInterval(iterArray[a]);
                    onceInterv[t] = window.setInterval(function () {
                        s.intervlOut(t, e);
                    }, 800), iterArray.push(onceInterv[t]);
                }
            }, intervlOut: function (t, e) {
                var s = !0, i = this.settings.pgeInstallClass;
                XTJSON.id = t;
                var n = getEnStr(this.settings.pgeRZRandNum, XTJSON), a = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: n };
                jQuery.ajax({
                    url: urls, dataType: "jsonp", data: { str: JSON.stringify(a) }, contentType: "application/json;utf-8", jsonp: "jsoncallback", success: function (i) {
                        s = !1;
                        var n = document.getElementById(e), a = n.value.length;
                        debugConsole && console.log(t + "的长度：" + a), debugConsole && console.log("x.data(长度)：" + i.data + ",x.code:" + i.code);
                        for (var r = "", o = 0; o < i.data; o++)
                            r += "*";
                        n.value = r;
                    }, error: function (t, s, n) {
                        debugConsole && console.log("--------+++++++++++"), this.pgeDownText = "控件进程异常停止!", $("#" + e).parent().html('<span  id="' + e + '_down" class="' + i + '" style="text-align:center;display:block;" ><a id="winA" href="javascript:void(0);">' + this.pgeDownText + "</a></span>"), debugConsole && console.log(n);
                    }, complete: function (t, s) {
                        return "timeout" == s ? (this.pgeDownText = "控件进程超时!", void $("#" + e).parent().html('<span  id="' + e + '_down" class="' + i + '" style="text-align:center;display:block;" ><a id="winA" href="javascript:void(0);">' + this.pgeDownText + "</a></span>")) : void 0;
                    } });
            }, closeProt: function (t) {
                CPJSON.id = t;
                var e = getEnStr(this.settings.pgeRZRandNum, CPJSON), s = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: e };
                if (jQuery.ajax({ url: urls, dataType: "jsonp", data: { str: JSON.stringify(s) }, contentType: "application/json;utf-8", jsonp: "jsoncallback", success: function (e) {
                        debugConsole && console.log("关闭密码控件保护成功"), debugConsole && console.log("x.data:" + e.data + ",x.code:" + e.code), inFlag[t].flag = !1;
                    }, error: function (t, e, s) {
                        debugConsole && console.log(s);
                    } }), "number" == typeof onceInterv[t]) {
                    for (var i = 0; i < iterArray.length; i++)
                        window.clearInterval(iterArray[i]);
                    onceInterv[t] = "";
                }
            }, ajaxOnce: function (t) {
                var e = "012345" + (new Date).getTime() + t;
                jQuery.ajax({ url: urls, dataType: "jsonp", data: { str: e }, contentType: "application/json;utf-8", jsonp: "jsoncallback", success: function () {
                    }, error: function (t, e, s) {
                        debugConsole && console.log(s);
                    } });
            }, moreJson: function (t) {
                var e = this.settings.pgeWindowID, s = { id: this.settings.pgeWindowID, interfacetype: 2, data: [{ datatype: 7, encrypttype: 0 }, { datatype: 9, encrypttype: 0 }, { datatype: 11, encrypttype: 0 }, { datatype: 10, encrypttype: 0 }] }, i = getEnStr(this.settings.pgeRZRandNum, s), n = { rankey: this.settings.pgeRZRandNum, datab: this.settings.pgeRZDataB, datac: i };
                jQuery.ajax({ url: urls, dataType: "jsonp", data: { str: JSON.stringify(n) }, contentType: "application/json;utf-8", jsonp: "jsoncallback", success: function (s) {
                        outs[e].aes = s[0].data, outs[e].mac = s[1].data, outs[e].hard = s[2].data, outs[e].cpu = s[3].data, t && t(s);
                    }, error: function (e, s, i) {
                        debugConsole && console.log(i), t && t(x);
                    } });
            }, pwdGetData: function (t, e) {
                var s = "";
                return jQuery.ajax({ timeout: 500, url: urls, type: "GET", async: !1, data: { jsoncallback: "jsoncallback", str: JSON.stringify(t) }, success: function (t) {
                        t = t.substring(13, t.length - 1), t = JSON.parse(t), s = t.data;
                    }, error: function () {
                        s = e ? e : "";
                    } }), s;
            } } });
}(jQuery);
;
angular.module("placeholder", []).directive("placeholder", function () {
    var e = "placeholder" in document.createElement("input");
    return { restrict: "A", require: "ngModel", link: function (r, l, a, t) {
            function n(e) {
                return i && "null" != i || (i = ""), o.text(e ? "" : i), e;
            }
            if (!e) {
                var c = l.attr("id"), i = "";
                c || (c = "placeholder-" + (new Date).getTime(), l.attr("id", c));
                var o = $('<label class="placeholder-ex" for="' + c + '"></label>');
                l.wrap('<div class="placeholder-wrap"></div>'), l.parent().prepend(o), a.$observe("placeholder", function (e) {
                    i = e, n(l.val());
                }), t.$parsers.push(n), t.$formatters.push(n);
            }
        } };
});
;
angular.module("pop", []).directive("pop", ["$rootScope", "$appConfig", "$sniffer", function (n, o, e) {
        return { restrict: "EAC", scope: { caption: "=", src: "=", popType: "=", buttons: "=", onClose: "&", isShow: "=", className: "=", entityClassName: "=" }, template: '<div ng-show="(!!isShow)"><div class="pop-backdrop"><iframe frameborder="0" style="width:100%;height:100%;"></iframe></div><div class="pop-entity {{entityClassName}}" ng-class="{\'pop-content\':(popType == \'0\'),\'pop-page\':(popType == \'1\')}"><div class="pop-container"><div class="pop-head clearfix"><span class="pop-title" ng-bind="caption"></span><span class="pop-close" ng-click="__close({\'$type\':\'0\',\'$object\':null});"></span></div><div class="pop-body {{className}}" ng-if="(popType == \'1\')" ng-init="bodyInit();"><div ng-include="src"></div></div><div class="pop-body {{className}}" ng-if="(popType == \'0\')"><div ng-bind-html="src"></div></div><div class="pop-foot"><button ng-repeat="btn in buttons" ng-click="__close({\'$type\':\'1\',\'$object\':btn});" ng-class="btn.className" ng-bind="btn.title"></button></div></div></div></div>', link: function (t, i) {
                function s() {
                    t.isShow = !1, t.popType = "", t.entityClassName = "";
                }
                if (e.msie <= 8)
                    try  {
                        var c = i.find("iframe");
                        c[0].src = "javascript:document.write('<script>document.domain=\"" + o.domain + "\"</script>')";
                        var p = c[0].contentDocument ? c[0].contentDocument : c[0].contentWindow ? c[0].contentWindow.document : c[0].document;
                        p && (p.open(), p.write('<html><head><script>document.domain="' + o.domain + "\"</script></head><body style='background-color:#666'></body></html>"), p.close());
                    } catch (a) {
                    }
                t.__isPop = !0, n.$on("$locationChangeSuccess", function (n, o, e) {
                    o != e && s();
                }), t.$watch("isShow", function (n) {
                    n ? (i.removeClass("ng-Hide"), i.show()) : (i.addClass("ng-Hide"), i.hide());
                }), t.bodyInit = function () {
                    var n = i.find(".pop-body").eq(0);
                    navigator.userAgent.indexOf("Firefox") > 0 ? n[0].addEventListener("DOMMouseScroll", function (o) {
                        n[0].scrollTop += o.detail > 0 ? 60 : -60, o.preventDefault();
                    }, !1) : n[0].onmousewheel = function (o) {
                        return o = o || window.event, n[0].scrollTop += o.wheelDelta > 0 ? -60 : 60, !1;
                    };
                }, t.__close = function (n) {
                    t.onClose && t.onClose(n);
                };
            } };
    }]);
;
angular.module("print", []).directive("print", ["$log", "$appConfig", "$sniffer", function (t, e, n) {
        return { restrict: "A", scope: { print: "=" }, link: function (t, o) {
                function r() {
                    if (m.mode == l.iframe)
                        return "";
                    if (m.standard == u.html5)
                        return "<!DOCTYPE html>";
                    var t = m.standard == u.loose ? " Transitional" : "", e = m.standard == u.loose ? "loose" : "strict";
                    return '<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01' + t + '//EN" "http://www.w3.org/TR/html4/' + e + '.dtd">';
                }
                function i() {
                    var t = "", o = "";
                    return m.extraHead && m.extraHead.replace(/([^,]+)/g, function (e) {
                        t += e;
                    }), $(document).find("link").filter(function () {
                        var t = $(this).attr("rel");
                        return "undefined" === $.type(t) == 0 && "stylesheet" == t.toLowerCase();
                    }).filter(function () {
                        var t = $(this).attr("media");
                        return "undefined" === $.type(t) || "" == t || "print" == t.toLowerCase() || "all" == t.toLowerCase();
                    }).each(function () {
                        o += '<link type="text/css" rel="stylesheet" href="' + $(this).attr("href") + '" >';
                    }), m.extraCss && m.extraCss.replace(/([^,\s]+)/g, function (t) {
                        o += '<link type="text/css" rel="stylesheet" href="' + t + '">';
                    }), n.msie <= 8 ? '<head><script>document.domain="' + e.domain + '"</script><title>' + m.popTitle + "</title>" + t + o + "</head>" : "<head><title>" + m.popTitle + "</title>" + t + o + "</head>";
                }
                function a(t) {
                    var e = "", n = m.retainAttr;
                    return t.each(function () {
                        for (var t = c($(this)), o = "", r = 0; r < n.length; r++) {
                            var i = $(t).attr(n[r]);
                            i && (o += (o.length > 0 ? " " : "") + n[r] + "='" + i + "'");
                        }
                        e += "<div " + o + ">" + $(t).html() + "</div>";
                    }), "<body style='padding-top:0;padding-bottom: 0;'>" + e + "</body>";
                }
                function c(t) {
                    var e = t.clone(), n = $("input,select,textarea", e);
                    return $("input,select,textarea", t).each(function (t) {
                        var e = $(this).attr("type");
                        "undefined" === $.type(e) && (e = $(this).is("select") ? "select" : $(this).is("textarea") ? "textarea" : "");
                        var o = n.eq(t);
                        "radio" == e || "checkbox" == e ? o.attr("checked", $(this).is(":checked")) : "text" == e ? o.attr("value", $(this).val()) : "select" == e ? $(this).find("option").each(function (t) {
                            $(this).is(":selected") && $("option", o).eq(t).attr("selected", !0);
                        }) : "textarea" == e && o.text($(this).val());
                    }), e;
                }
                function s() {
                    var t, n = m.id, o = "border:0;position:absolute;width:880px;height:0px;left:0px;top:0px;";
                    try  {
                        t = document.getElementById("printIframe"), t.src = "javascript:document.write('<script>document.domain=\"" + e.domain + "\"</script>')", t.doc = null, t.doc = t.contentDocument ? t.contentDocument : t.contentWindow ? t.contentWindow.document : t.document;
                    } catch (r) {
                        try  {
                            t = document.createElement("iframe"), document.body.appendChild(t), $(t).attr({ style: o, id: n, src: "" }), t.doc = null, t.doc = t.contentDocument ? t.contentDocument : t.contentWindow ? t.contentWindow.document : t.document;
                        } catch (i) {
                            throw i + ". iframes may not be supported in this browser.";
                        }
                    }
                    if (null == t.doc)
                        throw "Cannot find document.";
                    return t;
                }
                function d() {
                    var t = "location=yes,statusbar=no,directories=no,menubar=no,titlebar=no,toolbar=no,dependent=no";
                    t += ",width=" + m.popWd + ",height=" + m.popHt, t += ",resizable=yes,screenX=" + m.popX + ",screenY=" + m.popY + ",personalbar=no,scrollbars=yes";
                    var e = window.open("", "_blank", t);
                    return e.doc = e.document, e;
                }
                var p = 0, l = { iframe: "iframe", popup: "popup" }, u = { strict: "strict", loose: "loose", html5: "html5" }, h = { mode: l.iframe, standard: u.html5, popHt: 500, popWd: 400, popX: 200, popY: 200, popTitle: "", popClose: !1, extraCss: "", extraHead: "", retainAttr: ["id", "class", "style"] }, m = {}, f = function (t) {
                    $.extend(m, h, t), p++;
                    var e = "shinePrintArea_";
                    $("[id^=" + e + "]").remove(), m.id = e + p;
                    var n, c;
                    switch (m.mode) {
                        case l.iframe:
                            var u = new s;
                            n = u.doc, c = u.contentWindow || u;
                            break;
                        case l.popup:
                            c = new d, n = c.doc;
                    }
                    n.open(), n.write(r() + "<html>" + i() + a($(o)) + "</html>"), n.close(), $(n).ready(function () {
                        c.focus(), c.print(), m.mode == l.popup && m.popClose && setTimeout(function () {
                            c.close();
                        }, 2e3);
                    });
                };
                t.print = { print: f };
            } };
    }]);
;
angular.module("security", []).directive("securitytool", ["$globalData", "$httpPlus", "$utb", "$log", "$interval", function ($globalData, $httpPlus, $utb, $log, $interval) {
        function Messenger(id) {
            var debugConsole = !0, urls = "https://windows10.microdone.cn:5426";
            this.sid = "sign" + (new Date).getTime() + 1, this.pgeRZRandNum = "", this.pgeRZDataB = "", this.voucher = $globalData.getSessionContext("session_voucher"), this.sdkversion = "1";
            var ua = navigator.userAgent.toLowerCase(), chr = "", regStr_chrome = /chrome\/[\d.]+/gi;
            if (ua.indexOf("chrome") > 0) {
                var chromeVersion = ua.match(regStr_chrome).toString();
                chromeVersion = parseInt(chromeVersion.replace(/[^0-9.]/gi, "")), chr = chromeVersion >= 42 ? "chromeh" : "chromel";
            }
            this.env = { isWindows: -1 != ua.indexOf("windows") || -1 != ua.indexOf("win32") || -1 != ua.indexOf("win64"), isMac: -1 != ua.indexOf("macintosh") || -1 != ua.indexOf("mac os x"), isLinux: -1 != ua.indexOf("linux"), ie: -1 != ua.indexOf("msie") || -1 != ua.indexOf("trident"), firefox: -1 != ua.indexOf("firefox"), chrome: -1 != chr.indexOf("chromel"), opera: -1 != ua.indexOf("opera"), safari: -1 != ua.indexOf("version"), edge: -1 != ua.indexOf("edge") || -1 != chr.indexOf("chromeh") }, this.unserialize = function (e) {
                if ("string" == typeof e) {
                    for (var t = {}, i = e.split("&"), n = 0; n < i.length; n++) {
                        var s = i[n].split("=");
                        2 == s.length && (t[s[0]] = decodeURIComponent(s[1]));
                    }
                    return t;
                }
            }, this.createTag = function (e, t) {
                return this.env.isWindows && this.env.ie ? "<object id='" + id + "' classid='CLSID:EC176F0A-69BE-4059-8546-B01EF8C0FB9C' width='0' height='0'></object>" : this.env.edge ? (this.pgeRZRandNum = e, void (this.pgeRZDataB = t)) : this.env.isWindows || this.env.isMac ? "<embed id='" + id + "' type='application/x-sign-messenger' width='0' height='0'>" : "";
            }, this.high = null, this.admin = null, this.getObj = function (e) {
                var t = $("#" + id)[0];
                return void 0 != e && this.env.ie ? e ? (null == this.admin && (this.admin = t.getAdmin(e)), this.admin) : (null == this.high && (this.high = t.getAdmin(e)), this.high) : t;
            }, this.DNConvert = function (e) {
                var t = e.replace(/\s=/g, "=");
                return t = t.replace(/=\s/g, "="), t = t.replace(/=/g, ':"'), t = t.replace(/cn:/g, "CN:"), t = "{" + t.replace(/,/g, '",') + '"}', -1 != t.toUpperCase().indexOf("SN:") && (t = t.toUpperCase(), t = t.replace(/SN/g, "sn"), t = t.replace(/\s/g, ""), t = t.replace(/"(0+)/g, '"')), t;
            }, this.getVersion = function () {
                var e = null;
                if (this.env.edge) {
                    var t = { interfacetype: 1, data: {} }, i = getEnStr(this.pgeRZRandNum, t), n = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: i };
                    jQuery.ajax({ timeout: 500, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(n) }, success: function (t) {
                            debugConsole && console.log(JSON.stringify(t)), t = t.substring(13, t.length - 1), t = JSON.parse(t), e = t.data, debugConsole && console.log("返回版本号————" + JSON.stringify(t) + "————version:" + e);
                        } });
                } else
                    try  {
                        e = this.env.ie ? this.getObj(!1).version : navigator.plugins.SignMessenger.description;
                    } catch (s) {
                        return !1;
                    }
                return e;
            };
            var keychain, certificate, collection, evaldn, token;
            this.sign = function (dn, content) {
                if ("" == dn || "" == content)
                    return !1;
                try  {
                    if (!this.env.edge) {
                        if (this.env.isMac) {
                            certificate = null;
                            var token1, token2, token3, data;
                            if (token1 = this.getObj(!1).token("libnbcb_personal"), token2 = this.getObj(!1).token("libshuttle_p11v220.1.0.0_nb"), token3 = this.getObj(!1).token("libI3KP11_Ningbo_nb"), null == token1 && null == token2 && null == token3)
                                return !1;
                            if (null != token1 && token1.keyCount()) {
                                this.getObj(!1).codepage = -1, token1.selectKey(0);
                                var certificateCount = parseInt(token1.certificateCount() + "");
                                if (!(certificateCount > 0))
                                    return !1;
                                if (certificate = token1.getActiveCertificate(), null == certificate)
                                    return !1;
                                data = "328" == this.voucher ? certificate.sign(content, parseInt(549)) : certificate.sign(content, parseInt(32));
                            } else if (null != token2 && token2.keyCount()) {
                                this.getObj(!1).codepage = -1, token2.selectKey(0);
                                var certificateCount = parseInt(token2.certificateCount() + "");
                                if (!(certificateCount > 0))
                                    return !1;
                                if (certificate = token2.getActiveCertificate(), null == certificate)
                                    return !1;
                                data = "328" == this.voucher ? certificate.sign(content, parseInt(549)) : certificate.sign(content, parseInt(32));
                            } else {
                                if (null == token3 || !token3.keyCount())
                                    return !1;
                                this.getObj(!1).codepage = 65001, token3.selectKey(0);
                                var certificateCount = parseInt(token3.certificateCount() + "");
                                if (!(certificateCount > 0))
                                    return !1;
                                if (certificate = token3.getActiveCertificate(), null == certificate)
                                    return !1;
                                data = "328" == this.voucher ? certificate.sign(content, parseInt(549)) : certificate.sign(content, parseInt(544));
                            }
                            return void 0 == data ? !1 : (data || (data = {}), "string" == typeof data && data.indexOf("code") > 0 && data.indexOf("message") > 0 && (data = {}), data);
                        }
                        keychain = "328" == this.voucher ? this.getObj(!1).keychain("nbcb") : this.getObj(!1).keychain();
                        var dn_new = this.DNConvert(dn);
                        if (dn_new = eval("(" + dn_new + ")"), collection = keychain.query(dn_new), "328" == this.voucher) {
                            var o = this.getObj(!1);
                            if (o.codepage = 65001, collection.length > 1) {
                                certificate = collection.userChoose(), certificate.setPin("");
                                var data = certificate.sign(content, 544);
                                return certificate.setPin(""), "string" == typeof data && data.indexOf("code") > 0 && data.indexOf("message") > 0 && (data = {}), data;
                            }
                            certificate = collection.item(0), certificate.setPin("");
                            var data = certificate.sign(content, 544);
                            return certificate.setPin(""), "string" == typeof data && data.indexOf("code") > 0 && data.indexOf("message") > 0 && (data = {}), data;
                        }
                        if ("328" != this.voucher && "343" != this.voucher && "360" != this.voucher) {
                            var o = this.getObj(!1);
                            if (o.codepage = -1, collection.length > 1) {
                                certificate = collection.userChoose(), certificate.setPin("");
                                var data = certificate.sign(content, 36);
                                return certificate.setPin(""), "string" == typeof data && data.indexOf("code") > 0 && data.indexOf("message") > 0 && (data = {}), data;
                            }
                            certificate = collection.item(0), certificate.setPin("");
                            var data = certificate.sign(content, 36);
                            return certificate.setPin(""), "string" == typeof data && data.indexOf("code") > 0 && data.indexOf("message") > 0 && (data = {}), data;
                        }
                        var o = this.getObj(!1);
                        if (o.codepage = 65001, collection.length > 1) {
                            certificate = collection.userChoose(), certificate.setPin("");
                            var data = certificate.sign(content, 544);
                            return certificate.setPin(""), "string" == typeof data && data.indexOf("code") > 0 && data.indexOf("message") > 0 && (data = {}), data;
                        }
                        certificate = collection.item(0), certificate.setPin("");
                        var data = certificate.sign(content, 544);
                        return certificate.setPin(""), "string" == typeof data && data.indexOf("code") > 0 && data.indexOf("message") > 0 && (data = {}), data;
                    }
                    if (!this.env.isMac) {
                        var provider, keyFlag;
                        provider = "328" == this.voucher ? this.getProvider("nbcb", 0, 0, 65001) : "343" == this.voucher || "360" == this.voucher ? this.getProvider("", 0, 0, 65001) : this.getProvider("", 0, 0, -1), keyFlag = "328" == this.voucher ? 544 : "328" != this.voucher && "343" != this.voucher && "360" != this.voucher ? 36 : 544, pid = provider.pid;
                        var cn = this.DNConvert(dn);
                        cn = eval("(" + cn + ")");
                        var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                        if (certnumber > 0) {
                            certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                            var contentBase64 = base64encode(utf16to8(this.trimRight(content))), sign = this.signature(pid, contentBase64, keyFlag, 1);
                            return this.setPin(pid, "", 5), 0 == sign.code ? sign.signed : {};
                        }
                        return !1;
                    }
                    var provider, keyFlag;
                    if ("328" == this.voucher ? (keyFlag = 549, provider = this.getProvider("libI3KP11_Ningbo_nb", 1, 0, 65001)) : "343" == this.voucher || "360" == this.voucher ? (keyFlag = 544, provider = this.getProvider("libI3KP11_Ningbo_nb", 1, 0, 65001)) : (provider = "329" == this.voucher ? this.getProvider("libshuttle_p11v220.1.0.0_nb", 1, 0, -1) : this.getProvider("libnbcb_personal", 1, 0, -1), keyFlag = 32), code = provider.code, pid = provider.pid, 0 != code && 6 != code)
                        return !1;
                    var keyCount = this.keyCount(pid, 7);
                    if (keycount = keyCount.keycount, keycount > 0) {
                        this.selectKey(pid, 0, 8);
                        var cn = this.DNConvert(dn);
                        cn = eval("(" + cn + ")");
                        var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                        if (certnumber > 0) {
                            certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                            var contentBase64 = base64encode(utf16to8(this.trimRight(content))), sign = this.signature(pid, contentBase64, keyFlag, 1);
                            return this.setPin(pid, "", 5), 0 == sign.code ? sign.signed : {};
                        }
                        return !1;
                    }
                } catch (err) {
                    return debugConsole && console.log(err), !1;
                }
            }, this.getcert = function (dn) {
                if ("" == dn)
                    return !1;
                try  {
                    if (this.env.edge)
                        return this.getcertEdge(dn);
                    if (this.env.isMac) {
                        certificate = null;
                        var token1, token2, token3;
                        if (token1 = this.getObj(!1).token("libnbcb_personal"), token2 = this.getObj(!1).token("libshuttle_p11v220.1.0.0_nb"), token3 = this.getObj(!1).token("libI3KP11_Ningbo_nb"), null == token1 && null == token2 && null == token3)
                            return !1;
                        if (null != token1 && token1.keyCount()) {
                            if (this.getObj(!1).codepage = -1, token1.selectKey(0), !token1.certificateCount())
                                return !1;
                            if (certificate = token1.getActiveCertificate(), null == certificate)
                                return !1;
                        } else if (null != token2 && token2.keyCount()) {
                            if (this.getObj(!1).codepage = -1, token2.selectKey(0), !token2.certificateCount())
                                return !1;
                            if (certificate = token2.getActiveCertificate(), null == certificate)
                                return !1;
                        } else if (null != token3 && token3.keyCount()) {
                            if (this.getObj(!1).codepage = 65001, token3.selectKey(0), !token3.certificateCount())
                                return !1;
                            if (certificate = token3.getActiveCertificate(), null == certificate)
                                return !1;
                        }
                        return certificate.toString();
                    }
                    keychain = "328" == this.voucher ? this.getObj(!1).keychain("nbcb") : this.getObj(!1).keychain();
                    var dn_new = this.DNConvert(dn);
                    if (dn_new = eval("(" + dn_new + ")"), collection = keychain.query(dn_new), "328" != this.voucher && "343" != this.voucher && "360" != this.voucher) {
                        var o = this.getObj(!1);
                        o.codepage = -1;
                    } else {
                        var o = this.getObj(!1);
                        o.codepage = 65001;
                    }
                    return collection.length > 1 ? (certificate = collection.userChoose(), certificate.toString()) : (certificate = collection.item(0), certificate.toString());
                } catch (err) {
                    return debugConsole && console.log(err), !1;
                }
            }, this.certInfo = function (dn) {
                if ("" == dn)
                    return !1;
                try  {
                    if (this.env.edge)
                        return this.certInfoEdge(dn);
                    if (this.env.isMac) {
                        certificate = null;
                        var token1, token2, token3;
                        if (token1 = this.getObj(!1).token("libnbcb_personal"), token2 = this.getObj(!1).token("libshuttle_p11v220.1.0.0_nb"), token3 = this.getObj(!1).token("libI3KP11_Ningbo_nb"), null == token1 && null == token2 && null == token3)
                            return !1;
                        if (null != token1 && token1.keyCount()) {
                            if (this.getObj(!1).codepage = -1, token1.selectKey(0), !token1.certificateCount())
                                return !1;
                            if (certificate = token1.getActiveCertificate(), null == certificate)
                                return !1;
                        } else if (null != token2 && token2.keyCount()) {
                            if (this.getObj(!1).codepage = -1, token2.selectKey(0), !token2.certificateCount())
                                return !1;
                            if (certificate = token2.getActiveCertificate(), null == certificate)
                                return !1;
                        } else if (null != token3 && token3.keyCount()) {
                            if (this.getObj(!1).codepage = 65001, token3.selectKey(0), !token3.certificateCount())
                                return !1;
                            if (certificate = token3.getActiveCertificate(), null == certificate)
                                return !1;
                        }
                        return certificate.CN ? certificate.CN : certificate.subject(new Object).CN;
                    }
                    keychain = "328" == this.voucher ? this.getObj().keychain("nbcb") : this.getObj().keychain();
                    var dn_new = this.DNConvert(dn);
                    if (dn_new = eval("(" + dn_new + ")"), collection = keychain.query(dn_new), "328" != this.voucher && "343" != this.voucher && "360" != this.voucher) {
                        var o = this.getObj(!1);
                        o.codepage = -1;
                    } else {
                        var o = this.getObj(!1);
                        o.codepage = 65001;
                    }
                    return collection.length > 1 ? (certificate = collection.userChoose(), certificate.subject(new Object).CN) : (certificate = collection.item(0), certificate.subject(new Object).CN);
                } catch (err) {
                    return !1;
                }
            };
            var code;
            this.getProvider = function (e, t, i, n) {
                var s, r = { interfacetype: 0, data: {} };
                r.id = this.sid, r.data.name = e, r.data.type = t, r.data.pcode = i, r.data.codepage = n, debugConsole && console.log("获取provider————" + JSON.stringify(r));
                var c = (r.id, getEnStr(this.pgeRZRandNum, r)), o = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: c };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(o) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回provider————" + JSON.stringify(e) + "————code:" + code + "————x.pid:" + e.pid), s = e;
                    } }), s;
            }, this.getCsr = function (e, t, i) {
                var n, s = { interfacetype: 0, data: {} };
                s.id = this.sid, s.data.pid = e, s.data.pcode = t, s.data.csrinfo = i;
                var r = (s.id, getEnStr(this.pgeRZRandNum, s)), c = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: r };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(c) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回csr————" + JSON.stringify(e) + "————code:" + code + "————x.csr:" + e.csr), n = e;
                    } }), n;
            }, this.getcertEdge = function (dn) {
                if ("" == dn)
                    return !1;
                if (!this.env.isMac) {
                    var provider = null;
                    provider = "328" == this.voucher ? this.getProvider("nbcb", 0, 0, 65001) : "343" == this.voucher || "360" == this.voucher ? this.getProvider("", 0, 0, 65001) : this.getProvider("", 0, 0, -1), pid = provider.pid;
                    var cn = this.DNConvert(dn);
                    cn = eval("(" + cn + ")");
                    var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                    if (certnumber > 0) {
                        certlist = certs.certlist, debugConsole && console.log(certlist), certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                        var cert = this.getCertificateinfo(pid, 0);
                        return cert = cert.certinfo.pub;
                    }
                    return !1;
                }
                var provider;
                if (provider = "328" == this.voucher || "343" == this.voucher || "360" == this.voucher ? this.getProvider("libI3KP11_Ningbo_nb", 1, 0, 65001) : "329" == this.voucher ? this.getProvider("libshuttle_p11v220.1.0.0_nb", 1, 0, -1) : this.getProvider("libnbcb_personal", 1, 0, -1), code = provider.code, pid = provider.pid, 0 != code && 6 != code)
                    return !1;
                var keyCount = this.keyCount(pid, 7);
                if (keycount = keyCount.keycount, keycount > 0) {
                    this.selectKey(pid, 0, 8);
                    var cn = this.DNConvert(dn);
                    cn = eval("(" + cn + ")");
                    var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                    if (certnumber > 0) {
                        certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                        var cert = this.getCertificateinfo(pid, 0);
                        return cert = cert.certinfo.pub;
                    }
                    return !1;
                }
            }, this.certInfoEdge = function (dn) {
                if ("" == dn)
                    return !1;
                if (!this.env.isMac) {
                    var provider = null;
                    provider = "328" == this.voucher ? this.getProvider("nbcb", 0, 0, 65001) : "343" == this.voucher || "360" == this.voucher ? this.getProvider("", 0, 0, 65001) : this.getProvider("", 0, 0, -1), pid = provider.pid;
                    var cn = this.DNConvert(dn);
                    cn = eval("(" + cn + ")");
                    var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                    if (certnumber > 0) {
                        certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                        var cert = this.getCertificateinfo(pid, 0);
                        return cn = cert.certinfo.cn;
                    }
                    return !1;
                }
                var provider;
                if (provider = "328" == this.voucher || "343" == this.voucher || "360" == this.voucher ? this.getProvider("libI3KP11_Ningbo_nb", 1, 0, 65001) : "329" == this.voucher ? this.getProvider("libshuttle_p11v220.1.0.0_nb", 1, 0, -1) : this.getProvider("libnbcb_personal", 1, 0, -1), code = provider.code, pid = provider.pid, 0 != code && 6 != code)
                    return !1;
                var keyCount = this.keyCount(pid, 7);
                if (keycount = keyCount.keycount, keycount > 0) {
                    this.selectKey(pid, 0, 8);
                    var cn = this.DNConvert(dn);
                    cn = eval("(" + cn + ")");
                    var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                    if (certnumber > 0) {
                        certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                        var cert = this.getCertificateinfo(pid, 0);
                        return cn = cert.certinfo.cn;
                    }
                    return !1;
                }
            }, this.getCertificateList = function (e, t, i) {
                var n, s = { interfacetype: 0, data: {} };
                s.id = this.sid, s.data.pid = e, s.data.pcode = t, s.data.query = i, this.env.isMac && (s.data.query = { C: "CN" }), debugConsole && console.log("获取证书列表————" + JSON.stringify(s));
                var r = (s.id, getEnStr(this.pgeRZRandNum, s)), c = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: r };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(c) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回证书列表————" + JSON.stringify(e) + "————code:" + code), n = e;
                    } }), n;
            }, this.selectCertificate = function (e, t, i, n) {
                var s, r = { interfacetype: 0, data: {} };
                r.id = this.sid, r.data.pid = e, r.data.certid = t, r.data.pcode = i, r.data.type = n, debugConsole && console.log("获取选择证书————" + JSON.stringify(r));
                var c = (r.id, getEnStr(this.pgeRZRandNum, r)), o = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: c };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(o) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回选择证书————" + JSON.stringify(e) + "————code:" + code), s = e;
                    } }), s;
            }, this.keyCount = function (e, t) {
                var i, n = { interfacetype: 0, data: {} };
                n.id = this.sid, n.data.pid = e, n.data.pcode = t;
                var s = (n.id, getEnStr(this.pgeRZRandNum, n)), r = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: s };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(r) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回key个数————" + JSON.stringify(e) + "————code:" + code + "————keycount" + e.keycount), i = e;
                    } }), i;
            }, this.selectKey = function (e, t, i) {
                var n, s = { interfacetype: 0, data: {} };
                s.id = this.sid, s.data.pid = e, s.data.keyindex = t, s.data.pcode = i;
                var r = (s.id, getEnStr(this.pgeRZRandNum, s)), c = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: r };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(c) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回选择key————" + JSON.stringify(e) + "————code:" + code), n = e;
                    } }), n;
            }, this.getCertificateinfo = function (e, t) {
                var i, n = { interfacetype: 0, data: {} };
                n.id = this.sid, n.data.pid = e, n.data.ccode = t;
                var s = (n.id, getEnStr(this.pgeRZRandNum, n)), r = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: s };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(r) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, cn = e.certinfo.cn, certinfo = e.certinfo, debugConsole && console.log("返回证书信息————" + JSON.stringify(e) + "————code:" + code), i = e;
                    } }), i;
            }, this.signature = function (e, t, i, n) {
                var s, r = { interfacetype: 0, data: {} };
                r.id = this.sid, r.data.pid = e, r.data.sign = t, r.data.flags = i, r.data.ccode = n, debugConsole && console.log("证书签名—————" + JSON.stringify(r));
                var c = (r.id, getEnStr(this.pgeRZRandNum, r)), o = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: c };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(o) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回证书签名————" + JSON.stringify(e) + "————code:" + code), s = e;
                    }, error: function (e, t, i) {
                        debugConsole && console.log(i);
                    } }), s;
            }, this.setPin = function (e, t, i) {
                var n, s = { interfacetype: 0, data: {} };
                s.id = this.sid, s.data.pid = e, s.data.pin = t, s.data.ccode = i;
                var r = (s.id, getEnStr(this.pgeRZRandNum, s)), c = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: r };
                return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(c) }, success: function (e) {
                        e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回设置pin————" + JSON.stringify(e) + "————code:" + code), n = e;
                    } }), n;
            }, this.trimRight = function (e) {
                if (null == e)
                    return "";
                var t = new String(" 	\n\r"), i = new String(e);
                if (-1 != t.indexOf(i.charAt(i.length - 1))) {
                    for (var n = i.length - 1; n >= 0 && -1 != t.indexOf(i.charAt(n));)
                        n--;
                    i = i.substring(0, n + 1);
                }
                return i.toString();
            };
        }
        return { restrict: "A", template: '<li ng-if="showSecurity" ng-hide="shortShow"><label>安全认证方式：</label><div class="tool-select" ng-show="selectSms" ng-click="securityToolSMS()"><input type="radio" ng-model="$parent.securityToolId" custom-input name="security_type" value="1" /><span>短信验证</span></div><div class="tool-select" ng-show="selectUk" ng-click="securityToolUSB()"><input type="radio" ng-model="$parent.securityToolId" custom-input name="security_type" value="2" /><span>UK验证</span></div><div class="tool-select" ng-show="selectSmsAndUk"><input type="radio" ng-model="$parent.securityToolId" custom-input name="security_type" value="3" /><span>UK和短信验证</span></div><div class="tool-select" ng-show="selectPaypwd"><input type="radio" ng-model="$parent.securityToolId" custom-input name="security_type" value="4" /><span>电子支付密码</span></div></li><li ng-if="securityToolId==1 || securityToolId==3"><label>短信验证码：</label><input type="text" name="smsCode" ng-model="$parent.securitySmsCode" ng-disabled="inputDisabled" class="input-text" maxlength="6" placeholder="{{smsPlaceHolder}}" validator="required|dypassword"/><p class="message-tip"><span class="message-tip-span" ng-if="smsResultMsg!=\'\'">{{smsResultMsg}}，{{secondValue}}s后可</span><a class="message-tip-a" ng-click="getSMCode()" ng-class="{\'a-disabled\':btnDisabled}"><span class="message-tip-a-add" ng-if="smsTextStatus==1">{{smsBtnTextAdd}}</span><span class="message-tip-a-text" ng-class="{\'message-tip-a-black\':smsTextStatus==1,\'message-tip-a-red\':smsTextStatus==2}">{{smsBtnText}}</span></a></p></li><li ng-if="securityToolId==4"><label>电子支付密码：</label><input type="password" name="securityPayPwd" ng-model="$parent.securityPayPwd" maxlength="6" validator="require|password"/></li><div class="message-tip" ng-if="showDownloadSignCtrl"><div>尊敬的客户：您好！您正在网厅6.0，需安装全新签名控件。</div><div>第一步：<a href="{{downloadUrl}}" class="fb" target="_blank">点此下载控件</a></div><div>第二步：<a ng-click="refreshWindow()">点击此处</a>刷新当前页面</div></div><div class="message-tip" ng-if="(securityToolId == \'2\' || securityToolId == \'3\') && showOneKeySet">尊敬的客户：系统没有正确读取证书，请确认您的USB-KEY是否插好。下载并运行<a href=\'../../assets/nbcbEdit/NingBoBankBuddy.exe\' target=\'_blank\'>宁波银行网银向导</a>点击一键设置。</div><div class="message-tip" ng-if="(securityToolId == \'2\' || securityToolId == \'3\') && showCertActive">尊敬的客户：您好！您的证书尚未激活，请下载并运行<a href=\'../../assets/nbcbEdit/NingBoBankBuddy.exe\' target=\'_blank\'>宁波银行网银向导</a>，然后前往“宁波银行网上银行-证书管理-证书激活”根据提示输入激活码进行证书激活。</div><div class="message-tip" ng-if="(securityToolId == \'2\' || securityToolId == \'3\') && showCertDownload">尊敬的客户：您好！您的证书尚未下载，请下载并运行<a href=\'../../assets/nbcbEdit/NingBoBankBuddy.exe\' target=\'_blank\'>宁波银行网银向导</a>，然后前往“宁波银行网上银行-证书管理-证书下载”根据提示操作进行证书下载。</div><div class="message-tip error-msg" ng-if="(securityToolId == \'2\' || securityToolId == \'3\') && errorMsg!=\'\'">{{errorMsg}}</div>', scope: { onInit: "&" }, link: function (e, t) {
                function i(i) {
                    h = i.BSNCodeType, f = i.accountNo, v = i.signedOriginalMsg, p = i.subBSN ? i.subBSN : "0";
                    var n = $globalData.getSessionContext("session_voucher"), r = $globalData.getSessionContext("security");
                    if ("328" != n && "343" != n && "360" != n || "4" != r || $httpPlus.post("get2ndSignSrData.do", { actionName: h, subBSN: p, signedOriginalMsg: v }).success(function (t) {
                        "0000" == t.ec ? y = '<?xml version="1.0" encoding="utf-8"?><T><D><M><k>交易信息：</k><v>' + t.cd.signData2ndUK + "</v></M></D><E><M><k>交易数据</k><v>" + v + "</v></M></E></T>" : e.errorMsg = "页面初始化失败，请稍后再试！";
                    }), s({ BSNCodeType: i.BSNCodeType, payAccount: i.accountNo, fieldAmount: i.transferAmt, currencyType: i.currencyType, recAccount: i.recAccountNo }), m = new Messenger("MW_CSP_" + (new Date).getTime()), m.env.edge)
                        $httpPlus.post("getEdgeRandomForSign.do", {}).success(function (i) {
                            if ("0000" == i.ec) {
                                var n = i.cd.pgeRZRand, s = i.cd.pgeRZData, r = m.createTag(n, s);
                                t.append($(r)), e.showSecurity = !0;
                            }
                        });
                    else {
                        var c = m.createTag();
                        t.append($(c)), e.showSecurity = !0;
                    }
                    $httpPlus.post("getRandomSign.do", { actionName: h }).success(function (e) {
                        "0000" == e.ec && (b = e.cd.securityRandomCode);
                    });
                }
                function n(t) {
                    var i, n = {}, s = $globalData.getSessionContext("session_voucher"), r = $globalData.getSessionContext("security");
                    if (!ifSign)
                        return n;
                    if (n.signedOriginalMsg = t, n.subBSN = p, n.securityToolId = e.securityToolId, "" == n.securityToolId)
                        return void (e.errorMsg = "请选择您的安全认证方式。");
                    var c = $globalData.getSessionContext("certID");
                    if ("1" == n.securityToolId)
                        n.password = e.securitySmsCode;
                    else if ("2" == n.securityToolId || "3" == n.securityToolId) {
                        if ("2" == n.securityToolId && /(iphone|ipod|ipad|ios|android|windows phone)/.test(window.navigator.userAgent.toLowerCase()))
                            return void (e.errorMsg = "您的终端不支持UK验证，请选择短信验证。");
                        if (i = g(), 1 != i)
                            return void l();
                        if (!u())
                            return;
                        try  {
                            var o;
                            if ("4" == r && "328" != s && "343" != s && "360" != s)
                                o = m.sign("CN = " + c, t + b);
                            else {
                                if (!y)
                                    return;
                                o = m.sign("CN = " + c, y);
                            }
                            if ("object" == typeof o || "function" == typeof o)
                                return void (e.errorMsg = "您已取消该操作！");
                            if (0 == o)
                                return void (e.showOneKeySet = !0);
                            var a = m.getcert("CN = " + c);
                            n.certInfo = a, n.signedlMsg = o, n.sign2ndData = y;
                        } catch (d) {
                            return void (e.errorMsg = "您的控件下载不成功，无法进行签名操作。请至 宁波银行网上银行-安全中心-软件下载 菜单，下载安装签名控件；或者请参照个人网上银行USBKey使用说明书进行设置。");
                        }
                    } else
                        "4" == n.securityToolId && (n.password = e.securityPayPwd, n.passwordType = "1");
                    return n.repeatRandom = k, n;
                }
                function s(t) {
                    $httpPlus.post("securityCheck.do", t).success(function (t) {
                        if ("0000" == t.ec) {
                            var i = t.cd.securityTool;
                            k = t.cd.repeatRandom, r(i);
                        } else
                            e.errorMsg = "初始化您的安全认证方式失败，请重新加载。";
                    });
                }
                function r(t) {
                    switch (t) {
                        case "900":
                        case "400":
                        case "300":
                        case "600":
                            e.selectSms = !0, e.securityToolId = "1";
                            break;
                        case "800":
                            e.btnDisabled = !0, e.selectSms = !0, e.securityToolId = "1", $httpPlus.post("securityVerificationSMS.do", null).success(function (t) {
                                if ("0000" == t.ec) {
                                    var i = t.cd.mobileVerify;
                                    "1" != i ? e.errorMsg = "请前往宁波银行网上银行进行短信认证。" : e.btnDisabled = !1;
                                }
                            }), c("");
                            break;
                        case "580":
                            e.selectSms = !0, e.selectPaypwd = !0, e.securityToolId = "4", e.securityToolSMS = function () {
                                c("4");
                            };
                            break;
                        case "200":
                        case "270":
                            e.selectUk = !0, e.securityToolId = "2";
                            break;
                        case "100":
                        case "170":
                            e.selectSmsAndUk = !0, e.securityToolId = "3";
                            break;
                        case "180":
                            e.selectSms = !0, e.selectSmsAndUk = !0, e.securityToolId = "3", e.securityToolSMS = function () {
                                o("3");
                            };
                            break;
                        case "230":
                            e.selectSms = !0, e.selectUk = !0, e.securityToolId = "2";
                            break;
                        case "320":
                            e.selectSms = !0, e.selectUk = !0, e.securityToolId = "1";
                            break;
                        case "310":
                            e.selectSms = !0, e.selectSmsAndUk = !0, e.securityToolId = "1";
                            break;
                        case "280":
                            e.selectSms = !0, e.selectUk = !0, e.securityToolId = "2", e.securityToolSMS = function () {
                                o("2");
                            };
                    }
                    e.selectSms && (a(t), ("1" == e.securityToolId || "3" == e.securityToolId) && e.getSMCode());
                }
                function c(t) {
                    var i = $globalData.getSessionContext("mobileNo");
                    "" == i ? (e.errorMsg = "手机号为空，请至柜台维护或申请专业版网银", e.securityToolId = t) : $httpPlus.post("securityVerificationSMS.do", null).success(function (t) {
                        if ("0000" == t.ec) {
                            var i = t.cd.mobileVerify;
                            "1" != i && (e.errorMsg = "请前往宁波银行网上银行进行短信认证。");
                        }
                    });
                }
                function o(t) {
                    var i = $utb.nvl($globalData.getSessionContext("session_secMobile"), "");
                    "" == i ? (e.errorMsg = "手机号为空，请至柜台维护", e.securityToolId = t) : $httpPlus.post("checkuKeyIsSign.do", null).success(function (t) {
                        if ("0000" == t.ec) {
                            var i = t.cd.signFlag;
                            "0" == i && (e.errorMsg = "请前往宁波银行网上银行进行短信签约。");
                        }
                    });
                }
                function a(t) {
                    var i = $utb.nvl($globalData.getSessionContext("mobileNo"), ""), n = "", s = { signedOriginalMsg: v, actionName: h, subBSN: p };
                    switch (t) {
                        case "900":
                            n = "sendMessageMobileCode.do";
                            break;
                        case "400":
                            n = "sendMessageMobileCode.do";
                            break;
                        case "300":
                            n = "sendMessageCode.do";
                            break;
                        case "600":
                            n = "sendCardMessageCode.do", s = { cardNo: f };
                            break;
                        default:
                            n = "sendMessageuKeyCode.do";
                    }
                    e.getSMCode = function () {
                        if (!e.btnDisabled) {
                            if ("" == i && ("900" == t || "400" == t))
                                return void (e.errorMsg = "手机号为空，请至柜台维护或申请专业版网银");
                            e.securitySmsCode = "", e.btnDisabled = !0, e.smsTextStatus = 2, e.smsBtnText = "短信发送中...", $httpPlus.post(n, s).success(function (t) {
                                if ("0000" == t.ec) {
                                    var i = t.cd.sequence;
                                    "0000" == i ? (d("验证码发送失败", 10), e.errorMsg = t.cd.errorMessage) : ($log.debug(t.cd.mobileMsg), e.errorMsg = "", e.smsPlaceHolder = "交易序号" + i, e.inputDisabled = !1, d("验证码已发送", 60));
                                } else
                                    e.errorMsg = t.em, d("验证码发送失败", 10);
                            });
                        }
                    };
                }
                function d(t, i) {
                    e.smsResultMsg = t, e.smsTextStatus = 2, e.smsBtnText = "重发验证码", e.secondValue = i;
                    var n = $interval(function () {
                        e.secondValue--, 0 == e.secondValue && ($interval.cancel(n), e.btnDisabled = !1);
                    }, 1e3);
                }
                function l() {
                    isSupport = !0, m.env.edge ? m.env.isWindows ? e.downloadUrl = "./../../assets/nbcbEdit/nbcbEditEdge.exe" : m.env.isMac ? e.downloadUrl = "./../../assets/nbcbEdit/nbcbEditEdge.pkg" : isSupport = !1 : m.env.isWindows ? e.downloadUrl = "./../../assets/nbcbEdit/nbcbEdit.exe" : m.env.isMac ? e.downloadUrl = "./../../assets/nbcbEdit/nbcbEdit.pkg" : isSupport = !1, isSupport ? e.showDownloadSignCtrl = !0 : e.errorMsg = "对不起，本行安全控件暂不支持您当前访问的终端，如有疑问，请联系我行";
                }
                function u() {
                    var t = $globalData.getSessionContext("certID"), i = $globalData.getSessionContext("security"), n = $globalData.getSessionContext("session_voucher");
                    return "4" != i && "5" != i && "6" != i ? !0 : "329" != n && "343" != n && "360" != n && "328" != n || t && "nbcb" == t.substring(0, 4) ? t ? !0 : (e.showCertDownload = !0, !1) : (e.showCertActive = !0, !1);
                }
                function g() {
                    var e = "";
                    if (e = m.getVersion())
                        return 1;
                    try  {
                         {
                            new ActiveXObject("SIGNX.SignXCtrl.1");
                        }
                        return 2;
                    } catch (t) {
                        return 3;
                    }
                }
                var h, f, p, v, b, y, k, m = null;
                ifSign = !0, e.shortShow = t[0] && t[0].attributes && void 0 != t[0].attributes.isShortHide ? t[0].attributes.isShortHide : !1, e.showSecurity = !1, e.errorMsg = "", e.timestamp = "TS_" + $globalData.getTimestamp(), e.showDownloadSignCtrl = !1, e.showCertDownload = !1, e.showCertActive = !1, e.showOneKeySet = !1, e.downloadUrl = "", e.securityToolId = "", e.selectSms = !1, e.selectUk = !1, e.selectSmsAndUk = !1, e.selectPaypwd = !1, e.showSMS = function () {
                    return "1" == e.securityToolId || "3" == e.securityToolId;
                }, e.securityToolSMS = function () {
                    e.getSMCode();
                }, e.securityToolUSB = function () {
                }, e.smsPlaceHolder = "", e.smsTextStatus = 1, e.smsBtnTextAdd = "点击", e.smsBtnText = "发送短信验证码", e.smsResultMsg = "", e.secondValue = -1, e.securitySmsCode = "", e.btnDisabled = !1, e.inputDisabled = !0, e.refreshWindow = function () {
                    window.location.reload();
                };
                var C = { initToolParams: i, getSecurityParams: n };
                e.onInit({ $operator: C });
            } };
    }]);
;
angular.module("validator", []).directive("validator", ["$parse", function ($parse) {
        var VALIDATOR_PATTERN = { required: { test: function (r) {
                    return !!r;
                } }, r_required: { test: function () {
                    return !0;
                } }, email: /^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/, phone: /^((\(\d{3}\))|(\d{3}\-))?(\(0\d{2,3}\)|0\d{2,3}-)?[1-9]\d{6,7}$/, currency: /^\d+(\.\d+)?$/, number: /^\d+$/, integer: /^[-\+]?\d+$/, doubleNum: /^[-\+]?\d+(\.\d+)?$/, english: /^[A-Za-z]+$/, money: /^((\d{1,3}(\,\d{3})*)|\d+)?(\.(\d+)?)?$/, chinese: /^[\u4e00-\u9fa5]{2,8}$/, chineseOrChar: /^([A-Za-z\u4e00-\u9fa5]){1,8}$/, password: /^\d{6}$/, charOrNum6_20: /[a-zA-Z0-9]{6,20}/, charOrNum: /^[a-zA-Z0-9]{0,30}$/, charAndNum: /^[a-zA-z]+[0-9]+|[0-9]+[a-zA-z]+$/, date: /^(1[89]\d\d|2\d\d\d)-(1[0-2]|0\d)-([0-2]\d|3[01])$/, dypassword: /^\d{4}(\d{2})?$/, verifyImage: /[a-zA-Z0-9]{4}$/, name: /^[\u4E00-\u9FA5(\d)(a-z)(A-Z)]{1,20}$/, mobile: /^13[0-9]{9}$|^14[0-9]{9}$|^15[0-9]{9}$|^16[0-9]{9}$|^17[0-9]{9}$|^18[0-9]{9}$|^19[0-9]{9}$/, specialChar: { test: function (r) {
                    return new RegExp("<[\\s\\x00]*SCRIPT|SELECT\\s|INSERT\\s|DELETE\\s|UPDATE\\s|DROP\\s|<!--|-->|<FRAME|<IFRAME|<FRAMESET|<NOFRAME|<PLAINTEXT|<A\\s|<LINK|<MAP|<BGSOUND|<IMG|<FORM|<INPUT|<SELECT|<OPTION|<TEXTAREA|<APPLET|<OBJECT|<EMBED|<NOSCRIPT|<STYLE|ALERT[\\s\\x00]*\\(|<|>").test(r);
                } }, idCard: { test: function (r) {
                    if (!r || 18 != r.length)
                        return !1;
                    for (var e = r.substr(0, 17), a = 0; a < e.length; a++)
                        if (e.charAt(a) < "0" || e.charAt(a) > "9")
                            return !1;
                    if (parseInt(r.substr(6, 4), 10) < 1900 || parseInt(r.substr(6, 4), 10) > 2100)
                        return !1;
                    if (parseInt(r.substr(10, 2), 10) > 12 || parseInt(r.substr(10, 2), 10) < 1)
                        return !1;
                    if (parseInt(r.substr(12, 2), 10) > 31 || parseInt(r.substr(12, 2), 10) < 1)
                        return !1;
                    var t = new Array(7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2, 1), n = new Array("1", "0", "X", "9", "8", "7", "6", "5", "4", "3", "2");
                    "x" == r.charAt(17) && (r = r.replace("x", "X"));
                    for (var s = r.charAt(17), u = 0, d = 0; d < r.length - 1; d++)
                        u += r.charAt(d) * t[d];
                    var i = u % 11, o = n[i];
                    return s != o ? !1 : !0;
                } }, isDate: { test: function (value) {
                    if (null == value || "" == value)
                        return !1;
                    var pz1 = /^\d{4}\d{2}\d{2}$/;
                    if (!pz1.test(value))
                        return !1;
                    var d = value.replace(/-/g, ""), sYear = d.substring(0, 4), sMonth = d.substring(4, 6), sDay = d.substring(6, 8);
                    if (eval(sMonth) < 1 || eval(sMonth) > 12)
                        return !1;
                    var leapYear = sYear % 4 == 0 && sYear % 100 != 0 || sYear % 400 == 0 ? !0 : !1, monthDay = new Array(12);
                    return monthDay[0] = 31, monthDay[1] = leapYear ? 29 : 28, monthDay[2] = 31, monthDay[3] = 30, monthDay[4] = 31, monthDay[5] = 30, monthDay[6] = 31, monthDay[7] = 31, monthDay[8] = 30, monthDay[9] = 31, monthDay[10] = 30, monthDay[11] = 31, eval(sDay) < 1 || eval(sDay) > monthDay[eval(sMonth) - 1] ? !1 : !0;
                } } };
        return { require: "ngModel", link: function (r, e, a, t) {
                function n(e, a) {
                    var t, n, s, u = [e];
                    if (a)
                        for (s = a.split(",") || [], n = s.length, t = 0; n > t; t++)
                            u.push($parse(s[t])(r));
                    return u;
                }
                function s(e) {
                    for (var s = a.validator.split("|"), u = !0, d = 0; d < s.length; d++) {
                        var i, o, l, h = s[d];
                        o = "", 0 == h.indexOf("@") ? (h = h.substring(1), i = h.split(":"), h = i[0], o = i[1], l = r[h]) : (l = VALIDATOR_PATTERN[h], /^r_/.test(h) && (h = h.replace(/r_/, ""))), angular.isDefined(l) && (u ? (u = u && l.test.apply(l, n(e, o)), t.$setValidity(h, u)) : t.$setValidity(h, !0));
                    }
                    return e;
                }
                t.$parsers.push(s), t.$formatters.push(s);
            } };
    }]);
;
angular.module("verifycode", []).directive("verifycode", ["$log", "$appConfig", "$globalData", function (e, r, i) {
        return { restrict: "A", scope: { verifyOperator: "=?" }, link: function (e, t, a) {
                function c() {
                    var e = r.serverPath + "VerifyImage?flag=" + i.getTimestamp();
                    "advance" == a.verifycode && (e += "&gr=1"), t[0].src = e;
                }
                t[0].title = "看不清？点击更换图片", c(), t.click(function () {
                    c();
                }), e.verifyOperator = { refresh: c };
            } };
    }]);
;
angular.module("waiting", []).directive("waiting", ["$rootScope", "$appConfig", "$sniffer", function (i, o, t) {
        return { restrict: "EAC", scope: { caption: "=", isShow: "=" }, template: '<div ng-show="(!!isShow)"><div class="pop-backdrop"><iframe frameborder="0" style="width:100%;height:100%;"></iframe></div><div class="pop-waiting clearfix"><p class="pop-waiting-icon"></p><!--<p class="pop-waiting-title">{{caption}}</p> --></div></div>', link: function (n, e) {
                if (t.msie <= 8)
                    try  {
                        var c = e.find("iframe");
                        c[0].src = "javascript:document.write('<script>document.domain=\"" + o.domain + "\"</script>')";
                        var a = c[0].contentDocument ? c[0].contentDocument : c[0].contentWindow ? c[0].contentWindow.document : c[0].document;
                        a && (a.open(), a.write('<html><head><script>document.domain="' + o.domain + "\"</script></head><body style='background-color:#666'></body></html>"), a.close());
                    } catch (d) {
                    }
                i.$on("$locationChangeSuccess", function (i, o, t) {
                    o != t && (n.isShow = !1);
                }), n.$watch("isShow", function (i) {
                    i ? (e.removeClass("ng-Hide"), e.show()) : (e.addClass("ng-Hide"), e.hide());
                });
            } };
    }]);
;
angular.module("cardFilter", []).filter("card", function () {
    return function (r) {
        return r.replace(/([a-z0-9]{4})(?=([a-z0-9]))/g, "$1 ");
    };
}).filter("uncard", function () {
    return function (r) {
        return r.replace(/\s+/g, "");
    };
});
;
angular.module("filters", ["cardFilter", "moneyFilter", "maskFilter"]);
;
angular.module("maskFilter", []).filter("certmask", function () {
    return function (n) {
        return n ? (15 == n.length ? n = n.substring(0, 6) + "******" + n.substring(12) : 18 == n.length && (n = n.substring(0, 6) + "******" + n.substring(14)), n) : n;
    };
}).filter("mobilemask", function () {
    return function (n) {
        return n && n.length > 7 ? n.substring(0, 3) + "****" + n.substring(7) : n;
    };
}).filter("accountmask", function () {
    return function (n) {
        if (!n)
            return n;
        var r = n.length, t = "";
        if (r >= 6) {
            for (var u = 0; r - 6 > u; u++)
                t += "*";
            n = n.substring(0, 3) + t + n.substring(r - 3);
        }
        return n;
    };
});
;
angular.module("moneyFilter", []).filter("money", function () {
    return function (e, r) {
        return e && /^[-]?((\d{1,3}(\,\d{3})*)|\d+)?(\.(\d+)?)?$/.test(e) ? parseFloat(e).toFixed(r || 2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,") : e;
    };
}).filter("unmoney", function () {
    return function (e) {
        return e.replace(/[,]/g, "");
    };
}).filter("toChinese", function () {
    var e = /^(\+|-)?(\d+)(\.\d+)?$/;
    return function (r) {
        if (!e.test(r))
            return "";
        var t = "", n = "仟佰拾亿仟佰拾万仟佰拾元角分", a = "零壹贰叁肆伍陆柒捌玖";
        r = parseFloat(r).toFixed(2) + "00";
        var l = r.indexOf(".");
        l >= 0 && (r = r.substring(0, l) + r.substr(l + 1, 2));
        var u = r.length;
        n = n.substr(n.length - u);
        for (var c = 0; u > c; c++)
            t += a.substr(r.substr(c, 1), 1) + n.substr(c, 1);
        return t.replace(/零角零分$/, "整").replace(/零[仟佰拾]/g, "零").replace(/零{2,}/g, "零").replace(/零([亿|万])/g, "$1").replace(/亿[万]/g, "亿").replace(/零+元/g, "元").replace(/亿零{0,3}/g, "亿").replace(/^元/g, "零元").replace(/零元/g, "").replace(/零角/g, "").replace(/零分/g, "");
    };
});
;
angular.module("appConfig", []).constant("$appConfig", { isDebug: !1, requestLocal: !1, cache: !0, serverPath: "/perbank/", channel: "NH", passwordCtrl: !0, domain: "nbcb.com.cn", mybankUrl: "/nebank/" });
;
angular.module("cookie", []).factory("$cookie", ["$rootScope", "$cacheFactory", function () {
        function e(e) {
            return n.raw ? e : decodeURIComponent(e.replace(t, " "));
        }
        function r(r) {
            0 === r.indexOf('"') && (r = r.slice(1, -1).replace(/\\"/g, '"').replace(/\\\\/g, "\\")), r = e(r);
            try  {
                return n.json ? JSON.parse(r) : r;
            } catch (o) {
            }
        }
        function n(o, t, i) {
            if (void 0 !== t) {
                if (i = angular.extend({}, n.defaults, i), "number" == typeof i.expires) {
                    var a = i.expires, c = i.expires = new Date;
                    c.setDate(c.getDate() + a);
                }
                return t = n.json ? JSON.stringify(t) : String(t), document.cookie = [n.raw ? o : encodeURIComponent(o), "=", n.raw ? t : encodeURIComponent(t), i.expires ? "; expires=" + i.expires.toUTCString() : "", i.path ? "; path=" + i.path : "", i.domain ? "; domain=" + i.domain : "", i.secure ? "; secure" : ""].join("");
            }
            for (var p = document.cookie.split("; "), s = o ? void 0 : {}, u = 0, d = p.length; d > u; u++) {
                var f = p[u].split("="), l = e(f.shift()), g = f.join("=");
                if (o && o === l) {
                    s = r(g);
                    break;
                }
                o || (s[l] = r(g));
            }
            return s;
        }
        function o(e, r) {
            return void 0 !== n(e) ? (n(e, "", $.extend({}, r, { expires: -1 })), !0) : !1;
        }
        var t = /\+/g;
        return n.defaults = {}, { set: n, get: n, remove: o };
    }]);
;
angular.module("loginFlow", []).service("$loginFlow", ["$globalData", "$httpPlus", "$appConfig", "$rootScope", "$interval", "$routeHelper", "$routeParams", "$message", "$location", function (e, s, o, n, t, i, a, l) {
        function c(o, n, i, a) {
            a = a || {}, e.initSessionContext({ sessionId: o }), m("login.pre");
            var c = 0, p = null, f = null, h = function () {
                if (c++, 2 == c) {
                    var s = r(p.cd, f.cd), o = function () {
                        n && n(p);
                    };
                    if (e.initSessionContext(s), a.pre) {
                        var i = { interval: {} }, u = function (e, s) {
                            t.cancel(i.interval), s && s.jumpSucess && "0" == s.jumpSucess ? a.pre(a.result, whiteCallback, o) : d();
                        };
                        a.isPop ? l.jump("app/uploadIdImg/loginSafeCheck.tpl.html", { shortObject: i }, { onClose: u, className: "setNewHeight", entityClassName: "" }) : l.pop({ src: "app/uploadIdImg/loginSafeCheck.tpl.html", title: "安全验证", params: { shortObject: i }, onClose: u, className: "setNewHeight" });
                    } else
                        whiteCallback(o);
                }
            };
            whiteCallback = function (e) {
                u(p, e), g(), m("login.post");
            }, s.post("HBgetSessionData.do").success(function (e) {
                "0000" == e.ec ? (p = e, h()) : i && i(e);
            }), s.post("hallbank_loadRoleApp.do").success(function (e) {
                "0000" == e.ec ? (f = e, h()) : i && i(e);
            });
        }
        function r(e, s) {
            var o = {};
            for (var n in v)
                o[n] = e[n], n in s && (o[n] = s[n]);
            return o;
        }
        function u(e, s) {
            var o = e.cd.session_ImgSwitch, n = e.cd.session_uploadIdentity;
            "1" == o ? "1" == n || "4" == n ? l.pop({ src: "app/uploadIdImg/uploadIdentityNotice.tpl.html", title: "系统提示", params: {}, onClose: function () {
                    d(), i.jump("/main");
                }, className: "setBLpage" }) : "2" == n ? l.pop({ src: "app/uploadIdImg/uploadImgNotice.tpl.html", title: "系统提示", params: {}, onClose: function () {
                    d(), i.jump("/main");
                }, className: "setBLpage" }) : s && s() : s && s();
        }
        function p(e, s, o, n) {
            var t, i, a = -1;
            n = n || 10, angular.isUndefined(o) && (o = s, s = void 0), t = y[e] ? y[e] : y[e] = [], i = t.length;
            for (var l = 0; i > l; l++)
                if (parseInt(n, 10) < parseInt(t[l].prior, 10)) {
                    a = l;
                    break;
                }
            return -1 == a && (a = a.length), t.splice(a, 0, { type: e, prior: n, data: s, fn: o }), this;
        }
        function m(e) {
            var s, o;
            s = y[e] || [], o = s.length, angular.forEach(s, function (e) {
                e.fn && e.fn(e.data);
            });
        }
        function d(t) {
            t ? jQuery.ajax({ method: "post", url: o.serverPath + "HBlogout.do", async: !1, data: { EMP_SID: e.getSessionContext("sessionId"), WEB_CHN: o.channel } }) : (m("quit.pre"), s.post("HBlogout.do").success(function () {
                m("quit.post");
            })), I(), n.__userImg = "";
        }
        function g() {
            h(), B = t(function () {
                var s = e.getOperationTime(), o = (new Date).getTime(), n = 6e5, t = 0;
                o - s > n - t && f();
            }, 3e4);
        }
        function f() {
            i.jump("/sessionTimeout");
        }
        function h() {
            B && t.cancel(B);
        }
        function I() {
            h(), e.initSessionContext({});
        }
        function _(s, n, t, i) {
            var a;
            t !== !1 && (a = i || e.getSessionContext("sessionId"));
            var l = { login: "PB00001", EXT00008: "EXT00008", PB00600: "PB00600", PB04720: "PB04720", PB06500: "PB06500", EXT00027: "EXT00027" }, c = "", r = 0, n = n || {}, s = s ? l[s] : "";
            a && (window.top.name = "NBCB@" + a), s && !n.start && (n.start = s), "login" != n.start && "PB00001" != n.start || !a || (n.start = "");
            var u = e.getAppContext("initNBCBRootParams") || {};
            if (c += u.isHallbankSecurityNotice > 0 ? "isHallbankSecurityNotice=1" : "isHallbankSecurityNotice=0", n)
                for (key in n)
                    c += "&", c += key + "=" + encodeURIComponent(n[key]), r++;
            c = c ? "?" + c : "", window.onunload = null, window.location.href = o.mybankUrl + c;
        }
        function C(e) {
            var s = "", o = { EXT00025: "/HB00101_buyConfirm" };
            e = e || {}, e.start && (s = o[e.start]) ? i.jump(s, e) : i.jump("/perbank", {});
        }
        function N(e, s) {
            e = e || {}, e.start ? _("", e, !0, s) : _("", {}, !0, s);
        }
        var v = { iAccountInfo: "客户下挂账号列表（不包含电子账户）", iEAccountInfo: "客户的电子账户", security: "安全手段", certType: "证件类型", creditCertType: "信用卡证件类型", certNo: "证件号码", session_language: "当前语种", session_customerNameCN: "客户姓名", session_customerId: "网银客户ID", openNode: "客户开户网点", openNodeFlag: "开户网点判断", session_whiteCollarCust: "白领通标志", session_isWhiteCollar: "新理财白领通标志", customerType: "客户类型", fundAccountNo: "基金账号", mobileNo: "手机号", session_activeFlag: "电子账户激活标志", session_eAccFlag: "电子账户下挂标志", accountNo: "账号", session_moneyCust: "惠财银行标志", session_custLevel: "客户等级", session_userRemoteIP: "客户IP地址", session_secMobile: "客户安全认证手机号", certID: "证书ID", certSeq: "证书序号", customerLastLogon: "客户上次登录时间(信用卡版客户)", lastLoginTime: "客户上次登录时间(非信用卡版客户)", customerSex: "客户性别", sessionId: "会话ID", iUserSettingsInfoCopy: "用户设置", session_stylePath: "用户风格", session_voucher: "UKey标志", session_ZXloginType: "直销银行客户类型", session_countryNumber: "国别", session_isNewCustomer: "是否新客户", session_isXinFuBao: "是否薪福宝客户", session_switchToVersion: "1表示4.0跳转到5.0;0表示5.0跳转到4.0;直接登录5.0,4.0 为空", transChnFlag: "转账渠道开闭标志(10位) 每一位： 0关闭，  1开放，  2异常", session_highNetWorld: "是否高净值客户", session_HNWEndDate: "高净值截止时间", oldIpAddr: "上次登录IP", session_version: "网银登录版本 0 4.0登录 1 5.0登录", session_isDiamond: "是否钻石客户N-不是；c-是", session_ImgSwitch: "影像平台上传状态", session_uploadIdentity: "影像平台开关标识", session_isPrivateBank: "私银客户标识" }, y = {}, B = null;
        return { on: p, login: c, quit: d, clearSessionContext: I, switchToMybank: _, loginDispatch: C, ebankDispatch: N };
    }]);
;
!function ($) {
    function Messenger(id) {
        this.voucher = null, this.sdkversion = "1.0.0.6", this.collectionCNindexOne = "NBCB", this.collectionCNindexTwo = "041@";
        var urls = "https://windows10.microdone.cn:5426";
        this.sid = "sign" + (new Date).getTime() + 1, this.pgeRZRandNum = "", this.pgeRZDataB = "";
        var pid, debugConsole = !1, ua = navigator.userAgent.toLowerCase(), chr = "", regStr_chrome = /chrome\/[\d.]+/gi;
        if (ua.indexOf("chrome") > 0) {
            var chromeVersion = ua.match(regStr_chrome).toString();
            chromeVersion = parseInt(chromeVersion.replace(/[^0-9.]/gi, "")), chr = chromeVersion >= 42 ? "chromeh" : "chromel";
        }
        this.env = { isWindows: -1 != ua.indexOf("windows") || -1 != ua.indexOf("win32") || -1 != ua.indexOf("win64"), isMac: -1 != ua.indexOf("macintosh") || -1 != ua.indexOf("mac os x"), isLinux: -1 != ua.indexOf("linux"), ie: -1 != ua.indexOf("msie") || -1 != ua.indexOf("trident"), firefox: -1 != ua.indexOf("firefox"), chrome: -1 != chr.indexOf("chromel"), opera: -1 != ua.indexOf("opera"), safari: -1 != ua.indexOf("version"), edge: -1 != ua.indexOf("edge") || -1 != chr.indexOf("chromeh") }, this.unserialize = function (e) {
            if ("string" == typeof e) {
                for (var t = {}, i = e.split("&"), n = 0; n < i.length; n++) {
                    var c = i[n].split("=");
                    2 == c.length && (t[c[0]] = decodeURIComponent(c[1]));
                }
                return t;
            }
        }, this.createTag = function (e, t) {
            return this.env.isWindows && this.env.ie ? "<object id='" + id + "' classid='CLSID:EC176F0A-69BE-4059-8546-B01EF8C0FB9C' width='0' height='0'></object>" : this.env.edge ? (this.pgeRZRandNum = e, void (this.pgeRZDataB = t)) : this.env.isWindows || this.env.isMac ? "<embed id='" + id + "' type='application/x-sign-messenger' width='0' height='0'>" : "";
        }, this.high = null, this.admin = null, this.getObj = function (e) {
            var t = $("#" + id)[0];
            return void 0 != e && this.env.ie ? e ? (null == this.admin && (this.admin = t.getAdmin(e)), this.admin) : (null == this.high && (this.high = t.getAdmin(e)), this.high) : t;
        }, this.DNConvert = function (e) {
            var t = e.replace(/\s=/g, "=");
            return t = t.replace(/=\s/g, "="), t = t.replace(/=/g, ':"'), t = t.replace(/cn:/g, "CN:"), t = "{" + t.replace(/,/g, '",') + '"}', -1 != t.toUpperCase().indexOf("SN:") && (t = t.toUpperCase(), t = t.replace(/SN/g, "sn"), t = t.replace(/\s/g, ""), t = t.replace(/"(0+)/g, '"')), t;
        }, this.getVersion = function () {
            return this.env.ie ? this.getObj().version : navigator.plugins.SignMessenger.description;
        };
        var keychain, certificate, collection, evaldn, data, token;
        this.sign = function (dn, content) {
            if ("" == dn || "" == content)
                return !1;
            try  {
                if (this.env.isMac) {
                    var token1, token2, token3;
                    if (token1 = this.getObj(!1).token("libnbcb_personal"), token2 = this.getObj(!1).token("libshuttle_p11v220.1.0.0_nb"), token3 = this.getObj(!1).token("libI3KP11_Ningbo_nb"), null == token1 && null == token2 && null == token3)
                        return !1;
                    if (null != token1 && token1.keyCount())
                        this.getObj(!1).codepage = -1, token1.selectKey(0), token1.certificateCount() && (certificate = token1.getActiveCertificate()), data = certificate.sign(content, parseInt(32));
                    else if (null != token2 && token2.keyCount()) {
                        if (this.getObj(!1).codepage = -1, token2.selectKey(0), !token2.certificateCount())
                            return !1;
                        certificate = token2.getActiveCertificate(), data = certificate.sign(content, parseInt(32));
                    } else {
                        if (null == token3 || !token3.keyCount())
                            return !1;
                        if (this.getObj(!1).codepage = 65001, token3.selectKey(0), !token3.certificateCount())
                            return !1;
                        certificate = token3.getActiveCertificate(), data = certificate.sign(content, parseInt(544));
                    }
                    return data ? data : {};
                }
                keychain = this.getObj(!1).keychain();
                var dn_new = this.DNConvert(dn);
                if (dn_new = eval("(" + dn_new + ")"), collection = keychain.query(dn_new), "" != this.collectionCNindexOne && "" != this.collectionCNindexTwo && collection.length > 1)
                    for (i = 0; i < collection.length;)
                        -1 == collection.item(i).CN.toUpperCase().indexOf(this.collectionCNindexOne) && -1 == collection.item(i).CN.toUpperCase().indexOf(this.collectionCNindexTwo) ? collection.remove(i) : i++;
                if ("328" != this.voucher && "343" != this.voucher && "360" != this.voucher) {
                    var o = USBKeyTool.getObj(!1);
                    if (o.codepage = -1, collection.length > 1) {
                        certificate = collection.userChoose(), certificate.setPin("");
                        var data = certificate.sign(content, 36);
                        return certificate.setPin(""), data;
                    }
                    certificate = collection.item(0), certificate.setPin("");
                    var data = certificate.sign(content, 36);
                    return certificate.setPin(""), data;
                }
                var o = USBKeyTool.getObj(!1);
                if (o.codepage = 65001, collection.length > 1) {
                    certificate = collection.userChoose(), certificate.setPin("");
                    var data = certificate.sign(content, 544);
                    return certificate.setPin(""), data;
                }
                certificate = collection.item(0), certificate.setPin("");
                var data = certificate.sign(content, 544);
                return certificate.setPin(""), data;
            } catch (err) {
                return !1;
            }
        }, this.getcert = function (dn) {
            if ("" == dn)
                return !1;
            try  {
                if (this.env.isMac) {
                    var token1, token2, token3;
                    if (token1 = this.getObj(!1).token("libnbcb_personal"), token2 = this.getObj(!1).token("libshuttle_p11v220.1.0.0_nb"), token3 = this.getObj(!1).token("libI3KP11_Ningbo_nb"), null == token1 && null == token2 && null == token3)
                        return !1;
                    if (null != token1 && token1.keyCount()) {
                        if (this.getObj(!1).codepage = -1, token1.selectKey(0), token1.certificateCount() && (certificate = token1.getActiveCertificate(), null == certificate))
                            return !1;
                    } else if (null != token2 && token2.keyCount()) {
                        if (this.getObj(!1).codepage = -1, token2.selectKey(0), !token2.certificateCount())
                            return !1;
                        if (certificate = token2.getActiveCertificate(), null == certificate)
                            return !1;
                    } else if (null != token3 && token3.keyCount()) {
                        if (this.getObj(!1).codepage = 65001, token3.selectKey(0), !token3.certificateCount())
                            return !1;
                        if (certificate = token3.getActiveCertificate(), null == certificate)
                            return !1;
                    }
                    return certificate.toString();
                }
                keychain = this.getObj(!1).keychain();
                var dn_new = this.DNConvert(dn);
                if (dn_new = eval("(" + dn_new + ")"), collection = keychain.query(dn_new), "328" != this.voucher && "343" != this.voucher && "360" != this.voucher) {
                    var o = this.getObj(!1);
                    o.codepage = -1;
                } else {
                    var o = this.getObj(!1);
                    o.codepage = 65001;
                }
                if ("" != this.collectionCNindexOne && "" != this.collectionCNindexTwo && collection.length > 1)
                    for (i = 0; i < collection.length;)
                        -1 == collection.item(i).CN.toUpperCase().indexOf(this.collectionCNindexOne) && -1 == collection.item(i).CN.toUpperCase().indexOf(this.collectionCNindexTwo) ? collection.remove(i) : i++;
                return collection.length > 1 ? (certificate = collection.userChoose(), certificate.toString()) : (certificate = collection.item(0), certificate.toString());
            } catch (err) {
                return !1;
            }
        }, this.certInfo = function (dn) {
            if ("" == dn)
                return !1;
            try  {
                if (this.env.edge)
                    return this.certInfoEdge(dn);
                if (this.env.isMac) {
                    var token1, token2, token3;
                    if (token1 = this.getObj(!1).token("libnbcb_personal"), token2 = this.getObj(!1).token("libshuttle_p11v220.1.0.0_nb"), token3 = this.getObj(!1).token("libI3KP11_Ningbo_nb"), null == token1 && null == token2 && null == token3)
                        return !1;
                    if (null != token1 && token1.keyCount()) {
                        if (this.getObj(!1).codepage = -1, token1.selectKey(0), token1.certificateCount() && (certificate = token1.getActiveCertificate(), null == certificate))
                            return !1;
                    } else if (null != token2 && token2.keyCount()) {
                        if (this.getObj(!1).codepage = -1, token2.selectKey(0), !token2.certificateCount())
                            return !1;
                        if (certificate = token2.getActiveCertificate(), null == certificate)
                            return !1;
                    } else if (null != token3 && token3.keyCount()) {
                        if (this.getObj(!1).codepage = 65001, token3.selectKey(0), !token3.certificateCount())
                            return !1;
                        if (certificate = token3.getActiveCertificate(), null == certificate)
                            return !1;
                    }
                    return certificate.CN ? certificate.CN : certificate.subject(new Object).CN;
                }
                keychain = this.getObj(!1).keychain();
                var dn_new = this.DNConvert(dn);
                if (dn_new = eval("(" + dn_new + ")"), collection = keychain.query(dn_new), "328" != this.voucher && "343" != this.voucher && "360" != this.voucher) {
                    var o = this.getObj(!1);
                    o.codepage = -1;
                } else {
                    var o = this.getObj(!1);
                    o.codepage = 65001;
                }
                if ("" != this.collectionCNindexOne && "" != this.collectionCNindexTwo && collection.length > 1)
                    for (i = 0; i < collection.length;)
                        -1 == collection.item(i).subject(new Object).CN.toUpperCase().indexOf(this.collectionCNindexOne) && -1 == collection.item(i).subject(new Object).CN.toUpperCase().indexOf(this.collectionCNindexTwo) ? collection.remove(i) : i++;
                return collection.length > 1 ? (certificate = collection.userChoose(), certificate.subject(new Object).CN) : 1 == collection.length ? (certificate = collection.item(0), certificate.subject(new Object).CN) : this.getGuomiCN(dn);
            } catch (err) {
                return !1;
            }
        }, this.certInfoEdge = function (e) {
            return "" == e ? !1 : this.env.isMac ? this.getMacEdgeCN(e) : this.getWindowsEdgeCN(e);
        }, this.getMacEdgeCN = function (dn) {
            var provider1 = this.getProvider("libnbcb_personal", 1, 0, -1), provider2 = this.getProvider("libshuttle_p11v220.1.0.0_nb", 1, 0, -1), provider3 = this.getProvider("libI3KP11_Ningbo_nb", 1, 0, 65001), keyCN = null;
            if (null == keyCN) {
                var pid = provider1.pid;
                if (-1 != pid) {
                    var keycount = this.keyCount(pid, 7).keycount;
                    if (keycount > 0) {
                        this.selectKey(pid, 0, 8);
                        var cn = this.DNConvert(dn);
                        cn = eval("(" + cn + ")");
                        var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                        if (certnumber > 0) {
                            certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                            var cert = this.getCertificateinfo(pid, 0);
                            keyCN = cert.certinfo.cn, this.checkIsNbcbCN(keyCN) || (keyCN = null);
                        }
                    }
                }
            }
            if (null == keyCN) {
                var pid = provider2.pid;
                if (-1 != pid) {
                    var keycount = this.keyCount(pid, 7).keycount;
                    if (keycount > 0) {
                        this.selectKey(pid, 0, 8);
                        var cn = this.DNConvert(dn);
                        cn = eval("(" + cn + ")");
                        var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                        if (certnumber > 0) {
                            certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                            var cert = this.getCertificateinfo(pid, 0);
                            keyCN = cert.certinfo.cn, this.checkIsNbcbCN(keyCN) || (keyCN = null);
                        }
                    }
                }
            }
            if (null == keyCN) {
                var pid = provider3.pid;
                if (-1 != pid) {
                    var keycount = this.keyCount(pid, 7).keycount;
                    if (keycount > 0) {
                        this.selectKey(pid, 0, 8);
                        var cn = this.DNConvert(dn);
                        cn = eval("(" + cn + ")");
                        var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                        if (certnumber > 0) {
                            certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                            var cert = this.getCertificateinfo(pid, 0);
                            keyCN = cert.certinfo.cn, this.checkIsNbcbCN(keyCN) || (keyCN = null);
                        }
                    }
                }
            }
            return null == keyCN ? !1 : keyCN;
        }, this.getWindowsEdgeCN = function (dn) {
            var provider1 = this.getProvider("", 0, 0, -1), provider2 = this.getProvider("nbcb", 0, 0, -1), keyCN = null;
            if (null == keyCN) {
                var pid = provider1.pid, cn = this.DNConvert(dn);
                cn = eval("(" + cn + ")");
                var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                if (certnumber > 0) {
                    certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                    var cert = this.getCertificateinfo(pid, 0);
                    keyCN = cert.certinfo.cn, this.checkIsNbcbCN(keyCN) || (keyCN = null);
                }
            }
            if (null == keyCN) {
                var pid = provider2.pid, cn = this.DNConvert(dn);
                cn = eval("(" + cn + ")");
                var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                if (certnumber > 0) {
                    certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                    var cert = this.getCertificateinfo(pid, 0);
                    keyCN = cert.certinfo.cn, this.checkIsNbcbCN(keyCN) || (keyCN = null);
                }
            }
            return null == keyCN ? !1 : keyCN;
        };
        var code;
        this.getProvider = function (e, t, i, n) {
            var c, r = { interfacetype: 0, data: {} };
            r.id = this.sid, r.data.name = e, r.data.type = t, r.data.pcode = i, r.data.codepage = n, debugConsole && console.log("获取provider————" + JSON.stringify(r));
            var o = (r.id, getEnStr(this.pgeRZRandNum, r)), s = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: o };
            return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(s) }, success: function (e) {
                    e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回provider————" + JSON.stringify(e) + "————code:" + code + "————x.pid:" + e.pid), c = e;
                } }), c;
        }, this.getCsr = function (e, t, i) {
            var n, c = { interfacetype: 0, data: {} };
            c.id = this.sid, c.data.pid = e, c.data.pcode = t, c.data.csrinfo = i;
            var r = (c.id, getEnStr(this.pgeRZRandNum, c)), o = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: r };
            return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(o) }, success: function (e) {
                    e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回csr————" + JSON.stringify(e) + "————code:" + code + "————x.csr:" + e.csr), n = e;
                } }), n;
        }, this.getcertEdge = function (dn) {
            if ("" == dn)
                return !1;
            if (!this.env.isMac) {
                var provider = null;
                provider = "328" == this.voucher ? this.getProvider("nbcb", 0, 0, -1) : this.getProvider("", 0, 0, -1), pid = provider.pid;
                var cn = this.DNConvert(dn);
                cn = eval("(" + cn + ")");
                var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                if (certnumber > 0) {
                    certlist = certs.certlist, debugConsole && console.log(certlist), certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                    var cert = this.getCertificateinfo(pid, 0);
                    return cert = cert.certinfo.pub;
                }
                return !1;
            }
            var provider;
            if (provider = "328" == this.voucher || "343" == this.voucher ? this.getProvider("libI3KP11_Ningbo_nb", 1, 0, 65001) : "329" == this.voucher ? this.getProvider("libshuttle_p11v220.1.0.0_nb", 1, 0, -1) : this.getProvider("libnbcb_personal", 1, 0, -1), code = provider.code, pid = provider.pid, 0 != code && 6 != code)
                return !1;
            var keyCount = this.keyCount(pid, 7);
            if (keycount = keyCount.keycount, keycount > 0) {
                this.selectKey(pid, 0, 8);
                var cn = this.DNConvert(dn);
                cn = eval("(" + cn + ")");
                var certs = this.getCertificateList(pid, 5, cn), certnumber = certs.certnumber;
                if (certnumber > 0) {
                    certlist = certs.certlist, certid = certlist[0].certid, this.selectCertificate(pid, certid, 6, 1);
                    var cert = this.getCertificateinfo(pid, 0);
                    return cert = cert.certinfo.pub;
                }
                return !1;
            }
        }, this.getCertificateList = function (e, t, i) {
            var n, c = { interfacetype: 0, data: {} };
            c.id = this.sid, c.data.pid = e, c.data.pcode = t, c.data.query = i, this.env.isMac && (c.data.query = { C: "CN" }), debugConsole && console.log("获取证书列表————" + JSON.stringify(c));
            var r = (c.id, getEnStr(this.pgeRZRandNum, c)), o = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: r };
            return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(o) }, success: function (e) {
                    e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回证书列表————" + JSON.stringify(e) + "————code:" + code), n = e;
                } }), n;
        }, this.selectCertificate = function (e, t, i, n) {
            var c, r = { interfacetype: 0, data: {} };
            r.id = this.sid, r.data.pid = e, r.data.certid = t, r.data.pcode = i, r.data.type = n, debugConsole && console.log("获取选择证书————" + JSON.stringify(r));
            var o = (r.id, getEnStr(this.pgeRZRandNum, r)), s = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: o };
            return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(s) }, success: function (e) {
                    e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回选择证书————" + JSON.stringify(e) + "————code:" + code), c = e;
                } }), c;
        }, this.keyCount = function (e, t) {
            var i, n = { interfacetype: 0, data: {} };
            n.id = this.sid, n.data.pid = e, n.data.pcode = t;
            var c = (n.id, getEnStr(this.pgeRZRandNum, n)), r = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: c };
            return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(r) }, success: function (e) {
                    e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回key个数————" + JSON.stringify(e) + "————code:" + code + "————keycount" + e.keycount), i = e;
                } }), i;
        }, this.selectKey = function (e, t, i) {
            var n, c = { interfacetype: 0, data: {} };
            c.id = this.sid, c.data.pid = e, c.data.keyindex = t, c.data.pcode = i;
            var r = (c.id, getEnStr(this.pgeRZRandNum, c)), o = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: r };
            return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(o) }, success: function (e) {
                    e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回选择key————" + JSON.stringify(e) + "————code:" + code), n = e;
                } }), n;
        }, this.getCertificateinfo = function (e, t) {
            var i, n = { interfacetype: 0, data: {} };
            n.id = this.sid, n.data.pid = e, n.data.ccode = t;
            var c = (n.id, getEnStr(this.pgeRZRandNum, n)), r = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: c };
            return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(r) }, success: function (e) {
                    e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, cn = e.certinfo.cn, certinfo = e.certinfo, debugConsole && console.log("返回证书信息————" + JSON.stringify(e) + "————code:" + code), i = e;
                } }), i;
        }, this.signature = function (e, t, i, n) {
            var c, r = { interfacetype: 0, data: {} };
            r.id = this.sid, r.data.pid = e, r.data.sign = t, r.data.flags = i, r.data.ccode = n, debugConsole && console.log("证书签名—————" + JSON.stringify(r));
            var o = (r.id, getEnStr(this.pgeRZRandNum, r)), s = { rankey: this.pgeRZRandNum, datab: this.pgeRZDataB, datac: o };
            return jQuery.ajax({ timeout: 1e3, type: "GET", async: !1, url: urls, data: { jsoncallback: "jsoncallback", str: JSON.stringify(s) }, success: function (e) {
                    e = e.substring(13, e.length - 1), e = JSON.parse(e), code = e.code, debugConsole && console.log("返回证书签名————" + JSON.stringify(e) + "————code:" + code), c = e;
                }, error: function (e, t, i) {
                    alert("err"), debugConsole && console.log(i);
                } }), c;
        }, this.trimRight = function (e) {
            if (null == e)
                return "";
            var t = new String(" 	\n\r"), i = new String(e);
            if (-1 != t.indexOf(i.charAt(i.length - 1))) {
                for (var n = i.length - 1; n >= 0 && -1 != t.indexOf(i.charAt(n));)
                    n--;
                i = i.substring(0, n + 1);
            }
            return i.toString();
        }, this.checkIsNbcbCN = function (e) {
            return -1 == e.toUpperCase().indexOf(this.collectionCNindexOne) && -1 == e.toUpperCase().indexOf(this.collectionCNindexTwo) ? !1 : !0;
        }, this.getGuomiCN = function (dn) {
            var keychain = this.getObj(!1).keychain("nbcb"), dn_new = this.DNConvert(dn);
            if (dn_new = eval("(" + dn_new + ")"), collection = keychain.query(dn_new), "" != this.collectionCNindexOne && "" != this.collectionCNindexTwo && collection.length > 1)
                for (i = 0; i < collection.length;)
                    -1 == collection.item(i).subject(new Object).CN.toUpperCase().indexOf(this.collectionCNindexOne) && -1 == collection.item(i).subject(new Object).CN.toUpperCase().indexOf(this.collectionCNindexTwo) ? collection.remove(i) : i++;
            return collection.length > 1 ? (certificate = collection.userChoose(), certificate.subject(new Object).CN) : (certificate = collection.item(0), certificate.subject(new Object).CN);
        };
    }
    window.Messenger = Messenger;
}(jQuery);
;
angular.module("globalData", []).factory("$globalData", ["$cacheFactory", function (e) {
        function n() {
            return (new Date).getTime() + "_" + O++;
        }
        function t(e) {
            return e = e || 6, Math.random((new Date).getTime()).toString(36).slice(2, e + 2);
        }
        function i(e) {
            c = e;
        }
        function r(e) {
            return angular.isDefined(e) ? c[e] : c;
        }
        function a(e, n) {
            angular.isDefined(n) && (c[e] = n);
        }
        function g() {
            return l;
        }
        function o() {
            return m;
        }
        function s(e, n) {
            h.put(e, n);
        }
        function f(e) {
            return h.get(e);
        }
        function u(e) {
            e && (p = parseInt(e, 10));
        }
        function d() {
            return new Date(p);
        }
        function v(e) {
            e && (S = parseInt(e, 10));
        }
        function x() {
            return S;
        }
        function A() {
            var e, n = /chrome\/[\d.]+/gi;
            if ("Win32" == navigator.platform || "Windows" == navigator.platform)
                if (navigator.userAgent.indexOf("MSIE") > 0 || navigator.userAgent.indexOf("msie") > 0 || navigator.userAgent.indexOf("Trident") > 0 || navigator.userAgent.indexOf("trident") > 0)
                    e = navigator.userAgent.indexOf("ARM") > 0 ? 9 : 1;
                else if (navigator.userAgent.indexOf("Edge") > 0)
                    e = 10;
                else if (navigator.userAgent.indexOf("Chrome") > 0) {
                    var t = navigator.userAgent.match(n).toString();
                    t = parseInt(t.replace(/[^0-9.]/gi, "")), e = t >= 42 ? 10 : 2;
                } else
                    e = 2;
            else if ("Win64" == navigator.platform)
                if (navigator.userAgent.indexOf("Windows NT 6.2") > 0 || navigator.userAgent.indexOf("windows nt 6.2") > 0)
                    e = 1;
                else if (navigator.userAgent.indexOf("MSIE") > 0 || navigator.userAgent.indexOf("msie") > 0 || navigator.userAgent.indexOf("Trident") > 0 || navigator.userAgent.indexOf("trident") > 0)
                    e = 3;
                else if (navigator.userAgent.indexOf("Edge") > 0)
                    e = 10;
                else if (navigator.userAgent.indexOf("Chrome") > 0) {
                    var t = navigator.userAgent.match(n).toString();
                    t = parseInt(t.replace(/[^0-9.]/gi, "")), e = t >= 42 ? 10 : 2;
                } else
                    e = 2;
            else if (navigator.userAgent.indexOf("Linux") > 0)
                e = navigator.userAgent.indexOf("_64") > 0 ? 4 : 5, navigator.userAgent.indexOf("Android") > 0 && (e = 7);
            else if (navigator.userAgent.indexOf("Macintosh") > 0)
                if (navigator.userAgent.indexOf("Safari") > 0 && (navigator.userAgent.indexOf("Version/5.1") > 0 || navigator.userAgent.indexOf("Version/5.2") > 0 || navigator.userAgent.indexOf("Version/6") > 0))
                    e = 8;
                else if (navigator.userAgent.indexOf("Firefox") > 0 || navigator.userAgent.indexOf("Chrome") > 0) {
                    var t = navigator.userAgent.match(n);
                    null != t ? (t = t.toString(), t = parseInt(t.replace(/[^0-9.]/gi, "")), e = t >= 42 ? 11 : 6) : e = 6;
                } else
                    e = navigator.userAgent.indexOf("Opera") >= 0 && (navigator.userAgent.indexOf("Version/11.6") > 0 || navigator.userAgent.indexOf("Version/11.7") > 0) ? 6 : navigator.userAgent.indexOf("Safari") >= 0 ? 6 : 0;
            return e;
        }
        var O = 0, c = {}, l = {}, m = {}, p = (new Date).getTime(), S = (new Date).getTime(), h = e("applicationContext");
        return { getTimestamp: n, getRandomStr: t, initSessionContext: i, getSessionContext: r, setSessionContext: a, getContextParams: g, getBranchInfos: o, setAppContext: s, getAppContext: f, setSystemTime: u, getSystemTime: d, setOperationTime: v, getOperationTime: x, checkOsBrowser: A };
    }]);
;
angular.module("httpPlus", []).config(["$httpProvider", function (t) {
        t.defaults.headers.post["Content-Type"] = "application/x-www-form-urlencoded;charset=utf-8", t.responseInterceptors.push("httpInterceptor");
    }]).factory("$httpPlus", ["$http", "$globalData", "$appConfig", "$log", function (t, e, n, a) {
        function r(t, e, n) {
            var a, r = [];
            n = n || {};
            for (var o in t)
                a = t[o], a = null == a ? "" : a, r.push(n.encode !== !1 ? encodeURIComponent(o) + "=" + encodeURIComponent(a) : o + "=" + a);
            return r.join("&");
        }
        function o(a, o, c) {
            var i, s;
            return n.requestLocal || !n.cache ? u(a, o, c) : (c = c || {}, o = angular.extend({}, o) || {}, o.NOUSERVAR = "0", o.WEB_CHN = n.channel, i = { submitTimestamp: e.getTimestamp() }, angular.isDefined(c) && c.needLogin && (i.EMP_SID = e.getSessionContext("sessionId")), s = encodeURIComponent("?" + r(o, null, { encode: !1 })), a = n.serverPath + (c.cacheUrl ? c.cacheUrl : "cache") + "/" + a + s, configObj = { params: i }, angular.extend(configObj, c), t.get(a, configObj));
        }
        function c(t, e, n) {
            return n = n || {}, n.cacheUrl = "cache2", o(t, e, n);
        }
        function u(o, c, u) {
            n.requestLocal ? o = "../response/" + o.substring(o.lastIndexOf("/")) + ".json" : "/" != o.charAt(0) && (o = n.serverPath + o), c = angular.extend({}, c) || {}, c.WEB_CHN = n.channel, c.submitTimestamp = e.getTimestamp(), c.EMP_SID = e.getSessionContext("sessionId");
            var i = { transformRequest: r };
            return angular.extend(i, u), a.debug("[$httpPlus post " + o + " 提交数据]:"), a.debug(c), t.post(o, c, i);
        }
        function i(t, a, r) {
            var o = r ? r.data || {} : {}, c = $("#" + t), u = "/" == r.url.charAt(0) ? r.url : n.serverPath + (r.url || "");
            o.EMP_SID = e.getSessionContext("sessionId"), o.WEB_CHN = n.channel, o.submitTimestamp = e.getTimestamp(), c.attr("method", "post"), c.attr("enctype", "multipart/form-data"), c.attr("action", u), $.each(o, function (t, e) {
                var n = c.find("#" + t);
                if (n.length <= 0) {
                    var a = $('<input type="hidden" name="' + t + '" id="' + t + '">');
                    a.val(e), c.append(a);
                }
            });
            var i = {};
            return i.JContainer = document, s(t, a, i);
        }
        function s(t, e, n) {
            var a = (n ? n.JContainer : null, {});
            n = $.extend({}, a, n);
            var r = (new Date).getTime(), o = $('<iframe name="' + r + '" id="' + r + '" style="display:none;"></iframe>'), c = null;
            c = $("string" == typeof t ? "#" + t : t), c.attr("target", r), c.attr("method", "post"), c.attr("enctype", "multipart/form-data"), o.bind("load", function () {
                var t = $(window.frames[r].document.body), n = $(t).text();
                if ("" != n && e)
                    try  {
                        e(JSON.parse(n));
                    } catch (a) {
                    }
            }), $(document.body).append(o);
        }
        return { post: u, getCache: o, getCache2: c, transformData: r, fileUpload: i };
    }]).factory("httpInterceptor", ["$log", "$q", "$globalData", "$location", "$routeHelper", function (t, e, n, a, r) {
        return function (o) {
            return o.then(function (o) {
                return t.debug("[httpInterceptor " + o.config.url + " 返回报文]:"), t.debug(o), "EBLN1000" == o.data.ec ? (t.debug("会话超时"), a.path("/sessionTimeout"), e.reject(o)) : "EBPB0134" == o.data.ec ? (t.debug("系统错误，无访问权限"), r.defaultErrorShow(o.data.ec, "无访问权限"), e.reject(o)) : (n.setSystemTime(o.data.st), o);
            }, function (n) {
                return t.error(n), e.reject(n);
            });
        };
    }]);
;
angular.module("iss", []).factory("$iss", ["$rootScope", "$cacheFactory", "$browser", "$routeParams", "$routeParams", "$routeHelper", "$httpPlus", function (e, r, t, n, n, s, o) {
        function u() {
            d(), o.post("/bank/getHttpSessionId?token=" + strToken).success(function (e) {
                "0000" == e.ec && d({ jSessionId: e.cd.httpSessionId });
            }), e.$on("$routeChangeSuccess", function (e, r, t) {
                var n, o = (t || {}).$$route || {}, u = (r || {}).$$route || {}, d = o.regexp || { test: function () {
                        return !1;
                    } }, a = u.regexp || { test: function () {
                        return !1;
                    } }, f = s.getParamsCache() || {}, g = f._source ? { test: function (e) {
                        return e == f._source;
                    } } : d, v = a, _ = o.originalPath || "", l = u.originalPath || "";
                (n = c(g, v)) && i(n.event), _ && p.push(["_track", "#" + _, 1]), l && p.push(["_track", "#" + l, 0]);
            });
        }
        function c(e, r) {
            var t, n, s = g, o = s.length;
            for (t = 0; o > t; t++)
                if (n = s[t], e && e.test(n.source) && r && r.test(n.route))
                    return n;
            return null;
        }
        function i(e) {
            var r = [].concat(e);
            r.unshift("_event"), p.push(r);
        }
        function d(e) {
            e = e || {}, f.BFD_USER = f.BFD_USER || {}, e.userId && (f.BFD_USER.user_id = e.userId), !e.userId && (f.BFD_USER.user_id = "0"), e.jSessionId && (f.BFD_USER.user_cookie = e.jSessionId);
        }
        function a() {
            var e = [].slice.apply(arguments);
            i(e);
        }
        var f = window._BFD || {}, p = window._iss || [], g = [{ desc: "主导航-预约", source: "TJ001", route: "/appoint/", event: ["order", "order", "order"] }, { desc: "主导航-我的", source: "TJ001", route: "/perbank/", event: ["mine", "mine", "mine"] }, { desc: "主导航-登录", source: "TJ001", route: "", event: ["login", "login", "login"] }, { desc: "主导航-注册", source: "TJ001", route: "/HB00900/", event: ["register", "register", "register"] }, { desc: "二级导航-理财产品", source: "TJ002", route: "/HB00101/", event: ["finance", "finance", "finance"] }, { desc: "二级导航-基金产品", source: "TJ002", route: "/HB01104/", event: ["fund", "fund", "fund"] }, { desc: "二级导航-直销产品", source: "TJ002", route: "/HB00501/", event: ["direct", "direct", "direct"] }, { desc: "二级导航-定期存款", source: "TJ002", route: "/HB00301/", event: ["deposit", "deposit", "deposit"] }];
        return u(), { setUser: d, addEvent: a };
    }]);
;
angular.module("message", []).factory("$message", ["$rootScope", "$cacheFactory", function (t, n) {
        function e(n) {
            var e, a, r = { type: O }, s = null, i = angular.extend(r, n || {});
            i.src ? i.type = S : i.src = i.content, i.type == S && (e = "POP_HISTORY_" + (new Date).getTime(), a = [], C.put(e, a)), s = function () {
                t[h] = { type: "", isShow: !1, entityClassName: "" }, a = [], e && C.remove(e), i.onClose && i.onClose.apply(window, arguments);
            }, o({ title: i.title, type: i.type, className: i.className, entityClassName: i.entityClassName, params: i.params, src: i.src, isShow: !0, buttons: i.buttons, onClose: s, onNotify: i.onNotify, cacheKey: e }), i.type == S && a.push(t[h]);
        }
        function o(n, e) {
            var o = t[h];
            o && (o.backParams = null), t[h] = n, n.backParams = e;
        }
        function a() {
            return t[h] ? t[h].onClose : null;
        }
        function r(t, n) {
            var o = { type: O, title: "系统提示", buttons: [{ title: "关闭" }] };
            o.content = t, o.onClose = n, e(o);
        }
        function s(t, n, o) {
            var a = { type: O, title: "系统提示", buttons: [{ title: o }] };
            a.content = t, a.onClose = n, e(a);
        }
        function i(t, n, o, a) {
            var r = { type: O, title: "系统提示", buttons: [{ title: "确认", signature: "0" }, { title: "取消", signature: "1" }] };
            angular.extend(r, a), r.content = t, r.onClose = function (t, e) {
                t == w.HEADCLOSE ? o && o() : t == w.FOOTCLOSE && ("0" == e.signature ? n && n(e) : o && o(e));
            }, e(r);
        }
        function c(n) {
            var e = t[h] || {}, o = e.params || {};
            return void 0 == n ? o : o[n];
        }
        function l(n, e, r) {
            var s, i, c, l, u, p = { src: n, params: e };
            if (p = angular.extend(p, r), l = a(), r && r.onClose && angular.isFunction(r.onClose)) {
                u = r.onClose;
                var f = function (t, n) {
                    u(t, n), l(t, n);
                };
                p.onClose = f;
            }
            s = t[h], i = s.cacheKey, c = C.get(i), o(angular.extend({}, t[h], p)), c.push(t[h]), _hmt.push(["_trackPageview", n]);
        }
        function u(n) {
             {
                var e = t[h] || {}, a = e.cacheKey, r = C.get(a);
                r.pop();
            }
            r.length > 0 && o(r[r.length - 1], n);
        }
        function p(n) {
            var e = t[h] || {}, o = e.backParams;
            return void 0 == n ? o : (o || {})[n];
        }
        function f(n) {
            var e = t[h] || {}, o = e.onClose;
            t[h] = { type: "", isShow: !1 }, o && o("2", n);
        }
        function m(n) {
            var e = t[h] || {}, o = e.onNotify;
            o && o(n);
        }
        function g(t, n) {
            l("app/error/pop-error.tpl.html", { ec: t, em: n });
        }
        function y(n) {
            var e = function () {
                t[v] = { isShow: !1 };
            };
            return t[v] = { title: n || "数据加载中，请稍候...", isShow: !0 }, { close: e };
        }
        var C = n("pop-Cache"), h = "g_popObject", v = "g_waitingObject", O = "0", S = "1", w = { HEADCLOSE: "0", BODYCLOSE: "2", FOOTCLOSE: "1" };
        return { pop: e, alert: r, alertBtn: s, confirm: i, getParams: c, getBackParams: p, jump: l, back: u, close: f, notify: m, waiting: y, defaultErrorShow: g };
    }]);
;
!function () {
    var Y;
    angular.module("pinyin", []).factory("$pinyin", [function () {
            return new Y;
        }]), Y = function (Y) {
        var Z = { checkPolyphone: !1, charcase: "default" };
        this.options = $.extend({}, Z, Y), this.char_dict = "YDYQSXMWZSSXJBYMGCCZQPSSQBYCDSCDQLDYLYBSSJGYZZJJFKCCLZDHWDWZJLJPFYYNWJJTMYHZWZHFLZPPQHGSCYYYNJQYXXGJHHSDSJNKKTMOMLCRXYPSNQSECCQZGGLLYJLMYZZSECYKYYHQWJSSGGYXYZYJWWKDJHYCHMYXJTLXJYQBYXZLDWRDJRWYSRLDZJPCBZJJBRCFTLECZSTZFXXZHTRQHYBDLYCZSSYMMRFMYQZPWWJJYFCRWFDFZQPYDDWYXKYJAWJFFXYPSFTZYHHYZYSWCJYXSCLCXXWZZXNBGNNXBXLZSZSBSGPYSYZDHMDZBQBZCWDZZYYTZHBTSYYBZGNTNXQYWQSKBPHHLXGYBFMJEBJHHGQTJCYSXSTKZHLYCKGLYSMZXYALMELDCCXGZYRJXSDLTYZCQKCNNJWHJTZZCQLJSTSTBNXBTYXCEQXGKWJYFLZQLYHYXSPSFXLMPBYSXXXYDJCZYLLLSJXFHJXPJBTFFYABYXBHZZBJYZLWLCZGGBTSSMDTJZXPTHYQTGLJSCQFZKJZJQNLZWLSLHDZBWJNCJZYZSQQYCQYRZCJJWYBRTWPYFTWEXCSKDZCTBZHYZZYYJXZCFFZZMJYXXSDZZOTTBZLQWFCKSZSXFYRLNYJMBDTHJXSQQCCSBXYYTSYFBXDZTGBCNSLCYZZPSAZYZZSCJCSHZQYDXLBPJLLMQXTYDZXSQJTZPXLCGLQTZWJBHCTSYJSFXYEJJTLBGXSXJMYJQQPFZASYJNTYDJXKJCDJSZCBARTDCLYJQMWNQNCLLLKBYBZZSYHQQLTWLCCXTXLLZNTYLNEWYZYXCZXXGRKRMTCNDNJTSYYSSDQDGHSDBJGHRWRQLYBGLXHLGTGXBQJDZPYJSJYJCTMRNYMGRZJCZGJMZMGXMPRYXKJNYMSGMZJYMKMFXMLDTGFBHCJHKYLPFMDXLQJJSMTQGZSJLQDLDGJYCALCMZCSDJLLNXDJFFFFJCZFMZFFPFKHKGDPSXKTACJDHHZDDCRRCFQYJKQCCWJDXHWJLYLLZGCFCQDSMLZPBJJPLSBCJGGDCKKDEZSQCCKJGCGKDJTJDLZYCXKLQSCGJCLTFPCQCZGWPJDQYZJJBYJHSJDZWGFSJGZKQCCZLLPSPKJGQJHZZLJPLGJGJJTHJJYJZCZMLZLYQBGJWMLJKXZDZNJQSYZMLJLLJKYWXMKJLHSKJGBMCLYYMKXJQLBMLLKMDXXKWYXYSLMLPSJQQJQXYXFJTJDXMXXLLCXQBSYJBGWYMBGGBCYXPJYGPEPFGDJGBHBNSQJYZJKJKHXQFGQZKFHYGKHDKLLSDJQXPQYKYBNQSXQNSZSWHBSXWHXWBZZXDMNSJBSBKBBZKLYLXGWXDRWYQZMYWSJQLCJXXJXKJEQXSCYETLZHLYYYSDZPAQYZCMTLSHTZCFYZYXYLJSDCJQAGYSLCQLYYYSHMRQQKLDXZSCSSSYDYCJYSFSJBFRSSZQSBXXPXJYSDRCKGJLGDKZJZBDKTCSYQPYHSTCLDJDHMXMCGXYZHJDDTMHLTXZXYLYMOHYJCLTYFBQQXPFBDFHHTKSQHZYYWCNXXCRWHOWGYJLEGWDQCWGFJYCSNTMYTOLBYGWQWESJPWNMLRYDZSZTXYQPZGCWXHNGPYXSHMYQJXZTDPPBFYHZHTJYFDZWKGKZBLDNTSXHQEEGZZYLZMMZYJZGXZXKHKSTXNXXWYLYAPSTHXDWHZYMPXAGKYDXBHNHXKDPJNMYHYLPMGOCSLNZHKXXLPZZLBMLSFBHHGYGYYGGBHSCYAQTYWLXTZQCEZYDQDQMMHTKLLSZHLSJZWFYHQSWSCWLQAZYNYTLSXTHAZNKZZSZZLAXXZWWCTGQQTDDYZTCCHYQZFLXPSLZYGPZSZNGLNDQTBDLXGTCTAJDKYWNSYZLJHHZZCWNYYZYWMHYCHHYXHJKZWSXHZYXLYSKQYSPSLYZWMYPPKBYGLKZHTYXAXQSYSHXASMCHKDSCRSWJPWXSGZJLWWSCHSJHSQNHCSEGNDAQTBAALZZMSSTDQJCJKTSCJAXPLGGXHHGXXZCXPDMMHLDGTYBYSJMXHMRCPXXJZCKZXSHMLQXXTTHXWZFKHCCZDYTCJYXQHLXDHYPJQXYLSYYDZOZJNYXQEZYSQYAYXWYPDGXDDXSPPYZNDLTWRHXYDXZZJHTCXMCZLHPYYYYMHZLLHNXMYLLLMDCPPXHMXDKYCYRDLTXJCHHZZXZLCCLYLNZSHZJZZLNNRLWHYQSNJHXYNTTTKYJPYCHHYEGKCTTWLGQRLGGTGTYGYHPYHYLQYQGCWYQKPYYYTTTTLHYHLLTYTTSPLKYZXGZWGPYDSSZZDQXSKCQNMJJZZBXYQMJRTFFBTKHZKBXLJJKDXJTLBWFZPPTKQTZTGPDGNTPJYFALQMKGXBDCLZFHZCLLLLADPMXDJHLCCLGYHDZFGYDDGCYYFGYDXKSSEBDHYKDKDKHNAXXYBPBYYHXZQGAFFQYJXDMLJCSQZLLPCHBSXGJYNDYBYQSPZWJLZKSDDTACTBXZDYZYPJZQSJNKKTKNJDJGYYPGTLFYQKASDNTCYHBLWDZHBBYDWJRYGKZYHEYYFJMSDTYFZJJHGCXPLXHLDWXXJKYTCYKSSSMTWCTTQZLPBSZDZWZXGZAGYKTYWXLHLSPBCLLOQMMZSSLCMBJCSZZKYDCZJGQQDSMCYTZQQLWZQZXSSFPTTFQMDDZDSHDTDWFHTDYZJYQJQKYPBDJYYXTLJHDRQXXXHAYDHRJLKLYTWHLLRLLRCXYLBWSRSZZSYMKZZHHKYHXKSMDSYDYCJPBZBSQLFCXXXNXKXWYWSDZYQOGGQMMYHCDZTTFJYYBGSTTTYBYKJDHKYXBELHTYPJQNFXFDYKZHQKZBYJTZBXHFDXKDASWTAWAJLDYJSFHBLDNNTNQJTJNCHXFJSRFWHZFMDRYJYJWZPDJKZYJYMPCYZNYNXFBYTFYFWYGDBNZZZDNYTXZEMMQBSQEHXFZMBMFLZZSRXYMJGSXWZJSPRYDJSJGXHJJGLJJYNZZJXHGXKYMLPYYYCXYTWQZSWHWLYRJLPXSLSXMFSWWKLCTNXNYNPSJSZHDZEPTXMYYWXYYSYWLXJQZQXZDCLEEELMCPJPCLWBXSQHFWWTFFJTNQJHJQDXHWLBYZNFJLALKYYJLDXHHYCSTYYWNRJYXYWTRMDRQHWQCMFJDYZMHMYYXJWMYZQZXTLMRSPWWCHAQBXYGZYPXYYRRCLMPYMGKSJSZYSRMYJSNXTPLNBAPPYPYLXYYZKYNLDZYJZCZNNLMZHHARQMPGWQTZMXXMLLHGDZXYHXKYXYCJMFFYYHJFSBSSQLXXNDYCANNMTCJCYPRRNYTYQNYYMBMSXNDLYLYSLJRLXYSXQMLLYZLZJJJKYZZCSFBZXXMSTBJGNXYZHLXNMCWSCYZYFZLXBRNNNYLBNRTGZQYSATSWRYHYJZMZDHZGZDWYBSSCSKXSYHYTXXGCQGXZZSHYXJSCRHMKKBXCZJYJYMKQHZJFNBHMQHYSNJNZYBKNQMCLGQHWLZNZSWXKHLJHYYBQLBFCDSXDLDSPFZPSKJYZWZXZDDXJSMMEGJSCSSMGCLXXKYYYLNYPWWWGYDKZJGGGZGGSYCKNJWNJPCXBJJTQTJWDSSPJXZXNZXUMELPXFSXTLLXCLJXJJLJZXCTPSWXLYDHLYQRWHSYCSQYYBYAYWJJJQFWQCQQCJQGXALDBZZYJGKGXPLTZYFXJLTPADKYQHPMATLCPDCKBMTXYBHKLENXDLEEGQDYMSAWHZMLJTWYGXLYQZLJEEYYBQQFFNLYXRDSCTGJGXYYNKLLYQKCCTLHJLQMKKZGCYYGLLLJDZGYDHZWXPYSJBZKDZGYZZHYWYFQYTYZSZYEZZLYMHJJHTSMQWYZLKYYWZCSRKQYTLTDXWCTYJKLWSQZWBDCQYNCJSRSZJLKCDCDTLZZZACQQZZDDXYPLXZBQJYLZLLLQDDZQJYJYJZYXNYYYNYJXKXDAZWYRDLJYYYRJLXLLDYXJCYWYWNQCCLDDNYYYNYCKCZHXXCCLGZQJGKWPPCQQJYSBZZXYJSQPXJPZBSBDSFNSFPZXHDWZTDWPPTFLZZBZDMYYPQJRSDZSQZSQXBDGCPZSWDWCSQZGMDHZXMWWFYBPDGPHTMJTHZSMMBGZMBZJCFZWFZBBZMQCFMBDMCJXLGPNJBBXGYHYYJGPTZGZMQBQTCGYXJXLWZKYDPDYMGCFTPFXYZTZXDZXTGKMTYBBCLBJASKYTSSQYYMSZXFJEWLXLLSZBQJJJAKLYLXLYCCTSXMCWFKKKBSXLLLLJYXTYLTJYYTDPJHNHNNKBYQNFQYYZBYYESSESSGDYHFHWTCJBSDZZTFDMXHCNJZYMQWSRYJDZJQPDQBBSTJGGFBKJBXTGQHNGWJXJGDLLTHZHHYYYYYYSXWTYYYCCBDBPYPZYCCZYJPZYWCBDLFWZCWJDXXHYHLHWZZXJTCZLCDPXUJCZZZLYXJJTXPHFXWPYWXZPTDZZBDZCYHJHMLXBQXSBYLRDTGJRRCTTTHYTCZWMXFYTWWZCWJWXJYWCSKYBZSCCTZQNHXNWXXKHKFHTSWOCCJYBCMPZZYKBNNZPBZHHZDLSYDDYTYFJPXYNGFXBYQXCBHXCPSXTYZDMKYSNXSXLHKMZXLYHDHKWHXXSSKQYHHCJYXGLHZXCSNHEKDTGZXQYPKDHEXTYKCNYMYYYPKQYYYKXZLTHJQTBYQHXBMYHSQCKWWYLLHCYYLNNEQXQWMCFBDCCMLJGGXDQKTLXKGNQCDGZJWYJJLYHHQTTTNWCHMXCXWHWSZJYDJCCDBQCDGDNYXZTHCQRXCBHZTQCBXWGQWYYBXHMBYMYQTYEXMQKYAQYRGYZSLFYKKQHYSSQYSHJGJCNXKZYCXSBXYXHYYLSTYCXQTHYSMGSCPMMGCCCCCMTZTASMGQZJHKLOSQYLSWTMXSYQKDZLJQQYPLSYCZTCQQPBBQJZCLPKHQZYYXXDTDDTSJCXFFLLCHQXMJLWCJCXTSPYCXNDTJSHJWXDQQJSKXYAMYLSJHMLALYKXCYYDMNMDQMXMCZNNCYBZKKYFLMCHCMLHXRCJJHSYLNMTJZGZGYWJXSRXCWJGJQHQZDQJDCJJZKJKGDZQGJJYJYLXZXXCDQHHHEYTMHLFSBDJSYYSHFYSTCZQLPBDRFRZTZYKYWHSZYQKWDQZRKMSYNBCRXQBJYFAZPZZEDZCJYWBCJWHYJBQSZYWRYSZPTDKZPFPBNZTKLQYHBBZPNPPTYZZYBQNYDCPJMMCYCQMCYFZZDCMNLFPBPLNGQJTBTTNJZPZBBZNJKLJQYLNBZQHKSJZNGGQSZZKYXSHPZSNBCGZKDDZQANZHJKDRTLZLSWJLJZLYWTJNDJZJHXYAYNCBGTZCSSQMNJPJYTYSWXZFKWJQTKHTZPLBHSNJZSYZBWZZZZLSYLSBJHDWWQPSLMMFBJDWAQYZTCJTBNNWZXQXCDSLQGDSDPDZHJTQQPSWLYYJZLGYXYZLCTCBJTKTYCZJTQKBSJLGMGZDMCSGPYNJZYQYYKNXRPWSZXMTNCSZZYXYBYHYZAXYWQCJTLLCKJJTJHGDXDXYQYZZBYWDLWQCGLZGJGQRQZCZSSBCRPCSKYDZNXJSQGXSSJMYDNSTZTPBDLTKZWXQWQTZEXNQCZGWEZKSSBYBRTSSSLCCGBPSZQSZLCCGLLLZXHZQTHCZMQGYZQZNMCOCSZJMMZSQPJYGQLJYJPPLDXRGZYXCCSXHSHGTZNLZWZKJCXTCFCJXLBMQBCZZWPQDNHXLJCTHYZLGYLNLSZZPCXDSCQQHJQKSXZPBAJYEMSMJTZDXLCJYRYYNWJBNGZZTMJXLTBSLYRZPYLSSCNXPHLLHYLLQQZQLXYMRSYCXZLMMCZLTZSDWTJJLLNZGGQXPFSKYGYGHBFZPDKMWGHCXMSGDXJMCJZDYCABXJDLNBCDQYGSKYDQTXDJJYXMSZQAZDZFSLQXYJSJZYLBTXXWXQQZBJZUFBBLYLWDSLJHXJYZJWTDJCZFQZQZZDZSXZZQLZCDZFJHYSPYMPQZMLPPLFFXJJNZZYLSJEYQZFPFZKSYWJJJHRDJZZXTXXGLGHYDXCSKYSWMMZCWYBAZBJKSHFHJCXMHFQHYXXYZFTSJYZFXYXPZLCHMZMBXHZZSXYFYMNCWDABAZLXKTCSHHXKXJJZJSTHYGXSXYYHHHJWXKZXSSBZZWHHHCWTZZZPJXSNXQQJGZYZYWLLCWXZFXXYXYHXMKYYSWSQMNLNAYCYSPMJKHWCQHYLAJJMZXHMMCNZHBHXCLXTJPLTXYJHDYYLTTXFSZHYXXSJBJYAYRSMXYPLCKDUYHLXRLNLLSTYZYYQYGYHHSCCSMZCTZQXKYQFPYYRPFFLKQUNTSZLLZMWWTCQQYZWTLLMLMPWMBZSSTZRBPDDTLQJJBXZCSRZQQYGWCSXFWZLXCCRSZDZMCYGGDZQSGTJSWLJMYMMZYHFBJDGYXCCPSHXNZCSBSJYJGJMPPWAFFYFNXHYZXZYLREMZGZCYZSSZDLLJCSQFNXZKPTXZGXJJGFMYYYSNBTYLBNLHPFZDCYFBMGQRRSSSZXYSGTZRNYDZZCDGPJAFJFZKNZBLCZSZPSGCYCJSZLMLRSZBZZLDLSLLYSXSQZQLYXZLSKKBRXBRBZCYCXZZZEEYFGKLZLYYHGZSGZLFJHGTGWKRAAJYZKZQTSSHJJXDCYZUYJLZYRZDQQHGJZXSSZBYKJPBFRTJXLLFQWJHYLQTYMBLPZDXTZYGBDHZZRBGXHWNJTJXLKSCFSMWLSDQYSJTXKZSCFWJLBXFTZLLJZLLQBLSQMQQCGCZFPBPHZCZJLPYYGGDTGWDCFCZQYYYQYSSCLXZSKLZZZGFFCQNWGLHQYZJJCZLQZZYJPJZZBPDCCMHJGXDQDGDLZQMFGPSYTSDYFWWDJZJYSXYYCZCYHZWPBYKXRYLYBHKJKSFXTZJMMCKHLLTNYYMSYXYZPYJQYCSYCWMTJJKQYRHLLQXPSGTLYYCLJSCPXJYZFNMLRGJJTYZBXYZMSJYJHHFZQMSYXRSZCWTLRTQZSSTKXGQKGSPTGCZNJSJCQCXHMXGGZTQYDJKZDLBZSXJLHYQGGGTHQSZPYHJHHGYYGKGGCWJZZYLCZLXQSFTGZSLLLMLJSKCTBLLZZSZMMNYTPZSXQHJCJYQXYZXZQZCPSHKZZYSXCDFGMWQRLLQXRFZTLYSTCTMJCXJJXHJNXTNRZTZFQYHQGLLGCXSZSJDJLJCYDSJTLNYXHSZXCGJZYQPYLFHDJSBPCCZHJJJQZJQDYBSSLLCMYTTMQTBHJQNNYGKYRQYQMZGCJKPDCGMYZHQLLSLLCLMHOLZGDYYFZSLJCQZLYLZQJESHNYLLJXGJXLYSYYYXNBZLJSSZCQQCJYLLZLTJYLLZLLBNYLGQCHXYYXOXCXQKYJXXXYKLXSXXYQXCYKQXQCSGYXXYQXYGYTQOHXHXPYXXXULCYEYCHZZCBWQBBWJQZSCSZSSLZYLKDESJZWMYMCYTSDSXXSCJPQQSQYLYYZYCMDJDZYWCBTJSYDJKCYDDJLBDJJSODZYSYXQQYXDHHGQQYQHDYXWGMMMAJDYBBBPPBCMUUPLJZSMTXERXJMHQNUTPJDCBSSMSSSTKJTSSMMTRCPLZSZMLQDSDMJMQPNQDXCFYNBFSDQXYXHYAYKQYDDLQYYYSSZBYDSLNTFQTZQPZMCHDHCZCWFDXTMYQSPHQYYXSRGJCWTJTZZQMGWJJTJHTQJBBHWZPXXHYQFXXQYWYYHYSCDYDHHQMNMTMWCPBSZPPZZGLMZFOLLCFWHMMSJZTTDHZZYFFYTZZGZYSKYJXQYJZQBHMBZZLYGHGFMSHPZFZSNCLPBQSNJXZSLXXFPMTYJYGBXLLDLXPZJYZJYHHZCYWHJYLSJEXFSZZYWXKZJLUYDTMLYMQJPWXYHXSKTQJEZRPXXZHHMHWQPWQLYJJQJJZSZCPHJLCHHNXJLQWZJHBMZYXBDHHYPZLHLHLGFWLCHYYTLHJXCJMSCPXSTKPNHQXSRTYXXTESYJCTLSSLSTDLLLWWYHDHRJZSFGXTSYCZYNYHTDHWJSLHTZDQDJZXXQHGYLTZPHCSQFCLNJTCLZPFSTPDYNYLGMJLLYCQHYSSHCHYLHQYQTMZYPBYWRFQYKQSYSLZDQJMPXYYSSRHZJNYWTQDFZBWWTWWRXCWHGYHXMKMYYYQMSMZHNGCEPMLQQMTCWCTMMPXJPJJHFXYYZSXZHTYBMSTSYJTTQQQYYLHYNPYQZLCYZHZWSMYLKFJXLWGXYPJYTYSYXYMZCKTTWLKSMZSYLMPWLZWXWQZSSAQSYXYRHSSNTSRAPXCPWCMGDXHXZDZYFJHGZTTSBJHGYZSZYSMYCLLLXBTYXHBBZJKSSDMALXHYCFYGMQYPJYCQXJLLLJGSLZGQLYCJCCZOTYXMTMTTLLWTGPXYMZMKLPSZZZXHKQYSXCTYJZYHXSHYXZKXLZWPSQPYHJWPJPWXQQYLXSDHMRSLZZYZWTTCYXYSZZSHBSCCSTPLWSSCJCHNLCGCHSSPHYLHFHHXJSXYLLNYLSZDHZXYLSXLWZYKCLDYAXZCMDDYSPJTQJZLNWQPSSSWCTSTSZLBLNXSMNYYMJQBQHRZWTYYDCHQLXKPZWBGQYBKFCMZWPZLLYYLSZYDWHXPSBCMLJBSCGBHXLQHYRLJXYSWXWXZSLDFHLSLYNJLZYFLYJYCDRJLFSYZFSLLCQYQFGJYHYXZLYLMSTDJCYHBZLLNWLXXYGYYHSMGDHXXHHLZZJZXCZZZCYQZFNGWPYLCPKPYYPMCLQKDGXZGGWQBDXZZKZFBXXLZXJTPJPTTBYTSZZDWSLCHZHSLTYXHQLHYXXXYYZYSWTXZKHLXZXZPYHGCHKCFSYHUTJRLXFJXPTZTWHPLYXFCRHXSHXKYXXYHZQDXQWULHYHMJTBFLKHTXCWHJFWJCFPQRYQXCYYYQYGRPYWSGSUNGWCHKZDXYFLXXHJJBYZWTSXXNCYJJYMSWZJQRMHXZWFQSYLZJZGBHYNSLBGTTCSYBYXXWXYHXYYXNSQYXMQYWRGYQLXBBZLJSYLPSYTJZYHYZAWLRORJMKSCZJXXXYXCHDYXRYXXJDTSQFXLYLTSFFYXLMTYJMJUYYYXLTZCSXQZQHZXLYYXZHDNBRXXXJCTYHLBRLMBRLLAXKYLLLJLYXXLYCRYLCJTGJCMTLZLLCYZZPZPCYAWHJJFYBDYYZSMPCKZDQYQPBPCJPDCYZMDPBCYYDYCNNPLMTMLRMFMMGWYZBSJGYGSMZQQQZTXMKQWGXLLPJGZBQCDJJJFPKJKCXBLJMSWMDTQJXLDLPPBXCWRCQFBFQJCZAHZGMYKPHYYHZYKNDKZMBPJYXPXYHLFPNYYGXJDBKXNXHJMZJXSTRSTLDXSKZYSYBZXJLXYSLBZYSLHXJPFXPQNBYLLJQKYGZMCYZZYMCCSLCLHZFWFWYXZMWSXTYNXJHPYYMCYSPMHYSMYDYSHQYZCHMJJMZCAAGCFJBBHPLYZYLXXSDJGXDHKXXTXXNBHRMLYJSLTXMRHNLXQJXYZLLYSWQGDLBJHDCGJYQYCMHWFMJYBMBYJYJWYMDPWHXQLDYGPDFXXBCGJSPCKRSSYZJMSLBZZJFLJJJLGXZGYXYXLSZQYXBEXYXHGCXBPLDYHWETTWWCJMBTXCHXYQXLLXFLYXLLJLSSFWDPZSMYJCLMWYTCZPCHQEKCQBWLCQYDPLQPPQZQFJQDJHYMMCXTXDRMJWRHXCJZYLQXDYYNHYYHRSLSRSYWWZJYMTLTLLGTQCJZYABTCKZCJYCCQLJZQXALMZYHYWLWDXZXQDLLQSHGPJFJLJHJABCQZDJGTKHSSTCYJLPSWZLXZXRWGLDLZRLZXTGSLLLLZLYXXWGDZYGBDPHZPBRLWSXQBPFDWOFMWHLYPCBJCCLDMBZPBZZLCYQXLDOMZBLZWPDWYYGDSTTHCSQSCCRSSSYSLFYBFNTYJSZDFNDPDHDZZMBBLSLCMYFFGTJJQWFTMTPJWFNLBZCMMJTGBDZLQLPYFHYYMJYLSDCHDZJWJCCTLJCLDTLJJCPDDSQDSSZYBNDBJLGGJZXSXNLYCYBJXQYCBYLZCFZPPGKCXZDZFZTJJFJSJXZBNZYJQTTYJYHTYCZHYMDJXTTMPXSPLZCDWSLSHXYPZGTFMLCJTYCBPMGDKWYCYZCDSZZYHFLYCTYGWHKJYYLSJCXGYWJCBLLCSNDDBTZBSCLYZCZZSSQDLLMQYYHFSLQLLXFTYHABXGWNYWYYPLLSDLDLLBJCYXJZMLHLJDXYYQYTDLLLBUGBFDFBBQJZZMDPJHGCLGMJJPGAEHHBWCQXAXHHHZCHXYPHJAXHLPHJPGPZJQCQZGJJZZUZDMQYYBZZPHYHYBWHAZYJHYKFGDPFQSDLZMLJXKXGALXZDAGLMDGXMWZQYXXDXXPFDMMSSYMPFMDMMKXKSYZYSHDZKXSYSMMZZZMSYDNZZCZXFPLSTMZDNMXCKJMZTYYMZMZZMSXHHDCZJEMXXKLJSTLWLSQLYJZLLZJSSDPPMHNLZJCZYHMXXHGZCJMDHXTKGRMXFWMCGMWKDTKSXQMMMFZZYDKMSCLCMPCGMHSPXQPZDSSLCXKYXTWLWJYAHZJGZQMCSNXYYMMPMLKJXMHLMLQMXCTKZMJQYSZJSYSZHSYJZJCDAJZYBSDQJZGWZQQXFKDMSDJLFWEHKZQKJPEYPZYSZCDWYJFFMZZYLTTDZZEFMZLBNPPLPLPEPSZALLTYLKCKQZKGENQLWAGYXYDPXLHSXQQWQCQXQCLHYXXMLYCCWLYMQYSKGCHLCJNSZKPYZKCQZQLJPDMDZHLASXLBYDWQLWDNBQCRYDDZTJYBKBWSZDXDTNPJDTCTQDFXQQMGNXECLTTBKPWSLCTYQLPWYZZKLPYGZCQQPLLKCCYLPQMZCZQCLJSLQZDJXLDDHPZQDLJJXZQDXYZQKZLJCYQDYJPPYPQYKJYRMPCBYMCXKLLZLLFQPYLLLMBSGLCYSSLRSYSQTMXYXZQZFDZUYSYZTFFMZZSMZQHZSSCCMLYXWTPZGXZJGZGSJSGKDDHTQGGZLLBJDZLCBCHYXYZHZFYWXYZYMSDBZZYJGTSMTFXQYXQSTDGSLNXDLRYZZLRYYLXQHTXSRTZNGZXBNQQZFMYKMZJBZYMKBPNLYZPBLMCNQYZZZSJZHJCTZKHYZZJRDYZHNPXGLFZTLKGJTCTSSYLLGZRZBBQZZKLPKLCZYSSUYXBJFPNJZZXCDWXZYJXZZDJJKGGRSRJKMSMZJLSJYWQSKYHQJSXPJZZZLSNSHRNYPZTWCHKLPSRZLZXYJQXQKYSJYCZTLQZYBBYBWZPQDWWYZCYTJCJXCKCWDKKZXSGKDZXWWYYJQYYTCYTDLLXWKCZKKLCCLZCQQDZLQLCSFQCHQHSFSMQZZLNBJJZBSJHTSZDYSJQJPDLZCDCWJKJZZLPYCGMZWDJJBSJQZSYZYHHXJPBJYDSSXDZNCGLQMBTSFSBPDZDLZNFGFJGFSMPXJQLMBLGQCYYXBQKDJJQYRFKZTJDHCZKLBSDZCFJTPLLJGXHYXZCSSZZXSTJYGKGCKGYOQXJPLZPBPGTGYJZGHZQZZLBJLSQFZGKQQJZGYCZBZQTLDXRJXBSXXPZXHYZYCLWDXJJHXMFDZPFZHQHQMQGKSLYHTYCGFRZGNQXCLPDLBZCSCZQLLJBLHBZCYPZZPPDYMZZSGYHCKCPZJGSLJLNSCDSLDLXBMSTLDDFJMKDJDHZLZXLSZQPQPGJLLYBDSZGQLBZLSLKYYHZTTNTJYQTZZPSZQZTLLJTYYLLQLLQYZQLBDZLSLYYZYMDFSZSNHLXZNCZQZPBWSKRFBSYZMTHBLGJPMCZZLSTLXSHTCSYZLZBLFEQHLXFLCJLYLJQCBZLZJHHSSTBRMHXZHJZCLXFNBGXGTQJCZTMSFZKJMSSNXLJKBHSJXNTNLZDNTLMSJXGZJYJCZXYJYJWRWWQNZTNFJSZPZSHZJFYRDJSFSZJZBJFZQZZHZLXFYSBZQLZSGYFTZDCSZXZJBQMSZKJRHYJZCKMJKHCHGTXKXQGLXPXFXTRTYLXJXHDTSJXHJZJXZWZLCQSBTXWXGXTXXHXFTSDKFJHZYJFJXRZSDLLLTQSQQZQWZXSYQTWGWBZCGZLLYZBCLMQQTZHZXZXLJFRMYZFLXYSQXXJKXRMQDZDMMYYBSQBHGZMWFWXGMXLZPYYTGZYCCDXYZXYWGSYJYZNBHPZJSQSYXSXRTFYZGRHZTXSZZTHCBFCLSYXZLZQMZLMPLMXZJXSFLBYZMYQHXJSXRXSQZZZSSLYFRCZJRCRXHHZXQYDYHXSJJHZCXZBTYNSYSXJBQLPXZQPYMLXZKYXLXCJLCYSXXZZLXDLLLJJYHZXGYJWKJRWYHCPSGNRZLFZWFZZNSXGXFLZSXZZZBFCSYJDBRJKRDHHGXJLJJTGXJXXSTJTJXLYXQFCSGSWMSBCTLQZZWLZZKXJMLTMJYHSDDBXGZHDLBMYJFRZFSGCLYJBPMLYSMSXLSZJQQHJZFXGFQFQBPXZGYYQXGZTCQWYLTLGWSGWHRLFSFGZJMGMGBGTJFSYZZGZYZAFLSSPMLPFLCWBJZCLJJMZLPJJLYMQDMYYYFBGYGYZMLYZDXQYXRQQQHSYYYQXYLJTYXFSFSLLGNQCYHYCWFHCCCFXPYLYPLLZYXXXXXKQHHXSHJZCFZSCZJXCPZWHHHHHAPYLQALPQAFYHXDYLUKMZQGGGDDESRNNZLTZGCHYPPYSQJJHCLLJTOLNJPZLJLHYMHEYDYDSQYCDDHGZUNDZCLZYZLLZNTNYZGSLHSLPJJBDGWXPCDUTJCKLKCLWKLLCASSTKZZDNQNTTLYYZSSYSSZZRYLJQKCQDHHCRXRZYDGRGCWCGZQFFFPPJFZYNAKRGYWYQPQXXFKJTSZZXSWZDDFBBXTBGTZKZNPZZPZXZPJSZBMQHKCYXYLDKLJNYPKYGHGDZJXXEAHPNZKZTZCMXCXMMJXNKSZQNMNLWBWWXJKYHCPSTMCSQTZJYXTPCTPDTNNPGLLLZSJLSPBLPLQHDTNJNLYYRSZFFJFQWDPHZDWMRZCCLODAXNSSNYZRESTYJWJYJDBCFXNMWTTBYLWSTSZGYBLJPXGLBOCLHPCBJLTMXZLJYLZXCLTPNCLCKXTPZJSWCYXSFYSZDKNTLBYJCYJLLSTGQCBXRYZXBXKLYLHZLQZLNZCXWJZLJZJNCJHXMNZZGJZZXTZJXYCYYCXXJYYXJJXSSSJSTSSTTPPGQTCSXWZDCSYFPTFBFHFBBLZJCLZZDBXGCXLQPXKFZFLSYLTUWBMQJHSZBMDDBCYSCCLDXYCDDQLYJJWMQLLCSGLJJSYFPYYCCYLTJANTJJPWYCMMGQYYSXDXQMZHSZXPFTWWZQSWQRFKJLZJQQYFBRXJHHFWJJZYQAZMYFRHCYYBYQWLPEXCCZSTYRLTTDMQLYKMBBGMYYJPRKZNPBSXYXBHYZDJDNGHPMFSGMWFZMFQMMBCMZZCJJLCNUXYQLMLRYGQZCYXZLWJGCJCGGMCJNFYZZJHYCPRRCMTZQZXHFQGTJXCCJEAQCRJYHPLQLSZDJRBCQHQDYRHYLYXJSYMHZYDWLDFRYHBPYDTSSCNWBXGLPZMLZZTQSSCPJMXXYCSJYTYCGHYCJWYRXXLFEMWJNMKLLSWTXHYYYNCMMCWJDQDJZGLLJWJRKHPZGGFLCCSCZMCBLTBHBQJXQDSPDJZZGKGLFQYWBZYZJLTSTDHQHCTCBCHFLQMPWDSHYYTQWCNZZJTLBYMBPDYYYXSQKXWYYFLXXNCWCXYPMAELYKKJMZZZBRXYYQJFLJPFHHHYTZZXSGQQMHSPGDZQWBWPJHZJDYSCQWZKTXXSQLZYYMYSDZGRXCKKUJLWPYSYSCSYZLRMLQSYLJXBCXTLWDQZPCYCYKPPPNSXFYZJJRCEMHSZMSXLXGLRWGCSTLRSXBZGBZGZTCPLUJLSLYLYMTXMTZPALZXPXJTJWTCYYZLBLXBZLQMYLXPGHDSLSSDMXMBDZZSXWHAMLCZCPJMCNHJYSNSYGCHSKQMZZQDLLKABLWJXSFMOCDXJRRLYQZKJMYBYQLYHETFJZFRFKSRYXFJTWDSXXSYSQJYSLYXWJHSNLXYYXHBHAWHHJZXWMYLJCSSLKYDZTXBZSYFDXGXZJKHSXXYBSSXDPYNZWRPTQZCZENYGCXQFJYKJBZMLJCMQQXUOXSLYXXLYLLJDZBTYMHPFSTTQQWLHOKYBLZZALZXQLHZWRRQHLSTMYPYXJJXMQSJFNBXYXYJXXYQYLTHYLQYFMLKLJTMLLHSZWKZHLJMLHLJKLJSTLQXYLMBHHLNLZXQJHXCFXXLHYHJJGBYZZKBXSCQDJQDSUJZYYHZHHMGSXCSYMXFEBCQWWRBPYYJQTYZCYQYQQZYHMWFFHGZFRJFCDPXNTQYZPDYKHJLFRZXPPXZDBBGZQSTLGDGYLCQMLCHHMFYWLZYXKJLYPQHSYWMQQGQZMLZJNSQXJQSYJYCBEHSXFSZPXZWFLLBCYYJDYTDTHWZSFJMQQYJLMQXXLLDTTKHHYBFPWTYYSQQWNQWLGWDEBZWCMYGCULKJXTMXMYJSXHYBRWFYMWFRXYQMXYSZTZZTFYKMLDHQDXWYYNLCRYJBLPSXCXYWLSPRRJWXHQYPHTYDNXHHMMYWYTZCSQMTSSCCDALWZTCPQPYJLLQZYJSWXMZZMMYLMXCLMXCZMXMZSQTZPPQQBLPGXQZHFLJJHYTJSRXWZXSCCDLXTYJDCQJXSLQYCLZXLZZXMXQRJMHRHZJBHMFLJLMLCLQNLDXZLLLPYPSYJYSXCQQDCMQJZZXHNPNXZMEKMXHYKYQLXSXTXJYYHWDCWDZHQYYBGYBCYSCFGPSJNZDYZZJZXRZRQJJYMCANYRJTLDPPYZBSTJKXXZYPFDWFGZZRPYMTNGXZQBYXNBUFNQKRJQZMJEGRZGYCLKXZDSKKNSXKCLJSPJYYZLQQJYBZSSQLLLKJXTBKTYLCCDDBLSPPFYLGYDTZJYQGGKQTTFZXBDKTYYHYBBFYTYYBCLPDYTGDHRYRNJSPTCSNYJQHKLLLZSLYDXXWBCJQSPXBPJZJCJDZFFXXBRMLAZHCSNDLBJDSZBLPRZTSWSBXBCLLXXLZDJZSJPYLYXXYFTFFFBHJJXGBYXJPMMMPSSJZJMTLYZJXSWXTYLEDQPJMYGQZJGDJLQJWJQLLSJGJGYGMSCLJJXDTYGJQJQJCJZCJGDZZSXQGSJGGCXHQXSNQLZZBXHSGZXCXYLJXYXYYDFQQJHJFXDHCTXJYRXYSQTJXYEFYYSSYYJXNCYZXFXMSYSZXYYSCHSHXZZZGZZZGFJDLTYLNPZGYJYZYYQZPBXQBDZTZCZYXXYHHSQXSHDHGQHJHGYWSZTMZMLHYXGEBTYLZKQWYTJZRCLEKYSTDBCYKQQSAYXCJXWWGSBHJYZYDHCSJKQCXSWXFLTYNYZPZCCZJQTZWJQDZZZQZLJJXLSBHPYXXPSXSHHEZTXFPTLQYZZXHYTXNCFZYYHXGNXMYWXTZSJPTHHGYMXMXQZXTSBCZYJYXXTYYZYPCQLMMSZMJZZLLZXGXZAAJZYXJMZXWDXZSXZDZXLEYJJZQBHZWZZZQTZPSXZTDSXJJJZNYAZPHXYYSRNQDTHZHYYKYJHDZXZLSWCLYBZYECWCYCRYLCXNHZYDZYDYJDFRJJHTRSQTXYXJRJHOJYNXELXSFSFJZGHPZSXZSZDZCQZBYYKLSGSJHCZSHDGQGXYZGXCHXZJWYQWGYHKSSEQZZNDZFKWYSSTCLZSTSYMCDHJXXYWEYXCZAYDMPXMDSXYBSQMJMZJMTZQLPJYQZCGQHXJHHLXXHLHDLDJQCLDWBSXFZZYYSCHTYTYYBHECXHYKGJPXHHYZJFXHWHBDZFYZBCAPNPGNYDMSXHMMMMAMYNBYJTMPXYYMCTHJBZYFCGTYHWPHFTWZZEZSBZEGPFMTSKFTYCMHFLLHGPZJXZJGZJYXZSBBQSCZZLZCCSTPGXMJSFTCCZJZDJXCYBZLFCJSYZFGSZLYBCWZZBYZDZYPSWYJZXZBDSYUXLZZBZFYGCZXBZHZFTPBGZGEJBSTGKDMFHYZZJHZLLZZGJQZLSFDJSSCBZGPDLFZFZSZYZYZSYGCXSNXXCHCZXTZZLJFZGQSQYXZJQDCCZTQCDXZJYQJQCHXZTDLGSCXZSYQJQTZWLQDQZTQCHQQJZYEZZZPBWKDJFCJPZTYPQYQTTYNLMBDKTJZPQZQZZFPZSBNJLGYJDXJDZZKZGQKXDLPZJTCJDQBXDJQJSTCKNXBXZMSLYJCQMTJQWWCJQNJNLLLHJCWQTBZQYDZCZPZZDZYDDCYZZZCCJTTJFZDPRRTZTJDCQTQZDTJNPLZBCLLCTZSXKJZQZPZLBZRBTJDCXFCZDBCCJJLTQQPLDCGZDBBZJCQDCJWYNLLZYZCCDWLLXWZLXRXNTQQCZXKQLSGDFQTDDGLRLAJJTKUYMKQLLTZYTDYYCZGJWYXDXFRSKSTQTENQMRKQZHHQKDLDAZFKYPBGGPZREBZZYKZZSPEGJXGYKQZZZSLYSYYYZWFQZYLZZLZHWCHKYPQGNPGBLPLRRJYXCCSYYHSFZFYBZYYTGZXYLXCZWXXZJZBLFFLGSKHYJZEYJHLPLLLLCZGXDRZELRHGKLZZYHZLYQSZZJZQLJZFLNBHGWLCZCFJYSPYXZLZLXGCCPZBLLCYBBBBUBBCBPCRNNZCZYRBFSRLDCGQYYQXYGMQZWTZYTYJXYFWTEHZZJYWLCCNTZYJJZDEDPZDZTSYQJHDYMBJNYJZLXTSSTPHNDJXXBYXQTZQDDTJTDYYTGWSCSZQFLSHLGLBCZPHDLYZJYCKWTYTYLBNYTSDSYCCTYSZYYEBHEXHQDTWNYGYCLXTSZYSTQMYGZAZCCSZZDSLZCLZRQXYYELJSBYMXSXZTEMBBLLYYLLYTDQYSHYMRQWKFKBFXNXSBYCHXBWJYHTQBPBSBWDZYLKGZSKYHXQZJXHXJXGNLJKZLYYCDXLFYFGHLJGJYBXQLYBXQPQGZTZPLNCYPXDJYQYDYMRBESJYYHKXXSTMXRCZZYWXYQYBMCLLYZHQYZWQXDBXBZWZMSLPDMYSKFMZKLZCYQYCZLQXFZZYDQZPZYGYJYZMZXDZFYFYTTQTZHGSPCZMLCCYTZXJCYTJMKSLPZHYSNZLLYTPZCTZZCKTXDHXXTQCYFKSMQCCYYAZHTJPCYLZLYJBJXTPNYLJYYNRXSYLMMNXJSMYBCSYSYLZYLXJJQYLDZLPQBFZZBLFNDXQKCZFYWHGQMRDSXYCYTXNQQJZYYPFZXDYZFPRXEJDGYQBXRCNFYYQPGHYJDYZXGRHTKYLNWDZNTSMPKLBTHBPYSZBZTJZSZZJTYYXZPHSSZZBZCZPTQFZMYFLYPYBBJQXZMXXDJMTSYSKKBJZXHJCKLPSMKYJZCXTMLJYXRZZQSLXXQPYZXMKYXXXJCLJPRMYYGADYSKQLSNDHYZKQXZYZTCGHZTLMLWZYBWSYCTBHJHJFCWZTXWYTKZLXQSHLYJZJXTMPLPYCGLTBZZTLZJCYJGDTCLKLPLLQPJMZPAPXYZLKKTKDZCZZBNZDYDYQZJYJGMCTXLTGXSZLMLHBGLKFWNWZHDXUHLFMKYSLGXDTWWFRJEJZTZHYDXYKSHWFZCQSHKTMQQHTZHYMJDJSKHXZJZBZZXYMPAGQMSTPXLSKLZYNWRTSQLSZBPSPSGZWYHTLKSSSWHZZLYYTNXJGMJSZSUFWNLSOZTXGXLSAMMLBWLDSZYLAKQCQCTMYCFJBSLXCLZZCLXXKSBZQCLHJPSQPLSXXCKSLNHPSFQQYTXYJZLQLDXZQJZDYYDJNZPTUZDSKJFSLJHYLZSQZLBTXYDGTQFDBYAZXDZHZJNHHQBYKNXJJQCZMLLJZKSPLDYCLBBLXKLELXJLBQYCXJXGCNLCQPLZLZYJTZLJGYZDZPLTQCSXFDMNYCXGBTJDCZNBGBQYQJWGKFHTNPYQZQGBKPBBYZMTJDYTBLSQMPSXTBNPDXKLEMYYCJYNZCTLDYKZZXDDXHQSHDGMZSJYCCTAYRZLPYLTLKXSLZCGGEXCLFXLKJRTLQJAQZNCMBYDKKCXGLCZJZXJHPTDJJMZQYKQSECQZDSHHADMLZFMMZBGNTJNNLGBYJBRBTMLBYJDZXLCJLPLDLPCQDHLXZLYCBLCXZZJADJLNZMMSSSMYBHBSQKBHRSXXJMXSDZNZPXLGBRHWGGFCXGMSKLLTSJYYCQLTSKYWYYHYWXBXQYWPYWYKQLSQPTNTKHQCWDQKTWPXXHCPTHTWUMSSYHBWCRWXHJMKMZNGWTMLKFGHKJYLSYYCXWHYECLQHKQHTTQKHFZLDXQWYZYYDESBPKYRZPJFYYZJCEQDZZDLATZBBFJLLCXDLMJSSXEGYGSJQXCWBXSSZPDYZCXDNYXPPZYDLYJCZPLTXLSXYZYRXCYYYDYLWWNZSAHJSYQYHGYWWAXTJZDAXYSRLTDPSSYYFNEJDXYZHLXLLLZQZSJNYQYQQXYJGHZGZCYJCHZLYCDSHWSHJZYJXCLLNXZJJYYXNFXMWFPYLCYLLABWDDHWDXJMCXZTZPMLQZHSFHZYNZTLLDYWLSLXHYMMYLMBWWKYXYADTXYLLDJPYBPWUXJMWMLLSAFDLLYFLBHHHBQQLTZJCQJLDJTFFKMMMBYTHYGDCQRDDWRQJXNBYSNWZDBYYTBJHPYBYTTJXAAHGQDQTMYSTQXKBTZPKJLZRBEQQSSMJJBDJOTGTBXPGBKTLHQXJJJCTHXQDWJLWRFWQGWSHCKRYSWGFTGYGBXSDWDWRFHWYTJJXXXJYZYSLPYYYPAYXHYDQKXSHXYXGSKQHYWFDDDPPLCJLQQEEWXKSYYKDYPLTJTHKJLTCYYHHJTTPLTZZCDLTHQKZXQYSTEEYWYYZYXXYYSTTJKLLPZMCYHQGXYHSRMBXPLLNQYDQHXSXXWGDQBSHYLLPJJJTHYJKYPPTHYYKTYEZYENMDSHLCRPQFDGFXZPSFTLJXXJBSWYYSKSFLXLPPLBBBLBSFXFYZBSJSSYLPBBFFFFSSCJDSTZSXZRYYSYFFSYZYZBJTBCTSBSDHRTJJBYTCXYJEYLXCBNEBJDSYXYKGSJZBXBYTFZWGENYHHTHZHHXFWGCSTBGXKLSXYWMTMBYXJSTZSCDYQRCYTWXZFHMYMCXLZNSDJTTTXRYCFYJSBSDYERXJLJXBBDEYNJGHXGCKGSCYMBLXJMSZNSKGXFBNBPTHFJAAFXYXFPXMYPQDTZCXZZPXRSYWZDLYBBKTYQPQJPZYPZJZNJPZJLZZFYSBTTSLMPTZRTDXQSJEHBZYLZDHLJSQMLHTXTJECXSLZZSPKTLZKQQYFSYGYWPCPQFHQHYTQXZKRSGTTSQCZLPTXCDYYZXSQZSLXLZMYCPCQBZYXHBSXLZDLTCDXTYLZJYYZPZYZLTXJSJXHLPMYTXCQRBLZSSFJZZTNJYTXMYJHLHPPLCYXQJQQKZZSCPZKSWALQSBLCCZJSXGWWWYGYKTJBBZTDKHXHKGTGPBKQYSLPXPJCKBMLLXDZSTBKLGGQKQLSBKKTFXRMDKBFTPZFRTBBRFERQGXYJPZSSTLBZTPSZQZSJDHLJQLZBPMSMMSXLQQNHKNBLRDDNXXDHDDJCYYGYLXGZLXSYGMQQGKHBPMXYXLYTQWLWGCPBMQXCYZYDRJBHTDJYHQSHTMJSBYPLWHLZFFNYPMHXXHPLTBQPFBJWQDBYGPNZTPFZJGSDDTQSHZEAWZZYLLTYYBWJKXXGHLFKXDJTMSZSQYNZGGSWQSPHTLSSKMCLZXYSZQZXNCJDQGZDLFNYKLJCJLLZLMZZNHYDSSHTHZZLZZBBHQZWWYCRZHLYQQJBEYFXXXWHSRXWQHWPSLMSSKZTTYGYQQWRSLALHMJTQJSMXQBJJZJXZYZKXBYQXBJXSHZTSFJLXMXZXFGHKZSZGGYLCLSARJYHSLLLMZXELGLXYDJYTLFBHBPNLYZFBBHPTGJKWETZHKJJXZXXGLLJLSTGSHJJYQLQZFKCGNNDJSSZFDBCTWWSEQFHQJBSAQTGYPQLBXBMMYWXGSLZHGLZGQYFLZBYFZJFRYSFMBYZHQGFWZSYFYJJPHZBYYZFFWODGRLMFTWLBZGYCQXCDJYGZYYYYTYTYDWEGAZYHXJLZYYHLRMGRXXZCLHNELJJTJTPWJYBJJBXJJTJTEEKHWSLJPLPSFYZPQQBDLQJJTYYQLYZKDKSQJYYQZLDQTGJQYZJSUCMRYQTHTEJMFCTYHYPKMHYZWJDQFHYYXWSHCTXRLJHQXHCCYYYJLTKTTYTMXGTCJTZAYYOCZLYLBSZYWJYTSJYHBYSHFJLYGJXXTMZYYLTXXYPZLXYJZYZYYPNHMYMDYYLBLHLSYYQQLLNJJYMSOYQBZGDLYXYLCQYXTSZEGXHZGLHWBLJHEYXTWQMAKBPQCGYSHHEGQCMWYYWLJYJHYYZLLJJYLHZYHMGSLJLJXCJJYCLYCJPCPZJZJMMYLCQLNQLJQJSXYJMLSZLJQLYCMMHCFMMFPQQMFYLQMCFFQMMMMHMZNFHHJGTTHHKHSLNCHHYQDXTMMQDCYZYXYQMYQYLTDCYYYZAZZCYMZYDLZFFFMMYCQZWZZMABTBYZTDMNZZGGDFTYPCGQYTTSSFFWFDTZQSSYSTWXJHXYTSXXYLBYQHWWKXHZXWZNNZZJZJJQJCCCHYYXBZXZCYZTLLCQXYNJYCYYCYNZZQYYYEWYCZDCJYCCHYJLBTZYYCQWMPWPYMLGKDLDLGKQQBGYCHJXY", this.full_dict = { a: "啊阿锕", ai: "埃挨哎唉哀皑癌蔼矮艾碍爱隘诶捱嗳嗌嫒瑷暧砹锿霭", an: "鞍氨安俺按暗岸胺案谙埯揞犴庵桉铵鹌顸黯", ang: "肮昂盎", ao: "凹敖熬翱袄傲奥懊澳坳拗嗷噢岙廒遨媪骜聱螯鏊鳌鏖", ba: "芭捌扒叭吧笆八疤巴拔跋靶把耙坝霸罢爸茇菝萆捭岜灞杷钯粑鲅魃", bai: "白柏百摆佰败拜稗薜掰鞴", ban: "斑班搬扳般颁板版扮拌伴瓣半办绊阪坂豳钣瘢癍舨", bang: "邦帮梆榜膀绑棒磅蚌镑傍谤蒡螃", bao: "苞胞包褒雹保堡饱宝抱报暴豹鲍爆勹葆宀孢煲鸨褓趵龅", bo: "剥薄玻菠播拨钵波博勃搏铂箔伯帛舶脖膊渤泊驳亳蕃啵饽檗擘礴钹鹁簸跛", bei: "杯碑悲卑北辈背贝钡倍狈备惫焙被孛陂邶埤蓓呗怫悖碚鹎褙鐾", ben: "奔苯本笨畚坌锛", beng: "崩绷甭泵蹦迸唪嘣甏", bi: "逼鼻比鄙笔彼碧蓖蔽毕毙毖币庇痹闭敝弊必辟壁臂避陛匕仳俾芘荜荸吡哔狴庳愎滗濞弼妣婢嬖璧贲畀铋秕裨筚箅篦舭襞跸髀", bian: "鞭边编贬扁便变卞辨辩辫遍匾弁苄忭汴缏煸砭碥稹窆蝙笾鳊", biao: "标彪膘表婊骠飑飙飚灬镖镳瘭裱鳔", bie: "鳖憋别瘪蹩鳘", bin: "彬斌濒滨宾摈傧浜缤玢殡膑镔髌鬓", bing: "兵冰柄丙秉饼炳病并禀邴摒绠枋槟燹", bu: "捕卜哺补埠不布步簿部怖拊卟逋瓿晡钚醭", ca: "擦嚓礤", cai: "猜裁材才财睬踩采彩菜蔡", can: "餐参蚕残惭惨灿骖璨粲黪", cang: "苍舱仓沧藏伧", cao: "操糙槽曹草艹嘈漕螬艚", ce: "厕策侧册测刂帻恻", ceng: "层蹭噌", cha: "插叉茬茶查碴搽察岔差诧猹馇汊姹杈楂槎檫钗锸镲衩", chai: "拆柴豺侪茈瘥虿龇", chan: "搀掺蝉馋谗缠铲产阐颤冁谄谶蒇廛忏潺澶孱羼婵嬗骣觇禅镡裣蟾躔", chang: "昌猖场尝常长偿肠厂敞畅唱倡伥鬯苌菖徜怅惝阊娼嫦昶氅鲳", chao: "超抄钞朝嘲潮巢吵炒怊绉晁耖", che: "车扯撤掣彻澈坼屮砗", chen: "郴臣辰尘晨忱沉陈趁衬称谌抻嗔宸琛榇肜胂碜龀", cheng: "撑城橙成呈乘程惩澄诚承逞骋秤埕嵊徵浈枨柽樘晟塍瞠铖裎蛏酲", chi: "吃痴持匙池迟弛驰耻齿侈尺赤翅斥炽傺墀芪茌搋叱哧啻嗤彳饬沲媸敕胝眙眵鸱瘛褫蚩螭笞篪豉踅踟魑", chong: "充冲虫崇宠茺忡憧铳艟", chou: "抽酬畴踌稠愁筹仇绸瞅丑俦圳帱惆溴妯瘳雠鲋", chu: "臭初出橱厨躇锄雏滁除楚础储矗搐触处亍刍憷绌杵楮樗蜍蹰黜", chuan: "揣川穿椽传船喘串掾舛惴遄巛氚钏镩舡", chuang: "疮窗幢床闯创怆", chui: "吹炊捶锤垂陲棰槌", chun: "春椿醇唇淳纯蠢促莼沌肫朐鹑蝽", chuo: "戳绰蔟辶辍镞踔龊", ci: "疵茨磁雌辞慈瓷词此刺赐次荠呲嵯鹚螅糍趑", cong: "聪葱囱匆从丛偬苁淙骢琮璁枞", cu: "凑粗醋簇猝殂蹙", cuan: "蹿篡窜汆撺昕爨", cui: "摧崔催脆瘁粹淬翠萃悴璀榱隹", cun: "村存寸磋忖皴", cuo: "撮搓措挫错厝脞锉矬痤鹾蹉躜", da: "搭达答瘩打大耷哒嗒怛妲疸褡笪靼鞑", dai: "呆歹傣戴带殆代贷袋待逮怠埭甙呔岱迨逯骀绐玳黛", dan: "耽担丹单郸掸胆旦氮但惮淡诞弹蛋亻儋卩萏啖澹檐殚赕眈瘅聃箪", dang: "当挡党荡档谠凼菪宕砀铛裆", dao: "刀捣蹈倒岛祷导到稻悼道盗叨啁忉洮氘焘忑纛", de: "德得的锝", deng: "蹬灯登等瞪凳邓噔嶝戥磴镫簦", di: "堤低滴迪敌笛狄涤翟嫡抵底地蒂第帝弟递缔氐籴诋谛邸坻莜荻嘀娣柢棣觌砥碲睇镝羝骶", dian: "颠掂滇碘点典靛垫电佃甸店惦奠淀殿丶阽坫埝巅玷癜癫簟踮", diao: "碉叼雕凋刁掉吊钓调轺铞蜩粜貂", die: "跌爹碟蝶迭谍叠佚垤堞揲喋渫轶牒瓞褶耋蹀鲽鳎", ding: "丁盯叮钉顶鼎锭定订丢仃啶玎腚碇町铤疔耵酊", dong: "东冬董懂动栋侗恫冻洞垌咚岽峒夂氡胨胴硐鸫", dou: "兜抖斗陡豆逗痘蔸钭窦窬蚪篼酡", du: "都督毒犊独读堵睹赌杜镀肚度渡妒芏嘟渎椟橐牍蠹笃髑黩", duan: "端短锻段断缎彖椴煅簖", dui: "堆兑队对怼憝碓", dun: "墩吨蹲敦顿囤钝盾遁炖砘礅盹镦趸", duo: "掇哆多夺垛躲朵跺舵剁惰堕咄哚缍柁铎裰踱", e: "蛾峨鹅俄额讹娥恶厄扼遏鄂饿噩谔垩垭苊莪萼呃愕屙婀轭曷腭硪锇锷鹗颚鳄", en: "恩蒽摁唔嗯", er: "而儿耳尔饵洱二贰迩珥铒鸸鲕", fa: "发罚筏伐乏阀法珐垡砝", fan: "藩帆番翻樊矾钒繁凡烦反返范贩犯饭泛蘩幡犭梵攵燔畈蹯", fang: "坊芳方肪房防妨仿访纺放匚邡彷钫舫鲂", fei: "菲非啡飞肥匪诽吠肺废沸费芾狒悱淝妃绋绯榧腓斐扉祓砩镄痱蜚篚翡霏鲱", fen: "芬酚吩氛分纷坟焚汾粉奋份忿愤粪偾瀵棼愍鲼鼢", feng: "丰封枫蜂峰锋风疯烽逢冯缝讽奉凤俸酆葑沣砜", fu: "佛否夫敷肤孵扶拂辐幅氟符伏俘服浮涪福袱弗甫抚辅俯釜斧脯腑府腐赴副覆赋复傅付阜父腹负富讣附妇缚咐匐凫郛芙苻茯莩菔呋幞滏艴孚驸绂桴赙黻黼罘稃馥虍蚨蜉蝠蝮麸趺跗鳆", ga: "噶嘎蛤尬呷尕尜旮钆", gai: "该改概钙盖溉丐陔垓戤赅胲", gan: "干甘杆柑竿肝赶感秆敢赣坩苷尴擀泔淦澉绀橄旰矸疳酐", gang: "冈刚钢缸肛纲岗港戆罡颃筻", gong: "杠工攻功恭龚供躬公宫弓巩汞拱贡共蕻廾咣珙肱蚣蛩觥", gao: "篙皋高膏羔糕搞镐稿告睾诰郜蒿藁缟槔槁杲锆", ge: "哥歌搁戈鸽胳疙割革葛格阁隔铬个各鬲仡哿塥嗝纥搿膈硌铪镉袼颌虼舸骼髂", gei: "给", gen: "根跟亘茛哏艮", geng: "耕更庚羹埂耿梗哽赓鲠", gou: "钩勾沟苟狗垢构购够佝诟岣遘媾缑觏彀鸲笱篝鞲", gu: "辜菇咕箍估沽孤姑鼓古蛊骨谷股故顾固雇嘏诂菰哌崮汩梏轱牯牿胍臌毂瞽罟钴锢瓠鸪鹄痼蛄酤觚鲴骰鹘", gua: "刮瓜剐寡挂褂卦诖呱栝鸹", guai: "乖拐怪哙", guan: "棺关官冠观管馆罐惯灌贯倌莞掼涫盥鹳鳏", guang: "光广逛犷桄胱疒", gui: "瑰规圭硅归龟闺轨鬼诡癸桂柜跪贵刽匦刿庋宄妫桧炅晷皈簋鲑鳜", gun: "辊滚棍丨衮绲磙鲧", guo: "锅郭国果裹过馘蠃埚掴呙囗帼崞猓椁虢锞聒蜮蜾蝈", ha: "哈", hai: "骸孩海氦亥害骇咴嗨颏醢", han: "酣憨邯韩含涵寒函喊罕翰撼捍旱憾悍焊汗汉邗菡撖阚瀚晗焓颔蚶鼾", hen: "夯痕很狠恨", hang: "行杭航沆绗珩桁", hao: "壕嚎豪毫郝好耗号浩薅嗥嚆濠灏昊皓颢蚝", he: "呵喝荷菏核禾和何合盒貉阂河涸赫褐鹤贺诃劾壑藿嗑嗬阖盍蚵翮", hei: "嘿黑", heng: "哼亨横衡恒訇蘅", hong: "轰哄烘虹鸿洪宏弘红黉讧荭薨闳泓", hou: "喉侯猴吼厚候后堠後逅瘊篌糇鲎骺", hu: "呼乎忽瑚壶葫胡蝴狐糊湖弧虎唬护互沪户冱唿囫岵猢怙惚浒滹琥槲轷觳烀煳戽扈祜鹕鹱笏醐斛", hua: "花哗华猾滑画划化话劐浍骅桦铧稞", huai: "槐徊怀淮坏还踝", huan: "欢环桓缓换患唤痪豢焕涣宦幻郇奂垸擐圜洹浣漶寰逭缳锾鲩鬟", huang: "荒慌黄磺蝗簧皇凰惶煌晃幌恍谎隍徨湟潢遑璜肓癀蟥篁鳇", hui: "灰挥辉徽恢蛔回毁悔慧卉惠晦贿秽会烩汇讳诲绘诙茴荟蕙哕喙隳洄彗缋珲晖恚虺蟪麾", hun: "荤昏婚魂浑混诨馄阍溷缗", huo: "豁活伙火获或惑霍货祸攉嚯夥钬锪镬耠蠖", ji: "击圾基机畸稽积箕肌饥迹激讥鸡姬绩缉吉极棘辑籍集及急疾汲即嫉级挤几脊己蓟技冀季伎祭剂悸济寄寂计记既忌际妓继纪居丌乩剞佶佴脔墼芨芰萁蒺蕺掎叽咭哜唧岌嵴洎彐屐骥畿玑楫殛戟戢赍觊犄齑矶羁嵇稷瘠瘵虮笈笄暨跻跽霁鲚鲫髻麂", jia: "嘉枷夹佳家加荚颊贾甲钾假稼价架驾嫁伽郏拮岬浃迦珈戛胛恝铗镓痂蛱笳袈跏", jian: "歼监坚尖笺间煎兼肩艰奸缄茧检柬碱硷拣捡简俭剪减荐槛鉴践贱见键箭件健舰剑饯渐溅涧建僭谏谫菅蒹搛囝湔蹇謇缣枧柙楗戋戬牮犍毽腱睑锏鹣裥笕箴翦趼踺鲣鞯", jiang: "僵姜将浆江疆蒋桨奖讲匠酱降茳洚绛缰犟礓耩糨豇", jiao: "蕉椒礁焦胶交郊浇骄娇嚼搅铰矫侥脚狡角饺缴绞剿教酵轿较叫佼僬茭挢噍峤徼姣纟敫皎鹪蛟醮跤鲛", jie: "窖揭接皆秸街阶截劫节桔杰捷睫竭洁结解姐戒藉芥界借介疥诫届偈讦诘喈嗟獬婕孑桀獒碣锴疖袷颉蚧羯鲒骱髫", jin: "巾筋斤金今津襟紧锦仅谨进靳晋禁近烬浸尽卺荩堇噤馑廑妗缙瑾槿赆觐钅锓衿矜", jing: "劲荆兢茎睛晶鲸京惊精粳经井警景颈静境敬镜径痉靖竟竞净刭儆阱菁獍憬泾迳弪婧肼胫腈旌", jiong: "炯窘冂迥扃", jiu: "揪究纠玖韭久灸九酒厩救旧臼舅咎就疚僦啾阄柩桕鹫赳鬏", ju: "鞠拘狙疽驹菊局咀矩举沮聚拒据巨具距踞锯俱句惧炬剧倨讵苣苴莒掬遽屦琚枸椐榘榉橘犋飓钜锔窭裾趄醵踽龃雎鞫", juan: "捐鹃娟倦眷卷绢鄄狷涓桊蠲锩镌隽", jue: "撅攫抉掘倔爵觉决诀绝厥劂谲矍蕨噘崛獗孓珏桷橛爝镢蹶觖", jun: "均菌钧军君峻俊竣浚郡骏捃狻皲筠麇", ka: "喀咖卡佧咔胩", ke: "咯坷苛柯棵磕颗科壳咳可渴克刻客课岢恪溘骒缂珂轲氪瞌钶疴窠蝌髁", kai: "开揩楷凯慨剀垲蒈忾恺铠锎", kan: "刊堪勘坎砍看侃凵莰莶戡龛瞰", kang: "康慷糠扛抗亢炕坑伉闶钪", kao: "考拷烤靠尻栲犒铐", ken: "肯啃垦恳垠裉颀", keng: "吭忐铿", kong: "空恐孔控倥崆箜", kou: "抠口扣寇芤蔻叩眍筘", ku: "枯哭窟苦酷库裤刳堀喾绔骷", kua: "夸垮挎跨胯侉", kuai: "块筷侩快蒯郐蒉狯脍", kuan: "宽款髋", kuang: "匡筐狂框矿眶旷况诓诳邝圹夼哐纩贶", kui: "亏盔岿窥葵奎魁傀馈愧溃馗匮夔隗揆喹喟悝愦阕逵暌睽聩蝰篑臾跬", kun: "坤昆捆困悃阃琨锟醌鲲髡", kuo: "括扩廓阔蛞", la: "垃拉喇蜡腊辣啦剌摺邋旯砬瘌", lai: "莱来赖崃徕涞濑赉睐铼癞籁", lan: "蓝婪栏拦篮阑兰澜谰揽览懒缆烂滥啉岚懔漤榄斓罱镧褴", lang: "琅榔狼廊郎朗浪莨蒗啷阆锒稂螂", lao: "捞劳牢老佬姥酪烙涝唠崂栳铑铹痨醪", le: "勒乐肋仂叻嘞泐鳓", lei: "雷镭蕾磊累儡垒擂类泪羸诔荽咧漯嫘缧檑耒酹", ling: "棱冷拎玲菱零龄铃伶羚凌灵陵岭领另令酃塄苓呤囹泠绫柃棂瓴聆蛉翎鲮", leng: "楞愣", li: "厘梨犁黎篱狸离漓理李里鲤礼莉荔吏栗丽厉励砾历利傈例俐痢立粒沥隶力璃哩俪俚郦坜苈莅蓠藜捩呖唳喱猁溧澧逦娌嫠骊缡珞枥栎轹戾砺詈罹锂鹂疠疬蛎蜊蠡笠篥粝醴跞雳鲡鳢黧", lian: "俩联莲连镰廉怜涟帘敛脸链恋炼练挛蔹奁潋濂娈琏楝殓臁膦裢蠊鲢", liang: "粮凉梁粱良两辆量晾亮谅墚椋踉靓魉", liao: "撩聊僚疗燎寥辽潦了撂镣廖料蓼尥嘹獠寮缭钌鹩耢", lie: "列裂烈劣猎冽埒洌趔躐鬣", lin: "琳林磷霖临邻鳞淋凛赁吝蔺嶙廪遴檩辚瞵粼躏麟", liu: "溜琉榴硫馏留刘瘤流柳六抡偻蒌泖浏遛骝绺旒熘锍镏鹨鎏", "long": "龙聋咙笼窿隆垄拢陇弄垅茏泷珑栊胧砻癃", lou: "楼娄搂篓漏陋喽嵝镂瘘耧蝼髅", lu: "芦卢颅庐炉掳卤虏鲁麓碌露路赂鹿潞禄录陆戮垆摅撸噜泸渌漉璐栌橹轳辂辘氇胪镥鸬鹭簏舻鲈", lv: "驴吕铝侣旅履屡缕虑氯律率滤绿捋闾榈膂稆褛", luan: "峦孪滦卵乱栾鸾銮", lue: "掠略锊", lun: "轮伦仑沦纶论囵", luo: "萝螺罗逻锣箩骡裸落洛骆络倮荦摞猡泺椤脶镙瘰雒", ma: "妈麻玛码蚂马骂嘛吗唛犸嬷杩麽", mai: "埋买麦卖迈脉劢荬咪霾", man: "瞒馒蛮满蔓曼慢漫谩墁幔缦熳镘颟螨鳗鞔", mang: "芒茫盲忙莽邙漭朦硭蟒", meng: "氓萌蒙檬盟锰猛梦孟勐甍瞢懵礞虻蜢蠓艋艨黾", miao: "猫苗描瞄藐秒渺庙妙喵邈缈缪杪淼眇鹋蜱", mao: "茅锚毛矛铆卯茂冒帽貌贸侔袤勖茆峁瑁昴牦耄旄懋瞀蛑蝥蟊髦", me: "么", mei: "玫枚梅酶霉煤没眉媒镁每美昧寐妹媚坶莓嵋猸浼湄楣镅鹛袂魅", men: "门闷们扪玟焖懑钔", mi: "眯醚靡糜迷谜弥米秘觅泌蜜密幂芈冖谧蘼嘧猕獯汨宓弭脒敉糸縻麋", mian: "棉眠绵冕免勉娩缅面沔湎腼眄", mie: "蔑灭咩蠛篾", min: "民抿皿敏悯闽苠岷闵泯珉", ming: "明螟鸣铭名命冥茗溟暝瞑酩", miu: "谬", mo: "摸摹蘑模膜磨摩魔抹末莫墨默沫漠寞陌谟茉蓦馍嫫镆秣瘼耱蟆貊貘", mou: "谋牟某厶哞婺眸鍪", mu: "拇牡亩姆母墓暮幕募慕木目睦牧穆仫苜呒沐毪钼", na: "拿哪呐钠那娜纳内捺肭镎衲箬", nai: "氖乃奶耐奈鼐艿萘柰", nan: "南男难囊喃囡楠腩蝻赧", nao: "挠脑恼闹孬垴猱瑙硇铙蛲", ne: "淖呢讷", nei: "馁", nen: "嫩能枘恁", ni: "妮霓倪泥尼拟你匿腻逆溺伲坭猊怩滠昵旎祢慝睨铌鲵", nian: "蔫拈年碾撵捻念廿辇黏鲇鲶", niang: "娘酿", niao: "鸟尿茑嬲脲袅", nie: "捏聂孽啮镊镍涅乜陧蘖嗫肀颞臬蹑", nin: "您柠", ning: "狞凝宁拧泞佞蓥咛甯聍", niu: "牛扭钮纽狃忸妞蚴", nong: "脓浓农侬", nu: "奴努怒呶帑弩胬孥驽", nv: "女恧钕衄", nuan: "暖", nuenue: "虐", nue: "疟谑", nuo: "挪懦糯诺傩搦喏锘", ou: "哦欧鸥殴藕呕偶沤怄瓯耦", pa: "啪趴爬帕怕琶葩筢", pai: "拍排牌徘湃派俳蒎", pan: "攀潘盘磐盼畔判叛爿泮袢襻蟠蹒", pang: "乓庞旁耪胖滂逄", pao: "抛咆刨炮袍跑泡匏狍庖脬疱", pei: "呸胚培裴赔陪配佩沛掊辔帔淠旆锫醅霈", pen: "喷盆湓", peng: "砰抨烹澎彭蓬棚硼篷膨朋鹏捧碰坯堋嘭怦蟛", pi: "砒霹批披劈琵毗啤脾疲皮匹痞僻屁譬丕陴邳郫圮鼙擗噼庀媲纰枇甓睥罴铍痦癖疋蚍貔", pian: "篇偏片骗谝骈犏胼褊翩蹁", piao: "飘漂瓢票剽嘌嫖缥殍瞟螵", pie: "撇瞥丿苤氕", pin: "拼频贫品聘拚姘嫔榀牝颦", ping: "乒坪苹萍平凭瓶评屏俜娉枰鲆", po: "坡泼颇婆破魄迫粕叵鄱溥珀钋钷皤笸", pou: "剖裒踣", pu: "扑铺仆莆葡菩蒲埔朴圃普浦谱曝瀑匍噗濮璞氆镤镨蹼", qi: "期欺栖戚妻七凄漆柒沏其棋奇歧畦崎脐齐旗祈祁骑起岂乞企启契砌器气迄弃汽泣讫亟亓圻芑萋葺嘁屺岐汔淇骐绮琪琦杞桤槭欹祺憩碛蛴蜞綦綮趿蹊鳍麒", qia: "掐恰洽葜", qian: "牵扦钎铅千迁签仟谦乾黔钱钳前潜遣浅谴堑嵌欠歉佥阡芊芡荨掮岍悭慊骞搴褰缱椠肷愆钤虔箝", qiang: "枪呛腔羌墙蔷强抢嫱樯戗炝锖锵镪襁蜣羟跫跄", qiao: "橇锹敲悄桥瞧乔侨巧鞘撬翘峭俏窍劁诮谯荞愀憔缲樵毳硗跷鞒", qie: "切茄且怯窃郄唼惬妾挈锲箧", qin: "钦侵亲秦琴勤芹擒禽寝沁芩蓁蕲揿吣嗪噙溱檎螓衾", qing: "青轻氢倾卿清擎晴氰情顷请庆倩苘圊檠磬蜻罄箐謦鲭黥", qiong: "琼穷邛茕穹筇銎", qiu: "秋丘邱球求囚酋泅俅氽巯艽犰湫逑遒楸赇鸠虬蚯蝤裘糗鳅鼽", qu: "趋区蛆曲躯屈驱渠取娶龋趣去诎劬蕖蘧岖衢阒璩觑氍祛磲癯蛐蠼麴瞿黢", quan: "圈颧权醛泉全痊拳犬券劝诠荃獾悛绻辁畎铨蜷筌鬈", que: "缺炔瘸却鹊榷确雀阙悫", qun: "裙群逡", ran: "然燃冉染苒髯", rang: "瓤壤攘嚷让禳穰", rao: "饶扰绕荛娆桡", ruo: "惹若弱", re: "热偌", ren: "壬仁人忍韧任认刃妊纫仞荏葚饪轫稔衽", reng: "扔仍", ri: "日", rong: "戎茸蓉荣融熔溶容绒冗嵘狨缛榕蝾", rou: "揉柔肉糅蹂鞣", ru: "茹蠕儒孺如辱乳汝入褥蓐薷嚅洳溽濡铷襦颥", ruan: "软阮朊", rui: "蕊瑞锐芮蕤睿蚋", run: "闰润", sa: "撒洒萨卅仨挲飒", sai: "腮鳃塞赛噻", san: "三叁伞散彡馓氵毵糁霰", sang: "桑嗓丧搡磉颡", sao: "搔骚扫嫂埽臊瘙鳋", se: "瑟色涩啬铩铯穑", sen: "森", seng: "僧", sha: "莎砂杀刹沙纱傻啥煞脎歃痧裟霎鲨", shai: "筛晒酾", shan: "珊苫杉山删煽衫闪陕擅赡膳善汕扇缮剡讪鄯埏芟潸姗骟膻钐疝蟮舢跚鳝", shang: "墒伤商赏晌上尚裳垧绱殇熵觞", shao: "梢捎稍烧芍勺韶少哨邵绍劭苕潲蛸笤筲艄", she: "奢赊蛇舌舍赦摄射慑涉社设厍佘猞畲麝", shen: "砷申呻伸身深娠绅神沈审婶甚肾慎渗诜谂吲哂渖椹矧蜃", sheng: "声生甥牲升绳省盛剩胜圣丞渑媵眚笙", shi: "师失狮施湿诗尸虱十石拾时什食蚀实识史矢使屎驶始式示士世柿事拭誓逝势是嗜噬适仕侍释饰氏市恃室视试谥埘莳蓍弑唑饣轼耆贳炻礻铈铊螫舐筮豕鲥鲺", shou: "收手首守寿授售受瘦兽扌狩绶艏", shu: "蔬枢梳殊抒输叔舒淑疏书赎孰熟薯暑曙署蜀黍鼠属术述树束戍竖墅庶数漱恕倏塾菽忄沭涑澍姝纾毹腧殳镯秫鹬", shua: "刷耍唰涮", shuai: "摔衰甩帅蟀", shuan: "栓拴闩", shuang: "霜双爽孀", shui: "谁水睡税", shun: "吮瞬顺舜恂", shuo: "说硕朔烁蒴搠嗍濯妁槊铄", si: "斯撕嘶思私司丝死肆寺嗣四伺似饲巳厮俟兕菥咝汜泗澌姒驷缌祀祠锶鸶耜蛳笥", song: "松耸怂颂送宋讼诵凇菘崧嵩忪悚淞竦", sou: "搜艘擞嗽叟嗖嗾馊溲飕瞍锼螋", su: "苏酥俗素速粟僳塑溯宿诉肃夙谡蔌嗉愫簌觫稣", suan: "酸蒜算", sui: "虽隋随绥髓碎岁穗遂隧祟蓑冫谇濉邃燧眭睢", sun: "孙损笋荪狲飧榫跣隼", suo: "梭唆缩琐索锁所唢嗦娑桫睃羧", ta: "塌他它她塔獭挞蹋踏闼溻遢榻沓", tai: "胎苔抬台泰酞太态汰邰薹肽炱钛跆鲐", tan: "坍摊贪瘫滩坛檀痰潭谭谈坦毯袒碳探叹炭郯蕈昙钽锬覃", tang: "汤塘搪堂棠膛唐糖傥饧溏瑭铴镗耥螗螳羰醣", thang: "倘躺淌", theng: "趟烫", tao: "掏涛滔绦萄桃逃淘陶讨套挑鼗啕韬饕", te: "特", teng: "藤腾疼誊滕", ti: "梯剔踢锑提题蹄啼体替嚏惕涕剃屉荑悌逖绨缇鹈裼醍", tian: "天添填田甜恬舔腆掭忝阗殄畋钿蚺", tiao: "条迢眺跳佻祧铫窕龆鲦", tie: "贴铁帖萜餮", ting: "厅听烃汀廷停亭庭挺艇莛葶婷梃蜓霆", tong: "通桐酮瞳同铜彤童桶捅筒统痛佟僮仝茼嗵恸潼砼", tou: "偷投头透亠", tu: "凸秃突图徒途涂屠土吐兔堍荼菟钍酴", tuan: "湍团疃", tui: "推颓腿蜕褪退忒煺", tun: "吞屯臀饨暾豚窀", tuo: "拖托脱鸵陀驮驼椭妥拓唾乇佗坨庹沱柝砣箨舄跎鼍", wa: "挖哇蛙洼娃瓦袜佤娲腽", wai: "歪外", wan: "豌弯湾玩顽丸烷完碗挽晚皖惋宛婉万腕剜芄苋菀纨绾琬脘畹蜿箢", wang: "汪王亡枉网往旺望忘妄罔尢惘辋魍", wei: "威巍微危韦违桅围唯惟为潍维苇萎委伟伪尾纬未蔚味畏胃喂魏位渭谓尉慰卫倭偎诿隈葳薇帏帷崴嵬猥猬闱沩洧涠逶娓玮韪軎炜煨熨痿艉鲔", wen: "瘟温蚊文闻纹吻稳紊问刎愠阌汶璺韫殁雯", weng: "嗡翁瓮蓊蕹", wo: "挝蜗涡窝我斡卧握沃莴幄渥杌肟龌", wu: "巫呜钨乌污诬屋无芜梧吾吴毋武五捂午舞伍侮坞戊雾晤物勿务悟误兀仵阢邬圬芴庑怃忤浯寤迕妩骛牾焐鹉鹜蜈鋈鼯", xi: "昔熙析西硒矽晰嘻吸锡牺稀息希悉膝夕惜熄烯溪汐犀檄袭席习媳喜铣洗系隙戏细僖兮隰郗茜葸蓰奚唏徙饩阋浠淅屣嬉玺樨曦觋欷熹禊禧钸皙穸蜥蟋舾羲粞翕醯鼷", xia: "瞎虾匣霞辖暇峡侠狭下厦夏吓掀葭嗄狎遐瑕硖瘕罅黠", xian: "锨先仙鲜纤咸贤衔舷闲涎弦嫌显险现献县腺馅羡宪陷限线冼藓岘猃暹娴氙祆鹇痫蚬筅籼酰跹", xiang: "相厢镶香箱襄湘乡翔祥详想响享项巷橡像向象芗葙饷庠骧缃蟓鲞飨", xiao: "萧硝霄削哮嚣销消宵淆晓小孝校肖啸笑效哓咻崤潇逍骁绡枭枵筱箫魈", xie: "楔些歇蝎鞋协挟携邪斜胁谐写械卸蟹懈泄泻谢屑偕亵勰燮薤撷廨瀣邂绁缬榭榍歙躞", xin: "薪芯锌欣辛新忻心信衅囟馨莘歆铽鑫", xing: "星腥猩惺兴刑型形邢醒幸杏性姓陉荇荥擤悻硎", xiong: "兄凶胸匈汹雄熊芎", xiu: "休修羞朽嗅锈秀袖绣莠岫馐庥鸺貅髹", xu: "墟戌需虚嘘须徐许蓄酗叙旭序畜恤絮婿绪续讴诩圩蓿怵洫溆顼栩煦砉盱胥糈醑", xuan: "轩喧宣悬旋玄选癣眩绚儇谖萱揎馔泫洵渲漩璇楦暄炫煊碹铉镟痃", xue: "靴薛学穴雪血噱泶鳕", xun: "勋熏循旬询寻驯巡殉汛训讯逊迅巽埙荀薰峋徇浔曛窨醺鲟", ya: "压押鸦鸭呀丫芽牙蚜崖衙涯雅哑亚讶伢揠吖岈迓娅琊桠氩砑睚痖", yan: "焉咽阉烟淹盐严研蜒岩延言颜阎炎沿奄掩眼衍演艳堰燕厌砚雁唁彦焰宴谚验厣靥赝俨偃兖讠谳郾鄢芫菸崦恹闫阏洇湮滟妍嫣琰晏胭腌焱罨筵酽魇餍鼹", yang: "殃央鸯秧杨扬佯疡羊洋阳氧仰痒养样漾徉怏泱炀烊恙蛘鞅", yao: "邀腰妖瑶摇尧遥窑谣姚咬舀药要耀夭爻吆崾徭瀹幺珧杳曜肴鹞窈繇鳐", ye: "椰噎耶爷野冶也页掖业叶曳腋夜液谒邺揶馀晔烨铘", yi: "一壹医揖铱依伊衣颐夷遗移仪胰疑沂宜姨彝椅蚁倚已乙矣以艺抑易邑屹亿役臆逸肄疫亦裔意毅忆义益溢诣议谊译异翼翌绎刈劓佾诒圪圯埸懿苡薏弈奕挹弋呓咦咿噫峄嶷猗饴怿怡悒漪迤驿缢殪贻旖熠钇镒镱痍瘗癔翊衤蜴舣羿翳酏黟", yin: "茵荫因殷音阴姻吟银淫寅饮尹引隐印胤鄞堙茚喑狺夤氤铟瘾蚓霪龈", ying: "英樱婴鹰应缨莹萤营荧蝇迎赢盈影颖硬映嬴郢茔莺萦撄嘤膺滢潆瀛瑛璎楹鹦瘿颍罂", yo: "哟唷", yong: "拥佣臃痈庸雍踊蛹咏泳涌永恿勇用俑壅墉慵邕镛甬鳙饔", you: "幽优悠忧尤由邮铀犹油游酉有友右佑釉诱又幼卣攸侑莸呦囿宥柚猷牖铕疣蝣鱿黝鼬", yu: "迂淤于盂榆虞愚舆余俞逾鱼愉渝渔隅予娱雨与屿禹宇语羽玉域芋郁吁遇喻峪御愈欲狱育誉浴寓裕预豫驭禺毓伛俣谀谕萸蓣揄喁圄圉嵛狳饫庾阈妪妤纡瑜昱觎腴欤於煜燠聿钰鹆瘐瘀窳蝓竽舁雩龉", yuan: "鸳渊冤元垣袁原援辕园员圆猿源缘远苑愿怨院塬沅媛瑗橼爰眢鸢螈鼋", yue: "曰约越跃钥岳粤月悦阅龠樾刖钺", yun: "耘云郧匀陨允运蕴酝晕韵孕郓芸狁恽纭殒昀氲", za: "匝砸杂拶咂", zai: "栽哉灾宰载再在咱崽甾", zan: "攒暂赞瓒昝簪糌趱錾", zang: "赃脏葬奘戕臧", zao: "遭糟凿藻枣早澡蚤躁噪造皂灶燥唣缫", ze: "责择则泽仄赜啧迮昃笮箦舴", zei: "贼", zen: "怎谮", zeng: "增憎曾赠缯甑罾锃", zha: "扎喳渣札轧铡闸眨栅榨咋乍炸诈揸吒咤哳怍砟痄蚱齄", zhai: "摘斋宅窄债寨砦", zhan: "瞻毡詹粘沾盏斩辗崭展蘸栈占战站湛绽谵搌旃", zhang: "樟章彰漳张掌涨杖丈帐账仗胀瘴障仉鄣幛嶂獐嫜璋蟑", zhao: "招昭找沼赵照罩兆肇召爪诏棹钊笊", zhe: "遮折哲蛰辙者锗蔗这浙谪陬柘辄磔鹧褚蜇赭", zhen: "珍斟真甄砧臻贞针侦枕疹诊震振镇阵缜桢榛轸赈胗朕祯畛鸩", zheng: "蒸挣睁征狰争怔整拯正政帧症郑证诤峥钲铮筝", zhi: "芝枝支吱蜘知肢脂汁之织职直植殖执值侄址指止趾只旨纸志挚掷至致置帜峙制智秩稚质炙痔滞治窒卮陟郅埴芷摭帙忮彘咫骘栉枳栀桎轵轾攴贽膣祉祗黹雉鸷痣蛭絷酯跖踬踯豸觯", zhong: "中盅忠钟衷终种肿重仲众冢锺螽舂舯踵", zhou: "舟周州洲诌粥轴肘帚咒皱宙昼骤啄着倜诹荮鬻纣胄碡籀舳酎鲷", zhu: "珠株蛛朱猪诸诛逐竹烛煮拄瞩嘱主著柱助蛀贮铸筑住注祝驻伫侏邾苎茱洙渚潴驺杼槠橥炷铢疰瘃蚰竺箸翥躅麈", zhua: "抓", zhuai: "拽", zhuan: "专砖转撰赚篆抟啭颛", zhuang: "桩庄装妆撞壮状丬", zhui: "椎锥追赘坠缀萑骓缒", zhun: "谆准", zhuo: "捉拙卓桌琢茁酌灼浊倬诼廴蕞擢啜浞涿杓焯禚斫", zi: "兹咨资姿滋淄孜紫仔籽滓子自渍字谘嵫姊孳缁梓辎赀恣眦锱秭耔笫粢觜訾鲻髭", zong: "鬃棕踪宗综总纵腙粽", zou: "邹走奏揍鄹鲰", zu: "租足卒族祖诅阻组俎菹啐徂驵蹴", zuan: "钻纂攥缵", zui: "嘴醉最罪", zun: "尊遵撙樽鳟", zuo: "昨左佐柞做作坐座阝阼胙祚酢", cou: "薮楱辏腠", nang: "攮哝囔馕曩", o: "喔", dia: "嗲", chuai: "嘬膪踹", cen: "岑涔", diu: "铥", nou: "耨", fou: "缶", bia: "髟" }, this.polyphone = { 19969: "DZ", 19975: "WM", 19988: "QJ", 20048: "YL", 20056: "SC", 20060: "NM", 20094: "QG", 20127: "QJ", 20167: "QC", 20193: "YG", 20250: "KH", 20256: "ZC", 20282: "SC", 20285: "QJG", 20291: "TD", 20314: "YD", 20340: "NE", 20375: "TD", 20389: "YJ", 20391: "CZ", 20415: "PB", 20446: "YS", 20447: "SQ", 20504: "TC", 20608: "KG", 20854: "QJ", 20857: "ZC", 20911: "PF", 20504: "TC", 20608: "KG", 20854: "QJ", 20857: "ZC", 20911: "PF", 20985: "AW", 21032: "PB", 21048: "XQ", 21049: "SC", 21089: "YS", 21119: "JC", 21242: "SB", 21273: "SC", 21305: "YP", 21306: "QO", 21330: "ZC", 21333: "SDC", 21345: "QK", 21378: "CA", 21397: "SC", 21414: "XS", 21442: "SC", 21477: "JG", 21480: "TD", 21484: "ZS", 21494: "YX", 21505: "YX", 21512: "HG", 21523: "XH", 21537: "PB", 21542: "PF", 21549: "KH", 21571: "E", 21574: "DA", 21588: "TD", 21589: "O", 21618: "ZC", 21621: "KHA", 21632: "ZJ", 21654: "KG", 21679: "LKG", 21683: "KH", 21710: "A", 21719: "YH", 21734: "WOE", 21769: "A", 21780: "WN", 21804: "XH", 21834: "A", 21899: "ZD", 21903: "RN", 21908: "WO", 21939: "ZC", 21956: "SA", 21964: "YA", 21970: "TD", 22003: "A", 22031: "JG", 22040: "XS", 22060: "ZC", 22066: "ZC", 22079: "MH", 22129: "XJ", 22179: "XA", 22237: "NJ", 22244: "TD", 22280: "JQ", 22300: "YH", 22313: "XW", 22331: "YQ", 22343: "YJ", 22351: "PH", 22395: "DC", 22412: "TD", 22484: "PB", 22500: "PB", 22534: "ZD", 22549: "DH", 22561: "PB", 22612: "TD", 22771: "KQ", 22831: "HB", 22841: "JG", 22855: "QJ", 22865: "XQ", 23013: "ML", 23081: "WM", 23487: "SX", 23558: "QJ", 23561: "YW", 23586: "YW", 23614: "YW", 23615: "SN", 23631: "PB", 23646: "ZS", 23663: "ZT", 23673: "YG", 23762: "TD", 23769: "ZS", 23780: "QJ", 23884: "QK", 24055: "XH", 24113: "DC", 24162: "ZC", 24191: "GA", 24273: "QJ", 24324: "NL", 24377: "TD", 24378: "QJ", 24439: "PF", 24554: "ZS", 24683: "TD", 24694: "WE", 24733: "LK", 24925: "TN", 25094: "ZG", 25100: "XQ", 25103: "XH", 25153: "PB", 25170: "PB", 25179: "KG", 25203: "PB", 25240: "ZS", 25282: "FB", 25303: "NA", 25324: "KG", 25341: "ZY", 25373: "WZ", 25375: "XJ", 25384: "A", 25457: "A", 25528: "SD", 25530: "SC", 25552: "TD", 25774: "ZC", 25874: "ZC", 26044: "YW", 26080: "WM", 26292: "PB", 26333: "PB", 26355: "ZY", 26366: "CZ", 26397: "ZC", 26399: "QJ", 26415: "ZS", 26451: "SB", 26526: "ZC", 26552: "JG", 26561: "TD", 26588: "JG", 26597: "CZ", 26629: "ZS", 26638: "YL", 26646: "XQ", 26653: "KG", 26657: "XJ", 26727: "HG", 26894: "ZC", 26937: "ZS", 26946: "ZC", 26999: "KJ", 27099: "KJ", 27449: "YQ", 27481: "XS", 27542: "ZS", 27663: "ZS", 27748: "TS", 27784: "SC", 27788: "ZD", 27795: "TD", 27812: "O", 27850: "PB", 27852: "MB", 27895: "SL", 27898: "PL", 27973: "QJ", 27981: "KH", 27986: "HX", 27994: "XJ", 28044: "YC", 28065: "WG", 28177: "SM", 28267: "QJ", 28291: "KH", 28337: "ZQ", 28463: "TL", 28548: "DC", 28601: "TD", 28689: "PB", 28805: "JG", 28820: "QG", 28846: "PB", 28952: "TD", 28975: "ZC", 29100: "A", 29325: "QJ", 29575: "SL", 29602: "FB", 30010: "TD", 30044: "CX", 30058: "PF", 30091: "YSP", 30111: "YN", 30229: "XJ", 30427: "SC", 30465: "SX", 30631: "YQ", 30655: "QJ", 30684: "QJG", 30707: "SD", 30729: "XH", 30796: "LG", 30917: "PB", 31074: "NM", 31085: "JZ", 31109: "SC", 31181: "ZC", 31192: "MLB", 31293: "JQ", 31400: "YX", 31584: "YJ", 31896: "ZN", 31909: "ZY", 31995: "XJ", 32321: "PF", 32327: "ZY", 32418: "HG", 32420: "XQ", 32421: "HG", 32438: "LG", 32473: "GJ", 32488: "TD", 32521: "QJ", 32527: "PB", 32562: "ZSQ", 32564: "JZ", 32735: "ZD", 32793: "PB", 33071: "PF", 33098: "XL", 33100: "YA", 33152: "PB", 33261: "CX", 33324: "BP", 33333: "TD", 33406: "YA", 33426: "WM", 33432: "PB", 33445: "JG", 33486: "ZN", 33493: "TS", 33507: "QJ", 33540: "QJ", 33544: "ZC", 33564: "XQ", 33617: "YT", 33632: "QJ", 33636: "XH", 33637: "YX", 33694: "WG", 33705: "PF", 33728: "YW", 33882: "SR", 34067: "WM", 34074: "YW", 34121: "QJ", 34255: "ZC", 34259: "XL", 34425: "JH", 34430: "XH", 34485: "KH", 34503: "YS", 34532: "HG", 34552: "XS", 34558: "YE", 34593: "ZL", 34660: "YQ", 34892: "XH", 34928: "SC", 34999: "QJ", 35048: "PB", 35059: "SC", 35098: "ZC", 35203: "TQ", 35265: "JX", 35299: "JX", 35782: "SZ", 35828: "YS", 35830: "E", 35843: "TD", 35895: "YG", 35977: "MH", 36158: "JG", 36228: "QJ", 36426: "XQ", 36466: "DC", 36710: "JC", 36711: "ZYG", 36767: "PB", 36866: "SK", 36951: "YW", 37034: "YX", 37063: "XH", 37218: "ZC", 37325: "ZC", 38063: "PB", 38079: "TD", 38085: "QY", 38107: "DC", 38116: "TD", 38123: "YD", 38224: "HG", 38241: "XTC", 38271: "ZC", 38415: "YE", 38426: "KH", 38461: "YD", 38463: "AE", 38466: "PB", 38477: "XJ", 38518: "YT", 38551: "WK", 38585: "ZC", 38704: "XS", 38739: "LJ", 38761: "GJ", 38808: "SQ", 39048: "JG", 39049: "XJ", 39052: "HG", 39076: "CZ", 39271: "XT", 39534: "TD", 39552: "TD", 39584: "PB", 39647: "SB", 39730: "LG", 39748: "TPB", 40109: "ZQ", 40479: "ND", 40516: "HG", 40536: "HG", 40583: "QJ", 40765: "YQ", 40784: "QJ", 40840: "YK", 40863: "QJG" };
    }, Y.prototype = { constructor: Y }, Y.prototype.getCamelChars = function (Y) {
        if ("string" != typeof Y)
            throw new Error(-1, "函数getFisrt需要字符串类型参数!");
        for (var Z = new Array, L = 0, X = Y.length; X > L; L++) {
            var J = Y.charAt(L);
            Z.push(this._getChar(J));
        }
        return this._getResult(Z);
    }, Y.prototype.getFullChars = function (Y) {
        for (var Z = Y.length, L = "", X = (new RegExp("[a-zA-Z0-9- ]"), 0); Z > X; X++) {
            var J = Y.substr(X, 1), S = J.charCodeAt(0);
            if (S > 40869 || 19968 > S)
                L += J;
            else {
                var C = this._getFullChar(J);
                C !== !1 && (L += C);
            }
        }
        return L;
    }, Y.prototype._getFullChar = function (Y) {
        for (var Z in this.full_dict)
            if (-1 != this.full_dict[Z].indexOf(Y))
                return this._capitalize(Z);
        return "";
    }, Y.prototype._capitalize = function (Y) {
        if (Y.length > 0) {
            var Z = Y.substr(0, 1).toUpperCase(), L = Y.substr(1, Y.length);
            return Z + L;
        }
    }, Y.prototype._getChar = function (Y) {
        var Z = Y.charCodeAt(0);
        return Z > 40869 || 19968 > Z ? Y : this.options.checkPolyphone && this.polyphone[Z] ? this.polyphone[Z] : this.char_dict.charAt(Z - 19968);
    }, Y.prototype._getResult = function (Y) {
        if (!this.options.checkPolyphone)
            return Y.join("");
        for (var Z = [""], L = 0, X = Y.length; X > L; L++) {
            var J = Y[L], S = J.length;
            if (1 == S)
                for (var C = 0; C < Z.length; C++)
                    Z[C] += J;
            else {
                var Q = Z.slice(0);
                Z = [];
                for (var C = 0; S > C; C++) {
                    for (var H = Q.slice(0), T = 0; T < H.length; T++)
                        H[T] += J.charAt(C);
                    Z = Z.concat(H);
                }
            }
        }
        return Z;
    };
}();
;
angular.module("routeHelper", []).factory("$routeHelper", ["$location", "$cacheFactory", "$routeParams", "$log", "$globalData", "$rootScope", "$document", function (e, r, a, n, t) {
        function u(r, a) {
            $ && h.unshift(e.url()), n.debug("[$routeHelper jump " + r + "]:");
            var u = r;
            if (a) {
                n.debug(a);
                var o = t.getRandomStr();
                s.put(o, a), r += "/" + o;
            }
            e.path(r), _hmt.push(["_trackPageview", u]);
        }
        function o(e) {
            var r = {}, n = a[p];
            return angular.isDefined(n) && (r = s.get(n), angular.isDefined(r) || l()), r = angular.extend({}, a, r), void 0 === e ? r : r[e];
        }
        function i(e) {
            var r = {}, n = a[p];
            return angular.isDefined(n) && (r = s.get(n), angular.isDefined(r) || (e ? e() : l())), r = angular.extend({}, a, r);
        }
        function c() {
            if ($) {
                var r = h.shift();
                e.url(r);
            } else
                window.history.back();
        }
        function l() {
            u("/home");
        }
        function g(e, r) {
            u("/error", { ec: e, em: r });
        }
        function m(e) {
            var r = {}, n = (e || a)[p];
            return angular.isDefined(n) && (r = s.get(n)), r;
        }
        function f() {
            s.removeAll();
        }
        function d() {
            var r = e.search();
            return r;
        }
        var s = r("RouteCache"), h = [], p = "paramsid", v = document.createElement("b");
        v.innerHTML = "<!--[if lte IE 7]><i></i><![endif]-->";
        var $ = 1 === v.getElementsByTagName("i").length;
        return { jump: u, getJumpParams: o, getJumpParamsForward: i, jumpBack: c, jumpHome: l, defaultErrorShow: g, clearCache: f, getUrlParams: d, getParamsCache: m };
    }]);
;
angular.module("services", ["globalData", "httpPlus", "appConfig", "routeHelper", "utb", "message", "pinyin", "loginFlow", "cookie", "iss"]);
;
angular.module("utb", []).factory("$utb", ["$globalData", "$filter", "$httpPlus", "$q", "$log", "$appConfig", "$message", function (e, n, t, r, a, o, s) {
        function i(n) {
            var t = n, r = 0;
            if (n && (r = n.length) >= 9) {
                for (var a = "0", o = e.getSessionContext(x.FIELD_ICOLL_USERSETTINGS_INFO), s = o ? o.length : 0, i = 0; s > i; i++) {
                    var u = o[i], C = u[x.FIELD_ICOLL_USERSETTINGS_SETTINGNAME];
                    if ("PROTECT_FLAG" == C) {
                        a = u[x.FIELD_ICOLL_USERSETTINGS_SETTINGVALUE];
                        break;
                    }
                }
                var _ = "1" == a;
                _ && (t = n.substring(0, r - 9) + "*****" + n.substring(r - 4));
            }
            return t;
        }
        function u() {
            var n = [], t = e.getSessionContext(x.FIELD_ICOLL_ACCOUNT_INFO);
            if (!t)
                return n;
            for (var r = t.length, a = 0; r > a; a++) {
                var o = t[a], s = o[x.FIELD_ICOLL_ACCOUNT_TYPE], u = "", C = "";
                if ("CR" == s || "DP" == s) {
                    var _ = o[x.FIELD_ICOLL_ACCOUNT_NO], c = o[x.FIELD_CURRENCY_TYPE], l = o[x.FIELD_ACCOUNT_ALIAS];
                    u = _ + "|" + s + "|" + c, C = i(_), l && l.length > 0 && (C += "[" + l + "]"), n.push({ value: u, title: C });
                }
            }
            return n;
        }
        function C(e) {
            var n = e[x.FIELD_ICOLL_ACCOUNT_NO], t = e[x.FIELD_ICOLL_ACCOUNT_TYPE], r = e[x.FIELD_CURRENCY_TYPE], a = e[x.FIELD_ICOLL_ACCOUNT_OPENNODE], o = e[x.FIELD_ACCOUNT_ALIAS], s = n + "|" + t + "|" + r + "|" + a, u = i(n);
            return o && o.length > 0 && (u += "[" + o + "]"), { value: s, title: u };
        }
        function _(n) {
            var t = [], r = e.getSessionContext(x.FIELD_CUSTOMER_TYPE), a = e.getSessionContext(x.FIELD_ICOLL_ACCOUNT_INFO);
            if (!a)
                return t;
            if ("11" == r)
                for (var o = a.length, s = 0; o > s; s++) {
                    var u = a[s][x.FIELD_ICOLL_ACCOUNT_NO];
                    t.push({ value: u, title: i(u) });
                }
            else
                for (var o = a.length, s = 0; o > s; s++) {
                    var _ = a[s], c = _[x.FIELD_ICOLL_ACCOUNT_TYPE];
                    if ("undefined" != typeof n) {
                        if (n.indexOf(c) < 0)
                            continue;
                    } else {
                        if (null == c || "CR" == c)
                            continue;
                        if ("01" != r) {
                            var l = _[x.FIELD_ICOLL_ACCOUNT_SIGNFLAG];
                            if (!l || "0" == l)
                                continue;
                        }
                    }
                    t.push(C(_));
                }
            return t;
        }
        function c(n) {
            var t = [], r = e.getSessionContext(x.FIELD_CUSTOMER_TYPE), a = e.getSessionContext(x.FIELD_ICOLL_ACCOUNT_INFO);
            if (!a)
                return t;
            if ("11" == r)
                for (var o = a.length, s = 0; o > s; s++) {
                    var u = a[s][x.FIELD_ICOLL_ACCOUNT_NO];
                    t.push({ value: u, title: i(u) });
                }
            else
                for (var o = a.length, s = 0; o > s; s++) {
                    var _ = a[s], c = _[x.FIELD_ICOLL_ACCOUNT_TYPE];
                    if (!(n.indexOf(c) < 0)) {
                        if ("01" != r) {
                            var l = _[x.FIELD_ICOLL_ACCOUNT_SIGNFLAG];
                            if (!l || "0" == l)
                                continue;
                        }
                        t.push(C(_));
                    }
                }
            return t;
        }
        function l() {
            var n = [], t = e.getSessionContext(x.FIELD_ICOLL_EACCOUNT_INFO);
            if (!t)
                return n;
            for (var r = t.length, a = 0; r > a; a++) {
                var o = t[a];
                n.push(C(o));
            }
            return n;
        }
        function L(n) {
            var t, r = e.getSessionContext(x.FIELD_ICOLL_ACCOUNT_INFO);
            if (r) {
                t = r.length;
                for (var a = 0; t > a; a++) {
                    var o = r[a];
                    if (o[x.FIELD_ICOLL_ACCOUNT_NO] == n)
                        return o;
                }
            }
            if (r = e.getSessionContext(x.FIELD_ICOLL_EACCOUNT_INFO)) {
                t = r.length;
                for (var a = 0; t > a; a++) {
                    var o = r[a];
                    if (o[x.FIELD_ICOLL_ACCOUNT_NO] == n)
                        return o;
                }
            }
            return null;
        }
        function I(n, a, o) {
            for (var s = n.split("|"), i = r.defer(), u = [], C = e.getContextParams(), _ = 0; _ < s.length; _++) {
                var c = s[_];
                angular.isUndefined(C[c]) && u.push(c);
            }
            return 0 == u.length ? i.resolve(C) : o, i.promise;
        }
        function g(e, n) {
            var t = null;
            if (angular.isDefined(e))
                for (var r = 0; r < e.length; r++) {
                    var a = e[r];
                    if (a.key == n) {
                        t = a.value;
                        break;
                    }
                }
            return t;
        }
        function E(n, t) {
            var r, a = null, o = e.getContextParams(), s = o[n];
            if (angular.isDefined(t) || 1 != arguments.length) {
                for (var i = 0; s && i < s.length; i++)
                    if (r = s[i], r.key == t) {
                        a = r.value;
                        break;
                    }
                return a;
            }
            for (var u = s || [], C = [], _ = 0; _ < u.length; _++)
                C.push({ title: u[_].value, value: u[_].key });
            return C;
        }
        function f(n) {
            for (var a = n.split("|"), o = r.defer(), s = [], i = e.getBranchInfos(), u = 0; u < a.length; u++) {
                var C = a[u];
                angular.isUndefined(i[C]) && s.push(C);
            }
            return 0 == s.length ? o.resolve(i) : t.post("getBranchInfo.do", { appCode: s.join("|") }).success(function (e) {
                "0000" === e.ec && angular.extend(i, e.cd.branchInfo), o.resolve(i);
            }), o.promise;
        }
        function O(e, n) {
            return n = n ? n : "", e ? e : n;
        }
        function N(e) {
            return t.post("/bank/HBgetSrandNum?token=" + strToken).success(function (n) {
                "0000" == n.ec ? (window.srandNum = n.cd.mcrypt_key, e && e()) : a.debug("通讯请求加密数据失败");
            });
        }
        function D(n, r) {
            var a = o.channel, s = e.getSessionContext("sessionId"), i = {}, u = "";
            return i.WEB_CHN = a, i.timestamp = e.getTimestamp(), s && (i.EMP_SID = s), i = $.extend(i, r), u = t.transformData(i, null, { encode: !1 }), u && (u = "?" + u), o.serverPath + n + u;
        }
        function T(n, r, a) {
            var s = o.channel, i = e.getSessionContext("sessionId"), u = {}, C = {}, _ = "";
            return o.requestLocal || !o.cache ? D(n, r) : (a = a || {}, u = $.extend(u, r), u.WEB_CHN = s, C.timestamp = e.getTimestamp(), a.needLogin && i && (C.EMP_SID = i), _ = "?" + t.transformData(u, null, { encode: !1 }), _ = encodeURIComponent(_) + "?" + t.transformData(C, null, { encode: !1 }), o.serverPath + (a.cacheUrl ? a.cacheUrl : "cache") + "/" + n + _);
        }
        function d(e, n, t) {
            return t = t || {}, t.cacheUrl = "cache2", T(e, n, t);
        }
        function v(n) {
            var a = r.defer(), o = e.getAppContext("branch").value;
            return h() , a.promise;
        }
        function h() {
            return !!e.getSessionContext("sessionId");
        }
        function S(e, n) {
            s.pop({ title: "登录个人账户", src: "app/login/login.tpl.html", params: n, onClose: e, entityClassName: "loginEntity", className: "loginPop" });
        }
        function F(e) {
            return e ? !!e.__isPop : !1;
        }
        function p(e, n) {
            var t = e;
            t = 0 == n.indexOf("yyyyMMdd") ? e.substr(0, 4) + "/" + e.substr(4, 2) + "/" + e.substring(6) : e.replace(/-/g, "/");
            var r = new Date(Date.parse(t));
            if ("Invalid Date" == r || isNaN(r))
                throw new Error(e + " 无法转换为格式 " + n + " 的日期");
            return r;
        }
        function A(e, t, r, a) {
            var o = angular.isString(e);
            switch (inDate = o ? p(e, a) : new Date(e.getTime()), r = parseInt(r, 10), t) {
                case "y":
                    inDate.setFullYear(inDate.getFullYear() + r);
                    break;
                case "M":
                    inDate.setMonth(inDate.getMonth() + r);
                    break;
                case "d":
                    inDate.setDate(inDate.getDate() + r);
                    break;
                case "h":
                    inDate.setHours(inDate.getHours() + r);
                    break;
                case "m":
                    inDate.setMinutes(inDate.getMinutes() + r);
                    break;
                case "s":
                    inDate.setSeconds(inDate.getSeconds() + r);
            }
            return o && (inDate = n("date")(inDate, a)), inDate;
        }
        function U(e, n, t) {
            angular.isString(e) && (e = p(e, t)), angular.isString(n) && (n = p(n, t));
            var r = Math.abs(n.getTime() - e.getTime()), a = {};
            a.days = Math.floor(r / 864e5);
            var o = r % 864e5;
            a.hours = Math.floor(o / 36e5);
            var s = o % 36e5;
            a.minutes = Math.floor(s / 6e4);
            var i = s % 6e4;
            return a.seconds = Math.floor(i / 1e3), a;
        }
        function m(e) {
            var n = e, t = 0;
            return e && 15 == (t = e.length) && (n = e.substring(0, 4) + "********" + e.substring(12)), e && 18 == (t = e.length) && (n = e.substring(0, 4) + "********" + e.substring(14)), n;
        }
        function P(e, n, t) {
            var r, a = e ? e.length : 0, o = [];
            for (t = t || "|", r = 0; a > r; r++)
                o.push(e[r][n] || "");
            return o.join(t);
        }
        var x = { DEFAULT_STYLE_PATH: "default", USER_STYLE_FIELD: "session_stylePath", FIELD_ICOLL_ACCOUNT_INFO: "iAccountInfo", FIELD_ICOLL_ACCOUNT_NO: "accountNo", FIELD_ICOLL_ACCOUNT_TYPE: "accountType", FIELD_ICOLL_ACCOUNT_OPENNODE: "openNode", FIELD_ICOLL_ACCOUNT_SECURITY: "security", FIELD_ICOLL_ACCOUNT_SIGNFLAG: "signFlag", FIELD_BUSINESS_CODE: "currentBusinessCode", FIELD_CURRENCY_TYPE: "currencyType", FIELD_ACCOUNT_ALIAS: "accountAlias", FIELD_ICOLL_EACCOUNT_INFO: "iEAccountInfo", FIELD_CUSTOMER_ALIAS: "customerAlias", FIELD_LOGON_TYPE: "logonType", FIELD_USER_TYPE: "userType", FIELD_CUSTOMER_TYPE: "customerType", FIELD_ICOLL_USERSETTINGS_INFO: "iUserSettingsInfoCopy", FIELD_ICOLL_USERSETTINGS_SETTINGNAME: "settingName", FIELD_ICOLL_USERSETTINGS_SETTINGVALUE: "settingValue", ZH_CN: "zh_CN", FIELD_LANGUAGE: "session_language" };
        return { getMaskAccount: i, getBothAccounts: u, getAccounts: _, getSignedAccountsByType: c, getEAccounts: l, getAccountInfo: L, getContext: E, getContextParams: I, getContextParamValue: g, getBranchInfos: f, nvl: O, getSrandNum: N, getUrl: D, getCacheUrl: T, getCache2Url: d, getAdviceInfo: v, isLogin: h, login: S, isPop: F, parseDate: p, dateAdd: A, dateDistance: U, getMaskCertNo: m, concatField: P };
    }]);
