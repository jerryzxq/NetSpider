using Jint;
using Noesis.Javascript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Vcredit.Common.Ext;

namespace Vcredit.Common.Utility
{
    public static class RSAHelper
    {
        /// <summary>
        /// RSA的容器 可以解密的源字符串长度为 DWKEYSIZE/8-11 
        /// </summary>
        public const int DWKEYSIZE = 1024;

        /// <summary>
        /// RSA加密的密匙结构  公钥和私匙
        /// </summary>
        public struct RSAKey
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }

        #region 得到RSA的解谜的密匙对
        /// <summary>
        /// 得到RSA的解谜的密匙对
        /// </summary>
        /// <returns></returns>
        public static RSAKey GetRASKey()
        {
            RSACryptoServiceProvider.UseMachineKeyStore = true;
            //声明一个指定大小的RSA容器
            RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(DWKEYSIZE);
            //取得RSA容易里的各种参数
            RSAParameters p = rsaProvider.ExportParameters(true);

            return new RSAKey()
            {
                PublicKey = ComponentKey(p.Exponent, p.Modulus),
                PrivateKey = ComponentKey(p.D, p.Modulus)
            };
        }
        #endregion

        #region 检查明文的有效性 DWKEYSIZE/8-11 长度之内为有效 中英文都算一个字符
        /// <summary>
        /// 检查明文的有效性 DWKEYSIZE/8-11 长度之内为有效 中英文都算一个字符
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool CheckSourceValidate(string source)
        {
            return (DWKEYSIZE / 8 - 11) >= source.Length;
        }
        #endregion

        #region 组合解析密匙
        /// <summary>
        /// 组合成密匙字符串
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        private static string ComponentKey(byte[] b1, byte[] b2)
        {
            List<byte> list = new List<byte>();
            //在前端加上第一个数组的长度值 这样今后可以根据这个值分别取出来两个数组
            list.Add((byte)b1.Length);
            list.AddRange(b1);
            list.AddRange(b2);
            byte[] b = list.ToArray<byte>();
            return Convert.ToBase64String(b);
        }

        /// <summary>
        /// 解析密匙
        /// </summary>
        /// <param name="key">密匙</param>
        /// <param name="b1">RSA的相应参数1</param>
        /// <param name="b2">RSA的相应参数2</param>
        private static void ResolveKey(string key, out byte[] b1, out byte[] b2)
        {
            //从base64字符串 解析成原来的字节数组
            byte[] b = Convert.FromBase64String(key);
            //初始化参数的数组长度
            b1 = new byte[b[0]];
            b2 = new byte[b.Length - b[0] - 1];
            //将相应位置是值放进相应的数组
            for (int n = 1, i = 0, j = 0; n < b.Length; n++)
            {
                if (n <= b[0])
                {
                    b1[i++] = b[n];
                }
                else
                {
                    b2[j++] = b[n];
                }
            }
        }
        #endregion

        #region 字符串加密解密 公开方法
        /// <summary>
        /// 字符串加密
        /// </summary>
        /// <param name="source">源字符串 明文</param>
        /// <param name="key">密匙</param>
        /// <returns>加密遇到错误将会返回原字符串</returns>
        public static string EncryptString(string source, string key)
        {
            string encryptString = string.Empty;
            byte[] d;
            byte[] n;
            try
            {
                if (!CheckSourceValidate(source))
                {
                    throw new Exception("source string too long");
                }
                //解析这个密钥
                ResolveKey(key, out d, out n);
                BigInteger biN = new BigInteger(n);
                BigInteger biD = new BigInteger(d);
                encryptString = EncryptString(source, biD, biN);
            }
            catch
            {
                encryptString = source;
            }
            return encryptString;
        }

        /// <summary>
        /// 字符串解密
        /// </summary>
        /// <param name="encryptString">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>遇到解密失败将会返回原字符串</returns>
        public static string DecryptString(string encryptString, string key)
        {
            string source = string.Empty;
            byte[] e;
            byte[] n;
            try
            {
                //解析这个密钥
                ResolveKey(key, out e, out n);
                BigInteger biE = new BigInteger(e);
                BigInteger biN = new BigInteger(n);
                source = DecryptString(encryptString, biE, biN);
            }
            catch
            {
                source = encryptString;
            }
            return source;
        }
        #endregion

