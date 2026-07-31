#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRSL.EditorScripts
{
    // ensure class initializer is called whenever scripts recompile
    [InitializeOnLoad]
    public static class PlayModeStateChanged
    {
        // register an event handler when the class is initialized
        static PlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnEditorSceneManagerSceneOpened;
            EditorApplication.update += RunOnce;
            //LoadFixtureSettings();
        }

        static void RunOnce()
        {
            LoadFixtureSettings();
        //  Debug.Log("Running Once... " + EditorApplication.update);
            EditorApplication.update -= RunOnce;
        }

        static void OnEditorSceneManagerSceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            //Debug.LogFormat("SceneOpened: {0}", scene.name);
            LoadFixtureSettings();
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
    //        Debug.Log(state);
            if(state == PlayModeStateChange.EnteredEditMode)
            {
                LoadFixtureSettings();
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void LoadFixtureSettings()
        {
            GameObject[] objs;
            try
            {
                Scene scene = SceneManager.GetActiveScene();
                objs = scene.GetRootGameObjects();
            }
            catch(NullReferenceException e)
            {
                e.GetType();
                return;
            }
            try
            {  
                foreach(GameObject obj in objs)
                {
#if UDONSHARP
                    #pragma warning disable 0618 //suppressing obsoletion warnings
                    //VRStageLighting_RAW_Static[] staticLights = obj.GetUdonSharpComponentsInChildren<VRStageLighting_RAW_Static>();
                    VRStageLighting_AudioLink_Static[] audioLinkLights = obj.GetUdonSharpComponentsInChildren<VRStageLighting_AudioLink_Static>();
                    // VRStageLighting_Animated_Static[] animatedLights = obj.GetUdonSharpComponentsInChildren<VRStageLighting_Animated_Static>();
                    VRStageLighting_DMX_Static[] dmxLights = obj.GetUdonSharpComponentsInChildren<VRStageLighting_DMX_Static>();
                    //VRStageLighting_RAW_Laser[] rawLasers = obj.GetUdonSharpComponentsInChildren<VRStageLighting_RAW_Laser>();
                    VRStageLighting_AudioLink_Laser[] audioLinkLasers = obj.GetUdonSharpComponentsInChildren<VRStageLighting_AudioLink_Laser>();
                    // VRStageLighting_DMX_Static[] dmxLights = obj.GetUdonSharpComponentsInChildren<VRStageLighting_DMX_Static>();
                    VRSL_LocalUIControlPanel[] controlPanels = obj.GetUdonSharpComponentsInChildren<VRSL_LocalUIControlPanel>();
                    #pragma warning restore 0618 //suppressing obsoletion warnings
#else
                    //VRStageLighting_RAW_Static[] staticLights = obj.GetComponentsInChildren<VRStageLighting_RAW_Static>();
                    VRStageLighting_AudioLink_Static[] audioLinkLights = obj.GetComponentsInChildren<VRStageLighting_AudioLink_Static>();
                    // VRStageLighting_Animated_Static[] animatedLights = obj.GetComponentsInChildren<VRStageLighting_Animated_Static>();
                    VRStageLighting_DMX_Static[] dmxLights = obj.GetComponentsInChildren<VRStageLighting_DMX_Static>();
                    //VRStageLighting_RAW_Laser[] rawLasers = obj.GetComponentsInChildren<VRStageLighting_RAW_Laser>();
                    VRStageLighting_AudioLink_Laser[] audioLinkLasers = obj.GetComponentsInChildren<VRStageLighting_AudioLink_Laser>();
                    // VRStageLighting_DMX_Static[] dmxLights = obj.GetComponentsInChildren<VRStageLighting_DMX_Static>();
                    VRSL_LocalUIControlPanel[] controlPanels = obj.GetComponentsInChildren<VRSL_LocalUIControlPanel>();
#endif
                    if(dmxLights != null)
                    {
                        foreach(VRStageLighting_DMX_Static fixture in dmxLights)
                        {
                            fixture._SetProps();
                            if(Application.isPlaying)
                            {
                                fixture._UpdateInstancedProperties();
                            }
                            else
                            {
                                fixture._UpdateInstancedPropertiesSansDMX();
                            }
                        }
                    }

                    if(audioLinkLasers != null)
                    {
                        foreach(VRStageLighting_AudioLink_Laser fixture in audioLinkLasers)
                        {
                            if(fixture.objRenderers.Length > 0 && fixture.objRenderers[0] != null)
                            {
                                fixture._SetProps();
                                if(Application.isPlaying)
                                {
                                    fixture._UpdateInstancedProperties();
                                }
                                else
                                {
                                    fixture._UpdateInstancedPropertiesSansAudioLink();
                                }
                            }
                        }
                    }
                    if(audioLinkLights != null)
                    {
                        foreach(VRStageLighting_AudioLink_Static fixture in audioLinkLights)
                        {
                            if(fixture.objRenderers.Length > 0 && fixture.objRenderers[0] != null)
                            {
                                fixture._SetProps();
                                if(Application.isPlaying)
                                {
                                    fixture._UpdateInstancedProperties();
                                }
                                else
                                {
                                    fixture._UpdateInstancedPropertiesSansAudioLink();
                                }

                            }
                        }
                    }

                    if(controlPanels != null)
                    {
                        foreach(VRSL_LocalUIControlPanel panel in controlPanels)
                        {
                            panel._CheckDepthLightStatus();
                            //Debug.Log("AutoChecking Status");
                        }
                    }

                }
            }
            catch(NullReferenceException e)
            {
                e.GetType();
                return;
            }
        }
    }
}

#endif