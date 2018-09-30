using Microsoft.AspNetCore.Mvc;
using Countries.Web.Contracts;
using System.Collections.Generic;
using Countries.Web.Models;

namespace Countries.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetadataController : ControllerBase
    {
        private readonly IMetadataService _metadataService;

        public MetadataController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        [HttpGet("dependencyinjection")]
        public ActionResult<DIMetadata> DependencyInjection()
        {
            var results = _metadataService.GetDependencyInjectionProblems(HttpContext.RequestServices);

            return Ok(results);
        }

        [HttpGet("actions")]
        public ActionResult<Metadata> GetActions()
        {
            var result = _metadataService.GetMetadata();
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}