using System.Collections.Generic;

namespace ziopuzzle.WebRequest.Model
{
    public class HttpRequestModel
    {
        public string Method { get; set; } = "GET";
        public string Url { get; set; } = "";
        public Dictionary<string, string> Headers { get; set; } = [];
        public string Body { get; set; } = "";
        public string ResponseVariableName { get; set; } = "";
    }
}