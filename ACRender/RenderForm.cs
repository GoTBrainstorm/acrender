using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using D3D = Microsoft.DirectX.Direct3D;
using Font = System.Drawing.Font;

namespace Gawa.ACRender
{
    public partial class RenderForm : Form
    {
        public RenderForm()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
            Width = 800;
            Height = 600;

            m_deviceLost = false;
            m_device = null;
            m_presentation = null;
            m_mustRun = true;
            m_doLighting = false;
            m_doWires = false;
            m_invertBG = false;
            m_veryQuickMouseZoom = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                CreateDevice();
                (new Thread(new ThreadStart(RenderLoop))).Start();
            }
            catch (Exception)
            {
                Debugger.Break();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (m_mustRun)
            {
                m_mustRun = false;
                e.Cancel = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            m_device_DeviceLost(null, null);
            m_device.Dispose();
            base.OnClosed(e);
        }

        /// <summary>
        /// Creates the rendering device
        /// </summary>
        private void CreateDevice()
        {
            // Get the adapter ordinal
            int adapterOrdinal = Manager.Adapters.Default.Adapter;

            // set up capabilities
            Caps caps = Manager.GetDeviceCaps(adapterOrdinal,
                                              DeviceType.Hardware);
            CreateFlags flags = CreateFlags.SoftwareVertexProcessing;

            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
            {
                flags = CreateFlags.HardwareVertexProcessing;
            }
            if (caps.DeviceCaps.SupportsPureDevice)
            {
                flags |= CreateFlags.PureDevice;
            }

            // ste up presentation
            m_presentation = new PresentParameters();
            m_presentation.Windowed = true;
            m_presentation.SwapEffect = SwapEffect.Discard;
            //m_presentation.PresentFlag = PresentFlag.None;
            //m_presentation.MultiSample = MultiSampleType.None;
            //m_presentation.BackBufferWidth = Width;
            //m_presentation.BackBufferHeight = Height;
            m_presentation.EnableAutoDepthStencil = true;
            m_presentation.AutoDepthStencilFormat = DepthFormat.D16;

            // create the device and attach event handlers
            m_device = new Device(adapterOrdinal, DeviceType.Hardware, Handle, flags, m_presentation);
            m_device.DeviceLost += new EventHandler(m_device_DeviceLost);
            m_device.DeviceReset += new EventHandler(m_device_DeviceReset);

            m_camera = new Camera(Width, Height);
            m_axis = new Axis(m_device);
            //m_meshScene = new SimpleMeshScene(m_device, 0x01002752);
            //m_meshScene = new SimpleMeshScene(m_device, 0x0100005E);
            //m_meshScene = new SimpleMeshScene(m_device, 0x0100002C);
            m_meshScene = new SimpleMeshScene(m_device, 0x0100058F);

            //m_meshScene = new SimpleMeshScene(m_device, 0x010025e3);

            //m_meshScene = new SimpleMeshScene(m_device, 0x010000D6);

            m_device_DeviceReset(null, null);

            MouseMove += new MouseEventHandler(RenderForm_MouseMove);
            MouseUp += new MouseEventHandler(RenderForm_MouseUp);
            MouseWheel += new MouseEventHandler(RenderForm_MouseWheel);
            KeyUp += new KeyEventHandler(RenderForm_KeyUp);
        }

        /// <summary>
        /// Called when the device resets
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">args</param>
        private void m_device_DeviceReset(object sender, EventArgs e)
        {
            // set the projection matrix
            m_device.Transform.Projection = m_camera.Projection;
            m_device.Transform.World = m_camera.World;

            // set the viewstate
            m_device.RenderState.CullMode = Cull.None;
            m_device.RenderState.ZBufferEnable = true;

            m_debugFont = new Microsoft.DirectX.Direct3D.Font(m_device,
                                                              new Font(FontFamily.GenericSansSerif, 10.0f));

            m_axis.ReInitialize();
            m_meshScene.ReInitialize();
        }

        /// <summary>
        /// Called when the device got lost
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void m_device_DeviceLost(object sender, EventArgs e)
        {
            // dispose of our components
            m_axis.DisposeDueToLostDevice();
            m_meshScene.DisposeDueToLostDevice();

            if (m_debugFont != null)
            {
                m_debugFont.Dispose();
                m_debugFont = null;
            }
        }

        /// <summary>
        /// Attempts to recover the device. Will be called when we know
        /// that it was lost.
        /// </summary>
        protected void AttemptRecovery()
        {
            try
            {
                m_device.CheckCooperativeLevel();
            }
            catch (DeviceLostException)
            {
            }
            catch (DeviceNotResetException)
            {
                try
                {
                    // try to reset the device using our presentation params
                    m_device.Reset(m_presentation);
                    m_deviceLost = false;
                }
                catch (DeviceLostException)
                {
                    // do nothing if it's still lost
                }
            }
        }

        private void RenderLoop()
        {
            while (m_mustRun)
            {
                try
                {
                    // render the scene
                    RenderScene();
                }
                catch (Exception)
                {
                    Debugger.Break();
                }

                // update the model
                Thread.Sleep(20);
            }

            EventHandler hander = delegate { Close(); };

            Invoke(hander);
        }

        /// <summary>
        /// Renders the scene
        /// </summary>
        private void RenderScene()
        {
            // Try to get the device back if it got lost
            if (m_deviceLost)
            {
                AttemptRecovery();
            }
            // If we couldn't get the device back, don't try to render
            if (m_deviceLost)
            {
                return;
            }

            try
            {
                if (m_invertBG)
                {
                    m_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.White, 1.0f, 0);
                }
                else
                {
                    m_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                }
                m_device.BeginScene();

                m_device.Transform.View = m_camera.View;

                // render the axis without lighting
                m_device.RenderState.Lighting = false;
                m_axis.Render();


                Material mat = new Material();
                mat.Diffuse = Color.White;
                mat.Ambient = Color.White;
                m_device.Material = mat;

                m_device.RenderState.Lighting = m_doLighting;
                if (m_doLighting)
                {
                    m_device.Lights[0].Type = LightType.Directional;
                    m_device.Lights[0].Diffuse = Color.White;
                    m_device.Lights[0].Direction = new Vector3(-3, 0, 0);
                    m_device.Lights[0].Update();
                    m_device.Lights[0].Enabled = true;
                    m_device.RenderState.Ambient = Color.FromArgb(0x505050);
                }

                if (m_doWires)
                {
                    m_device.RenderState.FillMode = FillMode.WireFrame;
                }
                else
                {
                    m_device.RenderState.FillMode = FillMode.Solid;
                }

                m_meshScene.Render();

                // draw debug text
                int textX = 20, textY = 30;
                DrawText(ref textX, ref textY, "Submesh rendering: {0}. Submesh #{1}  (total amount: {2})",
                         m_meshScene.RenderSubMeshOnly, m_meshScene.SubMeshIndex, m_meshScene.SubMeshCount);
                DrawText(ref textX, ref textY,
                         "Lighting: {0}   Wireframe: {1}   Inverted BG: {2}    Quick zoom: {3}    No simple mesh texturing: {4}",
                         m_doLighting, m_doWires, m_invertBG, m_veryQuickMouseZoom,
                         m_meshScene.NoTexturingOnSimpleMeshes);
                DrawText(ref textX, ref textY, "Rendering mesh 0x{0:X8}", m_meshScene.SimpleMesh.ID);

                m_device.EndScene();
                m_device.Present();

                EventHandler handler = delegate { Invalidate(); };
                Invoke(handler);
            }
            catch (DeviceNotResetException)
            {
            }
            catch (DeviceLostException)
            {
                // Indicate that the device has been lost
                m_deviceLost = true;
            }
            catch (DirectXException)
            {
                Debugger.Break();
            }
        }

