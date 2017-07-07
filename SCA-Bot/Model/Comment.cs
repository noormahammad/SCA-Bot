using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCA_Bot.Model
{
    public class Comment
    {
        public Comment()
        {

        }
        public string CommentId { get; set; }

        public string ProjectCode { get; set; }
        public string ProjectLevel { get; set; }
        public string CommentType { get; set; }
        public string CommentText { get; set; }
        public string CreatedDate { get; set; }
        public string LastUpdatedDate { get; set; }

        public string CRUDflag { get; set; }

    }
}