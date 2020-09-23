#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NativeCanvasScaler), true)]
[CanEditMultipleObjects]
public class NativeCanvasScalerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Screen density simulation");

        var deviceSimulatorActive = SystemInfo.operatingSystem.StartsWith("Android") ||
                                    SystemInfo.operatingSystem.StartsWith("iOS");

        EditorGUILayout.BeginHorizontal();
        if (deviceSimulatorActive)
        {
            EditorGUILayout.PrefixLabel("Device");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(NativeCanvasScaler.DeviceInfo);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            this.DrawFactorToggle("1x", 1f);
            this.DrawFactorToggle("1.5x", 1.5f);
            this.DrawFactorToggle("2x", 2f);
            this.DrawFactorToggle("2.5x", 2.5f);
            this.DrawFactorToggle("3x", 3f);
        }
        EditorGUILayout.EndHorizontal();

        var helpBoxText = deviceSimulatorActive
            ? "Device Simulator active. Screen density is determined automatically."
            : "You can use the Device Simulator to accurately simulate mobile devices.";
        EditorGUILayout.HelpBox(helpBoxText, MessageType.Info);
    }

    private void DrawFactorToggle(string label, float factor, string tooltip = "")
    {
        if (GUILayout.Toggle(NativeCanvasScaler.SimulatedDensityFactor == factor, new GUIContent(label, tooltip), EditorStyles.toolbarButton))
        {
            NativeCanvasScaler.SimulatedDensityFactor = factor;

            foreach (var canvasScaler in this.targets)
            {
                EditorUtility.SetDirty(canvasScaler);
            }
        }
    }
}
#endif