        #region 字符串加密解密 私有  实现加解密的实现方法
        /// <summary>
        /// 用指定的密匙加密 
        /// </summary>
        /// <param name="source">明文</param>
        /// <param name="d">可以是RSACryptoServiceProvider生成的D</param>
        /// <param name="n">可以是RSACryptoServiceProvider生成的Modulus</param>
        /// <returns>返回密文</returns>
        private static string EncryptString(string source, BigInteger d, BigInteger n)
        {
            int len = source.Length;
            int len1 = 0;
            int blockLen = 0;
            if ((len % 128) == 0)
                len1 = len / 128;
            else
                len1 = len / 128 + 1;
            string block = "";
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < len1; i++)
            {
                if (len >= 128)
                    blockLen = 128;
                else
                    blockLen = len;
                block = source.Substring(i * 128, blockLen);
                byte[] oText = System.Text.Encoding.Default.GetBytes(block);
                BigInteger biText = new BigInteger(oText);
                BigInteger biEnText = biText.modPow(d, n);
                string temp = biEnText.ToHexString();
                result.Append(temp).Append("@");
                len -= blockLen;
            }
            return result.ToString().TrimEnd('@');
        }

        /// <summary>
        /// 用指定的密匙加密 
        /// </summary>
        /// <param name="source">密文</param>
        /// <param name="e">可以是RSACryptoServiceProvider生成的Exponent</param>
        /// <param name="n">可以是RSACryptoServiceProvider生成的Modulus</param>
        /// <returns>返回明文</returns>
        private static string DecryptString(string encryptString, BigInteger e, BigInteger n)
        {
            StringBuilder result = new StringBuilder();
            string[] strarr1 = encryptString.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strarr1.Length; i++)
            {
                string block = strarr1[i];
                BigInteger biText = new BigInteger(block, 16);
                BigInteger biEnText = biText.modPow(e, n);
                string temp = System.Text.Encoding.Default.GetString(biEnText.getBytes());
                result.Append(temp);
            }
            return result.ToString();
        }
        #endregion

        #region 广州、惠州社保RSA加密
        public static string EncryptStringByRsaJS(string password, string pubkey, string moder = "10001", string maxdigits = "67")
        {
            if (pubkey.IsEmpty())
            {
                return "";
            }
            var rsastr = "function encrypt_password() {var c ={\"e\":\"" + moder + "\",\"maxdigits\":" + maxdigits + ",\"n\":\"" + pubkey + "\"}; var h = c.maxdigits; var f = c.e;var k = c.n;setMaxDigits(h);var b = new RSAKeyPair(f, \"\", k);var password=\"" + password + "\"; return encryptedString(b, password);}function RSAKeyPair(b,c,a){this.e=biFromHex(b);this.d=biFromHex(c);this.m=biFromHex(a);this.chunkSize=2*biHighIndex(this.m);this.radix=16;this.barrett=new BarrettMu(this.m)}function twoDigit(a){return(a<10?\"0\":\"\")+String(a)}function encryptedString(l,o){var h=new Array();var b=o.length;var f=0;while(f<b){h[f]=o.charCodeAt(f);f++}while(h.length%l.chunkSize!=0){h[f++]=0}var g=h.length;var p=\"\";var e,d,c;for(f=0;f<g;f+=l.chunkSize){c=new BigInt();e=0;for(d=f;d<f+l.chunkSize;++e){c.digits[e]=h[d++];c.digits[e]+=h[d++]<<8}var n=l.barrett.powMod(c,l.e);var m=l.radix==16?biToHex(n):biToString(n,l.radix);p+=m+\" \"}return p.substring(0,p.length-1)}function decryptedString(e,f){var h=f.split(\" \");var a=\"\";var d,c,g;for(d=0;d<h.length;++d){var b;if(e.radix==16){b=biFromHex(h[d])}else{b=biFromString(h[d],e.radix)}g=e.barrett.powMod(b,e.d);for(c=0;c<=biHighIndex(g);++c){a+=String.fromCharCode(g.digits[c]&255,g.digits[c]>>8)}}if(a.charCodeAt(a.length-1)==0){a=a.substring(0,a.length-1)}return a}var biRadixBase=2;var biRadixBits=16;var bitsPerDigit=biRadixBits;var biRadix=1<<16;var biHalfRadix=biRadix>>>1;var biRadixSquared=biRadix*biRadix;var maxDigitVal=biRadix-1;var maxInteger=9999999999999998;var maxDigits;var ZERO_ARRAY;var bigZero,bigOne;function setMaxDigits(b){maxDigits=b;ZERO_ARRAY=new Array(maxDigits);for(var a=0;a<ZERO_ARRAY.length;a++){ZERO_ARRAY[a]=0}bigZero=new BigInt();bigOne=new BigInt();bigOne.digits[0]=1}setMaxDigits(20);var dpl10=15;var lr10=biFromNumber(1000000000000000);function BigInt(a){if(typeof a==\"boolean\"&&a==true){this.digits=null}else{this.digits=ZERO_ARRAY.slice(0)}this.isNeg=false}function biFromDecimal(e){var d=e.charAt(0)==\"-\";var c=d?1:0;var a;while(c<e.length&&e.charAt(c)==\"0\"){++c}if(c==e.length){a=new BigInt()}else{var b=e.length-c;var f=b%dpl10;if(f==0){f=dpl10}a=biFromNumber(Number(e.substr(c,f)));c+=f;while(c<e.length){a=biAdd(biMultiply(a,lr10),biFromNumber(Number(e.substr(c,dpl10))));c+=dpl10}a.isNeg=d}return a}function biCopy(b){var a=new BigInt(true);a.digits=b.digits.slice(0);a.isNeg=b.isNeg;return a}function biFromNumber(c){var a=new BigInt();a.isNeg=c<0;c=Math.abs(c);var b=0;while(c>0){a.digits[b++]=c&maxDigitVal;c>>=biRadixBits}return a}function reverseStr(c){var a=\"\";for(var b=c.length-1;b>-1;--b){a+=c.charAt(b)}return a}var hexatrigesimalToChar=new Array(\"0\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\",\"8\",\"9\",\"a\",\"b\",\"c\",\"d\",\"e\",\"f\",\"g\",\"h\",\"i\",\"j\",\"k\",\"l\",\"m\",\"n\",\"o\",\"p\",\"q\",\"r\",\"s\",\"t\",\"u\",\"v\",\"w\",\"x\",\"y\",\"z\");function biToString(d,f){var c=new BigInt();c.digits[0]=f;var e=biDivideModulo(d,c);var a=hexatrigesimalToChar[e[1].digits[0]];while(biCompare(e[0],bigZero)==1){e=biDivideModulo(e[0],c);digit=e[1].digits[0];a+=hexatrigesimalToChar[e[1].digits[0]]}return(d.isNeg?\"-\":\"\")+reverseStr(a)}function biToDecimal(d){var c=new BigInt();c.digits[0]=10;var e=biDivideModulo(d,c);var a=String(e[1].digits[0]);while(biCompare(e[0],bigZero)==1){e=biDivideModulo(e[0],c);a+=String(e[1].digits[0])}return(d.isNeg?\"-\":\"\")+reverseStr(a)}var hexToChar=new Array(\"0\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\",\"8\",\"9\",\"a\",\"b\",\"c\",\"d\",\"e\",\"f\");function digitToHex(c){var b=15;var a=\"\";for(i=0;i<4;++i){a+=hexToChar[c&b];c>>>=4}return reverseStr(a)}function biToHex(b){var a=\"\";var d=biHighIndex(b);for(var c=biHighIndex(b);c>-1;--c){a+=digitToHex(b.digits[c])}return a}function charToHex(k){var d=48;var b=d+9;var e=97;var h=e+25;var g=65;var f=65+25;var a;if(k>=d&&k<=b){a=k-d}else{if(k>=g&&k<=f){a=10+k-g}else{if(k>=e&&k<=h){a=10+k-e}else{a=0}}}return a}function hexToDigit(d){var b=0;var a=Math.min(d.length,4);for(var c=0;c<a;++c){b<<=4;b|=charToHex(d.charCodeAt(c))}return b}function biFromHex(e){var b=new BigInt();var a=e.length;for(var d=a,c=0;d>0;d-=4,++c){b.digits[c]=hexToDigit(e.substr(Math.max(d-4,0),Math.min(d,4)))}return b}function biFromString(l,k){var a=l.charAt(0)==\"-\";var e=a?1:0;var m=new BigInt();var b=new BigInt();b.digits[0]=1;for(var d=l.length-1;d>=e;d--){var f=l.charCodeAt(d);var g=charToHex(f);var h=biMultiplyDigit(b,g);m=biAdd(m,h);b=biMultiplyDigit(b,k)}m.isNeg=a;return m}function biDump(a){return(a.isNeg?\"-\":\"\")+a.digits.join(\" \")}function biAdd(b,g){var a;if(b.isNeg!=g.isNeg){g.isNeg=!g.isNeg;a=biSubtract(b,g);g.isNeg=!g.isNeg}else{a=new BigInt();var f=0;var e;for(var d=0;d<b.digits.length;++d){e=b.digits[d]+g.digits[d]+f;a.digits[d]=e&65535;f=Number(e>=biRadix)}a.isNeg=b.isNeg}return a}function biSubtract(b,g){var a;if(b.isNeg!=g.isNeg){g.isNeg=!g.isNeg;a=biAdd(b,g);g.isNeg=!g.isNeg}else{a=new BigInt();var f,e;e=0;for(var d=0;d<b.digits.length;++d){f=b.digits[d]-g.digits[d]+e;a.digits[d]=f&65535;if(a.digits[d]<0){a.digits[d]+=biRadix}e=0-Number(f<0)}if(e==-1){e=0;for(var d=0;d<b.digits.length;++d){f=0-a.digits[d]+e;a.digits[d]=f&65535;if(a.digits[d]<0){a.digits[d]+=biRadix}e=0-Number(f<0)}a.isNeg=!b.isNeg}else{a.isNeg=b.isNeg}}return a}function biHighIndex(b){var a=b.digits.length-1;while(a>0&&b.digits[a]==0){--a}return a}function biNumBits(c){var f=biHighIndex(c);var e=c.digits[f];var b=(f+1)*bitsPerDigit;var a;for(a=b;a>b-bitsPerDigit;--a){if((e&32768)!=0){break}e<<=1}return a}function biMultiply(h,g){var o=new BigInt();var f;var b=biHighIndex(h);var m=biHighIndex(g);var l,a,d;for(var e=0;e<=m;++e){f=0;d=e;for(j=0;j<=b;++j,++d){a=o.digits[d]+h.digits[j]*g.digits[e]+f;o.digits[d]=a&maxDigitVal;f=a>>>biRadixBits}o.digits[e+b+1]=f}o.isNeg=h.isNeg!=g.isNeg;return o}function biMultiplyDigit(a,g){var f,e,d;result=new BigInt();f=biHighIndex(a);e=0;for(var b=0;b<=f;++b){d=result.digits[b]+a.digits[b]*g+e;result.digits[b]=d&maxDigitVal;e=d>>>biRadixBits}result.digits[1+f]=e;return result}function arrayCopy(e,h,c,g,f){var a=Math.min(h+f,e.length);for(var d=h,b=g;d<a;++d,++b){c[b]=e[d]}}var highBitMasks=new Array(0,32768,49152,57344,61440,63488,64512,65024,65280,65408,65472,65504,65520,65528,65532,65534,65535);function biShiftLeft(b,h){var d=Math.floor(h/bitsPerDigit);var a=new BigInt();arrayCopy(b.digits,0,a.digits,d,a.digits.length-d);var g=h%bitsPerDigit;var c=bitsPerDigit-g;for(var e=a.digits.length-1,f=e-1;e>0;--e,--f){a.digits[e]=((a.digits[e]<<g)&maxDigitVal)|((a.digits[f]&highBitMasks[g])>>>(c))}a.digits[0]=((a.digits[e]<<g)&maxDigitVal);a.isNeg=b.isNeg;return a}var lowBitMasks=new Array(0,1,3,7,15,31,63,127,255,511,1023,2047,4095,8191,16383,32767,65535);function biShiftRight(b,h){var c=Math.floor(h/bitsPerDigit);var a=new BigInt();arrayCopy(b.digits,c,a.digits,0,b.digits.length-c);var f=h%bitsPerDigit;var g=bitsPerDigit-f;for(var d=0,e=d+1;d<a.digits.length-1;++d,++e){a.digits[d]=(a.digits[d]>>>f)|((a.digits[e]&lowBitMasks[f])<<g)}a.digits[a.digits.length-1]>>>=f;a.isNeg=b.isNeg;return a}function biMultiplyByRadixPower(b,c){var a=new BigInt();arrayCopy(b.digits,0,a.digits,c,a.digits.length-c);return a}function biDivideByRadixPower(b,c){var a=new BigInt();arrayCopy(b.digits,c,a.digits,0,a.digits.length-c);return a}function biModuloByRadixPower(b,c){var a=new BigInt();arrayCopy(b.digits,0,a.digits,0,c);return a}function biCompare(a,c){if(a.isNeg!=c.isNeg){return 1-2*Number(a.isNeg)}for(var b=a.digits.length-1;b>=0;--b){if(a.digits[b]!=c.digits[b]){if(a.isNeg){return 1-2*Number(a.digits[b]>c.digits[b])}else{return 1-2*Number(a.digits[b]<c.digits[b])}}}return 0}function biDivideModulo(g,f){var a=biNumBits(g);var e=biNumBits(f);var d=f.isNeg;var o,m;if(a<e){if(g.isNeg){o=biCopy(bigOne);o.isNeg=!f.isNeg;g.isNeg=false;f.isNeg=false;m=biSubtract(f,g);g.isNeg=true;f.isNeg=d}else{o=new BigInt();m=biCopy(g)}return new Array(o,m)}o=new BigInt();m=g;var k=Math.ceil(e/bitsPerDigit)-1;var h=0;while(f.digits[k]<biHalfRadix){f=biShiftLeft(f,1);++h;++e;k=Math.ceil(e/bitsPerDigit)-1}m=biShiftLeft(m,h);a+=h;var u=Math.ceil(a/bitsPerDigit)-1;var B=biMultiplyByRadixPower(f,u-k);while(biCompare(m,B)!=-1){++o.digits[u-k];m=biSubtract(m,B)}for(var z=u;z>k;--z){var l=(z>=m.digits.length)?0:m.digits[z];var A=(z-1>=m.digits.length)?0:m.digits[z-1];var w=(z-2>=m.digits.length)?0:m.digits[z-2];var v=(k>=f.digits.length)?0:f.digits[k];var c=(k-1>=f.digits.length)?0:f.digits[k-1];if(l==v){o.digits[z-k-1]=maxDigitVal}else{o.digits[z-k-1]=Math.floor((l*biRadix+A)/v)}var s=o.digits[z-k-1]*((v*biRadix)+c);var p=(l*biRadixSquared)+((A*biRadix)+w);while(s>p){--o.digits[z-k-1];s=o.digits[z-k-1]*((v*biRadix)|c);p=(l*biRadix*biRadix)+((A*biRadix)+w)}B=biMultiplyByRadixPower(f,z-k-1);m=biSubtract(m,biMultiplyDigit(B,o.digits[z-k-1]));if(m.isNeg){m=biAdd(m,B);--o.digits[z-k-1]}}m=biShiftRight(m,h);o.isNeg=g.isNeg!=d;if(g.isNeg){if(d){o=biAdd(o,bigOne)}else{o=biSubtract(o,bigOne)}f=biShiftRight(f,h);m=biSubtract(f,m)}if(m.digits[0]==0&&biHighIndex(m)==0){m.isNeg=false}return new Array(o,m)}function biDivide(a,b){return biDivideModulo(a,b)[0]}function biModulo(a,b){return biDivideModulo(a,b)[1]}function biMultiplyMod(b,c,a){return biModulo(biMultiply(b,c),a)}function biPow(c,e){var b=bigOne;var d=c;while(true){if((e&1)!=0){b=biMultiply(b,d)}e>>=1;if(e==0){break}d=biMultiply(d,d)}return b}function biPowMod(d,g,c){var b=bigOne;var e=d;var f=g;while(true){if((f.digits[0]&1)!=0){b=biMultiplyMod(b,e,c)}f=biShiftRight(f,1);if(f.digits[0]==0&&biHighIndex(f)==0){break}e=biMultiplyMod(e,e,c)}return b}function BarrettMu(a){this.modulus=biCopy(a);this.k=biHighIndex(this.modulus)+1;var b=new BigInt();b.digits[2*this.k]=1;this.mu=biDivide(b,this.modulus);this.bkplus1=new BigInt();this.bkplus1.digits[this.k+1]=1;this.modulo=BarrettMu_modulo;this.multiplyMod=BarrettMu_multiplyMod;this.powMod=BarrettMu_powMod}function BarrettMu_modulo(h){var g=biDivideByRadixPower(h,this.k-1);var e=biMultiply(g,this.mu);var d=biDivideByRadixPower(e,this.k+1);var c=biModuloByRadixPower(h,this.k+1);var k=biMultiply(d,this.modulus);var b=biModuloByRadixPower(k,this.k+1);var a=biSubtract(c,b);if(a.isNeg){a=biAdd(a,this.bkplus1)}var f=biCompare(a,this.modulus)>=0;while(f){a=biSubtract(a,this.modulus);f=biCompare(a,this.modulus)>=0}return a}function BarrettMu_multiplyMod(a,c){var b=biMultiply(a,c);return this.modulo(b)}function BarrettMu_powMod(c,f){var b=new BigInt();b.digits[0]=1;var d=c;var e=f;while(true){if((e.digits[0]&1)!=0){b=this.multiplyMod(b,d)}e=biShiftRight(e,1);if(e.digits[0]==0&&biHighIndex(e)==0){break}d=this.multiplyMod(d,d)}return b}function encryptForm(a,g,c){var h=c.maxdigits;var f=c.e;var k=c.n;setMaxDigits(h);var b=new RSAKeyPair(f,\"\",k);$.each(a,function(d,m){var e=m.name;var l=m.value;if(l!==null&&l!==\"\"&&$.inArray(e,g)>-1){m.value=encryptedString(b,l)}});return a}; var reustl=encrypt_password(); ";
            using (JavascriptContext context = new JavascriptContext())
            {
                context.Run(rsastr);
                var result = context.GetParameter("reustl");
                return result == null ? "" : result.ToString();
            };
            ////var engine = new Engine().Execute(rsastr);
            ////var result = engine.Invoke("encrypt_password", password).ToObject();
            //return result.ToString();
        }
        public static string EncryptStringByJS_SocialSecurity_huizhou(string e, string g)
        {
            var resultkey = ExceCreateKeys(e);
            var result = ExceEncrypt(resultkey, g);
            result = ExceStringToHex(result.ToString());
            return result.ToString();
        }

        private static string ExceCreateKeys(string e)
        {

            var rsastr = @"function createKeys() {
   var e='" + e + "';";
            rsastr += @" pc2bytes0 = [0, 4, 536870912, 536870916, 65536, 65540, 536936448, 536936452, 512, 516, 536871424, 536871428, 66048, 66052, 536936960, 536936964];
    pc2bytes1 = [0, 1, 1048576, 1048577, 67108864, 67108865, 68157440, 68157441, 256, 257, 1048832, 1048833, 67109120, 67109121, 68157696, 68157697];
    pc2bytes2 = [0, 8, 2048, 2056, 16777216, 16777224, 16779264, 16779272, 0, 8, 2048, 2056, 16777216, 16777224, 16779264, 16779272];
    pc2bytes3 = [0, 2097152, 134217728, 136314880, 8192, 2105344, 134225920, 136323072, 131072, 2228224, 134348800, 136445952, 139264, 2236416, 134356992, 136454144];
    pc2bytes4 = [0, 262144, 16, 262160, 0, 262144, 16, 262160, 4096, 266240, 4112, 266256, 4096, 266240, 4112, 266256];
    pc2bytes5 = [0, 1024, 32, 1056, 0, 1024, 32, 1056, 33554432, 33555456, 33554464, 33555488, 33554432, 33555456, 33554464, 33555488];
    pc2bytes6 = [0, 268435456, 524288, 268959744, 2, 268435458, 524290, 268959746, 0, 268435456, 524288, 268959744, 2, 268435458, 524290, 268959746];
    pc2bytes7 = [0, 65536, 2048, 67584, 536870912, 536936448, 536872960, 536938496, 131072, 196608, 133120, 198656, 537001984, 537067520, 537004032, 537069568];
    pc2bytes8 = [0, 262144, 0, 262144, 2, 262146, 2, 262146, 33554432, 33816576, 33554432, 33816576, 33554434, 33816578, 33554434, 33816578];
    pc2bytes9 = [0, 268435456, 8, 268435464, 0, 268435456, 8, 268435464, 1024, 268436480, 1032, 268436488, 1024, 268436480, 1032, 268436488];
    pc2bytes10 = [0, 32, 0, 32, 1048576, 1048608, 1048576, 1048608, 8192, 8224, 8192, 8224, 1056768, 1056800, 1056768, 1056800];
    pc2bytes11 = [0, 16777216, 512, 16777728, 2097152, 18874368, 2097664, 18874880, 67108864, 83886080, 67109376, 83886592, 69206016, 85983232, 69206528, 85983744];
    pc2bytes12 = [0, 4096, 134217728, 134221824, 524288, 528384, 134742016, 134746112, 16, 4112, 134217744, 134221840, 524304, 528400, 134742032, 134746128];
    pc2bytes13 = [0, 4, 256, 260, 0, 4, 256, 260, 1, 5, 257, 261, 1, 5, 257, 261];
    e += String.fromCharCode(104);
    e += String.fromCharCode(110);
    e += String.fromCharCode(105);
    e += String.fromCharCode(115);
    e += String.fromCharCode(105);
    for (var g = e.length >= 24 ? 3 : 1, j = Array(32 * g), k = [0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0], f, m, h = 0, r = 0, d, s = 0; s < g; s++) {
        left = e.charCodeAt(h++) << 24 | e.charCodeAt(h++) << 16 | e.charCodeAt(h++) << 8 | e.charCodeAt(h++);
        right = e.charCodeAt(h++) << 24 | e.charCodeAt(h++) << 16 | e.charCodeAt(h++) << 8 | e.charCodeAt(h++);
        d = (left >>> 4 ^ right) & 252645135;
        right ^= d;
        left ^= d << 4;
        d = (right >>> -16 ^ left) & 65535;
        left ^= d;
        right ^= d << -16;
        d = (left >>> 2 ^ right) & 858993459;
        right ^= d;
        left ^= d << 2;
        d = (right >>> -16 ^ left) & 65535;
        left ^= d;
        right ^= d << -16;
        d = (left >>> 1 ^ right) & 1431655765;
        right ^= d;
        left ^= d << 1;
        d = (right >>> 8 ^ left) & 16711935;
        left ^= d;
        right ^= d << 8;
        d = (left >>> 1 ^ right) & 1431655765;
        right ^= d;
        left ^= d << 1;
        d = left << 8 | right >>> 20 & 240;
        left = right << 24 | right << 8 & 16711680 | right >>> 8 & 65280 | right >>> 24 & 240;
        right = d;
        for (i = 0; i < k.length; i++) {
            if (k[i]) {
                left = left << 2 | left >>> 26;
                right = right << 2 | right >>> 26
            } else {
                left = left << 1 | left >>> 27;
                right = right << 1 | right >>> 27
            }
            left &= -15;
            right &= -15;
            f = pc2bytes0[left >>> 28] | pc2bytes1[left >>> 24 & 15] | pc2bytes2[left >>> 20 & 15] | pc2bytes3[left >>> 16 & 15] | pc2bytes4[left >>> 12 & 15] | pc2bytes5[left >>> 8 & 15] | pc2bytes6[left >>> 4 & 15];
            m = pc2bytes7[right >>> 28] | pc2bytes8[right >>> 24 & 15] | pc2bytes9[right >>> 20 & 15] | pc2bytes10[right >>> 16 & 15] | pc2bytes11[right >>> 12 & 15] | pc2bytes12[right >>> 8 & 15] | pc2bytes13[right >>> 4 & 15];
            d = (m >>> 16 ^ f) & 65535;
            j[r++] = f ^ d;
            j[r++] = m ^ d << 16
        }
    }
    return j
   };

