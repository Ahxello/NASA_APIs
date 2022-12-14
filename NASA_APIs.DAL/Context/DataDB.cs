using Microsoft.EntityFrameworkCore;
using NASA_APIs.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASA_APIs.DAL.Context
{
    public class DataDB : DbContext
    {
        public DbSet<DataValue> Values { get; set; } 
        public DbSet<DataSource> Sources { get; set; }
        public DbSet<DataAPODValues> APODValues { get; set; }
        public DataDB(DbContextOptions<DataDB> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);
            model.Entity<DataSource>()
                .HasMany<DataValue>()
                .WithOne(v => v.Source)
                .OnDelete(DeleteBehavior.Cascade);
            model.Entity<DataSource>()
               .HasMany<DataAPODValues>()
               .WithOne(v => v.Source)
               .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
