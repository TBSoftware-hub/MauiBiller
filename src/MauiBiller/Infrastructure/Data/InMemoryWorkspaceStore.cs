using MauiBiller.Core.Models;

namespace MauiBiller.Infrastructure.Data;

public sealed class InMemoryWorkspaceStore
{
    private const string SeedOwnerMemberId = "owner-1";

    public InMemoryWorkspaceStore()
    {
        var members = new[]
        {
            new WorkspaceMember(SeedOwnerMemberId, "Tim Bentley", "tim@tbsoftware.dev", WorkspaceMemberRole.Owner),
            new WorkspaceMember("member-1", "Alex Rivera", "alex@tbsoftware.dev", WorkspaceMemberRole.Contributor),
            new WorkspaceMember("member-2", "Jordan Kim", "jordan@tbsoftware.dev", WorkspaceMemberRole.Contributor)
        };

        Workspace = new Workspace(
            "workspace-1",
            "TBSoftware Billing Workspace",
            members[0].FullName,
            members[0].Email,
            members);

        Clients =
        [
            new Client("client-1", "Northwind Labs", "Avery Shaw", "avery@northwindlabs.io", false),
            new Client("client-2", "Blue Harbor Tech", "Morgan Lee", "morgan@blueharbor.tech", false),
            new Client("client-3", "Legacy Ops", "Casey Jones", "casey@legacyops.net", true)
        ];

        Projects =
        [
            new Project("project-1", "client-1", "Maui Mobile Rewrite", 150m, false),
            new Project("project-2", "client-1", "Architecture Discovery", 140m, false),
            new Project("project-3", "client-2", "Billing API Stabilization", 165m, false)
        ];

        WorkItems =
        [
            new WorkItem("workitem-1", "project-1", "Feature Implementation", 150m),
            new WorkItem("workitem-2", "project-1", "Bug Triage", 135m),
            new WorkItem("workitem-3", "project-2", "Technical Planning", 140m),
            new WorkItem("workitem-4", "project-3", "API Maintenance", 165m)
        ];

        TimeEntries =
        [
            new TimeEntry("time-1", "workitem-1", "owner-1", new DateOnly(2026, 3, 17), TimeSpan.FromHours(3.5), "Feature scaffold"),
            new TimeEntry("time-2", "workitem-3", "member-1", new DateOnly(2026, 3, 17), TimeSpan.FromHours(2), "Discovery workshop"),
            new TimeEntry("time-3", "workitem-4", "member-2", new DateOnly(2026, 3, 18), TimeSpan.FromHours(4.25), "Stabilized invoice endpoint")
        ];

        Expenses =
        [
            new Expense("expense-1", "client-1", "Device testing subscription", 49m),
            new Expense("expense-2", "client-2", "Monitoring surcharge", 85m)
        ];

        InvoiceDrafts =
        [
            new InvoiceDraft("invoice-1", "client-1", "INV-1001", 1240m, false),
            new InvoiceDraft("invoice-2", "client-2", "INV-1002", 890m, true)
        ];
    }

    public Workspace Workspace
    {
        get;
    }

    public IReadOnlyList<Client> Clients
    {
        get;
    }

    public IReadOnlyList<Project> Projects
    {
        get;
    }

    public IReadOnlyList<WorkItem> WorkItems
    {
        get;
    }

    public IReadOnlyList<TimeEntry> TimeEntries
    {
        get;
    }

    public IReadOnlyList<Expense> Expenses
    {
        get;
    }

    public IReadOnlyList<InvoiceDraft> InvoiceDrafts
    {
        get;
    }

    public WorkspaceSnapshot CreateOwnerBootstrapSnapshot(AuthenticatedUser owner)
    {
        var ownerDisplayName = string.IsNullOrWhiteSpace(owner.DisplayName)
            ? owner.Email.Split('@')[0]
            : owner.DisplayName;
        var ownerMember = new WorkspaceMember(owner.UserId, ownerDisplayName, owner.Email, WorkspaceMemberRole.Owner);
        var members = new List<WorkspaceMember> { ownerMember };
        members.AddRange(Workspace.Members.Where(member => member.Role is not WorkspaceMemberRole.Owner));

        var workspace = new Workspace(
            "workspace-1",
            $"{ownerDisplayName}'s Workspace",
            ownerMember.FullName,
            ownerMember.Email,
            members);

        var timeEntries = TimeEntries
            .Select(entry => entry.MemberId == SeedOwnerMemberId
                ? entry with { MemberId = ownerMember.Id }
                : entry)
            .ToList();

        return new WorkspaceSnapshot(
            workspace,
            Clients.ToList(),
            Projects.ToList(),
            WorkItems.ToList(),
            timeEntries,
            Expenses.ToList(),
            InvoiceDrafts.ToList());
    }
}
