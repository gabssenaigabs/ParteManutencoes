using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondoHub.Models.ViewModels
{
    public class AvisoMuralViewModel
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Mensagem { get; set; }
        public string Tipo { get; set; } = "";
        public string? ImagemPath { get; set; }
        public DateTime DataPublicacao { get; set; }
        public string UsuarioNome { get; set; } = "Usuário";
        public string UsuarioTipo { get; set; } = "Morador";
    }
}