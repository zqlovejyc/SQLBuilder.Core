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
using System.Reflection;
using System.Text;

#region Refrence Alias
//Table
using CusTableAttribute = SQLBuilder.Core.TableAttribute;
using SysTableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

//Column
using CusColumnAttribute = SQLBuilder.Core.ColumnAttribute;
using SysColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;

//Key
using CusKeyAttribute = SQLBuilder.Core.KeyAttribute;
using SysKeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;
#endregion

namespace SQLBuilder.Core
{
    /// <summary>
    /// SqlPack
    /// </summary>
	public class SqlPack
    {
        #region Private Field
        /// <summary>
        /// tableAlias
        /// </summary>
        private static readonly List<string> tableAlias = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        /// <summary>
        /// dicTableName
        /// </summary>
        private readonly Dictionary<string, string> dicTableName = new Dictionary<string, string>();

        /// <summary>
        /// tableAliasQueue
        /// </summary>
        private Queue<string> tableAliasQueue = new Queue<string>(tableAlias);
        #endregion

        #region Public Property
        /// <summary>
        /// 更新和新增时，是否对null值属性进行sql拼接操作
        /// </summary>
        public bool IsEnableNullValue { get; set; } = true;

        /// <summary>
        /// 是否启用表名和列名格式化
        /// </summary>
        public bool IsEnableFormat { get; set; } = true;

        /// <summary>
        /// 默认T类型
        /// </summary>
        public Type DefaultType { get; set; }

        /// <summary>
        /// IsSingleTable
        /// </summary>
        public bool IsSingleTable { get; set; }

        /// <summary>
        /// SelectFields
        /// </summary>
        public List<string> SelectFields { get; set; }

        /// <summary>
        /// SelectFieldsStr
        /// </summary>
        public string SelectFieldsStr => string.Join(",", this.SelectFields);

        /// <summary>
        /// Length
        /// </summary>
        public int Length => this.Sql.Length;

        /// <summary>
        /// Sql
        /// </summary>
        public StringBuilder Sql { get; set; }

        /// <summary>
        /// DatabaseType
        /// </summary>
        public DatabaseType DatabaseType { get; set; }

        /// <summary>
        /// DbParams
        /// </summary>
        public Dictionary<string, object> DbParams { get; set; }

        /// <summary>
        /// DbParamPrefix
        /// </summary>
        public string DbParamPrefix
        {
            get
            {
                switch (this.DatabaseType)
                {
                    case DatabaseType.SQLite: return "@";
                    case DatabaseType.SQLServer: return "@";
                    case DatabaseType.MySQL: return "?";
                    case DatabaseType.Oracle: return ":";
                    case DatabaseType.PostgreSQL: return ":";
                    default: return "";
                }
            }
        }

