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
using SQLBuilder.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// SqlBuilderCore扩展类
    /// </summary>
    public static class SqlBuilderExtensions
    {
        #region Execute
        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static int Execute<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.ExecuteBySql(@this.Sql, @this.DynamicParameters);
        }

        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await repository.ExecuteBySqlAsync(@this.Sql, @this.DynamicParameters);
        }
        #endregion

        #region ToEntity
        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static TEntity ToEntity<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return @this.ToEntity<TEntity, TEntity>(repository);
        }

        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static TReturn ToEntity<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.FindEntity<TReturn>(@this.Sql, @this.DynamicParameters);
        }

        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<TEntity> ToEntityAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await @this.ToEntityAsync<TEntity, TEntity>(repository);
        }

        /// <summary>
        /// 查询单个实体
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<TReturn> ToEntityAsync<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await repository.FindEntityAsync<TReturn>(@this.Sql, @this.DynamicParameters);
        }
        #endregion

        #region ToList
        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static List<TEntity> ToList<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return @this.ToList<TEntity, TEntity>(repository);
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static List<TReturn> ToList<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.FindList<TReturn>(@this.Sql, @this.DynamicParameters)?.ToList();
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<List<TEntity>> ToListAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await @this.ToListAsync<TEntity, TEntity>(repository);
        }

        /// <summary>
        /// 查询集合
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<List<TReturn>> ToListAsync<TEntity, TReturn>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return (await repository.FindListAsync<TReturn>(@this.Sql, @this.DynamicParameters))?.ToList();
        }
        #endregion

        #region ToObject
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static object ToObject<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return repository.FindObject(@this.Sql, @this.DynamicParameters);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static async Task<object> ToObjectAsync<TEntity>(this SqlBuilderCore<TEntity> @this, IRepository repository) where TEntity : class
        {
            return await repository.FindObjectAsync(@this.Sql, @this.DynamicParameters);
        }
        #endregion
    }
}