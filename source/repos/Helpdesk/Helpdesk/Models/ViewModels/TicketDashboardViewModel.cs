namespace Helpdesk.Models.ViewModels
{
    public class TicketStatusCount
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
    }

    public class TicketDashboardViewModel
    {
        public int TotalTickets { get; set; }
        public int NewToday { get; set; }
        public int Unassigned { get; set; }
        public IList<TicketStatusCount> ByStatus { get; set; } = new List<TicketStatusCount>();
        public string[] AllowedStatuses { get; set; } = Array.Empty<string>();
        public DateTime GeneratedAtUtc { get; set; }
    }
}