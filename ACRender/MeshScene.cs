using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Diagnostics;

namespace Gawa.ACRender
{
    public abstract class MeshScene<T> : IRenderable
    {
        public MeshScene(Device device, SimpleMesh mesh)
        {
            m_device = device;
            m_renderSubMeshOnly = false;
            m_noTexturingOnSimpleMeshes = false;
            m_texturedTrifans = new Dictionary<int, TrifanDrawInfo>();
            m_coloredTrifans = new Dictionary<int, TrifanDrawInfo>();
            Debug.Assert(mesh != null);
            m_simpleMesh = mesh;
        }

        /// <summary>
        /// Returns whether the specified mesh ID can be rendered using
        /// the implementing scene
        /// </summary>
        /// <param name="meshId">The ID of the mesh that needs to
        /// be checked.</param>
        /// <returns>Returns whether the specified mesh ID is
        /// valid for the implementing scene.</returns>
        public abstract bool IsValidMeshID(uint meshId);

        public void DisposeDueToLostDevice()
        {
            if (m_vbTextured != null)
            {
                m_vbTextured.Dispose();
                m_vbTextured = null;
            }
            if (m_ibTextured != null)
            {
                m_ibTextured.Dispose();
                m_ibTextured = null;
            }
            if (m_textures != null)
            {
                foreach (Texture texture in m_textures)
                {
                    if (texture != null)
                    {
                        texture.Dispose();
                    }
                }
                m_textures = null;
            }
            if (m_vbColored != null)
            {
                m_vbColored.Dispose();
                m_vbColored = null;
            }
            if (m_ibColored != null)
            {
                m_ibColored.Dispose();
                m_ibColored = null;
            }
        }

