
using System;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR && !COMPILER_UDONSHARP
namespace VRSL.EditorScripts
{
    // class AudioLinkLaserListItem
// {
//     public VRStageLighting_AudioLink_Laser laser;
//     public bool foldout;

//     public AudioLinkLaserListItem(VRStageLighting_AudioLink_Laser laser, bool foldout)
//     {
//         this.laser = laser;
//         this.foldout = foldout;
//     }
// }
public class AudioLinkListItem
{
    public VRStageLighting_AudioLink_Static light;
    public bool foldout;
    public bool isLaser;
    public VRStageLighting_AudioLink_Laser laser;
    //////////////////////////////////////////////////////////////////////////
    private bool Z_enableAudioLink; public bool P_enableAudioLink;
    private AudioLinkBandState Z_band; public AudioLinkBandState P_band;
    private int Z_delay; public int P_delay;
    private float Z_bandMultiplier; public float P_bandMultiplier;
    private bool Z_enableColorChord; public bool P_enableColorChord;
    private float Z_globalIntensity; public float P_globalIntensity;
    private float Z_finalIntensity; public float P_finalIntensity;
    private Color Z_lightColorTint; public Color P_lightColorTint;
    private bool Z_enableColorTextureSampling; public bool P_enableColorTextureSampling;
    private bool Z_enableThemeColorSampling; public bool P_enableThemeColorSampling;
    private Vector2 Z_textureSamplingCoordinates; public Vector2 P_textureSamplingCoordinates;
    private int Z_themeColorTarget; public int P_themeColorTarget;
    private Transform Z_targetToFollow; public Transform P_targetToFollow;
    private float Z_spinSpeed; public float P_spinSpeed;
    private bool Z_enableAutoSpin; public bool P_enableAutoSpin;
    private int Z_selectGOBO; public int P_selectGOBO;
    private float Z_coneWidth; public float P_coneWidth;
    private float Z_coneLength; public float P_coneLength;
    private float Z_maxConeLength; public float P_maxConeLength;
    ///////////////////////////////////////////////////////////////////////
    private float Z_coneFlatness; public float P_coneFlatness;
    private float Z_coneXRotation; public float P_coneXRotation;
    private float Z_coneYRotation; public float P_coneYRotation;
    private float Z_coneZRotation; public float P_coneZRotation;
    private int Z_laserCount; public int P_laserCount;
    private float Z_laserScroll; public float P_laserScroll;
    private float Z_laserThickness; public float P_laserThickness;

