using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Input;
using OpenTK;

namespace GTAMapViewer.Scenes
{
    internal class Scene : IDisposable
    {
        internal bool FirstTime;

        public ViewerWindow GameWindow { get; private set; }

        public int Width
        {
            get { return GameWindow.Width; }
        }

        public int Height
        {
            get { return GameWindow.Height; }
        }

        public System.Drawing.Rectangle Bounds
        {
            get { return GameWindow.Bounds; }
        }

        public bool IsCurrent
        {
            get { return GameWindow.CurrentScene == this; }
        }

        public KeyboardDevice Keyboard
        {
            get { return GameWindow.Keyboard; }
        }

        public MouseDevice Mouse
        {
            get { return GameWindow.Mouse; }
        }

        public bool CursorVisible
        {
            get { return GameWindow.CursorVisible; }
            set { GameWindow.CursorVisible = value; }
        }

        public Scene( ViewerWindow gameWindow )
        {
            GameWindow = gameWindow;
            FirstTime = true;
        }

        public virtual void OnEnter( bool firstTime )
        {

        }

        public virtual void OnExit()
        {

        }

        public virtual void OnMouseButtonDown( MouseButtonEventArgs e )
        {

        }

        public virtual void OnMouseButtonUp( MouseButtonEventArgs e )
        {

        }

        public virtual void OnMouseMove( MouseMoveEventArgs e )
        {

        }

        public virtual void OnMouseLeave( EventArgs e )
        {

        }

        public virtual void OnMouseEnter( EventArgs e )
        {

        }

        public virtual void OnMouseWheelChanged( MouseWheelEventArgs e )
        {

        }

        public virtual void OnKeyPress( KeyPressEventArgs e )
        {

        }

        public virtual void OnUpdateFrame( FrameEventArgs e )
        {

        }

        public virtual void OnRenderFrame( FrameEventArgs e )
        {

        }

        public virtual void Dispose()
        {

        }
    }
}
