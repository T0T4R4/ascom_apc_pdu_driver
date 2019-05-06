using Renci.SshNet;
using System;
using System.Collections.Generic;

namespace APCPDULib
{
    public class APCPDU : IDisposable
    {
        private string _hostname;
        private int _port;
        private string _username;
        private string _password;
        public SshClient _client;
        private ShellStream _stream;

        public List<Outlet> Outlets { get; internal set; }

        public string LastResponse { get; private set; }
        public bool IsConnected
        {
            get
            {
                if (_client != null)
                {
                    return _client.IsConnected;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Initialize a new PDU Client class but does not connect to it yet. You must follow by a call to Connect().
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public APCPDU(string hostname, int port, string username, string password, double keepAliveSecs = 0)
        {
            _hostname = hostname;
            _port = port;
            _username = username;
            _password = password;

            _client = new SshClient(_hostname, _port, _username, _password);
            if (keepAliveSecs > 0)
            {
                _client.KeepAliveInterval = TimeSpan.FromSeconds(keepAliveSecs);
            }
            this.Outlets = new List<Outlet>();
        }

        public void Disconnect()
        {
            if (_client != null)
            {
                if (_stream != null)
                    _stream = null;

                if (_client.IsConnected)
                    _client.Disconnect();

                _client = null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// Connects to the PDU and loads the Outlets list
        /// </summary>
        public void Connect()
        {
            if (_client != null && !_client.IsConnected)
            {
                _client.Connect();
            }

            _stream = _client.CreateShellStream("Tail", 0, 0, 0, 0, 1024);

            WaitForPrompt();

            GetOutlets();
        }

        private string WaitForPrompt()
        {

            // expect the initial prompt
            return _stream.Expect("apc>", new TimeSpan(0, 0, 30));
        }

        private List<string> ExecuteCommand(string command)
        {
            _stream.WriteLine(command);

            var resp = _stream.Expect("apc>", new TimeSpan(0, 0, 30));
            var lines = resp.Split('\n');

            var list = new List<string>();

            // remove first line (command) and last line (prompt)
            for (int i = 1; i < lines.Length - 1; i++)
            {
                var line = lines[i];
                if (i == 1)
                {
                    if (!line.StartsWith("E000"))
                        throw new Exception(String.Format("PDU Command Error: {0}", line));

                    continue; // skip result
                }

                if (String.IsNullOrWhiteSpace(line))
                    continue;

                list.Add(line);

            }

            return list;
        }

        public List<Outlet> GetOutlets()
        {
            var response = ExecuteCommand("olStatus all");

            this.Outlets = new List<Outlet>();
            foreach (var line in response)
            {
                this.Outlets.Add(Outlet.FromString(line));
            }

            return this.Outlets;
        }

        /// <summary>
        /// Changes the status of an outlet and returns True if successful
        /// </summary>
        /// <param name="outlet"></param>
        /// <param name="enabled"></param>
        public void SetOutletStatus(Outlet outlet, bool enabled) { SetOutletStatus(outlet.Id, enabled);  }

        /// <summary>
        /// Changes the status of an outlet and returns True if successful
        /// </summary>
        /// <param name="outletId"></param>
        /// <param name="successful"></param>
        /// <returns></returns>
        public bool SetOutletStatus(int outletId, bool enabled)
        {
            var cmd = string.Format("{0} {1}", (enabled ? "olOn" : "olOff"), outletId);
            ExecuteCommand(cmd);

            return true;
        }

        /// <summary>
        /// Returns True if a given outlet is enabled
        /// </summary>
        /// <param name="outlet"></param>
        /// <returns></returns>
        public bool GetOutletStatus(Outlet outlet)
        {
            return GetOutletStatus(outlet.Id);
        }

        /// <summary>
        /// Returns True if a given outlet is enabled
        /// </summary>
        /// <param name="outletId"></param>
        /// <returns></returns>
        public bool GetOutletStatus(int outletId)
        {
            var cmd = string.Format("olStatus {0}", outletId);
            var response = ExecuteCommand(cmd);

            var tmp = Outlet.FromString(response[0]);

            var outlet = this.Outlets.Find(o => o.Id == outletId);

            if (outlet != null)
                outlet.Status = tmp.Status;

            return outlet.Enabled;
        }

    }
}
