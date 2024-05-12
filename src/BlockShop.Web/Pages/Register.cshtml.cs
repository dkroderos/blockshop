using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockShop.Web.Pages;

public class RegisterModel(HttpClient httpClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(@"^(?=.*[^\w\d\s])(?=.*\d).{7,}$",
        ErrorMessage =
            "Password must have at least one special character, one digit, and be at least 7 characters long")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Repeat password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string RepeatPassword { get; set; } = string.Empty;

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
            ErrorMessage = "Please log out first";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Data validation failed";
            return Page();
        }

        const string url = "http://localhost:25625/register";
        var jsonBody = $$"""
                         {
                            "email": "{{Email}}",
                            "password": "{{Password}}"
                         }
                         """;

        var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(url, content);

            // ReSharper disable once UnusedVariable
#pragma warning disable 0169
            var result = await response.Content.ReadAsStringAsync();
#pragma warning restore 0169

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Register failed";
                ModelState.Clear();
                return Page();
            }

            SuccessMessage = "Registered successfully";
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
            var emailCheckResponse = await httpClient.GetAsync("http://localhost:25625/user/email");

            return emailCheckResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Something went wrong: {ex.Message}";
            return false;
        }
    }
}