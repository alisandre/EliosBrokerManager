using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliosBrokerManager.Models.Elios
{
    [Table("referti")]
    public class Referto
    {
        [Key]
        [Column("id_referto_broker")]
        public int IdRefertoBroker { get; set; }

        [Column("id_referto_elios")]
        public int? IdRefertoElios { get; set; }

        [Column("id_referto_esterno")]
        public int? IdRefertoEsterno { get; set; }

        [Column("ref_uid_elios")]
        [StringLength(20)]
        public string RefUidElios { get; set; }

        [Column("ref_utente")]
        [StringLength(75)]
        public string RefUtente { get; set; }

        [Column("ref_data")]
        public DateTime? RefData { get; set; }

        [Column("ref_html")]
        public byte[] RefHtml { get; set; }

        [Column("ref_pdf")]
        public byte[] RefPdf { get; set; }

        [Column("elios_note")]
        [StringLength(255)]
        public string EliosNote { get; set; }

        [Column("elios_stato")]
        public byte? EliosStato { get; set; }

        [Column("elios_stato_data")]
        public DateTime? EliosStatoData { get; set; }

        [Column("esterno_note")]
        [StringLength(255)]
        public string EsternoNote { get; set; }

        [Column("esterno_stato")]
        public byte? EsternoStato { get; set; }

        [Column("esterno_stato_data")]
        public DateTime? EsternoStatoData { get; set; }

        [Column("ins_data")]
        public DateTime InsData { get; set; } = DateTime.UtcNow;
    }
}
