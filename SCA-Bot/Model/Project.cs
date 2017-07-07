using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCA_Bot.Model
{ 
        public class Project
        {
            public Project()
            {
            }
 
            public string LLWCode { get; set; }
            public string LLWDescription { get; set; }
            public string CapitalCategoryCode { get; set; }
            public string StationCode { get; set; }
            public string StationDesc { get; set; }
            public string ActivityCode { get; set; }
            public string ActivityDesc { get; set; }
            public string SASStatus { get; set; }
            public string DesignCode { get; set; }
            public string PackageCode { get; set; }
            public string PackageDesc { get; set; }
            public string ContractNo { get; set; }
            public string ContractorName { get; set; }
            public string FMSNo { get; set; }
            public string SubmissionNo { get; set; }
            public string GroupNumber { get; set; }
            public string DSFNumber { get; set; }
            public string SolicitationNo { get; set; }
            public string CommitPlanYear { get; set; }
            public string StudioName { get; set; }
            public string StudioDesc { get; set; }
            public string StudioDirector { get; set; }
            public string DesignManager { get; set; }
            public string DesignProjectManager { get; set; }
            public string DesignConsultant { get; set; }
            public string SeniorProjectOfficer { get; set; }
            public string ChiefProjectOfficer { get; set; }
            public string ProjectOfficer { get; set; }
            public string OrgCode { get; set; }
            public string OrgName { get; set; }
            public string BuildingId { get; set; }
            public string BuildingAddress { get; set; }
            public string BuildingName { get; set; }
            public string Borough { get; set; }
            public int GeoDistrict { get; set; }
            public string CouncilDistrict { get; set; }
            public int OrgJurDistrict { get; set; }
            public string CapProjCategory { get; set; }
            public string ProjectName { get; set; }
            public string ProjectAlias { get; set; }
            public string ProjectType { get; set; }
            public string ManagementType { get; set; }
            public string RequestorCode { get; set; }
            public string Transportable { get; set; }
            public string MentorType { get; set; }
            public string CityWide { get; set; }
            public string Joc { get; set; }
            public string CharterMatch { get; set; }
            public string RequestType { get; set; }
            public string FECode { get; set; }
            public decimal FEComponentCategoryCode { get; set; }
            public string FEComponentCategoryDesc { get; set; }
            public string RebidFlag { get; set; }
            public string SchoolCode { get; set; }
            public string FundingSourceDesc { get; set; }
            public decimal DOEConstructAmt { get; set; }
            public decimal DOEAuthTotalAmt { get; set; }
            public decimal OriginalConstTotal { get; set; }
            public decimal OriginalPlanTotal { get; set; }
            public decimal LatestProgramGross { get; set; }
            public decimal LatestDesignGross { get; set; }
            public decimal LatestAdjustedCapacity { get; set; }
            public decimal LatestD75Seats { get; set; }
            public decimal LatestConstructionCost { get; set; }
            public string AdditionalWork { get; set; }
            public string ReasonCode { get; set; }
            public string ReasonCodeDesc { get; set; }
            public string FYPNo { get; set; }
            public string UpdatedBy { get; set; }
            public string UpdatedOn { get; set; }
            public string ContractMethod { get; set; }
            public string StakeHolder { get; set; }
            public string SpacePlanner { get; set; }
            public string BCCPlanExaminerDOB { get; set; }
            public string OccupancyDate { get; set; }
            public string CAPTrusteeYear { get; set; }
            public string NewSeats { get; set; }
            public string ExistingSeats { get; set; }
            public string SeatType { get; set; }
            public string ResoAFlag { get; set; }
            public string ResoAYear { get; set; }
            public string ResoAFund { get; set; }
            public string LLWUpdatesCompleted { get; set; }
            public string ProjectControlsRevProg { get; set; }
            public string CapitalPlanningReview { get; set; }
            public string PriorityDesc { get; set; }
            public string MMRReview { get; set; }
            public string CapitalCategoryDesc { get; set; }
            public decimal AwardAmount { get; set; }
            public decimal RevisedEngineerEstimate { get; set; }
            public string SandyProject { get; set; }

            public string CRUDflag { get; set; }
            public string SearchCriteria { get; set; }
        }
}