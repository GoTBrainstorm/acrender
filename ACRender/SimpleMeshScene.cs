using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Diagnostics;

namespace Gawa.ACRender
{
    public class SimpleMeshScene : MeshScene<SimpleMesh>
    {
        public SimpleMeshScene(Device device, uint simpleMeshId)
            : base(device, MeshProvider.Instance.LoadSimpleMesh(simpleMeshId))
        {
        }

        public override bool IsValidMeshID(uint meshId)
        {
            return true;
        }
    }
}
