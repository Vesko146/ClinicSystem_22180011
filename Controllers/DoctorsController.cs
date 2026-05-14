using ClinicSystem_22180011.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicSystem_22180011.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly Clinic22180011Context _context;
        private readonly UserManager<User> _userManager;

        public DoctorsController(Clinic22180011Context context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Doctors
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Index(string searchString)
        {
            var doctors = from d in _context.Doctors select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                doctors = doctors.Where(s => s.FullName.Contains(searchString));
            }

            return View(await doctors.ToListAsync());
        }

        // GET: Doctors/Details/5
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.DoctorComments)
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(string FullName, string Email, string Password, string ScheduleGroup, string Specialty, string Biography)
        {
            var user = new User { UserName = Email, Email = Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Doctor");

                // ТУК Е МАГИЯТА: Превръщаме служебните кодове в текст за пациентите
                string readableSchedule = ScheduleGroup switch
                {
                    "Alpha" => "Понеделник, Сряда и Петък сутрин.",
                    "Beta" => "Вторник, Четвъртък и Петък следобед.",
                    _ => ScheduleGroup 
                };

                var doctor = new Doctor
                {
                    FullName = FullName,
                    UserId = user.Id,
                    ScheduleGroup = readableSchedule, 
                    Specialty = Specialty,
                    Biography = Biography
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        // GET: Doctors/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        // POST: Doctors/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("DoctorId,FullName,Specialty,ScheduleGroup,Biography,LastModified22180011")] Doctor doctor) // Добавено Biography тук
        {
            if (id != doctor.DoctorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.DoctorId))
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
            return View(doctor);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MySchedule()
        {
            // 1. Get the Logged-in User's ID
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // 2. Find the Appointments for the Doctor linked to this User
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.Doctor.UserId == currentUserId)
                .OrderBy(a => a.AppointmentDate) // This shows the "Gaps" naturally
                .ToListAsync();

            return View(appointments);
        }

        [Authorize(Roles = "Admin")]
        // GET: Doctors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [Authorize(Roles = "Patient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int doctorId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Details", new { id = doctorId });
            }

            var userName = User.Identity.Name ?? "Анонимен";

            var comment = new DoctorComment
            {
                DoctorId = doctorId,
                Content = content,
                CreatedAt = DateTime.Now,
                PatientName = userName
            };

            _context.DoctorComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = doctorId });
        }


        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }
    }
}
