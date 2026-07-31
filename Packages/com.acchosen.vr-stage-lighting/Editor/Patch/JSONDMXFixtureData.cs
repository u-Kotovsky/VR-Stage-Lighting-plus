#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [Serializable]
    public struct JSONDMXFixtureData
    {
        public JSONDMXFixtureData(DMXFixtureData fixture, string[] fixtureTypes, string[] cd)
        {
            name = fixture.name;
            id = fixture.fixtureID;
            channel = fixture.dmxChannel;
            universe = fixture.dmxUniverse;
            fixtureDefintion = fixtureTypes[fixture.fixtureDefintion];
            channelNames = cd;
            position = fixture.position;
            rotation = fixture.rotation.eulerAngles;
            invertPan = fixture.invertPan;
            invertTilt = fixture.invertTilt;
            panRange = Mathf.Abs(fixture.maxMinPan);
            tiltRange = Mathf.Abs(fixture.maxMinTilt);
        }

        public int id;
        public string name;
        public int channel;
        public int universe;
        public string fixtureDefintion;
        public string[] channelNames;
        public Vector3 position;
        public Vector3 rotation;
        public bool invertPan;
        public bool invertTilt;
        public float panRange;
        public float tiltRange;
    }
}
#endif
