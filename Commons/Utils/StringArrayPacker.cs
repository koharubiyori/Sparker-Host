using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SparkerCommons.Utils
{
  // Encoding and decoding string array composed several (string length(4byte) + string content)
  public static class StringArrayPacker
  {
    public static byte[] Pack(params string[] items)
    {
      if (items == null) throw new ArgumentNullException(nameof(items));

      using (var ms = new MemoryStream())
      using (var writer = new BinaryWriter(ms, Encoding.UTF8))
      {
        foreach (var item in items)
        {
          byte[] itemBytes = Encoding.UTF8.GetBytes(item ?? string.Empty);
          writer.Write(itemBytes.Length);
          writer.Write(itemBytes);
        }

        writer.Write(0);
        writer.Flush();
        return ms.ToArray();
      }
    }

    public static string[][] Unpack(byte[] data)
    {
      if (data == null || data.Length == 0)
        throw new ArgumentException("Data is null or empty.", nameof(data));

      using (var ms = new MemoryStream(data))
      using (var reader = new BinaryReader(ms, Encoding.UTF8))
      {
        var items = new List<List<string>>() { new List<string>() };

        while (ms.Position < ms.Length)
        {
          int length = reader.ReadInt32();
          if (length == 0)
          {
            items.Add(new List<string>());
            continue;
          }
          
          byte[] itemBytes = reader.ReadBytes(length);
          string item = Encoding.UTF8.GetString(itemBytes);
          items[items.Count - 1].Add(item);
        }

        return items
          .Select(innerList => innerList.ToArray())
          .ToArray();
      }
    }
  }
}
