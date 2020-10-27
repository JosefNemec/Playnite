using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB.DataGetter
{
    public class DataGetter<T>
    {
        internal object collectonLock;
        internal string endpointPath;
        internal IgdbApi igdbApi;

        public DataGetter(IgdbApi igdbApi, string endpointPath, object collectonLock)
        {
            this.igdbApi = igdbApi;
            this.endpointPath = endpointPath;
            this.collectonLock = collectonLock;
        }

        public virtual async Task<T> Get(ulong ageId)
        {
            return await igdbApi.GetItem<T>(ageId, endpointPath, collectonLock);
        }
    }
}
