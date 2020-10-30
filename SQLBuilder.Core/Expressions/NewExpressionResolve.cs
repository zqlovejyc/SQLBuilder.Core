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
using System.Linq.Expressions;
using System.Text;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示一个构造函数调用
    /// </summary>
	public class NewExpressionResolve : BaseExpression<NewExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Update(NewExpression expression, SqlWrapper sqlWrapper)
        {
            for (int i = 0; i < expression.Members.Count; i++)
            {
                var m = expression.Members[i];
                var t = m.DeclaringType.ToString().Contains("AnonymousType") ? sqlWrapper.DefaultType : m.DeclaringType;
                (string columnName, bool isInsert, bool isUpdate) = sqlWrapper.GetColumnInfo(t, m);
                if (isUpdate)
                {
                    var value = expression.Arguments[i]?.ToObject();
                    if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                    {
                        sqlWrapper += columnName + " = ";
                        sqlWrapper.AddDbParameter(value);
                        sqlWrapper += ",";
                    }
                }
            }
            if (sqlWrapper[sqlWrapper.Length - 1] == ',')
            {
                sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
            }
            return sqlWrapper;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Insert(NewExpression expression, SqlWrapper sqlWrapper)
        {
            if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                sqlWrapper.Sql.Append("(");
            var fields = new List<string>();
            for (int i = 0; i < expression.Members?.Count; i++)
            {
                var m = expression.Members[i];
                var t = m.DeclaringType.ToString().Contains("AnonymousType") ? sqlWrapper.DefaultType : m.DeclaringType;
                (string columnName, bool isInsert, bool isUpdate) = sqlWrapper.GetColumnInfo(t, m);
                if (isInsert)
                {
                    var value = expression.Arguments[i]?.ToObject();
                    if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                    {
                        sqlWrapper.AddDbParameter(value);
                        if (!fields.Contains(columnName))
                            fields.Add(columnName);
                        sqlWrapper += ",";
                    }
                }
            }
            if (sqlWrapper[sqlWrapper.Length - 1] == ',')
            {
                sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
                if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                    sqlWrapper.Sql.Append(")");
                else
                    sqlWrapper.Sql.Append(" FROM DUAL");
            }
            sqlWrapper.Sql = new StringBuilder(string.Format(sqlWrapper.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlWrapper;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Select(NewExpression expression, SqlWrapper sqlWrapper)
        {
            for (var i = 0; i < expression.Members.Count; i++)
            {
                var argument = expression.Arguments[i];
                var member = expression.Members[i];
                SqlExpressionProvider.Select(argument, sqlWrapper);

                //添加字段别名
                if (argument is MemberExpression memberExpression && memberExpression.Member.Name != member.Name)
                    sqlWrapper.SelectFields[sqlWrapper.SelectFields.Count - 1] += $" AS {sqlWrapper.GetFormatName(member.Name)}";
                else if (argument is ConstantExpression constantExpression && constantExpression.Value?.ToString() != member.Name)
                    sqlWrapper.SelectFields[sqlWrapper.SelectFields.Count - 1] += $" AS {sqlWrapper.GetFormatName(member.Name)}";
            }
            return sqlWrapper;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper GroupBy(NewExpression expression, SqlWrapper sqlWrapper)
        {
            foreach (Expression item in expression.Arguments)
            {
                SqlExpressionProvider.GroupBy(item, sqlWrapper);
                sqlWrapper += ",";
            }
            sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
            return sqlWrapper;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper OrderBy(NewExpression expression, SqlWrapper sqlWrapper, params OrderType[] orders)
        {
            for (var i = 0; i < expression.Arguments.Count; i++)
            {
                SqlExpressionProvider.OrderBy(expression.Arguments[i], sqlWrapper);
                if (i <= orders.Length - 1)
                    sqlWrapper += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                else
                    sqlWrapper += " ASC,";
            }
            sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
            return sqlWrapper;
        }
        #endregion
    }
}