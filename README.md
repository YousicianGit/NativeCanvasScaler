# Native Canvas Scaler
The Canvas Scaler component is used for controlling the overall scale and pixel density of UI elements in the Canvas.
This implementation replicates the native behaviour of the device it runs on.

When the screen is smaller than the reference resolution, the contents of the canvas will be scaled down to fit the screen. It guarantees that the reference resolution will always be available and UI elements will not overlap.

When the screen is larger than the reference resolution, it increases the size of the canvas without scaling the screen above the device's screen density factor. For example, on iOS the width and height of the Canvas will be will be equal to the device resolution in "points":
https://www.paintcodeapp.com/news/ultimate-guide-to-iphone-resolutions

This is a trimmed-down version of Unity's Canvas Scaler, hence the naming of variables following Unity's style.

You can read more about how does works by checking the comments in [NativeCanvasScaler](Assets/NativeCanvasScaler.cs).
