using Playnite;
using System.Diagnostics;

int loops = 100000;
var names = new[]
{
    "Final Fantasy XIII-2",
    "Final Fantasy Ⅻ",
    "FINAL FANTASY X/X-2 HD Remaster",
    "Warhammer ↂↇ",
    "Carmageddon 2: Carpocalypse Now",
    "SOULCALIBUR IV",
    "Quake III: Team Arena",
    "THE KING OF FIGHTERS XIV STEAM EDITION",
    "A Hat in Time",
    "Battlefield V",
    "Tales of Monkey Island: Chapter 1 - Launch of the Screaming Narwhal",
    "Tales of Monkey Island: Chapter I - Launch of the Screaming Narwhal",
    "KOBOLD: Chapter I",
    "Crazy Machines 1.5 New from the Lab",
    "Half-Life 2: Episode One",
    "Unravel Two",
    "The Elder Scrolls II: Daggerfall Unity - GOG Cut",
    "Metal Slug XX",
    "The Uncanny X-Men",
    "Test X-",
    "The Witcher 3",
    "the Witcher 3",
    "A Game",
    "An Usual Game",
};

Stopwatch stopwatch = Stopwatch.StartNew();

SortableNameConverter snc = new(new[] { "the", "a", "an" });

stopwatch.Stop();
Console.WriteLine($"Constructor: {stopwatch.Elapsed}");
stopwatch.Restart();

for (int i = 0; i < loops; i++)
{
    foreach (var name in names)
    {
        snc.Convert(name);
    }
}

stopwatch.Stop();
Console.WriteLine($"{loops * names.Length} calls: {stopwatch.Elapsed}");
Console.ReadLine();