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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.LoadBalancer;
using SQLBuilder.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// IServiceCollection扩展类
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        #region AddRepository
        /// <summary>
        /// 注入泛型仓储
        /// </summary>
        /// <typeparam name="T">仓储类型</typeparam>
        /// <param name="this">依赖注入服务集合</param>
        /// <param name="sqlIntercept">sql拦截委托</param>
        /// <param name="isEnableFormat">是否启用对表名和列名格式化，默认：否</param>
        /// <param name="autoDispose">非事务的情况下，数据库连接是否自动释放，默认：是</param>
        /// <param name="countSyntax">分页计数语法，默认：COUNT(*)</param>
        /// <param name="lifeTime">生命周期，默认：Transient</param>
        /// <returns></returns>
        public static IServiceCollection AddRepository<T>(
            this IServiceCollection @this,
            Func<string, object, string> sqlIntercept = null,
            bool isEnableFormat = false,
            bool autoDispose = true,
            string countSyntax = "COUNT(*)",
            ServiceLifetime lifeTime = ServiceLifetime.Transient)
            where T : class, IRepository, new()
        {
            Func<IServiceProvider, T> @delegate = x => new()
            {
                AutoDispose = autoDispose,
                SqlIntercept = sqlIntercept,
                IsEnableFormat = isEnableFormat,
                CountSyntax = countSyntax,
                LoadBalancer = x.GetService<ILoadBalancer>()
            };

            switch (lifeTime)
            {
                case ServiceLifetime.Scoped:
                    @this.AddScoped(@delegate);
                    break;

                case ServiceLifetime.Transient:
                    @this.AddTransient(@delegate);
                    break;

                default:
                    throw new ArgumentException($"IRepository not allowed regist of `{lifeTime}`.");
            }

            return @this;
        }
        #endregion

        #region AddAllRepository
        /// <summary>
        /// 按需注入所有程序依赖的数据库仓储 <para>注意：仓储没有初始化MasterConnectionString和SlaveConnectionStrings</para>
        /// </summary>
        /// <param name="this">依赖注入服务集合</param>
        /// <param name="configuration">服务配置</param>
        /// <param name="sqlIntercept">sql拦截委托</param>
        /// <param name="isEnableFormat">是否启用对表名和列名格式化，默认：否</param>
        /// <param name="autoDispose">非事务的情况下，数据库连接是否自动释放，默认：是</param>
        /// <param name="countSyntax">分页计数语法，默认：COUNT(*)</param>
        /// <param name="connectionSection">连接字符串配置Section，默认：ConnectionStrings</param>
        /// <param name="lifeTime">生命周期，默认：Transient</param>
        /// <returns></returns>
        public static IServiceCollection AddAllRepository(
            this IServiceCollection @this,
            IConfiguration configuration,
            Func<string, object, string> sqlIntercept = null,
            bool isEnableFormat = false,
            bool autoDispose = true,
            string countSyntax = "COUNT(*)",
            string connectionSection = "ConnectionStrings",
            ServiceLifetime lifeTime = ServiceLifetime.Transient)
        {
            //数据库配置
            var configs = configuration.GetSection(connectionSection).Get<Dictionary<string, List<string>>>();

            //注入所有数据库
            if (configs.IsNotNull() && configs.Values.IsNotNull() && configs.Values.Any(x => x.IsNotNullOrEmpty()))
            {
                var databaseTypes = configs.Values.Where(x => x.IsNotNullOrEmpty()).Select(x => x[0].ToLower()).Distinct();
                foreach (var databaseType in databaseTypes)
                {
                    //SqlServer
                    if (databaseType.EqualIgnoreCase("SqlServer"))
                        @this.AddRepository<SqlRepository>(sqlIntercept, isEnableFormat, autoDispose, countSyntax, lifeTime);

                    //MySql
                    if (databaseType.EqualIgnoreCase("MySql"))
                        @this.AddRepository<MySqlRepository>(sqlIntercept, isEnableFormat, autoDispose, countSyntax, lifeTime);

                    //Oracle
                    if (databaseType.EqualIgnoreCase("Oracle"))
                        @this.AddRepository<OracleRepository>(sqlIntercept, isEnableFormat, autoDispose, countSyntax, lifeTime);

                    //Sqlite
                    if (databaseType.EqualIgnoreCase("Sqlite"))
                        @this.AddRepository<SqliteRepository>(sqlIntercept, isEnableFormat, autoDispose, countSyntax, lifeTime);

                    //PostgreSql
                    if (databaseType.EqualIgnoreCase("PostgreSql"))
                        @this.AddRepository<NpgsqlRepository>(sqlIntercept, isEnableFormat, autoDispose, countSyntax, lifeTime);
                }
            }

            return @this;
        }
        #endregion

        #region GetConnectionInformation
        /// <summary>
        /// 获取数据库连接信息
        /// </summary>
        /// <param name="configuration">服务配置</param>
        /// <param name="key">数据库标识键值</param>
        /// <param name="defaultName">默认数据库名称</param>
        /// <param name="connectionSection">连接字符串配置Section，默认：ConnectionStrings</param>
        /// <returns></returns>
        public static (
            DatabaseType databaseType,
            string masterConnectionString,
            (string connectionString, int weight)[] SlaveConnectionStrings)
            GetConnectionInformation(
            IConfiguration configuration,
            string key,
            string defaultName,
            string connectionSection)
        {
            //数据库标识键值
            key = key.IsNullOrEmpty() ? defaultName : key;

            //数据库配置
            var configs = configuration.GetSection($"{connectionSection}:{key}").Get<List<string>>();

            //数据库类型
            var databaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), configs[0]);

            //从库连接集合
            var slaveConnectionStrings = new List<(string connectionString, int weight)>();
            if (configs.Count > 2)
            {
                for (int i = 2; i < configs.Count; i++)
                {
                    if (configs[i].IsNullOrEmpty() || !configs[i].Contains(";"))
                        continue;

                    var slaveConnectionStringArray = configs[i].Split(';');
                    var slaveConnectionString = string.Join(";", slaveConnectionStringArray.Where(x => x.IsNotNullOrEmpty() && !x.StartsWithIgnoreCase("weight")));
                    var weight = int.Parse(slaveConnectionStringArray.FirstOrDefault(x => x.StartsWithIgnoreCase("weight"))?.Split('=')[1] ?? "1");
                    slaveConnectionStrings.Add((slaveConnectionString, weight));
                }
            }

            return (databaseType, configs[1], slaveConnectionStrings.ToArray());
        }
        #endregion

        #region CreateRepositoryFactory
        /// <summary>
        /// 创建IRepository委托，依赖AddAllRepository注入不同类型仓储
        /// </summary>
        /// <param name="provider">服务驱动</param>
        /// <param name="configuration"></param>
        /// <param name="defaultName">默认数据库名称</param>
        /// <param name="connectionSection">连接字符串配置Section，默认：ConnectionStrings</param>
        /// <returns></returns>
        public static Func<string, IRepository> CreateRepositoryFactory(
            IServiceProvider provider,
            IConfiguration configuration,
            string defaultName,
            string connectionSection = "ConnectionStrings")
        {
            return key =>
            {
                //获取数据库连接信息
                var (databaseType, masterConnectionStrings, slaveConnectionStrings) =
                    GetConnectionInformation(configuration, key, defaultName, connectionSection);

                //获取对应数据库类型的仓储
                IRepository repository = databaseType switch
                {
                    DatabaseType.SqlServer => provider.GetRequiredService<SqlRepository>(),
                    DatabaseType.MySql => provider.GetRequiredService<MySqlRepository>(),
                    DatabaseType.Oracle => provider.GetRequiredService<OracleRepository>(),
                    DatabaseType.Sqlite => provider.GetRequiredService<SqliteRepository>(),
                    DatabaseType.PostgreSql => provider.GetRequiredService<NpgsqlRepository>(),
                    _ => throw new ArgumentException($"Invalid database type `{databaseType}`."),
                };

                repository.MasterConnectionString = masterConnectionStrings;
                repository.SlaveConnectionStrings = slaveConnectionStrings;

                return repository;
            };
        }
        #endregion

        #region AddSqlBuilder
        /// <summary>
        /// SQLBuilder仓储注入扩展
        /// <para>注意：若要启用读写分离，则需要注入ILoadBalancer服务；</para>
        /// </summary>
        /// <param name="this">依赖注入服务集合</param>
        /// <param name="configuration">服务配置</param>
        /// <param name="defaultName">默认数据库名称</param>
        /// <param name="sqlIntercept">sql拦截委托</param>
        /// <param name="isEnableFormat">是否启用对表名和列名格式化，默认：否</param>
        /// <param name="autoDispose">非事务的情况下，数据库连接是否自动释放，默认：是</param>
        /// <param name="countSyntax">分页计数语法，默认：COUNT(*)</param>
        /// <param name="connectionSection">连接字符串配置Section，默认：ConnectionStrings</param>
        /// <param name="isInjectLoadBalancer">是否注入从库负载均衡，默认注入单例权重轮询方式(WeightRoundRobinLoadBalancer)，可以设置为false实现自定义方式</param>
        /// <param name="lifeTime">生命周期，默认：Transient</param>
        /// <returns></returns>
        /// <remarks>
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
        ///             "Base": [ "SqlServer", "数据库连接字符串","server=localhost;uid=sa;pwd=123;weight=1","server=localhost2;uid=sa;pwd=123;weight=1" ],
        ///             "Sqlserver": [ "SqlServer", "数据库连接字符串" ],
        ///             "Oracle": [ "Oracle", "数据库连接字符串" ],
        ///             "MySql": [ "MySql", "数据库连接字符串" ],
        ///             "Sqlite": [ "Sqlite", "数据库连接字符串" ],
        ///             "Pgsql": [ "PostgreSql", "数据库连接字符串" ]
        ///         }
        ///     }
        ///     //Controller获取方法
        ///     private readonly IRepository _repository;
        ///     public WeatherForecastController(Func&lt;string, IRepository&gt; handler)
        ///     {
        ///         _repository = handler("Sqlserver");
        ///     }
        ///     </code>
        /// </remarks>
        public static IServiceCollection AddSqlBuilder(
            this IServiceCollection @this,
            IConfiguration configuration,
            string defaultName,
            Func<string, object, string> sqlIntercept = null,
            bool isEnableFormat = false,
            bool autoDispose = true,
            string countSyntax = "COUNT(*)",
            string connectionSection = "ConnectionStrings",
            bool isInjectLoadBalancer = true,
            ServiceLifetime lifeTime = ServiceLifetime.Transient)
        {
            //注入负载均衡
            if (isInjectLoadBalancer)
                @this.AddSingleton<ILoadBalancer, WeightRoundRobinLoadBalancer>();

            //按需注入所有依赖的仓储
            @this.AddAllRepository(configuration, sqlIntercept, isEnableFormat, autoDispose, countSyntax, connectionSection, lifeTime);

            //根据生命周期类型注入服务
            switch (lifeTime)
            {
                case ServiceLifetime.Singleton:
                    @this.AddSingleton(x => CreateRepositoryFactory(x, configuration, defaultName, connectionSection));
                    break;

                case ServiceLifetime.Transient:
                    @this.AddTransient(x => CreateRepositoryFactory(x, configuration, defaultName, connectionSection));
                    break;

                case ServiceLifetime.Scoped:
                    @this.AddScoped(x => CreateRepositoryFactory(x, configuration, defaultName, connectionSection));
                    break;

                default:
                    break;
            }

            return @this;
        }
        #endregion
    }
}
