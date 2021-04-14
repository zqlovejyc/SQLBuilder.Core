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

using Microsoft.Extensions.DependencyInjection;
using SkyApm;
using SkyApm.Common;
using SkyApm.Tracing;
using SkyApm.Utilities.DependencyInjection;
using SQLBuilder.Core.SkyWalking.Diagnostics;
using System;

namespace SQLBuilder.Core.SkyWalking.Extensions
{
    /// <summary>
    /// SkyWalking扩展类
    /// </summary>
    public static class SkyWalkingExtensions
    {
        /// <summary>
        /// 注入SkyApm链路追踪
        /// </summary>
        /// <param name="this"></param>
        /// <param name="component">自定义设定SQLBuilder组件，若未设定则默认采用SqlClient组件，
        /// <para>具体设定参照：https://github.com/dotnetcore/FreeSql/issues/222</para>
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddSqlBuilderSkyApm(this IServiceCollection @this, StringOrIntValue? component = null)
        {
            @this.AddSkyApmExtensions().AddSqlBuilderSkyApm(component);

            return @this;
        }

        /// <summary>
        /// 注入SkyApm链路追踪
        /// </summary>
        /// <param name="this"></param>
        /// <param name="component">自定义设定SQLBuilder组件，若未设定则默认采用SqlClient组件，
        /// <para>具体设定参照：https://github.com/dotnetcore/FreeSql/issues/222</para>
        /// </param>
        /// <returns></returns>
        public static SkyApmExtensions AddSqlBuilderSkyApm(this SkyApmExtensions @this, StringOrIntValue? component = null)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(SkyApmExtensions));

            @this.Services.AddSingleton<ITracingDiagnosticProcessor>(x =>
                new SqlBuilderTracingDiagnosticProcessor(
                    x.GetRequiredService<ITracingContext>(),
                    x.GetRequiredService<IExitSegmentContextAccessor>(),
                    component));

            return @this;
        }
    }
}
