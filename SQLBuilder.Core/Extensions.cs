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

using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using SQLBuilder.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SQLBuilder.Core
{
    /// <summary>
    /// 扩展类
    /// </summary>
	public static class Extensions
    {
        #region Like
        /// <summary>
        /// LIKE
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool Like(this object @this, string value) => true;
        #endregion

        #region LikeLeft
        /// <summary>
        /// LIKE '% _ _ _'
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool LikeLeft(this object @this, string value) => true;
        #endregion

        #region LikeRight
        /// <summary>
        /// LIKE '_ _ _ %'
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool LikeRight(this object @this, string value) => true;
        #endregion

        #region NotLike
        /// <summary>
        /// NOT LIKE
        /// </summary>
        /// <param name="this">扩展对象自身</param>
        /// <param name="value">包含的字符串</param>
        /// <returns>bool</returns>
        public static bool NotLike(this object @this, string value) => true;
        #endregion

        #region In
        /// <summary>
        /// IN
        /// </summary>
        /// <typeparam name="T">IN数组里面的数据类型</typeparam>
        /// <param name="this">扩展对象自身</param>
        /// <param name="array">IN数组</param>
        /// <returns>bool</returns>
        public static bool In<T>(this object @this, params T[] array) => true;
        #endregion

        #region NotIn
        /// <summary>
        /// NOT IN
        /// </summary>
        /// <typeparam name="T">NOT IN数组里面的数据类型</typeparam>
        /// <param name="this">扩展对象自身</param>
        /// <param name="array">NOT IN数组</param>
        /// <returns>bool</returns>
        public static bool NotIn<T>(this object @this, params T[] array) => true;
        #endregion

        #region True
        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() => parameter => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> True<T1, T2>() => (p1, p2) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> True<T1, T2, T3>() => (p1, p2, p3) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> True<T1, T2, T3, T4>() => (p1, p2, p3, p4) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> True<T1, T2, T3, T4, T5>() => (p1, p2, p3, p4, p5) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> True<T1, T2, T3, T4, T5, T6>() => (p1, p2, p3, p4, p5, p6) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> True<T1, T2, T3, T4, T5, T6, T7>() => (p1, p2, p3, p4, p5, p6, p7) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> True<T1, T2, T3, T4, T5, T6, T7, T8>() => (p1, p2, p3, p4, p5, p6, p7, p8) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> True<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => (p1, p2, p3, p4, p5, p6, p7, p8, p9) => true;

        /// <summary>
        /// True
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> True<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() => (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) => true;
        #endregion

        #region False
        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>() => parameter => false;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> False<T1, T2>() => (p1, p2) => false;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> False<T1, T2, T3>() => (p1, p2, p3) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> False<T1, T2, T3, T4>() => (p1, p2, p3, p4) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> False<T1, T2, T3, T4, T5>() => (p1, p2, p3, p4, p5) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> False<T1, T2, T3, T4, T5, T6>() => (p1, p2, p3, p4, p5, p6) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> False<T1, T2, T3, T4, T5, T6, T7>() => (p1, p2, p3, p4, p5, p6, p7) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> False<T1, T2, T3, T4, T5, T6, T7, T8>() => (p1, p2, p3, p4, p5, p6, p7, p8) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> False<T1, T2, T3, T4, T5, T6, T7, T8, T9>() => (p1, p2, p3, p4, p5, p6, p7, p8, p9) => true;

        /// <summary>
        /// False
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> False<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>() => (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) => true;
        #endregion

        #region Or
        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> Or<T1, T2>(this Expression<Func<T1, T2, bool>> @this, Expression<Func<T1, T2, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> Or<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> @this, Expression<Func<T1, T2, T3, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> Or<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> @this, Expression<Func<T1, T2, T3, T4, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> Or<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> Or<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> Or<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> Or<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> Or<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> Or<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>(Expression.OrElse(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region And
        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> @this, Expression<Func<T, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> And<T1, T2>(this Expression<Func<T1, T2, bool>> @this, Expression<Func<T1, T2, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> And<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> @this, Expression<Func<T1, T2, T3, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> And<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> @this, Expression<Func<T1, T2, T3, T4, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> And<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> And<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> And<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> And<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> And<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="this"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> And<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> @this, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> other)
        {
            var invokedExpr = Expression.Invoke(other, @this.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>(Expression.AndAlso(@this.Body, invokedExpr), @this.Parameters);
        }
        #endregion

        #region WhereIf
        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereIf<T>(this Expression<Func<T, bool>> @this, bool condition, Expression<Func<T, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> WhereIf<T>(this Expression<Func<T, bool>> @this, bool condition, Expression<Func<T, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> WhereIf<T1, T2>(this Expression<Func<T1, T2, bool>> @this, bool condition, Expression<Func<T1, T2, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, bool>> WhereIf<T1, T2>(this Expression<Func<T1, T2, bool>> @this, bool condition, Expression<Func<T1, T2, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> WhereIf<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> @this, bool condition, Expression<Func<T1, T2, T3, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, bool>> WhereIf<T1, T2, T3>(this Expression<Func<T1, T2, T3, bool>> @this, bool condition, Expression<Func<T1, T2, T3, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> WhereIf<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, bool>> WhereIf<T1, T2, T3, T4>(this Expression<Func<T1, T2, T3, T4, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> WhereIf<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, bool>> WhereIf<T1, T2, T3, T4, T5>(this Expression<Func<T1, T2, T3, T4, T5, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> WhereIf<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, bool>> WhereIf<T1, T2, T3, T4, T5, T6>(this Expression<Func<T1, T2, T3, T4, T5, T6, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> other)
        {
            if (condition)
                @this = @this.And(other);

            return @this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="this"></param>
        /// <param name="condition"></param>
        /// <param name="other"></param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public static Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> WhereIf<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> @this, bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> other, Action callback)
        {
            if (condition)
            {
                @this = @this.And(other);

                callback?.Invoke();
            }

            return @this;
        }
        #endregion

        #region ToLambda
        /// <summary>
        /// ToLambda
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static Expression<T> ToLambda<T>(this Expression @this, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<T>(@this, parameters);
        }
        #endregion

        #region ToObject
        /// <summary>
        /// 转换Expression为object
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object ToObject(this Expression @this)
        {
            object obj = null;
            switch (@this.NodeType)
            {
                case ExpressionType.Constant:
                    obj = (@this as ConstantExpression)?.Value;
                    break;
                case ExpressionType.Convert:
                    obj = (@this as UnaryExpression)?.Operand?.ToObject();
                    break;
                default:
                    var isNullable = @this.Type.IsNullable();
                    switch (@this.Type.GetCoreType().Name.ToLower())
                    {
                        case "string":
                            obj = @this.ToLambda<Func<string>>().Compile().Invoke();
                            break;
                        case "int16":
                            if (isNullable)
                                obj = @this.ToLambda<Func<short?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<short>>().Compile().Invoke();
                            break;
                        case "int32":
                            if (isNullable)
                                obj = @this.ToLambda<Func<int?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<int>>().Compile().Invoke();
                            break;
                        case "int64":
                            if (isNullable)
                                obj = @this.ToLambda<Func<long?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<long>>().Compile().Invoke();
                            break;
                        case "decimal":
                            if (isNullable)
                                obj = @this.ToLambda<Func<decimal?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<decimal>>().Compile().Invoke();
                            break;
                        case "double":
                            if (isNullable)
                                obj = @this.ToLambda<Func<double?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<double>>().Compile().Invoke();
                            break;
                        case "datetime":
                            if (isNullable)
                                obj = @this.ToLambda<Func<DateTime?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<DateTime>>().Compile().Invoke();
                            break;
                        case "boolean":
                            if (isNullable)
                                obj = @this.ToLambda<Func<bool?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<bool>>().Compile().Invoke();
                            break;
                        case "byte":
                            if (isNullable)
                                obj = @this.ToLambda<Func<byte?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<byte>>().Compile().Invoke();
                            break;
                        case "char":
                            if (isNullable)
                                obj = @this.ToLambda<Func<char?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<char>>().Compile().Invoke();
                            break;
                        case "single":
                            if (isNullable)
                                obj = @this.ToLambda<Func<float?>>().Compile().Invoke();
                            else
                                obj = @this.ToLambda<Func<float>>().Compile().Invoke();
                            break;
                        default:
                            obj = @this.ToLambda<Func<object>>().Compile().Invoke();
                            break;
                    }
                    break;
            }
            return obj;
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// linq正序排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return source.BuildIOrderedQueryable<T>(property, "OrderBy");
        }

        /// <summary>
        /// linq倒叙排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return source.BuildIOrderedQueryable<T>(property, "OrderByDescending");
        }

        /// <summary>
        /// linq正序多列排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return source.BuildIOrderedQueryable<T>(property, "ThenBy");
        }

        /// <summary>
        /// linq倒序多列排序扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return source.BuildIOrderedQueryable<T>(property, "ThenByDescending");
        }

        /// <summary>
        /// 根据属性和排序方法构建IOrderedQueryable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static IOrderedQueryable<T> BuildIOrderedQueryable<T>(this IQueryable<T> source, string property, string methodName)
        {
            var props = property.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);
            var result = typeof(Queryable).GetMethods().Single(
              method => method.Name == methodName
                && method.IsGenericMethodDefinition
                && method.GetGenericArguments().Length == 2
                && method.GetParameters().Length == 2)
              .MakeGenericMethod(typeof(T), type)
              .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
        #endregion

        #region Substring
        /// <summary>
        /// 从分隔符开始向尾部截取字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="lastIndexOf">true：从最后一个匹配的分隔符开始截取，false：从第一个匹配的分隔符开始截取，默认：true</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string separator, bool lastIndexOf = true)
        {
            var start = (lastIndexOf ? @this.LastIndexOf(separator) : @this.IndexOf(separator)) + separator.Length;
            var length = @this.Length - start;
            return @this.Substring(start, length);
        }
        #endregion

        #region GetCoreType
        /// <summary>
        /// 如果type是Nullable类型则返回UnderlyingType，否则则直接返回type本身
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>Type</returns>
        public static Type GetCoreType(this Type @this)
        {
            if (@this?.IsNullable() == true)
            {
                @this = Nullable.GetUnderlyingType(@this);
            }
            return @this;
        }
        #endregion

        #region IsNullable
        /// <summary>
        /// 判断类型是否是Nullable类型
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>bool</returns>
        public static bool IsNullable(this Type @this)
        {
            return @this.IsValueType && @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        #endregion

        #region IsNull
        /// <summary>
        /// 是否为空
        /// </summary>
        /// <param name="this">object对象</param>
        /// <returns>bool</returns>
        public static bool IsNull(this object @this)
        {
            return @this == null || @this == DBNull.Value;
        }
        #endregion

        #region IsNullOrEmpty
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="this">待验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrEmpty(this string @this)
        {
            return string.IsNullOrEmpty(@this);
        }
        #endregion

        #region IsNullOrWhiteSpace
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="this">待验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrWhiteSpace(this string @this)
        {
            return string.IsNullOrWhiteSpace(@this);
        }
        #endregion

        #region ToSafeValue
        /// <summary>
        /// 转换为安全类型的值
        /// </summary>
        /// <param name="this">object对象</param>
        /// <param name="type">type</param>
        /// <returns>object</returns>
        public static object ToSafeValue(this object @this, Type type)
        {
            return @this == null ? null : Convert.ChangeType(@this, type.GetCoreType());
        }
        #endregion

        #region ToDynamicParameters
        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter[] @this)
        {
            if (@this?.Length > 0)
            {
                var args = new DynamicParameters();
                @this.ToList().ForEach(p => args.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size));
                return args;
            }
            return null;
        }

        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this List<DbParameter> @this)
        {
            if (@this?.Count > 0)
            {
                var args = new DynamicParameters();
                @this.ForEach(p => args.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size));
                return args;
            }
            return null;
        }

        /// <summary>
        ///  DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter @this)
        {
            if (@this != null)
            {
                var args = new DynamicParameters();
                args.Add(@this.ParameterName, @this.Value, @this.DbType, @this.Direction, @this.Size);
                return args;
            }
            return null;
        }

        /// <summary>
        ///  IDictionary转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>        
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                var args = new DynamicParameters();
                foreach (var item in @this)
                {
                    args.Add(item.Key, item.Value);
                }
                return args;
            }
            return null;
        }
        #endregion

        #region ToDbParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="command">The command.</param>        
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbCommand command)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = x.Key;
                    parameter.Value = x.Value;
                    return parameter;
                }).ToArray();
            }
            return null;
        }

        /// <summary>
        ///  An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="connection">The connection.</param>        
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbConnection connection)
        {
            if (@this?.Count > 0)
            {
                var command = connection.CreateCommand();
                return @this.Select(x =>
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = x.Key;
                    parameter.Value = x.Value;
                    return parameter;
                }).ToArray();
            }
            return null;
        }
        #endregion

        #region ToSqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a SQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a SqlParameter[].</returns>
        public static SqlParameter[] ToSqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new SqlParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToMySqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a MySQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a MySqlParameter[].</returns>
        public static MySqlParameter[] ToMySqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new MySqlParameter(x.Key.Replace("@", "?").Replace(":", "?"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToSqliteParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Sqlite parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a SqliteParameter[].</returns>
        public static SqliteParameter[] ToSqliteParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new SqliteParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToOracleParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Oracle parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a OracleParameter[].</returns>
        public static OracleParameter[] ToOracleParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new OracleParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToNpgsqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a PostgreSQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a NpgsqlParameter[].</returns>
        public static NpgsqlParameter[] ToNpgsqlParameters(this IDictionary<string, object> @this)
        {
            if (@this?.Count > 0)
            {
                return @this.Select(x => new NpgsqlParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
            }
            return null;
        }
        #endregion

        #region ToDataTable
        /// <summary>
        /// IDataReader转换为DataTable
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(this IDataReader @this)
        {
            var table = new DataTable();
            if (@this?.IsClosed == false)
            {
                table.Load(@this);
            }
            return table;
        }

        /// <summary>
        /// List集合转DataTable
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">list数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this List<T> @this)
        {
            DataTable dt = null;
            if (@this?.Count > 0)
            {
                dt = new DataTable(typeof(T).Name);
                var typeName = typeof(T).Name;
                var first = @this.FirstOrDefault();
                var firstTypeName = first.GetType().Name;
                if (typeName.Contains("Dictionary`2") || (typeName == "Object" && (firstTypeName == "DapperRow" || firstTypeName == "DynamicRow")))
                {
                    var dic = first as IDictionary<string, object>;
                    dt.Columns.AddRange(dic.Select(o => new DataColumn(o.Key, o.Value?.GetType().GetCoreType() ?? typeof(object))).ToArray());
                    var dics = @this.Select(o => o as IDictionary<string, object>);
                    foreach (var item in dics)
                    {
                        dt.Rows.Add(item.Select(o => o.Value).ToArray());
                    }
                }
                else
                {
                    var props = typeName == "Object" ? first.GetType().GetProperties() : typeof(T).GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                    foreach (var prop in props)
                    {
                        dt.Columns.Add(prop.Name, prop?.PropertyType.GetCoreType() ?? typeof(object));
                    }
                    foreach (var item in @this)
                    {
                        var values = new object[props.Length];
                        for (var i = 0; i < props.Length; i++)
                        {
                            if (!props[i].CanRead) continue;
                            values[i] = props[i].GetValue(item, null);
                        }
                        dt.Rows.Add(values);
                    }
                }
            }
            return dt;
        }
        #endregion

        #region ToDataSet
        /// <summary>
        /// IDataReader转换为DataSet
        /// </summary>
        /// <param name="this">reader数据源</param>
        /// <returns>DataSet</returns>
        public static DataSet ToDataSet(this IDataReader @this)
        {
            var ds = new DataSet();
            if (@this.IsClosed == false)
            {
                do
                {
                    var schemaTable = @this.GetSchemaTable();
                    var dt = new DataTable();
                    for (var i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        var row = schemaTable.Rows[i];
                        dt.Columns.Add(new DataColumn((string)row["ColumnName"], (Type)row["DataType"]));
                    }
                    while (@this.Read())
                    {
                        var dataRow = dt.NewRow();
                        for (var i = 0; i < @this.FieldCount; i++)
                        {
                            dataRow[i] = @this.GetValue(i);
                        }
                        dt.Rows.Add(dataRow);
                    }
                    ds.Tables.Add(dt);
                }
                while (@this.NextResult());
            }
            return ds;
        }
        #endregion

        #region ToDynamic
        /// <summary>
        /// IDataReader数据转为dynamic对象
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>dynamic</returns>
        public static dynamic ToDynamic(this IDataReader @this)
        {
            return @this.ToDynamics()?.FirstOrDefault();
        }

        /// <summary>
        /// IDataReader数据转为dynamic对象集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>dynamic集合</returns>
        public static IEnumerable<dynamic> ToDynamics(this IDataReader @this)
        {
            if (@this?.IsClosed == false)
            {
                using (@this)
                {
                    while (@this.Read())
                    {
                        var row = new ExpandoObject() as IDictionary<string, object>;
                        for (var i = 0; i < @this.FieldCount; i++)
                        {
                            row.Add(@this.GetName(i), @this.GetValue(i));
                        }
                        yield return row;
                    }
                }
            }
        }
        #endregion

        #region ToDictionary
        /// <summary>
        /// IDataReader数据转为Dictionary对象
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object> ToDictionary(this IDataReader @this)
        {
            return @this.ToDictionaries()?.FirstOrDefault();
        }

        /// <summary>
        /// IDataReader数据转为Dictionary对象集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>Dictionary集合</returns>
        public static IEnumerable<Dictionary<string, object>> ToDictionaries(this IDataReader @this)
        {
            if (@this?.IsClosed == false)
            {
                using (@this)
                {
                    while (@this.Read())
                    {
                        var dic = new Dictionary<string, object>();
                        for (var i = 0; i < @this.FieldCount; i++)
                        {
                            dic[@this.GetName(i)] = @this.GetValue(i);
                        }
                        yield return dic;
                    }
                }
            }
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// IDataReader数据转为强类型实体
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>强类型实体</returns>
        public static T ToEntity<T>(this IDataReader @this)
        {
            var result = @this.ToEntities<T>();
            if (result != null)
            {
                return result.FirstOrDefault();
            }
            return default(T);
        }

        /// <summary>
        /// IDataReader数据转为强类型实体集合
        /// </summary>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>强类型实体集合</returns>
        public static IEnumerable<T> ToEntities<T>(this IDataReader @this)
        {
            if (@this?.IsClosed == false)
            {
                using (@this)
                {
                    var fields = new List<string>();
                    for (int i = 0; i < @this.FieldCount; i++)
                    {
                        fields.Add(@this.GetName(i));
                    }
                    while (@this.Read())
                    {
                        var instance = Activator.CreateInstance<T>();
                        var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                        foreach (var p in props)
                        {
                            if (!p.CanWrite) continue;
                            var field = fields.Where(o => o.ToLower() == p.Name.ToLower()).FirstOrDefault();
                            if (!field.IsNullOrEmpty() && !@this[field].IsNull())
                            {
                                p.SetValue(instance, @this[field].ToSafeValue(p.PropertyType), null);
                            }
                        }
                        yield return instance;
                    }
                }
            }
        }
        #endregion

        #region ToList
        /// <summary>
        /// IDataReader转换为T集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型集合</returns>
        public static List<T> ToList<T>(this IDataReader @this)
        {
            List<T> list = null;
            if (@this?.IsClosed == false)
            {
                var type = typeof(T);
                if (type.Name.Contains("IDictionary`2"))
                {
                    list = @this.ToDictionaries()?.Select(o => o as IDictionary<string, object>).ToList() as List<T>;
                }
                else if (type.Name.Contains("Dictionary`2"))
                {
                    list = @this.ToDictionaries()?.ToList() as List<T>;
                }
                else if (type.IsClass && type.Name != "Object" && type.Name != "String")
                {
                    list = @this.ToEntities<T>()?.ToList() as List<T>;
                }
                else
                {
                    var result = @this.ToDynamics()?.ToList();
                    list = result as List<T>;
                    if (list == null)
                    {
                        //适合查询单个字段的结果集
                        list = result.Select(o => (T)(o as IDictionary<string, object>)?.Select(x => x.Value).FirstOrDefault()).ToList();
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// IDataReader转换为T集合的集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型集合的集合</returns>
        public static List<List<T>> ToLists<T>(this IDataReader @this)
        {
            var result = new List<List<T>>();
            if (@this?.IsClosed == false)
            {
                using (@this)
                {
                    var type = typeof(T);
                    do
                    {
                        #region IDictionary
                        if (type.Name.Contains("Dictionary`2"))
                        {
                            var list = new List<Dictionary<string, object>>();
                            while (@this.Read())
                            {
                                var dic = new Dictionary<string, object>();
                                for (var i = 0; i < @this.FieldCount; i++)
                                {
                                    dic[@this.GetName(i)] = @this.GetValue(i);
                                }
                                list.Add(dic);
                            }
                            if (type.Name.Contains("IDictionary`2"))
                            {
                                result.Add(list.Select(o => o as IDictionary<string, object>).ToList() as List<T>);
                            }
                            else
                            {
                                result.Add(list as List<T>);
                            }
                        }
                        #endregion

                        #region Class T
                        else if (type.IsClass && type.Name != "Object" && type.Name != "String")
                        {
                            var list = new List<T>();
                            var fields = new List<string>();
                            for (int i = 0; i < @this.FieldCount; i++)
                            {
                                fields.Add(@this.GetName(i));
                            }
                            while (@this.Read())
                            {
                                var instance = Activator.CreateInstance<T>();
                                var props = instance.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                                foreach (var p in props)
                                {
                                    if (!p.CanWrite) continue;
                                    var field = fields.Where(o => o.ToLower() == p.Name.ToLower()).FirstOrDefault();
                                    if (!field.IsNullOrEmpty() && !@this[field].IsNull())
                                    {
                                        p.SetValue(instance, @this[field].ToSafeValue(p.PropertyType), null);
                                    }
                                }
                                list.Add(instance);
                            }
                            result.Add(list);
                        }
                        #endregion

                        #region dynamic
                        else
                        {
                            var list = new List<dynamic>();
                            while (@this.Read())
                            {
                                var row = new ExpandoObject() as IDictionary<string, object>;
                                for (var i = 0; i < @this.FieldCount; i++)
                                {
                                    row.Add(@this.GetName(i), @this.GetValue(i));
                                }
                                list.Add(row);
                            }
                            var item = list as List<T>;
                            if (item == null)
                            {
                                //适合查询单个字段的结果集
                                item = list.Select(o => (T)(o as IDictionary<string, object>)?.Select(x => x.Value).FirstOrDefault()).ToList();
                            }
                            result.Add(item);
                        }
                        #endregion
                    } while (@this.NextResult());
                }
            }
            return result;
        }
        #endregion

        #region ToFirstOrDefault
        /// <summary>
        /// IDataReader转换为T类型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this">IDataReader数据源</param>
        /// <returns>T类型对象</returns>
        public static T ToFirstOrDefault<T>(this IDataReader @this)
        {
            var list = @this.ToList<T>();
            if (list != null)
            {
                return list.FirstOrDefault();
            }
            return default(T);
        }
        #endregion

        #region SqlInject
        /// <summary>
        /// 判断是否sql注入
        /// </summary>
        /// <param name="this">sql字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static bool IsSqlInject(this string @this, string pattern = @"(?:')|(?:--)|(/\*(?:.|[\n\r])*?\*/)|(\b(select|update|union|and|or|delete|insert|trancate|char|into|substr|ascii|declare|exec|count|master|into|drop|execute)\b)")
        {
            if (@this.IsNullOrEmpty())
                return false;
            return Regex.IsMatch(@this, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 正则表达式替换sql
        /// </summary>
        /// <param name="this">sql字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static string ReplaceSqlWithRegex(this string @this, string pattern = @"(?:')|(?:--)|(/\*(?:.|[\n\r])*?\*/)|(\b(select|update|union|and|or|delete|insert|trancate|char|into|substr|ascii|declare|exec|count|master|into|drop|execute)\b)")
        {
            if (@this.IsNullOrEmpty())
                return @this;
            return Regex.Replace(@this, pattern, "");
        }
        #endregion

        #region Contains
        /// <summary>
        /// 正则判断是否包含目标字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="value">目标字符串，例如：判断是否包含ASC或DESC为@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)"</param>
        /// <param name="options">匹配模式</param>
        /// <returns></returns>
        public static bool Contains(this string @this, string value, RegexOptions options)
        {
            return Regex.IsMatch(@this, value, options);
        }
        #endregion

        #region Attribute
        /// <summary>
        /// 获取首个指定特性
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetFirstOrDefaultAttribute<T>() as T;
        }

        /// <summary>
        /// 获取指定特性集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object[] GetAttributes<T>(this MemberInfo @this) where T : Attribute
        {
            return @this?.GetCustomAttributes(typeof(T), false);
        }

        /// <summary>
        /// 获取首个指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object GetFirstOrDefaultAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetAttributes<T>()?.FirstOrDefault();
        }

        /// <summary>
        /// 是否包含指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool ContainsAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetAttributes<T>()?.Length > 0;
        }
        #endregion

        #region AddSQLBuilder
        /// <summary>
        /// SQLBuilder仓储注入扩展
        /// </summary>
        /// <param name="this">依赖注入服务集合</param>
        /// <param name="configuration">服务配置</param>
        /// <param name="defaultName">默认数据库名称</param>
        /// <param name="sqlIntercept">sql拦截委托</param>
        /// <param name="isEnableFormat">是否启用对表名和列名格式化，默认启用</param>
        /// <param name="type">依赖注入模式，默认单例模式</param>
        /// <returns></returns>
        /// <example>
        ///     <code>
        ///     //appsetting.json
        ///     {
        ///         "Logging": {
        ///             "LogLevel": {
        ///                 "Default": "Information",
        ///                 "Microsoft": "Warning",
        ///                 "Microsoft.Hosting.Lifetime": "Information"
        ///             }
        ///         },
        ///         "AllowedHosts": "*",
        ///         "ConnectionStrings": {
        ///             "Base": [ "SQLServer", "数据库连接字符串" ],
        ///             "Sqlserver": [ "SQLServer", "数据库连接字符串" ],
        ///             "Oracle": [ "Oracle", "数据库连接字符串" ],
        ///             "MySql": [ "MySQL", "数据库连接字符串" ],
        ///             "Sqlite": [ "SQLite", "数据库连接字符串" ],
        ///             "Pgsql": [ "PostgreSQL", "数据库连接字符串" ]
        ///         }
        ///     }
        ///     //Controller获取方法
        ///     private readonly IRepository _repository;
        ///     public WeatherForecastController(Func&lt;string, IRepository&gt; handler)
        ///     {
        ///         _repository = handler("Sqlserver");
        ///     }
        ///     </code>
        /// </example>
        public static IServiceCollection AddSQLBuilder(
            this IServiceCollection @this,
            IConfiguration configuration,
            string defaultName,
            Func<string, object, string> sqlIntercept = null,
            bool isEnableFormat = true,
            DependencyInjectionType type = DependencyInjectionType.Singleton)
        {
            Func<string, IRepository> @delegate = key =>
            {
                key = key.IsNullOrEmpty() ? defaultName : key;
                var config = configuration.GetSection($"ConnectionStrings:{key}").Get<List<string>>();
                var databaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), config[0]);
                switch (databaseType)
                {
                    case DatabaseType.SQLServer:
                        return new SqlRepository(config[1])
                        {
                            SqlIntercept = sqlIntercept,
                            IsEnableFormat = isEnableFormat
                        };
                    case DatabaseType.MySQL:
                        return new MySqlRepository(config[1])
                        {
                            SqlIntercept = sqlIntercept,
                            IsEnableFormat = isEnableFormat
                        };
                    case DatabaseType.Oracle:
                        return new OracleRepository(config[1])
                        {
                            SqlIntercept = sqlIntercept,
                            IsEnableFormat = isEnableFormat
                        };
                    case DatabaseType.SQLite:
                        return new SqliteRepository(config[1])
                        {
                            SqlIntercept = sqlIntercept,
                            IsEnableFormat = isEnableFormat
                        };
                    case DatabaseType.PostgreSQL:
                        return new NpgsqlRepository(config[1])
                        {
                            SqlIntercept = sqlIntercept,
                            IsEnableFormat = isEnableFormat
                        };
                    default:
                        throw new ArgumentException("数据库类型配置有误！");
                }
            };
            switch (type)
            {
                case DependencyInjectionType.Singleton:
                    @this.AddSingleton(x => @delegate);
                    break;
                case DependencyInjectionType.Transient:
                    @this.AddTransient(x => @delegate);
                    break;
                case DependencyInjectionType.Scoped:
                    @this.AddScoped(x => @delegate);
                    break;
                default:
                    break;
            }
            return @this;
        }
        #endregion
    }

    /// <summary>
    /// Oracle的DynamicParameters实现，用于支持Oracle游标类型
    /// </summary>
    public class OracleDynamicParameters : SqlMapper.IDynamicParameters
    {
        private readonly DynamicParameters dynamicParameters = new DynamicParameters();

        private readonly List<OracleParameter> oracleParameters = new List<OracleParameter>();

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oracleDbType"></param>
        /// <param name="direction"></param>
        public void Add(string name, OracleDbType oracleDbType, ParameterDirection direction)
        {
            var oracleParameter = new OracleParameter(name, oracleDbType, direction);
            oracleParameters.Add(oracleParameter);
        }

        /// <summary>
        /// Add
        /// </summary>
        /// <param name="name"></param>
        /// <param name="oracleDbType"></param>
        /// <param name="direction"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        public void Add(string name, OracleDbType oracleDbType, ParameterDirection direction, object value = null, int? size = null)
        {
            OracleParameter oracleParameter;
            if (size.HasValue)
                oracleParameter = new OracleParameter(name, oracleDbType, size.Value, value, direction);
            else
                oracleParameter = new OracleParameter(name, oracleDbType, value, direction);
            oracleParameters.Add(oracleParameter);
        }

        /// <summary>
        /// AddParameters
        /// </summary>
        /// <param name="command"></param>
        /// <param name="identity"></param>
        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            ((SqlMapper.IDynamicParameters)dynamicParameters).AddParameters(command, identity);
            if (command is OracleCommand oracleCommand)
                oracleCommand.Parameters.AddRange(oracleParameters.ToArray());
        }
    }
}
