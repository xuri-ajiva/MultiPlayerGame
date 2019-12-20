using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenGL;
using OpenTK.Graphics;

namespace Game {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            var g = new GlControl();

            this.Controls.Add( g );
            g.Render += GOnRender;
            g.ContextCreated += GOnContextCreated;
            g.ContextDestroying += GOnContextDestroying;
            g.ContextUpdate += GOnContextUpdate;
            g.Dock = DockStyle.Fill;
            g.AnimationTimer = true;
            g.AnimationTime = 1000;
            g.DoubleBuffer = true;
            g.Animation = true;

        }

        private void GOnContextUpdate(object sender, GlControlEventArgs e) {
            Console.WriteLine( "update" );
        }

        private void GOnContextDestroying(object sender, GlControlEventArgs e) {
            
            Console.WriteLine( "destroy" );
        }

        private void GOnContextCreated(object sender, GlControlEventArgs e) {
            Console.WriteLine( "create" );
        }

        private void GOnRender(object sender, GlControlEventArgs e) {
            Console.WriteLine( "render" );
        }

    }
}