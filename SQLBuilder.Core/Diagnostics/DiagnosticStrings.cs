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

namespace SQLBuilder.Core.Diagnostics
{
    /// <summary>
    /// 诊断常量
    /// </summary>
    public static class DiagnosticStrings
    {
        /// <summary>
        /// 监听名称
        /// </summary>
        public const string DiagnosticListenerName = "SQLBuilderDiagnosticListener";

        /// <summary>
        /// 前缀
        /// </summary>
        public const string Prefix = "SQLBuilder ";

        /// <summary>
        /// 执行前
        /// </summary>
        public const string BeforeExecute = Prefix + "ExecuteBefore";

        /// <summary>
        /// 执行后
        /// </summary>
        public const string AfterExecute = Prefix + "ExecuteAfter";

        /// <summary>
        /// 执行异常
        /// </summary>
        public const string ErrorExecute = Prefix + "ExecuteError";
    }
}
