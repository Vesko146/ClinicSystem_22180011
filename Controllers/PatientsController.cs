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
    
    public class PatientsController : Controller
    {
        private readonly Clinic22180011Context _context;
        private readonly UserManager<User> _userManager;

        public PatientsController(Clinic22180011Context context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patients
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index()
        {
            // 1. Вземаме текущия логнат потребител
            var currentUserId = _userManager.GetUserId(User);

            // 2. Ако е Админ - вижда всичко
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Patients.ToListAsync());
            }

            // 3. Ако е Лекар - вижда само тези, които са го избрали
            if (User.IsInRole("Doctor"))
            {
                // Първо намираме кой е този лекар в нашата таблица Doctors
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == currentUserId);

                if (doctor != null)
                {
                    var myPatients = await _context.Patients
                        .Where(p => p.ChosenDoctorId == doctor.DoctorId)
                        .ToListAsync();
                    return View(myPatients);
                }
            }

            return View(new List<Patient>()); 
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
     .Include(p => p.Appointments) // This loads the related data
     .FirstOrDefaultAsync(m => m.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,FirstName,LastName,Phone,LastModified22180011")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PatientId,FirstName,LastName,Phone,LastModified22180011")] Patient patient)
        {
            if (id != patient.PatientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.PatientId))
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
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> ChooseDoctor()
        {
            ViewBag.Doctors = new SelectList(await _context.Doctors.ToListAsync(), "DoctorId", "FullName");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseDoctor(int chosenDoctorId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == currentUserId);

            if (patient != null)
            {
                patient.ChosenDoctorId = chosenDoctorId;
                _context.Update(patient);
                await _context.SaveChangesAsync();

                return RedirectToAction("AvailableSlots", "Appointments", new { doctorId = chosenDoctorId });
            }

            return RedirectToAction("Index", "Home");
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}
