﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("EXAMINE")]
    public class ExamineCommand : BaseCommand
    {
        private string _mailbox;
        public string MailBox
        {
            get
            {
                return this._mailbox;
            }
        }

        public override void Parse(string Tag, string Data)
        {
            base.Parse(Tag, Data);
            string[] args = GetDataTokens(1);
            this._mailbox = args[0];
        }
    }
}
