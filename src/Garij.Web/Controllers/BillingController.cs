using Garij.Application.DTOs;
using Garij.Application.Interfaces;
using Garij.Domain.Enums;
using Garij.Domain.Exceptions;
using Garij.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Garij.Web.Controllers;

[Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.FrontDesk))]
public class BillingController : Controller
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var invoices = await _billingService.GetAllInvoicesAsync();
        return View(invoices);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _billingService.GetInvoiceByIdAsync(id);
        if (invoice is null)
        {
            return NotFound();
        }

        return View(invoice);
    }

    [HttpGet]
    public IActionResult Create(int serviceJobId)
    {
        if (serviceJobId <= 0)
        {
            // Surfaced through the shared error view so the refusal keeps the site layout and
            // explains itself, rather than returning a bare 400 with an empty body.
            return this.BadRequestView(
                $"'{serviceJobId}' is not a valid service job number. Open the job you want to bill from the Service Jobs list and generate its invoice from there.");
        }

        return View(serviceJobId);
    }

    [HttpPost, ActionName("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirmed(int serviceJobId)
    {
        if (serviceJobId <= 0)
        {
            // Same refusal as the GET: there is no job form to redisplay for an id that can
            // never identify a job, so the shared error view carries the message instead.
            return this.BadRequestView(
                $"'{serviceJobId}' is not a valid service job number. Open the job you want to bill from the Service Jobs list and generate its invoice from there.");
        }

        try
        {
            var invoice = await _billingService.GenerateInvoiceAsync(serviceJobId);
            TempData["SuccessMessage"] = $"Invoice {invoice.InvoiceNumber} generated and job marked Completed.";
            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }
        catch (Exception ex) when (ex is NotFoundException or BusinessRuleException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Create", serviceJobId);
        }
    }

    [HttpGet]
    public async Task<IActionResult> RecordPayment(int invoiceId)
    {
        var invoice = await _billingService.GetInvoiceByIdAsync(invoiceId);
        if (invoice is null)
        {
            return NotFound();
        }

        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(int invoiceId, PaymentTransactionDto payment)
    {
        payment.InvoiceId = invoiceId;

        try
        {
            await _billingService.RecordPaymentAsync(payment);
            TempData["SuccessMessage"] = "Payment recorded successfully.";
            return RedirectToAction(nameof(Details), new { id = invoiceId });
        }
        catch (Exception ex) when (ex is NotFoundException or BusinessRuleException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var invoice = await _billingService.GetInvoiceByIdAsync(invoiceId);
            if (invoice is null)
            {
                return NotFound();
            }

            return View(invoice);
        }
    }
}