    public AudioLinkListItem(VRStageLighting_AudioLink_Static light, bool foldout)
    {
        this.light = light;
        this.foldout = this.light.foldout = foldout;
        this.isLaser = false;
        Z_enableAudioLink = P_enableAudioLink = this.light.EnableAudioLink;
        Z_band = P_band = this.light.Band;
        Z_delay = P_delay = this.light.Delay;
        Z_bandMultiplier = P_bandMultiplier = this.light.BandMultiplier;
        Z_enableColorChord = P_enableColorChord = this.light.ColorChord;
        Z_globalIntensity = P_globalIntensity = this.light.GlobalIntensity;
        Z_finalIntensity = P_finalIntensity = this.light.FinalIntensity;
        Z_lightColorTint = P_lightColorTint = this.light.LightColorTint;
        Z_enableColorTextureSampling = P_enableColorTextureSampling = this.light.ColorTextureSampling;
        Z_textureSamplingCoordinates = P_textureSamplingCoordinates = this.light.TextureSamplingCoordinates;
        Z_enableThemeColorSampling = P_enableThemeColorSampling = this.light.ThemeColorSampling;
        Z_themeColorTarget = P_themeColorTarget = this.light.ThemeColorTarget;
        Z_targetToFollow = P_targetToFollow = this.light.targetToFollow;
        Z_spinSpeed = P_spinSpeed = this.light.SpinSpeed;
        Z_enableAutoSpin = P_enableAutoSpin =  this.light.ProjectionSpin;
        Z_selectGOBO = P_selectGOBO = this.light.SelectGOBO;
        Z_coneWidth = P_coneWidth = this.light.ConeWidth;
        Z_coneLength = P_coneLength = this.light.ConeLength;
        Z_maxConeLength = P_maxConeLength = this.light.MaxConeLength;
        

    }
    public AudioLinkListItem(VRStageLighting_AudioLink_Laser laser, bool foldout)
    {
        this.laser = laser;
        this.foldout = this.laser.foldout = foldout;
        this.isLaser = true;
        Z_enableAudioLink = P_enableAudioLink = this.laser.EnableAudioLink;
        Z_band = P_band = this.laser.Band;
        Z_delay = P_delay = this.laser.Delay;
        Z_bandMultiplier = P_bandMultiplier = this.laser.BandMultiplier;
        Z_enableColorChord = P_enableColorChord = this.laser.ColorChord;
        Z_globalIntensity = P_globalIntensity = this.laser.GlobalIntensity;
        Z_finalIntensity = P_finalIntensity = this.laser.FinalIntensity;
        Z_lightColorTint = P_lightColorTint = this.laser.LightColorTint;
        Z_enableColorTextureSampling = P_enableColorTextureSampling = this.laser.ColorTextureSampling;
        Z_textureSamplingCoordinates = P_textureSamplingCoordinates = this.laser.TextureSamplingCoordinates;
        Z_enableThemeColorSampling = P_enableThemeColorSampling = this.laser.ThemeColorSampling;
        Z_themeColorTarget = P_themeColorTarget = this.laser.ThemeColorTarget;
        Z_coneFlatness = P_coneFlatness = this.laser.ConeFlatness;
        Z_coneXRotation = P_coneXRotation =  this.laser.ConeXRotation;
        Z_coneYRotation = P_coneYRotation = this.laser.ConeYRotation;
        Z_coneZRotation = P_coneZRotation = this.laser.ConeZRotation;
        Z_laserCount = P_laserCount = this.laser.LaserCount;
        Z_laserScroll = P_laserScroll = this.laser.LaserScroll;
        Z_coneWidth = P_coneWidth = this.laser.ConeWidth;
        Z_coneLength = P_coneLength = this.laser.ConeLength;
        Z_laserThickness = P_laserThickness = this.laser.LaserThickness;
    }
    public void ResetChanges(VRStageLighting_AudioLink_Static li , bool closeMenus)
    {
        try
        {
            if(closeMenus)
            {
                    var so = new SerializedObject(light);
                    so.FindProperty("foldout").boolValue = false;
                    so.ApplyModifiedProperties();
            }
        }
        catch(ArgumentException e)
        {
            e.GetType();
        }

#if UDONSHARP
        #pragma warning disable 0618 //suppressing obsoletion warnings
        light.UpdateProxy();
        #pragma warning restore 0618 //suppressing obsoletion warnings
#endif
        light.EnableAudioLink = P_enableAudioLink = Z_enableAudioLink;
        light.Band = P_band = Z_band;
        light.Delay = P_delay = Z_delay;
        light.BandMultiplier = P_bandMultiplier = Z_bandMultiplier;
        light.ColorChord = P_enableColorChord = Z_enableColorChord;
        light.GlobalIntensity = P_globalIntensity = Z_globalIntensity;
        light.FinalIntensity = P_finalIntensity = Z_finalIntensity;
        light.LightColorTint = P_lightColorTint = Z_lightColorTint;
        light.ColorTextureSampling = P_enableColorTextureSampling = Z_enableColorTextureSampling;
        light.TextureSamplingCoordinates = P_textureSamplingCoordinates = Z_textureSamplingCoordinates;
        light.ThemeColorSampling = P_enableThemeColorSampling = Z_enableThemeColorSampling;
        light.ThemeColorTarget = P_themeColorTarget = Z_themeColorTarget;
        light.targetToFollow = P_targetToFollow = Z_targetToFollow;
        light.ProjectionSpin = P_enableAutoSpin = Z_enableAutoSpin;
        light.SpinSpeed = P_spinSpeed = Z_spinSpeed;
        light.SelectGOBO = P_selectGOBO = Z_selectGOBO;
        light.ConeWidth = P_coneWidth = Z_coneWidth;
        light.ConeLength = P_coneLength = Z_coneLength;
        light.MaxConeLength = P_maxConeLength = Z_maxConeLength;
        light.foldout = false;
#if UDONSHARP
        #pragma warning disable 0618 //suppressing obsoletion warnings
        light.ApplyProxyModifications();
        #pragma warning restore 0618 //suppressing obsoletion warnings
#endif
    }
    public void ResetChanges(VRStageLighting_AudioLink_Laser li , bool closeMenus)
    {
        try
        {
            if(closeMenus)
            {
                var so = new SerializedObject(laser);
                so.FindProperty("foldout").boolValue = false;
                so.ApplyModifiedProperties();
            }
        }
        catch(ArgumentException e)
        {
            e.GetType();
        }
#if UDONSHARP
        #pragma warning disable 0618 //suppressing obsoletion warnings
        laser.UpdateProxy();
        #pragma warning restore 0618 //suppressing obsoletion warnings
#endif
        laser.EnableAudioLink = P_enableAudioLink = Z_enableAudioLink;
        laser.Band = P_band = Z_band;
        laser.Delay = P_delay = Z_delay;
        laser.BandMultiplier = P_bandMultiplier = Z_bandMultiplier;
        laser.ColorChord = P_enableColorChord = Z_enableColorChord;
        laser.GlobalIntensity = P_globalIntensity = Z_globalIntensity;
        laser.FinalIntensity = P_finalIntensity = Z_finalIntensity;
        laser.LightColorTint = P_lightColorTint = Z_lightColorTint;
        laser.ColorTextureSampling = P_enableColorTextureSampling = Z_enableColorTextureSampling;
        laser.TextureSamplingCoordinates = P_textureSamplingCoordinates = Z_textureSamplingCoordinates;
        laser.ThemeColorSampling = P_enableThemeColorSampling = Z_enableThemeColorSampling;
        laser.ThemeColorTarget = P_themeColorTarget = Z_themeColorTarget;
        laser.ConeXRotation = P_coneXRotation = Z_coneXRotation;
        laser.ConeYRotation = P_coneYRotation = Z_coneYRotation;
        laser.ConeZRotation = P_coneZRotation = Z_coneZRotation;
        laser.LaserCount = P_laserCount = Z_laserCount;
        laser.ConeWidth = P_coneWidth = Z_coneWidth;
        laser.ConeLength = P_coneLength = Z_coneLength;
        laser.LaserScroll = P_laserScroll = Z_laserScroll;
        laser.foldout = false;
#if UDONSHARP
        #pragma warning disable 0618 //suppressing obsoletion warnings
        laser.ApplyProxyModifications();
        #pragma warning restore 0618 //suppressing obsoletion warnings
#endif
    }

