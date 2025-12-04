using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CondoHub.Models.Entities
{
    public enum MaintenanceCategory
    {
        Encanamento = 1,
        Elétrica = 2,
        Pintura = 3,
        Limpeza = 4,
        Elevador = 5,
        Jardim = 6,
        Segurança = 7,
        Outros = 8
    }

    public enum MaintenancePriority
    {
        Baixa = 1,
        Média = 2,
        Alta = 3,
        Urgente = 4
    }

    public enum MaintenanceStatus
    {
        Pendente = 1,
        EmAndamento = 2,
        Concluída = 3,
        Cancelada = 4
    }

    public class Maintenance
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public DateTime DataAgendada { get; set; }
        public DateTime DataConclusao { get; set; }
        public string? Custo { get; set; }

        public string RequisitanteId { get; set; }
        public ApplicationUser Requisitante { get; set; }

        public string? Empresa { get; set; }

        [Required]
        public MaintenanceCategory Category { get; set; }

        [Required]
        public MaintenancePriority Priority { get; set; }

        [Required]
        public MaintenanceStatus Status { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }
}