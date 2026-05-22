using ClinicSystem_22180011.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClinicSystem_22180011.Controllers
{
    [Authorize(Roles = "Doctor, Admin")]
    public class ExamDetailsController : Controller
    {
        private readonly Clinic22180011Context _context;
        private readonly UserManager<User> _userManager;

        public ExamDetailsController(Clinic22180011Context context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ExamDetails/Create?appointId=5
        public IActionResult Create(int appointId)
        {
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointId == appointId);

            if (appointment == null) return NotFound();

            if (User.IsInRole("Doctor"))
            {
                var currentUserId = _userManager.GetUserId(User);
                if (appointment.Doctor?.UserId != currentUserId)
                {
                    return Forbid(); 
                }
            }

            if (!string.IsNullOrEmpty(appointment.Status) && appointment.Status.Contains("НЗОК"))
            {
                ViewBag.PaymentType = "НЗОК";
            }
            else
            {
                ViewBag.PaymentType = "Платен";
            }

            ViewBag.AppointmentInfo = $"Пациент: {appointment.Patient.FirstName} {appointment.Patient.LastName}, Дата: {appointment.AppointmentDate}";

            var model = new ExamDetail { AppointId = appointId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamDetail model)
        {
            if (ModelState.IsValid)
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor) 
                    .FirstOrDefaultAsync(a => a.AppointId == model.AppointId);

                if (appointment == null) return NotFound();

                if (User.IsInRole("Doctor"))
                {
                    var currentUserId = _userManager.GetUserId(User);
                    if (appointment.Doctor?.UserId != currentUserId)
                    {
                        return Forbid();
                    }
                }

                string currentPaymentType = "Платен";
                if (appointment != null && !string.IsNullOrEmpty(appointment.Status) && appointment.Status.Contains("НЗОК"))
                {
                    currentPaymentType = "НЗОК";
                }

                var existingDetail = await _context.ExamDetails
                    .FirstOrDefaultAsync(ed => ed.AppointId == model.AppointId);

                if (existingDetail != null)
                {
                    existingDetail.Diagnosis = model.Diagnosis;
                    existingDetail.Prescription = model.Prescription;
                    existingDetail.PaymentType = currentPaymentType; 
                    existingDetail.LastModified22180011 = DateTime.Now;

                    _context.Entry(existingDetail).State = EntityState.Modified;
                    TempData["Success"] = "Диагнозата и рецептата бяха редактирани успешно!";
                }
                else
                {
                    model.PaymentType = currentPaymentType; 
                    model.LastModified22180011 = DateTime.Now;
                    _context.ExamDetails.Add(model);
                    TempData["Success"] = "Диагнозата и рецептата са записани успешно!";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Appointments");
            }
            return View(model);
        }

        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var examDetail = await _context.ExamDetails
                .Include(e => e.Appoint)
                .ThenInclude(a => a.Patient)
                .Include(e => e.Appoint)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(m => m.DetailId == id);

            if (examDetail == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (examDetail.Appoint?.Doctor?.UserId != currentUserId)
            {
                return Forbid();
            }

            // Предаваме информация за пациента на екрана, за да знае лекарят кого редактира
            ViewBag.AppointmentInfo = $"Пациент: {examDetail.Appoint?.Patient?.FirstName} {examDetail.Appoint?.Patient?.LastName}, Дата: {examDetail.Appoint?.AppointmentDate}";
            ViewBag.PaymentType = examDetail.PaymentType;

            return View(examDetail);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Edit(int detailId, [Bind("DetailId,AppointId,Diagnosis,Prescription,PaymentType,LastModified22180011")] ExamDetail model)
        {
            // Проверяваме дали DetailId от URL-а съвпада с това в изпратения модел
            if (detailId != model.DetailId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDetail = await _context.ExamDetails
                        .Include(ed => ed.Appoint)
                            .ThenInclude(a => a.Doctor) 
                        .FirstOrDefaultAsync(ed => ed.DetailId == detailId);

                    if (existingDetail == null) return NotFound();

                    var currentUserId = _userManager.GetUserId(User);
                    if (existingDetail.Appoint?.Doctor?.UserId != currentUserId)
                    {
                        return Forbid();
                    }

                    // Обновяваме само медицинските данни
                    existingDetail.Diagnosis = model.Diagnosis;
                    existingDetail.Prescription = model.Prescription;

                    // Задължително обновяваме времето на последна модификация
                    existingDetail.LastModified22180011 = DateTime.Now;

                    _context.Entry(existingDetail).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Медицинският картон беше редактиран успешно!";
                    return RedirectToAction("Index", "Appointments");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ExamDetails.Any(e => e.DetailId == detailId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Възникна многопотребителски конфликт. Данните вече бяха променени от друг източник.");
                        return View(model);
                    }
                }
            }
            return View(model);
        }
    }
}