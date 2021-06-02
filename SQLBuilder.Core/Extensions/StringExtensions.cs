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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// string扩展类
    /// </summary>
    public static class StringExtensions
    {
        #region Substring
        /// <summary>
        /// 从分隔符开始向尾部截取字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="lastIndexOf">true：从最后一个匹配的分隔符开始截取，false：从第一个匹配的分隔符开始截取，默认：true</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string separator, bool lastIndexOf = true)
        {
            var startIndex = (lastIndexOf ?
                @this.LastIndexOf(separator, StringComparison.OrdinalIgnoreCase) :
                @this.IndexOf(separator, StringComparison.OrdinalIgnoreCase)) +
                separator.Length;

            var length = @this.Length - startIndex;
            return @this.Substring(startIndex, length);
        }
        #endregion

        #region IsNullOrEmpty
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="this">待验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrEmpty(this string @this)
        {
            return string.IsNullOrEmpty(@this);
        }

        /// <summary>
        /// 判断集合是否为空
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The collection to act on.</param>
        /// <returns>true if a null or is t>, false if not.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> @this)
        {
            return @this == null || !@this.Any();
        }
        #endregion

        #region IsNotNullOrEmpty
        /// <summary>
        /// 判断字符串是否不为空
        /// </summary>
        /// <param name="this">待验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsNotNullOrEmpty(this string @this)
        {
            return !@this.IsNullOrEmpty();
        }

        /// <summary>
        /// 判断集合是否不为空
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="this">The collection to act on.</param>
        /// <returns>true if a not null or is t>, false if not.</returns>
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> @this)
        {
            return @this != null && @this.Any();
        }
        #endregion

        #region IsNullOrWhiteSpace
        /// <summary>
        /// 判断字符串是否为空
        /// </summary>
        /// <param name="this">待验证的字符串</param>
        /// <returns>bool</returns>
        public static bool IsNullOrWhiteSpace(this string @this)
        {
            return string.IsNullOrWhiteSpace(@this);
        }
        #endregion

        #region SqlInject
        /// <summary>
        /// 判断是否sql注入
        /// </summary>
        /// <param name="this">sql字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static bool IsSqlInject(this string @this, string pattern = @"(?:')|(?:--)|(/\*(?:.|[\n\r])*?\*/)|(\b(select|update|union|and|or|delete|insert|trancate|char|into|substr|ascii|declare|exec|count|master|into|drop|execute)\b)")
        {
            if (@this.IsNullOrEmpty())
                return false;
            return Regex.IsMatch(@this, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 正则表达式替换sql
        /// </summary>
        /// <param name="this">sql字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static string ReplaceSqlWithRegex(this string @this, string pattern = @"(?:')|(?:--)|(/\*(?:.|[\n\r])*?\*/)|(\b(select|update|union|and|or|delete|insert|trancate|char|into|substr|ascii|declare|exec|count|master|into|drop|execute)\b)")
        {
            if (@this.IsNullOrEmpty())
                return @this;
            return Regex.Replace(@this, pattern, "");
        }
        #endregion

        #region Contains
        /// <summary>
        /// 判断是否包含目标字符串，区分大小写
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="strs">目标字符串数组"</param>
        /// <returns></returns>
        public static bool Contains(this string @this, params string[] strs)
        {
            if (@this.IsNullOrEmpty())
                return false;

            if (strs.IsNotNullOrEmpty())
            {
                foreach (var item in strs)
                {
                    if (item != null && @this.Contains(item))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 正则判断是否包含目标字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="value">目标字符串，例如：判断是否包含ASC或DESC为@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)"</param>
        /// <param name="options">匹配模式</param>
        /// <returns></returns>
        public static bool Contains(this string @this, string value, RegexOptions options)
        {
            return Regex.IsMatch(@this, value, options);
        }

        /// <summary>
        /// 忽略大小写的字符串相等比较，判断是否以任意一个待比较字符串相等
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="strs">待比较字符串数组</param>
        /// <returns>bool</returns>
        public static bool EqualIgnoreCase(this string @this, params string[] strs)
        {
            if (strs.IsNotNullOrEmpty())
            {
                foreach (var item in strs)
                {
                    if (string.Equals(@this, item, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 忽略大小写的字符串开始比较，判断是否以任意一个待比较字符串开始
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="strs">待比较字符串数组</param>
        /// <returns>bool</returns>
        public static bool StartsWithIgnoreCase(this string @this, params string[] strs)
        {
            if (@this.IsNullOrEmpty())
                return false;

            if (strs.IsNotNullOrEmpty())
            {
                foreach (var item in strs)
                {
                    if (item != null && @this.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 忽略大小写的字符串结束比较，判断是否以任意一个待比较字符串结束
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="strs">待比较字符串数组</param>
        /// <returns>bool</returns>
        public static bool EndsWithIgnoreCase(this string @this, params string[] strs)
        {
            if (@this.IsNullOrEmpty())
                return false;

            if (strs.IsNotNullOrEmpty())
            {
                foreach (var item in strs)
                {
                    if (item != null && @this.EndsWith(item, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 忽略大小写的字符串包含比较，判断是否以任意一个待比较字符串是否包含
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="strs">待比较字符串数组</param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string @this, params string[] strs)
        {
            if (@this.IsNullOrEmpty())
                return false;

            if (strs.IsNotNullOrEmpty())
            {
                foreach (var item in strs)
                {
                    if (item != null && @this.Contains(item, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
        #endregion
    }
}
