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

using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLBuilder.Core.LoadBalancer
{
    /// <summary>
    /// 随机方式
    /// </summary>
    public class RandomLoadBalancer : ILoadBalancer
    {
        private readonly Random _random;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="seed"></param>
        public RandomLoadBalancer(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        /// <summary>
        /// 获取数据集合中的一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">唯一标识，用于多数据库情况下的负载均衡</param>
        /// <param name="data">数据集合</param>
        /// <param name="weights">权重集合，当前随机方式，权重无效</param>
        /// <returns></returns>
        public T Get<T>(string key, IEnumerable<T> data, int[] weights = null)
        {
            var count = data.Count();

            return count == 0 ? default : data.ElementAt(_random.Next(count));
        }
    }
}