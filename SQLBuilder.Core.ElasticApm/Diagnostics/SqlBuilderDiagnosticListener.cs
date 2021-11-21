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
using Elastic.Apm;
using Elastic.Apm.Api;
using Microsoft.Extensions.DiagnosticAdapter;
using SQLBuilder.Core.Diagnostics;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using SQLBuilder.Core.Parameters;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SQLBuilder.Core.ElasticApm.Diagnostics
{
    /// <summary>
    /// SqlBuilder日志诊断订阅实现类
    /// </summary>
    public class SqlBuilderDiagnosticListener
    {
        private readonly IApmAgent _apmAgent;
        private readonly ConcurrentDictionary<string, ISpan> _spans = new();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="apmAgent"></param>
        public SqlBuilderDiagnosticListener(IApmAgent apmAgent)
        {
            _apmAgent = apmAgent;
        }

        /// <summary>
        /// 获取sql参数json
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string GetParameterJson(object parameters)
        {
            var parameterJson = string.Empty;
            if (parameters is DynamicParameters dynamicParameters)
                parameterJson = dynamicParameters
                    .ParameterNames?
                    .ToDictionary(k => k, v => dynamicParameters.Get<object>(v))
                    .ToJson();
            else if (parameters is OracleDynamicParameters oracleDynamicParameters)
                parameterJson = oracleDynamicParameters
                    .OracleParameters
                    .ToDictionary(k => k.ParameterName, v => v.Value)
                    .ToJson();
            else
                parameterJson = parameters.ToJson();

            return parameterJson;
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="operationId">操作id</param>
        /// <param name="databaseType">数据库类型</param>
        /// <param name="dataSource">数据库</param>
        /// <param name="timestamp">时间戳</param>
        [DiagnosticName(DiagnosticStrings.BeforeExecute)]
        public void ExecuteBefore(string sql, object parameters, string operationId, DatabaseType databaseType, string dataSource, long? timestamp)
        {
            if (sql.IsNullOrEmpty() || operationId.IsNullOrEmpty())
                return;

            var segment = _apmAgent?.Tracer.CurrentSpan ?? _apmAgent?.Tracer.CurrentTransaction as IExecutionSegment;
            if (segment == null)
                return;

            var span = segment.StartSpan(
                sql?.Split(' ').FirstOrDefault(),
                ApiConstants.TypeDb,
                databaseType.ToString(),
                DiagnosticStrings.BeforeExecute,
                false);

            if (span == null)
                return;

            span.SetLabel("sql", sql);
            span.SetLabel("parameters", GetParameterJson(parameters));
            span.SetLabel("databaseType", databaseType.ToString());
            span.SetLabel("dataSource", dataSource);
            span.SetLabel("timestamp", timestamp.Value);
            span.SetLabel("executeBefore", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            _spans.TryAdd(operationId, span);
        }

        /// <summary>
        /// 执行后
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="elapsedMilliseconds">耗时</param>
        /// <param name="operationId">操作id</param>
        /// <param name="dataSource">数据库</param>
        [DiagnosticName(DiagnosticStrings.AfterExecute)]
        public void ExecuteAfter(string sql, object parameters, long? elapsedMilliseconds, string operationId, string dataSource)
        {
            if (elapsedMilliseconds == null || operationId.IsNullOrEmpty())
                return;

            if (!_spans.TryRemove(operationId, out var span))
                return;

            if (span == null)
                return;

            span.Outcome = Outcome.Success;
            span.Duration = elapsedMilliseconds;
            span.Context.Db = new Database
            {
                Statement = $"sql: {sql}{Environment.NewLine}parameters: {GetParameterJson(parameters)}",
                Instance = dataSource,
                Type = Database.TypeSql
            };
            span.SetLabel("executeAfter", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            span.SetLabel("elapsedMilliseconds", $"{elapsedMilliseconds.Value}ms");
            span.End();
        }

        /// <summary>
        /// 执行异常
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="elapsedMilliseconds">耗时</param>
        /// <param name="operationId">操作id</param>
        [DiagnosticName(DiagnosticStrings.ErrorExecute)]
        public void ExecuteError(Exception exception, long? elapsedMilliseconds, string operationId)
        {
            if (exception == null || operationId.IsNullOrEmpty())
                return;

            if (!_spans.TryRemove(operationId, out var span))
                return;

            if (span == null)
                return;

            span.Outcome = Outcome.Failure;
            span.Duration = elapsedMilliseconds;
            span.CaptureException(exception);
            span.SetLabel("executeError", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            span.SetLabel("elapsedMilliseconds", $"{elapsedMilliseconds.Value}ms");
            span.End();
        }
    }
}
