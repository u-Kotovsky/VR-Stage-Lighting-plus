#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    public class DMXListItem
    {
        public VRStageLighting_DMX_Static light;

        public bool foldout;

        ///////////////////////////////////////////////////////////////////////
        private bool Z_enableDMXChannels;
        public bool P_enableDMXChannels;
        private int Z_fixtureID;
        public int P_fixtureID;
        private int Z_dmxChannel;
        public int P_dmxChannel;
        private int Z_dmxUniverse;
        public int P_dmxUniverse;
        private int Z_Channel;
        public int P_Channel;
        private bool Z_legacyGoboRange;
        public bool P_legacyGoboRange;
        private float Z_globalIntensity;
        public float P_globalIntensity;
        private float Z_finalIntensity;
        public float P_finalIntensity;
        private Color Z_lightColorTint;
        public Color P_lightColorTint;
        private bool Z_invertPan;
        public bool P_invertPan;
        private bool Z_invertTilt;
        public bool P_invertTilt;
        private bool Z_isUpsideDown;
        public bool P_isUpsideDown;
        private bool Z_enableAutoSpin;
        public bool P_enableAutoSpin;
        private bool Z_enableStrobe;
        public bool P_enableStrobe;
        private float Z_tiltOffsetBlue;
        public float P_tiltOffsetBlue;
        private float Z_panOffsetBlueGreen;
        public float P_panOffsetBlueGreen;
        private int Z_selectGOBO;
        public int P_selectGOBO;
        private float Z_coneWidth;
        public float P_coneWidth;
        private float Z_coneLength;
        public float P_coneLength;
        private float Z_maxConeLength;
        public float P_maxConeLength;
        private float Z_maxMinPan;
        public float P_maxMinPan;
        private float Z_maxMinTilt;
        public float P_maxMinTilt;
        //////////////////////////////////////////////////////////////


        public DMXListItem(VRStageLighting_DMX_Static light, bool foldout)
        {
            this.light = light;
            this.foldout = this.light.foldout = foldout;
            Z_enableDMXChannels = P_enableDMXChannels = this.light.enableDMXChannels;
            Z_fixtureID = P_fixtureID = this.light.fixtureID;
            Z_dmxChannel = P_dmxChannel = this.light.dmxChannel;
            Z_dmxUniverse = P_dmxUniverse = this.light.dmxUniverse;
            Z_Channel = P_Channel = this.light.Channel;
            Z_legacyGoboRange = P_legacyGoboRange = this.light.legacyGoboRange;
            Z_globalIntensity = P_globalIntensity = this.light.globalIntensity;
            Z_finalIntensity = P_finalIntensity = this.light.finalIntensity;
            Z_lightColorTint = P_lightColorTint = this.light.lightColorTint;
            Z_invertPan = P_invertPan = this.light.invertPan;
            Z_invertTilt = P_invertTilt = this.light.invertTilt;
            Z_isUpsideDown = P_isUpsideDown = this.light.isUpsideDown;
            Z_enableAutoSpin = P_enableAutoSpin = this.light.enableAutoSpin;
            Z_enableStrobe = P_enableStrobe = this.light.enableStrobe;
            Z_tiltOffsetBlue = P_tiltOffsetBlue = this.light.tiltOffsetBlue;
            Z_panOffsetBlueGreen = P_panOffsetBlueGreen = this.light.panOffsetBlueGreen;
            Z_selectGOBO = P_selectGOBO = this.light.selectGOBO;
            Z_coneWidth = P_coneWidth = this.light.coneWidth;
            Z_coneLength = P_coneLength = this.light.coneLength;
            Z_maxConeLength = P_maxConeLength = this.light.maxConeLength;
            Z_maxMinPan = P_maxMinPan = this.light.maxMinPan;
            Z_maxMinTilt = P_maxMinTilt = this.light.maxMinTilt;
        }

        public void ResetChanges(bool closeMenus)
        {
            try
            {
                if (closeMenus)
                {
                    var so = new SerializedObject(light);
                    so.FindProperty("foldout").boolValue = false;
                    so.ApplyModifiedProperties();
                }
            }
            catch (ArgumentException e)
            {
                e.GetType();
            }
#if UDONSHARP
#pragma warning disable 0618 //suppressing obsoletion warnings
            light.UpdateProxy();
#pragma warning restore 0618 //suppressing obsoletion warnings
#endif
            light.enableDMXChannels = P_enableDMXChannels = Z_enableDMXChannels;
            light.fixtureID = P_fixtureID = Z_fixtureID;
            light.dmxChannel = P_dmxChannel = Z_dmxChannel;
            light.dmxUniverse = P_dmxUniverse = Z_dmxUniverse;
            light.Channel = P_Channel = Z_Channel;
            light.legacyGoboRange = P_legacyGoboRange = Z_legacyGoboRange;
            light.globalIntensity = P_globalIntensity = Z_globalIntensity;
            light.finalIntensity = P_finalIntensity = Z_finalIntensity;
            light.lightColorTint = P_lightColorTint = Z_lightColorTint;
            light.invertPan = P_invertPan = Z_invertPan;
            light.invertTilt = P_invertTilt = Z_invertTilt;
            light.isUpsideDown = P_isUpsideDown = Z_isUpsideDown;
            light.enableAutoSpin = P_enableAutoSpin = Z_enableAutoSpin;
            light.enableStrobe = P_enableStrobe = Z_enableStrobe;
            light.tiltOffsetBlue = P_tiltOffsetBlue = Z_tiltOffsetBlue;
            light.panOffsetBlueGreen = P_panOffsetBlueGreen = Z_panOffsetBlueGreen;
            light.selectGOBO = P_selectGOBO = Z_selectGOBO;
            light.coneWidth = P_coneWidth = Z_coneWidth;
            light.coneLength = P_coneLength = Z_coneLength;
            light.maxConeLength = P_maxConeLength = Z_maxConeLength;
            light.maxMinPan = P_maxMinPan = Z_maxMinPan;
            light.maxMinTilt = P_maxMinTilt = Z_maxMinTilt;
            light.foldout = false;
#if UDONSHARP
#pragma warning disable 0618 //suppressing obsoletion warnings
            light.ApplyProxyModifications();
#pragma warning restore 0618 //suppressing obsoletion warnings
#endif
        }

        public void ApplyChanges()
        {
            //  Undo.RecordObject(light, "Undo Apply Changes");
            //  PrefabUtility.RecordPrefabInstancePropertyModifications(light);
            try
            {
                var so = new SerializedObject(light);
                so.FindProperty("enableDMXChannels").boolValue = P_enableDMXChannels;
                so.FindProperty("fixtureID").intValue = P_fixtureID;
                so.FindProperty("dmxChannel").intValue = P_dmxChannel;
                so.FindProperty("Channel").intValue = P_Channel;
                so.FindProperty("legacyGoboRange").boolValue = P_legacyGoboRange;
                so.FindProperty("globalIntensity").floatValue = P_globalIntensity;
                so.FindProperty("finalIntensity").floatValue = P_finalIntensity;
                so.FindProperty("lightColorTint").colorValue = P_lightColorTint;
                so.FindProperty("invertPan").boolValue = P_invertPan;
                so.FindProperty("invertTilt").boolValue = P_invertTilt;
                so.FindProperty("isUpsideDown").boolValue = P_isUpsideDown;
                so.FindProperty("enableAutoSpin").boolValue = P_enableAutoSpin;
                so.FindProperty("enableStrobe").boolValue = P_enableStrobe;
                so.FindProperty("tiltOffsetBlue").floatValue = P_tiltOffsetBlue;
                so.FindProperty("panOffsetBlueGreen").floatValue = P_panOffsetBlueGreen;
                so.FindProperty("selectGOBO").intValue = P_selectGOBO;
                so.FindProperty("coneWidth").floatValue = P_coneWidth;
                so.FindProperty("coneLength").floatValue = P_coneLength;
                so.FindProperty("maxConeLength").floatValue = P_maxConeLength;
                so.FindProperty("maxMinPan").floatValue = P_maxMinPan;
                so.FindProperty("maxMinTilt").floatValue = P_maxMinTilt;
                so.FindProperty("foldout").boolValue = foldout;
                so.ApplyModifiedProperties();

#if UDONSHARP
#pragma warning disable 0618 //suppressing obsoletion warnings
                light.UpdateProxy();
#pragma warning restore 0618 //suppressing obsoletion warnings
#endif
                light.enableDMXChannels = Z_enableDMXChannels = P_enableDMXChannels;
                light.fixtureID = Z_fixtureID = P_fixtureID;
                light.dmxChannel = Z_dmxChannel = P_dmxChannel;
                light.dmxUniverse = Z_dmxUniverse = P_dmxUniverse;
                light.Channel = Z_Channel = P_Channel;
                light.legacyGoboRange = Z_legacyGoboRange = P_legacyGoboRange;
                light.globalIntensity = Z_globalIntensity = P_globalIntensity;
                light.finalIntensity = Z_finalIntensity = P_finalIntensity;
                light.lightColorTint = Z_lightColorTint = P_lightColorTint;
                light.invertPan = Z_invertPan = P_invertPan;
                light.invertTilt = Z_invertTilt = P_invertTilt;
                light.isUpsideDown = Z_isUpsideDown = P_isUpsideDown;
                light.enableAutoSpin = Z_enableAutoSpin = P_enableAutoSpin;
                light.enableStrobe = Z_enableStrobe = P_enableStrobe;
                light.tiltOffsetBlue = Z_tiltOffsetBlue = P_tiltOffsetBlue;
                light.panOffsetBlueGreen = Z_panOffsetBlueGreen = P_panOffsetBlueGreen;
                light.selectGOBO = Z_selectGOBO = P_selectGOBO;
                light.coneWidth = Z_coneWidth = P_coneWidth;
                light.coneLength = Z_coneLength = P_coneLength;
                light.maxConeLength = Z_maxConeLength = P_maxConeLength;
                light.maxMinPan = Z_maxMinPan = P_maxMinPan;
                light.maxMinTilt = Z_maxMinTilt = P_maxMinTilt;
                light.foldout = foldout;
#if UDONSHARP
#pragma warning disable 0618 //suppressing obsoletion warnings
                light.ApplyProxyModifications();
#pragma warning restore 0618 //suppressing obsoletion warnings
#endif
                if (PrefabUtility.IsPartOfAnyPrefab(light))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(light);
                }
            }
            catch (Exception e)
            {
                e.GetType();
            }
        }
    }
}
#endif