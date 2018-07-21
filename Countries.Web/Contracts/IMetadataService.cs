using Countries.Web.Models;

namespace Countries.Web.Contracts
{
    public interface IMetadataService
    {
        Metadata GetMetadata();
    }
}