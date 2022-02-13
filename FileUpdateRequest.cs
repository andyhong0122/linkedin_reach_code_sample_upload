using System;
using System.Collections.Generic;
using System.Text;

namespace Sabio.Models.Requests
{
    public class FileUpdateRequest: FileAddRequest, IModelIdentifier
    {
        public int Id { get; set; }
    }
}