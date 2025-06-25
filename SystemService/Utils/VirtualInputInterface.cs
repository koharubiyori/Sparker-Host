using System.Runtime.InteropServices;

namespace SparkerSystemService.Utils;

public class VirtualInputInterface
{
  private const string DllName = "Sparker-Virtual-Input-Interface.dll";

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern IntPtr createMouseInstance();

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern IntPtr createKeyboardInstance();

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int initializeInstance(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int abortInstance(IntPtr instance);

  // Mouse functions
  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseLeftButtonDown(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseLeftButtonUp(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseLeftButtonClick(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseRightButtonDown(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseRightButtonUp(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseRightButtonClick(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseMiddleButtonDown(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseMiddleButtonUp(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseMiddleButtonClick(IntPtr instance);

  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int mouseMoveCursor(IntPtr instance, int x, int y);

  // Keyboard functions
  [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
  public static extern int keyboardKeyDown(IntPtr instance, byte key);
}