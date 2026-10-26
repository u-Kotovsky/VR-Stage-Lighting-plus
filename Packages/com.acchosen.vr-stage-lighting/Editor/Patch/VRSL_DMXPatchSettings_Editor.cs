#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [CustomEditor(typeof(VRSL_DMXPatchSettings))]
    public class VRSL_DMXPatchSettings_Editor: Editor 
    {
        private SerializedProperty data, idStrings, targetScene, scenePath;
        private SceneAsset sceneAsset;
        private VRSL_DMXPatchSettings settings;
        
        private void OnEnable()
        {
            settings = (VRSL_DMXPatchSettings) target;
            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.scenePath);
        }

        public override void OnInspectorGUI() 
        {
            DrawDefaultInspector();
            var so = new SerializedObject(settings);
            // Load the real class values into the serialized copy
            so.Update();
            
            if (settings != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Target Scene", sceneAsset, typeof(SceneAsset), false);
                EditorGUI.EndDisabledGroup();
                
                if (GUILayout.Button("Save Scene DMX Patch Data"))
                {
                    settings.SetDmxFixtureData();
                    settings.ForceSave();
                }
                if (GUILayout.Button("Load Scene DMX Patch Data"))
                {
                    settings.LoadDmxFixtureData();
                }
                if (GUILayout.Button("Export To JSON File"))
                {
                    settings.ToJsonFile(true);
                }
                if (GUILayout.Button("Export To MVR File"))
                {
                    settings.ToMVRFile();
                }
                if (GUILayout.Button("Export To PDF File (Windows)"))
                {
#if !UNITY_EDITOR_LINUX && !UNITY_ANDROID  && !UNITY_IOS
                    settings.ToPdf();
#else
                    EditorUtility.DisplayDialog("PDF export error", "PDF export is currently a Windows only feature", "OK", "Cancel");
#endif
                }
                if (settings.data != null)
                {
                    for (var i = 0; i < settings.data.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(settings.data[i].name);
                        EditorGUILayout.LabelField("DMX Universe: " + settings.data[i].dmxUniverse, GUILayout.Width(100f));
                        EditorGUILayout.LabelField("DMX Channel: " + settings.data[i].dmxChannel);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            // Write back changed values and evtl mark as dirty and handle undo/redo
            so.ApplyModifiedProperties();
        }
    }
}
#endif