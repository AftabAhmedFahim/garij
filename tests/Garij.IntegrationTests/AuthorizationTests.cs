using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Garij.IntegrationTests;

/// <summary>
/// Exercises role-based [Authorize] enforcement over real HTTP requests against the
/// seeded demo accounts (admin@garij.com, frontdesk@garij.com, mechanic@garij.com),
/// covering both the original "mechanic cannot reach billing" check from
/// docs/aftab-stage1-readiness.md and the full controller role-guard sweep from
/// issue #23 (Customer/Vehicle/ServiceJob/Parts/Notification).
/// </summary>
public class AuthorizationTests : IClassFixture<AuthorizationTestFactory>
{
    private static readonly Regex AntiForgeryTokenPattern = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    private readonly AuthorizationTestFactory _factory;

    public AuthorizationTests(AuthorizationTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateNonRedirectingClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task LoginAsync(HttpClient client, string email, string password)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        var loginHtml = await loginPage.Content.ReadAsStringAsync();
        var token = AntiForgeryTokenPattern.Match(loginHtml).Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(token), "Could not find __RequestVerificationToken on the login page.");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token,
        });

        var response = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousRequest_ToBilling_RedirectsToLogin()
    {
        var client = CreateNonRedirectingClient();

        var response = await client.GetAsync("/Billing");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Mechanic_RequestingBilling_IsDenied()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "mechanic@garij.com", "Mechanic@12345");

        var response = await client.GetAsync("/Billing");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Mechanic_RequestingReport_IsDenied()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "mechanic@garij.com", "Mechanic@12345");

        var response = await client.GetAsync("/Report");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Admin_RequestingBilling_IsAllowed()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "admin@garij.com", "Admin@12345");

        var response = await client.GetAsync("/Billing");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_RequestingReport_IsAllowed()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "admin@garij.com", "Admin@12345");

        var response = await client.GetAsync("/Report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Mechanic_RequestingCustomer_IsDenied()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "mechanic@garij.com", "Mechanic@12345");

        var response = await client.GetAsync("/Customer");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Mechanic_RequestingNotification_IsDenied()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "mechanic@garij.com", "Mechanic@12345");

        var response = await client.GetAsync("/Notification");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Mechanic_RequestingServiceJob_IsAllowed()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "mechanic@garij.com", "Mechanic@12345");

        var response = await client.GetAsync("/ServiceJob");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Mechanic_RequestingParts_IsAllowed()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "mechanic@garij.com", "Mechanic@12345");

        var response = await client.GetAsync("/Parts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FrontDesk_RequestingCustomer_IsAllowed()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "frontdesk@garij.com", "Staff@12345");

        var response = await client.GetAsync("/Customer");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Mechanic_RequestingDashboard_IsDenied()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "mechanic@garij.com", "Mechanic@12345");

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task FrontDesk_RequestingDashboard_IsAllowed()
    {
        var client = CreateNonRedirectingClient();
        await LoginAsync(client, "frontdesk@garij.com", "Staff@12345");

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
