#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
#if UDONSHARP
using UdonSharpEditor;
#endif

namespace VRSL.EditorScripts
{
    public class VRSL_DMXPatchExporter : Editor
    {
        private static VRSL_LocalUIControlPanel _panel;
        private static List<GameObject> _sceneObjects = new List<GameObject>();
        private static bool _hasLocalPanel;

        private const string Prefix = "VRSL DMX Patch Exporter";
        
        private static List<GameObject> GetAllObjectsOnlyInScene()
        {
            var objectsInScene = new List<GameObject>();

            foreach (var go in (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)))
            {
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && 
                    !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                {
                    objectsInScene.Add(go);
                }
            }

            return objectsInScene;
        }
        
        private static void CheckForLocalPanel()
        {
            _sceneObjects = GetAllObjectsOnlyInScene();
            _hasLocalPanel = false;
            _panel = null;
            
            foreach (var go in _sceneObjects)
            {
                #if UDONSHARP
                #pragma warning disable 0618 //suppressing obsoletion warnings
                _panel =  go.GetUdonSharpComponent<VRSL_LocalUIControlPanel>();
                #pragma warning restore 0618
                #else
                panel =  go.GetComponent<VRSL_LocalUIControlPanel>();
                #endif
                
                if (_panel != null)
                {
                    _hasLocalPanel = true;
                    break;
                }
            }
        }

        [MenuItem("VRSL/Save Patch Data", priority = 500)]
        public static void SavePatchData()
        {
            CheckForLocalPanel();
            if (!_hasLocalPanel)
            {
                Debug.LogWarning($"{Prefix}: Please make sure there is a VRSL-LocalUIControlPanel in your scene before attempting to save."); 
                return;
            }
            
            if (_panel.fixtureSaveFile == "NONE")
            {
                try
                {
                    var asset = ScriptableObject.CreateInstance<VRSL_DMXPatchSettings>();
                    asset.SetDmxFixtureData();
                    var name = $"VRSL DMX Patch Data_{SceneManager.GetActiveScene().name}.asset";
                    var parentDirectory = "VRSL DMX Patch Folder";
                    if(AssetDatabase.IsValidFolder("Assets/" + parentDirectory) == false)
                    {
                        AssetDatabase.CreateFolder("Assets", parentDirectory);
                    }
                    string path = "Assets/" + parentDirectory + "/" + name;
                    AssetDatabase.CreateAsset(asset, path);
                    //AssetDatabase.SaveAssets();

                    //EditorUtility.FocusProjectWindow();

                    //Selection.activeObject = asset;

                    var so = new SerializedObject(_panel);
                    so.FindProperty(nameof(_panel.fixtureSaveFile)).stringValue = AssetDatabase.GUIDFromAssetPath(path).ToString();
                    so.ApplyModifiedProperties();
#if UDONSHARP
                    #pragma warning disable 0618 //suppressing obsoletion warnings
                    _panel.UpdateProxy();
                    #pragma warning restore 0618 //suppressing obsoletion warnings
                    _panel.fixtureSaveFile = so.FindProperty("fixtureSaveFile").stringValue;
                    #pragma warning disable 0618 //suppressing obsoletion warnings
                    _panel.ApplyProxyModifications();
                    #pragma warning restore 0618 //suppressing obsoletion warnings
#else
                    panel.fixtureSaveFile = so.FindProperty("fixtureSaveFile").stringValue;
#endif

                    //asset = (VRSL_DMXPatchSettings) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(panel.fixtureSaveFile), typeof(VRSL_DMXPatchSettings));
                    //asset.SetScene();
                    //asset.SetDMXFixtureData();
                    asset.ForceSave();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{Prefix}: Failed to create patch data.");
                    Debug.LogException(e);
                }
            }
            else
            {
                try
                {
                    var asset = (VRSL_DMXPatchSettings) AssetDatabase.LoadAssetAtPath(
                        AssetDatabase.GUIDToAssetPath(_panel.fixtureSaveFile), typeof(VRSL_DMXPatchSettings));
                    asset.SetScene();
                    asset.SetDmxFixtureData();
                    asset.ForceSave();
                }
                catch (NullReferenceException e)
                {
                    // Debug.LogError($"{Prefix}: Could not find patch data file. Removing Link patch data link.");
                    Debug.LogException(e);
                    var so = new SerializedObject(_panel);
                    so.FindProperty(nameof(_panel.fixtureSaveFile)).stringValue = "NONE";
                    so.ApplyModifiedProperties();
#if UDONSHARP
                    #pragma warning disable 0618 //suppressing obsoletion warnings
                    _panel.UpdateProxy();
                    #pragma warning restore 0618 //suppressing obsoletion warnings
                    _panel.fixtureSaveFile = so.FindProperty("fixtureSaveFile").stringValue;
                    #pragma warning disable 0618 //suppressing obsoletion warnings
                    _panel.ApplyProxyModifications();
                    #pragma warning restore 0618 //suppressing obsoletion warnings
#else
                    panel.fixtureSaveFile = so.FindProperty("fixtureSaveFile").stringValue;
#endif
                    SavePatchData();
                }
                catch(Exception e)
                {
                    Debug.LogError($"{Prefix}: Failed to save patch data.");
                    Debug.LogException(e);
                }
            }
        }
        
