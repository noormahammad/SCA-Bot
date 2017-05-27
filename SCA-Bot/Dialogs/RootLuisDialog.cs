using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using Microsoft.Bot.Builder.FormFlow;
using System.Linq;
using System.Web;

namespace SCA_Bot.Dialogs
{
    [LuisModel("1c4c74d8-0558-4dd5-ac38-4893e87568d1", "c2c950862578433c834ffc4ae5341bc2")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        #region Variables
        private const string EntityGeographyCity = "builtin.geography.city";

        private const string EntitySchoolName = "SchoolName";

        private const string EntityBorough = "Borough";
        private const string EntityLLW = "LLW";
        private const string EntityDesignBundle = "DesignBundle";
        private const string EntityPackage = "Package";

        private IList<string> PhaseOptions = new List<string> { "Pre-Scope", "Scope", "Design", "Bid & Award", "Request for Contract", "Construction", "Closeout" };
        #endregion

        #region None Intent
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }
        #endregion

        #region Help Intent
        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi! Try asking me things like 'show me active projects in school 146', 'display schedule for Design bunlde 12012' or 'give me a list of construction projects in manhattan' or ask your own question.");

            context.Wait(this.MessageReceived);
        }
        #endregion

        #region SearchProjects Intent

        [LuisIntent("SearchProjects")]
        public async Task Search(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;

            var projectsQuery = new ProjectsQuery();

            //EntityRecommendation cityEntityRecommendation;

            //if (result.TryFindEntity(EntityGeographyCity, out cityEntityRecommendation))
            //{
            //    cityEntityRecommendation.Type = "Destination";
            //}

            var projectsFormDialog = new FormDialog<ProjectsQuery>(projectsQuery, this.BuildProjectsForm, FormOptions.PromptInStart, result.Entities);

            context.Call(projectsFormDialog, this.ResumeAfterProjectsFormDialog);
        }

        private IForm<ProjectsQuery> BuildProjectsForm()
        {
            OnCompletionAsyncDelegate<ProjectsQuery> processProjectsSearch = async (context, state) =>
            {
                var message = "Searching for projects";
                if (!string.IsNullOrEmpty(state.SchoolName))
                {
                    message += $" in {state.SchoolName}...";
                }
                else if (!string.IsNullOrEmpty(state.Borough))
                {
                    message += $" near {state.Borough.ToUpperInvariant()} airport...";
                }

                await context.PostAsync(message);
            };

            return new FormBuilder<ProjectsQuery>()
                .Field(nameof(ProjectsQuery.SchoolName), (state) => string.IsNullOrEmpty(state.Borough))
                .Field(nameof(ProjectsQuery.Borough), (state) => string.IsNullOrEmpty(state.SchoolName))
                .OnCompletion(processProjectsSearch)
                .Build();
        }

        private async Task ResumeAfterProjectsFormDialog(IDialogContext context, IAwaitable<ProjectsQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var projects = await this.GetProjectsAsync(searchQuery);

                await context.PostAsync($"I found {projects.Count()} projects:");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var project in projects)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = $"{project.SchoolName} {project.LLWDescription}",
                        Subtitle = $"LLW# {project.LLW}. Cost {project.ConstructionCost.ToString("C0")}.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = project.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=nycsca+in+" + HttpUtility.UrlEncode(project.BuildingAddress)
                            },
                            new CardAction()
                            {
                                Title = "Schedules",
                                Type = ActionTypes.ImBack,
                                Value = $"Show Schedules for LLW {project.LLW}"
                            },
                            new CardAction()
                            {
                                Title = "Comments",
                                Type = ActionTypes.ImBack,
                                Value = $"Show Comments for LLW {project.LLW}"
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        #endregion

        #region DisplaySchedules Intent

        [LuisIntent("DisplaySchedule")]
        public async Task Schedules(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityRecommendation;

            if (result.TryFindEntity(EntityLLW, out entityRecommendation))
            {
                entityRecommendation.Type = "LLW";
                await context.PostAsync($"Looking up schedules for LLW# {entityRecommendation.Entity}");
            }
            else if (result.TryFindEntity(EntityDesignBundle, out entityRecommendation))
            {
                entityRecommendation.Type = "DesignBundle";
                await context.PostAsync($"Looking up schedules for Design Bundle# {entityRecommendation.Entity}");
            }
            else if (result.TryFindEntity(EntityPackage, out entityRecommendation))
            {
                entityRecommendation.Type = "Package";
                await context.PostAsync($"Looking up schedules for Package No# {entityRecommendation.Entity}");
            }

            if (entityRecommendation != null && !string.IsNullOrEmpty(entityRecommendation.Type))
            {
                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                for (int i = 0; i < 7; i++)
                {
                    HeroCard card = new HeroCard()
                    {
                        Title = PhaseOptions[i],
                        Subtitle = $"{entityRecommendation.Type}# {entityRecommendation.Entity}",
                        Images = new List<CardImage>()
                    {
                        new CardImage()
                        {
                          Url = $"https://placeholdit.imgix.net/~text?txtsize=12&txt=Actual%20Begin%20Date%2012/3/2017%0AActual%20End%20Date%205/1/2018%0A%0AForecast%20Begin%20Date%2012/3/2017%0AForecast%20End%20Date%205/1/2018&w=250&h=120&txttrack=1&txtclr=302&txtfont=bold"
                        }
                        }
                    };

                    resultMessage.Attachments.Add(card.ToAttachment());
                }
                await context.PostAsync(resultMessage);
            }
            context.Wait(this.MessageReceived);
        }

        #endregion

        #region Get Data for demo purposes

        private async Task<IEnumerable<Project>> GetProjectsAsync(ProjectsQuery searchQuery)
        {
            var projects = new List<Project>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                Project project = new Project()
                {
                    LLWDescription = GetLLWDescriptions()[random.Next(1, 14)],
                    SchoolName = $"{searchQuery.SchoolName ?? searchQuery.Borough}",
                    BuildingAddress = $"{GetAddresses()[random.Next(1, 24)]}, {GetBoroughs()[random.Next(1, 5)]}, NY",
                    LLW = random.Next(100000, 999999),
                    ConstructionCost = random.Next(950000, 4000000),
                    AuthorizedTotalAmount = random.Next(950000, 4000000),
                    //Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Project+{i}&w=500&h=260&txtclr=0{random.Next(0,9)}{random.Next(0,9)}&txtfont=bold"
                    Image = $"https://nycsca.imgix.net/{GetBuildingPics()[random.Next(1, 11)]}"
                };

                projects.Add(project);
            }

            projects.Sort((h1, h2) => h1.ConstructionCost.CompareTo(h2.ConstructionCost));

            return projects;
        }
        private static string[] GetLLWDescriptions()
        {
            string[] llwdescriptions = new string[] {"PRE-K CENETR",
                                                                "LEASE RENOVATIONS (PHASE I)",
                                                                "PRE-K CENTER PROJECT",
                                                                "ADDITION",
                                                                "HS Replacement",
                                                                "NEW BUILDING",
                                                                "FEASIBILITY STUDY:80-25 132 ST",
                                                                "STEM CENTER",
                                                                "LEASE RENOVATION",
                                                                "FY97 RESO A MODULARS",
                                                                "RESO A- BLACK BOX THEATER",
                                                                "PRE-K CENTER (Phase II Work)",
                                                                "ECF REPLACEMENT",
                                                                "DEMOLITION",
                                                                "EARLY DEMOLITION PACKAGE"
            };
            return llwdescriptions;
        }

        private static string[] GetAddresses()
        {
            string[] addresses = new string[] {
                                                "80-55 CORNISH AVENUE",
                                                "5018-5024 4TH AVENUE",
                                                "104-14 ROOSEVELT AVENUE",
                                                "47-01 111TH STREET",
                                                "21 WEST END AVENUE",
                                                "656 WEST 33RD STREET",
                                                "111-10 ASTORIA BOULEVARD",
                                                "15 FAIRFIELD STREET",
                                                "128-02 7TH AVENUE",
                                                "108-18 ROOSEVELT AVENUE",
                                                "57-08 99 STREET",
                                                "4302 4TH AVENUE",
                                                "227-243 WEST 61ST STREET",
                                                "54-25 101ST STREET",
                                                "34-74 113 STREET",
                                                "317 HOYT STREET",
                                                "227-243 WEST 61ST STREET",
                                                "1855 STILLWELL AVENUE",
                                                "104 GORDON STREET",
                                                "70-24 47TH AVENUE",
                                                "7805 7 AVENUE",
                                                "170-45 84TH AVENUE",
                                                "1375 MACE AVENUE",
                                                "1860 2ND AVENUE ",
                                                "128-02 7TH AVENUE"
                                            };
            return addresses;
        }

        private static string[] GetBoroughs()
        {
            string[] boroughs = new string[] {
                    "Manhattan",
                    "Queens",
                    "Bronx",
                    "Brooklyn",
                    "Staten Island",
                    "Long Island"
            };
            return boroughs;
        }

        private static string[] GetBuildingPics()
        {
            string[] pics = new string[] {
                                        "314SEP191ren.jpg",
                                        "315Q2.jpg",
                                        "ISHS868M.jpg",
                                        "PS170K.jpg",
                                        "PS320Q.jpg",
                                        "PS339Q.jpg",
                                        "PS343M.jpg",
                                        "PS35Qadditionexterior.jpg",
                                        "PS62R.jpg",
                                        "PS96XRendering.jpg",
                                        "PS971.jpg"
            };
            return pics;
        }
        #endregion

    }
}
