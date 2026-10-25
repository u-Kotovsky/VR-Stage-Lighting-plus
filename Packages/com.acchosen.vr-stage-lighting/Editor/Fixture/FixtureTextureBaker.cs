#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if VRSL_GPUREADBACK
using VRSL.GPUReadBack;
#endif

namespace VRSL.EditorScripts
{
    public class FixtureTextureBaker : EditorWindow
    {
        private static FixtureTextureBaker _window;
        private string _targetAssetPath = "Assets/VRSL_BakeTexture.asset";
        public VRStageLighting_DMX_Static[] selectedFixtures = Array.Empty<VRStageLighting_DMX_Static>();
        #if VRSL_GPUREADBACK
        public VRSL_ReadBackFunction[] selectedGpuReadBacks = Array.Empty<VRSL_ReadBackFunction>();
        #endif
        public bool replaceParameterTextureInCurrentScene = true;
        public bool overrideParameterTextureIfExists = true;
        public bool showPreviewTexture;
        public bool parameterInfoFoldout = true;
        
        [MenuItem("VRSL/Fixture Texture Baker")]
        private static void OpenWindow()
        {
            if (_window == null)
            {
                _window = CreateWindow<FixtureTextureBaker>();
            }
            
            _window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Fixture Parameter Map Baker", EditorStyles.boldLabel);

            parameterInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(parameterInfoFoldout, "Parameter Texture Info");
            if (parameterInfoFoldout)
            {
                EditorGUI.indentLevel++;
                EditorStyles.label.wordWrap = true;
                EditorGUILayout.LabelField("Parameter Map is required for properly mapping each parameter to it's coarse, fine, ultra and uber channels. That map is used by DMX Parameter shader to convert raw bytes into floats with specified precision. That way we can properly calculate smoothing and snapping. And it requires less code to read a single parameter.",
                    EditorStyles.label);
                EditorGUILayout.LabelField("You can read it as:\nRed: Coarse Channel (Same index);\nGreen: Fine Channel (Index + 1);\nBlue: Ultra Channel (Index + 2);\nAlpha: Uber Channel (Index + 3) (unused for now)",
                    EditorStyles.label);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            // TODO: choose what asset to override

            replaceParameterTextureInCurrentScene = EditorGUILayout.ToggleLeft("Replace Parameter Texture in Current Scene", replaceParameterTextureInCurrentScene);
            overrideParameterTextureIfExists = EditorGUILayout.ToggleLeft("Override Parameter Texture If Exists", overrideParameterTextureIfExists);
            
            var so = new SerializedObject(this);

            // If empty or slots are missing/null
            if (selectedFixtures == null || selectedFixtures.Length > 0 && selectedFixtures.All(x => x == null))
            {
                selectedFixtures = Array.Empty<VRStageLighting_DMX_Static>();
            }
            
            var fixtures = so.FindProperty(nameof(selectedFixtures));
            if (fixtures != null)
            {
                EditorGUILayout.PropertyField(fixtures);
                so.ApplyModifiedProperties();
            }
            
            if (GUILayout.Button("Get DMX Fixtures from scene"))
            {
                selectedFixtures = FindObjectsOfType<VRStageLighting_DMX_Static>().Where(x => x.enableDMXChannels).ToArray();
                so.Update();
            }
            
            #if VRSL_GPUREADBACK
            // If empty or slots are missing/null
            if (selectedGpuReadBacks == null || selectedGpuReadBacks.Length > 0 && selectedGpuReadBacks.All(x => x == null))
            {
                selectedGpuReadBacks = Array.Empty<VRSL_ReadBackFunction>();
            }

            var readbacks = so.FindProperty(nameof(selectedGpuReadBacks));
            if (readbacks != null)
            {
                EditorGUILayout.PropertyField(readbacks);
                so.ApplyModifiedProperties();
            }
            
            if (GUILayout.Button("Get DMX GPUReadBacks from scene"))
            {
                selectedGpuReadBacks = FindObjectsOfType<VRSL_ReadBackFunction>().ToArray();
                so.Update();
            }
            #endif
            
            // todo: add option for asset selection/path
            // todo: add option for file save popup
            
            if (fixtures != null && GUILayout.Button("Bake"))
            {
                BakeTexture();
            }

            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VRSL_BakeTexture.asset");
            showPreviewTexture = EditorGUILayout.BeginFoldoutHeaderGroup(showPreviewTexture, "Show Preview Texture");
            if (showPreviewTexture)
            {
                DrawPreview(position, asset);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void BakeTexture()
        {
            // Create texture
            var texture = new Texture2D(128, 128, TextureFormat.RGBAFloat, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            
            // go through each fixture, grab their definition and channel start,
            // set pixel masks based on their parameter definitions
            foreach (var fixture in selectedFixtures)
            {
                if (fixture == null) continue;
                try
                {
                    var info = FixtureManager.GetFixtureInfoByGuid(fixture.fixtureInfoGuid);

                    // Fixtures
                    for (var i = 0; i < info.Parameters.Length; i++)
                    {
                        // get previous channels
                        var previousChannelCount = 0;
                        for (var j = 0; j < i; j++)
                        {
                            var previousParameter = info.Parameters[j];
                            if (previousParameter.fine) previousChannelCount++;
                            if (previousParameter.ultra) previousChannelCount++;
                            if (previousParameter.uber) previousChannelCount++;
                        }
                        
                        var index = fixture.GlobalChannelIndex + i + previousChannelCount;
                        var parameter = info.Parameters[i];
                        
                        if (!GetTextureCoords(index, texture.width, texture.height, out var x, out var y))
                        {
                            continue;
                        }

                        float g = parameter.fine ? 1 : 0;
                        float b = parameter.ultra ? 1 : 0;
                        //float a = parameter.uber ? 1 : 0; // for now I don't think anyone will ever use it, and it is easier to read texture with alpha = 1
                        var color = new Color(1,g,b,1);
                        
                        //Debug.Log($"{index}; x:{x}, y:{y} col:{color}");
                        
                        texture.SetPixel(y, x, color);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Failed to process '{0}'", fixture.name);
                    Debug.LogException(e);
                    // soft ignore for now
                }
            }
        
            #if VRSL_GPUREADBACK
            // GPUReadBack functions
            Debug.Log($"{selectedGpuReadBacks.Length}");
            for (var index = 0; index < selectedGpuReadBacks.Length; index++)
            {
                var function = selectedGpuReadBacks[index];

                for (int i = 0; i < function.size; i++)
                {
                    var globalChannelIndex = GetGlobalChannel(function.dmxUnviverses[i], function.dmxChannels[i]);

                    if (!GetTextureCoords(globalChannelIndex, texture.width, texture.height, out var x, out var y))
                    {
                        continue;
                    }

                    // TODO: read channelLength
                    //float g = parameter.fine ? 1 : 0;
                    //float b = parameter.ultra ? 1 : 0;
                    //float a = parameter.uber ? 1 : 0; // for now I don't think anyone will ever use it, and it is easier to read texture with alpha = 1
                    //var color = new Color(1,g,b,1);
                    var color = new Color(1, 0, 0, 1);
                    
                    //Debug.Log($"{index}, {i}, {globalChannelIndex}; x:{x}, y:{y} col:{color}");
                    
                    texture.SetPixel(y, x, color);
                }
            }
            #endif
            
            texture.Apply();
            
            // Save asset
            // TODO: find a way to override asset content only (to keep same GUIDs so references won't break in other things)
            if (File.Exists(_targetAssetPath))
            {
                Debug.LogWarning($"Texture at path '{_targetAssetPath}' already exists, overwriting.");
                File.Delete(_targetAssetPath);
            }
            AssetDatabase.CreateAsset(texture, _targetAssetPath);
            AssetDatabase.SaveAssets();

            if (replaceParameterTextureInCurrentScene)
            {
                Debug.Log($"Replace parameter texture in current scene.");
                // Assign texture to current Control Panel in the scene
                var asset = AssetDatabase.LoadAssetAtPath(_targetAssetPath, typeof(Texture2D)) as Texture2D;
                var controlPanel = UnityEngine.Object.FindObjectOfType<VRSL_LocalUIControlPanel>();
                //controlPanel.dmxParametersMap = asset;
                var so2 = new SerializedObject(controlPanel);
                so2.FindProperty(nameof(controlPanel.dmxParametersMap)).objectReferenceValue = asset;
                so2.ApplyModifiedProperties();
            }

            // Save to PNG // Not doing that right now because it needs to have compression set to none
            // and it would require to use AssetImport extension which isn't quite what I need to do at the moment
            //SaveTextureAsPNG(texture, assetPath);
        }
        
        private int GetGlobalChannel(int universe, int channel)
        {
            return channel - 1 + ((universe - 1) * 512);
        }

        private bool GetTextureCoords(int index, int width, int height, out int x, out int y)
        {
            // Coords on 2D texture
            x = (int)((float)index / width);
            y = (int)((float)index % height);
            if (y > height) // Skip out of bounds.
            {
                Debug.LogError($"y is above texture width. x: {x}, y: {y}, {width}x{height}, index: {index}");
                return false;
            }

            return true;
        }

        private void SaveTextureAsPNG(Texture2D texture, string path)
        {
            var bytes = texture.EncodeToPNG();
            if (File.Exists(path))
            {
                Debug.LogWarning("Texture at path " + path + " already exists, overwriting.");
                File.Delete(path);
            }
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            Debug.Log("Texture created, saved and imported: " + texture.name + " vs orig: " +texture.format);
        }

        private static void DrawPreview(Rect position, Texture2D texture)
        {
            const float pad = 12f;
            var px = position.width / 2f;
            var py = GUILayoutUtility.GetRect(128, 128).y;
            var size = MathF.Max(0, MathF.Min(position.width, position.height - py) - pad);
            
            px -= size / 2f;
            
            EditorGUI.DrawPreviewTexture(new Rect(px, py, size,size), texture);
        }
    }
}
#endif