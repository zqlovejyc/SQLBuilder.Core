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
    /// 抽象基类
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
	public abstract class BaseSqlBuilder<T> : ISqlBuilder where T : Expression
    {
        #region Public Virtural Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Update(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Update方法");

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Insert(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Insert方法");

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Select(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Select方法");

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Join(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Join方法");

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Where(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Where方法");

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack In(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.In方法");

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack GroupBy(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.GroupBy方法");

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack OrderBy(T expression, SqlPack sqlPack, params OrderType[] orders) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.OrderBy方法");

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Max(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Max方法");

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Min(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Min方法");

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Avg(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Avg方法");

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Count(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Count方法");

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public virtual SqlPack Sum(T expression, SqlPack sqlPack) => throw new NotImplementedException("未实现" + typeof(T).Name + "ISqlBuilder.Sum方法");
        #endregion

        #region Implementation ISqlBuilder Interface
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Update(Expression expression, SqlPack sqlPack) => Update((T)expression, sqlPack);

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Insert(Expression expression, SqlPack sqlPack) => Insert((T)expression, sqlPack);

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Select(Expression expression, SqlPack sqlPack) => Select((T)expression, sqlPack);

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Join(Expression expression, SqlPack sqlPack) => Join((T)expression, sqlPack);

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Where(Expression expression, SqlPack sqlPack) => Where((T)expression, sqlPack);

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack In(Expression expression, SqlPack sqlPack) => In((T)expression, sqlPack);

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack GroupBy(Expression expression, SqlPack sqlPack) => GroupBy((T)expression, sqlPack);

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public SqlPack OrderBy(Expression expression, SqlPack sqlPack, params OrderType[] orders) => OrderBy((T)expression, sqlPack, orders);

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Max(Expression expression, SqlPack sqlPack) => Max((T)expression, sqlPack);

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Min(Expression expression, SqlPack sqlPack) => Min((T)expression, sqlPack);

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Avg(Expression expression, SqlPack sqlPack) => Avg((T)expression, sqlPack);

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Count(Expression expression, SqlPack sqlPack) => Count((T)expression, sqlPack);

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public SqlPack Sum(Expression expression, SqlPack sqlPack) => Sum((T)expression, sqlPack);
        #endregion
    }
}
