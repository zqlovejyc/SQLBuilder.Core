#region License
/***
 * Copyright © 2018-2022, 张强 (943620963@qq.com).
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

using Elastic.Apm;
using Elastic.Apm.DiagnosticSource;
using SQLBuilder.Core.Diagnostics;
using System;
using System.Diagnostics;

namespace SQLBuilder.Core.ElasticApm.Diagnostics
{
    /// <summary>
    /// SqlBuilder ElasticApm日志诊断订阅者实现
    /// </summary>
    public class SqlBuilderDiagnosticSubscriber : IDiagnosticsSubscriber
    {
        /// <summary>
        /// 订阅方法
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IApmAgent agent)
        {
            if (!agent.ConfigurationReader.Enabled)
                return null;

            return DiagnosticListener
                .AllListeners
                .Subscribe(new SqlBuilderObserver<DiagnosticListener>(listener =>
                {
                    if (listener.Name == DiagnosticStrings.DiagnosticListenerName)
                        listener.SubscribeWithAdapter(
                                new SqlBuilderDiagnosticListener(agent));
                }));
        }
    }
}
