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
using SQLBuilder.Core.Extensions;

namespace SQLBuilder.Core.LoadBalancer
{
    /// <summary>
    /// 带权重负载均衡算法
    /// <para>设 A、B、C 三个节点的权重分别为：4、2、1，演算步骤如下：</para>
    /// <list type="number">
    ///      <item>{ 4, 2, 1}   A   {-3, 2, 1}</item>
    ///      <item>{ 1, 4, 2}   B   { 1,-3, 2}</item>
    ///      <item>{ 5,-1, 3}   A   {-2,-1, 3}</item>
    ///      <item>{ 2, 1, 4}   C   { 2, 1,-3}</item>
    ///      <item>{ 6, 3,-2}   A   {-1, 3,-2}</item>
    ///      <item>{ 3, 5,-1}   B   { 3,-2,-1}</item>
    ///      <item>{ 7, 0, 0}   A   { 0, 0, 0}</item>
    /// </list>
    /// </summary>
    public class WeightRoundRobin
    {
        #region 字段
        /// <summary>
        /// 状态值
        /// </summary>
        private readonly int[] _states;

        /// <summary>
        /// 次数
        /// </summary>
        private readonly int[] _times;

        /// <summary>
        /// 权重集合
        /// </summary>
        private readonly int[] _weights;

        /// <summary>
        /// 权重和
        /// </summary>
        private readonly int _totalWeight;
        #endregion

        #region 构造
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="weights"></param>
        public WeightRoundRobin(int[] weights)
        {
            if (weights.IsNullOrEmpty())
                throw new ArgumentException($"`{nameof(weights)}` cannot be null or empty.");

            _weights = weights;
            _totalWeight = weights.Sum();
            _states = new int[weights.Length];
            _times = new int[weights.Length];

            Array.Copy(_weights, _states, weights.Length);
        }
        #endregion

        #region 方法
        /// <summary>
        /// 根据权重选择，并返回该项是第几次选中
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public int Get(out int times)
        {
            var index = _states.MaxIndex();

            _states[index] -= _totalWeight;

            for (var i = 0; i < _weights.Length; i++)
            {
                _states[i] += _weights[i];
            }

            times = ++_times[index];

            return index;
        }

        /// <summary>
        /// 根据权重选择
        /// </summary>
        /// <returns></returns>
        public int Get() => Get(out _);
        #endregion
    }

    /// <summary>
    /// 权重轮询方式
    /// </summary>
    public class WeightRoundRobinLoadBalancer : ILoadBalancer
    {
        private static readonly ConcurrentDictionary<string, Lazy<WeightRoundRobin>> _weightRoundRobins = new();

        /// <summary>
        /// 获取数据集合中的一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">唯一标识，用于多数据库情况下的负载均衡</param>
        /// <param name="data">数据集合</param>
        /// <param name="weights">权重集合</param>
        /// <returns></returns>
        public T Get<T>(string key, IEnumerable<T> data, int[] weights = null)
        {
            if (data.IsNullOrEmpty())
                return default;

            var count = data.Count();
            var weightList = new List<int>();

            if (weights == null)
                data.ToList().ForEach(x => weightList.Add(1));
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (i < weights.Length)
                        weightList.Add(weights[i]);
                    else
                        weightList.Add(1);
                }
            }

            key = $"{key}_{data.GetHashCode()}";

            var weightRoundRobin = _weightRoundRobins.GetOrAdd(key,
                key => new Lazy<WeightRoundRobin>(() => new WeightRoundRobin(weightList.ToArray()))).Value;

            //获取索引值
            var index = weightRoundRobin.Get();

            return data.ElementAt(index);
        }
    }
}