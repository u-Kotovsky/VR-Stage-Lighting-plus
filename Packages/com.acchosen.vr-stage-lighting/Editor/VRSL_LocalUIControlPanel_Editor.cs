using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UDONSHARP
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCShader;
#else
using static UnityEngine.Shader;
using UnityEngine.Rendering;
#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UDONSHARP
using UdonSharpEditor;
#endif
#endif

using VRSL;
 
#if !COMPILER_UDONSHARP && UNITY_EDITOR
namespace VRSL.EditorScripts
{
    [CustomEditor(typeof(VRSL_LocalUIControlPanel))]
    public class VRSL_LocalUIControlPanel_Editor : Editor
    {
        SerializedProperty audioLinkLasers, audiolinkLights, dmxLights, isUsingDMX,isUsingAudioLink, fixtureDefGUID, volumetricMeshQuality;


        public void OnEnable() 
        {
            audioLinkLasers = serializedObject.FindProperty("audioLinkLasers");
            audiolinkLights = serializedObject.FindProperty("audiolinkLights");
            dmxLights = serializedObject.FindProperty("dmxLights");
            isUsingDMX = serializedObject.FindProperty("isUsingDMX");
            isUsingAudioLink = serializedObject.FindProperty("isUsingAudioLink");
            fixtureDefGUID = serializedObject.FindProperty("fixtureDefGUID");
            volumetricMeshQuality = serializedObject.FindProperty("volumetricMeshQuality");
        }
        
        public void _RemoveEmptyMaterials()
        {
            var controlPanel = (VRSL_LocalUIControlPanel)target;
            var count = 0;
            for(int i = 0; i < controlPanel.fixtureMaterials.Length; i++)
            {
                if(controlPanel.fixtureMaterials[i] == null)
                {
                    count++;
                }
            }
            
            var newArray = new Material[controlPanel.fixtureMaterials.Length - count];
            var otherCount = 0;
            for(int i = 0; i < controlPanel.fixtureMaterials.Length; i++)
            {
                if(controlPanel.fixtureMaterials[i] != null)
                {
                    newArray[otherCount] = controlPanel.fixtureMaterials[i];
                    otherCount++;
                }
            }
            controlPanel.fixtureMaterials = newArray;
        }
        
        public static void DrawLogo()
        {
            //GUILayout.BeginArea(new Rect(0,0, Screen.width, Screen.height));
            //GUILayout.FlexibleSpace();
            //GUI.DrawTexture(pos,logo,ScaleMode.ScaleToFit);
            //EditorGUI.DrawPreviewTexture(new Rect(0,0,400,150), logo);
            Vector2 contentOffset = new Vector2(0f, -2f);
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fixedHeight = 150;
            //style.fixedWidth = 300;
            style.contentOffset = contentOffset;
            style.alignment = TextAnchor.MiddleCenter;
            var rect = GUILayoutUtility.GetRect(300f, 140f, style);
            //GUILayout.Label(logo,style, GUILayout.MaxWidth(500), GUILayout.MaxHeight(200));
            //GUI.Box(rect, logo,style);
            //GUILayout.Label(logo);
            //GUILayout.FlexibleSpace();
            //GUILayout.EndArea();
        }
        
        private static Rect DrawShurikenCenteredTitle(string title, Vector2 contentOffset, int headerHeight)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fontSize = 14;
            style.fixedHeight = headerHeight;
            style.contentOffset = contentOffset;
            style.alignment = TextAnchor.MiddleCenter;
            var rect = GUILayoutUtility.GetRect(16f, headerHeight, style);

            GUI.Box(rect, title, style);
            return rect;
        }
        
        public static void ShurikenHeaderCentered(string title)
        {
            DrawShurikenCenteredTitle(title, new Vector2(0f, -2f), 22);
        }
        
        private GUIContent Label(string label)
        {
            var content = new GUIContent();
            content.text = label;
            return content;
        }
        
        public override void OnInspectorGUI()
        {
#if UDONSHARP
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
#endif
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            DrawLogo();
            //ShurikenHeaderCentered(GetVersion());
            EditorGUILayout.Space();
            VRSL_LocalUIControlPanel controlPanel = (VRSL_LocalUIControlPanel)target;
            if (GUILayout.Button(new GUIContent("Force Update Target AudioLink Sample Texture",
                    "Updates all AudioLink VRSL Fixtures to sample from the selected target texture when texture sampling is enabled on the fixture.")))
            {
                controlPanel._ForceUpdateVideoSampleTexture();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Apply Quality Modes to All Materials",
                    "Applies currently set quality modes to all materials.")))
            {
                controlPanel._UpdateAllQualityModes();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Search For VRSL Materials",
                    "Adds VRSL Compatible Materials in scene to materials lists")))
            {
                controlPanel._GetNewMaterials();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Remove Empty Materials",
                    "Removes all Empty Material slots from material lists.")))
            {
                _RemoveEmptyMaterials();
            }
            EditorGUILayout.Space();
            if(isUsingDMX.boolValue)
            {
                //EditorGUILayout.PropertyField(dmxLights,true);
                for(int i = 0; i < dmxLights.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(dmxLights.GetArrayElementAtIndex(i));
                }
            }
            if(isUsingAudioLink.boolValue)
            {
                for(int i = 0; i < dmxLights.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(audiolinkLights.GetArrayElementAtIndex(i));
                    EditorGUILayout.PropertyField(audioLinkLasers.GetArrayElementAtIndex(i));
                    //EditorGUILayout.PropertyField(audiolinkLights, true);
                    //EditorGUILayout.PropertyField(audioLinkLasers,true);
                }
            }
            EditorGUILayout.LabelField("Fixture Definition GUID: " + fixtureDefGUID.stringValue);
            EditorGUILayout.LabelField("Volumetric Mesh Quality State: " + volumetricMeshQuality.intValue);
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                //Debug.Log("Found changes");
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
        }
    }
}
#endif