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
using System;
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示具有二进制运算符的表达式
    /// </summary>
	public class BinaryExpressionResolver : BaseExpression<BinaryExpression>
    {
        #region Select
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Select(BinaryExpression expression, SqlWrapper sqlWrapper)
        {
            var field = expression?.ToObject();
            if (field != null)
                SqlExpressionProvider.Select(Expression.Constant(field), sqlWrapper);

            return sqlWrapper;
        }
        #endregion

        #region Join
        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Join(BinaryExpression expression, SqlWrapper sqlWrapper)
        {
            var startIndex = sqlWrapper.Length;

            //嵌套解析
            var operatorIndex = ExpressionNestedResolver(expression, sqlWrapper, nameof(Join));

            //取非解析
            ExpressionNotResolver(expression, sqlWrapper, startIndex, operatorIndex);

            return sqlWrapper;
        }
        #endregion

        #region Where
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Where(BinaryExpression expression, SqlWrapper sqlWrapper)
        {
            var startIndex = sqlWrapper.Length;

            //嵌套解析
            var operatorIndex = ExpressionNestedResolver(expression, sqlWrapper, nameof(Where));

            //取非解析
            ExpressionNotResolver(expression, sqlWrapper, startIndex, operatorIndex);

            return sqlWrapper;
        }
        #endregion

        #region Having
        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Having(BinaryExpression expression, SqlWrapper sqlWrapper)
        {
            var startIndex = sqlWrapper.Length;

            //嵌套解析
            var operatorIndex = ExpressionNestedResolver(expression, sqlWrapper, nameof(Having));

            //取非解析
            ExpressionNotResolver(expression, sqlWrapper, startIndex, operatorIndex);

            return sqlWrapper;
        }
        #endregion

        #region OperatorResolver
        /// <summary>
        /// Expression操作符解析
        /// </summary>
        /// <param name="expressionNodeType">表达式树节点类型</param>
        /// <param name="operatorIndex">操作符索引</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <param name="useIs">是否使用is</param>
        public static void OperatorResolver(ExpressionType expressionNodeType, int operatorIndex, SqlWrapper sqlWrapper, bool useIs = false)
        {
            switch (expressionNodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sqlWrapper.Insert(operatorIndex, " AND ");
                    break;
                case ExpressionType.Equal:
                    if (useIs)
                        sqlWrapper.Insert(operatorIndex, " IS ");
                    else
                        sqlWrapper.Insert(operatorIndex, " = ");
                    break;
                case ExpressionType.GreaterThan:
                    sqlWrapper.Insert(operatorIndex, " > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sqlWrapper.Insert(operatorIndex, " >= ");
                    break;
                case ExpressionType.NotEqual:
                    if (useIs)
                        sqlWrapper.Insert(operatorIndex, " IS NOT ");
                    else
                        sqlWrapper.Insert(operatorIndex, " <> ");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sqlWrapper.Insert(operatorIndex, " OR ");
                    break;
                case ExpressionType.LessThan:
                    sqlWrapper.Insert(operatorIndex, " < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sqlWrapper.Insert(operatorIndex, " <= ");
                    break;
                case ExpressionType.Add:
                    sqlWrapper.Insert(operatorIndex, " + ");
                    break;
                case ExpressionType.Subtract:
                    sqlWrapper.Insert(operatorIndex, " - ");
                    break;
                case ExpressionType.Multiply:
                    sqlWrapper.Insert(operatorIndex, " * ");
                    break;
                case ExpressionType.Divide:
                    sqlWrapper.Insert(operatorIndex, " / ");
                    break;
                case ExpressionType.Modulo:
                    sqlWrapper.Insert(operatorIndex, " % ");
                    break;
                default:
                    throw new NotImplementedException("NotImplemented ExpressionType " + expressionNodeType);
            }
        }
        #endregion

        #region ExpressionNestedResolver
        /// <summary>
        /// Expression嵌套解析
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <param name="method">方法名，可选值：Join、Having、Where</param>
        public static int ExpressionNestedResolver(BinaryExpression expression, SqlWrapper sqlWrapper, string method)
        {
            //左侧嵌套
            var lExpr = expression.Left as BinaryExpression;

            var llIsBinaryExpr = lExpr?.Left is BinaryExpression;
            var llIsBoolMethod = typeof(bool) == (lExpr?.Left as MethodCallExpression)?.Method.ReturnType;

            var lrIsBinaryExpr = lExpr?.Right is BinaryExpression;
            var lrIsBoolMethod = typeof(bool) == (lExpr?.Right as MethodCallExpression)?.Method.ReturnType;

            var lNested = (llIsBinaryExpr || llIsBoolMethod) && (lrIsBinaryExpr || lrIsBoolMethod);

            if (lNested)
                sqlWrapper.Append("(");

            if (method.EqualIgnoreCase(nameof(Join)))
                SqlExpressionProvider.Join(expression.Left, sqlWrapper);

            else if (method.EqualIgnoreCase(nameof(Having)))
                SqlExpressionProvider.Having(expression.Left, sqlWrapper);

            else
                SqlExpressionProvider.Where(expression.Left, sqlWrapper);

            if (lNested)
                sqlWrapper.Append(")");

            var operatorIndex = sqlWrapper.Length;

            //右侧嵌套
            var rExpr = expression.Right as BinaryExpression;

            var rlIsBinaryExpr = rExpr?.Left is BinaryExpression;
            var rlIsBoolMethod = typeof(bool) == (rExpr?.Left as MethodCallExpression)?.Method.ReturnType;

            var rrIsBinaryExpr = rExpr?.Right is BinaryExpression;
            var rrIsBoolMethod = typeof(bool) == (rExpr?.Right as MethodCallExpression)?.Method.ReturnType;

            var rNested = (rlIsBinaryExpr || rlIsBoolMethod) && (rrIsBinaryExpr || rrIsBoolMethod);

            if (rNested)
                sqlWrapper.Append("(");

            if (method.EqualIgnoreCase(nameof(Having)))
                SqlExpressionProvider.Having(expression.Right, sqlWrapper);

            else
                SqlExpressionProvider.Where(expression.Right, sqlWrapper);

            if (rNested)
                sqlWrapper.Append(")");

            return operatorIndex;
        }
        #endregion

        #region ExpressionNotResolver
        /// <summary>
        /// Expression表达式取非解析
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="sqlWrapper"></param>
        /// <param name="startIndex"></param>
        /// <param name="operatorIndex"></param>
        /// <returns></returns>
        public static void ExpressionNotResolver(BinaryExpression expression, SqlWrapper sqlWrapper, int startIndex, int operatorIndex)
        {
            //表达式左侧为bool类型常量且为true时，不进行sql拼接
            if (!(expression.Left.NodeType == ExpressionType.Constant && expression.Left.ToObject() is bool left && left))
            {
                //若表达式右侧为bool类型，且为false时，条件取非
                if ((ExpressionType.Constant == expression.Right.NodeType ||
                    (ExpressionType.Convert == expression.Right.NodeType &&
                    expression.Right is UnaryExpression unary &&
                    ExpressionType.Constant == unary.Operand.NodeType)) &&
                    expression.Right.ToObject() is bool res)
                {
                    if (!res)
                    {
                        var subString = sqlWrapper.Substring(startIndex, sqlWrapper.Length - startIndex);

                        //IS NOT、IS                      
                        if (subString.Contains("IS NOT"))
                        {
                            var index = sqlWrapper.LastIndexOf("IS NOT");
                            if (index != -1)
                                sqlWrapper.Replace("IS NOT", "IS", index, 6);
                        }
                        if (subString.Contains("IS") && subString.LastIndexOf("IS") != subString.LastIndexOf("IS NOT"))
                        {
                            var index = sqlWrapper.LastIndexOf("IS");
                            if (index != -1)
                                sqlWrapper.Replace("IS", "IS NOT", index, 2);
                        }

                        //NOT LIKE、LIKE
                        if (subString.Contains("NOT LIKE"))
                        {
                            var index = sqlWrapper.LastIndexOf("NOT LIKE");
                            if (index != -1)
                                sqlWrapper.Replace("NOT LIKE", "LIKE", index, 8);
                        }
                        if (subString.Contains("LIKE") && subString.LastIndexOf("LIKE") != (subString.LastIndexOf("NOT LIKE") + 4))
                        {
                            var index = sqlWrapper.LastIndexOf("LIKE");
                            if (index != -1)
                                sqlWrapper.Replace("LIKE", "NOT LIKE", index, 4);
                        }

                        //NOT IN、IN
                        if (subString.Contains("NOT IN"))
                        {
                            var index = sqlWrapper.LastIndexOf("NOT IN");
                            if (index != -1)
                                sqlWrapper.Replace("NOT IN", "IN", index, 6);
                        }
                        if (subString.Contains("IN") && subString.LastIndexOf("IN") != (subString.LastIndexOf("NOT IN") + 4))
                        {
                            var index = sqlWrapper.LastIndexOf("IN");
                            if (index != -1)
                                sqlWrapper.Replace("IN", "NOT IN", index, 2);
                        }

                        //AND、OR
                        if (subString.Contains("AND"))
                        {
                            var index = sqlWrapper.LastIndexOf("AND");
                            if (index != -1)
                                sqlWrapper.Replace("AND", "OR", index, 3);
                        }
                        if (subString.Contains("OR"))
                        {
                            var index = sqlWrapper.LastIndexOf("OR");
                            if (index != -1)
                                sqlWrapper.Replace("OR", "AND", index, 2);
                        }

                        //=、<>
                        if (subString.Contains(" = "))
                        {
                            var index = sqlWrapper.LastIndexOf(" = ");
                            if (index != -1)
                                sqlWrapper.Replace(" = ", " <> ", index, 3);
                        }
                        if (subString.Contains("<>"))
                        {
                            var index = sqlWrapper.LastIndexOf("<>");
                            if (index != -1)
                                sqlWrapper.Replace("<>", "=", index, 2);
                        }

                        //>、<
                        if (subString.Contains(" > "))
                        {
                            var index = sqlWrapper.LastIndexOf(" > ");
                            if (index != -1)
                                sqlWrapper.Replace(" > ", " <= ", index, 3);
                        }
                        if (subString.Contains(" < "))
                        {
                            var index = sqlWrapper.LastIndexOf(" < ");
                            if (index != -1)
                                sqlWrapper.Replace(" < ", " >= ", index, 3);
                        }

                        //>=、<=
                        if (subString.Contains(" >= "))
                        {
                            var index = sqlWrapper.LastIndexOf(" >= ");
                            if (index != -1)
                                sqlWrapper.Replace(" >= ", " < ", index, 4);
                        }
                        if (subString.Contains(" <= "))
                        {
                            var index = sqlWrapper.LastIndexOf(" <= ");
                            if (index != -1)
                                sqlWrapper.Replace(" <= ", " > ", index, 4);
                        }
                    }
                }
                else
                {
                    OperatorResolver(
                        expression.NodeType,
                        operatorIndex,
                        sqlWrapper,
                        sqlWrapper.EndsWith("NULL"));
                }
            }
        }
        #endregion
    }
}