using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using System.Threading.Tasks;
using System.Linq;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndustryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public IndustryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetIndustries()
        {
            var industries = await _context.Industries
                .Select(i => new { id = i.IndustryId, name = i.Name })
                .ToListAsync();
            return Ok(industries);
        }
    }
}
