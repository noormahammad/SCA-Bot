using SCA_Bot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace SCA_Bot.Services
{
    public class ScheduleService
    {
        private string SchedulesApiUrl =  WebConfigurationManager.AppSettings["SCAApiUrlbaseAddress"] + "api/schedule/Get?llwCode=";

        private static readonly string ApiKey = WebConfigurationManager.AppSettings["BingSpellCheckApiKey"];

        public async Task<List<Schedule>> GetSchedulesAsync(string llwCode)
        {            
            List<Schedule> schedules = new List<Schedule>();
            if (string.IsNullOrEmpty(llwCode))
                return schedules;

            try
                {
                    using (var client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(SchedulesApiUrl + llwCode);

                        if (response.IsSuccessStatusCode)
                            schedules = await response.Content.ReadAsAsync<List<Schedule>>();
                    }                        
                }
                catch(Exception ex)
                {                   
                }
            
            return schedules;
        }
    }
}