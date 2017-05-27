using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCA_Bot
{
    [Serializable]
    public class Project
    {
        public string LLWDescription { get; set; }

        public string SchoolName { get; set; }

        public string Status { get; set; }

        public int LLW { get; set; }

        public int DesignBundle { get; set; }

        public string Image { get; set; }

        public string BuildingAddress { get; set; }

        public string Borough { get; set; }

        public string ProjectType { get; set; }

        public int AuthorizedTotalAmount { get; set; }

        public int ConstructionCost { get; set; }

        public string ProjectOfficer { get; set; }

        public string DesignProjectManager { get; set; }
    }
}