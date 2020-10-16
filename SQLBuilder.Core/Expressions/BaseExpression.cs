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

using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using System;
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 抽象基类
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
	public abstract class BaseExpression<T> : IExpression where T : Expression
    {
        #region Public Virtural Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Update(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Update方法");

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Insert(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Insert方法");

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Select(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Select方法");

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Join(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Join方法");

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Where(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Where方法");

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper In(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.In方法");

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper GroupBy(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.GroupBy方法");

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper OrderBy(T expression, SqlWrapper sqlWrapper, params OrderType[] orders) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.OrderBy方法");

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Max(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Max方法");

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Min(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Min方法");

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Avg(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Avg方法");

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Count(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Count方法");

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Sum(T expression, SqlWrapper sqlWrapper) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Sum方法");
        #endregion

        #region Implementation ISqlBuilder Interface
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Update(Expression expression, SqlWrapper sqlWrapper) => Update((T)expression, sqlWrapper);

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Insert(Expression expression, SqlWrapper sqlWrapper) => Insert((T)expression, sqlWrapper);

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Select(Expression expression, SqlWrapper sqlWrapper) => Select((T)expression, sqlWrapper);

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Join(Expression expression, SqlWrapper sqlWrapper) => Join((T)expression, sqlWrapper);

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Where(Expression expression, SqlWrapper sqlWrapper) => Where((T)expression, sqlWrapper);

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper In(Expression expression, SqlWrapper sqlWrapper) => In((T)expression, sqlWrapper);

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper GroupBy(Expression expression, SqlWrapper sqlWrapper) => GroupBy((T)expression, sqlWrapper);

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper OrderBy(Expression expression, SqlWrapper sqlWrapper, params OrderType[] orders) => OrderBy((T)expression, sqlWrapper, orders);

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Max(Expression expression, SqlWrapper sqlWrapper) => Max((T)expression, sqlWrapper);

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Min(Expression expression, SqlWrapper sqlWrapper) => Min((T)expression, sqlWrapper);

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Avg(Expression expression, SqlWrapper sqlWrapper) => Avg((T)expression, sqlWrapper);

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Count(Expression expression, SqlWrapper sqlWrapper) => Count((T)expression, sqlWrapper);

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Sum(Expression expression, SqlWrapper sqlWrapper) => Sum((T)expression, sqlWrapper);
        #endregion
    }
}
