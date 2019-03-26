using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AgentTransferBot.Models
{
 public enum UserExp
    {
        Purchase,
        Cashout,
        Rehab
    }

    public enum YesNo
    {
        Yes,
        No
    }

    [Serializable]
    public class QQForm
    {
        [Prompt("Entity {||}")]
        public YesNo? Entity;

        [Prompt("Please enter your Fico Score.")]
        public string FicoScore;

        [Prompt("Choose your {&} {||}")]
        public UserExp? LoanPurpose;

        public static IForm<QQForm> BuildForm()
        {
            return new FormBuilder<QQForm>().Build();
        }
    }

   
}