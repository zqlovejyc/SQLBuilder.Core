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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// MemberInfo扩展类
    /// </summary>
    public static class MemberInfoExtensions
    {
        #region GetAttribute
        /// <summary>
        /// 获取首个指定特性
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetFirstOrDefaultAttribute<T>() as T;
        }
        #endregion

        #region GetAttributes
        /// <summary>
        /// 获取指定特性集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object[] GetAttributes<T>(this MemberInfo @this) where T : Attribute
        {
            return @this?.GetCustomAttributes(typeof(T), false);
        }

        /// <summary>
        /// 获取指定特性集合
        /// </summary>
        /// <param name="this"></param>
        /// <param name="attributeTypes"></param>
        /// <returns></returns>
        public static object[] GetAttributes(this MemberInfo @this, params Type[] attributeTypes)
        {
            if (attributeTypes.IsNullOrEmpty())
                return null;

            var res = new List<object>();
            foreach (var type in attributeTypes)
            {
                var attributes = @this?.GetCustomAttributes(type, false);
                if (attributes?.Length > 0)
                    res.AddRange(attributes);
            }

            return res.ToArray();
        }
        #endregion

        #region GetFirstOrDefaultAttribute
        /// <summary>
        /// 获取首个指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static object GetFirstOrDefaultAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetAttributes<T>()?.FirstOrDefault();
        }
        #endregion

        #region ContainsAttribute
        /// <summary>
        /// 是否包含指定特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool ContainsAttribute<T>(this MemberInfo @this) where T : Attribute
        {
            return @this.GetAttributes<T>()?.Length > 0;
        }
        #endregion
    }
}
