using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using OpenTK;
using OpenGL;
using Khronos;
using OpenGL.CoreUI;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Drawing;
using System.Threading;
using GameFramework;
using OpenTKFramework.Framework;
using KeyPressEventArgs = OpenTK.KeyPressEventArgs;
using Size = System.Drawing.Size;

namespace Game {
    class Program {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            //Application.Run(new Form1());
            new MainClass();
        }
    }

    class MainClass : OpenTKFramework.MainClass {
        private SnakeGame sg;
        private bool      update = true;

        public override void Initialize(object sender, EventArgs e) { this.sg = new SnakeGame( this.Window.ClientSize ); }

        public override void Update(object sender, FrameEventArgs e) {
            if ( !this.update ) return;

            if ( this.sg.GameUpdate_Tick() ) return;
            this.update = false;
            new Thread( () => {
                var result = MessageBox.Show(/*new WindowWrapper(this.Window.WindowInfo.Handle),*/ @"Retry?", @"You Failed", MessageBoxButtons.AbortRetryIgnore);

                this.sg.ResultSwitch( result );
                this.update = true;
            } ).Start();
        }

        public override void Render(object sender, FrameEventArgs e) {
            this.I.ClearScreen( Color.CornflowerBlue );

            // Add future render commands here
            //I.DrawString("FPS: "         + (1.0 / e.Time), new System.Drawing.Point(10, 10), Color.Firebrick);
            //I.DrawString("Average FPS: " + frameRate,      new System.Drawing.Point(10, 30), Color.Firebrick);
            this.sg.PaintEventHandler( null, this.I );
        }

        public override void Shutdown(object sender, EventArgs e) { }

        public MainClass() : base() {
            this.Window = Create();
            this.Window.WindowBorder = WindowBorder.Resizable;
            this.Window.Cursor = MouseCursor.Empty;
            this.Window.KeyPress += WindowOnKeyPress;
            this.Window.Resize += (sender, args) => {
                this.Window.ClientSize = new Size( ( (int) ( this.Window.ClientSize.Width / SnakeGame.MultiScale ) ) * SnakeGame.MultiScale, ( (int) ( this.Window.ClientSize.Height / SnakeGame.MultiScale ) ) * SnakeGame.MultiScale );

                this.sg.SetSize( this.Window.ClientSize );
            };

            this.Window.VSync                 = VSyncMode.Off;
            this.Window.TargetRenderFrequency = 100000;

            Run();
        }

        private void WindowOnKeyPress(object sender, KeyPressEventArgs e) { this.sg.ClientKeyPress( sender, e ); }
    }

    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        private IntPtr _hwnd;
    }
}