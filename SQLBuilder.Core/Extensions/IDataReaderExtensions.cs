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

using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// IDataReader扩展类
    /// </summary>
    public static class IDataReaderExtensions
    {
        #region ToDataTable
        /// <summary>
        /// IDataReader转换为DataTable
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(this IDataReader @this)
        {
            var table = new DataTable();

            if (@this.IsNull() || @this.IsClosed)
                return table;

            using (@this)
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
            if (@this.IsNullOrEmpty())
                return default;

            var dt = new DataTable(typeof(T).Name);
            var type = typeof(T);
            var first = @this.First();
            var firstType = first.GetType();

            if (type.IsDictionaryType() || (type.IsDynamicOrObjectType() && firstType.IsDictionaryType()))
            {
                var dic = first as IDictionary<string, object>;
                dt.Columns.AddRange(dic.Select(o => new DataColumn(o.Key, o.Value?.GetType().GetCoreType() ?? typeof(object))).ToArray());

                var dics = @this.Select(o => o as IDictionary<string, object>);
                foreach (var item in dics)
                    dt.Rows.Add(item.Select(o => o.Value).ToArray());
            }
            else
            {
                var props = type.IsDynamicOrObjectType()
                    ? firstType.GetProperties()
                    : typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in props)
                    dt.Columns.Add(prop.Name, prop?.PropertyType.GetCoreType() ?? typeof(object));

                foreach (var item in @this)
                {
                    var values = new object[props.Length];
                    for (var i = 0; i < props.Length; i++)
                    {
                        if (!props[i].CanRead)
                            continue;

                        values[i] = props[i].GetValue(item, null);
                    }
                    dt.Rows.Add(values);
                }
            }

            return dt;
        }
        #endregion

        #region ToDataSet
        /// <summary>
        /// IDataReader转换为DataSet
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataSet</returns>
        public static DataSet ToDataSet(this IDataReader @this)
        {
            var ds = new DataSet();

            if (@this.IsNull() || @this.IsClosed)
                return ds;

            using (@this)
            {
                do
                {
                    var schemaTable = @this.GetSchemaTable();
                    var dt = new DataTable();
                    for (var i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        var row = schemaTable.Rows[i];
                        dt.Columns.Add(new DataColumn((string)row["ColumnName"], (Type)row["DataType"]));
                    }
                    while (@this.Read())
                    {
                        var dataRow = dt.NewRow();
                        for (var i = 0; i < @this.FieldCount; i++)
                            dataRow[i] = @this.GetValue(i);
                        dt.Rows.Add(dataRow);
                    }
                    ds.Tables.Add(dt);
                }
                while (@this.NextResult());
            }

            return ds;
        }
        #endregion

        #region ToDynamic
        /// <summary>
        /// IDataReader数据转为dynamic对象
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>dynamic</returns>
        public static dynamic ToDynamic(this IDataReader @this)
        {
            return @this.ToDynamics()?.FirstOrDefault();
        }

        /// <summary>
        /// IDataReader数据转为dynamic对象集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>dynamic集合</returns>
        public static IEnumerable<dynamic> ToDynamics(this IDataReader @this)
        {
            var res = new List<dynamic>();

            if (@this.IsNull() || @this.IsClosed)
                return res;

            using (@this)
            {
                while (@this.Read())
                {
                    var row = new Dictionary<string, object>();

                    for (var i = 0; i < @this.FieldCount; i++)
                        row.Add(@this.GetName(i), @this.GetValue(i));

                    res.Add(row);
                }
            }

            return res;
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// IDataReader数据转为Dictionary对象
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object> ToDictionary(this IDataReader @this)
        {
            return @this.ToDictionaries()?.FirstOrDefault();
        }

        /// <summary>
        /// IDataReader数据转为Dictionary对象集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>Dictionary集合</returns>
        public static IEnumerable<Dictionary<string, object>> ToDictionaries(this IDataReader @this)
        {
            if (@this.IsNull() || @this.IsClosed)
                yield break;

            using (@this)
            {
                while (@this.Read())
                {
                    var dic = new Dictionary<string, object>();
                    for (var i = 0; i < @this.FieldCount; i++)
                        dic[@this.GetName(i)] = @this.GetValue(i);

                    yield return dic;
                }
            }
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// IDataReader数据转为强类型实体
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>强类型实体</returns>
        public static T ToEntity<T>(this IDataReader @this)
        {
            var result = @this.ToEntities<T>();
            if (result != null)
                return result.FirstOrDefault();

            return default;
        }

        /// <summary>
        /// IDataReader数据转为强类型实体集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>强类型实体集合</returns>
        public static IEnumerable<T> ToEntities<T>(this IDataReader @this)
        {
            if (@this.IsNull() || @this.IsClosed)
                yield break;

            using (@this)
            {
                var fields = new List<string>();
                for (int i = 0; i < @this.FieldCount; i++)
                    fields.Add(@this.GetName(i));

                while (@this.Read())
                {
                    var instance = Activator.CreateInstance<T>();
                    var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var p in props)
                    {
                        if (!p.CanWrite)
                            continue;

                        var field = fields.Where(o => o.EqualIgnoreCase(p.Name)).FirstOrDefault();
                        if (field.IsNotNullOrEmpty() && @this[field].IsNotNull())
                            p.SetValue(instance, @this[field].ToSafeValue(p.PropertyType), null);
                    }
                    yield return instance;
                }
            }
        }
        #endregion

        #region ToList
        /// <summary>
        /// IDataReader转换为T集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型集合</returns>
        public static List<T> ToList<T>(this IDataReader @this)
        {
            if (@this.IsNull() || @this.IsClosed)
                return default;

            List<T> list = null;
            var type = typeof(T);
            if (type.AssignableTo(typeof(Dictionary<,>)))
                list = @this.ToDictionaries()?.ToList() as List<T>;

            else if (type.AssignableTo(typeof(IDictionary<,>)))
                list = @this.ToDictionaries()?.Select(o => o as IDictionary<string, object>).ToList() as List<T>;

            else if (type.IsClass && !type.IsDynamicOrObjectType() && !type.IsStringType())
                list = @this.ToEntities<T>()?.ToList() as List<T>;

            else
            {
                var result = @this.ToDynamics();
                if (result != null && result.Any())
                {
                    list = result.ToList() as List<T>;
                    if (list == null && (type.IsStringType() || type.IsValueType))
                        //适合查询单个字段的结果集
                        list = result.Select(o => (T)(o as IDictionary<string, object>).Select(x => x.Value).FirstOrDefault()).ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// IDataReader转换为T集合的集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型集合的集合</returns>
        public static List<List<T>> ToLists<T>(this IDataReader @this)
        {
            var result = new List<List<T>>();

            if (@this.IsNull() || @this.IsClosed)
                return result;

            using (@this)
            {
                var type = typeof(T);
                do
                {
                    #region IDictionary
                    if (type.IsDictionaryType())
                    {
                        var list = new List<Dictionary<string, object>>();
                        while (@this.Read())
                        {
                            var dic = new Dictionary<string, object>();
                            for (var i = 0; i < @this.FieldCount; i++)
                                dic[@this.GetName(i)] = @this.GetValue(i);

                            list.Add(dic);
                        }

                        if (!type.AssignableTo(typeof(Dictionary<,>)))
                            result.Add(list.Select(o => o as IDictionary<string, object>).ToList() as List<T>);
                        else
                            result.Add(list as List<T>);
                    }
                    #endregion

                    #region Class T
                    else if (type.IsClass && !type.IsDynamicOrObjectType() && !type.IsStringType())
                    {
                        var list = new List<T>();
                        var fields = new List<string>();
                        for (int i = 0; i < @this.FieldCount; i++)
                            fields.Add(@this.GetName(i));

                        while (@this.Read())
                        {
                            var instance = Activator.CreateInstance<T>();
                            var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                            foreach (var p in props)
                            {
                                if (!p.CanWrite)
                                    continue;

                                var field = fields.Where(o => o.EqualIgnoreCase(p.Name)).FirstOrDefault();
                                if (field.IsNotNullOrEmpty() && @this[field].IsNotNull())
                                    p.SetValue(instance, @this[field].ToSafeValue(p.PropertyType), null);
                            }

                            list.Add(instance);
                        }

                        result.Add(list);
                    }
                    #endregion

                    #region dynamic
                    else
                    {
                        var list = new List<dynamic>();
                        while (@this.Read())
                        {
                            var row = new ExpandoObject() as IDictionary<string, object>;
                            for (var i = 0; i < @this.FieldCount; i++)
                                row.Add(@this.GetName(i), @this.GetValue(i));

                            list.Add(row);
                        }
                        var item = list as List<T>;
                        if (item == null && (type.IsStringType() || type.IsValueType))
                            //适合查询单个字段的结果集
                            item = list.Select(o => (T)(o as IDictionary<string, object>).Select(x => x.Value).FirstOrDefault()).ToList();

                        result.Add(item);
                    }
                    #endregion
                } while (@this.NextResult());
            }

            return result;
        }
        #endregion

        #region ToFirstOrDefault
        /// <summary>
        /// IDataReader转换为T类型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型对象</returns>
        public static T ToFirstOrDefault<T>(this IDataReader @this)
        {
            var list = @this.ToList<T>();
            if (list.IsNotNull())
                return list.FirstOrDefault();

            return default;
        }
        #endregion
    }
}
