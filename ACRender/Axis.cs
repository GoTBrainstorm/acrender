using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Diagnostics;

namespace Gawa.ACRender
{
    /// <summary>
    /// Class for creating and drawing the axis
    /// </summary>
    public class Axis : Gawa.ACRender.IRenderable
    {
        public Axis(Device device)
        {
            m_device = device;
        }

        public void ReInitialize()
        {
            int vertexCount = 6;

            m_vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored),
                vertexCount, m_device,
                Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

            GraphicsStream buffer = m_vertexBuffer.Lock(0, 0, LockFlags.None);
            buffer.Write(new CustomVertex.PositionColored(0, 0, 0, Color.Red.ToArgb()));
            buffer.Write(new CustomVertex.PositionColored(1, 0, 0, Color.Red.ToArgb()));
            buffer.Write(new CustomVertex.PositionColored(0, 0, 0, Color.Green.ToArgb()));
            buffer.Write(new CustomVertex.PositionColored(0, 1, 0, Color.Green.ToArgb()));
            buffer.Write(new CustomVertex.PositionColored(0, 0, 0, Color.Blue.ToArgb()));
            buffer.Write(new CustomVertex.PositionColored(0, 0, 1, Color.Blue.ToArgb()));
            m_vertexBuffer.Unlock();
        }

        public void DisposeDueToLostDevice()
        {
            if (m_vertexBuffer != null)
            {
                m_vertexBuffer.Dispose();
                m_vertexBuffer = null;
            }
        }

        /// <summary>
        /// Renders the axis
        /// </summary>
        public void Render()
        {
            m_device.SetStreamSource(0, m_vertexBuffer, 0);
            m_device.VertexFormat = m_vertexBuffer.Description.VertexFormat;
            m_device.DrawPrimitives(PrimitiveType.LineList, 0, 3);
        }

        /// <summary>
        /// The vertex buffer
        /// </summary>
        private VertexBuffer m_vertexBuffer;
        /// <summary>
        /// The device.
        /// </summary>
        private Device m_device;
    }
}
