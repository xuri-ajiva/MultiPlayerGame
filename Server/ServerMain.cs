using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedComponents;

namespace Server {
    class ServerMain {
        private List<Connection.ClStreamObj> _clStreams;

        public ServerMain() {
            Console.WriteLine( "Init server... " );
            this._clStreams = new List<Connection.ClStreamObj>();
        }

        static void Main(string[] args) { new ServerMain().Start(); }

        private void Start() {
            var server = Connection.InitServer( Connection.PORT_I );
            Console.WriteLine( "Waiting for Connections..." );
            foreach ( var cl in Connection.ListenTcp( server ) ) {
                Console.WriteLine( "Got an Connection..." );
                new Thread( () => ManageClient( cl ) ).Start();
            }
        }

        private void ManageClient(TcpClient cl) {
            var str = cl.GetStream();
            var ob  = new Connection.ClStreamObj( str, cl );
            this._clStreams.Add( ob );
            var t = new Thread( () => streamReader( str, cl, ob ) );
            t.Start();
            t.Join();

            this._clStreams.Remove( ob );
            Console.WriteLine( "client dc..." );
            //new Thread( () => streamWriter( str,cl ) ).Start();
        }

        void streamReader(NetworkStream st, TcpClient cl, Connection.ClStreamObj myClStreamObj) {
            Console.WriteLine( "reader running" );
            while ( cl.Connected ) {
                if ( st.DataAvailable ) {
                    Console.WriteLine( "revise data.." );
                    new Thread( () => processBuffer( Connection.ReadStream( st, cl.Available ), cl.GetHashCode() ) ).Start();
                    Connection.WriteStream( st, NetMessage.OK );
                }
            }
        }

        private void processBuffer(byte[] readStream, int v) {
            for ( var i = 0; i < this._clStreams.Count; i++ ) {
                streamWriter( this._clStreams[i].Ns, this._clStreams[i].Cl, readStream );
            }
        }

        void streamWriter(Stream st, TcpClient cl, byte[] data) {
            Console.WriteLine( "sending data..." );
            Connection.WriteStream( st, data );
        }
    }
}