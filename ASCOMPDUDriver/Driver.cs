//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Switch driver for APC Power Distribution Unit
//
// Description:	This is an ASCOM Driver which allows the control of an APC Power Distribution unit.
//              It uses the APCPDULib library.
//
// Implements:	ASCOM Switch interface version: 1.0.0
// Author:		T0T4R4 <matt.clayton.oz@gmail.com>
//


// This is used to define code in the template that is specific to one class implementation
// unused code canbe deleted and this definition removed.
#define Switch

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;

namespace ASCOM.APCPDU
{
    //
    // Your driver's DeviceID is ASCOM.APCPDU.Switch
    //
    // The Guid attribute sets the CLSID for ASCOM.APCPDU.Switch
    // The ClassInterface/None addribute prevents an empty interface called
    // _APCPDU from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Switch Driver for APCPDU.
    /// </summary>
    [Guid("548c3b08-62dd-4ab9-b737-06904925b5ac")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Switch : ISwitchV2
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.APCPDU.Switch";

        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "ASCOM Switch Driver for APC PDU.";


        // Constants used for Profile persistence
        internal static string
            traceStateProfileName = "Trace Level",
            hostProfileName = "Host",
            portProfileName = "Port",
            usernameProfileName = "Username",
            passwordProfileName = "Password",
            keepAliveIntervalName = "KeepAliveInterval"
            ;

        // Variables to hold the default device configuration
        internal static string
            traceStateDefault = "false",
            hostDefault = "",
            usernameDefault = "apc",
            passwordDefault = "apc";

        internal static int portDefault = 22; // default SSH port (Ensure that you have enabled SSH on your PDU)
        internal static int keepAliveIntervalDefault = 10;

        // Variables to hold the currrent device configuration
        internal static string pdu_host, pdu_username, pdu_password;
        internal static int pdu_port, pdu_keepAliveInterval;

        private static APCPDULib.APCPDU pdu = null;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal static TraceLogger tl;

        /// <summary>
        /// Initializes a new instance of the <see cref="APCPDU"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Switch()
        {
            tl = new TraceLogger(null, "APCPDU");

            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl.LogMessage("Switch", "Starting initialisation");

            utilities = new Util(); 
            astroUtilities = new AstroUtils(); 

            tl.LogMessage("Switch", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ISwitchV2 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm())
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            throw new ASCOM.ActionNotImplementedException("Action '" + actionName + "' is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            //CheckConnected("CommandBlind");
            //// Call CommandString and return as soon as it finishes
            //this.CommandString(command, raw);
            //// or
            throw new ASCOM.MethodNotImplementedException("CommandBlind");
            // DO NOT have both these sections!  One or the other
        }

        public bool CommandBool(string command, bool raw)
        {
            //CheckConnected("CommandBool");
            //string ret = CommandString(command, raw);
            //// TODO decode the return string and return true or false
            //// or
            throw new ASCOM.MethodNotImplementedException("CommandBool");
            // DO NOT have both these sections!  One or the other
        }

        public string CommandString(string command, bool raw)
        {
            //CheckConnected("CommandString");
            // it's a good idea to put all the low level communication with the device here,
            // then all communication calls this function
            // you need something to ensure that only one command is in progress at a time
            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            // Clean up the tracelogger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
        }

        public bool Connected
        {
            get
            {
                return IsConnected;
            }
            set
            {
                if (value == IsConnected)
                    return; // setting to the current state does nothing

                if (value)
                {
                    connectToPDU();
                }
                else
                {
                    disconnectFromPDU();
                }
            }
        }


        public string Description
        {
            get
            {
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverInfo = "Information about the driver itself. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            get
            {
                return 2;
            }
        }

        public string Name
        {
            get
            {
                return "ASCOM.APCPDU.Switch";
            }
        }

        #endregion

        #region ISwitchV2 Implementation

        /// <summary>
        /// The number of switches managed by this driver
        /// </summary>
        public short MaxSwitch
        {
            get; private set;
        }

        /// <summary>
        /// Return the Id of the Outlet as displayed on the APC PDU Web Interface
        /// </summary>
        /// <param name="id">index of the switch</param>
        /// <returns>
        /// Id of the Outlet as string
        /// </returns>
        public string GetSwitchName(short id)
        {
            if (!IsConnected)
                throw new Exception("Error in 'GetSwitchName': PDU not connected");

            if (id >= 0 && id < pdu.Outlets.Count)
                return pdu.Outlets[id].Id.ToString();
            else
                throw new Exception("Error in 'GetSwitchName': Id out of range");
        }

        /// <summary>
        /// Sets a switch name to a specified value
        /// </summary>
        /// <param name="id">The number of the switch whose name is to be set</param>
        /// <param name="name">The name of the switch</param>
        public void SetSwitchName(short id, string name)
        {
            throw new MethodNotImplementedException("SetSwitchName");
        }

        /// <summary>
        /// The Name of the Outlet as displayed on the APC PDU Web Interface
        /// </summary>
        /// <param name="id">index of the switch </param>
        /// <returns>Name of the Outlet</returns>
        public string GetSwitchDescription(short id)
        {
            if (!IsConnected)
                throw new Exception("Error in 'GetSwitchDescription': PDU not connected");

            if (id >= 0 && id < pdu.Outlets.Count)
                return pdu.Outlets[id].Name;
            else
                throw new Exception("Error in 'GetSwitchDescription': Id out of range");
        }

        /// <summary>
        /// Reports if the specified switch can be written to.
        /// This is false if the switch cannot be written to, for example a limit switch or a sensor.
        /// The default is true.
        /// </summary>
        /// <param name="id">The number of the switch whose write state is to be returned</param><returns>
        ///   <c>true</c> if the switch can be written to, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="MethodNotImplementedException">If the method is not implemented</exception>
        /// <exception cref="InvalidValueException">If id is outside the range 0 to MaxSwitch - 1</exception>
        public bool CanWrite(short id)
        {
            return IsConnected; // we can always write provided we are connected !
        }

        #region boolean switch members

        /// <summary>
        /// Return the state of switch n
        /// a multi-value switch must throw a not implemented exception
        /// </summary>
        /// <param name="id">index of the switch</param>
        /// <returns>
        /// True or false
        /// </returns>
        public bool GetSwitch(short id)
        {
            if (!IsConnected)
                throw new Exception("Error in 'GetSwitch': PDU device not connected !");

            if (id >= 0 && id < pdu.Outlets.Count)
            {
                var outlet_id = pdu.Outlets[id].Id ;
                return pdu.GetOutletStatus(outlet_id);
            }
            else
                throw new Exception($"Error in 'GetSwitch(id={id})': 'id' should be less than {pdu.Outlets.Count}");
        }

        /// <summary>
        /// Sets a switch to the specified state
        /// If the switch cannot be set then throws a MethodNotImplementedException.
        /// A multi-value switch must throw a not implemented exception
        /// setting it to false will set it to its minimum value.
        /// </summary>
        /// <param name="id">index of the switch</param>
        /// <param name="state">true to turn switch ON, false to turn switch OFF</param>
        public void SetSwitch(short id, bool state)
        {
            if (!CanWrite(id))
            {
                throw new Exception($"Error in SetSwitch(id={id}): Cannot Write");
            }

            if (id >= 0 && id < pdu.Outlets.Count)
            {
                var outlet_id = pdu.Outlets[id].Id;

                if (!pdu.SetOutletStatus(outlet_id, state))
                {
                    throw new Exception($"Error in SetSwitch(id={id}): Failed to change state");
                }
            }
            else
                throw new Exception($"Error in SetSwitch(id={id}): 'id' should be less than {pdu.Outlets.Count}");

        }

        #endregion

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Switch";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                return (pdu != null) && (pdu.IsConnected);
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";

                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));

                pdu_host = driverProfile.GetValue(driverID, hostProfileName, string.Empty, hostDefault);

                var portStr = driverProfile.GetValue(driverID, portProfileName, string.Empty, portDefault.ToString());
                int port;
                if (int.TryParse(portStr, out port))
                    pdu_port = port;

                pdu_username = driverProfile.GetValue(driverID, usernameProfileName, string.Empty, usernameDefault);
                pdu_password = driverProfile.GetValue(driverID, passwordProfileName, string.Empty, passwordDefault);

                var keepAliveIntervalStr = driverProfile.GetValue(driverID, keepAliveIntervalName, string.Empty, keepAliveIntervalDefault.ToString());
                int keepAliveInterval;
                if (int.TryParse(keepAliveIntervalStr, out keepAliveInterval))
                    pdu_keepAliveInterval = keepAliveInterval;
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());

