using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLBuilder.Core.Diagnostics.Diagnostics;
using System;
using System.Diagnostics;

namespace SQLBuilder.Core.Diagnostics.Extensions
{
    /// <summary>
    /// SqlBuilder日志诊断扩展类
    /// </summary>
    public static class SqlBuilderDiagnosticExtensions
    {
        /// <summary>
        /// 注入SqlBuilder日志诊断
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration">服务配置</param>
        /// <returns></returns>
        public static IServiceCollection AddSqlBuilderDiagnostic(
            this IServiceCollection @this,
            IConfiguration configuration)
        {
            var enableDiagnosticListener = configuration.GetValue<bool>("SqlBuilder:EnableDiagnosticListener");
            if (enableDiagnosticListener)
                @this.AddSingleton<SqlBuilderDiagnosticListener>();

            return @this;
        }

        /// <summary>
        /// 使用SqlBuilder日志诊断
        /// </summary>
        /// <param name="this"></param>
        /// <param name="configuration">服务配置</param>
        /// <param name="executeBefore">执行前</param>
        /// <param name="executeAfter">执行后</param>
        /// <param name="executeError">执行异常</param>
        /// <param name="executeDispose">执行数据库连接释放</param>
        /// <param name="disposeError">数据库连接释放异常</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSqlBuilderDiagnostic(
            this IApplicationBuilder @this,
            IConfiguration configuration,
            EventHandler<SqlBuilderDiagnosticBeforeMessage> executeBefore = null,
            EventHandler<SqlBuilderDiagnosticAfterMessage> executeAfter = null,
            EventHandler<SqlBuilderDiagnosticErrorMessage> executeError = null,
            EventHandler<SqlBuilderDiagnosticDisposeMessage> executeDispose = null,
            EventHandler<SqlBuilderDiagnosticErrorMessage> disposeError = null)
        {
            var enableDiagnosticListener = configuration.GetValue<bool>("SqlBuilder:EnableDiagnosticListener");
            if (enableDiagnosticListener)
            {
                var sqlBuilderDiagnosticListener = @this.ApplicationServices.GetRequiredService<SqlBuilderDiagnosticListener>();

                sqlBuilderDiagnosticListener.OnExecuteBefore += executeBefore;
                sqlBuilderDiagnosticListener.OnExecuteAfter += executeAfter;
                sqlBuilderDiagnosticListener.OnExecuteError += executeError;
                sqlBuilderDiagnosticListener.OnExecuteDispose += executeDispose;
                sqlBuilderDiagnosticListener.OnDisposeError += disposeError;

                DiagnosticListener
                    .AllListeners
                    .Subscribe(new SqlBuilderObserver<DiagnosticListener>(
                        listener =>
                        {
                            if (listener.Name == DiagnosticStrings.DiagnosticListenerName)
                                listener.SubscribeWithAdapter(sqlBuilderDiagnosticListener);
                        }));
            }

            return @this;
        }
    }
}
