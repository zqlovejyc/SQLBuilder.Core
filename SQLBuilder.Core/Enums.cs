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

namespace SQLBuilder.Core
{
    #region DatabaseType
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// SqlServer数据库类型
        /// </summary>
        SQLServer,

        /// <summary>
        /// MySQL数据库类型
        /// </summary>
        MySQL,

        /// <summary>
        /// Oracle数据库类型
        /// </summary>
        Oracle,

        /// <summary>
        /// SQLite数据库类型
        /// </summary>
        SQLite,

        /// <summary>
        /// PostgreSQL数据库类型
        /// </summary>
        PostgreSQL,
    }
    #endregion

    #region OrderType
    /// <summary>
    /// 排序方式
    /// </summary>
    public enum OrderType
    {
         /// <summary>
        /// 升序
        /// </summary>
        Ascending,

        /// <summary>
        /// 降序
        /// </summary>
        Descending
    }
    #endregion
}