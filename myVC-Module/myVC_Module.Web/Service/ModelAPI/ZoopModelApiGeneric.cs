using System;
using System.Collections.Generic;

namespace Zoop.ModelApi
{
    public class Error
    {
        public string status { get; set; }
        public int status_code { get; set; }
        public string type { get; set; }
        public string category { get; set; }
        public string message { get; set; }
        public string message_display { get; set; }
        public string response_code { get; set; }        
        public List<string> reasons { get; set; }
    }

    public class Generic
    {
        public Error error { get; set; }
    }

}
