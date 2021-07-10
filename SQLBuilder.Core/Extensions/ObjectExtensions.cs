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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLBuilder.Core.Attributes;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Repositories;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using SysColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;

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
        /// 聚合函数Min
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
        public static object ToSafeValue(this object @this, Type type) =>
            @this == null ? null : Convert.ChangeType(@this, type.GetCoreType());
        #endregion

        #region IsNull
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="this">object对象</param>
        /// <returns>bool</returns>
        public static bool IsNull(this object @this) =>
            @this == null || @this == DBNull.Value;
        #endregion

        #region IsNotNull
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="this">object对象</param>
        /// <returns>bool</returns>
        public static bool IsNotNull(this object @this) =>
            !@this.IsNull();
        #endregion

        #region ToJson
        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="this">待序列化的对象</param>
        /// <returns>string</returns>
        public static string ToJson(this object @this) =>
            JsonConvert.SerializeObject(@this);

        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="this">待序列化的对象</param>
        /// <param name="settings">JsonSerializerSettings配置</param>
        /// <returns></returns>
        public static string ToJson(this object @this, JsonSerializerSettings settings) =>
            JsonConvert.SerializeObject(@this, settings ?? new JsonSerializerSettings());

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

        #region To
        /// <summary>
        /// To
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T To<T>(this object @this)
        {
            if (@this != null)
            {
                var targetType = typeof(T);

                if (@this.GetType() == targetType)
                    return (T)@this;

                var converter = TypeDescriptor.GetConverter(@this);
                if (converter != null)
                {
                    if (converter.CanConvertTo(targetType))
                        return (T)converter.ConvertTo(@this, targetType);
                }

                converter = TypeDescriptor.GetConverter(targetType);
                if (converter != null)
                {
                    if (converter.CanConvertFrom(@this.GetType()))
                        return (T)converter.ConvertFrom(@this);
                }

                if (@this == DBNull.Value)
                    return (T)(object)null;
            }

            return (T)@this;
        }
        #endregion

        #region ToColumns
        /// <summary>
        /// 根据实体类型获取所有列的查询字符串
        /// </summary>
        /// <param name="this">实体Type类型</param>
        /// <param name="repository">仓储</param>
        /// <returns></returns>
        public static string ToColumns(this Type @this, IRepository repository) =>
            @this.ToColumns(repository.IsEnableFormat, repository.DatabaseType);

        /// <summary>
        /// 根据实体类型获取所有列的查询字符串
        /// </summary>
        /// <param name="this">实体Type类型</param>
        /// <param name="format">是否启用格式化</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns></returns>
        public static string ToColumns(this Type @this, bool format, DatabaseType databaseType)
        {
            var properties = @this.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
            if (properties.IsNullOrEmpty())
                return "*";

            var columns = new StringBuilder();

            //遍历属性
            foreach (var property in properties)
            {
                var select = true;
                var columnFormat = false;
                var propertyName = property.Name;
                var columnName = string.Empty;

                //获取特性
                var attributes = property.GetAttributes(
                    typeof(KeyAttribute),
                    typeof(ColumnAttribute),
                    typeof(SysColumnAttribute));

                if (attributes.IsNotNullOrEmpty())
                {
                    foreach (var attribute in attributes)
                    {
                        var (name, res, colFormat) = attribute switch
                        {
                            KeyAttribute key => (key.Name, true, key.Format),
                            ColumnAttribute column => (column.Name, !(!column.Update && !column.Insert), column.Format),
                            SysColumnAttribute sys => (sys.Name, true, false),
                            _ => (null, true, false)
                        };

                        //判断是否要进行查询
                        if (!res)
                        {
                            select = res;
                            continue;
                        }

                        //只匹配第一个name不为空的特性
                        if (columnName.IsNullOrEmpty() && name.IsNotNullOrEmpty())
                            columnName = name;

                        //判断是否单独启用格式化
                        if (colFormat)
                            columnFormat = colFormat;
                    }
                }

                //判断是否要进行查询
                if (!select)
                    continue;

                //全局格式化、单独格式化
                if (format || columnFormat)
                {
                    //格式化模板
                    var template = databaseType switch
                    {
                        DatabaseType.Sqlite => "\"{0}\"",
                        DatabaseType.SqlServer => "[{0}]",
                        DatabaseType.MySql => "`{0}`",
                        DatabaseType.Oracle => "\"{0}\"",
                        DatabaseType.PostgreSql => "\"{0}\"",
                        _ => "{0}",
                    };

                    columns.Append(columnName.IsNullOrEmpty()
                        ? string.Format(template, propertyName)
                        : $"{string.Format(template, columnName)} AS {string.Format(template, propertyName)}");
                }
                //非格式化
                else
                {
                    columns.Append(columnName.IsNullOrEmpty()
                        ? propertyName
                        : $"{columnName} AS {propertyName}");
                }

                columns.Append(",");
            }

            columns.Remove(columns.Length - 1, 1);

            return columns.ToString();
        }
        #endregion
    }
}