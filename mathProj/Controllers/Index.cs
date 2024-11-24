using BL.Serives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace mathProj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndexController : ControllerBase
    {
        private readonly IndexService _indexService;

        public IndexController(IndexService indexService)
        {
            _indexService = indexService;
        }
        // GET: api/index
        [HttpGet]
        public ActionResult Index()
        {
            //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
            //var data = _indexService.UserBalancer(""); // Call the service method
            return Ok();
        }

        // GET: api/index/details/5
        [HttpGet("details/{id}")]
        public ActionResult Details(int id)
        {
            return Ok($"Details View for ID: {id}");
        }

        // GET: api/index/create
        [HttpGet("create")]
        public ActionResult Create()
        {
            return Ok("Create View");
        }

        // POST: api/index/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return BadRequest();
            }
        }

        // GET: api/index/edit/5
        [HttpGet("edit/{id}")]
        public ActionResult Edit(int id)
        {
            return Ok($"Edit View for ID: {id}");
        }

        // POST: api/index/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [FromForm] IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return BadRequest();
            }
        }

        // GET: api/index/delete/5
        [HttpGet("delete/{id}")]
        public ActionResult Delete(int id)
        {
            return Ok($"Delete View for ID: {id}");
        }

        // POST: api/index/delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, [FromForm] IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