        private void DrawText(ref int x, ref int y, string format, params object[] args)
        {
            if (m_invertBG)
            {
                m_debugFont.DrawText(null, String.Format(format, args), x, y, Color.Black);
            }
            else
            {
                m_debugFont.DrawText(null, String.Format(format, args), x, y, Color.White);
            }

            y += m_debugFont.Description.Height;
        }

        private void RenderForm_MouseWheel(object sender, MouseEventArgs e)
        {
            float magnitude = .001f;
            if (m_veryQuickMouseZoom)
            {
                magnitude = .03f;
            }
            m_camera.Zoom(-e.Delta * magnitude);
        }

        private void RenderForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (m_mouseIsDown)
                {
                    Point delta = new Point(e.X - m_lastMouseLoc.X, e.Y - m_lastMouseLoc.Y);
                    m_camera.Rotate(delta.X * .03f, -(float)delta.Y * .03f);
                }
                m_lastMouseLoc = new Point(e.X, e.Y);
                m_mouseIsDown = true;
            }
        }

        private void RenderForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_mouseIsDown = false;
            }
        }

        private void RenderForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.M)
            {
                if (m_simpleMeshSelection == null)
                {
                    m_simpleMeshSelection = new SimpleMeshSelection();
                }

                if (m_simpleMeshSelection.ShowDialog(this) == DialogResult.OK)
                {
                    uint selectedFileId = m_simpleMeshSelection.SelectedFileId;
                    m_meshScene.ChangeMesh(selectedFileId);
                }
            }
            else if (e.KeyCode == Keys.D)
            {
                if (m_dungeonBlockSelection == null)
                {
                    m_dungeonBlockSelection = new DungeonBlockSelection();
                }

                if (m_dungeonBlockSelection.ShowDialog(this) == DialogResult.OK)
                {
                    uint selectedFileId = m_dungeonBlockSelection.SelectedFileId;
                    m_meshScene.ChangeMesh(selectedFileId);
                }
            }
            else if (e.KeyCode == Keys.N)
            {
                if (m_simpleMeshSelection == null)
                {
                    m_simpleMeshSelection = new SimpleMeshSelection();
                }
                uint nextID = m_simpleMeshSelection.FindNext(m_meshScene.SimpleMesh.ID);
                m_meshScene.ChangeMesh(nextID);
            }
            else if (e.KeyCode == Keys.P)
            {
                if (m_simpleMeshSelection == null)
                {
                    m_simpleMeshSelection = new SimpleMeshSelection();
                }
                uint nextID = m_simpleMeshSelection.FindPrevious(m_meshScene.SimpleMesh.ID);
                m_meshScene.ChangeMesh(nextID);
            }
            else if (e.KeyCode == Keys.U)
            {
                UsageList usage = new UsageList(m_meshScene.SimpleMesh.ID);
                usage.ShowDialog(this);
            }
            else if (e.KeyCode == Keys.K)
            {
                if (m_complexMeshSelection == null)
                {
                    m_complexMeshSelection = new ComplexMeshSelection();
                }

                if (m_complexMeshSelection.ShowDialog(this) == DialogResult.OK)
                {
                    //uint selectedFileId = m_complexMeshSelection.SelectedFileId;
                    //m_meshScene.ChangeMesh(selectedFileId);
                }
            }
            else if (e.KeyCode == Keys.T)
            {
                if (m_texturePreview == null)
                {
                    m_texturePreview = new TexturePreview();
                }

                if (m_texturePreview.ShowDialog(this) == DialogResult.OK)
                {
                }
            }
            else if (e.KeyCode == Keys.E)
            {
                TreeOutput.OutputTree(Settings.TreeExportDir, m_meshScene.SimpleMesh);
                MessageBox.Show("Tree has been exported " + Settings.TreeExportDir);
            }
            else if (e.KeyCode == Keys.L)
            {
                m_doLighting = !m_doLighting;
            }
            else if (e.KeyCode == Keys.W)
            {
                m_doWires = !m_doWires;
            }
            else if (e.KeyCode == Keys.B)
            {
                m_invertBG = !m_invertBG;
            }
            else if (e.KeyCode == Keys.G)
            {
                m_meshScene.NoTexturingOnSimpleMeshes = !m_meshScene.NoTexturingOnSimpleMeshes;
                m_meshScene.ChangeMesh(m_meshScene.SimpleMesh.ID);
            }
            else if (e.KeyCode == Keys.Q)
            {
                m_veryQuickMouseZoom = !m_veryQuickMouseZoom;
            }
            else if (e.KeyCode == Keys.S && m_meshScene != null)
            {
                m_meshScene.RenderSubMeshOnly = !m_meshScene.RenderSubMeshOnly;
            }
            else if (e.KeyCode == Keys.C && m_meshScene != null)
            {
                m_meshScene.CycleSubMesh();
            }
            else if (e.KeyCode == Keys.H)
            {
                Help help = new Help();
                help.ShowDialog(this);
            }
        }

        /// <summary>
        /// Form for selecting a simple mesh
        /// </summary>
        private SimpleMeshSelection m_simpleMeshSelection;

        /// <summary>
        /// Form for selecting a complex mesh
        /// </summary>
        private ComplexMeshSelection m_complexMeshSelection;

        /// <summary>
        /// Selection of dungeon blocks
        /// </summary>
        private DungeonBlockSelection m_dungeonBlockSelection;

        /// <summary>
        /// Form for previewing textures
        /// </summary>
        private TexturePreview m_texturePreview;

        private bool m_mouseIsDown = false;

        private SimpleMeshScene m_meshScene;

        /// <summary>
        /// Last mouse position. Used for positioning the camera
        /// </summary>
        private Point m_lastMouseLoc = new Point();

        /// <summary>
        /// device that we're rendering to
        /// </summary>
        private Device m_device;

        /// <summary>
        /// Used to tell whether the device got lost.
        /// </summary>
        private bool m_deviceLost;

        /// <summary>
        /// Our presentation parameters
        /// </summary>
        private PresentParameters m_presentation;

        /// <summary>
        /// Vertex buffer for rendering the axis
        /// </summary>
        private Axis m_axis;

        /// <summary>
        /// The camera class
        /// </summary>
        private Camera m_camera;

        /// <summary>
        /// Font used for drawing debug texts
        /// </summary>
        private Microsoft.DirectX.Direct3D.Font m_debugFont;

        /// <summary>
        /// Used to cut off the render loop
        /// </summary>
        private bool m_mustRun;

        /// <summary>
        /// Whether to do lighting or not.
        /// </summary>
        private bool m_doLighting;

        /// <summary>
        /// Whether to do wireframe or not.
        /// </summary>
        private bool m_doWires;

        /// <summary>
        /// Whether to invert the bg color
        /// </summary>
        private bool m_invertBG;

        /// <summary>
        /// Whether the mouse zoom is accelerated
        /// </summary>
        private bool m_veryQuickMouseZoom;
    }
}