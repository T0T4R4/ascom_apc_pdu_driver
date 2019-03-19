// This implements a console application that can be used to test an ASCOM driver
//

// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.

#define Switch
// remove this to bypass the code that uses the chooser to select the driver
#define UseChooser

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace ASCOM
{
    public class Options
    {

        [Option('o', "outlet", Required = false, HelpText = "Outlet Id on which to perform ON/OFF tests", Default = 22)]
        public short OutletId { get; set; }


    }

    class Program
    {
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

#if UseChooser
            // choose the device
            string id = ASCOM.DriverAccess.Switch.Choose("ASCOM.APCPDU.Switch");
            if (string.IsNullOrEmpty(id))
                return;
            // create this device
            ASCOM.DriverAccess.Switch device = new ASCOM.DriverAccess.Switch(id);
#else
            // this can be replaced by this code, it avoids the chooser and creates the driver class directly.
            ASCOM.DriverAccess.Switch device = new ASCOM.DriverAccess.Switch("ASCOM.APCPDU.Switch");
#endif
            // now run some tests, adding code to your driver so that the tests will pass.
            // these first tests are common to all drivers.
            Console.WriteLine("name " + device.Name);
            Console.WriteLine("description " + device.Description);
            Console.WriteLine("DriverInfo " + device.DriverInfo);
            Console.WriteLine("driverVersion " + device.DriverVersion);

            // Connect to the driver.
            device.Connected = true;

            var sw_name = device.GetSwitchName(_options.OutletId);

            var sw_status = device.GetSwitch(_options.OutletId);
            Console.WriteLine($"Switch #{_options.OutletId} '{sw_name}' : {sw_status}");

            Console.WriteLine("Toggling status...");
            device.SetSwitch(_options.OutletId, !sw_status);

            sw_status = device.GetSwitch(_options.OutletId);
            Console.WriteLine($"Switch #{_options.OutletId} '{sw_name}' : {sw_status}");

            // Disconnect from the Device
            device.Connected = false;

            Console.WriteLine("Press Enter to finish");
            Console.ReadLine();
        }
    }
}
