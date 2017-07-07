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
    public class CommentsService
    {
        private string CommentsApiUrl = WebConfigurationManager.AppSettings["SCAApiUrlbaseAddress"] + "api/comment/Get?llwCode=";
        private string HistoricalCommentsApiUrl = WebConfigurationManager.AppSettings["SCAApiUrlbaseAddress"] + "api/comment/History?llwCode=";

        private static readonly string ApiKey = WebConfigurationManager.AppSettings["SCA_API_KEY"];

        public async Task<List<Comment>> GetCommentsAsync(string llwCode)
        {
            List<Comment> comments = new List<Comment>();
            if (string.IsNullOrEmpty(llwCode))
                return comments;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("API_KEY", ApiKey);

                    HttpResponseMessage response = await client.GetAsync(CommentsApiUrl + llwCode);

                    if (response.IsSuccessStatusCode)
                        comments = await response.Content.ReadAsAsync<List<Comment>>();

                    //If no comments found, search for historical comments
                    if (comments.Count == 0)
                    {
                        response = await client.GetAsync(HistoricalCommentsApiUrl + llwCode);

                        if (response.IsSuccessStatusCode)
                            comments = await response.Content.ReadAsAsync<List<Comment>>();
                    }
                }
            }
            catch (Exception ex)
            {
            }

            if (comments.Count > 10)
                comments = comments.GetRange(0, 10);

            return comments;
        }
    }
}