using Microsoft.EntityFrameworkCore;
using SpellCrafter.Models.DbClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCrafter.Models
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<Addon> Addons => Set<Addon>();
        public ApplicationContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
