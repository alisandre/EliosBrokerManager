using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliosBrokerManager.Models.Jibria
{
    [Table("ELIOS_QUEUE")]
    public class EliosQueueItem
    {
        [Key]
        [Column("CODICE")]
        public int Codice { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("ID_ACCETTAZIONE")]
        public string IdAccettazione { get; set; } = null!;

        [Column("DATA_ACCETTAZIONE")]
        public DateTime? DataAccettazione { get; set; }

        [Column("ID_PAZIENTE")]
        public int IdPaziente { get; set; }

        [MaxLength(30)]
        [Column("COGNOME")]
        public string? Cognome { get; set; }

        [MaxLength(30)]
        [Column("NOME")]
        public string? Nome { get; set; }

        [Column("DATA_NASCITA")]
        public DateTime? DataNascita { get; set; }

        [MaxLength(16)]
        [Column("CODICE_FISCALE")]
        public string? CodiceFiscale { get; set; }

        [MaxLength(15)]
        [Column("CODICE_ESAME")]
        public string? CodiceEsame { get; set; }

        [MaxLength(255)]
        [Column("DESCRIZIONE_ESAME")]
        public string? DescrizioneEsame { get; set; }

        [Column("DATA_INSERIMENTO")]
        public DateTime? DataInserimento { get; set; }

        [MaxLength(20)]
        [Column("STATO_INVIO")]
        public string? StatoInvio { get; set; } = "IN_ATTESA";

        [Column("DATA_INVIO")]
        public DateTime? DataInvio { get; set; }

        [MaxLength(255)]
        [Column("NOTE_ERRORE")]
        public string? NoteErrore { get; set; }

        [MaxLength(150)]
        [Column("STATO_PACS")]
        public string? StatoPacs { get; set; }

        [Column("DATA_ULT_AGG_PACS", TypeName = "timestamp")]
        public DateTime? DataUltAggPacs { get; set; }

        [MaxLength(255)]
        [Column("ERRORE_PACS")]
        public string? ErrorePacs { get; set; }
    }
}
