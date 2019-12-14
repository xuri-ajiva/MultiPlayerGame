using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Client {
    public partial class Client : Form {
        public event Action<string> UpdateS;

        public Client() { InitializeComponent(); }

        private void Button1_Click(object sender, EventArgs e) {
            OnUpdateS( XMl.To_XML( new XMl( "button1", "Text", this.textBox1.Text ) ) );
        }

        public void CallBack(SharedComponents.NetMessage.Message _message, string data) {
            var xml = XMl.From_XML<XMl>( data );

            var b = this.Controls.Find( xml.Control, true )[0];

            b.GetType().GetProperty( xml.Property ).SetValue( b, xml.Vaule );
        }

        protected virtual void OnUpdateS(string obj) { UpdateS?.Invoke( obj ); }
    }

    public class XMl {
        public XMl() { }

        #region Xml

        public static T From_XML <T>(string xml) {
            var serializer = new XmlSerializer( typeof(T) );
            using ( var reader = new StringReader( xml ) ) {
                return (T) serializer.Deserialize( reader );
            }
        }

        public static string To_XML <T>(T contend) {
            var x = new XmlSerializer( typeof(T) );
            using ( var textWriter = new StringWriter() ) {
                x.Serialize( textWriter, contend );
                return textWriter.ToString();
            }
        }

        #endregion

        /// <inheritdoc />
        public XMl(string control, string property, object vaule) {
            this.Control  = control  ?? throw new ArgumentNullException( nameof(control) );
            this.Property = property ?? throw new ArgumentNullException( nameof(property) );
            this.Vaule    = vaule    ?? throw new ArgumentNullException( nameof(vaule) );
        }

        public string Control { [DebuggerStepThrough] get; set; }

        public string Property { [DebuggerStepThrough] get; set; }

        public object Vaule { [DebuggerStepThrough] get; set; }
    }
}