using MauiBiller.Navigation;
using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.Billing;

public sealed class BillingPage : AppPlaceholderPage
{
    public BillingPage()
        : base(
            "Billing",
            "Review billable time, expenses, and recurring invoice data before creating a client-ready PDF invoice.",
            CreateAction("Open Create Invoice", AppRoutes.CreateInvoice))
    {
    }
}
