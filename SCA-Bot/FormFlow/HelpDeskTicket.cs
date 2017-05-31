using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SCA_Bot.FormFlow
{
    [Serializable]
    public class HelpDeskTicket
    {
        public TicketTypeOptions? TicketType;
        [Prompt("Which of the following {&} does your issue belongs to? {||}")]
        public CategoryOptions? Category;
        [Prompt("Please enter a brief description of your issue. {||}")]
        public string IssueDescription;
        public static IForm<HelpDeskTicket> BuildForm()
        {
            return new FormBuilder<HelpDeskTicket>().Message("Alright, I can help you creating a Help Desk ticket.").Build();

        }
    }

    public enum TicketTypeOptions
    {
        Incident,
        ServiceRequest
    }
    public enum CategoryOptions
    {
        Software,
        Hardware,
        Application
    }

   
   
}