using frutas;
using Microsoft.EntityFrameworkCore;

public class FruitDb(DbContextOptions<FruitDb> options) : DbContext(options)
{

    public DbSet<Fruit> Fruits => Set<Fruit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Fruit>()
            .ToTable("fruta", "padonde");
    }
}
