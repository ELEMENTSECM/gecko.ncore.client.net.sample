using System;
using Gecko.NCore.Client;
using Gecko.NCore.Client.Documents;
using Gecko.NCore.Client.Functions;
using Gecko.NCore.Client.Metadata;
using Gecko.NCore.Client.ObjectModel;

namespace EphorteTests
{
	public static class ServiceModelContextFactory
	{
        public static Uri GetServicesBaseUrl()
        {
            return new Uri(Configuration.ServicesBaseUrl);
        }

		public static IEphorteContext Create(EphorteContextIdentity ephorteContextIdentity)
		{
			var servicesBaseUrl = GetServicesBaseUrl();

			var objectModelAdapter = CreateObjectModelAdapter(ephorteContextIdentity, servicesBaseUrl);
			var documentsAdapter = CreateDocumentsAdapter(ephorteContextIdentity, servicesBaseUrl);
			const IFunctionsAdapter functionsAdapter = null;
            const IMetadataAdapter metadataAdapter = null;
            return new EphorteContext(objectModelAdapter, functionsAdapter, documentsAdapter, metadataAdapter);
		}

	    private static IObjectModelAdapter CreateObjectModelAdapter(EphorteContextIdentity ephorteContextIdentity, Uri servicesBaseUrl)
	    {
	        var objectModelSettings = new ClientSettings
	            {
                    Address = new Uri(servicesBaseUrl, "services/objectmodel/v3/en/ObjectModelService.svc").ToString(),
                    EndpointName = "ObjectModelServiceV3En_WSHttpBinding"
	            };

	        return new Gecko.NCore.Client.ObjectModel.V3.En.ObjectModelAdapterV3En(ephorteContextIdentity, objectModelSettings);
		}

		private static IDocumentsAdapter CreateDocumentsAdapter(EphorteContextIdentity ephorteContextIdentity, Uri servicesBaseUrl)
		{
            var documentServiceSettings = new ClientSettings
	            {
                    Address = new Uri(servicesBaseUrl, "services/documents/v3/DocumentService.svc").ToString(),
                    EndpointName = "BasicHttpBinding_DocumentService"
	            };

		    return new Gecko.NCore.Client.Documents.V3.DocumentsAdapter(ephorteContextIdentity, documentServiceSettings);
		}
    }
}