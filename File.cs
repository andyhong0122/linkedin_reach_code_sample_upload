
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Linq;

namespace Sabio.Models.Domain
{
    public class File
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }

    }
}