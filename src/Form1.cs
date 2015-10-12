using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
// using DInput=Microsoft.DirectX.DirectInput;
using DPlay = Microsoft.DirectX.DirectPlay;
using DSound = Microsoft.DirectX.DirectSound;

namespace AirplaneWar
{
    public struct CULLINFO
    {
        public Vector3[] vecFrustum;    // corners of the view frustum
        public Plane[] planeFrustum;    // planes of the view frustum
    };

    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        //	private DInput.Device didev;
        private Device d3dDevice;
        private DPlay.Client dpc = null;
        private DPlay.Server dps = null;
        private DSound.Device dsoundDevice = null;
        private Radar map = null;
        public static bool focus = false;
        public static string hoststring = null;
        public static bool terrainFlat = false;
        public static bool smallTexture = false;
        public static bool fog = true;
        private static System.Drawing.Color SkyColor = System.Drawing.Color.FromArgb(54, 79, 159);
        bool connected = false;
        int playerId = 0;

        Landscape landscape;
        airplane aplane;
        MyAirplane myAirplane;
        hangar myhangar;

        CULLINFO m_cullinfo;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }

        protected void InitDirectPlay()
        {
            // create a direct play connection
            dpc = new DPlay.Client();

            DPlay.ApplicationDescription appdesc = new DPlay.ApplicationDescription();
            appdesc.GuidApplication = new Guid("B32DD425-DB33-4f9c-972F-C68269C409F6");

            DPlay.Address dpa = new DPlay.Address(hoststring, 895);
            DPlay.Address dpdi = new DPlay.Address();

            dpdi.ServiceProvider = DPlay.Address.ServiceProviderTcpIp;

            // Set up our event handlers
            dpc.ConnectComplete += new DPlay.ConnectCompleteEventHandler(this.ConnectComplete);

            //		dpc.ConnectComplete += new ConnectCompleteEventHandler(this.ConnectComplete);
            dpc.Receive += new DPlay.ReceiveEventHandler(this.DataReceivedMsg);
            //		dpc.SessionTerminated += new SessionTerminatedEventHandler(this.SessionLost);

            dpc.Connect(appdesc, dpa, dpdi, null, 0);

        }

        protected void StartServer()
        {
            // create a direct play server connection
            dps = new DPlay.Server();
            DPlay.ApplicationDescription appdesc = new DPlay.ApplicationDescription();
            appdesc.GuidApplication = new Guid("B32DD425-DB33-4f9c-972F-C68269C409F6");
            appdesc.MaxPlayers = 0;
            appdesc.SessionName = "AWarServer";
            appdesc.Flags = DPlay.SessionFlags.ClientServer | DPlay.SessionFlags.NoDpnServer;

            DPlay.Address dpa = new DPlay.Address("192.168.1.5", 895);

            // Add our event handlers
            //		dps.PlayerDestroyed += new PlayerDestroyedEventHandler(this.DestroyPlayerMsg);
            //		dps.Receive += new DPlay.ReceiveEventHandler(this.Receive);

            //		dps.Receive+=new DPlay.ReceiveEventHandler(this.Receive);
            //		dps.IndicateConnect+=new DPlay.IndicateConnectEventHandler(this.ConnectComplete);

            dps.Receive += new DPlay.ReceiveEventHandler(this.DataReceivedMsg);

            dps.Host(appdesc, dpa);

        }

        protected void InitDSound()
        {
            dsoundDevice = new DSound.Device();
            dsoundDevice.SetCooperativeLevel(this, DSound.CooperativeLevel.Priority);
        }

        protected bool InitD3D()
        {
            m_cullinfo = new CULLINFO();
            m_cullinfo.vecFrustum = new Vector3[8];
            m_cullinfo.planeFrustum = new Plane[6];
            // direct 3d parameters
            PresentParameters d3dpp = new PresentParameters();
            d3dpp.Windowed = true;
            d3dpp.SwapEffect = SwapEffect.Discard;
            d3dpp.EnableAutoDepthStencil = true;

            if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, Manager.Adapters.Default.CurrentDisplayMode.Format, Usage.DepthStencil, ResourceType.Surface, DepthFormat.D24S8) && Manager.CheckDepthStencilMatch(0, DeviceType.Hardware, Manager.Adapters.Default.CurrentDisplayMode.Format, Manager.Adapters.Default.CurrentDisplayMode.Format, DepthFormat.D24S8))
            {
                d3dpp.AutoDepthStencilFormat = DepthFormat.D24S8;
            }
            else
            {
                MessageBox.Show(this, "Your hardware doesn't support 32 bits Stencil Buffer, try setting the display to 32 bits mode.\n We will use 16 bits format but the game is going to look funny.");
                //	simple=true;
                d3dpp.AutoDepthStencilFormat = DepthFormat.D16;
            }

            // create the direct3d device
            d3dDevice = new Device(0, DeviceType.Hardware, this, CreateFlags.MultiThreaded | CreateFlags.SoftwareVertexProcessing, d3dpp);

            // Setup the event handlers for our device
            //	d3dDevice.DeviceLost += new System.EventHandler(this.InvalidateDeviceObjects);
            d3dDevice.DeviceReset += new System.EventHandler(this.RestoreDeviceObjects);
            //	d3dDevice.Disposing += new System.EventHandler(this.DeleteDeviceObjects);
            //	d3dDevice.DeviceResizing += new System.ComponentModel.CancelEventHandler(this.EnvironmentResized);

            RestoreDeviceObjects(null, null);
            return true;

        }

        protected void RestoreDeviceObjects(System.Object sender, System.EventArgs e)
        {
            // Turn on the zbuffer
            d3dDevice.RenderState.ZBufferEnable = true;

            // prepare light

            d3dDevice.Lights[0].Type = LightType.Directional;
            d3dDevice.Lights[0].Diffuse = Color.FromArgb(0, Color.White);

            d3dDevice.Lights[0].Direction = new Vector3(1, -10, 1);
            d3dDevice.Lights[0].Enabled = true;
            d3dDevice.Lights[0].Commit();

            // Turn on lighting
            d3dDevice.RenderState.Lighting = true;

            // Turn on full ambient light to white
            d3dDevice.RenderState.Ambient = System.Drawing.Color.FromArgb(0x00333333); // Color.White;

            if (fog)
            {
                /* add this if you want fog */
                d3dDevice.RenderState.FogEnable = true;
                d3dDevice.RenderState.FogColor = System.Drawing.Color.WhiteSmoke;
                d3dDevice.RenderState.FogStart = 10f;
                d3dDevice.RenderState.FogEnd = 150000f;
                d3dDevice.RenderState.FogDensity = 0.01f;
                d3dDevice.RenderState.FogVertexMode = FogMode.None;
                d3dDevice.RenderState.FogTableMode = FogMode.Linear;
            }
            // prepare camara
            CamaraLen();
        }


        protected void CamaraLen()
        {
            // The projection matrix describes the camera's lenses
            // For the projection matrix, we set up a perspective transform (which
            // transforms geometry from 3D view space to 2D viewport space, with
            // a perspective divide making objects smaller in the distance). To build
            // a perpsective transform, we need the field of view (1/4 pi is common),
            // the aspect ratio, and the near and far clipping planes (which define at
            // what distances geometry should be no longer be rendered).
            Matrix matProj = Matrix.PerspectiveFovLH((float)3.14159 / 4, 1, 1, 110000);
            d3dDevice.SetTransform(TransformType.Projection, matProj);

            UpdateCullInfo(m_cullinfo, d3dDevice.GetTransform(TransformType.View), d3dDevice.GetTransform(TransformType.Projection));

        }

        //-----------------------------------------------------------------------------
        // Name: UpdateCullInfo()
        // Desc: Sets up the frustum planes, endpoints, and center for the frustum
        //       defined by a given view matrix and projection matrix.  This info will 
        //       be used when culling each object in CullObject().
        //-----------------------------------------------------------------------------
        private void UpdateCullInfo(CULLINFO pCullInfo, Matrix pMatView, Matrix pMatProj)
        {
            Matrix mat = Matrix.Multiply(pMatView, pMatProj);
            mat.Invert();

            pCullInfo.vecFrustum[0] = new Vector3(-1.0f, -1.0f, 0.0f); // xyz
            pCullInfo.vecFrustum[1] = new Vector3(1.0f, -1.0f, 0.0f); // Xyz
            pCullInfo.vecFrustum[2] = new Vector3(-1.0f, 1.0f, 0.0f); // xYz
            pCullInfo.vecFrustum[3] = new Vector3(1.0f, 1.0f, 0.0f); // XYz
            pCullInfo.vecFrustum[4] = new Vector3(-1.0f, -1.0f, 1.0f); // xyZ
            pCullInfo.vecFrustum[5] = new Vector3(1.0f, -1.0f, 1.0f); // XyZ
            pCullInfo.vecFrustum[6] = new Vector3(-1.0f, 1.0f, 1.0f); // xYZ
            pCullInfo.vecFrustum[7] = new Vector3(1.0f, 1.0f, 1.0f); // XYZ

            for (int i = 0; i < 8; i++)
                pCullInfo.vecFrustum[i] = Vector3.TransformCoordinate(pCullInfo.vecFrustum[i], mat);

            pCullInfo.planeFrustum[0] = Plane.FromPoints(pCullInfo.vecFrustum[0],
                pCullInfo.vecFrustum[1], pCullInfo.vecFrustum[2]); // Near
            pCullInfo.planeFrustum[1] = Plane.FromPoints(pCullInfo.vecFrustum[6],
                pCullInfo.vecFrustum[7], pCullInfo.vecFrustum[5]); // Far
            pCullInfo.planeFrustum[2] = Plane.FromPoints(pCullInfo.vecFrustum[2],
                pCullInfo.vecFrustum[6], pCullInfo.vecFrustum[4]); // Left
            pCullInfo.planeFrustum[3] = Plane.FromPoints(pCullInfo.vecFrustum[7],
                pCullInfo.vecFrustum[3], pCullInfo.vecFrustum[5]); // Right
            pCullInfo.planeFrustum[4] = Plane.FromPoints(pCullInfo.vecFrustum[2],
                pCullInfo.vecFrustum[3], pCullInfo.vecFrustum[6]); // Top
            pCullInfo.planeFrustum[5] = Plane.FromPoints(pCullInfo.vecFrustum[1],
                pCullInfo.vecFrustum[0], pCullInfo.vecFrustum[4]); // Bottom
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // release keyboard
                //		didev.Unacquire();
                /*		if (dpc!=null) 
                        {
                            dpc.UnRegisterMessageHandler();
                            dpc.Close(0);
                        } */

                /*	if (dps!=null) 
                    {
                        dps.Dispose(true);
                    }  */

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(792, 573);
            this.Name = "Airplane War";
            this.Text = "Airplane War";
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
        }
        #endregion

        private void InitializeGraphics()
        {
            m_cullinfo = new CULLINFO();

            InitD3D();

            InitDSound();

            DplayForm dialog = new DplayForm();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                StartServer();
            }
            else
            {
                InitDirectPlay();
            }
            Cursor.Hide();

            map = new Radar(d3dDevice, this);
            // init objects geometry
            landscape = new Landscape(d3dDevice);
            myAirplane = new MyAirplane(d3dDevice, this.Font, dsoundDevice, landscape);
            aplane = new airplane(d3dDevice);

            myAirplane.m_vPosition.X = 22000.0f;
            myAirplane.m_vPosition.Z = 44000.0f;
            myAirplane.m_vPosition.Y = myAirplane.m_vPosition.Y - landscape.Intersect(myAirplane.m_vPosition, new Vector3(0f, -1f, 0f)) + 1.1f;
            myAirplane.prepos = myAirplane.m_vPosition;

            myhangar = new hangar(d3dDevice);

        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Form1 app = new Form1();
            app.InitializeGraphics();
            app.Show();
            timer measure = new timer();
            measure.Start();
            while (app.Created)
            {
                measure.Stop();
                /* If you can generate more than 50 frames per second (1/50 = 0.02)
                 * your hardware is pretty cool but we don't really need to
                 * consume so much cpu so let it wait a little */
                while (measure.Time() < 0.02d)
                {
                    Application.DoEvents();
                    measure.Stop();
                }
                measure.Start();
                app.Render();
                Application.DoEvents();
                //	Thread.Sleep(40);
            }
            // should clean up 3d and exit
        }


        private void Render()
        {
            // Make sure the UI is responsive
            //	this.Invoke(new DoEventsCallback(this.ProcessMessages));

            // rendering

            // clear the rectangle
            d3dDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, SkyColor, 1.0f, 0);

            // Begin the scene.
            UpdateCullInfo(m_cullinfo, d3dDevice.Transform.View, d3dDevice.Transform.Projection);
            d3dDevice.BeginScene();

            focus = this.Focused;
            myAirplane.height = landscape.Intersect(myAirplane.m_vPosition, new Vector3(0f, -1f, 0f));
            myAirplane.realheight = landscape.RealHeight(myAirplane.m_vPosition);
            myAirplane.Render();

            aplane.Render();
            landscape.Render(m_cullinfo, myAirplane.m_vPosition);
            myhangar.Render();

            map.Render(myAirplane);

            sendMessage();

            // End the scene.
            d3dDevice.EndScene();

            // copy the render to the window
            d3dDevice.Present();

        }

        protected void sendMessage()
        {
            try
            {
                if (dpc != null)
                {
                    if (connected)
                    {
                        DPlay.NetworkPacket stm = new DPlay.NetworkPacket();
                        Quaternion qr = Quaternion.Normalize(myAirplane.rotation);
                        stm.Write(MessageType.SendMessage);
                        stm.Write(myAirplane.m_vPosition.X);
                        stm.Write(myAirplane.m_vPosition.Y);
                        stm.Write(myAirplane.m_vPosition.Z);
                        stm.Write(qr.X);
                        stm.Write(qr.Y);
                        stm.Write(qr.Z);
                        stm.Write(qr.W);
                        dpc.Send(stm, 0, DPlay.SendFlags.NoLoopback | DPlay.SendFlags.NonSequential | DPlay.SendFlags.Coalesce | DPlay.SendFlags.NoComplete);
                    }
                }
                else
                {
                    if (playerId != 0)
                    {
                        DPlay.NetworkPacket stm = new DPlay.NetworkPacket();
                        Quaternion qr = Quaternion.Normalize(myAirplane.rotation);
                        stm.Write(MessageType.SendMessage);
                        stm.Write(myAirplane.m_vPosition.X);
                        stm.Write(myAirplane.m_vPosition.Y);
                        stm.Write(myAirplane.m_vPosition.Z);
                        stm.Write(qr.X);
                        stm.Write(qr.Y);
                        stm.Write(qr.Z);
                        stm.Write(qr.W);
                        dps.SendTo(playerId, stm, 0, DPlay.SendFlags.NoLoopback | DPlay.SendFlags.NonSequential | DPlay.SendFlags.Coalesce | DPlay.SendFlags.NoComplete);
                    }
                }
            }
            catch (DPlay.ConnectionLostException con)
            {
            }
        }

        public enum MessageType
        {
            //Messages
            SendMessage, //Send a message to someone
        }

        void ConnectComplete(object sender, DPlay.ConnectCompleteEventArgs e)
        {
            connected = true;
        }

        void DataReceivedMsg(object sender, DPlay.ReceiveEventArgs e)
        {
            // We've received data, process it

            MessageType msg = (MessageType)e.Message.ReceiveData.Read(typeof(MessageType));
            switch (msg)
            {
                case MessageType.SendMessage:
                    playerId = e.Message.SenderID;
                    float x = (float)e.Message.ReceiveData.Read(typeof(float));
                    float y = (float)e.Message.ReceiveData.Read(typeof(float));
                    float z = (float)e.Message.ReceiveData.Read(typeof(float));
                    float x2 = (float)e.Message.ReceiveData.Read(typeof(float));
                    float y2 = (float)e.Message.ReceiveData.Read(typeof(float));
                    float z2 = (float)e.Message.ReceiveData.Read(typeof(float));
                    float w = (float)e.Message.ReceiveData.Read(typeof(float));
                    aplane.Translate(playerId, x, y, z, x2, y2, z2, w);
                    map.Translate(playerId, x, y, z);
                    break;
            }
            e.Message.ReceiveData.Dispose(); // Don't need the data anymore
        }

        private void Form1_Deactivate(object sender, System.EventArgs e)
        {
            focus = false;
        }
    }
}
