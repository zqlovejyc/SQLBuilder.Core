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

using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示包含集合初始值设定项的构造函数调用
    /// </summary>
    public class ListInitExpressionResolve : BaseExpression<ListInitExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Insert(ListInitExpression expression, SqlWrapper sqlWrapper)
        {
            var fields = new List<string>();
            var array = expression.ToObject() as IEnumerable<object>;
            for (var i = 0; i < array.Count(); i++)
            {
                if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                    sqlWrapper.Append("(");

                if (i > 0 && sqlWrapper.DatabaseType == DatabaseType.Oracle)
                    sqlWrapper.Append(" UNION ALL SELECT ");

                var properties = array.ElementAt(i)?.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var type = p.DeclaringType.ToString().Contains("AnonymousType") ?
                        sqlWrapper.DefaultType :
                        p.DeclaringType;

                    var (columnName, isInsert, isUpdate) = sqlWrapper.GetColumnInfo(type, p);
                    if (isInsert)
                    {
                        var value = p.GetValue(array.ElementAt(i), null);
                        if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                        {
                            sqlWrapper.AddDbParameter(value);
                            if (!fields.Contains(columnName))
                                fields.Add(columnName);
                            sqlWrapper += ",";
                        }
                    }
                }

                if (sqlWrapper[^1] == ',')
                {
                    sqlWrapper.Remove(sqlWrapper.Length - 1, 1);
                    if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                        sqlWrapper.Append("),");
                    else
                        sqlWrapper.Append(" FROM DUAL");
                }
            }

            if (sqlWrapper[^1] == ',')
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);

            sqlWrapper.Reset(string.Format(sqlWrapper.ToString(), string.Join(",", fields).TrimEnd(',')));

            return sqlWrapper;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper GroupBy(ListInitExpression expression, SqlWrapper sqlWrapper)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlExpressionProvider.GroupBy(Expression.Constant(array[i], array[i].GetType()), sqlWrapper);
                }
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);
            }

            return sqlWrapper;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper OrderBy(ListInitExpression expression, SqlWrapper sqlWrapper, params OrderType[] orders)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlExpressionProvider.OrderBy(Expression.Constant(array[i], array[i].GetType()), sqlWrapper);

                    if (i <= orders.Length - 1)
                        sqlWrapper += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                    else if (!array[i].ToString().ToUpper().Contains("ASC") && !array[i].ToString().ToUpper().Contains("DESC"))
                        sqlWrapper += " ASC,";
                    else
                        sqlWrapper += ",";
                }
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);
            }

            return sqlWrapper;
        }
        #endregion
    }
}