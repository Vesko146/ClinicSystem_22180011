using ClinicSystem_22180011.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ClinicSystem_22180011.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly Clinic22180011Context _context;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            Clinic22180011Context context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [Display(Name = "Текущ телефонен номер")]
        public string Phone { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Телефонният номер е задължителен.")]
            [StringLength(10, MinimumLength = 10, ErrorMessage = "Телефонният номер трябва да бъде точно 10 символа.")]
            [RegularExpression(@"^[0-9]+$", ErrorMessage = "Телефонният номер трябва да съдържа само цифри.")]
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient != null)
            {
                Phone = patient.Phone;
            }
            else
            {
                Phone = "Няма въведен телефон";
            }

            Input = new InputModel
            {
                // PhoneNumber = patient?.Phone
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                // 1. Обновяваме телефона в системната таблица AspNetUsers
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Грешка при запис на телефона.";
                    return RedirectToPage();
                }

                // 2. СИНХРОНИЗАЦИЯ: Обновяваме телефона и в твоята таблица Patients
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (patient != null)
                {
                    patient.Phone = Input.PhoneNumber;
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Профилът е обновен успешно!";
            return RedirectToPage();
        }
    }
}
