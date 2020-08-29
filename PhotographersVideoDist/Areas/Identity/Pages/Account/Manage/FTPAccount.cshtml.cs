using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Models;

namespace PhotographersVideoDist.Areas.Identity.Pages.Account.Manage
{
    public class FTPAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDataProtector _dataProtector;
        private readonly ILogger<FTPAccountModel> _logger;

        public FTPAccountModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<FTPAccountModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(FTPAccountModel));
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "FTP Brugernavn")]
            public string FTPUserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "FTP Password")]
            public string NewFTPPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Gentag Password")]
            [Compare("NewFTPPassword", ErrorMessage = "Password er ikke ens, prøv igen!")]
            public string ConfirmFTPPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Load user and check loaded user not null.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kunne ikke indlæse bruger med ID: '{_userManager.GetUserId(User)}'.");
            }

            // Load current password and decrypt.
            string encryptedPassword = user.FTP_EncryptedPassword;
            string plainPassword = string.Empty;
			if (!string.IsNullOrEmpty(encryptedPassword))
			{
                plainPassword =  _dataProtector.Unprotect(encryptedPassword);
			}

            Input = new InputModel
            {
                FTPUserName = user.FTP_UserName,
                NewFTPPassword = plainPassword
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Load user from db and check loaded user not null.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kunne ikke indlæse bruger med ID: '{_userManager.GetUserId(User)}'.");
            }

            // Check ModelState is valid.
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

			// Update username if is changed.
			if (user.FTP_UserName != Input.FTPUserName)
			{
                user.FTP_UserName = Input.FTPUserName;
			}

            // Encrypt the password before save to db.
            var encryptedPassword = _dataProtector.Protect(Input.NewFTPPassword);
			if (user.FTP_EncryptedPassword != encryptedPassword)
			{
                user.FTP_EncryptedPassword = encryptedPassword;
			}

            // Update profile settings.
            await _userManager.UpdateAsync(user);

            // Refresh login and set status message.
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "FTP kontoen blev opdateret med succes.";

            // Log update information and update page.
            _logger.LogInformation("Brugeren opdaterede sin FTP konto.");
            return RedirectToPage();
        }
    }
}
