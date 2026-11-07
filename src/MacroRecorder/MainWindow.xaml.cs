using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MasoomDev.MacroRecorder;

public partial class MainWindow : Window
{
 readonly ObservableCollection<MacroEvent> events = new();
 readonly Stopwatch clock = new();
 readonly HashSet<int> activeHotkeys = new();
 readonly HashSet<int> replayKeys = new();
 readonly HashSet<string> replayButtons = new();
 readonly JsonSerializerOptions json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
 NativeInput.HookProc? mouseCallback, keyboardCallback;
 IntPtr mouseHook, keyboardHook;
 CancellationTokenSource? replayCancellation;
 volatile bool recording, playing, paused;
 long lastMovementTick;
 int lastX = int.MinValue, lastY = int.MinValue;

 public MainWindow() { InitializeComponent(); EventList.ItemsSource = events; }
 void Status(string value) => Dispatcher.BeginInvoke(() => StatusText.Text = value);
 void Refresh()
 {
  EventsText.Text = $"Events: {events.Count}";
  StopRecordButton.IsEnabled = recording;
  PlayButton.IsEnabled = !recording && !playing;
  StopPlayButton.IsEnabled = playing;
  PauseButton.IsEnabled = playing;
  PauseButton.Content = paused ? "▶ Resume" : "⏸ Pause";
 }
 void AddEvent(MacroEvent item)
 {
  if (!recording) return;
  if (events.Count >= 100000) { StopRecording(); MessageBox.Show("Recording stopped at 100,000 events."); return; }
  events.Add(item); Refresh();
 }
 private void StartRecording_Click(object sender, RoutedEventArgs e)
 {
  if (recording || playing) return;
  if (MouseCheck.IsChecked != true && KeyboardCheck.IsChecked != true) { MessageBox.Show("Enable mouse or keyboard recording."); return; }
  events.Clear(); activeHotkeys.Clear(); lastX = int.MinValue; lastY = int.MinValue; lastMovementTick = 0;
  mouseCallback = OnMouse; keyboardCallback = OnKeyboard;
  try
  {
   recording = true; clock.Restart();
   keyboardHook = NativeInput.SetWindowsHookEx(NativeInput.WH_KEYBOARD_LL, keyboardCallback, IntPtr.Zero, 0);
   if (keyboardHook == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
   if (MouseCheck.IsChecked == true)
   {
    mouseHook = NativeInput.SetWindowsHookEx(NativeInput.WH_MOUSE_LL, mouseCallback, IntPtr.Zero, 0);
    if (mouseHook == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
   }
   Status("Recording — press F8 to stop"); Refresh();
  }
  catch(Exception ex) { StopRecording(); MessageBox.Show(ex.Message, "Recording error"); }
 }
 private void StopRecording_Click(object sender, RoutedEventArgs e) => StopRecording();
 void StopRecording()
 {
  recording = false; clock.Stop();
  if (mouseHook != IntPtr.Zero) { NativeInput.UnhookWindowsHookEx(mouseHook); mouseHook = IntPtr.Zero; }
  if (keyboardHook != IntPtr.Zero) { NativeInput.UnhookWindowsHookEx(keyboardHook); keyboardHook = IntPtr.Zero; }
  activeHotkeys.Clear(); Status("Recording stopped"); Refresh();
 }
 IntPtr OnMouse(int code, IntPtr msg, IntPtr ptr)
 {
  if (code >= 0 && recording)
  {
   var data = Marshal.PtrToStructure<NativeInput.MSLLHOOKSTRUCT>(ptr);
   if ((data.flags & NativeInput.LLMHF_INJECTED) == 0)
   {
    double t = clock.Elapsed.TotalSeconds;
    int m = (int)msg;
    if (m == NativeInput.WM_MOUSEMOVE && MovementCheck.IsChecked == true)
    {
     long tick = Stopwatch.GetTimestamp();
     if ((tick - lastMovementTick) * 1000.0 / Stopwatch.Frequency >= 30 && Math.Abs(data.pt.X - lastX) + Math.Abs(data.pt.Y - lastY) >= 3)
     {
      lastMovementTick = tick; lastX = data.pt.X; lastY = data.pt.Y;
      AddEvent(new MacroEvent { Time = t, Type = "mouse_move", X = data.pt.X, Y = data.pt.Y });
     }
    }
    else if (m is NativeInput.WM_LBUTTONDOWN or NativeInput.WM_LBUTTONUP or NativeInput.WM_RBUTTONDOWN or NativeInput.WM_RBUTTONUP or NativeInput.WM_MBUTTONDOWN or NativeInput.WM_MBUTTONUP or NativeInput.WM_XBUTTONDOWN or NativeInput.WM_XBUTTONUP)
    {
     string button = m switch
     {
      NativeInput.WM_LBUTTONDOWN or NativeInput.WM_LBUTTONUP => "left",
      NativeInput.WM_RBUTTONDOWN or NativeInput.WM_RBUTTONUP => "right",
      NativeInput.WM_MBUTTONDOWN or NativeInput.WM_MBUTTONUP => "middle",
      _ => (data.mouseData >> 16) == 2 ? "x2" : "x1"
     };
     bool down = m is NativeInput.WM_LBUTTONDOWN or NativeInput.WM_RBUTTONDOWN or NativeInput.WM_MBUTTONDOWN or NativeInput.WM_XBUTTONDOWN;
     AddEvent(new MacroEvent { Time=t, Type="mouse_click", X=data.pt.X, Y=data.pt.Y, Button=button, Pressed=down });
    }
    else if (m is NativeInput.WM_MOUSEWHEEL or NativeInput.WM_MOUSEHWHEEL)
    {
     int delta = unchecked((short)(data.mouseData >> 16)) / 120;
     AddEvent(new MacroEvent { Time=t, Type="mouse_scroll", X=data.pt.X, Y=data.pt.Y, Dx=m==NativeInput.WM_MOUSEHWHEEL ? delta : 0, Dy=m==NativeInput.WM_MOUSEWHEEL ? delta : 0 });
    }
   }
  }
  return NativeInput.CallNextHookEx(mouseHook, code, msg, ptr);
 }
 IntPtr OnKeyboard(int code, IntPtr msg, IntPtr ptr)
 {
  if (code >= 0)
  {
   var data = Marshal.PtrToStructure<NativeInput.KBDLLHOOKSTRUCT>(ptr);
   int m=(int)msg, vk=(int)data.vkCode;
   bool down = m is NativeInput.WM_KEYDOWN or NativeInput.WM_SYSKEYDOWN;
   bool up = m is NativeInput.WM_KEYUP or NativeInput.WM_SYSKEYUP;
   if ((data.flags & NativeInput.LLKHF_INJECTED) == 0 && (down || up))
   {
    if (vk == 0x77 || vk == 0x78)
    {
     if (up) activeHotkeys.Remove(vk);
     if (down && activeHotkeys.Add(vk))
     {
      if (vk == 0x77) Dispatcher.BeginInvoke(() => { if (recording) StopRecording(); else StopPlayback(); });
      else if (playing) Dispatcher.BeginInvoke(() => TogglePause());
     }
     if (vk == 0x77 && (recording || playing)) return (IntPtr)1;
     if (vk == 0x78 && playing) return (IntPtr)1;
    }
    if (recording && KeyboardCheck.IsChecked == true)
    {
     AddEvent(new MacroEvent { Time=clock.Elapsed.TotalSeconds, Type="key", Pressed=down, VirtualKey=vk,
      Key=new MacroKey { Kind="special", Value=KeyInterop.KeyFromVirtualKey(vk).ToString().ToLowerInvariant() } });
    }
   }
  }
  return NativeInput.CallNextHookEx(keyboardHook, code, msg, ptr);
 }
 private void Clear_Click(object sender, RoutedEventArgs e)
 {
  if (playing || recording) return;
  events.Clear(); Refresh(); Status("Cleared");
 }
 private void Save_Click(object sender, RoutedEventArgs e)
 {
  if (playing || recording || events.Count==0) return;
  var dialog = new SaveFileDialog { Filter="Macro JSON|*.json", FileName="recording.json" };
  if (dialog.ShowDialog()!=true) return;
  try { System.IO.File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(new MacroFile { Events=events.ToList() },json)); Status("Saved " + System.IO.Path.GetFileName(dialog.FileName)); }
  catch(Exception ex) { MessageBox.Show(ex.Message,"Save error"); }
 }
 private void Load_Click(object sender, RoutedEventArgs e)
 {
  if (playing || recording) return;
  var dialog = new OpenFileDialog { Filter="Macro JSON|*.json" };
  if (dialog.ShowDialog()!=true) return;
  try
  {
   var file = JsonSerializer.Deserialize<MacroFile>(System.IO.File.ReadAllText(dialog.FileName),json) ?? throw new InvalidOperationException("Invalid macro file");
   if (file.Events is null || file.Events.Count>100000 || file.Events.Any(x => !double.IsFinite(x.Time) || x.Time<0 || x.Time>86400 || x.Type is not ("key" or "mouse_move" or "mouse_click" or "mouse_scroll") || x.X is < -100000 or > 100000 || x.Y is < -100000 or > 100000)) throw new InvalidOperationException("Invalid or oversized macro file");
   events.Clear(); foreach(var item in file.Events.OrderBy(x=>x.Time)) events.Add(item);
   Refresh(); Status("Loaded " + System.IO.Path.GetFileName(dialog.FileName));
  }
  catch(Exception ex) { MessageBox.Show(ex.Message,"Load error"); }
 }
 private async void Play_Click(object sender, RoutedEventArgs e)
 {
  if (playing || recording || events.Count==0) return;
  if (!int.TryParse(RepeatCount.Text,out int loops) || loops is <1 or >10000 ||
   !double.TryParse(RepeatMinutes.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out double minutes) ||
   !double.IsFinite(minutes) || minutes is <0 or >1440)
  { MessageBox.Show("Repeat count must be 1–10000, and minutes must be 0–1440."); return; }
  if (!double.TryParse(((ComboBoxItem)SpeedCombo.SelectedItem).Tag?.ToString(),CultureInfo.InvariantCulture,out double speed)) speed=1;
  var snapshot=events.ToArray();
  replayCancellation=new CancellationTokenSource(); playing=true; paused=false; activeHotkeys.Clear(); Refresh();
  keyboardCallback=OnKeyboard;
  keyboardHook=NativeInput.SetWindowsHookEx(NativeInput.WH_KEYBOARD_LL,keyboardCallback,IntPtr.Zero,0);
  if (keyboardHook==IntPtr.Zero) { playing=false; Refresh(); replayCancellation.Dispose(); replayCancellation=null; MessageBox.Show("Emergency stop hotkey unavailable."); return; }
  var token=replayCancellation.Token;
  try
  {
   await Task.Run(async () =>
   {
    var deadline=minutes>0 ? DateTimeOffset.UtcNow.AddMinutes(minutes) : DateTimeOffset.MaxValue;
    for(int n=0; !token.IsCancellationRequested && (minutes>0 ? DateTimeOffset.UtcNow<deadline : n<loops); n++)
    {
     Status($"Playback loop {n+1}");
     double previous=0;
     foreach(var item in snapshot)
     {
      token.ThrowIfCancellationRequested();
      if(DateTimeOffset.UtcNow>=deadline) break;
      await DelayWithPause(Math.Max(0,(item.Time-previous)*1000/speed),deadline,token);
      previous=item.Time; token.ThrowIfCancellationRequested();
      Execute(item);
     }
    }
   },token);
   Status("Playback completed");
  }
  catch(OperationCanceledException) { Status("Playback stopped"); }
  catch(Exception ex) { Status("Playback error"); MessageBox.Show(ex.Message,"Playback error"); }
  finally
  {
   playing=false; paused=false; ReleaseHeld();
   if(keyboardHook!=IntPtr.Zero) { NativeInput.UnhookWindowsHookEx(keyboardHook); keyboardHook=IntPtr.Zero; }
   replayCancellation?.Dispose(); replayCancellation=null; Refresh();
  }
 }
 async Task DelayWithPause(double milliseconds,DateTimeOffset deadline,CancellationToken token)
 {
  double remaining=milliseconds;
  while(remaining>0 || paused)
  {
   token.ThrowIfCancellationRequested();
   if(DateTimeOffset.UtcNow>=deadline) throw new OperationCanceledException(token);
   if(paused) { await Task.Delay(30,token); continue; }
   double step=Math.Min(remaining,20);
   await Task.Delay(TimeSpan.FromMilliseconds(step),token); remaining-=step;
  }
 }
 void Execute(MacroEvent e)
 {
  switch(e.Type)
  {
   case "mouse_move": NativeInput.Move(e.X,e.Y); break;
   case "mouse_click":
    NativeInput.Move(e.X,e.Y); NativeInput.Click(e.Button,e.Pressed);
    if(e.Pressed) replayButtons.Add(e.Button); else replayButtons.Remove(e.Button);
    break;
   case "mouse_scroll": NativeInput.Scroll(e.Dx,e.Dy); break;
   case "key":
    int? vk=e.VirtualKey ?? LegacyKey(e.Key);
    if(vk is null) return;
    NativeInput.Key((ushort)vk.Value,e.Pressed);
    if(e.Pressed) replayKeys.Add(vk.Value); else replayKeys.Remove(vk.Value);
    break;
  }
 }
 static int? LegacyKey(MacroKey? key)
 {
  if(key?.Value is null) return null;
  string value=key.Value.ToLowerInvariant();
  if(key.Kind=="char" && value.Length==1)
  {
   char c=char.ToUpperInvariant(value[0]);
   if(c is >= 'A' and <= 'Z' or >= '0' and <= '9') return c;
   if(c==' ') return 0x20;
   return null;
  }
  var names=new Dictionary<string,int> {
   ["space"]=32,["enter"]=13,["return"]=13,["tab"]=9,["backspace"]=8,["esc"]=27,["shift"]=16,
   ["shift_l"]=160,["shift_r"]=161,["ctrl"]=17,["ctrl_l"]=162,["ctrl_r"]=163,
   ["alt"]=18,["alt_l"]=164,["alt_r"]=165,["cmd"]=91,["cmd_l"]=91,["cmd_r"]=92,
   ["delete"]=46,["up"]=38,["down"]=40,["left"]=37,["right"]=39,["home"]=36,["end"]=35,
   ["page_up"]=33,["page_down"]=34,["caps_lock"]=20
  };
  if(names.TryGetValue(value,out int code)) return code;
  if(value.StartsWith('f') && int.TryParse(value[1..],out int f) && f is >=1 and <=24) return 0x6F+f;
  if(Enum.TryParse<Key>(value,true,out var parsed)) return KeyInterop.VirtualKeyFromKey(parsed);
  return null;
 }
 void ReleaseHeld()
 {
  foreach(int vk in replayKeys.ToArray()) try { NativeInput.Key((ushort)vk,false); } catch { }
  foreach(string b in replayButtons.ToArray()) try { NativeInput.Click(b,false); } catch { }
  replayKeys.Clear(); replayButtons.Clear();
 }
 private void Pause_Click(object sender,RoutedEventArgs e) => TogglePause();
 void TogglePause() { if(!playing) return; paused=!paused; Refresh(); Status(paused?"Paused":"Playing"); }
 private void StopPlayback_Click(object sender,RoutedEventArgs e) => StopPlayback();
 void StopPlayback() { replayCancellation?.Cancel(); Status("Stopping playback"); }
 private void OnWindowClosing(object? sender,CancelEventArgs e)
 {
  if(recording) StopRecording();
  if(playing) { StopPlayback(); ReleaseHeld(); }
  if(mouseHook!=IntPtr.Zero) NativeInput.UnhookWindowsHookEx(mouseHook);
  if(keyboardHook!=IntPtr.Zero) NativeInput.UnhookWindowsHookEx(keyboardHook);
 }
}
