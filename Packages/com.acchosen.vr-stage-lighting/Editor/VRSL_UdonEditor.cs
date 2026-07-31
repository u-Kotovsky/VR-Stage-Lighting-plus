using UnityEngine;

#if UDONSHARP
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using System.IO;

#if UDONSHARP
using UdonSharpEditor;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
#endif

#endif


namespace VRSL.EditorScripts
{
    #if UNITY_EDITOR && !COMPILER_UDONSHARP
    [CanEditMultipleObjects]
    public class VRSL_UdonEditor : Editor
    {
        public static Texture logo;
        
        public void OnEnable() 
        {
            logo = Resources.Load("VRStageLighting-Logo") as Texture;
        }
        
        public static string GetVersion()
        {
            var path = Application.dataPath;
            path = path.Replace("Assets","");
            path += "Packages"  + "\\" + "com.acchosen.vr-stage-lighting" + "\\" + "Runtime" + "\\"  + "VERSION.txt";

            var reader = new StreamReader(path); 
            var versionNum = reader.ReadToEnd();
            return $"VR Stage Lighting ver: <b><color=#b33cff>{versionNum}</color></b>";
        }
        
        public static void DrawLogo()
        {
            Vector2 contentOffset = new Vector2(0f, -2f);
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fixedHeight = 150;
            //style.fixedWidth = 300;
            style.contentOffset = contentOffset;
            style.alignment = TextAnchor.MiddleCenter;
            var rect = GUILayoutUtility.GetRect(300f, 140f, style);
            GUI.Box(rect, logo,style);
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
    }
    #endif
}
