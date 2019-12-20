using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;

namespace OpenGl.Performance {
    static class Program {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            new MainClass();
        }
    }

    class MainClass : OpenTKFramework.MainClass {
        #region Overrides of MainClass

        /// <inheritdoc />
        public override void Initialize(object sender, EventArgs e) { }

        /// <inheritdoc />
        public override void Shutdown(object sender, EventArgs e) { }

        private const int size = 5;

        public override void Render(object sender, FrameEventArgs e) {
            this.I.ClearScreen( Color.Black );

            for ( int i = 0; i < this.Window.ClientSize.Width / size; i++ ) {
                for ( int j = 0; j < this.Window.ClientSize.Height / size; j++ ) {
                    var rec = new RectangleF( i * size, j * size, size, size );

                    this.I.DrawRect( rec, ColorGenerator(this.totalFramesRendert,i,j) );

                    // this.I.DrawRect(, Color.FromArgb((int) (Math.Sin( (double)i ) + 1)*255/2, (int) (Math.Cos( (double)j  ) + 1) *255 /2,0 /*(int) (-Math.Sin( (double)this.totalFramesRendert /size ) + 1) *255 /2 */));
                    //this.I.DrawPoint( new PointF(i,j), Color.FromArgb( i%255,j %255,10 )  );
                }
            }

            this.I.DrawString( "FramesDrawn: " + this.totalFramesRendert, PointF.Subtract( new Point( this.Window.ClientSize ), new Size( 200, 50 ) ), Color.AliceBlue );
        }

        public override void Update(object sender, FrameEventArgs e) { }

        #endregion


        private static Color ColorGenerator(int frame, int x, int y) {

            const int a = 255;

            var use = (double) frame / 50;


            int r = (int) ( ( Math.Sin( use )  + 1 ) / 2 * ( x     % 255 ) );
            int g = (int) ( ( Math.Cos( use )  + 1 ) / 2 * ( y     % 255 ) );
            int b = (int) ( (-(Math.Sin(use )) + 1 ) / 2 * ( frame/100 % 255 ) );

            return Color.FromArgb( a, r, g, b );
        }

        public MainClass() {
            Create();
            this.Window.Resize += delegate(object sender, EventArgs args) {
                this.Window.ClientSize = new Size( ( (int) ( this.Window.ClientSize.Width / size ) ) * size, ( (int) ( this.Window.ClientSize.Height / size ) ) * size );
            };

            Run( 10 );
        }
    }
}