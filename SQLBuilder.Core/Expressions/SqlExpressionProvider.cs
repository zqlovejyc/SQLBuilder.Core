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
using System;
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// SqlBuilderProvider
    /// </summary>
	public class SqlExpressionProvider
    {
        #region GetExpressionResolver
        /// <summary>
        /// GetExpressionResolver
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>IExpression</returns>
        private static IExpression GetExpressionResolver(Expression expression) =>
            expression switch
            {
                ConstantExpression => new ConstantExpressionResolver(),
                BinaryExpression => new BinaryExpressionResolver(),
                MemberExpression => new MemberExpressionResolver(),
                MethodCallExpression => new MethodCallExpressionResolver(),
                NewArrayExpression => new NewArrayExpressionResolver(),
                NewExpression => new NewExpressionResolver(),
                UnaryExpression => new UnaryExpressionResolver(),
                MemberInitExpression => new MemberInitExpressionResolver(),
                ListInitExpression => new ListInitExpressionResolver(),
                InvocationExpression => new InvocationExpressionResolver(),
                LambdaExpression => new LambdaExpressionResolver(),
                ParameterExpression => new ParameterExpressionResolver(),
                ConditionalExpression => new ConditionalExpressionReslover(),
                BlockExpression => throw new NotImplementedException($"NotImplemented {nameof(BlockExpression)}"),
                DebugInfoExpression => throw new NotImplementedException($"NotImplemented {nameof(DebugInfoExpression)}"),
                DefaultExpression => throw new NotImplementedException($"NotImplemented {nameof(DefaultExpression)}"),
                DynamicExpression => throw new NotImplementedException($"NotImplemented {nameof(DynamicExpression)}"),
                GotoExpression => throw new NotImplementedException($"NotImplemented {nameof(GotoExpression)}"),
                IndexExpression => throw new NotImplementedException($"NotImplemented {nameof(IndexExpression)}"),
                LabelExpression => throw new NotImplementedException($"NotImplemented {nameof(LabelExpression)}"),
                LoopExpression => throw new NotImplementedException($"NotImplemented {nameof(LoopExpression)}"),
                SwitchExpression => throw new NotImplementedException($"NotImplemented {nameof(SwitchExpression)}"),
                TryExpression => throw new NotImplementedException($"NotImplemented {nameof(TryExpression)}"),
                TypeBinaryExpression => throw new NotImplementedException($"NotImplemented {nameof(TypeBinaryExpression)}"),
                RuntimeVariablesExpression => throw new NotImplementedException($"NotImplemented {nameof(RuntimeVariablesExpression)}"),
                _ => throw new NotImplementedException($"NotImplemented {nameof(Expression)}")
            };
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Update(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Update(expression, sqlWrapper);
        #endregion

        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Insert(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Insert(expression, sqlWrapper);
        #endregion

        #region Select
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Select(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Select(expression, sqlWrapper);
        #endregion

        #region Join
        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Join(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Join(expression, sqlWrapper);
        #endregion

        #region Where
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Where(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Where(expression, sqlWrapper);
        #endregion

        #region In
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void In(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).In(expression, sqlWrapper);
        #endregion

        #region GroupBy
        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void GroupBy(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).GroupBy(expression, sqlWrapper);
        #endregion

        #region Having
        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Having(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Having(expression, sqlWrapper);
        #endregion

        #region OrderBy
        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <param name="orders">排序方式</param>
        public static void OrderBy(Expression expression, SqlWrapper sqlWrapper, params OrderType[] orders) =>
            GetExpressionResolver(expression).OrderBy(expression, sqlWrapper, orders);
        #endregion

        #region Max
        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Max(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Max(expression, sqlWrapper);
        #endregion

        #region Min
        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Min(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Min(expression, sqlWrapper);
        #endregion

        #region Avg
        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Avg(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Avg(expression, sqlWrapper);
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Count(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Count(expression, sqlWrapper);
        #endregion

        #region Sum
        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Sum(Expression expression, SqlWrapper sqlWrapper) =>
            GetExpressionResolver(expression).Sum(expression, sqlWrapper);
        #endregion
    }
}