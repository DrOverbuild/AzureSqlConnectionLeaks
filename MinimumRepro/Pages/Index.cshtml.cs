using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.DbContext;
using Web.Entities;

namespace MinimumRepro.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly BasicDbContext _context;
    
    public BasicEntity? Event { get; set; }

    public IndexModel(ILogger<IndexModel> logger, BasicDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task OnGet()
    {
        Event = await _context.BasicEntities.FindAsync(1);
    }
}