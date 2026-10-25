#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [CustomEditor(typeof(VRStageLighting_AudioLink_Laser))]
    [CanEditMultipleObjects]
    public class VRStageLighting_AudioLink_Laser_Editor : VRSL_UdonEditor
    {
        private void OnSceneGUI(SceneView sceneView) { }

        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += HierarchyChanged;
            SceneView.duringSceneGui += OnSceneGUI;
        }
    
        private void OnDisable( )
        {
            EditorApplication.hierarchyChanged -= HierarchyChanged;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void HierarchyChanged()
        {
            var fixture = (VRStageLighting_AudioLink_Laser)target;
            UpdateSettings(fixture);
        }
        
        private void UpdateSettings(VRStageLighting_AudioLink_Laser fixture)
        {
            if (fixture.objRenderers.Length <= 0) return;
            if (fixture.objRenderers.Any(rend => rend == null)) return;
            
            fixture._SetProps();
            
            if (Application.isPlaying)
            {
                fixture._UpdateInstancedProperties();
            }
            else
            {
                fixture._UpdateInstancedPropertiesSansAudioLink();
            }
        }

        public override void OnInspectorGUI()
        {
#if UDONSHARP
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
#endif
            DrawCenteredInfo();
            
            EditorGUILayout.Space();
            
            var fixture = (VRStageLighting_AudioLink_Laser)target;
            
            EditorGUI.BeginChangeCheck();
            
            base.OnInspectorGUI();
            
            if (EditorGUI.EndChangeCheck())
            {
                foreach(var obj in targets)
                {
                    var f = (VRStageLighting_AudioLink_Laser)obj;
                    UpdateSettings(f);
                }
            }
        }
    }
}
#endif