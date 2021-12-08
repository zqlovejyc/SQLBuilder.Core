#region License
/***
 * Copyright © 2018-2022, 张强 (943620963@qq.com).
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
using SQLBuilder.Core.Extensions;
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示具有条件运算符的表达式
    /// </summary>
    public class ConditionalExpressionReslover : BaseExpression<ConditionalExpression>
    {
        #region Where
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Where(ConditionalExpression expression, SqlWrapper sqlWrapper)
        {
            var isNot = false;
            var testExpression = expression.Test;

            //UnaryExpression
            if (testExpression is UnaryExpression unaryExpression)
            {
                isNot = unaryExpression.NodeType == ExpressionType.Not;
                testExpression = unaryExpression.Operand;
            }

            //MethodCallExpression
            if (testExpression is MethodCallExpression methodCallExpression)
            {
                var test = methodCallExpression.ToObject<bool>(out var res);
                if (res)
                {
                    if (isNot ? !test : test)
                        SqlExpressionProvider.Where(expression.IfTrue, sqlWrapper);
                    else
                        SqlExpressionProvider.Where(expression.IfFalse, sqlWrapper);
                }
            }

            //BinaryExpression
            if (testExpression is BinaryExpression binaryExpression)
            {
                var test = binaryExpression.ToObject<bool>(out var res);
                if (!res)
                {
                    test = binaryExpression.Left.ToObject<bool>(out var left);
                    if (!left)
                    {
                        SqlExpressionProvider.Where(binaryExpression.Left, sqlWrapper);
                        BinaryExpressionResolver.OperatorResolver(
                            binaryExpression.NodeType,
                            sqlWrapper.Length,
                            sqlWrapper,
                            sqlWrapper.EndsWith("NULL"));
                    }

                    var right = false;
                    if (!left)
                        test = binaryExpression.Right.ToObject<bool>(out right);

                    if (left || !right)
                    {
                        SqlExpressionProvider.Where(binaryExpression.Right, sqlWrapper);
                        BinaryExpressionResolver.OperatorResolver(
                            binaryExpression.NodeType,
                            sqlWrapper.Length,
                            sqlWrapper,
                            sqlWrapper.EndsWith("NULL"));
                    }
                }

                if (isNot ? !test : test)
                    SqlExpressionProvider.Where(expression.IfTrue, sqlWrapper);
                else
                    SqlExpressionProvider.Where(expression.IfFalse, sqlWrapper);
            }

            return sqlWrapper;
        }
        #endregion
    }
}
