using System;
using Oracle.ManagedDataAccess.Client;

namespace SQLBuilder.Core.Attributes
{
    /// <summary>
    /// Oracle列特性，此特性仅用于Oracle特殊数据库字段类型使用，如：[OracleColumn(DbType = OracleDbType.NClob)]
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
