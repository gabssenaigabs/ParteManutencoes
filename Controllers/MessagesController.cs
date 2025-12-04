using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CondoHub.Models.Entities;
using CondoHub.Data;
using Microsoft.AspNetCore.Identity;
using CondoHub.Models.ViewModels;

namespace CondoHub.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [AllowAnonymous]
        public async Task<IActionResult> Mural()
        {
            var avisos = _context.Set<Messages>().OrderByDescending(a => a.DataPublicacao).ToList();
            var usuarios = _context.Users.ToList();
            var mural = avisos.Select(a =>
            {
                var usuario = usuarios.FirstOrDefault(u => u.Id == a.UsuarioId);
                return new AvisoMuralViewModel
                {
                    Id = a.Id,
                    Titulo = a.Titulo,
                    Mensagem = a.Mensagem,
                    Tipo = a.Tipo.ToString(),
                    ImagemPath = a.ImagemPath,
                    DataPublicacao = a.DataPublicacao,
                    UsuarioNome = usuario?.Nome ?? "Usuário",
                    UsuarioTipo = usuario?.Role.ToString() ?? "Morador"
                };
            }).ToList();
            return View(mural);
        }


        [HttpGet]
        public IActionResult CriarAviso()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarAviso(Messages aviso, IFormFile? ImagemPath)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    return View(aviso);
                }

                if (ImagemPath != null && ImagemPath.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImagemPath.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImagemPath.CopyToAsync(stream);
                    }

                    aviso.ImagemPath = fileName;
                }

                aviso.UsuarioId = user.Id;
                aviso.DataPublicacao = DateTime.Now;
                _context.Add(aviso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Mural));
            }
            return View(aviso);
        }
    }
}