#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [CustomEditor(typeof(VRSL_DMXPatchSettings))]
    public class VRSL_DMXPatchSettings_Editor: Editor 
    {
        private SerializedProperty data, idStrings, targetScene, scenePath;
        SceneAsset sceneAsset;
        VRSL_DMXPatchSettings settings = null;
        private void OnEnable()
        {
            // // Link the properties
            // data = serializedObject.FindProperty("data");
            // idStrings = serializedObject.FindProperty("idStrings");
            // targetScene = serializedObject.FindProperty("targetScene");
            // scenePath = serializedObject.FindProperty("scenePath");
            
            settings = (VRSL_DMXPatchSettings) target;
            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.scenePath);
        }

        public override void OnInspectorGUI() 
        {
            DrawDefaultInspector();
            SerializedObject so = new SerializedObject(settings);
            // Load the real class values into the serialized copy
            so.Update();
            if(settings != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Target Scene", sceneAsset, typeof(SceneAsset), false);
                EditorGUI.EndDisabledGroup();
                // if(GUILayout.Button("Check Data Status"))
                // {
                //     settings.CheckData();
                // }
                if(GUILayout.Button("Save Scene DMX Patch Data"))
                {
                    settings.SetDMXFixtureData();
                    settings.ForceSave();
                 //   EditorUtility.SetDirty(settings);
                 //   Undo.RecordObject(settings, "Undo Save Scene DXM Patch Data");

                }
                if(GUILayout.Button("Load Scene DMX Patch Data"))
                {
                    settings.LoadDMXFixtureData();
                }
                if(GUILayout.Button("Export To JSON File"))
                {
                    settings.ToJsonFile(true);
                }
                if(GUILayout.Button("Export To MVR File"))
                {
                    settings.ToMVRFile();
                }
                if(GUILayout.Button("Export To PDF File (Windows)"))
                {
#if !UNITY_EDITOR_LINUX && !UNITY_ANDROID  && !UNITY_IOS
                    settings.ToPDF();
#else
                    EditorUtility.DisplayDialog("PDF export error", "PDF export is currently a Windows only feature", "OK", "Cancel");
#endif
                }
                if(settings.data != null)
                {
                    for(int i = 0; i < settings.data.Length; i++)
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