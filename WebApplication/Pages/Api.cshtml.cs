using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication.Pages;

[Authorize]
public class ApiModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        HttpClientFactory = _httpClientFactory;
    }
    public string? Data { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }
    
    public async Task OnGetAsync()
    {
        using var httpclient = _httpClientFactory.CreateClient();
        httpclient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await HttpContext.GetTokenAsync("access_token"));
        Data = await httpclient.GetStringAsync("https://localhost:7001/WeatherForecast");
    }
}