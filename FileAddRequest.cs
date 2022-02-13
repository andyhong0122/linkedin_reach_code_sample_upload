using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sabio.Models.Requests
{
    public class FileAddRequest
    {
        [Required]
        [MinLength(2), MaxLength(300)]
        public string Url { get; set; }
        [Required]
        public int FileTypeId { get; set; }
    }
}