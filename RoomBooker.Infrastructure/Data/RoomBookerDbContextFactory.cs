using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RoomBooker.Infrastructure.Data
{
    public class RoomBookerDbContextFactory : IDesignTimeDbContextFactory<RoomBookerDbContext>
    {
        public RoomBookerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RoomBookerDbContext>();

            var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=RoomBookerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            optionsBuilder.UseSqlServer(connectionString);

            return new RoomBookerDbContext(optionsBuilder.Options);
        }
    }
}
