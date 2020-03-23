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
    /// 表示具有常数值的表达式
    /// </summary>
	public class ConstantExpressionResolve : BaseSqlBuilder<ConstantExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(ConstantExpression expression, SqlPack sqlPack)
        {
            //表达式左侧为bool类型常量
            if (expression.NodeType == ExpressionType.Constant && expression.Value is bool b)
            {
                var sql = sqlPack.ToString().ToUpper().Trim();
                if (!b && (sql.EndsWith("WHERE") || sql.EndsWith("AND") || sql.EndsWith("OR")))
                {
                    sqlPack += " 1 = 0 ";
                }
            }
            else
            {
                sqlPack.AddDbParameter(expression.Value);
            }
            return sqlPack;
        }

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack In(ConstantExpression expression, SqlPack sqlPack)
        {
            sqlPack.AddDbParameter(expression.Value);
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack GroupBy(ConstantExpression expression, SqlPack sqlPack)
        {
            var tableName = sqlPack.GetTableName(sqlPack.DefaultType);
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            sqlPack += tableAlias + sqlPack.GetColumnName(expression.Value.ToString()) + ",";
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
		public override SqlPack OrderBy(ConstantExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            var tableName = sqlPack.GetTableName(sqlPack.DefaultType);
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            var field = expression.Value.ToString();
            if (!field.ToUpper().Contains(" ASC") && !field.ToUpper().Contains(" DESC"))
                field = sqlPack.GetColumnName(field);
            sqlPack += tableAlias + field;
            if (orders?.Length > 0)
                sqlPack += $" { (orders[0] == OrderType.Descending ? "DESC" : "ASC")}";
            return sqlPack;
        }
        #endregion
    }
}