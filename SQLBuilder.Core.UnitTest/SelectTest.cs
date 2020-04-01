using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLBuilder.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLBuilder.Core.UnitTest
{
    [TestClass]
    public class SelectTest
    {
        #region Max
        /// <summary>
        /// 求最小值,NULL 值不包括在计算中
        /// </summary>
        [TestMethod]
        public void Test_Max()
        {
            var builder = SqlBuilder.Max<UserInfo>(u => u.Id).Where(o => o.Id == 3);
            Assert.AreEqual("SELECT MAX([Id]) FROM [Base_UserInfo] WHERE [Id] = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }
        #endregion

        #region Min
        /// <summary>
        /// 求最小值,NULL 值不包括在计算中
        /// </summary>
        [TestMethod]
        public void Test_Min()
        {
            var builder = SqlBuilder.Min<UserInfo>(u => u.Id);
            Assert.AreEqual("SELECT MIN([Id]) FROM [Base_UserInfo]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }
        #endregion

        #region Avg
        /// <summary>
        /// 求平均值,NULL 值不包括在计算中
        /// </summary>
        [TestMethod]
        public void Test_Avg()
        {
            var builder = SqlBuilder.Avg<UserInfo>(u => u.Id);
            Assert.AreEqual("SELECT AVG([Id]) FROM [Base_UserInfo]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }
        #endregion

        #region Count
        /// <summary>
        /// 计数1,NULL 值不包括在计算中
        /// </summary>
        [TestMethod]
        public void Test_Count_01()
        {
            var builder = SqlBuilder.Count<UserInfo>(u => u.Id);
            Assert.AreEqual("SELECT COUNT([Id]) FROM [Base_UserInfo]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 计数2
        /// </summary>
        [TestMethod]
        public void Test_Count_02()
        {
            var builder = SqlBuilder.Count<UserInfo>();
            Assert.AreEqual("SELECT COUNT(*) FROM [Base_UserInfo]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }
        #endregion

        #region Sum
        /// <summary>
        /// 求和
        /// </summary>
        [TestMethod]
        public void Test_Sum()
        {
            var builder = SqlBuilder.Sum<UserInfo>(u => u.Id);
            Assert.AreEqual("SELECT SUM([Id]) FROM [Base_UserInfo]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }
        #endregion

        #region Group By
        /// <summary>
        /// 分组1
        /// </summary>
        [TestMethod]
        public void Test_GroupBy_01()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .Where(o => o.Name == "张强")
                                    .GroupBy(u => u.Id);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 GROUP BY A.[Id]", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 分组2
        /// </summary>
        [TestMethod]
        public void Test_GroupBy_02()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .Where(o => o.Name == "张强")
                                    .GroupBy(u => new { u.Id, u.Email });
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 GROUP BY A.[Id],A.[Email]", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 分组3
        /// </summary>
        [TestMethod]
        public void Test_GroupBy_03()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .Where(o => o.Name == "张强")
                                    .GroupBy(u => new[] { "Id", "Email" });
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 GROUP BY A.[Id],A.[Email]", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 分组4
        /// </summary>
        [TestMethod]
        public void Test_GroupBy_04()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .Where(o => o.Name == "张强")
                                    .GroupBy(u => new List<string> { "Id", "Email" });
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 GROUP BY A.[Id],A.[Email]", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 分组5
        /// </summary>
        [TestMethod]
        public void Test_GroupBy_05()
        {
            var groupBy = "Id,Email".Split(',');
            var builder = SqlBuilder.Select<UserInfo>()
                                    .Where(o => o.Name == "张强")
                                    .GroupBy(u => groupBy);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 GROUP BY A.[Id],A.[Email]", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 分组6
        /// </summary>
        [TestMethod]
        public void Test_GroupBy_06()
        {
            var groupFields = "Id,Email".Split(',');
            var builder = SqlBuilder.Select<UserInfo>()
                                    .Where(o => o.Name == "张强")
                                    .GroupBy(u => groupFields);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 GROUP BY A.[Id],A.[Email]", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 分组7
        /// </summary>
        [TestMethod]
        public void Test_GroupBy_07()
        {
            var groupFields = "Id,Email".Split(',').ToList();
            var builder = SqlBuilder.Select<UserInfo>()
                                    .Where(o => o.Name == "张强")
                                    .GroupBy(u => groupFields);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 GROUP BY A.[Id],A.[Email]", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }
        #endregion

        #region Order By
        /// <summary>
        /// 排序1
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_01()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => new { u.Id, u.Email }, OrderType.Ascending, OrderType.Descending);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC,A.[Email] DESC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序2
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_02()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => new { u.Id, u.Email }, OrderType.Descending);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] DESC,A.[Email] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序3
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_03()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => new { u.Id, u.Email });
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC,A.[Email] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序4
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_04()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => new[] { "Id", "Email" });
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC,A.[Email] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序5
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_05()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => new List<string> { "[Id]", "Email" });
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC,A.[Email] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序6
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_06()
        {
            var orderFields = "Id,Email".Split(',');
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => orderFields.ToList());
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC,A.[Email] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序7
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_07()
        {
            var orderFields = "Id,Email".Split(',').ToList();
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => orderFields);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC,A.[Email] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序8
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_08()
        {
            var orderField = "Id";
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => orderField);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序9
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_09()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => "[Id] DESC");
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] DESC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序10
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_10()
        {
            var orderFields = "Id,Email".Split(',');
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => orderFields);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC,A.[Email] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序11
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_11()
        {
            var orderFields = "Id,Email".Split(',');
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => orderFields.ToList(), OrderType.Descending, OrderType.Descending);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] DESC,A.[Email] DESC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序12
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_12()
        {
            var orderField = "Id";
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => orderField, OrderType.Descending);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] DESC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序13
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_13()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => u.Id, OrderType.Descending);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] DESC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序14
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_14()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => u.Id, OrderType.Ascending);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] ASC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序15
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_15()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => u.Id);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 排序16
        /// </summary>
        [TestMethod]
        public void Test_OrderBy_16()
        {
            var builder = SqlBuilder.Select<UserInfo>()
                                    .OrderBy(u => "[Id] DESC");
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A ORDER BY A.[Id] DESC", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }
        #endregion

        #region Top
        /// <summary>
        /// Top01
        /// </summary>
        [TestMethod]
        public void Test_Select_Top_01()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, u.Name }).Top(100);
            Assert.AreEqual("SELECT TOP 100 A.[Id],A.[Name] FROM [Base_UserInfo] AS A", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// Top02
        /// </summary>
        [TestMethod]
        public void Test_Select_Top_02()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, u.Name }, DatabaseType.MySQL).Top(100);
            Assert.AreEqual("SELECT A.`Id`,A.`Name` FROM `Base_UserInfo` AS A LIMIT 100 OFFSET 0", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// Top03
        /// </summary>
        [TestMethod]
        public void Test_Select_Top_03()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, u.Name }, DatabaseType.Oracle).Top(100);
            Assert.AreEqual("SELECT A.\"Id\",A.\"Name\" FROM \"Base_UserInfo\" A WHERE ROWNUM <= 100", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }
        #endregion

        #region Distinct
        /// <summary>
        /// Distinct01
        /// </summary>
        [TestMethod]
        public void Test_Select_Distinct_01()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, u.Name }).Top(100).Distinct();
            Assert.AreEqual("SELECT DISTINCT TOP 100 A.[Id],A.[Name] FROM [Base_UserInfo] AS A", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// Distinct02
        /// </summary>
        [TestMethod]
        public void Test_Select_Distinct_02()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, u.Name }).Distinct().Top(100);
            Assert.AreEqual("SELECT DISTINCT TOP 100 A.[Id],A.[Name] FROM [Base_UserInfo] AS A", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }
        #endregion

        #region Select
        /// <summary>
        /// 查询1
        /// </summary>
        [TestMethod]
        public void Test_Select_01()
        {
            var builder = SqlBuilder.Select<UserInfo>();
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询2
        /// </summary>
        [TestMethod]
        public void Test_Select_02()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Id);
            Assert.AreEqual("SELECT A.[Id] FROM [Base_UserInfo] AS A", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询3
        /// </summary>
        [TestMethod]
        public void Test_Select_03()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, u.Name });
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询4
        /// </summary>
        [TestMethod]
        public void Test_Select_04()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => new { u.Id, UserName = u.Name });
            Assert.AreEqual("SELECT A.[Id],A.[Name] AS UserName FROM [Base_UserInfo] AS A", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询5
        /// </summary>
        [TestMethod]
        public void Test_Select_05()
        {
            var entity = new { name = "新用户" };
            var builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => u.Name == entity.name);
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0", builder.Sql);
            Assert.AreEqual("新用户", builder.Parameters.First().Value);
        }

        /// <summary>
        /// 查询6
        /// </summary>
        [TestMethod]
        public void Test_Select_06()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Id)
                                    .Where(u => u.Name.Like("张三"));
            Assert.AreEqual("SELECT A.[Id] FROM [Base_UserInfo] AS A WHERE A.[Name] LIKE '%' + @Param0 + '%'", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询7
        /// </summary>
        [TestMethod]
        public void Test_Select_07()
        {
            var name = "张三";
            var builder = SqlBuilder.Select<UserInfo>(u => u.Id)
                                    .Where(u => u.Name.NotLike(name));
            Assert.AreEqual("SELECT A.[Id] FROM [Base_UserInfo] AS A WHERE A.[Name] NOT LIKE '%' + @Param0 + '%'", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询8
        /// </summary>
        [TestMethod]
        public void Test_Select_08()
        {
            var name = "张三";
            var builder = SqlBuilder.Select<UserInfo>(u => u.Id)
                                    .Where(u => u.Name.LikeRight(name));
            Assert.AreEqual("SELECT A.[Id] FROM [Base_UserInfo] AS A WHERE A.[Name] LIKE @Param0 + '%'", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询9
        /// </summary>
        [TestMethod]
        public void Test_Select_09()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => u.Id.In(1, 2, 3));
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Id] IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询10
        /// </summary>
        [TestMethod]
        public void Test_Select_10()
        {
            int[] aryId = { 1, 2, 3 };
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => u.Id.In(aryId));
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Id] IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询11
        /// </summary>
        [TestMethod]
        public void Test_Select_11()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => u.Name.In(new string[] { "a", "b" }));
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] IN (@Param0,@Param1)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询12
        /// </summary>
        [TestMethod]
        public void Test_Select_12()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => u.Id.NotIn(1, 2, 3));
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Id] NOT IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询13
        /// </summary>
        [TestMethod]
        public void Test_Select_13()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Id)
                                    .Where(
                                        u => u.Name == "b"
                                        && (u.Id > 2 && u.Name != null)
                                        && u.Id > int.MinValue
                                        && u.Id < int.MaxValue
                                        && u.Id.In(1, 2, 3)
                                        && u.Name.Like("a")
                                        && u.Name.LikeLeft("b")
                                        && u.Name.LikeRight("c")
                                        || u.Id == null
                                    );
            Assert.AreEqual("SELECT A.[Id] FROM [Base_UserInfo] AS A WHERE (((((((A.[Name] = @Param0 AND (A.[Id] > @Param1 AND A.[Name] IS NOT NULL)) AND A.[Id] > @Param2) AND A.[Id] < @Param3) AND A.[Id] IN (@Param4,@Param5,@Param6)) AND A.[Name] LIKE '%' + @Param7 + '%') AND A.[Name] LIKE '%' + @Param8) AND A.[Name] LIKE @Param9 + '%') OR A.[Id] IS NULL", builder.Sql);
            Assert.AreEqual(10, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询14
        /// </summary>
        [TestMethod]
        public void Test_Select_14()
        {
            var builder = SqlBuilder.Select<UserInfo, Account>((u, a) => new { u.Id, a.Name })
                                    .Join<Account>((u, a) => u.Id == a.UserId && (u.Email == "111" || u.Email == "222"));
            Assert.AreEqual("SELECT A.[Id],B.[Name] FROM [Base_UserInfo] AS A JOIN [Base_Account] AS B ON A.[Id] = B.[UserId] AND (A.[Email] = @Param0 OR A.[Email] = @Param1)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询15
        /// </summary>
        [TestMethod]
        public void Test_Select_15()
        {
            var builder = SqlBuilder.Select<UserInfo, Account>((u, a) => new { u.Id, a.Name })
                                    .InnerJoin<Account>((u, a) => u.Id == a.UserId);
            Assert.AreEqual("SELECT A.[Id],B.[Name] FROM [Base_UserInfo] AS A INNER JOIN [Base_Account] AS B ON A.[Id] = B.[UserId]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询16
        /// </summary>
        [TestMethod]
        public void Test_Select_16()
        {
            var builder = SqlBuilder.Select<UserInfo, Account>((u, a) => new { u.Id, a.Name })
                                    .LeftJoin<Account>((u, a) => u.Id == a.UserId);
            Assert.AreEqual("SELECT A.[Id],B.[Name] FROM [Base_UserInfo] AS A LEFT JOIN [Base_Account] AS B ON A.[Id] = B.[UserId]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询17
        /// </summary>
        [TestMethod]
        public void Test_Select_17()
        {
            var builder = SqlBuilder.Select<UserInfo, Account>((u, a) => new { u.Id, a.Name })
                                    .RightJoin<Account>((u, a) => u.Id == a.UserId);
            Assert.AreEqual("SELECT A.[Id],B.[Name] FROM [Base_UserInfo] AS A RIGHT JOIN [Base_Account] AS B ON A.[Id] = B.[UserId]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询18
        /// </summary>
        [TestMethod]
        public void Test_Select_18()
        {
            var builder = SqlBuilder.Select<UserInfo, Account>((u, a) => new { u.Id, a.Name })
                                    .FullJoin<Account>((u, a) => u.Id == a.UserId);
            Assert.AreEqual("SELECT A.[Id],B.[Name] FROM [Base_UserInfo] AS A FULL JOIN [Base_Account] AS B ON A.[Id] = B.[UserId]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询19
        /// </summary>
        [TestMethod]
        public void Test_Select_19()
        {
            var builder = SqlBuilder.Select<UserInfo, Account, Student, Class, City, Country>((u, a, s, d, e, f) => new { u.Id, a.Name, StudentName = s.Name, ClassName = d.Name, e.CityName, CountryName = f.Name })
                                    .Join<Account>((u, a) => u.Id == a.UserId)
                                    .LeftJoin<Account, Student>((a, s) => a.Id == s.AccountId)
                                    .RightJoin<Student, Class>((s, c) => s.Id == c.UserId)
                                    .InnerJoin<Class, City>((c, d) => c.CityId == d.Id)
                                    .FullJoin<City, Country>((c, d) => c.CountryId == d.Id)
                                    .Where(u => u.Id != null);
            Assert.AreEqual("SELECT A.[Id],B.[Name],C.[Name] AS StudentName,D.[Name] AS ClassName,E.[City_Name],F.[Name] AS CountryName FROM [Base_UserInfo] AS A JOIN [Base_Account] AS B ON A.[Id] = B.[UserId] LEFT JOIN [Base_Student] AS C ON B.[Id] = C.[AccountId] RIGHT JOIN [Base_Class] AS D ON C.[Id] = D.[UserId] INNER JOIN [Base_City] AS E ON D.[CityId] = E.[Id] FULL JOIN [Base_Country] AS F ON E.[CountryId] = F.[Country_Id] WHERE A.[Id] IS NOT NULL", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询20
        /// </summary>
        [TestMethod]
        public void Test_Select_20()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name }, DatabaseType.MySQL)
                                    .Where(u => 1 == 1)
                                    .AndWhere(u => u.Name == "");
            Assert.AreEqual("SELECT A.`Id`,A.`Name` FROM `Base_UserInfo` AS A WHERE (A.`Name` = ?Param0)", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询21
        /// </summary>
        [TestMethod]
        public void Test_Select_21()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => u.Name.Contains("11"))
                                    .AndWhere(u => !string.IsNullOrEmpty(u.Name))
                                    .AndWhere(u => string.IsNullOrEmpty(u.Email));
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] LIKE '%' + @Param0 + '%' AND ((A.[Name] IS NOT NULL AND A.[Name] <> '')) AND ((A.[Email] IS NULL OR A.[Email] = ''))", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询22
        /// </summary>
        [TestMethod]
        public void Test_Select_22()
        {
            var expr = Extensions.True<UserInfo>();
            expr = expr.And(o => o.Id > 0);
            expr = expr.Or(o => o.Email != "");
            var builder = SqlBuilder.Select<UserInfo>().Where(expr);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Id] > @Param0 OR A.[Email] <> @Param1", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询23
        /// </summary>
        [TestMethod]
        public void Test_Select_23()
        {
            var builder = SqlBuilder.Select<UserInfo>().WithKey(2);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Id] = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询24
        /// </summary>
        [TestMethod]
        public void Test_Select_24()
        {
            var expr = Extensions.True<UserInfo>();
            var builder = SqlBuilder.Select<UserInfo>().Where(expr);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询25
        /// </summary>
        [TestMethod]
        public void Test_Select_25()
        {
            var expr = Extensions.False<UserInfo>();
            var builder = SqlBuilder.Select<UserInfo>().Where(expr);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE  1 = 0 ", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询26
        /// </summary>
        [TestMethod]
        public void Test_Select_26()
        {
            var entity = new UserInfo { Name = "新用户" };
            var builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => u.Name == entity.Name);
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询27
        /// </summary>
        [TestMethod]
        public void Test_Select_27()
        {
            SqlBuilderCore<UserInfo> builder = null;
            var list = new List<UserInfo> { new UserInfo { Name = "新用户" } };
            list.ForEach(_ =>
            {
                builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => u.Name == _.Name);
            });
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询28
        /// </summary>
        [TestMethod]
        public void Test_Select_28()
        {
            SqlBuilderCore<UserInfo> builder = null;
            var list = new List<UserInfo> { new UserInfo { Id = 2 } };
            list.ForEach(_ =>
            {
                builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => u.Id.Equals(_.Id));
            });
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Id] = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询29
        /// </summary>
        [TestMethod]
        public void Test_Select_29()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => !u.Name.Equals(null));
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] IS NOT NULL", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询30
        /// </summary>
        [TestMethod]
        public void Test_Select_30()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => !u.Name.Equals(null) == true);
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] IS NOT NULL", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询31
        /// </summary>
        [TestMethod]
        public void Test_Select_31()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => u.Name.Equals(null) == false);
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] IS NOT NULL", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询32
        /// </summary>
        [TestMethod]
        public void Test_Select_32()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => u.Name.Equals(null) == true);
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] IS NULL", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询33
        /// </summary>
        [TestMethod]
        public void Test_Select_33()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => u.Name.Contains("11"))
                                    .AndWhere(u => !string.IsNullOrEmpty(u.Name) == false)
                                    .AndWhere(u => string.IsNullOrEmpty(u.Email) == true);
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Name] LIKE '%' + @Param0 + '%' AND ((A.[Name] IS NULL OR A.[Name] = '')) AND ((A.[Email] IS NULL OR A.[Email] = ''))", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询34
        /// </summary>
        [TestMethod]
        public void Test_Select_34()
        {
            SqlBuilderCore<UserInfo> builder = null;
            var list = new List<UserInfo> { new UserInfo { Id = 2 } };
            list.ForEach(_ =>
            {
                builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => u.Id.Equals(_.Id) == false);
            });
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Id] <> @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询35
        /// </summary>
        [TestMethod]
        public void Test_Select_35()
        {
            SqlBuilderCore<UserInfo> builder = null;
            var list = new List<UserInfo> { new UserInfo { Id = 2 } };
            list.ForEach(_ =>
            {
                builder = SqlBuilder.Select<UserInfo>(o => new { o.Id, o.Name })
                                    .Where(u => !u.Id.Equals(_.Id) == false);
            });
            Assert.AreEqual("SELECT A.[Id],A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Id] = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询36
        /// </summary>
        [TestMethod]
        public void Test_Select_36()
        {
            var expr = Extensions.True<UserInfo>();
            expr = expr.And(o => o.Id > 0 == false);
            var builder = SqlBuilder.Select<UserInfo>().Where(expr);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Id] <= @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询37
        /// </summary>
        [TestMethod]
        public void Test_Select_37()
        {
            var expr = Extensions.True<UserInfo>();
            expr = expr.And(o => o.Id >= 0 == false);
            var builder = SqlBuilder.Select<UserInfo>().Where(expr);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Id] < @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询38
        /// </summary>
        [TestMethod]
        public void Test_Select_38()
        {
            var expr = Extensions.True<UserInfo>();
            expr = expr.And(o => o.Id == null == false);
            var builder = SqlBuilder.Select<UserInfo>().Where(expr);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Id] IS NOT NULL", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询39
        /// </summary>
        [TestMethod]
        public void Test_Select_39()
        {
            var expr = Extensions.True<UserInfo>();
            expr = expr.And(o => !(o.Id > 0 && o.Id < 5));
            var builder = SqlBuilder.Select<UserInfo>().Where(expr);
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Id] <= @Param0 OR A.[Id] >= @Param1", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询40
        /// </summary>
        [TestMethod]
        public void Test_Select_40()
        {
            var builder = SqlBuilder.Select<City3>().Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => o.Id > 0);
            Assert.AreEqual("SELECT A.[Id],A.[City_Name],A.[Age],A.[Address] FROM [Base_City3] AS A WHERE A.[Id] > @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询41
        /// </summary>
        [TestMethod]
        public void Test_Select_41()
        {
            var builder = SqlBuilder.Select<City3>().Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => o.CityName.ToUpper() == "郑州");
            Assert.AreEqual("SELECT A.[Id],A.[City_Name],A.[Age],A.[Address] FROM [Base_City3] AS A WHERE UPPER(A.[City_Name]) = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询42
        /// </summary>
        [TestMethod]
        public void Test_Select_42()
        {
            var builder = SqlBuilder.Select<City3>().Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => o.CityName.ToLower() == "郑州");
            Assert.AreEqual("SELECT A.[Id],A.[City_Name],A.[Age],A.[Address] FROM [Base_City3] AS A WHERE LOWER(A.[City_Name]) = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询43
        /// </summary>
        [TestMethod]
        public void Test_Select_43()
        {
            var builder = SqlBuilder.Select<City3>().Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => o.CityName.Trim() == "郑州");
            Assert.AreEqual("SELECT A.[Id],A.[City_Name],A.[Age],A.[Address] FROM [Base_City3] AS A WHERE LTRIM(RTRIM(A.[City_Name])) = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询44
        /// </summary>
        [TestMethod]
        public void Test_Select_44()
        {
            var builder = SqlBuilder.Select<City3>().Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => o.CityName.TrimStart() == "郑州");
            Assert.AreEqual("SELECT A.[Id],A.[City_Name],A.[Age],A.[Address] FROM [Base_City3] AS A WHERE LTRIM(A.[City_Name]) = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询45
        /// </summary>
        [TestMethod]
        public void Test_Select_45()
        {
            var builder = SqlBuilder.Select<City3>().Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => o.CityName.TrimEnd() == "郑州");
            Assert.AreEqual("SELECT A.[Id],A.[City_Name],A.[Age],A.[Address] FROM [Base_City3] AS A WHERE RTRIM(A.[City_Name]) = @Param0", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询46
        /// </summary>
        [TestMethod]
        public void Test_Select_46()
        {
            var builder = SqlBuilder.Select<City3>(databaseType: DatabaseType.MySQL).Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => !string.IsNullOrEmpty(o.CityName) && o.CityName.Trim() == "郑州".Trim());
            Assert.AreEqual("SELECT A.`Id`,A.`City_Name`,A.`Age`,A.`Address` FROM `Base_City3` AS A WHERE (A.`City_Name` IS NOT NULL AND A.`City_Name` <> '') AND TRIM(A.`City_Name`) = TRIM(?Param0)", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询47
        /// </summary>
        [TestMethod]
        public void Test_Select_47()
        {
            var builder = SqlBuilder.Select<City3>(databaseType: DatabaseType.MySQL).Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => o.CityName.Trim().Contains("郑州".Trim()));
            Assert.AreEqual("SELECT A.`Id`,A.`City_Name`,A.`Age`,A.`Address` FROM `Base_City3` AS A WHERE TRIM(A.`City_Name`) LIKE CONCAT('%',TRIM(?Param0),'%')", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询48
        /// </summary>
        [TestMethod]
        public void Test_Select_48()
        {
            var builder = SqlBuilder.Select<City3>(databaseType: DatabaseType.SQLite).Select(o => new { o.Id, o.CityName, o.Age, o.Address }).Where(o => o.CityName.Trim().Contains("郑州".Trim()));
            Assert.AreEqual("SELECT A.\"Id\",A.\"City_Name\",A.\"Age\",A.\"Address\" FROM \"Base_City3\" AS A WHERE TRIM(A.\"City_Name\") LIKE '%' || TRIM(@Param0) || '%'", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询49
        /// </summary>
        [TestMethod]
        public void Test_Select_49()
        {
            var builder = SqlBuilder.Select<UserInfo>().Where(x => new[] { "a", "b", "c" }.Contains(x.Name));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询50
        /// </summary>
        [TestMethod]
        public void Test_Select_50()
        {
            var builder = SqlBuilder.Select<UserInfo>().Where(x => !new[] { "a", "b", "c" }.Contains(x.Name));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] NOT IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询51
        /// </summary>
        [TestMethod]
        public void Test_Select_51()
        {
            var array = new[] { "a", "b", "c" };
            var builder = SqlBuilder.Select<UserInfo>().Where(x => !array.Contains(x.Name));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] NOT IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询52
        /// </summary>
        [TestMethod]
        public void Test_Select_52()
        {
            var array = new[] { "a", "b", "c" };
            var builder = SqlBuilder.Select<UserInfo>().Where(x => !x.Name.In(array));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] NOT IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询53
        /// </summary>
        [TestMethod]
        public void Test_Select_53()
        {
            var array = new[] { "a", "b", "c" };
            var builder = SqlBuilder.Select<UserInfo>().Where(x => !x.Name.NotIn(array));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询54
        /// </summary>
        [TestMethod]
        public void Test_Select_54()
        {
            var array = new[] { "a", "b", "c" };
            var builder = SqlBuilder.Select<UserInfo>().Where(x => !!array.Contains(x.Name));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询55
        /// </summary>
        [TestMethod]
        public void Test_Select_55()
        {
            var builder = SqlBuilder.Select<MyStudent>().Where(x => x.IsEffective.Value);
            Assert.AreEqual("SELECT * FROM [student] AS A WHERE A.[IsEffective] = 1", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询56
        /// </summary>
        [TestMethod]
        public void Test_Select_56()
        {
            var builder = SqlBuilder.Select<MyStudent>().Where(x => x.IsEffective == true);
            Assert.AreEqual("SELECT * FROM [student] AS A WHERE A.[IsEffective] = 1", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询57
        /// </summary>
        [TestMethod]
        public void Test_Select_57()
        {
            var builder = SqlBuilder.Select<MyStudent>().Where(x => x.IsEffective == false);
            Assert.AreEqual("SELECT * FROM [student] AS A WHERE A.[IsEffective] <> 1", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询58
        /// </summary>
        [TestMethod]
        public void Test_Select_58()
        {
            var builder = SqlBuilder.Select<MyStudent>().Where(x => x.IsEffective == true && x.IsOnLine);
            Assert.AreEqual("SELECT * FROM [student] AS A WHERE A.[IsEffective] = 1 AND A.[IsOnLine] = 1", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询59
        /// </summary>
        [TestMethod]
        public void Test_Select_59()
        {
            var builder = SqlBuilder.Select<MyStudent>().Where(x => x.IsEffective == true && x.IsOnLine == true);
            Assert.AreEqual("SELECT * FROM [student] AS A WHERE A.[IsEffective] = 1 AND A.[IsOnLine] = 1", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询60
        /// </summary>
        [TestMethod]
        public void Test_Select_60()
        {
            var builder = SqlBuilder.Select<MyStudent>().Where(x => !x.IsEffective.Value && !x.IsOnLine);
            Assert.AreEqual("SELECT * FROM [student] AS A WHERE A.[IsEffective] <> 1 AND A.[IsOnLine] <> 1", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询61
        /// </summary>
        [TestMethod]
        public void Test_Select_61()
        {
            var builder = SqlBuilder.Select<MyStudent>().Where(x => !x.IsEffective == true && !x.IsOnLine == true);
            Assert.AreEqual("SELECT * FROM [student] AS A WHERE A.[IsEffective] <> 1 AND A.[IsOnLine] <> 1", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询62
        /// </summary>
        [TestMethod]
        public void Test_Select_62()
        {
            var builder = SqlBuilder
                                .Select<UserInfo, Account, Student, Class, City, Country>((u, a, s, d, e, f) => new { u, a.Name, StudentName = s.Name, ClassName = d.Name, e.CityName, CountryName = f.Name })
                                .Join<Account>((u, a) => u.Id == a.UserId)
                                .LeftJoin<Account, Student>((a, s) => a.Id == s.AccountId)
                                .RightJoin<Student, Class>((s, c) => s.Id == c.UserId)
                                .InnerJoin<Class, City>((c, d) => c.CityId == d.Id)
                                .FullJoin<City, Country>((c, d) => c.CountryId == d.Id)
                                .Where(u => u.Id != null);
            Assert.AreEqual("SELECT A.*,B.[Name],C.[Name] AS StudentName,D.[Name] AS ClassName,E.[City_Name],F.[Name] AS CountryName FROM [Base_UserInfo] AS A JOIN [Base_Account] AS B ON A.[Id] = B.[UserId] LEFT JOIN [Base_Student] AS C ON B.[Id] = C.[AccountId] RIGHT JOIN [Base_Class] AS D ON C.[Id] = D.[UserId] INNER JOIN [Base_City] AS E ON D.[CityId] = E.[Id] FULL JOIN [Base_Country] AS F ON E.[CountryId] = F.[Country_Id] WHERE A.[Id] IS NOT NULL", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询63
        /// </summary>
        [TestMethod]
        public void Test_Select_63()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => o, DatabaseType.MySQL)
                                    .Where(u => 1 == 1)
                                    .AndWhere(u => u.Name == "");
            Assert.AreEqual("SELECT A.* FROM `Base_UserInfo` AS A WHERE (A.`Name` = ?Param0)", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询64
        /// </summary>
        [TestMethod]
        public void Test_Select_64()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => new { o }, DatabaseType.MySQL)
                                    .Where(u => 1 == 1)
                                    .AndWhere(u => u.Name == "");
            Assert.AreEqual("SELECT A.* FROM `Base_UserInfo` AS A WHERE (A.`Name` = ?Param0)", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询65
        /// </summary>
        [TestMethod]
        public void Test_Select_65()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => null, DatabaseType.MySQL)
                                    .Where(u => 1 == 1)
                                    .AndWhere(u => u.Name == "");
            Assert.AreEqual("SELECT A.* FROM `Base_UserInfo` AS A WHERE (A.`Name` = ?Param0)", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询66
        /// </summary>
        [TestMethod]
        public void Test_Select_66()
        {
            var builder = SqlBuilder.Select<UserInfo>(o => "A.*", DatabaseType.MySQL)
                                    .Where(u => 1 == 1)
                                    .AndWhere(u => u.Name == "");
            Assert.AreEqual("SELECT A.* FROM `Base_UserInfo` AS A WHERE (A.`Name` = ?Param0)", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询67
        /// </summary>
        [TestMethod]
        public void Test_Select_67()
        {
            var builder = SqlBuilder.Select<UserInfo, Account>((u, a) => null)
                                    .Join<Account>((u, a) => u.Id == a.UserId && (u.Email == "111" || u.Email == "222"));
            Assert.AreEqual("SELECT A.* FROM [Base_UserInfo] AS A JOIN [Base_Account] AS B ON A.[Id] = B.[UserId] AND (A.[Email] = @Param0 OR A.[Email] = @Param1)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询68
        /// </summary>
        [TestMethod]
        public void Test_Select_68()
        {
            var builder = SqlBuilder.Select<UserInfo, Account>((u, a) => "B.*")
                                    .Join<Account>((u, a) => u.Id == a.UserId && (u.Email == "111" || u.Email == "222"));
            Assert.AreEqual("SELECT B.* FROM [Base_UserInfo] AS A JOIN [Base_Account] AS B ON A.[Id] = B.[UserId] AND (A.[Email] = @Param0 OR A.[Email] = @Param1)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询69
        /// </summary>
        [TestMethod]
        public void Test_Select_69()
        {
            var list = new[] { "a", "b", "c" }.ToList();
            var builder = SqlBuilder.Select<UserInfo>().Where(x => list.Contains(x.Name));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询70
        /// </summary>
        [TestMethod]
        public void Test_Select_70()
        {
            var list = new[] { "a", "b", "c" }.Distinct();
            var builder = SqlBuilder.Select<UserInfo>().Where(x => list.Contains(x.Name));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询71
        /// </summary>
        [TestMethod]
        public void Test_Select_71()
        {
            var name = "test";
            var builder = SqlBuilder.Select<UserInfo>().Where(x => x.Name.Contains(name));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Name] LIKE '%' + @Param0 + '%'", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询72
        /// </summary>
        [TestMethod]
        public void Test_Select_72()
        {
            var list = new[] { 1, 2, 3 }.ToList();
            var builder = SqlBuilder.Select<UserInfo>().Where(x => list.Contains(x.Id.Value));
            Assert.AreEqual("SELECT * FROM [Base_UserInfo] AS A WHERE A.[Id] IN (@Param0,@Param1,@Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询73
        /// </summary>
        [TestMethod]
        public void Test_Select_73()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => u.Id > 1000 || (u.Id < 10 && u.Name.Equals("张三")));
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Id] > @Param0 OR (A.[Id] < @Param1 AND A.[Name] = @Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询74
        /// </summary>
        [TestMethod]
        public void Test_Select_74()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => (u.Id < 10 && u.Name.Equals("张三")) || u.Id > 1000);
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE (A.[Id] < @Param0 AND A.[Name] = @Param1) OR A.[Id] > @Param2", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 查询75
        /// </summary>
        [TestMethod]
        public void Test_Select_75()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Name)
                                    .Where(u => u.Id.Equals(1000) || (u.Id < 10 && u.Name.Equals("张三")));
            Assert.AreEqual("SELECT A.[Name] FROM [Base_UserInfo] AS A WHERE A.[Id] = @Param0 OR (A.[Id] < @Param1 AND A.[Name] = @Param2)", builder.Sql);
            Assert.AreEqual(3, builder.Parameters.Count);
        }
        #endregion

        #region Page
        /// <summary>
        /// 分页1
        /// </summary>
        [TestMethod]
        public void Test_Page_01()
        {
            var builder = SqlBuilder.Select<MyStudent>(databaseType: DatabaseType.MySQL)
                                  .Where(o => o.Score != null)
                                  .AndWhere(o => o.Name == "")
                                  .OrWhere(o => o.Subject == "")
                                  .Page(3, 2, "Id", "select * from student");
            Assert.AreEqual(@"DROP TEMPORARY TABLE IF EXISTS $TEMPORARY;CREATE TEMPORARY TABLE $TEMPORARY SELECT * FROM (select * from student) AS T;SELECT COUNT(1) AS Total FROM $TEMPORARY;SELECT * FROM $TEMPORARY AS X ORDER BY `Id` LIMIT 3 OFFSET 3;DROP TABLE $TEMPORARY;", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 分页2
        /// </summary>
        [TestMethod]
        public void Test_Page_02()
        {
            var builder = SqlBuilder.Select<MyStudent>(databaseType: DatabaseType.MySQL)
                                  .Where(o => o.Score != null)
                                  .AndWhere(o => o.Name == "")
                                  .OrWhere(o => o.Subject == "")
                                  .Page(3, 2, "`Id`");
            Assert.AreEqual(@"DROP TEMPORARY TABLE IF EXISTS $TEMPORARY;CREATE TEMPORARY TABLE $TEMPORARY SELECT * FROM (SELECT * FROM `student` AS A WHERE A.`Score` IS NOT NULL AND (A.`Name` = ?Param0) OR (A.`Subject` = ?Param1)) AS T;SELECT COUNT(1) AS Total FROM $TEMPORARY;SELECT * FROM $TEMPORARY AS X ORDER BY `Id` LIMIT 3 OFFSET 3;DROP TABLE $TEMPORARY;", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 分页3
        /// </summary>
        [TestMethod]
        public void Test_Page_03()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Id)
                                  .Where(u => u.Name == "b" && (u.Id > 2 && u.Name != null && (u.Email == "11" || u.Email == "22" && u.Email == "ee")))
                                  .Page(10, 1, "[Id]");
            Assert.AreEqual(@"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;SELECT * INTO #TEMPORARY FROM (SELECT A.[Id] FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 AND ((A.[Id] > @Param1 AND A.[Name] IS NOT NULL) AND (A.[Email] = @Param2 OR (A.[Email] = @Param3 AND A.[Email] = @Param4)))) AS T;SELECT COUNT(1) AS Total FROM #TEMPORARY;SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY [Id]) AS RowNumber, * FROM #TEMPORARY) AS N WHERE RowNumber BETWEEN 1 AND 10;DROP TABLE #TEMPORARY;", builder.Sql);
            Assert.AreEqual(5, builder.Parameters.Count);
        }

        /// <summary>
        /// 分页4
        /// </summary>
        [TestMethod]
        public void Test_Page_04()
        {
            var builder = SqlBuilder.Select<UserInfo>(u => u.Id)
                                  .Where(u => u.Name == "b" && (u.Id > 2 && u.Name != null && (u.Email == "11" || u.Email == "22" || u.Email == "ee")))
                                  .PageByWith(10, 1, "Id");
            Assert.AreEqual(@"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;WITH T AS (SELECT A.[Id] FROM [Base_UserInfo] AS A WHERE A.[Name] = @Param0 AND ((A.[Id] > @Param1 AND A.[Name] IS NOT NULL) AND ((A.[Email] = @Param2 OR A.[Email] = @Param3) OR A.[Email] = @Param4))) SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY [Id]) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN 1 AND 10;DROP TABLE #TEMPORARY;", builder.Sql);
            Assert.AreEqual(5, builder.Parameters.Count);
        }

        /// <summary>
        /// 分页5
        /// </summary>
        [TestMethod]
        public void Test_Page_05()
        {
            var builder = SqlBuilder.Select<UserInfo>().PageByWith(10, 1, "Id", "WITH T AS (SELECT * FROM Base_UserInfo)");
            Assert.AreEqual(@"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;WITH T AS (SELECT * FROM Base_UserInfo) SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY [Id]) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN 1 AND 10;DROP TABLE #TEMPORARY;", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 分页6
        /// </summary>
        [TestMethod]
        public void Test_Page_06()
        {
            var builder = SqlBuilder.Select<UserInfo>().Page(10, 1, "Id", "WITH T AS (SELECT * FROM Base_UserInfo)");
            Assert.AreEqual(@"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;WITH T AS (SELECT * FROM Base_UserInfo) SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY [Id]) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN 1 AND 10;DROP TABLE #TEMPORARY;", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 分页7
        /// </summary>
        [TestMethod]
        public void Test_Page_07()
        {
            var builder = SqlBuilder.Select<UserInfo>(databaseType: DatabaseType.MySQL).PageByWith(10, 1, "Id", "WITH T AS (SELECT * FROM `Base_UserInfo`)");
            Assert.AreEqual(@"WITH T AS (SELECT * FROM `Base_UserInfo`) SELECT COUNT(1) AS Total FROM T;WITH T AS (SELECT * FROM `Base_UserInfo`) SELECT * FROM T ORDER BY `Id` LIMIT 10 OFFSET 0;", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }
        #endregion
    }
}