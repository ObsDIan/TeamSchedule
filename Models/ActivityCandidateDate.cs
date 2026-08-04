namespace TeamSchedule.Models;

public class ActivityCandidateDate
{
    public long CandidateDateId { get; set; }
    public long ActivityId { get; set; }
    public TeamActivity? Activity { get; set; }
    public DateTime CandidateDate { get; set; }

    public ICollection<ActivityResponse> Responses { get; set; } = new List<ActivityResponse>();
}
