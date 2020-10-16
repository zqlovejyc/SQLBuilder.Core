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
    /// 表示具有一元运算符的表达式
    /// </summary>
	public class UnaryExpressionResolve : BaseExpression<UnaryExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Select(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Select(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Insert(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Insert(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Update(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Update(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Where(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            var startIndex = sqlWrapper.Length;
            SqlExpressionProvider.Where(expression.Operand, sqlWrapper);
            if (expression.NodeType == ExpressionType.Not)
            {
                var subString = sqlWrapper.ToString().Substring(startIndex, sqlWrapper.ToString().Length - startIndex).ToUpper();

                //IS NOT、IS                     
                if (subString.Contains("IS NOT"))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("IS NOT");
                    if (index != -1) sqlWrapper.Sql.Replace("IS NOT", "IS", index, 6);
                }
                if (subString.Contains("IS") && subString.LastIndexOf("IS") != subString.LastIndexOf("IS NOT"))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("IS");
                    if (index != -1) sqlWrapper.Sql.Replace("IS", "IS NOT", index, 2);
                }

                //NOT LIKE、LIKE
                if (subString.Contains("NOT LIKE"))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("NOT LIKE");
                    if (index != -1) sqlWrapper.Sql.Replace("NOT LIKE", "LIKE", index, 8);
                }
                if (subString.Contains("LIKE") && subString.LastIndexOf("LIKE") != (subString.LastIndexOf("NOT LIKE") + 4))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("LIKE");
                    if (index != -1) sqlWrapper.Sql.Replace("LIKE", "NOT LIKE", index, 4);
                }

                //NOT IN、IN
                if (subString.Contains("NOT IN"))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("NOT IN");
                    if (index != -1) sqlWrapper.Sql.Replace("NOT IN", "IN", index, 6);
                }
                if (subString.Contains("IN") && subString.LastIndexOf("IN") != (subString.LastIndexOf("NOT IN") + 4))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("IN");
                    if (index != -1) sqlWrapper.Sql.Replace("IN", "NOT IN", index, 2);
                }

                //AND、OR
                if (subString.Contains("AND"))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("AND");
                    if (index != -1) sqlWrapper.Sql.Replace("AND", "OR", index, 3);
                }
                if (subString.Contains("OR"))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("OR");
                    if (index != -1) sqlWrapper.Sql.Replace("OR", "AND", index, 2);
                }

                //=、<>
                if (subString.Contains(" = "))
                {
                    var index = sqlWrapper.ToString().LastIndexOf(" = ");
                    if (index != -1) sqlWrapper.Sql.Replace(" = ", " <> ", index, 3);
                }
                if (subString.Contains("<>"))
                {
                    var index = sqlWrapper.ToString().LastIndexOf("<>");
                    if (index != -1) sqlWrapper.Sql.Replace("<>", "=", index, 2);
                }

                //>、<
                if (subString.Contains(" > "))
                {
                    var index = sqlWrapper.ToString().LastIndexOf(" > ");
                    if (index != -1) sqlWrapper.Sql.Replace(" > ", " <= ", index, 3);
                }
                if (subString.Contains(" < "))
                {
                    var index = sqlWrapper.ToString().LastIndexOf(" < ");
                    if (index != -1) sqlWrapper.Sql.Replace(" < ", " >= ", index, 3);
                }

                //>=、<=
                if (subString.Contains(" >= "))
                {
                    var index = sqlWrapper.ToString().LastIndexOf(" >= ");
                    if (index != -1) sqlWrapper.Sql.Replace(" >= ", " < ", index, 4);
                }
                if (subString.Contains(" <= "))
                {
                    var index = sqlWrapper.ToString().LastIndexOf(" <= ");
                    if (index != -1) sqlWrapper.Sql.Replace(" <= ", " > ", index, 4);
                }
            }
            return sqlWrapper;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper GroupBy(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.GroupBy(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper OrderBy(UnaryExpression expression, SqlWrapper sqlWrapper, params OrderType[] orders)
        {
            SqlExpressionProvider.OrderBy(expression.Operand, sqlWrapper, orders);
            return sqlWrapper;
        }

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Max(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Max(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Min(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Min(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Avg(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Avg(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Count(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Count(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Sum(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Sum(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Join(UnaryExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Join(expression.Operand, sqlWrapper);
            return sqlWrapper;
        }
        #endregion
    }
}