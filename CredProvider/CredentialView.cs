using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CredProvider.NET.Interop2;

namespace SparkerCredProvider
{
  public class CredentialDescriptor
  {
    public _CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR Descriptor { get; set; }

    public _CREDENTIAL_PROVIDER_FIELD_STATE State { get; set; }

    public string Value { get; set; }
  }

  public class CredentialView
  {
    public ICredentialProviderCredentialEvents events;
    public ICredentialProviderCredential credential;
    private readonly List<CredentialDescriptor> fields
        = new List<CredentialDescriptor>();

    public CredentialProviderBase Provider { get; private set; }

    public const string CPFG_LOGON_PASSWORD_GUID = "60624cfa-a477-47b1-8a8e-3a4a19981827";
    public const string CPFG_CREDENTIAL_PROVIDER_LOGO = "2d837775-f6cd-464e-a745-482fd0b47493";
    public const string CPFG_CREDENTIAL_PROVIDER_LABEL = "286bbff3-bad4-438f-b007-79b7267c3d48";

    public bool Active { get; set; }

    public int DescriptorCount => fields.Count;

    public CredentialView(CredentialProviderBase provider)
    {
      Provider = provider;
    }

    public virtual void AddField(
        _CREDENTIAL_PROVIDER_FIELD_TYPE cpft,
        string pszLabel,
        _CREDENTIAL_PROVIDER_FIELD_STATE state,
        string defaultValue = null,
        Guid guidFieldType = default
    )
    {
      if (!Active)
      {
        throw new NotSupportedException();
      }

      fields.Add(new CredentialDescriptor
      {
        State = state,
        Value = defaultValue,
        Descriptor = new _CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR
        {
          dwFieldID = (uint)fields.Count,
          cpft = cpft,
          pszLabel = pszLabel,
          guidFieldType = guidFieldType
        }
      });
    }

    public virtual bool GetField(int dwIndex, [Out] IntPtr ppcpfd)
    {
      Logger.Write($"dwIndex: {dwIndex}; descriptors: {fields.Count}");

      if (dwIndex >= fields.Count)
      {
        return false;
      }

      var field = fields[dwIndex];

      var pcpfd = Marshal.AllocHGlobal(Marshal.SizeOf(field.Descriptor));

      Marshal.StructureToPtr(field.Descriptor, pcpfd, false);
      Marshal.StructureToPtr(pcpfd, ppcpfd, false);

      return true;
    }

    public string GetValue(int dwFieldId)
    {
      return fields[dwFieldId].Value;
    }

    public void SetValue(int dwFieldId, string val)
    {
      fields[dwFieldId].Value = val;
      events.SetFieldString(credential, (uint)dwFieldId, val);
    }

    public void GetFieldState(
        int dwFieldId,
        out _CREDENTIAL_PROVIDER_FIELD_STATE pcpfs,
        out _CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE pcpfis
    )
    {
      Logger.Write();

      var field = fields[dwFieldId];

      Logger.Write($"Returning field state: {field.State}, interactiveState: None");

      pcpfs = field.State;
      pcpfis = _CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE.CPFIS_NONE;
    }
  }
}
