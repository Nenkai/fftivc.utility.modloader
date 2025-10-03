# fftivc.utility.modloader

[FINAL FANTASY TACTICS - The Ivalice Chronicles](https://store.steampowered.com/app/1004640/) Mod loader for [Reloaded-II](https://github.com/Reloaded-Project/Reloaded-II) using [FF16Tools](https://github.com/Nenkai/FF16Tools).

## Usage

Make sure that you have Reloaded-II and download the latest FFTIVC mod loader in [Releases](https://github.com/Nenkai/ff16.utility.modloader/releases) (the .7z file).

Add FFTIVC as a registered game within Reloaded-II.

Drag and drop the .7z file to the left pane of Reloaded-II.

### Removing Mods

If you'd like to remove mods, head to the game's folder, `data` and remove any `modded` files.

## Mod File Structure

Refer to [**this page**](https://nenkai.github.io/ffxvi-modding/modding/creating_mods_fft/).

## Building

You may need to remove the `dstorage.dll` files in `runtimes` folders after compiling, otherwise could cause conflicts with the game's dstorage.dll
