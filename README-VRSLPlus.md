# VR-Stage-Lighting-plus

## Notice
This is mostly just a fork of [VR-Stage-Lighting](https://github.com/AcChosen/VR-Stage-Lighting) but with my own changes

> [!CAUTION]
> Please do not import this package into your project if you have original VRSL installed, IT WILL CONFLICT!<br>
> MAKE SURE TO BACKUP YOUR PROJECT BEFORE DOING ANYTHING!<br>
> This is mostly experimental but feel free to try it out!

---

## Changes
It mostly contains some code cleanup, though partial, I didn't touch everything, did not refactor things I want to.<br>
Also uses Binary encoding made by [Micca](https://github.com/micksam7). Code used from [micksam7/VRC-MDMX](https://github.com/micksam7/VRC-MDMX/tree/main/BinaryGridnode)<br>
Now it has a prefab that uses [Torvid21/LUTBeams](https://github.com/Torvid21/LUTBeams)
- requires this repository to be installed in your Assets folder for now, ~~in future would be nice to have it in package~~ now it has VPM repository [u-Kotovsky/LUTBeams](https://u-kotovsky.github.io/LUTBeams/) (fork made for VPM package)<br>
new UI panel that fits VideoTXL style (very much based on VideoTXL)

---

## Supported packages
Currently supported packages:
 - [u-Kotovsky/VRSL-GPUReadBack](https://github.com/u-Kotovsky/VRSL-GPUReadBack) [Get thru VPM](https://u-kotovsky.github.io/VRSL-GPUReadBack/)

---

## Dependencies
-  [u-Kotovsky/LUTBeams](https://github.com/u-Kotovsky/LUTBeams) [Get thru VPM](https://u-kotovsky.github.io/LUTBeams/) 

---

## Setup
- Download & put `Packages/com.acchosen.vr-stage-lighting` into your projects `Packages` folder and it should work **for now**
 - Make sure you do not have original VRSL installed in order for this to work.
- Install LUTBeam VPM from [u-Kotovsky/LUTBeams](https://u-kotovsky.github.io/LUTBeams/) (fork made for VPM package)
- You need to put LocalUI panel through VRSL/Control Panel and hit Add LocalUI panel
- Assign video texture to `videoSampleTexture` property in LocalUI panel.
 - This setup mostly meant for 1920x1080 so other resolutions probably won't work right
 - and there's no other ways to put gridnode (probably for the best)
- Once you put all your fixtures and assigned channels to them, make sure to save your work, open VRSL/Fixture Texture Baker, select all your fixtures and click Bake! It will generate a Parameters mapping texture that will let shader know where coarse, fine, ultra and uber channels are (for smoothing and snapping).
 - You can read texture map as 
  - Red: Coarse (Same index)
  - Green: Fine (Index + 1)
  - Blue: Ultra (Index + 2)
  - Alpha: Uber (Index + 3) (unused for now, defaults to 1 for readability)
