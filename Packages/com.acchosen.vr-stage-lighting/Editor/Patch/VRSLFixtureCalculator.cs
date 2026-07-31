#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRSL.EditorScripts
{
    public class VRSLFixtureCalculator : EditorWindow
    {
        private static int _fixtureCount;
        private static VRSL_LocalUIControlPanel _controlPanel;
        private static VRSL_FixtureDefinitions _fixtureDefinitions;
        private static List<string> _uniqueFixtureDefinitions = new();
        
        private static VRSLFixtureCalculator _window;

        public VRStageLighting_DMX_Static[] selectedFixtures = Array.Empty<VRStageLighting_DMX_Static>();
        
        [MenuItem("VRSL/Fixture Calculator")]
        private static void OpenWindow()
        {
            if (_window == null)
            {
                _window = CreateWindow<VRSLFixtureCalculator>();
            }
            
            _window.Show();
        }

        private string GetNextAvailableChannel()
        {
            int nextFreeChannel = NextFreeChannel();
            int universe = nextFreeChannel/512;
            int localChannel = nextFreeChannel%512;
            StringBuilder sb = new();
            sb.Append(universe);
            sb.Append(".");
            sb.Append(localChannel);
            sb.Append(" (");
            sb.Append(nextFreeChannel);
            sb.Append(")");
            return sb.ToString();
        }

        private bool overrideStartChannel = false;
        private int overrideStartChannelIndex = 0;
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Yip Yap VRSL Fixture Calculator");
            EditorGUILayout.LabelField("Fixture count: " + _fixtureCount.ToString());
            EditorGUILayout.LabelField("Next free channel: " + GetNextAvailableChannel());
            EditorGUILayout.LabelField("Fixture types used: " + string.Join(", ", _uniqueFixtureDefinitions.ToArray()));
            
            EditorGUILayout.Space();
            
            var so = new SerializedObject(this);
            var fixtures = so.FindProperty(nameof(selectedFixtures));
            if (fixtures != null)
            {
                EditorGUILayout.PropertyField(fixtures);
                so.ApplyModifiedProperties();

                if (selectedFixtures != null && selectedFixtures.Length > 0)
                {
                    overrideStartChannel = EditorGUILayout.Toggle("Override Start Channel", overrideStartChannel);
                    GUI.enabled = overrideStartChannel;
                    overrideStartChannelIndex = EditorGUILayout.IntField("Override Start Channel", overrideStartChannelIndex);
                    GUI.enabled = true;
                    if (GUILayout.Button("Auto assign ids"))
                    {
                        if (overrideStartChannel)
                        {
                            AutoAssignChannels(selectedFixtures, overrideStartChannelIndex);
                        }
                        else
                        {
                            AutoAssignChannels(selectedFixtures, NextFreeChannel());
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("There are no selected fixtures.");
                }
                so.ApplyModifiedProperties();
            }
            else
            {
                EditorGUILayout.LabelField("Selected fixtures are null.");
            }
        }

        public static void AutoAssignChannels(VRStageLighting_DMX_Static[] selectedFixtures, int startIndex = 0)
        {
            int lastFreeId = startIndex;
            
            for (var i = 0; i < selectedFixtures.Length; i++)
            {
                var fixture = selectedFixtures[i];
                if (fixture == null)
                {
                    continue;
                }
                
                var so2 = new SerializedObject(fixture);
                var def = _fixtureDefinitions.definitions[fixture.fixtureDefintion];
                var universe = so2.FindProperty(nameof(fixture.dmxUniverse));
                var channel = so2.FindProperty(nameof(fixture.dmxChannel));
                var globalChannel = so2.FindProperty(nameof(fixture.globalChannelIndex));
                
                fixture.GetUniverseAndChannel(lastFreeId, out int uni, out int ch);

                if (ch + def.channelNames.Length >= 512)
                {
                    // Some LD programs do not like bounds.
                    Debug.Log("Fix bounds at " + i);
                    uni++;
                    ch = 1;
                    lastFreeId = fixture.GetGlobalChannel(uni, ch);
                }
                
                universe.intValue = uni;
                channel.intValue = ch;
                globalChannel.intValue = lastFreeId;
                so2.ApplyModifiedProperties();
                so2.Dispose();
                            
                //fixture.GlobalChannelIndex = lastFreeId;
                //fixture.SetLocalChannelFromGlobal(lastFreeId);

                var channelCount = GetChannelCount(fixture);
                lastFreeId += channelCount;
                Debug.Log($"Assigned {fixture.GlobalChannelIndex}, ch count: {channelCount}, last free id: {lastFreeId}");
            }
        }

        public static int GetChannelCount(VRStageLighting_DMX_Static fixture)
        {
            if (_controlPanel == null) _controlPanel = FindObjectOfType<VRSL_LocalUIControlPanel>();
            if (_controlPanel == null)
            {
                throw new Exception("Fail, control panel is null");
            }
            
            if (_fixtureDefinitions == null) _fixtureDefinitions = VRStageLighting_DMX_Static_Editor.GetFixtureOptions(_controlPanel.fixtureDefGUID);
            if (_fixtureDefinitions == null)
            {
                throw new Exception("Fail, fixture def is null");
            }
            
            var definition = _fixtureDefinitions.definitions[fixture.fixtureDefintion];
            return definition.channelNames.Length;
        }

        public static void SetNextFreeChannel(VRStageLighting_DMX_Static fixtureToFit)
        {
            if (_controlPanel == null) _controlPanel = FindObjectOfType<VRSL_LocalUIControlPanel>();
            if (_controlPanel == null)
            {
                throw new Exception("Fail, control panel is null");
            }
            
            if (_fixtureDefinitions == null) _fixtureDefinitions = VRStageLighting_DMX_Static_Editor.GetFixtureOptions(_controlPanel.fixtureDefGUID);
            if (_fixtureDefinitions == null)
            {
                throw new Exception("Fail, fixture def is null");
            }
            
            var definition = _fixtureDefinitions.definitions[fixtureToFit.fixtureDefintion];
            var channel = NextFreeChannel(definition.channelNames.Length);
            fixtureToFit.GlobalChannelIndex = channel;
        }

        public static int NextFreeChannel(VRStageLighting_DMX_Static fixtureToFit)
        {
            if (_controlPanel == null) _controlPanel = FindObjectOfType<VRSL_LocalUIControlPanel>();
            if (_controlPanel == null)
            {
                throw new Exception("Fail, control panel is null");
            }
            
            if (_fixtureDefinitions == null) _fixtureDefinitions = VRStageLighting_DMX_Static_Editor.GetFixtureOptions(_controlPanel.fixtureDefGUID);
            if (_fixtureDefinitions == null)
            {
                throw new Exception("Fail, fixture def is null");
            }
            
            var definition = _fixtureDefinitions.definitions[fixtureToFit.fixtureDefintion];
            return NextFreeChannel(definition.channelNames.Length);
        }

        public static int NextFreeChannel(int width = 0)
        {
            if (_controlPanel == null) _controlPanel = FindObjectOfType<VRSL_LocalUIControlPanel>();
            if (_controlPanel == null)
            {
                _fixtureCount = 0;
                return 0;
            }
            
            var fixtures = FindObjectsByType<VRStageLighting_DMX_Static>(FindObjectsSortMode.None);
            if (fixtures == null)
            {
                _fixtureCount = 0;
                return 0;
            }

            _fixtureCount = fixtures.Length;
            _fixtureDefinitions = VRStageLighting_DMX_Static_Editor.GetFixtureOptions(_controlPanel.fixtureDefGUID);

            var busyChannels = new List<(int, int)>();
            var startOfFreeChannelSpace = 0;
            
            _uniqueFixtureDefinitions.Clear();
            
            foreach (var fixture in fixtures)
            {
                var definition = _fixtureDefinitions.definitions[fixture.fixtureDefintion];

                if (!_uniqueFixtureDefinitions.Contains(definition.name))
                {
                    _uniqueFixtureDefinitions.Add(definition.name);
                }

                var globalChannel = fixture.GlobalChannelIndex;
                var channelCount = definition.channelNames.Length;
                var endChannel = globalChannel + channelCount;
                
                busyChannels.Add((globalChannel, channelCount));

                if (endChannel > startOfFreeChannelSpace)
                {
                    startOfFreeChannelSpace = endChannel;
                }
            }
            
            busyChannels.Sort();
            
            var result = startOfFreeChannelSpace;
            
            for (var i = 0; i < busyChannels.Count; i++)
            {
                var channel = busyChannels[i];
                var start = channel.Item1 + channel.Item2;

                if (i < busyChannels.Count - 1)
                {
                    var nextChannel = busyChannels[i + 1];
                    var nextStart = nextChannel.Item1;

                    if (nextStart - start >= width)
                    {
                        result = start;
                    }
                }
            }
            
            return result;
        }
    }
}
#endif