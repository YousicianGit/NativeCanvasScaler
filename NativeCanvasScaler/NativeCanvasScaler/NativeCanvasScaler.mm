#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>

extern "C" float GetScreenScaleFactor()
{
    return NSScreen.mainScreen.backingScaleFactor;
}
