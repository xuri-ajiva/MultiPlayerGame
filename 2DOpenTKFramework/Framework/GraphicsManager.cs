using System;
using System.Drawing;
using System.Linq;
using NAudio.Wave;
using OpenTK.Graphics.OpenGL;
using OpenTKFramework.Framework;

namespace GameFramework {
    public class GraphicsManager {
        private static GraphicsManager instance = null;

        public static GraphicsManager Instance {
            get {
                if ( instance == null ) {
                    instance = new GraphicsManager();
                }

                return instance;
            }
        }

        private GraphicsManager() { }

        // The actual font texture is embeded in code. It is mono-space.
        private int originalW  = 855;  // Width of font texutre
        private int originalH  = 15;   // Height of font texture
        private int fontWidth  = 1024; // Padded width of font texture
        private int fontHeight = 16;   // Padded height of font texture
        private int charWidth  = 9;    // Pixel width of each character
        private int charHeight = 15;   // Pixel height of each character
        private int fontHandle = 0;    // Hardware accelerated font handle

        public float Depth { get { return this.currentDepth; } }

        private Color             lastClear     = Color.Red;
        private OpenTK.GameWindow game          = null;
        private float             currentDepth  = -1.0f;
        private bool              isInitialized = false;

        public void tests() {
            if ( !this.isInitialized ) {
                Error( "Trying to draw line without intializing graphics manager!" );
            }

            IncreaseDepth();

            var c = Color.BlueViolet;
            var r = new Random();

            const float size = 150F;
            var         p    = new PointF( 200, 200 );

            GL.Color3( c.R, c.G, c.B );

            //DrewCycle( new PointF(400,400),20, Color.Aqua, 30  );

            //PrimitiveType[] ts = Enum.GetValues(typeof(PrimitiveType)).Cast<PrimitiveType>().ToArray();
            //const int       m  = 100;
            //
            //for ( int i = 0; i < ts.Length; i++ ) {
            //    
            //    DrawString( ts[i].ToString(),new Point((m )*i , (m/2) ), Color.Chocolate);
            //
            //
            //    GL.Color3( c.R, c.G, c.B );
            //    GL.Begin( ts[i] );
            //    for ( int j = 0; j < 20 ; j++ ) {
            //        GL.Vertex3( i * m + r.Next( (int) ( m / 1.3 ) ), m + r.Next( (int) ( m / 1.3 ) ), this.currentDepth );
            //    }
            //
            //    GL.End();
            //}
        }

        #region staff

        private void Error(string error) {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine( error );
            Console.ForegroundColor = old;
        }

        public void Initialize(OpenTK.GameWindow window) {
            if ( this.isInitialized ) {
                Error( "Trying to double intialize graphics manager!" );
            }

            GL.MatrixMode( MatrixMode.Modelview );
            GL.LoadIdentity();
            this.lastClear = Color.CadetBlue;
            this.game      = window;

            GL.Enable( EnableCap.Texture2D );
            GL.BindTexture( TextureTarget.Texture2D, 0 );
            GL.Enable( EnableCap.Blend );
            GL.Enable( EnableCap.DepthTest );
            GL.ClearColor( Color.CadetBlue );

            this.fontHandle = GetFontTexture();

            this.isInitialized = true;

            SetScreenSize( this.game.ClientSize.Width, this.game.ClientSize.Height );

            this.game.Load += (sender, e) => {
                this.game.VSync = OpenTK.VSyncMode.On;
            };

            this.game.Resize += (sender, e) => {
                SetScreenSize( this.game.ClientSize.Width, this.game.ClientSize.Height );
            };
        }

        public void Shutdown() {
            if ( !this.isInitialized ) {
                Error( "Trying to shut down a non initialized graphics manager!" );
            }

            GL.DeleteTexture( this.fontHandle );
            this.game          = null;
            this.isInitialized = false;
        }

        public void SetScreenSize(int width, int height) {
            if ( !this.isInitialized ) {
                Error( "Trying to set screen size without intializing graphics manager!" );
            }

            GL.MatrixMode( MatrixMode.Projection );
            GL.LoadIdentity();
            GL.Ortho( 0, width, height, 0, -1.0, 1.0 );
            GL.Viewport( 0, 0, width, height );
            GL.MatrixMode( MatrixMode.Modelview );
        }

        public void ClearScreen(Color clearColor) {
            if ( !this.isInitialized ) {
                Error( "Trying to clear screen size without intializing graphics manager!" );
            }

            if ( clearColor != this.lastClear ) {
                GL.ClearColor( clearColor );
            }

            GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
        }

