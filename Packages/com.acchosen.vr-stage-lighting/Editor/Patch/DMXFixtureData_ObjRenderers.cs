#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [Serializable]
    public struct DMXFixtureData_ObjRenderers
    {
        public DMXFixtureData_ObjRenderers(MeshRenderer[] objRenderers)
        {
            objRenderers_name = new string[objRenderers.Length];
            objRenderers_GlobalObjectId = new string[objRenderers.Length];
            for(int i = 0; i < objRenderers.Length; i++)
            {
                objRenderers_name[i] = objRenderers[i].name;
                GlobalObjectId objRenderers_id = GlobalObjectId.GetGlobalObjectIdSlow(objRenderers[i]);
                objRenderers_GlobalObjectId[i] = objRenderers_id.ToString();
            }
        }
        public string[] objRenderers_name;
        public string[] objRenderers_GlobalObjectId;
        
        public MeshRenderer[] GetRenderers()
        {
            List<MeshRenderer> renderers = new List<MeshRenderer>();
            for(int i = 0; i < objRenderers_GlobalObjectId.Length; i++)
            {
                GlobalObjectId id;
                if(GlobalObjectId.TryParse(objRenderers_GlobalObjectId[i], out id))
                {
                    MeshRenderer x = (MeshRenderer) GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                    //UnityEngine.Debug.Log("Found Renderer: " + x.name);
                    renderers.Add(x);
                }
            }
            return renderers.ToArray();
        }
    }
}
#endif