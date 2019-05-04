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

        [Option('o', "outlet", Required = false, HelpText = "PDU Outlet Id on which to perform ON/OFF tests")]
        public string OutletId { get; set; }


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

            try
            {

                // Connect to the driver.
                device.Connected = true;

                if (device.Connected)
                {
                    Console.WriteLine("Listing Outlets:");
                    for (short i = 0; i < device.MaxSwitch; i++)
                    {
                        var outlet_id = device.GetSwitchName(i);
                        var outlet_name = device.GetSwitchName(i);
                        var outlet_isOn = device.GetSwitch(i);
                        Console.WriteLine($"   #{outlet_id} '{outlet_name}' = {outlet_isOn}");
                    }

                    if (!string.IsNullOrEmpty(_options.OutletId))
                    {
                        // is there an outlet with this Id ?
                        for (short i = 0; i < device.MaxSwitch; i++)
                        {
                            if (device.GetSwitchName(i) == _options.OutletId)
                            {
                                var state = device.GetSwitch(i);
                                device.SetSwitch(i, !state); // toggle switch state
                                var newstate = device.GetSwitch(i);
                                Console.WriteLine((state == newstate) ? "Toggle Successful" : "Toggle failed");
                                break;
                            }
                        }
                    }

                }

                // Disconnect from the Device
                device.Connected = false;

                Console.WriteLine("Press Enter to finish");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}
