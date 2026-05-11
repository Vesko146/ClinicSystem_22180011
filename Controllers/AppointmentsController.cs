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
        private readonly UserManager<User> _userManager;

        public AppointmentsController(Clinic22180011Context context, UserManager<User> userManager)
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
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> AvailableSlots(int doctorId, DateTime date)
        {
            // 1. Вземаме заетите часове от базата
            var takenSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            // 2. Генерираме всички възможни 15-минутни слотове за работния ден
            var allSlots = new List<DateTime>();
            var startTime = date.Date.AddHours(8); // Започваме в 08:00
            var endTime = date.Date.AddHours(17);   // Приключваме в 17:00

            while (startTime < endTime)
            {
                allSlots.Add(startTime);
                startTime = startTime.AddMinutes(15); // На всеки 15 минути
            }

            ViewBag.DoctorId = doctorId;
            ViewBag.SelectedDate = date;
            ViewBag.AllSlots = allSlots;      // Всички часове
            ViewBag.TakenSlots = takenSlots;  // Само заетите

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
