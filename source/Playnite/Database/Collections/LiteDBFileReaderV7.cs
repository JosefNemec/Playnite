using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static LiteDBConversion.Constants;

// This is copy from LiteDB 5 version and it's only used for recovery of corrupted databases.
// We use LiteDB 4 in Playnite, see description of ItemCollection<TItem> to see why.
// LiteDB 4 has some super rare bug that can corrupt database when file is being open and process is hard killed,
// even when journal is used and sharing is not enabled. People reported corrupted DBs when hard resettingg PCs while Playnite is running.
// The corruption is very weird because V5 can read these files just fine during upgrade from V4 to V5.
// So if we detect corruption we try to use this upgrade code to fix the db.
// Standard "raw" reading of collection doesn't work when DB is damabged in this way so this is the only way how to recover data.
namespace LiteDBConversion
{
    internal class BsonReader
    {
        private bool _utcDate = false;

        public BsonReader(bool utcDate)
        {
            _utcDate = utcDate;
        }

        /// <summary>
        /// Main method - deserialize using ByteReader helper
        /// </summary>
        public LiteDB.BsonDocument Deserialize(byte[] bson)
        {
            return this.ReadDocument(new ByteReader(bson));
        }

        /// <summary>
        /// Read a BsonDocument from reader
        /// </summary>
        public LiteDB.BsonDocument ReadDocument(ByteReader reader)
        {
            var length = reader.ReadInt32();
            var end = reader.Position + length - 5;
            var obj = new LiteDB.BsonDocument();

            while (reader.Position < end)
            {
                var value = this.ReadElement(reader, out string name);
                obj.RawValue[name] = value;
            }

            reader.ReadByte(); // zero

            return obj;
        }

        /// <summary>
        /// Read an BsonArray from reader
        /// </summary>
        public LiteDB.BsonArray ReadArray(ByteReader reader)
        {
            var length = reader.ReadInt32();
            var end = reader.Position + length - 5;
            var arr = new LiteDB.BsonArray();

            while (reader.Position < end)
            {
                var value = this.ReadElement(reader, out string name);
                arr.Add(value);
            }

            reader.ReadByte(); // zero

            return arr;
        }

        /// <summary>
        /// Reads an element (key-value) from an reader
        /// </summary>
        private LiteDB.BsonValue ReadElement(ByteReader reader, out string name)
        {
            var type = reader.ReadByte();
            name = reader.ReadCString();

            if (type == 0x01) // Double
            {
                return reader.ReadDouble();
            }
            else if (type == 0x02) // String
            {
                return reader.ReadBsonString();
            }
            else if (type == 0x03) // Document
            {
                return this.ReadDocument(reader);
            }
            else if (type == 0x04) // Array
            {
                return this.ReadArray(reader);
            }
            else if (type == 0x05) // Binary
            {
                var length = reader.ReadInt32();
                var subType = reader.ReadByte();
                var bytes = reader.ReadBytes(length);

                switch (subType)
                {
                    case 0x00: return bytes;
                    case 0x04: return new Guid(bytes);
                }
            }
            else if (type == 0x07) // ObjectId
            {
                return new LiteDB.ObjectId(reader.ReadBytes(12));
            }
            else if (type == 0x08) // Boolean
            {
                return reader.ReadBoolean();
            }
            else if (type == 0x09) // DateTime
            {
                var ts = reader.ReadInt64();

                // catch specific values for MaxValue / MinValue #19
                if (ts == 253402300800000) return DateTime.MaxValue;
                if (ts == -62135596800000) return DateTime.MinValue;

                var date = LiteDB.BsonValue.UnixEpoch.AddMilliseconds(ts);

                return _utcDate ? date : date.ToLocalTime();
            }
            else if (type == 0x0A) // Null
            {
                return LiteDB.BsonValue.Null;
            }
            else if (type == 0x10) // Int32
            {
                return reader.ReadInt32();
            }
            else if (type == 0x12) // Int64
            {
                return reader.ReadInt64();
            }
            else if (type == 0x13) // Decimal
            {
                return reader.ReadDecimal();
            }
            else if (type == 0xFF) // MinKey
            {
                return LiteDB.BsonValue.MinValue;
            }
            else if (type == 0x7F) // MaxKey
            {
                return LiteDB.BsonValue.MaxValue;
            }

            throw new NotSupportedException("BSON type not supported");
        }
    }

