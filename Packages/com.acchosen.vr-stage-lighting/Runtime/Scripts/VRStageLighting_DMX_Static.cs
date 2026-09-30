using System;
using UnityEngine;

#if UDONSHARP
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;

#if UDONSHARP
using UdonSharpEditor;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
#endif
#endif

namespace VRSL
{
#if UDONSHARP
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
#endif
    public class VRStageLighting_DMX_Static :
#if UDONSHARP
        UdonSharpBehaviour
#else
        MonoBehaviour
#endif
    {
        #region DMX Settings
        [Header("DMX Settings")]
        [Tooltip ("Enables DMX mode for this fixture.")]
        public bool enableDMXChannels = true;
        public bool IsDMX
        {
            get
            {
                return enableDMXChannels;
            }
            set
            {
                enableDMXChannels = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Tooltip ("The ID number for this fixture. This is mostly for organizational purposes and is entirely optional. Most DMX software have an ID attached to each fixture to run the fixtures through commands more easily, and it is recommended to have those IDs lined up here as well for the sake simplicity. This ID is public and can also be used for Udon scripting as well.")]
        public int fixtureID;
        
        [Tooltip ("The industry standard DMX Channel this fixture begins on. Most standard VRSL fixtures are 13 channels")]
        public int dmxChannel = 1;
        
        [Tooltip ("The industry standard Artnet Universe. Use this to choose which universe to read the DMX Channel from.")]
        public int dmxUniverse = 1;

        private int calculatedDMXChannel;
        public int _GetDMXChannel()
        {
            return calculatedDMXChannel;
        }
        private int calculatedDMXUniverse;
        public int _GetUniverse()
        {
            return calculatedDMXUniverse;
        }

        [HideInInspector]
        public int globalChannelIndex;
        public int GlobalChannelIndex
        {
            get
            {
                SetGlobalChannelFromLocal();
                return globalChannelIndex;
            }
            set
            {
                globalChannelIndex = Math.Abs(value);
                SetLocalChannelFromGlobal();
            }
        }
        
        public bool enableDmxTranslate;
        public int dmxTranslateChannel;
        
        [Tooltip ("Chooses the which of the 13 Channels of the current sector to sample from when single channel mode is enabled. Do not worry about this value if you are not using a single-channeled fixture.")]
        [Range(0.0f, 12.0f)]
        public int Channel;
        public bool legacyGoboRange;
        #endregion
        
        #region General Settings
        [Space(5)]
        [Header("General Settings")]
        [Range(0,1)]
        [Tooltip ("Sets the overall intensity of the shader. Good for animating or scripting effects related to intensity. Its max value is controlled by Final Intensity.")]
        public float globalIntensity = 1;
        public float GlobalIntensity
        {
            get
            {
                return globalIntensity;
            }
            set
            {
                previousGlobalIntensity = globalIntensity;
                globalIntensity = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(0,1)]
        [Tooltip ("Sets the maximum brightness value of Global Intensity. Good for personalized settings of the max brightness of the shader by other users via UI.")]
        public float finalIntensity = 1;
        public float FinalIntensity
        {
            get
            {
                return finalIntensity;
            }
            set
            {
                previousFinalIntensity = finalIntensity;
                finalIntensity = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Tooltip ("Choose between setting the Final Intensity for all meshes, or individual meshes")]
        public bool finalIntensityComponentMode;
        public bool FinalIntensityComponentMode
        {
            get
            {
                return finalIntensityComponentMode;
            }
            set
            {
                finalIntensityComponentMode = value;
                _UpdateInstancedProperties();
            }            
        }
        
        [Range(0,1)]
        [Tooltip ("Sets the maximum brightness value of Global Intensity For Volumetric Meshes Only. Good for personalized settings of the max brightness of the shader by other users via UI.")]
        public float finalIntensityVolumetric = 1;
        public float FinalIntensityVolumetric
        {
            get
            {
                return finalIntensityVolumetric;
            }
            set
            {
                previousFinalIntensityVolumetric = finalIntensityVolumetric;
                finalIntensityVolumetric  = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(0,1)]
        [Tooltip ("Sets the maximum brightness value of Global Intensity For Projection Meshes Only. Good for personalized settings of the max brightness of the shader by other users via UI.")]
        public float finalIntensityProjection = 1;
        public float FinalIntensityProjection
        {
            get
            {
                return finalIntensityProjection;
            }
            set
            {
                previousFinalIntensityProjection = finalIntensityProjection;
                finalIntensityProjection  = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(0,1)]
        [Tooltip ("Sets the maximum brightness value of Global Intensity For Fixture Meshes Only. Good for personalized settings of the max brightness of the shader by other users via UI.")]
        public float finalIntensityFixture = 1;
        public float FinalIntensityFixture
        {
            get
            {
                return finalIntensityFixture;
            }
            set
            {
                previousFinalIntensityFixture = finalIntensityFixture;
                finalIntensityFixture  = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Tooltip ("The main color of the light. Leave it at default white for DMX mode.")]
        [ColorUsage(false,true)]
        public Color lightColorTint = Color.white * 2.0f;
        public Color LightColorTint
        {
            get
            {
                return lightColorTint;
            }
            set
            {
                previousColorTint = lightColorTint;
                lightColorTint = value;
                _UpdateInstancedProperties();
            }
        }
        #endregion
        
        #region Movement Settings
        [Space(5)]
        [Header("Movement Settings")]
        [Tooltip ("Invert the pan values (Left/Right Movement) for movers.")]
        public bool invertPan;
        public bool InvertPan
        {
            get
            {
                return invertPan;
            }
            set
            {
                invertPan = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Tooltip ("Invert the tilt values (Up/Down Movement) for movers.")]
        public bool invertTilt;
        public bool InvertTilt
        {
            get
            {
                return invertTilt;
            }
            set
            {
                invertTilt = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Tooltip ("Enable this if the mover is hanging upside down.")]
        public bool isUpsideDown;
        #endregion
        
        #region Fixture Settings
        [Space(5)]
        [Header("Fixture Settings")]
        [Tooltip ("Enable projection spinning (Udon Override Only).")]
        public bool enableAutoSpin = true;
        public bool ProjectionSpin
        {
            get
            {
                return enableAutoSpin;
            }
            set
            {
                enableAutoSpin = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Tooltip ("Enable strobe effects (via DMX Only).")]
        public bool enableStrobe = true;
        
        [Range(0,360.0f)]
        [Tooltip ("Tilt (Up/Down) offset/movement. Directly controls tilt when in Udon Mode; is an offset when in DMX mode.")]
        public float tiltOffsetBlue = 90.0f;
        public float Tilt
        {
            get
            {
                return tiltOffsetBlue;
            }
            set
            {
                tiltOffsetBlue = value;
                _UpdateInstancedProperties();
            }
        }
        private float startTiltOffset;

        [Range(0,360.0f)]
        [Tooltip ("Pan (Left/Right) offset/movement. Directly controls pan when in Udon Mode; is an offset when in DMX mode.")]
        public float panOffsetBlueGreen;
        public float Pan
        {
            get
            {
                return panOffsetBlueGreen;
            }
            set
            {
                panOffsetBlueGreen = value;
                _UpdateInstancedProperties();
            }
        }
        private float startPanOffset;
        
        [Range(1,8)]
        [Tooltip ("Use this to change what projection is selected. This is overridden in DMX mode.")]
        public int selectGOBO = 1;
        public int SelectGOBO
        {
            get
            {
                return selectGOBO;
            }
            set
            {
                previousGOBOSelection = selectGOBO;
                selectGOBO = value;
                _UpdateInstancedProperties();
            }
        }
        
        //[Header("Mesh Settings")]
        [Tooltip ("The meshes used to make up the light. You need atleast 1 mesh in this group for the script to work properly.")]
        public MeshRenderer[] objRenderers;

        [Range(0, 5.5f)]
        [Tooltip ("Controls the radius of a mover/spot light.")]
        public float coneWidth = 2.5f;
        public float ConeWidth
        {
            get
            {
                return coneWidth;
            }
            set
            {
                previousConeWidth = coneWidth;
                coneWidth = value;
                _UpdateInstancedProperties();
            }
        }

        [Range(0.5f,10.0f)]
        [Tooltip ("Controls the length of the cone of a mover/spot light.")]
        public float coneLength = 8.5f;
        public float ConeLength
        {
            get
            {
                return ConeLength;
            }
            set
            {
                previousConeLength = coneLength;
                coneLength = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(0.275f,10.0f)]
        [Tooltip ("Controls the mesh length of the cone of a mover/spot light")]
        public float maxConeLength = 1.0f;
        public float MaxConeLength
        {
            get
            {
                return MaxConeLength;
            }
            set
            {
                previousMaxConeLength = maxConeLength;
                maxConeLength = value;
                _UpdateInstancedProperties();
            }
        }

        public float maxMinPan = 180f;
        public float maxMinTilt = -180f;

        [HideInInspector]
        public int fixtureDefintion;

        public string fixtureInfoGuid; // points to a asset guid that has info for this fixture.
        #endregion
        
        private void SetGlobalChannelFromLocal()
        {
            //globalChannelIndex = dmxChannel - 1 + ((dmxUniverse - 1) * 512);
            globalChannelIndex = GetGlobalChannel(dmxUniverse, dmxChannel);
        }
        
        private void SetLocalChannelFromGlobal()
        {
            GetUniverseAndChannel(globalChannelIndex, out dmxUniverse, out dmxChannel);
            
            calculatedDMXChannel = dmxChannel;
            calculatedDMXUniverse = dmxUniverse;
        }

        public int GetGlobalChannel(int universe, int channel)
        {
            return channel - 1 + ((universe - 1) * 512);
        }

        public void GetUniverseAndChannel(int globalChannelIndex, out int universe, out int channel)
        {
            var offset = globalChannelIndex + 1;
            universe = (offset / 512) + 1;
            channel = offset % 512;
        }

        private bool wasChanged;
        private MaterialPropertyBlock props;
        private bool enableInstancing;
        private float targetPanAngle, targetTiltAngle;
        private UnityEngine.Vector3 targetToFollowLast;
        private Color previousColorTint;
        private Transform previousTargetToFollowTransform;
        
        private float previousConeWidth, previousConeLength, previousGlobalIntensity, previousFinalIntensity, previousMaxConeLength;
        private float previousFinalIntensityVolumetric, previousFinalIntensityProjection, previousFinalIntensityFixture;
        private int previousGOBOSelection;
        
        [HideInInspector]
        public bool foldout;

        private void Start()
        {
            Init(true);
        }

        private void Init(bool withDmx)
        {
            if (objRenderers == null || objRenderers.Length == 0)
            {
                Debug.LogError($"There are no object renderers on {gameObject.name}.");
                return;
            }
            
            _SetProps();
            
            previousColorTint = lightColorTint;
            previousConeWidth = coneWidth;
            previousConeLength = coneLength;
            previousMaxConeLength = maxConeLength;
            previousGOBOSelection = selectGOBO;
            previousGlobalIntensity = globalIntensity;
            previousFinalIntensity = finalIntensity;
            previousFinalIntensityFixture = finalIntensityFixture;
            previousFinalIntensityProjection = finalIntensityProjection;
            previousFinalIntensityVolumetric = finalIntensityVolumetric;
            
            if(withDmx)
            {
                _UpdateInstancedProperties();
            }
            else
            {
                _UpdateInstancedPropertiesSansDMX();
            }
        }
        
        public void _SetProps()
        {
            props = new MaterialPropertyBlock();
        }

        private int RawDmxConversion() // UNIVERSE.CHANNEL into GLOBAL_CHANNEL
        {
            calculatedDMXChannel = dmxChannel;
            calculatedDMXUniverse = dmxUniverse;
            return Mathf.Abs(dmxChannel + ((dmxUniverse - 1) * 512)) - 1;
        }

        private MaterialPropertyBlock _SetFinalIntensityComponents(MaterialPropertyBlock props, MeshRenderer renderer)
        {
            if (!finalIntensityComponentMode) return props;
            
            if (renderer.gameObject.name.Contains("Volume") || 
               renderer.gameObject.name.Contains("volume") || 
               renderer.gameObject.name.Contains("Flare") || 
               renderer.gameObject.name.Contains("flare"))
            {
                props.SetFloat("_FinalIntensity", finalIntensityVolumetric);
            }
            else if(renderer.gameObject.name.Contains("Project") || 
                    renderer.gameObject.name.Contains("project"))
            {
                props.SetFloat("_FinalIntensity", finalIntensityProjection);
            }
            else
            {
                props.SetFloat("_FinalIntensity", finalIntensityFixture);
            }
            
            return props;
        }
        
        public void _UpdateInstancedProperties()
        {
            if(props == null)
            {
                if (objRenderers == null || objRenderers.Length == 0)
                {
                    Debug.LogError($"There are no object renderers on {gameObject.name}.");
                    return;
                }
                
                _SetProps();
            }

            props.SetInt("_EnableDMX", enableDMXChannels ? 1 : 0);
            props.SetInt("_DMXChannel", RawDmxConversion());
            props.SetInt("_EnableDMXTranslateChannel", enableDmxTranslate ? 1 : 0);
            props.SetInt("_DMXTranslateChannel", dmxTranslateChannel);
            props.SetInt("_NineUniverseMode", 0);
            props.SetInt("_PanInvert", invertPan ? 1 : 0);
            props.SetInt("_TiltInvert", invertTilt ? 1 : 0);
            props.SetInt("_LegacyGoboRange", legacyGoboRange ? 1 : 0);
            props.SetInt("_EnableStrobe", enableStrobe ? 1 : 0);
            props.SetInt("_EnableSpin", enableAutoSpin ? 1 : 0);
            props.SetInt("_ProjectionSelection", selectGOBO);
            props.SetFloat("_FixtureRotationX", tiltOffsetBlue);
            props.SetFloat("_FixtureBaseRotationY", panOffsetBlueGreen);
            props.SetColor("_Emission", lightColorTint);
            props.SetColor("_EmissionDMX", lightColorTint);
            props.SetFloat("_ConeWidth", coneWidth);
            props.SetFloat("_GlobalIntensity", globalIntensity);
            props.SetFloat("_FinalIntensity", finalIntensity);
            props.SetFloat("_ConeLength", Mathf.Abs(coneLength - 10.5f));
            props.SetFloat("_MaxConeLength", maxConeLength);
            props.SetFloat("_MaxMinPanAngle", maxMinPan/2.0f);
            props.SetFloat("_MaxMinTiltAngle", maxMinTilt/2.0f);
            
            foreach (var meshRenderer in objRenderers)
            {
                ApplyPropertyBlockWithFinalIntensity(meshRenderer, props);
            }
        }
        
        public void _UpdateInstancedPropertiesSansDMX()
        {
            if(props == null)
            {
                if (objRenderers == null || objRenderers.Length == 0)
                {
                    Debug.LogError($"There are no object renderers on {gameObject.name}.");
                    return;
                }
                
                _SetProps();
            }
            
            props.SetInt("_EnableDMX", 0);
            props.SetInt("_DMXChannel", RawDmxConversion());
            props.SetInt("_EnableDMXTranslateChannel", enableDmxTranslate ? 1 : 0);
            props.SetInt("_DMXTranslateChannel", dmxTranslateChannel);
            props.SetInt("_NineUniverseMode", 0);
            props.SetInt("_PanInvert", invertPan ? 1 : 0);
            props.SetInt("_TiltInvert", invertTilt ? 1 : 0);
            props.SetInt("_LegacyGoboRange", legacyGoboRange ? 1 : 0);
            props.SetInt("_EnableStrobe", 0);
            props.SetInt("_EnableSpin", enableAutoSpin ? 1 : 0);
            props.SetInt("_ProjectionSelection", selectGOBO);
            props.SetFloat("_FixtureRotationX", tiltOffsetBlue);
            props.SetFloat("_FixtureBaseRotationY", panOffsetBlueGreen);
            props.SetColor("_Emission", lightColorTint);
            props.SetColor("_EmissionDMX", lightColorTint);
            props.SetFloat("_ConeWidth", coneWidth);
            props.SetFloat("_GlobalIntensity", globalIntensity);
            props.SetFloat("_FinalIntensity", finalIntensity);
            props.SetFloat("_ConeLength", Mathf.Abs(coneLength - 10.5f));
            props.SetFloat("_MaxConeLength", maxConeLength);
            props.SetFloat("_MaxMinPanAngle", maxMinPan/2.0f);
            props.SetFloat("_MaxMinTiltAngle", maxMinTilt/2.0f);
            
            foreach (var meshRenderer in objRenderers)
            {
                ApplyPropertyBlockWithFinalIntensity(meshRenderer, props);
            }
        }

        private void ApplyPropertyBlockWithFinalIntensity(MeshRenderer component, MaterialPropertyBlock propertyBlock)
        {
            if (component)
            {
                component.SetPropertyBlock(_SetFinalIntensityComponents(propertyBlock, component));
            }
        }
        
        public string _DMXChannelToString()
        {
            return "DMX Channel: " + calculatedDMXChannel + "  Universe: " + calculatedDMXUniverse + "  shader: " + RawDmxConversion();
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate()
        {
            var e = Event.current;

            if (e == null) return;
            if (e.type == EventType.ExecuteCommand && e.commandName == "Duplicate")
            {
                Init(false);
            }
        }
#endif
    }
}