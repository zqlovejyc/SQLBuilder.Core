namespace SQLBuilder.Core.Repositories
{
    /// <summary>
    /// 用于标识多实例注入，标识相同服务的不同实例
    /// </summary>
    public interface INameService
    {
        /// <summary>
        /// 当前实例名称，需要保持唯一性
        /// </summary>
        string ServiceName { get; set; }
    }
}
