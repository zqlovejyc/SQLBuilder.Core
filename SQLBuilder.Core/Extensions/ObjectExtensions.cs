#region License
/***
* Copyright © 2018-2021, 张强 (943620963@qq.com).
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* without warranties or conditions of any kind, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// object扩展类
    /// </summary>
    public static class ObjectExtensions
    {
        #region Like
        /// <summary>
        /// LIKE
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool Like(this object @this, string value) => true;
        #endregion

        #region LikeLeft
        /// <summary>
        /// LIKE '% _ _ _'
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool LikeLeft(this object @this, string value) => true;
        #endregion

        #region LikeRight
        /// <summary>
        /// LIKE '_ _ _ %'
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool LikeRight(this object @this, string value) => true;
        #endregion

        #region NotLike
        /// <summary>
        /// NOT LIKE
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool NotLike(this object @this, string value) => true;
        #endregion

        #region In
        /// <summary>
        /// IN
        /// </summary>
        /// <typeparam name="T">IN数组里面的数据类型</typeparam>
        /// <param name="this">扩展对象自身</param>
        /// <param name="array">IN数组</param>
        /// <returns>bool</returns>
        public static bool In<T>(this object @this, params T[] array) => true;
        #endregion

        #region NotIn
        /// <summary>
        /// NOT IN
        /// </summary>
        /// <typeparam name="T">NOT IN数组里面的数据类型</typeparam>
        /// <param name="this">扩展对象自身</param>
        /// <param name="array">NOT IN数组</param>
        /// <returns>bool</returns>
        public static bool NotIn<T>(this object @this, params T[] array) => true;
        #endregion

        #region Count
        /// <summary>
        /// 聚合函数Count
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T Count<T>(this object @this) => default;
        #endregion

        #region Sum
        /// <summary>
        /// 聚合函数Sum
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T Sum<T>(this object @this) => default;
        #endregion

        #region Avg
        /// <summary>
        /// 聚合函数Avg
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T Avg<T>(this object @this) => default;
        #endregion

        #region Max
        /// <summary>
        /// 聚合函数Max
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T Max<T>(this object @this) => default;
        #endregion

        #region Min
        /// <summary>
        /// 聚合函数Max
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T Min<T>(this object @this) => default;
        #endregion

        #region ToSafeValue
        /// <summary>
        /// 转换为安全类型的值
        /// </summary>
        /// <param name="this">object对象</param>
        /// <param name="type">type</param>
        /// <returns>object</returns>
        public static object ToSafeValue(this object @this, Type type)
        {
            return @this == null ? null : Convert.ChangeType(@this, type.GetCoreType());
        }
        #endregion

        #region IsNull
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="this">object对象</param>
        /// <returns>bool</returns>
        public static bool IsNull(this object @this)
        {
            return @this == null || @this == DBNull.Value;
        }
        #endregion

        #region IsNotNull
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="this">object对象</param>
        /// <returns>bool</returns>
        public static bool IsNotNull(this object @this)
        {
            return !@this.IsNull();
        }
        #endregion

        #region ToJson
        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="this">待序列化的对象</param>
        /// <returns>string</returns>
        public static string ToJson(this object @this)
        {
            return JsonConvert.SerializeObject(@this);
        }

        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="this">待序列化的对象</param>
        /// <param name="settings">JsonSerializerSettings配置</param>
        /// <returns></returns>
        public static string ToJson(this object @this, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(@this, settings ?? new JsonSerializerSettings());
        }

        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="this">待序列化的对象</param>
        /// <param name="camelCase">是否驼峰</param>
        /// <param name="indented">是否缩进</param>
        /// <param name="nullValueHandling">空值处理</param>
        /// <param name="converter">json转换，如：new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }</param>
        /// <returns>string</returns>
        public static string ToJson(this object @this, bool camelCase = false, bool indented = false, NullValueHandling nullValueHandling = NullValueHandling.Include, JsonConverter converter = null)
        {
            var options = new JsonSerializerSettings();
            if (camelCase)
                options.ContractResolver = new CamelCasePropertyNamesContractResolver();

            if (indented)
                options.Formatting = Formatting.Indented;

            options.NullValueHandling = nullValueHandling;

            if (converter != null)
                options.Converters?.Add(converter);

            return JsonConvert.SerializeObject(@this, options);
        }
        #endregion
    }
}