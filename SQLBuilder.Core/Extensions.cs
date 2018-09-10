#region License
/***
* Copyright © 2018, 张强 (943620963@qq.com).
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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.Reflection;
using System.Dynamic;
using Dapper;
using Npgsql;

namespace SQLBuilder.Core
{
    /// <summary>
    /// 扩展类
    /// </summary>
	public static class Extensions
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

        #region True
        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() => parameter => true;
        #endregion

        #region False
        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() => parameter => false;
        #endregion

        #region Or
        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region And
        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region ToLambda
        /// <summary>
        /// ToLambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Expression<T> ToLambda<T>(this Expression @this, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<T>(@this, parameters);
        }
        #endregion

        #region ToObject
        /// <summary>
        /// 转换Expression为object
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object ToObject(this Expression @this)
        {
            object obj = null;
            switch (@this.NodeType)
            {
                case ExpressionType.Constant:
                    obj = (@this as ConstantExpression)?.Value;
                    break;
                case ExpressionType.Convert:
                    obj = (@this as UnaryExpression)?.Operand?.ToObject();
                    break;
                default:
                    var isNullable = @this.Type.IsNullable();
                    switch (@this.Type.GetCoreType().Name.ToLower())
                    {
                        case "string":
                            obj = @this.ToLambda<Func<String>>().Compile().Invoke();
                            break;
                        case "int16":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Int16?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Int16>>().Compile().Invoke();
                            }
                            break;
                        case "int32":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Int32?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Int32>>().Compile().Invoke();
                            }
                            break;
                        case "int64":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Int64?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Int64>>().Compile().Invoke();
                            }
                            break;
                        case "decimal":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Decimal?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Decimal>>().Compile().Invoke();
                            }
                            break;
                        case "double":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Double?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Double>>().Compile().Invoke();
                            }
                            break;
                        case "datetime":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<DateTime?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<DateTime>>().Compile().Invoke();
                            }
                            break;
                        case "boolean":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Boolean?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Boolean>>().Compile().Invoke();
                            }
                            break;
                        case "byte":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Byte?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Byte>>().Compile().Invoke();
                            }
                            break;
                        case "char":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Char?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Char>>().Compile().Invoke();
                            }
                            break;
                        case "single":
                            if (isNullable)
                            {
                                obj = @this.ToLambda<Func<Single?>>().Compile().Invoke();
                            }
                            else
                            {
                                obj = @this.ToLambda<Func<Single>>().Compile().Invoke();
                            }
                            break;
                        default:
                            obj = @this.ToLambda<Func<Object>>().Compile().Invoke();
                            break;
                    }
                    break;
            }
            return obj;
        }
        #endregion

        #region Substring
        /// <summary>
        /// 从分隔符开始向尾部截取字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="lastIndexOf">true：从最后一个匹配的分隔符开始截取，false：从第一个匹配的分隔符开始截取，默认：true</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string separator, bool lastIndexOf = true)
        {
            var start = (lastIndexOf ? @this.LastIndexOf(separator) : @this.IndexOf(separator)) + separator.Length;
            var length = @this.Length - start;
            return @this.Substring(start, length);
        }
        #endregion

        #region GetCoreType
        /// <summary>
        /// 如果type是Nullable类型则返回UnderlyingType，否则则直接返回type本身
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>Type</returns>
        public static Type GetCoreType(this Type @this)
        {
            if (@this?.IsNullable() == true && @this.IsValueType)
            {
                @this = Nullable.GetUnderlyingType(@this);
            }
            return @this;
        }
        #endregion

        #region IsNullable
        /// <summary>
        /// 判断类型是否是Nullable类型
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>bool</returns>
        public static bool IsNullable(this Type @this)
        {
            return !@this.IsValueType || (@this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Nullable<>));
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

        #region ToDynamicParameters
        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter[] @this)
        {
            if (@this?.Length > 0)
            {
                var args = new DynamicParameters();
                @this.ToList().ForEach(p => args.Add(p.ParameterName.Replace("?", "@").Replace(":", "@"), p.Value, p.DbType, p.Direction, p.Size));
                return args;
            }
            return null;
        }

        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this List<DbParameter> @this)
        {
            if (@this?.Count > 0)
            {
                var args = new DynamicParameters();
                @this.ForEach(p => args.Add(p.ParameterName.Replace("?", "@").Replace(":", "@"), p.Value, p.DbType, p.Direction, p.Size));
                return args;
            }
            return null;
        }

        /// <summary>
        ///  DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter @this)
        {
            if (@this != null)
            {
                var args = new DynamicParameters();
                args.Add(@this.ParameterName.Replace("?", "@").Replace(":", "@"), @this.Value, @this.DbType, @this.Direction, @this.Size);
                return args;
            }
            return null;
        }

        /// <summary>
        ///  IDictionary转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>        
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                var args = new DynamicParameters();
                foreach (var item in @this)
                {
                    args.Add(item.Key.Replace("?", "@").Replace(":", "@"), item.Value);
                }
                return args;
            }
            return null;
        }
        #endregion

        #region ToDbParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="command">The command.</param>        
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbCommand command)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = x.Key;
                    parameter.Value = x.Value;                    
                    return parameter;
                }).ToArray();
            }
            return null;
        }

        /// <summary>
        ///  An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="connection">The connection.</param>        
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbConnection connection)
        {
            if (@this?.Count > 0)
            {
                var command = connection.CreateCommand();
                return @this.Select(x =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = x.Key;
                    parameter.Value = x.Value;
                    return parameter;
                }).ToArray();
            }
            return null;
        }
        #endregion

        #region ToSqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a SQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a SqlParameter[].</returns>
        public static SqlParameter[] ToSqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new SqlParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToMySqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a MySQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a MySqlParameter[].</returns>
        public static MySqlParameter[] ToMySqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new MySqlParameter(x.Key.Replace("@", "?").Replace(":", "?"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToSqliteParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Sqlite parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a SqliteParameter[].</returns>
        public static SqliteParameter[] ToSqliteParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new SqliteParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToOracleParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Oracle parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a OracleParameter[].</returns>
        public static OracleParameter[] ToOracleParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new OracleParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToNpgsqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a PostgreSQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a NpgsqlParameter[].</returns>
        public static NpgsqlParameter[] ToNpgsqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new NpgsqlParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToDataTable
        /// <summary>
        /// IDataReader转换为DataTable
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(this IDataReader @this)
        {
            var table = new DataTable();
            if (@this?.IsClosed == false)
            {
                table.Load(@this);
            }
            return table;
        }

        /// <summary>
        /// List集合转DataTable
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">list数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this List<T> @this)
        {
            DataTable dt = null;
            if (@this?.Count > 0)
            {
                dt = new DataTable(typeof(T).Name);
                var typeName = typeof(T).Name;
                var first = @this.FirstOrDefault();
                var firstTypeName = first.GetType().Name;
                if (typeName == "Object" && (firstTypeName == "DapperRow" || firstTypeName == "DynamicRow"))
                {
                    var dic = first as IDictionary<string, object>;
                    dt.Columns.AddRange(dic.Select(o => new DataColumn(o.Key)).ToArray());
                    var dics = @this.Select(o => o as IDictionary<string, object>);
                    foreach (var item in dics)
                    {
                        dt.Rows.Add(item.Select(o => o.Value).ToArray());
                    }
                }
                else
                {
                    var props = typeName == "Object" ? first.GetType().GetProperties() : typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var prop in props)
                    {
                        dt.Columns.Add(prop.Name, prop.PropertyType.GetCoreType());
                    }
                    foreach (var item in @this)
                    {
                        var values = new object[props.Length];
                        for (var i = 0; i < props.Length; i++)
                        {
                            if (!props[i].CanRead) continue;
                            values[i] = props[i].GetValue(item, null);
                        }
                        dt.Rows.Add(values);
                    }
                }
            }
            return dt;
        }
        #endregion

        #region ToDynamic
        /// <summary>
        /// IDataReader数据转为dynamic对象
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <param name="isDisposable">是否释放</param>
        /// <returns>dynamic</returns>
        public static dynamic ToDynamic(this IDataReader @this, bool isDisposable)
        {
            dynamic result = null;
            if (@this?.IsClosed == false)
            {
                if (!isDisposable || (isDisposable && @this.Read()))
                {
                    result = new ExpandoObject();
                    for (var i = 0; i < @this.FieldCount; i++)
                    {
                        try
                        {
                            ((IDictionary<string, object>)result).Add(@this.GetName(i), @this.GetValue(i));
                        }
                        catch
                        {
                            ((IDictionary<string, object>)result).Add(@this.GetName(i), null);
                        }
                    }
                }
                if (isDisposable)
                {
                    @this.Close();
                    @this.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// IDataReader数据转为dynamic对象集合
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>dynamic集合</returns>
        public static List<dynamic> ToDynamic(this IDataReader @this)
        {
            List<dynamic> list = null;
            if (@this?.IsClosed == false)
            {
                list = new List<dynamic>();
                using (@this)
                {
                    while (@this.Read())
                    {
                        list.Add(@this.ToDynamic(false));
                    }
                }
            }
            return list;
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// IDataReader数据转为dynamic对象
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <param name="isDisposable">是否释放</param>
        /// <returns>dynamic</returns>
        public static Dictionary<string, object> ToDictionary(this IDataReader @this, bool isDisposable)
        {
            Dictionary<string, object> result = null;
            if (@this?.IsClosed == false)
            {
                if (!isDisposable || (isDisposable && @this.Read()))
                {
                    result = new Dictionary<string, object>();
                    for (var i = 0; i < @this.FieldCount; i++)
                    {
                        try
                        {
                            result.Add(@this.GetName(i), @this.GetValue(i));
                        }
                        catch
                        {
                            result.Add(@this.GetName(i), null);
                        }
                    }
                }
                if (isDisposable)
                {
                    @this.Close();
                    @this.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// IDataReader数据转为dynamic对象集合
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>dynamic集合</returns>
        public static List<Dictionary<string, object>> ToDictionary(this IDataReader @this)
        {
            List<Dictionary<string, object>> list = null;
            if (@this?.IsClosed == false)
            {
                list = new List<Dictionary<string, object>>();
                using (@this)
                {
                    while (@this.Read())
                    {
                        list.Add(@this.ToDictionary(false));
                    }
                }
            }
            return list;
        }
        #endregion

        #region ToList
        /// <summary>
        /// IDataReader转换为T集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">reader数据源</param>
        /// <returns>T类型集合</returns>
        public static List<T> ToList<T>(this IDataReader @this)
        {
            List<T> list = null;
            if (@this?.IsClosed == false)
            {
                list = new List<T>();
                var type = typeof(T);
                if (type == typeof(Dictionary<string, object>))
                {
                    list = @this.ToDictionary() as List<T>;
                }
                else if (type.IsClass && type.Name != "Object")
                {
                    using (@this)
                    {
                        var fields = new List<string>();
                        for (int i = 0; i < @this.FieldCount; i++)
                        {
                            fields.Add(@this.GetName(i).ToLower());
                        }
                        while (@this.Read())
                        {
                            var instance = Activator.CreateInstance<T>();
                            var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                            foreach (var p in props)
                            {
                                if (!p.CanWrite) continue;
                                if (fields.Contains(p.Name.ToLower()) && !@this[p.Name].IsNull())
                                {
                                    p.SetValue(instance, @this[p.Name].ToSafeValue(p.PropertyType), null);
                                }
                            }
                            list.Add(instance);
                        }
                    }
                }
                else
                {
                    list = @this.ToDynamic() as List<T>;
                }
            }
            return list;
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// IDataReader转换为T类型实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">reader数据源</param>
        /// <returns>T类型实体</returns>
        public static T ToEntity<T>(this IDataReader @this)
        {
            var result = default(T);
            if (@this?.IsClosed == false)
            {
                var type = typeof(T);
                if (type == typeof(Dictionary<string, object>))
                {
                    result = (@this.ToDictionary() as List<T>).FirstOrDefault();
                }
                else if (type.IsClass && type.Name != "Object")
                {
                    using (@this)
                    {
                        var fields = new List<string>();
                        for (int i = 0; i < @this.FieldCount; i++)
                        {
                            fields.Add(@this.GetName(i).ToLower());
                        }
                        if (@this.Read())
                        {
                            result = Activator.CreateInstance<T>();
                            var props = result.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                            foreach (var p in props)
                            {
                                if (!p.CanWrite) continue;
                                if (fields.Contains(p.Name.ToLower()) && !@this[p.Name].IsNull())
                                {
                                    p.SetValue(result, @this[p.Name].ToSafeValue(p.PropertyType), null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = (T)@this.ToDynamic(true);
                }
            }
            return result;
        }
        #endregion
    }
}
