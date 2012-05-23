using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GTAMapViewer.Resource;
using GTAMapViewer.Graphics;

namespace GTAMapViewer.World
{
    internal class ObjectDefinition
    {
        private int myUses;

        public readonly String ModelName;
        public readonly String TextureDictName;

        public Model Model
        {
            get;
            set;
        }

        public bool ModelRequested
        {
            get;
            private set;
        }

        public bool Loaded
        {
            get { return Model != null; }
        }

        public RenderLayer RenderLayer
        {
            get
            {
                return HasFlags( ObjectFlag.Alpha2 ) ?
                    RenderLayer.Alpha2 : HasFlags( ObjectFlag.Alpha1 ) ?
                    RenderLayer.Alpha1 : Graphics.RenderLayer.Base;
            }
        }

        public readonly float DrawDist;
        public readonly float DrawDist2;
        public readonly ObjectFlag Flags;

        public int Uses
        {
            get { return myUses; }
            set
            {
                if ( myUses != value )
                {
                    myUses = value;

                    if ( value == 0 && ModelRequested )
                        Unload();
                }
            }
        }

        public ObjectDefinition( String model, String txd, float drawDist, ObjectFlag flags )
        {
            ModelName = model;
            TextureDictName = txd;
            DrawDist = drawDist;
            DrawDist2 = DrawDist * DrawDist;
            Flags = flags;

            ModelRequested = false;
        }

        public bool HasFlags( ObjectFlag flag )
        {
            return ( Flags & flag ) == flag;
        }

        public void RequestModel()
        {
            if ( !ModelRequested )
            {
                ModelRequested = true;
                ResourceManager.RequestModel( this );
            }
        }

        public void Unload()
        {
            if ( ModelRequested )
            {
                Model = null;
                ModelRequested = false;
                ResourceManager.UnloadModel( this );
            }
        }
    }
}
