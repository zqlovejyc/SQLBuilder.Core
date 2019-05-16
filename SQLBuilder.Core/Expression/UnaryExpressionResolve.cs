#region License
/***
 * Copyright © 2018-2019, 张强 (943620963@qq.com).
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
    /// 表示具有一元运算符的表达式
    /// </summary>
	public class UnaryExpressionResolve : BaseSqlBuilder<UnaryExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Select(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Select(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Insert(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Update(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Update(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(UnaryExpression expression, SqlPack sqlPack)
        {
            var startIndex = sqlPack.Length;
            SqlBuilderProvider.Where(expression.Operand, sqlPack);
            if (expression.NodeType == ExpressionType.Not)
            {
                var subString = sqlPack.ToString().Substring(startIndex, sqlPack.ToString().Length - startIndex).ToUpper();

                //IS NOT、IS                     
                if (subString.Contains("IS NOT"))
                {
                    var index = sqlPack.ToString().LastIndexOf("IS NOT");
                    if (index != -1) sqlPack.Sql.Replace("IS NOT", "IS", index, 6);
                }
                if (subString.Contains("IS") && subString.LastIndexOf("IS") != subString.LastIndexOf("IS NOT"))
                {
                    var index = sqlPack.ToString().LastIndexOf("IS");
                    if (index != -1) sqlPack.Sql.Replace("IS", "IS NOT", index, 2);
                }

                //NOT LIKE、LIKE
                if (subString.Contains("NOT LIKE"))
                {
                    var index = sqlPack.ToString().LastIndexOf("NOT LIKE");
                    if (index != -1) sqlPack.Sql.Replace("NOT LIKE", "LIKE", index, 8);
                }
                if (subString.Contains("LIKE") && subString.LastIndexOf("LIKE") != (subString.LastIndexOf("NOT LIKE") + 4))
                {
                    var index = sqlPack.ToString().LastIndexOf("LIKE");
                    if (index != -1) sqlPack.Sql.Replace("LIKE", "NOT LIKE", index, 4);
                }

                //=、<>
                if (subString.Contains(" = "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" = ");
                    if (index != -1) sqlPack.Sql.Replace(" = ", " <> ", index, 3);
                }
                if (subString.Contains("<>"))
                {
                    var index = sqlPack.ToString().LastIndexOf("<>");
                    if (index != -1) sqlPack.Sql.Replace("<>", "=", index, 2);
                }

                //>、<
                if (subString.Contains(" > "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" > ");
                    if (index != -1) sqlPack.Sql.Replace(" > ", " <= ", index, 3);
                }
                if (subString.Contains(" < "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" < ");
                    if (index != -1) sqlPack.Sql.Replace(" < ", " >= ", index, 3);
                }

                //>=、<=
                if (subString.Contains(" >= "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" >= ");
                    if (index != -1) sqlPack.Sql.Replace(" >= ", " < ", index, 4);
                }
                if (subString.Contains(" <= "))
                {
                    var index = sqlPack.ToString().LastIndexOf(" <= ");
                    if (index != -1) sqlPack.Sql.Replace(" <= ", " > ", index, 4);
                }

                //AND、OR
                if (subString.Contains("AND"))
                {
                    var index = sqlPack.ToString().LastIndexOf("AND");
                    if (index != -1) sqlPack.Sql.Replace("AND", "OR", index, 3);
                }
                if (subString.Contains("OR"))
                {
                    var index = sqlPack.ToString().LastIndexOf("OR");
                    if (index != -1) sqlPack.Sql.Replace("OR", "AND", index, 2);
                }
            }
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack GroupBy(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.GroupBy(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(UnaryExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            SqlBuilderProvider.OrderBy(expression.Operand, sqlPack, orders);
            return sqlPack;
        }

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Max(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Max(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Min(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Min(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Avg(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Avg(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Count(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Count(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Sum(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Sum(expression.Operand, sqlPack);
            return sqlPack;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Join(UnaryExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Join(expression.Operand, sqlPack);
            return sqlPack;
        }
        #endregion
    }
}