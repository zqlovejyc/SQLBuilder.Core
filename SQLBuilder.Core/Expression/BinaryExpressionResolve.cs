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
    /// 表示具有二进制运算符的表达式
    /// </summary>
	public class BinaryExpressionResolve : BaseSqlBuilder<BinaryExpression>
    {
        #region Private Methods
        /// <summary>
        /// OperatorParser
        /// </summary>
        /// <param name="expressionNodeType">表达式树节点类型</param>
        /// <param name="operatorIndex">操作符索引</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="useIs">是否使用is</param>
        private void OperatorParser(ExpressionType expressionNodeType, int operatorIndex, SqlPack sqlPack, bool useIs = false)
        {
            switch (expressionNodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sqlPack.Sql.Insert(operatorIndex, " AND ");
                    break;
                case ExpressionType.Equal:
                    if (useIs)
                    {
                        sqlPack.Sql.Insert(operatorIndex, " IS ");
                    }
                    else
                    {
                        sqlPack.Sql.Insert(operatorIndex, " = ");
                    }
                    break;
                case ExpressionType.GreaterThan:
                    sqlPack.Sql.Insert(operatorIndex, " > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sqlPack.Sql.Insert(operatorIndex, " >= ");
                    break;
                case ExpressionType.NotEqual:
                    if (useIs)
                    {
                        sqlPack.Sql.Insert(operatorIndex, " IS NOT ");
                    }
                    else
                    {
                        sqlPack.Sql.Insert(operatorIndex, " <> ");
                    }
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sqlPack.Sql.Insert(operatorIndex, " OR ");
                    break;
                case ExpressionType.LessThan:
                    sqlPack.Sql.Insert(operatorIndex, " < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sqlPack.Sql.Insert(operatorIndex, " <= ");
                    break;
                case ExpressionType.Add:
                    sqlPack.Sql.Insert(operatorIndex, " + ");
                    break;
                case ExpressionType.Subtract:
                    sqlPack.Sql.Insert(operatorIndex, " - ");
                    break;
                case ExpressionType.Multiply:
                    sqlPack.Sql.Insert(operatorIndex, " * ");
                    break;
                case ExpressionType.Divide:
                    sqlPack.Sql.Insert(operatorIndex, " / ");
                    break;
                case ExpressionType.Modulo:
                    sqlPack.Sql.Insert(operatorIndex, " % ");
                    break;
                default:
                    throw new NotImplementedException("未实现的节点类型" + expressionNodeType);
            }
        }
        #endregion

        #region Override Base Class Methods
        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Join(BinaryExpression expression, SqlPack sqlPack)
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
                sqlPack += "(";
            }
            SqlBuilderProvider.Join(expression.Left, sqlPack);
            if (leftNested)
            {
                sqlPack += ")";
            }

            var operatorIndex = sqlPack.Sql.Length;

            //右侧嵌套
            var rightBinary = expression.Right as BinaryExpression;
            isBinaryLeft = rightBinary?.Left is BinaryExpression;
            isBoolMethodCallLeft = (rightBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            isBinaryRight = rightBinary?.Right is BinaryExpression;
            isBoolMethodCallRight = (rightBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var rightNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (rightNested)
            {
                sqlPack += "(";
            }
            SqlBuilderProvider.Where(expression.Right, sqlPack);
            if (rightNested)
            {
                sqlPack += ")";
            }

            var sqlLength = sqlPack.Sql.Length;
            if (sqlLength - operatorIndex == 5 && sqlPack.ToString().ToUpper().EndsWith("NULL"))
            {
                OperatorParser(expression.NodeType, operatorIndex, sqlPack, true);
            }
            else
            {
                OperatorParser(expression.NodeType, operatorIndex, sqlPack);
            }
            return sqlPack;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Where(BinaryExpression expression, SqlPack sqlPack)
        {
            var startIndex = sqlPack.Length;

            //左侧嵌套
            var leftBinary = expression.Left as BinaryExpression;
            var isBinaryLeft = leftBinary?.Left is BinaryExpression;
            var isBoolMethodCallLeft = (leftBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var isBinaryRight = leftBinary?.Right is BinaryExpression;
            var isBoolMethodCallRight = (leftBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var leftNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (leftNested)
            {
                sqlPack += "(";
            }
            SqlBuilderProvider.Where(expression.Left, sqlPack);
            if (leftNested)
            {
                sqlPack += ")";
            }

            var signIndex = sqlPack.Length;

            //右侧嵌套
            var rightBinary = expression.Right as BinaryExpression;
            isBinaryLeft = rightBinary?.Left is BinaryExpression;
            isBoolMethodCallLeft = (rightBinary?.Left as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            isBinaryRight = rightBinary?.Right is BinaryExpression;
            isBoolMethodCallRight = (rightBinary?.Right as MethodCallExpression)?.Method.ReturnType == typeof(bool);
            var rightNested = (isBinaryLeft || isBoolMethodCallLeft) && (isBinaryRight || isBoolMethodCallRight);
            if (rightNested)
            {
                sqlPack += "(";
            }
            SqlBuilderProvider.Where(expression.Right, sqlPack);
            if (rightNested)
            {
                sqlPack += ")";
            }

            //表达式左侧为bool类型常量且为true时，不进行Sql拼接
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

                        //NOT IN、IN
                        if (subString.Contains("NOT IN"))
                        {
                            var index = sqlPack.ToString().LastIndexOf("NOT IN");
                            if (index != -1) sqlPack.Sql.Replace("NOT IN", "IN", index, 6);
                        }
                        if (subString.Contains("IN") && subString.LastIndexOf("IN") != (subString.LastIndexOf("NOT IN") + 4))
                        {
                            var index = sqlPack.ToString().LastIndexOf("IN");
                            if (index != -1) sqlPack.Sql.Replace("IN", "NOT IN", index, 2);
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
                    }
                }
                else
                {
                    if (sqlPack.ToString().ToUpper().EndsWith("NULL"))
                        OperatorParser(expression.NodeType, signIndex, sqlPack, true);
                    else
                        OperatorParser(expression.NodeType, signIndex, sqlPack);
                }
            }
            return sqlPack;
        }
        #endregion
    }
}