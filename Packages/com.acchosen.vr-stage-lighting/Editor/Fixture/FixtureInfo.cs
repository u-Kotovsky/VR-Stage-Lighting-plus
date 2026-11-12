using System;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [CreateAssetMenu(menuName = "VRSL/Fixture Info")]
    public class FixtureInfo : ScriptableObject
    {
        public string Name;
        public string Description;
        public FixtureParameterInfo[] Parameters;
        public FixtureAuthor[] Authors;

        private void OnEnable()
        {
            FixtureManager.UpdateFixtures();
        }

        public string GetGuid()
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out string guid, out long localId))
            {
                return guid;
            }

            throw new Exception("Failed to get Guid");
        }

        public int GetChannelWidth()
        {
            var count = 0;
            
            foreach (var fixtureParameterInfo in Parameters)
            {
                count++;
                if (fixtureParameterInfo.fine) count++;
                if (fixtureParameterInfo.ultra) count++;
                if (fixtureParameterInfo.uber) count++;
            }
            
            return count;
        }
    }

    [Serializable]
    public class FixtureParameterInfo
    {
        public string name;
        public bool fine;
        public bool ultra;
        public bool uber;
    }

    [Serializable]
    public class FixtureAuthor
    {
        public string Name;
        public string[] Contacts;
    }
}
