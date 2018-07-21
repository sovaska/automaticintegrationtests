using Microsoft.AspNetCore.Mvc;
using Countries.Web.Contracts;

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

        [HttpGet]
        public ActionResult<Models.Metadata> GetActions()
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