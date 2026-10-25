#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using UnityEditor;
using UnityEngine;

namespace VRSL.EditorScripts
{
    [Obsolete("Being replaced by FixtureInfo", false)]
    [CustomEditor(typeof(VRSL_FixtureDefinitions))]
    public class VRSL_FixtureDefinitionss_Editor: Editor 
    {
        private SerializedProperty _definitions;
        private VRSL_FixtureDefinitions _fixtureDefinitions;
        
        private void OnEnable()
        {
            // Link the properties
            _fixtureDefinitions = (VRSL_FixtureDefinitions) target;
            _definitions = serializedObject.FindProperty(nameof(_definitions));
        }

        public override void OnInspectorGUI() 
        {
            DrawDefaultInspector();
            serializedObject.Update();
            //EditorGUI.BeginChangeCheck();
            
            if(_fixtureDefinitions != null && _definitions.isArray)
            {
                var size = _definitions.arraySize;
                    
                EditorGUILayout.BeginHorizontal();
                var newSize = Mathf.Abs(EditorGUILayout.IntField("Size", size));
                if (GUILayout.Button("-", GUILayout.Width(25f)))
                {
                    newSize--;
                    if (newSize < 1)
                    {
                        newSize = 1;
                    }
                }
                if (GUILayout.Button("+", GUILayout.Width(25f)))
                {
                    newSize++;
                }
                EditorGUILayout.EndHorizontal();
                
                
                if (GUILayout.Button("Save Changes"))
                {
                    serializedObject.ApplyModifiedProperties();
                    _fixtureDefinitions.ForceSave();
                }
                
                VRSLStyles.GUILine(1);
                
                GUILayout.Space(25);
                
                bool mainIncreased = false;
                if (newSize != size)
                {
                    _definitions.arraySize = newSize;
                    _fixtureDefinitions.DefinitionsArraySize = newSize;
                    mainIncreased = newSize > size;
                }
                EditorGUI.indentLevel++;
                
                for (int i = 0; i < newSize; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    var defProp = _definitions.GetArrayElementAtIndex(i); 
                    var nameProp = defProp.FindPropertyRelative("name");
                    EditorGUILayout.BeginHorizontal("box");
                    nameProp.stringValue = EditorGUILayout.TextField($"Definition {i + 1}", nameProp.stringValue);
                        
                    var channelNamesProp = defProp.FindPropertyRelative("channelNames");
                    if (i >= size && mainIncreased)
                    {
                        nameProp.stringValue = string.Empty;
                        if (channelNamesProp.isArray)
                        {
                            channelNamesProp.arraySize = 1;
                            _fixtureDefinitions.definitions[i].SetNewChannelSize(1);
                            var channel = channelNamesProp.GetArrayElementAtIndex(0); 
                            channel.stringValue = string.Empty;
                        }
                    }
                    else
                    {
                        var chanSize = channelNamesProp.arraySize;
                        var newChanSize = chanSize;
                        if (GUILayout.Button("-", GUILayout.Width(25f)))
                        {
                            newChanSize--;
                            if (newChanSize < 1)
                            {
                                newChanSize = 1;
                            }
                        }
                        if (GUILayout.Button("+", GUILayout.Width(25f)))
                        {
                            newChanSize++;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel++;
                        EditorGUI.indentLevel++;
                        defProp.FindPropertyRelative("foldOut").boolValue = 
                            EditorGUILayout.Foldout(defProp.FindPropertyRelative("foldOut").boolValue, "Channels");
                        if (defProp.FindPropertyRelative("foldOut").boolValue)
                        {
                            if (channelNamesProp.isArray)
                            {
                                var increased = false;
                                if (newChanSize != chanSize)
                                {
                                    channelNamesProp.arraySize = newChanSize;
                                    _fixtureDefinitions.definitions[i].SetNewChannelSize(newChanSize);
                                    increased = newChanSize > chanSize;
                                }

                                EditorGUI.indentLevel++;
                                for (var j = 0; j < newChanSize; j++)
                                {
                                    var channel = channelNamesProp.GetArrayElementAtIndex(j); 
                                    channel.stringValue = EditorGUILayout.TextField("Channel " + (j + 1), channel.stringValue);
                                    if (j == newChanSize-1 && increased)
                                    {
                                        channel.stringValue = "";
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUI.indentLevel--;
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                }
                EditorGUI.indentLevel--;
            }
            
            //if(EditorGUI.EndChangeCheck())
            //{
                serializedObject.ApplyModifiedProperties();
                // if(_fixtureDefinitions != null)
                // {
                //     _fixtureDefinitions.ForceSave()
                // }
            //}
        }
    }
}
#endif