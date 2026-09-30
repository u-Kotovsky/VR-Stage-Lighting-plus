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
        private void GuiLine(int lineHeight = 1)
        {
            try
            {
                Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
                rect.height = lineHeight;

                EditorGUI.DrawRect(rect, new Color ( 0.5f,0.5f,0.5f, 1 ) );
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private SerializedProperty definitions;
        private VRSL_FixtureDefinitions fd;
        //SerializedObject so;
        
        private void OnEnable()
        {
            // Link the properties
            fd = (VRSL_FixtureDefinitions) target;
            //so = new SerializedObject(fd);
            definitions = serializedObject.FindProperty("definitions");
        }

        public override void OnInspectorGUI() 
        {
            DrawDefaultInspector();
            serializedObject.Update();
            //EditorGUI.BeginChangeCheck();
            
            if(fd != null && definitions.isArray)
            {
                var size = definitions.arraySize;
                    
                EditorGUILayout.BeginHorizontal();
                var newSize = EditorGUILayout.IntField("Size", size);
                newSize = Mathf.Abs(newSize);
                if(GUILayout.Button("-", GUILayout.Width(25f)))
                {
                    newSize--;
                    if(newSize < 1){newSize = 1;}
                }
                if(GUILayout.Button("+", GUILayout.Width(25f)))
                {
                    newSize++;
                }
                EditorGUILayout.EndHorizontal();
                if(GUILayout.Button("Save Changes"))
                {
                    serializedObject.ApplyModifiedProperties();
                    fd.ForceSave();
                }
                GuiLine();
                GUILayout.Space(25);
                bool mainIncreased = false;
                if (newSize != size)
                {
                    definitions.arraySize = newSize;
                    fd.DefinitionsArraySize = newSize;
                    mainIncreased = newSize > size;
                }
                EditorGUI.indentLevel++;
                //EditorGUI.indentLevel++;
                //definitions.arraySize = EditorGUILayout.IntField("Size",definitions.arraySize);
                for(int i = 0; i < newSize; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    SerializedProperty defProp = definitions.GetArrayElementAtIndex(i); 
                    SerializedProperty nameProp = defProp.FindPropertyRelative("name");
                    EditorGUILayout.BeginHorizontal("box");
                    nameProp.stringValue = EditorGUILayout.TextField($"Definition {i + 1}", nameProp.stringValue);
                        
                    SerializedProperty channelNamesProp = defProp.FindPropertyRelative("channelNames");
                    if (i >= size && mainIncreased)
                    {
                        nameProp.stringValue = string.Empty;
                        if (channelNamesProp.isArray)
                        {
                            channelNamesProp.arraySize = 1;
                            fd.definitions[i].SetNewChannelSize(1);
                            SerializedProperty channel = channelNamesProp.GetArrayElementAtIndex(0); 
                            channel.stringValue = string.Empty;
                        }
                    }
                    else
                    {
                        int chanSize = channelNamesProp.arraySize;
                        int newChanSize = chanSize;
                        if(GUILayout.Button("-", GUILayout.Width(25f)))
                        {
                            newChanSize--;
                            if(newChanSize < 1){newChanSize = 1;}
                        }
                        if(GUILayout.Button("+", GUILayout.Width(25f)))
                        {
                            newChanSize++;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel++;
                        EditorGUI.indentLevel++;
                        defProp.FindPropertyRelative("foldOut").boolValue = EditorGUILayout.Foldout(defProp.FindPropertyRelative("foldOut").boolValue, "Channels");
                        if(defProp.FindPropertyRelative("foldOut").boolValue)
                        {
                            if(channelNamesProp.isArray)
                            {
                                bool increased = false;
                                if (newChanSize != chanSize)
                                {
                                    channelNamesProp.arraySize = newChanSize;
                                    fd.definitions[i].SetNewChannelSize(newChanSize);
                                    increased = newChanSize > chanSize;
                                }

                                EditorGUI.indentLevel++;
                                // EditorGUI.indentLevel++;
                                for (int j = 0; j < newChanSize; j++)
                                {
                                    SerializedProperty channel = channelNamesProp.GetArrayElementAtIndex(j); 
                                    channel.stringValue = EditorGUILayout.TextField("Channel " + (j + 1).ToString(), channel.stringValue);
                                    if(j == newChanSize-1 && increased)
                                    {
                                        channel.stringValue = "";
                                    }
                                }
                                //EditorGUI.indentLevel--;
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
                //EditorGUI.indentLevel--;
            }
            
            //if(EditorGUI.EndChangeCheck())
            //{
                serializedObject.ApplyModifiedProperties();
                // if(fd != null)
                // {
                //     fd.ForceSave()
                // }
            //}
        }
    }
}
#endif