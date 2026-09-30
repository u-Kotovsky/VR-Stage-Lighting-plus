using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [CreateAssetMenu(menuName = "VRSL/Fixture Manager")]
    public class FixtureManager : ScriptableObject
    {
        public List<FixtureInfo> Fixtures;

        public static FixtureManager instance;

        public FixtureManager()
        {
            instance = this;
        }
        
        // TODO: on assembly reload make sure all fixtures in the project are assigned!

        public static void UpdateFixtures()
        {
            var maanger = GetAllInstances<FixtureManager>()[0];
            
            maanger.Fixtures = GetAllInstances<FixtureInfo>();
        }
        
        public static List<T> GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(so => so != null)
                .ToList();
        }

        public static List<string> GetAllFixtureInfoNames()
        {
            return instance.Fixtures.Select(instanceFixture => instanceFixture.name).ToList();
        }

        public static int GetFixtureIndexByGuid(string searchGuid)
        {
            var index = -1;

            for (var i = 0; i < instance.Fixtures.Count; i++)
            {
                var instanceFixture = instance.Fixtures[i];
                //string guid;
                //long localId;
                
                //if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instanceFixture, out guid, out localId)) continue;

                var guid = instanceFixture.GetGuid();
                if (guid == searchGuid)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                throw new Exception($"Failed to find fixture index with guid: '{searchGuid}'");
            }

            return index;
        }

        public static FixtureInfo GetFixtureInfoByGuid(string searchGuid)
        {
            foreach (var instanceFixture in from instanceFixture
                         in instance.Fixtures let guid = instanceFixture.GetGuid() where guid == searchGuid select instanceFixture)
            {
                return instanceFixture;
            }

            throw new Exception($"Failed to find fixture info with guid '{searchGuid}'");
        }
    }

    [CustomEditor(typeof(FixtureManager))]
    public class FixtureManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            var component = (FixtureManager)target;

            if (GUILayout.Button("Get All Fixtures"))
            {
                FixtureManager.UpdateFixtures();
            }
        }
    }
}