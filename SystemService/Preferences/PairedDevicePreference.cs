namespace SparkerSystemService.Preferences;

public record PairedDeviceData(
  Dictionary<string, PairedDevice>? Devices = null
);

public record PairedDevice;

public class PairedDevicePreference() : Preference<PairedDeviceData>(
  nameof(PairedDevicePreference), 
  new PairedDeviceData()
)
{
  private Dictionary<string, PairedDevice> _devices = new();

  protected override void OnInitialized()
  {
    _devices = Value.Devices ?? new Dictionary<string, PairedDevice>();
  }
  
  public void AddDevice(string deviceId, PairedDevice device)
  {
    _devices.Add(deviceId, device);
    Value = Value with { Devices = _devices };
  }
  
  public bool Exists(string deviceId)
  {
    return _devices.ContainsKey(deviceId);
  }

  public void RemoveDevice(string deviceId)
  {
    _devices.Remove(deviceId);
    Value = Value with { Devices = _devices };
  }
}