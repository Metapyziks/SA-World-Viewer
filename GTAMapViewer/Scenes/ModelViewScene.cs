using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using GTAMapViewer.Graphics;
using GTAMapViewer.Resource;
using GTAMapViewer.World;

namespace GTAMapViewer.Scenes
{
    internal class ModelViewScene : Scene
    {
        private const float CameraMoveSpeed = 16.0f;

        private static readonly float[] stViewDists = new float[]
        {
            512.0f, 1024.0f, 2048.0f, 3072.0f, 4096.0f, 8192.0f, 16384.0f, 32768.0f
        };

        private Camera myCamera;
        private ModelShader myShader;
        private Cell myCell;
        private TextureDictionary myTexDict;

        private int myCurViewDist;

        private bool myIgnoreMouse;
        private bool myCaptureMouse;

        public ModelViewScene( ViewerWindow gameWindow )
            : base( gameWindow )
        {
            myIgnoreMouse = false;
            myCaptureMouse = true;

            myCurViewDist = 2;
        }

        public override void OnEnter( bool firstTime )
        {
            base.OnEnter( firstTime );

            GL.ClearColor( Color4.CornflowerBlue );

            CursorVisible = !myCaptureMouse;

            if ( firstTime )
            {
                myCamera = new Camera( MathHelper.Pi / 3.0f, (float) Width / (float) Height, stViewDists[ myCurViewDist ] );
                myShader = new ModelShader();
                myShader.Camera = myCamera;
                myShader.FogColour = Color4.CornflowerBlue;
                myCell = ItemManager.GetCell( 0 );
                myTexDict = ResourceManager.LoadTextureDictionary( "ballyswater" );
            }
        }

        public override void OnResize()
        {
            myCamera.ScreenRatio = (float) Width / (float) Height;
            GL.Viewport( GameWindow.ClientRectangle );
        }

        public override void OnExit()
        {
            CursorVisible = true;
        }

        public override void OnMouseWheelChanged( MouseWheelEventArgs e )
        {
            myCurViewDist = ( myCurViewDist + e.Delta + stViewDists.Length ) % stViewDists.Length;
            myCamera.ViewDistance = stViewDists[ myCurViewDist ];
            myCamera.UpdatePerspectiveMatrix();
        }

        public override void OnKeyPress( KeyPressEventArgs e )
        {
            switch ( e.KeyChar )
            {
                case (char) 0x1b:
                    myCaptureMouse = !myCaptureMouse;
                    CursorVisible = !myCaptureMouse;
                    break;
                case 'f':
                case 'F':
                    GameWindow.Fullscreen = !GameWindow.Fullscreen;
                    break;
            }
        }

        public override void OnUpdateFrame( OpenTK.FrameEventArgs e )
        {
            Vector3 movement = new Vector3( 0.0f, 0.0f, 0.0f );
            float angleY = myCamera.Rotation.Y;
            float angleX = myCamera.Rotation.X;

            if ( Keyboard[ Key.D ] )
            {
                movement.X += (float) Math.Cos( angleY );
                movement.Z += (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.A ] )
            {
                movement.X -= (float) Math.Cos( angleY );
                movement.Z -= (float) Math.Sin( angleY );
            }
            if ( Keyboard[ Key.S ] )
            {
                movement.Z += (float) Math.Cos( angleY ) * (float) Math.Cos( angleX );
                movement.Y += (float) Math.Sin( angleX );
                movement.X -= (float) Math.Sin( angleY ) * (float) Math.Cos( angleX );
            }
            if ( Keyboard[ Key.W ] )
            {
                movement.Z -= (float) Math.Cos( angleY ) * (float) Math.Cos( angleX );
                movement.Y -= (float) Math.Sin( angleX );
                movement.X += (float) Math.Sin( angleY ) * (float) Math.Cos( angleX );
            }

            if ( movement.Length != 0 )
            {
                movement.Normalize();
                myCamera.Position = myCamera.Position + movement * CameraMoveSpeed * (float) e.Time
                    * ( Keyboard[ Key.ShiftLeft ] ? 4.0f : 1.0f )
                    * stViewDists[ myCurViewDist ] / 512.0f;
            }

            ResourceManager.CheckGLDisposals();
        }

        public override void OnMouseMove( MouseMoveEventArgs e )
        {
            if ( myIgnoreMouse )
            {
                myIgnoreMouse = false;
                return;
            }

            if ( myCaptureMouse )
            {
                Vector2 rot = myCamera.Rotation;

                rot.Y += e.XDelta / 180.0f;
                rot.X += e.YDelta / 180.0f;
                rot.X = Tools.Clamp( rot.X, (float) -Math.PI / 2.0f, (float) Math.PI / 2.0f );

                myCamera.Rotation = rot;

                myIgnoreMouse = true;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point( Bounds.Left + Width / 2, Bounds.Top + Height / 2 );
            }
        }

        public override void OnRenderFrame( OpenTK.FrameEventArgs e )
        {
            myCamera.UpdateViewMatrix();

            myShader.StartBatch();
            myCell.Render( myShader, RenderLayer.Base );
            myCell.Render( myShader, RenderLayer.Alpha1 );
            myCell.Render( myShader, RenderLayer.Alpha2 );
            myShader.EndBatch();
        }
    }
}
