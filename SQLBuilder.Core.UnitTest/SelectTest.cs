﻿using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using SQLBuilder.Core.UnitTest.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace SQLBuilder.Core.UnitTest
{
    public class SelectTest
    {
        #region Max
        /// <summary>
        /// 求最小值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Max_01()
        {
            var builder = SqlBuilder
                            .Max<UserInfo>(u =>
                                u.Id)
                            .Where(o =>
                                o.Id == 3);

            Assert.Equal("SELECT MAX(Id) FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(3, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 求最小值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Max_02()
        {
            var builder = SqlBuilder
                            .Max<UserInfo>(u =>
                                new { u.Id })
                            .Where(o =>
                                o.Id == 3);

            Assert.Equal("SELECT MAX(Id) FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(3, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 求最小值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Max_03()
        {
            var builder = SqlBuilder
                            .Max<UserInfo>(u =>
                                "Id")
                            .Where(o =>
                                o.Id == 3);

            Assert.Equal("SELECT MAX(Id) FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(3, builder.Parameters["@p__1"].data);
        }
        #endregion

        #region Min
        /// <summary>
        /// 求最小值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Min_01()
        {
            var builder = SqlBuilder.Min<UserInfo>(u => u.Id);

            Assert.Equal("SELECT MIN(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 求最小值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Min_02()
        {
            var builder = SqlBuilder.Min<UserInfo>(u => new { u.Id });

            Assert.Equal("SELECT MIN(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 求最小值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Min_03()
        {
            var builder = SqlBuilder.Min<UserInfo>(u => "Id");

            Assert.Equal("SELECT MIN(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }
        #endregion

        #region Avg
        /// <summary>
        /// 求平均值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Avg_01()
        {
            var builder = SqlBuilder.Avg<UserInfo>(u => u.Id);

            Assert.Equal("SELECT AVG(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 求平均值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Avg_02()
        {
            var builder = SqlBuilder.Avg<UserInfo>(u => new { u.Id });

            Assert.Equal("SELECT AVG(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 求平均值,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Avg_03()
        {
            var builder = SqlBuilder.Avg<UserInfo>(u => "Id");

            Assert.Equal("SELECT AVG(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }
        #endregion

        #region Count
        /// <summary>
        /// 计数1,NULL 值不包括在计算中
        /// </summary>
        [Fact]
        public void Test_Count_01()
        {
            var builder = SqlBuilder.Count<UserInfo>(u => u.Id);

            Assert.Equal("SELECT COUNT(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 计数2
        /// </summary>
        [Fact]
        public void Test_Count_02()
        {
            var builder = SqlBuilder.Count<UserInfo>();

            Assert.Equal("SELECT COUNT(*) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 计数3
        /// </summary>
        [Fact]
        public void Test_Count_03()
        {
            var builder = SqlBuilder.Count<UserInfo>(x => null);

            Assert.Equal("SELECT COUNT(*) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 计数4
        /// </summary>
        [Fact]
        public void Test_Count_04()
        {
            var builder = SqlBuilder.Count<UserInfo>(x => new { });

            Assert.Equal("SELECT COUNT(*) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 计数5
        /// </summary>
        [Fact]
        public void Test_Count_05()
        {
            var builder = SqlBuilder.Count<UserInfo>(x => "DISTINCT Id");

            Assert.Equal("SELECT COUNT(DISTINCT Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 计数6
        /// </summary>
        [Fact]
        public void Test_Count_06()
        {
            var builder = SqlBuilder.Count<UserInfo>(x => new { x.Id });

            Assert.Equal("SELECT COUNT(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 计数7
        /// </summary>
        [Fact]
        public void Test_Count_07()
        {
            var builder = SqlBuilder
                            .Count<UserInfo, Account>((x, y) =>
                                new { x.Id })
                            .InnerJoin<Account>((x, y) =>
                                x.Id == y.UserId)
                            .Where(x =>
                                !x.Name.IsNullOrEmpty());

            Assert.Equal("SELECT COUNT(x.Id) FROM Base_UserInfo AS x INNER JOIN Base_Account AS y ON x.Id = y.UserId WHERE (x.Name IS NOT NULL AND x.Name <> '')", builder.Sql);
            Assert.Empty(builder.Parameters);
        }
        #endregion

        #region Sum
        /// <summary>
        /// 求和
        /// </summary>
        [Fact]
        public void Test_Sum_01()
        {
            var builder = SqlBuilder.Sum<UserInfo>(u => u.Id);

            Assert.Equal("SELECT SUM(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 求和
        /// </summary>
        [Fact]
        public void Test_Sum_02()
        {
            var builder = SqlBuilder.Sum<UserInfo>(u => new { u.Id });

            Assert.Equal("SELECT SUM(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 求和
        /// </summary>
        [Fact]
        public void Test_Sum_03()
        {
            var builder = SqlBuilder.Sum<UserInfo>(u => "Id");

            Assert.Equal("SELECT SUM(Id) FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }
        #endregion

        #region Group By
        /// <summary>
        /// 分组1
        /// </summary>
        [Fact]
        public void Test_GroupBy_01()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                u.Id);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组2
        /// </summary>
        [Fact]
        public void Test_GroupBy_02()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                new { u.Id, u.Email });

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id,Email", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组3
        /// </summary>
        [Fact]
        public void Test_GroupBy_03()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                new[] { "Id", "Email" });

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id,Email", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组4
        /// </summary>
        [Fact]
        public void Test_GroupBy_04()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                new List<string> { "Id", "Email" });

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id,Email", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组5
        /// </summary>
        [Fact]
        public void Test_GroupBy_05()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                "Id,Email".Split(new[] { ',' }));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id,Email", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组6
        /// </summary>
        [Fact]
        public void Test_GroupBy_06()
        {
            var groupFields = "Id,Email".Split(',');

            var builder = SqlBuilder
                                .Select<UserInfo>()
                                .Where(o =>
                                    o.Name == "张强")
                                .GroupBy(u =>
                                    groupFields);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id,Email", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组7
        /// </summary>
        [Fact]
        public void Test_GroupBy_07()
        {
            var groupFields = "Id,Email".Split(',').ToList();

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                groupFields);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id,Email", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组8
        /// </summary>
        [Fact]
        public void Test_GroupBy_08()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Student>(
                                (x, y) => new { x.Email, y.Name }, isEnableFormat: true)
                            .InnerJoin<Student>(
                                (x, y) => x.Id == y.UserId)
                            .Where(
                                o => o.Name == "张强")
                            .GroupBy<Student>(
                                (x, y) => new { x.Email, y.Name });

            Assert.Equal("SELECT [x].[Email],[y].[Name] FROM [Base_UserInfo] AS [x] INNER JOIN [Base_Student] AS [y] ON [x].[Id] = [y].[UserId] WHERE [x].[Name] = @p__1 GROUP BY [x].[Email],[y].[Name]", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组9
        /// </summary>
        [Fact]
        public void Test_GroupBy_09()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                "Id");

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组10
        /// </summary>
        [Fact]
        public void Test_GroupBy_10()
        {
            var groupField = "Id";

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                groupField);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 分组11
        /// </summary>
        [Fact]
        public void Test_GroupBy_11()
        {
            var user = new UserInfo { Name = "Id" };

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(o =>
                                o.Name == "张强")
                            .GroupBy(u =>
                                user.Name);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name = @p__1 GROUP BY Id", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);
        }
        #endregion

        #region Having
        /// <summary>
        /// Having01
        /// </summary>
        [Fact]
        public void Test_Having_01()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(x =>
                                new { x.Name, NameCount = x.Name.Count<int>() })
                            .Where(o =>
                                o.Id > 1)
                            .GroupBy(u =>
                                u.Name)
                            .Having(x =>
                                x.Name.Count<int>() > 1)
                            .OrderBy(x =>
                                x.Name);

            Assert.Equal("SELECT Name,COUNT(Name) AS NameCount FROM Base_UserInfo WHERE Id > @p__1 GROUP BY Name HAVING COUNT(Name) > @p__2 ORDER BY Name", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// Having02
        /// </summary>
        [Fact]
        public void Test_Having_02()
        {
            var count = 3;

            var builder = SqlBuilder
                            .Select<UserInfo>(x =>
                                new { x.Name, NameCount = x.Name.Count<int>() })
                            .Where(o =>
                                o.Id > 1)
                            .GroupBy(u =>
                                u.Name)
                            .Having(x =>
                                x.Name.Count<int>() > count)
                            .OrderBy(x =>
                                x.Name);

            Assert.Equal("SELECT Name,COUNT(Name) AS NameCount FROM Base_UserInfo WHERE Id > @p__1 GROUP BY Name HAVING COUNT(Name) > @p__2 ORDER BY Name", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// Having03
        /// </summary>
        [Fact]
        public void Test_Having_03()
        {
            var list = new List<int> { 1, 2, 3 };

            var builder = SqlBuilder
                            .Select<UserInfo>(x =>
                                new { x.Name, NameCount = x.Name.Count<int>() })
                            .Where(o =>
                                o.Id > 1)
                            .GroupBy(u =>
                                u.Name)
                            .Having(x =>
                                x.Name.Count<int>() > list.Last())
                            .OrderBy(x =>
                                x.Name);

            Assert.Equal("SELECT Name,COUNT(Name) AS NameCount FROM Base_UserInfo WHERE Id > @p__1 GROUP BY Name HAVING COUNT(Name) > @p__2 ORDER BY Name", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// Having04
        /// </summary>
        [Fact]
        public void Test_Having_04()
        {
            var input = new[]
            {
                new { Id = 1 },
                new { Id = 2 }
            };

            var builder = SqlBuilder
                            .Select<Teacher, Class>((x, y) =>
                                new { x.ClassId })
                            .InnerJoin<Class>((x, y) =>
                                x.ClassId == y.Id)
                            .Where<Class>((x, y) =>
                                input.Select(k => k.Id).Contains(x.ClassId))
                            .GroupBy<Class>((x, y) =>
                                x.ClassId)
                            .Having(x =>
                                x.ClassId.Count<int>() == input.Length);

            Assert.Equal("SELECT x.ClassId FROM Base_Teacher AS x INNER JOIN Base_Class AS y ON x.ClassId = y.Id WHERE x.ClassId IN (@p__1,@p__2) GROUP BY x.ClassId HAVING COUNT(x.ClassId) = @p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// Having05
        /// </summary>
        [Fact]
        public void Test_Having_05()
        {
            var input = new[]
            {
                new { Id = 1 },
                new { Id = 2 }
            };

            var builder = SqlBuilder
                            .Select<Teacher, Class>((x, y) =>
                                new { x.ClassId })
                            .InnerJoin<Class>((x, y) =>
                                x.ClassId == y.Id && y.Name.IsNullOrEmpty() == false)
                            .Where<Class>((x, y) =>
                                !(x.ClassId == 0))
                            .GroupBy<Class>((x, y) =>
                                x.ClassId)
                            .Having(x =>
                                x.ClassId.Count<int>() == input.Length &&
                                (x.ClassId == 1 || x.ClassId == 2) &&
                                !(x.ClassId > 10) &&
                                (x.ClassId > 10) == false);

            Assert.Equal("SELECT x.ClassId FROM Base_Teacher AS x INNER JOIN Base_Class AS y ON x.ClassId = y.Id AND (y.Name IS NOT NULL AND y.Name <> '') WHERE x.ClassId <> @p__1 GROUP BY x.ClassId HAVING (COUNT(x.ClassId) = @p__2 AND (x.ClassId = @p__3 OR x.ClassId = @p__4)) AND x.ClassId <= @p__5 AND x.ClassId <= @p__6", builder.Sql);
            Assert.Equal(6, builder.Parameters.Count);

            Assert.True(builder.Parameters["@p__2"].type.IsDbType);
            Assert.Equal(DbType.Int32, builder.Parameters["@p__2"].type.DbType);
        }
        #endregion

        #region Order By
        /// <summary>
        /// 排序1
        /// </summary>
        [Fact]
        public void Test_OrderBy_01()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                new { u.Id, u.Email },
                                OrderType.Ascending,
                                OrderType.Descending);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC,Email DESC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序2
        /// </summary>
        [Fact]
        public void Test_OrderBy_02()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                new { u.Id, u.Email },
                                OrderType.Descending);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id DESC,Email ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序3
        /// </summary>
        [Fact]
        public void Test_OrderBy_03()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                new { u.Id, u.Email });

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC,Email ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序4
        /// </summary>
        [Fact]
        public void Test_OrderBy_04()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                new[] { "Id", "Email" });

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC,Email ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序5
        /// </summary>
        [Fact]
        public void Test_OrderBy_05()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                new List<string> { "Id", "Email" });

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC,Email ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序6
        /// </summary>
        [Fact]
        public void Test_OrderBy_06()
        {
            var orderFields = "Id,Email";

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                orderFields.Split(new[] { ',' }).ToList());

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC,Email ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序7
        /// </summary>
        [Fact]
        public void Test_OrderBy_07()
        {
            var orderFields = "Id,Email".Split(',').ToList();

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                orderFields);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC,Email ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序8
        /// </summary>
        [Fact]
        public void Test_OrderBy_08()
        {
            var orderField = "Id";

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                orderField);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序9
        /// </summary>
        [Fact]
        public void Test_OrderBy_09()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                "Id DESC");

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id DESC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序10
        /// </summary>
        [Fact]
        public void Test_OrderBy_10()
        {
            var orderFields = "Id,Email".Split(',');

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                orderFields);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC,Email ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序11
        /// </summary>
        [Fact]
        public void Test_OrderBy_11()
        {
            var orderFields = "Id,Email".Split(',');

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                orderFields.ToList(),
                                OrderType.Descending,
                                OrderType.Descending);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id DESC,Email DESC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序12
        /// </summary>
        [Fact]
        public void Test_OrderBy_12()
        {
            var orderField = "Id";

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                orderField,
                                OrderType.Descending);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id DESC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序13
        /// </summary>
        [Fact]
        public void Test_OrderBy_13()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                u.Id,
                                OrderType.Descending);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id DESC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序14
        /// </summary>
        [Fact]
        public void Test_OrderBy_14()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                u.Id,
                                OrderType.Ascending);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id ASC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序15
        /// </summary>
        [Fact]
        public void Test_OrderBy_15()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                u.Id);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序16
        /// </summary>
        [Fact]
        public void Test_OrderBy_16()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                "Id DESC");

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Id DESC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 排序17
        /// </summary>
        [Fact]
        public void Test_OrderBy_17()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Student>(
                                (x, y) => new { x.Email, y.Name }, isEnableFormat: true)
                            .InnerJoin<Student>(
                                (x, y) => x.Id == y.UserId)
                            .Where(
                                o => o.Name == "张强")
                            .GroupBy<Student>(
                                (x, y) => new { x.Email, y.Name })
                            .OrderBy<Student>(
                                (x, y) => y.Name);

            Assert.Equal("SELECT [x].[Email],[y].[Name] FROM [Base_UserInfo] AS [x] INNER JOIN [Base_Student] AS [y] ON [x].[Id] = [y].[UserId] WHERE [x].[Name] = @p__1 GROUP BY [x].[Email],[y].[Name] ORDER BY [y].[Name]", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张强", builder.Parameters["@p__1"].data);

            Assert.True(builder.Parameters["@p__1"].type.IsDbType);
            Assert.Equal(DbType.String, builder.Parameters["@p__1"].type.DbType);
        }

        /// <summary>
        /// 排序18
        /// </summary>
        [Fact]
        public void Test_OrderBy_18()
        {
            var user = new UserInfo { Name = "Name" };

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .OrderBy(u =>
                                user.Name,
                                OrderType.Descending);

            Assert.Equal("SELECT * FROM Base_UserInfo ORDER BY Name DESC", builder.Sql);
            Assert.Empty(builder.Parameters);
        }
        #endregion

        #region Top
        /// <summary>
        /// Top01
        /// </summary>
        [Fact]
        public void Test_Select_Top_01()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new { u.Id, u.Name }, isEnableFormat: true)
                            .Top(100);

            Assert.Equal("SELECT TOP 100 [Id],[Name] FROM [Base_UserInfo]", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// Top02
        /// </summary>
        [Fact]
        public void Test_Select_Top_02()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new { u.Id, u.Name }, DatabaseType.MySql, isEnableFormat: true)
                            .Top(100);

            Assert.Equal("SELECT `Id`,`Name` FROM `Base_UserInfo` LIMIT 100 OFFSET 0", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// Top03
        /// </summary>
        [Fact]
        public void Test_Select_Top_03()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new { u.Id, u.Name }, DatabaseType.Oracle)
                            .OrderBy(x =>
                                x.Name)
                            .Top(100);

            Assert.Equal("SELECT * FROM (SELECT Id,Name FROM Base_UserInfo ORDER BY Name) T WHERE ROWNUM <= 100", builder.Sql);
            Assert.Empty(builder.Parameters);
        }
        #endregion

        #region Distinct
        /// <summary>
        /// Distinct01
        /// </summary>
        [Fact]
        public void Test_Select_Distinct_01()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new { u.Id, u.Name })
                            .Top(100)
                            .Distinct();

            Assert.Equal("SELECT DISTINCT TOP 100 Id,Name FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// Distinct02
        /// </summary>
        [Fact]
        public void Test_Select_Distinct_02()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new { u.Id, u.Name })
                            .Distinct()
                            .Top(100);

            Assert.Equal("SELECT DISTINCT TOP 100 Id,Name FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }
        #endregion

        #region Select
        /// <summary>
        /// 查询1
        /// </summary>
        [Fact]
        public void Test_Select_01()
        {
            var builder = SqlBuilder.Select<UserInfo>();

            Assert.Equal("SELECT * FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询2
        /// </summary>
        [Fact]
        public void Test_Select_02()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Id);

            Assert.Equal("SELECT Id FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询3
        /// </summary>
        [Fact]
        public void Test_Select_03()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, u.Name });

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询4
        /// </summary>
        [Fact]
        public void Test_Select_04()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, UserName = u.Name });

            Assert.Equal("SELECT Id,Name AS UserName FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询5
        /// </summary>
        [Fact]
        public void Test_Select_05()
        {
            var entity = new { name = "新用户" };

            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                u.Name == entity.name);

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Name = @p__1", builder.Sql);
            Assert.Equal("新用户", builder.Parameters["@p__1"].data);

            Assert.True(builder.Parameters["@p__1"].type.IsDbType);
            Assert.Equal(DbType.String, builder.Parameters["@p__1"].type.DbType);
        }

        /// <summary>
        /// 查询6
        /// </summary>
        [Fact]
        public void Test_Select_06()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                u.Name.Like("张三"));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE Name LIKE '%' + @p__1 + '%'", builder.Sql);
            Assert.Equal("张三", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询7
        /// </summary>
        [Fact]
        public void Test_Select_07()
        {
            var name = "张三";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                u.Name.NotLike(name));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE Name NOT LIKE '%' + @p__1 + '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张三", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询8
        /// </summary>
        [Fact]
        public void Test_Select_08()
        {
            var name = "张三";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                u.Name.StartsWith(name));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE Name LIKE @p__1 + '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张三", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询9
        /// </summary>
        [Fact]
        public void Test_Select_09()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Id.In(1, 2, 3));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Id IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询10
        /// </summary>
        [Fact]
        public void Test_Select_10()
        {
            int[] aryId = { 1, 2, 3 };

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Id.In(aryId));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Id IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询11
        /// </summary>
        [Fact]
        public void Test_Select_11()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Name.In(new string[] { "a", "b" }));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Name IN (@p__1,@p__2)", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询12
        /// </summary>
        [Fact]
        public void Test_Select_12()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Id.NotIn(1, 2, 3));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Id NOT IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);

            Assert.True(builder.Parameters["@p__1"].type.IsDbType);
            Assert.True(builder.Parameters["@p__2"].type.IsDbType);
            Assert.True(builder.Parameters["@p__3"].type.IsDbType);

            Assert.Equal(DbType.Int64, builder.Parameters["@p__1"].type.DbType);
            Assert.Equal(DbType.Int64, builder.Parameters["@p__2"].type.DbType);
            Assert.Equal(DbType.Int64, builder.Parameters["@p__3"].type.DbType);
        }

        /// <summary>
        /// 查询13
        /// </summary>
        [Fact]
        public void Test_Select_13()
        {
            var builder = SqlBuilder
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
                                u.Id == null);

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE (((((((Name = @p__1 AND (Id > @p__2 AND Name IS NOT NULL)) AND Id > @p__3) AND Id < @p__4) AND Id IN (@p__5,@p__6,@p__7)) AND Name LIKE '%' + @p__8 + '%') AND Name LIKE '%' + @p__9) AND Name LIKE @p__10 + '%') OR Id IS NULL", builder.Sql);
            Assert.Equal(10, builder.Parameters.Count);

            Assert.True(builder.Parameters["@p__1"].type.IsDbType);
            Assert.Equal(DbType.String, builder.Parameters["@p__1"].type.DbType);

            Assert.True(builder.Parameters["@p__5"].type.IsDbType);
            Assert.Equal(DbType.Int64, builder.Parameters["@p__5"].type.DbType);

            Assert.True(builder.Parameters["@p__10"].type.IsDbType);
            Assert.Equal(DbType.String, builder.Parameters["@p__10"].type.DbType);
        }

        /// <summary>
        /// 查询14
        /// </summary>
        [Fact]
        public void Test_Select_14()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { u.Id, a.Name })
                            .Join<Account>((u, a) =>
                                u.Id == a.UserId &&
                                (u.Email == "111" ||
                                u.Email == "222"));

            Assert.Equal("SELECT u.Id,a.Name FROM Base_UserInfo AS u JOIN Base_Account AS a ON u.Id = a.UserId AND (u.Email = @p__1 OR u.Email = @p__2)", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询15
        /// </summary>
        [Fact]
        public void Test_Select_15()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { u.Id, a.Name })
                            .InnerJoin<Account>((u, a) =>
                                u.Id == a.UserId);

            Assert.Equal("SELECT u.Id,a.Name FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询16
        /// </summary>
        [Fact]
        public void Test_Select_16()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { u.Id, a.Name })
                            .LeftJoin<Account>((u, a) =>
                                u.Id == a.UserId);

            Assert.Equal("SELECT u.Id,a.Name FROM Base_UserInfo AS u LEFT JOIN Base_Account AS a ON u.Id = a.UserId", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询17
        /// </summary>
        [Fact]
        public void Test_Select_17()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { u.Id, a.Name })
                            .RightJoin<Account>((u, a) =>
                                u.Id == a.UserId);

            Assert.Equal("SELECT u.Id,a.Name FROM Base_UserInfo AS u RIGHT JOIN Base_Account AS a ON u.Id = a.UserId", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询18
        /// </summary>
        [Fact]
        public void Test_Select_18()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { u.Id, a.Name })
                            .FullJoin<Account>((u, a) =>
                                u.Id == a.UserId);

            Assert.Equal("SELECT u.Id,a.Name FROM Base_UserInfo AS u FULL JOIN Base_Account AS a ON u.Id = a.UserId", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询19
        /// </summary>
        [Fact]
        public void Test_Select_19()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, UserInfo, Account, Student, Class, City, Country>((u, t, a, s, d, e, f) =>
                                new { u.Id, UId = t.Id, a.Name, StudentName = s.Name, ClassName = d.Name, e.CityName, CountryName = f.Name })
                            .Join<UserInfo>((x, t) =>
                                x.Id == t.Id) //注意此处单表多次Join所以要指明具体表别名，否则都会读取第一个表别名
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
                            .Where(x =>
                                x.Id != null);

            Assert.Equal("SELECT u.Id,t.Id AS UId,a.Name,s.Name AS StudentName,d.Name AS ClassName,e.City_Name AS CityName,f.Name AS CountryName FROM Base_UserInfo AS u JOIN Base_UserInfo AS t ON u.Id = t.Id JOIN Base_Account AS a ON u.Id = a.UserId LEFT JOIN Base_Student AS s ON a.Id = s.AccountId RIGHT JOIN Base_Class AS d ON s.Id = d.UserId INNER JOIN Base_City AS e ON d.CityId = e.Id FULL JOIN Base_Country AS f ON e.CountryId = f.Country_Id WHERE u.Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询20
        /// </summary>
        [Fact]
        public void Test_Select_20()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name },
                                DatabaseType.MySql,
                                isEnableFormat: true)
                            .Where(u =>
                                1 == 1)
                            .AndWhere(u =>
                                u.Name == "");

            Assert.Equal("SELECT `Id`,`Name` FROM `Base_UserInfo` WHERE (`Name` = ?p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("", builder.Parameters["?p__1"].data);

            Assert.True(builder.Parameters["?p__1"].type.IsDbType);
            Assert.Equal(DbType.String, builder.Parameters["?p__1"].type.DbType);
        }

        /// <summary>
        /// 查询21
        /// </summary>
        [Fact]
        public void Test_Select_21()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Name.Contains("11"))
                            .AndWhere(u =>
                                !string.IsNullOrEmpty(u.Name))
                            .AndWhere(u =>
                                string.IsNullOrEmpty(u.Email));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Name LIKE '%' + @p__1 + '%' AND ((Name IS NOT NULL AND Name <> '')) AND ((Email IS NULL OR Email = ''))", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("11", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询22
        /// </summary>
        [Fact]
        public void Test_Select_22()
        {
            var expr = LinqExtensions.True<UserInfo>();
            expr = expr.And(o => o.Id > 0);
            expr = expr.Or(o => o.Email != "");

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(expr);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Id > @p__1 OR Email <> @p__2", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);

            Assert.True(builder.Parameters["@p__2"].type.IsDbType);
            Assert.True(builder.Parameters["@p__2"].type.IsFixedLength);
            Assert.Equal(20, builder.Parameters["@p__2"].type.FixedLength);
            Assert.Equal(DbType.AnsiStringFixedLength, builder.Parameters["@p__2"].type.DbType);
        }

        /// <summary>
        /// 查询23
        /// </summary>
        [Fact]
        public void Test_Select_23()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .WithKey(2);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(2, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询24
        /// </summary>
        [Fact]
        public void Test_Select_24()
        {
            var expr = LinqExtensions.True<UserInfo>();

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(expr);

            Assert.Equal("SELECT * FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询25
        /// </summary>
        [Fact]
        public void Test_Select_25()
        {
            var expr = LinqExtensions.False<UserInfo>();

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(expr);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE  1 = 0 ", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询26
        /// </summary>
        [Fact]
        public void Test_Select_26()
        {
            var entity = new UserInfo { Name = "新用户" };

            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                u.Name == entity.Name);

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Name = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("新用户", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询27
        /// </summary>
        [Fact]
        public void Test_Select_27()
        {
            SqlBuilderCore<UserInfo> builder = null;

            var list = new List<UserInfo>
            {
                new UserInfo { Name = "新用户" }
            };

            list.ForEach(_ =>
            {
                builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                u.Name == _.Name);
            });

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Name = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("新用户", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询28
        /// </summary>
        [Fact]
        public void Test_Select_28()
        {
            SqlBuilderCore<UserInfo> builder = null;

            var list = new List<UserInfo>
            {
                new UserInfo { Id = 2 }
            };

            list.ForEach(_ =>
            {
                builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                u.Id.Equals(_.Id));
            });

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(2, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询29
        /// </summary>
        [Fact]
        public void Test_Select_29()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                !u.Name.Equals(null));

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Name IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询30
        /// </summary>
        [Fact]
        public void Test_Select_30()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                !u.Name.Equals(null) == true);
            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Name IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询31
        /// </summary>
        [Fact]
        public void Test_Select_31()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                u.Name.Equals(null) == false);

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Name IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询32
        /// </summary>
        [Fact]
        public void Test_Select_32()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                u.Name.Equals(null) == true);

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Name IS NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询33
        /// </summary>
        [Fact]
        public void Test_Select_33()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Name.Contains("11"))
                            .AndWhere(u =>
                                !string.IsNullOrEmpty(u.Name) == false)
                            .AndWhere(u =>
                                string.IsNullOrEmpty(u.Email) == true);

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Name LIKE '%' + @p__1 + '%' AND ((Name IS NULL OR Name = '')) AND ((Email IS NULL OR Email = ''))", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("11", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询34
        /// </summary>
        [Fact]
        public void Test_Select_34()
        {
            SqlBuilderCore<UserInfo> builder = null;

            var list = new List<UserInfo>
            {
                new UserInfo { Id = 2 }
            };

            list.ForEach(_ =>
            {
                builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                u.Id.Equals(_.Id) == false);
            });

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Id <> @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(2, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询35
        /// </summary>
        [Fact]
        public void Test_Select_35()
        {
            SqlBuilderCore<UserInfo> builder = null;

            var list = new List<UserInfo>
            {
                new UserInfo { Id = 2 }
            };

            list.ForEach(_ =>
            {
                builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o.Id, o.Name })
                            .Where(u =>
                                !u.Id.Equals(_.Id) == false);
            });

            Assert.Equal("SELECT Id,Name FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(2, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询36
        /// </summary>
        [Fact]
        public void Test_Select_36()
        {
            var expr = LinqExtensions.True<UserInfo>();
            expr = expr.And(o => o.Id > 0 == false);

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(expr);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Id <= @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(0, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询37
        /// </summary>
        [Fact]
        public void Test_Select_37()
        {
            var expr = LinqExtensions.True<UserInfo>();
            expr = expr.And(o => o.Id >= 0 == false);

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(expr);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Id < @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(0, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询38
        /// </summary>
        [Fact]
        public void Test_Select_38()
        {
            var expr = LinqExtensions.True<UserInfo>();
            expr = expr.And(o => o.Id == null == false);

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(expr);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询39
        /// </summary>
        [Fact]
        public void Test_Select_39()
        {
            var expr = LinqExtensions.True<UserInfo>();
            expr = expr.And(o => !(o.Id > 0 && o.Id < 5));

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(expr);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Id <= @p__1 OR Id >= @p__2", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询40
        /// </summary>
        [Fact]
        public void Test_Select_40()
        {
            var builder = SqlBuilder
                            .Select<City3>()
                            .Select(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                o.Id > 0);

            Assert.Equal("SELECT Id,City_Name AS CityName,Age,Address FROM Base_City3 WHERE Id > @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(0, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询41
        /// </summary>
        [Fact]
        public void Test_Select_41()
        {
            var builder = SqlBuilder
                            .Select<City3>(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                o.CityName.ToUpper() == "郑州");

            Assert.Equal("SELECT Id,City_Name AS CityName,Age,Address FROM Base_City3 WHERE UPPER(City_Name) = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("郑州", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询42
        /// </summary>
        [Fact]
        public void Test_Select_42()
        {
            var builder = SqlBuilder
                            .Select<City3>(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                o.CityName.ToLower() == "郑州");

            Assert.Equal("SELECT Id,City_Name AS CityName,Age,Address FROM Base_City3 WHERE LOWER(City_Name) = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("郑州", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询43
        /// </summary>
        [Fact]
        public void Test_Select_43()
        {
            var builder = SqlBuilder
                            .Select<City3>(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                o.CityName.Trim() == "郑州");

            Assert.Equal("SELECT Id,City_Name AS CityName,Age,Address FROM Base_City3 WHERE LTRIM(RTRIM(City_Name)) = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("郑州", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询44
        /// </summary>
        [Fact]
        public void Test_Select_44()
        {
            var builder = SqlBuilder
                            .Select<City3>(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                o.CityName.TrimStart() == "郑州");

            Assert.Equal("SELECT Id,City_Name AS CityName,Age,Address FROM Base_City3 WHERE LTRIM(City_Name) = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("郑州", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询45
        /// </summary>
        [Fact]
        public void Test_Select_45()
        {
            var builder = SqlBuilder
                            .Select<City3>(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                o.CityName.TrimEnd() == "郑州");

            Assert.Equal("SELECT Id,City_Name AS CityName,Age,Address FROM Base_City3 WHERE RTRIM(City_Name) = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("郑州", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询46
        /// </summary>
        [Fact]
        public void Test_Select_46()
        {
            var builder = SqlBuilder
                            .Select<City3>(
                                databaseType: DatabaseType.MySql,
                                isEnableFormat: true)
                            .Select(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                !string.IsNullOrEmpty(o.CityName) &&
                                o.CityName.Trim() == "郑州".Trim());

            Assert.Equal("SELECT `Id`,`City_Name` AS `CityName`,`Age`,`Address` FROM `Base_City3` WHERE (`City_Name` IS NOT NULL AND `City_Name` <> '') AND TRIM(`City_Name`) = TRIM(?p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("郑州", builder.Parameters["?p__1"].data);
        }

        /// <summary>
        /// 查询47
        /// </summary>
        [Fact]
        public void Test_Select_47()
        {
            var builder = SqlBuilder
                            .Select<City3>(
                                databaseType: DatabaseType.MySql)
                            .Select(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                o.CityName.Trim().Contains("郑州".Trim()));

            Assert.Equal("SELECT Id,City_Name AS CityName,Age,Address FROM Base_City3 WHERE TRIM(City_Name) LIKE CONCAT('%',TRIM(?p__1),'%')", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("郑州", builder.Parameters["?p__1"].data);
        }

        /// <summary>
        /// 查询48
        /// </summary>
        [Fact]
        public void Test_Select_48()
        {
            var builder = SqlBuilder
                            .Select<City3>(
                                databaseType: DatabaseType.Sqlite,
                                isEnableFormat: true)
                            .Select(o =>
                                new { o.Id, o.CityName, o.Age, o.Address })
                            .Where(o =>
                                o.CityName.Trim().Contains("郑州".Trim()));

            Assert.Equal("SELECT \"Id\",\"City_Name\" AS \"CityName\",\"Age\",\"Address\" FROM \"Base_City3\" WHERE TRIM(\"City_Name\") LIKE '%' || TRIM(@p__1) || '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("郑州", builder.Parameters["@p__1"].data);

            Assert.True(builder.Parameters["@p__1"].type.IsDbType);
            Assert.Equal(DbType.AnsiString, builder.Parameters["@p__1"].type.DbType);
        }

        /// <summary>
        /// 查询49
        /// </summary>
        [Fact]
        public void Test_Select_49()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                new[] { "a", "b", "c" }.Contains(x.Name));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询50
        /// </summary>
        [Fact]
        public void Test_Select_50()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                !new[] { "a", "b", "c" }.Contains(x.Name));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name NOT IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询51
        /// </summary>
        [Fact]
        public void Test_Select_51()
        {
            var array = new[] { "a", "b", "c" };

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                !array.Contains(x.Name));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name NOT IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询52
        /// </summary>
        [Fact]
        public void Test_Select_52()
        {
            var array = new[] { "a", "b", "c" };

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                !x.Name.In(array));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name NOT IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询53
        /// </summary>
        [Fact]
        public void Test_Select_53()
        {
            var array = new[] { "a", "b", "c" };

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                !x.Name.NotIn(array));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询54
        /// </summary>
        [Fact]
        public void Test_Select_54()
        {
            var array = new[] { "a", "b", "c" };

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                !!array.Contains(x.Name));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询55
        /// </summary>
        [Fact]
        public void Test_Select_55()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>()
                            .Where(x =>
                                x.IsEffective.Value);

            Assert.Equal("SELECT * FROM student WHERE IsEffective = 1", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询56
        /// </summary>
        [Fact]
        public void Test_Select_56()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>()
                            .Where(x =>
                                x.IsEffective == true);

            Assert.Equal("SELECT * FROM student WHERE IsEffective = 1", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询57
        /// </summary>
        [Fact]
        public void Test_Select_57()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>()
                            .Where(x =>
                                x.IsEffective == false);

            Assert.Equal("SELECT * FROM student WHERE IsEffective = 0", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询58
        /// </summary>
        [Fact]
        public void Test_Select_58()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>()
                            .Where(x =>
                                x.IsEffective == true &&
                                x.IsOnLine);

            Assert.Equal("SELECT * FROM student WHERE IsEffective = 1 AND IsOnLine = 1", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询59
        /// </summary>
        [Fact]
        public void Test_Select_59()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>()
                            .Where(x =>
                                x.IsEffective == true &&
                                x.IsOnLine == true);

            Assert.Equal("SELECT * FROM student WHERE IsEffective = 1 AND IsOnLine = 1", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询60
        /// </summary>
        [Fact]
        public void Test_Select_60()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>()
                            .Where(x =>
                                !x.IsEffective.Value &&
                                !x.IsOnLine);

            Assert.Equal("SELECT * FROM student WHERE IsEffective <> 1 AND IsOnLine <> 1", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询61
        /// </summary>
        [Fact]
        public void Test_Select_61()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>()
                            .Where(x =>
                                !x.IsEffective == true &&
                                !x.IsOnLine == true);

            Assert.Equal("SELECT * FROM student WHERE IsEffective <> 1 AND IsOnLine <> 1", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询62
        /// </summary>
        [Fact]
        public void Test_Select_62()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account, Student, Class, City, Country>((u, a, s, d, e, f) =>
                                new { u, a.Name, StudentName = s.Name, ClassName = d.Name, e.CityName, CountryName = f.Name })
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
                            .Where(u =>
                                u.Id != null);

            Assert.Equal("SELECT u.*,a.Name,s.Name AS StudentName,d.Name AS ClassName,e.City_Name AS CityName,f.Name AS CountryName FROM Base_UserInfo AS u JOIN Base_Account AS a ON u.Id = a.UserId LEFT JOIN Base_Student AS s ON a.Id = s.AccountId RIGHT JOIN Base_Class AS d ON s.Id = d.UserId INNER JOIN Base_City AS e ON d.CityId = e.Id FULL JOIN Base_Country AS f ON e.CountryId = f.Country_Id WHERE u.Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询63
        /// </summary>
        [Fact]
        public void Test_Select_63()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                o, DatabaseType.MySql, isEnableFormat: true)
                            .Where(u =>
                                1 == 1)
                            .AndWhere(u =>
                                u.Name == "");

            Assert.Equal("SELECT * FROM `Base_UserInfo` WHERE (`Name` = ?p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("", builder.Parameters["?p__1"].data);
        }

        /// <summary>
        /// 查询64
        /// </summary>
        [Fact]
        public void Test_Select_64()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { o }, DatabaseType.MySql)
                            .Where(u =>
                                1 == 1)
                            .AndWhere(u =>
                                u.Name == "");

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE (Name = ?p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("", builder.Parameters["?p__1"].data);
        }

        /// <summary>
        /// 查询65
        /// </summary>
        [Fact]
        public void Test_Select_65()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                null, DatabaseType.MySql)
                            .Where(u =>
                                1 == 1)
                            .AndWhere(u =>
                                u.Name == "");

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE (Name = ?p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("", builder.Parameters["?p__1"].data);
        }

        /// <summary>
        /// 查询66
        /// </summary>
        [Fact]
        public void Test_Select_66()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                "*", DatabaseType.MySql)
                            .Where(u =>
                                1 == 1)
                            .AndWhere(u =>
                                u.Name == "");

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE (Name = ?p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("", builder.Parameters["?p__1"].data);
        }

        /// <summary>
        /// 查询67
        /// </summary>
        [Fact]
        public void Test_Select_67()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { u })
                            .Join<Account>((u, a) =>
                                u.Id == a.UserId &&
                                (u.Email == "111" ||
                                u.Email == "222"));

            Assert.Equal("SELECT u.* FROM Base_UserInfo AS u JOIN Base_Account AS a ON u.Id = a.UserId AND (u.Email = @p__1 OR u.Email = @p__2)", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询68
        /// </summary>
        [Fact]
        public void Test_Select_68()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                "u.*")
                            .Join<Account>((u, a) =>
                                u.Id == a.UserId &&
                                (u.Email == "111" ||
                                u.Email == "222"));

            Assert.Equal("SELECT u.* FROM Base_UserInfo AS u JOIN Base_Account AS a ON u.Id = a.UserId AND (u.Email = @p__1 OR u.Email = @p__2)", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询69
        /// </summary>
        [Fact]
        public void Test_Select_69()
        {
            var list = new[] { "a", "b", "c" }.ToList();

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                list.Contains(x.Name));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询70
        /// </summary>
        [Fact]
        public void Test_Select_70()
        {
            var list = new[] { "a", "b", "c" }.Distinct();

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                list.Contains(x.Name));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询71
        /// </summary>
        [Fact]
        public void Test_Select_71()
        {
            var name = "test";

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                x.Name.Contains(name));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Name LIKE '%' + @p__1 + '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("test", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询72
        /// </summary>
        [Fact]
        public void Test_Select_72()
        {
            var list = new[] { 1, 2, 3 }.ToList();

            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .Where(x =>
                                list.Contains(x.Id.Value));

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE Id IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询73
        /// </summary>
        [Fact]
        public void Test_Select_73()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Id > 1000 ||
                                (u.Id < 10 &&
                                u.Name.Equals("张三")));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Id > @p__1 OR (Id < @p__2 AND Name = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询74
        /// </summary>
        [Fact]
        public void Test_Select_74()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                (u.Id < 10 &&
                                u.Name.Equals("张三")) ||
                                u.Id > 1000);

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE (Id < @p__1 AND Name = @p__2) OR Id > @p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询75
        /// </summary>
        [Fact]
        public void Test_Select_75()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Id.Equals(1000) ||
                                (u.Id < 10 &&
                                u.Name.Equals("张三")));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Id = @p__1 OR (Id < @p__2 AND Name = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询76
        /// </summary>
        [Fact]
        public void Test_Select_76()
        {
            var list = new[] { new { id = 1 }, new { id = 2 } };

            var id = "100";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                list.Select(k => k.id).Contains(u.Id.Value) &&
                                u.Id == int.Parse(id) ||
                                (u.Id < 10 &&
                                u.Name.Equals("张三")));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE (Id IN (@p__1,@p__2) AND Id = @p__3) OR (Id < @p__4 AND Name = @p__5)", builder.Sql);
            Assert.Equal(5, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询77
        /// </summary>
        [Fact]
        public void Test_Select_77()
        {
            var id = "100";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Id == int.Parse(id) ||
                                (u.Id < 10 &&
                                u.Name.Equals(Const.Name)));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Id = @p__1 OR (Id < @p__2 AND Name = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询78
        /// </summary>
        [Fact]
        public void Test_Select_78()
        {
            var id = "100";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Name)
                            .Where(u =>
                                u.Id == int.Parse(id) ||
                                (u.Id < 10 &&
                                u.Name == Const.Name));

            Assert.Equal("SELECT Name FROM Base_UserInfo WHERE Id = @p__1 OR (Id < @p__2 AND Name = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询79
        /// </summary>
        [Fact]
        public void Test_Select_79()
        {
            //Where条件拼接
            var whereCondition = LinqExtensions.True<UserInfo>();
            whereCondition = whereCondition.And(x => x.Email == "123");

            //Join条件拼接
            var joinCondition = LinqExtensions.True<UserInfo, Account>();
            joinCondition = joinCondition.And((u, a) => !a.Name.IsNullOrEmpty() && u.Id == a.UserId);
            joinCondition = joinCondition.And((u, a) => u.Id == 1 && !(a.UserId == 2));
            joinCondition = joinCondition.And((u, a) => a.Id == 1);

            //sql构建
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { u.Id, a.Name })
                            .InnerJoin<Account>(joinCondition)
                            .Where(whereCondition);

            Assert.Equal("SELECT u.Id,a.Name FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON (a.Name IS NOT NULL AND a.Name <> '') AND u.Id = a.UserId AND u.Id = @p__1 AND a.UserId <> @p__2 AND a.Id = @p__3 WHERE u.Email = @p__4", builder.Sql);
            Assert.Equal(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询80
        /// </summary>
        [Fact]
        public void Test_Select_80()
        {
            //Where条件拼接
            var whereCondition = LinqExtensions.True<UserInfo>();
            whereCondition = whereCondition.And(x => x.Email == "123");

            //Join条件拼接
            var name = "";
            var joinCondition = LinqExtensions
                                    .True<UserInfo, Account>()
                                    .And((x, y) =>
                                        x.Id == y.UserId)
                                    .WhereIf(
                                        !name.IsNullOrEmpty(),
                                        (x, y) => x.Name.Contains(name));

            //sql构建
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { u.Id, a.Name })
                            .InnerJoin<Account>(joinCondition)
                            .Where(whereCondition);

            Assert.Equal("SELECT u.Id,a.Name FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId WHERE u.Email = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("123", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询81
        /// </summary>
        [Fact]
        public void Test_Select_81()
        {
            //Join条件拼接
            var name = "123";
            var joinCondition = LinqExtensions
                                    .True<UserInfo, Account>()
                                    .And((x, y) =>
                                        x.Id == y.UserId)
                                    .WhereIf(
                                        !name.IsNullOrEmpty(),
                                        (x, y) => x.Name.Contains(name));

            //sql构建
            var hasWhere = false;
            var email = "123@qq.com";
            var builder = SqlBuilder
                            .Select<UserInfo, Account>(
                                (u, a) => new { u.Id, a.Name })
                            .InnerJoin<Account>(
                                joinCondition)
                            .WhereIf(
                                !name.IsNullOrEmpty(),
                                x => x.Name.Contains(name),
                                ref hasWhere)
                            .WhereIf(
                                !email.IsNullOrEmpty(),
                                x => x.Email == email,
                                ref hasWhere);

            Assert.Equal("SELECT u.Id,a.Name FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId AND u.Name LIKE '%' + @p__1 + '%' WHERE (u.Name LIKE '%' + @p__2 + '%') AND (u.Email = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询82
        /// </summary>
        [Fact]
        public void Test_Select_82()
        {
            //Join条件拼接
            var name = "123";
            var joinCondition = LinqExtensions
                                    .True<UserInfo, Account>()
                                    .And((x, y) =>
                                        x.Id == y.UserId)
                                    .WhereIf(
                                        !name.IsNullOrEmpty(),
                                        (x, y) => x.Name.Contains(name));

            //sql构建
            var hasWhere = false;
            var email = "123@qq.com";
            var builder = SqlBuilder
                            .Select<UserInfo, Account>(
                                (u, a) => new { u.Id, UserName = "u.Name" })
                            .InnerJoin<Account>(
                                joinCondition)
                            .WhereIf(
                                !name.IsNullOrEmpty(),
                                x => x.Name.Contains(name),
                                ref hasWhere)
                            .WhereIf(
                                !email.IsNullOrEmpty(),
                                x => x.Email == email,
                                ref hasWhere);

            Assert.Equal("SELECT u.Id,u.Name AS UserName FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId AND u.Name LIKE '%' + @p__1 + '%' WHERE (u.Name LIKE '%' + @p__2 + '%') AND (u.Email = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询83，linq写法，注意目前仅支持单表查询
        /// </summary>
        [Fact]
        public void Test_Select_83()
        {
            var query = from a in new SqlBuilderCore<UserInfo>(DatabaseType.SqlServer, false)
                        where a.Id == 1
                        orderby new { a.Id, a.Email } ascending
                        select new
                        {
                            a.Name,
                            Address = a.Email
                        };

            Assert.Equal("SELECT Name,Email AS Address FROM Base_UserInfo WHERE Id = @p__1 ORDER BY Id ASC,Email ASC", query.Sql);
            Assert.Single(query.Parameters);
            Assert.Equal(1, query.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询84，linq写法，注意目前仅支持单表查询
        /// </summary>
        [Fact]
        public void Test_Select_84()
        {
            var query = from a in SqlBuilder.Select<UserInfo>()
                        where a.Id == 1
                        orderby new { a.Id, a.Email } descending
                        select new
                        {
                            a.Name,
                            Address = a.Email
                        };

            Assert.Equal("SELECT Name,Email AS Address FROM Base_UserInfo WHERE Id = @p__1 ORDER BY Id DESC,Email DESC", query.Sql);
            Assert.Single(query.Parameters);
            Assert.Equal(1, query.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询85，linq写法，注意目前仅支持单表查询
        /// </summary>
        [Fact]
        public void Test_Select_85()
        {
            var query = from a in SqlBuilder.Select<UserInfo>()
                        where a.Id != null && a.Name.Contains("1")
                        group a by a.Id into g
                        orderby g.Id descending
                        select new
                        {
                            g.Id
                        };

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE Id IS NOT NULL AND Name LIKE '%' + @p__1 + '%' GROUP BY Id ORDER BY Id DESC", query.Sql);
            Assert.Single(query.Parameters);
            Assert.Equal("1", query.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询86
        /// </summary>
        [Fact]
        public void Test_Select_86()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                a)
                            .InnerJoin<Account>((u, a) =>
                                u.Id == a.UserId);

            Assert.Equal("SELECT a.* FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询87
        /// </summary>
        [Fact]
        public void Test_Select_87()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                null)
                            .InnerJoin<Account>((u, a) =>
                                u.Id == a.UserId);

            Assert.Equal("SELECT * FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询88
        /// </summary>
        [Fact]
        public void Test_Select_88()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((u, a) =>
                                new { })
                            .InnerJoin<Account>((u, a) =>
                                u.Id == a.UserId);

            Assert.Equal("SELECT * FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询89
        /// </summary>
        [Fact]
        public void Test_Select_89()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(o =>
                                new { }, DatabaseType.MySql, isEnableFormat: true)
                            .Where(u =>
                                1 == 1)
                            .AndWhere(u =>
                                u.Name == "");

            Assert.Equal("SELECT * FROM `Base_UserInfo` WHERE (`Name` = ?p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("", builder.Parameters["?p__1"].data);
        }

        /// <summary>
        /// 查询90
        /// </summary>
        [Fact]
        public void Test_Select_90()
        {
            var expr = LinqExtensions
                            .True<Student>()
                            .WhereIf(false, x => x.UserId == 1);

            var builder = SqlBuilder
                            .Select<UserInfo, Student>((x, y) =>
                                x)
                            .LeftJoin<Student>((x, y) =>
                                x.Id == y.UserId)
                            .Where(expr);

            Assert.Equal("SELECT x.* FROM Base_UserInfo AS x LEFT JOIN Base_Student AS y ON x.Id = y.UserId", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询91
        /// </summary>
        [Fact]
        public void Test_Select_91()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account, Student, Class, City, Country>((u, a, s, d, e, f) =>
                                new { u, a.Name, StudentName = s.Name, ClassName = d.Name, e.CityName, CountryName = f.Name })
                            .Join<Account>((x, y) =>
                                x.Id == y.UserId)
                            .LeftJoin<Account, Student>((x, y) =>
                                x.Id == y.AccountId)
                            .RightJoin<Class, Student>((x, y) =>
                                y.Id == x.UserId)
                            .InnerJoin<Class, City>((x, y) =>
                                x.CityId == y.Id)
                            .FullJoin<City, Country>((x, y) =>
                                x.CountryId == y.Id)
                            .Where(u =>
                                u.Id != null);

            Assert.Equal("SELECT u.*,a.Name,s.Name AS StudentName,d.Name AS ClassName,e.City_Name AS CityName,f.Name AS CountryName FROM Base_UserInfo AS u JOIN Base_Account AS a ON u.Id = a.UserId LEFT JOIN Base_Student AS s ON a.Id = s.AccountId RIGHT JOIN Base_Class AS d ON s.Id = d.UserId INNER JOIN Base_City AS e ON d.CityId = e.Id FULL JOIN Base_Country AS f ON e.CountryId = f.Country_Id WHERE u.Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询92
        /// </summary>
        [Fact]
        public void Test_Select_92()
        {
            var field = "Name";

            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, UserName = $"{field}" });

            Assert.Equal("SELECT Id,Name AS UserName FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询93
        /// </summary>
        [Fact]
        public void Test_Select_93()
        {
            //Join条件拼接
            var name = "123∞";
            var joinCondition = LinqExtensions
                                    .True<UserInfo, Account>()
                                    .And((x, y) =>
                                        x.Id == y.UserId)
                                    .WhereIf(
                                        !name.IsNullOrEmpty(),
                                        (x, y) => name.EndsWith("∞") ? x.Name.Contains(name.Trim('∞')) : x.Name == name);

            //sql构建
            var hasWhere = false;
            var email = "123@qq.com";
            var builder = SqlBuilder
                            .Select<UserInfo, Account>(
                                (u, a) => new { u.Id, UserName = "u.Name" })
                            .InnerJoin<Account>(
                                joinCondition)
                            .WhereIf(
                                !name.IsNullOrEmpty(),
                                x => name.EndsWith("∞") ? x.Name.Contains(name.TrimEnd('∞', '*')) : x.Name == name,
                                ref hasWhere)
                            .WhereIf(
                                !email.IsNullOrEmpty(),
                                x => x.Email == email,
                                ref hasWhere);

            Assert.Equal("SELECT u.Id,u.Name AS UserName FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId AND u.Name LIKE '%' + @p__1 + '%' WHERE (u.Name LIKE '%' + @p__2 + '%') AND (u.Email = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询94
        /// </summary>
        [Fact]
        public void Test_Select_94()
        {
            //Join条件拼接
            var name = "123[∞]";
            var joinCondition = LinqExtensions
                                    .True<UserInfo, Account>()
                                    .And((x, y) =>
                                        x.Id == y.UserId)
                                    .WhereIf(
                                        !name.IsNullOrEmpty(),
                                        (x, y) => name.EndsWith("∞") ? x.Name.Contains(name.Trim('∞')) : x.Name == name);

            //sql构建
            var hasWhere = false;
            var email = "123@qq.com";
            var builder = SqlBuilder
                            .Select<UserInfo, Account>(
                                (u, a) => new { u.Id, UserName = "u.Name" })
                            .InnerJoin<Account>(
                                joinCondition)
                            .WhereIf(
                                !name.IsNullOrEmpty(),
                                x => name.EndsWith("[∞]") ? x.Name.Contains(name.Replace("[∞]", "")) : x.Name == name,
                                ref hasWhere)
                            .WhereIf(
                                !email.IsNullOrEmpty(),
                                x => x.Email == email,
                                ref hasWhere);

            Assert.Equal("SELECT u.Id,u.Name AS UserName FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId AND u.Name = @p__1 WHERE (u.Name LIKE '%' + @p__2 + '%') AND (u.Email = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询95
        /// </summary>
        [Fact]
        public void Test_Select_95()
        {
            Func<string[], string> @delegate = x => $"ks.{x[0]}{x[1]}{x[2]} WITH(NOLOCK)";

            var builder = SqlBuilder
                            .Select<UserInfo, Account, Student, Class, City, Country>((u, a, s, d, e, f) =>
                                new { u, a.Name, StudentName = s.Name, ClassName = d.Name, e.CityName, CountryName = f.Name },
                                tableNameFunc: @delegate)
                            .Join<Account>((x, y) =>
                                x.Id == y.UserId,
                                @delegate)
                            .LeftJoin<Account, Student>((x, y) =>
                                x.Id == y.AccountId,
                                @delegate)
                            .RightJoin<Class, Student>((x, y) =>
                                y.Id == x.UserId,
                                @delegate)
                            .InnerJoin<Class, City>((x, y) =>
                                x.CityId == y.Id,
                                @delegate)
                            .FullJoin<City, Country>((x, y) =>
                                x.CountryId == y.Id,
                                @delegate)
                            .Where(u =>
                                u.Id != null);

            Assert.Equal("SELECT u.*,a.Name,s.Name AS StudentName,d.Name AS ClassName,e.City_Name AS CityName,f.Name AS CountryName FROM ks.Base_UserInfo AS u WITH(NOLOCK) JOIN ks.Base_Account AS a WITH(NOLOCK) ON u.Id = a.UserId LEFT JOIN ks.Base_Student AS s WITH(NOLOCK) ON a.Id = s.AccountId RIGHT JOIN ks.Base_Class AS d WITH(NOLOCK) ON s.Id = d.UserId INNER JOIN ks.Base_City AS e WITH(NOLOCK) ON d.CityId = e.Id FULL JOIN ks.Base_Country AS f WITH(NOLOCK) ON e.CountryId = f.Country_Id WHERE u.Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询96
        /// </summary>
        [Fact]
        public void Test_Select_96()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, UserName = $"Name" });

            Assert.Equal("SELECT Id,Name AS UserName FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询97
        /// </summary>
        [Fact]
        public void Test_Select_97()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new { u.Id, UserName = $"Name" })
                            .Where(x =>
                                new HashSet<int> { 1, 2, 3 }.Contains(x.Id.Value));

            Assert.Equal("SELECT Id,Name AS UserName FROM Base_UserInfo WHERE Id IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询98
        /// </summary>
        [Fact]
        public void Test_Select_98()
        {
            var email = "123@qq.com";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new UserInfo
                                {
                                    Id = u.Id,
                                    Name = $"'张三'",
                                    Email = $"'{email}'"
                                },
                                isEnableFormat: true);

            Assert.Equal("SELECT [Id],'张三' AS [Name],'123@qq.com' AS [Email] FROM [Base_UserInfo]", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询99
        /// </summary>
        [Fact]
        public void Test_Select_99()
        {
            var email = "123@qq.com";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new UserInfo
                                {
                                    Id = u.Id,
                                    Name = $"'张三'",
                                    Email = $"'{email}'"
                                });

            Assert.Equal("SELECT Id,'张三' AS Name,'123@qq.com' AS Email FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询100
        /// </summary>
        [Fact]
        public void Test_Select_100()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new UserInfo { });

            Assert.Equal("SELECT * FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询101
        /// </summary>
        [Fact]
        public void Test_Select_101()
        {
            //Join条件拼接
            var name = "123∞";
            var joinCondition = LinqExtensions
                                    .True<UserInfo, Account>()
                                    .And((x, y) =>
                                        x.Id == y.UserId)
                                    .WhereIf(
                                        !name.IsNullOrEmpty(),
                                        (x, y) => name.EndsWith("∞") ? x.Name.Contains(name.Trim('∞')) : x.Name == name);

            //sql构建
            var hasWhere = false;
            var email = "123@qq.com";
            var builder = SqlBuilder
                            .Select<UserInfo, Account>(
                                (u, a) => new { u.Id, UserName = "u.Name" })
                            .InnerJoin<Account>(
                                joinCondition)
                            .WhereIf(
                                !name.IsNullOrEmpty(),
                                x => x.Email != null && name.EndsWith("∞") ? x.Name.Contains(name.TrimEnd('∞', '*')) : x.Name == name,
                                ref hasWhere)
                            .WhereIf(
                                !email.IsNullOrEmpty(),
                                x => x.Email == email,
                                ref hasWhere);

            Assert.Equal("SELECT u.Id,u.Name AS UserName FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId AND u.Name LIKE '%' + @p__1 + '%' WHERE (u.Email IS NOT NULL AND u.Name LIKE '%' + @p__2 + '%') AND (u.Email = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询102
        /// </summary>
        [Fact]
        public void Test_Select_102()
        {
            //Join条件拼接
            var name = "123∞";
            var joinCondition = LinqExtensions
                                    .True<UserInfo, Account>()
                                    .And((x, y) =>
                                        x.Id == y.UserId)
                                    .WhereIf(
                                        !name.IsNullOrEmpty(),
                                        (x, y) => name.EndsWith("∞") ? x.Name.Contains(name.Trim('∞')) : x.Name == name);

            //sql构建
            var hasWhere = false;
            var email = "123@qq.com";
            var builder = SqlBuilder
                            .Select<UserInfo, Account>(
                                (u, a) => new { u.Id, UserName = "u.Name" })
                            .InnerJoin<Account>(
                                joinCondition)
                            .WhereIf(
                                !name.IsNullOrEmpty(),
                                x => !name.EndsWith("∞") && x.Email != null ? x.Name.Contains(name.TrimEnd('∞', '*')) : x.Name == name,
                                ref hasWhere)
                            .WhereIf(
                                !email.IsNullOrEmpty(),
                                x => x.Email == email,
                                ref hasWhere);

            Assert.Equal("SELECT u.Id,u.Name AS UserName FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId AND u.Name LIKE '%' + @p__1 + '%' WHERE (u.Email IS NOT NULL AND u.Name = @p__2) AND (u.Email = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询103
        /// </summary>
        [Fact]
        public void Test_Select_103()
        {
            //Join条件拼接
            var name = "123∞";
            var joinCondition = LinqExtensions
                                    .True<UserInfo, Account>()
                                    .And((x, y) =>
                                        x.Id == y.UserId)
                                    .WhereIf(
                                        !name.IsNullOrEmpty(),
                                        (x, y) => name.EndsWith("∞") ? x.Name.Contains(name.Trim('∞')) : x.Name == name);

            //sql构建
            var hasWhere = false;
            var email = "123@qq.com";
            var builder = SqlBuilder
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
                                ref hasWhere);

            Assert.Equal("SELECT u.Id,u.Name AS UserName FROM Base_UserInfo AS u INNER JOIN Base_Account AS a ON u.Id = a.UserId AND u.Name LIKE '%' + @p__1 + '%' WHERE (u.Email IS NOT NULL AND u.Name = @p__2) AND (u.Email = @p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询104
        /// </summary>
        [Fact]
        public void Test_Select_104()
        {
            var input = new DryBoxInput
            {
                BarCodes = new[] { "111" },
                BarCodeType = BarCodeType.UnitId
            };

            var builder = SqlBuilder
                            .Select<UnitInfoEntity>(
                                x => x, DatabaseType.Oracle)
                            .Where(x =>
                                x.Enabled == 1 &&
                                input.BarCodeType == BarCodeType.Panel
                                ? input.BarCodes.Contains(x.PanelNo)
                                : input.BarCodes.Contains(x.UnitId));

            Assert.Equal("SELECT * FROM WF_UNITINFO WHERE ENABLED = :p__1 AND UNITID IN (:p__2)", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询105
        /// </summary>
        [Fact]
        public void Test_Select_105()
        {
            var input = new DryBoxInput
            {
                BarCodes = new[] { "111" },
                BarCodeType = BarCodeType.UnitId
            };

            var builder = SqlBuilder
                            .Select<UnitInfoEntity>(
                                x => x, DatabaseType.Oracle)
                            .WhereIf(
                                input.BarCodeType == BarCodeType.Panel || input.BarCodeType == BarCodeType.UnitId,
                                x =>
                                input.BarCodeType == BarCodeType.Panel
                                ? x.PanelNo == "N/A"
                                : x.PanelNo != "N/A");

            Assert.Equal("SELECT * FROM WF_UNITINFO WHERE (PANELNO <> :p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("N/A", builder.Parameters[":p__1"].data);
        }

        /// <summary>
        /// 查询106
        /// </summary>
        [Fact]
        public void Test_Select_106()
        {
            var condition = LinqExtensions
                                .True<UserInfo>()
                                .And(x => ((x.Email != "" && x.Id > 10) || x.Email == "") && true)
                                .And(x => x.Sex == 1);

            var builder = SqlBuilder
                            .Select<UserInfo>(
                                x => x, DatabaseType.Oracle)
                            .Where(condition);

            Assert.Equal("SELECT * FROM Base_UserInfo WHERE ((Email <> :p__1 AND Id > :p__2) OR Email = :p__3) AND Sex = :p__4", builder.Sql);
            Assert.Equal(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询107
        /// </summary>
        [Fact]
        public void Test_Select_107()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, UserInfo, Account, Student, Class, City, Country>((u, t, a, s, d, e, f) =>
                                new { u.Id, UId = t.Id, a.Name, StudentName = s.Name, ClassName = d.Name, e.CityName, CountryName = f.Name })
                            .Join<UserInfo>((x, t) =>
                                x.Id == t.Id) //注意此处单表多次Join所以要指明具体表别名，否则都会读取第一个表别名
                            .Join<Account>((x, y) =>
                                x.Id == y.UserId)
                            .LeftJoin<Account, Student>((x, y) =>
                                x.Id == y.AccountId)
                            .RightJoin<Student, Class>((x, y) =>
                                x.Id == y.UserId)
                            .InnerJoin<Class, City>((u, d, e) =>
                                d.CityId == e.Id && u.Id > e.Id)
                            .FullJoin<City, Country>((x, y) =>
                                x.CountryId == y.Id)
                            .Where(x =>
                                x.Id != null);

            Assert.Equal("SELECT u.Id,t.Id AS UId,a.Name,s.Name AS StudentName,d.Name AS ClassName,e.City_Name AS CityName,f.Name AS CountryName FROM Base_UserInfo AS u JOIN Base_UserInfo AS t ON u.Id = t.Id JOIN Base_Account AS a ON u.Id = a.UserId LEFT JOIN Base_Student AS s ON a.Id = s.AccountId RIGHT JOIN Base_Class AS d ON s.Id = d.UserId INNER JOIN Base_City AS e ON d.CityId = e.Id AND u.Id > e.Id FULL JOIN Base_Country AS f ON e.CountryId = f.Country_Id WHERE u.Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询108
        /// </summary>
        [Fact]
        public void Test_Select_108()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((x, y) =>
                                 new { x, AccountName = y.Name })
                            .InnerJoin<UserInfo, Account>((x, y) =>
                                 x.Id == y.UserId)
                            .Where(x =>
                                x.Id != null);

            Assert.Equal("SELECT x.*,y.Name AS AccountName FROM Base_UserInfo AS x INNER JOIN Base_Account AS y ON x.Id = y.UserId WHERE x.Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询109
        /// </summary>
        [Fact]
        public void Test_Select_109()
        {
            var builder = SqlBuilder
                            .Select<UserInfo, Account>((x, y) =>
                                 new { x, AccountName = y.Name })
                            .InnerJoin<Account, UserInfo>((x, y) =>
                                 x.UserId == y.Id)
                            .Where(x =>
                                x.Id != null);

            Assert.Equal("SELECT x.*,y.Name AS AccountName FROM Base_UserInfo AS x INNER JOIN Base_Account AS y ON y.UserId = x.Id WHERE x.Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询110
        /// </summary>
        [Fact]
        public void Test_Select_110()
        {
            var builder = SqlBuilder
                            .Select<Teacher>(x =>
                                new { x })
                            .Where(x =>
                                x.Type == TeacherType.A &&
                                x.Name != null);

            Assert.Equal("SELECT * FROM Base_Teacher WHERE Type = @p__1 AND Name IS NOT NULL", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(TeacherType.A, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询111
        /// </summary>
        [Fact]
        public void Test_Select_111()
        {
            var teacherType = TeacherType.A;

            var builder = SqlBuilder
                            .Select<Teacher>(x =>
                                new { x })
                            .Where(x =>
                                x.Type == teacherType &&
                                x.Name != null);

            Assert.Equal("SELECT * FROM Base_Teacher WHERE Type = @p__1 AND Name IS NOT NULL", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(TeacherType.A, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询112
        /// </summary>
        [Fact]
        public void Test_Select_112()
        {
            //Join逻辑：默认取如：“Join<T2,T3,T4>()” 中的最后一个T4，如果T4被Join过，则依次向前递推，如果全部被Join过，则重置为默认的T4
            var builder = SqlBuilder
                            .Select<UserInfo, UserInfo, Account>((x1, x2, y) =>
                                new { x1, AccountName = y.Name })
                            .InnerJoin<UserInfo, Account>((x, y) =>
                                y.UserId == x.Id)
                            .InnerJoin<Account, UserInfo>((x1, y, x2) =>
                                x1.Id == x2.Sex &&
                                x1.Name == y.Name)
                            .Where(x =>
                                x.Id != null);

            Assert.Equal("SELECT x1.*,y.Name AS AccountName FROM Base_UserInfo AS x1 INNER JOIN Base_Account AS y ON y.UserId = x1.Id INNER JOIN Base_UserInfo AS x2 ON x1.Id = x2.Sex AND x1.Name = y.Name WHERE x1.Id IS NOT NULL", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询113
        /// </summary>
        [Fact]
        public void Test_Select_113()
        {
            var teacherType = TeacherType.A;

            var builder = SqlBuilder
                            .Select<Teacher, Class>((x, y) =>
                                 new TeacherResponse { TeacherName = x.Name, ClassName = y.Name })
                            .InnerJoin<Class>((x, y) =>
                                x.ClassId == y.Id)
                            .Where(x =>
                                x.Type == teacherType &&
                                x.Name != null);

            Assert.Equal("SELECT x.Name AS TeacherName,y.Name AS ClassName FROM Base_Teacher AS x INNER JOIN Base_Class AS y ON x.ClassId = y.Id WHERE x.Type = @p__1 AND x.Name IS NOT NULL", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(TeacherType.A, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询114
        /// </summary>
        [Fact]
        public void Test_Select_114()
        {
            var builder = SqlBuilder
                            .Select<TestTable>(x =>
                                new { x.Id, x.Name, x.OtherName, OrderName = x.Order, GroupName = x.Group })
                            .Where(x => x.Id == 1);

            Assert.Equal("SELECT Id,Name,OtherName,[Order] AS OrderName,[Group] AS GroupName FROM [Base_Test] WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal(1, builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询115
        /// </summary>
        [Fact]
        public void Test_Select_115()
        {
            var field = "Name";

            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, UserName = field });

            Assert.Equal("SELECT Id,Name AS UserName FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询116
        /// </summary>
        [Fact]
        public void Test_Select_116()
        {
            var name = "张三";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                u.Name.StartsWithIgnoreCase(name));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE UPPER(Name) LIKE UPPER(@p__1) + '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张三", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询117
        /// </summary>
        [Fact]
        public void Test_Select_117()
        {
            var name = "张三";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                u.Name.EndsWithIgnoreCase(name));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE UPPER(Name) LIKE '%' + UPPER(@p__1)", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张三", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询118
        /// </summary>
        [Fact]
        public void Test_Select_118()
        {
            var user = new UserInfo { Name = "张" };

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                u.Name.StartsWithIgnoreCase(user.Name));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE UPPER(Name) LIKE UPPER(@p__1) + '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询119
        /// </summary>
        [Fact]
        public void Test_Select_119()
        {
            var user = new UserInfo { Name = "张" };

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                u.Name.ContainsIgnoreCase(user.Name));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE UPPER(Name) LIKE '%' + UPPER(@p__1) + '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询120
        /// </summary>
        [Fact]
        public void Test_Select_120()
        {
            var name = "张";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                u.Name.ContainsIgnoreCase(name));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE UPPER(Name) LIKE '%' + UPPER(@p__1) + '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询121
        /// </summary>
        [Fact]
        public void Test_Select_121()
        {
            var name = "张";

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id)
                            .Where(u =>
                                !u.Name.ContainsIgnoreCase(name));

            Assert.Equal("SELECT Id FROM Base_UserInfo WHERE UPPER(Name) NOT LIKE '%' + UPPER(@p__1) + '%'", builder.Sql);
            Assert.Single(builder.Parameters);
            Assert.Equal("张", builder.Parameters["@p__1"].data);
        }

        /// <summary>
        /// 查询122
        /// </summary>
        [Fact]
        public void Test_Select_122()
        {
            var format = false;
            var databaseType = DatabaseType.SqlServer;

            var builder = SqlBuilder.Select<UserInfo>(
                x => typeof(UserDto).ToColumns(format, databaseType),
                databaseType,
                isEnableFormat: format);

            Assert.Equal("SELECT [SEX],Name FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询123
        /// </summary>
        [Fact]
        public void Test_Select_123()
        {
            var log = new Log { Id = Guid.NewGuid() };

            var builder = SqlBuilder
                            .Select<Log>()
                            .Where(x => x.Id == log.Id);

            Assert.Equal("SELECT * FROM Base_Log WHERE Id = @p__1", builder.Sql);
            Assert.Equal(log.Id, builder.Parameters["@p__1"].data);
            Assert.Equal(DbType.Guid, builder.Parameters["@p__1"].type.DbType);
        }

        /// <summary>
        /// 查询124
        /// </summary>
        [Fact]
        public void Test_Select_124()
        {
            var id = Guid.NewGuid();

            var builder = SqlBuilder
                            .Select<Log>()
                            .Where(x => x.Id == id);

            Assert.Equal("SELECT * FROM Base_Log WHERE Id = @p__1", builder.Sql);
            Assert.Equal(id, builder.Parameters["@p__1"].data);
            Assert.Equal(DbType.Guid, builder.Parameters["@p__1"].type.DbType);
        }

        /// <summary>
        /// 查询125
        /// </summary>
        [Fact]
        public void Test_Select_125()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>()
                            .Where(x =>
                                !(x.IsEffective == true) &&
                                !(x.IsOnLine == true));

            Assert.Equal("SELECT * FROM student WHERE IsEffective <> 1 AND IsOnLine <> 1", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询126
        /// </summary>
        [Fact]
        public void Test_Select_126()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>(databaseType: DatabaseType.PostgreSql)
                            .Where(x =>
                                !(x.IsEffective == true) &&
                                !(x.IsOnLine == true));

            Assert.Equal("SELECT * FROM student WHERE IsEffective IS NOT TRUE AND IsOnLine IS NOT TRUE", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询127
        /// </summary>
        [Fact]
        public void Test_Select_127()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>(databaseType: DatabaseType.PostgreSql)
                            .Where(x =>
                                x.IsEffective == false &&
                                x.IsOnLine);

            Assert.Equal("SELECT * FROM student WHERE IsEffective IS FALSE AND IsOnLine IS TRUE", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询128
        /// </summary>
        [Fact]
        public void Test_Select_128()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>(databaseType: DatabaseType.PostgreSql)
                            .Where(x =>
                                !(x.IsEffective == false) &&
                                x.IsOnLine != false);

            Assert.Equal("SELECT * FROM student WHERE IsEffective IS NOT FALSE AND IsOnLine IS TRUE", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 查询129
        /// </summary>
        [Fact]
        public void Test_Select_129()
        {
            var idHash = new HashSet<int> { 1, 2, 3 };

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new { u.Id, UserName = $"Name" })
                            .Where(x =>
                                idHash.Contains(x.Id.Value));

            Assert.Equal("SELECT Id,Name AS UserName FROM Base_UserInfo WHERE Id IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询130
        /// </summary>
        [Fact]
        public void Test_Select_130()
        {
            var idHash = new HashSet<int> { 1, 2, 3 };

            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                new { u.Id, UserName = $"Name" })
                            .Where(x =>
                                !idHash.Contains(x.Id.Value));

            Assert.Equal("SELECT Id,Name AS UserName FROM Base_UserInfo WHERE Id NOT IN (@p__1,@p__2,@p__3)", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }
        #endregion

        #region Page
        /// <summary>
        /// 分页1
        /// </summary>
        [Fact]
        public void Test_Page_01()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>(
                                databaseType: DatabaseType.MySql)
                            .Where(o =>
                                o.Score != null)
                            .AndWhere(o =>
                                o.Name == "")
                            .OrWhere(o =>
                                o.Subject == "")
                            .Page(3, 2, "Id", "select * from student");

            Assert.Equal(@"SELECT COUNT(*) AS `TOTAL` FROM (select * from student) AS T;select * from student ORDER BY Id ASC LIMIT 3 OFFSET 3;", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 分页2
        /// </summary>
        [Fact]
        public void Test_Page_02()
        {
            var builder = SqlBuilder
                            .Select<MyStudent>(
                                databaseType: DatabaseType.MySql, isEnableFormat: true)
                            .Where(o =>
                                o.Score != null)
                            .AndWhere(o =>
                                o.Name == "")
                            .OrWhere(o =>
                                o.Subject == "")
                            .Page(3, 2, "`Id`");

            Assert.Equal(@"SELECT COUNT(*) AS `TOTAL` FROM (SELECT * FROM `student` WHERE `Score` IS NOT NULL AND (`Name` = ?p__1) OR (`Subject` = ?p__2)) AS T;SELECT * FROM `student` WHERE `Score` IS NOT NULL AND (`Name` = ?p__1) OR (`Subject` = ?p__2) ORDER BY `Id` ASC LIMIT 3 OFFSET 3;", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 分页3
        /// </summary>
        [Fact]
        public void Test_Page_03()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>(u =>
                                u.Id, isEnableFormat: true)
                            .Where(u =>
                                u.Name == "b" &&
                                (u.Id > 2 &&
                                u.Name != null &&
                                (u.Email == "11" ||
                                u.Email == "22" &&
                                u.Email == "ee")))
                            .Page(10, 1, "[Id]");

            Assert.Equal(@"SELECT COUNT(*) AS [TOTAL] FROM (SELECT [Id] FROM [Base_UserInfo] WHERE [Name] = @p__1 AND (([Id] > @p__2 AND [Name] IS NOT NULL) AND ([Email] = @p__3 OR ([Email] = @p__4 AND [Email] = @p__5)))) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY [Id] ASC) AS [ROWNUMBER], * FROM (SELECT [Id] FROM [Base_UserInfo] WHERE [Name] = @p__1 AND (([Id] > @p__2 AND [Name] IS NOT NULL) AND ([Email] = @p__3 OR ([Email] = @p__4 AND [Email] = @p__5)))) AS T) AS N WHERE [ROWNUMBER] BETWEEN 1 AND 10;", builder.Sql);
            Assert.Equal(5, builder.Parameters.Count);
        }

        /// <summary>
        /// 分页4
        /// </summary>
        [Fact]
        public void Test_Page_04()
        {
            var builder = SqlBuilder
                            .Select<UserInfo>()
                            .PageByWith(10, 1, "Id", "WITH T AS (SELECT * FROM Base_UserInfo)");

            Assert.Equal(@"WITH T AS (SELECT * FROM Base_UserInfo) SELECT COUNT(*) AS [TOTAL] FROM T;WITH T AS (SELECT * FROM Base_UserInfo),R AS (SELECT ROW_NUMBER() OVER (ORDER BY Id ASC) AS [ROWNUMBER], * FROM T) SELECT * FROM R WHERE [ROWNUMBER] BETWEEN 1 AND 10;", builder.Sql);
            Assert.Empty(builder.Parameters);
        }
        #endregion
    }
}