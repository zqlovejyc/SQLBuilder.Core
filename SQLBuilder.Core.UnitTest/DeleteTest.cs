using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLBuilder.Core.Entry;

namespace SQLBuilder.Core.UnitTest
{
    [TestClass]
    public class DeleteTest
    {
        /// <summary>
        /// 删除1
        /// </summary>
        [TestMethod]
        public void Test_Delete_01()
        {
            var builder = SqlBuilder.Delete<UserInfo>();
            Assert.AreEqual("DELETE FROM [Base_UserInfo]", builder.Sql);
            Assert.AreEqual(0, builder.Parameters.Count);
        }

        /// <summary>
        /// 删除2
        /// </summary>
        [TestMethod]
        public void Test_Delete_02()
        {
            var id = 3;
            var builder = SqlBuilder.Delete<UserInfo>().Where(u => u.Id == id);
            Assert.AreEqual("DELETE FROM [Base_UserInfo] WHERE [Id] = @p__1", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 删除3
        /// </summary>
        [TestMethod]
        public void Test_Delete_03()
        {
            var builder = SqlBuilder.Delete<UserInfo>().Where(u => u.Id > 1 && u.Id < 3);
            Assert.AreEqual("DELETE FROM [Base_UserInfo] WHERE [Id] > @p__1 AND [Id] < @p__2", builder.Sql);
            Assert.AreEqual(2, builder.Parameters.Count);
        }

        /// <summary>
        /// 删除4
        /// </summary>
        [TestMethod]
        public void Test_Delete_04()
        {
            var user = new UserInfo { Id = 2 };
            var builder = SqlBuilder.Delete<UserInfo>().WithKey(user);
            Assert.AreEqual("DELETE FROM [Base_UserInfo] WHERE [Id] = @p__1", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }

        /// <summary>
        /// 删除5
        /// </summary>
        [TestMethod]
        public void Test_Delete_05()
        {
            var builder = SqlBuilder.Delete<UserInfo>().WithKey(2);
            Assert.AreEqual("DELETE FROM [Base_UserInfo] WHERE [Id] = @p__1", builder.Sql);
            Assert.AreEqual(1, builder.Parameters.Count);
        }
    }
}