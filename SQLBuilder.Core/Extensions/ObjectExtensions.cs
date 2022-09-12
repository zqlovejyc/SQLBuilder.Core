#region License
/***
* Copyright © 2018-2025, 张强 (943620963@qq.com).
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
using Oracle.ManagedDataAccess.Client;
using SQLBuilder.Core.Attributes;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Repositories;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
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

            return @this == null ? default : (T)@this;
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
                        : (columnName.EqualIgnoreCase(propertyName)
                        ? string.Format(template, columnName)
                        : $"{string.Format(template, columnName)} AS {string.Format(template, propertyName)}"));
                }
                //非格式化
                else
                {
                    columns.Append(columnName.IsNullOrEmpty()
                        ? propertyName
                        : (columnName.EqualIgnoreCase(propertyName)
                        ? columnName
                        : $"{columnName} AS {propertyName}"));
                }

                columns.Append(",");
            }

            columns.Remove(columns.Length - 1, 1);

            return columns.ToString();
        }
        #endregion

        #region GetOracleDbType
        /// <summary>
        /// 获取OracelDbType类型
        /// </summary>
        /// <param name="this"></param>
        /// <param name="stringType">string类型对应的默认OracleDbType类型</param>
        /// <param name="defaultType">默认OracleDbType类型</param>
        /// <returns></returns>
        public static OracleDbType GetOracleDbType(this object @this, OracleDbType stringType = OracleDbType.Varchar2, OracleDbType defaultType = OracleDbType.Varchar2)
        {
            return @this switch
            {
                byte[] => OracleDbType.Blob,
                char => OracleDbType.Char,
                string => stringType,
                DateTime => OracleDbType.Date,
                byte or sbyte => OracleDbType.Byte,
                int or uint => OracleDbType.Int32,
                short or ushort => OracleDbType.Int16,
                long or ulong => OracleDbType.Int64,
                decimal => OracleDbType.Decimal,
                double => OracleDbType.Double,
                float => OracleDbType.Single,
                _ => defaultType,
            };
        }
        #endregion

        #region GetType
        /// <summary>
        /// 根据OracleDbType类型获取对应的系统数据类型
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type GetType(this OracleDbType @this)
        {
            return @this switch
            {
                OracleDbType.BFile => typeof(byte[]),
                OracleDbType.BinaryDouble => typeof(double),
                OracleDbType.BinaryFloat => typeof(float),
                OracleDbType.Blob => typeof(byte[]),
                OracleDbType.Boolean => typeof(bool),
                OracleDbType.Byte => typeof(byte),
                OracleDbType.Char => typeof(char),
                OracleDbType.Clob => typeof(string),
                OracleDbType.Date => typeof(DateTime),
                OracleDbType.Decimal => typeof(decimal),
                OracleDbType.Double => typeof(double),
                OracleDbType.Int16 => typeof(short),
                OracleDbType.Int32 => typeof(int),
                OracleDbType.Int64 => typeof(long),
                OracleDbType.IntervalDS => typeof(double),
                OracleDbType.IntervalYM => typeof(int),
                OracleDbType.Long => typeof(string),
                OracleDbType.LongRaw => typeof(byte[]),
                OracleDbType.NChar => typeof(string),
                OracleDbType.NClob => typeof(string),
                OracleDbType.NVarchar2 => typeof(string),
                OracleDbType.Raw => typeof(byte[]),
                OracleDbType.RefCursor => typeof(object),
                OracleDbType.Single => typeof(float),
                OracleDbType.TimeStamp => typeof(DateTime),
                OracleDbType.TimeStampLTZ => typeof(DateTime),
                OracleDbType.TimeStampTZ => typeof(DateTime),
                OracleDbType.Varchar2 => typeof(string),
                OracleDbType.XmlType => typeof(string),
                OracleDbType.Json => typeof(string),
                _ => default
            };
        }

        /// <summary>
        /// 根据DbType类型获取对应的系统数据类型
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Type GetType(this DbType @this)
        {
            return @this switch
            {
                DbType.AnsiString => typeof(string),
                DbType.AnsiStringFixedLength => typeof(string),
                DbType.Binary => typeof(byte[]),
                DbType.Boolean => typeof(bool),
                DbType.Byte => typeof(byte),
                DbType.Currency => typeof(decimal),
                DbType.Date => typeof(DateTime),
                DbType.DateTime => typeof(DateTime),
                DbType.DateTime2 => typeof(DateTime),
                DbType.DateTimeOffset => typeof(DateTimeOffset),
                DbType.Decimal => typeof(decimal),
                DbType.Double => typeof(double),
                DbType.Guid => typeof(Guid),
                DbType.Int16 => typeof(short),
                DbType.Int32 => typeof(int),
                DbType.Int64 => typeof(long),
                DbType.Object => typeof(object),
                DbType.SByte => typeof(sbyte),
                DbType.Single => typeof(float),
                DbType.String => typeof(string),
                DbType.StringFixedLength => typeof(string),
                DbType.Time => typeof(DateTime),
                DbType.UInt16 => typeof(ushort),
                DbType.UInt32 => typeof(uint),
                DbType.UInt64 => typeof(ulong),
                DbType.VarNumeric => typeof(decimal),
                DbType.Xml => typeof(string),
                _ => default,
            };
        }
        #endregion

        #region Separate
        /// <summary>
        /// 追加分隔符字符串，忽略开头，常用于拼接
        /// </summary>
        /// <param name="this">字符串构造者</param>
        /// <param name="separator">分隔符</param>
        /// <returns></returns>
        public static StringBuilder Separate(this StringBuilder @this, string separator)
        {
            if (@this.IsNull() || separator.IsNullOrEmpty())
                return @this;

            if (@this.Length > 0)
                @this.Append(separator);

            return @this;
        }
        #endregion

        #region Join
        /// <summary>
        /// 把一个列表组合成为一个字符串，默认逗号分隔
        /// </summary>
        /// <param name="this"></param>
        /// <param name="separator">组合分隔符，默认逗号</param>
        /// <returns>string</returns>
        public static string Join(this IEnumerable @this, string separator = ",")
        {
            var sb = new StringBuilder();

            if (@this.IsNull())
                return sb.ToString();

            foreach (var item in @this)
            {
                sb.Separate(separator).Append(item + "");
            }

            return sb.ToString();
        }
        #endregion
    }
}