using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

/// <summary>
/// The Canvas Scaler component is used for controlling the overall scale and pixel density of UI elements in the Canvas.
/// This implementation replicates the native behaviour of the device it runs on.
///
/// When the screen is smaller than the reference resolution, the contents of the canvas will be scaled down to fit the
/// screen. It guarantees that the reference resolution will always be available and UI elements will not overlap.
///
/// When the screen is larger than the reference resolution, it increases the size of the canvas without scaling the
/// screen above the device's screen density factor. For example, on iOS the width and height of the Canvas will be
/// will be equal to the device resolution in "points":
/// https://www.paintcodeapp.com/news/ultimate-guide-to-iphone-resolutions
/// </summary>
/// <remarks>
/// This is a trimmed-down version of Unity's Canvas Scaler, hence the naming of variables following Unity's style.
///
/// You can find the source code for this plugin in the following repository:
/// https://github.com/YousicianGit/NativeCanvasScaler
/// </remarks>
// ReSharper disable InconsistentNaming
[RequireComponent(typeof(Canvas))]
[ExecuteAlways]
[DisallowMultipleComponent]
public class NativeCanvasScaler : UIBehaviour
{
	[Tooltip("If a sprite has this 'Pixels Per Unit' setting, then one pixel in the sprite will cover one unit in the UI.")]
	[SerializeField] protected float m_ReferencePixelsPerUnit = 100;

	/// <summary>
	/// If a sprite has this 'Pixels Per Unit' setting, then one pixel in the sprite will cover one unit in the UI.
	/// </summary>
	public float referencePixelsPerUnit
	{
		get => m_ReferencePixelsPerUnit;
		set => m_ReferencePixelsPerUnit = value;
	}

	[Tooltip("The minimum resolution the UI layout is designed for. If the screen resolution is larger, the UI will be expanded, and if it's smaller, the UI will be scaled down.")]
	[SerializeField] protected Vector2 m_ReferenceResolution = new Vector2(375, 667);

	/// <summary>
	/// The minimum resolution the UI layout is designed for.
	/// </summary>
	/// <remarks>
	/// If the screen resolution is larger, the UI will be expanded, and if it's smaller, the UI will be scaled down.
	/// </remarks>
	public Vector2 referenceResolution
	{
		get => m_ReferenceResolution;
		set => m_ReferenceResolution = value;
	}

	private Canvas m_Canvas;
	private float m_PrevScaleFactor = 1;
	private float m_PrevReferencePixelsPerUnit = 100;

#if UNITY_EDITOR
	/// <summary>
	/// Device information to show in the inspector when the Device Simulator is active
	/// </summary>
	public static string DeviceInfo { get; private set; }

	/// <summary>
	/// When Device Simulator is inactive, a density factor can be selected from the inspector.
	/// </summary>
	/// <remarks>
	/// This is useful for testing desktop scaling, which is currently not supported by Device Simulator.
	/// </remarks>
	public static float SimulatedDensityFactor { get; set; } = 2f;
#endif

	protected override void OnEnable()
	{
		base.OnEnable();
		m_Canvas = GetComponent<Canvas>();
		Handle();

		Assert.AreEqual(m_Canvas.renderMode, RenderMode.ScreenSpaceOverlay);
	}

	protected override void OnDisable()
	{
		SetScaleFactor(1);
		SetReferencePixelsPerUnit(100);
		base.OnDisable();
	}

	/// <summary>
	/// Checks each frame whether the canvas needs to be rescaled.
	/// </summary>
	protected virtual void Update()
	{
		Handle();
	}

