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
        public bool enableDMXChannels = true;
        public bool IsDMX
        {
            get => enableDMXChannels;
            set
            {
                enableDMXChannels = value;
                _UpdateInstancedProperties();
            }
        }
        
        public int fixtureID;
        public int dmxChannel = 1;
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
        
        [Range(0.0f, 12.0f)]
        public int Channel;
        public bool legacyGoboRange;
        #endregion
        
        #region General Settings
        [Range(0,1)]
        public float globalIntensity = 1;
        public float GlobalIntensity
        {
            get => globalIntensity;
            set
            {
                globalIntensity = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(0,1)]
        public float finalIntensity = 1;
        public float FinalIntensity
        {
            get => finalIntensity;
            set
            {
                finalIntensity = value;
                _UpdateInstancedProperties();
            }
        }
        
        public bool finalIntensityComponentMode;
        public bool FinalIntensityComponentMode
        {
            get => finalIntensityComponentMode;
            set
            {
                finalIntensityComponentMode = value;
                _UpdateInstancedProperties();
            }            
        }
        
        [Range(0,1)]
        public float finalIntensityVolumetric = 1;
        public float FinalIntensityVolumetric
        {
            get => finalIntensityVolumetric;
            set
            {
                finalIntensityVolumetric = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(0,1)]
        public float finalIntensityProjection = 1;
        public float FinalIntensityProjection
        {
            get => finalIntensityProjection;
            set
            {
                finalIntensityProjection  = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(0,1)]
        public float finalIntensityFixture = 1;
        public float FinalIntensityFixture
        {
            get => finalIntensityFixture;
            set
            {
                finalIntensityFixture  = value;
                _UpdateInstancedProperties();
            }
        }
        
        [ColorUsage(false,true)]
        public Color lightColorTint = Color.white * 2.0f;
        public Color LightColorTint
        {
            get => lightColorTint;
            set
            {
                lightColorTint = value;
                _UpdateInstancedProperties();
            }
        }
        #endregion
        
        #region Movement Settings
        public bool invertPan;
        public bool InvertPan
        {
            get => invertPan;
            set
            {
                invertPan = value;
                _UpdateInstancedProperties();
            }
        }
        
        public bool invertTilt;
        public bool InvertTilt
        {
            get => invertTilt;
            set
            {
                invertTilt = value;
                _UpdateInstancedProperties();
            }
        }
        
        public bool isUpsideDown;
        #endregion
        
        #region Fixture Settings
        public bool enableAutoSpin = true;
        public bool ProjectionSpin
        {
            get => enableAutoSpin;
            set
            {
                enableAutoSpin = value;
                _UpdateInstancedProperties();
            }
        }
        
        public bool enableStrobe = true;
        
        [Range(0,360.0f)]
        public float tiltOffsetBlue = 90.0f;
        public float Tilt
        {
            get => tiltOffsetBlue;
            set
            {
                tiltOffsetBlue = value;
                _UpdateInstancedProperties();
            }
        }

        [Range(0,360.0f)]
        public float panOffsetBlueGreen;
        public float Pan
        {
            get => panOffsetBlueGreen;
            set
            {
                panOffsetBlueGreen = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(1,8)]
        public int selectGOBO = 1;
        public int SelectGOBO
        {
            get => selectGOBO;
            set
            {
                selectGOBO = value;
                _UpdateInstancedProperties();
            }
        }
        
        [NonReorderable]
        public MeshRenderer[] objRenderers;

        [Range(0, 5.5f)]
        public float coneWidth = 2.5f;
        public float ConeWidth
        {
            get => coneWidth;
            set
            {
                coneWidth = value;
                _UpdateInstancedProperties();
            }
        }

        [Range(0.5f, 10.0f)]
        public float coneLength = 8.5f;
        public float ConeLength
        {
            get => ConeLength;
            set
            {
                coneLength = value;
                _UpdateInstancedProperties();
            }
        }
        
        [Range(0.275f,10.0f)]
        public float maxConeLength = 1.0f;
        public float MaxConeLength
        {
            get => MaxConeLength;
            set
            {
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

        #region DMX512 Helpers
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

        /// <summary>
        /// Get global channel index based on universe and channel
        /// </summary>
        /// <param name="universe"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public int GetGlobalChannel(int universe, int channel)
        {
            return channel - 1 + ((universe - 1) * 512);
        }

        /// <summary>
        /// Get universe and channel based on global channel index
        /// </summary>
        /// <param name="globalChannelIndex"></param>
        /// <param name="universe"></param>
        /// <param name="channel"></param>
        public void GetUniverseAndChannel(int globalChannelIndex, out int universe, out int channel)
        {
            var offset = globalChannelIndex + 1;
            universe = (offset / 512) + 1;
            channel = offset % 512;
        }
        #endregion

        private MaterialPropertyBlock props;
        
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
            
            _ResetProps();
            
            if (withDmx)
            {
                _UpdateInstancedProperties();
            }
            else
            {
                _UpdateInstancedPropertiesSansDMX();
            }
        }
        
        public void _ResetProps()
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
            
            if (renderer.gameObject.name.ToLower().Contains("volume") || 
               renderer.gameObject.name.ToLower().Contains("flare"))
            {
                props.SetFloat("_FinalIntensity", finalIntensityVolumetric);
            }
            else if(renderer.gameObject.name.ToLower().Contains("project"))
            {
                props.SetFloat("_FinalIntensity", finalIntensityProjection);
            }
            else
            {
                props.SetFloat("_FinalIntensity", finalIntensityFixture);
            }
            
            return props;
        }
        
        public void _UpdateInstancedProperties() // Runtime
        {
            if(props == null)
            {
                if (objRenderers == null || objRenderers.Length == 0)
                {
                    Debug.LogError($"There are no object renderers on {gameObject.name}.");
                    return;
                }
                
                _ResetProps();
            }

            props.SetInt("_EnableDMX", enableDMXChannels ? 1 : 0);
            props.SetInt("_DMXChannel", RawDmxConversion());
            props.SetInt("_EnableDMXTranslateChannel", enableDmxTranslate ? 1 : 0);
            props.SetInt("_DMXTranslateChannel", dmxTranslateChannel);
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
        
        public void _UpdateInstancedPropertiesSansDMX() // Editor, non-playmode
        {
            if (props == null)
            {
                if (objRenderers == null || objRenderers.Length == 0)
                {
                    Debug.LogError($"There are no object renderers on {gameObject.name}.");
                    return;
                }
                
                _ResetProps();
            }
            
            props.SetInt("_EnableDMX", 0);
            props.SetInt("_DMXChannel", RawDmxConversion());
            props.SetInt("_EnableDMXTranslateChannel", enableDmxTranslate ? 1 : 0);
            props.SetInt("_DMXTranslateChannel", dmxTranslateChannel);
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