var createKeysresult=createKeys();
";
            using (JavascriptContext context = new JavascriptContext())
            {
                context.Run(rsastr);
                object[] result = (object[])context.GetParameter("createKeysresult");

                string restr = String.Join(",", result);
                restr = "[" + restr + "]";
                return restr;
            };

        }

        private static string ExceStringToHex(string e)
        {
            string restr = @"function stringToHex() {
           var e='" + e + "';";
            restr += @"for (var g = '', j ='', k = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'], f = 0; f < e.length; f++)
                if (f == 0) g = e.charCodeAt(f);
                else g += ':' + e.charCodeAt(f);
            for (f = 0; f < g.length; f++) j += k[g.charCodeAt(f) >> 4] + k[g.charCodeAt(f) & 15];
            return j
        };
        var stringToHexresult=stringToHex()
   ";
            using (JavascriptContext context = new JavascriptContext())
            {
                context.Run(restr);
                var result = context.GetParameter("stringToHexresult");
                return result == null ? "" : result.ToString();
            };
        }

        private static string ExceEncrypt(string e, string g)
        {
            var rsastr = @"function encrypt() {
      var e=" + e + ";var g='" + g + "'; ";
            rsastr += @"var j = [16843776, 0, 65536, 16843780, 16842756, 66564, 4, 65536, 1024, 16843776, 16843780, 1024, 16778244, 16842756, 16777216, 4, 1028, 16778240, 16778240, 66560, 66560, 16842752, 16842752, 16778244, 65540, 16777220, 16777220, 65540, 0, 1028, 66564, 16777216, 65536, 16843780, 4, 16842752, 16843776, 16777216, 16777216, 1024, 16842756, 65536, 66560, 16777220, 1024, 4, 16778244, 66564, 16843780, 65540, 16842752, 16778244, 16777220, 1028, 66564, 16843776, 1028, 16778240, 16778240, 0, 65540, 66560, 0, 16842756],
        k = [-2146402272, -2147450880, 32768, 1081376, 1048576, 32, -2146435040, -2147450848, -2147483616, -2146402272, -2146402304, -2147483648, -2147450880, 1048576, 32, -2146435040, 1081344, 1048608, -2147450848, 0, -2147483648, 32768, 1081376, -2146435072, 1048608, -2147483616, 0, 1081344, 32800, -2146402304, -2146435072, 32800, 0, 1081376, -2146435040, 1048576, -2147450848, -2146435072, -2146402304, 32768, -2146435072, -2147450880, 32, -2146402272, 1081376, 32, 32768, -2147483648, 32800, -2146402304, 1048576, -2147483616, 1048608, -2147450848, -2147483616, 1048608, 1081344, 0, -2147450880, 32800, -2147483648, -2146435040, -2146402272, 1081344],
        f = [520, 134349312, 0, 134348808, 134218240, 0, 131592, 134218240, 131080, 134217736, 134217736, 131072, 134349320, 131080, 134348800, 520, 134217728, 8, 134349312, 512, 131584, 134348800, 134348808, 131592, 134218248, 131584, 131072, 134218248, 8, 134349320, 512, 134217728, 134349312, 134217728, 131080, 520, 131072, 134349312, 134218240, 0, 512, 131080, 134349320, 134218240, 134217736, 512, 0, 134348808, 134218248, 131072, 134217728, 134349320, 8, 131592, 131584, 134217736, 134348800, 134218248, 520, 134348800, 131592, 8, 134348808, 131584],
        m = [8396801, 8321, 8321, 128, 8396928, 8388737, 8388609, 8193, 0, 8396800, 8396800, 8396929, 129, 0, 8388736, 8388609, 1, 8192, 8388608, 8396801, 128, 8388608, 8193, 8320, 8388737, 1, 8320, 8388736, 8192, 8396928, 8396929, 129, 8388736, 8388609, 8396800, 8396929, 129, 0, 0, 8396800, 8320, 8388736, 8388737, 1, 8396801, 8321, 8321, 128, 8396929, 129, 1, 8192, 8388609, 8193, 8396928, 8388737, 8193, 8320, 8388608, 8396801, 128, 8388608, 8192, 8396928],
        h = [256, 34078976, 34078720, 1107296512, 524288, 256, 1073741824, 34078720, 1074266368, 524288, 33554688, 1074266368, 1107296512, 1107820544, 524544, 1073741824, 33554432, 1074266112, 1074266112, 0, 1073742080, 1107820800, 1107820800, 33554688, 1107820544, 1073742080, 0, 1107296256, 34078976, 33554432, 1107296256, 524544, 524288, 1107296512, 256, 33554432, 1073741824, 34078720, 1107296512, 1074266368, 33554688, 1073741824, 1107820544, 34078976, 1074266368, 256, 33554432, 1107820544, 1107820800, 524544, 1107296256, 1107820800, 34078720, 0, 1074266112, 1107296256, 524544, 33554688, 1073742080, 524288, 0, 1074266112, 34078976, 1073742080],
        r = [536870928, 541065216, 16384, 541081616, 541065216, 16, 541081616, 4194304, 536887296, 4210704, 4194304, 536870928, 4194320, 536887296, 536870912, 16400, 0, 4194320, 536887312, 16384, 4210688, 536887312, 16, 541065232, 541065232, 0, 4210704, 541081600, 16400, 4210688, 541081600, 536870912, 536887296, 16, 541065232, 4210688, 541081616, 4194304, 16400, 536870928, 4194304, 536887296, 536870912, 16400, 536870928, 541081616, 4210688, 541065216, 4210704, 541081600, 0, 541065232, 16, 16384, 541065216, 4210704, 16384, 4194320, 536887312, 0, 541081600, 536870912, 4194320, 536887312],
        d = [2097152, 69206018, 67110914, 0, 2048, 67110914, 2099202, 69208064, 69208066, 2097152, 0, 67108866, 2, 67108864, 69206018, 2050, 67110912, 2099202, 2097154, 67110912, 67108866, 69206016, 69208064, 2097154, 69206016, 2048, 2050, 69208066, 2099200, 2, 67108864, 2099200, 67108864, 2099200, 2097152, 67110914, 67110914, 69206018, 69206018, 2, 2097154, 67108864, 67110912, 2097152, 69208064, 2050, 2099202, 69208064, 2050, 67108866, 69208066, 69206016, 2099200, 0, 2, 69208066, 0, 2099202, 69206016, 2048, 67108866, 67110912, 2048, 2097154],
        s = [268439616, 4096, 262144, 268701760, 268435456, 268439616, 64, 268435456, 262208, 268697600, 268701760, 266240, 268701696, 266304, 4096, 64, 268697600, 268435520, 268439552, 4160, 266240, 262208, 268697664, 268701696, 4160, 0, 0, 268697664, 268435520, 268439552, 266304, 262144, 266304, 262144, 268701696, 4096, 64, 268697664, 4096, 266304, 268439552, 64, 268435520, 268697600, 268697664, 268435456, 262144, 268439616, 0, 268701760, 262208, 268435520, 268697600, 268439552, 268439616, 0, 268701760, 266240, 266240, 4160, 4160, 262208, 268435456, 268701696],
        u = e,
        n = 0,
        o, l, c, p, q, b, a, t, w, x, z = g.length,
        v = 0,
        y = u.length == 32 ? 3 : 9;
    t = y == 3 ? [0, 32, 2] : [0, 32, 2, 62, 30, -2, 64, 96, 2];
    g += '\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000';
    for (tempresult = result = ''; n < z;) {
        b = g.charCodeAt(n++) << 16 | g.charCodeAt(n++);
        a = g.charCodeAt(n++) << 16 | g.charCodeAt(n++);
        c = (b >>> 4 ^ a) & 252645135;
        a ^= c;
        b ^= c << 4;
        c = (b >>> 16 ^ a) & 65535;
        a ^= c;
        b ^= c << 16;
        c = (a >>> 2 ^ b) & 858993459;
        b ^= c;
        a ^= c << 2;
        c = (a >>> 8 ^ b) & 16711935;
        b ^= c;
        a ^= c << 8;
        c = (b >>> 1 ^ a) & 1431655765;
        a ^= c;
        b ^= c << 1;
        b = b << 1 | b >>> 31;
        a = a << 1 | a >>> 31;
        for (l = 0; l < y; l += 3) {
            w = t[l + 1];
            x = t[l + 2];
            for (o = t[l]; o != w; o += x) {
                p = a ^ u[o];
                q = (a >>> 4 | a << 28) ^ u[o + 1];
                c = b;
                b = a;
                a = c ^ (k[p >>> 24 & 63] | m[p >>> 16 & 63] | r[p >>> 8 & 63] | s[p & 63] | j[q >>> 24 & 63] | f[q >>> 16 & 63] | h[q >>> 8 & 63] | d[q & 63])
            }
            c = b;
            b = a;
            a = c
        }
        b = b >>> 1 | b << 31;
        a = a >>> 1 | a << 31;
        c = (b >>> 1 ^ a) & 1431655765;
        a ^= c;
        b ^= c << 1;
        c = (a >>> 8 ^ b) & 16711935;
        b ^= c;
        a ^= c << 8;
        c = (a >>> 2 ^ b) & 858993459;
        b ^= c;
        a ^= c << 2;
        c = (b >>> 16 ^ a) & 65535;
        a ^= c;
        b ^= c << 16;
        c = (b >>> 4 ^ a) & 252645135;
        a ^= c;
        b ^= c << 4;
        tempresult += String.fromCharCode(b >>> 24, b >>> 16 & 255, b >>> 8 & 255, b & 255, a >>> 24, a >>> 16 & 255, a >>> 8 & 255, a & 255);
        v += 16;
        if (v == 512) {
            result += tempresult;
            tempresult = '';
            v = 0
        }
    }
    return result + tempresult
}

