using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliosBrokerManager.Models.Elios
{
    [Table("accettazioni")]
    public class Accettazione
    {
        [Key]
        [Column("id_acc_broker")]
        public int IdAccBroker { get; set; }

        [Column("id_acc_elios")]
        public int? IdAccElios { get; set; }

        [Column("id_acc_esterno")]
        [StringLength(30)]
        public string IdAccEsterno { get; set; }

        [Column("id_paziente_broker")]
        public int IdPazienteBroker { get; set; }

        [Column("id_uo")]
        public byte IdUo { get; set; } // TINYINT

        [Column("data_acc")]
        public DateTime DataAcc { get; set; }

        [Column("flg_mod")]
        public byte FlgMod { get; set; }

        [Column("data_imp")]
        public DateTime? DataImp { get; set; }

        [Column("num_imp_1")]
        [StringLength(5)]
        public string NumImp1 { get; set; }

        [Column("num_imp_2")]
        [StringLength(10)]
        public string NumImp2 { get; set; }
        
        [Column("cod_priorita")]
        public char CodPriorita { get; set; }

        [Column("quesito_diagnostico")]
        [StringLength(255)]
        public string? QuesitoDiagnostico { get; set; }

        [Column("medico_richiedente")]
        [StringLength(255)]
        public string? MedicoRichiedente { get; set; }

        [Column("pagato")]
        public byte? Pagato { get; set; }

        [Column("note_acc")]
        [StringLength(255)]
        public string? NoteAcc { get; set; }

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
        public byte EsternoStato { get; set; }

        [Column("esterno_stato_data")]
        public DateTime EsternoStatoData { get; set; }

        [Column("ins_data")]
        public DateTime? InsData { get; set; } = DateTime.UtcNow;

        public ICollection<AccettazioneDett> AccettazioniDett { get; set; }
    }
}
