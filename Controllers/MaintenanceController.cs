using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CondoHub.Data;
using CondoHub.Models.Entities;

namespace CondoHub.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string filter = "all")
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.IsGestor = User.IsInRole("GESTOR");
            ViewBag.CurrentFilter = filter;

            var query = _context.Maintenance
                .Include(m => m.Requisitante)
                .AsQueryable();

            // Apply filter based on the filter parameter
            if (!string.IsNullOrEmpty(filter) && filter != "all")
            {
                query = filter switch
                {
                    "pendente" => query.Where(m => m.Status == MaintenanceStatus.Pendente),
                    "andamento" => query.Where(m => m.Status == MaintenanceStatus.EmAndamento),
                    "concluida" => query.Where(m => m.Status == MaintenanceStatus.Concluída),
                    "cancelada" => query.Where(m => m.Status == MaintenanceStatus.Cancelada),
                    _ => query
                };
            }

            // Order by newest first (RequestDate desc). Keep Priority as secondary ordering.
            var list = await query
                .OrderByDescending(m => m.RequestDate)
                .ThenByDescending(m => m.Priority)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Maintenance
                .Include(m => m.Requisitante)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return NotFound();
            ViewBag.IsGestor = User.IsInRole("GESTOR");

            return View(item);
        }

        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Maintenance m)
        {
            Console.WriteLine("[DEBUG] POST /Maintenance/Create received");

            // Populate fields that are not provided by the form but are required by the model
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                m.RequisitanteId = user.Id;
                m.Requisitante = user;
            }

            // Ensure required enum fields have defaults before validating ModelState
            if (m.Priority == 0) m.Priority = MaintenancePriority.Média;
            if (m.Status == 0) m.Status = MaintenanceStatus.EmAndamento;
            if (m.Category == 0) m.Category = MaintenanceCategory.Outros;
            if (m.DataAgendada == default) m.DataAgendada = DateTime.UtcNow;

            // Remove possible modelstate errors produced during binding for these fields
            ModelState.Remove(nameof(m.Requisitante));
            ModelState.Remove(nameof(m.RequisitanteId));
            ModelState.Remove(nameof(m.Priority));
            ModelState.Remove(nameof(m.Status));
            ModelState.Remove(nameof(m.Category));
            ModelState.Remove(nameof(m.DataAgendada));

            // Re-validate the model to ensure ModelState reflects current values
            TryValidateModel(m);

            if (ModelState.IsValid)
            {
                m.RequestDate = DateTime.UtcNow;
                m.DataSolicitacao = DateTime.UtcNow;

                _context.Add(m);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Log modelstate errors for debugging
            foreach (var kv in ModelState)
            {
                if (kv.Value.Errors.Any())
                {
                    Console.WriteLine($"[DEBUG] ModelState error - {kv.Key}: {string.Join("; ", kv.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            return View(m);
        }

        [Authorize(Roles = "GESTOR")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Maintenance.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [Authorize(Roles = "GESTOR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Maintenance m)
        {
            if (id != m.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(m);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Exists(m.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(m);
        }

        [HttpPost]
        [Authorize(Roles = "GESTOR")]
        public async Task<IActionResult> UpdateStatus(int id, MaintenanceStatus status)
        {
            var item = await _context.Maintenance.FindAsync(id);
            if (item != null)
            {
                item.Status = status;

                if (status == MaintenanceStatus.Concluída)
                    item.DataConclusao = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            // If request is AJAX, return JSON so client can update the UI inline
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax)
            {
                return Json(new {
                    success = true,
                    id,
                    status = status.ToString(),
                    statusClass = status == MaintenanceStatus.Concluída ? "status-concluida" : (status == MaintenanceStatus.EmAndamento ? "status-andamento" : (status == MaintenanceStatus.Pendente ? "status-pendente" : "status-cancelada")),
                    dataConclusao = item?.DataConclusao
                });
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Calendar()
        {
            var events = await _context.Maintenance
                .Where(m => m.Status == MaintenanceStatus.EmAndamento || m.Status == MaintenanceStatus.Pendente)
                .OrderBy(m => m.DataAgendada)
                .ToListAsync();

            return View(events);
        }

        private bool Exists(int id)
        {
            return _context.Maintenance.Any(e => e.Id == id);
        }
    }
}
