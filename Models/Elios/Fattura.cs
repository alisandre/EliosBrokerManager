using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliosBrokerManager.Models.Elios
{
    [Table("fatture")]
    public class Fattura
    {
        [Key]
        [Column("id_fattura")]
        public int IdFattura { get; set; }

        [Column("fattura")]
        public byte[] FatturaBlob { get; set; }

        [Column("data_fattura")]
        public DateTime? DataFattura { get; set; }

        [Column("num_fattura")]
        [StringLength(10)]
        public string NumFattura { get; set; }
    }
}
