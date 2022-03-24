#region License
/***
 * Copyright © 2018-2025, 张强 (943620963@qq.com).
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
using Oracle.ManagedDataAccess.Client;

namespace SQLBuilder.Core.Attributes
{
    /// <summary>
    /// Oracle列特性，此特性仅用于Oracle特殊数据库字段类型使用，如：[OracleColumn(DbType = OracleDbType.NClob)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class OracleColumnAttribute : Attribute
    {
        /// <summary>
        /// Oracle数据类型
        /// </summary>
        public OracleDbType DbType { get; set; }
    }
}
