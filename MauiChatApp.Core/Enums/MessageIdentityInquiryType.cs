﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiChatApp.Core.Enums
{
    public enum MessageIdentityInquiryType

    {
        All = 0, //You should only be available if the identity is set.
        CurrentRoom,
        CurrentUser,
        Users,
        Rooms,
        AvailableUsers //This will only be available in the demo verison. Identities should be logged in already.
    }
}
