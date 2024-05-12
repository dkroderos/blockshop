using System.Text.Json;
using BlockShop.Api.Contracts;
using BlockShop.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockShop.Web.Pages;

public class BlocksModel(HttpClient httpClient) : PageModel
{
    public List<BlockResponse> Blocks { get; set; } = [];

    private static JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task OnGetAsync()
    {
        const string url = "http://localhost:25625/blocks?pageSize=100&cortColumn=createdat";
        var request = await httpClient.GetAsync(url);
        if (!request.IsSuccessStatusCode) return;

        var content = await request.Content.ReadAsStreamAsync();

        var response =
            await JsonSerializer.DeserializeAsync<PagedListWrapper<BlockResponse>>(content, _options);

        if (response is not null)
            Blocks.AddRange(response.Items);
    }

    public async Task<IActionResult> OnPostAddCommentAsync(Guid blockId, string commentContent)
    {
        if (!await IsLoggedInAsync()) return RedirectToPage();

        const string url = "http://localhost:25625/comments";
        var jsonBody = $$"""
                         {
                            "blockId": "{{blockId.ToString()}}",
                            "content": "{{commentContent}}"
                         }
                         """;

        var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");

        try
        {
            await httpClient.PostAsync(url, content);
            return RedirectToPage();
        }
        catch (Exception)
        {
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(Guid commentId)
    {
        if (!await IsLoggedInAsync()) return RedirectToPage();

        var url = $"http://localhost:25625/comments/{commentId.ToString()}";
        try
        {
            await httpClient.DeleteAsync(url);

            return RedirectToPage();
        }
        catch (Exception)
        {
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid blockId)
    {
        var url = $"http://localhost:25625/blocks/{blockId.ToString()}";
        await httpClient.DeleteAsync(url);

        // var response = await httpClient.DeleteAsync(url);
        // if (!response.IsSuccessStatusCode) ErrorMessage = response.StatusCode.ToString();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBuyBlockAsync(Guid blockId)
    {
        var url = $"http://localhost:25625/blocks/buy/{blockId.ToString()}";
        await httpClient.GetAsync(url);

        // var response = await httpClient.DeleteAsync(url);
        // if (!response.IsSuccessStatusCode)
        //     return RedirectToPage();

        return RedirectToPage();
    }

    public async Task<IEnumerable<CommentResponse>> GetBlockCommentsAsync(Guid blockId)
    {
        var request = await httpClient.GetAsync($"http://localhost:25625/blocks/{blockId}/comments?pageSize=100");
        if (!request.IsSuccessStatusCode) return [];

        var content = await request.Content.ReadAsStreamAsync();
        var response = await JsonSerializer.DeserializeAsync<PagedListWrapper<CommentResponse>>(content, _options);

        var comments = new List<CommentResponse>();

        if (response is not null)
            comments.AddRange(response.Items);

        return comments;
    }

    public async Task<string?> GetUserEmailAsync(Guid id)
    {
        var url = $"http://localhost:25625/user/{id}/email";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var email = await response.Content.ReadAsStringAsync();
        var trimmedEmail = email.Trim('"');

        return trimmedEmail;
    }

    public async Task<bool> IsLoggedInAsync()
    {
        try
        {
            var emailCheckResponse = await httpClient.GetAsync("http://localhost:25625/user/email");
            return emailCheckResponse.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> LoggedUserOwnsItemAsync(Guid creatorId)
    {
        const string url = "http://localhost:25625/user/id";
        var response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) return false;

        var id = response.Content.ReadAsStringAsync().Result.Trim('"');

        return id.Equals(creatorId.ToString());
    }
}