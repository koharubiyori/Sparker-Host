namespace SparkerUserService.Preferences;

public record ReminderData(
  bool FirstRun = true
);

public class ReminderPreference() : Preference<ReminderData>(
  nameof(ReminderPreference), 
  new ReminderData()
)
{
  public bool FirstRun {
    get => Value.FirstRun;
    set => Value = Value with { FirstRun = value };
  }
}
