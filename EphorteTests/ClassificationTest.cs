using System.Linq;
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
    }
}
