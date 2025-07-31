# window-positions-toggle
This is a window size/position helper. You can set up size/location information for different applications and then pressing a hotkey will moved the focused window to the next predefined size/location for that application.

You can have multiple preset sizes and locations, so if there are a few places/sizes that you tend to use a particular applcation at, you just press the hotkey a few times and instantly it is where you want it. 

If a window is not already at one of the saved size+locations, it will snap to the first one. After that, it will snap to the next saved size + location. When it gets to the end of the list of saved size+locations, it snaps back to the first one. This means you can move a window wherever you want and resize it however you want to work with it temporarily, then have it back how you like it in a keypress or two.

Works with cinnamon well. *Should* work with most X11-based WMs.


# Usage

1. Install dependencies

2. Set up a configuration JSON file as directed below, you can run the app with "-v" and it will wait 3 seconds then tell you what the class name of the active window is, and also give you Top, Left, Width, Height attributes for the active window so you can easily add them to your JSON config file.

Note: I may need to add offsets for these. They may be off by the width of the drop shadows that cinnamon is rendering. They will be close to what you want, anyways.

3. Once you have what class to match on, and one or more PreferredPositions in your JSON config file, then focus a window with a class in your json file, and run the application. (I suggest setting up a hotkey somehow or you're only ever going to have it be your terminal window you're running the app from that's the focused window, obviously.)

When run, the app will look for a class match in the JSON file, once it finds one, it will place the matched window that is focused when the app is run at the first PreferredPosition for that class. If there is more than one PreferredPosition for that class in the JSON file, it will loop through all of the PreferredPositions, changing the window location and size to match each PreferredPosition.

If the app is already at one of the PreferredPositions, it will always move the focused window to the next PreferredPosition on the next run of the app. If it is at none of the PreferredPositions when run, it will snap the focused window to the first PreferredPosition

## Calculating offsets:
Take a look in getSavedIndexOf() in Program.cs. Offsets can be calculated by setting a program using the app and JSON file, then running wmctrl -lG and seeing what the reported Left and Top are vs. what values you set with the JSON config. I already have per-machine handling in that file for my laptop vs my desktop since both use different dpi scaling. Modify with your offsets.

Some windows, like gnome-terminal, do not have offsets, but will only allow certain widths/heights to be set.

I may just move everything to be per-window class offsets in the JSON

NOTE: When dpi scaling, things go sideways. Read the JSON examples carefully.

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

Github desktop when set to go to 300, 300 actually reports that it is at 300, 244. This is why there's "ExtraTopOffset": 56. That adds an additional offset correction of +56 to whatever is reported. These can also be negative.

Jetbrains Rider has "LeftTopScalingMultiple": 2.0 because if it is set to Left:100, Top:100, wmctrl -lG will report it is at 200, 200. If it is set to Left:200, Top:300, wmctrl -lG will report it is at 400, 600, etc...

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
  },
  {
    "ClassPattern": "github desktop.GitHub Desktop",
    "ExtraLeftOffset": -0,
    "ExtraTopOffset": 56,
    "PreferredPositions": [
      {
        "Left": 300,
        "Top": 300,
        "Width": 2560,
        "Height": 1800
      },
      {
        "Left": 400,
        "Top": 400,
        "Width": 2560,
        "Height": 1800
      }
    ]
  },
  {
    "ClassPattern": "jetbrains-rider.jetbrains-rider",
    "LeftTopScalingMultiple": 2.0,
    "PreferredPositions": [
      {
        "Left": 890,
        "Top": 0,
        "Width": 4230,
        "Height": 2798
      },
      {
        "Left": 5120,
        "Top": 0,
        "Width": 2560,
        "Height": 2800
      }
    ]
  }
]
```