    internal class ByteReader
    {
        private byte[] _buffer;
        private int _length;
        private int _pos;

        public int Position { get { return _pos; } set { _pos = value; } }

        public ByteReader(byte[] buffer)
        {
            _buffer = buffer;
            _length = buffer.Length;
            _pos = 0;
        }

        public void Skip(int length)
        {
            _pos += length;
        }

        #region Native data types

        public Byte ReadByte()
        {
            var value = _buffer[_pos];

            _pos++;

            return value;
        }

        public Boolean ReadBoolean()
        {
            var value = _buffer[_pos];

            _pos++;

            return value == 0 ? false : true;
        }

        public UInt16 ReadUInt16()
        {
            _pos += 2;
            return BitConverter.ToUInt16(_buffer, _pos - 2);
        }

        public UInt32 ReadUInt32()
        {
            _pos += 4;
            return BitConverter.ToUInt32(_buffer, _pos - 4);
        }

        public UInt64 ReadUInt64()
        {
            _pos += 8;
            return BitConverter.ToUInt64(_buffer, _pos - 8);
        }

        public Int16 ReadInt16()
        {
            _pos += 2;
            return BitConverter.ToInt16(_buffer, _pos - 2);
        }

        public Int32 ReadInt32()
        {
            _pos += 4;
            return BitConverter.ToInt32(_buffer, _pos - 4);
        }

        public Int64 ReadInt64()
        {
            _pos += 8;
            return BitConverter.ToInt64(_buffer, _pos - 8);
        }

        public Single ReadSingle()
        {
            _pos += 4;
            return BitConverter.ToSingle(_buffer, _pos - 4);
        }

        public Double ReadDouble()
        {
            _pos += 8;
            return BitConverter.ToDouble(_buffer, _pos - 8);
        }

        public Decimal ReadDecimal()
        {
            _pos += 16;
            var a = BitConverter.ToInt32(_buffer, _pos - 16);
            var b = BitConverter.ToInt32(_buffer, _pos - 12);
            var c = BitConverter.ToInt32(_buffer, _pos - 8);
            var d = BitConverter.ToInt32(_buffer, _pos - 4);
            return new Decimal(new int[] { a, b, c, d });
        }

        public Byte[] ReadBytes(int count)
        {
            var buffer = new byte[count];

            System.Buffer.BlockCopy(_buffer, _pos, buffer, 0, count);

            _pos += count;

            return buffer;
        }

        #endregion

        #region Extended types

        public string ReadString()
        {
            var length = this.ReadInt32();
            var str = Encoding.UTF8.GetString(_buffer, _pos, length);
            _pos += length;

            return str;
        }

        public string ReadString(int length)
        {
            var str = Encoding.UTF8.GetString(_buffer, _pos, length);
            _pos += length;

            return str;
        }

        /// <summary>
        /// Read BSON string add \0x00 at and of string and add this char in length before
        /// </summary>
        public string ReadBsonString()
        {
            var length = this.ReadInt32();
            var str = Encoding.UTF8.GetString(_buffer, _pos, length - 1);
            _pos += length;

            return str;
        }

        public string ReadCString()
        {
            var pos = _pos;
            var length = 0;

            while (true)
            {
                if (_buffer[pos] == 0x00)
                {
                    var str = Encoding.UTF8.GetString(_buffer, _pos, length);
                    _pos += length + 1; // read last 0x00
                    return str;
                }
                else if (pos > _length)
                {
                    return "_";
                }

                pos++;
                length++;
            }
        }

