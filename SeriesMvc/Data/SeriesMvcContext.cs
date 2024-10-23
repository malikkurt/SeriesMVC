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
        public DbSet<SeriesMvc.Models.MovieActor> MovieActor { get; set; } = default!;
        public DbSet<SeriesMvc.Models.MovieCategory> MovieCategory { get; set; } = default!;
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MovieActor>()
                .HasKey(ma => new { ma.MovieId, ma.ActorId });

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);

            modelBuilder.Entity<MovieCategory>()
                .HasKey(mc => new { mc.MovieId, mc.CategoryId });

            modelBuilder.Entity<MovieCategory>()
                .HasOne(mc => mc.Movie)
                .WithMany(m => m.MovieCategories)
                .HasForeignKey(mc => mc.MovieId);

            modelBuilder.Entity<MovieCategory>()
                .HasOne(mc => mc.Category)
                .WithMany(c => c.MovieCategories)
                .HasForeignKey(mc => mc.CategoryId);
        }

    }
}
