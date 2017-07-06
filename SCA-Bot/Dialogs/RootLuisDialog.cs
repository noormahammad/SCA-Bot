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
using SCA_Bot.FormFlow;
using SCA_Bot.Model;
using System.Net.Http;
using SCA_Bot.Services;

namespace SCA_Bot.Dialogs
{
    [LuisModel("Your Model ID", "Your Subscription Key")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        #region Variables       

        private const string EntityNumber = "builtin.number";

        private const string EntitySchoolName = "SchoolName";

        private const string EntityBorough = "Borough";
        private const string EntityLLW = "LLW";
        private const string EntityDesignBundle = "DesignBundle";
        private const string EntityPackage = "Package";        

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
            await context.PostAsync("Hi! I am super-powered SCA Bot! Try asking me things like 'show me active projects in school 146', 'display schedule for Design bunlde 12012' or 'give me a list of construction projects in manhattan' or ask your own question.");

            context.Wait(this.MessageReceived);
        }
        #endregion

        #region CreateTicket Intent
        [LuisIntent("CreateTicket")]
        public async Task CreateTicket(IDialogContext context, LuisResult result)
        {
            var helpDeskTicketForm = FormDialog.FromForm(HelpDeskTicket.BuildForm, options: FormOptions.PromptInStart);
            context.Call(helpDeskTicketForm, HelpDeskTicketDialogResumeAfter);
        }
        private async Task HelpDeskTicketDialogResumeAfter(IDialogContext context, IAwaitable<HelpDeskTicket> result)
        {
            await context.PostAsync("Thanks! A Ticket has been created and sent to Help Desk! Someone will get in touch with you soon.");
            context.Wait(this.MessageReceived);
        }
        #endregion

        #region GetITTrainingCalendar
        [LuisIntent("GetITTrainingCalendar")]
        private async Task GetITTrainingCalendar(IDialogContext context, LuisResult result)
        {
            Attachment attachment1 = new Attachment();
            attachment1.Name = "SCA-IT-Training-Calendar.pdf";
            attachment1.ContentType = "application/pdf";
            attachment1.ContentUrl = "https://s3.amazonaws.com/nycsca-videos/sca-it-training-calendar.pdf";

            var message = context.MakeMessage();
            message.Attachments.Add(attachment1);
            message.Text = "Here is the schedule for IT Training";

            await context.PostAsync(message);
        }
        #endregion

        #region GetUserGuide
        [LuisIntent("GetUserGuide")]
        private async Task GetUserGuide(IDialogContext context, LuisResult result)
        {
            Attachment attachment1 = new Attachment();
            attachment1.Name = "eFleet-Mobile-App-User-Guide.ppt";
            attachment1.ContentType = "application/vnd.ms-powerpoint";
            attachment1.ContentUrl = "https://s3.amazonaws.com/nycsca-videos/efleet.ppt";

            var message = context.MakeMessage();
            message.Attachments.Add(attachment1);
            message.Text = "Following user guide describes how to use eFleet mobile app to log sca vehicle trip mileage.";

            await context.PostAsync(message);
        }
        #endregion

        #region DisplayVideo
        [LuisIntent("DisplayVideo")]
        private async Task DisplayVideo(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();

            var videoCard = new VideoCard
            {
                Title = "Review Estimate",
                Subtitle = "by SCA IT Dept.",
                Text = "This video shows how to login to CES/ProEST application and review estimates.",
                Image = new ThumbnailUrl
                {
                    Url = "https://s3.amazonaws.com/nycsca-pics/proest-thumbnail.JPG"
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "https://s3.amazonaws.com/nycsca-videos/proest1.mp4"
                    }
                },
                Buttons = new List<CardAction>
                            {
                                new CardAction()
                                {
                                    Title = "Login to review Estimate",
                                    Type = ActionTypes.OpenUrl,
                                    Value = "https://nycscauat.proest.com"
                                }
                            }
            };

