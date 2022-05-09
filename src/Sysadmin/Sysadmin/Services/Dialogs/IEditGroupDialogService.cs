﻿using SysAdmin.ActiveDirectory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SysAdmin.Services.Dialogs
{
    public interface IEditGroupDialogService
    {

        GroupEntry Group { get; set; }

        Task<bool?> ShowDialog(object xamlRoot);
    }
}
