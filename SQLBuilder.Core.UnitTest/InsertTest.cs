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
    public class InsertTest
    {
        #region 单个新增
        /// <summary>
        /// 单个新增1
        /// </summary>
        [TestMethod]
        public void Test_Insert_01()
        {
            var builder = SqlBuilder.Insert<UserInfo, object>(() => new
            {
                Name = "张三",
                Sex = 2
            });
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Name],[Sex]) VALUES (@Param0,@Param1)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 单个新增2
        /// </summary>
        [TestMethod]
        public void Test_Insert_02()
        {
            var builder = SqlBuilder.Insert<UserInfo, object>(() => new UserInfo
            {
                Name = "张三",
                Sex = 2
            });
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Name],[Sex]) VALUES (@Param0,@Param1)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 单个新增3
        /// </summary>
        [TestMethod]
        public void Test_Insert_03()
        {
            var userInfo = new UserInfo
            {
                Name = "张强",
                Sex = 2
            };
            var builder = SqlBuilder.Insert<UserInfo, object>(() => userInfo);
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Id],[Sex],[Name],[Email]) VALUES (NULL,@Param0,@Param1,NULL)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 单个新增4
        /// </summary>
        [TestMethod]
        public void Test_Insert_04()
        {
            var builder = SqlBuilder.Insert<Student, object>(() => new Student
            {
                Name = DateTime.Now.ToLongTimeString(),
                AccountId = (new Random()).Next(1, 100)
            });
            Assert.AreEqual("INSERT INTO [Base_Student] ([Name],[AccountId]) VALUES (@Param0,@Param1)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 单个新增5
        /// </summary>
        [TestMethod]
        public void Test_Insert_05()
        {
            var userInfo = new UserInfo
            {
                Name = "张强",
                Sex = 2
            };
            var builder = SqlBuilder.Insert<UserInfo, object>(() => userInfo, isEnableNullValue: false);
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Sex],[Name]) VALUES (@Param0,@Param1)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }
        #endregion

        #region 批量新增
        /// <summary>
        /// 批量新增1
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_01()
        {
            var builder = SqlBuilder.Insert<UserInfo, object>(() => new[]
             {
                new UserInfo { Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            });
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Name],[Sex]) VALUES (@Param0,@Param1),(@Param2,@Param3)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增2
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_02()
        {
            var builder = SqlBuilder.Insert<UserInfo, object>(() => new[]
             {
                new { Name = "张三", Sex = 2 },
                new { Name = "张三", Sex = 2 }
            });
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Name],[Sex]) VALUES (@Param0,@Param1),(@Param2,@Param3)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增3
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_03()
        {
            var array = new List<UserInfo>
            {
                new UserInfo { Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            }.ToArray();
            var builder = SqlBuilder.Insert<UserInfo, object>(() => array);
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Id],[Sex],[Name],[Email]) VALUES (NULL,@Param0,@Param1,NULL),(NULL,@Param2,@Param3,NULL)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增4
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_04()
        {
            var array = new List<dynamic>
            {
                new { Name = "张三", Sex = 2 },
                new { Name = "张三", Sex = 2 }
            }.ToArray();
            var builder = SqlBuilder.Insert<UserInfo, object>(() => array);
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Name],[Sex]) VALUES (@Param0,@Param1),(@Param2,@Param3)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增5
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_05()
        {
            var builder = SqlBuilder.Insert<Class, object>(() => new[]
             {
                new Class { CityId = 2, UserId = 2, Name = "张三" },
                new Class { CityId = 3, UserId = 3, Name = "李四" }
            });
            Assert.AreEqual("INSERT INTO [Base_Class] ([UserId],[Name]) VALUES (@Param0,@Param1),(@Param2,@Param3)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增6
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_06()
        {
            var data = new[]
            {
                new Class { CityId = 2, UserId = 2, Name = "张三" },
                new Class { CityId = 3, UserId = 3, Name = "李四" }
            };
            var builder = SqlBuilder.Insert<Class, object>(() => data);
            Assert.AreEqual("INSERT INTO [Base_Class] ([UserId],[Name]) VALUES (@Param0,@Param1),(@Param2,@Param3)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增7
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_07()
        {
            var builder = SqlBuilder.Insert<Class, object>(() => new[]
             {
                new { CityId = 2, UserId = 2, Name = "张三" },
                new { CityId = 3, UserId = 3, Name = "李四" }
            });
            Assert.AreEqual("INSERT INTO [Base_Class] ([UserId],[Name]) VALUES (@Param0,@Param1),(@Param2,@Param3)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增8
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_08()
        {
            var data = new[]
            {
                new { CityId = 2, UserId = 2, Name = "张三" },
                new { CityId = 3, UserId = 3, Name = "李四" }
            };
            var builder = SqlBuilder.Insert<Class, object>(() => data);
            Assert.AreEqual("INSERT INTO [Base_Class] ([UserId],[Name]) VALUES (@Param0,@Param1),(@Param2,@Param3)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增9
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_09()
        {
            var list = new List<UserInfo>
            {
                new UserInfo{ Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            };
            var builder = SqlBuilder.Insert<UserInfo, object>(() => list.ToArray());
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Id],[Sex],[Name],[Email]) VALUES (NULL,@Param0,@Param1,NULL),(NULL,@Param2,@Param3,NULL)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增10
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_10()
        {
            var list = new List<UserInfo>
            {
                new UserInfo{ Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            };
            var builder = SqlBuilder.Insert<UserInfo, object>(() => list);
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Id],[Sex],[Name],[Email]) VALUES (NULL,@Param0,@Param1,NULL),(NULL,@Param2,@Param3,NULL)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增11
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_11()
        {
            var builder = SqlBuilder.Insert<UserInfo, object>(() => new List<UserInfo>
            {
                new UserInfo{ Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            });
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Id],[Sex],[Name],[Email]) VALUES (NULL,@Param0,@Param1,NULL),(NULL,@Param2,@Param3,NULL)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增12
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_12()
        {
            var builder = SqlBuilder.Insert<UserInfo, object>(() => new List<dynamic>
            {
                new { Name = "张三", Sex = 2 },
                new { Name = "张三", Sex = 2 }
            });
            Assert.AreEqual("INSERT INTO [Base_UserInfo] ([Name],[Sex]) VALUES (@Param0,@Param1),(@Param2,@Param3)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }
        #endregion
    }
}