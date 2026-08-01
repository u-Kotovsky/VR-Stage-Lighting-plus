# VR-Stage-Lighting-plus

## Notice
This is mostly just a fork of [VR-Stage-Lighting](https://github.com/AcChosen/VR-Stage-Lighting) but with my own changes

> [!CAUTION]
> Please do not import this package into your project if you have original VRSL installed, IT WILL CONFLICT!<br>
> MAKE SURE TO BACKUP YOUR PROJECT BEFORE DOING ANYTHING!<br>
> This is mostly experimental but feel free to try it out!

## Changes
It mostly contains some code cleanup, though partial, I didn't touch everything, did not refactor things I want to.<br>
Also uses Binary encoding made by [Micca](https://github.com/micksam7). Code used from [their MDMX project](https://github.com/micksam7/VRC-MDMX/tree/main/BinaryGridnode)<br>
Now it has a prefab that uses [LUTBeams](https://github.com/Torvid21/LUTBeams) (requires this repository to be installed in your Assets folder for now, in future would be nice to have it in package)<br>
new UI panel that fits VideoTXL style (very much based on VideoTXL)

## Setup
- Download & put `Packages/com.acchosen.vr-stage-lighting` into your projects `Packages` folder and it should work **for now**
 - Make sure you do not have original VRSL installed in order for this to work.
- You need to put LocalUI panel through VRSL/Control Panel and hit Add LocalUI panel
- Assign video texture to source video texture property in LocalUI panel.
 - This setup mostly meant for 1920x1080 so other resolutions probably won't work right
 - and there's no other ways to put gridnode (probably for the best)