var encryptrestult=encrypt();
";
            using (JavascriptContext context = new JavascriptContext())
            {
                context.Run(rsastr);
                var result = context.GetParameter("encryptrestult");
                return result == null ? "" : result.ToString();
            };

        }
        #endregion

        //public static string EncryptStringByJS_SocialSecurity_huizhou1(string e, string g)
        //{
        //    var funstr = "function encrypt(e,g){var j=[16843776,0,65536,16843780,16842756,66564,4,65536,1024,16843776,16843780,1024,16778244,16842756,16777216,4,1028,16778240,16778240,66560,66560,16842752,16842752,16778244,65540,16777220,16777220,65540,0,1028,66564,16777216,65536,16843780,4,16842752,16843776,16777216,16777216,1024,16842756,65536,66560,16777220,1024,4,16778244,66564,16843780,65540,16842752,16778244,16777220,1028,66564,16843776,1028,16778240,16778240,0,65540,66560,0,16842756],k=[-2146402272,-2147450880,32768,1081376,1048576,32,-2146435040,-2147450848,-2147483616,-2146402272,-2146402304,-2147483648,-2147450880,1048576,32,-2146435040,1081344,1048608,-2147450848,0,-2147483648,32768,1081376,-2146435072,1048608,-2147483616,0,1081344,32800,-2146402304,-2146435072,32800,0,1081376,-2146435040,1048576,-2147450848,-2146435072,-2146402304,32768,-2146435072,-2147450880,32,-2146402272,1081376,32,32768,-2147483648,32800,-2146402304,1048576,-2147483616,1048608,-2147450848,-2147483616,1048608,1081344,0,-2147450880,32800,-2147483648,-2146435040,-2146402272,1081344],f=[520,134349312,0,134348808,134218240,0,131592,134218240,131080,134217736,134217736,131072,134349320,131080,134348800,520,134217728,8,134349312,512,131584,134348800,134348808,131592,134218248,131584,131072,134218248,8,134349320,512,134217728,134349312,134217728,131080,520,131072,134349312,134218240,0,512,131080,134349320,134218240,134217736,512,0,134348808,134218248,131072,134217728,134349320,8,131592,131584,134217736,134348800,134218248,520,134348800,131592,8,134348808,131584],m=[8396801,8321,8321,128,8396928,8388737,8388609,8193,0,8396800,8396800,8396929,129,0,8388736,8388609,1,8192,8388608,8396801,128,8388608,8193,8320,8388737,1,8320,8388736,8192,8396928,8396929,129,8388736,8388609,8396800,8396929,129,0,0,8396800,8320,8388736,8388737,1,8396801,8321,8321,128,8396929,129,1,8192,8388609,8193,8396928,8388737,8193,8320,8388608,8396801,128,8388608,8192,8396928],h=[256,34078976,34078720,1107296512,524288,256,1073741824,34078720,1074266368,524288,33554688,1074266368,1107296512,1107820544,524544,1073741824,33554432,1074266112,1074266112,0,1073742080,1107820800,1107820800,33554688,1107820544,1073742080,0,1107296256,34078976,33554432,1107296256,524544,524288,1107296512,256,33554432,1073741824,34078720,1107296512,1074266368,33554688,1073741824,1107820544,34078976,1074266368,256,33554432,1107820544,1107820800,524544,1107296256,1107820800,34078720,0,1074266112,1107296256,524544,33554688,1073742080,524288,0,1074266112,34078976,1073742080],r=[536870928,541065216,16384,541081616,541065216,16,541081616,4194304,536887296,4210704,4194304,536870928,4194320,536887296,536870912,16400,0,4194320,536887312,16384,4210688,536887312,16,541065232,541065232,0,4210704,541081600,16400,4210688,541081600,536870912,536887296,16,541065232,4210688,541081616,4194304,16400,536870928,4194304,536887296,536870912,16400,536870928,541081616,4210688,541065216,4210704,541081600,0,541065232,16,16384,541065216,4210704,16384,4194320,536887312,0,541081600,536870912,4194320,536887312],d=[2097152,69206018,67110914,0,2048,67110914,2099202,69208064,69208066,2097152,0,67108866,2,67108864,69206018,2050,67110912,2099202,2097154,67110912,67108866,69206016,69208064,2097154,69206016,2048,2050,69208066,2099200,2,67108864,2099200,67108864,2099200,2097152,67110914,67110914,69206018,69206018,2,2097154,67108864,67110912,2097152,69208064,2050,2099202,69208064,2050,67108866,69208066,69206016,2099200,0,2,69208066,0,2099202,69206016,2048,67108866,67110912,2048,2097154],s=[268439616,4096,262144,268701760,268435456,268439616,64,268435456,262208,268697600,268701760,266240,268701696,266304,4096,64,268697600,268435520,268439552,4160,266240,262208,268697664,268701696,4160,0,0,268697664,268435520,268439552,266304,262144,266304,262144,268701696,4096,64,268697664,4096,266304,268439552,64,268435520,268697600,268697664,268435456,262144,268439616,0,268701760,262208,268435520,268697600,268439552,268439616,0,268701760,266240,266240,4160,4160,262208,268435456,268701696],u=e,n=0,o,l,c,p,q,b,a,t,w,x,z=g.length,v=0,y=u.length==32?3:9;t=y==3?[0,32,2]:[0,32,2,62,30,-2,64,96,2];g+=\"\\u0000\\u0000\\u0000\\u0000\\u0000\\u0000\\u0000\\u0000\";for(tempresult=result=\"\";n<z;){b=g.charCodeAt(n++)<<16|g.charCodeAt(n++);a=g.charCodeAt(n++)<<16|g.charCodeAt(n++);c=(b>>>4^a)&252645135;a^=c;b^=c<<4;c=(b>>>16^a)&65535;a^=c;b^=c<<16;c=(a>>>2^b)&858993459;b^=c;a^=c<<2;c=(a>>>8^b)&16711935;b^=c;a^=c<<8;c=(b>>>1^a)&1431655765;a^=c;b^=c<<1;b=b<<1|b>>>31;a=a<<1|a>>>31;for(l=0;l<y;l+=3){w=t[l+1];x=t[l+2];for(o=t[l];o!=w;o+=x){p=a^u[o];q=(a>>>4|a<<28)^u[o+1];c=b;b=a;a=c^(k[p>>>24&63]|m[p>>>16&63]|r[p>>>8&63]|s[p&63]|j[q>>>24&63]|f[q>>>16&63]|h[q>>>8&63]|d[q&63])}c=b;b=a;a=c}b=b>>>1|b<<31;a=a>>>1|a<<31;c=(b>>>1^a)&1431655765;a^=c;b^=c<<1;c=(a>>>8^b)&16711935;b^=c;a^=c<<8;c=(a>>>2^b)&858993459;b^=c;a^=c<<2;c=(b>>>16^a)&65535;a^=c;b^=c<<16;c=(b>>>4^a)&252645135;a^=c;b^=c<<4;tempresult+=String.fromCharCode(b>>>24,b>>>16&255,b>>>8&255,b&255,a>>>24,a>>>16&255,a>>>8&255,a&255);v+=16;if(v==512){result+=tempresult;tempresult=\"\";v=0}}return result+tempresult}function createKeys(e){pc2bytes0=[0,4,536870912,536870916,65536,65540,536936448,536936452,512,516,536871424,536871428,66048,66052,536936960,536936964];pc2bytes1=[0,1,1048576,1048577,67108864,67108865,68157440,68157441,256,257,1048832,1048833,67109120,67109121,68157696,68157697];pc2bytes2=[0,8,2048,2056,16777216,16777224,16779264,16779272,0,8,2048,2056,16777216,16777224,16779264,16779272];pc2bytes3=[0,2097152,134217728,136314880,8192,2105344,134225920,136323072,131072,2228224,134348800,136445952,139264,2236416,134356992,136454144];pc2bytes4=[0,262144,16,262160,0,262144,16,262160,4096,266240,4112,266256,4096,266240,4112,266256];pc2bytes5=[0,1024,32,1056,0,1024,32,1056,33554432,33555456,33554464,33555488,33554432,33555456,33554464,33555488];pc2bytes6=[0,268435456,524288,268959744,2,268435458,524290,268959746,0,268435456,524288,268959744,2,268435458,524290,268959746];pc2bytes7=[0,65536,2048,67584,536870912,536936448,536872960,536938496,131072,196608,133120,198656,537001984,537067520,537004032,537069568];pc2bytes8=[0,262144,0,262144,2,262146,2,262146,33554432,33816576,33554432,33816576,33554434,33816578,33554434,33816578];pc2bytes9=[0,268435456,8,268435464,0,268435456,8,268435464,1024,268436480,1032,268436488,1024,268436480,1032,268436488];pc2bytes10=[0,32,0,32,1048576,1048608,1048576,1048608,8192,8224,8192,8224,1056768,1056800,1056768,1056800];pc2bytes11=[0,16777216,512,16777728,2097152,18874368,2097664,18874880,67108864,83886080,67109376,83886592,69206016,85983232,69206528,85983744];pc2bytes12=[0,4096,134217728,134221824,524288,528384,134742016,134746112,16,4112,134217744,134221840,524304,528400,134742032,134746128];pc2bytes13=[0,4,256,260,0,4,256,260,1,5,257,261,1,5,257,261];e+=String.fromCharCode(104);e+=String.fromCharCode(110);e+=String.fromCharCode(105);e+=String.fromCharCode(115);e+=String.fromCharCode(105);for(var g=e.length>=24?3:1,j=Array(32*g),k=[0,0,1,1,1,1,1,1,0,1,1,1,1,1,1,0],f,m,h=0,r=0,d,s=0;s<g;s++){left=e.charCodeAt(h++)<<24|e.charCodeAt(h++)<<16|e.charCodeAt(h++)<<8|e.charCodeAt(h++);right=e.charCodeAt(h++)<<24|e.charCodeAt(h++)<<16|e.charCodeAt(h++)<<8|e.charCodeAt(h++);d=(left>>>4^right)&252645135;right^=d;left^=d<<4;d=(right>>>-16^left)&65535;left^=d;right^=d<<-16;d=(left>>>2^right)&858993459;right^=d;left^=d<<2;d=(right>>>-16^left)&65535;left^=d;right^=d<<-16;d=(left>>>1^right)&1431655765;right^=d;left^=d<<1;d=(right>>>8^left)&16711935;left^=d;right^=d<<8;d=(left>>>1^right)&1431655765;right^=d;left^=d<<1;d=left<<8|right>>>20&240;left=right<<24|right<<8&16711680|right>>>8&65280|right>>>24&240;right=d;for(i=0;i<k.length;i++){if(k[i]){left=left<<2|left>>>26;right=right<<2|right>>>26}else{left=left<<1|left>>>27;right=right<<1|right>>>27}left&=-15;right&=-15;f=pc2bytes0[left>>>28]|pc2bytes1[left>>>24&15]|pc2bytes2[left>>>20&15]|pc2bytes3[left>>>16&15]|pc2bytes4[left>>>12&15]|pc2bytes5[left>>>8&15]|pc2bytes6[left>>>4&15];m=pc2bytes7[right>>>28]|pc2bytes8[right>>>24&15]|pc2bytes9[right>>>20&15]|pc2bytes10[right>>>16&15]|pc2bytes11[right>>>12&15]|pc2bytes12[right>>>8&15]|pc2bytes13[right>>>4&15];d=(m>>>16^f)&65535;j[r++]=f^d;j[r++]=m^d<<16}}return j}function stringToHex(e){for(var g=\"\",j=\"\",k=[\"0\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\",\"8\",\"9\",\"a\",\"b\",\"c\",\"d\",\"e\",\"f\"],f=0;f<e.length;f++)if(f==0)g=e.charCodeAt(f);else g+=\":\"+e.charCodeAt(f);for(f=0;f<g.length;f++)j+=k[g.charCodeAt(f)>>4]+k[g.charCodeAt(f)&15];return j};";
        //    var engine = new Engine().Execute(funstr);

        //    var resultkey = engine.Invoke("createKeys", e).ToObject();
        //    var result = engine.Invoke("encrypt", resultkey, g).ToObject();
        //    result = engine.Invoke("stringToHex", result.ToString()).ToObject();
        //    return result.ToString();
        //}

    }

}
