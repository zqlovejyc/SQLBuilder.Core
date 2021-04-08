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

using Dapper;
using SkyApm;
using SkyApm.Common;
using SkyApm.Diagnostics;
using SkyApm.Tracing;
using SkyApm.Tracing.Segments;
using SQLBuilder.Core.Diagnostics;
using SQLBuilder.Core.Extensions;
using SQLBuilder.Core.Parameters;
using System;
using System.Linq;

namespace SQLBuilder.Core.SkyWalking.Diagnostics
{
    /// <summary>
    /// SQLBuilder跟踪诊断处理器
    /// </summary>
    public class SqlBuilderTracingDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        /// <summary>
        /// 私有字段
        /// </summary>
        private readonly ITracingContext _tracingContext;
        private readonly IExitSegmentContextAccessor _contextAccessor;
        private readonly StringOrIntValue? _component;

        /// <summary>
        /// 监听名称
        /// </summary>
        public string ListenerName { get; } = DiagnosticStrings.DiagnosticListenerName;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tracingContext"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="component"></param>
        public SqlBuilderTracingDiagnosticProcessor(
            ITracingContext tracingContext,
            IExitSegmentContextAccessor contextAccessor,
            StringOrIntValue? component = null)
        {
            _tracingContext = tracingContext;
            _contextAccessor = contextAccessor;
            _component = component ?? Components.SQLCLIENT;
        }

        /// <summary>
        /// 创建SegmentContext
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dataSource">数据源</param>
        /// <returns></returns>
        private SegmentContext CreateExitSegmentContext(string sql, string dataSource)
        {
            var operationName = $"{DiagnosticStrings.Prefix}{sql.Split(' ').FirstOrDefault()}";
            var context = _tracingContext.CreateExitSegmentContext(operationName, dataSource);

            context.Span.SpanLayer = SpanLayer.DB;
            context.Span.Component = _component.Value;
            context.Span.AddTag(Tags.DB_TYPE, "Sql");

            return context;
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="message">诊断消息</param>
        [DiagnosticName(DiagnosticStrings.BeforeExecute)]
        public void ExecuteBefore([Object] DiagnosticsMessage message)
        {
            if (message != null && !message.Sql.IsNullOrEmpty())
            {
                var parameterJson = string.Empty;
                if (message.Parameters is DynamicParameters dynamicParameters)
                    parameterJson = dynamicParameters
                        .ParameterNames?
                        .ToDictionary(k => k, v => dynamicParameters.Get<object>(v))
                        .ToJson();
                else if (message.Parameters is OracleDynamicParameters oracleDynamicParameters)
                    parameterJson = oracleDynamicParameters
                        .OracleParameters
                        .ToDictionary(k => k.ParameterName, v => v.Value)
                        .ToJson();
                else
                    parameterJson = message.Parameters.ToJson();

                var context = CreateExitSegmentContext(message.Sql, message.DataSource);

                context.Span.AddLog(LogEvent.Event(DiagnosticStrings.BeforeExecute.Split(' ').Last()));
                context.Span.AddLog(LogEvent.Message($"sql --> {message.Sql}"));
                context.Span.AddLog(LogEvent.Message($"parameters --> {parameterJson}"));
                context.Span.AddLog(LogEvent.Message($"databaseType --> {message.DatabaseType}"));
                context.Span.AddLog(LogEvent.Message($"dataSource --> {message.DataSource}"));
                context.Span.AddLog(LogEvent.Message($"timestamp --> {message.Timestamp}"));
            }
        }

        /// <summary>
        /// 执行后
        /// </summary>
        /// <param name="elapsedMilliseconds">耗时</param>
        [DiagnosticName(DiagnosticStrings.AfterExecute)]
        public void ExecuteAfter([Property(Name = "ElapsedMilliseconds")] long? elapsedMilliseconds)
        {
            var context = _contextAccessor.Context;
            if (context != null)
            {
                context.Span.AddLog(LogEvent.Event(DiagnosticStrings.AfterExecute.Split(' ').Last()));
                context.Span.AddLog(LogEvent.Message($"elapsedMilliseconds --> {elapsedMilliseconds}ms"));
                _tracingContext.Release(context);
            }
        }

        /// <summary>
        /// 执行异常
        /// </summary>
        /// <param name="ex">异常</param>
        [DiagnosticName(DiagnosticStrings.ErrorExecute)]
        public void ExecuteError([Property(Name = "Exception")] Exception ex)
        {
            var context = _contextAccessor.Context;
            if (context != null)
            {
                context.Span.ErrorOccurred(ex);
                _tracingContext.Release(context);
            }
        }
    }
}
