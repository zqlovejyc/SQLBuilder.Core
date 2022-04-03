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

using SQLBuilder.Core.Attributes;

namespace SQLBuilder.Core.Models
{
    /// <summary>
    /// 表实体列信息
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// 自定义列名
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Oracle sequence名称，用于Oracle自增列，可以在数据库层面设置为默认值
        /// <para>用于 Insert 返回自增列值时查询 OracleSequenceName.currval 使用</para>
        /// </summary>
        public string OracleSequenceName { get; set; }

        /// <summary>
        /// 是否新增
        /// </summary>
        public bool IsInsert { get; set; } = true;

        /// <summary>
        /// 是否更新
        /// </summary>
        public bool IsUpdate { get; set; } = true;

        /// <summary>
        /// 数据类型
        /// </summary>
        public DataTypeAttribute DataType { get; set; }
    }
}
