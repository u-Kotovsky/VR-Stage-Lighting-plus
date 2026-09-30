#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace VRSL.EditorScripts
{
    [Obsolete("Being replaced by FixtureInfo", false)]
    [Serializable]
    public struct FixtureDefinition
    {
        public FixtureDefinition(string n)
        {
            name = n;
            channelNames = new string[1];
            foldOut = false;
        }
        
        public string name;
        public string[] channelNames;
        public bool foldOut;

        public void SetNewChannelSize(int size)
        {
            string[] newChannelNames = new string[size];
            if(channelNames != null)
            {
                int loopVal;
                if(newChannelNames.Length > channelNames.Length)
                {
                    loopVal = channelNames.Length;
                }
                else
                {
                    loopVal = newChannelNames.Length;
                }

                for(int i = 0; i < loopVal; i++)
                {
                    newChannelNames[i] = channelNames[i];
                }
                channelNames = newChannelNames;
            }
            else
            {
                channelNames = new string[1];
                channelNames[0] = "";
            }
        }
    }
    
    [CreateAssetMenu(menuName = "VRSL/DMX Fixture Definition File", fileName = "VRSL DMX Fixture Definitions")]
    [Serializable]
    public class VRSL_FixtureDefinitions : ScriptableObject
    {
        [HideInInspector]
        public FixtureDefinition[] definitions = new FixtureDefinition[1];

        public VRSL_FixtureDefinitions()
        {
            if(definitions != null)
            {
                if(definitions.Length > 0)
                {
                    definitions[0].channelNames = new string[1];
                }
            }
        }
        public void ForceSave()
        {
            //string assetPath =  AssetDatabase.GetAssetPath(this.GetInstanceID());
            //if(targetScene != null)
                //AssetDatabase.RenameAsset(assetPath, "VRSL DMX Fixture Definitions_" + targetScene.name);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<VRSL_FixtureDefinitions>(AssetDatabase.GetAssetPath(GetInstanceID())); 
        }

        public string[] GetNames()
        {
            string[] names = new string[definitions.Length];
            for(int i = 0; i < definitions.Length; i++)
            {
                names[i] = definitions[i].name;
            }
            return names;
        }
        
        public string[] GetChannelDefinition(int defID)
        {
            return definitions[defID].channelNames;
        }
        
        public int DefinitionsArraySize
        {
            get => definitions.Length;
            set
            {
                FixtureDefinition[] newDefinitions = new FixtureDefinition[value];
                
                int loopVal;
                
                if(newDefinitions.Length > definitions.Length)
                {
                    loopVal = definitions.Length;
                }
                else
                {
                    loopVal = newDefinitions.Length;
                }
                for(int i = 0; i < loopVal; i++)
                {
                    newDefinitions[i] = definitions[i];
                }
                
                definitions = newDefinitions;
            }
        } 
    }
}
#endif