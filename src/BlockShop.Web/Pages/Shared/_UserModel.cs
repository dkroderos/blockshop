using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockShop.Web.Pages.Shared;

// ReSharper disable once InconsistentNaming
public class _UserModel(HttpClient httpClient) : PageModel
{
    public string? Email { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            const string url = "http://blockshop.api:25625/user/email";
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
                Email = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            var a = ex.Message;
        }
    }
}