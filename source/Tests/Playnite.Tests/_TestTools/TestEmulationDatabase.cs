using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Emulators;

namespace Playnite.Tests
{
    public class TestEmulationDatabaseReader : EmulationDatabase.IEmulationDatabaseReader
    {
        private readonly Func<string, IEnumerable<DatGame>> getByCrc;
        private readonly Func<string, IEnumerable<DatGame>> getByRomName;
        private readonly Func<string, IEnumerable<DatGame>> getByRomNamePartial;
        private readonly Func<string, IEnumerable<DatGame>> getBySerial;

        public string DatabaseName { get; internal set; }

        public TestEmulationDatabaseReader(
            string databaseName,
            Func<string, IEnumerable<DatGame>> getByCrc,
            Func<string, IEnumerable<DatGame>> getByRomName,
            Func<string, IEnumerable<DatGame>> getByRomNamePartial,
            Func<string, IEnumerable<DatGame>> getBySerial)
        {
            DatabaseName = databaseName;
            this.getByCrc = getByCrc;
            this.getByRomName = getByRomName;
            this.getByRomNamePartial = getByRomNamePartial;
            this.getBySerial = getBySerial;
        }

        public void Dispose()
        {
        }

        public IEnumerable<DatGame> GetByCrc(string checksum)
        {
            return getByCrc(checksum);
        }

        public IEnumerable<DatGame> GetByRomName(string romName)
        {
            return getByRomName(romName);
        }

        public IEnumerable<DatGame> GetByRomNamePartial(string romNamePart)
        {
            return getByRomNamePartial(romNamePart);
        }

        public IEnumerable<DatGame> GetBySerial(string serial)
        {
            return getBySerial(serial);
        }
    }
}
