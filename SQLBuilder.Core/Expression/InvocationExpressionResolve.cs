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
    /// 表示将委托或lambda表达式应用于参数表达式列表的表达式
    /// </summary>
    public class InvocationExpressionResolve : BaseSqlBuilder<InvocationExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(InvocationExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Where(expression.Expression, sqlPack);
            return sqlPack;
        }
        #endregion
    }
}
