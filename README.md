# ff16.utility.modloader

[Final Fantasy XVI / 16](https://store.steampowered.com/app/2515020/FINAL_FANTASY_XVI/) Mod loader for [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) using [FF16Tools](https://github.com/Nenkai/FF16Tools).

## Usage

Make sure that you have Reloaded-II and download the latest FFXVI mod loader in [Releases](https://github.com/Nenkai/ff16.utility.modloader/releases) (the .7z file).

Add Final Fantasy XVI as a registered game within Reloaded-II.

Drag and drop the .7z file to the left pane of Reloaded-II.

### Removing Mods

If you'd like to remove mods, head to the game's folder, `data` and remove any `.diff.pac` files.

## Mod File Structure

Refer to [**this page**](https://nenkai.github.io/ffxvi-modding/modding/creating_mods/).

You can use [**the template mod**](https://github.com/Nenkai/ff16.utility.modloader/releases/download/1.0.1/ff16.template.helloworld.zip) (which changes some of the main menu ui text to `Hello World`) for reference.

## Building

You may need to remove the `dstorage.dll` files in `runtimes` folders after compiling, otherwise could cause conflicts with the game's dstorage.dll

## Discord

<a href="https://discord.gg/D7jhUDfYZh">
  <img src="https://discordapp.com/api/guilds/1284918645675397140/widget.png?style=banner2" alt="Discord Banner 1"/>
</a>
