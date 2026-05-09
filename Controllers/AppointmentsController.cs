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
       

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> BookAppointment()
        {
            // Извличаме списъка с лекари, за да ги покажем на пациента
            var doctors = await _context.Doctors.ToListAsync();
            return View(doctors);
        }

        public async Task<IActionResult> SelectSlot(int doctorId, DateTime date)
        {
            var takenSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .Select(a => a.AppointmentDate)
                .ToListAsync();

            var slots = new List<SlotViewModel>();
            DateTime startTime = date.Date.AddHours(8); // Почваме от 8:00

            for (int i = 0; i < 16; i++) // 16 слота по 30 мин = 8 часа
            {
                slots.Add(new SlotViewModel
                {
                    Time = startTime,
                    IsTaken = takenSlots.Contains(startTime)
                });
                startTime = startTime.AddMinutes(30);
            }
            return View(slots);
        }

        // GET: Appointments
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index()
        {
            var clinic22180011Context = _context.Appointments.Include(a => a.Doctor).Include(a => a.Patient);
            return View(await clinic22180011Context.ToListAsync());
        }

        // GET: Appointments/Details/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.AppointId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize(Roles = "Patient")]
        public IActionResult Create()
        {
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId");
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId");
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")] // Само пациенти могат да записват
        public async Task<IActionResult> Create([Bind("DoctorId,AppointmentDate")] Appointment appointment)
        {
            // 1. Вземаме ID-то на логнатия потребител (User Login от диаграмата)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Намираме кой пациент в нашата таблица съответства на този потребител
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUserId);

            if (patient == null)
            {
                return BadRequest("Вашият потребител не е свързан с пациентски профил.");
            }

            appointment.PatientId = patient.PatientId;
            appointment.Status = "Pending"; // Начален статус

            // 3. ПРОВЕРКА ЗА СВОБОДЕН ЧАС (Validate Slot от диаграмата)
            bool isTaken = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == appointment.AppointmentDate);

            if (isTaken)
            {
                // Разклонение "Slot taken -> Error" от диаграмата
                ModelState.AddModelError("", "Този час вече е зает! Моля, изберете друг слот.");
                ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "Name", appointment.DoctorId);
                return View(appointment);
            }

            // 4. ЗАПИС (Create Appointment -> Update Schedule от диаграмата)
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Show New Schedule
            }

            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "Name", appointment.DoctorId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", appointment.PatientId);
            return View(appointment);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, [Bind("AppointId,PatientId,DoctorId,AppointmentDate,Status,LastModified22180011")] Appointment appointment)
        {
            if (id != appointment.AppointId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "DoctorId", "DoctorId", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "PatientId", "PatientId", appointment.PatientId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.AppointId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointId == id);
        }
    }
}
