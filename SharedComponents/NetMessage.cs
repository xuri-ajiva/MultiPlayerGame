using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents {
    public class NetMessage {
        public const           int    MS_LENGTH     = 8;
        public const           int    FRAQ_LENGTH_I = 4;
        public static readonly byte[] MS            = new byte[] { 1, 2, 3, 4 };
        public static readonly byte[] OK            = Combine( MS, new byte[] { 1, 1, 1, 1 } );
        public static readonly byte[] LOG           = Combine( MS, new byte[] { 2, 2, 2, 2 } );
        public static readonly byte[] XML           = Combine( MS, new byte[] { 3, 4, 4, 3 } );
        public static readonly byte[] ERROR         = Combine( MS, new byte[] { 255, 255, 255, 255 } );

        public enum Message {
            OK, ERROR, LOG, XML, Undefined
        }

        public static byte[] MessageError(string error) { return Combine( Combine( MS, ERROR ), Encoding.UTF8.GetBytes( error ) ); }

        public static Message GetMessage(byte[] ms) {
            if ( ms.Length != MS_LENGTH ) return Message.Undefined;
            var msg = SubArray( ms, 0, MS_LENGTH );
            if ( Equals( msg, OK ) ) return Message.OK;
            if ( Equals( msg, ERROR ) ) return Message.ERROR;
            if ( Equals( msg, LOG ) ) return Message.LOG;
            if ( Equals( msg, XML ) ) return Message.XML;

            return Message.Undefined;
        }

        public static bool Equals <T>(T[] array1, T[] array2) {
            if ( array1.Length != array2.Length ) return false;
            for ( int i = 0; i < array1.Length; i++ ) {
                if ( !array1[i].Equals( array2[i] ) ) return false;
            }

            return true;
        }

        public static T[] SubArray <T>(T[] data, int index, int length) {
            T[] result = new T[length];
            Array.Copy( data, index, result, 0, length );
            return result;
        }

        public static T[] Combine <T>(T[] array1, T[] array2) {
            var tmp = new T[array1.Length + array2.Length];
            array1.CopyTo( tmp, 0 );
            array2.CopyTo( tmp, array1.Length );
            return tmp;
        }
    }
}