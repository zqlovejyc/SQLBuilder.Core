#region License
/***
 * Copyright © 2018-2021, 张强 (943620963@qq.com).
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
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 描述一个lambda表达式
    /// </summary>
    public class LambdaExpressionResolver : BaseExpression<LambdaExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Where(LambdaExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Where(expression.Body, sqlWrapper);

            return sqlWrapper;
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Having(LambdaExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Having(expression.Body, sqlWrapper);

            return sqlWrapper;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Select(LambdaExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Select(expression.Body, sqlWrapper);

            return sqlWrapper;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Join(LambdaExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Join(expression.Body, sqlWrapper);

            return sqlWrapper;
        }
        #endregion
    }
}