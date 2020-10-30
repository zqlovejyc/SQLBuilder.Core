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
using SQLBuilder.Core.Extensions;
using SQLBuilder.Core.Enums;

#region Refrence Alias
//Table
using CusTableAttribute = SQLBuilder.Core.Attributes.TableAttribute;
using SysTableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

//Column
using CusColumnAttribute = SQLBuilder.Core.Attributes.ColumnAttribute;
using SysColumnAttribute = System.ComponentModel.DataAnnotations.Schema.ColumnAttribute;

//Key
using CusKeyAttribute = SQLBuilder.Core.Attributes.KeyAttribute;
using SysKeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;
#endregion

namespace SQLBuilder.Core.Entry
{
    /// <summary>
    /// SqlWrapper
    /// </summary>
	public class SqlWrapper
    {
        #region Private Field
        /// <summary>
        /// 表别名字典
        /// </summary>
        private readonly Dictionary<string, string> aliasDictionary = new Dictionary<string, string>();
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
        /// 是否单表操作
        /// </summary>
        public bool IsSingleTable { get; set; }

        /// <summary>
        /// 已选择的表字段
        /// </summary>
        public List<string> SelectFields { get; set; }

        /// <summary>
        /// 已选择的表字段拼接字符串
        /// </summary>
        public string SelectFieldsStr => string.Join(",", this.SelectFields);

        /// <summary>
        /// 当前sql长度
        /// </summary>
        public int Length => this.Sql.Length;

        /// <summary>
        /// Sql
        /// </summary>
        public StringBuilder Sql { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DatabaseType { get; set; }

        /// <summary>
        /// 数据库参数
        /// </summary>
        public Dictionary<string, object> DbParameters { get; set; }

        /// <summary>
        /// 数据参数化前缀
        /// </summary>
        public string DbParamPrefix
        {
            get
            {
                return this.DatabaseType switch
                {
                    DatabaseType.Sqlite => "@",
                    DatabaseType.SqlServer => "@",
                    DatabaseType.MySql => "?",
                    DatabaseType.Oracle => ":",
                    DatabaseType.PostgreSql => ":",
                    _ => "",
                };
            }
        }

        /// <summary>
        /// 格式化模板
        /// </summary>
        public string FormatTempl
        {
            get
            {
                return this.DatabaseType switch
                {
                    DatabaseType.Sqlite => "\"{0}\"",
                    DatabaseType.SqlServer => "[{0}]",
                    DatabaseType.MySql => "`{0}`",
                    DatabaseType.Oracle => "\"{0}\"",
                    DatabaseType.PostgreSql => "\"{0}\"",
                    _ => "{0}",
                };
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        public SqlWrapper()
        {
            this.DbParameters = new Dictionary<string, object>();
            this.Sql = new StringBuilder();
            this.SelectFields = new List<string>();
        }
        #endregion

        #region Public Methods
        #region this[index]
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>char</returns>
        public char this[int index] => this.Sql[index];
        #endregion

        #region operator +
        /// <summary>
        /// 操作符
        /// </summary>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="sql">sql语句</param>
        /// <returns>SqlWrapper</returns>
        public static SqlWrapper operator +(SqlWrapper sqlWrapper, string sql)
        {
            sqlWrapper.Sql.Append(sql);
            return sqlWrapper;
        }
        #endregion

        #region Clear
        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            this.SelectFields.Clear();
            this.Sql.Clear();
            this.DbParameters.Clear();
            this.aliasDictionary.Clear();
        }
        #endregion

        #region AddDbParameter
        /// <summary>
        /// 新增格式化参数
        /// </summary>
        /// <param name="parameterValue">参数值</param>
        /// <param name="parameterKey">参数名称</param>
        public void AddDbParameter(object parameterValue, string parameterKey = null)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                this.Sql.Append("NULL");

            else if (parameterKey.IsNullOrEmpty())
            {
                var name = this.DbParamPrefix + "p__" + (this.DbParameters.Count + 1);
                this.DbParameters.Add(name, parameterValue);
                this.Sql.Append(name);
            }
            else
            {
                var name = this.DbParamPrefix + parameterKey;
                this.DbParameters.Add(name, parameterValue);
                this.Sql.Append(name);
            }
        }
        #endregion

        #region SetTableAlias
        /// <summary>
        /// 设置表别名
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableAlias">表别名</param>
        /// <returns>bool</returns>
        public bool SetTableAlias(string tableName, string tableAlias = null)
        {
            if (tableAlias.IsNullOrEmpty())
                tableAlias = "t";

            tableAlias = this.GetFormatName(tableAlias);

            if (!this.aliasDictionary.Keys.Contains(tableAlias))
            {
                this.aliasDictionary.Add(tableAlias, tableName);
                return true;
            }

            return false;
        }
        #endregion

        #region GetTableAlias
        /// <summary>
        /// 获取表别名
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableAlias">表别名</param>
        /// <returns>string</returns>
        public string GetTableAlias(string tableName, string tableAlias = null)
        {
            if (!this.IsSingleTable)
            {
                if (!tableAlias.IsNullOrEmpty())
                {
                    tableAlias = this.GetFormatName(tableAlias);

                    //表别名+表名 同时满足
                    if (aliasDictionary.Keys.Contains(tableAlias) && aliasDictionary[tableAlias] == tableName)
                        return tableAlias;
                }

                //根据表名获取别名
                if (aliasDictionary.Values.Contains(tableName))
                    return aliasDictionary.FirstOrDefault(x => x.Value == tableName).Key;
            }
            return string.Empty;
        }
        #endregion

        #region GetFormatName
        /// <summary>
        /// 获取格式化名称
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
        /// 获取表名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>string</returns>
        public string GetTableName(Type type)
        {
            var tableName = this.GetFormatName(type.Name);
            if (type.GetFirstOrDefaultAttribute<CusTableAttribute>() is CusTableAttribute cta)
            {
                if (!cta.Name.IsNullOrEmpty())
                    tableName = this.GetFormatName(cta.Name);

                if (!cta.Schema.IsNullOrEmpty())
                    tableName = $"{this.GetFormatName(cta.Schema)}.{tableName}";
            }
            else if (type.GetFirstOrDefaultAttribute<SysTableAttribute>() is SysTableAttribute sta)
            {
                if (!sta.Name.IsNullOrEmpty())
                    tableName = this.GetFormatName(sta.Name);

                if (!sta.Schema.IsNullOrEmpty())
                    tableName = $"{this.GetFormatName(sta.Schema)}.{tableName}";
            }
            return tableName;
        }
        #endregion

        #region GetColumnName
        /// <summary>
        /// 获取列名
        /// </summary>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public string GetColumnName(string columnName) => this.GetFormatName(columnName);
        #endregion

        #region GetColumnInfo
        /// <summary>
        /// 获取列信息
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
                isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<SysColumnAttribute>());

