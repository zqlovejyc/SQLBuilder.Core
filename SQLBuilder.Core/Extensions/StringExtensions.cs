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

using SQLBuilder.Core.Enums;
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
        /// 从分隔符开始向尾部截取字符串，不包含分隔符字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="lastIndexOf">true：从最后一个匹配的分隔符开始截取，false：从第一个匹配的分隔符开始截取，默认：true</param>
        /// <param name="comparisonType">字符串比较类型，默认区分大小写</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string separator, bool lastIndexOf = true, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (@this.IsNullOrEmpty())
                return string.Empty;

            if (separator.IsNullOrEmpty())
                return string.Empty;

            //分隔符索引
            var startIndex = lastIndexOf
                ? @this.LastIndexOf(separator, comparisonType)
                : @this.IndexOf(separator, comparisonType);

            if (startIndex == -1)
                return string.Empty;

            startIndex += separator.Length;

            //截取长度
            var length = @this.Length - startIndex;

            return @this.Substring(startIndex, length);
        }

        /// <summary>
        /// 根据开始和结束字符串截取字符串，不包含开始和结束字符串
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="start">开始字符串</param>
        /// <param name="end">结束字符串</param>
        /// <param name="startIsIndexOf">开始字符串是否是IndexOf，默认true，否则LastIndexOf</param>
        /// <param name="endIsIndexOf">结束字符串是否是IndexOf，默认true，否则LastIndexOf</param>
        /// <param name="comparisonType">字符串比较类型，默认区分大小写</param>
        /// <returns>string</returns>
        public static string Substring(this string @this, string start, string end, bool startIsIndexOf = true, bool endIsIndexOf = true, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (@this.IsNullOrEmpty())
                return string.Empty;

            if (start.IsNullOrEmpty())
                return string.Empty;

            if (end.IsNullOrEmpty())
                return string.Empty;

            //开始字符串索引
            int startIndex;

            if (startIsIndexOf)
                startIndex = @this.IndexOf(start, comparisonType);
            else
                startIndex = @this.LastIndexOf(start, comparisonType);

            if (startIndex == -1)
                return string.Empty;

            startIndex += start.Length;

            //结束字符串索引
            int endIndex;

            if (endIsIndexOf)
                endIndex = @this.IndexOf(end, startIndex, comparisonType);
            else
                endIndex = @this.LastIndexOf(end, comparisonType);

            if (endIndex == -1)
                return string.Empty;

            //截取长度
            var length = endIndex - startIndex;

            if (length < 0)
                return string.Empty;

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
        /// 判断是否包含目标字符串，区分大小写，任意一个满足即可
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="strs">目标字符串数组"</param>
        /// <returns></returns>
        public static bool Contains(this string @this, params string[] strs)
        {
            return @this.Contains(MatchType.Any, strs);
        }

        /// <summary>
        /// 判断是否包含目标字符串，区分大小写
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="type">匹配类型</param>
        /// <param name="strs">目标字符串数组"</param>
        /// <returns></returns>
        public static bool Contains(this string @this, MatchType type, params string[] strs)
        {
            if (@this.IsNullOrEmpty())
                return false;

            if (strs.IsNullOrEmpty())
                return false;

            foreach (var item in strs)
            {
                if (type == MatchType.Any)
                {
                    if (item != null && @this.Contains(item))
                        return true;
                }
                else
                {
                    if (item == null || !@this.Contains(item))
                        return false;
                }
            }

            if (type == MatchType.Any)
                return false;

            return true;
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
        #endregion

        #region ContainsIgnoreCase
        /// <summary>
        /// 忽略大小写的字符串包含比较，任意一个满足即可
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="strs">目标字符串数组</param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string @this, params string[] strs)
        {
            return @this.ContainsIgnoreCase(MatchType.Any, strs);
        }

        /// <summary>
        /// 忽略大小写的字符串包含比较
        /// </summary>
        /// <param name="this">源字符串</param>
        /// <param name="type">匹配类型</param>
        /// <param name="strs">目标字符串数组</param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string @this, MatchType type, params string[] strs)
        {
            if (@this.IsNullOrEmpty())
                return false;

            if (strs.IsNullOrEmpty())
                return false;

            foreach (var item in strs)
            {
                if (type == MatchType.Any)
                {
                    if (item.IsNotNull() && @this.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                else
                {
                    if (item.IsNull() || @this.IndexOf(item, StringComparison.OrdinalIgnoreCase) < 0)
                        return false;
                }
            }

            if (type == MatchType.Any)
                return false;

            return true;
        }
        #endregion

        #region Equals
        /// <summary>
        /// 字符串相等比较，判断是否以任意一个待比较字符串相等
        /// </summary>
        /// <param name="this">当前字符串</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <param name="strs">待比较字符串数组</param>
        /// <returns></returns>
        public static bool Equals(this string @this, bool ignoreCase, params string[] strs)
        {
            if (strs.IsNotNullOrEmpty())
            {
                foreach (var item in strs)
                {
                    if (ignoreCase)
                    {
                        if (string.Equals(@this, item, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    else
                    {
                        if (string.Equals(@this, item))
                            return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region EqualIgnoreCase
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
        #endregion

        #region StartsWithIgnoreCase
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
        #endregion

        #region EndsWithIgnoreCase
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
        #endregion
    }
}
