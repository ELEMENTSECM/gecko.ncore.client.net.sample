using System;
using System.Linq;
using Gecko.NCore.Client;
using Gecko.NCore.Client.ObjectModel.V3.En;
using Gecko.NCore.Client.Querying;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace EphorteTests
{
    [TestClass]
    public class UnitTest : EphorteUnitTestBase
    {
        static class DocumentLinkTypeId
        {
            public const string MainDocument = "H"; //Hoveddokument
            public const string Attachment = "V";   //Vedlegg
        }

        static class VariantFormatId
        {
            public const string Production = "P"; //Production
        }


        static class FileFormatId //NOTE: Subset of FileFormat (db-table LAGRFORMAT)
        {
            public const string RaText = "RA-TEKST"; //(TXT)
            public const string RaPdf = "RA-PDF";    //(PDF)
        }

        const string TextFileContent = "The file is checked in as part of the batch insert.";
        const string MainFileName = "TestMainDocument_NOTE";

        const string SecondAttachmentFileName = "TestAttachmentDocument.pdf";

        const int PredefinedQueryId = -30;//Recent cases


        //Support-case: 2013/190 
        //http://ephorte/ephorte/?url=shared%2faspx%2fDefault%2fdetails.aspx%3ff%3dViewSA%26SA_ID%3d14327
        //http://ephorte/ephorte/shared/aspx/GetDoc.aspx?JP_ID=29152&JP_HDOKTYPE_G=PDF&WorkFolder=&EphorteDb=EPHORTE

        [TestMethod]
        public void CreateRegistryEntryWithPdfDocument()
        {
            CreateRegistryEntryWithDocument(false, FileFormatId.RaPdf);
        }
        
        [TestMethod]
        public void CreateRegistryEntryWithPdfDocumentAndOneAttachment()
        {
            CreateRegistryEntryWithDocument(true, FileFormatId.RaPdf);
        }

        [TestMethod]
        public void CreateRegistryEntryWithTxtDocument()
        {
            CreateRegistryEntryWithDocument(false, FileFormatId.RaText);
        }

        [TestMethod]
        public void CreateRegistryEntryWithTxtDocumentAndOneAttachment()
        {
            CreateRegistryEntryWithDocument(true, FileFormatId.RaText);
        }

        [TestMethod]
        public void VerifyRecipientFunctionalityU()
        {
            CreateTwoRegistryEntriesOfType("U");
        }

        [TestMethod]
        public void VerifyRecipientFunctionalityI()
        {
            CreateTwoRegistryEntriesOfType("I");
        }

        [TestMethod]
        public void VerifyRecipientFunctionalityX()
        {
            CreateTwoRegistryEntriesOfType("X");
        }

        [TestMethod]
        public void PredefinedSearchQuery()
        {
            var result = EphorteContext.Query<Query>().FirstOrDefault(q => q.Id == PredefinedQueryId);
            if (result != null)
            {
                var predefQueryResult = (from c in EphorteContext.Query<Case>()
                                         where QueryContext.Current.StoredQuery(c, PredefinedQueryId)
                                         select c as DataObject).Any();
                var regularQueryResult = (from c in EphorteContext.Query<Case>()
                                          where c.CaseDate < DateTime.Now
                                          select c).Any();
                Assert.AreEqual(predefQueryResult, regularQueryResult, "Expected predefined query {0} will return some Cases.", PredefinedQueryId);
            }
            else
            {
                Assert.Fail("Could not find predefined query for id={0}.", PredefinedQueryId);
            }
        }

        public void CreateTwoRegistryEntriesOfType(string registryEntryTypeId)
        {
            var mainCase = GetMainCase();

            const string moreThanOneRecipientExpectedPostfix = " m.fl.";

            var registryEntry1 = _ephorte.AddRegistryEntryToCase(mainCase, registryEntryTypeId, registryEntryTypeId + " - With one recipient ");
            var registryEntry2 = _ephorte.AddRegistryEntryToCase(mainCase, registryEntryTypeId, registryEntryTypeId + " - With two recipients ");

            if (registryEntryTypeId == "I")
            {
                _ephorte.AddSenderRecipient(registryEntry1, "SenderFN", "LN");

                _ephorte.AddSenderRecipient(registryEntry2, "SenderFN1", "LN1");
                _ephorte.AddSenderRecipient(registryEntry2, "SenderFN2", "LN2");
            }
            if(registryEntryTypeId == "U")
            {
                _ephorte.AddRecipient(registryEntry1, "RecipientFN", "LN");

                _ephorte.AddRecipient(registryEntry2, "RecipientFN1", "LN1");
                _ephorte.AddRecipient(registryEntry2, "RecipientFN2", "LN2");
            }

            if (registryEntryTypeId == "X")
            {
                var r1 = _ephorte.AddSenderRecipient(registryEntry1, "InternalFN", "LN");

                var r2 = _ephorte.AddSenderRecipient(registryEntry2, "InternalFN1", "LN1");
                var r3 = _ephorte.AddSenderRecipient(registryEntry2, "InternalFN2", "LN2");
                foreach (var recipient in new[] { r1, r2, r3 })
                {
                    //TODO: Fix hardcoded values
                    recipient.AdministrativeUnitId = Configuration.RecipientXAdministrativeUnitId;
                    recipient.RegistryManagementUnitId = Configuration.RecipientXRegistryManagementUnitId;
                }
            }

            EphorteContext.SaveChanges();

            //First registry-entry has ONE recipient, so registryEntry.SenderRecipient must NOT end with " m.fl."

            string registryEntry1SenderRecipient = GetRegistryEntry(registryEntry1).SenderRecipient;
            Console.WriteLine("SenderRecipient: " + registryEntry1SenderRecipient);
            Assert.IsNotNull(registryEntry1SenderRecipient, "SenderRecipient must not be null");
            Assert.IsFalse(registryEntry1SenderRecipient.EndsWith(moreThanOneRecipientExpectedPostfix), "registryEntry.SenderRecipient (one recipient)");

            //Second registry-entry has two recipients, and registryEntry.SenderRecipient DOES end with " m.fl."
            Assert.IsTrue(GetRegistryEntry(registryEntry2).SenderRecipient.EndsWith(moreThanOneRecipientExpectedPostfix), "registryEntry.SenderRecipient (multiple)");
        }

        public void CreateRegistryEntryWithDocument(bool addAttachment, string fileformatId)
        {
            var mainCase = GetMainCase();
            const string registryEntryTypeId = "U"; //Currently hardcoded

            //Add registry-entry to case
            var registryEntry = _ephorte.AddRegistryEntryToCase(mainCase, registryEntryTypeId, "One document - " + (addAttachment ? "ONE" : "NO") + " attachment (" + fileformatId + ")");
            if (registryEntryTypeId == "U")
            {
                //If type is U, then there MUST be an at least one external recipient
                _ephorte.AddRecipient(registryEntry, "FirstName1", "LastName1");                
            }
            DocumentDescription documentDescription1;
            DocumentObject documentObject1;
            AddMainDocument(registryEntry, out documentDescription1, out documentObject1);

            //Save changes (and attach uploaded document)
            EphorteContext.SaveChanges();

            LogEphorteWebLink(registryEntry);

            //The checksum is now updated:
            Assert.IsNotNull(documentObject1.Checksum, "documentObject1.Checksum");


            //---------------------------------------------------
            //Add a new version of the main document
            //---------------------------------------------------
            var documentObject2 = AddNewVersionOfDocument(documentDescription1, fileformatId);

            //Fetch the updated document-object to verify that the checsum is set.
            var documentObject2Reloaded = GetDocumentObject(documentObject2);
            Assert.IsNotNull(documentObject2Reloaded, "documentObject2Reloaded");
            Assert.IsNotNull(documentObject2Reloaded.Checksum, "documentObject2Reloaded.Checksum");

            //NOTE:
            //At this point we have a case, a registry-entry with ONE external recipient and ONE main-document. The main-document has two versions.
            Assert.AreEqual(0, GetRegistryEntry(registryEntry).NumberOfSubDocuments, "NumberOfSubDocuments");

            if (!addAttachment)
                return;
            //---------------------------------------------------
            //Add attachment
            var documentDescription2 = AddAttachment(registryEntry);
            EphorteContext.SaveChanges();

            //Verify that the FileFormatId has been resolved correctly: (.PDF -> RA-PDF)
            Assert.AreEqual(FileFormatId.RaPdf, GetDocumentObject(documentDescription2.Id, VariantFormatId.Production, 1).FileformatId, "FileformatId documentDescription2");

            //After the attachment is added, NumberOfSubDocuments should be 1:
            Assert.AreEqual(1, GetRegistryEntry(registryEntry).NumberOfSubDocuments, "NumberOfSubDocuments");
        }

        private DocumentDescription AddAttachment(RegistryEntry registryEntry)
        {
            //Add document description
            var documentDescription2 = _ephorte.AddDocumentDescription();
            documentDescription2.DocumentTitle = "Attachment " + DateTime.Now;
            //Add a document-object with the given document-description. 
            var documentObject3 = _ephorte.AddDocumentObject(documentDescription2, 1, VariantFormatId.Production);

            //Add a registry-entry-document with the given registry-entry and document-description.
            _ephorte.AddRegistryEntryDocument(registryEntry, documentDescription2, DocumentLinkTypeId.Attachment);

            //The (attachment) document is uploded, and will be checked in when doing SaveChanges
            _ephorte.UploadTextDocument(documentObject3, SecondAttachmentFileName, TextFileContent + " - attachment");
            return documentDescription2;
        }

        private DocumentObject AddNewVersionOfDocument(DocumentDescription documentDescription1, string fileformatId)
        {
            var documentObject = _ephorte.AddDocumentObject(documentDescription1.Id, 2, VariantFormatId.Production, MainFileName, fileformatId);

            EphorteContext.SaveChanges();

            Assert.AreEqual(fileformatId, GetDocumentObject(documentDescription1.Id, VariantFormatId.Production, 2).FileformatId, "FileformatId documentDescription1");

            //This time we add a text-document to an existing document-description
            _ephorte.CheckinTextDocument(documentDescription1.Id, documentObject.VariantFormatId, documentObject.VersionNumber, TextFileContent + " - v2");

            //Note that the checksum is not yet calculated "locally".
            Assert.IsNull(documentObject.Checksum);

            return GetDocumentObject(documentObject); //Re-fetch document-object
        }

        private void AddMainDocument(RegistryEntry registryEntry,
                                     out DocumentDescription documentDescription1,
                                     out DocumentObject documentObject1)
        {
            //Add document description
            documentDescription1 = _ephorte.AddDocumentDescription();

            //Add a document-object with a given document-description
            documentObject1 = _ephorte.AddDocumentObject(documentDescription1, 1, VariantFormatId.Production);

            //Add a registry-entry-document with the given registry-entry and document-description. This is the main document (H
            _ephorte.AddRegistryEntryDocument(registryEntry, documentDescription1, DocumentLinkTypeId.MainDocument);

            //This first version of the (main) document is uploded before any the pending changes are saved, and will be checked in when doing SaveChanges
            _ephorte.UploadTextDocument(documentObject1, MainFileName, TextFileContent + " - v1");
        }

        private RegistryEntry GetRegistryEntry(RegistryEntry registryEntry)
        {
            var query = from o in EphorteContext.Query<RegistryEntry>()
                        where o.Id == registryEntry.Id
                        select o;
            return query.FirstOrDefault();
        }


        private static Case _mainCase;
        private Case GetMainCase()
        {
            try
            {
                if (_mainCase != null)
                    return _mainCase;
                _mainCase = _ephorte.AddCase("TestCase " + DateTime.Now);
                EphorteContext.SaveChanges();
                return _mainCase;
            }
            finally
            {
                if (_mainCase != null)
                    LogEphorteWebLink(_mainCase);
            }
        }

        private const string EphorteUrl = "http://localhost/Ephorte";
        //private const string EphorteUrl = "http://server.name/site-url";
        private static void LogEphorteWebLink(RegistryEntry registryEntry)
        {
            Console.WriteLine("Link to registry-entry in ephorteweb: " + EphorteUrl + "/locator/?SHOW=JP&JP_ID=" + registryEntry.Id);
        }

        private static void LogEphorteWebLink(Case @case)
        {
            Console.WriteLine("Link to case in ephorteweb: " + EphorteUrl + "/locator/?SHOW=SA&SA_ID=" + @case.Id);
        }

        private DocumentObject GetDocumentObject(int documentDescriptionId, string variantFormatId, int versionNumber)
        {
            var query = from o in EphorteContext.Query<DocumentObject>()
                        where o.DocumentDescriptionId == documentDescriptionId &&
                              o.VariantFormatId == variantFormatId &&
                              o.VersionNumber == versionNumber
                        select o;
            return query.FirstOrDefault();
        }

        private DocumentObject GetDocumentObject(DocumentObject documentObject1)
        {
            return GetDocumentObject(documentObject1.DocumentDescriptionId, documentObject1.VariantFormatId, documentObject1.VersionNumber);
        }

        protected override void OnEphorteContextChanged(IEphorteContext ephorteContext)
        {
            _ephorte = new EphorteContextHelper(ephorteContext);
        }

        private EphorteContextHelper _ephorte;
    }
}
