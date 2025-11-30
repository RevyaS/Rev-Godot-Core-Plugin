How to use in project:
1. git submodule add git@github.com:RevyaS/Rev-Godot-Core-Plugin.git
2. Add the dependency RevGodotCore.dll, eg;
    ```
    <ItemGroup>
      <Reference Include="/media/rev/DATA/Users/Files/Works/Godot Libraries/Rev Godot Core/.godot/mono/temp/bin/Debug/RevGodotCore.dll" />
    </ItemGroup>
    ```

Update the plugin in your project (take latest plugin changes):
git submodule update --remote

How to clone projects using addon:
git clone --recurse-submodules <repo>