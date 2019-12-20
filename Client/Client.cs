#region using

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using SharedComponents;

#endregion

namespace Client {
    public partial class Client : Form {
        public Client() { InitializeComponent(); }
        public event Action<string, EventArgs> UpdateS;
        public event Action<string,EventArgs> RequestS;

        private void Button1_Click(object sender, EventArgs e) { RPCCall( "button1", "Text", this.textBox1.Text ); }

        private void RPCCall(string control, string property, object value) => OnUpdateS( XMl.To_XML( new XMl( control, property, value ) ) );

        private void CMDCall(string control, string property) => OnRequestS( XMl.To_XML( new XMl( control, property, "Call No User Input!" ) ) );

        public void CallBack(NetMessage.Message _message, string data) {
            var xml = XMl.From_XML<XMl>( data );

            if ( xml.Value.GetType() == typeof(XMl.XmlColor) ) xml.Value = ( (XMl.XmlColor) xml.Value ).ToColor();

            var b = this.Controls.Find( xml.Control, true )[0];
            try {
                b.GetType().GetProperty( xml.Property )?.SetValue( b, xml.Value );
            } catch (Exception e) {
                MessageBox.Show( e.Message + "\n\n" + e.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }
        }

        private void Client_Load(object sender, EventArgs e) {
            const int size = 40;
            for ( var i = 0; i < 3; i++ ) {
                for ( var j = 0; j < 3; j++ ) {
                    var b = new Button { Name = i + "|" + j, Location = new Point( i * size, j * size ), Size = new Size( size, size ) };
                    b.Click += BOnClick;
                    this.panel1.Controls.Add( b );
                }
            }
        }

        private void BOnClick(object sender, EventArgs e) {
            var b = sender as Button;
            b.BackColor = this.button3.BackColor;
            RPCCall( b.Name, nameof(b.BackColor), new XMl.XmlColor( this.button3.BackColor ) );
        }

        private void Button2_Click(object sender, EventArgs e) {
            var b = this.Controls.Find( this.textBox2.Text, true )[0];
            b.GetType().GetProperty( "Text" )?.SetValue( b, "test" );
        }

        private void Button3_Click(object sender, EventArgs e) {
            if ( this.colorDialog1.ShowDialog() == DialogResult.OK ) ( (Button) sender ).BackColor = this.colorDialog1.Color;
        }

        private void Button4_Click(object sender, EventArgs e) {
            CMDCall( nameof(this.richTextBox1), nameof(this.richTextBox1.Text) );
            Wait( 1000 );
            this.richTextBox1.Text += this.myName;
            RPCCall( nameof(this.richTextBox1), nameof(this.richTextBox1.Text), this.richTextBox1.Text );
            this.panel1.Enabled = true;
        }

        public void CallIn(NetMessage.Message ms, string data) {
            var xml = XMl.From_XML<XMl>( data );

            if ( xml.Value.GetType() == typeof(XMl.XmlColor) ) xml.Value = ( (XMl.XmlColor) xml.Value ).ToColor();

            var b = this.Controls.Find( xml.Control, true )[0];
            try {
                xml.Value = b.GetType().GetProperty( xml.Property )?.GetValue( b );
                if ( xml.Value?.GetType() == typeof(Color) ) xml.Value = new XMl.XmlColor( (Color) xml.Value );
            } catch (Exception e) {
                MessageBox.Show( e.Message + "\n\n" + e.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error );
            }

            OnUpdateS( XMl.To_XML( xml ) );
        }

        protected virtual void OnUpdateS(string obj) { this.UpdateS?.Invoke( obj, new EventArgs() ); }

        protected virtual void OnRequestS(string obj) { this.RequestS?.Invoke( obj, new EventArgs() ); }

        public static void Wait(int milliseconds) {
            Timer timer1 = new Timer();
            if ( milliseconds == 0 || milliseconds < 0 ) return;
            timer1.Interval = milliseconds;
            timer1.Enabled  = true;
            timer1.Start();
            timer1.Tick += (s, e) => {
                timer1.Enabled = false;
                timer1.Stop();
            };
            while ( timer1.Enabled ) {
                Application.DoEvents();
            }
        }

        string myName = "Player: " + new Random().NextDouble() + "\n";

        private void Client_FormClosing(object sender, FormClosingEventArgs e) {
            this.richTextBox1.Text = this.richTextBox1.Text.Replace( this.myName, "" );
            RPCCall( nameof(this.richTextBox1), nameof(this.richTextBox1.Text), this.richTextBox1.Text );
        }
    }

    [XmlInclude( typeof(XmlColor) )]
    public class XMl {
        public XMl() { }

        /// <inheritdoc />
        public XMl(string control, string property, object value) {
            this.Control  = control  ?? throw new ArgumentNullException( nameof(control) );
            this.Property = property ?? throw new ArgumentNullException( nameof(property) );
            this.Value    = value    ?? throw new ArgumentNullException( nameof(value) );
        }

        public string Control { [DebuggerStepThrough] get; set; }

        public string Property { [DebuggerStepThrough] get; set; }

        public object Value { [DebuggerStepThrough] get; set; }

        public class XmlColor {
            private Color color_ = Color.Black;

            public XmlColor() { }
            public XmlColor(Color c) => this.color_ = c;

            [XmlAttribute]
            public string Web {
                get => ColorTranslator.ToHtml( this.color_ );
                set {
                    try {
                        if ( this.Alpha == 0xFF ) // preserve named color value if possible
                            this.color_ = ColorTranslator.FromHtml( value );
                        else
                            this.color_ = Color.FromArgb( this.Alpha, ColorTranslator.FromHtml( value ) );
                    } catch (Exception) {
                        this.color_ = Color.Black;
                    }
                }
            }

            [XmlAttribute]
            public byte Alpha {
                get => this.color_.A;
                set {
                    if ( value != this.color_.A ) // avoid hammering named color if no alpha change
                        this.color_ = Color.FromArgb( value, this.color_ );
                }
            }

            public static explicit operator Color(XmlColor x) => x.ToColor();

            public static explicit operator XmlColor(Color c) => new XmlColor( c );


            public Color ToColor() => this.color_;

            public void FromColor(Color c) { this.color_ = c; }

            public bool ShouldSerializeAlpha() => this.Alpha < 0xFF;
        }

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
    }
}