        /// <summary>
        /// Initialized the vertex & index buffers for this mesh scene
        /// </summary>
        public void ReInitialize()
        {
            if (m_noTexturing)
            {
                ReInitializeWithoutTexturing();
                return;
            }

            // prepare vars
            m_coloredTrifans.Clear();
            m_texturedTrifans.Clear();

            // loop through the texturing info and determine for each of them what
            // type of decoration it is.
            m_decorations = new MeshDecoration[m_simpleMesh.TextureInfo.Length];
            m_textures = new Texture[m_decorations.Length];
            for (int i = 0; i < m_decorations.Length; i++)
            {
                m_decorations[i] = new MeshDecoration(m_simpleMesh.TextureInfo[i]);
                if (m_decorations[i].DecorationType == DecorationType.Texture)
                {
                    Bitmap bmp = m_decorations[i].Texture.GetBitmap();
                    m_textures[i] = new Texture(m_device, bmp, 0, Pool.Managed);
                }
            }

            // we'll have to create two sets of index buffers:
            // one for textured trifans and one for colored trifans
            // we'll have to determine for each trifan to which of these it belongs

            List<VertexLookup> texturedVertexTable = new List<VertexLookup>();
            List<VertexLookup> coloredVertexTable = new List<VertexLookup>();
            List<ushort> texturedIndexList = new List<ushort>();
            List<ushort> coloredIndexList = new List<ushort>();

            for (int i = 0; i < (int)m_simpleMesh.SecondTrifanSet.TrifanCount; i++)
            {
                TrifanInfo trifan = m_simpleMesh.SecondTrifanSet.TrifanData[i];
                bool drawSolidColor = false;
                drawSolidColor = (trifan.Flags & 0x04) == 0x04;
                // draw solid when this trifan is drawn using a solid color
                drawSolidColor = drawSolidColor || (m_decorations[(int)trifan.TextureIndex].DecorationType == DecorationType.SolidColor);

                if (drawSolidColor)
                {
                    Color color = m_decorations[(int)trifan.TextureIndex].SolidColor;
                    TrifanDrawInfo trifanInfo = new TrifanDrawInfo();
                    trifanInfo.IndexBufferStart = (ushort)coloredIndexList.Count;
                    trifanInfo.PrimitiveCount = trifan.VertexCount - 2;
                    for (int j = 0; j < trifan.VertexIndices.Length; j++)
                    {
                        VertexLookup lookup = new VertexLookup(trifan.VertexIndices[j], 0, color);
                        ushort vertexIndex;
                        if (!coloredVertexTable.Contains(lookup))
                        {
                            coloredVertexTable.Add(lookup);
                        }
                        vertexIndex = (ushort)coloredVertexTable.IndexOf(lookup);
                        coloredIndexList.Add(vertexIndex);
                    }
                    m_coloredTrifans.Add(i, trifanInfo);
                }
                else
                {
                    TrifanDrawInfo trifanInfo = new TrifanDrawInfo();
                    trifanInfo.IndexBufferStart = (ushort)texturedIndexList.Count;
                    trifanInfo.PrimitiveCount = trifan.VertexCount - 2;
                    for (int j = 0; j < trifan.VertexIndices.Length; j++)
                    {
                        VertexLookup lookup = new VertexLookup(trifan.VertexIndices[j], trifan.UVIndex[j]);
                        ushort vertexIndex;
                        if (!texturedVertexTable.Contains(lookup))
                        {
                            texturedVertexTable.Add(lookup);
                        }
                        vertexIndex = (ushort)texturedVertexTable.IndexOf(lookup);
                        texturedIndexList.Add(vertexIndex);
                    }
                    m_texturedTrifans.Add(i, trifanInfo);
                }
            }

            // now create the actual index & vertex buffers

            // texture vb & ib
            if (texturedVertexTable.Count > 0)
            {
                m_vbTextured = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured),
                    texturedVertexTable.Count, m_device, Usage.WriteOnly,
                    CustomVertex.PositionNormalTextured.Format, Pool.Managed);
                GraphicsStream stream = m_vbTextured.Lock(0, 0, LockFlags.None);
                foreach (VertexLookup lookup in texturedVertexTable)
                {
                    VertexInfo vertex = m_simpleMesh.VertexInfo[(int)lookup.VertexId];
                    stream.Write(new CustomVertex.PositionNormalTextured(vertex.X,
                        vertex.Y, vertex.Z, vertex.NX, vertex.NY, vertex.NZ,
                        vertex.CUVData[lookup.UVIndex].U, vertex.CUVData[lookup.UVIndex].V));
                }
                m_vbTextured.Unlock();

                m_ibTextured = new IndexBuffer(typeof(ushort), texturedIndexList.Count,
                    m_device, Usage.WriteOnly, Pool.Managed);
                stream = m_ibTextured.Lock(0, 0, LockFlags.None);
                stream.Write(texturedIndexList.ToArray());
                m_ibTextured.Unlock();
            }
            m_vbTexturedLength = texturedVertexTable.Count;

