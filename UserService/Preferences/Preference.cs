using System.Text.Json;
using System.Text.Json.Serialization;
using SparkerCommons;

namespace SparkerUserService.Preferences;

public class Preference
{
  public static readonly ReminderPreference Reminder = new();

  public static void InitializeAllPreferences()
  {
    Reminder.Initialize();
  }
}

[JsonSerializable(typeof(ReminderData))]
public partial class PreferenceJsonGenContext : JsonSerializerContext;

public class Preference<T>
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

  protected Preference(string name, T defaultValue)
  {
    _name = name;
    _defaultValue = defaultValue;
  }

  public void Initialize()
  {
    _value = TryGetValueFromFile() ?? _defaultValue;
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
    await Task.Run(() =>
    {
      var json = JsonSerializer.Serialize(value, typeof(T), PreferenceJsonGenContext.Default);
      try
      {
        File.WriteAllText(DataFilePath, json);
      }
      catch (Exception ex)
      {
        if (ex is FileNotFoundException or DirectoryNotFoundException)
        {
          CreateValueFile();
          File.WriteAllText(DataFilePath, json);
        } else throw;
      }
    });
  }
}