        public DateTime ReadDateTime()
        {
            // fix #921 converting index key into LocalTime
            // this is not best solution because uctDate must be a global parameter
            // this will be review in v5
            var date = new DateTime(this.ReadInt64(), DateTimeKind.Utc);

            return date.ToLocalTime();
        }

        public Guid ReadGuid()
        {
            return new Guid(this.ReadBytes(16));
        }

        public LiteDB.ObjectId ReadObjectId()
        {
            return new LiteDB.ObjectId(this.ReadBytes(12));
        }

        // Legacy PageAddress structure: [uint, ushort]
        // public PageAddress ReadPageAddress()
        // {
        //     return new PageAddress(this.ReadUInt32(), this.ReadUInt16());
        // }

        public LiteDB.BsonValue ReadBsonValue(ushort length)
        {
            var type = (LiteDB.BsonType)this.ReadByte();

            switch (type)
            {
                case LiteDB.BsonType.Null: return LiteDB.BsonValue.Null;

                case LiteDB.BsonType.Int32: return this.ReadInt32();
                case LiteDB.BsonType.Int64: return this.ReadInt64();
                case LiteDB.BsonType.Double: return this.ReadDouble();
                case LiteDB.BsonType.Decimal: return this.ReadDecimal();

                case LiteDB.BsonType.String: return this.ReadString(length);

                case LiteDB.BsonType.Document: return new BsonReader(false).ReadDocument(this);
                case LiteDB.BsonType.Array: return new BsonReader(false).ReadArray(this);

                case LiteDB.BsonType.Binary: return this.ReadBytes(length);
                case LiteDB.BsonType.ObjectId: return this.ReadObjectId();
                case LiteDB.BsonType.Guid: return this.ReadGuid();

                case LiteDB.BsonType.Boolean: return this.ReadBoolean();
                case LiteDB.BsonType.DateTime: return this.ReadDateTime();

                case LiteDB.BsonType.MinValue: return LiteDB.BsonValue.MinValue;
                case LiteDB.BsonType.MaxValue: return LiteDB.BsonValue.MaxValue;
            }

            throw new NotImplementedException();
        }

        #endregion
    }

    internal static class BsonDocumentExtensions
    {
        public static T GetOrDefault<K, T>(this IDictionary<K, T> dict, K key, T defaultValue = default(T))
        {
            if (dict.TryGetValue(key, out T result))
            {
                return result;
            }

            return defaultValue;
        }

        public static LiteDB.BsonValue Index(this LiteDB.BsonValue source, string key)
        {
            if (source is LiteDB.BsonDocument doc)
            {
                return doc.RawValue.GetOrDefault(key, LiteDB.BsonValue.Null);
            }

            throw new NotSupportedException();
        }

