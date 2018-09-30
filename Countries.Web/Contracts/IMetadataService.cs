using Countries.Web.Models;
using System;
using System.Collections.Generic;

namespace Countries.Web.Contracts
{
    public interface IMetadataService
    {
        Metadata GetMetadata();
        DIMetadata GetDependencyInjectionProblems(IServiceProvider serviceProvider);
    }
}