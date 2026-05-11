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

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> AvailableSlots(int doctorId, DateTime? date)
        {
            // Ако doctorId е 0, значи нещо се е объркало и го върни пациента към избора
            if (doctorId == 0)
            {
                return RedirectToAction("ChooseDoctor", "Patients");
            }
            DateTime selectedDate = date ?? DateTime.Today;

            var takenSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == selectedDate.Date)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            var allSlots = new List<DateTime>();
            var currentSlot = selectedDate.Date.AddHours(8); 
            var endOfDay = selectedDate.Date.AddHours(17);  

            while (currentSlot < endOfDay)
            {
                allSlots.Add(currentSlot);
                currentSlot = currentSlot.AddMinutes(15);
            }

            ViewBag.DoctorId = doctorId;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.TakenSlots = takenSlots;

            return View(allSlots); 
        }
      
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
