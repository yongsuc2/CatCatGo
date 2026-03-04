param(
    [Parameter(Mandatory=$true)]
    [string]$InputVideo,

    [Parameter(Mandatory=$true)]
    [int]$Fps,

    [Parameter(Mandatory=$true)]
    [string]$BgColorHex,

    [int]$FuzzPercent = 25
)

$ErrorActionPreference = "Stop"

$baseName = [System.IO.Path]::GetFileNameWithoutExtension($InputVideo)
$framesDir = "${baseName}_frames"
$alphaDir  = "${baseName}_frames_alpha"

New-Item -ItemType Directory -Force -Path $framesDir  | Out-Null
New-Item -ItemType Directory -Force -Path $alphaDir   | Out-Null

Write-Host "[1/2] 동영상 -> PNG 프레임 추출 (fps=$Fps)"
& ffmpeg -y -i $InputVideo -vf "fps=$Fps" "$framesDir/frame_%04d.png"

$firstFrame = Join-Path $framesDir "frame_0001.png"
$size = & magick identify -format "%wx%h" $firstFrame
$w = [int]($size -split 'x')[0] - 1
$h = [int]($size -split 'x')[1] - 1

Write-Host "[2/2] PNG 배경 투명화 - floodfill (bg=#${BgColorHex}, fuzz=${FuzzPercent}%)"

Get-ChildItem $framesDir -Filter "frame_*.png" | ForEach-Object {
    $inPath  = $_.FullName
    $outPath = Join-Path $alphaDir $_.Name

    & magick $inPath -fuzz "${FuzzPercent}%" -fill none `
        -draw "color 0,0 floodfill" `
        -draw "color $w,0 floodfill" `
        -draw "color 0,$h floodfill" `
        -draw "color $w,$h floodfill" `
        $outPath

    Write-Host "  $($_.Name)"
}

Write-Host "완료:"
Write-Host "  원본 프레임    : $framesDir"
Write-Host "  투명 PNG 프레임: $alphaDir"