            // color vb & ib
            if (coloredVertexTable.Count > 0)
            {
                m_vbColored = new VertexBuffer(typeof(CustomVertex.PositionNormalColored),
                coloredVertexTable.Count, m_device, Usage.WriteOnly,
                CustomVertex.PositionNormalColored.Format, Pool.Managed);
                GraphicsStream stream = m_vbColored.Lock(0, 0, LockFlags.None);
                foreach (VertexLookup lookup in coloredVertexTable)
                {
                    VertexInfo vertex = m_simpleMesh.VertexInfo[(int)lookup.VertexId];
                    stream.Write(new CustomVertex.PositionNormalColored(vertex.X,
                        vertex.Y, vertex.Z, vertex.NX, vertex.NY, vertex.NZ,
                        lookup.Color.ToArgb()));
                }
                m_vbColored.Unlock();

                m_ibColored = new IndexBuffer(typeof(ushort), coloredIndexList.Count,
                    m_device, Usage.WriteOnly, Pool.Managed);
                stream = m_ibColored.Lock(0, 0, LockFlags.None);
                stream.Write(coloredIndexList.ToArray());
                m_ibColored.Unlock();
            }
            m_vbColoredLength = coloredVertexTable.Count;
        }

        public void ReInitializeWithoutTexturing()
        {
            // prepare vars
            m_coloredTrifans.Clear();
            m_texturedTrifans.Clear();

            // loop through the texturing info and determine for each of them what
            // type of decoration it is.
            m_decorations = new MeshDecoration[0];
            m_textures = new Texture[0];


            // create a single trifan set of red trifans
            List<VertexLookup> coloredVertexTable = new List<VertexLookup>();
            List<ushort> coloredIndexList = new List<ushort>();

            for (int i = 0; i < (int)m_simpleMesh.SecondTrifanSet.TrifanCount; i++)
            {
                TrifanInfo trifan = m_simpleMesh.SecondTrifanSet.TrifanData[i];

                Color color = Color.Red;
                TrifanDrawInfo trifanInfo = new TrifanDrawInfo();
                trifanInfo.IndexBufferStart = (ushort)coloredIndexList.Count;
                trifanInfo.PrimitiveCount = trifan.VertexCount - 2;
                for (int j = 0; j < trifan.VertexIndices.Length; j++)
                {
                    VertexLookup lookup = new VertexLookup(trifan.VertexIndices[j], 0, color);
                    ushort vertexIndex;
                    if (!coloredVertexTable.Contains(lookup))
                    {
                        coloredVertexTable.Add(lookup);
                    }
                    vertexIndex = (ushort)coloredVertexTable.IndexOf(lookup);
                    coloredIndexList.Add(vertexIndex);
                }
                m_coloredTrifans.Add(i, trifanInfo);
            }

            // now create the actual index & vertex buffers

            // texture vb & ib
            m_vbTexturedLength = 0;

            // color vb & ib
            if (coloredVertexTable.Count > 0)
            {
                m_vbColored = new VertexBuffer(typeof(CustomVertex.PositionNormalColored),
                coloredVertexTable.Count, m_device, Usage.WriteOnly,
                CustomVertex.PositionNormalColored.Format, Pool.Managed);
                GraphicsStream stream = m_vbColored.Lock(0, 0, LockFlags.None);
                foreach (VertexLookup lookup in coloredVertexTable)
                {
                    VertexInfo vertex = m_simpleMesh.VertexInfo[(int)lookup.VertexId];
                    stream.Write(new CustomVertex.PositionNormalColored(vertex.X,
                        vertex.Y, vertex.Z, vertex.NX, vertex.NY, vertex.NZ,
                        lookup.Color.ToArgb()));
                }
                m_vbColored.Unlock();

                m_ibColored = new IndexBuffer(typeof(ushort), coloredIndexList.Count,
                    m_device, Usage.WriteOnly, Pool.Managed);
                stream = m_ibColored.Lock(0, 0, LockFlags.None);
                stream.Write(coloredIndexList.ToArray());
                m_ibColored.Unlock();
            }
            m_vbColoredLength = coloredVertexTable.Count;
        }

        /// <summary>
        /// Helper struct for storing info about how we'll draw each trifan
        /// </summary>
        protected struct TrifanDrawInfo
        {
            public ushort IndexBufferStart;
            public int PrimitiveCount;
        }

        protected struct VertexLookup
        {
            public VertexLookup(ushort vertexId, byte uvIndex)
            {
                VertexId = vertexId;
                UVIndex = uvIndex;
                Color = Color.Empty;
            }

            public VertexLookup(ushort vertexId, byte uvIndex, Color color)
            {
                VertexId = vertexId;
                UVIndex = uvIndex;
                Color = color;
            }

            public ushort VertexId;
            public byte UVIndex;
            public Color Color;
        }

        public void Render()
        {
            lock (this)
            {
                if (m_renderSubMeshOnly)
                {
                    RenderSingleTrifan(m_subMeshIndex);
                    return;
                }


                if (m_vbTexturedLength > 0)
                {
                    m_device.SetStreamSource(0, m_vbTextured, 0);
                    m_device.Indices = m_ibTextured;
                    m_device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
                    int currentTextureId = -1;

                    /*
                    m_pDirect3DDevice->SetRenderState(D3DRS_ALPHABLENDENABLE,true);		//alpha blending enabled
m_pDirect3DDevice->SetRenderState(D3DRS_SRCBLEND,D3DBLEND_SRCALPHA);	//source blend factor
m_pDirect3DDevice->SetRenderState(D3DRS_DESTBLEND,D3DBLEND_INVSRCALPHA);	//destination blend factor

m_pDirect3DDevice->SetTextureStageState(0,D3DTSS_ALPHAARG1,D3DTA_TEXTURE);	//alpha from texture
                     * */

                    m_device.RenderState.AlphaBlendEnable = true;
                    m_device.RenderState.SourceBlend = Blend.SourceAlpha;
                    m_device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
                    m_device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;

                    // render each textured trifan
                    foreach (int trifanId in m_texturedTrifans.Keys)
                    {
                        TrifanDrawInfo info = m_texturedTrifans[trifanId];
                        if (currentTextureId != m_simpleMesh.SecondTrifanSet.TrifanData[trifanId].TextureIndex)
                        {
                            currentTextureId = (int)m_simpleMesh.SecondTrifanSet.TrifanData[trifanId].TextureIndex;
                            m_device.SetTexture(0, m_textures[currentTextureId]);
                        }
                        m_device.DrawIndexedPrimitives(PrimitiveType.TriangleFan, 0, 0,
                            m_vbTexturedLength, info.IndexBufferStart, info.PrimitiveCount);
                    }
                    m_device.SetTexture(0, null);
                }

                if (m_vbColoredLength > 0)
                {
                    m_device.SetStreamSource(0, m_vbColored, 0);
                    m_device.Indices = m_ibColored;
                    m_device.VertexFormat = CustomVertex.PositionNormalColored.Format;

                    // render each colored trifan
                    foreach (int trifanId in m_coloredTrifans.Keys)
                    {
                        TrifanDrawInfo info = m_coloredTrifans[trifanId];
                        m_device.DrawIndexedPrimitives(PrimitiveType.TriangleFan, 0, 0,
                            m_vbColoredLength, info.IndexBufferStart, info.PrimitiveCount);
                    }
                }

                m_device.SetStreamSource(0, null, 0);
                m_device.Indices = null;
            }
        }

        public void RenderSingleTrifan(int index)
        {
            bool isTextured = m_texturedTrifans.ContainsKey(index);
            if (isTextured)
            {
                m_device.SetStreamSource(0, m_vbTextured, 0);
                m_device.Indices = m_ibTextured;
                m_device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
                int currentTextureId = -1;

                TrifanDrawInfo info = m_texturedTrifans[index];
                if (currentTextureId != m_simpleMesh.SecondTrifanSet.TrifanData[index].TextureIndex)
                {
                    currentTextureId = (int)m_simpleMesh.SecondTrifanSet.TrifanData[index].TextureIndex;
                    m_device.SetTexture(0, m_textures[currentTextureId]);
                }
                m_device.DrawIndexedPrimitives(PrimitiveType.TriangleFan, 0, 0,
                    m_vbTexturedLength, info.IndexBufferStart, info.PrimitiveCount);
                m_device.SetTexture(0, null);
            }
            else
            {
                m_device.SetStreamSource(0, m_vbColored, 0);
                m_device.Indices = m_ibColored;
                m_device.VertexFormat = CustomVertex.PositionNormalColored.Format;

                // render each colored trifan
                TrifanDrawInfo info = m_coloredTrifans[index];
                m_device.DrawIndexedPrimitives(PrimitiveType.TriangleFan, 0, 0,
                    m_vbColoredLength, info.IndexBufferStart, info.PrimitiveCount);
            }
        }

        /// <summary>
        /// Changes the mesh to the specified ID
        /// </summary>
        /// <param name="id">The ID of the new mesh</param>
        public void ChangeMesh(uint id)
        {
            lock (this)
            {
                DisposeDueToLostDevice();

                // if we're loading a dungeon block, then we'll have to convert this
                // to a simple mesh
                if ((id & 0xFF000000) == 0x0D000000)
                {
                    DungeonBlock block = DungeonProvider.Instance.GetDungeonBlock(id);
                    m_noTexturing = true;
                    m_simpleMesh = new SimpleMesh(block);
                }
                else
                {
                    m_noTexturing = m_noTexturingOnSimpleMeshes;
                    m_simpleMesh = MeshProvider.Instance.LoadSimpleMesh(id);
                }
                m_subMeshIndex = 0;
                ReInitialize();
            }
        }

        public bool RenderSubMeshOnly
        {
            get { return m_renderSubMeshOnly; }
            set { m_renderSubMeshOnly = value; }
        }

        public int SubMeshIndex
        {
            get { return m_subMeshIndex; }
        }

        public int SubMeshCount
        {
            get { return (int)m_simpleMesh.SecondTrifanSet.TrifanCount; }
        }

        public void CycleSubMesh()
        {
            m_subMeshIndex = (m_subMeshIndex + 1) % (int)m_simpleMesh.SecondTrifanSet.TrifanCount;
        }

        public SimpleMesh SimpleMesh
        {
            get { return m_simpleMesh; }
        }

        public bool NoTexturingOnSimpleMeshes
        {
            get { return m_noTexturingOnSimpleMeshes; }
            set { m_noTexturingOnSimpleMeshes = value; }
        }

        /// <summary>
        /// Whether we should render only submeshes
        /// </summary>
        private bool m_renderSubMeshOnly;
        /// <summary>
        /// The index of the submesh number that's crrently visible
        /// </summary>
        private int m_subMeshIndex;
        /// <summary>
        /// The index of the texture that's currently in stage 0
        /// </summary>
        //private int m_currentTextureId;
        /// <summary>
        /// Length of the textured vertex buffer
        /// </summary>
        private int m_vbTexturedLength;
        /// <summary>
        /// Length of the colored vertex buffer
        /// </summary>
        private int m_vbColoredLength;
        /// <summary>
        /// Vertex buffer for textured trifans
        /// </summary>
        private VertexBuffer m_vbTextured;
        /// <summary>
        /// Index buffer for textured trifans
        /// </summary>
        private IndexBuffer m_ibTextured;
        /// <summary>
        /// Texture collection
        /// </summary>
        private Texture[] m_textures;
        /// <summary>
        /// Collection of mesh decorations
        /// </summary>
        private MeshDecoration[] m_decorations;
        /// <summary>
        /// Vertex buffer for colored trifans
        /// </summary>
        private VertexBuffer m_vbColored;
        /// <summary>
        /// Index buffer for colored trifans
        /// </summary>
        private IndexBuffer m_ibColored;
        /// <summary>
        /// The mesh
        /// </summary>
        protected SimpleMesh m_simpleMesh;
        /// <summary>
        /// The device.
        /// </summary>
        private Device m_device;
        /// <summary>
        /// List of trifans that are textured
        /// </summary>
        private Dictionary<int, TrifanDrawInfo> m_texturedTrifans;
        /// <summary>
        /// List of trifans that are colored.
        /// </summary>
        private Dictionary<int, TrifanDrawInfo> m_coloredTrifans;
        /// <summary>
        /// Avoid texturing if set
        /// </summary>
        private bool m_noTexturing;
        /// <summary>
        /// Whether to do texturing on simple meshes
        /// </summary>
        private bool m_noTexturingOnSimpleMeshes;
    }
}
