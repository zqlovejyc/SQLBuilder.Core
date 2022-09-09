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

using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示调用构造函数并初始化新对象的一个或多个成员
    /// </summary>
    public class MemberInitExpressionResolver : BaseExpression<MemberInitExpression>
    {
        #region Select
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Select(MemberInitExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Bindings?.Count > 0)
            {
                foreach (MemberAssignment memberAssignment in expression.Bindings)
                {
                    if (memberAssignment.Expression is MemberExpression memberExpr && memberExpr.Expression is ParameterExpression parameterExpr)
                    {
                        var type = parameterExpr.Type;
                        var tableName = sqlWrapper.GetTableName(type);
                        var tableAlias = sqlWrapper.GetTableAlias(tableName, parameterExpr.Name);

                        if (tableAlias.IsNotNullOrEmpty())
                            tableAlias += ".";

                        var fieldName = tableAlias + GetColumnInfo(type, memberExpr.Member, sqlWrapper).ColumnName;

                        sqlWrapper.AddField(fieldName);
                    }
                    else
                    {
                        var fieldName = memberAssignment.Expression.ToObject().ToString();

                        sqlWrapper.AddField(fieldName);
                    }

                    var aliasName = sqlWrapper.GetColumnName(memberAssignment.Member.Name);

                    var field = sqlWrapper.SelectFields[sqlWrapper.FieldCount - 1];

                    if (field.IsNotNullOrEmpty() && field.Contains(".") && !field.Contains("(", ")"))
                        field = field.Split('.').LastOrDefault();

                    if (!field.Equals(!sqlWrapper.IsEnableFormat, aliasName))
                        sqlWrapper.SelectFields[sqlWrapper.FieldCount - 1] += $" AS {aliasName}";
                }
            }
            else
            {
                sqlWrapper.AddField("*");
            }

            return sqlWrapper;
        }
        #endregion

        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Insert(MemberInitExpression expression, SqlWrapper sqlWrapper)
        {
            if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                sqlWrapper.Append("(");

            var fields = new List<string>();
            foreach (MemberAssignment m in expression.Bindings)
            {
                var type = m.Member.DeclaringType.IsAnonymousType() ?
                    sqlWrapper.DefaultType :
                    m.Member.DeclaringType;

                var columnInfo = GetColumnInfo(type, m.Member, sqlWrapper);
                if (columnInfo.IsInsert)
                {
                    var value = m.Expression.ToObject();
                    if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                    {
                        sqlWrapper.AddDbParameter(value, columnInfo.DataType);
                        if (!fields.Contains(columnInfo.ColumnName))
                            fields.Add(columnInfo.ColumnName);
                        sqlWrapper += ",";
                    }
                }
            }

            if (sqlWrapper[^1] == ',')
            {
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);
                if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                    sqlWrapper.Append(")");
                else
                    sqlWrapper.Append(" FROM DUAL");
            }

            sqlWrapper.Reset(string.Format(sqlWrapper.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlWrapper;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Update(MemberInitExpression expression, SqlWrapper sqlWrapper)
        {
            foreach (MemberAssignment m in expression.Bindings)
            {
                var type = m.Member.DeclaringType.IsAnonymousType() ?
                    sqlWrapper.DefaultType :
                    m.Member.DeclaringType;

                var columnInfo = GetColumnInfo(type, m.Member, sqlWrapper);
                if (columnInfo.IsUpdate)
                {
                    var value = m.Expression.ToObject();
                    if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                    {
                        sqlWrapper += columnInfo.ColumnName + " = ";
                        sqlWrapper.AddDbParameter(value, columnInfo.DataType);
                        sqlWrapper += ",";
                    }
                }
            }

            sqlWrapper.RemoveLast(',');

            return sqlWrapper;
        }
        #endregion
    }
}