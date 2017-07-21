using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Steam
{
    public static class Extensions
    {
        public static string ReadNullTermString(this Stream stream, Encoding encoding)
        {
            int characterSize = encoding.GetByteCount("e");

            using (MemoryStream ms = new MemoryStream())
            {

                while (true)
                {
                    byte[] data = new byte[characterSize];
                    stream.Read(data, 0, characterSize);

                    if (encoding.GetString(data, 0, characterSize) == "\0")
                    {
                        break;
                    }

                    ms.Write(data, 0, data.Length);
                }

                return encoding.GetString(ms.ToArray());
            }
        }
    }

    public class AppInfo
    {
        public class AppState
        {
            public int Size;
            public int State;
            public int LastUpdate;
            public ulong AccessToken;
            public byte[] Checksum;
            public int ChangeNumber;
        }

        public AppState Header;
        public DataFieldList Sections;
    }

    public class DataField
    {
        public string Name;

        public DataFieldList ListData;
        public string StringData;
        public int IntData;
        public ulong UlongData;
        public float FloatData;

        public DataField(string name)
        {
            Name = name;
        }

        public DataField(string name, string data)
        {
            Name = name;
            StringData = data;
        }

        public DataField(string name, int data)
        {
            Name = name;
            IntData = data;
        }

        public DataField(string name, ulong data)
        {
            Name = name;
            UlongData = data;
        }

        public DataField(string name, float data)
        {
            Name = name;
            FloatData = data;
        }

        public DataField(string name, DataFieldList data)
        {
            Name = name;
            ListData = data;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    // Using custom list instead of Dictionary because steam list collection can contain more items with the same key
    public class DataFieldList : List<DataField>
    {
        public DataField this[string index]
        {
            get
            {
                return this.FirstOrDefault(a => string.Compare(a.Name, index, true) == 0);
            }
        }

        public bool HasIndex(string index)
        {
            return this.FirstOrDefault(a => string.Compare(a.Name, index, true) == 0) != null;
        }
    }

    /// <summary>
    /// Parser for binary base vdf Valve files
    /// </summary>
    [Obsolete("Do not use without further testing and fixing.", false)]
    public class VdfParser
    {
        enum ValueType : byte
        {
            None = 0,
            String = 1,
            Int32 = 2,
            Float32 = 3,
            Pointer = 4,
            WideString = 5,
            Color = 6,
            UInt64 = 7,
            End = 8,
        }      

        public static Dictionary<int, AppInfo> LoadFromFile(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(file))
                {
                    var apps = new Dictionary<int, AppInfo>();
                    var version = reader.ReadInt32();
                    var universe = reader.ReadInt32();

                    while (true)
                    {
                        var appId = reader.ReadInt32();
                        if (appId == 0)
                        {
                            break;
                        }

                        var appInfo = new AppInfo();
                        apps.Add(appId, appInfo);

                        var appHeader = new AppInfo.AppState()
                        {
                            Size = reader.ReadInt32(),
                            State = reader.ReadInt32(),
                            LastUpdate = reader.ReadInt32(),
                            AccessToken = reader.ReadUInt64(),
                            Checksum = reader.ReadBytes(20),
                            ChangeNumber = reader.ReadInt32()
                        };

                        appInfo.Header = appHeader;

                        var appSections = new DataFieldList();

                        while (true)
                        {
                            var sectionId = reader.ReadByte();
                            if (sectionId == 0)
                            {
                                break;
                            }

                            // Skip the 0x00 byte before section name.
                            reader.ReadByte();

                            var sectionName = reader.BaseStream.ReadNullTermString(Encoding.UTF8);
                            appSections.Add(new DataField(sectionName, parseAppSection(reader, true)));
                        }

                        appInfo.Sections = appSections;
                    }

                    return apps;
                }
            }
        }

        static DataFieldList parseAppSection(BinaryReader reader, bool root)
        {
            var data = new DataFieldList();

            while (true)
            {
                var valueType = (ValueType)reader.ReadByte();
                if (valueType == ValueType.End)
                {
                    // There's one additional 0x08 byte at the end of the root subsection.
                    if (root)
                    {
                        reader.ReadByte();
                    }

                    break;
                }
                
                var key = reader.BaseStream.ReadNullTermString(Encoding.UTF8);
                var section = new DataField(key);

                switch (valueType)
                {
                    case ValueType.None:
                        {
                            section.ListData = parseAppSection(reader, false);
                            break;
                        }

                    case ValueType.String:
                        {
                            section.StringData = reader.BaseStream.ReadNullTermString(Encoding.UTF8);
                            break;
                        }

                    case ValueType.WideString:
                        {
                            throw new Exception("Failed to read steam data, WideString found.");
                        }

                    case ValueType.Int32:
                        {
                            section.IntData = reader.ReadInt32();
                            break;
                        }

                    case ValueType.Color:
                        {
                            section.IntData = reader.ReadInt32();
                            break;
                        }

                    case ValueType.Pointer:
                        {
                            section.IntData = reader.ReadInt32();
                            break;
                        }

                    case ValueType.UInt64:
                        {
                            section.UlongData = reader.ReadUInt64();
                            break;
                        }

                    case ValueType.Float32:
                        {
                            var buff = new byte[8];
                            reader.Read(buff, 0, 4);
                            section.FloatData = BitConverter.ToSingle(buff, 0);
                            break;
                        }
                }

                data.Add(section);
            }

            return data;
        }
    }
}
