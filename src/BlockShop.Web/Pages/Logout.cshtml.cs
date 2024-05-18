using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockShop.Web.Pages;

public class LogoutModel(HttpClient httpClient) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        const string url = "http://blockshop.api:25625/user/logout";
        await httpClient.GetAsync(url);

        return RedirectToPage("/blocks");
    }
}