                driverProfile.WriteValue(driverID, hostProfileName, pdu_host.ToString());
                driverProfile.WriteValue(driverID, portProfileName, pdu_port.ToString());
                driverProfile.WriteValue(driverID, usernameProfileName, pdu_username.ToString());
                driverProfile.WriteValue(driverID, passwordProfileName, pdu_password.ToString());
                driverProfile.WriteValue(driverID, keepAliveIntervalName, pdu_keepAliveInterval.ToString());

            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal static void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }

        #endregion

        #region Non Implemented


        public double MaxSwitchValue(short id)
        {
            throw new System.NotImplementedException();
        }

        public double MinSwitchValue(short id)
        {
            throw new System.NotImplementedException();
        }

        public double SwitchStep(short id)
        {
            throw new System.NotImplementedException();
        }

        public double GetSwitchValue(short id)
        {
            throw new System.NotImplementedException();
        }

        public void SetSwitchValue(short id, double value)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region custom


        private void connectToPDU()
        {
            // disconnect for safety
            disconnectFromPDU();

            // Connect to the device
            try
            {
                pdu = new APCPDULib.APCPDU(pdu_host, pdu_port, pdu_username, pdu_password);

                pdu.Connect();

                // Note: the number of accessible PDU outlets depending on the permissions given to the user used to log on the device
                this.MaxSwitch = (short)pdu.Outlets.Count;

            }
            catch (Exception e)
            {
                LogMessage("", $"{e.ToString()}");

                // disconnect for safety
                disconnectFromPDU();

            }
        }

        private void disconnectFromPDU()
        {
            // gracefully disconnect from PDU
            try
            {
                if ((pdu != null) && (pdu.IsConnected))
                    pdu.Disconnect(); // graceful disconnection
            }
            catch (Exception e)
            {
                LogMessage("", $"{e.ToString()}");
            }
            finally
            {
                pdu = null; // forces the client to disconnect even if we have failed doing it gracefully
            }
        }
        #endregion
    }
}
