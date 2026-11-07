using System.Runtime.InteropServices;
namespace MasoomDev.MacroRecorder;

internal static class NativeInput
{
 public const int WH_KEYBOARD_LL=13, WH_MOUSE_LL=14, WM_KEYDOWN=0x100, WM_KEYUP=0x101, WM_SYSKEYDOWN=0x104, WM_SYSKEYUP=0x105;
 public const int WM_MOUSEMOVE=0x200, WM_LBUTTONDOWN=0x201, WM_LBUTTONUP=0x202, WM_RBUTTONDOWN=0x204, WM_RBUTTONUP=0x205, WM_MBUTTONDOWN=0x207, WM_MBUTTONUP=0x208, WM_MOUSEWHEEL=0x20A, WM_MOUSEHWHEEL=0x20E, WM_XBUTTONDOWN=0x20B, WM_XBUTTONUP=0x20C;
 public const uint LLMHF_INJECTED=1, LLKHF_INJECTED=0x10;
 public delegate IntPtr HookProc(int code,IntPtr wParam,IntPtr lParam);
 [DllImport("user32.dll",SetLastError=true)] public static extern IntPtr SetWindowsHookEx(int idHook,HookProc callback,IntPtr module,uint threadId);
 [DllImport("user32.dll",SetLastError=true)] public static extern bool UnhookWindowsHookEx(IntPtr handle);
 [DllImport("user32.dll")] public static extern IntPtr CallNextHookEx(IntPtr handle,int code,IntPtr wParam,IntPtr lParam);
 [DllImport("user32.dll")] static extern uint SendInput(uint count,[In] INPUT[] inputs,int size);
 [DllImport("user32.dll")] static extern bool SetCursorPos(int x,int y);
 [StructLayout(LayoutKind.Sequential)] public struct POINT {public int X;public int Y;}
 [StructLayout(LayoutKind.Sequential)] public struct MSLLHOOKSTRUCT {public POINT pt;public uint mouseData;public uint flags;public uint time;public IntPtr dwExtraInfo;}
 [StructLayout(LayoutKind.Sequential)] public struct KBDLLHOOKSTRUCT {public uint vkCode;public uint scanCode;public uint flags;public uint time;public IntPtr dwExtraInfo;}
 [StructLayout(LayoutKind.Sequential)] struct INPUT {public uint type;public InputUnion U;}
 [StructLayout(LayoutKind.Explicit)] struct InputUnion {[FieldOffset(0)] public MOUSEINPUT mi;[FieldOffset(0)] public KEYBDINPUT ki;}
 [StructLayout(LayoutKind.Sequential)] struct MOUSEINPUT {public int dx,dy;public uint mouseData,dwFlags,time;public IntPtr dwExtraInfo;}
 [StructLayout(LayoutKind.Sequential)] struct KEYBDINPUT {public ushort wVk,wScan;public uint dwFlags,time;public IntPtr dwExtraInfo;}
 static void Emit(INPUT input) {if(SendInput(1,new[]{input},Marshal.SizeOf<INPUT>())!=1)throw new InvalidOperationException("Input rejected by Windows; check permissions.");}
 public static void Move(int x,int y)=>SetCursorPos(x,y);
 public static void Click(string button,bool down) {
  var b=button.ToLowerInvariant();
  uint flag=b switch {
   "left"=>down?0x2u:0x4u, "right"=>down?0x8u:0x10u,
   "middle"=>down?0x20u:0x40u, "x1" or "x2"=>down?0x80u:0x100u,
   _=>throw new InvalidOperationException("Unsupported mouse button "+button)
  };
  Emit(new INPUT{type=0,U=new InputUnion{mi=new MOUSEINPUT{dwFlags=flag,mouseData=b=="x2"?2u:b=="x1"?1u:0u}}});
 }
 public static void Scroll(int horizontal,int vertical) {
  if(vertical!=0)Emit(new INPUT{type=0,U=new InputUnion{mi=new MOUSEINPUT{dwFlags=0x800,mouseData=unchecked((uint)(vertical*120))}}});
  if(horizontal!=0)Emit(new INPUT{type=0,U=new InputUnion{mi=new MOUSEINPUT{dwFlags=0x1000,mouseData=unchecked((uint)(horizontal*120))}}});
 }
 public static void Key(ushort vk,bool down) {
  Emit(new INPUT{type=1,U=new InputUnion{ki=new KEYBDINPUT{wVk=vk,dwFlags=down?0u:2u}}});
 }
}