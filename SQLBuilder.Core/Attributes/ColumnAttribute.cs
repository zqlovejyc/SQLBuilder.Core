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

using System;

namespace SQLBuilder.Core.Attributes
{
    /// <summary>
    /// 指定列名
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">列名</param>    
        public ColumnAttribute(string name = null) => Name = name;

        /// <summary>
        /// 数据库表列名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 新增是否有效
        /// </summary>
        public bool Insert { get; set; } = true;

        /// <summary>
        /// 更新是否有效
        /// </summary>
        public bool Update { get; set; } = true;

        /// <summary>
        /// 是否启用格式化，用于全局不启用格式化时，该列名为数据库关键字，此时需要单独启用格式化
        /// </summary>
        public bool Format { get; set; }
    }
}
