using System.Collections.Generic;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.World
{
    internal abstract class Cell
    {
        private List<InstPlacement> myTempPlacements;

        public Cell()
        {
            myTempPlacements = new List<InstPlacement>();
        }

        public void AddPlacement( InstPlacement placement )
        {
            myTempPlacements.Add( placement );
        }

        public void AddPlacements( IEnumerable<InstPlacement> placements )
        {
            myTempPlacements.AddRange( placements );
        }

        public void FinalisePlacements()
        {
            OnFinalisePlacements( myTempPlacements );
            myTempPlacements = null;
        }

        protected abstract void OnFinalisePlacements( IEnumerable<InstPlacement> placements );
        public abstract IEnumerable<Instance> GetInstances();
        public abstract void Render( ModelShader shader, RenderLayer layer );
    }
}
