using System.Collections.Generic;

namespace Countries.Web.Models
{
    public class DIMetadata
    {
        public List<ServiceMetadata> Problems { get; set; } = new List<ServiceMetadata>();
        public List<ServiceMetadata> NotValidatedTypes { get; set; } = new List<ServiceMetadata>();
        public List<string> ValidTypes { get; set; } = new List<string>();
    }
}
