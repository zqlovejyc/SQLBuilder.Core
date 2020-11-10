using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using System;
using System.Collections.Generic;

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
            var builder = SqlBuilder.Insert<UserInfo>(() => new
            {
                Name = "张三",
                Sex = 2
            });
            Assert.AreEqual("INSERT INTO Base_UserInfo (Name,Sex) VALUES (@p__1,@p__2)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 单个新增2
        /// </summary>
        [TestMethod]
        public void Test_Insert_02()
        {
            var builder = SqlBuilder.Insert<UserInfo>(() => new UserInfo
            {
                Name = "张三",
                Sex = 2
            });
            Assert.AreEqual("INSERT INTO Base_UserInfo (Name,Sex) VALUES (@p__1,@p__2)", builder.Sql);
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
            var builder = SqlBuilder.Insert<UserInfo>(() => userInfo, isEnableNullValue: true);
            Assert.AreEqual("INSERT INTO Base_UserInfo (Id,Sex,Name,Email) VALUES (NULL,@p__1,@p__2,NULL)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 单个新增4
        /// </summary>
        [TestMethod]
        public void Test_Insert_04()
        {
            var builder = SqlBuilder.Insert<Student>(() => new Student
            {
                Name = DateTime.Now.ToLongTimeString(),
                AccountId = (new Random()).Next(1, 100)
            });
            Assert.AreEqual("INSERT INTO Base_Student (Name,AccountId) VALUES (@p__1,@p__2)", builder.Sql);
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
            var builder = SqlBuilder.Insert<UserInfo>(() => userInfo);
            Assert.AreEqual("INSERT INTO Base_UserInfo (Sex,Name) VALUES (@p__1,@p__2)", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 单个新增6
        /// </summary>
        [TestMethod]
        public void Test_Insert_06()
        {
            var userInfo = new Account
            {
                Name = "张强",
                UserId = 100,
                Id = 1
            };
            var builder = SqlBuilder.Insert<Account>(() => userInfo);
            Assert.AreEqual("INSERT INTO Base_Account (UserId,Name) VALUES (@p__1,@p__2)", builder.Sql);
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
            var builder = SqlBuilder.Insert<UserInfo>(() => new[]
            {
                new UserInfo { Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            }, DatabaseType.Oracle);
            Assert.AreEqual("INSERT INTO Base_UserInfo (Name,Sex) SELECT :p__1,:p__2 FROM DUAL UNION ALL SELECT :p__3,:p__4 FROM DUAL", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增2
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_02()
        {
            var builder = SqlBuilder.Insert<UserInfo>(() => new[]
            {
                new { Name = "张三", Sex = 2 },
                new { Name = "张三", Sex = 2 }
            });
            Assert.AreEqual("INSERT INTO Base_UserInfo (Name,Sex) VALUES (@p__1,@p__2),(@p__3,@p__4)", builder.Sql);
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
            var builder = SqlBuilder.Insert<UserInfo>(() => array, isEnableNullValue: true);
            Assert.AreEqual("INSERT INTO Base_UserInfo (Id,Sex,Name,Email) VALUES (NULL,@p__1,@p__2,NULL),(NULL,@p__3,@p__4,NULL)", builder.Sql);
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
            var builder = SqlBuilder.Insert<UserInfo>(() => array);
            Assert.AreEqual("INSERT INTO Base_UserInfo (Name,Sex) VALUES (@p__1,@p__2),(@p__3,@p__4)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增5
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_05()
        {
            var builder = SqlBuilder.Insert<Class>(() => new[]
            {
                new Class { CityId = 2, UserId = 2, Name = "张三" },
                new Class { CityId = 3, UserId = 3, Name = "李四" }
            });
            Assert.AreEqual("INSERT INTO Base_Class (UserId,Name) VALUES (@p__1,@p__2),(@p__3,@p__4)", builder.Sql);
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
            var builder = SqlBuilder.Insert<Class>(() => data);
            Assert.AreEqual("INSERT INTO Base_Class (UserId,Name) VALUES (@p__1,@p__2),(@p__3,@p__4)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增7
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_07()
        {
            var builder = SqlBuilder.Insert<Class>(() => new[]
            {
                new { CityId = 2, UserId = 2, Name = "张三" },
                new { CityId = 3, UserId = 3, Name = "李四" }
            }, isEnableFormat: true);
            Assert.AreEqual("INSERT INTO [Base_Class] ([UserId],[Name]) VALUES (@p__1,@p__2),(@p__3,@p__4)", builder.Sql);
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
            var builder = SqlBuilder.Insert<Class>(() => data);
            Assert.AreEqual("INSERT INTO Base_Class (UserId,Name) VALUES (@p__1,@p__2),(@p__3,@p__4)", builder.Sql);
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
            var builder = SqlBuilder.Insert<UserInfo>(() => list.ToArray(), isEnableNullValue: true);
            Assert.AreEqual("INSERT INTO Base_UserInfo (Id,Sex,Name,Email) VALUES (NULL,@p__1,@p__2,NULL),(NULL,@p__3,@p__4,NULL)", builder.Sql);
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
            var builder = SqlBuilder.Insert<UserInfo>(() => list, isEnableNullValue: true);
            Assert.AreEqual("INSERT INTO Base_UserInfo (Id,Sex,Name,Email) VALUES (NULL,@p__1,@p__2,NULL),(NULL,@p__3,@p__4,NULL)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增11
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_11()
        {
            var builder = SqlBuilder.Insert<UserInfo>(() => new List<UserInfo>
            {
                new UserInfo{ Name = "张三", Sex = 2 },
                new UserInfo { Name = "张三", Sex = 2 }
            }, isEnableNullValue: true);
            Assert.AreEqual("INSERT INTO Base_UserInfo (Id,Sex,Name,Email) VALUES (NULL,@p__1,@p__2,NULL),(NULL,@p__3,@p__4,NULL)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }

        /// <summary>
        /// 批量新增12
        /// </summary>
        [TestMethod]
        public void Test_Batch_Insert_12()
        {
            var builder = SqlBuilder.Insert<UserInfo>(() => new List<dynamic>
            {
                new { Name = "张三", Sex = 2 },
                new { Name = "张三", Sex = 2 }
            });
            Assert.AreEqual("INSERT INTO Base_UserInfo (Name,Sex) VALUES (@p__1,@p__2),(@p__3,@p__4)", builder.Sql);
            Assert.AreEqual(4, builder.Parameters.Count);
        }
        #endregion
    }
}