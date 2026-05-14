using ClinicSystem_22180011.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem_22180011.Controllers
{
    [Authorize(Roles = "Doctor, Admin")]
    public class ExamDetailsController : Controller
    {
        private readonly Clinic22180011Context _context;

        public ExamDetailsController(Clinic22180011Context context)
        {
            _context = context;
        }

        // GET: ExamDetails/Create?appointId=5
        public IActionResult Create(int appointId)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.AppointId == appointId);

            if (appointment == null) return NotFound();

            ViewBag.AppointmentInfo = $"Пациент: {appointment.Patient.FirstName} {appointment.Patient.LastName}, Дата: {appointment.AppointmentDate}";

            var model = new ExamDetail { AppointId = appointId };
            return View(model);
        }

        // POST: ExamDetails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamDetail model)
        {
            if (ModelState.IsValid)
            {
                model.LastModified22180011 = DateTime.Now;
                _context.ExamDetails.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Диагнозата и рецептата са записани успешно!";
                return RedirectToAction("Index", "Appointments");
            }
            return View(model);
        }
    }
}
