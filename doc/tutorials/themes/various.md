Referencing theme files
---------------------



What all these PART_ element names
---------------------


Color definitions
---------------------

You may be wondering why are some colors defined with just 6 digits like `#112233` and other with 8 digits like `#BB112233`. WPF uses RGB system to define color values where each color is defined by values ranging from `00` to `FF` (using [hex](https://simple.wikipedia.org/wiki/Hexadecimal_numeral_system) digits): `#RRGGBB`. However you can also define alpha transparency using another two digits specify transparency intensity: `#AARRGGBB`. Where `00` is fully transparent and `FF` is fully opaque.

Using custom controls and 3rd party assemblies
---------------------

The way Playnite currently loads theme files doesn't natively support use of 3rd party assemblies unless they are manually placed in application folder, meaning they can't be distributed inside [pthm](distributionAndUpdates.md) files.

Using custom fonts
---------------------

Limitations of using 3rd party assemblies also apply to use of custom fonts. Currently recommended option is to install desired font in Windows font storage and then use it as any other built-in Windows font in your XAML files.