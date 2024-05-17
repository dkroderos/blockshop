using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockShop.Web.Pages;

public class LoginModel(HttpClient httpClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public string SuccessMessage = string.Empty;
    public string ErrorMessage = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        return await IsLoggedInAsync() ? RedirectToPage("/blocks") : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (await IsLoggedInAsync())
        {
            ErrorMessage = "You are already logged in";
            return Page();
        }
        
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Data validation failed";
            return Page();
        }

        const string url = "http://blockshop.api:25625/login?useCookies=true";
        var jsonBody = $$"""
                         {
                            "Email": "{{Email}}",
                            "Password": "{{Password}}",
                            "twoFactorCode": "string",
                            "twoFactorRecoveryCode": "string"
                         }
                         """;

        var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Login failed";
                ModelState.Clear();
                return Page();
            }

            SuccessMessage = "Logged in successfully";
            return RedirectToPage("/blocks");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Something went wrong: {ex.Message}";
            return Page();
        }
    }

    public async Task<bool> IsLoggedInAsync()
    {
        try
        {
            var emailCheckResponse = await httpClient.GetAsync("http://blockshop.api:25625/user/email");

            return emailCheckResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Something went wrong: {ex.Message}";
            return false;
        }
    }
}