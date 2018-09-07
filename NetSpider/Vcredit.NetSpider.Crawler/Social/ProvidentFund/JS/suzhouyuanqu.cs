using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Vcredit.Common;
using Vcredit.Common.Constants;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.DataAccess.Cache;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;


namespace Vcredit.NetSpider.Crawler.Social.ProvidentFund.JS
{
    public class suzhouyuanqu : IProvidentFundCrawler
    {
        #region 公共变量
        IPluginHtmlParser HtmlParser = PluginServiceManager.GetHtmlParserPlugin();//html解析插件
        IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();//json解析插件
        IPluginSecurityCode secParser = PluginServiceManager.GetSecurityCodeParserPlugin();//验证码解析插件
        CookieCollection cookies = new CookieCollection();
        HttpHelper httpHelper = new HttpHelper();
        HttpResult httpResult = null;
        HttpItem httpItem = null;
        string baseUrl = "http://www.sipspf.org.cn/person_online/";
        string fundCity = "js_suzhouyuanqu";
        #endregion
        #region 私有变量
        string Url = string.Empty;
        string postdata = string.Empty;
        decimal payRate = (decimal)0.12;
        List<string> results = new List<string>();
        int PaymentMonths = 0;
        #endregion
        public VerCodeRes ProvidentFundInit(Entity.Service.ProvidentFund.ProvidentFundReq fundReq = null)
        {
            VerCodeRes Res = new VerCodeRes();
            string Url = string.Empty;
            string postdata = string.Empty;
            List<string> results = new List<string>();
            string token = CommonFun.GetGuidID();
            Res.Token = token;
            try
            {
                Url = baseUrl + "emp/loginend.jsp";
                httpItem = new HttpItem()
                {
                    URL = Url,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);

                Url = baseUrl + "service/identify.do?sessionid=" + cookies["JSESSIONID"].Value;
                httpItem = new HttpItem()
                {
                    URL = Url,
                    Referer = baseUrl + "emp/loginend.jsp",
                    ResultType = ResultType.Byte,
                    CookieCollection = cookies,
                    ResultCookieType = ResultCookieType.CookieCollection
                };
                httpResult = httpHelper.GetHtml(httpItem);
                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    Res.StatusDescription = ServiceConsts.StatusDescription_httpfail;
                    Res.StatusCode = ServiceConsts.StatusCode_fail;
                    return Res;
                }
                cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);


                Res.VerCodeBase64 = CommonFun.GetVercodeBase64(httpResult.ResultByte);
                //保存验证码图片在本地
                FileOperateHelper.WriteVerCodeImage(token, httpResult.ResultByte);
                Res.VerCodeUrl = CommonFun.GetVercodeUrl(token);
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitSuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;

