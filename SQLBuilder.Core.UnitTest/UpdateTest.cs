using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SQLBuilder.Core.UnitTest
{
    public class UpdateTest
    {
        /// <summary>
        /// 修改1
        /// </summary>
        [Fact]
        public void Test_Update_01()
        {
            var builder = SqlBuilder
                            .Update<UserInfo>(() =>
                                new
                                {
                                    Name = "",
                                    Sex = 1,
                                    Email = "123456@qq.com"
                                },
                                isEnableFormat: true);

            Assert.Equal("UPDATE [Base_UserInfo] SET [Name] = @p__1,[Sex] = @p__2,[Email] = @p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改2
        /// </summary>
        [Fact]
        public void Test_Update_02()
        {
            var builder = SqlBuilder
                            .Update<UserInfo>(() =>
                                new UserInfo
                                {
                                    Sex = 1,
                                    Email = "123456@qq.com"
                                })
                            .Where(u =>
                                u.Id == 1);

            Assert.Equal("UPDATE Base_UserInfo SET Sex = @p__1,Email = @p__2 WHERE Id = @p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改3
        /// </summary>
        [Fact]
        public void Test_Update_03()
        {
            var userInfo = new UserInfo
            {
                Name = "张强",
                Sex = 2
            };

            var builder = SqlBuilder
                            .Update<UserInfo>(() =>
                                userInfo, isEnableNullValue: true)
                            .Where(u =>
                                u.Id == 1);

            Assert.Equal("UPDATE Base_UserInfo SET Sex = @p__1,Name = @p__2,Email = NULL WHERE Id = @p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改4
        /// </summary>
        [Fact]
        public void Test_Update_04()
        {
            var builder = SqlBuilder
                            .Update<UserInfo>(() =>
                                new
                                {
                                    Sex = 1,
                                    Email = "123456@qq.com"
                                })
                            .Where(u =>
                                u.Id == 1);

            Assert.Equal("UPDATE Base_UserInfo SET Sex = @p__1,Email = @p__2 WHERE Id = @p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改5
        /// </summary>
        [Fact]
        public void Test_Update_05()
        {
            var builder = SqlBuilder
                            .Update<Class>(() =>
                                new
                                {
                                    UserId = 1,
                                    Name = "123456@qq.com"
                                }, DatabaseType.MySql)
                            .Where(u =>
                                u.CityId == 1);

            Assert.Equal("UPDATE Base_Class SET Name = ?p__1 WHERE CityId = ?p__2", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改6
        /// </summary>
        [Fact]
        public void Test_Update_06()
        {
            var builder = SqlBuilder
                            .Update<Class>(() =>
                                new Class
                                {
                                    UserId = 1,
                                    Name = "123456@qq.com"
                                }, DatabaseType.MySql)
                            .Where(u =>
                                u.CityId == 1);

            Assert.Equal("UPDATE Base_Class SET Name = ?p__1 WHERE CityId = ?p__2", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改7
        /// </summary>
        [Fact]
        public void Test_Update_07()
        {
            var data = new
            {
                UserId = 1,
                Name = "123456@qq.com"
            };

            var builder = SqlBuilder
                            .Update<Class>(() =>
                                data, DatabaseType.MySql)
                            .Where(u =>
                                u.CityId == 1);

            Assert.Equal("UPDATE Base_Class SET Name = ?p__1 WHERE CityId = ?p__2", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改8
        /// </summary>
        [Fact]
        public void Test_Update_08()
        {
            var data = new UserInfo
            {
                Id = 2,
                Name = "张强"
            };

            var builder = SqlBuilder
                            .Update<UserInfo>(() =>
                                data, DatabaseType.MySql, true)
                            .WithKey(data);

            Assert.Equal("UPDATE Base_UserInfo SET Sex = ?p__1,Name = ?p__2,Email = NULL WHERE Id = ?p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改9
        /// </summary>
        [Fact]
        public void Test_Update_09()
        {
            var data = new UserInfo
            {
                Id = 2,
                Name = "张强"
            };

            var builder = SqlBuilder
                            .Update<UserInfo>(() =>
                                data, DatabaseType.MySql, isEnableFormat: true)
                            .WithKey(2);

            Assert.Equal("UPDATE `Base_UserInfo` SET `Sex` = ?p__1,`Name` = ?p__2 WHERE `Id` = ?p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改10
        /// </summary>
        [Fact]
        public void Test_Update_10()
        {
            var ids = new[] { 1, 2, 3, 4, 5 };

            var builder = SqlBuilder
                            .Update<UserInfo>(() =>
                                new UserInfo
                                {
                                    Sex = 1,
                                    Email = "123456@qq.com"
                                })
                            .Where(u =>
                                ids.Contains(u.Id.Value));

            Assert.Equal("UPDATE Base_UserInfo SET Sex = @p__1,Email = @p__2 WHERE Id IN (@p__3,@p__4,@p__5,@p__6,@p__7)", builder.Sql);
            Assert.Equal(7, builder.Parameters.Count);
        }

        /// <summary>
        /// 修改11
        /// </summary>
        [Fact]
        public void Test_Update_11()
        {
            var dic = new Dictionary<string, object>
            {
                ["Sex"] = 1,
                ["Email"] = "123456@qq.com"
            };

            var builder = SqlBuilder
                            .Update<UserInfo>(() => dic)
                            .Where(u =>
                                u.Id == 1);

            Assert.Equal("UPDATE Base_UserInfo SET Sex = @p__1,Email = @p__2 WHERE Id = @p__3", builder.Sql);
            Assert.Equal(3, builder.Parameters.Count);
        }
    }
}