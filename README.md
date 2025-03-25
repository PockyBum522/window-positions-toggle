# window-positions-toggle
Using wmctrl to snap my windows into the positions I want automatically, depending on what window is currently selected. Works with cinnamon well.


# Usage

1. Install dependencies

2. Set up a configuration JSON file as directed below, you can run the app with "-v" and it will wait 3 seconds then tell you what the class name of the active window is, and also give you Top, Left, Width, Height attributes for the active window so you can easily add them to your JSON config file.

Note: I may need to add offsets for these. They may be off by the width of the drop shadows that cinnamon is rendering. They will be close to what you want, anyways.

3. Once you have what class to match on, and one or more PreferredPositions in your JSON config file, then focus a window with a class in your json file, and run the application. (I suggest setting up a hotkey somehow or you're only ever going to have it be your terminal window you're running the app from that's the focused window, obviously.)

When run, the app will look for a class match in the JSON file, once it finds one, it will place the matched window that is focused when the app is run at the first PreferredPosition for that class. If there is more than one PreferredPosition for that class in the JSON file, it will loop through all of the PreferredPositions, changing the window location and size to match each PreferredPosition.

If the app is already at one of the PreferredPositions, it will always move the focused window to the next PreferredPosition on the next run of the app. If it is at none of the PreferredPositions when run, it will snap the focused window to the first PreferredPosition


# Installation

Install below dependencies. Update _userPreferencesPath at the top of Program.cs, which is where the configuration JSON dotfile will be stored.

If you want to set up a hotkey to make this easy, and have xbindkeys, then add this to your .xbindkeysrc:

```
# Alt + r
"/usr/sbin/dotnet /your/path/to/bin/WindowPositionsToggle/WindowPositionsToggle.dll"
    m:0x18 + c:27
```


# Prerequisites
```
sudo apt install wmctrl -y
```

## If you want an easy way to call the script with a hotkey:
```
sudo apt install xbindkeys -y
```


# Example JSON file:

As shown, Nemo will only ever snap to one position, with the top left corner of the window snapping to 600, 100. And the window size will resize to 600x500.

xed will snap to two positions. If it's not at either of them, it will first snap to the top left of the top left monitor, then if the program is run again while it is at that position and size, it will then snap to 300, 400 at a size of 500x400 for the window. Repeatedly running the program after that will just keep snapping the window between those two positions/sizes.

```
[
  {
    "ClassPattern": "nemo.Nemo",
    "PreferredPositions": [
      {
        "Left": 600,
        "Top": 100,
        "Width": 600,
        "Height": 500
      }
    ]
  }, 
  {
    "ClassPattern": "xed.Xed",
    "PreferredPositions": [
      {
        "Left": 1,
        "Top": 1,
        "Width": 500,
        "Height": 400
      },
      {
        "Left": 300,
        "Top": 400,
        "Width": 500,
        "Height": 400
      }
    ]
  }
]
```

