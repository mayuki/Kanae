using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kanae.Data;

namespace Kanae.Web.ViewModels.Manage
{
    public class ManageIndexViewModel
    {
        public IEnumerable<MediaInfo> Items { get; set; }
    }
}