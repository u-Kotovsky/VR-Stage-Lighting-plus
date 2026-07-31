#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [Serializable]
    public struct DMXFixtureData
    {
        public DMXFixtureData(VRStageLighting_DMX_Static fixture, GlobalObjectId id)
        {
            name = fixture.gameObject.name;
            position = fixture.gameObject.transform.position;
            rotation = fixture.gameObject.transform.rotation;
            targetObjectId = id.targetObjectId;
            targetPrefabId = id.targetPrefabId;
            assetGUID = id.assetGUID.ToString();
            enableDMXChannels = fixture.enableDMXChannels;
            fixtureID = fixture.fixtureID;
            Channel = fixture.Channel;
            legacyGoboRange = fixture.legacyGoboRange;
            globalIntensity = fixture.globalIntensity;
            finalIntensity = fixture.finalIntensity;
            lightColorTint = fixture.lightColorTint;
            invertPan = fixture.invertPan;
            invertTilt = fixture.invertTilt;
            isUpsideDown = fixture.isUpsideDown;
            enableAutoSpin = fixture.enableAutoSpin;
            enableStrobe = fixture.enableStrobe;
            tiltOffsetBlue = fixture.tiltOffsetBlue;
            panOffsetBlueGreen = fixture.panOffsetBlueGreen;
            selectGOBO = fixture.selectGOBO;
            globalChannelIndex = fixture.globalChannelIndex;
            //objRenderers = fixture.objRenderers;
            objRenderers = new DMXFixtureData_ObjRenderers(fixture.objRenderers);
            coneWidth = fixture.coneWidth;
            coneLength = fixture.coneLength;
            maxConeLength = fixture.maxConeLength;
            maxMinPan = fixture.maxMinPan;
            maxMinTilt = fixture.maxMinTilt;
            fixtureDefintion = fixture.fixtureDefintion;

            dmxChannel = fixture.dmxChannel;
            dmxUniverse = fixture.dmxUniverse;
        }

        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public ulong targetPrefabId;
        public ulong targetObjectId;
        public string assetGUID;
        public bool enableDMXChannels;
        public int fixtureID;
        public int dmxChannel;
        public int dmxUniverse;
        public int fixtureDefintion;
        public int Channel;
        public bool legacyGoboRange;
        public float globalIntensity;
        public float finalIntensity;
        public Color lightColorTint;
        public bool invertPan;
        public bool invertTilt;
        public bool isUpsideDown;
        public bool enableAutoSpin;
        public bool enableStrobe;
        public float tiltOffsetBlue;
        public float panOffsetBlueGreen;
        public int selectGOBO;
        public int globalChannelIndex;
        public DMXFixtureData_ObjRenderers objRenderers;
        public float coneWidth;
        public float coneLength;
        public float maxConeLength;
        public float maxMinPan;
        public float maxMinTilt;
    }
}
#endif