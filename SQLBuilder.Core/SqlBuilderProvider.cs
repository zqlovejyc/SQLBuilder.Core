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

using System;
using System.Linq.Expressions;

namespace SQLBuilder.Core
{
    /// <summary>
    /// SqlBuilderProvider
    /// </summary>
	public class SqlBuilderProvider
    {
        #region Private Static Methods
        /// <summary>
        /// GetExpressionResolve
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>ISqlBuilder</returns>
        private static ISqlBuilder GetExpressionResolve(Expression expression)
        {
            #region Implementation Of Expression
            //null
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "不能为null");
            }
            //表示具有常数值的表达式
            else if (expression is ConstantExpression)
            {
                return new ConstantExpressionResolve();
            }
            //表示具有二进制运算符的表达式
            else if (expression is BinaryExpression)
            {
                return new BinaryExpressionResolve();
            }
            //表示访问字段或属性
            else if (expression is MemberExpression)
            {
                return new MemberExpressionResolve();
            }
            //表示对静态方法或实例方法的调用
            else if (expression is MethodCallExpression)
            {
                return new MethodCallExpressionResolve();
            }
            //表示创建一个新数组，并可能初始化该新数组的元素
            else if (expression is NewArrayExpression)
            {
                return new NewArrayExpressionResolve();
            }
            //表示一个构造函数调用
            else if (expression is NewExpression)
            {
                return new NewExpressionResolve();
            }
            //表示具有一元运算符的表达式
            else if (expression is UnaryExpression)
            {
                return new UnaryExpressionResolve();
            }
            //表示调用构造函数并初始化新对象的一个或多个成员
            else if (expression is MemberInitExpression)
            {
                return new MemberInitExpressionResolve();
            }
            //表示包含集合初始值设定项的构造函数调用
            else if (expression is ListInitExpression)
            {
                return new ListInitExpressionResolve();
            }
            //表示将委托或lambda表达式应用于参数表达式列表的表达式
            else if (expression is InvocationExpression)
            {
                return new InvocationExpressionResolve();
            }
            //描述一个lambda表达式
            else if (expression is LambdaExpression)
            {
                return new LambdaExpressionResolve();
            }
            //表示命名参数表达式
            else if (expression is ParameterExpression)
            {
                return new ParameterExpressionResolve();
            }
            #endregion

            #region Unimplementation Of Expression
            else if (expression is BlockExpression)
            {
                throw new NotImplementedException("未实现的BlockExpression2Sql");
            }
            else if (expression is ConditionalExpression)
            {
                throw new NotImplementedException("未实现的ConditionalExpression2Sql");
            }
            else if (expression is DebugInfoExpression)
            {
                throw new NotImplementedException("未实现的DebugInfoExpression2Sql");
            }
            else if (expression is DefaultExpression)
            {
                throw new NotImplementedException("未实现的DefaultExpression2Sql");
            }
            else if (expression is DynamicExpression)
            {
                throw new NotImplementedException("未实现的DynamicExpression2Sql");
            }
            else if (expression is GotoExpression)
            {
                throw new NotImplementedException("未实现的GotoExpression2Sql");
            }
            else if (expression is IndexExpression)
            {
                throw new NotImplementedException("未实现的IndexExpression2Sql");
            }
            else if (expression is LabelExpression)
            {
                throw new NotImplementedException("未实现的LabelExpression2Sql");
            }
            else if (expression is LoopExpression)
            {
                throw new NotImplementedException("未实现的LoopExpression2Sql");
            }
            else if (expression is RuntimeVariablesExpression)
            {
                throw new NotImplementedException("未实现的RuntimeVariablesExpression2Sql");
            }
            else if (expression is SwitchExpression)
            {
                throw new NotImplementedException("未实现的SwitchExpression2Sql");
            }
            else if (expression is TryExpression)
            {
                throw new NotImplementedException("未实现的TryExpression2Sql");
            }
            else if (expression is TypeBinaryExpression)
            {
                throw new NotImplementedException("未实现的TypeBinaryExpression2Sql");
            }
            throw new NotImplementedException("未实现的Expression2Sql");
            #endregion
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Update(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Update(expression, sqlPack);

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Insert(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Insert(expression, sqlPack);

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Select(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Select(expression, sqlPack);

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Join(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Join(expression, sqlPack);

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Where(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Where(expression, sqlPack);

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void In(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).In(expression, sqlPack);

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void GroupBy(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).GroupBy(expression, sqlPack);

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        public static void OrderBy(Expression expression, SqlPack sqlPack, params OrderType[] orders) => GetExpressionResolve(expression).OrderBy(expression, sqlPack, orders);

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Max(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Max(expression, sqlPack);

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Min(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Min(expression, sqlPack);

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Avg(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Avg(expression, sqlPack);

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Count(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Count(expression, sqlPack);

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        public static void Sum(Expression expression, SqlPack sqlPack) => GetExpressionResolve(expression).Sum(expression, sqlPack);
        #endregion
    }
}