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
using SQLBuilder.Core.Extensions;
using System;
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示具有二进制运算符的表达式
    /// </summary>
	public class BinaryExpressionResolve : BaseExpression<BinaryExpression>
    {
        #region Private Methods
        /// <summary>
        /// OperatorParser
        /// </summary>
        /// <param name="expressionNodeType">表达式树节点类型</param>
        /// <param name="operatorIndex">操作符索引</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="useIs">是否使用is</param>
        private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, SqlWrapper sqlWrapper, bool useIs = false)
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

        #region Override Base Class Methods
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Select(BinaryExpression expression, SqlWrapper sqlWrapper)
        {
            var field = expression?.ToObject();
            if (field != null)
                SqlExpressionProvider.Select(Expression.Constant(field), sqlWrapper);

            return sqlWrapper;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Join(BinaryExpression expression, SqlWrapper sqlWrapper)
        {
            //左侧嵌套
            var leftBinary = expression.Left as BinaryExpression;
            var isBinaryLeft = leftBinary?.Left is BinaryExpression;
            var isBoolMethodCallLeft = (leftBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var isBinaryRight = leftBinary?.Right is BinaryExpression;
            var isBoolMethodCallRight = (leftBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var leftNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (leftNested)
            {
                sqlWrapper += "(";
            }
            SqlExpressionProvider.Join(expression.Left, sqlWrapper);
            if (leftNested)
            {
                sqlWrapper += ")";
            }

            var operatorIndex = sqlWrapper.Length;

            //右侧嵌套
            var rightBinary = expression.Right as BinaryExpression;
            isBinaryLeft = rightBinary?.Left is BinaryExpression;
            isBoolMethodCallLeft = (rightBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            isBinaryRight = rightBinary?.Right is BinaryExpression;
            isBoolMethodCallRight = (rightBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var rightNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (rightNested)
            {
                sqlWrapper += "(";
            }
            SqlExpressionProvider.Where(expression.Right, sqlWrapper);
            if (rightNested)
            {
                sqlWrapper += ")";
            }

            //表达式左侧为bool类型常量且为true时，不进行sql拼接
            if (!(expression.Left.NodeType == ExpressionType.Constant && expression.Left.ToObject() is bool b && b))
            {
                var sqlLength = sqlWrapper.Length;
                if (sqlLength - operatorIndex == 5 && sqlWrapper.EndsWith("NULL"))
                    OperatorParser(expression.NodeType, operatorIndex, sqlWrapper, true);
                else
                    OperatorParser(expression.NodeType, operatorIndex, sqlWrapper);
            }

            return sqlWrapper;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Where(BinaryExpression expression, SqlWrapper sqlWrapper)
        {
            var startIndex = sqlWrapper.Length;

            //左侧嵌套
            var leftBinary = expression.Left as BinaryExpression;
            var isBinaryLeft = leftBinary?.Left is BinaryExpression;
            var isBoolMethodCallLeft = (leftBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var isBinaryRight = leftBinary?.Right is BinaryExpression;
            var isBoolMethodCallRight = (leftBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var leftNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (leftNested)
            {
                sqlWrapper += "(";
            }
            SqlExpressionProvider.Where(expression.Left, sqlWrapper);
            if (leftNested)
            {
                sqlWrapper += ")";
            }

            var signIndex = sqlWrapper.Length;

            //右侧嵌套
            var rightBinary = expression.Right as BinaryExpression;
            isBinaryLeft = rightBinary?.Left is BinaryExpression;
            isBoolMethodCallLeft = (rightBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            isBinaryRight = rightBinary?.Right is BinaryExpression;
            isBoolMethodCallRight = (rightBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var rightNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (rightNested)
            {
                sqlWrapper += "(";
            }
            SqlExpressionProvider.Where(expression.Right, sqlWrapper);
            if (rightNested)
            {
                sqlWrapper += ")";
            }

            //表达式左侧为bool类型常量且为true时，不进行sql拼接
            if (!(expression.Left.NodeType == ExpressionType.Constant && expression.Left.ToObject() is bool b && b))
            {
                //若表达式右侧为bool类型，且为false时，条件取非
                if ((expression.Right.NodeType == ExpressionType.Constant
                    || (expression.Right.NodeType == ExpressionType.Convert
                    && expression.Right is UnaryExpression unary
                    && unary.Operand.NodeType == ExpressionType.Constant))
                    && expression.Right.ToObject() is bool r)
                {
                    if (!r)
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
                    if (sqlWrapper.EndsWith("NULL"))
                        OperatorParser(expression.NodeType, signIndex, sqlWrapper, true);
                    else
                        OperatorParser(expression.NodeType, signIndex, sqlWrapper);
                }
            }

            return sqlWrapper;
        }
        #endregion
    }
}