**I found a critical bug with my tool (transitions between states are not copied and they modify the original) so until it is fixed I am archiving this tool.**

# VRC Menu Merger

Merges one or more VRChat menus, parameter lists and animator controllers.

## Usage

**As with any Unity plugin you should backup your Unity project before using it.**

There are 2 ways to use this tool: with a VRChat avatar or without.

### With a VRChat avatar

1. Drag the prefab into your scene or add the `VRC_Menu_Merger_Instance` component to a game object
2. Select your VRChat avatar
3. Enable the animators you want to merge. Pick the animators you want to merge into each other.
4. Enable if you want to merge your VRC menus and/or parameters. Pick the menus and parameters you want to merge into each other.
5. Click merge!

It will output all of the new assets into the specified directory.

<img src="Assets/component empty.png" width="400">

### Without a VRChat avatar

The steps are identical however you need to go to PeanutTools -> VRC Menu Merger.

<img src="Assets/editor window empty.png" width="400">

## Ideas for future

- support all animation layers (not just base)
- better nested menu merging
