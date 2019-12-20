using System;
using System.Drawing;
using System.Security.Cryptography;
using GameFramework;
using OpenTK;

namespace OpenTKFramework {
    public abstract class MainClass {
        public int    numFrames  = 0;
        public double framesTime = 0.0;
        public int    frameRate  = 0;


        public GraphicsManager   I;
        public OpenTK.GameWindow Window = null;

        public abstract void Initialize(object sender, EventArgs e);

        public abstract void Update(object sender, FrameEventArgs e);

        public abstract void Render(object sender, FrameEventArgs e);

        public abstract void Shutdown(object sender, EventArgs e);

        public GameWindow Create() {
            // Create static (global) window instance
            Window = new OpenTK.GameWindow();

            //// Hook up the initialize callback
            //Window.Load += new EventHandler<EventArgs>( Initialize );
            //// Hook up the update callback
            //Window.UpdateFrame += new EventHandler<FrameEventArgs>( Update );
            //// Hook up the render callback
            //Window.RenderFrame += new EventHandler<FrameEventArgs>( Render );
            //// Hook up the shutdown callback
            //Window.Unload += new EventHandler<EventArgs>( Shutdown );

            // Hook up the initialize callback
            Window.Load += delegate(object sender, EventArgs e) {
                GraphicsManager.Instance.Initialize( Window );
                I = GraphicsManager.Instance;
                Initialize( sender, e );
            };
            // Hook up the update callback
            Window.UpdateFrame += delegate(object sender, FrameEventArgs e) {
                numFrames  += 1;
                framesTime += e.Time;

                if ( framesTime >= 1.0f ) {
                    frameRate  = (int) ( System.Convert.ToDouble( numFrames ) / framesTime );
                    framesTime = 0.0;
                    numFrames  = 0;
                }

                Update( sender, e );
            };
            // Hook up the render callback
            Window.RenderFrame += delegate(object sender, FrameEventArgs e) {
                Render( sender, e );
                I.SwapBuffers();
            };
            // Hook up the shutdown callback
            Window.Unload += delegate(object sender, EventArgs e) {
                GraphicsManager.Instance.Shutdown();
                Shutdown( sender, e );
            };

            // Set window title and size
            Window.Title      = "Game Name";
            Window.ClientSize = new Size( 800, 600 );

            return Window;
        }

        public void Run() {
            // Run the game at 60 frames per second. This method will NOT return
            // until the window is closed.
            Window.Run( 60.0f );

            // If we made it down here the window was closed. Call the windows
            // Dispose method to free any resources that the window might hold
            Window.Dispose();

        #if DEBUG
            Console.ReadLine();
        #endif
        }
    }
}