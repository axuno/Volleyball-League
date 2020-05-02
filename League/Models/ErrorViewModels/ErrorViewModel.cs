using System;

namespace League.Models.Error
{
    public class ErrorViewModel
    {
        public string OrigPath { get; set; }

        public Exception Exception { get; set; }

        public string StatusCode { get; set; }

        public string StatusText { get; set; }

        public string Description { get; set; }
     }
}
