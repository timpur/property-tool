using System;
using Microsoft.EntityFrameworkCore;

namespace PropertyTool.DataBase
{
    public class PropertyContext : DbContext
    {
        public DbSet<Property> Properties { get; set; }

        public PropertyContext(DbContextOptions options) : base(options)
        {
        }
    }
}