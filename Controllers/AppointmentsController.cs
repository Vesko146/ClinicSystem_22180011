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

        public async Task<IActionResult> Index(string searchString)
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsQueryable();

            // Филтрация по роли (твоята оригинална логика)
            if (User.IsInRole("Patient"))
            {
                query = query.Where(a => a.Patient.UserId == userId);
            }
            else if (User.IsInRole("Doctor"))
            {
                query = query.Where(a => a.Doctor.UserId == userId);
            }

            // Търсене (ако е въведено нещо)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.Doctor.FullName.Contains(searchString)
                                      || a.Patient.FirstName.Contains(searchString)
                                      || a.Patient.LastName.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate >= DateTime.Now)
                .ThenBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // ДЕТАЙЛИ: Работи с твоя AppointId
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.AppointId == id);

            if (appointment == null) return NotFound();

            // Сигурност: Само лекар/пациент на прегледа или Админ могат да виждат детайли
            var userId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && appointment.Doctor?.UserId != userId && appointment.Patient?.UserId != userId)
            {
                return Forbid();
            }

            return View(appointment);
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> SelectDoctor()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "FullName");
            return View();
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> AvailableSlots(int doctorId, DateTime? date)
        {
            if (doctorId == 0) return RedirectToAction("SelectDoctor");

            DateTime selectedDate = date ?? DateTime.Today;
            var dayOfWeek = selectedDate.DayOfWeek;
            var doctor = await _context.Doctors.FindAsync(doctorId);

            int startHour = 0; int endHour = 0;

            if (doctor.ScheduleGroup == "Alpha")
            {
                if (dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Wednesday) { startHour = 8; endHour = 17; }
                else if (dayOfWeek == DayOfWeek.Friday) { startHour = 8; endHour = 12; }
            }
            else if (doctor.ScheduleGroup == "Beta")
            {
                if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Thursday) { startHour = 8; endHour = 17; }
                else if (dayOfWeek == DayOfWeek.Friday) { startHour = 13; endHour = 17; }
            }

            var allSlots = new List<DateTime>();
            if (startHour != 0)
            {
                var currentSlot = selectedDate.Date.AddHours(startHour);
                var endOfDay = selectedDate.Date.AddHours(endHour);
                while (currentSlot < endOfDay)
                {
                    // Твоята защита от 1 час
                    if (currentSlot > DateTime.Now.AddHours(1))
                    {
                        allSlots.Add(currentSlot);
                    }
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

           
            var existingAppointment = await _context.Appointments
                .AnyAsync(a => a.PatientId == patient.PatientId && a.AppointmentDate >= DateTime.Now);

            if (existingAppointment)
            {
                TempData["Error"] = "Вече имате записан час! Трябва първо да го откажете от списъка, ако искате да изберете нов лекар или време.";
                return RedirectToAction(nameof(Index)); 
            }

            var lastAppoint = await _context.Appointments
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.LastModified22180011)
                .FirstOrDefaultAsync();

            if (lastAppoint != null && lastAppoint.LastModified22180011.HasValue &&
                DateTime.Now.Subtract(lastAppoint.LastModified22180011.Value).TotalSeconds < 30)
            {
                TempData["Error"] = "Моля, изчакайте 30 секунди преди следващата заявка.";
                return RedirectToAction(nameof(Index));
            }

            var newAppointment = new Appointment
            {
                DoctorId = doctorId,
                PatientId = patient.PatientId,
                AppointmentDate = slot,
                Status = "Confirmed",
                LastModified22180011 = DateTime.Now
            };

            _context.Add(newAppointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Часът е записан успешно!";
            return RedirectToAction(nameof(Index)); 
        }


        [Authorize(Roles = "Patient,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.AppointId == id);

            if (appointment == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (User.IsInRole("Patient") && appointment.Patient?.UserId != userId)
            {
                return Forbid(); // Не можеш да триеш чужд час
            }

            return View(appointment);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> ExportToCSV()
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsQueryable();

            if (User.IsInRole("Doctor"))
            {
                query = query.Where(a => a.Doctor.UserId == userId);
            }

            var appointments = await query.ToListAsync();

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Дата и час;Статус;Лекар;Пациент");

            foreach (var item in appointments)
            {
                string date = item.AppointmentDate.ToString("dd.MM.yyyy HH:mm");
                string doctor = item.Doctor?.FullName ?? "Няма информация";
                string patient = item.Patient != null ? $"{item.Patient.FirstName} {item.Patient.LastName}" : "Няма информация";

                string statusBg = item.Status switch
                {
                    "Confirmed" => "Потвърден",
                    "Completed" => "Завършен",
                    "Cancelled" => "Отказан",
                    _ => item.Status 
                };

                builder.AppendLine($"{date};{statusBg};{doctor};{patient}");
            }

            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var content = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            var finalFile = bom.Concat(content).ToArray();

            return File(finalFile, "text/csv", "pregledi_export.csv");
        }
    }
}