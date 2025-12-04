using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondoHub.Models.Entities
{
    public enum TipoAviso
    {
        Manutenção,
        Evento,
        Comunicado,
        Aviso
    }

    public class Messages
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Mensagem { get; set; }
        public TipoAviso Tipo { get; set; }
        public string? ImagemPath { get; set; }
        public DateTime DataPublicacao { get; set; } = DateTime.Now;
        public string? UsuarioId { get; set; }
    }
}