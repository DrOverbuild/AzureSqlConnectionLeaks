using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Web.Entities;

namespace MinimumRepro.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _configuration;
    public BasicEntity? Event { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task OnGet()
    {
        await using var dbConn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        dbConn.Open();
        await using var command = dbConn.CreateCommand();
        command.CommandText = "SELECT TOP 1 [Id], [RecordDate], [Active], [Description] FROM BasicEntities";
        await using var reader = await command.ExecuteReaderAsync();

        if (!reader.Read())
        {
            return;
        }

        Event = new BasicEntity
        {
            Id = reader.GetInt32(0),
            RecordDate = reader.GetDateTime(1),
            Active = reader.GetBoolean(2),
            Description = reader.GetString(3)
        };
    }
}