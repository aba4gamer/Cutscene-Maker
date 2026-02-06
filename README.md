# Custcene Maker
Tool that helps you making cutscene for Super Mario Galaxy 1 & 2.
README in progress. If you can't run the app install .NET 10 but I think actually it's not mandatory.

## To do:

- ~~Update the README.md~~ *Done*
- ~~Fix the warning spam when building~~ *Done*
- ~~"Fix" the menu buttons to make them work~~ *Done*
- ~~Move the RARC handlers in Abacus~~ *Done*
- ~~Add resizable timeline~~ *Done*
- ~~Fix the SubPart box in timeline to make it match the part line~~ *Done*
- ~~Add support for exporting with Yaz0~~ *Done*
- ~~Add that when you click on `Save As` the name of the file appears instead of `DemoMyCutscene.arc`~~ *Done*
- ~~Add support for creating arcs~~ *Done*
- ~~Add the ability to move parts before or after other parts in the timeline~~ *Done*
- ~~Add zoom functionality in the timeline~~ *Done*
- ~~Add optimization to the timeline~~ *Done*
- ~~Add an empty cutscene screen (cutscene without part saying how to create a new part)~~ *Done*
- ~~Add a proper "button" to the cutscenes in the cutscene sidebar~~ *Done*
- ~~Add some context menus because people like them~~ *Done*
- ~~Add ComboBox for CastName, ActionType and other things~~ *I (Antonio225) believe that it's done.*
- ~~Add a better warning for unsaved progress~~ *Done but a bit bugged*
- ~~Add some cool icons (if I can do them cool lel)~~ *Done*
- ~~Add icons to the timeline part~~ *Done*
- ~~Add storage and a settings menu~~ *Done*

### Auto completion support:

* From **Action:** `CastName` (**yes**), `ActionType` (**yes**), `PosName` (**yes**), `AnimName` (**yes**[^1]);
* From **Player:** `PosName` (**yes**) `BckName` (**yes**);
* From **Camera:** `CameraTargetName` (**yes**), `AnimCameraName` (**yes**[^1]);
* From **Sound:** `Bgm` (**yes**), `SystemSe` (**yes**), `ActionSe` (**no**[^2]);
* From **Wipe:** `WipeName` (**yes**), `WipeType` (**yes**);

[^1]: Antonio225 still didn't understand how to convert ObjNameTable english entries to link to **.arc**s filenames so... Support for it might not work properly.
[^2]: We figured out that `SystemSe` and `ActionSe` are not the same. We're maybe going to add it in 2.2.0 once we know more about `ActionSe` and how it works.

## Building
To build use:
- [Hack.io](https://github.com/SuperHackio/Hack.io)
- [Hack.io.BCSV](https://github.com/SuperHackio/Hack.io)
- [Hack.io.RARC](https://github.com/SuperHackio/Hack.io)
- [Hack.io.YAZ0](https://github.com/SuperHackio/Hack.io)
- [BidirectionalDictionary](https://github.com/iiKuzmychov/BidirectionalDictionary)
- [Svg.Controls.Avalonia](https://github.com/wieslawsoltes/Svg.Skia)
- [Tommy](https://github.com/dezhidki/Tommy)

## Credits
- [Hack.io](https://github.com/SuperHackio/Hack.io) libraries by [Super Hackio](https://github.com/SuperHackio)
- [MessageBox.Avalonia](https://github.com/AvaloniaCommunity/MessageBox.Avalonia) by [CreateLab](https://github.com/CreateLab)
- [BidirectionalDictionary](https://github.com/iiKuzmychov/BidirectionalDictionary) by [iikuzmychov](https://github.com/iikuzmychov)
- [Svg.Controls.Avalonia](https://github.com/wieslawsoltes/Svg.Skia) by [wieslawsoltes](https://github.com/wieslawsoltes/)
- [Tommy](https://github.com/dezhidki/Tommy) by [dezhidki](https://github.com/dezhidki)
- [Fluent System](https://github.com/microsoft/fluentui-system-icons) by [Microsoft](https://github.com/microsoft)
- [Symbols Nerd Font](https://www.nerdfonts.com) by Ryan L McIntyre
- Cinema cutscene icon by Freepix
