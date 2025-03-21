using Microsoft.EntityFrameworkCore;
using Test_Project.Server.Models.Tables;

namespace Test_Project.Server.API.Context;

public class EfContext : DbContext
{
    public EfContext(DbContextOptions<EfContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}
