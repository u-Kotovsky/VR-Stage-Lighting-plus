#define TEXTMESHPRO

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UDONSHARP
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using static VRC.SDKBase.VRCShader;
#else
using static UnityEngine.Shader;
using UnityEngine.Rendering;
#endif

#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UDONSHARP
using UdonSharpEditor;
#endif
#endif

#if TEXTMESHPRO
using TMPro;
#endif

namespace VRSL
{    
    public enum VolumetricQualityModes
    {
        High,
        Medium,
        Low
    }
    
    public enum DefaultQualityModes
    {
        High,
        Low
    }

#if UDONSHARP
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRSL_LocalUIControlPanel : UdonSharpBehaviour
#else
    public class VRSL_LocalUIControlPanel : MonoBehaviour
#endif
    {
        public const int MaxUniverseCount = 30;
        private readonly int _universalIntensity = Shader.PropertyToID("_UniversalIntensity");
        private readonly int _mainTex = Shader.PropertyToID("_MainTex");
        private readonly int _disableStrobe = Shader.PropertyToID("_DisableStrobe");
        private readonly int _maximumSmoothnessDmx = Shader.PropertyToID("_MaximumSmoothnessDMX");
        private readonly int _minimumSmoothnessDmx = Shader.PropertyToID("_MinimumSmoothnessDMX");
        private readonly int _samplingTexture = Shader.PropertyToID("_SamplingTexture");
        private readonly int _nineUniverseMode = Shader.PropertyToID("_NineUniverseMode");
        
        public CustomRenderTexture globalChannelCrt;
        public CustomRenderTexture sourceVideoTexture;
        public Material targetDeserializerMaterial;

        #region Fixtures
        [SerializeField, HideInInspector]
        private VRStageLighting_AudioLink_Laser[] audioLinkLasers;
        [SerializeField, HideInInspector]
        private VRStageLighting_AudioLink_Static[] audiolinkLights;
        [SerializeField, HideInInspector]
        private VRStageLighting_DMX_Static[] dmxLights;
        #endregion
        
        #region Quality modes
        [Header("Quality Modes")]
        public VolumetricQualityModes volumetricQuality;
        public bool lockVolumetricQualityMode;
        [Space(5.0f)]
        public DefaultQualityModes blinderProjectionQuality;
        public bool lockBlinderProjectionQualityMode;
        [Space(5.0f)]
        public DefaultQualityModes parProjectionQuality;
        public bool lockParProjectionQualityMode;
        [Space(5.0f)]
        public DefaultQualityModes otherProjectionQuality;
        public bool lockOtherProjectionQualityMode;
        [Space(5.0f)]
        public DefaultQualityModes discoballQuality;
        public bool lockDiscoballQualityMode;
        [Space(5.0f)]
        public DefaultQualityModes lensFlareQuality;
        public bool lockLensFlareQualityMode;
        [Space(5.0f)]
        public DefaultQualityModes strobeQuality;
        #endregion
        
        [Header("Video Sampling")] // Unoptimized?
        public Texture videoSampleTargetTexture;

        #region Materials
        [Header("Materials")]
        public Material[] fixtureMaterials;
        public Material[] volumetricMaterials;
        public Material[] projectionMaterials;
        public Material[] discoBallMaterials;
        public Material[] laserMaterials;
        #endregion
        
        [Space(5)]
        [Header("Post Processing Animators")]
        public Animator bloomAnimator;
        
        #region UI elements
        [Space(5)]
        [Header("UI Sliders")]
        public Slider masterSlider;
        public Slider fixtureSlider;
        public Slider volumetricSlider;
        public Slider projectionSlider;
        public Slider discoBallSlider;
        public Slider laserSlider;
        public Slider bloomSlider;
        public Text masterSliderText, fixtureSliderText, volumetricSliderText, projectionSliderText, discoBallSliderText, laserSliderText, bloomSliderText;
#if TEXTMESHPRO
        public TextMeshProUGUI masterSliderTextPro, fixtureSliderTextPro, volumetricSliderTextPro, projectionSliderTextPro, discoBallSliderTextPro, laserSliderTextPro, bloomSliderTextPro;
#endif
        public float fixtureIntensityMax = 1.0f, volumetricIntensityMax = 1.0f, projectionIntensityMax = 1.0f, discoballIntensityMax = 1.0f, laserIntensityMax = 1.0f;
        
        public Button strobeHighButton, strobeLowButton;
        
        public Toggle volumetricNoiseToggle;
        public Button volumetricHighButton,volumetricMedButton, volumetricLowButton;
        public Text volumetricHighText, volumetricMedText, volumetricLowText;
        public Button blinderProjectionHighButton,  blinderProjectionLowButton;
        public Text blinderProjectionHighText,  blinderProjectionLowText;
        
        public Button parProjectionHighButton,  parProjectionLowButton;
        public Text parProjectionHighText,  parProjectionLowText;
        public Button otherProjectionHighButton,  otherProjectionLowButton;
        public Text otherProjectionHighText,  otherProjectionLowText;
        
        public Button discoballHighButton,  discoballLowButton;
        public Text discoballHighText,  discoballLowText;
        public Button lensFlareHighButton,  lensFlareLowButton;
        public Text lensFlareHighText, lensFlareLowText;

        public Button globalStrobeToggleButton;
        public Text globalStrobeLabel;
        #endregion
        
        public bool isUsingDMX = true;
        public bool isUsingAudioLink = true;
        
        [Space(10)]
        [Header("0 = Horizontal Mode  1 = Vertical Mode  2 = Legacy Mode")]
        [Range(0 ,2)]
        public int DMXMode;
        
        private const int HORIZONTAL_MODE = 0;
        private const int VERTICAL_MODE = 1;
        private const int LEGACY_MODE = 2;
        
        [Space(20)]
        public bool delayStrobeForGI = true;
        
        #region DMX custom render textures
        [Space(5)]
        public CustomRenderTexture[] DMX_CRTS_Horizontal;
        public CustomRenderTexture[] DMX_CRTS_Vertical;
        public CustomRenderTexture[] DMX_CRTS_Legacy;
        public CustomRenderTexture[] AudioLink_CRTs;
        #endregion

        [HideInInspector]
        public int fixtureGizmos;

        [HideInInspector]
        public float panRangeTarget = 180f; 
        [HideInInspector]
        public float tiltRangeTarget = -180f;

        [HideInInspector]
        public bool useLegacyStaticLights;
        public bool useExtendedUniverses;
        
        //public bool adjustInGameInterpolation;
        public bool sperateInGameInterpolationSpeed = true;
        public float inGameInterpolationModifier = 1.55f;
        
        public bool outputDebugLogs;

        [HideInInspector]
        public int volumetricMeshQuality;

        [HideInInspector]
        public string fixtureDefGUID = "4d88361aa1276d64d8a60009bfb590ed";

        [HideInInspector]
        public string fixtureSaveFile = "NONE";

        [HideInInspector]
        public bool useDMXGI;

        private int 
            _Udon_DMXGridRenderTexture, 
            _Udon_DMXGridRenderTextureMovement, 
            _Udon_DMXGridSpinTimer, 
            _Udon_DMXGridStrobeTimer, 
            _Udon_DMXGridStrobeOutput;

        #region Properties
        [SerializeField, FieldChangeCallback(nameof(VolumetricNoise))]
        private bool _volumetricNoise = true;
        public bool VolumetricNoise
        {
#if (UNITY_ANDROID || UNITY_IOS) && UDONSHARP
            set {
                _volumetricNoise = false;
                _CheckDepthLightStatus();
            }
            get => false;
#else
            set
            {
                _volumetricNoise = value;
                _CheckDepthLightStatus();
            }
            get => _volumetricNoise;
#endif
        }

        [SerializeField, FieldChangeCallback(nameof(RequireDepthLight))]
        private bool _requireDepthLight = true;
        public bool RequireDepthLight
        {
#if (UNITY_ANDROID || UNITY_IOS) && UDONSHARP
            set {
                _requireDepthLight = false;
                _CheckDepthLightStatus();
                _DepthLightStatusReport();
            }
            get => false;
#else
            set
            {
                _requireDepthLight = value;
                _CheckDepthLightStatus();
            }
            get => _requireDepthLight;
#endif
        }

        [SerializeField, FieldChangeCallback(nameof(GlobalDisableStrobe))]
        private bool _globalDisableStrobe = false;
        public bool GlobalDisableStrobe
        {
            set
            {
                _globalDisableStrobe = value;
                SetStrobeTextureStatus();
            }
            get => _globalDisableStrobe;
        }
        #endregion

        public float _targetCRTUpdateRate;

        public void _ToggleGlobalStrobe()
        {
            GlobalDisableStrobe = !GlobalDisableStrobe;
        }

        private void SetStrobeTextureStatus()
        {
            foreach(var renderTexture in DMX_CRTS_Legacy)
            {
                if(renderTexture == null) continue;
                if(renderTexture.name.Contains("Strobe") && renderTexture.material.HasProperty(_disableStrobe))
                {
                    renderTexture.material.SetFloat(_disableStrobe, GlobalDisableStrobe ? 1f : 0f);
                }
            }       
            foreach(var renderTexture in DMX_CRTS_Horizontal)
            {
                if(renderTexture == null) continue;
                if(renderTexture.name.Contains("Strobe") && renderTexture.material.HasProperty(_disableStrobe))
                {
                    renderTexture.material.SetFloat(_disableStrobe, GlobalDisableStrobe ? 1f : 0f);
                }
            }        
            foreach(var renderTexture in DMX_CRTS_Vertical)
            {
                if(renderTexture == null) continue;
                if(renderTexture.name.Contains("Strobe") && renderTexture.material.HasProperty(_disableStrobe))
                {
                    renderTexture.material.SetFloat(_disableStrobe, GlobalDisableStrobe ? 1f : 0f);
                }
            }
            SetGlobalStrobeUI();     
        }
        
        private void _SetTextureIDS()
        {
            _Udon_DMXGridRenderTexture = PropertyToID("_Udon_DMXGridRenderTexture");
            _Udon_DMXGridRenderTextureMovement = PropertyToID("_Udon_DMXGridRenderTextureMovement");
            _Udon_DMXGridSpinTimer = PropertyToID("_Udon_DMXGridSpinTimer");
            _Udon_DMXGridStrobeTimer = PropertyToID("_Udon_DMXGridStrobeTimer");
            _Udon_DMXGridStrobeOutput = PropertyToID("_Udon_DMXGridStrobeOutput");
        }

        private void ReduceInGameInterpolation()
        {
            if (!sperateInGameInterpolationSpeed) return;
            
            foreach(var renderTexture in DMX_CRTS_Horizontal)
            {
                if(renderTexture.material != null && renderTexture.material.name.Contains("Interpolated"))
                {
                    float max = Mathf.Clamp01(renderTexture.material.GetFloat(_maximumSmoothnessDmx));
                    float min = Mathf.Clamp01(renderTexture.material.GetFloat(_minimumSmoothnessDmx));
                    renderTexture.material.SetFloat(_maximumSmoothnessDmx, Mathf.Clamp01(max/inGameInterpolationModifier));
                    renderTexture.material.SetFloat(_minimumSmoothnessDmx, Mathf.Clamp01(min/inGameInterpolationModifier));
                }
            }
            foreach(var renderTexture in DMX_CRTS_Vertical)
            {
                if(renderTexture.material != null && renderTexture.material.name.Contains("Interpolated"))
                {
                    float max = Mathf.Clamp01(renderTexture.material.GetFloat(_maximumSmoothnessDmx));
                    float min = Mathf.Clamp01(renderTexture.material.GetFloat(_minimumSmoothnessDmx));
                    renderTexture.material.SetFloat(_maximumSmoothnessDmx, Mathf.Clamp01(max/inGameInterpolationModifier));
                    renderTexture.material.SetFloat(_minimumSmoothnessDmx, Mathf.Clamp01(min/inGameInterpolationModifier));
                }
            }
            foreach(var renderTexture in DMX_CRTS_Legacy)
            {
                if(renderTexture.material != null && renderTexture.material.name.Contains("Interpolated"))
                {
                    float max = Mathf.Clamp01(renderTexture.material.GetFloat(_maximumSmoothnessDmx));
                    float min = Mathf.Clamp01(renderTexture.material.GetFloat(_minimumSmoothnessDmx));
                    renderTexture.material.SetFloat(_maximumSmoothnessDmx, Mathf.Clamp01(max/inGameInterpolationModifier));
                    renderTexture.material.SetFloat(_minimumSmoothnessDmx, Mathf.Clamp01(min/inGameInterpolationModifier));
                }
            }
        }

        public void OnEnable() 
        {
            _CheckDepthLightStatus();
        }
        
        private void Start()
        {
            _EnsureDeserializerGotVideoCRT();
            _CheckBloomAnimator();
            _SetTextureIDS();
            _CheckDepthLightStatus();
            _SetFinalIntensity();
            _SetFixtureIntensity();
            _SetVolumetricIntensity();
            _SetProjectionIntensity();
            _SetDiscoBallIntensity();
            _SetBloomIntensity();
            _CheckDMX();
            _CheckAudioLink();
            _CheckkExtendedUniverses();
            _ForceUpdateVideoSampleTexture();
            _SetVolumetricQualityMode();
            _SetBlinderProjectionQualityMode();
            _SetParProjectionQualityMode();
            _SetOtherProjectionQualityMode();
            _SetDiscoBallQualityMode();
            _SetLensFlareQualtiyMode();
            _CheckButtonLockStatus();
            _SetStrobeHigh();

#if !UNITY_EDITOR
            ReduceInGameInterpolation();
#endif
            SetStrobeTextureStatus();
        }
        
        private void _EnsureDeserializerGotVideoCRT()
        {
            if (targetDeserializerMaterial)
            {
                if (sourceVideoTexture)
                {
                    if (targetDeserializerMaterial.HasTexture(_mainTex))
                    {
                        Debug.Log($"'{gameObject.name}': Applied texture '{sourceVideoTexture.name}' on '{targetDeserializerMaterial.name}' material");
                        targetDeserializerMaterial.SetTexture(_mainTex, sourceVideoTexture);

                        if (globalChannelCrt)
                        {
                            Debug.Log($"'{gameObject.name}': Applied material '{targetDeserializerMaterial.name}' on '{globalChannelCrt.name}' custom render texture");
                            globalChannelCrt.material = targetDeserializerMaterial;
                        }
                        else
                        {
                            Debug.LogError($"Global channel container was not found (GlobalChannelCRT) on '{gameObject.name}' please assign one.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Deserializer material on '{gameObject.name}' doesn't have texture property.");
                    }
                }
                else
                {
                    Debug.LogError($"VideoCRT on '{gameObject.name}' was not found. Please assign one.");
                }
            }
            else
            {
                Debug.LogError($"Deserializer material on '{gameObject.name}' was not found. Please assign one.");
            }
        }
        
        private void _CheckBloomAnimator()
        {
            if (bloomAnimator) return;
            var anim = GameObject.Find("PostProcessingExample-Bloom");
            if (anim) bloomAnimator = anim.GetComponent<Animator>();
        }
        
        private void _CheckButtonLockStatus()
        {
            if (lockVolumetricQualityMode)
            {
                SetButtonInteractableSafe(volumetricHighButton, false);
                SetButtonInteractableSafe(volumetricMedButton, false);
                SetButtonInteractableSafe(volumetricLowButton, false);
            }
            if (lockBlinderProjectionQualityMode)
            {
                SetButtonInteractableSafe(blinderProjectionHighButton, false);
                SetButtonInteractableSafe(blinderProjectionLowButton, false);
            }
            if (lockLensFlareQualityMode)
            {
                SetButtonInteractableSafe(lensFlareHighButton, false);
                SetButtonInteractableSafe(lensFlareLowButton, false);
            }
            if (lockParProjectionQualityMode)
            {
                SetButtonInteractableSafe(parProjectionHighButton, false);
                SetButtonInteractableSafe(parProjectionLowButton, false);
            }
            if (lockOtherProjectionQualityMode)
            {
                SetButtonInteractableSafe(otherProjectionHighButton, false);
                SetButtonInteractableSafe(otherProjectionLowButton, false);
            }
            if (lockDiscoballQualityMode)
            {
                SetButtonInteractableSafe(discoballHighButton, false);
                SetButtonInteractableSafe(discoballLowButton, false);
            }
        }
        
        #region UI Util
        private void SetGlobalStrobeUI()
        {
            globalStrobeToggleButton.gameObject.SetActive(isUsingDMX);
        }
        
        private void SetTextSafe(Text text, string value)
        {
            if (text)
            {
                text.text = value;
            }
        }

#if TEXTMESHPRO
        private void SetTextSafe(TextMeshProUGUI text, string value)
        {
            if (text)
            {
                text.text = value;
            }
        }
#endif
        private void SetButtonInteractableSafe(Button button, bool interactable)
        {
            if (button)
            {
                button.interactable = interactable;
            }
        }

        private void SetButtonInteractableSafe(Button buttonHigh, Button buttonMedium, Button buttonLow, VolumetricQualityModes quality)
        {
            SetButtonInteractableSafe(buttonHigh, quality != VolumetricQualityModes.High);
            SetButtonInteractableSafe(buttonMedium, quality != VolumetricQualityModes.Medium);
            SetButtonInteractableSafe(buttonLow, quality != VolumetricQualityModes.Low);
        }

        private void SetButtonInteractableSafe(Button buttonHigh, Button buttonLow, DefaultQualityModes quality)
        {
            SetButtonInteractableSafe(buttonHigh, quality != DefaultQualityModes.High);
            SetButtonInteractableSafe(buttonLow, quality != DefaultQualityModes.Low);
        }
        #endregion

        #region Quality buttons
        #region Volumetric Quality Buttons
        public void UpdateVolumetricButtons() // Safe update UI buttons
        {
            SetButtonInteractableSafe(volumetricHighButton, volumetricMedButton, volumetricLowButton, volumetricQuality);
        }
        
        public void _SetVolumetricHigh()
        {
            if (lockVolumetricQualityMode) return;
            volumetricQuality = VolumetricQualityModes.High;
            _SetVolumetricQualityMode();
            UpdateVolumetricButtons();
        }
        
        public void _SetVolumetricMed()
        {
            if (lockVolumetricQualityMode) return;
            volumetricQuality = VolumetricQualityModes.Medium;
            _SetVolumetricQualityMode();
            UpdateVolumetricButtons();
        }
        
        public void _SetVolumetricLow()
        {
            if (lockVolumetricQualityMode) return;
            volumetricQuality = VolumetricQualityModes.Low;
            _SetVolumetricQualityMode();
            UpdateVolumetricButtons();
        }
        #endregion
        #region Projection Blinders Quality Buttons
        public void UpdateBlindersButtons() // Safe update UI buttons
        {
            SetButtonInteractableSafe(blinderProjectionHighButton, blinderProjectionLowButton, blinderProjectionQuality);
        }
        
        public void _SetProjectionBlindersHigh()
        {
            if (lockBlinderProjectionQualityMode) return;
            blinderProjectionQuality = DefaultQualityModes.High;
            _SetBlinderProjectionQualityMode();
            UpdateBlindersButtons();
        }
        public void _SetProjectionBlindersLow()
        {
            if (lockBlinderProjectionQualityMode) return;
            blinderProjectionQuality = DefaultQualityModes.Low;
            _SetBlinderProjectionQualityMode();
            UpdateBlindersButtons();
        }
        #endregion
        #region Projection Pars Quality Buttons
        public void UpdateParsButtons() // Safe update UI buttons
        {
            SetButtonInteractableSafe(parProjectionHighButton, parProjectionLowButton, parProjectionQuality);
        }
        public void _SetProjectionParsHigh()
        {
            if (lockParProjectionQualityMode) return;
            parProjectionQuality = DefaultQualityModes.High;
            _SetParProjectionQualityMode();
            UpdateParsButtons();
        }
        public void _SetProjectionParsLow()
        {
            if (lockParProjectionQualityMode) return;
            parProjectionQuality = DefaultQualityModes.Low;
            _SetParProjectionQualityMode();
            UpdateParsButtons();
        }
        #endregion
        #region Projection Other Quality Buttons
        public void UpdateOtherButtons() // Safe update UI buttons
        {
            SetButtonInteractableSafe(otherProjectionHighButton, otherProjectionLowButton, otherProjectionQuality);
        }
        public void _SetProjectionOtherHigh()
        {
            if (lockOtherProjectionQualityMode) return;
            otherProjectionQuality = DefaultQualityModes.High;
            _SetOtherProjectionQualityMode();
            UpdateOtherButtons();
        }
        public void _SetProjectionOtherLow()
        {
            if (lockOtherProjectionQualityMode) return;
            otherProjectionQuality = DefaultQualityModes.Low;
            _SetOtherProjectionQualityMode();
            UpdateOtherButtons();
        }
        #endregion
        #region Discoball Quality Buttons
        public void UpdateDiscoButtons() // Safe update UI buttons
        {   
            SetButtonInteractableSafe(discoballHighButton, discoballLowButton, discoballQuality);
        }
        public void _SetDiscoballHigh()
        {
            if (lockDiscoballQualityMode) return;
            discoballQuality = DefaultQualityModes.High;
            _SetDiscoBallQualityMode();
            UpdateDiscoButtons();
        }
        public void _SetDiscoballLow()
        {
            if (lockDiscoballQualityMode) return;
            discoballQuality = DefaultQualityModes.Low;
            _SetDiscoBallQualityMode();
            UpdateDiscoButtons();
        }
        #endregion
        #region Lens Flare Quality Buttons
        public void UpdateLensFlareButtons() // Safe update UI buttons
        {   
            SetButtonInteractableSafe(lensFlareHighButton, lensFlareLowButton, lensFlareQuality);
        }

        public void _SetLensFlareHigh()
        {
            if (lockLensFlareQualityMode) return;
            lensFlareQuality = DefaultQualityModes.High;
            _SetLensFlareQualtiyMode();
            UpdateLensFlareButtons();
        }

        public void _SetLensFlareLow()
        {
            if (lockLensFlareQualityMode) return;
            lensFlareQuality = DefaultQualityModes.Low;
            _SetLensFlareQualtiyMode();
            UpdateLensFlareButtons();
        }
        #endregion
        #region Strobe Quality Buttons
        public void UpdateStrobeButtons() // Safe update UI buttons
        {
            SetButtonInteractableSafe(strobeHighButton, strobeLowButton, strobeQuality);
        }

        public void _SetStrobeHigh()
        {
            //if (lockLensFlareQualityMode) return;
            strobeQuality = DefaultQualityModes.High;
            _SetStrobeQualtiyMode();
            UpdateStrobeButtons();
        }

        public void _SetStrobeLow()
        {
            //if (lockLensFlareQualityMode) return;
            strobeQuality = DefaultQualityModes.Low;
            _SetStrobeQualtiyMode();
            UpdateStrobeButtons();
        }
        #endregion
        #endregion

        public void _UpdateAllQualityModes()
        {
            _SetDiscoBallQualityMode();
            _SetVolumetricQualityMode();
            _SetParProjectionQualityMode();
            _SetOtherProjectionQualityMode();
            _SetBlinderProjectionQualityMode();
            _SetLensFlareQualtiyMode();
        }
        
        #region Set Quality Mode Methods
        public void _SetVolumetricQualityMode()
        {
            SetVolumetricQuality();
        }

        public void _SetBlinderProjectionQualityMode()
        {
            SetBlinderProjectionQuality();
        }

        public void _SetParProjectionQualityMode()
        {
            SetParProjectionQuality();
        }

        public void _SetOtherProjectionQualityMode()
        {
            SetOtherProjectionQuality();
        }
        public void _SetDiscoBallQualityMode()
        {
            SetDiscoballQuality();
        }

        public void _SetLensFlareQualtiyMode()
        {
            SetLensFlareQuality();
        }
        public void _SetStrobeQualtiyMode()
        {
            SetStrobeQuality();
        }
        
        private void SetStrobeQuality()
        {
            GlobalDisableStrobe = strobeQuality == DefaultQualityModes.Low;
        }
        #endregion

        public void _CheckDepthLightStatus()
        {
            foreach(var material in volumetricMaterials)
            {
                material.SetInt("_PotatoMode", VolumetricNoise ? 0 : 1);
                material.SetInt("_UseDepthLight", RequireDepthLight ? 1 : 0);
                if(material.HasProperty("_UseDepthLight"))
                {
                    SetKeyword(material, "_USE_DEPTH_LIGHT", Mathf.FloorToInt(material.GetInt("_UseDepthLight")) == 1);
                }
                if(material.HasProperty("_MAGIC_NOISE_ON_MED"))
                {
                    SetKeyword(material, "_MAGIC_NOISE_ON_MED", Mathf.FloorToInt(material.GetInt("_MAGIC_NOISE_ON_MED")) == 1);
                }
                if(material.HasProperty("_MAGIC_NOISE_ON_HIGH"))
                {
                    SetKeyword(material, "_MAGIC_NOISE_ON_HIGH", Mathf.FloorToInt(material.GetInt("_MAGIC_NOISE_ON_HIGH")) == 1);
                }
                if(material.HasProperty("_PotatoMode"))
                {
                    SetKeyword(material, "_POTATO_MODE_ON", Mathf.FloorToInt(material.GetInt("_PotatoMode")) == 1);
                }
            }
            foreach(var mat in projectionMaterials)
            {
                mat.SetInt("_UseDepthLight", RequireDepthLight ? 1 : 0);
            }
            if(fixtureMaterials != null)
            {
                foreach(var mat in fixtureMaterials)
                {
                    if(mat)
                    {
                        mat.SetInt("_UseDepthLight", RequireDepthLight ? 1 : 0);
                        if(mat.HasProperty("_UseDepthLight"))
                        {
                            SetKeyword(mat, "_USE_DEPTH_LIGHT", Mathf.FloorToInt(mat.GetInt("_UseDepthLight")) == 1 ? true : false);
                        }
                    }
                }
            }
        }
        
        #region CustomRenderTexture methods
        private void EnableCRTS(CustomRenderTexture[] customRenderTextures)
        {
            foreach(var customRenderTexture in customRenderTextures)
            {
                customRenderTexture.updateMode = CustomRenderTextureUpdateMode.Realtime;
#if UNITY_2022
                customRenderTexture.updatePeriod = _targetCRTUpdateRate;
#endif
                if(customRenderTexture.name.Contains("Color"))
                {
                    if(outputDebugLogs)
                    {
                        Debug.Log("DMX Color: " + customRenderTexture.name);
                    }
#if UDONSHARP
                    VRCShader.SetGlobalTexture(_Udon_DMXGridRenderTexture, customRenderTexture);
#else
                    Shader.SetGlobalTexture(_Udon_DMXGridRenderTexture, rt, RenderTextureSubElement.Default);
#endif
                }
                else if(customRenderTexture.name.Contains("Movement"))
                {
                    if(outputDebugLogs)
                    {
                        Debug.Log("DMX Movement: " + customRenderTexture.name);
                    }
#if UDONSHARP
                    VRCShader.SetGlobalTexture(_Udon_DMXGridRenderTextureMovement, customRenderTexture);
#else
                    Shader.SetGlobalTexture(_Udon_DMXGridRenderTextureMovement, rt, RenderTextureSubElement.Default);
#endif
                }
                else if(customRenderTexture.name.Contains("Spin"))
                {
                    if(outputDebugLogs)
                    {
                        Debug.Log("DMX Spin Timings: " + customRenderTexture.name);
                    }
#if UDONSHARP
                    VRCShader.SetGlobalTexture(_Udon_DMXGridSpinTimer, customRenderTexture);
#else
                    Shader.SetGlobalTexture(_Udon_DMXGridSpinTimer, rt, RenderTextureSubElement.Default);
#endif
                }
                else if(customRenderTexture.name.Contains("Strobe"))
                {
                    if(customRenderTexture.name.Contains("Timings"))
                    {
                        if(outputDebugLogs)
                        {
                            Debug.Log("DMX Strobe Timings: " + customRenderTexture.name);
                            
                        }
#if UDONSHARP
                        VRCShader.SetGlobalTexture(_Udon_DMXGridStrobeTimer, customRenderTexture);
#else
                        Shader.SetGlobalTexture(_Udon_DMXGridStrobeTimer, rt, RenderTextureSubElement.Default);
#endif
                    }
                    else
                    {
                        //Debug.Log("Setting Strobe Output");
                        if(delayStrobeForGI)
                        {
                            if(customRenderTexture.name.Contains("Delay-Final") && DMXMode != LEGACY_MODE)
                            {
#if UDONSHARP
                                if(outputDebugLogs)
                                {
                                    Debug.Log("DMX Strobe Output: " + customRenderTexture.name);
                                }
                                VRCShader.SetGlobalTexture(_Udon_DMXGridStrobeOutput, customRenderTexture);
#else
                                Shader.SetGlobalTexture(_Udon_DMXGridStrobeOutput, rt, RenderTextureSubElement.Default);
#endif
                            }
                            else if(DMXMode == LEGACY_MODE)
                            {
#if UDONSHARP
                                if(outputDebugLogs)
                                {
                                    Debug.Log("DMX Strobe Output: " + customRenderTexture.name);
                                }
                                VRCShader.SetGlobalTexture(_Udon_DMXGridStrobeOutput, customRenderTexture);
#else
                                Shader.SetGlobalTexture(_Udon_DMXGridStrobeOutput, rt, RenderTextureSubElement.Default);
#endif  
                            }
                        }
                        else
                        {
                            if(customRenderTexture.name.Contains("Delay") == false)
                            {           
#if UDONSHARP
                                if(outputDebugLogs)
                                {
                                    Debug.Log("Strobe Output: " + customRenderTexture.name);
                                }
                                VRCShader.SetGlobalTexture(_Udon_DMXGridStrobeOutput, customRenderTexture);
#else
                                Shader.SetGlobalTexture(_Udon_DMXGridStrobeOutput, rt, RenderTextureSubElement.Default);
#endif
                            }
                        }
                    }
                }
            }
        }
        
        private void DisableCRTS(CustomRenderTexture[] customRenderTextures)
        {
            foreach(var customRenderTexture in customRenderTextures)
            {
                customRenderTexture.updateMode = CustomRenderTextureUpdateMode.OnDemand;
            }
        }
        
        public void _CheckDMX()
        {
            if(isUsingDMX)
            {
                switch(DMXMode)
                {
                    case HORIZONTAL_MODE:
                        EnableCRTS(DMX_CRTS_Horizontal);
                        DisableCRTS(DMX_CRTS_Vertical);
                        DisableCRTS(DMX_CRTS_Legacy);
                        break;
                    case VERTICAL_MODE:
                        DisableCRTS(DMX_CRTS_Horizontal);
                        EnableCRTS(DMX_CRTS_Vertical);
                        DisableCRTS(DMX_CRTS_Legacy);
                        break;
                    case LEGACY_MODE:
                        DisableCRTS(DMX_CRTS_Horizontal);
                        DisableCRTS(DMX_CRTS_Vertical);
                        EnableCRTS(DMX_CRTS_Legacy);
                        break;
                    default:
                        DisableCRTS(DMX_CRTS_Horizontal);
                        DisableCRTS(DMX_CRTS_Vertical);
                        DisableCRTS(DMX_CRTS_Legacy);
                        break;
                }
            }
            else
            {
                DisableCRTS(DMX_CRTS_Horizontal);
                DisableCRTS(DMX_CRTS_Vertical);
                DisableCRTS(DMX_CRTS_Legacy);
            }
        }

        public void _SetDMXHorizontal()
        {
            if(isUsingDMX)
            {
                DMXMode = HORIZONTAL_MODE;
                _CheckDMX();
            }
        }
        
        public void _SetDMXVertical()
        {
            if(isUsingDMX)
            {
                DMXMode = VERTICAL_MODE;
                _CheckDMX();
            }
        }
        
        public void _SetDMXLegacy()
        {
            if(isUsingDMX)
            {
                DMXMode = LEGACY_MODE;
                _CheckDMX();
            }
        }
        
        public void _CheckAudioLink()
        {
            if(isUsingAudioLink)
            {
                EnableCRTS(AudioLink_CRTs);
            }
            else
            {
                DisableCRTS(AudioLink_CRTs);
            }
        }
        #endregion
        
        public void _CheckkExtendedUniverses()
        {
            foreach(var customRenderTexture in DMX_CRTS_Horizontal)
            {
                customRenderTexture.material.SetInt(_nineUniverseMode, 0);
            }
            foreach(var customRenderTexture in DMX_CRTS_Vertical)
            {
                customRenderTexture.material.SetInt(_nineUniverseMode, 0);
            }
        }

        private void ApplyIfPropertyExists(Material material, int nameId, Texture texture)
        {
            if (material.HasProperty(nameId))
            {
                material.SetTexture(nameId, texture);
            }
        }

        public void _ForceUpdateVideoSampleTexture()
        {
            if(videoSampleTargetTexture == null)
            {
                return;
            }
            
            foreach(var material in laserMaterials)
            {
                ApplyIfPropertyExists(material, _samplingTexture, videoSampleTargetTexture);
            }
            foreach(var material in fixtureMaterials)
            {
                ApplyIfPropertyExists(material, _samplingTexture, videoSampleTargetTexture);
            }
            foreach(var material in discoBallMaterials)
            {
                ApplyIfPropertyExists(material, _samplingTexture, videoSampleTargetTexture);
            }
            foreach(var material in projectionMaterials)
            {
                ApplyIfPropertyExists(material, _samplingTexture, videoSampleTargetTexture);
            }
            foreach(var material in volumetricMaterials)
            {
                ApplyIfPropertyExists(material, _samplingTexture, videoSampleTargetTexture);
            }
        }
        
        public void _SetFinalIntensity()
        {
            fixtureIntensityMax = masterSlider.value;
            volumetricIntensityMax = masterSlider.value;
            projectionIntensityMax = masterSlider.value;
            discoballIntensityMax = masterSlider.value;
            laserIntensityMax = masterSlider.value;
            _SetFixtureIntensity();
            _SetVolumetricIntensity();
            _SetProjectionIntensity();
            _SetDiscoBallIntensity();
            _SetLaserIntensity();
            var text = Mathf.Round(masterSlider.value * 100.0f).ToString();
            SetTextSafe(masterSliderText, text);
            SetTextSafe(masterSliderTextPro, text);
        }

        public void _SetFixtureIntensity()
        {
            foreach(var material in fixtureMaterials)
            {
                if(material != null)
                {
                    material.SetFloat(_universalIntensity, Mathf.Lerp(0.0f, fixtureIntensityMax, fixtureSlider.value));
                }
            }
            var text = Mathf.Round(fixtureSlider.value * 100.0f).ToString();
            SetTextSafe(fixtureSliderText, text);
            SetTextSafe(fixtureSliderTextPro, text);
        }

        public void _SetVolumetricIntensity()
        {
            foreach(var material in volumetricMaterials)
            {
                if(material != null)
                {
                    material.SetFloat(_universalIntensity, Mathf.Lerp(0.0f, volumetricIntensityMax, volumetricSlider.value));
                }
            }
            var text = Mathf.Round(volumetricSlider.value * 100.0f).ToString();
            SetTextSafe(volumetricSliderText, text);
            SetTextSafe(volumetricSliderTextPro, text);
        }

        public void _SetProjectionIntensity()
        {
            foreach(var material in projectionMaterials)
            {
                if(material != null)
                {
                    material.SetFloat(_universalIntensity, Mathf.Lerp(0.0f, projectionIntensityMax, projectionSlider.value));
                }
            }
            var text = Mathf.Round(projectionSlider.value * 100.0f).ToString();
            SetTextSafe(projectionSliderText, text);
            SetTextSafe(projectionSliderTextPro, text);
        }

        public void _SetDiscoBallIntensity()
        {
            foreach(var material in discoBallMaterials)
            {
                if(material != null)
                {
                    material.SetFloat(_universalIntensity, Mathf.Lerp(0.0f, discoballIntensityMax, discoBallSlider.value));
                }
            }
            var text = Mathf.Round(discoBallSlider.value * 100.0f).ToString();
            SetTextSafe(discoBallSliderText, text);
            SetTextSafe(discoBallSliderTextPro, text);
        }

        public void _SetLaserIntensity()
        {
            foreach(var material in laserMaterials)
            {
                if(material != null)
                {
                    material.SetFloat(_universalIntensity, Mathf.Lerp(0.0f, laserIntensityMax, laserSlider.value));
                }
            }
            var text = Mathf.Round(laserSlider.value * 100.0f).ToString();
            SetTextSafe(laserSliderText, text);
            SetTextSafe(laserSliderTextPro, text);
        }
        
        public void _SetBloomIntensity()
        {
            if(bloomAnimator != null)
            {
                bloomAnimator.SetFloat("BloomIntensity", bloomSlider.value);
                var text = Mathf.Round(bloomSlider.value * 100.0f).ToString();
                SetTextSafe(bloomSliderText, text);
                SetTextSafe(bloomSliderTextPro, text);
            }
            else
            {
                bloomSlider.gameObject.SetActive(false);
            }
        }

        private void SetKeyword(Material mat, string keyword, bool status)
        {
            if (status)
            {
                mat.EnableKeyword(keyword);
            } 
            else 
            {
                mat.DisableKeyword(keyword);
            }
        }

        private void SetVolumetricQuality()
        {
            foreach(var material in volumetricMaterials)
            {
                if(material == null) continue;
                if(volumetricQuality == VolumetricQualityModes.High)
                {
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.DisableKeyword("_ALPHATEST_ON");  
                    //target.SetInt("_BlendSrc", 1);
                    material.SetInt("_BlendDst", 1);
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_AlphaToCoverage", 0);
                    material.SetInt("_HQMode", 1);
                    material.SetInt("_RenderMode", 0);
                    if(material.HasProperty("_MAGIC_NOISE_ON_MED"))
                    {
                        SetKeyword(material, "_MAGIC_NOISE_ON_MED", (Mathf.FloorToInt(material.GetInt("_MAGIC_NOISE_ON_MED"))) == 1 ? true : false);
                    }
                    if(material.HasProperty("_MAGIC_NOISE_ON_HIGH"))
                    {
                        SetKeyword(material, "_MAGIC_NOISE_ON_HIGH", (Mathf.FloorToInt(material.GetInt("_MAGIC_NOISE_ON_HIGH"))) == 1 ? true : false);
                    }
                    if(material.HasProperty("_UseDepthLight"))
                    {
                        SetKeyword(material, "_USE_DEPTH_LIGHT", (Mathf.FloorToInt(material.GetInt("_UseDepthLight"))) == 1 ? true : false);
                    }
                    if(material.HasProperty("_PotatoMode"))
                    {
                        SetKeyword(material, "_POTATO_MODE_ON", (Mathf.FloorToInt(material.GetInt("_PotatoMode"))) == 1 ? true : false);
                    }
                    if(material.HasProperty("_HQMode"))
                    {
                        SetKeyword(material, "_HQ_MODE", (Mathf.FloorToInt(material.GetInt("_HQMode"))) == 1 ? true : false);
                    }
                    //SetKeyword(target, "_2D_NOISE_ON", (Mathf.FloorToInt(target.GetInt("_2D_NOISE_ON"))) == 1 ? true : false);
                    material.renderQueue = 3002;
                }
                else if(volumetricQuality == VolumetricQualityModes.Medium) 
                {
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.DisableKeyword("_ALPHATEST_ON");  
                    //target.SetInt("_BlendSrc", 1);
                    material.SetInt("_BlendDst", 1);
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_AlphaToCoverage", 0);
                    material.SetInt("_HQMode", 0);
                    material.SetInt("_RenderMode", 1);
                    if(material.HasProperty("_MAGIC_NOISE_ON_MED"))
                    {
                        SetKeyword(material, "_MAGIC_NOISE_ON_MED", (Mathf.FloorToInt(material.GetInt("_MAGIC_NOISE_ON_MED"))) == 1);
                    }
                    if(material.HasProperty("_MAGIC_NOISE_ON_HIGH"))
                    {
                        SetKeyword(material, "_MAGIC_NOISE_ON_HIGH", (Mathf.FloorToInt(material.GetInt("_MAGIC_NOISE_ON_HIGH"))) == 1);
                    }
                    if(material.HasProperty("_UseDepthLight"))
                    {
                        SetKeyword(material, "_USE_DEPTH_LIGHT", (Mathf.FloorToInt(material.GetInt("_UseDepthLight"))) == 1);
                    }
                    if(material.HasProperty("_PotatoMode"))
                    {
                        SetKeyword(material, "_POTATO_MODE_ON", (Mathf.FloorToInt(material.GetInt("_PotatoMode"))) == 1);
                    }
                    if(material.HasProperty("_HQMode"))
                    {
                        SetKeyword(material, "_HQ_MODE", (Mathf.FloorToInt(material.GetInt("_HQMode"))) == 1);
                    }
                    //SetKeyword(target, "_2D_NOISE_ON", (Mathf.FloorToInt(target.GetInt("_2D_NOISE_ON"))) == 1 ? true : false);
                    material.renderQueue = 3002;
                }
                else
                {
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.EnableKeyword("_ALPHATEST_ON");
                    //target.SetInt("_BlendSrc", 0);
                    material.SetInt("_BlendDst", 0);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_AlphaToCoverage", 1);
                    material.SetInt("_HQMode", 0);
                    material.SetInt("_RenderMode", 2);
                    if(material.HasProperty("_MAGIC_NOISE_ON_MED"))
                    {
                        SetKeyword(material, "_MAGIC_NOISE_ON_MED", (Mathf.FloorToInt(material.GetInt("_MAGIC_NOISE_ON_MED"))) == 1);
                    }
                    if(material.HasProperty("_MAGIC_NOISE_ON_HIGH"))
                    {
                        SetKeyword(material, "_MAGIC_NOISE_ON_HIGH", (Mathf.FloorToInt(material.GetInt("_MAGIC_NOISE_ON_HIGH"))) == 1);
                    }
                    if(material.HasProperty("_UseDepthLight"))
                    {
                        SetKeyword(material, "_USE_DEPTH_LIGHT", (Mathf.FloorToInt(material.GetInt("_UseDepthLight"))) == 1);
                    }
                    if(material.HasProperty("_PotatoMode"))
                    {
                        SetKeyword(material, "_POTATO_MODE_ON", (Mathf.FloorToInt(material.GetInt("_PotatoMode"))) == 1);
                    }
                    if(material.HasProperty("_HQMode"))
                    {
                        SetKeyword(material, "_HQ_MODE", (Mathf.FloorToInt(material.GetInt("_HQMode"))) == 1);
                        
                    }
                    //SetKeyword(target, "_2D_NOISE_ON", (Mathf.FloorToInt(target.GetInt("_2D_NOISE_ON"))) == 1 ? true : false);
                    material.renderQueue = 2452;
                }
            }
        }
        
        private void SetBlinderProjectionQuality()
        {
            foreach(var material in projectionMaterials)
            {
                if(material == null) continue;
                if(material.name.Contains("Blinder"))
                {
                    if(blinderProjectionQuality == DefaultQualityModes.High) 
                    {
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.DisableKeyword("_ALPHATEST_ON");  
                        //target.SetInt("_BlendSrc", 1);
                        material.SetInt("_BlendDst", 1);
                        material.SetInt("_ZWrite", 0);
                        material.SetInt("_AlphaToCoverage", 0);
                        material.SetInt("_RenderMode", 1);
                        material.renderQueue = 3001;
                    }
                    else
                    {
                        material.SetOverrideTag("RenderType", "Opaque");
                        material.EnableKeyword("_ALPHATEST_ON");
                        //target.SetInt("_BlendSrc", 0);
                        material.SetInt("_BlendDst", 0);
                        material.SetInt("_ZWrite", 1);
                        material.SetInt("_AlphaToCoverage", 1);
                        material.SetInt("_RenderMode", 2);
                        material.renderQueue = 2451;
                    }
                }
            }
        }
        
        private void SetParProjectionQuality()
        {
            foreach(var material in projectionMaterials)
            {
                if(material == null) continue;
                if(material.name.Contains("Par"))
                {
                    if(parProjectionQuality == DefaultQualityModes.High) 
                    {
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.DisableKeyword("_ALPHATEST_ON");  
                        //target.SetInt("_BlendSrc", 1);
                        material.SetInt("_BlendDst", 1);
                        material.SetInt("_ZWrite", 0);
                        material.SetInt("_AlphaToCoverage", 0);
                        material.SetInt("_RenderMode", 1);
                        material.renderQueue = 3001;
                    }
                    else
                    {
                        material.SetOverrideTag("RenderType", "Opaque");
                        material.EnableKeyword("_ALPHATEST_ON");
                        //target.SetInt("_BlendSrc", 0);
                        material.SetInt("_BlendDst", 0);
                        material.SetInt("_ZWrite", 1);
                        material.SetInt("_AlphaToCoverage", 1);
                        material.SetInt("_RenderMode", 2);
                        material.renderQueue = 2451;
                    }
                }
            }
        }
        
        private void SetOtherProjectionQuality()
        {
            foreach(var material in projectionMaterials)
            {
                if(material == null) continue;
                if(material.name.Contains("Par") == false && material.name.Contains("Blinder")==false)
                {
                    if(otherProjectionQuality == DefaultQualityModes.High) 
                    {
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.DisableKeyword("_ALPHATEST_ON");  
                        //target.SetInt("_BlendSrc", 1);
                        material.SetInt("_BlendDst", 1);
                        material.SetInt("_ZWrite", 0);
                        material.SetInt("_AlphaToCoverage", 0);
                        material.SetInt("_RenderMode", 1);
                        material.renderQueue = 3001;
                    }
                    else
                    {
                        material.SetOverrideTag("RenderType", "Opaque");
                        material.EnableKeyword("_ALPHATEST_ON");
                        //target.SetInt("_BlendSrc", 0);
                        material.SetInt("_BlendDst", 0);
                        material.SetInt("_ZWrite", 1);
                        material.SetInt("_AlphaToCoverage", 1);
                        material.SetInt("_RenderMode", 2);
                        material.renderQueue = 2451;
                    }
                }
            }
        }

        private void SetDiscoballQuality()
        {
            foreach(var material in discoBallMaterials)
            {
                if(material == null) continue;
                if(discoballQuality == DefaultQualityModes.High) 
                {
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.DisableKeyword("_ALPHATEST_ON");  
                    //target.SetInt("_BlendSrc", 1);
                    material.SetInt("_BlendDst", 1);
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_AlphaToCoverage", 0);
                    material.SetInt("_RenderMode", 1);
                    material.renderQueue = 3001;
                }
                else
                {
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.EnableKeyword("_ALPHATEST_ON");
                    //target.SetInt("_BlendSrc", 0);
                    material.SetInt("_BlendDst", 0);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_AlphaToCoverage", 1);
                    material.SetInt("_RenderMode", 2);
                    material.renderQueue = 2451;
                }
            }
        }
        
        private void SetLensFlareQuality()
        {
            foreach(var material in fixtureMaterials)
            {
                if(material == null) continue;
                if(material.name.Contains("Flare"))
                {
                    if(lensFlareQuality == DefaultQualityModes.High) 
                    {
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.DisableKeyword("_ALPHATEST_ON");  
                        //target.SetInt("_BlendSrc", 1);
                        material.SetInt("_BlendDst", 1);
                        material.SetInt("_ZWrite", 0);
                        material.SetInt("_AlphaToCoverage", 0);
                        material.SetInt("_RenderMode", 1);
                        material.renderQueue = 3001;
                    }
                    else
                    {
                        material.SetOverrideTag("RenderType", "Opaque");
                        material.EnableKeyword("_ALPHATEST_ON");
                        //target.SetInt("_BlendSrc", 0);
                        material.SetInt("_BlendDst", 0);
                        material.SetInt("_ZWrite", 1);
                        material.SetInt("_AlphaToCoverage", 1);
                        material.SetInt("_RenderMode", 2);
                        material.renderQueue = 2451;
                    }
                }
            }
        }
    
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private static List<GameObject> GetAllObjectsOnlyInScene()
        {
            var objectsInScene = new List<GameObject>();

            foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (!EditorUtility.IsPersistent(obj.transform.root.gameObject) && !(obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave))
                    objectsInScene.Add(obj);
            }

            return objectsInScene;
        }

        public void _GetNewMaterials()
        {
            var sceneObjects = GetAllObjectsOnlyInScene();
            var freshFixtureMats = new List<Material>();
            freshFixtureMats.AddRange(fixtureMaterials);
            var freshVolumetricMats = new List<Material>();
            freshVolumetricMats.AddRange(volumetricMaterials);
            var freshProjectionMats = new List<Material>();
            freshProjectionMats.AddRange(projectionMaterials);
            
            foreach(var obj in sceneObjects)
            {
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                if(meshRenderer != null)
                {
                    if(obj.name.Contains("Fixture") && (obj.name.Contains("Lamp") || obj.name.Contains("Mesh")))
                    {
                        if(!freshFixtureMats.Contains(meshRenderer.sharedMaterial) && ((meshRenderer.sharedMaterial.shader.FindPropertyIndex("_Band") != -1) || meshRenderer.sharedMaterial.shader.FindPropertyIndex("_DMXChannel") != -1))
                        {
                            freshFixtureMats.Add(meshRenderer.sharedMaterial);
                        }
                    }
                    else if(obj.name.Contains("Volumetric") && ((meshRenderer.sharedMaterial.shader.FindPropertyIndex("_Band") != -1) || meshRenderer.sharedMaterial.shader.FindPropertyIndex("_DMXChannel") != -1))
                    {
                        if(!freshVolumetricMats.Contains(meshRenderer.sharedMaterial))
                        {
                            freshVolumetricMats.Add(meshRenderer.sharedMaterial);
                        }
                    }
                    else if (obj.name.Contains("Projection") && ((meshRenderer.sharedMaterial.shader.FindPropertyIndex("_Band") != -1) || meshRenderer.sharedMaterial.shader.FindPropertyIndex("_DMXChannel") != -1))
                    {
                        if(!freshProjectionMats.Contains(meshRenderer.sharedMaterial))
                        {
                            freshProjectionMats.Add(meshRenderer.sharedMaterial);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            fixtureMaterials = freshFixtureMats.ToArray();
            volumetricMaterials = freshVolumetricMats.ToArray();
            projectionMaterials = freshProjectionMats.ToArray();
            if(PrefabUtility.IsPartOfAnyPrefab(this))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
        }
#endif
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(VRSL_LocalUIControlPanel))]
    public class VRSL_LocalUIControlPanel_Editor : Editor
    {
        public static Texture logo;
        //public static string ver = "VR Stage Lighting ver:" + " <b><color=#6a15ce> 2.1</color></b>";
        SerializedProperty audioLinkLasers, audiolinkLights, dmxLights, isUsingDMX,isUsingAudioLink, fixtureDefGUID, volumetricMeshQuality;

        static string GetVersion()
        {
            var path = Application.dataPath;
            path = path.Replace("Assets","");
            path += "Packages"  + "\\" + "com.acchosen.vr-stage-lighting" + "\\" + "Runtime" + "\\"  + "VERSION.txt";

            var reader = new StreamReader(path); 
            var versionNum = reader.ReadToEnd();
            var ver = "VR Stage Lighting ver:" + " <b><color=#b33cff>" + versionNum + "</color></b>";
            return ver;
        }

        public void OnEnable() 
        {
            logo = Resources.Load("VRStageLighting-Logo") as Texture;
            audioLinkLasers = serializedObject.FindProperty("audioLinkLasers");
            audiolinkLights = serializedObject.FindProperty("audiolinkLights");
            dmxLights = serializedObject.FindProperty("dmxLights");
            isUsingDMX = serializedObject.FindProperty("isUsingDMX");
            isUsingAudioLink = serializedObject.FindProperty("isUsingAudioLink");
            fixtureDefGUID = serializedObject.FindProperty("fixtureDefGUID");
            volumetricMeshQuality = serializedObject.FindProperty("volumetricMeshQuality");
        }
        
        public void _RemoveEmptyMaterials()
        {
            var controlPanel = (VRSL_LocalUIControlPanel)target;
            var count = 0;
            for(int i = 0; i < controlPanel.fixtureMaterials.Length; i++)
            {
                if(controlPanel.fixtureMaterials[i] == null)
                {
                    count++;
                }
            }
            
            var newArray = new Material[controlPanel.fixtureMaterials.Length - count];
            var otherCount = 0;
            for(int i = 0; i < controlPanel.fixtureMaterials.Length; i++)
            {
                if(controlPanel.fixtureMaterials[i] != null)
                {
                    newArray[otherCount] = controlPanel.fixtureMaterials[i];
                    otherCount++;
                }
            }
            controlPanel.fixtureMaterials = newArray;
        }
        
        public static void DrawLogo()
        {
            //GUILayout.BeginArea(new Rect(0,0, Screen.width, Screen.height));
            //GUILayout.FlexibleSpace();
            //GUI.DrawTexture(pos,logo,ScaleMode.ScaleToFit);
            //EditorGUI.DrawPreviewTexture(new Rect(0,0,400,150), logo);
            Vector2 contentOffset = new Vector2(0f, -2f);
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fixedHeight = 150;
            //style.fixedWidth = 300;
            style.contentOffset = contentOffset;
            style.alignment = TextAnchor.MiddleCenter;
            var rect = GUILayoutUtility.GetRect(300f, 140f, style);
            //GUILayout.Label(logo,style, GUILayout.MaxWidth(500), GUILayout.MaxHeight(200));
            GUI.Box(rect, logo,style);
            //GUILayout.Label(logo);
            //GUILayout.FlexibleSpace();
            //GUILayout.EndArea();
        }
        
        private static Rect DrawShurikenCenteredTitle(string title, Vector2 contentOffset, int headerHeight)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fontSize = 14;
            style.fixedHeight = headerHeight;
            style.contentOffset = contentOffset;
            style.alignment = TextAnchor.MiddleCenter;
            var rect = GUILayoutUtility.GetRect(16f, headerHeight, style);

            GUI.Box(rect, title, style);
            return rect;
        }
        
        public static void ShurikenHeaderCentered(string title)
        {
            DrawShurikenCenteredTitle(title, new Vector2(0f, -2f), 22);
        }
        
        private GUIContent Label(string label)
        {
            var content = new GUIContent();
            content.text = label;
            return content;
        }
        
        public override void OnInspectorGUI()
        {
#if UDONSHARP
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
#endif
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            DrawLogo();
            ShurikenHeaderCentered(GetVersion());
            EditorGUILayout.Space();
            VRSL_LocalUIControlPanel controlPanel = (VRSL_LocalUIControlPanel)target;
            if (GUILayout.Button(new GUIContent("Force Update Target AudioLink Sample Texture",
                    "Updates all AudioLink VRSL Fixtures to sample from the selected target texture when texture sampling is enabled on the fixture.")))
            {
                controlPanel._ForceUpdateVideoSampleTexture();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Apply Quality Modes to All Materials",
                    "Applies currently set quality modes to all materials.")))
            {
                controlPanel._UpdateAllQualityModes();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Search For VRSL Materials",
                    "Adds VRSL Compatible Materials in scene to materials lists")))
            {
                controlPanel._GetNewMaterials();
            }
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Remove Empty Materials",
                    "Removes all Empty Material slots from material lists.")))
            {
                _RemoveEmptyMaterials();
            }
            EditorGUILayout.Space();
            if(isUsingDMX.boolValue)
            {
                //EditorGUILayout.PropertyField(dmxLights,true);
                for(int i = 0; i < dmxLights.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(dmxLights.GetArrayElementAtIndex(i));
                }
            }
            if(isUsingAudioLink.boolValue)
            {
                for(int i = 0; i < dmxLights.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(audiolinkLights.GetArrayElementAtIndex(i));
                    EditorGUILayout.PropertyField(audioLinkLasers.GetArrayElementAtIndex(i));
                    //EditorGUILayout.PropertyField(audiolinkLights, true);
                    //EditorGUILayout.PropertyField(audioLinkLasers,true);
                }
            }
            EditorGUILayout.LabelField("Fixture Definition GUID: " + fixtureDefGUID.stringValue);
            EditorGUILayout.LabelField("Volumetric Mesh Quality State: " + volumetricMeshQuality.intValue);
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                //Debug.Log("Found changes");
                serializedObject.ApplyModifiedProperties();
                Repaint();
            }
        }
    }
    #endif
}