    public void ApplyChanges(VRStageLighting_AudioLink_Static li)
    {
      //  Undo.RecordObject(light, "Undo Apply Changes");
      //  PrefabUtility.RecordPrefabInstancePropertyModifications(light);

        var so = new SerializedObject(light);
        so.FindProperty("enableAudioLink").boolValue = P_enableAudioLink;
        so.FindProperty("band").enumValueIndex = (int) P_band;
        so.FindProperty("delay").intValue = P_delay;
        so.FindProperty("bandMultiplier").floatValue = P_bandMultiplier;
        so.FindProperty("enableColorChord").boolValue = P_enableColorChord; 
        so.FindProperty("globalIntensity").floatValue = P_globalIntensity;
        so.FindProperty("finalIntensity").floatValue = P_finalIntensity;
        so.FindProperty("lightColorTint").colorValue = P_lightColorTint;
        so.FindProperty("enableColorTextureSampling").boolValue = P_enableColorTextureSampling;
        so.FindProperty("textureSamplingCoordinates").vector2Value = P_textureSamplingCoordinates;
        so.FindProperty("enableThemeColorSampling").boolValue = P_enableThemeColorSampling;
        so.FindProperty("themeColorTarget").intValue = P_themeColorTarget;
        so.FindProperty("targetToFollow").objectReferenceValue = P_targetToFollow;
        so.FindProperty("enableAutoSpin").boolValue = P_enableAutoSpin;
        so.FindProperty("spinSpeed").floatValue = P_spinSpeed;
        so.FindProperty("selectGOBO").intValue = P_selectGOBO;
        so.FindProperty("coneWidth").floatValue = P_coneWidth;
        so.FindProperty("coneLength").floatValue = P_coneLength;
        so.FindProperty("maxConeLength").floatValue = P_maxConeLength;
        so.FindProperty("foldout").boolValue = foldout;
        so.ApplyModifiedProperties();

        //var soTarget = new SerializedObject(light.targetToFollow.gameObject);

    
#if UDONSHARP
        #pragma warning disable 0618 //suppressing obsoletion warnings
        light.UpdateProxy();
        #pragma warning restore 0618 //suppressing obsoletion warnings
#endif
        light.EnableAudioLink = Z_enableAudioLink = P_enableAudioLink;
        light.Band = Z_band = P_band;
        light.Delay = Z_delay = P_delay;
        light.BandMultiplier = Z_bandMultiplier = P_bandMultiplier;
        light.ColorChord = Z_enableColorChord = P_enableColorChord;
        light.GlobalIntensity = Z_globalIntensity = P_globalIntensity;
        light.FinalIntensity = Z_finalIntensity = P_finalIntensity;
        light.LightColorTint = Z_lightColorTint = P_lightColorTint;
        light.ColorTextureSampling = Z_enableColorTextureSampling = P_enableColorTextureSampling;
        light.TextureSamplingCoordinates = Z_textureSamplingCoordinates = P_textureSamplingCoordinates;
        light.ThemeColorSampling = Z_enableThemeColorSampling = P_enableThemeColorSampling;
        light.ThemeColorTarget = Z_themeColorTarget = P_themeColorTarget;
        light.targetToFollow = Z_targetToFollow = P_targetToFollow;
        light.ProjectionSpin = Z_enableAutoSpin = P_enableAutoSpin;
        light.SpinSpeed = Z_spinSpeed = P_spinSpeed;
        light.SelectGOBO = Z_selectGOBO = P_selectGOBO;
        light.ConeWidth = Z_coneWidth = P_coneWidth;
        light.ConeLength = Z_coneLength = P_coneLength;
        light.MaxConeLength = Z_maxConeLength = P_maxConeLength;
        light.foldout = foldout;
#if UDONSHARP
        #pragma warning disable 0618 //suppressing obsoletion warnings
        light.ApplyProxyModifications();
        #pragma warning restore 0618 //suppressing obsoletion warnings
#endif
        if(PrefabUtility.IsPartOfAnyPrefab(light))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(light);
        }
    }

