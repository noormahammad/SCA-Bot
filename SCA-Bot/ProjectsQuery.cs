namespace SCA_Bot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class ProjectsQuery
    {
        [Prompt("Please enter {&}")]
        [Optional]
        public string SchoolName { get; set; }

        [Prompt("Please enter {&}")]
        [Optional]
        public string Borough { get; set; }

        [Prompt("Please enter {&}")]
        [Optional]
        public string DesignBundle { get; set; }

    }
}