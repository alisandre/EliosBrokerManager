using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliosBrokerManager.Models.Elios
{
    [Table("pazienti")]
    public class Paziente
    {
        [Key]
        [Column("id_paziente_broker")]
        public int IdPazienteBroker { get; set; }

        [Column("id_paziente_elios")]
        public int? IdPazienteElios { get; set; }

        [Column("id_paziente_esterno")]
        public int? IdPazienteEsterno { get; set; }

        [Column("cognome")]
        [StringLength(30)]
        public string Cognome { get; set; }

        [Column("nome")]
        [StringLength(30)]
        public string Nome { get; set; }

        [Column("tipo_cf")]
        [StringLength(3)]
        public string TipoCf { get; set; }

        [Column("cod_fisc")]
        [StringLength(16)]
        public string CodFisc { get; set; }

        [Column("sesso")]
        [StringLength(1)]
        public string? Sesso { get; set; }

        [Column("data_nascita")]
        public DateTime? DataNascita { get; set; }

        [Column("istat_comune_nas")]
        [StringLength(6)]
        public string? IstatComuneNas { get; set; }

        [Column("istat_comune_res")]
        [StringLength(6)]
        public string? IstatComuneRes { get; set; }

        [Column("indirizzo")]
        [StringLength(150)]
        public string? Indirizzo { get; set; }

        [Column("cap")]
        [StringLength(5)]
        public string? Cap { get; set; }

        [Column("prefisso")]
        [StringLength(10)]
        public string? Prefisso { get; set; } = "39";

        [Column("tel_cell")]
        [StringLength(20)]
        public string? TelCell { get; set; }

        [Column("email")]
        [StringLength(50)]
        public string? Email { get; set; }

        [Column("privacy")]
        public DateTime? Privacy { get; set; }

        [Column("pwd_web")]
        [StringLength(10)]
        public string? PwdWeb { get; set; }

        [Column("aggiorna_psw")]
        public byte? AggiornaPsw { get; set; }

        [Column("elios_note")]
        [StringLength(255)]
        public string? EliosNote { get; set; }

        [Column("esterno_note")]
        [StringLength(255)]
        public string? EsternoNote { get; set; }

        [Column("elios_stato")]
        public byte? EliosStato { get; set; }

        [Column("elios_stato_data")]
        public DateTime? EliosStatoData { get; set; }

        [Column("esterno_stato")]
        public byte? EsternoStato { get; set; }

        [Column("esterno_stato_data")]
        public DateTime? EsternoStatoData { get; set; }

        [Column("ins_data")]
        public DateTime? InsData { get; set; } = DateTime.UtcNow;
    }
}
