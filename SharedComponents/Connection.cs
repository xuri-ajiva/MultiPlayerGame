using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents {
    public class Connection {
        public class ClStreamObj {
            /// <inheritdoc />
            public ClStreamObj(NetworkStream ns, TcpClient cl) {
                this.Ns = ns;
                this.Cl = cl;
            }

            public NetworkStream Ns { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }

            public TcpClient Cl { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }
        }

        public static string ReadString(Stream s) { return new StreamReader( s ).ReadLine(); }

        public static byte[] ReadStream(Stream s, int length) {
            var buffer = new byte[length];
            try {
                s.Read( buffer, 0, buffer.Length );
            } catch (Exception e) {
                //Console.WriteLine( e );
                return new byte[0];
            }

            return buffer;
        }

        public static void WriteStream(Stream s, byte[] data) {
            try { s.Write( data, 0, data.Length ); } catch (Exception e) {
                //Console.WriteLine( e );
            }
        }

        public static TcpListener InitServer(int port) {
            var tcpSer = new TcpListener( new IPEndPoint( IPAddress.Any, port ) );
            tcpSer.Start( 1000 );
            return tcpSer;
        }

        public static IEnumerable<TcpClient> ListenTcp(TcpListener tcl) {
            while ( true ) {
                yield return tcl.AcceptTcpClient();
            }
        }

        public static TcpClient Connect(int port, string ipAddress) {
            var iEndpoint = new IPEndPoint( IPAddress.Parse( ipAddress ), port );
            var tcpCl     = new TcpClient();

            tcpCl.Connect( iEndpoint );
            return tcpCl;
        }

        public const           int    BUFFER_SIZE = 20000;
        public static readonly byte[] BUFFER      = new byte[BUFFER_SIZE];
        public const           Int32  PORT_I      = 1134;
        public const           string IP_LOCAL    = "127.0.0.1";
    }
}