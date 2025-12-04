using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CondoHub.Data;
using CondoHub.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CondoHub.Controllers
{
    [Authorize]
    public class AcessController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcessController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var accesses = _context.Accesses.OrderByDescending(a => a.DataHoraAcesso).ToList();
            return View(accesses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,NumeroApartamento,Cargo,InformacaoVisitante,TipoAcesso,Observacoes")] Access access)
        {
            if (ModelState.IsValid)
            {
                access.DataHoraAcesso = System.DateTime.Now;
                access.InformacaoVisitante ??= "";
                access.Observacoes ??= "";
                access.NumeroApartamento ??= "";

                var user = _context.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (user != null)
                    access.UserId = user.Id;
                _context.Add(access);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(access);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var access = _context.Accesses.FirstOrDefault(m => m.AcessoId == id);
            if (access == null)
            {
                return NotFound();
            }

            return View(access);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var access = _context.Accesses.FirstOrDefault(m => m.AcessoId == id);
            if (access == null)
            {
                return NotFound();
            }
            return View(access);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AcessoId,Nome,NumeroApartamento,Cargo,InformacaoVisitante,DataHoraAcesso,TipoAcesso,Observacoes")] Access access)
        {
            if (id != access.AcessoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(access);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    ModelState.AddModelError("", "Erro ao atualizar o registro.");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(access);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var access = _context.Accesses.FirstOrDefault(m => m.AcessoId == id);
            if (access == null)
            {
                return NotFound();
            }

            return View(access);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var access = _context.Accesses.Find(id);
            if (access != null)
            {
                _context.Accesses.Remove(access);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}