#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [InitializeOnLoad]
    [CustomEditor(typeof(VRStageLighting_AudioLink_Static))]
    [CanEditMultipleObjects]
    public class VRStageLighting_AudioLink_Static_Editor : VRSL_UdonEditor
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

        void UpdateSettings(VRStageLighting_AudioLink_Static fixture)
        {
            try{
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
                            fixture._CheckConstraints(fixture);
                        }
                    }
                }
            }
            catch(NullReferenceException e)
            {
                e.ToString();
            }
        }
    
        private void HierarchyChanged( )
        {
            VRStageLighting_AudioLink_Static fixture = (VRStageLighting_AudioLink_Static)target;
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
            void GuiLine( int i_height = 1 )

   {
        try{
       //GUIStyle g = GUIStyle.none;
       //g.fixedHeight = 6;
       Rect rect = EditorGUILayout.GetControlRect(false, i_height);

       rect.height = i_height;

       EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );
        }
        catch(Exception e)
        {
            e.GetType();
        }

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

            VRStageLighting_AudioLink_Static fixture = (VRStageLighting_AudioLink_Static)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();




            EditorGUILayout.Space();
            GuiLine();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Fine Intensity Controls", SectionLabel());
            serializedObject.Update();
            EditorGUILayout.PropertyField( serializedObject.FindProperty("finalIntensityComponentMode"), new GUIContent("Control Component Intensities"));
            EditorGUI.indentLevel++;
            if(serializedObject.FindProperty("finalIntensityComponentMode").boolValue){

                serializedObject.FindProperty("finalIntensityVolumetric").floatValue  = EditorGUILayout.Slider(new GUIContent("Volumetric Intensity",
                "Sets the maximum brightness value of Global Intensity for volumetric meshes only. Good for personalized settings of the max brightness of the shader by other users via UI."), fixture.finalIntensityVolumetric, 0.0f, 1.0f);
                
                serializedObject.FindProperty("finalIntensityProjection").floatValue  = EditorGUILayout.Slider(new GUIContent("Projection Intensity",
                "Sets the maximum brightness value of Global Intensity for projection meshes only. Good for personalized settings of the max brightness of the shader by other users via UI."), fixture.finalIntensityProjection, 0.0f, 1.0f);

                serializedObject.FindProperty("finalIntensityFixture").floatValue  = EditorGUILayout.Slider(new GUIContent("Fixture/Other Intensity",
                "Sets the maximum brightness value of Global Intensity for everything else. Good for personalized settings of the max brightness of the shader by other users via UI."), fixture.finalIntensityFixture, 0.0f, 1.0f);
            }
            else{
                serializedObject.FindProperty("finalIntensity").floatValue  = EditorGUILayout.Slider(new GUIContent("Final Intensity",
                "Sets the maximum brightness value of Global Intensity. Good for personalized settings of the max brightness of the shader by other users via UI."), fixture.finalIntensity, 0.0f, 1.0f);
            }

            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                foreach(UnityEngine.Object obj in targets)
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