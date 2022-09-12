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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SQLBuilder.Core.Extensions;

namespace SQLBuilder.Core.LoadBalancer
{
    /// <summary>
    /// 带权重负载均衡算法
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
        /// 最小权重
        /// </summary>
        private readonly int _minWeight;
        #endregion

        #region 构造
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="weights"></param>
        public WeightRoundRobin(int[] weights)
        {
            _weights = weights;
            _minWeight = weights.Min();
            _states = new int[weights.Length];
            _times = new int[weights.Length];
        }
        #endregion

        #region 方法
        /// <summary>
        /// 获取最大状态
        /// </summary>
        /// <param name="states"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static int GetMaxState(int[] states, out int index)
        {
            index = 0;
            var state = int.MinValue;

            for (var i = 0; i < states.Length; i++)
            {
                if (states[i] > state)
                {
                    state = states[i];
                    index = i;
                }
            }

            return state;
        }

        /// <summary>
        /// 根据权重选择，并返回该项是第几次选中
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public int Get(out int times)
        {
            //选择状态最大值
            var state = GetMaxState(_states, out var index);

            //如果所有状态都不达标，则集体加盐
            if (state < _minWeight)
            {
                for (var i = 0; i < _weights.Length; i++)
                    _states[i] += _weights[i];

                //重新选择状态最大值
                GetMaxState(_states, out index);
            }

            //已选择，减状态
            _states[index] -= _minWeight;

            times = ++_times[index];

            return index;
        }

        /// <summary>
        /// 根据权重选择
        /// </summary>
        /// <returns></returns>
        public int Get() => Get(out var _);
        #endregion
    }

    /// <summary>
    /// 权重轮询方式
    /// </summary>
    public class WeightRoundRobinLoadBalancer : ILoadBalancer
    {
        private static readonly object _lock = new();

        private static readonly ConcurrentDictionary<string, WeightRoundRobin> _weightRoundRobins = new();

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

            key = $"{key}_{weightList.Join("_")}";

            WeightRoundRobin weightRoundRobin;

            if (_weightRoundRobins.ContainsKey(key))
                weightRoundRobin = _weightRoundRobins[key];
            else
            {
                lock (_lock)
                {
                    weightRoundRobin = _weightRoundRobins.GetOrAdd(key, key =>
                        _weightRoundRobins[key] = new WeightRoundRobin(weightList.ToArray()));
                }
            }

            //获取索引值
            var index = weightRoundRobin.Get();

            return data.ElementAt(index);
        }
    }
}