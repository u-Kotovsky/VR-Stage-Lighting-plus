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
        public VRStageLighting_DMX_Static[] selectedFixtures = Array.Empty<VRStageLighting_DMX_Static>();
        private string _targetAssetPath = "Assets/VRSL_BakeTexture.asset";
        
        [MenuItem("VRSL/Fixture Texture Baker")]
        private static void OpenWindow()
        {
            if (_window == null)
            {
                _window = CreateWindow<FixtureTextureBaker>();
            }
            
            _window.Show();
        }

        //private float v1;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Fixture Parameter Map Baker", EditorStyles.boldLabel);

            // TODO: choose what asset to override
            // TODO: add preview in the GUI
            
            var so = new SerializedObject(this);
            var fixtures = so.FindProperty(nameof(selectedFixtures));
            if (fixtures != null)
            {
                EditorGUILayout.PropertyField(fixtures);
                so.ApplyModifiedProperties();

                if (GUILayout.Button("Bake"))
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
                                
                                var x = (int)((float)index / texture.width);
                                var y = (int)((float)index % texture.width);
                                if (y > texture.height)
                                {
                                    Debug.LogError($"y is above texture width. x: {x}, y: {y}, {texture.width}x{texture.height}");
                                    continue;
                                }

                                float g = parameter.fine ? 1 : 0;
                                float b = parameter.ultra ? 1 : 0;
                                float a = parameter.uber ? 1 : 0;
                                var color = new Color(1,g,b,1);
                                
                                Debug.Log($"{index}; x:{x}, y:{y} col:{color}");
                                
                                texture.SetPixel(y, x, color);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogErrorFormat("Failed to process '{0}' because: {1}", fixture.name, e);
                            // soft ignore for now
                        }
                    }
                    
                    // asset save
                    if (File.Exists(_targetAssetPath))
                    {
                        Debug.LogWarning($"Texture at path '{_targetAssetPath}' already exists, overwriting.");
                        File.Delete(_targetAssetPath);
                    }
                    AssetDatabase.CreateAsset(texture, _targetAssetPath);
                    AssetDatabase.SaveAssets();
                    
                    // Save to PNG
                    //SaveTextureAsPNG(texture, assetPath);
                }
            }

            if (GUILayout.Button("Get Fixtures from scene"))
            {
                selectedFixtures = FindObjectsOfType<VRStageLighting_DMX_Static>().Where(x => x.enableDMXChannels).ToArray();
                so.Update();
            }

            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VRSL_BakeTexture.asset");
            DrawPreview(asset);
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

        private void DrawPreview(Texture2D texture)
        {
            // todo: fix preview fit in the window math
            // todo: add proper description and info why and how this is useful
            // todo: add option for asset selection/path
            // todo: add option for file save popup
            GUILayout.Label("Preview", EditorStyles.boldLabel);
            
            //v1 = EditorGUILayout.Slider("v1", v1, 0, 256);
            
            const float pad = 12f;
            
            var px = position.width / 2f;
            const float py = 100f;
            
            var pxw = position.width - pad;
            var pxh = pxw;// (position.height - py) - pad;
            
            px -= pxw / 2f;
            
            //pxw = Mathf.Max(pxw, pxh);
            
            EditorGUI.DrawPreviewTexture(new Rect(px, py, pxw, pxh), texture);
        }
    }
}
#endif