        public void SwapBuffers() {
            if ( !this.isInitialized ) {
                Error( "Trying to swap buffers without intializing graphics manager!" );
            }

            this.game.SwapBuffers();
            this.currentDepth = -1.0f;
        }

        public void IncreaseDepth() {
            if ( !this.isInitialized ) {
                Error( "Trying to increase depth without intializing graphics manager!" );
            }

            this.currentDepth += 0.0005f;
            if ( this.currentDepth > 1.0f ) {
                this.currentDepth = 1.0f;
            }
        }

        #endregion

        public void DrawRect(Rectangle rect, Color c, PrimitiveType dm = PrimitiveType.Quads) {
            RectangleF rf = new RectangleF( rect.X, rect.Y, rect.Width, rect.Height );
            DrawRect( rf, c, dm );
        }

        public void DrawPoint(Point point, Color c, PrimitiveType dm =PrimitiveType.Points) {
            PointF rf = new PointF( point.X, point.Y );
            DrawPoint( rf, c,dm );
        }

        public void DrawLine(Point p1, Point p2, Color c, PrimitiveType dm = PrimitiveType.Lines) {
            PointF pf1 = new PointF( p1.X, p1.Y );
            PointF pf2 = new PointF( p2.X, p2.Y );
            DrawLine( pf1, pf2, c, dm );
        }

        public void DrawTriangle(Point p, float s, Color c, bool flip = false, PrimitiveType dm = PrimitiveType.Polygon) {
            PointF pf = new PointF( p.X, p.Y );
            DrawTriangle( pf, s, c, flip, dm );
        }

        public void DrawCycle(Point p, float r, Color c, int numOfSegments, PrimitiveType dm = PrimitiveType.Polygon) {
            PointF pf = new PointF( p.X, p.Y );
            DrawCycle( pf, r, c, numOfSegments, dm );
        }

        public void DrawRect(RectangleF rect, Color c, PrimitiveType dm = PrimitiveType.Quads) {
            if ( !this.isInitialized ) {
                Error( "Trying to draw rect without intializing graphics manager!" );
            }

            IncreaseDepth();

            GL.Color3( c.R, c.G, c.B );
            GL.Begin( dm );
            GL.Vertex3( rect.X, rect.Y + rect.Height, this.currentDepth );
            GL.Vertex3( rect.X         + rect.Width,  rect.Y + rect.Height, this.currentDepth );
            GL.Vertex3( rect.X         + rect.Width,  rect.Y,               this.currentDepth );
            GL.Vertex3( rect.X,                       rect.Y,               this.currentDepth );
            GL.End();
        }

        public void DrawPoint(PointF point, Color c, PrimitiveType dm = PrimitiveType.Points) {
            if ( !this.isInitialized ) {
                Error( "Trying to draw point without intializing graphics manager!" );
            }

            IncreaseDepth();

            GL.Color3( c.R, c.G, c.B );
            GL.Begin( dm );
            GL.Vertex3( point.X, point.Y, this.currentDepth );
            GL.End();
        }

        public void DrawLine(PointF p1, PointF p2, Color c, PrimitiveType dm = PrimitiveType.Lines) {
            if ( !this.isInitialized ) {
                Error( "Trying to draw line without intializing graphics manager!" );
            }

            IncreaseDepth();

            GL.Color3( c.R, c.G, c.B );
            GL.Begin( dm );
            GL.Vertex3( p1.X, p1.Y, this.currentDepth );
            GL.Vertex3( p2.X, p2.Y, this.currentDepth );
            GL.End();
        }

        public void DrawCycle(PointF p, float r, Color c, int numOfSegments, PrimitiveType dm = PrimitiveType.Polygon) {
            if ( !this.isInitialized ) {
                Error( "Trying to draw line without intializing graphics manager!" );
            }

            IncreaseDepth();

            GL.Color3( c.R, c.G, c.B );
            GL.Begin( dm );

            for ( int ii = 0; ii < numOfSegments; ii++ ) {
                var theta = 2.0f * Math.PI * ii / numOfSegments;

                var x = r * Math.Cos( theta ); //calculate the x component
                var y = r * Math.Sin( theta ); //calculate the y component

                GL.Vertex3( x + p.X, y + p.Y, this.currentDepth ); //output vertex
            }

            GL.End();
        }

