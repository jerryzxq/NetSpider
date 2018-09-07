/*********************************************  
 * * 功能描述： Xml文件处理工具类
 * * 创 建 人:  张志博
 * * 日    期:  2014/9/19
 * * 修 改 人:  
 * * 修改日期: 
 * * 修改描述:  
 * * 版    本:  1.0
 * *******************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Xml;

namespace Vcredit.Common
{
    /// <summary>
    /// XmlHelper 的摘要说明
    /// </summary>
    public class XmlHelper
    {
        private string msg;
        public XmlHelper()
        {
        }
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，非空时返回该属性值，否则返回串联值</param>
        /// <returns>string</returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Read(path, "/Node", "")
         * XmlHelper.Read(path, "/Node/Element[@Attribute='Name']", "Attribute")
         ************************************************/
        public static string Read(string path, string node, string key, string value)
        {
            string str1 = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList nodeList = doc.SelectSingleNode(node).ChildNodes;//获取NewDataSet节点的所有子节点
                foreach (XmlNode xn in nodeList)//遍历所有子节点
                {
                    XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                    if (xe.GetAttribute("key") == key)
                    {
                        str1 = xe.GetAttribute(value);
                        continue;
                    }
                }
                return str1;
            }
            catch (Exception ex) { string aaa = ex.ToString(); }
            return value;
        }
        public static List<string> ReadList(string path, string node, string key, string value)
        {
            string str1 = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList nodeList = doc.SelectSingleNode(node).ChildNodes;//获取NewDataSet节点的所有子节点
                List<string> list = new List<string>();
                foreach (XmlNode xn in nodeList)//遍历所有子节点
                {
                    XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                    if (key.Length > 0)
                    {
                        if (xe.GetAttribute("key") == key)
                        {
                            str1 = xe.GetAttribute(value);
                            list.Add(str1);
                            continue;
                        }
                    }
                    else
                    {
                        str1 = xe.GetAttribute(value);
                        list.Add(str1);
                    }
                }
                return list;
            }
            catch (Exception ex) { string aaa = ex.ToString(); }
            return new List<string>();
        }
        /// <summary>
        /// 创建一个XML文档，成功创建后操作路径将直接指向该文件
        /// </summary>
        /// <param name="fileName">文件物理路径名</param>
        /// <param name="xmlString">xml字符串</param>
        public static void CreateXmlFile(string fileName, string xmlString)
        {
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.LoadXml(xmlString);
                xDoc.Save(fileName);

            }
            catch { throw; }
        }
        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="node"></param>
        /// <param name="node1"></param>
        /// <param name="element"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public static void Insert(string path, string node, string node1, string element, string attribute, string value, string attribute1, string value1)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode xn = doc.SelectSingleNode(node); //查找根node节点
                XmlElement xel = doc.CreateElement(node1);
                XmlElement xe = doc.CreateElement(element);
                if (attribute.Equals(""))
                    xe.InnerText = value;
                else
                    xe.SetAttribute(attribute, value);
                xe.SetAttribute(attribute1, value1);
                xel.AppendChild(xe);
                xn.AppendChild(xel);
                //}
                doc.Save(path);
            }
            catch { }
        }
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="element">元素名，非空时插入新元素，否则在该元素中插入属性</param>
        /// <param name="attribute">属性名，非空时插入该元素属性值，否则插入元素值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Insert(path, "/Node", "Element", "", "Value")
         * XmlHelper.Insert(path, "/Node", "Element", "Attribute", "Value")
         * XmlHelper.Insert(path, "/Node", "", "Attribute", "Value")
         ************************************************/
        public static void Insert(string path, string node, string element, string key, string value, string info)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode xn = doc.SelectSingleNode(node);
                if (element.Equals(""))
                {
                    //if (!attribute.Equals(""))
                    //{
                    //    XmlElement xe = (XmlElement)xn;
                    //    xe.SetAttribute(attribute, value);
                    //}
                }
                else
                {
                    XmlElement xe = doc.CreateElement(element);
                    //if (attribute.Equals(""))
                    //    xe.InnerText = value;
                    //else
                    xe.SetAttribute("key", key);
                    xe.SetAttribute("value", value);
                    xe.SetAttribute("info", info);
                    xn.AppendChild(xe);
                }
                doc.Save(path);
            }
            catch (Exception ex)
            {
                string aaa = ex.ToString();
            }
        }
        public static void InsertNopath(XmlDocument doc, string node, string element, string key, string value, string info)
        {
            try
            {
                XmlNode xn = doc.SelectSingleNode(node);
                if (element.Equals(""))
                {
                }
                else
                {
                    XmlElement xe = doc.CreateElement(element);
                    xe.SetAttribute("key", key);
                    xe.SetAttribute("value", value);
                    xe.SetAttribute("info", info);
                    xn.AppendChild(xe);
                }
            }
            catch (Exception ex)
            {
                string aaa = ex.ToString();
            }
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，非空时修改该节点属性值，否则修改节点值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Insert(path, "/Node", "", "Value")
         * XmlHelper.Insert(path, "/Node", "Attribute", "Value")
         ************************************************/
        public static void UpdateAfterLoad(XmlDocument doc, string node, string element, string key, string value, string info)
        {
            try
            {

                XmlNodeList nodeList = doc.SelectSingleNode(node).ChildNodes;//获取NewDataSet节点的所有子节点
                foreach (XmlNode xn in nodeList)//遍历所有子节点
                {
                    XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                    if (xe.GetAttribute("key") == key)
                    {
                        xe.SetAttribute("key", key);
                        xe.SetAttribute("value", value);
                        xe.SetAttribute("info", info);

                        continue;
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// 先加载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static XmlDocument Loadpath(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return doc;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="path"></param>
        /// <param name="doc"></param>
        public static void Savepath(string path, XmlDocument doc)
        {
            try
            {
                doc.Save(path);
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 用于批量修改修改数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，非空时修改该节点属性值，否则修改节点值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Insert(path, "/Node", "", "Value")
         * XmlHelper.Insert(path, "/Node", "Attribute", "Value")
         ************************************************/
        public static void UpdateByDom(string path, string node, string element, string key, string value, string info)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList nodeList = doc.SelectSingleNode(node).ChildNodes;//获取NewDataSet节点的所有子节点
                foreach (XmlNode xn in nodeList)//遍历所有子节点
                {
                    XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                    if (xe.GetAttribute("key") == key)
                    {
                        xe.SetAttribute("key", key);
                        xe.SetAttribute("value", value);
                        xe.SetAttribute("info", info);
                        doc.Save(path);
                        continue;
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，非空时修改该节点属性值，否则修改节点值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Insert(path, "/Node", "", "Value")
         * XmlHelper.Insert(path, "/Node", "Attribute", "Value")
         ************************************************/
        public static void Update(string path, string node, string element, string key, string value, string info)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNodeList nodeList = doc.SelectSingleNode(node).ChildNodes;//获取NewDataSet节点的所有子节点
                foreach (XmlNode xn in nodeList)//遍历所有子节点
                {
                    XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                    if (xe.GetAttribute("key") == key)
                    {
                        xe.SetAttribute("key", key);
                        xe.SetAttribute("value", value);
                        xe.SetAttribute("info", info);
                        doc.Save(path);
                        continue;
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="node">节点</param>
        /// <param name="attribute">属性名，非空时删除该节点属性值，否则删除节点值</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        /**************************************************
         * 使用示列:
         * XmlHelper.Delete(path, "/Node", "")
         * XmlHelper.Delete(path, "/Node", "Attribute")
         ************************************************/
        public static void Delete(string path, string rootnode, string node, string attribute, string arrvalue)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode xn = doc.SelectSingleNode(rootnode);
                XmlNodeList xnl = doc.SelectSingleNode(rootnode).ChildNodes;
                for (int j = 0; j < xnl.Count; j++)
                {
                    XmlElement xe1 = (XmlElement)xnl.Item(j);

                    if (xe1.GetAttribute(attribute) == arrvalue)
                    {
                        xn.RemoveChild(xe1);
                        if (j < xnl.Count) j = j - 1;
                    }
                }
                doc.Save(path);
            }
            catch { }
        }
        /// <summary>
        /// 删除属性带key的节点,key:GUID
        /// </summary>
        /// <param name="path">XML文件路径</param>
        /// <param name="rootnode">根节点</param>
        /// <param name="key">key:Guid</param>
        /// <param name="msg">返回的信息</param>
        public static void DeleteXMLNode(string path, string rootnode, string znode, string type, string key)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(path);
                    XmlNodeList xl = xmlDoc.SelectSingleNode(rootnode).ChildNodes;
                    for (int i = 0; i < xl.Count; i++)
                    {
                        XmlElement xe = (XmlElement)xl[i];//第i个dbGust子节点
                        XmlNodeList node = xe.GetElementsByTagName(znode);
                        if (node.Count > 0)
                        {
                            for (int j = 0; j < node.Count; j++)
                            {
                                XmlNode node1 = node.Item(j);
                                XmlElement xe1 = (XmlElement)node.Item(j);
                                if (xe1.GetAttribute(type) == key)
                                {
                                    xmlDoc.SelectSingleNode(rootnode).RemoveChild(node[0].ParentNode);//删除该节点
                                    break;
                                }
                            }
                        }
                    }
                    xmlDoc.Save(path);
                }
            }
            catch (XmlException ex)
            {
            }
        }
        /// <summary>
        /// 返回符合指定名称的节点数
        /// </summary>
        /// <param name="nodeName">节点名</param>
        /// <returns>节点数</returns>
        public static int Count(string path, string nodeName, string znode)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                XmlElement xe = (XmlElement)xmlDoc.SelectSingleNode(nodeName); //查找根node节点
                XmlNodeList nodeList = xe.GetElementsByTagName(znode);
                return nodeList.Count;
            }
            catch (Exception e)
            {
                throw (new Exception(e.Message));
            }
        }
        /// <summary>
        /// 返回最后的数字
        /// </summary>
        /// <param name="path"></param>
        /// <param name="nodeName"></param>
        /// <param name="znode"></param>
        /// <returns></returns>
        public static string num(string path, string nodeName, string znode, string type)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                XmlElement xe = (XmlElement)xmlDoc.SelectSingleNode(nodeName); //查找根node节点
                XmlNodeList nodeList = xe.GetElementsByTagName(znode);
                int i = nodeList.Count;
                XmlElement xe1 = (XmlElement)nodeList.Item(nodeList.Count - 1);
                string inum = xe1.GetAttribute(type);
                return inum;
            }
            catch (Exception e)
            {
                throw (new Exception(e.Message));
            }
        }
    }

}

