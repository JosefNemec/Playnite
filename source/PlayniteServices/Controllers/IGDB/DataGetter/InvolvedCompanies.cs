using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Filters;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB.DataGetter
{
    public class InvolvedCompanies : DataGetter<ExpandedInvolvedCompany>
    {
        private static readonly object cacheLock = new object();
        private Companies companies;

        public InvolvedCompanies(IgdbApi igdbApi) : base(igdbApi, "involved_companies", cacheLock)
        {
            companies = new Companies(igdbApi);
        }

        public override Task<ExpandedInvolvedCompany> Get(ulong ageId)
        {
            return GetItem(ageId);
        }

        public async Task<ExpandedInvolvedCompany> GetItem(ulong companyId)
        {
            var company = await igdbApi.GetItem<InvolvedCompany>(companyId, endpointPath, collectonLock);
            var expandedCompany = new ExpandedInvolvedCompany();
            company.CopyProperties(expandedCompany, false, new List<string>()
            {
                nameof(InvolvedCompany.company)
            });

            expandedCompany.company = await companies.Get(company.company);
            return expandedCompany;
        }
    }
}