    public void ApplyChanges(VRStageLighting_AudioLink_Laser li)
    {
      //  Undo.RecordObject(light, "Undo Apply Changes");
      //  PrefabUtility.RecordPrefabInstancePropertyModifications(light);

        var so = new SerializedObject(laser);
        so.FindProperty("enableAudioLink").boolValue = P_enableAudioLink;
        so.FindProperty("band").enumValueIndex = (int) P_band;
        so.FindProperty("delay").intValue = P_delay;
        so.FindProperty("bandMultiplier").floatValue = P_bandMultiplier;
        so.FindProperty("enableColorChord").boolValue = P_enableColorChord; 
        so.FindProperty("globalIntensity").floatValue = P_globalIntensity;
        so.FindProperty("finalIntensity").floatValue = P_finalIntensity;
        so.FindProperty("lightColorTint").colorValue = P_lightColorTint;
        so.FindProperty("enableColorTextureSampling").boolValue = P_enableColorTextureSampling;
        so.FindProperty("textureSamplingCoordinates").vector2Value = P_textureSamplingCoordinates;
        so.FindProperty("enableThemeColorSampling").boolValue = P_enableThemeColorSampling;
        so.FindProperty("themeColorTarget").intValue = P_themeColorTarget;

        so.FindProperty("coneXRotation").floatValue = P_coneXRotation;
        so.FindProperty("coneYRotation").floatValue = P_coneYRotation;
        so.FindProperty("coneZRotation").floatValue = P_coneZRotation;

        so.FindProperty("coneWidth").floatValue = P_coneWidth;
        so.FindProperty("coneLength").floatValue = P_coneLength;
        so.FindProperty("laserCount").intValue = P_laserCount;
        so.FindProperty("laserScroll").floatValue = P_laserScroll;
        so.FindProperty("laserThickness").floatValue = P_laserThickness;
        so.FindProperty("foldout").boolValue = foldout;
        so.ApplyModifiedProperties();

        //var soTarget = new SerializedObject(light.targetToFollow.gameObject);

    
#if UDONSHARP
        #pragma warning disable 0618 //suppressing obsoletion warnings
        laser.UpdateProxy();
        #pragma warning restore 0618 //suppressing obsoletion warnings
#endif
        laser.EnableAudioLink = Z_enableAudioLink = P_enableAudioLink;
        laser.Band = Z_band = P_band;
        laser.Delay = Z_delay = P_delay;
        laser.BandMultiplier = Z_bandMultiplier = P_bandMultiplier;
        laser.ColorChord = Z_enableColorChord = P_enableColorChord;
        laser.GlobalIntensity = Z_globalIntensity = P_globalIntensity;
        laser.FinalIntensity = Z_finalIntensity = P_finalIntensity;
        laser.LightColorTint = Z_lightColorTint = P_lightColorTint;
        laser.ColorTextureSampling = Z_enableColorTextureSampling = P_enableColorTextureSampling;
        laser.TextureSamplingCoordinates = Z_textureSamplingCoordinates = P_textureSamplingCoordinates;
        laser.ThemeColorSampling = Z_enableThemeColorSampling = P_enableThemeColorSampling;
        laser.ThemeColorTarget = Z_themeColorTarget = P_themeColorTarget;

        laser.ConeXRotation = Z_coneXRotation = P_coneXRotation;
        laser.ConeYRotation = Z_coneYRotation = P_coneYRotation;
        laser.ConeZRotation = Z_coneZRotation = P_coneZRotation;

        laser.LaserCount = Z_laserCount = P_laserCount;
        laser.ConeWidth = Z_coneWidth = P_coneWidth;
        laser.ConeLength = Z_coneLength = P_coneLength;
        laser.LaserScroll = Z_laserScroll = P_laserScroll;
        laser.LaserThickness = Z_laserThickness = P_laserThickness;
        laser.foldout = foldout;
#if UDONSHARP
        #pragma warning disable 0618 //suppressing obsoletion warnings
        laser.ApplyProxyModifications();
        #pragma warning restore 0618 //suppressing obsoletion warnings
#endif

        if(PrefabUtility.IsPartOfAnyPrefab(laser))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(laser);
        }
    }


}
}
#endif