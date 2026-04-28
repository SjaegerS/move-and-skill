using UnityEditor;
using UnityEngine;

namespace UltimateBackgroundsCollection
{
    [CustomEditor(typeof(ParallaxEffectFlyMode))]
    public class ParallaxEffectFlyModeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ParallaxEffectFlyMode parallaxEffectFlyMode = (ParallaxEffectFlyMode)target;

            EditorGUILayout.HelpBox("To ensure proper positioning of the layers, it is not recommanded to change the default values of this script.", MessageType.Info);

            parallaxEffectFlyMode.parallaxIntensityX =
                EditorGUILayout.Slider(new GUIContent("Parallax Intensity X", "The higher the value, the greater the parallax effect on the horizontal axis."), parallaxEffectFlyMode.parallaxIntensityX, 0.0f, 1.0f);
            parallaxEffectFlyMode.independantSpeed =
                EditorGUILayout.Slider(new GUIContent("Independant Speed", "The layer will move independently to the left if the value is less than 0, and to the right if the value is greater than 0."), parallaxEffectFlyMode.independantSpeed, -1.0f, 1.0f);

            if (GUI.changed)
                EditorUtility.SetDirty(parallaxEffectFlyMode);
        }
    }
}