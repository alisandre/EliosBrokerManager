using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliosBrokerManager.Models.Jibria
{
    [Table("IMPEGN")]
    public class Impegnativa
    {

        [Key]
        [Column("CODIMP")]
        public int CodiceImpegnativa { get; set; }

        [Column("NUMERORIC")]
        public string NumeroImpegnativa { get; set; } = null!;

        [Column("DATAACC")]
        public DateTime DataImpegnativa { get; set; } = DateTime.UtcNow;
    }
}
