using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SparkerCommons.Utils
{
  // This class provides methods to pack and unpack arrays of strings into a byte array.
  // Total length(4) + [Item length(4) + Item bytes] * n + Separator
  public static class StringArrayPacker
  {
    private const int Separator = -1;

    public static byte[] Pack(params string[] items)
    {
      if (items == null) throw new ArgumentNullException(nameof(items));

      using (var ms = new MemoryStream())
      using (var writer = new BinaryWriter(ms, Encoding.UTF8))
      {
        using (var tempMs = new MemoryStream())
        using (var tempWriter = new BinaryWriter(tempMs, Encoding.UTF8))
        {
          foreach (var item in items)
          {
            var itemBytes = Encoding.UTF8.GetBytes(item ?? string.Empty);
            tempWriter.Write(itemBytes.Length);
            tempWriter.Write(itemBytes);
          }

          tempWriter.Write(Separator);
          tempWriter.Flush();

          var totalLength = (int)tempMs.Length;
          writer.Write(totalLength);

          tempMs.Position = 0;
          tempMs.CopyTo(ms);
        }

        writer.Flush();
        return ms.ToArray();
      }
    }

    public static string[][] Unpack(byte[] data)
    {
      if (data == null || data.Length < 4)
        throw new ArgumentException("Data is null, empty, or too short.", nameof(data));

      using (var ms = new MemoryStream(data))
      using (var reader = new BinaryReader(ms, Encoding.UTF8))
      {
        var totalLength = reader.ReadInt32();
        var items = new List<List<string>> { new List<string>() };

        while (ms.Position < totalLength)
        {
          var length = reader.ReadInt32();
          if (length == Separator)
          {
            items.Add(new List<string>());
            continue;
          }

          var itemBytes = reader.ReadBytes(length);
          var item = Encoding.UTF8.GetString(itemBytes);
          items[items.Count - 1].Add(item);
        }

        return items
          .Where(innerList => innerList.Count > 0)
          .Select(innerList => innerList.ToArray())
          .ToArray();
      }
    }
  }
}