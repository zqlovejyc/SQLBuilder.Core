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
        /// <param name="countSyntax">分页计数语法，默认：COUNT(*)</param>
        /// <param name="connectionSection">连接字符串配置Section，默认：ConnectionStrings</param>
        /// <param name="isInjectLoadBalancer">是否注入从库负载均衡，默认注入单例权重轮询方式(WeightRoundRobinLoadBalancer)，可以设置为false实现自定义方式</param>
        /// <param name="lifeTime">生命周期，默认单例</param>
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
            string countSyntax = "COUNT(*)",
            string connectionSection = "ConnectionStrings",
            bool isInjectLoadBalancer = true,
            ServiceLifetime lifeTime = ServiceLifetime.Singleton)
        {
            //注入负载均衡
            if (isInjectLoadBalancer)
                @this.AddSingleton<ILoadBalancer, WeightRoundRobinLoadBalancer>();

            //从库负载均衡获取算法
            ILoadBalancer loadBalancer = null;

            //自定义仓储委托
            Func<string, IRepository> @delegate = key =>
            {
                //获取数据库连接信息
                var (databaseType, masterConnectionString, slaveConnectionStrings) =
                    GetDbConnectionInformation(configuration, defaultName, connectionSection, key);

                //实例化仓储
                return databaseType switch
                {
                    DatabaseType.SqlServer => new SqlRepository(masterConnectionString, configuration)
                    {
                        SqlIntercept = sqlIntercept,
                        IsEnableFormat = isEnableFormat,
                        CountSyntax = countSyntax,
                        LoadBalancer = loadBalancer,
                        SlaveConnectionStrings = slaveConnectionStrings
                    },
                    DatabaseType.MySql => new MySqlRepository(masterConnectionString, configuration)
                    {
                        SqlIntercept = sqlIntercept,
                        IsEnableFormat = isEnableFormat,
                        CountSyntax = countSyntax,
                        LoadBalancer = loadBalancer,
                        SlaveConnectionStrings = slaveConnectionStrings
                    },
                    DatabaseType.Oracle => new OracleRepository(masterConnectionString, configuration)
                    {
                        SqlIntercept = sqlIntercept,
                        IsEnableFormat = isEnableFormat,
                        CountSyntax = countSyntax,
                        LoadBalancer = loadBalancer,
                        SlaveConnectionStrings = slaveConnectionStrings
                    },
                    DatabaseType.Sqlite => new SqliteRepository(masterConnectionString, configuration)
                    {
                        SqlIntercept = sqlIntercept,
                        IsEnableFormat = isEnableFormat,
                        CountSyntax = countSyntax,
                        LoadBalancer = loadBalancer,
                        SlaveConnectionStrings = slaveConnectionStrings
                    },
                    DatabaseType.PostgreSql => new NpgsqlRepository(masterConnectionString, configuration)
                    {
                        SqlIntercept = sqlIntercept,
                        IsEnableFormat = isEnableFormat,
                        CountSyntax = countSyntax,
                        LoadBalancer = loadBalancer,
                        SlaveConnectionStrings = slaveConnectionStrings
                    },
                    _ => throw new ArgumentException("数据库类型配置有误！"),
                };
            };

            //根据生命周期类型注入服务
            switch (lifeTime)
            {
                case ServiceLifetime.Singleton:
                    @this.AddSingleton(x => { loadBalancer = x.GetService<ILoadBalancer>(); return @delegate; });
                    break;
                case ServiceLifetime.Transient:
                    @this.AddTransient(x => { loadBalancer = x.GetService<ILoadBalancer>(); return @delegate; });
                    break;
                case ServiceLifetime.Scoped:
                    @this.AddScoped(x => { loadBalancer = x.GetService<ILoadBalancer>(); return @delegate; });
                    break;
                default:
                    break;
            }
            return @this;
        }

        /// <summary>
        /// 获取数据库连接信息
        /// </summary>
        /// <param name="configuration">服务配置</param>
        /// <param name="defaultName">默认数据库名称</param>
        /// <param name="connectionSection">连接字符串配置Section，默认：ConnectionStrings</param>
        /// <param name="key">数据库json配置key</param>
        /// <returns></returns>
        public static (
            DatabaseType databaseType,
            string masterConnectionString,
            (string connectionString, int weight)[] slaveConnectionStrings)
            GetDbConnectionInformation(
            IConfiguration configuration,
            string defaultName,
            string connectionSection,
            string key)
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
    }
}
