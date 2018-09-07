using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Vcredit.ExtTrade.CommonLayer.helper
{
    public class AnalysisVCode
    {
        public static string GetVerifycode(Byte[] bytes)
        {
            string srcCode = string.Empty;
            Bitmap map = null; // (Bitmap)Image.FromFile(fullPath);
            map = new Bitmap(Image.FromStream(new MemoryStream(bytes)));
            try
            {
                //tesseract->SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                //tesseract->SetVariable("language_model_penalty_non_freq_dict_word", "0");
                //tesseract->SetVariable("language_model_penalty_non_dict_word ", "0");
                //tesseract->SetVariable("tessedit_char_blacklist", "xyz");
                //tesseract->SetVariable("classify_bln_numeric_mode", "1");

                tessnet2.Tesseract ocr = new tessnet2.Tesseract();//声明一个OCR类
                string defaultCharList = "0123456789abcdefghigklmnopqrstuvwxyzABCDEFGHIGKLMNOPQRSTUVWXWZ";
                defaultCharList = "abcdefghigklmnopqrstuvwxyzABCDEFGHIGKLMNOPQRSTUVWXWZ";

                //设置识别变量，白名单，当前只能识别数字及英文字符。
                //ocr.SetVariable("tessedit_char_blacklist", "1");//设置黑名单
                //ocr.SetVariable("classify_bln_numeric_mode", "1");
                //ocr.SetVariable("language_model_penalty_non_dict_word", "r");
                ocr.SetVariable("tessedit_char_whitelist", defaultCharList);

                var rootPath = ConfigData.AnalysisVCodeTempPath;

                var tmpePath = Path.Combine(rootPath, "tmpe");
                if (!Directory.Exists(tmpePath))
                    throw new ArgumentException("请配置正确的 AnalysisVCodeTempPath");

                ocr.Init(tmpePath, "eng", false); //应用当前语言包。注，Tessnet2是支持多国语的。语言包下载链接：http://code.google.com/p/tesseract-ocr/downloads/list
                List<tessnet2.Word> result = new List<tessnet2.Word>();
                result = ocr.DoOCR(map, Rectangle.Empty);//执行识别操作
                foreach (tessnet2.Word word in result)//遍历识别结果。
                {
                    srcCode += word.Text;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return srcCode;
        }

    }
}
