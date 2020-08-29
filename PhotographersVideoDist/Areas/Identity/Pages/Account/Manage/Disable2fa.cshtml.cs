using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Models;

namespace PhotographersVideoDist.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Disable2faModel> _logger;

        public Disable2faModel(
            UserManager<ApplicationUser> userManager,
            ILogger<Disable2faModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kunne ikke indlæse bruger med ID: '{_userManager.GetUserId(User)}'.");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                throw new InvalidOperationException($"Kunne ikke deaktivere 2FA for brugeren med ID: '{_userManager.GetUserId(User)}' as it's not currently enabled.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Kunne ikke indlæse bruger med ID: '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Der opstod en uventet fejl i forsøget på at deaktivere 2FA for brugeren med ID: '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("Bruger med ID '{UserId}' har deaktiveret 2FA for sin konto.", _userManager.GetUserId(User));
            StatusMessage = "2FA blev deaktiveret. Du kan aktivere 2FA igen når du har opsat en godkendelses app for din konto.";
            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}