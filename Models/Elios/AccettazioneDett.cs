using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliosBrokerManager.Models.Elios
{ 
    [Table("accettazioni_dett")]
    public class AccettazioneDett
    {
        [Key]
        [Column("id_acc_dett_broker")]
        public int IdAccDettBroker { get; set; }

        [Column("id_acc_broker")]
        public int? IdAccBroker { get; set; }

        [ForeignKey("IdAccBroker")]
        public Accettazione Accettazione { get; set; }

        [Column("id_acc_dett_elios")]
        public int? IdAccDettElios { get; set; }

        [Column("id_esame_broker")]
        public ushort? IdEsameBroker { get; set; }

        [Column("flg_invio_portale")]
        public byte? FlgInvioPortale { get; set; }

        [Column("id_referto_broker")]
        public int? IdRefertoBroker { get; set; }

        [Column("id_studio")]
        [StringLength(64)]
        public string? IdStudio { get; set; }

        [Column("id_macchina")]
        public byte? IdMacchina { get; set; }

        [Column("tipo_esa")]
        [StringLength(1)]
        public string? TipoEsa { get; set; }

        [Column("med_ref")]
        [StringLength(20)]
        public string? MedRef { get; set; }

        [Column("numero_coda")]
        [StringLength(10)]
        public string? NumeroCoda { get; set; }

        [Column("data_prenotazione")]
        public DateTime? DataPrenotazione { get; set; }

        [Column("data_esecuzione")]
        public DateTime? DataEsecuzione { get; set; }

        [Column("accession_number")]
        [StringLength(64)]
        public string? AccessionNumber { get; set; }

        [Column("id_fattura_broker")]
        public int? IdFatturaBroker { get; set; }
        [Column("id_fattura_elios")]
        public int? IdFatturaElios { get; set; }
        [Column("id_fattura_esterno")]
        public int? IdFatturaEsterno { get; set; }

        [Column("elios_note")]
        [StringLength(255)]
        public string? EliosNote { get; set; }

        [Column("elios_stato")]
        public byte? EliosStato { get; set; }

        [Column("elios_stato_data")]
        public DateTime? EliosStatoData { get; set; }

        [Column("esterno_note")]
        [StringLength(255)]
        public string? EsternoNote { get; set; }

        [Column("esterno_stato")]
        public byte? EsternoStato { get; set; }

        [Column("esterno_stato_data")]
        public DateTime? EsternoStatoData { get; set; }

        [Column("ins_data")]
        public DateTime? InsData { get; set; } = DateTime.UtcNow;
    }
}