        public static void WriteIndex(this LiteDB.BsonValue source, string key, LiteDB.BsonValue value)
        {
            if (source is LiteDB.BsonDocument doc)
            {
                doc.RawValue[key] = value ?? LiteDB.BsonValue.Null;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static LiteDB.BsonValue Index(this LiteDB.BsonValue source, int index)
        {
            if (source is LiteDB.BsonArray array)
            {
                return array.RawValue[index];
            }

            throw new NotSupportedException();
        }

        public static unsafe bool IsFullZero(this byte[] data)
        {
            fixed (byte* bytes = data)
            {
                int len = data.Length;
                int rem = len % (sizeof(long) * 16);
                long* b = (long*)bytes;
                long* e = (long*)(bytes + len - rem);

                while (b < e)
                {
                    if ((*(b) | *(b + 1) | *(b + 2) | *(b + 3) | *(b + 4) |
                        *(b + 5) | *(b + 6) | *(b + 7) | *(b + 8) |
                        *(b + 9) | *(b + 10) | *(b + 11) | *(b + 12) |
                        *(b + 13) | *(b + 14) | *(b + 15)) != 0)
                        return false;
                    b += 16;
                }

                for (int i = 0; i < rem; i++)
                    if (data[len - 1 - i] != 0)
                        return false;

                return true;
            }
        }
    }

    internal class IndexInfo
    {
        public string Collection { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
        public bool Unique { get; set; }
    }

    /// <summary>
    /// Interface to read current or old datafile structure - Used to shirnk/upgrade datafile from old LiteDB versions
    /// </summary>
    interface IFileReader
    {
        /// <summary>
        /// Get all collections name from database
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetCollections();

        /// <summary>
        /// Get all indexes from collection (except _id index)
        /// </summary>
        IEnumerable<IndexInfo> GetIndexes(string name);

        /// <summary>
        /// Get all documents from a collection
        /// </summary>
        IEnumerable<LiteDB.BsonDocument> GetDocuments(string collection);
    }

    internal class Constants
    {
        public const string HeaderPage_HEADER_INFO = "** This is a LiteDB file **";
        public const int PageAddress_SIZE = 5;
        public const int BasePage_SLOT_SIZE = 4;

        public const int DataService_MAX_DATA_BYTES_PER_PAGE =
            PAGE_SIZE - // 8192
            PAGE_HEADER_SIZE - // [32 bytes]
            BasePage_SLOT_SIZE - // [4 bytes]
            DataBlock_DATA_BLOCK_FIXED_SIZE; // [6 bytes];

        public const int DataBlock_DATA_BLOCK_FIXED_SIZE = 1 + // DataIndex
                                                 PageAddress_SIZE; // NextBlock

        public const int P_EXTEND = 0; // 00-00 [byte]
        public const int P_NEXT_BLOCK = 1; // 01-05 [pageAddress]
        public const int P_BUFFER = 6; // 06-EOF [byte[]]

        /// <summary>
        /// The size of each page in disk - use 8192 as all major databases
        /// </summary>
        public const int PAGE_SIZE = 8192;

        /// <summary>
        /// Header page size
        /// </summary>
        public const int PAGE_HEADER_SIZE = 32;

        /// <summary>
        /// Bytes used in encryption salt
        /// </summary>
        public const int ENCRYPTION_SALT_SIZE = 16;

        /// <summary>
        /// Define ShareCounter buffer as writable
        /// </summary>
        public static int BUFFER_WRITABLE = -1;

        /// <summary>
        /// Define index name max length
        /// </summary>
        public static int INDEX_NAME_MAX_LENGTH = 32;

        /// <summary>
        /// Max level used on skip list (index).
        /// </summary>
        public const int MAX_LEVEL_LENGTH = 32;

        /// <summary>
        /// Max size of a index entry - usde for string, binary, array and documents. Need fit in 1 byte length
        /// </summary>
        public const int MAX_INDEX_KEY_LENGTH = 1023;

        /// <summary>
        /// Get max length of 1 single index node
        /// </summary>
        public const int MAX_INDEX_LENGTH = 1400;

        /// <summary>
        /// Get how many slots collection pages will have for free list page (data/index)
        /// </summary>
        public const int PAGE_FREE_LIST_SLOTS = 5;

        /// <summary>
        /// Document limit size - 2048 data pages limit (about 16Mb - same size as MongoDB)
        /// Using 2047 because first/last page can contain less than 8150 bytes.
        /// </summary>
        public const int MAX_DOCUMENT_SIZE = 2047 * DataService_MAX_DATA_BYTES_PER_PAGE;

        /// <summary>
        /// Define how many transactions can be open simultaneously
        /// </summary>
        public const int MAX_OPEN_TRANSACTIONS = 100;

        /// <summary>
        /// Define how many pages all transaction will consume, in memory, before persist in disk. This amount are shared across all open transactions
        /// 100,000 ~= 1Gb memory
        /// </summary>
        public const int MAX_TRANSACTION_SIZE = 100_000; // 100_000 (default) - 1000 (for tests)

        /// <summary>
        /// Size, in PAGES, for each buffer array (used in MemoryStore)
        /// It's an array to increase after each extend - limited in highest value
        /// Each byte array will be created with this size * PAGE_SIZE
        /// Use minimal 12 to allocate at least 85Kb per segment (will use LOH)
        /// </summary>
        public static int[] MEMORY_SEGMENT_SIZES = new int[] { 12, 50, 100, 500, 1000 }; // 8Mb per extend

        /// <summary>
        /// Define how many documents will be keep in memory until clear cache and remove support to orderby/groupby
        /// </summary>
        public const int VIRTUAL_INDEX_MAX_CACHE = 2000;

        /// <summary>
        /// Define how many bytes each merge sort container will be created
        /// </summary>
        public const int CONTAINER_SORT_SIZE = 100 * PAGE_SIZE;
    }

    /// <summary>
    /// Internal class to read old LiteDB v4 database version (datafile v7 structure)
    /// </summary>
    internal class FileReaderV7 : IFileReader
    {
        // v7 uses 4k page size
        private const int V7_PAGE_SIZE = 4096;

        private readonly Stream _stream;
        private readonly LiteDB.BsonDocument _header;

        private byte[] _buffer = new byte[V7_PAGE_SIZE];

        public FileReaderV7(Stream stream, string password)
        {
            _stream = stream;

            // only userVersion was avaiable in old file format versions
            _header = this.ReadPage(0);

            if (password == null && _header["salt"].AsBinary.IsFullZero() == false)
            {
                throw new LiteDB.LiteException("Current data file requires password");
            }
        }

        /// <summary>
        /// Read all collection based on header page
        /// </summary>
        public IEnumerable<string> GetCollections()
        {
            return _header["collections"].AsDocument.Keys;
        }

        /// <summary>
        /// Read all indexes from all collection pages
        /// </summary>
        public IEnumerable<IndexInfo> GetIndexes(string collection)
        {
            var pageID = (uint)_header["collections"].AsDocument[collection].AsInt32;
            var page = this.ReadPage(pageID);

            foreach (var index in page["indexes"].AsArray)
            {
                string name = Regex.Replace(index.Index("name").AsString, @"[^a-z0-9]", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (name.Length > INDEX_NAME_MAX_LENGTH)
                {
                    name = name.Substring(0, INDEX_NAME_MAX_LENGTH);
                }

                yield return new IndexInfo
                {
                    Collection = collection,
                    Name = name,
                    Expression = index.Index("expression").AsString,
                    Unique = index.Index("unique").AsBoolean
                };
            }
        }

        /// <summary>
        /// Get all document using an indexInfo as start point (_id index).
        /// </summary>
        public IEnumerable<LiteDB.BsonDocument> GetDocuments(string collection)
        {
            var colPageID = (uint)_header["collections"].AsDocument[collection].AsInt32;
            var col = this.ReadPage(colPageID);
            var headPageID = (uint)col.Index("indexes").Index(0).Index("headPageID").AsInt32;

            var indexPages = this.VisitIndexPages(headPageID);

            foreach (var indexPageID in indexPages)
            {
                var indexPage = this.ReadPage(indexPageID);

                foreach (var node in indexPage["nodes"].AsArray)
                {
                    var dataBlock = node.Index("dataBlock");

                    // if datablock link to a data page
                    if (dataBlock.Index("pageID").AsInt32 != -1)
                    {
                        // read dataPage and data block
                        var dataPage = this.ReadPage((uint)dataBlock.Index("pageID").AsInt32);

                        if (dataPage["pageType"].AsInt32 != 4) continue;

                        var block = dataPage["blocks"].AsArray.FirstOrDefault(x => x.Index("index") == dataBlock.Index("index"))?.AsDocument;

                        if (block == null) continue;

                        // read byte[] from block or from extend pages
                        var data = block["extendPageID"] == -1 ?
                            block["data"].AsBinary :
                            this.ReadExtendData((uint)block["extendPageID"].AsInt32);

                        if (data.Length == 0) continue;

                        // BSON format still same from all version
                        var doc = LiteDB.BsonSerializer.Deserialize(data);

                        // change _id PK in _chunks collection
                        if (collection == "_chunks")
                        {
                            var parts = doc["_id"].AsString.Split('\\');

                            if (!int.TryParse(parts[1], out var n)) throw new LiteDB.LiteException("_id");

                            doc["_id"] = new LiteDB.BsonDocument
                            {
                                ["f"] = parts[0],
                                ["n"] = n
                            };
                        }

                        yield return doc;
                    }
                }
            }
        }

        /// <summary>
        /// Read all database pages from v7 structure into a flexible BsonDocument - only read what really needs
        /// </summary>
        private LiteDB.BsonDocument ReadPage(uint pageID)
        {
            if (pageID * V7_PAGE_SIZE > _stream.Length) return null;

            _stream.Position = pageID * V7_PAGE_SIZE; // v7 uses 4k page size

            _stream.Read(_buffer, 0, V7_PAGE_SIZE);

            // decrypt encrypted page (except header page - header are plain data)
            var reader = new ByteReader(_buffer);

            // reading page header
            var page = new LiteDB.BsonDocument
            {
                ["pageID"] = (int)reader.ReadUInt32(),
                ["pageType"] = (int)reader.ReadByte(),
                ["prevPageID"] = (int)reader.ReadUInt32(),
                ["nextPageID"] = (int)reader.ReadUInt32(),
                ["itemCount"] = (int)reader.ReadUInt16()
            };

            // skip freeByte + reserved
            reader.ReadBytes(2 + 8);

            #region Header (1)

            // read header
            if (page["pageType"] == 1)
            {
                var info = reader.ReadString(27);
                var ver = reader.ReadByte();

                if (string.CompareOrdinal(info, HeaderPage_HEADER_INFO) != 0 || ver != 7)
                {
                    throw new LiteDB.LiteException("");
                }

                // skip ChangeID + FreeEmptyPageID + LastPageID
                reader.ReadBytes(2 + 4 + 4);
                page["userVersion"] = (int)reader.ReadUInt16();
                page["password"] = reader.ReadBytes(20);
                page["salt"] = reader.ReadBytes(16);
                page["collections"] = new LiteDB.BsonDocument();

                var cols = reader.ReadByte();

                for (var i = 0; i < cols; i++)
                {
                    var name = reader.ReadString();
                    var colPageID = reader.ReadUInt32();

                    page["collections"].WriteIndex(name, (int)colPageID);
                }
            }

            #endregion

            #region Collection (2)

            // collection page
            else if (page["pageType"] == 2)
            {
                page["collectionName"] = reader.ReadString();
                page["indexes"] = new LiteDB.BsonArray();
                reader.ReadBytes(12);

                for (var i = 0; i < 16; i++)
                {
                    var index = new LiteDB.BsonDocument();

                    var field = reader.ReadString();
                    var eq = field.IndexOf('=');

                    if (eq > 0)
                    {
                        index["name"] = field.Substring(0, eq);
                        index["expression"] = field.Substring(eq + 1);
                    }
                    else
                    {
                        index["name"] = field;
                        index["expression"] = "$." + field;
                    }

                    index["unique"] = reader.ReadBoolean();
                    index["headPageID"] = (int)reader.ReadUInt32();

                    // skip HeadNode (index) + TailNode + FreeIndexPageID
                    reader.ReadBytes(2 + 6 + 4);

                    if (field.Length > 0)
                    {
                        page["indexes"].AsArray.Add(index);
                    }
                }
            }

            #endregion

            #region Index (3)
            else if (page["pageType"] == 3)
            {
                page["nodes"] = new LiteDB.BsonArray();

                for (var i = 0; i < page["itemCount"].AsInt32; i++)
                {
                    var node = new LiteDB.BsonDocument
                    {
                        ["index"] = (int)reader.ReadUInt16()
                    };

                    var levels = reader.ReadByte();

                    // skip Slot + PrevNode + NextNode
                    reader.ReadBytes(1 + 6 + 6);

                    var length = reader.ReadUInt16();

                    // skip DataType + KeyValue
                    reader.ReadBytes(1 + length);

                    node["dataBlock"] = new LiteDB.BsonDocument
                    {
                        ["pageID"] = (int)reader.ReadUInt32(),
                        ["index"] = (int)reader.ReadUInt16()
                    };

                    // reading Prev[0]
                    node["prev"] = new LiteDB.BsonDocument
                    {
                        ["pageID"] = (int)reader.ReadUInt32(),
                        ["index"] = (int)reader.ReadUInt16()
                    };

                    // reading Next[0]
                    node["next"] = new LiteDB.BsonDocument
                    {
                        ["pageID"] = (int)reader.ReadUInt32(),
                        ["index"] = (int)reader.ReadUInt16()
                    };

                    // skip Prev/Next[1..N]
                    reader.ReadBytes((levels - 1) * (6 + 6));

                    page["nodes"].AsArray.Add(node);
                }
            }

            #endregion

            #region Data (4)
            else if (page["pageType"] == 4)
            {
                page["blocks"] = new LiteDB.BsonArray();

                for (var i = 0; i < page["itemCount"].AsInt32; i++)
                {
                    var block = new LiteDB.BsonDocument
                    {
                        ["index"] = (int)reader.ReadUInt16(),
                        ["extendPageID"] = (int)reader.ReadUInt32()
                    };

                    var length = reader.ReadUInt16();

                    block["data"] = reader.ReadBytes(length);

                    page["blocks"].AsArray.Add(block);
                }
            }

            #endregion

            #region Extend (5)
            else if (page["pageType"] == 5)
            {
                page["data"] = reader.ReadBytes(page["itemCount"].AsInt32);
            }

            #endregion

            return page;
        }

        public int UserVersion => (int)_header["userVersion"];

        /// <summary>
        /// Read extend data block
        /// </summary>
        private byte[] ReadExtendData(uint extendPageID)
        {
            // read all extended pages and build byte array
            using (var buffer = new MemoryStream())
            {
                while (extendPageID != uint.MaxValue)
                {
                    var page = this.ReadPage(extendPageID);

                    if (page["pageType"].AsInt32 != 5) return new byte[0];

                    buffer.Write(page["data"].AsBinary, 0, page["itemCount"].AsInt32);

                    extendPageID = (uint)page["nextPageID"].AsInt32;
                }

                return buffer.ToArray();
            }
        }

        /// <summary>
        /// Visit all index pages by starting index page. Get a list with all index pages from a collection
        /// </summary>
        private HashSet<uint> VisitIndexPages(uint startPageID)
        {
            var toVisit = new HashSet<uint>(new uint[] { startPageID });
            var visited = new HashSet<uint>();

            while (toVisit.Count > 0)
            {
                var indexPageID = toVisit.First();

                toVisit.Remove(indexPageID);

                var indexPage = this.ReadPage(indexPageID);

                if (indexPage == null || indexPage["pageType"] != 3) continue;

                visited.Add(indexPageID);
                foreach (var node in indexPage["nodes"].AsArray)
                {
                    var prev = (uint)node.Index("prev").Index("pageID").AsInt32;
                    var next = (uint)node.Index("next").Index("pageID").AsInt32;

                    if (!visited.Contains(prev)) toVisit.Add(prev);
                    if (!visited.Contains(next)) toVisit.Add(next);
                }
            }

            return visited;
        }
    }
}