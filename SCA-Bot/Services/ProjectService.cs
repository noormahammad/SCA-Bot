using SCA_Bot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace SCA_Bot.Services
{
    public class ProjectService
    {
        private string ProjectsApiUrl = WebConfigurationManager.AppSettings["SCAApiUrlbaseAddress"];

        private static readonly string ApiKey = WebConfigurationManager.AppSettings["SCA_API_KEY"];
        /// <summary>
        /// Searches for active Projects. criteria can be any partial value of school name, llw code, design bundle code, package code, project name, borough, building id,
        /// contractor, project officer, senior project officer, building name, buidling id, chief project officer, etc
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public async Task<List<Project>> GetProjectsAsync(string searchCriteria)
        {
            List<Project> projects = new List<Project>();
            if (string.IsNullOrEmpty(searchCriteria.Trim()))
                return projects;
            
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("API_KEY", ApiKey);
                    
                    HttpResponseMessage response = await client.GetAsync($"{ProjectsApiUrl}api/project/search/{searchCriteria.Trim()}");

                    if (response.IsSuccessStatusCode)
                        projects = await response.Content.ReadAsAsync<List<Project>>();

                    //If no comments found, search for historical comments
                    if (projects.Count == 0)
                    {
                        response = await client.GetAsync($"{ProjectsApiUrl}api/project/BuildingHistory?searchCriteria={searchCriteria.Trim()}");

                        if (response.IsSuccessStatusCode)
                            projects = await response.Content.ReadAsAsync<List<Project>>();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if(projects.Count > 10)            
                projects = projects.GetRange(0, 10);
            
            return projects;
        }
        /// <summary>
        /// Retrieves project details based on LLW Code, Design bundle code or Package Code
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public async Task<Project> GetProjectsDetailsAsync(string code)
        {
            Project project = new Project();
            if (string.IsNullOrEmpty(code))
                return project;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("API_KEY", ApiKey);

                    HttpResponseMessage response = await client.GetAsync($"{ProjectsApiUrl}api/project/details/{code}");

                    if (response.IsSuccessStatusCode)
                        project = await response.Content.ReadAsAsync<Project>();
                }
            }
            catch (Exception ex)
            {
            }
            
            return project;
        }

    }
}