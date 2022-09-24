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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SQLBuilder.Core.Extensions;

namespace SQLBuilder.Core.LoadBalancer
{
    /// <summary>
    /// 原子计数器
    /// </summary>
    public sealed class AtomicCounter
    {
        private int _value;

        /// <summary>
        /// Gets the current value of the counter.
        /// </summary>
        public int Value
        {
            get => Volatile.Read(ref _value);
            set => Volatile.Write(ref _value, value);
        }

        /// <summary>
        /// Atomically increments the counter value by 1.
        /// </summary>
        public int Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// Atomically decrements the counter value by 1.
        /// </summary>
        public int Decrement()
        {
            return Interlocked.Decrement(ref _value);
        }

        /// <summary>
        /// Atomically resets the counter value to 0.
        /// </summary>
        public void Reset()
        {
            Interlocked.Exchange(ref _value, 0);
        }
    }

    /// <summary>
    /// 轮询方式
    /// </summary>
    public class RoundRobinLoadBalancer : ILoadBalancer
    {
        private static readonly ConcurrentDictionary<string, Lazy<AtomicCounter>> _counters = new();

        /// <summary>
        /// 获取数据集合中的一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">唯一标识，用于多数据库情况下的负载均衡</param>
        /// <param name="data">数据集合</param>
        /// <param name="weights">权重集合，当前轮询方式，权重无效</param>
        /// <returns></returns>
        public T Get<T>(string key, IEnumerable<T> data, int[] weights = null)
        {
            if (data.IsNullOrEmpty())
                return default;

            var count = data.Count();

            key = $"{key}_{data.GetHashCode()}";

            var counter = _counters.GetOrAdd(key, key =>
                new Lazy<AtomicCounter>(() => new AtomicCounter())).Value;

            var offset = counter.Increment() - 1;

            var index = (offset & 0x7FFFFFFF) % count;

            return data.ElementAt(index);
        }
    }
}