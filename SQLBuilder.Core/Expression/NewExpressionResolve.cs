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

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SQLBuilder.Core
{
    /// <summary>
    /// 表示一个构造函数调用
    /// </summary>
	public class NewExpressionResolve : BaseSqlBuilder<NewExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Update(NewExpression expression, SqlPack sqlPack)
        {
            for (int i = 0; i < expression.Members.Count; i++)
            {
                var m = expression.Members[i];
                var t = m.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : m.DeclaringType;
                (string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(t, m);
                if (isUpdate)
                {
                    var value = expression.Arguments[i]?.ToObject();
                    if (value != null || (sqlPack.IsEnableNullValue && value == null))
                    {
                        sqlPack += columnName + " = ";
                        sqlPack.AddDbParameter(value);
                        sqlPack += ",";
                    }
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
            {
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(NewExpression expression, SqlPack sqlPack)
        {
            if (sqlPack.DatabaseType != DatabaseType.Oracle)
                sqlPack.Sql.Append("(");
            var fields = new List<string>();
            for (int i = 0; i < expression.Members?.Count; i++)
            {
                var m = expression.Members[i];
                var t = m.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : m.DeclaringType;
                (string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(t, m);
                if (isInsert)
                {
                    var value = expression.Arguments[i]?.ToObject();
                    if (value != null || (sqlPack.IsEnableNullValue && value == null))
                    {
                        sqlPack.AddDbParameter(value);
                        if (!fields.Contains(columnName)) fields.Add(columnName);
                        sqlPack += ",";
                    }
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
            {
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                if (sqlPack.DatabaseType != DatabaseType.Oracle)
                    sqlPack.Sql.Append(")");
                else
                    sqlPack.Sql.Append(" FROM DUAL");
            }
            sqlPack.Sql = new StringBuilder(string.Format(sqlPack.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlPack;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Select(NewExpression expression, SqlPack sqlPack)
        {
            for (var i = 0; i < expression.Members.Count; i++)
            {
                var argument = expression.Arguments[i];
                var member = expression.Members[i];
                SqlBuilderProvider.Select(argument, sqlPack);
                //添加字段别名
                if (argument is MemberExpression memberExpression && memberExpression.Member.Name != member.Name)
                    sqlPack.SelectFields[sqlPack.SelectFields.Count - 1] += " AS " + member.Name;
            }
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(NewExpression expression, SqlPack sqlPack)
        {
            foreach (Expression item in expression.Arguments)
            {
                SqlBuilderProvider.GroupBy(item, sqlPack);
            }
            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(NewExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            for (var i = 0; i < expression.Arguments.Count; i++)
            {
                SqlBuilderProvider.OrderBy(expression.Arguments[i], sqlPack);
                if (i <= orders.Length - 1)
                    sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                else
                    sqlPack += " ASC,";
            }
            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            return sqlPack;
        }
        #endregion
    }
}