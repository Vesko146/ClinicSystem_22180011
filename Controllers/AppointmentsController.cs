using ClinicSystem_22180011.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicSystem_22180011.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly Clinic22180011Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentsController(Clinic22180011Context context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Това показва списъка с всички часове, за да не дава 404
        public async Task<IActionResult> Index()
        {
            var context = _context.Appointments.Include(a => a.Doctor).Include(a => a.Patient);
            return View(await context.ToListAsync());
        }
        // СТЪПКА 1: Пациентът избира лекар (Падащо меню)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> SelectDoctor()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "Name");
            return View();
        }

        // СТЪПКА 2: Показване на часовете (Синьо/Сиво)
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> AvailableSlots(int doctorId, DateTime date)
        {
            // Вземаме вече заетите часове от базата
            var takenSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.DoctorId = doctorId;
            ViewBag.SelectedDate = date;
            ViewBag.TakenSlots = takenSlots;

            return View();
        }

        // СТЪПКА 3: Финален запис и проверка за дублиране (Race Condition)
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book(int doctorId, DateTime slot)
        {
            var userId = _userManager.GetUserId(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            var newAppointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patient.PatientId,
                AppointmentDate = slot,
                Status = "Confirmed"
            };

            try
            {
                _context.Add(newAppointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Часът е записан успешно!";
            }
            catch
            {
                // Ако уникалният индекс в SQL сработи, ще хвърли грешка тук
                TempData["Error"] = "Упс! Този час току-що беше зает от друг пациент.";
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MySchedule()
        {
            var userId = _userManager.GetUserId(User);

            // Филтрираме прегледите: само тези, които са за лекаря с това UserId
            var myAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Doctor.UserId == userId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(myAppointments);
        }
    }
}
