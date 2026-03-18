using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.Billing;

public sealed class CreateInvoicePage : AppPlaceholderPage
{
    public CreateInvoicePage()
        : base(
            "Create Invoice",
            "Assemble invoice data into a reviewable draft that will later feed the cross-platform PDF generation flow.")
    {
    }
}
