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

using Oracle.ManagedDataAccess.Client;
using SQLBuilder.Core.Attributes;
using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Parameters;
using SQLBuilder.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Sql = SQLBuilder.Core.Entry.SqlBuilder;
using CusColumnAttribute = SQLBuilder.Core.Attributes.ColumnAttribute;
using SysColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// SqlBuilderCore扩展类
    /// </summary>
    public static class SqlBuilderExtensions
    {
        #region Execute
        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static int Execute<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.ExecuteBySql(@this.Sql, @this.DynamicParameters);
        }

        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await repository.ExecuteBySqlAsync(@this.Sql, @this.DynamicParameters);
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static TEntity ToEntity<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return @this.ToEntity<TEntity, TEntity>(repository);
        }

        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static TReturn ToEntity<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.FindEntity<TReturn>(@this.Sql, @this.DynamicParameters);
        }

        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<TEntity> ToEntityAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await @this.ToEntityAsync<TEntity, TEntity>(repository);
        }

        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<TReturn> ToEntityAsync<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await repository.FindEntityAsync<TReturn>(@this.Sql, @this.DynamicParameters);
        }
        #endregion

        #region ToList
        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static List<TEntity> ToList<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return @this.ToList<TEntity, TEntity>(repository);
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static List<TReturn> ToList<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.FindList<TReturn>(@this.Sql, @this.DynamicParameters)?.ToList();
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<List<TEntity>> ToListAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await @this.ToListAsync<TEntity, TEntity>(repository);
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<List<TReturn>> ToListAsync<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return (await repository.FindListAsync<TReturn>(@this.Sql, @this.DynamicParameters))?.ToList();
        }
        #endregion

        #region ToObject
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static object ToObject<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.FindObject(@this.Sql, @this.DynamicParameters);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<object> ToObjectAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await repository.FindObjectAsync(@this.Sql, @this.DynamicParameters);
        }
        #endregion

        #region ToPage
        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <param name="orderField"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static (List<TEntity> list, long total) ToPage<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository, string orderField, bool isAscending, int pageSize, int pageIndex) where TEntity : class
        {
            var (list, total) = repository.FindList<TEntity>(@this.Sql, @this.DynamicParameters, orderField, isAscending, pageSize, pageIndex);

            return (list?.ToList(), total);
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <param name="orderField"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static (List<TReturn> list, long total) ToPage<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository, string orderField, bool isAscending, int pageSize, int pageIndex) where TEntity : class
        {
            var (list, total) = repository.FindList<TReturn>(@this.Sql, @this.DynamicParameters, orderField, isAscending, pageSize, pageIndex);

            return (list?.ToList(), total);
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <param name="orderField"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static async Task<(List<TEntity> list, long total)> ToPageAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository, string orderField, bool isAscending, int pageSize, int pageIndex) where TEntity : class
        {
            var (list, total) = await repository.FindListAsync<TEntity>(@this.Sql, @this.DynamicParameters, orderField, isAscending, pageSize, pageIndex);

            return (list?.ToList(), total);
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <param name="orderField"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static async Task<(List<TReturn> list, long total)> ToPageAsync<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository, string orderField, bool isAscending, int pageSize, int pageIndex) where TEntity : class
        {
            var (list, total) = await repository.FindListAsync<TReturn>(@this.Sql, @this.DynamicParameters, orderField, isAscending, pageSize, pageIndex);

            return (list?.ToList(), total);
        }
        #endregion

        #region ToDataTable
        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.FindTable(@this.Sql, @this.DynamicParameters);
        }

        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <param name="orderField"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static (DataTable table, long total) ToDataTable<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository, string orderField, bool isAscending, int pageSize, int pageIndex) where TEntity : class
        {
            return repository.FindTable(@this.Sql, @this.DynamicParameters, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<DataTable> ToDataTableAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await repository.FindTableAsync(@this.Sql, @this.DynamicParameters);
        }

        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <param name="orderField"></param>
        /// <param name="isAscending"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public static async Task<(DataTable table, long total)> ToDataTableAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository, string orderField, bool isAscending, int pageSize, int pageIndex) where TEntity : class
        {
            return await repository.FindTableAsync(@this.Sql, @this.DynamicParameters, orderField, isAscending, pageSize, pageIndex);
        }
        #endregion

        #region Insert
        /// <summary>
        /// Oracle <see cref="OracleDynamicParameters"/>参数类型Insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：否</param>
        /// <param name="sqlIntercept">sql拦截委托</param>
        /// <param name="isEnableFormat">是否启用对表名和列名格式化，默认：否</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <param name="typeConverter">自定义OracleDbType转换器</param>
        /// <returns></returns>
        public static (string sql, OracleDynamicParameters) Insert<T>(
            this T entity,
            bool isEnableNullValue = false,
            Func<string, object, string> sqlIntercept = null,
            bool isEnableFormat = false,
            Func<string, string> tableNameFunc = null,
            Func<object, OracleDbType> typeConverter = null) where T : class
        {
            var oracleParameters = new OracleDynamicParameters();

            var builder = Sql.Insert<T>(() => entity, DatabaseType.Oracle, isEnableNullValue, sqlIntercept, isEnableFormat, tableNameFunc);

            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var fields = builder.Sql.Substring("(", ")").Split(',').Select(x => x.Trim(new[] { '"', ' ' })).ToList();
            var fieldValus = builder.Sql.Substring("SELECT ", " FROM DUAL", true, false).Replace(" FROM DUAL UNION ALL SELECT ", ",").Split(',').Select(x => x.Trim()).ToList();

            if (builder.Parameters.IsNotNullOrEmpty())
            {
                var index = 1;
                for (int i = 0; i < fieldValus.Count; i++)
                {
                    var field = fields[i];
                    var fieldValue = fieldValus[i];

                    if (!fieldValue.EqualIgnoreCase("NULL"))
                    {
                        var pKey = $":p__{index}";
                        var pValue = builder.Parameters[pKey];

                        var propertyInfo = propertyInfos.FirstOrDefault(x =>
                            x.GetAttribute<CusColumnAttribute>()?.Name == field ||
                            x.GetAttribute<SysColumnAttribute>()?.Name == field);

                        if (propertyInfo.IsNull())
                            propertyInfo = propertyInfos.FirstOrDefault(x => x.Name.EqualIgnoreCase(field));

                        var oracleColumn = propertyInfo.GetAttribute<OracleColumnAttribute>();

                        if (oracleColumn.IsNotNull())
                            oracleParameters.Add(pKey.TrimStart(':'), pValue, oracleColumn.DbType);
                        else
                            oracleParameters.Add(pKey.TrimStart(':'), pValue, typeConverter != null ? typeConverter(pValue) : pValue.GetOracleDbType());

                        index++;
                    }
                }
            }

            return ($"{builder.Sql[..(builder.Sql.IndexOf("SELECT") - 1)]} VALUES ({string.Join(',', fieldValus)})", oracleParameters);
        }

        /// <summary>
        /// Oracle <see cref="OracleDynamicParameters"/>参数类型Insert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：否</param>
        /// <param name="sqlIntercept">sql拦截委托</param>
        /// <param name="isEnableFormat">是否启用对表名和列名格式化，默认：否</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <param name="typeConverter">自定义OracleDbType转换器</param>
        /// <returns></returns>
        public static (string sql, OracleDynamicParameters) Insert<T>(
            this Expression<Func<object>> expression,
            bool isEnableNullValue = false,
            Func<string, object, string> sqlIntercept = null,
            bool isEnableFormat = false,
            Func<string, string> tableNameFunc = null,
            Func<object, OracleDbType> typeConverter = null) where T : class
        {
            var oracleParameters = new OracleDynamicParameters();

            var builder = Sql.Insert<T>(expression, DatabaseType.Oracle, isEnableNullValue, sqlIntercept, isEnableFormat, tableNameFunc);

            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var fields = builder.Sql.Substring("(", ")").Split(',').Select(x => x.Trim(new[] { '"', ' ' })).ToList();
            var fieldValus = builder.Sql.Substring("SELECT ", " FROM DUAL", true, false).Replace(" FROM DUAL UNION ALL SELECT ", ",").Split(',').Select(x => x.Trim()).ToList();

            if (builder.Parameters.IsNotNullOrEmpty())
            {
                var index = 1;
                for (int i = 0; i < fieldValus.Count; i++)
                {
                    var field = fields[i % fields.Count];
                    var fieldValue = fieldValus[i];

                    if (!fieldValue.EqualIgnoreCase("NULL"))
                    {
                        var pKey = $":p__{index}";
                        var pValue = builder.Parameters[pKey];

                        var propertyInfo = propertyInfos.FirstOrDefault(x =>
                            x.GetAttribute<CusColumnAttribute>()?.Name == field ||
                            x.GetAttribute<SysColumnAttribute>()?.Name == field);

                        if (propertyInfo.IsNull())
                            propertyInfo = propertyInfos.FirstOrDefault(x => x.Name.EqualIgnoreCase(field));

                        var oracleColumn = propertyInfo.GetAttribute<OracleColumnAttribute>();

                        if (oracleColumn.IsNotNull())
                            oracleParameters.Add(pKey.TrimStart(':'), pValue, oracleColumn.DbType);
                        else
                            oracleParameters.Add(pKey.TrimStart(':'), pValue, typeConverter != null ? typeConverter(pValue) : pValue.GetOracleDbType());

                        index++;
                    }
                }
            }

            return (builder.Sql, oracleParameters);
        }
        #endregion

        #region Update
        /// <summary>
        /// Oracle <see cref="OracleDynamicParameters"/>参数类型Update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">要更新的实体</param>
        /// <param name="predicate">条件表达式</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：否</param>
        /// <param name="sqlIntercept">sql拦截委托</param>
        /// <param name="isEnableFormat">是否启用对表名和列名格式化，默认：否</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <param name="typeConverter">自定义OracleDbType转换器</param>
        /// <returns></returns>
        public static (string sql, OracleDynamicParameters) Update<T>(
            this T entity,
            Expression<Func<T, bool>> predicate,
            bool isEnableNullValue = false,
            Func<string, object, string> sqlIntercept = null,
            bool isEnableFormat = false,
            Func<string, string> tableNameFunc = null,
            Func<object, OracleDbType> typeConverter = null) where T : class
        {
            return Update<T>(() => entity, predicate, isEnableNullValue, sqlIntercept, isEnableFormat, tableNameFunc, typeConverter);
        }

        /// <summary>
        /// Oracle <see cref="OracleDynamicParameters"/>参数类型Update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="predicate">条件表达式</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：否</param>
        /// <param name="sqlIntercept">sql拦截委托</param>
        /// <param name="isEnableFormat">是否启用对表名和列名格式化，默认：否</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <param name="typeConverter">自定义OracleDbType转换器</param>
        /// <returns></returns>
        public static (string sql, OracleDynamicParameters) Update<T>(
            this Expression<Func<object>> expression,
            Expression<Func<T, bool>> predicate,
            bool isEnableNullValue = false,
            Func<string, object, string> sqlIntercept = null,
            bool isEnableFormat = false,
            Func<string, string> tableNameFunc = null,
            Func<object, OracleDbType> typeConverter = null) where T : class
        {
            var oracleParameters = new OracleDynamicParameters();

            var builder = Sql.Update<T>(expression, DatabaseType.Oracle, isEnableNullValue, sqlIntercept, isEnableFormat, tableNameFunc).Where(predicate);

            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var fields = builder.Sql.Substring("SET", "WHERE").Split(',').Select(x => x.Trim()).ToList();

            if (builder.Parameters.IsNotNullOrEmpty())
            {
                var index = 1;

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i].Split('=')[0].Trim(new[] { '"', ' ' });
                    var fieldValue = fields[i].Split('=')[1].Trim();

                    if (!fieldValue.EqualIgnoreCase("NULL"))
                    {
                        var pKey = $":p__{index}";
                        var pValue = builder.Parameters[pKey];

                        var propertyInfo = propertyInfos.FirstOrDefault(x =>
                            x.GetAttribute<CusColumnAttribute>()?.Name == field ||
                            x.GetAttribute<SysColumnAttribute>()?.Name == field);

                        if (propertyInfo.IsNull())
                            propertyInfo = propertyInfos.FirstOrDefault(x => x.Name.EqualIgnoreCase(field));

                        var oracleColumn = propertyInfo.GetAttribute<OracleColumnAttribute>();

                        if (oracleColumn.IsNotNull())
                            oracleParameters.Add(pKey.TrimStart(':'), pValue, oracleColumn.DbType);
                        else
                            oracleParameters.Add(pKey.TrimStart(':'), pValue, typeConverter != null ? typeConverter(pValue) : pValue.GetOracleDbType());

                        index++;
                    }
                }

                for (var i = index - 1; i < builder.Parameters.Count; i++)
                {
                    var pKey = $":p__{i + 1}";
                    var pValue = builder.Parameters[pKey];
                    oracleParameters.Add(pKey.TrimStart(':'), pValue, typeConverter != null ? typeConverter(pValue) : pValue.GetOracleDbType());
                }
            }

            return (builder.Sql, oracleParameters);
        }
        #endregion
    }
}