                SpiderCacheHelper.SetCache(token, cookies);
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_InitError;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_InitError, e);
            }
            return Res;
        }

        public ProvidentFundQueryRes ProvidentFundQuery(Entity.Service.ProvidentFund.ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            ProvidentFundDetail detailRes = new ProvidentFundDetail();
            Res.ProvidentFundCity = fundCity;
            string referUrl = string.Empty;
            try
            {
                //获取缓存
                if (SpiderCacheHelper.GetCache(fundReq.Token) != null)
                {
                    cookies = (CookieCollection)SpiderCacheHelper.GetCache(fundReq.Token);
                    SpiderCacheHelper.RemoveCache(fundReq.Token);
                }
                //校验参数
                    if (fundReq.Username.IsEmpty() || fundReq.Password.IsEmpty())
                    {
                        Res.StatusDescription = ServiceConsts.RequiredEmpty;
                        Res.StatusCode = ServiceConsts.StatusCode_fail;
                        return Res;
                    }

                    #region 第一步，登录
                    Url = baseUrl + "service/problem.do?sessionid=" + cookies["JSESSIONID"].Value;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = baseUrl + "emp/loginend.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    int answer = 0;
                    bool hasanswer = false;
                    List<string> signlist = new List<string> { "加", "减", "乘", "除以" };
                    for (int i = 0; i < signlist.Count; i++)
                    {
                        if (httpResult.Html.Contains(signlist[i]))
                        {
                            int firstnumber = CommonFun.GetMidStr(httpResult.Html, "", signlist[i]).ToInt(0);
                            int secondnumber = CommonFun.GetMidStr(httpResult.Html, signlist[i], "等于").ToInt(0);

                            switch (i)
                            {
                                case 0:
                                    answer = firstnumber + secondnumber;
                                    break;
                                case 1:
                                    answer = firstnumber - secondnumber;
                                    break;
                                case 2:
                                    answer = firstnumber * secondnumber;
                                    break;
                                case 3:
                                    answer = firstnumber / secondnumber;
                                    break;
                            }
                            hasanswer = true;
                            break;
                        }
                    }
                    if (!hasanswer)
                    {
                        Res.StatusDescription = "获取问题失败";
                        Res.StatusCode = ServiceConsts.StatusCode_error;
                        return Res;
                    }
                    Url = baseUrl + "service/EMPLogin/param?sessionid=" + cookies["JSESSIONID"].Value;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = baseUrl + "emp/loginend.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    #region rsa加密
                    string javastr = @"function rsaencrypt(t1,t2,t3,p2){

var RSAUtils = {};

var biRadixBase = 2;
var biRadixBits = 16;
var bitsPerDigit = biRadixBits;
var biRadix = 1 << 16; 
var biHalfRadix = biRadix >>> 1;
var biRadixSquared = biRadix * biRadix;
var maxDigitVal = biRadix - 1;
var maxInteger = 9999999999999998;
var digit;
var maxDigits;
var ZERO_ARRAY;
var bigZero, bigOne;

var BigInt = function(flag) {
	if (typeof flag == 'boolean' && flag == true) {
		this.digits = null;
	} else {
		this.digits = ZERO_ARRAY.slice(0);
	}
	this.isNeg = false;
};

RSAUtils.setMaxDigits = function(value) {
	maxDigits = value;
	ZERO_ARRAY = new Array(maxDigits);
	for (var iza = 0; iza < ZERO_ARRAY.length; iza++) ZERO_ARRAY[iza] = 0;
	bigZero = new BigInt();
	bigOne = new BigInt();
	bigOne.digits[0] = 1;
};
RSAUtils.setMaxDigits(20);

var dpl10 = 15;

RSAUtils.biFromNumber = function(i) {
	var result = new BigInt();
	result.isNeg = i < 0;
	i = Math.abs(i);
	var j = 0;
	while (i > 0) {
		result.digits[j++] = i & maxDigitVal;
		i = Math.floor(i / biRadix);
	}
	return result;
};

var lr10 = RSAUtils.biFromNumber(1000000000000000);

RSAUtils.biFromDecimal = function(s) {
	var isNeg = s.charAt(0) == '-';
	var i = isNeg ? 1 : 0;
	var result;
	// Skip leading zeros.
	while (i < s.length && s.charAt(i) == '0') ++i;
	if (i == s.length) {
		result = new BigInt();
	}
	else {
		var digitCount = s.length - i;
		var fgl = digitCount % dpl10;
		if (fgl == 0) fgl = dpl10;
		result = RSAUtils.biFromNumber(Number(s.substr(i, fgl)));
		i += fgl;
		while (i < s.length) {
			result = RSAUtils.biAdd(RSAUtils.biMultiply(result, lr10),
					RSAUtils.biFromNumber(Number(s.substr(i, dpl10))));
			i += dpl10;
		}
		result.isNeg = isNeg;
	}
	return result;
};

RSAUtils.biCopy = function(bi) {
	var result = new BigInt(true);
	result.digits = bi.digits.slice(0);
	result.isNeg = bi.isNeg;
	return result;
};

RSAUtils.reverseStr = function(s) {
	var result = '';
	for (var i = s.length - 1; i > -1; --i) {
		result += s.charAt(i);
	}
	return result;
};

var hexatrigesimalToChar = [
	'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
	'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
	'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
	'u', 'v', 'w', 'x', 'y', 'z'
];

RSAUtils.biToString = function(x, radix) { 
	var b = new BigInt();
	b.digits[0] = radix;
	var qr = RSAUtils.biDivideModulo(x, b);
	var result = hexatrigesimalToChar[qr[1].digits[0]];
	while (RSAUtils.biCompare(qr[0], bigZero) == 1) {
		qr = RSAUtils.biDivideModulo(qr[0], b);
		digit = qr[1].digits[0];
		result += hexatrigesimalToChar[qr[1].digits[0]];
	}
	return (x.isNeg ? '-' : '') + RSAUtils.reverseStr(result);
};

RSAUtils.biToDecimal = function(x) {
	var b = new BigInt();
	b.digits[0] = 10;
	var qr = RSAUtils.biDivideModulo(x, b);
	var result = String(qr[1].digits[0]);
	while (RSAUtils.biCompare(qr[0], bigZero) == 1) {
		qr = RSAUtils.biDivideModulo(qr[0], b);
		result += String(qr[1].digits[0]);
	}
	return (x.isNeg ? '-' : '') + RSAUtils.reverseStr(result);
};

var hexToChar = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'a', 'b', 'c', 'd', 'e', 'f'];

RSAUtils.digitToHex = function(n) {
	var mask = 0xf;
	var result = '';
	for (var i = 0; i < 4; ++i) {
		result += hexToChar[n & mask];
		n >>>= 4;
	}
	return RSAUtils.reverseStr(result);
};

RSAUtils.biToHex = function(x) {
	var result = '';
	var n = RSAUtils.biHighIndex(x);
	for (var i = RSAUtils.biHighIndex(x); i > -1; --i) {
		result += RSAUtils.digitToHex(x.digits[i]);
	}
	return result;
};

RSAUtils.charToHex = function(c) {
	var ZERO = 48;
	var NINE = ZERO + 9;
	var littleA = 97;
	var littleZ = littleA + 25;
	var bigA = 65;
	var bigZ = 65 + 25;
	var result;

	if (c >= ZERO && c <= NINE) {
		result = c - ZERO;
	} else if (c >= bigA && c <= bigZ) {
		result = 10 + c - bigA;
	} else if (c >= littleA && c <= littleZ) {
		result = 10 + c - littleA;
	} else {
		result = 0;
	}
	return result;
};

RSAUtils.hexToDigit = function(s) {
	var result = 0;
	var sl = Math.min(s.length, 4);
	for (var i = 0; i < sl; ++i) {
		result <<= 4;
		result |= RSAUtils.charToHex(s.charCodeAt(i));
	}
	return result;
};

RSAUtils.biFromHex = function(s) {
	var result = new BigInt();
	var sl = s.length;
	for (var i = sl, j = 0; i > 0; i -= 4, ++j) {
		result.digits[j] = RSAUtils.hexToDigit(s.substr(Math.max(i - 4, 0), Math.min(i, 4)));
	}
	return result;
};

RSAUtils.biFromString = function(s, radix) {
	var isNeg = s.charAt(0) == '-';
	var istop = isNeg ? 1 : 0;
	var result = new BigInt();
	var place = new BigInt();
	place.digits[0] = 1;
	for (var i = s.length - 1; i >= istop; i--) {
		var c = s.charCodeAt(i);
		var digit = RSAUtils.charToHex(c);
		var biDigit = RSAUtils.biMultiplyDigit(place, digit);
		result = RSAUtils.biAdd(result, biDigit);
		place = RSAUtils.biMultiplyDigit(place, radix);
	}
	result.isNeg = isNeg;
	return result;
};

RSAUtils.biDump = function(b) {
	return (b.isNeg ? '-' : '') + b.digits.join(' ');
};

RSAUtils.biAdd = function(x, y) {
	var result;

	if (x.isNeg != y.isNeg) {
		y.isNeg = !y.isNeg;
		result = RSAUtils.biSubtract(x, y);
		y.isNeg = !y.isNeg;
	}
	else {
		result = new BigInt();
		var c = 0;
		var n;
		for (var i = 0; i < x.digits.length; ++i) {
			n = x.digits[i] + y.digits[i] + c;
			result.digits[i] = n % biRadix;
			c = Number(n >= biRadix);
		}
		result.isNeg = x.isNeg;
	}
	return result;
};

RSAUtils.biSubtract = function(x, y) {
	var result;
	if (x.isNeg != y.isNeg) {
		y.isNeg = !y.isNeg;
		result = RSAUtils.biAdd(x, y);
		y.isNeg = !y.isNeg;
	} else {
		result = new BigInt();
		var n, c;
		c = 0;
		for (var i = 0; i < x.digits.length; ++i) {
			n = x.digits[i] - y.digits[i] + c;
			result.digits[i] = n % biRadix;
			// Stupid non-conforming modulus operation.
			if (result.digits[i] < 0) result.digits[i] += biRadix;
			c = 0 - Number(n < 0);
		}
		// Fix up the negative sign, if any.
		if (c == -1) {
			c = 0;
			for (var i = 0; i < x.digits.length; ++i) {
				n = 0 - result.digits[i] + c;
				result.digits[i] = n % biRadix;
				// Stupid non-conforming modulus operation.
				if (result.digits[i] < 0) result.digits[i] += biRadix;
				c = 0 - Number(n < 0);
			}
			// Result is opposite sign of arguments.
			result.isNeg = !x.isNeg;
		} else {
			// Result is same sign.
			result.isNeg = x.isNeg;
		}
	}
	return result;
};

RSAUtils.biHighIndex = function(x) {
	var result = x.digits.length - 1;
	while (result > 0 && x.digits[result] == 0) --result;
	return result;
};

RSAUtils.biNumBits = function(x) {
	var n = RSAUtils.biHighIndex(x);
	var d = x.digits[n];
	var m = (n + 1) * bitsPerDigit;
	var result;
	for (result = m; result > m - bitsPerDigit; --result) {
		if ((d & 0x8000) != 0) break;
		d <<= 1;
	}
	return result;
};

RSAUtils.biMultiply = function(x, y) {
	var result = new BigInt();
	var c;
	var n = RSAUtils.biHighIndex(x);
	var t = RSAUtils.biHighIndex(y);
	var u, uv, k;

	for (var i = 0; i <= t; ++i) {
		c = 0;
		k = i;
		for (var j = 0; j <= n; ++j, ++k) {
			uv = result.digits[k] + x.digits[j] * y.digits[i] + c;
			result.digits[k] = uv & maxDigitVal;
			c = uv >>> biRadixBits;
			//c = Math.floor(uv / biRadix);
		}
		result.digits[i + n + 1] = c;
	}
	// Someone give me a logical xor, please.
	result.isNeg = x.isNeg != y.isNeg;
	return result;
};

RSAUtils.biMultiplyDigit = function(x, y) {
	var n, c, uv;

	var result = new BigInt();
	n = RSAUtils.biHighIndex(x);
	c = 0;
	for (var j = 0; j <= n; ++j) {
		uv = result.digits[j] + x.digits[j] * y + c;
		result.digits[j] = uv & maxDigitVal;
		c = uv >>> biRadixBits;
		//c = Math.floor(uv / biRadix);
	}
	result.digits[1 + n] = c;
	return result;
};

RSAUtils.arrayCopy = function(src, srcStart, dest, destStart, n) {
	var m = Math.min(srcStart + n, src.length);
	for (var i = srcStart, j = destStart; i < m; ++i, ++j) {
		dest[j] = src[i];
	}
};

var highBitMasks = [0x0000, 0x8000, 0xC000, 0xE000, 0xF000, 0xF800,
        0xFC00, 0xFE00, 0xFF00, 0xFF80, 0xFFC0, 0xFFE0,
        0xFFF0, 0xFFF8, 0xFFFC, 0xFFFE, 0xFFFF];

RSAUtils.biShiftLeft = function(x, n) {
	var digitCount = Math.floor(n / bitsPerDigit);
	var result = new BigInt();
	RSAUtils.arrayCopy(x.digits, 0, result.digits, digitCount,
	          result.digits.length - digitCount);
	var bits = n % bitsPerDigit;
	var rightBits = bitsPerDigit - bits;
	for (var i = result.digits.length - 1, i1 = i - 1; i > 0; --i, --i1) {
		result.digits[i] = ((result.digits[i] << bits) & maxDigitVal) |
		                   ((result.digits[i1] & highBitMasks[bits]) >>>
		                    (rightBits));
	}
	result.digits[0] = ((result.digits[i] << bits) & maxDigitVal);
	result.isNeg = x.isNeg;
	return result;
};

var lowBitMasks = [0x0000, 0x0001, 0x0003, 0x0007, 0x000F, 0x001F,
        0x003F, 0x007F, 0x00FF, 0x01FF, 0x03FF, 0x07FF,
        0x0FFF, 0x1FFF, 0x3FFF, 0x7FFF, 0xFFFF];

RSAUtils.biShiftRight = function(x, n) {
	var digitCount = Math.floor(n / bitsPerDigit);
	var result = new BigInt();
	RSAUtils.arrayCopy(x.digits, digitCount, result.digits, 0,
	          x.digits.length - digitCount);
	var bits = n % bitsPerDigit;
	var leftBits = bitsPerDigit - bits;
	for (var i = 0, i1 = i + 1; i < result.digits.length - 1; ++i, ++i1) {
		result.digits[i] = (result.digits[i] >>> bits) |
		                   ((result.digits[i1] & lowBitMasks[bits]) << leftBits);
	}
	result.digits[result.digits.length - 1] >>>= bits;
	result.isNeg = x.isNeg;
	return result;
};

RSAUtils.biMultiplyByRadixPower = function(x, n) {
	var result = new BigInt();
	RSAUtils.arrayCopy(x.digits, 0, result.digits, n, result.digits.length - n);
	return result;
};

RSAUtils.biDivideByRadixPower = function(x, n) {
	var result = new BigInt();
	RSAUtils.arrayCopy(x.digits, n, result.digits, 0, result.digits.length - n);
	return result;
};

RSAUtils.biModuloByRadixPower = function(x, n) {
	var result = new BigInt();
	RSAUtils.arrayCopy(x.digits, 0, result.digits, 0, n);
	return result;
};

RSAUtils.biCompare = function(x, y) {
	if (x.isNeg != y.isNeg) {
		return 1 - 2 * Number(x.isNeg);
	}
	for (var i = x.digits.length - 1; i >= 0; --i) {
		if (x.digits[i] != y.digits[i]) {
			if (x.isNeg) {
				return 1 - 2 * Number(x.digits[i] > y.digits[i]);
			} else {
				return 1 - 2 * Number(x.digits[i] < y.digits[i]);
			}
		}
	}
	return 0;
};

RSAUtils.biDivideModulo = function(x, y) {
	var nb = RSAUtils.biNumBits(x);
	var tb = RSAUtils.biNumBits(y);
	var origYIsNeg = y.isNeg;
	var q, r;
	if (nb < tb) {
		// |x| < |y|
		if (x.isNeg) {
			q = RSAUtils.biCopy(bigOne);
			q.isNeg = !y.isNeg;
			x.isNeg = false;
			y.isNeg = false;
			r = RSAUtils.biSubtract(y, x);
			x.isNeg = true;
			y.isNeg = origYIsNeg;
		} else {
			q = new BigInt();
			r = RSAUtils.biCopy(x);
		}
		return [q, r];
	}

	q = new BigInt();
	r = x;

	var t = Math.ceil(tb / bitsPerDigit) - 1;
	var lambda = 0;
	while (y.digits[t] < biHalfRadix) {
		y = RSAUtils.biShiftLeft(y, 1);
		++lambda;
		++tb;
		t = Math.ceil(tb / bitsPerDigit) - 1;
	}
	r = RSAUtils.biShiftLeft(r, lambda);
	nb += lambda; // Update the bit count for x.
	var n = Math.ceil(nb / bitsPerDigit) - 1;

	var b = RSAUtils.biMultiplyByRadixPower(y, n - t);
	while (RSAUtils.biCompare(r, b) != -1) {
		++q.digits[n - t];
		r = RSAUtils.biSubtract(r, b);
	}
	for (var i = n; i > t; --i) {
    var ri = (i >= r.digits.length) ? 0 : r.digits[i];
    var ri1 = (i - 1 >= r.digits.length) ? 0 : r.digits[i - 1];
    var ri2 = (i - 2 >= r.digits.length) ? 0 : r.digits[i - 2];
    var yt = (t >= y.digits.length) ? 0 : y.digits[t];
    var yt1 = (t - 1 >= y.digits.length) ? 0 : y.digits[t - 1];
		if (ri == yt) {
			q.digits[i - t - 1] = maxDigitVal;
		} else {
			q.digits[i - t - 1] = Math.floor((ri * biRadix + ri1) / yt);
		}

		var c1 = q.digits[i - t - 1] * ((yt * biRadix) + yt1);
		var c2 = (ri * biRadixSquared) + ((ri1 * biRadix) + ri2);
		while (c1 > c2) {
			--q.digits[i - t - 1];
			c1 = q.digits[i - t - 1] * ((yt * biRadix) | yt1);
			c2 = (ri * biRadix * biRadix) + ((ri1 * biRadix) + ri2);
		}

		b = RSAUtils.biMultiplyByRadixPower(y, i - t - 1);
		r = RSAUtils.biSubtract(r, RSAUtils.biMultiplyDigit(b, q.digits[i - t - 1]));
		if (r.isNeg) {
			r = RSAUtils.biAdd(r, b);
			--q.digits[i - t - 1];
		}
	}
	r = RSAUtils.biShiftRight(r, lambda);
	q.isNeg = x.isNeg != origYIsNeg;
	if (x.isNeg) {
		if (origYIsNeg) {
			q = RSAUtils.biAdd(q, bigOne);
		} else {
			q = RSAUtils.biSubtract(q, bigOne);
		}
		y = RSAUtils.biShiftRight(y, lambda);
		r = RSAUtils.biSubtract(y, r);
	}
	if (r.digits[0] == 0 && RSAUtils.biHighIndex(r) == 0) r.isNeg = false;

	return [q, r];
};

RSAUtils.biDivide = function(x, y) {
	return RSAUtils.biDivideModulo(x, y)[0];
};

RSAUtils.biModulo = function(x, y) {
	return RSAUtils.biDivideModulo(x, y)[1];
};

RSAUtils.biMultiplyMod = function(x, y, m) {
	return RSAUtils.biModulo(RSAUtils.biMultiply(x, y), m);
};

RSAUtils.biPow = function(x, y) {
	var result = bigOne;
	var a = x;
	while (true) {
		if ((y & 1) != 0) result = RSAUtils.biMultiply(result, a);
		y >>= 1;
		if (y == 0) break;
		a = RSAUtils.biMultiply(a, a);
	}
	return result;
};

RSAUtils.biPowMod = function(x, y, m) {
	var result = bigOne;
	var a = x;
	var k = y;
	while (true) {
		if ((k.digits[0] & 1) != 0) result = RSAUtils.biMultiplyMod(result, a, m);
		k = RSAUtils.biShiftRight(k, 1);
		if (k.digits[0] == 0 && RSAUtils.biHighIndex(k) == 0) break;
		a = RSAUtils.biMultiplyMod(a, a, m);
	}
	return result;
};

var BarrettMu = function(m) {
	this.modulus = RSAUtils.biCopy(m);
	this.k = RSAUtils.biHighIndex(this.modulus) + 1;
	var b2k = new BigInt();
	b2k.digits[2 * this.k] = 1; // b2k = b^(2k)
	this.mu = RSAUtils.biDivide(b2k, this.modulus);
	this.bkplus1 = new BigInt();
	this.bkplus1.digits[this.k + 1] = 1; // bkplus1 = b^(k+1)
	this.modulo = BarrettMu_modulo;
	this.multiplyMod = BarrettMu_multiplyMod;
	this.powMod = BarrettMu_powMod;
};

function BarrettMu_modulo(x) {
	var $dmath = RSAUtils;
	var q1 = $dmath.biDivideByRadixPower(x, this.k - 1);
	var q2 = $dmath.biMultiply(q1, this.mu);
	var q3 = $dmath.biDivideByRadixPower(q2, this.k + 1);
	var r1 = $dmath.biModuloByRadixPower(x, this.k + 1);
	var r2term = $dmath.biMultiply(q3, this.modulus);
	var r2 = $dmath.biModuloByRadixPower(r2term, this.k + 1);
	var r = $dmath.biSubtract(r1, r2);
	if (r.isNeg) {
		r = $dmath.biAdd(r, this.bkplus1);
	}
	var rgtem = $dmath.biCompare(r, this.modulus) >= 0;
	while (rgtem) {
		r = $dmath.biSubtract(r, this.modulus);
		rgtem = $dmath.biCompare(r, this.modulus) >= 0;
	}
	return r;
}

function BarrettMu_multiplyMod(x, y) {
	var xy = RSAUtils.biMultiply(x, y);
	return this.modulo(xy);
}

function BarrettMu_powMod(x, y) {
	var result = new BigInt();
	result.digits[0] = 1;
	var a = x;
	var k = y;
	while (true) {
		if ((k.digits[0] & 1) != 0) result = this.multiplyMod(result, a);
		k = RSAUtils.biShiftRight(k, 1);
		if (k.digits[0] == 0 && RSAUtils.biHighIndex(k) == 0) break;
		a = this.multiplyMod(a, a);
	}
	return result;
}

var RSAKeyPair = function(encryptionExponent, decryptionExponent, modulus) {
	var $dmath = RSAUtils;
	this.e = $dmath.biFromHex(encryptionExponent);
	this.d = $dmath.biFromHex(decryptionExponent);
	this.m = $dmath.biFromHex(modulus);
	this.chunkSize = 2 * $dmath.biHighIndex(this.m);
	this.radix = 16;
	this.barrett = new BarrettMu(this.m);
};

RSAUtils.getKeyPair = function(encryptionExponent, decryptionExponent, modulus) {
	return new RSAKeyPair(encryptionExponent, decryptionExponent, modulus);
};

RSAUtils.encryptedString = function(key, s) {
	var a = [];
	var sl = s.length;
	var i = 0;
	while (i < sl) {
		a[i] = s.charCodeAt(i);
		i++;
	}

	while (a.length % key.chunkSize != 0) {
		a[i++] = 0;
	}

	var al = a.length;
	var result = '';
	var j, k, block;
	for (i = 0; i < al; i += key.chunkSize) {
		block = new BigInt();
		j = 0;
		for (k = i; k < i + key.chunkSize; ++j) {
			block.digits[j] = a[k++];
			block.digits[j] += a[k++] << 8;
		}
		var crypt = key.barrett.powMod(block, key.e);
		var text = key.radix == 16 ? RSAUtils.biToHex(crypt) : RSAUtils.biToString(crypt, key.radix);
		result += text + ' ';
	}
	return result.substring(0, result.length - 1);
};

RSAUtils.decryptedString = function(key, s) {
	var blocks = s.split(' ');
	var result = '';
	var i, j, block;
	for (i = 0; i < blocks.length; ++i) {
		var bi;
		if (key.radix == 16) {
			bi = RSAUtils.biFromHex(blocks[i]);
		}
		else {
			bi = RSAUtils.biFromString(blocks[i], key.radix);
		}
		block = key.barrett.powMod(bi, key.d);
		for (j = 0; j <= RSAUtils.biHighIndex(block); ++j) {
			result += String.fromCharCode(block.digits[j] & 255,
			                              block.digits[j] >> 8);
		}
	}
	if (result.charCodeAt(result.length - 1) == 0) {
		result = result.substring(0, result.length - 1);
	}
	return result;
};

RSAUtils.setMaxDigits(130);

return RSAUtils.encryptedString(RSAUtils.getKeyPair(t1,t2,t3), p2); 
}";
                    #endregion
                    object[] ob = new object[4];
                    ob[0] = "010001";
                    ob[1] = fundReq.Username;
                    ob[2] = httpResult.Html.Replace(":010001", "");
                    ob[3] = CommonFun.GetTimeStamp() + ":" + fundReq.Password;
                    //string method = string.Format("rsaencrypt('010001','{0}','{1}','{2}')", socialReq.Username,httpResult.Html.Replace(":010001",""), CommonFun.GetTimeStamp() + ":" + socialReq.Password);
                    //string pass = JavaScriptHelper.JavaScriptEval(javastr, method).ToString();
                    string raspassword = JavaScriptHelper.JavaScriptExecute(javastr, "rsaencrypt", ob).ToString();

                    Url = baseUrl + "service/EMPLogin/login";
                    postdata = string.Format("uname={0}&upass={1}&sessionid={2}&identify={3}&answer={4}&param3={5}", fundReq.Username, CommonFun.GetMd5Str(fundReq.Password), cookies["JSESSIONID"].Value, fundReq.Vercode, answer, raspassword);
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "POST",
                        Postdata = postdata,
                        Referer = baseUrl + "emp/loginend.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9]+[0-9]+");
                    string regkey = httpResult.Html;
                    if (regkey == "identifyWrong")
                    {
                        Res.StatusDescription = "验证码错误！";
                        Res.StatusCode = ServiceConsts.StatusCode_error;
                        return Res;
                    }
                    if (regkey == "pwdError")
                    {
                        Res.StatusDescription = "密码错误！";
                        Res.StatusCode = ServiceConsts.StatusCode_error;
                        return Res;
                    }
                    if (regkey == "noMembId")
                    {
                        Res.StatusDescription = "编号错误！";
                        Res.StatusCode = ServiceConsts.StatusCode_error;
                        return Res;
                    }
                    if (!regex.IsMatch(httpResult.Html))
                    {
                        Res.StatusDescription = regkey;
                        Res.StatusCode = ServiceConsts.StatusCode_error;
                        return Res;
                    }


                    Url = baseUrl + "emp/loginForwardProcess.jsp?regkey=" + regkey;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Referer = baseUrl + "emp/loginend.jsp",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    cookies = CommonFun.GetCookieCollection(cookies, httpResult.CookieCollection);
                    cookies.Add(new Cookie { Name = "EmpRegKey", Value = regkey, Path = "/", Domain = "www.sipspf.org.cn" });
                    cookies.Add(new Cookie { Name = "ERKey", Value = regkey, Path = "/", Domain = "www.sipspf.org.cn" });
                    cookies.Add(new Cookie { Name = "LoginType", Value = "1", Path = "/", Domain = "www.sipspf.org.cn" });
                    #endregion

                    #region 第二步，个人基本信息
                    Url = baseUrl + "emp/queryinfo.jsp";
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    Url = baseUrl + "service/EmpInfo/getInfo?regkey=" + regkey;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);

                    string a = "{\"" + httpResult.Html.Replace(":", "\":\"").Replace(",", "\",\"") + "\"}";
                    BaseInfo baseinfo = jsonParser.DeserializeObject<BaseInfo>(a);
                    Res.Name = baseinfo.EMP_NAME;
                    Res.IdentityCard = baseinfo.EMP_IDCARD;
                    Res.Sex = baseinfo.EMP_SEX;
                    Res.CompanyName = baseinfo.CORP_NAME;
                    Res.CompanyNo = baseinfo.CORP_ID;
                    Res.EmployeeNo = baseinfo.EMP_ID_SOCIAL;
                    Res.Phone = baseinfo.EMP_MOBILE.IsEmpty() ? baseinfo.EMP_PHONE : baseinfo.EMP_MOBILE;
                    #endregion

                    #region 第三步，明细
                    Url = baseUrl + "account/accountQuery?regkey=" + regkey + "&itype=2"; ;
                    httpItem = new HttpItem()
                    {
                        URL = Url,
                        Method = "post",
                        CookieCollection = cookies,
                        ResultCookieType = ResultCookieType.CookieCollection
                    };
                    httpResult = httpHelper.GetHtml(httpItem);
                    List<DetailInfo> _detaillist = jsonParser.DeserializeObject<List<DetailInfo>>(httpResult.Html);
                    foreach (DetailInfo _detail in _detaillist)
                    {
                        detailRes = new ProvidentFundDetail();
                        detailRes.ProvidentFundTime = _detail.belongMonth;
                        detailRes.ProvidentFundBase = _detail.baseMoney.ToDecimal(0);
                        detailRes.PersonalPayAmount = _detail.medicalHosingAcc.ToDecimal(0) / 2;
                        detailRes.CompanyPayAmount = detailRes.PersonalPayAmount;
                        detailRes.PaymentFlag = ServiceConsts.ProvidentFund_PaymentFlag_Normal;
                        detailRes.PaymentType = ServiceConsts.ProvidentFund_PaymentType_Normal;

                        Res.ProvidentFundDetailList.Add(detailRes);
                    }

                    DateTime startdate = new DateTime();
                    startdate = DateTime.ParseExact(DateTime.Now.AddYears(-4).Year.ToString() + "06", "yyyyMM", null);
                    while (startdate < DateTime.Now)
                    {
                        Url = "http://www.sipspf.org.cn/sipspf/web/auth/account";
                        postdata = string.Format("year={0}&page=0", startdate.ToString("yyyyMM"));
                        httpItem = new HttpItem()
                        {
                            URL = Url,
                            Method = "post",
                            Postdata = postdata,
                            CookieCollection = cookies,
                            ResultCookieType = ResultCookieType.CookieCollection
                        };
                        httpResult = httpHelper.GetHtml(httpItem);
                        results = HtmlParser.GetResultFromParser(httpResult.Html, "//div[@id='workInfo']//table/tr");
                        foreach (string item in results)
                        {
                            List<string> tdrow = HtmlParser.GetResultFromParser(item, "td");
                            if (tdrow.Count != 17 || tdrow[9].ToDecimal(0) == 0 || tdrow[3].ToDecimal(0) == 0)
                            {
                                continue;
                            }

                            detailRes = null;
                            string ProvidentFundTime = tdrow[2].Replace("缴交", "").Replace("补缴", "").ToInt(0).ToString();
                            if (ProvidentFundTime != "0")
                            {
                                detailRes = Res.ProvidentFundDetailList.Where(o => o.ProvidentFundTime == ProvidentFundTime).FirstOrDefault();
                            }

                            if (detailRes == null)
                            {
                                detailRes = new ProvidentFundDetail();
                                DateTime dt;
                                DateTime.TryParseExact(tdrow[1], "yyyy-MM-dd", null,System.Globalization.DateTimeStyles.None, out dt);
                                detailRes.PayTime = DateTime.ParseExact(tdrow[1], "yyyy-MM-dd", null); 
                                if (tdrow[2].StartsWith("缴交"))
                                {
                                    detailRes.ProvidentFundTime = tdrow[2].Replace("缴交", "");
                                    detailRes.PaymentFlag = ServiceConsts.SocialSecurity_PaymentFlag_Normal;
                                    detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                }
                                else if (tdrow[2].StartsWith("补缴"))
                                {
                                    if (tdrow[2].Replace("补缴", "").ToInt(0) == 0)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        detailRes.ProvidentFundTime = tdrow[2].Replace("补缴", "");
                                        detailRes.PaymentFlag = "补缴";
                                        detailRes.PaymentType = ServiceConsts.SocialSecurity_PaymentType_Normal;
                                    }
                                }
                                else
                                {
                                    detailRes.PaymentFlag = tdrow[2];
                                    detailRes.PaymentType = tdrow[2];
                                }
                                detailRes.ProvidentFundBase = tdrow[3].ToDecimal(0);
                                detailRes.PersonalPayAmount = tdrow[9].ToDecimal(0) / 2;
                                detailRes.CompanyPayAmount = detailRes.PersonalPayAmount;
                                detailRes.Description = tdrow[2];

                                Res.ProvidentFundDetailList.Add(detailRes);
                            }
                            detailRes.Description = tdrow[2];
                        }
                        startdate = startdate.AddMonths(6);
                    }
                    #endregion
                
                Res.PaymentMonths = PaymentMonths;
                Res.StatusDescription = fundCity + ServiceConsts.ProvidentFund_QuerySuccess;
                Res.StatusCode = ServiceConsts.StatusCode_success;
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Log4netAdapter.WriteError(fundCity + ServiceConsts.ProvidentFund_QueryError, e);
            }
            return Res;
        }

        class BaseInfo
        {
            public string EMP_SEX { get; set; }
            public string EMP_MEMBER { get; set; }
            public string EMP_NAME { get; set; }
            public string EMP_BIRTHDAY { get; set; }
            public string EMP_CREATEDAY { get; set; }
            public string EMP_ID { get; set; }
            public string CORP_ID { get; set; }
            public string CORP_NAME { get; set; }
            public string EMP_IDCARD { get; set; }
            public string EMP_STATE { get; set; }
            public string EMP_GJJCODE { get; set; }
            public string EMP_ID_SOCIAL { get; set; }
            public string EMP_MOBILE { get; set; }
            public string EMP_PHONE { get; set; }
            public string EMP_POST { get; set; }
            public string EMP_EMAIL { get; set; }
            public string EMP_ADDRESS { get; set; }
        }

        class DetailInfo
        {
            public string baseMoney { get; set; }
            public string belongMonth { get; set; }
            public string commonRetiredAcc { get; set; }
            public string medicalHosingAcc { get; set; }
            public string renderType { get; set; }
            public string retiredMedicalAcc { get; set; }
            public string specilRetiredAcc { get; set; }
            public string sumMoney { get; set; }
        }
    }
    
}
