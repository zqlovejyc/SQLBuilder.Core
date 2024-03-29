﻿using SQLBuilder.Core.Entry;
using SQLBuilder.Core.UnitTest.Models;
using System.Data;
using Xunit;

namespace SQLBuilder.Core.UnitTest
{
    public class DeleteTest
    {
        /// <summary>
        /// 删除1
        /// </summary>
        [Fact]
        public void Test_Delete_01()
        {
            var builder = SqlBuilder.Delete<UserInfo>();

            Assert.Equal("DELETE FROM Base_UserInfo", builder.Sql);
            Assert.Empty(builder.Parameters);
        }

        /// <summary>
        /// 删除2
        /// </summary>
        [Fact]
        public void Test_Delete_02()
        {
            var id = 3;
            var builder = SqlBuilder
                            .Delete<UserInfo>()
                            .Where(u =>
                                u.Id == id);

            Assert.Equal("DELETE FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);

            Assert.True(builder.Parameters["@p__1"].type.IsDbType);
            Assert.Equal(DbType.Int64, builder.Parameters["@p__1"].type.DbType);
        }

        /// <summary>
        /// 删除3
        /// </summary>
        [Fact]
        public void Test_Delete_03()
        {
            var builder = SqlBuilder
                            .Delete<UserInfo>()
                            .Where(u =>
                                u.Id > 1 &&
                                u.Id < 3);

            Assert.Equal("DELETE FROM Base_UserInfo WHERE Id > @p__1 AND Id < @p__2", builder.Sql);
            Assert.Equal(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 删除4
        /// </summary>
        [Fact]
        public void Test_Delete_04()
        {
            var user = new UserInfo { Id = 2 };

            var builder = SqlBuilder
                            .Delete<UserInfo>()
                            .WithKey(user);

            Assert.Equal("DELETE FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
        }

        /// <summary>
        /// 删除5
        /// </summary>
        [Fact]
        public void Test_Delete_05()
        {
            var builder = SqlBuilder
                            .Delete<UserInfo>()
                            .WithKey(2);

            Assert.Equal("DELETE FROM Base_UserInfo WHERE Id = @p__1", builder.Sql);
            Assert.Single(builder.Parameters);
        }

        /// <summary>
        /// 删除6
        /// </summary>
        [Fact]
        public void Test_Delete_06()
        {
            var builder = SqlBuilder
                            .Delete<MultiplePrimaryKeyEntity>()
                            .WithKey(2, "test");

            Assert.Equal("DELETE FROM MultipleKey WHERE Id = @p__1 AND CompanyId = @p__2", builder.Sql);
            Assert.Equal(2, builder.Parameters["@p__1"].data);
            Assert.Equal("test", builder.Parameters["@p__2"].data);
        }
    }
}