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
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示创建一个新数组，并可能初始化该新数组的元素
    /// </summary>
	public class NewArrayExpressionResolve : BaseExpression<NewArrayExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper In(NewArrayExpression expression, SqlWrapper sqlWrapper)
        {
            sqlWrapper += "(";
            foreach (Expression expressionItem in expression.Expressions)
            {
                SqlExpressionProvider.In(expressionItem, sqlWrapper);
                sqlWrapper += ",";
            }

            if (sqlWrapper[^1] == ',')
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);

            sqlWrapper += ")";

            return sqlWrapper;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Insert(NewArrayExpression expression, SqlWrapper sqlWrapper)
        {
            foreach (Expression expressionItem in expression.Expressions)
            {
                SqlExpressionProvider.Insert(expressionItem, sqlWrapper);
                if (sqlWrapper.DatabaseType == DatabaseType.Oracle)
                    sqlWrapper += " UNION ALL SELECT ";
                else
                    sqlWrapper += ",";
            }

            if (sqlWrapper[^1] == ',')
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);

            if (sqlWrapper.LastIndexOf(" UNION ALL SELECT ") > -1)
                sqlWrapper.Remove(sqlWrapper.Length - 18, 18);

            return sqlWrapper;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper GroupBy(NewArrayExpression expression, SqlWrapper sqlWrapper)
        {
            for (var i = 0; i < expression.Expressions.Count; i++)
            {
                SqlExpressionProvider.GroupBy(expression.Expressions[i], sqlWrapper);
            }

            sqlWrapper.Remove(sqlWrapper.Length - 1, 1);

            return sqlWrapper;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper OrderBy(NewArrayExpression expression, SqlWrapper sqlWrapper, params OrderType[] orders)
        {
            for (var i = 0; i < expression.Expressions.Count; i++)
            {
                SqlExpressionProvider.OrderBy(expression.Expressions[i], sqlWrapper);

                if (i <= orders.Length - 1)
                    sqlWrapper += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                else if (expression.Expressions[i] is ConstantExpression order)
                {
                    if (!order.Value.ToString().ToUpper().Contains("ASC") && !order.Value.ToString().ToUpper().Contains("DESC"))
                        sqlWrapper += " ASC,";
                    else
                        sqlWrapper += ",";
                }
                else
                    sqlWrapper += " ASC,";
            }

            sqlWrapper.Remove(sqlWrapper.Length - 1, 1);

            return sqlWrapper;
        }
        #endregion
    }
}