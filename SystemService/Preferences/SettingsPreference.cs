namespace SparkerSystemService.Preferences;

public record SettingsData(
  bool LanOnly = true  
);

public class SettingsPreference() : Preference<SettingsData>(
  nameof(SettingsPreference), 
  new SettingsData()
)
{
  public bool LanOnly
  {
    get => Value.LanOnly;
    set => Value = Value with { LanOnly = value };
  }
}
