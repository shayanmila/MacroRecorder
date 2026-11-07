using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MasoomDev.MacroRecorder;
public static class InfoDialog
{
 public static void ShowHelp(Window owner)
 {
  Show(owner,"Help — Masoom Macro Recorder",
   "QUICK START\n\n1. Select Mouse, Keyboard, or both.\n2. Click Record and perform the actions you want to capture.\n3. Press F8 or click Stop to finish.\n4. Review events, then select Play.\n\nPLAYBACK\n\nRepeats: number of times to play the macro.\nMinutes: timed playback; when greater than zero it overrides Repeats.\nSpeed: 0.5x to 2x.\nF8: emergency stop. F9: pause or resume playback.\n\nFILES AND EDITING\n\nSave/Load use local JSON files. Select an event to Delete or move it Up/Down. Drag the divider above the event list to resize it. Resize the window from its borders or lower-right corner.\n\nSAFETY\n\nRecorded macros can contain typed passwords or other sensitive information. Store them securely and avoid recording credentials. Run macros only on systems you control. Playback can be limited in administrator windows.");
 }
 public static void ShowAbout(Window owner)
 {
  Show(owner,"About — Masoom Macro Recorder",
   "MASOOM MACRO RECORDER\n\nNative Windows desktop macro recorder\nMade by MasoomDev\n\nTechnology: C# / .NET 8 / WPF\nVersion: "+(typeof(App).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).OfType<System.Reflection.AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion ?? typeof(App).Assembly.GetName().Version?.ToString(3) ?? "Unknown")+"\n\nFeatures: mouse and keyboard recording, JSON save/load, repeat playback, adjustable speed, event editing and emergency hotkeys.\n\nGitHub: github.com/shayanmila/MacroRecorder\n\n© MasoomDev. All rights reserved.");
 }
 static void Show(Window owner,string title,string content)
 {
  var window=new Window { Title=title,Owner=owner,Width=480,Height=430,MinWidth=350,MinHeight=280,WindowStartupLocation=WindowStartupLocation.CenterOwner,Background=Brushes.White,ResizeMode=ResizeMode.CanResize };
  var panel=new DockPanel { Margin=new Thickness(18) };
  var close=new Button { Content="Close",Width=90,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,12,0,0) };
  close.Click+=(_,_)=>window.Close();
  DockPanel.SetDock(close,Dock.Bottom);panel.Children.Add(close);
  panel.Children.Add(new ScrollViewer { VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Content=new TextBlock { Text=content,TextWrapping=TextWrapping.Wrap,FontFamily=new FontFamily("Segoe UI"),FontSize=14,Foreground=Brushes.Black } });
  window.Content=panel;window.ShowDialog();
 }
}