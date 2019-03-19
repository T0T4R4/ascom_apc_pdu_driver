using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using APCPDULib;
using CommandLine;

namespace APCPDUTest
{
    class Program
    {

        public class Options
        {
            [Option('h', "host", Required = true, HelpText = "Hostname or ip address of the PDU unit")]
            public string Host { get; set; }

            [Option('p', "port", Required = false, HelpText = "SSH port to connect to on the PDU unit (defaults to 22)", Default = 22)]
            public int Port { get; set; }

            [Option('u', "username", Required = true, HelpText = "Username to connect to the PDU unit via SSH")]
            public string Username { get; set; }

            [Option('w', "password", Required = true, HelpText = "Password to connect to the PDU unit via SSH")]
            public string Password { get; set; }

            [Option('o', "outlet", Required = false, HelpText = "Test outlet Id", Default = 22)]
            public short TestOutletId{ get; set; }
        }

        static void HandleParseError(IEnumerable<Error> errors)
        {
            foreach (Error error in errors)
            {
                Console.WriteLine($"Wrong or missing value for {error.ToString()}");
            }
        }

        static void RunOptionsAndReturnExitCode(Options opts)
        {
            _options = opts;

            DoTests();
        }

        static Options _options;

        static void Main(string[] args)
        {
            // Parse command line arguments
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));


        }

        static void DoTests()
        {
            // Performs basic tests
            //

            APCPDU pdu = null;
            try
            {
                using (pdu = new APCPDU(_options.Host, _options.Port, _options.Username, _options.Password))
                {
                    pdu.Connect();

                    Console.WriteLine(string.Format("Connected: {0}", pdu.IsConnected));

                    if (pdu.IsConnected)
                    {
                        Console.WriteLine("Listing Outlets:");
                        pdu.Outlets.ForEach(o => Console.WriteLine($"Outlet '{o.Name}' is currently '{o.Status}'"));

                        if (pdu.Outlets.Count > 0)
                        {
                            var enabled = pdu.GetOutletStatus(_options.TestOutletId);
                            Console.WriteLine($"Status of outlet #1: {enabled}");
                        }

                    }
                }

            }
            catch (Exception e)
            {
                if (pdu != null)
                    pdu.Disconnect();

                Console.WriteLine(e.ToString());
            }
            finally
            {
                Console.Write("Application Ended. Please press a key to exit");
                Console.ReadKey();
            }
        }
    }
}
