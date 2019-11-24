#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
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
using System.Linq.Expressions;

namespace SQLBuilder.Core
{
    /// <summary>
    /// SqlBuilder
    /// </summary>
	public static class SqlBuilder
    {
        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：是</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Insert<T>(Expression<Func<object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer, bool isEnableNullValue = true)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Insert(expression, isEnableNullValue);
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Delete<T>(DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Delete();
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：是</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Update<T>(Expression<Func<object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer, bool isEnableNullValue = true)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Update(expression, isEnableNullValue);
        }
        #endregion

        #region Select
        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T>(Expression<Func<T, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2>(Expression<Func<T, T2, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3>(Expression<Func<T, T2, T3, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
            where T3 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4>(Expression<Func<T, T2, T3, T4, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Select<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Select(expression);
        }
        #endregion

        #region Max
        /// <summary>
        /// Max
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Max<T>(Expression<Func<T, object>> expression, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Max(expression);
        }
        #endregion

        #region Min
        /// <summary>
        /// Min
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Min<T>(Expression<Func<T, object>> expression, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Min(expression);
        }
        #endregion

        #region Avg
        /// <summary>
        /// Avg
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Avg<T>(Expression<Func<T, object>> expression, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Avg(expression);
        }
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Count<T>(Expression<Func<T, object>> expression = null, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Count(expression);
        }
        #endregion

        #region Sum
        /// <summary>
        /// Sum
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="DatabaseType">数据库类型</param>
        /// <returns>SqlBuilderCore</returns>
        public static SqlBuilderCore<T> Sum<T>(Expression<Func<T, object>> expression, DatabaseType DatabaseType = DatabaseType.SQLServer)
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType).Sum(expression);
        }
        #endregion

        #region GetTableName
        /// <summary>
        /// 获取实体对应的数据库表名
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>string</returns>
        public static string GetTableName<T>()
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType.SQLServer).GetTableName();
        }
        #endregion

        #region GetPrimaryKey
        /// <summary>
        /// 获取实体对应的数据库表的主键名(多主键)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetPrimaryKey<T>()
            where T : class
        {
            return new SqlBuilderCore<T>(DatabaseType.SQLServer).GetPrimaryKey();
        }
        #endregion
    }
}
