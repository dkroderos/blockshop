using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockShop.Web.Pages;

public class CreateBlockModel(HttpClient httpClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Image is required")]
    public string Image { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }

    public string SuccessMessage = string.Empty;
    public string ErrorMessage = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        return await IsLoggedInAsync() ? Page() : RedirectToPage("/blocks");
    }

    public async Task<bool> IsLoggedInAsync()
    {
        try
        {
            var emailCheckResponse = await httpClient.GetAsync("http://blockshop.api:25625/user/email");

            return emailCheckResponse.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await IsLoggedInAsync())
        {
            ErrorMessage = "Please log in first";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Data validation failed";
            return Page();
        }

        const string url = "http://blockshop.api:25625/blocks";
        var jsonBody = $$"""
                         {
                            "name": "{{Name}}",
                            "description": "{{Description}}",
                            "image": "{{Image}}",
                            "price": {{Price}}
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
                ErrorMessage = "Block creation failed";
                ModelState.Clear();
                return Page();
            }

            SuccessMessage = "Block created successfully";
            return RedirectToPage("/blocks");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Something went wrong: {ex.Message}";
            return Page();
        }
    }
}