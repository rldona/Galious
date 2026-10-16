#!/bin/sh
# Compila GameHaptics.m en el plugin de macOS de Unity (universal: arm64 y x86_64).
set -e
cd "$(dirname "$0")"
OUT=../../unity/MazeOfKnight/Assets/Plugins/macOS/GameHaptics.bundle
mkdir -p "$OUT/Contents/MacOS"
clang -bundle -fobjc-arc -arch arm64 -arch x86_64 -mmacosx-version-min=11.0 \
  -framework Foundation -framework GameController -framework CoreHaptics \
  -o "$OUT/Contents/MacOS/GameHaptics" GameHaptics.m
cat > "$OUT/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
<key>CFBundleExecutable</key><string>GameHaptics</string>
<key>CFBundleIdentifier</key><string>com.mazeofhollow.gamehaptics</string>
<key>CFBundlePackageType</key><string>BNDL</string>
<key>CFBundleName</key><string>GameHaptics</string>
</dict></plist>
PLIST
echo "GameHaptics.bundle → $OUT"
