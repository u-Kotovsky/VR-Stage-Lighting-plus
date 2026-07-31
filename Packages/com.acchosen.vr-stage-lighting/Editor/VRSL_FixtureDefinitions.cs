#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace VRSL.EditorScripts
{
    [Serializable]
    public struct FixtureDefintion
    {
        public FixtureDefintion(string n)
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
            int loopVal = 0;
            if(channelNames != null)
            {
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
    
    [CreateAssetMenuAttribute(menuName = "VRSL/DMX Fixture Definition File", fileName = "VRSL DMX Fixture Definitions")]
    [System.Serializable]
    public class VRSL_FixtureDefinitions : ScriptableObject
    {
        [HideInInspector]
        public FixtureDefintion[] definitions = new FixtureDefintion[1];

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
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<VRSL_FixtureDefinitions>(AssetDatabase.GetAssetPath(this.GetInstanceID())); 
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
            get
            {
                return definitions.Length;
            }
            set
            {
                FixtureDefintion[] newDefinitions = new FixtureDefintion[value];
                int loopVal = 0;
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
                //definitions = new FixtureDefintion[value];
            }
        } 
    }
}
#endif