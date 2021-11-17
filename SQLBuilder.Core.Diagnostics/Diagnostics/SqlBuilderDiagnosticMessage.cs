using Microsoft.Extensions.Logging;
using SQLBuilder.Core.Enums;
using System;
using System.Data.Common;

namespace SQLBuilder.Core.Diagnostics.Diagnostics
{
    /// <summary>
    /// SqlBuilder日志诊断执行前消息
    /// </summary>
    public class SqlBuilderDiagnosticBeforeMessage
    {
        /// <summary>
        /// sql语句
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// json参数
        /// </summary>
        public string ParameterJson { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DatabaseType { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long? Timespan { get; set; }

        /// <summary>
        /// 操作id
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecuteBefore { get; set; }

        /// <summary>
        /// 日志
        /// </summary>
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public ILogger<SqlBuilderDiagnosticListener> Logger { get; set; }
    }

    /// <summary>
    /// SqlBuilder日志诊断执行后消息
    /// </summary>
    public class SqlBuilderDiagnosticAfterMessage
    {
        /// <summary>
        /// sql语句
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// json参数
        /// </summary>
        public string ParameterJson { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 操作id
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// 耗时(ms)
        /// </summary>
        public long? ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecuteAfter { get; set; }

        /// <summary>
        /// 日志
        /// </summary>
        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public ILogger<SqlBuilderDiagnosticListener> Logger { get; set; }
    }

    /// <summary>
    /// SqlBuilder日志诊断执行异常消息
    /// </summary>
    public class SqlBuilderDiagnosticErrorMessage
    {
        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 操作id
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// 耗时(ms)
        /// </summary>
        public long? ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecuteError { get; set; }

        /// <summary>
        /// 日志
        /// </summary>

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public ILogger<SqlBuilderDiagnosticListener> Logger { get; set; }
    }

    /// <summary>
    /// SqlBuilder日志诊断执行数据库连接释放消息
    /// </summary>
    public class SqlBuilderDiagnosticDisposeMessage
    {
        /// <summary>
        /// 主库数据库连接对象
        /// </summary>
        public DbConnection MasterConnection { get; set; }

        /// <summary>
        /// 从库数据库连接对象
        /// </summary>
        public DbConnection SalveConnection { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecuteDispose { get; set; }

        /// <summary>
        /// 日志
        /// </summary>

        [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public ILogger<SqlBuilderDiagnosticListener> Logger { get; set; }
    }
}
