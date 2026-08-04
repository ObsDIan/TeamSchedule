namespace TeamSchedule.Models;

public enum AvailabilityStatus
{
    Available = 1,
    Busy = 2,
    Maybe = 3
}

public enum TeamRole
{
    Owner = 1,
    Member = 2
}

public enum ActivityStatus
{
    Open = 1,
    Confirmed = 2,
    Cancelled = 3
}

public enum ActivityResponseStatus
{
    Join = 1,
    Decline = 2,
    Maybe = 3
}

public enum ParticipationStatus
{
    Joined = 1,
    Withdrawn = 2
}

public enum UserAccountStatus
{
    Active = 1,
    Disabled = 2
}
