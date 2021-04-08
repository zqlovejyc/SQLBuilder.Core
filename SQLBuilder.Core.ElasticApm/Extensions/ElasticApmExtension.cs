﻿#region License
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

using Elastic.Apm.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SQLBuilder.Core.ElasticApm.Diagnostics;
using Elastic.Apm.Extensions.Hosting;

namespace SQLBuilder.Core.ElasticApm.Extensions
{
    /// <summary>
    /// ElasticApm扩展类
    /// </summary>
    public static class ElasticApmExtension
    {
        /// <summary>
        /// 注入SQLBuilder的ElasticApm追踪
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSqlBuilderElasticApm(this IApplicationBuilder @this, IConfiguration configuration = null)
        {
            return @this.UseElasticApm(
                configuration,
                new SqlBuilderDiagnosticSubscriber());
        }

        /// <summary>
        /// 注入SQLBuilder的ElasticApm追踪
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static IHostBuilder UseSqlBuilderElasticApm(this IHostBuilder @this)
        {
            return @this.UseElasticApm(new SqlBuilderDiagnosticSubscriber());
        }
    }
}