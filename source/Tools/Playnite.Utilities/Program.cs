using LiteDB;
using Playnite.Common;
using Playnite.Emulators;
using SqlNado;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Utilities
{
    class Program
    {
        static void Main(string[] args)
        {
            DatParser.ProcessLibretroDb(@"e:\Devel\libretro-database", @"d:\Downloads\dats", true, true);
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}

