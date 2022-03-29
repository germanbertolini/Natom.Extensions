using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Natom.Extensions.Common.Helpers
{
    public static class RequestHelper
    {
        public static string GetOSFromUserAgent(string agent)
        {
            string os = null;

            if (!string.IsNullOrEmpty(agent))
            {
                //OPERATIVE SYSTEM
                int startPoint = agent.IndexOf('(') + 1;
                int endPoint = agent.IndexOf(')');

                if (startPoint <= 0 || endPoint <= 0)
                    os = agent;
                else
                    os = agent.Substring(startPoint, (endPoint - startPoint));

                if (agent.Contains("Windows"))
                    os = os.Split(';').Where(m => m.Contains("Windows")).FirstOrDefault() ?? os;
                else if (agent.Contains("iOS"))
                    os = os.Split(';').Where(m => m.Contains("iOS")).FirstOrDefault() ?? os;
                else if (agent.Contains("Android"))
                    os = os.Split(';').Where(m => m.Contains("Android")).FirstOrDefault() ?? os;

                //WEB BROWSER
                var regex = new Regex(@"/(firefox|msie|chrome|safari)[/\s]([\d.]+)/ig");
                var matches = regex.Matches(agent);
                if (matches.Count > 0)
                {
                    os += ";" + String.Join(';', matches.ToList().Select(m => m.Value));
                }

                //WINDOWS APPLICATIONS
                os = os.Replace("Microsoft ", string.Empty);

            }
            return os;
        }
    }
}
