<p></p>

<p align="center">
<img src="https://zqlovejyc.gitee.io/zqutils-js/Images/SQL.png" height="80"/>
</p>

<div align="center">

[![star](https://gitee.com/zqlovejyc/SQLBuilder.Core/badge/star.svg)](https://gitee.com/zqlovejyc/SQLBuilder.Core/stargazers) [![fork](https://gitee.com/zqlovejyc/SQLBuilder.Core/badge/fork.svg)](https://gitee.com/zqlovejyc/SQLBuilder.Core/members) [![GitHub stars](https://img.shields.io/github/stars/zqlovejyc/SQLBuilder.Core?logo=github)](https://github.com/zqlovejyc/SQLBuilder.Core/stargazers) [![GitHub forks](https://img.shields.io/github/forks/zqlovejyc/SQLBuilder.Core?logo=github)](https://github.com/zqlovejyc/SQLBuilder.Core/network) [![GitHub license](https://img.shields.io/badge/license-Apache2-yellow)](https://github.com/zqlovejyc/SQLBuilder.Core/blob/master/LICENSE) [![nuget](https://img.shields.io/nuget/v/Zq.SQLBuilder.Core.svg?cacheSeconds=10800)](https://www.nuget.org/packages/Zq.SQLBuilder.Core)

</div>

<div align="left">

.NET Standard 2.0、.NET Standard 2.1、.NET 5 版本SQLBuilder，Expression表达式转换为SQL语句，支持SqlServer、MySql、Oracle、Sqlite、PostgreSql；基于Dapper实现了不同数据库对应的数据仓储Repository；

</div>

## 🍟 文档地址

- 单元测试：[https://github.com/zqlovejyc/SQLBuilder.Core/tree/master/SQLBuilder.Core.UnitTest](https://github.com/zqlovejyc/SQLBuilder.Core/tree/master/SQLBuilder.Core.UnitTest)


**目前文档正在逐步完善中。**


## 🌭 开源地址

- Gitee：[https://gitee.com/zqlovejyc/SQLBuilder.Core](https://gitee.com/zqlovejyc/SQLBuilder.Core)
- GitHub：[https://github.com/zqlovejyc/SQLBuilder.Core](https://github.com/zqlovejyc/SQLBuilder.Core)
- Nuget：[https://www.nuget.org/packages/Zq.SQLBuilder.Core](https://www.nuget.org/packages/Zq.SQLBuilder.Core)
- Myget：[https://www.myget.org/feed/zq-myget/package/nuget/Zq.SQLBuilder.Core](https://www.myget.org/feed/zq-myget/package/nuget/Zq.SQLBuilder.Core)

## 🥥 框架扩展包

|                                                                     包类型                                                                      | 名称                                       |                                                                                          版本                                                                                           | 描述                       |
| :---------------------------------------------------------------------------------------------------------------------------------------------: | ------------------------------------------ | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | -------------------------- |
|                   [![nuget](https://shields.io/badge/-Nuget-blue?cacheSeconds=604800)](https://www.nuget.org/packages/Zq.SQLBuilder.Core)                   | Zq.SQLBuilder.Core                                     |                                     [![nuget](https://img.shields.io/nuget/v/Zq.SQLBuilder.Core.svg?cacheSeconds=10800)](https://www.nuget.org/packages/Zq.SQLBuilder.Core)                                     | SQLBuilder.Core 核心包              |
|   [![nuget](https://shields.io/badge/-Nuget-blue?cacheSeconds=604800)](https://www.nuget.org/packages/Zq.SQLBuilder.Core.SkyWalking)   | Zq.SQLBuilder.Core.SkyWalking     |     [![nuget](https://img.shields.io/nuget/v/Zq.SQLBuilder.Core.SkyWalking.svg?cacheSeconds=10800)](https://www.nuget.org/packages/Zq.SQLBuilder.Core.SkyWalking)     | SQLBuilder.Core SkyWalking 扩展包          |

## 🚀 快速入门

- #### 新增

```csharp
//新增
await _repository.InsertAsync(entity);

//批量新增
await _repository.InsertAsync(entities);

//新增
await SqlBuilder
        .Insert<MsdBoxEntity>(() =>
            entity)
        .ExecuteAsync(
            _repository);

```

- #### 删除

```csharp
//删除
await _repository.DeleteAsync(entity);

//批量删除
await _repository.DeleteAsync(entitties);

//条件删除
await _repository.DeleteAsync<MsdBoxEntity>(x => x.Id == "1");
```

- #### 更新

```csharp
//更新
await _repository.UpdateAsync(entity);

//批量更新
await _repository.UpdateAsync(entities);

//条件更新
await _repository.UpdateAsync<MsdBoxEntity>(x => x.Id == "1", () => entity);

//更新
await SqlBuilder
        .Update<MsdBoxEntity>(() =>
            entity,
            DatabaseType.MySql,
            isEnableFormat:true)
        .Where(x =>
            x.Id == "1")
        .ExecuteAsync(
            _repository);
```
- #### 查询

```csharp
//简单查询
await _repository.FindListAsync<MsdBoxEntity>(x => x.Id == "1");

//复杂查询
await SqlBuilder
        .Select<UserInfo, Account>(
            (u, a) => new { u.Id, UserName = "u.Name" })
        .InnerJoin<Account>(
            joinCondition)
        .WhereIf(
            !name.IsNullOrEmpty(),
            x => x.Email != null && (!name.EndsWith("∞") ? x.Name.Contains(name.TrimEnd('∞', '*')) : x.Name == name),
            ref hasWhere)
        .WhereIf(
            !email.IsNullOrEmpty(),
            x => x.Email == email,
            ref hasWhere)
        .ToListAsync(
            _repository);

//分页查询
await SqlBuilder
        .Select<UserInfo, Account>(
            (u, a) => new { u.Id, UserName = "u.Name" })
        .InnerJoin<Account>(
            joinCondition)
        .WhereIf(
            !name.IsNullOrEmpty(),
            x => x.Email != null && (!name.EndsWith("∞") ? x.Name.Contains(name.TrimEnd('∞', '*')) : x.Name == name),
            ref hasWhere)
        .WhereIf(
            !email.IsNullOrEmpty(),
            x => x.Email == email,
            ref hasWhere)
        .ToListAsync(
                _repository.UseMasterOrSlave(false),
                input.OrderField,
                input.Ascending,
                input.PageSize,
                input.PageIndex);


//仓储分页查询
await _repository.FindListAsync(condition, input.OrderField, input.Ascending, input.PageSize, input.PageIndex);
```

### 🌌 IOC注入

根据appsettions.json配置自动注入不同类型数据仓储，支持一主多从配置

```csharp
//注入SQLBuilder仓储
services.AddSQLBuilder(Configuration, "Base", (sql, parameter) =>
{
    //写入文本日志
    if (WebHostEnvironment.IsDevelopment())
    {
        if (parameter is DynamicParameters dynamicParameters)
            _logger.LogInformation($@"SQL语句：{sql}  参数：{dynamicParameters
                .ParameterNames?
                .ToDictionary(k => k, v => dynamicParameters.Get<object>(v))
                .ToJson()}");
        else if (parameter is OracleDynamicParameters oracleDynamicParameters)
            _logger.LogInformation($@"SQL语句：{sql} 参数：{oracleDynamicParameters
                .OracleParameters
                .ToDictionary(k => k.ParameterName, v => v.Value)
                .ToJson()}");
        else
            _logger.LogInformation($"SQL语句：{sql}  参数：{parameter.ToJson()}");
    }

    return null;
});


```

### 🌳 数据库配置

```csharp
//appsettions.json
"ConnectionStrings": {
  "Base": [
    "Oracle",
    "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.100)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=name)));Persist Security Info=True;User ID=test;Password=123;",
    "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.101)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=name)));Persist Security Info=True;User ID=test;Password=123;",
    "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.102)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=name)));Persist Security Info=True;User ID=test;Password=123;",
  ],
  "Cap": [
    "MySql",
    "Server=127.0.0.1;Database=db;Uid=root;Pwd=123456;SslMode=None;"
  ]
}
```


### 📰 事务

```csharp
//方式一
IRepository trans = null;
try
{
    //开启事务
    trans = _repository.BeginTrans();

    //数据库写操作
    await _repository.InsertAsync(entity);

    //提交事务
    trans.Commit();
}
catch (Exception)
{
    //回滚事务
    trans?.Rollback();
    throw;
}

//方式二
bool res = true;
await _repository.ExecuteTransAsync(async dbTran =>
{
    res = (await dbTran.InsertAsync(entity)) > 0;
    res = res && (await dbTran.InsertAsync(objEntity)) > 0;
});
```

### 📯 仓储+切库

```csharp
private readonly Func<string, IRepository> _handler;
private readonly IRepository _repository;

public MyService(Func<string, IRepository> hander)
{
    _handler = hander;

    //默认base数据仓储
    _repository = hander(null);
}

//base仓储
var baseRepository=_handler("Base");

//cap仓储
var capRepository=_handler("Cap");
```

### 🎣 读写分离

```csharp
//方式一
_repository.Master = false;

//方式二
_repository.UseMasterOrSlave(master)
```

## 🍻 贡献代码

`SQLBuilder.Core` 遵循 `Apache-2.0` 开源协议，欢迎大家提交 `PR` 或 `Issue`。
