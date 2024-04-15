# Overview

The objective of this project is to create an experience that involves projection mapping and depth sensory through the Xbox One Kinect. The primary programming language is in C# and the script engine is Unity. 

By mapping a scene into the real world, we create an interactive environment where users are picked up by the Kinect. Then, given a marker that determines their position in relation to the scene in Unity (hence, projection mapping). 

The images will appear and have various animations depending on the scene. The image is projected towards the ceiling in this current version of the project.

# Necessities
- Machines that runs Windows OS
- Projector for Unity scene and Xbox One Kinect (We are using a Short throw by Epson)
- Appropriate adapter for the Xbox One Kinect, HDMI cables for connectivity
- At least 1080p monitor display from the PCs
- Two Laptops or PCs to host Unity scene (as long as they run Windows OS)
- Open Space of at least 15 x 15 feet for the projection.
- Black curtains for environmental effect

# Tech

### Versions / Tech
- Unity version *must* be 2023.2.15f1(64-bit)
- Microsoft Windows 10 (current OS we are using)
- Kinect for Windows SDK 2.0 *must* be downloaded and installed

### Links to Plugins
[Unity Archive](https://unity.com/releases/editor/archive)

[Kinect for Windows SDK](https://learn.microsoft.com/en-us/windows/apps/design/devices/kinect-for-windows)

Unity Pro Packages is the add-on we are using for this project. This was imported into the Unity Project from our end, so there should not be any need to download this Unity Package.

![Screenshot of the Unity Packages location for linking Kinect One and Unity](/Resources/UnityPackages.png)

### If facing an error with Unity Pro packages...
- Unity Pro packages must be downloaded for Unity.
- Here is a link as a tutorial for setup on your remote machine... [YouTube Tutorial](https://www.youtube.com/watch?v=6EkQA3GakFI&t=99s)


# Instructions


### Setup (Unity)
1. Download the GitHub repo by doing a `git clone <httpsWebURL>` in your selected folder.
2. If multiple people are working on this project, then we recommend separate branches from <main>
3. Locate the `/Scenes` Directory
4. Example Scene One is Circles2

### Setup (Projector)
1. Ensure projector is aimed towards the ceiling (can also be aimed at the wall). Depending on your projector, you may want to place the project close to your chosen plane (wall, ceiling, floor), or farther out.
2. Plug your projector into outlet and pc
3. Select Projector from pc

### Setup (Kinect Studio)
1. Ensure you have a Xbox One Kinect w/USB adapater
2. Plug the Xbox One Kinect into your PC
3. Start Kinect Studio V2.0 on your Windows Machine
4. On the top-left of the Kinect there will be a connection button. Press that button to connect your Kinect.
5. Video will start upon pressing the button and will show what the Kinect tracks.

### Setup (Unity) ...continued
3. Open `Circles2.unity` file (if following the example scene)
4. Run the file by pressing the 'play' button. 
5. As long as the Kinect Studio V2.0 is running, the the scene will play.