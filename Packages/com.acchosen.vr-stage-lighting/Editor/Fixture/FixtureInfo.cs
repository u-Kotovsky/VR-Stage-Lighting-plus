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
            string guid;
            long localId;
                
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out guid, out localId))
            {
                return guid;
            }
            else
            {
                throw new Exception("Failed to get Guid");
            }

            return null;
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
