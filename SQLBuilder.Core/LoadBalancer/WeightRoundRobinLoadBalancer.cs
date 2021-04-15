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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SQLBuilder.Core.LoadBalancer
{
    /// <summary>
    /// 带权重负载均衡算法
    /// </summary>
    public class WeightRoundRobin
    {
        #region 属性
        /// <summary>
        /// 权重集合
        /// </summary>
        public int[] Weights { get; set; }

        /// <summary>
        /// 最小权重
        /// </summary>
        private readonly int minWeight;

        /// <summary>
        /// 状态值
        /// </summary>
        private readonly int[] _states;

        /// <summary>
        /// 次数
        /// </summary>
        private readonly int[] _times;
        #endregion

        #region 构造
        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="weights"></param>
        public WeightRoundRobin(int[] weights)
        {
            Weights = weights;

            minWeight = weights.Min();

            _states = new int[weights.Length];
            _times = new int[weights.Length];
        }
        #endregion

        #region 方法
        /// <summary>
        /// 获取最大状态
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        private int GetMaxState(int[] ds, out int idx)
        {
            var n = int.MinValue;
            idx = 0;
            for (var i = 0; i < ds.Length; i++)
            {
                if (ds[i] > n)
                {
                    n = ds[i];
                    idx = i;
                }
            }

            return n;
        }

        /// <summary>
        /// 根据权重选择，并返回该项是第几次选中
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public int Get(out int times)
        {
            // 选择状态最大值
            var cur = GetMaxState(_states, out var idx);

            // 如果所有状态都不达标，则集体加盐
            if (cur < minWeight)
            {
                for (var i = 0; i < Weights.Length; i++)
                {
                    _states[i] += Weights[i];
                }

                // 重新选择状态最大值
                cur = GetMaxState(_states, out idx);
            }

            // 已选择，减状态
            _states[idx] -= minWeight;

            times = ++_times[idx];

            return idx;
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
        private readonly SemaphoreSlim _lock = new(1, 1);

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
            try
            {
                _lock.Wait();

                //初始化权重数组
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

                //初始化权重算法
                var weightRoundRobin = _weightRoundRobins.GetOrAdd($"{key}_{weightList.Count}", new WeightRoundRobin(weightList.ToArray()));

                //获取索引值
                var index = weightRoundRobin.Get();

                return data.ElementAt(index);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}