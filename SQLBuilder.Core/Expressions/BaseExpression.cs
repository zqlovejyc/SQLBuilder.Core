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
    /// Expression抽象基类
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    public abstract class BaseExpression<T> : IExpression where T : Expression
    {
        #region Virtural Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Update(T expression, SqlWrapper sqlWrapper) =>
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Update");

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Insert(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Insert");

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Select(T expression, SqlWrapper sqlWrapper) =>
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Select");

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Join(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Join");

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Where(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Where");

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper In(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression In");

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper GroupBy(T expression, SqlWrapper sqlWrapper) =>
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression GroupBy");

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Having(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Having");

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper OrderBy(T expression, SqlWrapper sqlWrapper, params OrderType[] orders) =>
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression OrderBy");

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Max(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Max");

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Min(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Min");

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Avg(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Avg");

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Count(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Count");

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public virtual SqlWrapper Sum(T expression, SqlWrapper sqlWrapper) => 
            throw new NotImplementedException("NotImplemented " + typeof(T).Name + "IExpression Sum");
        #endregion

        #region Implemented Methods
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Update(Expression expression, SqlWrapper sqlWrapper) => 
            this.Update((T)expression, sqlWrapper);

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Insert(Expression expression, SqlWrapper sqlWrapper) => 
            this.Insert((T)expression, sqlWrapper);

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Select(Expression expression, SqlWrapper sqlWrapper) =>
            this.Select((T)expression, sqlWrapper);

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Join(Expression expression, SqlWrapper sqlWrapper) => 
            this.Join((T)expression, sqlWrapper);

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Where(Expression expression, SqlWrapper sqlWrapper) =>
            this.Where((T)expression, sqlWrapper);

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper In(Expression expression, SqlWrapper sqlWrapper) => 
            this.In((T)expression, sqlWrapper);

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper GroupBy(Expression expression, SqlWrapper sqlWrapper) =>
            this.GroupBy((T)expression, sqlWrapper);

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Having(Expression expression, SqlWrapper sqlWrapper) =>
            this.Having((T)expression, sqlWrapper);

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper OrderBy(Expression expression, SqlWrapper sqlWrapper, params OrderType[] orders) =>
            this.OrderBy((T)expression, sqlWrapper, orders);

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Max(Expression expression, SqlWrapper sqlWrapper) =>
            this.Max((T)expression, sqlWrapper);

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Min(Expression expression, SqlWrapper sqlWrapper) => 
            this.Min((T)expression, sqlWrapper);

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Avg(Expression expression, SqlWrapper sqlWrapper) => 
            this.Avg((T)expression, sqlWrapper);

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Count(Expression expression, SqlWrapper sqlWrapper) => 
            this.Count((T)expression, sqlWrapper);

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql包装器</param>
        /// <returns>SqlWrapper</returns>
        public SqlWrapper Sum(Expression expression, SqlWrapper sqlWrapper) => 
            this.Sum((T)expression, sqlWrapper);
        #endregion
    }
}