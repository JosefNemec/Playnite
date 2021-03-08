using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Utilities
{
    enum Token
    {
        None,
        Property,
        PropertyValue,
        PropertyStringValue
    }

    public class DatProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return $"{Name} : {Value}";
        }
    }

    public class DatObject
    {
        public string Name { get; set; }
        public List<DatProperty> Properties { get; set; } = new List<DatProperty>();

        public DatObject()
        {
        }

        public DatObject(string name)
        {
            Name = name;
        }
    }

    class Program
    {
        static int readIndex = 0;
        static char[] datFile = null;
        static int curLine = 1;
        static int curLinePos = 1;

        static Dictionary<string, PropertyInfo> datGameProperties = new Dictionary<string, PropertyInfo>();
        static Dictionary<string, PropertyInfo> datGameRomProperties = new Dictionary<string, PropertyInfo>();

        static void Main(string[] args)
        {
            foreach (var prop in typeof(DatGame).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute(typeof(DatPropertyAttribute)) is DatPropertyAttribute datAtt)
                {
                    datGameProperties.Add(datAtt.Name, prop);
                }
            }

            foreach (var prop in typeof(DatGameRom).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute(typeof(DatPropertyAttribute)) is DatPropertyAttribute datAtt)
                {
                    datGameRomProperties.Add(datAtt.Name, prop);
                }
            }

            //var datFilePath = @"e:\Devel\libretro-database\dat\Cave Story.dat";
            //var datFilePath = @"e:\Devel\libretro-database\dat\DOOM.dat";
            //datFile = File.ReadAllText(datFilePath, Encoding.UTF8).ToCharArray();
            //var root = ParseObject();

            var games = new List<DatGame>();
            //foreach (var file in Directory.GetFiles(@"e:\Devel\libretro-database\metadat\", "*.dat", SearchOption.AllDirectories))
            foreach (var file in Directory.GetFiles(@"e:\Devel\libretro-database\dat\", "*.dat", SearchOption.AllDirectories))
            {
                readIndex = 0;
                curLine = 1;
                curLinePos = 1;
                datFile = File.ReadAllText(file, Encoding.UTF8).ToCharArray();
                var root = ParseObject();
                foreach (var child in root.Properties)
                {
                    if (child.Name == "game")
                    {
                        games.Add(ParseGame(child.Value as DatObject));
                    }
                }
            }

            //var games = new List<DatGame>();
            //foreach (var child in root.Properties)
            //{
            //    if (child.Name == "game")
            //    {
            //        games.Add(ParseGame(child.Value as DatObject));
            //    }
            //}
        }

        private static DatGame ParseGame(DatObject datObj)
        {
            if (datObj == null)
            {
                throw new ArgumentNullException("Can't parse game object, no object given.");
            }

            var datGame = new DatGame();
            foreach (var prop in datObj.Properties)
            {
                if (datGameProperties.TryGetValue(prop.Name, out var propInfo))
                {
                    if (prop.Name == "rom")
                    {
                        if (datGame.Roms == null)
                        {
                            datGame.Roms = new List<DatGameRom>();
                        }

                        var datRom = new DatGameRom();
                        foreach (var romDatProp in (prop.Value as DatObject).Properties)
                        {
                            if (datGameRomProperties.TryGetValue(romDatProp.Name, out var romPropInfo))
                            {
                                SetParsedProperty(romPropInfo, datRom, (string)romDatProp.Value);
                            }
                        }

                        datGame.Roms.Add(datRom);
                    }
                    else
                    {
                        if (prop.Value is DatObject)
                        {
                            throw new Exception("Can't assign object to uknown dat property.");
                        }

                        SetParsedProperty(propInfo, datGame, (string)prop.Value);
                    }
                }
            }

            return datGame;
        }

        private static void SetParsedProperty(PropertyInfo prop, object datGame, string propValue)
        {
            if (prop.PropertyType == typeof(uint))
            {
                prop.SetValue(datGame, uint.Parse(propValue));
            }
            else if (prop.PropertyType == typeof(int))
            {
                prop.SetValue(datGame, int.Parse(propValue));
            }
            else if (prop.PropertyType == typeof(long))
            {
                prop.SetValue(datGame, long.Parse(propValue));
            }
            else if (prop.PropertyType == typeof(string))
            {
                prop.SetValue(datGame, propValue);
            }
            else
            {
                throw new Exception("Uknown target type.");
            }
        }

        private static DatProperty ParseProperty()
        {
            var datProperty = new DatProperty();
            var propName = string.Empty;
            var propValue = string.Empty;
            Token curToken = Token.Property;
            while (curToken != Token.None)
            {
                if (readIndex >= datFile.Length)
                {
                    break;
                }

                var chr = datFile[readIndex];
                readIndex++;
                curLinePos++;
                var whiteSpace = char.IsWhiteSpace(chr);

                if (whiteSpace && curToken == Token.Property)
                {
                    curToken = Token.PropertyValue;
                    datProperty.Name = propName;
                    continue;
                }

                if (!whiteSpace && curToken == Token.Property)
                {
                    propName += chr;
                    continue;
                }

                if (chr == '"' && curToken == Token.PropertyValue)
                {
                    curToken = Token.PropertyStringValue;
                    continue;
                }

                if (chr == '"' && curToken == Token.PropertyStringValue)
                {
                    datProperty.Value = propValue;
                    curToken = Token.None;
                    continue;
                }

                if (whiteSpace && curToken == Token.PropertyStringValue ||
                    !whiteSpace && curToken == Token.PropertyStringValue)
                {
                    propValue += chr;
                    continue;
                }

                if (chr == '(')
                {
                    datProperty.Value = ParseObject();
                    break;
                }

                if (!whiteSpace && curToken == Token.PropertyValue)
                {
                    propValue += chr;
                    continue;
                }

                if (curToken == Token.PropertyValue && (whiteSpace || chr == ')'))
                {
                    datProperty.Value = propValue;
                    curToken = Token.None;
                    continue;
                }

                break;
            }

            return datProperty;
        }

        private static DatObject ParseObject()
        {
            var datObject = new DatObject();
            Token curToken = Token.Property;
            while (curToken != Token.None)
            {
                if (readIndex >= datFile.Length)
                {
                    break;
                }

                var chr = datFile[readIndex];
                readIndex++;
                curLinePos++;
                if (chr == '\n')
                {
                    curLinePos = 0;
                    curLine++;
                    continue;
                }

                if (chr == '\r')
                {
                    continue;
                }

                var whiteSpace = char.IsWhiteSpace(chr);
                if (whiteSpace)
                {
                    continue;
                }

                if (chr == ')')
                {
                    break;
                }

                readIndex--;
                datObject.Properties.Add(ParseProperty());
            }

            return datObject;
        }
    }
}

