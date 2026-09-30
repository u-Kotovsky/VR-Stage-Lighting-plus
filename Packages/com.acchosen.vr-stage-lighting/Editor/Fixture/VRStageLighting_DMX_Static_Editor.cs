#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [CustomEditor(typeof(VRStageLighting_DMX_Static))]
    [CanEditMultipleObjects]
    public class VRStageLighting_DMX_Static_Editor : VRSL_UdonEditor
    {
        private GUIStyle _titleStyle;
        private GUIContent _colorLabel;
        private VRSL_LocalUIControlPanel _panel;

        private string[] _fixDefinitionNames = new string[1];
        
        public static GUIStyle InfoLabel()
        {
            GUIStyle g = new GUIStyle();
            g.fontSize = 13;
            g.fontStyle = FontStyle.Italic;
            g.normal.textColor = Color.white;
            return g;
        }

        public static GUIStyle SectionLabel()
        {
            GUIStyle g = new GUIStyle();
            g.fontSize = 15;
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            return g;
        }

        public new void OnEnable()
        {
            base.OnEnable();
            _titleStyle = SectionLabel();
            _colorLabel = new GUIContent();
            _colorLabel.text = "Emission Color";
            EditorApplication.hierarchyChanged += HierarchyChanged;
            SceneView.duringSceneGui += OnSceneGUI;
            GetPanel();
        }

        private void OnSceneGUI(SceneView sceneView) { }
        
        public static VRSL_FixtureDefinitions GetFixtureOptions(string fixtureDefGuid)
        {
            var fixDefAsset = (VRSL_FixtureDefinitions)AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(fixtureDefGuid), typeof(VRSL_FixtureDefinitions));
            return fixDefAsset;
        }

        public static string[] GetFixtureOptionNames(string fixtureDefGuid)
        {
            return GetFixtureOptions(fixtureDefGuid).GetNames();
        }

        public void GetPanel()
        {
            var component = FindObjectOfType<VRSL_LocalUIControlPanel>();

            _panel = component;
            
            if (_panel != null)
            {
                _fixDefinitionNames = GetFixtureOptionNames(_panel.fixtureDefGUID);
            }
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= HierarchyChanged;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void HierarchyChanged()
        {
            var fixture = (VRStageLighting_DMX_Static)target;
            UpdateSettings(fixture);
        }

        private void UpdateSettings(VRStageLighting_DMX_Static fixture)
        {
            if (fixture == null)
            {
                Debug.LogError($"fixture is null");
                return;
            }
            
            if (fixture.objRenderers == null || fixture.objRenderers.Length == 0)
            {
                Debug.LogError($"There are no object renderers in fixture {fixture.gameObject.name}");
                return;
            }
            
            var isEmpty = fixture.objRenderers.Any(x => x == null);

            if (!isEmpty)
            {
                fixture._SetProps();
                
                if (Application.isPlaying)
                {
                    fixture._UpdateInstancedProperties();
                }
                else
                {
                    fixture._UpdateInstancedPropertiesSansDMX();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
#if UDONSHARP
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
#endif
            //DrawLogo();
            ShurikenHeaderCentered(GetVersion());
            
            //EditorGUILayout.Space();
            
            var fixture = (VRStageLighting_DMX_Static)target;
            EditorGUI.BeginChangeCheck();

            #region DMX Settings
            GUILayout.Label("DMX Settings", _titleStyle);
            
            serializedObject.FindProperty("enableDMXChannels").boolValue = EditorGUILayout.Toggle(new GUIContent(
                "Enable DMX", "The industry standard DMX Channel this fixture begins on. Most standard VRSL fixtures are 13 channels"), fixture.enableDMXChannels);
            
            if (serializedObject.FindProperty("enableDMXChannels").boolValue && _panel != null)
            {
                EditorGUI.indentLevel++;

                #region Fixture Info
                var fixtureInfoGuidProperty = serializedObject.FindProperty(nameof(VRStageLighting_DMX_Static.fixtureInfoGuid));
                var fixtureInfos = FixtureManager.instance.Fixtures;
                
                bool validFixtureInfo = true;
                if (fixtureInfoGuidProperty.stringValue.Equals(string.Empty) &&  fixtureInfos != null && fixtureInfos.Count > 0)
                {
                    Debug.LogError($"Fixture '{fixture.name}' had an empty GUID '{fixtureInfoGuidProperty.stringValue}'. Sets to default first available!");
                    fixtureInfoGuidProperty.stringValue = fixtureInfos[0].GetGuid();
                    serializedObject.ApplyModifiedProperties();
                }
                else if (fixtureInfos == null || fixtureInfos.Count == 0)
                {
                    Debug.LogError($"Fixture '{fixture.name}' had an empty GUID. No available fixture infos are available!");
                    validFixtureInfo = false;
                }

                if (validFixtureInfo)
                {
                    var fixtureInfoIndex = FixtureManager.GetFixtureIndexByGuid(fixtureInfoGuidProperty.stringValue);
                    var fixtureInfoNames = FixtureManager.GetAllFixtureInfoNames().ToArray();
                    var newIndex = EditorGUILayout.Popup("Fixture Type (new)", fixtureInfoIndex, fixtureInfoNames);
                    var newGuid = fixtureInfos[newIndex].GetGuid();
                    
                    if (!fixtureInfoGuidProperty.stringValue.Equals(newGuid))
                    {
                        Debug.Log($"Fixture Info GUids don't match, updating fixture! '{fixtureInfoGuidProperty.stringValue}' -> '{newGuid}'");
                        fixtureInfoGuidProperty.stringValue = newGuid;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    GUILayout.Label("Fixture Type (new) is not available at this moment.");
                }
                #endregion
                
                serializedObject.FindProperty("fixtureDefintion").intValue = EditorGUILayout.Popup(
                    "Fixture Type", serializedObject.FindProperty("fixtureDefintion").intValue, _fixDefinitionNames);
                serializedObject.FindProperty("fixtureID").intValue = EditorGUILayout.IntField(new GUIContent(
                    "Fixture ID", "The ID number for this fixture. This is mostly for organizational purposes and is entirely optional. Most DMX software have an ID attached to each fixture to run the fixtures through commands more easily, and it is recommended to have those IDs lined up here as well for the sake simplicity. This ID is public and can also be used for Udon scripting as well."), fixture.fixtureID);
                serializedObject.FindProperty("dmxChannel").intValue = EditorGUILayout.IntSlider(new GUIContent(
                    "DMX Channel", "The industry standard DMX Channel this fixture begins on. Most standard VRSL fixtures are 13 channels"), fixture.dmxChannel, 1, 512);
                serializedObject.FindProperty("dmxUniverse").intValue = EditorGUILayout.IntSlider(new GUIContent(
                    "Universe", "The industry standard Artnet Universe. Use this to choose which universe to read the DMX Channel from."), fixture.dmxUniverse, 1, VRSL_LocalUIControlPanel.MaxUniverseCount);
                    
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = false;
                EditorGUILayout.IntField("Next available channel", VRSLFixtureCalculator.NextFreeChannel(fixture));
                GUI.enabled = true;
                
                if (GUILayout.Button("Set", GUILayout.Width(40)))
                {
                    VRSLFixtureCalculator.SetNextFreeChannel(fixture);
                }
                EditorGUILayout.EndHorizontal();
                
                GUI.enabled = false;
                EditorGUILayout.IntField("Current channel", fixture.GlobalChannelIndex);
                GUI.enabled = true;
                
                var enableDmxTranslate = serializedObject.FindProperty(nameof(VRStageLighting_DMX_Static.enableDmxTranslate));
                enableDmxTranslate.boolValue = EditorGUILayout.Toggle(new GUIContent("DMX Translate", "Enable XYZ positioning through DMX512"), fixture.enableDmxTranslate);
                if (enableDmxTranslate.boolValue)
                {
                    EditorGUI.indentLevel++;
                    serializedObject.FindProperty(nameof(VRStageLighting_DMX_Static.dmxTranslateChannel)).intValue = EditorGUILayout.IntSlider(new GUIContent(
                            "DMX translate channel", "Starting channel for XYZ positioning through DMX512"), fixture.dmxTranslateChannel, 0, VRSL_LocalUIControlPanel.MaxUniverseCount * 512);
                    EditorGUI.indentLevel--;
                }
                
                serializedObject.FindProperty("legacyGoboRange").boolValue = EditorGUILayout.Toggle(new GUIContent(
                                    "Legacy Gobo Range", "Use Only the first 6 gobos instead of all. This is for legacy content where only 6 gobos were originally supported and the channel range was different."), fixture.legacyGoboRange);
                            
                EditorGUI.indentLevel--;
            }
            
            #endregion
            
            EditorGUILayout.Space();
            
            #region General Settings
            GUILayout.Label("General Settings", _titleStyle);
            serializedObject.FindProperty("globalIntensity").floatValue = EditorGUILayout.Slider(new GUIContent(
                    "Global Intensity", "Sets the overall intensity of the shader. Good for animating or scripting effects related to intensity. Its max value is controlled by Final Intensity."),
                fixture.globalIntensity, 0.0f, 1.0f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("finalIntensityComponentMode"), new GUIContent("Control Component Intensities"));
            
            EditorGUI.indentLevel++;
            if (serializedObject.FindProperty("finalIntensityComponentMode").boolValue)
            {
                serializedObject.FindProperty("finalIntensityVolumetric").floatValue = EditorGUILayout.Slider(new GUIContent(
                        "Volumetric Intensity", "Sets the maximum brightness value of Global Intensity for volumetric meshes only. Good for personalized settings of the max brightness of the shader by other users via UI."),
                    fixture.finalIntensityVolumetric, 0.0f, 1.0f);

                serializedObject.FindProperty("finalIntensityProjection").floatValue = EditorGUILayout.Slider(new GUIContent(
                        "Projection Intensity", "Sets the maximum brightness value of Global Intensity for projection meshes only. Good for personalized settings of the max brightness of the shader by other users via UI."),
                    fixture.finalIntensityProjection, 0.0f, 1.0f);

                serializedObject.FindProperty("finalIntensityFixture").floatValue = EditorGUILayout.Slider(new GUIContent(
                        "Fixture/Other Intensity", "Sets the maximum brightness value of Global Intensity for everything else. Good for personalized settings of the max brightness of the shader by other users via UI."),
                    fixture.finalIntensityFixture, 0.0f, 1.0f);
            }
            else
            {
                serializedObject.FindProperty("finalIntensity").floatValue = EditorGUILayout.Slider(new GUIContent(
                        "Final Intensity", "Sets the maximum brightness value of Global Intensity. Good for personalized settings of the max brightness of the shader by other users via UI."),
                    fixture.finalIntensity, 0.0f, 1.0f);
            }
            EditorGUI.indentLevel--;
            
            serializedObject.FindProperty("lightColorTint").colorValue = EditorGUILayout.ColorField(_colorLabel, fixture.lightColorTint, true, true, true);
            #endregion
            
            EditorGUILayout.Space();
            
            #region Movement settings
            GUILayout.Label("Movement Settings", _titleStyle);
            serializedObject.FindProperty("invertPan").boolValue = EditorGUILayout.Toggle(new GUIContent(
                "Invert Pan", "Invert the tilt values (Up/Down Movement) for movers."), fixture.invertPan);
            serializedObject.FindProperty("invertTilt").boolValue = EditorGUILayout.Toggle(new GUIContent(
                "Invert Tilt", "Enable this if the mover is hanging upside down."), fixture.invertTilt);
            serializedObject.FindProperty("isUpsideDown").boolValue = EditorGUILayout.Toggle(new GUIContent(
                "Is Upside Down?", "Enable projection spinning (Udon Override Only)."), fixture.isUpsideDown);
            serializedObject.FindProperty("maxMinPan").floatValue = EditorGUILayout.FloatField(new GUIContent(
                "Max/Min Pan Range", "Control the range of rotation for the pan channel of the fixture"), fixture.maxMinPan);
            serializedObject.FindProperty("maxMinTilt").floatValue = EditorGUILayout.FloatField(new GUIContent(
                "Max/Min Tilt Range", "Control the range of rotation for the tilt channel of the fixture"), fixture.maxMinTilt);
            #endregion
            
            EditorGUILayout.Space();
            
            #region Fixture settings
            GUILayout.Label("Fixture Settings", _titleStyle);
            serializedObject.FindProperty("enableAutoSpin").boolValue = EditorGUILayout.Toggle(new GUIContent(
                "Enable Projection Spin",
                "Enable projection spinning (Udon Override Only)."), fixture.enableAutoSpin);
            serializedObject.FindProperty("enableStrobe").boolValue = EditorGUILayout.Toggle(new GUIContent(
                "Enable Strobe Functionality",
                "Enable strobe effects (via DMX Only)."), fixture.enableStrobe);
            serializedObject.FindProperty("tiltOffsetBlue").floatValue = EditorGUILayout.Slider(new GUIContent(
                    "Tilt Offset",
                    "Tilt (Up/Down) offset/movement. Directly controls tilt when in Udon Mode; is an offset when in DMX mode."),
                fixture.tiltOffsetBlue, 0.0f, 360.0f);
            serializedObject.FindProperty("panOffsetBlueGreen").floatValue = EditorGUILayout.Slider(new GUIContent(
                    "Pan Offset",
                    "Pan (Left/Right) offset/movement. Directly controls pan when in Udon Mode; is an offset when in DMX mode."),
                fixture.panOffsetBlueGreen, 0.0f, 360.0f);
            serializedObject.FindProperty("selectGOBO").intValue = EditorGUILayout.IntSlider(new GUIContent(
                    "Projection GOBO Selection",
                    "The meshes used to make up the light. You need atleast 1 mesh in this group for the script to work properly."),
                fixture.selectGOBO, 1, 8);
            #endregion
            
            EditorGUILayout.Space();
            
            #region Mesh settings
            GUILayout.Label("Mesh Settings", _titleStyle);
            
            serializedObject.FindProperty("coneWidth").floatValue = EditorGUILayout.Slider(new GUIContent(
                "Fixture Cone Width", "Controls the radius of a mover/spot light."), fixture.coneWidth, 0, 5.5f);
            serializedObject.FindProperty("coneLength").floatValue = EditorGUILayout.Slider(new GUIContent(
                "Fixture Cone Length", "Controls the length of the cone of a mover/spot light."), fixture.coneLength, .5f, 10.0f);
            serializedObject.FindProperty("maxConeLength").floatValue = EditorGUILayout.Slider("Max Cone Length", fixture.maxConeLength, .275f, 10.0f);

            var objRenderers = serializedObject.FindProperty("objRenderers");
            EditorGUILayout.PropertyField(objRenderers, true);

            if (GUILayout.Button("Get all children renderers"))
            {
                var renderers = fixture.gameObject.GetComponentsInChildren<MeshRenderer>();
                fixture.objRenderers = renderers;
            }
            
            #endregion
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                foreach (var obj in targets)
                {
                    var fixture1 = (VRStageLighting_DMX_Static)obj;
                    UpdateSettings(fixture1);
                }
            }
        }
    }
}

#endif