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

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// Type扩展类
    /// </summary>
    public static class TypeExtensions
    {
        #region GetCoreType
        /// <summary>
        /// 如果type是Nullable类型则返回UnderlyingType，否则则直接返回type本身
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>Type</returns>
        public static Type GetCoreType(this Type @this)
        {
            if (@this?.IsNullable() == true)
            {
                @this = Nullable.GetUnderlyingType(@this);
            }
            return @this;
        }
        #endregion

        #region IsNullable
        /// <summary>
        /// 判断类型是否是Nullable类型
        /// </summary>
        /// <param name="this">类型</param>
        /// <returns>bool</returns>
        public static bool IsNullable(this Type @this)
        {
            return @this.IsValueType && @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        #endregion

        #region IsAnonymousType
        /// <summary>
        /// 是否是匿名类型
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type @this) =>
            @this != null && (@this.FullName.StartsWith("<>f__AnonymousType") || @this.FullName.StartsWith("VB$AnonymousType"));
        #endregion
    }
}
