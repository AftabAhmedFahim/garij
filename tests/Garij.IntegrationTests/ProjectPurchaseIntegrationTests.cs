using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Garij.IntegrationTests;

public class ProjectPurchaseIntegrationTests : IClassFixture<AuthorizationTestFactory>
{
    private static readonly Regex AntiForgeryTokenPattern = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    private readonly AuthorizationTestFactory _factory;

    public ProjectPurchaseIntegrationTests(AuthorizationTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateNonRedirectingClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task<string> ExtractAntiForgeryTokenAsync(HttpResponseMessage response)
    {
        var html = await response.Content.ReadAsStringAsync();
        var match = AntiForgeryTokenPattern.Match(html);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    [Fact]
    public async Task AnonymousRequest_ToPurchaseIndex_Returns200OK()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Purchase");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("UNLOCK FULL GARIJ ACCESS", content);
        Assert.Contains("Lifetime License", content);
    }

    [Fact]
    public async Task SeededAdmin_CanAccessDashboard_BecauseAdminIsSeededWithActiveLicense()
    {
        var client = CreateNonRedirectingClient();

        // Login as admin
        var loginPage = await client.GetAsync("/Account/Login");
        var token = await ExtractAntiForgeryTokenAsync(loginPage);

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "admin@garij.com",
            ["Password"] = "Admin@12345",
            ["__RequestVerificationToken"] = token,
        });

        var loginResponse = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        // Access dashboard
        var dashboardResponse = await client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
    }

    [Fact]
    public async Task NewRegisteredUser_WithoutLicense_IsRedirectedToPurchase()
    {
        var client = CreateNonRedirectingClient();

        // Register a new user without buying
        var registerPage = await client.GetAsync("/Account/Register");
        var token = await ExtractAntiForgeryTokenAsync(registerPage);
        var uniqueEmail = $"unlicensed_{Guid.NewGuid():N}@test.com";

        var registerForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Unlicensed User",
            ["Email"] = uniqueEmail,
            ["PhoneNumber"] = "+1999999999",
            ["Password"] = "Test@12345",
            ["ConfirmPassword"] = "Test@12345",
            ["__RequestVerificationToken"] = token,
        });

        var registerResponse = await client.PostAsync("/Account/Register", registerForm);
        Assert.Equal(HttpStatusCode.Redirect, registerResponse.StatusCode);

        // Attempting to access Dashboard should redirect to /Purchase
        var dashboardResponse = await client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.Redirect, dashboardResponse.StatusCode);
        Assert.Contains("/Purchase", dashboardResponse.Headers.Location!.ToString());
    }

    [Fact]
    public async Task PurchaseCheckout_GrantsLicense_AndUnlocksDashboard()
    {
        var client = CreateNonRedirectingClient();
        var uniqueEmail = $"purchaser_{Guid.NewGuid():N}@test.com";

        // Register new user
        var registerPage = await client.GetAsync("/Account/Register");
        var token = await ExtractAntiForgeryTokenAsync(registerPage);

        var registerForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Purchasing Garage Owner",
            ["Email"] = uniqueEmail,
            ["PhoneNumber"] = "+1888888888",
            ["Password"] = "Test@12345",
            ["ConfirmPassword"] = "Test@12345",
            ["__RequestVerificationToken"] = token,
        });

        await client.PostAsync("/Account/Register", registerForm);

        // Visit Purchase page to get token
        var purchasePage = await client.GetAsync("/Purchase");
        var purchaseToken = await ExtractAntiForgeryTokenAsync(purchasePage);

        // Submit checkout in test mode
        var checkoutForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["BuyerName"] = "Purchasing Garage Owner",
            ["BuyerEmail"] = uniqueEmail,
            ["WorkshopName"] = "Apex Garage",
            ["PaymentMethod"] = "CreditCard",
            ["IsTestPayment"] = "true",
            ["__RequestVerificationToken"] = purchaseToken,
        });

        var checkoutResponse = await client.PostAsync("/Purchase/Checkout", checkoutForm);
        Assert.Equal(HttpStatusCode.Redirect, checkoutResponse.StatusCode);
        Assert.Contains("/Purchase/Success", checkoutResponse.Headers.Location!.ToString());

        // Now, accessing Dashboard should be allowed (200 OK)!
        var dashboardResponse = await client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
    }

    [Theory]
    [InlineData("/About")]
    [InlineData("/Services")]
    [InlineData("/Testimonials")]
    [InlineData("/Pricing")]
    public async Task AnonymousUser_CanAccessPublicPages_WithoutLicense(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/About")]
    [InlineData("/Services")]
    [InlineData("/Testimonials")]
    [InlineData("/Pricing")]
    public async Task UnlicensedLoggedInUser_CanAccessPublicPages_WhileDashboardIsLocked(string url)
    {
        var client = CreateNonRedirectingClient();
        var uniqueEmail = $"unpaid_public_{Guid.NewGuid():N}@test.com";

        // Register new unlicensed user
        var registerPage = await client.GetAsync("/Account/Register");
        var token = await ExtractAntiForgeryTokenAsync(registerPage);

        var registerForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FullName"] = "Unpaid Public Visitor",
            ["Email"] = uniqueEmail,
            ["PhoneNumber"] = "+1777777777",
            ["Password"] = "Test@12345",
            ["ConfirmPassword"] = "Test@12345",
            ["__RequestVerificationToken"] = token,
        });

        await client.PostAsync("/Account/Register", registerForm);

        // 1. Verify public page is accessible with 200 OK
        var pageResponse = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);

        // 2. Verify dashboard is locked (redirects to /Purchase)
        var dashboardResponse = await client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.Redirect, dashboardResponse.StatusCode);
        Assert.Contains("/Purchase", dashboardResponse.Headers.Location!.ToString());
    }
}
