using EPiServer.Core;

namespace EPiServer.Azure.StorageExplorer.Sample.Models.Pages
{
    public interface IHasRelatedContent
    {
        ContentArea RelatedContentArea { get; }
    }
}