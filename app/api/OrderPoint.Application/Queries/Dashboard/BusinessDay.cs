namespace OrderPoint.Application.Queries.Dashboard;

// When the bar's day starts ("BusinessDay:StartHour", local time; 0 = midnight): a night that runs past midnight still
// counts as one day for "today" in the dashboard and the bartender's shift
internal sealed record BusinessDay(TimeSpan StartTime);