        public void DrawTriangle(PointF p, float s, Color c, bool flip = false, PrimitiveType dm = PrimitiveType.Polygon) {
            if ( !this.isInitialized ) {
                Error( "Trying to draw line without intializing graphics manager!" );
            }

            IncreaseDepth();

            GL.Color3( c.R, c.G, c.B );
            GL.Begin( PrimitiveType.Polygon );

            var h = 0.866025403784439D * s; //Math.Sqrt( (double) 3 / 4 ) * s
            
            p = new PointF(p.X -s/2, p.Y + (float)h/2);

            GL.Vertex3( p.X,             p.Y,                      this.currentDepth );
            GL.Vertex3( p.X + s,         p.Y,                      this.currentDepth );
            GL.Vertex3( p.X + ( s / 2 ), p.Y + ( flip ? +h : -h ), this.currentDepth );

            GL.End();
        }


        public void DrawString(string str, PointF position, Color color) {
            if ( !this.isInitialized ) {
                Error( "Trying to draw line without intializing graphics manager!" );
            }

            DrawString( str, new Point( (int) position.X, (int) position.Y ), color );
        }

        public void DrawString(string str, Point position, Color color) {
            if ( !this.isInitialized ) {
                Error( "Trying to draw line without intializing graphics manager!" );
            }

            GL.MatrixMode( MatrixMode.Modelview );
            GL.PushMatrix();
            GL.LoadIdentity();

            IncreaseDepth();

            // TODO: ZDepth accurate text
            float   charPieceX = ( (float) this.originalW / (float) this.fontWidth ) / 95.0f;
            float[] vertices   = new float[str.Length * 3 * 4];
            float[] texcoords  = new float[str.Length * 2 * 4];
            float[] colors     = new float[str.Length * 3 * 4];
            for ( int i = 0; i < str.Length * 4 * 3; i += 3 ) {
                colors[i + 0] = (float) color.R / 255.0f;
                colors[i + 1] = (float) color.G / 255.0f;
                colors[i + 2] = (float) color.B / 255.0f;
            }

            GL.BindTexture( TextureTarget.Texture2D, this.fontHandle );

            int vertex = 0;
            for ( int count = 0; count < str.Length; count++ ) {
                vertices[vertex++]           = position.X + this.charWidth * count;
                vertices[vertex++]           = position.Y;
                vertices[vertex++]           = this.Depth;
                texcoords[count * 2 * 4 + 0] = charPieceX * ( str[count] - 32 );
                texcoords[count * 2 * 4 + 1] = 0.0f;
                vertices[vertex++]           = position.X + this.charWidth * count;
                vertices[vertex++]           = position.Y + this.charHeight;
                vertices[vertex++]           = this.Depth;
                texcoords[count * 2 * 4 + 2] = charPieceX             * ( str[count] - 32 );
                texcoords[count * 2 * 4 + 3] = (float) this.originalH / (float) this.fontHeight;
                vertices[vertex++]           = position.X + this.charWidth * ( count + 1 );
                vertices[vertex++]           = position.Y + this.charHeight;
                vertices[vertex++]           = this.Depth;
                texcoords[count * 2 * 4 + 4] = charPieceX             * ( str[count] - 32 + 1 );
                texcoords[count * 2 * 4 + 5] = (float) this.originalH / (float) this.fontHeight;
                vertices[vertex++]           = position.X + this.charWidth * ( count + 1 );
                vertices[vertex++]           = position.Y;
                vertices[vertex++]           = this.Depth;
                texcoords[count * 2 * 4 + 6] = charPieceX * ( str[count] - 32 + 1 );
                texcoords[count * 2 * 4 + 7] = 0.0f;
            }

            GL.EnableClientState( ArrayCap.VertexArray );
            GL.EnableClientState( ArrayCap.ColorArray );
            GL.EnableClientState( ArrayCap.TextureCoordArray );
            GL.VertexPointer( 3, VertexPointerType.Float, 0, vertices );
            GL.TexCoordPointer( 2, TexCoordPointerType.Float, 0, texcoords );
            GL.ColorPointer( 3, ColorPointerType.Float, 0, colors );
            GL.DrawArrays( PrimitiveType.Quads, 0, str.Length * 4 );
            GL.DisableClientState( ArrayCap.ColorArray );
            GL.DisableClientState( ArrayCap.VertexArray );
            GL.DisableClientState( ArrayCap.TextureCoordArray );

            GL.BindTexture( TextureTarget.Texture2D, 0 );

            GL.PopMatrix();
        }

        private int GetFontTexture() {
            int id = GL.GenTexture();
            GL.BindTexture( TextureTarget.Texture2D, id );

            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear );

            // Upload the image data to the GPU
            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, this.fontWidth, this.fontHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, FontDataClass.FontData );

            GL.BindTexture( TextureTarget.Texture2D, 0 );

            // FontData = null; // If we don't set this to null, then the game can re-init
            return id;
        }
    }
}