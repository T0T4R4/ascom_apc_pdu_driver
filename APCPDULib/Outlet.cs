using System;
using System.Collections.Generic;
using System.Text;

namespace APCPDULib
{
    public class Outlet
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public string Status { get; internal set; }
        public bool Enabled { get { return this.Status == "On"; } }

        public static Outlet FromString(string line)
        {
            var tokens = line.Split(':'); // of the form NUMER:TEXT:ON|OFF

            return new Outlet()
            {
                Id = int.Parse(tokens[0].Trim()),
                Name = tokens[1].Trim(),
                Status = tokens[2].Trim()
            };

        }
    }
}
