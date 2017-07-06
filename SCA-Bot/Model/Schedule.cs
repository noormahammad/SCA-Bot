using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCA_Bot.Model
{
    public class Schedule
    {
        public Schedule()
        {

        }

        public string ProjectCode { get; set; }
        public string ProjectLevel { get; set; }
        public string PhaseCode { get; set; }
        public string PhaseName { get; set; }
        public string OriginalBeginDate { get; set; }
        public string OriginalEndDate { get; set; }
        public string ForecastBeginDate { get; set; }
        public string ForecastEndDate { get; set; }
        public string ActualBeginDate { get; set; }
        public string ActualEndDate { get; set; }
        public int OrderbyPhase { get; set; }

        public string CRUDflag { get; set; }
    }
}