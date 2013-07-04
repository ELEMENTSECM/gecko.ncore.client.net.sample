using System;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using Gecko.NCore.Client.ObjectModel.V3.En;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace EphorteTests
{
    [TestClass]
    public class ClassificationTest : EphorteUnitTestBase
    {
        [TestMethod]
        public void QueryForClassThatDoesNotExist_ReturnsZeroRecords()
        {
            //Norwegian
            //var query = context.Query<Klassering>().Where(x => x.Ordningsverdi.Tittel == "12345678910").Count();
            
            //var query = EphorteContext.Query<Classification>().Where(x => x.Class.Id == "DoesNotExist");

            //This query would give same result:
            var query = EphorteContext.Query<Classification>().Where(x => x.ClassId == "DoesNotExist");

            var count = query.Count();
            Assert.AreEqual(0, count);
        } 
        
        [TestMethod]
        public void QueryForClassificationThatIsInUse_DoesNotReturnsZeroRecords()
        {
            var firstClassification = EphorteContext.Query<Classification>().First(c => c.Description != null);

            Console.WriteLine(firstClassification.ClassId);

            var query1 = EphorteContext.Query<Classification>().Where(x => x.ClassId == firstClassification.ClassId);

            Assert.AreNotEqual(0, query1.Count(), "Query 1");

            //WILL Currently fail:
            //var query2 = EphorteContext.Query<Classification>().Where(x => x.Class.Id == firstClassification.ClassId);
            //Assert.AreNotEqual(0, query2.Count(), "Query 2");
        }
    }
}
