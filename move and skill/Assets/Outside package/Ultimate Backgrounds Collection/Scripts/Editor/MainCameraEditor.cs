using UnityEditor;
using UnityEngine;

namespace UltimateBackgroundsCollection
{
    [CustomEditor(typeof(MainCamera))]
    public class MainCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MainCamera mainCamera = (MainCamera)target;

            EditorGUILayout.HelpBox("In some scenes, if Vertical Axis and Camera Size are locked by default, it is not recommended to unlock them.", MessageType.Info);

            mainCamera.smoothCamera =
                EditorGUILayout.Toggle(new GUIContent("Smooth Camera", "Enable or disable smooth player tracking by the camera."), mainCamera.smoothCamera);
            mainCamera.lockVerticalAxis =
                EditorGUILayout.Toggle(new GUIContent("Lock Vertical Axis", "It is not recommended to uncheck if the vertical axis is locked by default."), mainCamera.lockVerticalAxis);
            mainCamera.lockCameraSize =
                EditorGUILayout.Toggle(new GUIContent("Lock Camera Size", "It is not recommended to uncheck if the camera size is locked by default."), mainCamera.lockCameraSize);

            if (!mainCamera.lockCameraSize)
            {
                mainCamera.cameraSize =
                    EditorGUILayout.Slider(new GUIContent("Camera Size", "Increases or decreases the camera size."), mainCamera.cameraSize, 5.0f, 9.0f);

                mainCamera.playerOffsetY =
                    EditorGUILayout.Slider(new GUIContent("Player Offset Y", "Adjusts the offset between the camera and the player on the vertical axis. The default value is 0.7."), mainCamera.playerOffsetY, 0.0f, 1.0f);
            }

            mainCamera.playerOffsetX =
                EditorGUILayout.Slider(new GUIContent("Player Offset X", "Adjusts the offset between the camera and the player on the horizontal axis. The default value is 0."), mainCamera.playerOffsetX, -1.0f, 1.0f);

            if (GUI.changed)
                EditorUtility.SetDirty(mainCamera);
        }
    }
}
