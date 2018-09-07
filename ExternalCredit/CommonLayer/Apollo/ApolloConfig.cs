using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized; 
using System.Web.Script.Serialization;
using System.Xml;

namespace VVcredit.ExtTrade.CommonLayer
{
    public class ApolloConfig
    {
        #region AttributeRegion
        private static string DEFAULT_VALUE = null;//"undefined"; 
        private static string DEFAULT_NAMESPACE_NAME = ConfigConsts.NAMESPACE_APPLICATION; 
        #endregion
         
        static ApolloConfig() {  }

        static Com.Ctrip.Framework.Apollo.Config Config(string NameSpaceName)
        { 
            return ConfigService.GetConfig(NameSpaceName??DEFAULT_NAMESPACE_NAME);
        }

        #region PropertyRegion
        /// <summary>
        /// Return the property value with the given key, or
        /// {@code defaultValue} if the key doesn't exist. </summary>
        /// <param name="key"> the property name </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value </returns>
        public static string GetProperty(string key, string namespaceName = null)
        {
            try
            { 
                return Config(namespaceName).GetProperty(key, DEFAULT_VALUE);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }

        /// <summary>
        /// Return the property value with the given key, or
        /// {@code defaultValue} if the key doesn't exist. </summary>
        /// <param name="key"> the property name </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value as int </returns>
        public static int? GetIntProperty(string key, string namespaceName = null)
        {
            try
            { 
                return Config(namespaceName).GetIntProperty(key, null);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetIntProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }

        /// <summary>
        /// Return the property value with the given key, or
        /// {@code defaultValue} if the key doesn't exist. </summary>
        /// <param name="key"> the property name </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value as long </returns>
        public static long? GetLongProperty(string key, string namespaceName = null)
        {
            try
            {
                return Config(namespaceName).GetIntProperty(key, null);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetLongProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }
        /// <summary>
        /// Return the property value with the given key, or
        /// {@code defaultValue} if the key doesn't exist. </summary>
        /// <param name="key"> the property name </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value as short </returns>
        public static short? GetShortProperty(string key, string namespaceName = null)
        {
            try
            {
                return Config(namespaceName).GetShortProperty(key, null);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetShortProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }

        /// <summary>
        /// Return the property value with the given key, or
        /// {@code defaultValue} if the key doesn't exist. </summary>
        /// <param name="key"> the property name </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value as float </returns>
        public static float? GetFloatProperty(string key, string namespaceName = null)
        {
            try
            {
                return Config(namespaceName).GetFloatProperty(key, null);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetFloatProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }

        /// <summary>
        /// Return the property value with the given key, or
        /// {@code defaultValue} if the key doesn't exist. </summary>
        /// <param name="key"> the property name </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value as double </returns>
        public static double? GetDoubleProperty(string key, string namespaceName = null)
        {
            try
            {
                 return Config(namespaceName).GetDoubleProperty(key, null);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetDoubleProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }

        /// <summary>
        /// Return the property value with the given key, or
        /// {@code defaultValue} if the key doesn't exist. </summary>
        /// <param name="key"> the property name </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value as byte </returns>
        public static sbyte? GetByteProperty(string key, string namespaceName = null)
        {
            try
            {
                return Config(namespaceName).GetByteProperty(key, null);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetDoubleProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }

        /// <summary>
        /// Return the property value with the given key, or
        /// {@code defaultValue} if the key doesn't exist. </summary>
        /// <param name="key"> the property name </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value as bool </returns>
        public static bool? GetBooleanProperty(string key, string namespaceName = null)
        {
            try
            {
               return Config(namespaceName).GetBooleanProperty(key, null);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetBooleanProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }

        /// <summary>
        /// Return the array property value with the given key, or {@code defaultValue} if the key doesn't
        /// exist.
        /// </summary>
        /// <param name="key"> the property name </param>
        /// <param name="delimiter"> the delimeter regex </param>
        /// <param name="defaultValue"> the default value when key is not found or any error occurred </param>
        /// <returns> the property value as array </returns>
        public static string[] GetArrayProperty(string key, string delimiter, string namespaceName)
        {
            try
            {
                return Config(namespaceName).GetArrayProperty(key, delimiter, null);
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetArrayProperty:[namespace:" + namespaceName + ",key:" + key + "]", e);
            }
            return null;
        }

        /// <summary>
        /// Return a set of the property names
        /// </summary>
        /// <returns> the property names </returns>
        public static ISet<string> GetPropertyNames(string namespaceName = null)
        {
            try
            {
                return Config(namespaceName).GetPropertyNames();
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetArrayProperty:[namespace:" + namespaceName, e);
            }
            return null;
        }
        public static NameValueCollection GetPropertyNameValueCollection(string namespaceName = null)
        {
            NameValueCollection nvCollection = new NameValueCollection();
            try
            {
                foreach (var item in GetPropertyNames(namespaceName))
                    nvCollection.Add(item, GetProperty(item, namespaceName));
                return nvCollection;
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetPropertyNameValueCollection:[namespace:" + namespaceName + "]", e);
            }
            return nvCollection;
        }
        #endregion

        #region JsonRegion
        /// <summary>
        /// get json string 
        /// </summary>
        /// <param name="namespaceName">namespace name</param>
        /// <returns>json string</returns>
        public static string GetJson(string namespaceName)
        {
            return GetProperty("content", namespaceName + ".json");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="namespaceName">namespace name</param>
        /// <returns>T object</returns>
        public static T GetJson<T>(string namespaceName) where T : new()
        {
            try
            {
                return new JavaScriptSerializer().Deserialize<T>(GetJson(namespaceName));
            }
            catch (Exception e)
            {
                LogRecorder.Write("GetJson:[namespace:" + namespaceName + "]", e);
            }
            return new T();
        }
        #endregion

        #region 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="namespaceName">namespace name</param>
        /// <returns>xml string</returns>
        public static string GetXml(string namespaceName)
        {
            return GetProperty("content", namespaceName + ".xml");
        } 
        #endregion
    }
}
