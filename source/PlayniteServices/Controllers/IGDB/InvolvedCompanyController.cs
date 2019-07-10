using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    [Route("igdb/involved_companies")]
    public class InvolvedCompanyController : IgdbItemController
    {
        private static readonly object CacheLock = new object();

        private const string endpointPath = "involved_companies";

        [HttpGet("{companyId}")]
        public async Task<ServicesResponse<ExpandedInvolvedCompany>> Get(ulong companyId)
        {
            return await GetItem(companyId);
        }

        public static async Task<ServicesResponse<ExpandedInvolvedCompany>> GetItem(ulong companyId)
        {
            var company = await GetItem<InvolvedCompany>(companyId, endpointPath, CacheLock);
            var expandedCompany = new ExpandedInvolvedCompany();
            company.CopyProperties(expandedCompany, false, new List<string>()
            {
                nameof(InvolvedCompany.company)
            });

            expandedCompany.company = (await CompanyController.GetItem(company.company)).Data;
            return new ServicesResponse<ExpandedInvolvedCompany>(expandedCompany);
        }
    }
}
