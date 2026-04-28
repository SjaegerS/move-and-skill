using UnityEditor;
using UnityEngine;

namespace UltimateBackgroundsCollection
{
    [CustomEditor(typeof(MainCameraFlyMode))]
    public class MainCameraFlyModeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MainCameraFlyMode mainCameraFlyMode = (MainCameraFlyMode)target;

            mainCameraFlyMode.smoothCamera =
                EditorGUILayout.Toggle(new GUIContent("Smooth Camera", "Enable or disable smooth player tracking by the camera."), mainCameraFlyMode.smoothCamera);

            mainCameraFlyMode.playerOffsetX =
                EditorGUILayout.Slider(new GUIContent("Player Offset X", "Adjusts the offset between the camera and the player on the horizontal axis. The default value is 0."), mainCameraFlyMode.playerOffsetX, -1.0f, 1.0f);

            if (GUI.changed)
                EditorUtility.SetDirty(mainCameraFlyMode);
        }
    }
}
