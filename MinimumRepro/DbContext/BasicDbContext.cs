using Microsoft.EntityFrameworkCore;
using Web.Entities;

namespace Web.DbContext;

public class BasicDbContext: Microsoft.EntityFrameworkCore.DbContext
{
    public BasicDbContext()
    {
        
    }

    public BasicDbContext(DbContextOptions<BasicDbContext> options) : base(options)
    {
        
    }
    
    public virtual DbSet<BasicEntity> BasicEntities { get; set; } = null!;
}