            if (isHaveColumnAttribute)
            {
                if (member.GetFirstOrDefaultAttribute<CusColumnAttribute>() is CusColumnAttribute cca)
                {
                    columnName = cca.Name;
                    isInsert = cca.Insert;
                    isUpdate = cca.Update;
                }
                else if (member?.GetFirstOrDefaultAttribute<SysColumnAttribute>() is SysColumnAttribute sca)
                    columnName = sca.Name;
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
                        columnName = sys.Name;
                }
            }

            columnName ??= member?.Name;

            //判断列是否是Key
            if (member?.GetFirstOrDefaultAttribute<CusKeyAttribute>() is CusKeyAttribute cka)
            {
                isUpdate = false;
                if (!cka.Name.IsNullOrEmpty() && cka.Name != columnName)
                    columnName = cka.Name;
            }
            else if (member?.GetFirstOrDefaultAttribute<SysKeyAttribute>() is SysKeyAttribute ska)
                isUpdate = false;

            return (this.GetColumnName(columnName), isInsert, isUpdate);
        }
        #endregion

        #region GetPrimaryKey
        /// <summary>
        /// 获取主键
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>Tuple</returns>
        public List<(string key, string property)> GetPrimaryKey(Type type)
        {
            var result = new List<(string key, string property)>();
            var props = type.GetProperties();

            var isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<CusKeyAttribute>());
            if (!isHaveColumnAttribute)
                isHaveColumnAttribute = props.Any(x => x.ContainsAttribute<SysKeyAttribute>());

            if (isHaveColumnAttribute)
            {
                var properties = props.Where(x => x.ContainsAttribute<CusKeyAttribute>()).ToList();
                if (properties.Count() == 0)
                    properties = props.Where(x => x.ContainsAttribute<SysKeyAttribute>()).ToList();

                foreach (var property in properties)
                {
                    var propertyName = property?.Name;
                    string keyName = null;

                    if (property?.GetFirstOrDefaultAttribute<CusKeyAttribute>() is CusKeyAttribute cka)
                        keyName = cka.Name ?? propertyName;
                    else if (property?.GetFirstOrDefaultAttribute<SysKeyAttribute>() is SysKeyAttribute ska)
                        keyName = propertyName;

                    result.Add((this.GetColumnName(keyName), propertyName));
                }
            }
            return result;
        }
        #endregion

        #region ToString
        /// <summary>
        /// ToString重置
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() => this.Sql.ToString();
        #endregion
        #endregion
    }
}