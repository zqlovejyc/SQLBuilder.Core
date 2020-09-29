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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
/****************************
* [Author] 张强
* [Date] 2020-09-29
* [Describe] 轮询负载均衡接口
* **************************/
namespace SQLBuilder.Core.LoadBalancer
{
    /// <summary>
    /// 轮询方式
    /// </summary>
    public class RoundRobinLoadBalancer : ILoadBalancer
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private int _index;

        /// <summary>
        /// 获取数据集合中的一条数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据集合</param>
        /// <param name="weights">权重集合，当前轮询方式，权重无效</param>
        /// <returns></returns>
        public T Get<T>(IEnumerable<T> data, int[] weights = null)
        {
            try
            {
                _lock.Wait();

                if (_index >= data.Count())
                {
                    _index = 0;
                }
                var retval = data.ElementAt(_index);
                _index++;

                return retval;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}