using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.Clients;

public sealed class ClientDetailsPage : AppPlaceholderPage
{
    public ClientDetailsPage()
        : base(
            "Client Details",
            "Display client profile details, contact data, and billing context that will later drive projects and invoices.")
    {
    }
}