	///<summary>
	/// Method that handles calculations of canvas scaling.
	///</summary>
	protected virtual void Handle()
	{
		if (m_Canvas == null || !m_Canvas.isRootCanvas)
			return;

#if UNITY_EDITOR
		float screenDensityFactor = SimulatedDensityFactor;

		if (SystemInfo.operatingSystem.StartsWith("iOS"))
		{
			// Guess the screen density factor based on the resolution
			DeviceInfo = "Unknown device (1x)";

			void GuessDensity(int pixels, float density, string device)
			{
				if (Screen.width == pixels || Screen.height == pixels)
				{
					screenDensityFactor = density;
					DeviceInfo = $"{device} ({density:0.##}x)";
				}
			}

			GuessDensity(2160, 2f, "iPad 7, 8");
			GuessDensity(2048, 2f, "iPad Retina");
			GuessDensity(2360, 2f, "iPad Air 4");
			GuessDensity(2224, 2f, "iPad Pro 10.5\"");
			GuessDensity(2388, 2f, "iPad Pro 11\"");
			GuessDensity(2732, 2f, "iPad Pro 12.9\"");

			GuessDensity(960, 2f, "iPhone 4");
			GuessDensity(1136, 2f, "iPhone 5, SE, iPod Touch");
			GuessDensity(1334, 2f, "iPhone 6, 7, 8");
			GuessDensity(2208, 3f, "iPhone 6, 7, 8 Plus"); // Actual render resolution, used by game window presets
			GuessDensity(1920, 3f / 1.15f, "iPhone 6, 7, 8 Plus (Sim)"); // Actual screen resolution, used by Device Simulator
			GuessDensity(2436, 3f, "iPhone X, XS, 11 Pro");
			GuessDensity(1792, 2f, "iPhone XR, 11");
			GuessDensity(2688, 3f, "iPhone XS Max, 11 Pro Max");
		}
		else if (SystemInfo.operatingSystem.StartsWith("Android"))
		{
			// Guess the screen density factor based on DPI. This assumes that the DPI values are simulated correctly
			// by the Device Simulator
			screenDensityFactor = Screen.dpi / 160;

			// Show the density qualifier in the inspector
			// See: https://developer.android.com/training/multiscreen/screendensities
			string densityQualifier;
			if (screenDensityFactor < 1.5f)
			{
				densityQualifier = "mdpi";
			}
			else if (screenDensityFactor < 2f)
			{
				densityQualifier = "hdpi";
			}
			else if (screenDensityFactor < 3f)
			{
				densityQualifier = "xhdpi";
			}
			else if (screenDensityFactor < 4f)
			{
				densityQualifier = "xxhdpi";
			}
			else
			{
				densityQualifier = "xxxhdpi";
			}

			DeviceInfo = $"Android {densityQualifier} ({screenDensityFactor:0.##}x)";
		}
#elif UNITY_EDITOR
		// Use the screen density factor set in the canvas scaler's inspector
		var screenDensityFactor = ScreenDensityFactor;
#elif UNITY_ANDROID
		// On Android a medium-density screen with 160 dpi is the "baseline" density
		// See: https://developer.android.com/training/multiscreen/screendensities
		var screenDensityFactor = Screen.dpi / 160f;
#elif UNITY_IOS
		// This returns UIScreen.mainScreen.scale
		var screenDensityFactor = GetScreenScaleFactor();
#elif UNITY_STANDALONE_OSX
		// This returns NSScreen.mainScreen.backingScaleFactor
		// Note: It's always 2.0 for retina screens and 1.0 otherwise. While this value does not take into account the
		// screen scaling, it's actually taken into account in Screen.width/height.
		// For example on a Macbook Pro 15" with 2880x1800 screen resolution:
		// When scaling is set to "looks like 1680x1050" the render resolution is 3360x2100.
		// When scaling is set to "looks like 1024x640" the render resolution is 2048x1280.
		var screenDensityFactor = GetScreenScaleFactor();
#elif UNITY_STANDALONE_WIN
		// On Windows the "100% scaling" is 96 DPI.
		var screenDensityFactor = Screen.dpi / 96;
#else
		#error Screen density factor is not defined for this platform
#endif

		// If we don't get a valid screen density (e.g. Screen.dpi is zero), we assume 1x density
		if (screenDensityFactor <= 0)
		{
			screenDensityFactor = 1;
		}

		var scaleFactor = Mathf.Min(Screen.width / m_ReferenceResolution.x, Screen.height / m_ReferenceResolution.y, screenDensityFactor);

		SetScaleFactor(scaleFactor);
		SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
	}

#if UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
	[System.Runtime.InteropServices.DllImport("__Internal")]
	private static extern float GetScreenScaleFactor();
#endif

	/// <summary>
	/// Sets the scale factor on the canvas.
	/// </summary>
	/// <param name="scaleFactor">The scale factor to use.</param>
	protected void SetScaleFactor(float scaleFactor)
	{
		if (scaleFactor == m_PrevScaleFactor)
			return;

		m_Canvas.scaleFactor = scaleFactor;
		m_PrevScaleFactor = scaleFactor;
	}

	/// <summary>
	/// Sets the referencePixelsPerUnit on the Canvas.
	/// </summary>
	/// <param name="referencePixelsPerUnit">The new reference pixels per Unity value</param>
	protected void SetReferencePixelsPerUnit(float referencePixelsPerUnit)
	{
		if (referencePixelsPerUnit == m_PrevReferencePixelsPerUnit)
			return;

		m_Canvas.referencePixelsPerUnit = referencePixelsPerUnit;
		m_PrevReferencePixelsPerUnit = referencePixelsPerUnit;
	}
}
