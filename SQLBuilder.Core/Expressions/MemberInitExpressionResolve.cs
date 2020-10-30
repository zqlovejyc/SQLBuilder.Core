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
    /// 表示调用构造函数并初始化新对象的一个或多个成员
    /// </summary>
    public class MemberInitExpressionResolve : BaseExpression<MemberInitExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Insert(MemberInitExpression expression, SqlWrapper sqlWrapper)
        {
            if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                sqlWrapper.Sql.Append("(");
            var fields = new List<string>();
            foreach (MemberAssignment m in expression.Bindings)
            {
                var type = m.Member.DeclaringType.ToString().Contains("AnonymousType") ? sqlWrapper.DefaultType : m.Member.DeclaringType;
                (string columnName, bool isInsert, bool isUpdate) = sqlWrapper.GetColumnInfo(type, m.Member);
                if (isInsert)
                {
                    var value = m.Expression.ToObject();
                    if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                    {
                        sqlWrapper.AddDbParameter(value);
                        if (!fields.Contains(columnName)) fields.Add(columnName);
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
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Update(MemberInitExpression expression, SqlWrapper sqlWrapper)
        {
            foreach (MemberAssignment m in expression.Bindings)
            {
                var type = m.Member.DeclaringType.ToString().Contains("AnonymousType") ? sqlWrapper.DefaultType : m.Member.DeclaringType;
                (string columnName, bool isInsert, bool isUpdate) = sqlWrapper.GetColumnInfo(type, m.Member);
                if (isUpdate)
                {
                    var value = m.Expression.ToObject();
                    if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                    {
                        sqlWrapper += columnName + " = ";
                        sqlWrapper.AddDbParameter(value);
                        sqlWrapper += ",";
                    }
                }
            }
            if (sqlWrapper[sqlWrapper.Length - 1] == ',')
                sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
            return sqlWrapper;
        }
        #endregion
    }
}