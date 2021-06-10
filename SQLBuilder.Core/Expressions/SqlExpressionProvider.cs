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
        #region Private Static Methods
        /// <summary>
        /// GetExpressionResolver
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>ISqlBuilder</returns>
        private static IExpression GetExpressionResolver(Expression expression)
        {
            #region Implemented Expression
            if (expression == null)
                throw new ArgumentNullException("Expression", "Expression is null");

            //表示具有常数值的表达式
            else if (expression is ConstantExpression)
                return new ConstantExpressionResolver();

            //表示具有二进制运算符的表达式
            else if (expression is BinaryExpression)
                return new BinaryExpressionResolver();

            //表示访问字段或属性
            else if (expression is MemberExpression)
                return new MemberExpressionResolver();

            //表示对静态方法或实例方法的调用
            else if (expression is MethodCallExpression)
                return new MethodCallExpressionResolver();

            //表示创建一个新数组，并可能初始化该新数组的元素
            else if (expression is NewArrayExpression)
                return new NewArrayExpressionResolver();

            //表示一个构造函数调用
            else if (expression is NewExpression)
                return new NewExpressionResolver();

            //表示具有一元运算符的表达式
            else if (expression is UnaryExpression)
                return new UnaryExpressionResolver();

            //表示调用构造函数并初始化新对象的一个或多个成员
            else if (expression is MemberInitExpression)
                return new MemberInitExpressionResolver();

            //表示包含集合初始值设定项的构造函数调用
            else if (expression is ListInitExpression)
                return new ListInitExpressionResolver();

            //表示将委托或lambda表达式应用于参数表达式列表的表达式
            else if (expression is InvocationExpression)
                return new InvocationExpressionResolver();

            //描述一个lambda表达式
            else if (expression is LambdaExpression)
                return new LambdaExpressionResolver();

            //表示命名参数表达式
            else if (expression is ParameterExpression)
                return new ParameterExpressionResolver();

            //表示条件表达式
            else if (expression is ConditionalExpression)
                return new ConditionalExpressionReslover();
            #endregion

            #region NotImplemented Expression
            else if (expression is BlockExpression)
                throw new NotImplementedException($"NotImplemented {nameof(BlockExpression)}");

            else if (expression is DebugInfoExpression)
                throw new NotImplementedException($"NotImplemented {nameof(DebugInfoExpression)}");

            else if (expression is DefaultExpression)
                throw new NotImplementedException($"NotImplemented {nameof(DefaultExpression)}");

            else if (expression is DynamicExpression)
                throw new NotImplementedException($"NotImplemented {nameof(DynamicExpression)}");

            else if (expression is GotoExpression)
                throw new NotImplementedException($"NotImplemented {nameof(GotoExpression)}");

            else if (expression is IndexExpression)
                throw new NotImplementedException($"NotImplemented {nameof(IndexExpression)}");

            else if (expression is LabelExpression)
                throw new NotImplementedException($"NotImplemented {nameof(LabelExpression)}");

            else if (expression is LoopExpression)
                throw new NotImplementedException($"NotImplemented {nameof(LoopExpression)}");

            else if (expression is RuntimeVariablesExpression)
                throw new NotImplementedException($"NotImplemented {nameof(LoopExpression)}");

            else if (expression is SwitchExpression)
                throw new NotImplementedException($"NotImplemented {nameof(SwitchExpression)}");

            else if (expression is TryExpression)
                throw new NotImplementedException($"NotImplemented {nameof(TryExpression)}");

            else if (expression is TypeBinaryExpression)
                throw new NotImplementedException($"NotImplemented {nameof(TypeBinaryExpression)}");

            else
                throw new NotImplementedException($"NotImplemented {nameof(Expression)}");
            #endregion
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Update(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Update(expression, sqlWrapper);

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Insert(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Insert(expression, sqlWrapper);

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Select(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Select(expression, sqlWrapper);

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Join(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Join(expression, sqlWrapper);

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Where(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Where(expression, sqlWrapper);

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void In(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).In(expression, sqlWrapper);

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void GroupBy(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).GroupBy(expression, sqlWrapper);

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Having(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Having(expression, sqlWrapper);

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <param name="orders">排序方式</param>
        public static void OrderBy(Expression expression, SqlWrapper sqlWrapper, params OrderType[] orders) => GetExpressionResolver(expression).OrderBy(expression, sqlWrapper, orders);

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Max(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Max(expression, sqlWrapper);

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Min(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Min(expression, sqlWrapper);

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Avg(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Avg(expression, sqlWrapper);

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Count(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Count(expression, sqlWrapper);

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        public static void Sum(Expression expression, SqlWrapper sqlWrapper) => GetExpressionResolver(expression).Sum(expression, sqlWrapper);
        #endregion
    }
}