            message.Attachments.Add(videoCard.ToAttachment());
            await context.PostAsync(message);
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
                        Subtitle = $"LLW# {project.LLW}. Constr.Cost {project.ConstructionCost.ToString("C0")}.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = project.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
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
                            },
                            new CardAction()
                            {
                                Title = "People",
                                Type = ActionTypes.ImBack,
                                Value = $"show people assigned to LLW {project.LLW}"
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
            else if(result.TryFindEntity(EntityNumber,out entityRecommendation))
            {
                entityRecommendation.Type = "Project";
                await context.PostAsync($"Looking up schedules for Project {entityRecommendation.Entity}");
            }

            if (entityRecommendation != null && !string.IsNullOrEmpty(entityRecommendation.Type))
            {
                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                ScheduleService scheduleService = new ScheduleService();
                List<Schedule> schedules = await scheduleService.GetSchedulesAsync(entityRecommendation.Entity);

                foreach (Schedule schedule in schedules)
                {
                    string beginDate;
                    string endDate;

                    if (!string.IsNullOrEmpty(schedule.ActualBeginDate))
                        beginDate = $"Actual Begin Date   {schedule.ActualBeginDate}";
                    else
                        beginDate = $"Forecast Begin Date   {schedule.ForecastBeginDate}";

                    if (!string.IsNullOrEmpty(schedule.ActualEndDate))
                        endDate = $"Actual End Date   {schedule.ActualEndDate}";
                    else
                        endDate = $"Forecast End Date   {schedule.ForecastEndDate}";

                    HeroCard card = new HeroCard()
                    {
                        Title = schedule.PhaseName,
                        Subtitle = $"{beginDate}          {endDate}" ,
                        Text = $"{entityRecommendation.Type}# {entityRecommendation.Entity}"
                    };
                    resultMessage.Attachments.Add(card.ToAttachment());
                }
                await context.PostAsync(resultMessage);
            }
            else
            {
                await context.PostAsync("Sorry, i am not able to find a valid project number in your message. please type something like 'show me schedule for db 12012''");
            }
            context.Wait(this.MessageReceived);
        }

        #endregion

        #region DisplayComments Intent
        [LuisIntent("DisplayComments")]
        public async Task DisplayComments(IDialogContext context, LuisResult result )
        {
            EntityRecommendation entityRecommendation;

            if (result.TryFindEntity(EntityLLW, out entityRecommendation))
            {
                entityRecommendation.Type = "LLW";
                await context.PostAsync($"Retrieving project comments/notes for LLW# {entityRecommendation.Entity}");
            }
            else if (result.TryFindEntity(EntityDesignBundle, out entityRecommendation))
            {
                entityRecommendation.Type = "DesignBundle";
                await context.PostAsync($"Retrieving project comments/notes for Design Bundle# {entityRecommendation.Entity}");
            }
            else if (result.TryFindEntity(EntityPackage, out entityRecommendation))
            {
                entityRecommendation.Type = "Package";
                await context.PostAsync($"Retrieving project comments/notes for Package No# {entityRecommendation.Entity}");
            }
            else if (result.TryFindEntity(EntityNumber, out entityRecommendation))
            {
                entityRecommendation.Type = "Project";
                await context.PostAsync($"Retrieving project comments/notes for Project {entityRecommendation.Entity}");
            }

            if (entityRecommendation != null && !string.IsNullOrEmpty(entityRecommendation.Type))
            {
                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                for (int i = 0; i < 7; i++)
                {
                    HeroCard herocard = new HeroCard()
                    {
                        Title = $"ODC Memo/Comment #{i+1}",
                        Text =  GetComments()[i].Length > 120?GetComments()[i].Substring(0,120):GetComments()[i],
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More",
                                Type = ActionTypes.ImBack,
                                Value = $"More on ODC Memo/Comment #{i+1}"
                            }
                        }
                    };

                    resultMessage.Attachments.Add(herocard.ToAttachment());
                }
                await context.PostAsync(resultMessage);
            }
            else
            {
                await context.PostAsync("Sorry, i am not able to find a valid project number in your message. please type something like 'display comments for llw 12012', or 'show comments for design bundle 132043''");
            }
            context.Wait(this.MessageReceived);
        }
        #endregion

        #region DisplayCommentDetail Intent
        [LuisIntent("DisplayCommentDetail")]
        public async Task DisplayCommentDetail(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityRecommendation;

            if (result.TryFindEntity(EntityNumber, out entityRecommendation))
                await context.PostAsync($"Here is full text of the comment#{entityRecommendation.Entity}: {GetComments()[int.Parse(entityRecommendation.Entity)-1]}");
            else
                await context.PostAsync("Sorry, i did not get that! type help if you need assistance!");

            context.Wait(this.MessageReceived);
        }
        #endregion

        #region FindPeopleForProject
        [LuisIntent("FindPeopleForProject")]
        public async Task FindPeopleForProject(IDialogContext context, LuisResult result)
        {
            EntityRecommendation entityRecommendation;
            if (result.TryFindEntity(EntityLLW, out entityRecommendation))
            {
                entityRecommendation.Type = "LLW";
                await context.PostAsync($"Following people are assigned to LLW# {entityRecommendation.Entity}");
            }
            else if (result.TryFindEntity(EntityDesignBundle, out entityRecommendation))
            {
                entityRecommendation.Type = "DesignBundle";
                await context.PostAsync($"Following people are assigned to Design Bundle# {entityRecommendation.Entity}");
            }
            else if (result.TryFindEntity(EntityPackage, out entityRecommendation))
            {
                entityRecommendation.Type = "Package";
                await context.PostAsync($"Following people are assigned to Package No# {entityRecommendation.Entity}");
            }
            else if (result.TryFindEntity(EntityNumber, out entityRecommendation))
            {
                entityRecommendation.Type = "Project";
                await context.PostAsync($"Following people are assigned to Project {entityRecommendation.Entity}");
            }

            if (entityRecommendation != null && !string.IsNullOrEmpty(entityRecommendation.Type))
            {
                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                for (int i = 0; i < 6; i++)
                {
                    var random = new Random(i);
                    ThumbnailCard thumbnailCard = new ThumbnailCard()
                    {
                        Title = GetRoles()[i],
                        Text = GetPeople()[i,0],
                        Images = new List<CardImage>()
                        {
                            new CardImage { Url = GetPeople()[i,1] }
                        }
                    };

                    resultMessage.Attachments.Add(thumbnailCard.ToAttachment());
                }
                await context.PostAsync(resultMessage);
            }
            else
            {
                await context.PostAsync("Sorry, i am not able to find a valid project number in your message. please type something like 'display comments for llw 12012', or 'show comments for design bundle 132043''");
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
        private static string[] GetComments()
        {
            string[] comments = new string[] {
                        "Funding Comment:Per K. Maher - We have realized and confirmed with the Brooklyn BP that the $500,000 allocated for LLW# 102649 was intended to fund the STEM Lab under LLW# 97353. Please cancel LLW# 102649 with a note that the Brooklyn Borough President intended to fund the Stem Lab under LLW# 97353. Transferred $500k from LLW 102649 to LLW 97353 from FY16 Reso A BP as an additional allocation. Additionally, please have the project tagged as referred to legislator as it is still underfunded at this time and cannot proceed without sufficient funding in place.",
                        "Funding Comment:Per C.O'Leary and C. Liu, increase the project budget to $1M. ",
                        "Bernard F. and G.S.Debra were assigned as DM & DPM per Boardroom Meeting on 03/17/16.",
                        "Occupancy updated to September 2021 per OSP MEMO.",
                        "4/30/15 Changed FY15 Priority 1 to FY16 Priority 5. Await D15432 construction completion (Ext Work), forecast end 2/2/16F. ",
                        "08/24/2016 - Comments from OIS: BP Section 211. FandE ONLY.",
                        "08/20/2015 12:45:43 PM - Package D015345  was updated from CA.BA.AC  to CP.BA.PR.  - System Generated Solicitation Cancellation Reason: Bid come in well above estimate",
                        "08/20/2015 12:45:43 PM - Package D015345  was updated from CA.BA.AC  to CP.BA.PR.  - System Generated Solicitation Cancellation Reason: Bid come in well above estimate",
                        "Bid & Award process started on 04/09/15 and the project was awarded on 08/26/15.",
                        "Per A&E re-forecasted Turnover date from 09/04/15 to 09/11/15.",
                        "Per M. Redelick  - Changed agency from SCA to DSF. ",
                        "A&E re-forecasted T/O date from 01/04/16 to 01/15/16 due to leasing issue."
            };
            return comments;
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

        private static string[] GetRoles()
        {
            string[] roles = new string[]
            {
                "Project Officer",
                "SPO",
                "DPM",
                "DM",
                "Studio Director",
                "CPO"
            };
            return roles;
        }
        public static string[,] GetPeople()
        {
            string[,] people = new string[,]
            {
                {"HEGHES, CORNEL","https://s3.amazonaws.com/nycsca-pics/people/HEGHESCORNEL.JPG" },
                { "TIEDEMANN, ERIC", "https://s3.amazonaws.com/nycsca-pics/people/TIDEMANNERICP.JPG"},
                { "SAINZ, CARLOS","https://s3.amazonaws.com/nycsca-pics/people/SAINGCARLOS.JPG"},
                { "MORRISON, CLEVELAND","https://s3.amazonaws.com/nycsca-pics/people/MORRISONCLEVELAND.JPG"},
                { "ABNERI, ELAN","https://s3.amazonaws.com/nycsca-pics/people/ABNERIELAN.JPG"},
                { "COLOMBO, CARL","https://s3.amazonaws.com/nycsca-pics/people/COLOMBOCARL.JPG"}
            };
            return people;
        }
        #endregion

    }
}
