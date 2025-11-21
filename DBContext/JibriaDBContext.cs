using EliosBrokerManager.Models.Jibria;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;

namespace EliosBrokerManager.DBContext
{
    public class JibriaDBContext: DbContext
    {
        public DbSet<EliosQueueItem> EliosQueue { get; set; }

        protected readonly IConfiguration _config;

        public JibriaDBContext(IConfiguration configuration)
        {
            _config = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string jibriaCS = _config.GetConnectionString("csJibria");            
            options.UseFirebird(jibriaCS);            
        }
    }
}
