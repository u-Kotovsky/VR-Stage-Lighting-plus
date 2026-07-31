using System;

#if UNITY_EDITOR && !COMPILER_UDONSHARP

namespace VRSL.EditorScripts
{
    [Serializable]
    public struct JSONDMXFixtureData_Container
    {
        public JSONDMXFixtureData_Container(JSONDMXFixtureData[] d)
        {
            fixtures = d;
        }

        public JSONDMXFixtureData[] fixtures;
    }
}
#endif