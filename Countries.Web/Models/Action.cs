using System.Collections.Generic;

namespace Countries.Web.Models
{
    public class Action
    {
        public string Template { get; set; }
        public List<string> Methods { get; set; } = new List<string>();
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    }
}
