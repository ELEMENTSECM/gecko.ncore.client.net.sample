using Gecko.NCore.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EphorteTests
{
    public class EphorteUnitTestBase
    {
        private IEphorteContext _ephorteContext;
        protected IEphorteContext EphorteContext
        {
            get { return _ephorteContext; }
            set
            {
                _ephorteContext = value;
                OnEphorteContextChanged(value);
            }
        }

        protected virtual void OnEphorteContextChanged(IEphorteContext ephorteContext) { }

        protected EphorteContextIdentity EphorteContextIdentity60099331 { get; set; }

        public TestContext TestContext { get; set; }
        protected EphorteContextIdentity EphorteContextIdentity { get; set; }

        [TestInitialize]
        public virtual void TestInitialize()
        {
            LogOnAsConfiguredUser();
        }

        protected void LogOnAsConfiguredUser()
        {
            EphorteContextIdentity = new EphorteContextIdentity
                                         {
                                             Username = Configuration.EphorteUsername,
                                             Password = Configuration.EphortePassword,
                                             Role = Configuration.EphorteRole,
                                             Database = Configuration.EphorteDatabase,
                                             ExternalSystemName = Configuration.ExternalSystemName
                                         };

            EphorteContext = ServiceModelContextFactory.Create(EphorteContextIdentity);
        }
    }
}