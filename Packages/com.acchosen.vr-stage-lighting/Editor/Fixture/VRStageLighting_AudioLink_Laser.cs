#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [CustomEditor(typeof(VRStageLighting_AudioLink_Laser))]
    [CanEditMultipleObjects]
    public class VRStageLighting_AudioLink_Laser_Editor : VRSL_UdonEditor
    {
        void OnSceneGUI(SceneView sceneView)
        {

        }

        new void OnEnable( )
        {
            base.OnEnable();
            EditorApplication.hierarchyChanged += HierarchyChanged;
            SceneView.duringSceneGui += this.OnSceneGUI;
        }
    
        void OnDisable( )
        {
            EditorApplication.hierarchyChanged -= HierarchyChanged;
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }

        private void HierarchyChanged( )
        {
            VRStageLighting_AudioLink_Laser fixture = (VRStageLighting_AudioLink_Laser)target;
            UpdateSettings(fixture);
        }



        void UpdateSettings(VRStageLighting_AudioLink_Laser fixture)
        {
            if(fixture.objRenderers.Length > 0)
            {
                bool isEmpty = false;
                foreach(MeshRenderer rend in fixture.objRenderers)
                {
                    if(rend == null)
                    {
                        isEmpty = true;
                        break;
                    }
                }
                if(!isEmpty)
                {
                    fixture._SetProps();
                    if(Application.isPlaying)
                    {
                        fixture._UpdateInstancedProperties();
                    }
                    else
                    {
                        fixture._UpdateInstancedPropertiesSansAudioLink();
                    }

                }
            }
        }
        public static GUIStyle SectionLabel()
        {
            GUIStyle g = new GUIStyle();
            g.fontSize = 15;
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            return g;
        }

        public override void OnInspectorGUI()
        {
#if UDONSHARP
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
#endif
            DrawLogo();
            ShurikenHeaderCentered(GetVersion());
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            //EditorGUILayout.Space();
            VRStageLighting_AudioLink_Laser fixture = (VRStageLighting_AudioLink_Laser)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if(EditorGUI.EndChangeCheck())
            {
                foreach(Object obj in targets)
                {
                    VRStageLighting_AudioLink_Laser f = (VRStageLighting_AudioLink_Laser)obj;
                    UpdateSettings(f);
                }
            }
        }
    }
}
#endif