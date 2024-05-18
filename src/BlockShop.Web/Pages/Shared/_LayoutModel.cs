using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockShop.Web.Pages.Shared;

// ReSharper disable once InconsistentNaming
public class _LayoutModel(HttpClient httpClient) : PageModel
{
    public string? Email { get; set; }

    public async Task OnGetAsync()
    {
        const string url = "http://blockshop.api:25624/user/email";
        var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
            Email = await response.Content.ReadAsStringAsync();
    }
}