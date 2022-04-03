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

namespace SQLBuilder.Core.Attributes
{
    /// <summary>
    /// 指定表主键
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class KeyAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public KeyAttribute(string name = null) => Name = name;

        /// <summary>
        /// 主键名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 是否自增列
        /// </summary>
        public bool Identity { get; set; }

        /// <summary>
        /// 是否启用格式化，用于全局不启用格式化时，该列名为数据库关键字，此时需要单独启用格式化
        /// </summary>
        public bool Format { get; set; }

        /// <summary>
        /// Oracle sequence名称，用于Oracle自增列，可以在数据库层面设置为默认值
        /// <para>用于 Insert 返回自增列值时查询 OracleSequenceName.currval 使用</para>
        /// </summary>
        public string OracleSequenceName { get; set; }
    }
}
