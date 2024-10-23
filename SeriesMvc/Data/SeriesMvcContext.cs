using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SeriesMvc.Models;

namespace SeriesMvc.Data
{
    public class SeriesMvcContext : DbContext
    {
        public SeriesMvcContext (DbContextOptions<SeriesMvcContext> options)
            : base(options)
        {
        }

        public DbSet<SeriesMvc.Models.Movie> Movie { get; set; } = default!;
        public DbSet<SeriesMvc.Models.Actor> Actor { get; set; } = default!;
        public DbSet<SeriesMvc.Models.Category> Category { get; set; } = default!;
    }
}