        /// <summary>
        /// FormatTempl
        /// </summary>
        public string FormatTempl
        {
            get
            {
                switch (this.DatabaseType)
                {
                    case DatabaseType.SQLite: return "\"{0}\"";
                    case DatabaseType.SQLServer: return "[{0}]";
                    case DatabaseType.MySQL: return "`{0}`";
                    case DatabaseType.Oracle: return "\"{0}\"";
                    case DatabaseType.PostgreSQL: return "\"{0}\"";
                    default: return "{0}";
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// SqlPack
        /// </summary>
        public SqlPack()
        {
            this.DbParams = new Dictionary<string, object>();
            this.Sql = new StringBuilder();
            this.SelectFields = new List<string>();
        }
        #endregion

        #region Public Methods
        #region this[index]
        /// <summary>
        /// this[index]
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>char</returns>
        public char this[int index] => this.Sql[index];
        #endregion

        #region operator +
        /// <summary>
        /// operator +
        /// </summary>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="sql">sql语句</param>
        /// <returns>SqlPack</returns>
        public static SqlPack operator +(SqlPack sqlPack, string sql)
        {
            sqlPack.Sql.Append(sql);
            return sqlPack;
        }
        #endregion

        #region Clear
        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            this.SelectFields.Clear();
            this.Sql.Clear();
            this.DbParams.Clear();
            this.dicTableName.Clear();
            this.tableAliasQueue = new Queue<string>(tableAlias);
        }
        #endregion

        #region AddDbParameter
        /// <summary>
        /// AddDbParameter
        /// </summary>
        /// <param name="parameterValue">参数值</param>
        /// <param name="parameterKey">参数名称</param>
        public void AddDbParameter(object parameterValue, string parameterKey = null)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
            {
                this.Sql.Append("NULL");
            }
            else if (parameterKey.IsNullOrEmpty())
            {
                var name = this.DbParamPrefix + "P" + (this.DbParams.Count + 1);
                this.DbParams.Add(name, parameterValue);
                this.Sql.Append(name);
            }
            else
            {
                var name = this.DbParamPrefix + parameterKey;
                this.DbParams.Add(name, parameterValue);
                this.Sql.Append(name);
            }
        }
        #endregion

        #region SetTableAlias
        /// <summary>
        /// SetTableAlias
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>bool</returns>
        public bool SetTableAlias(string tableName)
        {
            if (!this.dicTableName.Keys.Contains(tableName))
            {
                var tableAlias = this.tableAliasQueue.Dequeue();

                if (this.IsEnableFormat == true)
                    tableAlias = string.Format(this.FormatTempl, tableAlias);

                this.dicTableName.Add(tableName, tableAlias);
                return true;
            }
            return false;
        }
        #endregion

        #region GetTableAlias
        /// <summary>
        /// GetTableAlias
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>string</returns>
        public string GetTableAlias(string tableName)
        {
            if (!this.IsSingleTable && this.dicTableName.Keys.Contains(tableName))
            {
                return this.dicTableName[tableName];
            }
            return string.Empty;
        }
        #endregion

        #region GetFormatName
        /// <summary>
        /// GetFormatName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetFormatName(string name)
        {
            if (
                this.IsEnableFormat == true &&
                name?.StartsWith("[") == false &&
                name?.StartsWith("`") == false &&
                name?.StartsWith("\"") == false)
            {
                name = string.Format(this.FormatTempl, name);
            }
            return name;
        }
        #endregion

        #region GetTableName
        /// <summary>
        /// GetTableName
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>string</returns>
        public string GetTableName(Type type)
        {
            var tableName = this.GetFormatName(type.Name);
            if (type.GetFirstOrDefaultAttribute<CusTableAttribute>() is CusTableAttribute cta)
            {
                if (!cta.Name.IsNullOrEmpty())
                {
                    tableName = this.GetFormatName(cta.Name);
                }
                if (!cta.Schema.IsNullOrEmpty())
                {
                    tableName = $"{this.GetFormatName(cta.Schema)}.{tableName}";
                }
            }
            else if (type.GetFirstOrDefaultAttribute<SysTableAttribute>() is SysTableAttribute sta)
            {
                if (!sta.Name.IsNullOrEmpty())
                {
                    tableName = this.GetFormatName(sta.Name);
                }
                if (!sta.Schema.IsNullOrEmpty())
                {
                    tableName = $"{this.GetFormatName(sta.Schema)}.{tableName}";
                }
            }
            return tableName;
        }
        #endregion

        #region GetColumnName
        /// <summary>
        /// GetFormatColumnName
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public string GetColumnName(string columnName) => this.GetFormatName(columnName);
        #endregion

        #region GetColumnInfo
        /// <summary>
        /// GetColumnInfo
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="member">成员</param>
        /// <returns>Tuple</returns>
        public (string columnName, bool isInsert, bool isUpdate) GetColumnInfo(Type type, MemberInfo member)
        {
            string columnName = null;
            var isInsert = true;
            var isUpdate = true;
            var props = type.GetProperties();
            var isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<CusColumnAttribute>());
            if (!isHaveColumnAttribute)
            {
                isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<SysColumnAttribute>());
            }
            if (isHaveColumnAttribute)
            {
                if (member.GetFirstOrDefaultAttribute<CusColumnAttribute>() is CusColumnAttribute cca)
                {
                    columnName = cca.Name;
                    isInsert = cca.Insert;
                    isUpdate = cca.Update;
                }
                else if (member?.GetFirstOrDefaultAttribute<SysColumnAttribute>() is SysColumnAttribute sca)
                {
                    columnName = sca.Name;
                }
                else
                {
                    var p = props.Where(x => x.Name == member?.Name).FirstOrDefault();
                    if (p?.GetFirstOrDefaultAttribute<CusColumnAttribute>() is CusColumnAttribute cus)
                    {
                        columnName = cus.Name;
                        isInsert = cus.Insert;
                        isUpdate = cus.Update;
                    }
                    else if (p?.GetFirstOrDefaultAttribute<SysColumnAttribute>() is SysColumnAttribute sys)
                    {
                        columnName = sys.Name;
                    }
                }
            }
            columnName = columnName ?? member?.Name;
            //判断列是否是Key
            if (member?.GetFirstOrDefaultAttribute<CusKeyAttribute>() is CusKeyAttribute cka)
            {
                isUpdate = false;
                if (!cka.Name.IsNullOrEmpty() && cka.Name != columnName)
                    columnName = cka.Name;
            }
            else if (member?.GetFirstOrDefaultAttribute<SysKeyAttribute>() is SysKeyAttribute ska)
            {
                isUpdate = false;
            }
            return (this.GetColumnName(columnName), isInsert, isUpdate);
        }
        #endregion

        #region GetPrimaryKey
        /// <summary>
        /// GetPrimaryKey
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>Tuple</returns>
        public List<(string key, string property)> GetPrimaryKey(Type type)
        {
            var result = new List<(string key, string property)>();
            var props = type.GetProperties();
            var isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<CusKeyAttribute>());
            if (!isHaveColumnAttribute)
            {
                isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<SysKeyAttribute>());
            }
            if (isHaveColumnAttribute)
            {
                var properties = props.Where(x => x.ContainsAttribute<CusKeyAttribute>()).ToList();
                if (properties.Count() == 0)
                {
                    properties = props.Where(x => x.ContainsAttribute<SysKeyAttribute>()).ToList();
                }
                foreach (var property in properties)
                {
                    var propertyName = property?.Name;
                    string keyName = null;
                    if (property?.GetFirstOrDefaultAttribute<CusKeyAttribute>() is CusKeyAttribute cka)
                    {
                        keyName = cka.Name ?? propertyName;
                    }
                    else if (property?.GetFirstOrDefaultAttribute<SysKeyAttribute>() is SysKeyAttribute ska)
                    {
                        keyName = propertyName;
                    }
                    result.Add((this.GetColumnName(keyName), propertyName));
                }
            }
            return result;
        }
        #endregion

        #region ToString
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() => this.Sql.ToString();
        #endregion
        #endregion
    }
}