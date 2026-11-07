using System.Text.Json.Serialization;
namespace MasoomDev.MacroRecorder;
public sealed class MacroFile
{
 [JsonPropertyName("app")] public string App { get; set; } = "Masoom Macro Recorder";
 [JsonPropertyName("version")] public int Version { get; set; } = 2;
 [JsonPropertyName("events")] public List<MacroEvent> Events { get; set; } = new();
}
public sealed class MacroEvent
{
 [JsonPropertyName("time")] public double Time { get; set; }
 [JsonPropertyName("type")] public string Type { get; set; } = "";
 [JsonPropertyName("x")] public int X { get; set; }
 [JsonPropertyName("y")] public int Y { get; set; }
 [JsonPropertyName("dx")] public int Dx { get; set; }
 [JsonPropertyName("dy")] public int Dy { get; set; }
 [JsonPropertyName("button")] public string Button { get; set; } = "left";
 [JsonPropertyName("pressed")] public bool Pressed { get; set; }
 [JsonPropertyName("key")] public MacroKey? Key { get; set; }
 [JsonPropertyName("vk")] public int? VirtualKey { get; set; }
 public override string ToString() => Type switch
 {
  "mouse_move" => $"{Time,8:F3}s  MOVE      {X},{Y}",
  "mouse_click" => $"{Time,8:F3}s  {Button,-6}    {(Pressed ? "DOWN" : "UP"),-4} {X},{Y}",
  "mouse_scroll" => $"{Time,8:F3}s  SCROLL    {Dx},{Dy}",
  "key" => $"{Time,8:F3}s  KEY       {(Pressed ? "DOWN" : "UP"),-4} {Key?.Value ?? VirtualKey?.ToString() ?? "?"}",
  _ => $"{Time,8:F3}s  {Type}"
 };
}
public sealed class MacroKey
{
 [JsonPropertyName("kind")] public string Kind { get; set; } = "special";
 [JsonPropertyName("value")] public string? Value { get; set; }
}