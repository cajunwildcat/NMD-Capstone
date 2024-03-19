# Overview

The objective of this project is to create an experience that involves projection mapping and depth sensory through the Xbox One Kinect. The primary programming language is in C# and the script engine is Unity. 

By mapping a scene into the real world, we create an interactive environment where users are picked up by the Kinect. Then given a marker that determines their position in relation to the scene in Unity (hence, projection mapping). 

The images will appear and have various animations depending on the New Media Designer. The image is projected towards the ceiling in this current version of the project. (Subject to being aimed at the floor given angle of projectors on Y axis.)

# Necessities
- Projector for Unity scene and Xbox One Kinect
- Laptop/PC to host Unity scene
- Open Space of at least 15 x 15 feet for the projection.

# Tech

### Versions / Tech
- Unity version *must* be 2020.3.23f1(64-bit)
- Microsoft Windows 10 (current OS we are using)
- Kinect for Windows SDK 2.0 *must* be downloaded and installed

### Links to Plugins
[Unity Archive](https://unity.com/releases/editor/archive)

[Kinect for Windows SDK](https://learn.microsoft.com/en-us/windows/apps/design/devices/kinect-for-windows)

Unity Pro Packages is the add-on we are using for this project. This was imported into the Unity Project from our end, so there should not be any need to download this Unity Package.

![Screenshot of the Unity Packages location for linking Kinect One and Unity](/Resources/UnityPackages.png)

### If facing an error with Unity Pro packages...
- Unity Pro packages must be downloaded for Unity.
- Here is a link as a tutorial for setup on your home machine... [YouTube Tutorial](https://www.youtube.com/watch?v=6EkQA3GakFI&t=99s)


# Instructions


### Setup (Unity)
1. Download the GitHub repo by doing a `git clone <httpsWebURL>` in your selected folder.
2. Locate the `/Scenes` Directory

### Setup (Projector)
1. Ensure projector is aimed towards the ceiling
2. Plug your projector into outlet and pc
3. Select Projector from pc

### Setup (Kinect Studio)
1. Ensure you have a Xbox One Kinect w/USB adapater
2. Plug the Xbox One Kinect into your PC
3. Start Kinect Studio V2.0 on your Windows Machine
4. On the top-left there will be a connect button. Press that to connect your Kinect.
5. Video will start upon pressing the button and show what the Kinect sees.

### Setup (Unity) ...continued
3. Open `Circles2.unity` file
4. Run the file by pressing the 'play' button. 
5. As long as the Kinect Studio V2.0 is running, the the scene will play.