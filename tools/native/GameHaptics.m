// Vibración de los mandos en macOS con el framework de Apple (GameController + CoreHaptics), para Rumble.cs: el Input
// System de Unity no hace vibrar los mandos en macOS (los de Xbox sobre todo). Se compila con build_haptics.sh en
// unity/MazeOfKnight/Assets/Plugins/macOS/GameHaptics.bundle.
#import <Foundation/Foundation.h>
#import <GameController/GameController.h>
#import <CoreHaptics/CoreHaptics.h>

static CHHapticEngine *engine;
static GCController *engineOwner;
static id<CHHapticAdvancedPatternPlayer> player;

static GCController *CurrentController(void) {
    GCController *c = GCController.current;
    if (c == nil) c = GCController.controllers.firstObject;
    return c;
}

static BOOL EnsureEngine(void) {
    GCController *c = CurrentController();
    if (c == nil || c.haptics == nil) { engine = nil; engineOwner = nil; return NO; }
    if (engine != nil && engineOwner == c) return YES;
    engine = [c.haptics createEngineWithLocality:GCHapticsLocalityDefault];
    engineOwner = c;
    if (engine == nil) return NO;
    engine.resetHandler = ^{ [engine startAndReturnError:nil]; };
    engine.stoppedHandler = ^(CHHapticEngineStoppedReason reason) { engine = nil; };
    return [engine startAndReturnError:nil];
}

// 1 si hay un mando que puede vibrar.
int GH_Available(void) {
    @autoreleasepool { return EnsureEngine() ? 1 : 0; }
}

// Vibra con esa fuerza (0–1) y nitidez (0 grave, 1 aguda) durante 'seconds'; sustituye lo que estuviera vibrando.
void GH_Rumble(float intensity, float sharpness, float seconds) {
    @autoreleasepool {
        if (!EnsureEngine()) return;
        NSError *err = nil;
        CHHapticEventParameter *i = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:intensity];
        CHHapticEventParameter *s = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:sharpness];
        CHHapticEvent *ev = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticContinuous parameters:@[i, s] relativeTime:0 duration:seconds];
        CHHapticPattern *pattern = [[CHHapticPattern alloc] initWithEvents:@[ev] parameters:@[] error:&err];
        if (pattern == nil) return;
        if (player != nil) [player stopAtTime:CHHapticTimeImmediate error:nil];
        player = [engine createAdvancedPlayerWithPattern:pattern error:&err];
        [player startAtTime:CHHapticTimeImmediate error:&err];
    }
}

void GH_Stop(void) {
    @autoreleasepool {
        if (player != nil) [player stopAtTime:CHHapticTimeImmediate error:nil];
        player = nil;
    }
}
