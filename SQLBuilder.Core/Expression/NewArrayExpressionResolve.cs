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

using System.Linq.Expressions;

namespace SQLBuilder.Core
{
    /// <summary>
    /// 表示创建一个新数组，并可能初始化该新数组的元素
    /// </summary>
	public class NewArrayExpressionResolve : BaseSqlBuilder<NewArrayExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack In(NewArrayExpression expression, SqlPack sqlPack)
        {
            sqlPack += "(";
            foreach (Expression expressionItem in expression.Expressions)
            {
                SqlBuilderProvider.In(expressionItem, sqlPack);
                sqlPack += ",";
            }
            if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
            sqlPack += ")";
            return sqlPack;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(NewArrayExpression expression, SqlPack sqlPack)
        {
            foreach (Expression expressionItem in expression.Expressions)
            {
                SqlBuilderProvider.Insert(expressionItem, sqlPack);
                if (sqlPack.DatabaseType == DatabaseType.Oracle)
                    sqlPack += " UNION ALL SELECT ";
                else
                    sqlPack += ",";
            }
            if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
            if (sqlPack.Sql.ToString().LastIndexOf(" UNION ALL SELECT ") > -1)
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 18, 18);
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack GroupBy(NewArrayExpression expression, SqlPack sqlPack)
        {
            for (var i = 0; i < expression.Expressions.Count; i++)
            {
                SqlBuilderProvider.GroupBy(expression.Expressions[i], sqlPack);
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
        public override SqlPack OrderBy(NewArrayExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            for (var i = 0; i < expression.Expressions.Count; i++)
            {
                SqlBuilderProvider.OrderBy(expression.Expressions[i], sqlPack);
                if (i <= orders.Length - 1)
                {
                    sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                }
                else if (expression.Expressions[i] is ConstantExpression order)
                {
                    if (!order.Value.ToString().ToUpper().Contains("ASC") && !order.Value.ToString().ToUpper().Contains("DESC"))
                        sqlPack += " ASC,";
                    else
                        sqlPack += ",";
                }
                else
                {
                    sqlPack += " ASC,";
                }
            }
            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            return sqlPack;
        }
        #endregion
    }
}