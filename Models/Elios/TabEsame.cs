using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliosBrokerManager.Models.Elios
{
    [Table("tab_esami")]
    public class TabEsame
    {
        [Key]
        [Column("id_esame_broker")]
        public ushort IdEsameBroker { get; set; }

        [Column("id_esame_elios")]
        public ushort? IdEsameElios { get; set; }

        [Column("id_esame_esterno")]
        [StringLength(15)]
        public string? IdEsameEsterno { get; set; }

        [Column("id_uo")]
        public byte? IdUo { get; set; }

        [Column("cod_min")]
        [StringLength(10)]
        public string? CodMin { get; set; }

        [Column("descrizione")]
        [StringLength(255)]
        public string? Descrizione { get; set; }

        [Column("branca")]
        public byte? Branca { get; set; }

        [Column("note_elios")]
        [StringLength(255)]
        public string? NoteElios { get; set; }

        [Column("note_esterno")]
        [StringLength(255)]
        public string? NoteEsterno { get; set; }
    }
}
