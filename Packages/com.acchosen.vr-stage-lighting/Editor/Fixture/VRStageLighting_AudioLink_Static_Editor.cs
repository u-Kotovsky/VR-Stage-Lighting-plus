#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UDONSHARP
using UdonSharpEditor;
#endif

namespace VRSL.EditorScripts
{
    [InitializeOnLoad]
    [CustomEditor(typeof(VRStageLighting_AudioLink_Static))]
    [CanEditMultipleObjects]
    public class VRStageLighting_AudioLink_Static_Editor : VRSL_UdonEditor
    {
        private const string DisplayVersionText = "VR Stage Lighting: <b><color=white>{0}</color></b>"; // 6a15ce is hard for eye to read.
        private const string PackagePath = "Packages/com.acchosen.vr-stage-lighting/package.json";
        private readonly Package _package = VRSLPackageUtility.ReadPackage(PackagePath);
        
        void OnSceneGUI(SceneView sceneView) { }

        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += HierarchyChanged;
            SceneView.duringSceneGui += OnSceneGUI;
        }
    
        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= HierarchyChanged;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void UpdateSettings(VRStageLighting_AudioLink_Static fixture)
        {
            if (fixture.objRenderers.Length <= 0) return;
            if (fixture.objRenderers.Any(renderer => renderer == null))
            {
                Debug.LogError($"'{fixture.name}' has unassigned objRenderers! Please remove them or assign objects!");
                return;
            }
            
            fixture._SetProps();
            
            if (Application.isPlaying)
            {
                fixture._UpdateInstancedProperties();
            }
            else
            {
                fixture._UpdateInstancedPropertiesSansAudioLink();
                fixture._CheckConstraints(fixture);
            }
        }
    
        private void HierarchyChanged()
        {
            var fixture = (VRStageLighting_AudioLink_Static)target;
            UpdateSettings(fixture);
        }
        
        public static GUIStyle SectionLabel()
        {
            GUIStyle g = new GUIStyle();
            g.fontSize = 14;
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            return g;
        }

        public override void OnInspectorGUI()
        {
#if UDONSHARP
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
#endif
            VRSLStyles.ShurikenHeaderCentered($"UnityEditor: {Application.unityVersion}, {DisplayVersionText.Replace("{0}", _package.version)}");
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            var fixture = (VRStageLighting_AudioLink_Static)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            

            EditorGUILayout.Space();
            VRSLStyles.GUILine(1);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Fine Intensity Controls", SectionLabel());
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(fixture.finalIntensityComponentMode)), 
                new GUIContent("Control Component Intensities"));
            
            EditorGUI.indentLevel++;
            if(serializedObject.FindProperty(nameof(fixture.finalIntensityComponentMode)).boolValue)
            {
                serializedObject.FindProperty(nameof(fixture.finalIntensityVolumetric)).floatValue  = EditorGUILayout.Slider(new GUIContent("Volumetric Intensity",
                "Sets the maximum brightness value of Global Intensity for volumetric meshes only. Good for personalized settings of the max brightness of the shader by other users via UI."), 
                    fixture.finalIntensityVolumetric, 0.0f, 1.0f);
                
                serializedObject.FindProperty(nameof(fixture.finalIntensityProjection)).floatValue  = EditorGUILayout.Slider(new GUIContent("Projection Intensity",
                "Sets the maximum brightness value of Global Intensity for projection meshes only. Good for personalized settings of the max brightness of the shader by other users via UI."), 
                    fixture.finalIntensityProjection, 0.0f, 1.0f);

                serializedObject.FindProperty(nameof(fixture.finalIntensityFixture)).floatValue  = EditorGUILayout.Slider(new GUIContent("Fixture/Other Intensity",
                "Sets the maximum brightness value of Global Intensity for everything else. Good for personalized settings of the max brightness of the shader by other users via UI."), 
                    fixture.finalIntensityFixture, 0.0f, 1.0f);
            }
            else
            {
                serializedObject.FindProperty(nameof(fixture.finalIntensity)).floatValue  = EditorGUILayout.Slider(new GUIContent("Final Intensity",
                "Sets the maximum brightness value of Global Intensity. Good for personalized settings of the max brightness of the shader by other users via UI."), fixture.finalIntensity, 0.0f, 1.0f);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                foreach(var obj in targets)
                {
                    VRStageLighting_AudioLink_Static f = (VRStageLighting_AudioLink_Static)obj;
                    UpdateSettings(f);
                }
                //EditorGUIUtility.LookLikeControls();
            }
        }
    }
}
#endif