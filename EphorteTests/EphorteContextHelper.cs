using System.IO;
using Gecko.NCore.Client;
using Gecko.NCore.Client.ObjectModel.V3.En;

namespace EphorteTests
{
    class EphorteContextHelper
    {
        private readonly IEphorteContext _ephorteContext;
        public EphorteContextHelper(IEphorteContext ephorteContext)
        {
            _ephorteContext = ephorteContext;
        }

        public Case AddCase(string title)
        {
            var _case = _ephorteContext.Create<Case>();
            _case.Title = title;
            _ephorteContext.Add(_case);
            return _case;
        }

        public RegistryEntry AddRegistryEntryToCase(Case @case, string registryEntryTypeId, string title)
        {
            var registryEntry = _ephorteContext.Create<RegistryEntry>();
            registryEntry.Title = title;
            registryEntry.RegistryEntryTypeId = registryEntryTypeId;
            registryEntry.Case = @case;
            _ephorteContext.Add(registryEntry);
            return registryEntry;
        }

        public DocumentObject AddDocumentObject(DocumentDescription documentDescription, int versionNumber,
                                                string variantFormatId)
        {
            var documentObject = CreateDocumentObject(versionNumber, variantFormatId);

            documentObject.DocumentDescription = documentDescription;
            _ephorteContext.Add(documentObject);
            return documentObject;
        }

        public DocumentObject AddDocumentObject(int documentDescriptionId, int versionNumber, string variantFormatId, string fileName, string fileformatId)
        {
            var documentObject = CreateDocumentObject(versionNumber, variantFormatId);

            documentObject.DocumentDescriptionId = documentDescriptionId;
            documentObject.FilePath = fileName;
            if (fileformatId != null)
            {
                documentObject.FileformatId = fileformatId;
            }
            _ephorteContext.Add(documentObject);
            return documentObject;
        }

        private DocumentObject CreateDocumentObject(int versionNumber, string variantFormatId)
        {
            var documentObject = _ephorteContext.Create<DocumentObject>();
            documentObject.VersionNumber = versionNumber;
            documentObject.VariantFormatId = variantFormatId;
            return documentObject;
        }

        public DocumentDescription AddDocumentDescription()
        {
            var documentDescription = _ephorteContext.Create<DocumentDescription>();
            _ephorteContext.Add(documentDescription);
            return documentDescription;
        }

        public void CheckinTextDocument(int documentDescriptionId, string variantFormatId, int versionNumber,
                                        string fileContent)
        {
            using (var myFile = new MemoryStream())
            {
                using (var myFileWriter = new StreamWriter(myFile))
                {
                    myFileWriter.Write(fileContent);
                    myFileWriter.Flush();

                    myFile.Seek(0, SeekOrigin.Begin);
                    _ephorteContext.Documents.CheckIn(documentDescriptionId, variantFormatId, versionNumber, myFile);
                }
            }
        }

        public void UploadTextDocument(DocumentObject documentObject, string fileName, string fileContent)
        {
            using (var myFile = new MemoryStream())
            {
                using (var myFileWriter = new StreamWriter(myFile))
                {
                    myFileWriter.Write(fileContent);
                    myFileWriter.Flush();

                    myFile.Seek(0, SeekOrigin.Begin);
                    _ephorteContext.Documents.Upload(documentObject, myFile, fileName);
                }
            }
        }


        public void AddRegistryEntryDocument(RegistryEntry registryEntry, DocumentDescription documentDescription,
                                             string documentLinkTypeId)
        {
            var registryEntryDocument = _ephorteContext.Create<RegistryEntryDocument>();
            registryEntryDocument.RegistryEntry = registryEntry;
            registryEntryDocument.DocumentDescription = documentDescription;
            registryEntryDocument.DocumentLinkTypeId = documentLinkTypeId;
            _ephorteContext.Add(registryEntryDocument);
        }

        public void AddRecipient(RegistryEntry registryEntry, string firstName, string lastName)
        {
            var senderRecipient = AddSenderRecipient(registryEntry, firstName, lastName);
            senderRecipient.IsRecipient = true;
        }

        public SenderRecipient AddSenderRecipient(RegistryEntry registryEntry, string firstName, string lastName)
        {
            var senderRecipient = _ephorteContext.Create<SenderRecipient>();
            if (registryEntry.Id == 0)
                senderRecipient.RegistryEntry = registryEntry;
            else
                senderRecipient.RegistryEntryId = registryEntry.Id;

            senderRecipient.Email = firstName + "." + lastName + "@does-not-exist.no";
            senderRecipient.Name = firstName + " " + lastName;
            _ephorteContext.Add(senderRecipient);
            return senderRecipient;
        }
    }
}