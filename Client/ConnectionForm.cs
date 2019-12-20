using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using SharedComponents;

namespace Client {
    public partial class ConnectionForm : Form {
        public ConnectionForm() { InitializeComponent(); }

        AutoCompleteStringCollection _a = new AutoCompleteStringCollection();

        private void Advanced_expand_CheckedChanged(object sender, EventArgs e) {
            var v = ( this.advanced_expand.Checked ? +1 : -1 ) * 40;
            this.advanced_expand.Location = new Point( this.advanced_expand.Location.X, this.advanced_expand.Location.Y + v );

            this.Size = new Size( this.Size.Width, this.Size.Height + v );
        }

        private void ConnectionForm_Load(object sender, EventArgs e) {
            loadAutoCompleate();
            this.textBox1.AutoCompleteCustomSource = this._a;
            this.textBox1.AutoCompleteSource       = AutoCompleteSource.CustomSource;
            this.textBox1.AutoCompleteMode         = AutoCompleteMode.SuggestAppend;
            this.textBox2.Text                     = Connection.PORT_I.ToString();
            this.advanced_expand.Checked           = false;
        }

        void saveAutoComleation() {
            File.Open( Application.ExecutablePath + "settings.xml", FileMode.OpenOrCreate ).Close();
            try {
                Eternal.Settings.XMLOPERATION.SAVE( new Settings() { C = this._a }, Application.ExecutablePath + "settings.xml" );
            } catch (Exception e) {
                Console.WriteLine( e );
            }
        }

        private void loadAutoCompleate() {
            try {
                this._a = Eternal.Settings.XMLOPERATION.LOAD<Settings>( Application.ExecutablePath + "settings.xml" ).C;
            } catch { }

            if ( this._a == null ) this._a = new AutoCompleteStringCollection();

            this._a.Add( "localhost" );
            this._a.Add( Connection.IP_LOCAL );
        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e) {
            if ( !char.IsControl( e.KeyChar ) && !char.IsDigit( e.KeyChar ) && ( e.KeyChar != '.' ) ) {
                e.Handled = true;
            }
        }

        private void Button1_Click(object sender, EventArgs e) {
            this._a.Add( this.textBox1.Text );
            TcpClient cl = null;
            try {
                cl = Connection.Connect( int.Parse( this.textBox2.Text ), this.textBox1.Text );
                Program.manageClient( cl );
            } catch (Exception xe) {
                MessageBox.Show( xe.StackTrace + "\n" + xe.Message );
                return;
            }

            saveAutoComleation();
            this.Hide();
            //new Thread( () => new Program().manageClient( cl ) ).Start();
            Program._form          =  new Client();
            Program._form.UpdateS  += Program.SendUpdate;
            Program._form.RequestS += Program.RequestUpdate;
            Program._form.ShowDialog( this );
            Environment.Exit( 0 );
        }

        private class Settings {
            public AutoCompleteStringCollection C;

            public Settings() { }
        }
    }
}