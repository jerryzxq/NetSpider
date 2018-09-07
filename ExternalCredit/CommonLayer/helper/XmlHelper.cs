using System.Xml;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using System.Text;

namespace Vcredit.ExtTrade.CommonLayer
{
    /// <summary>
    /// Xml的操作公共类
    /// </summary>    
    public class XmlHelper
    {
        #region 字段定义
        /// <summary>
        /// Xml文档
        /// </summary>
        private XmlDocument _xml;
        /// <summary>
        /// XML的根节点
        /// </summary>
        private XmlElement _rootelement;
        /// <summary>
        /// 当前操作的xml节点
        /// </summary>
        private XmlNode currentXmlNode;
        #endregion

        #region 构造方法
        /// <summary>
        /// 实例化XmlHelper对象
        /// </summary>
        /// <param name="xmlFilePath">Xml文件的绝对相对路径</param>
        public XmlHelper(string xmlstr)
        {
            CreateXMLElementWithXml(xmlstr);
        }
  
        #endregion

        #region 创建XML的根节点
        /// <summary>
        /// 创建XML的根节点
        /// </summary>
        private void CreateXMLElement(string filePath)
        {
            //创建一个XML对象
            _xml = new XmlDocument();

            if (File.Exists(filePath))
            {
                //加载XML文件
                _xml.LoadXml(File.ReadAllText(filePath,System.Text.Encoding.GetEncoding("GBK")));
            }

            //为XML的根节点赋值
            _rootelement = _xml.DocumentElement;
            currentXmlNode = _rootelement.Clone();
        }
        private void CreateXMLElementWithXml(string xml)
        {
            //创建一个XML对象
            _xml = new XmlDocument();

            //加载XML文件
            _xml.LoadXml(xml);

            //为XML的根节点赋值
            _rootelement = _xml.DocumentElement;
            currentXmlNode = _rootelement.Clone();
        }
        #endregion

        #region 获取指定XPath表达式的节点对象
        /// <summary>
        /// 获取指定XPath表达式的节点对象
        /// 
        /// </summary>        
        /// <param name="xPath">XPath表达式,
        /// 范例1: @"Skill/First/SkillItem", 等效于 @"//Skill/First/SkillItem"
        /// 范例2: @"Table[USERNAME='a']" , []表示筛选,USERNAME是Table下的一个子节点.
        /// 范例3: @"ApplyPost/Item[@itemName='岗位编号']",@itemName是Item节点的属性.
        /// </param>
        public XmlNode GetNode(string xPath)
        {
            //返回XPath节点
            return _rootelement.SelectSingleNode(xPath);
        }
        public XmlNode GetNode(XmlNode node,string path)
        {
           return   node.SelectSingleNode(path);
        }
        public XmlNodeList GetNodeList(XmlNode node, string path)
        {
            return node.SelectNodes(path);
        }
        public string GetNodeValue(XmlNode node, string path)
        {
             var xmlnode= node.SelectSingleNode(path);
             if (xmlnode == null)
                 return  null;
             return xmlnode.InnerText;
        }
        
        public XmlNode SetAndReturnCurrentXmlNode(string xPath)
        {
            currentXmlNode = GetNode(xPath);
            return currentXmlNode;
        }
  
        public string GetCurrentXmlNodeValue(string xPath)
        {
          var xmlnode=currentXmlNode.SelectSingleNode(xPath);
            if (xmlnode == null)
                return string.Empty;
            return xmlnode.InnerText;
           
        }
        public XmlNodeList GetCurrentXmlNodeNodeList(string xPath)
        {
            return currentXmlNode.SelectNodes(xPath);
        }
        public XmlNode GetCurrentXmlNodeNode(string xPath)
        {
            return currentXmlNode.SelectSingleNode(xPath);
        }
        // <summary>  
        /// 对象序列化成 XML String  
        /// </summary>  
        public static string XmlSerialize<T>(T obj)
        {
            //MemoryStream ms = new MemoryStream();
            //StreamWriter textWriter = new StreamWriter(ms, Encoding.UTF8);
            //string xmlMessage = null;
            //try
            //{

            //    XmlSerializer serializer = new XmlSerializer(typeof(T));
            //    serializer.Serialize(textWriter, obj);
            //    xmlMessage = Encoding.UTF8.GetString(ms.GetBuffer());

            //}
            //finally
            //{
            //    ms.Close();
            //    textWriter.Close();

            //}
            //return xmlMessage;
            string xmlString = string.Empty;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, obj);
                xmlString = Encoding.UTF8.GetString(ms.ToArray());
            }
            return xmlString;
        }  
        public void SaveToXMLFile(string fileName)
        {
            _xml.Save(fileName);
        }

        #endregion

      



    
    }
}
