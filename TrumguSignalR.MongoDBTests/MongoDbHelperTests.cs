using System;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrumguSignalR.Model;
using TrumguSignalR.Model.MongoModel;
using TrumguSignalR.MongoDB;

namespace TrumguSignalR.MongoDBTests
{
    [TestClass()]
    public class MongoDbHelperTests
    {
        
        private MongoDbHelper _dal = new MongoDbHelper(ConfigurationManager.AppSettings["MongoDBConStringEncrypt"], "stock",true,true);
        [TestMethod()]
        public void MongoDbHelperTest()
        {
            
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateCollectionIndexTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateCollectionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateCollectionTest1()
        {
            Assert.Fail();
        }

//        [TestMethod()]
//        public void FindTest()
//        {
//            var list = _dal.Get<stock_signal>("stock_signal", m => true, 100, 2, null);
//           
//            Assert.IsTrue(list.Count==2);
//        }
//
//        [TestMethod()]
//        public void FindTest1()
//        {
//            var count = _dal.Find<stock_signal>(m => true).Count();
//            Assert.IsTrue(count>0);
//        }

        [TestMethod()]
        public void FindByPageTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindByPageTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertManyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InsertManyTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateTest2()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateTest3()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateManyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateManyTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteManyTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DeleteManyTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ClearCollectionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FindTest2()
        {
            Assert.Fail();
        }
    }
}