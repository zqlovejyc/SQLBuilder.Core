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
using SQLBuilder.Core.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示访问字段或属性
    /// </summary>
	public class MemberExpressionResolver : BaseExpression<MemberExpression>
    {
        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Insert(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var objectArray = new List<object>();
            var fields = new List<string>();
            var convertRes = expression.ToObject();
            if (convertRes.IsNull())
                return sqlWrapper;

            if (!convertRes.GetType().IsDictionaryType() &&
                convertRes is IEnumerable collection)
                foreach (var item in collection)
                {
                    objectArray.Add(item);
                }
            else
                objectArray.Add(convertRes);

            for (var i = 0; i < objectArray.Count; i++)
            {
                if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                    sqlWrapper.Append("(");

                if (i > 0 && sqlWrapper.DatabaseType == DatabaseType.Oracle)
                    sqlWrapper.Append(" UNION ALL SELECT ");

                var objectType = objectArray[i]?.GetType();
                var isDictionaryType = objectType.IsDictionaryType();

                PropertyInfo[] properties;
                IDictionary<string, object> objectDic = null;

                if (!isDictionaryType)
                    properties = objectType?.GetProperties();
                else
                {
                    properties = sqlWrapper.DefaultType.GetProperties();
                    objectDic = objectArray[i] as IDictionary<string, object>;
                }

                foreach (var property in properties)
                {
                    if (isDictionaryType && !objectDic.Any(x => x.Key.EqualIgnoreCase(property.Name)))
                        continue;

                    var type = property.DeclaringType.IsAnonymousType() || isDictionaryType ?
                        sqlWrapper.DefaultType :
                        property.DeclaringType;

                    var (columnName, isInsert, isUpdate, dbType) = sqlWrapper.GetColumnInfo(type, property);
                    if (isInsert)
                    {
                        object value;

                        if (isDictionaryType)
                            value = objectDic.FirstOrDefault(x => x.Key.EqualIgnoreCase(property.Name)).Value;
                        else
                            value = property.GetValue(objectArray[i], null);

                        if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                        {
                            sqlWrapper.AddDbParameter(value, dbType);

                            if (!fields.Contains(columnName))
                                fields.Add(columnName);

                            sqlWrapper += ",";
                        }
                    }
                }

                if (sqlWrapper[^1] == ',')
                {
                    sqlWrapper.Remove(sqlWrapper.Length - 1, 1);
                    if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                        sqlWrapper.Append("),");
                    else
                        sqlWrapper.Append(" FROM DUAL");
                }
            }

            sqlWrapper.RemoveLast(',');

            sqlWrapper.Reset(string.Format(sqlWrapper.ToString(), string.Join(",", fields).TrimEnd(',')));

            return sqlWrapper;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Update(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var convertRes = expression.ToObject();
            if (convertRes.IsNull())
                return sqlWrapper;

            var convertType = convertRes?.GetType();
            var isDictionaryType = convertType.IsDictionaryType();

            PropertyInfo[] properties;
            IDictionary<string, object> convertDic = null;

            if (!isDictionaryType)
                properties = convertType?.GetProperties();
            else
            {
                properties = sqlWrapper.DefaultType.GetProperties();
                convertDic = convertRes as IDictionary<string, object>;
            }

            if (properties.IsNotNullOrEmpty())
            {
                foreach (var item in properties)
                {
                    if (isDictionaryType && !convertDic.Any(x => x.Key.EqualIgnoreCase(item.Name)))
                        continue;

                    var type = item.DeclaringType.IsAnonymousType() || isDictionaryType ?
                        sqlWrapper.DefaultType :
                        item.DeclaringType;

                    var (columnName, isInsert, isUpdate, dbType) = sqlWrapper.GetColumnInfo(type, item);
                    if (isUpdate)
                    {
                        object value;

                        if (isDictionaryType)
                            value = convertDic.FirstOrDefault(x => x.Key.EqualIgnoreCase(item.Name)).Value;
                        else
                            value = item.GetValue(convertRes, null);

                        if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                        {
                            sqlWrapper += columnName + " = ";
                            sqlWrapper.AddDbParameter(value, dbType);
                            sqlWrapper += ",";
                        }
                    }
                }
            }

            sqlWrapper.RemoveLast(',');

            return sqlWrapper;
        }
        #endregion

        #region Select
        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Select(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Expression.NodeType != ExpressionType.Constant)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;

                var tableName = sqlWrapper.GetTableName(type);
                var parameter = expression.Expression as ParameterExpression;
                var tableAlias = sqlWrapper.GetTableAlias(tableName, parameter?.Name);

                if (tableAlias.IsNotNullOrEmpty())
                    tableAlias += ".";

                sqlWrapper.AddField(tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName);
            }
            else
            {
                var field = expression.ToObject()?.ToString();
                sqlWrapper.AddField(field);
            }

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
        public override SqlWrapper Join(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;

            var tableName = sqlWrapper.GetTableName(type);
            var parameter = expression.Expression as ParameterExpression;
            var tableAlias = sqlWrapper.GetTableAlias(tableName, parameter?.Name);

            if (tableAlias.IsNotNullOrEmpty())
                tableAlias += ".";

            sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;

            return sqlWrapper;
        }
        #endregion

        #region Max
        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Max(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression?.Member != null)
            {
                var columnName = sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                sqlWrapper.AddField(columnName);
            }

            return sqlWrapper;
        }
        #endregion

        #region Min
        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Min(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression?.Member != null)
            {
                var columnName = sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                sqlWrapper.AddField(columnName);
            }

            return sqlWrapper;
        }
        #endregion

        #region Avg
        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Avg(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression?.Member != null)
            {
                var columnName = sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                sqlWrapper.AddField(columnName);
            }

            return sqlWrapper;
        }
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Count(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression?.Member != null)
            {
                var columnName = sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                sqlWrapper.AddField(columnName);
            }

            return sqlWrapper;
        }
        #endregion

        #region Sum
        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Sum(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression?.Member != null)
            {
                var columnName = sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                sqlWrapper.AddField(columnName);
            }

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
        public override SqlWrapper Where(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            //此处判断expression的Member是否是可空值类型
            if (expression.Expression?.NodeType == ExpressionType.MemberAccess && expression.Member.DeclaringType.IsNullable())
                expression = expression.Expression as MemberExpression;

            if (expression != null)
            {
                if (expression.Expression?.NodeType == ExpressionType.Parameter)
                {
                    var type = expression.Expression.Type != expression.Member.DeclaringType ?
                               expression.Expression.Type :
                               expression.Member.DeclaringType;

                    var tableName = sqlWrapper.GetTableName(type);
                    var tableAlias = (expression.Expression as ParameterExpression)?.Name;
                    tableAlias = sqlWrapper.GetTableAlias(tableName, tableAlias);

                    if (tableAlias.IsNotNullOrEmpty())
                        tableAlias += ".";

                    sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;

                    //字段是bool类型
                    if (expression.NodeType == ExpressionType.MemberAccess && expression.Type.GetCoreType() == typeof(bool))
                        sqlWrapper += " = 1";
                }
                else
                    sqlWrapper.AddDbParameter(expression.ToObject());
            }

            return sqlWrapper;
        }
        #endregion

        #region In
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper In(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var convertRes = expression.ToObject();
            if (convertRes is IEnumerable collection)
            {
                sqlWrapper += "(";
                foreach (var item in collection)
                {
                    SqlExpressionProvider.In(Expression.Constant(item), sqlWrapper);
                    sqlWrapper += ",";
                }

                sqlWrapper.RemoveLast(',');

                sqlWrapper += ")";
            }

            return sqlWrapper;
        }
        #endregion

        #region GroupBy
        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper GroupBy(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var tableAlias = string.Empty;
            var tableName = string.Empty;

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                tableName = sqlWrapper.GetTableName(type);
                tableAlias = (expression.Expression as ParameterExpression)?.Name;
            }

            if (expression.Expression.NodeType == ExpressionType.Constant)
                tableName = sqlWrapper.GetTableName(sqlWrapper.DefaultType);

            tableAlias = sqlWrapper.GetTableAlias(tableName, tableAlias);

            if (tableAlias.IsNotNullOrEmpty())
                tableAlias += ".";

            if (expression.Expression.NodeType == ExpressionType.Parameter)
                sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;

            if (expression.Expression.NodeType == ExpressionType.Constant ||
                expression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                var convertRes = expression.ToObject();
                if (convertRes != null)
                {
                    if (convertRes is IList collection)
                    {
                        foreach (var item in collection)
                        {
                            SqlExpressionProvider.GroupBy(Expression.Constant(item), sqlWrapper);

                            sqlWrapper += ",";
                        }

                        sqlWrapper.RemoveLast(',');
                    }

                    if (typeof(string) == convertRes.GetType() && convertRes is string str)
                        SqlExpressionProvider.GroupBy(Expression.Constant(str), sqlWrapper);
                }
            }

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
        public override SqlWrapper Having(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            //此处判断expression的Member是否是可空值类型
            if (expression.Expression?.NodeType == ExpressionType.MemberAccess && expression.Member.DeclaringType.IsNullable())
                expression = expression.Expression as MemberExpression;

            if (expression != null)
            {
                if (expression.Expression?.NodeType == ExpressionType.Parameter)
                {
                    var type = expression.Expression.Type != expression.Member.DeclaringType ?
                               expression.Expression.Type :
                               expression.Member.DeclaringType;

                    var tableName = sqlWrapper.GetTableName(type);
                    var tableAlias = (expression.Expression as ParameterExpression)?.Name;
                    tableAlias = sqlWrapper.GetTableAlias(tableName, tableAlias);

                    if (tableAlias.IsNotNullOrEmpty())
                        tableAlias += ".";

                    sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;

                    //字段是bool类型
                    if (expression.NodeType == ExpressionType.MemberAccess && expression.Type.GetCoreType() == typeof(bool))
                        sqlWrapper += " = 1";
                }
                else
                    sqlWrapper.AddDbParameter(expression.ToObject());
            }

            return sqlWrapper;
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper OrderBy(MemberExpression expression, SqlWrapper sqlWrapper, params OrderType[] orders)
        {
            var tableAlias = string.Empty;
            var tableName = string.Empty;

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                tableName = sqlWrapper.GetTableName(type);
            }

            if (expression.Expression.NodeType == ExpressionType.Constant)
                tableName = sqlWrapper.GetTableName(sqlWrapper.DefaultType);

            tableAlias = sqlWrapper.GetTableAlias(tableName, tableAlias);

            if (tableAlias.IsNotNullOrEmpty())
                tableAlias += ".";

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                if (orders?.Length > 0)
                    sqlWrapper += $" { (orders[0] == OrderType.Descending ? "DESC" : "ASC")}";
            }

            if (expression.Expression.NodeType == ExpressionType.Constant ||
                expression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                var convertRes = expression.ToObject();
                if (convertRes != null)
                {
                    if (convertRes is IList collection)
                    {
                        var i = 0;

                        foreach (var item in collection)
                        {
                            SqlExpressionProvider.OrderBy(Expression.Constant(item), sqlWrapper);

                            if (i <= orders.Length - 1)
                                sqlWrapper += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                            else if (item.ToString().ContainsIgnoreCase("ASC", "DESC") == false)
                                sqlWrapper += " ASC,";
                            else
                                sqlWrapper += ",";

                            i++;
                        }

                        sqlWrapper.RemoveLast(',');
                    }

                    if (typeof(string) == convertRes.GetType() && convertRes is string str)
                    {
                        SqlExpressionProvider.OrderBy(Expression.Constant(str), sqlWrapper);

                        if (!str.ContainsIgnoreCase("ASC", "DESC"))
                        {
                            if (orders.Length >= 1)
                                sqlWrapper += $" { (orders[0] == OrderType.Descending ? "DESC" : "ASC")},";
                            else
                                sqlWrapper += " ASC,";

                            sqlWrapper.RemoveLast(',');
                        }
                    }
                }
            }

            return sqlWrapper;
        }
        #endregion
    }
}