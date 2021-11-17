using System;

namespace SQLBuilder.Core.Diagnostics.Diagnostics
{
    /// <summary>
    /// IObserver泛型实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SqlBuilderObserver<T> : IObserver<T>
    {
        private readonly Action<T> _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        public SqlBuilderObserver(Action<T> next)
        {
            _next = next;
        }

        /// <summary>
        /// 完成
        /// </summary>
        public void OnCompleted()
        {
        }

        /// <summary>
        /// 出错
        /// </summary>
        /// <param name="error"></param>
        public void OnError(Exception error)
        {
        }

        /// <summary>
        /// 下一步
        /// </summary>
        /// <param name="value"></param>
        public void OnNext(T value) => _next(value);
    }
}
