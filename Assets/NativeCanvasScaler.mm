#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern "C" float GetScreenScaleFactor()
{
    return UIScreen.mainScreen.scale;
}
