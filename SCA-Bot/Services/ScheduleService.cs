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
        private string HistoricalSchedulesApiUrl = WebConfigurationManager.AppSettings["SCAApiUrlbaseAddress"] + "api/schedule/History?llwCode=";

        private static readonly string ApiKey = WebConfigurationManager.AppSettings["SCA_API_KEY"];

        public async Task<List<Schedule>> GetSchedulesAsync(string llwCode)
        {            
            List<Schedule> schedules = new List<Schedule>();
            if (string.IsNullOrEmpty(llwCode))
                return schedules;

            try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("API_KEY",ApiKey);

                        HttpResponseMessage response = await client.GetAsync(SchedulesApiUrl + llwCode);

                        if (response.IsSuccessStatusCode)
                            schedules = await response.Content.ReadAsAsync<List<Schedule>>();
                        
                        //if no scheudles found, search for historical schedules
                        if (schedules.Count == 0)
                        {
                            response = await client.GetAsync(HistoricalSchedulesApiUrl + llwCode);

                            if (response.IsSuccessStatusCode)
                                schedules = await response.Content.ReadAsAsync<List<Schedule>>();
                        }
                    }                        
                }
                catch(Exception ex)
                {                   
                }

            if (schedules.Count > 10)
                schedules = schedules.GetRange(0, 10);

            return schedules;
        }
    }
}