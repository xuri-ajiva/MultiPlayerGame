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
        private static Client _form;

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            _ = new Program();
        }

        public Program() {
            Console.WriteLine( "init Client" );
            var cl = Connection.Connect( Connection.PORT_I, Connection.IP_LOCAL );
            manageClient( cl );
            //new Thread( () => new Program().manageClient( cl ) ).Start();
            _form         =  new Client();
            _form.UpdateS += SendUpdate;
            Application.Run( _form );
            Environment.Exit( 0 );
        }

        private void SendUpdate(string obj) {
            var send = NetMessage.Combine( NetMessage.XML, Encoding.UTF8.GetBytes( obj ) );
            Connection.WriteStream( this._stream, send );
        }

        private NetworkStream _stream;

        private void manageClient(TcpClient cl) {
            NetworkStream str = cl.GetStream();
            this._stream = str;
            new Thread( () => streamReader( str, cl ) ).Start();
            new Thread( () => streamWriter( str, cl ) ).Start();
        }

        void streamReader(NetworkStream st, TcpClient cl) {
            while ( cl.Connected ) {
                if ( st.DataAvailable ) {
                    processBuffer( Connection.ReadStream( st, cl.Available ) );
                    //Connection.WriteStream( st, NetMessage.OK );
                }
            }
        }

        private void processBuffer(byte[] readStream) {
            var head = NetMessage.SubArray( readStream, 0, NetMessage.MS_LENGTH );

            var ms = NetMessage.GetMessage( head );
            switch (ms) {
                case NetMessage.Message.OK:    break;
                case NetMessage.Message.ERROR: break;
                case NetMessage.Message.LOG:
                    Console.WriteLine( Encoding.UTF8.GetString( NetMessage.SubArray( readStream, NetMessage.MS_LENGTH, readStream.Length - NetMessage.MS_LENGTH ) ) );
                    break;
                case NetMessage.Message.Undefined: break;
                case NetMessage.Message.XML:
                    _form.Invoke( new Action( () => _form.CallBack( ms, Encoding.UTF8.GetString( NetMessage.SubArray( readStream, NetMessage.MS_LENGTH, readStream.Length - NetMessage.MS_LENGTH ) ) ) ) );
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            //Console.WriteLine( ms );
        }

        void streamWriter(Stream st, TcpClient cl) {
            while ( cl.Connected ) {
                var ln = Console.ReadLine();
                ln = string.IsNullOrEmpty( ln ) ? " " : ln;
                var send = NetMessage.Combine( NetMessage.LOG, Encoding.UTF8.GetBytes( ln ) );
                Connection.WriteStream( st, send );
            }
        }
    }
}