        [MenuItem("VRSL/Load Patch Data", priority = 501)]
        public static void LoadPatchData()
        {
            CheckForLocalPanel();
            if (!_hasLocalPanel)
            {
                Debug.LogWarning($"{Prefix}: Please make sure there is a VRSL-LocalUIControlPanel in your scene before attempting to Load."); 
                return;
            }
            
            if(_panel.fixtureSaveFile != "NONE")
            {
                try
                {
                    var asset = (VRSL_DMXPatchSettings) AssetDatabase.LoadAssetAtPath(
                        AssetDatabase.GUIDToAssetPath(_panel.fixtureSaveFile), typeof(VRSL_DMXPatchSettings));
                    asset.LoadDmxFixtureData();
                }
                catch
                {
                    Debug.LogError($"{Prefix}: Failed to load patch data.");
                }
            }
            else
            {
                Debug.LogError($"{Prefix}: Fixture Save File Not Found. The file may have been lost or has not been created yet.");
            }
        }
        
        [MenuItem("VRSL/Export/To JSON", priority = 502)]
        public static void ExportToJson()
        {
            CheckForLocalPanel();
            
            if (!_hasLocalPanel)
            {
                Debug.LogWarning($"{Prefix}: Please make sure there is a VRSL-LocalUIControlPanel in your scene before attempting to export."); 
                return;
            }
            
            if (_panel.fixtureSaveFile != "NONE")
            {
                try
                {
                    var asset = (VRSL_DMXPatchSettings) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_panel.fixtureSaveFile), typeof(VRSL_DMXPatchSettings));
                    asset.ToJsonFile(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{Prefix}: Failed to export patch data.");
                    Debug.LogException(e);
                }
            }
            else
            {
                Debug.LogError($"{Prefix}: Fixture Save File Not Found. The file may have been lost or has not been created yet.");
            }
        }

        [MenuItem("VRSL/Export/To MVR", priority = 503)]
        public static void ExportToMvr()
        {
            CheckForLocalPanel();
            
            if (!_hasLocalPanel)
            {
                Debug.LogWarning($"{Prefix}: Please make sure there is a VRSL-LocalUIControlPanel in your scene before attempting to export."); 
                return;
            }
            
            if (_panel.fixtureSaveFile != "NONE")
            {
                try
                {
                    var asset = (VRSL_DMXPatchSettings) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_panel.fixtureSaveFile), typeof(VRSL_DMXPatchSettings));
                    asset.ToMVRFile();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{Prefix}: Failed to export patch data.");
                    Debug.LogException(e);
                    throw;
                }
            }
            else
            {
                Debug.LogError($"{Prefix}: Fixture Save File Not Found. The file may have been lost or has not been created yet.");
            }
        }

        [MenuItem("VRSL/Export/To PDF (Windows)", priority = 504)]
        public static void ExportToPdf()
        {
            CheckForLocalPanel();
            if (!_hasLocalPanel)
            {
                Debug.LogWarning($"{Prefix}: Please make sure there is a VRSL-LocalUIControlPanel in your scene before attempting to export."); 
                return;
            }
            
            if (_panel.fixtureSaveFile != "NONE")
            {
                try
                {
#if !UNITY_EDITOR_LINUX && !UNITY_ANDROID && !UNITY_IOS
                    var asset = (VRSL_DMXPatchSettings) AssetDatabase.LoadAssetAtPath(
                        AssetDatabase.GUIDToAssetPath(_panel.fixtureSaveFile), typeof(VRSL_DMXPatchSettings));
                    asset.ToPdf();
#else
                    EditorUtility.DisplayDialog("PDF export error", "PDF export is currently a Windows only feature", "OK", "Cancel");
#endif
                }
                catch (Exception e)
                {
                    Debug.LogError($"{Prefix}: Failed to export patch data.");
                    Debug.LogException(e);
                    throw;
                }
            }
            else
            {
                Debug.LogError($"{Prefix}: Fixture Save File Not Found. The file may have been lost or has not been created yet.");
            }
        }
    }
}
#endif
