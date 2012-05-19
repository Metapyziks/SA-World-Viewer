using System;

using OpenTK.Graphics.OpenGL;

namespace GTAMapViewer.Graphics
{
    internal class Texture
    {
        protected static int GetNextPOTS( int wid, int hei )
        {
            int max = wid > hei ? wid : hei;

            return (int) Math.Pow( 2.0, Math.Ceiling( Math.Log( max, 2.0 ) ) );
        }

        private static Texture stCurrentLoadedTexture;

        public static Texture Current
        {
            get
            {
                return stCurrentLoadedTexture;
            }
        }

        private int myID;
        private bool myLoaded;

        public TextureTarget TextureTarget { get; private set; }

        public bool Ready
        {
            get
            {
                return myID > -1;
            }
        }

        public int ID
        {
            get
            {
                if ( !Ready )
                    GL.GenTextures( 1, out myID );

                return myID;
            }
        }

        public Texture( TextureTarget target )
        {
            TextureTarget = target;

            myID = -1;
            myLoaded = false;
        }

        public void Update()
        {
            myLoaded = false;
        }

        protected virtual void Load()
        {

        }

        public void Bind()
        {
            if ( stCurrentLoadedTexture != this )
            {
                GL.BindTexture( TextureTarget, ID );
                stCurrentLoadedTexture = this;
            }

            if ( !myLoaded )
            {
                Load();
                myLoaded = true;
            }
        }
    }
}
