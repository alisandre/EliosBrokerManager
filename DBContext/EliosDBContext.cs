using EliosBrokerManager.Models.Elios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace EliosBrokerManager.DBContext
{
    public class EliosDBContext: DbContext
    {
        public DbSet<Accettazione> Accettazione { get; set; }
        public DbSet<AccettazioneDett> AccettazioneDett { get; set; }
        public DbSet<Fattura> Fattura { get; set; }
        public DbSet<Paziente> Paziente { get; set; }
        public DbSet<Referto> Referto { get; set; }
        public DbSet<TabEsame> TabEsame { get; set; }
        public DbSet<TabStato> TabStato { get; set; }

        protected readonly IConfiguration _config;

        public EliosDBContext(IConfiguration configuration)
        {
            _config = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string eliosCS = _config.GetConnectionString("csEliosBroker");
            var mySqlVersion = new MySqlServerVersion(new Version(8, 1, 0));
            options.UseMySql(
                eliosCS,
                mySqlVersion,
                mySqlOptions => mySqlOptions.CommandTimeout(120)
            );
        }
    }
}
