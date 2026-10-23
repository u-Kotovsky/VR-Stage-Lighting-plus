#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    public class FixtureTextureBaker : EditorWindow
    {
        private static FixtureTextureBaker _window;
        private string _targetAssetPath = "Assets/VRSL_BakeTexture.asset";
        public VRStageLighting_DMX_Static[] selectedFixtures = Array.Empty<VRStageLighting_DMX_Static>();
        public bool replaceParameterTextureInCurrentScene = true;
        public bool overrideParameterTextureIfExists = true;
        public bool showPreviewTexture = false;
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

            parameterInfoFoldout = EditorGUILayout.Foldout(parameterInfoFoldout, "Parameter Texture Info");
            if (parameterInfoFoldout)
            {
                EditorGUI.indentLevel++;
                EditorStyles.label.wordWrap = true;
                EditorGUILayout.LabelField($"Parameter Map is required for properly mapping each parameter to it's coarse, fine, ultra and uber channels. That map is used by DMX Parameter shader to convert raw bytes into floats with specified precision. That way we can properly calculate smoothing and snapping. And it requires less code to read a single parameter.",
                    EditorStyles.label);
                EditorGUILayout.LabelField($"You can read it as:\nRed: Coarse Channel (Same index);\nGreen: Fine Channel (Index + 1);\nBlue: Ultra Channel (Index + 2);\nAlpha: Uber Channel (Index + 3) (unused for now)",
                    EditorStyles.label);
                EditorGUI.indentLevel--;
            }
            
            // TODO: choose what asset to override

            EditorGUILayout.BeginHorizontal();
            replaceParameterTextureInCurrentScene = EditorGUILayout.ToggleLeft("Replace Parameter Texture in Current Scene", replaceParameterTextureInCurrentScene);
            overrideParameterTextureIfExists = EditorGUILayout.ToggleLeft("Override Parameter Texture If Exists", overrideParameterTextureIfExists);
            EditorGUILayout.EndHorizontal();
            showPreviewTexture = EditorGUILayout.ToggleLeft("Show Preview Texture", showPreviewTexture);
            
            var so = new SerializedObject(this);

            if (selectedFixtures == null) selectedFixtures = Array.Empty<VRStageLighting_DMX_Static>();
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
            
            // todo: add option for asset selection/path
            // todo: add option for file save popup
            
            if (fixtures != null && GUILayout.Button("Bake"))
            {
                BakeTexture();
            }

            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VRSL_BakeTexture.asset");
            if (showPreviewTexture)
            {
                DrawPreview(position, asset);
            }
        }

        private void BakeTexture()
        {
            // Create texture
            var texture = new Texture2D(128, 128, TextureFormat.RGBAFloat, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            
            var pixels = new Color[texture.width * texture.height];
            pixels[0] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            
            // go through each fixture, grab their definition and channel start,
            // set pixel masks based on their parameter definitions
            foreach (var fixture in selectedFixtures)
            {
                if (fixture == null) continue;
                try
                {
                    var info = FixtureManager.GetFixtureInfoByGuid(fixture.fixtureInfoGuid);

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
                        
                        // Coords on 2D texture
                        var x = (int)((float)index / texture.width);
                        var y = (int)((float)index % texture.width);
                        if (y > texture.height) // Skip out of bounds.
                        {
                            Debug.LogError($"y is above texture width. x: {x}, y: {y}, {texture.width}x{texture.height}");
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
            GUILayout.Label("Preview", EditorStyles.boldLabel);
            
            const float pad = 12f;
            var px = position.width / 2f;
            var py = GUILayoutUtility.GetRect(128, 128).y;
            var size = MathF.Min(position.width, position.height - py) - pad;
            
            px -= size / 2f;
            
            EditorGUI.DrawPreviewTexture(new Rect(px, py, size,size), texture);
        }
    }
}
#endif