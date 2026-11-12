#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRSL.EditorScripts
{
    public class VRSLFixtureCalculator : EditorWindow
    {
        private const string Prefix = "VRSL Fixture Calculator";
        private static int _fixtureCount;
        private static VRSL_LocalUIControlPanel _controlPanel;
        private static readonly List<string> UniqueFixtureDefinitions = new();
        
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

        private bool _overrideStartChannel;
        private int _overrideStartChannelIndex;
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("VRSL Fixture Calculator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Fixture count: " + _fixtureCount.ToString());
            EditorGUILayout.LabelField("Next free channel: " + GetNextAvailableChannel());
            EditorGUILayout.LabelField("Fixture types used: " + string.Join(", ", UniqueFixtureDefinitions.ToArray()));
            
            EditorGUILayout.Space();
            
            var so = new SerializedObject(this);
            var fixtures = so.FindProperty(nameof(selectedFixtures));
            if (fixtures != null)
            {
                EditorGUILayout.PropertyField(fixtures);
                so.ApplyModifiedProperties();

                if (selectedFixtures != null && selectedFixtures.Length > 0)
                {
                    _overrideStartChannel = EditorGUILayout.Toggle("Override Start Channel", _overrideStartChannel);
                    GUI.enabled = _overrideStartChannel;
                    _overrideStartChannelIndex = EditorGUILayout.IntField("Override Start Channel", _overrideStartChannelIndex);
                    GUI.enabled = true;
                    if (GUILayout.Button("Auto assign ids"))
                    {
                        if (_overrideStartChannel)
                        {
                            AutoAssignChannels(selectedFixtures, _overrideStartChannelIndex);
                        }
                        else
                        {
                            AutoAssignChannels(selectedFixtures, NextFreeChannel());
                        }
                    }

                    if (GUILayout.Button("Reverse selection"))
                    {
                        selectedFixtures = selectedFixtures.Reverse().ToArray();
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
                if (fixture == null) continue;
                
                var channelWidth = FixtureManager.GetFixtureInfoByGuid(fixture.fixtureInfoGuid).GetChannelWidth();
                
                var so2 = new SerializedObject(fixture);
                var universe = so2.FindProperty(nameof(fixture.dmxUniverse));
                var channel = so2.FindProperty(nameof(fixture.dmxChannel));
                var globalChannel = so2.FindProperty(nameof(fixture.globalChannelIndex));
                
                fixture.GetUniverseAndChannel(lastFreeId, out int uni, out int ch);

                if (ch + channelWidth >= 512)
                {
                    // Some LD programs do not like universe bounds so we have to put fixture on "new universe" instead.
                    // TODO: add an option to ignore that
                    Debug.LogWarning($"{Prefix}: Fix universe bounds at " + i);
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

                lastFreeId += channelWidth;
                Debug.Log($"{Prefix} Assigned '{fixture.GlobalChannelIndex}', channel count: '{channelWidth}', last free id: '{lastFreeId}'");
            }
        }

        public static void SetNextFreeChannel(VRStageLighting_DMX_Static fixtureToFit)
        {
            // TODO: use serialized object for this
            var fixtureInfo = FixtureManager.GetFixtureInfoByGuid(fixtureToFit.fixtureInfoGuid);
            var channel = NextFreeChannel(fixtureInfo.GetChannelWidth());
            fixtureToFit.GlobalChannelIndex = channel;
        }

        public static int NextFreeChannel(VRStageLighting_DMX_Static fixtureToFit)
        {
            var fixtureInfo = FixtureManager.GetFixtureInfoByGuid(fixtureToFit.fixtureInfoGuid);
            return NextFreeChannel(fixtureInfo.GetChannelWidth());
        }

        public static int NextFreeChannel(int width = 0) // width - the fixture to fit in
        {
            var fixtures = FindObjectsByType<VRStageLighting_DMX_Static>(FindObjectsSortMode.None);
            if (fixtures == null)
            {
                _fixtureCount = 0;
                return 0;
            }

            _fixtureCount = fixtures.Length;

            var fixtureInfos = FixtureManager.GetInstance().Fixtures;

            var busyChannels = new List<(int, int)>();
            var startOfFreeChannelSpace = 0;
            
            UniqueFixtureDefinitions.Clear();
            
            foreach (var fixture in fixtures)
            {
                var fixtureInfo = fixtureInfos[FixtureManager.GetFixtureIndexByGuid(fixture.fixtureInfoGuid)];

                if (!UniqueFixtureDefinitions.Contains(fixtureInfo.GetGuid()))
                {
                    UniqueFixtureDefinitions.Add(fixtureInfo.GetGuid());
                }

                var globalChannel = fixture.GlobalChannelIndex;
                var channelCount = fixtureInfo.GetChannelWidth();
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
                var channel = busyChannels[i]; // global channel index, channel count
                var start = channel.Item1 + channel.Item2;
                
                result = start;
            }
            
            return result;
        }
    }
}
#endif