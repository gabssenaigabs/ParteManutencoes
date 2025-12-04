using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CondoHub.Models.Entities
{
    [Table("Access")]
    public class Access
    {
        [Key]
        public int AcessoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = "";

        [StringLength(10)]
        public string? NumeroApartamento { get; set; }

        [Required]
        [StringLength(100)]
        public string Cargo { get; set; } = "";

        [StringLength(100)]
        public string? InformacaoVisitante { get; set; }

        [Required]
        public DateTime DataHoraAcesso { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoAcesso { get; set; } = "";

        [StringLength(500)]
        public string? Observacoes { get; set; }

        [ForeignKey("ApplicationUser")]
        public string? UserId { get; set; }

        public virtual ApplicationUser? ApplicationUser { get; set; }

        public Access()
        {
            DataHoraAcesso = DateTime.Now;
        }
    }
}