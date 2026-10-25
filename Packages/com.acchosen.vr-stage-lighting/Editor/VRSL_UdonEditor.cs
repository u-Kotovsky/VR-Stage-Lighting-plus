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


    #if UNITY_EDITOR && !COMPILER_UDONSHARP
namespace VRSL.EditorScripts
{
    [CanEditMultipleObjects]
    public class VRSL_UdonEditor : Editor
    {
        private const string DisplayVersionText = "VR Stage Lighting: <b><color=white>{0}</color></b>"; // 6a15ce is hard for eye to read.
        private const string PackagePath = "Packages/com.acchosen.vr-stage-lighting/package.json";
        private readonly Package _package = VRSLPackageUtility.ReadPackage(PackagePath);

        protected void DrawCenteredInfo()
        {
            VRSLStyles.ShurikenHeaderCentered($"UnityEditor: {Application.unityVersion}, {DisplayVersionText.Replace("{0}", _package.version)}");
        }
    }
}
#endif