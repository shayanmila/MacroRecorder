# Generate a branded Windows ICO with a 256x256 PNG entry.
# Windows PowerShell on the GitHub Windows runner provides System.Drawing.
Add-Type -AssemblyName System.Drawing
$target = Join-Path $PSScriptRoot '..\src\MacroRecorder\Assets\MasoomMacro.ico'
New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
$bitmap = New-Object System.Drawing.Bitmap(256,256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::Transparent)
$purple = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(117,64,172))
$white = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
function RoundedRect($x,$y,$w,$h,$r) {
 $p = New-Object System.Drawing.Drawing2D.GraphicsPath
 $d=$r*2
 $p.AddArc($x,$y,$d,$d,180,90);$p.AddArc(($x+$w-$d),$y,$d,$d,270,90)
 $p.AddArc(($x+$w-$d),($y+$h-$d),$d,$d,0,90);$p.AddArc($x,($y+$h-$d),$d,$d,90,90)
 $p.CloseFigure();return $p
}
$graphics.FillPath($purple,(RoundedRect 10 10 236 236 52))
$graphics.FillPath($white,(RoundedRect 43 60 170 135 25))
$graphics.FillEllipse($purple,70,92,45,45)
$graphics.FillEllipse($purple,141,92,45,45)
$graphics.FillPath($purple,(RoundedRect 72 153 112 16 7))
$stream = New-Object System.IO.MemoryStream
$bitmap.Save($stream,[System.Drawing.Imaging.ImageFormat]::Png)
$bytes=$stream.ToArray()
$fs=[System.IO.File]::Create($target)
$writer=New-Object System.IO.BinaryWriter($fs)
$writer.Write([UInt16]0);$writer.Write([UInt16]1);$writer.Write([UInt16]1)
$writer.Write([byte]0);$writer.Write([byte]0);$writer.Write([byte]0);$writer.Write([byte]0)
$writer.Write([UInt16]1);$writer.Write([UInt16]32)
$writer.Write([UInt32]$bytes.Length);$writer.Write([UInt32]22)
$writer.Write($bytes)
$writer.Dispose();$stream.Dispose();$graphics.Dispose();$bitmap.Dispose()
Write-Host "Created $target"
