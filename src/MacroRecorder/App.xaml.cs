using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace MasoomDev.MacroRecorder;

public partial class App : Application
{
 private static int reported;
 protected override void OnStartup(StartupEventArgs e)
 {
  AppDomain.CurrentDomain.UnhandledException += (_,args) =>
   Report(args.ExceptionObject as Exception ?? new Exception("Unknown fatal exception"));
  DispatcherUnhandledException += OnDispatcherError;
  TaskScheduler.UnobservedTaskException += (_,args) => { Report(args.Exception); args.SetObserved(); };
  base.OnStartup(e);
 }
 private void OnDispatcherError(object sender, DispatcherUnhandledExceptionEventArgs e)
 {
  Report(e.Exception);
  e.Handled = true;
  Shutdown(1);
 }
 private static void Report(Exception ex)
 {
  try
  {
   string dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MasoomDev","MacroRecorder");
   Directory.CreateDirectory(dir);
   string path=Path.Combine(dir,"startup-error.log");
   File.AppendAllText(path,$"[{DateTimeOffset.Now:O}] {ex}\r\n\r\n",Encoding.UTF8);
   if(System.Threading.Interlocked.Exchange(ref reported,1)==0)
    MessageBox.Show("Macro Recorder encountered an error.\n\nA diagnostic log was saved to:\n"+path+"\n\n"+ex.Message,"Macro Recorder — Startup Error",MessageBoxButton.OK,MessageBoxImage.Error);
  }
  catch { }
 }
}