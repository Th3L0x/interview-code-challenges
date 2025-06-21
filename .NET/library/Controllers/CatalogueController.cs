using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;
using System.Collections;

namespace OneBeyondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogueController : ControllerBase
    {
        private readonly ILogger<CatalogueController> _logger;
        private readonly ICatalogueRepository _catalogueRepository;

        public CatalogueController(ILogger<CatalogueController> logger, ICatalogueRepository catalogueRepository)
        {
            _logger = logger;
            _catalogueRepository = catalogueRepository;   
        }

        [HttpGet]
        [Route("GetCatalogue")]
        public IList<BookStock> Get()
        {
            return _catalogueRepository.GetCatalogue();
        }

        [HttpGet]
        [Route("OnLoan")]
        public IList<BorrowerOnLoan> OnLoan()
        {
            return _catalogueRepository.GetBorrowersOnActiveLoans();
        }

        [HttpPost]
        [Route("SearchCatalogue")]
        public IList<BookStock> Post(CatalogueSearch search)
        {
            return _catalogueRepository.SearchCatalogue(search);
        }
    }
}