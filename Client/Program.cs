using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SharedComponents;

namespace Client {
    class Program {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        public static Client _form;

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            try {
                _ = new Program();
            } catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine( e.StackTrace + "\n\n" + e.Message );
            }
        }

        public Program() {
            Application.Run( new ConnectionForm() );
            //Console.WriteLine( "init Client" );
        }

        public static void SendUpdate(string obj) {
            var send = NetMessage.Combine( NetMessage.XML, Encoding.Unicode.GetBytes( obj.Replace( "\n", "" ) ) );
            Connection.WriteStream( _stream, send );
        }

        private static NetworkStream _stream;

        public static void manageClient(TcpClient cl) {
            NetworkStream str = cl.GetStream();

            _stream = str;
            new Thread( () => streamReader( str, cl ) ).Start();
            //new Thread( () => streamWriter( str, cl ) ).Start();
        }

        static void streamReader(NetworkStream st, TcpClient cl) {
            while ( cl.Connected ) {
                if ( st.DataAvailable ) {
                    //new Thread( () =>  processBuffer( Connection.ReadStream( st, cl.Available ) ) ).Start();
                    new Thread( () => DataAvailable( st ) ).Start();
                    //Connection.WriteStream( st, NetMessage.OK );
                }
            }
        }

        private static void DataAvailable(NetworkStream s) {
            var b = new byte[NetMessage.MS_LENGTH];
            s.Read( b, 0, b.Length );
            var ms   = NetMessage.GetMessage( b );
            var data = Connection.ReadString( s );

            processMessage( ms, data );
        }

        private static void processMessage(NetMessage.Message message, string data) {
            switch (message) {
                case NetMessage.Message.OK: break;
                case NetMessage.Message.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine( data );
                    Console.ResetColor();
                    break;
                case NetMessage.Message.LOG:
                    Console.WriteLine( data );
                    break;
                case NetMessage.Message.Undefined: break;
                case NetMessage.Message.XML:
                    _form.Invoke( new Action( () => _form.CallBack( message, data ) ) );
                    break;
                case NetMessage.Message.GET:
                    _form.Invoke( new Action( () => _form.CallIn( message, data ) ) );
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static void processBuffer(byte[] readStream) {
            if ( readStream.Length < NetMessage.MS_LENGTH ) return;
            var head = NetMessage.SubArray( readStream, 0, NetMessage.MS_LENGTH );
            var ms   = NetMessage.GetMessage( head );

            processMessage( ms, Encoding.Unicode.GetString( NetMessage.SubArray( readStream, NetMessage.MS_LENGTH, readStream.Length - NetMessage.MS_LENGTH ) ) );
        }

        static void streamWriter(Stream st, TcpClient cl) {
            while ( cl.Connected ) {
                var ln = Console.ReadLine();
                ln = string.IsNullOrEmpty( ln ) ? " " : ln;
                var send = NetMessage.Combine( NetMessage.LOG, Encoding.UTF8.GetBytes( ln.Replace( "\n", "" ) ) );
                Connection.WriteStream( st, send );
            }
        }

        public static void RequestUpdate(string obj) {
            var send = NetMessage.Combine( NetMessage.GET, Encoding.Unicode.GetBytes( obj.Replace( "\n", "" ) ) );
            Connection.WriteStream( _stream, send );
        }
    }
}