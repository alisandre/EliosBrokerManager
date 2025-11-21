using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EliosBrokerManager.Models.Elios
{
    [Table("tab_stati")]
    public class TabStato
    {
        [Key]
        [Column("id_stato_broker")]
        public byte IdStatoBroker { get; set; }

        [Column("descrizione")]
        [StringLength(50)]
        public string Descrizione { get; set; }
    }
}
