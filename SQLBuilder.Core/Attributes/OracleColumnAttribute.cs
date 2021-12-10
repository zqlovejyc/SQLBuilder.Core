using System;
using Oracle.ManagedDataAccess.Client;

namespace SQLBuilder.Core.Attributes
{
    /// <summary>
    /// Oracle列特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class OracleColumnAttribute : Attribute
    {
        /// <summary>
        /// Oracle数据类型
        /// </summary>
        public OracleDbType DbType { get; set; }
    }
}
