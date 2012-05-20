using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GTAMapViewer.Resource;

namespace GTAMapViewer.Items
{
    internal class ObjectDefinition
    {
        public readonly String ModelName;
        public readonly String TextureDictName;

        public Model Model
        {
            get;
            private set;
        }

        public bool ModelLoaded
        {
            get { return Model != null; }
        }

        public readonly float DrawDist;
        public readonly ObjectFlag Flags;

        public ObjectDefinition( String model, String txd, float drawDist, ObjectFlag flags )
        {
            ModelName = model;
            TextureDictName = txd;
            DrawDist = drawDist;
            Flags = flags;
        }

        public bool HasFlags( ObjectFlag flag )
        {
            return ( Flags & flag ) == flag;
        }

        public void Load()
        {
            Model = ResourceManager.LoadModel( ModelName, TextureDictName );
        }

        public void Unload()
        {
            ResourceManager.UnloadModel( ModelName, TextureDictName );
            Model = null;
        }
    }
}
