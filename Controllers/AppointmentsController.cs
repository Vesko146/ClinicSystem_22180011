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


        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsQueryable();

            if (User.IsInRole("Patient"))
            {
                query = query.Where(a => a.Patient.UserId == userId);
            }
            else if (User.IsInRole("Doctor"))
            {
                query = query.Where(a => a.Doctor.UserId == userId);
            }
           
            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate >= DateTime.Now)
                .ThenBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
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
            if (doctorId == 0) return RedirectToAction("ChooseDoctor", "Patients");

            DateTime selectedDate = date ?? DateTime.Today;
            var dayOfWeek = selectedDate.DayOfWeek;

            // Вземаме лекаря, за да видим коя смяна е
            var doctor = await _context.Doctors.FindAsync(doctorId);

            int startHour = 0;
            int endHour = 0;

            // ЛОГИКА ЗА ГРАФИЦИТЕ
            if (doctor.ScheduleGroup == "Alpha")
            {
                // Понеделник и Сряда: Цял ден (08:00 - 17:00)
                if (dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday)
                {
                    startHour = 8; endHour = 17;
                }
                // Петък: Само сутрин (08:00 - 12:00)
                else if (dayOfWeek == DayOfWeek.Friday)
                {
                    startHour = 8; endHour = 12;
                }
            }
            else if (doctor.ScheduleGroup == "Beta")
            {
                // Вторник и Четвъртък: Цял ден (08:00 - 17:00)
                if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday)
                {
                    startHour = 8; endHour = 17;
                }
                // Петък: Само следобед (13:00 - 17:00)
                else if (dayOfWeek == DayOfWeek.Friday)
                {
                    startHour = 13; endHour = 17;
                }
            }

            var allSlots = new List<DateTime>();
            if (startHour != 0)
            {
                var currentSlot = selectedDate.Date.AddHours(startHour);
                var endOfDay = selectedDate.Date.AddHours(endHour);
                while (currentSlot < endOfDay)
                {
                    allSlots.Add(currentSlot);
                    currentSlot = currentSlot.AddMinutes(15);
                }
            }

            var takenSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == selectedDate.Date)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.DoctorId = doctorId;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.TakenSlots = takenSlots;
            ViewBag.IsWorkingDay = (startHour != 0);
             ViewBag.DoctorName = doctor.FullName;

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
