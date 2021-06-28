using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SQLBuilder.Core.Diagnostics;
using SQLBuilder.Core.ElasticApm.Diagnostics;
using SQLBuilder.Core.ElasticApm.Extensions;
using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SQLBuilder.Core
{
    public class Program
    {
        #region Print
        /// <summary>
        /// 打印输出
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="description"></param>
        /// <param name="separator"></param>
        public static void Print<T>(SqlBuilderCore<T> builder, string description = "", string separator = "") where T : class, new()
        {
            if (!string.IsNullOrEmpty(separator))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"--------------------------------[ {separator} ]----------------------------------");
                Console.WriteLine();
            }
            if (!string.IsNullOrEmpty(description))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(description);
            }
            if (builder != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(builder.Sql);
                if (builder.Parameters != null)
                {
                    foreach (KeyValuePair<string, object> item in builder.Parameters)
                    {
                        Console.WriteLine(item.ToString());
                    }
                }
            }
            Console.WriteLine();
        }
        #endregion

        #region Main
        /// <summary>
        /// 主函数
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main(string[] args)
        {
            //SqlBuilderTest();

            //DiagnosticSourceTest();

            var hostBuilder = CreateHostBuilder(args);

            await hostBuilder.RunConsoleAsync();

            Console.ReadLine();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => services.AddHostedService<HostedService>())
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .UseSqlBuilderElasticApm();
        #endregion

        #region SqlBuilderTest
        public static void SqlBuilderTest()
        {
            #region Select
            Print(
                SqlBuilder.Select<UserInfo>(),
                "查询单表所有字段",
                "Select"
            );

            var expr = LinqExtensions.True<UserInfo>();
            expr = expr.And(o => o.Id > 0);
            expr = expr.Or(o => o.Email != "");
            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .Where(expr),
                "查询单表所有字段，linq拼接条件"
            );

            Print(
                SqlBuilder.Select<UserInfo>(u => u.Id),
                "查询单表单个字段"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => u.Id)
                    .WithKey(2, 3),
                "根据主键进行查询"
           );

            Print(
                SqlBuilder.Select<UserInfo>(u => new { u.Id, u.Name }),
                "查询单表多个字段"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => new { u.Id, u.Name })
                    .Top(100),
                "查询单表多个字段，并返回指定TOP数量的数据"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u =>
                        new { u.Id, u.Name }, DatabaseType.MySql)
                    .Top(100),
                "查询单表多个字段，并返回指定TOP数量的数据 MySQL"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => 
                        new { u.Id, u.Name }, DatabaseType.Oracle)
                    .Top(100),
                "查询单表多个字段，并返回指定TOP数量的数据 Oracle"
            );

            Print(
               SqlBuilder
                    .Select<UserInfo>(u => 
                        new { u.Id, u.Name })
                    .Top(100)
                    .Distinct(),
               "查询单表多个字段，DISTINCT"
            );

            Print(
               SqlBuilder
                    .Select<UserInfo>(u =>
                        new { u.Id, u.Name })
                    .Distinct()
                    .Top(100),
               "查询单表多个字段，DISTINCT"
            );

            Print(
                SqlBuilder.Select<UserInfo>(u => new { u.Id, UserName = u.Name }),
                "查询单表多个字段 起别名"
            );

            Print(
                SqlBuilder
                    .Select<Student>(o => 
                        new { o.Id, o.Name })
                    .Where(x =>
                        x.IsEffective.Value && 
                        x.IsOnLine),
                "查询单表，带where bool类型字段"
            );

            Print(
               SqlBuilder
                    .Select<Student>(o =>
                        new { o.Id, o.Name })
                    .Where(x =>
                        !x.IsEffective.Value && 
                        !x.IsOnLine),
               "查询单表，带where bool类型字段"
            );

            Print(
               SqlBuilder
                    .Select<Student>(o =>
                        new { o.Id, o.Name })
                    .Where(x => 
                        x.IsEffective == true &&
                        x.IsOnLine == false),
               "查询单表，带where bool类型字段"
            );

            var entity = new { name = "新用户" };
            Print(
                SqlBuilder
                    .Select<UserInfo>(o =>
                        new { o.Id, o.Name })
                    .Where(u => 
                        u.Name.Equals(entity.name)),
                "查询单表，带where 变量"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(o =>
                        new { o.Id, o.Name })
                    .Where(u => 
                        1 == 1)
                    .AndWhere(u =>
                        u.Name == ""),
                "查询单表，带where 1==1"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u =>
                        u.Id)
                    .Where(u =>
                        u.Name.Like("张三")),
                "查询单表，带where Like条件"
            );

            var name = "张三";
            Print(
                SqlBuilder
                    .Select<UserInfo>(u =>
                        u.Id)
                    .Where(u => 
                        u.Name.NotLike(name)),
                "查询单表，带where Not Like"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => 
                        u.Id)
                    .Where(u => 
                        u.Name.StartsWith(name)),
                "查询单表，带where StartsWith条件"
            );

            Print(
              SqlBuilder
                    .Select<UserInfo>(u => 
                        u.Name)
                    .Where(u => 
                        !"a,b,c".Split(new[] { ',' }).Contains(u.Name)),
              "查询单表，带where contains条件，写法一"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u =>
                        u.Name)
                    .Where(u => 
                        new string[] { "a", "b" }.Contains(u.Name)),
                "查询单表，带where contains条件，写法二"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => 
                        "*")
                    .Where(u => 
                        !new string[] { "a", "b" }.Contains(u.Name)),
                "查询单表，带where contains条件，写法三"
            );

            int[] arrayId = { 1, 2, 3 };
            Print(
               SqlBuilder
                    .Select<UserInfo>(u =>
                        u.Name)
                    .Where(u =>
                        arrayId.Contains(u.Id.Value)),
               "查询单表，带where contains条件，写法四"
            );

            var user = new UserInfo { Name = "a,b,c" };
            Print(
              SqlBuilder
                .Select<UserInfo>(u => 
                    u.Name)
                .Where(u => 
                    user.Name.Split(new[] { ',' }).Contains(u.Name)),
              "查询单表，带where contains条件，写法五"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => 
                        u.Name)
                    .Where(u => 
                        u.Id.In(1, 2, 3)),
                "查询单表，带where in条件，写法一"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => 
                        u.Name)
                    .Where(u => 
                        u.Id.In(arrayId)),
                "查询单表，带where in条件，写法二"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => 
                        u.Name)
                    .Where(u => 
                        !u.Name.In(new string[] { "a", "b" })),
                "查询单表，带where in条件，写法三"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u =>
                        u.Name)
                    .Where(u => 
                        u.Id.NotIn(1, 2, 3)),
                "查询单表，带where not in条件，写法一"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u => 
                        u.Name)
                    .Where(u =>
                        u.Name.Contains("11"))
                    .AndWhere(u => 
                        !string.IsNullOrEmpty(u.Name))
                    .AndWhere(u => 
                        string.IsNullOrEmpty(u.Email)),
                "查询单表，带where Contains、string.IsNullOrEmpty、!string.IsNullOrEmpty"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u =>
                        u.Id)
                    .Where(u =>
                        u.Name == "b" &&
                        (u.Id > 2 && u.Name != null) &&
                        u.Id > int.MinValue &&
                        u.Id < int.MaxValue &&
                        u.Id.In(1, 2, 3) &&
                        u.Name.Like("a") &&
                        u.Name.EndsWith("b") &&
                        u.Name.StartsWith("c") ||
                        u.Id == null),
                "查询单表，带多个where条件"
            );
            #endregion

            #region Join
            Print(
                SqlBuilder
                    .Select<UserInfo, Account>((u, a) =>
                        new { u.Id, a.Name })
                    .Join<Account>((u, a) =>
                        u.Id == a.UserId &&
                        (u.Email == "111" ||
                        u.Email == "222")),
                "多表Join关联查询",
                "Join"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo, Account>((u, a) =>
                        new { u.Id, a.Name })
                    .InnerJoin<Account>((u, a) =>
                        u.Id == a.UserId),
                "多表InnerJoin关联查询"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo, Account>((u, a) => new { u.Id, a.Name })
                    .LeftJoin<Account>((u, a) => u.Id == a.UserId),
                "多表LeftJoin关联查询"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo, Account>((u, a) =>
                        new { u.Id, a.Name })
                    .RightJoin<Account>((u, a) =>
                        u.Id == a.UserId),
                "多表RightJoin关联查询"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo, Account>((u, a) =>
                        new { u.Id, a.Name })
                    .FullJoin<Account>((u, a) =>
                        u.Id == a.UserId),
                "多表FullJoin关联查询"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo, UserInfo, Account, Student, Class, City, Country>((u, t, a, s, d, e, f) =>
                         new
                         {
                             u.Id,
                             UId = t.Id,
                             a.Name,
                             StudentName = s.Name,
                             ClassName = d.Name,
                             e.CityName,
                             CountryName = f.Name
                         })
                    .Join<UserInfo>((x, t) =>
                        x.Id == t.Id)
                    .Join<Account>((x, y) =>
                        x.Id == y.UserId)
                    .LeftJoin<Account, Student>((x, y) =>
                        x.Id == y.AccountId)
                    .RightJoin<Student, Class>((x, y) =>
                        x.Id == y.UserId)
                    .InnerJoin<Class, City>((x, y) =>
                        x.CityId == y.Id)
                    .FullJoin<City, Country>((x, y) =>
                        x.CountryId == y.Id)
                    .Where(x => x.Id != null),
                "多表复杂关联查询"
            );
            #endregion

            #region Page
            Print(
                SqlBuilder
                    .Select<MyStudent>(
                        databaseType: DatabaseType.MySql)
                    .Where(o =>
                        o.Score != null)
                    .AndWhere(o =>
                        o.Name == "")
                    .OrWhere(o =>
                        o.Subject == "")
                    .Page(3, 2, "Id", "select * from student"),
               "查询单表，带多个where条件 分页1",
               "Page"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u =>
                        u.Id)
                    .Where(u =>
                        u.Name == "b" &&
                        (u.Id > 2 &&
                        u.Name != null &&
                        (u.Email == "11" ||
                        u.Email == "22" ||
                        u.Email == "ee")))
                    .Page(10, 1, "Id"),
                "查询单表，带多个where条件 分页2"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>(u =>
                        u.Id)
                    .Where(u =>
                        u.Name == "b" &&
                        (u.Id > 2 &&
                        u.Name != null &&
                        (u.Email == "11" ||
                        u.Email == "22" ||
                        u.Email == "ee")))
                    .Page(10, 1, "Id"),
                "查询单表，带多个where条件 分页3"
            );

            Print(
                SqlBuilder
                    .Select<MyStudent>(
                        databaseType: DatabaseType.MySql)
                    .Where(o =>
                        o.Score != null)
                    .AndWhere(o =>
                        o.Name == "")
                    .OrWhere(o =>
                        o.Subject == "")
                    .Page(3, 2, "Id"),
              "查询单表，带多个where条件 分页4"
            );
            #endregion

            #region GroupBy
            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .Where(o =>
                        o.Name == "张强")
                    .GroupBy(u =>
                        u.Id),
                "GroupBy分组查询 用法1",
                "GroupBy"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .Where(o =>
                        o.Name == "张强")
                    .GroupBy(u =>
                        new { u.Id, u.Email }),
                "GroupBy分组查询 用法2-1"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .Where(o =>
                        o.Name == "张强")
                    .GroupBy(u =>
                        new[] { "Id", "Email" }),
                "GroupBy分组查询 用法2-2"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .Where(o =>
                        o.Name == "张强")
                    .GroupBy(u =>
                        new List<string> { "Id", "Email" }),
                "GroupBy分组查询 用法2-3"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .Where(o =>
                        o.Name == "张强")
                    .GroupBy(u =>
                        "Id,Email".Split(new[] { ',' })),
                "GroupBy分组查询 用法2-4"
            );

            var groupByField = "Id";
            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .Where(o =>
                        o.Name == "张强")
                    .GroupBy(u =>
                        groupByField),
                "GroupBy分组查询 用法2-5"
            );

            var groupFields = "Id,Email".Split(',');
            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .Where(o =>
                        o.Name == "张强")
                    .GroupBy(u =>
                        groupFields),
                "GroupBy分组查询 用法2-6"
            );
            #endregion

            #region OrderBy
            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .OrderBy(u =>
                        new[] { "Id", "Email" },
                        OrderType.Ascending,
                        OrderType.Descending),
                "OrderBy排序 用法1-1",
                "OrderBy"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .OrderBy(u =>
                        new List<string> { "Id", "Email" },
                        OrderType.Ascending,
                        OrderType.Descending),
                "OrderBy排序 用法1-2"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .OrderBy(u =>
                        "Id,Email".Split(new[] { ',' }),
                        OrderType.Ascending,
                        OrderType.Descending),
               "OrderBy排序 用法1-3"
            );

            var orderByFields = "Id,Email".Split(',').ToList();
            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .OrderBy(u =>
                        orderByFields,
                        OrderType.Ascending,
                        OrderType.Descending),
               "OrderBy排序 用法1-4"
            );

            var orderByField = "Id DESC";
            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .OrderBy(u => orderByField),
               "OrderBy排序 用法1-4"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .OrderBy(u => "Id DESC"),
               "OrderBy排序 用法1-5"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .OrderBy(u =>
                        new { u.Id, u.Email },
                        OrderType.Descending,
                        OrderType.Descending),
                "OrderBy排序 用法2"
            );

            Print(
                SqlBuilder
                    .Select<UserInfo>()
                    .OrderBy(u => new { u.Id, u.Email }),
                "OrderBy排序 用法3"
           );
            #endregion

            #region Max
            Print(
                SqlBuilder
                    .Max<UserInfo>(u =>
                        u.Id)
                    .Where(o =>
                        o.Id == 3),
                "返回一列中的最大值。NULL 值不包括在计算中。",
                "Max"
            );
            #endregion

            #region Min
            Print(
                SqlBuilder.Min<UserInfo>(u => u.Id),
                "返回一列中的最小值。NULL 值不包括在计算中。",
                "Min"
            );
            #endregion

            #region Avg
            Print(
                SqlBuilder.Avg<UserInfo>(u => u.Id),
                "返回数值列的平均值。NULL 值不包括在计算中。",
                "Avg"
            );
            #endregion

            #region Count
            Print(
                SqlBuilder.Count<UserInfo>(),
                "返回表中的记录数",
                "Count"
            );

            Print(
                SqlBuilder.Count<UserInfo>(u => u.Id),
                "返回指定列的值的数目（NULL 不计入）"
            );
            #endregion

            #region Sum
            Print(
                SqlBuilder.Sum<UserInfo>(u => u.Id),
                "返回数值列的总数（总额）。",
                "Sum"
            );
            #endregion

            #region Delete
            Print(
                SqlBuilder.Delete<UserInfo>(),
                "全表删除",
                "Delete"
            );
            int id = 3;
            Print(
                SqlBuilder
                    .Delete<UserInfo>()
                    .Where(u => u.Id == id),
                "根据where条件删除指定表记录"
            );

            Print(
                SqlBuilder
                    .Delete<UserInfo>()
                    .WithKey(2, 1),
                "根据主键条件删除指定表记录1"
            );
            Print(
                SqlBuilder
                    .Delete<UserInfo>()
                    .WithKey(new UserInfo { Id = 2, Sex = 1 }),
                "根据主键条件删除指定表记录2"
            );

            Print(
                SqlBuilder
                    .Delete<UserInfo>()
                    .Where(u => u.Id > 1 && u.Id < 3),
                "根据where条件删除指定表记录"
            );
            #endregion

            #region Update
            Print(
                SqlBuilder.Update<UserInfo>(() => new { Name = "", Sex = 1, Email = "123456@qq.com" }),
                "全表更新",
                "Update"
            );

            var obj = new UserInfo { Name = "", Sex = 1, Email = "123456@qq.com", Id = 2 };
            Print(
                SqlBuilder.Update<UserInfo>(() => obj).WithKey(obj),
                "根据主键更新"
            );

            Print(
                SqlBuilder
                    .Update<UserInfo>(() =>
                        new UserInfo { Sex = 1, Email = "123456@qq.com" })
                    .Where(u =>
                        u.Id == 1),
                "根据where条件更新指定表记录 用法1"
            );

            var userInfo2 = new UserInfo
            {
                Name = "张强",
                Sex = 2
            };
            Print(
                SqlBuilder
                    .Update<UserInfo>(() =>
                        userInfo2)
                    .Where(u =>
                        u.Id == 1),
                "根据where条件更新指定表记录 用法2-1"
            );

            Print(
                SqlBuilder
                    .Update<UserInfo>(() =>
                        userInfo2, isEnableNullValue: false)
                    .Where(u =>
                        u.Id == 1),
                "根据where条件更新指定表记录 用法2-2"
            );

            Print(
                SqlBuilder
                    .Update<UserInfo>(() =>
                        new { Sex = 1, Email = "123456@qq.com" }, DatabaseType.MySql)
                    .Where(u =>
                        u.Id == 1),
                "根据where条件更新指定表记录 用法3"
            );

            Print(
                SqlBuilder
                    .Update<Class>(() =>
                        new { UserId = 1, Name = "123456@qq.com" }, DatabaseType.MySql)
                    .Where(u =>
                        u.CityId == 1),
                "根据where条件更新指定表记录 用法4"
            );

            Print(
                SqlBuilder
                    .Update<Class>(() =>
                        new Class { UserId = 1, Name = "123456@qq.com" }, DatabaseType.MySql)
                    .Where(u =>
                        u.CityId == 1),
                "根据where条件更新指定表记录 用法5"
            );

            var classData = new { UserId = 1, Name = "123456@qq.com" };
            Print(
                SqlBuilder
                    .Update<Class>(() =>
                        classData, DatabaseType.MySql)
                    .Where(u =>
                        u.CityId == 1),
                "根据where条件更新指定表记录 用法6"
            );
            #endregion

            #region Insert
            Print(
                SqlBuilder.Insert<UserInfo>(() => new { Name = "张强", Sex = 2 }),
                "插入数据 用法1",
                "Insert"
            );

            Print(
                SqlBuilder.Insert<UserInfo>(() => new UserInfo { Name = "张三", Sex = 2 }),
                "插入数据 用法2"
            );

            var userInfo = new UserInfo
            {
                Name = "张强",
                Sex = 2
            };
            Print(
                SqlBuilder.Insert<UserInfo>(() => userInfo),
                "插入数据 用法3-1"
            );
            Print(
                SqlBuilder.Insert<UserInfo>(() => userInfo, isEnableNullValue: false),
                "插入数据 用法3-2"
            );

            Print(
                SqlBuilder.Insert<UserInfo>(() => new[]
                {
                    new UserInfo { Name = "张三", Sex = 2 },
                    new UserInfo { Name = "张三", Sex = 2 }
                }),
                "插入数据 用法4"
            );

            Print(
                SqlBuilder.Insert<UserInfo>(() => new[]
                {
                    new { Name = "张三", Sex = 2 },
                    new { Name = "张三", Sex = 2 }
                }),
                "插入数据 用法5"
            );

            var array = new List<UserInfo>
            {
                new UserInfo{ Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            }.ToArray();
            Print(
                SqlBuilder.Insert<UserInfo>(() => array),
                "插入数据 用法6"
            );

            var array2 = new List<dynamic>
            {
                new { Name = "张三", Sex = 2 },
                new { Name = "张三", Sex = 2 }
            }.ToArray();
            Print(
                SqlBuilder.Insert<UserInfo>(() => array2),
                "插入数据 用法7"
            );

            Print(
                SqlBuilder.Insert<Class>(() => new[]
                {
                    new Class { CityId = 2, UserId = 2, Name = "张三" },
                    new Class { CityId = 3, UserId = 3, Name = "李四" }
                }),
                "插入数据 用法8"
            );

            Print(
                SqlBuilder.Insert<Class>(() => new[]
                {
                    new { CityId = 2, UserId = 2, Name = "张三" },
                    new { CityId = 3, UserId = 3, Name = "李四" }
                }),
                "插入数据 用法9"
            );

            var data = new[]
            {
                new { CityId = 2, UserId = 2, Name = "张三" },
                new { CityId = 3, UserId = 3, Name = "李四" }
            };
            Print(
                SqlBuilder.Insert<Class>(() => data),
                "插入数据 用法10"
            );

            Print(
              SqlBuilder.Insert<Student>(() => new Student
              {
                  Name = DateTime.Now.ToLongTimeString(),
                  Age = (new Random()).Next(1, 100)
              }),
              "插入数据 用法11"
            );

            var list = new List<UserInfo>
            {
                new UserInfo{ Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            };
            Print(
                SqlBuilder.Insert<UserInfo>(() => list.ToArray()),
               "插入数据 用法12"
            );

            Print(
                SqlBuilder.Insert<UserInfo>(() => list, isEnableNullValue: false),
               "插入数据 用法13"
            );

            Print(
                SqlBuilder.Insert<UserInfo>(() => new List<UserInfo>
                {
                    new UserInfo{ Name = "张三", Sex = 2 },
                    new UserInfo { Name = "张三", Sex = 2 }
                }),
                "插入数据 用法14"
            );

            Print(
                SqlBuilder.Insert<UserInfo>(() => new List<dynamic>
                {
                    new { Name = "张三", Sex = 2 },
                    new { Name = "张三", Sex = 2 }
                }),
                "插入数据 用法15"
            );
            #endregion

            #region GetPrimaryKey
            Print<UserInfo>(null, string.Join(",", SqlBuilder.GetPrimaryKey<UserInfo>()), "GetPrimaryKey");
            #endregion

            #region GetTableName
            Print<UserInfo>(null, SqlBuilder.GetTableName<UserInfo>(), "GetTableName");
            #endregion

            #region SqlIntercept
            Print(
                SqlBuilder.Select<UserInfo>(sqlIntercept: (sql, parameter) =>
                {
                    Console.WriteLine($"执行sql日志：{sql}");
                    //不修改原sql内容
                    return null;
                }),
                "查询单表所有字段",
                "Select"
            );

            Print(
                SqlBuilder.Select<UserInfo>(sqlIntercept: (sql, parameter) =>
                {
                    Console.WriteLine($"执行sql日志：{sql}");
                    //修改原sql
                    return sql.Replace(" AS [t]", "");
                }),
                "查询单表所有字段",
                "Select"
            );
            #endregion
        }
        #endregion

        #region DiagnosticSource 
        public static void DiagnosticSourceTest()
        {
            #region DiagnosticSource
            var diagnosticListener =
                new DiagnosticListener(DiagnosticStrings.DiagnosticListenerName);

            ////订阅方法一
            //DiagnosticListener.AllListeners.Subscribe(new MyObserver<DiagnosticListener>(listener =>
            //{
            //    //判断发布者的名字
            //    if (listener.Name == DiagnosticStrings.DiagnosticListenerName)
            //    {
            //        //获取订阅信息
            //        listener.Subscribe(new MyObserver<KeyValuePair<string, object>>(listenerData =>
            //        {
            //            Console.WriteLine($"监听名称:{listenerData.Key}");
            //            dynamic data = listenerData.Value;
            //            Console.WriteLine(data.Sql);
            //        }));
            //    }
            //}));

            ////订阅方法二
            DiagnosticListener.AllListeners.Subscribe(new MyObserver<DiagnosticListener>(listener =>
            {
                if (listener.Name == DiagnosticStrings.DiagnosticListenerName)
                {
                    //适配订阅
                    listener.SubscribeWithAdapter(new MyDiagnosticListener());
                }
            }));

            //订阅方法三
            //diagnosticListener.SubscribeWithAdapter(new MyDiagnosticListener());
            diagnosticListener.SubscribeWithAdapter(new SqlBuilderDiagnosticListener(null));

            //发送日志诊断消息
            if (diagnosticListener.IsEnabled(DiagnosticStrings.BeforeExecute) &&
                diagnosticListener.IsEnabled(DiagnosticStrings.AfterExecute) &&
                diagnosticListener.IsEnabled(DiagnosticStrings.ErrorExecute))
            {
                var message = new DiagnosticsMessage
                {
                    Sql = "select * from table",
                    Parameters = new Dictionary<string, object>
                    {
                        ["key"] = "123"
                    },
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                diagnosticListener.Write(
                    DiagnosticStrings.BeforeExecute,
                    message);

                message.ElapsedMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message.Timestamp;
                diagnosticListener.Write(
                    DiagnosticStrings.AfterExecute,
                    message);

                message.ElapsedMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message.Timestamp;
                message.Exception = new Exception("测试异常");
                diagnosticListener.Write(
                    DiagnosticStrings.ErrorExecute,
                    message);
            }
            #endregion
        }
        #endregion
    }

    public class MyObserver<T> : IObserver<T>
    {
        private Action<T> _next;
        public MyObserver(Action<T> next)
        {
            _next = next;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value) => _next(value);
    }

    public class MyDiagnosticListener
    {
        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="message">诊断消息</param>
        [DiagnosticName(DiagnosticStrings.BeforeExecute)]
        public void ExecuteBefore(string sql, IDictionary<string, object> parameters)
        {
            Console.WriteLine(sql);
            Console.WriteLine(parameters["key"]);
        }
    }
}