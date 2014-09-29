﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("COPY")]
    public class CopyCommand : BaseCommand, IMailProcessCommand
    {
        public bool IsUIDCommand { get; set; }
        public MessageIDList MessageID { get; set; }
    }
}
