using Caktoje.Constants.Enums;

namespace Caktoje.Data.Bindings;

public class DayClosedBinding
{
    public required DateOnly Date { get; set; }
    public required RecurringEnum RecurringType { get; set; }
}