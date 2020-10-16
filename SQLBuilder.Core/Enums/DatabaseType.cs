#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
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

namespace SQLBuilder.Core.Enums
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// SqlServer数据库类型
        /// </summary>
        SqlServer,

        /// <summary>
        /// MySql数据库类型
        /// </summary>
        MySql,

        /// <summary>
        /// Oracle数据库类型
        /// </summary>
        Oracle,

        /// <summary>
        /// Sqlite数据库类型
        /// </summary>
        Sqlite,

        /// <summary>
        /// PostgreSql数据库类型
        /// </summary>
        PostgreSql
    }
}
