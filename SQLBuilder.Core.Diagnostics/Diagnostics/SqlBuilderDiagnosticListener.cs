using Dapper;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using SQLBuilder.Core.Parameters;
using System;
using System.Data.Common;
using System.Linq;

namespace SQLBuilder.Core.Diagnostics.Diagnostics
{
    /// <summary>
    /// SqlBuilder日志诊断订阅实现类
    /// </summary>
    public class SqlBuilderDiagnosticListener
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger<SqlBuilderDiagnosticListener> _logger;

        /// <summary>
        /// SqlBuilder执行前
        /// </summary>
        public EventHandler<SqlBuilderDiagnosticBeforeMessage> OnExecuteBefore { get; set; }

        /// <summary>
        /// SqlBuilder执行后
        /// </summary>
        public EventHandler<SqlBuilderDiagnosticAfterMessage> OnExecuteAfter { get; set; }

        /// <summary>
        /// SqlBuilder执行异常
        /// </summary>
        public EventHandler<SqlBuilderDiagnosticErrorMessage> OnExecuteError { get; set; }

        /// <summary>
        /// SqlBuilder执行数据库连接释放
        /// </summary>
        public EventHandler<SqlBuilderDiagnosticDisposeMessage> OnExecuteDispose { get; set; }

        /// <summary>
        /// SqlBuilder数据库连接释放异常
        /// </summary>
        public EventHandler<SqlBuilderDiagnosticErrorMessage> OnDisposeError { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public SqlBuilderDiagnosticListener(ILogger<SqlBuilderDiagnosticListener> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取sql参数json
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static string GetParameterJson(object parameters)
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

            if (this.OnExecuteBefore != null)
            {
                this.OnExecuteBefore(this, new SqlBuilderDiagnosticBeforeMessage
                {
                    Sql = sql,
                    ParameterJson = GetParameterJson(parameters),
                    DatabaseType = databaseType,
                    DataSource = dataSource,
                    Timespan = timestamp,
                    OperationId = operationId,
                    ExecuteBefore = DateTime.Now,
                    Logger = _logger
                });
            }
            else
            {
                _logger.LogInformation(@$"
[{DiagnosticStrings.BeforeExecute}]
[sql] = {sql}
[parameters] = {GetParameterJson(parameters)}
[databaseType] = {databaseType}
[dataSource] = {dataSource}
[timestamp] = {timestamp}
[operationId] = {operationId}
[executeBefore] = {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            }
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

            if (this.OnExecuteAfter != null)
            {
                this.OnExecuteAfter(this, new SqlBuilderDiagnosticAfterMessage
                {
                    Sql = sql,
                    ParameterJson = GetParameterJson(parameters),
                    DataSource = dataSource,
                    OperationId = operationId,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    ExecuteAfter = DateTime.Now,
                    Logger = _logger
                });
            }
            else
            {
                _logger.LogInformation(@$"
[{DiagnosticStrings.AfterExecute}]
[sql] = {sql}
[parameters] = {GetParameterJson(parameters)}
[dataSource] = {dataSource}
[operationId] = {operationId}
[elapsedMilliseconds] = {elapsedMilliseconds}
[executeAfter] = {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            }
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

            if (this.OnExecuteError != null)
            {
                this.OnExecuteError(this, new SqlBuilderDiagnosticErrorMessage
                {
                    Exception = exception,
                    OperationId = operationId,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    ExecuteError = DateTime.Now,
                    Logger = _logger
                });
            }
            else
            {
                _logger.LogInformation(@$"
[{DiagnosticStrings.ErrorExecute}]
[exception] = {exception}
[operationId] = {operationId}
[elapsedMilliseconds] = {elapsedMilliseconds}
[executeError] = {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            }
        }

        /// <summary>
        /// 执行数据库连接释放
        /// </summary>
        /// <param name="masterConnection">主库连接</param>
        /// <param name="salveConnection">从库连接</param>
        [DiagnosticName(DiagnosticStrings.DisposeExecute)]
        public void ExecuteDispose(DbConnection masterConnection, DbConnection salveConnection)
        {
            if (masterConnection == null && salveConnection == null)
                return;

            if (this.OnExecuteDispose != null)
            {
                this.OnExecuteDispose(this, new SqlBuilderDiagnosticDisposeMessage
                {
                    MasterConnection = masterConnection,
                    SalveConnection = salveConnection,
                    ExecuteDispose = DateTime.Now,
                    Logger = _logger
                });
            }
            else
            {
                _logger.LogInformation(@$"
[{DiagnosticStrings.DisposeExecute}]
[masterConnection] = {masterConnection?.ConnectionString}
[salveConnection] = {salveConnection?.ConnectionString}
[executeError] = {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            }
        }

        /// <summary>
        /// 数据库连接释放异常
        /// </summary>
        /// <param name="exception">异常</param>
        [DiagnosticName(DiagnosticStrings.DisposeException)]
        public void DisposeException(Exception exception)
        {
            if (exception == null)
                return;

            if (this.OnDisposeError != null)
            {
                this.OnDisposeError(this, new SqlBuilderDiagnosticErrorMessage
                {
                    Exception = exception,
                    ExecuteError = DateTime.Now,
                    Logger = _logger
                });
            }
            else
            {
                _logger.LogInformation(@$"
[{DiagnosticStrings.DisposeException}]
[exception] = {exception}
[executeError] = {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            }
        }
    }
}
