using System.Text.Json;
using System.Text.Json.Serialization;
using SparkerCommons;

namespace SparkerSystemService.Preferences;

public static class Preference
{
  public static readonly ReminderPreference Reminder = new();
  public static readonly SettingsPreference Settings = new();
  public static readonly PairedDevicePreference PairedDevice = new();

  public static void InitializeAllPreferences()
  {
    Reminder.Initialize();
    Settings.Initialize();
    PairedDevice.Initialize();
  }
}

[JsonSerializable(typeof(ReminderData))]
[JsonSerializable(typeof(SettingsData))]
public partial class PreferenceJsonGenContext : JsonSerializerContext
{
}

public abstract class Preference<T>
{
  private readonly string _name;
  private T _value;
  protected T Value 
  {
    get => _value;
    set => _ = UpdateValue(value);
  }
  
  private string DataFilePath => Path.Combine(Constants.PreferenceDirPath, _name) + ".json";
  private readonly T _defaultValue;
  
  protected virtual void OnInitialized() {}

  protected Preference(string name, T defaultValue)
  {
    _name = name;
    _defaultValue = defaultValue;
  }

  public void Initialize()
  {
    _value = TryGetValueFromFile() ?? _defaultValue;
    OnInitialized();
  }

  private T? TryGetValueFromFile()
  {
    try
    {
      var json = File.ReadAllText(DataFilePath);
      return (T)JsonSerializer.Deserialize(json, typeof(T), PreferenceJsonGenContext.Default)!;
    }
    catch (Exception ex)
    {
      if (ex is FileNotFoundException or DirectoryNotFoundException)
      {
        return default;
      } throw;
    }
  }

  private void CreateValueFile()
  {
    if (File.Exists(DataFilePath)) return;

    var dirName = Path.GetDirectoryName(DataFilePath)!;
    if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
    File.Create(DataFilePath).Close();
  }

  private async Task UpdateValue(T value)
  {
    _value = value;
    var json = JsonSerializer.Serialize(value, typeof(T), PreferenceJsonGenContext.Default);
    try
    {
      await File.WriteAllTextAsync(DataFilePath, json);
    }
    catch (Exception ex)
    {
      if (ex is FileNotFoundException or DirectoryNotFoundException)
      {
        CreateValueFile();
        await File.WriteAllTextAsync(DataFilePath, json);
      } else throw;
    }
  }
}

