using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using razorgripassignmentapi.Data.DTOs;
using razorgripassignmentapi.Data.Models;

namespace razorgripassignmentapi.Data.DbContext
{
    public class RazorgripDbContext:IdentityDbContext<ApplicationUser>
    {
        public RazorgripDbContext(DbContextOptions<RazorgripDbContext> options)
           : base(options)
        {

        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

        }
    }
}
