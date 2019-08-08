using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Penguin.Net.Telnet
{
    /// <summary>
    /// An incredibly thin Telnet client
    /// </summary>
    public class TelnetClient : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the client
        /// </summary>
        /// <param name="RemoteIP">The remote address (or host) of the server</param>
        /// <param name="Port">The port to connect on</param>
        public TelnetClient(string RemoteIP, int Port = 43)
        {
            tcpClient = new TcpClient();

            tcpClient.Connect(new IPEndPoint(this.ResolveIP(RemoteIP), Port));
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Generic disposal method. Closes connection
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends the requested string to the connected server
        /// </summary>
        /// <param name="toSend">The string to sent</param>
        /// <param name="sendTerminator">if true, a newline is automatically appended</param>
        /// <param name="Tries">Attempts to send multiple times if a failure occurs but wont help since it doesn't reopen the connection</param>
        /// <returns>The response from the server</returns>
        public string Send(string toSend, bool sendTerminator = true, int Tries = 3)
        {
            if (sendTerminator)
            {
                toSend += "\r\n";
            }

            byte[] data = System.Text.Encoding.ASCII.GetBytes(toSend);

            int tried = 0;

            Exception toThrow = new Exception("Something went wrong and we failed to catch it");

            while (tried++ < Tries)
            {
                try
                {
                    NetworkStream stream = this.tcpClient.GetStream();
                    stream.Write(data, 0, data.Length);

                    data = new byte[256];

                    string responseData = string.Empty;
                    int bytes = stream.Read(data, 0, data.Length);
                    while (bytes > 0)
                    {
                        responseData += System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                        bytes = stream.Read(data, 0, data.Length);
                    }

                    return responseData;
                }
                catch (Exception ex)
                {
                    toThrow = ex;
                }
            }

            throw toThrow;
        }

        /// <summary>
        /// Generic disposal method. Closes connection
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.tcpClient.Close();

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Attempts to resolve an IP (or host) into a .Net IPAddress
        /// </summary>
        /// <param name="toResolve">The string to resolve</param>
        /// <returns>a .Net IPAddress</returns>
        protected IPAddress ResolveIP(string toResolve)
        {
            if (IpResolutions.ContainsKey(toResolve))
            {
                return IpResolutions[toResolve];
            }

            if (IPAddress.TryParse(toResolve, out IPAddress toReturn))
            {
                IpResolutions.TryAdd(toResolve, toReturn);
                return toReturn;
            }
            else
            {
                IPHostEntry hostEntry;

                hostEntry = Dns.GetHostEntry(toResolve);

                //you might get more than one ip for a hostname since
                //DNS supports more than one record

                if (hostEntry.AddressList.Length > 0)
                {
                    toReturn = hostEntry.AddressList[0];
                    IpResolutions.TryAdd(toResolve, toReturn);
                    return toReturn;
                }
                else
                {
                    throw new Exception("Ip address not valid and no host resolution found");
                }
            }
        }

        #endregion Methods

        #region Fields

        private static readonly ConcurrentDictionary<string, IPAddress> IpResolutions = new ConcurrentDictionary<string, IPAddress>();
        private bool disposedValue = false;

        #endregion Fields

        #region Properties

        private TcpClient tcpClient { get; set; }

        #endregion Properties

        // To detect redundant calls
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TelnetClient()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }
    }
}