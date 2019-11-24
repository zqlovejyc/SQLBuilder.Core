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
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SQLBuilder.Core
{
    /// <summary>
    /// 配置工具类
    /// </summary>
    public class ConfigurationManager
    {
        #region Public Property
        /// <summary>
        /// app配置
        /// </summary>
        public static IConfigurationRoot Configuration { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ConfigurationManager()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }
        #endregion

        #region SetConfigurationFile
        /// <summary>
        /// 设置app配置
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="basePath">文件路径，默认：Directory.GetCurrentDirectory()</param>
        public static void SetConfigurationFile(string fileName, string basePath = null)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(string.IsNullOrEmpty(basePath) ? Directory.GetCurrentDirectory() : basePath)
                .AddJsonFile(fileName, optional: true, reloadOnChange: true)
                .Build();
        }
        #endregion

        #region GetAppSettings
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetAppSettings<T>(string key)
        {
            return Configuration.GetValue<T>(key);
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetAppSettings<T>(string key, T defaultValue)
        {
            return Configuration.GetValue<T>(key, defaultValue);
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="type">配置对象类型</param>
        /// <param name="key">配置名称</param>
        /// <returns>返回object类型配置对象</returns>
        public static object GetAppSettings(Type type, string key)
        {
            return Configuration.GetValue(type, key);
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="type">配置对象类型</param>
        /// <param name="key">配置名称</param>
        /// <param name="defaultValue"></param>
        /// <returns>返回object类型配置对象</returns>
        public static object GetAppSettings(Type type, string key, object defaultValue)
        {
            return Configuration.GetValue(type, key, defaultValue);
        }
        #endregion

        #region GetConnectionString
        /// <summary>
        /// 获取ConnectionStrings节点下的链接字符串
        /// </summary>
        /// <param name="name">连接字符串名称</param>
        /// <returns>返回连接字符串</returns>
        public static string GetConnectionString(string name)
        {
            return Configuration.GetConnectionString(name);
        }
        #endregion

        #region GetOptionsAppSettings
        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsAppSettings<T>() where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration)
                .BuildServiceProvider()
                .GetService<IOptions<T>>();
            return options?.Value;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsAppSettings<T>(string key) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptions<T>>();
            return options?.Value;
        }
        #endregion

        #region GetOptionsMonitorAppSettings
        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitorAppSettings<T>() where T : class, new()
        {
            var options = new ServiceCollection()
               .Configure<T>(Configuration)
               .BuildServiceProvider()
               .GetService<IOptionsMonitor<T>>();
            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitorAppSettings<T>(string key) where T : class, new()
        {
            var options = new ServiceCollection()
               .Configure<T>(Configuration.GetSection(key))
               .BuildServiceProvider()
               .GetService<IOptionsMonitor<T>>();
            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="listener">监听配置变化时的委托</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitorAppSettings<T>(Action<T> listener) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration)
                .BuildServiceProvider()
                .GetService<IOptionsMonitor<T>>();
            if (listener != null) options?.OnChange(listener);
            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="listener">监听配置变化时的委托</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitorAppSettings<T>(Action<T, string> listener) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration)
                .BuildServiceProvider()
                .GetService<IOptionsMonitor<T>>();
            if (listener != null) options?.OnChange(listener);
            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <param name="listener">监听配置变化时的委托</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitorAppSettings<T>(string key, Action<T> listener) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptionsMonitor<T>>();
            if (listener != null) options?.OnChange(listener);
            return options?.CurrentValue;
        }

        /// <summary>
        /// 获取配置并映射到实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="key">配置名称</param>
        /// <param name="listener">监听配置变化时的委托</param>
        /// <returns>返回T类型配置对象</returns>
        public static T GetOptionsMonitorAppSettings<T>(string key, Action<T, string> listener) where T : class, new()
        {
            var options = new ServiceCollection()
                .Configure<T>(Configuration.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptionsMonitor<T>>();
            if (listener != null) options?.OnChange(listener);
            return options?.CurrentValue;
        }
        #endregion
    }
}
