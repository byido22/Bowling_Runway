using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenGL;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Media;
using System.Threading.Tasks; 



namespace Bowling_Runway
{
    public partial class Form1 : Form
    {
        int ri = 1;


        private float rotationX = 0f;
        private float rotationY = 0f;
        private float rotationZ = 0f;
        private float cameraDistance = 50.0f; // ערך ברירת מחדל
        public GLUquadric quadric;  // משתנה גלובלי
        public bool isPainting = false;  // 🔄 משתנה שמונע קריאות חוזרות
        private uint textureWood, textureCeiling, textureLane, textureDarkWall, textureBall, texturePin, texturePin1, textureAluminum; // משתנה לשמירת טקסטורת הפינים
        private uint textureWall, textureWallBlue, textureFrontdown,textureFloor;
        public Ball bowlingBall;
        public List<Pin> pins;
        private List<Pin> leftLanePins = new List<Pin>();
        private List<Pin> rightLanePins = new List<Pin>();
        private Dictionary<string, float[]> shadowMatrices = new Dictionary<string, float[]>();
        public float[] lightPos = { 0.0f, 10.0f,-15.0f, 0.0f };  // מקור אור קרוב יותר למסלול, כמו שהסברתי
        public BufferedPanel panel1;
        private bool showArrow = true; // האם להציג את החץ?
        private float arrowDirection = 0.0f; // זווית החץ
        private float arrowLength = 5.0f; // אורך החץ

        float[] lightPinsPos = { 0.0f, 8.0f, -52.0f, 0.0f }; // תאורה רכה מעל הפינים
        float[] lightPinsDiffuse = { 0.5f, 0.5f, 0.5f, 0.5f };
        float[] lightPinsSpecular = { 2.0f, 2.0f, 2.0f, 0.0f }; // משפר את ההברקה
        uint m_uint_HWND = 0;

        public uint HWND
        {
            get { return m_uint_HWND; }
        }

        uint hdc = 0;

        public uint DC
        {
            get { return hdc; }
        }
        uint hglrc = 0;

        public uint RC
        {
            get { return hglrc; }
        }

        public Form1()
        {
            InitializeComponent();  // הגדרת כל הרכיבים
            InitializeOpenGL();                                            // הפעלת OpenGL אחרי שכל הרכיבים מוכנים
            CalculateShadowMatrices(); // חובה לקרוא לפונקציה כדי למנוע KeyNotFoundException
        }

        ~Form1()
        {
            GLU.gluDeleteQuadric(quadric); //!!!
            WGL.wglDeleteContext(hglrc);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            panel1.Paint += Panel1_Paint;
            this.DoubleBuffered = true;

            trackBarX.TabStop = false;
            trackBarY.TabStop = false;

            trackBarX.Minimum = -45;
            trackBarX.Maximum = 45;
            trackBarX.Value = 0;
            trackBarX.Scroll += TrackBarX_Scroll;

            trackBarY.Minimum = -180;
            trackBarY.Maximum = 180;
            trackBarY.Value = 0;
            trackBarY.Scroll += TrackBarY_Scroll;

            trackBarZ.Minimum = -180;
            trackBarZ.Maximum = 180;
            trackBarZ.Value = 70;
            trackBarZ.Scroll += TrackBarZ_Scroll;

            InitializeGame();
            bowlingBall.InitializeBallTimer();

            //Console.WriteLine("✅ טיימר הכדור התחיל לפעול!");
            this.Focus();
        }


        ////////////////////////
        /// ////////////////////
        /// ///Initialize Game//
        /// ////////////////////
        /// ////////////////////
        private void RestartGame(object sender, EventArgs e)
        {
            Console.WriteLine("🔄 Restarting game...");

            CreateBall();
            CreatePins();
            cameraDistance = 50.0f; // Ensure this is the default starting distance
            rotationX = 0;
            rotationY = 0;
            rotationZ = 0;
            SetCamera();
            GLU.gluDeleteQuadric(quadric);

            // Loading new game
            bowlingBall.soundPlayed = false;
            InitializeGame();
            showArrow = true;
            SetupLights();
            // רענון המסך
            panel1.Invalidate();
        }
        private void InitializeOpenGL()
        {
            
            hdc = WGL.GetDC((uint)panel1.Handle.ToInt64());
            WGL.PIXELFORMATDESCRIPTOR pfd = new WGL.PIXELFORMATDESCRIPTOR();
            WGL.ZeroPixelDescriptor(ref pfd);
            pfd.nSize = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(pfd);
            pfd.nVersion = 1;
            pfd.dwFlags = WGL.PFD_DRAW_TO_WINDOW | WGL.PFD_SUPPORT_OPENGL | WGL.PFD_DOUBLEBUFFER;
            pfd.iPixelType = (byte)WGL.PFD_TYPE_RGBA;
            pfd.cColorBits = 32;
            pfd.cStencilBits = 32;

            int pixelFormat = WGL.ChoosePixelFormat(hdc, ref pfd);
            WGL.SetPixelFormat(hdc, pixelFormat, ref pfd);

            uint hglrc = WGL.wglCreateContext(hdc);
            WGL.wglMakeCurrent(hdc, hglrc);

            GL.glEnable(GL.GL_DEPTH_TEST);
            GL.glDepthFunc(0x0203);
            //ResetLighting();


            LoadAllTextures();
            ResizeOpenGL();

        }
        private void ResizeOpenGL()
        {
            GL.glViewport(0, 0, panel1.Width, panel1.Height);
            GL.glMatrixMode(GL.GL_PROJECTION);
            //GL.glLoadIdentity();
            GLU.gluPerspective(60.0, (double)panel1.Width / (double)panel1.Height, 0.1, 150.0);
            GL.glMatrixMode(GL.GL_MODELVIEW);
            GL.glLoadIdentity();
        
       }
        private void InitializeGame()
        {
            quadric = GLU.gluNewQuadric();  // ✅ יצירה פעם אחת בלבד

            GLU.gluQuadricTexture(quadric, 1);
            GLU.gluQuadricNormals(quadric, GLU.GLU_SMOOTH);
            Console.WriteLine("IM HERE");
            SetupLights();

            CreateBall();
            CreatePins();
            //CreateStaticPins();
            GeneratePinDisplayLists();
            CalculateShadowMatrices();
            InitializeShadowMatrices();
        }
        private void GeneratePinDisplayLists()
        {
            foreach (var pin in pins) // מסלול ראשי
            {
                pin.GenerateDisplayLists();
            }
        }
        private void CreateBall()
        {
            if (bowlingBall == null)
            {
                bowlingBall = new Ball(this);
            }
            bowlingBall.Position = new Vector3(0, -3.8f, 35.0f);
            bowlingBall.Velocity = Vector3.Zero;
            bowlingBall.ballRotation = 0;
            bowlingBall.ballSpeed = 0;
        }
        private void CreatePins()
        {
            pins = new List<Pin>();
            float startX = 0.0f;
            float startZ = -45.9f;
            float spacing = 1.8f;
            float pinY = -4.7f;

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col <= row; col++)
                {
                    float x = startX + col * spacing - (row * spacing / 2);
                    float z = startZ - row * spacing;
                    Pin pin = new Pin(new Vector3(x, pinY, z), quadric, texturePin, texturePin1);
                    pins.Add(pin);
                }
            }
        }
        ////////////////////////
        /// ////////////////////
        /// End Game Initialize/
        /// ////////////////////
        /// ////////////////////


        ////////////////////////
        /// ////////////////////
        /// /////Lights////////
        /// ///////////////////
        /// ////////////////////
        private void SetupLights()
        {
            GL.glEnable(GL.GL_LIGHTING);
            GL.glEnable(GL.GL_NORMALIZE);
            GL.glLightModeli(GL.GL_LIGHT_MODEL_TWO_SIDE, (int)GL.GL_TRUE);  // הפעלת תאורה דו-צדדית
            float[] localViewer = { 0.0f };  // Use float instead of int
            GL.glLightModelfv(GL.GL_LIGHT_MODEL_LOCAL_VIEWER, localViewer);
            GL.glNormal3f(0, -1, 0);


            // ✅ תאורה סביבתית כללית
            float[] ambientLight = { 0.3f, 0.3f, 0.3f, 1.0f };
            GL.glLightModelfv(GL.GL_LIGHT_MODEL_AMBIENT, ambientLight);

            // 🔆 **שני אורות ראשיים**
            float[] light0Pos = { 0.0f, 20.0f, 10.0f, 1.0f };  // מסלול
            float[] light1Pos = { 0.0f, 20.0f, -40.0f, 1.0f }; // מעל הפינים
            float[] light2Pos = { 0.0f, 20.0f, 40.0f, 1.0f };

            float[] lightDiffuseMain = { 1.5f, 1.5f, 1.5f, 1.0f };
            float[] lightSpecularMain = { 1.0f, 1.0f, 1.0f, 1.0f };

            GL.glEnable(GL.GL_LIGHT0);
            GL.glLightfv(GL.GL_LIGHT0, GL.GL_POSITION, light0Pos);
            GL.glLightfv(GL.GL_LIGHT0, GL.GL_DIFFUSE, lightDiffuseMain);
            GL.glLightfv(GL.GL_LIGHT0, GL.GL_SPECULAR, lightSpecularMain);

            GL.glEnable(GL.GL_LIGHT0);
            GL.glLightfv(GL.GL_LIGHT2, GL.GL_POSITION, light2Pos);
            GL.glLightfv(GL.GL_LIGHT2, GL.GL_DIFFUSE, lightDiffuseMain);
            GL.glLightfv(GL.GL_LIGHT2, GL.GL_SPECULAR, lightSpecularMain);

            GL.glEnable(GL.GL_LIGHT1);
            GL.glLightfv(GL.GL_LIGHT1, GL.GL_POSITION, light1Pos);
            GL.glLightfv(GL.GL_LIGHT1, GL.GL_DIFFUSE, lightDiffuseMain);
            GL.glLightfv(GL.GL_LIGHT1, GL.GL_SPECULAR, lightSpecularMain);
            UpdateShadowMatrix();

        }
        private void ResetLighting()
        {
            float[] zeroColor = { 0.0f, 0.0f, 0.0f, 1.0f };

            // ✅ איפוס התאורה הגלובלית
            GL.glLightModelfv(GL.GL_LIGHT_MODEL_AMBIENT, zeroColor);

            // ✅ איפוס כל מקורות האור
            for (int i = 0; i < 3; i++) // ב-OpenGL יש עד 8 מקורות אור
            {
                GL.glLightfv(GL.GL_LIGHT0 + (uint)i, GL.GL_AMBIENT, zeroColor);
                GL.glLightfv(GL.GL_LIGHT0 + (uint)i, GL.GL_DIFFUSE, zeroColor);
                GL.glLightfv(GL.GL_LIGHT0 + (uint)i, GL.GL_SPECULAR, zeroColor);
                GL.glDisable(GL.GL_LIGHT0 + (uint)i);
            }

            // ✅ כיבוי מערכת התאורה
            GL.glDisable(GL.GL_LIGHTING);

            Console.WriteLine("✅ All lighting and materials have been reset.");
        }
        private void AlleyLight_CheckedChanged(object sender, EventArgs e)
        {
            if (alleyLight.Checked)
            {
                SetupLights(); // ✅ מפעיל את התאורה הכללית של המסלול
                Console.WriteLine("🏟️ Alley lighting enabled.");
            }
            else
            {
                ResetLighting(); // ✅ מכבה את התאורה הכללית
                Console.WriteLine("❌ Alley lighting disabled.");
            }

            panel1.Invalidate(); // ריענון הציור
            //GL.glFlush();
        }
        private void LightSwitch_CheckedChanged(object sender, EventArgs e)
        {
            if (Lightswitch.Checked)
            {
                Console.WriteLine("🔆 Lights Enabled");
                SetupLights();  // הפעלת תאורה
            }
            else
            {
                Console.WriteLine("🌑 Lights Disabled");
                ResetLighting(); // כיבוי תאורה
                GL.glDisable(GL.GL_LIGHTING);
            }

            panel1.Invalidate(); // רענון הציור לאחר שינוי התאורה
            //GL.glFlush();

        }
        private void PinsLight_CheckedChanged(object sender, EventArgs e)
        {
            GL.glLightModeli(GL.GL_LIGHT_MODEL_TWO_SIDE, (int)GL.GL_TRUE);  // הפעלת תאורה דו-צדדית

            if (pinsLight.Checked)
            {
                GL.glEnable(GL.GL_LIGHT2);
                GL.glLightfv(GL.GL_LIGHT2, GL.GL_POSITION, lightPinsPos);
                GL.glLightfv(GL.GL_LIGHT2, GL.GL_DIFFUSE, lightPinsDiffuse);
                GL.glLightfv(GL.GL_LIGHT2, GL.GL_SPECULAR, lightPinsSpecular);

                Console.WriteLine("🎳 Pins lighting enabled.");
            }
            else
            {
                GL.glDisable(GL.GL_LIGHT2);
                Console.WriteLine("❌ Pins lighting disabled.");
            }
            UpdateShadowMatrix();

            panel1.Invalidate(); // ריענון הציור
            GL.glFlush();
        }
        ////////////////////////
        /// ////////////////////
        /// ///End Lights///////
        /// ////////////////////
        /// ////////////////////

     
        ////////////////////////
        /// ////////////////////
        /// חישוב מטריצת הצל//
        /// ///////////////////
        /// ////////////////////
        private void UpdateShadowMatrix()
        {
            float[] shadowMatrix = new float[16];

            // חישוב מחדש לפי המיקום הנוכחי של מקור האור
            float[] floorPlane = { 0.0f, 20.0f, 30.0f, 0.0f };
            CalculateShadowMatrix(shadowMatrix, floorPlane, lightPos);

            // העמסה למודל ההקרנה של OpenGL
            GL.glMatrixMode(GL.GL_MODELVIEW);
            GL.glPushMatrix();
            GL.glMultMatrixf(shadowMatrix);
            GL.glPopMatrix();
        }
        private void InitializeShadowMatrices()
        {
            if (shadowMatrices.ContainsKey("floor"))
            {
                foreach (var pin in pins)
                {
                    pin.SetShadowMatrix(shadowMatrices["floor"]);
                }

                bowlingBall.SetShadowMatrix(shadowMatrices["floor"]);
                Console.WriteLine("✅ Shadow matrices set to all pins and ball.");
            }
            else
            {
                Console.WriteLine("❌ Error: Missing floor shadow matrix, cannot set shadows!");
            }
        }
        private void CalculateShadowMatrices()
        {

            shadowMatrices["floor"] = new float[16];
            shadowMatrices["wallBack"] = new float[16];
            shadowMatrices["wallLeft"] = new float[16];
            shadowMatrices["wallRight"] = new float[16];

            Console.WriteLine("🔆 Calculating shadow matrices with light at: " +
                              $"X={lightPos[0]}, Y={lightPos[1]}, Z={lightPos[2]}");

             // חישוב צל לרצפה
            float[] floorPlane = { 0.0f, 1.0f, 0.0f, 0.0f };  // תואם לגובה המסלול
            CalculateShadowMatrix(shadowMatrices["floor"], floorPlane, lightPos);            
            // צל לקיר האחורי
            float[] backWallPlane = { 0.0f, 0.0f, 1.0f, -55.0f };
            CalculateShadowMatrix(shadowMatrices["wallBack"], backWallPlane, lightPos);            
            // צל לקיר שמאל
            float[] leftWallPlane = { -1.0f, 0.0f, 0.0f, 25.0f };
            CalculateShadowMatrix(shadowMatrices["wallLeft"], leftWallPlane, lightPos);            
            // צל לקיר ימין
            float[] rightWallPlane = { 1.0f, 0.0f, 0.0f, -25.0f };
            CalculateShadowMatrix(shadowMatrices["wallRight"], rightWallPlane, lightPos);
        }
        private void CalculateShadowMatrix(float[] shadowMatrix, float[] plane, float[] lightPos)
        {
            float dot = plane[0] * lightPos[0] +
                        plane[1] * lightPos[1] +
                        plane[2] * lightPos[2] +
                        plane[3] * lightPos[3];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i == j)
                        shadowMatrix[i * 4 + j] = dot - lightPos[j] * plane[i];
                    else
                        shadowMatrix[i * 4 + j] = lightPos[j] * plane[i];
                }
            }

            // 📏 קנה מידה לתיקון פרופורציות הצל
            shadowMatrix[0] *= 0.85f;  // ציר X
            shadowMatrix[10] *= 0.85f; // ציר Z

            Console.WriteLine($"📐 Calculated shadow matrix for plane at Y={plane[3]}");
        }
        ///////////////////////////
        /// ///////////////////////
        /// סוף חישוב המטריצות///
        /// //////////////////////
        //////////////////////////


        ///////////////////////
        /// ///////////////////
        /// /////Draw//////////
        /// ///////////////////
        /// ///////////////////
        private void DrawSingleLane(float offsetX, float laneY, float laneZ, bool isReflection, bool useTexture)
        {
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureLane);
            GL.glPushMatrix();
            GL.glTranslatef(offsetX, laneY, laneZ);
            //GL.glEnable(GL.GL_LIGHTING);
            GL.glBegin(GL.GL_QUADS);
            GL.glNormal3f(0, 1, 0); // נורמל כלפי מעלה
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(5.0f, 0.01f, -47.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(-5.0f, 0.01f, -47.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(-5.0f, 0.01f, 47.5f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(5.0f, 0.01f, 47.5f);
            GL.glEnd();

            GL.glPopMatrix();

            GL.glDisable(GL.GL_TEXTURE_2D);


            //Console.WriteLine("DrawSingleLane: Finished drawing lane.");
        }
        private void DrawRoom()
        {
            float[] matDiffuse = { 1.0f, 1.0f, 1.0f, 1.0f };
            float laneWidth = 10.0f; // רוחב מסלול
            float laneSpacing = 4.0f; // רווח בין המסלולים
            float laneY = -4.8f;
            float laneZ = -7.3f;
            GL.glDisable(GL.GL_CULL_FACE);
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureWall);
            GL.glEnable(GL.GL_DEPTH_TEST);  // לוודא שבודקים עומק בזמן ציור החדר
            GL.glDepthMask((byte)GL.GL_TRUE);     // לאפשר כתיבה ל-Depth Buffer

            GL.glBegin(GL.GL_QUADS);

            // 🔷 קיר קדמי (פונה אחורה)
            GL.glNormal3f(1.0f, 0.0f, 0.0f);
            GL.glTexCoord2f(0, 0); GL.glVertex3f(-25, -5, 55);
            GL.glTexCoord2f(3, 0); GL.glVertex3f(25, -5, 55);
            GL.glTexCoord2f(3, 3); GL.glVertex3f(25, 25, 55);
            GL.glTexCoord2f(0, 3); GL.glVertex3f(-25, 25, 55);

            // 🔷 קיר שמאל (פונה ימינה)
            GL.glNormal3f(1.0f, 0.0f, 0.0f);
            GL.glTexCoord2f(0, 0); GL.glVertex3f(-25, -5, 55);
            GL.glTexCoord2f(3, 0); GL.glVertex3f(-25, -5, -50.1f);
            GL.glTexCoord2f(3, 3); GL.glVertex3f(-25, 25, -50.1f);
            GL.glTexCoord2f(0, 3); GL.glVertex3f(-25, 25, 55);

            // 🔷 קיר ימין (פונה שמאלה)
            GL.glNormal3f(0.0f, 0.0f, 1.0f);
            GL.glTexCoord2f(0, 0); GL.glVertex3f(25, -5, 55);
            GL.glTexCoord2f(3, 0); GL.glVertex3f(25, -5, -50.1f);
            GL.glTexCoord2f(3, 3); GL.glVertex3f(25, 25, -50.1f);
            GL.glTexCoord2f(0, 3); GL.glVertex3f(25, 25, 55);

            GL.glEnd();

            // 🔷 יצירת חור בקיר האחורי
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureWallBlue);
            GL.glBegin(GL.GL_QUADS);

            // חלק עליון (מעל הפתח)
            GL.glNormal3f(0.0f, 0.0f, -1.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(-25.0f, 0.8f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(25.0f, 0.8f, -50.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(25.0f, 25.0f, -50.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-25.0f, 25.0f, -50.0f);
            GL.glEnd();


            // גליל תחתון לאורך הקיר האחורי
            GL.glPushMatrix();
            GL.glTranslatef(-25.0f, 0.8f, -50.0f);
            GL.glRotatef(90.0f, 0.0f, 1.0f, 0.0f);  // סיבוב סביב ציר ה-Y
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureWood);
            GLU.gluQuadricTexture(quadric, 1);
            //GLU.gluQuadricNormals(quadric, GLU.GLU_SMOOTH);  // אם תרצה תאורה טובה
            GLU.gluCylinder(quadric, 0.05f, 0.05f, 50.0f, 32, 32);

            // כיבוי טקסטורה אחרי השימוש (למנוע זליגה לפרימיטיבים אחרים)
            GL.glDisable(GL.GL_TEXTURE_2D);
            GL.glPopMatrix();


            // חלק תחתון קיר קדמי
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureWood);
            GL.glBegin(GL.GL_QUADS);
            GL.glNormal3f(0.0f, 0.0f, -1.0f);

            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(-25.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(-20.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(-20.0f, 0.8f, -50.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-25.0f, 0.8f, -50.0f);

            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(20.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(25.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(25.0f, 0.8f, -50.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(20.0f, 0.8f, -50.0f);

            GL.glTexCoord2f(0.0f, 0.5f); GL.glVertex3f(-8.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(0.5f, 0.5f); GL.glVertex3f(-6.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(0.5f, 0.0f); GL.glVertex3f(-6.0f, 0.8f, -50.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-8.0f, 0.8f, -50.0f);

            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(6.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(8.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(8.0f, 0.8f, -50.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(6.0f, 0.8f, -50.0f);
            GL.glEnd();


            // 🔷 תא הפינים
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureDarkWall);
            GL.glBegin(GL.GL_QUADS);

            // קיר אחורי פנימי
            GL.glNormal3f(0.0f, 0.0f, -1.0f);
            GL.glTexCoord2f(0, 0); GL.glVertex3f(-25.0f, -5.0f, -55.0f);
            GL.glTexCoord2f(1, 0); GL.glVertex3f(25.0f, -5.0f, -55.0f);
            GL.glTexCoord2f(1, 1); GL.glVertex3f(25.0f, 10.0f, -55.0f);
            GL.glTexCoord2f(0, 1); GL.glVertex3f(-25.0f, 10.0f, -55.0f);

            // 🔷 קיר פנימי שמאל
            GL.glNormal3f(1.0f, 0.0f, 0.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(-6.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(-6.0f, -5.0f, -55.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(-6.0f, 10.0f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-6.0f, 10.0f, -50.0f);

            GL.glNormal3f(1.0f, 0.0f, 0.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(-20.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(-20.0f, -5.0f, -55.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(-20.0f, 10.0f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-20.0f, 10.0f, -50.0f);

            GL.glNormal3f(1.0f, 0.0f, 0.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(8.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(8.0f, -5.0f, -55.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(8.0f, 10.0f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(8.0f, 10.0f, -50.0f);


            // 🔷 קיר פנימי ימין
            GL.glNormal3f(0.0f, 0.0f, -1.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(6.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(6.0f, -5.0f, -55.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(6.0f, 10.0f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(6.0f, 10.0f, -50.0f);

            GL.glNormal3f(0.0f, 0.0f, -1.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(-8.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(-8.0f, -5.0f, -55.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(-8.0f, 10.0f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-8.0f, 10.0f, -50.0f);

            GL.glNormal3f(0.0f, 0.0f, -1.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(20.0f, -5.0f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(20.0f, -5.0f, -55.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(20.0f, 10.0f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(20.0f, 10.0f, -50.0f);

            // 🎯 תקרה פנימית
            GL.glNormal3f(0.0f, -1.0f, 0.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(-25.0f, 0.8f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(25.0f, 0.8f, -50.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(25.0f, 0.8f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-25.0f, 0.8f, -55.0f);
            GL.glEnd();

            // 🎯 תקרה
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureCeiling);
            GL.glBegin(GL.GL_QUADS);
            GL.glNormal3f(0.0f, 1.0f, 0.0f);
            GL.glTexCoord2f(0, 0); GL.glVertex3f(-25, 25, 55);
            GL.glTexCoord2f(1, 0); GL.glVertex3f(25, 25, 55);
            GL.glTexCoord2f(1, 1); GL.glVertex3f(25, 25, -50);
            GL.glTexCoord2f(0, 1); GL.glVertex3f(-25, 25, -50);
            GL.glEnd();

            // 🎯 רצפה
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureFloor);
            GL.glBegin(GL.GL_QUADS);
            GL.glNormal3f(0.0f, 1.0f, 0.0f);

            GL.glTexCoord2f(0, 0); GL.glVertex3f(-25, -4.85f, 55);
            GL.glTexCoord2f(200, 0); GL.glVertex3f(-20.4f, -4.85f, 55);
            GL.glTexCoord2f(200, 200); GL.glVertex3f(-20.4f, -4.85f, -55);
            GL.glTexCoord2f(0, 200); GL.glVertex3f(-25, -4.85f, -55);

            GL.glTexCoord2f(0, 0); GL.glVertex3f(-7.6f, -4.85f, 55);
            GL.glTexCoord2f(200, 0); GL.glVertex3f(-6.4f, -4.85f, 55);
            GL.glTexCoord2f(200, 200); GL.glVertex3f(-6.4f, -4.85f, -55);
            GL.glTexCoord2f(0, 200); GL.glVertex3f(-7.6f, -4.85f, -55);

            GL.glTexCoord2f(0, 0); GL.glVertex3f(6.4f, -4.85f, 55);
            GL.glTexCoord2f(200, 0); GL.glVertex3f(7.6f, -4.85f, 55);
            GL.glTexCoord2f(200, 200); GL.glVertex3f(7.6f, -4.85f, -55);
            GL.glTexCoord2f(0, 200); GL.glVertex3f(6.4f, -4.85f, -55);

            GL.glTexCoord2f(0, 0); GL.glVertex3f(20.4f, -4.85f, 55);
            GL.glTexCoord2f(200, 0); GL.glVertex3f(25.0f, -4.85f, 55);
            GL.glTexCoord2f(200, 200); GL.glVertex3f(25.0f, -4.85f, -55);
            GL.glTexCoord2f(0, 200); GL.glVertex3f(20.4f, -4.85f, -55);

            GL.glTexCoord2f(0, 0); GL.glVertex3f(-25.0f, -4.85f, 55);
            GL.glTexCoord2f(200, 0); GL.glVertex3f(25.0f, -4.85f, 55);
            GL.glTexCoord2f(200, 200); GL.glVertex3f(25.0f, -4.85f, 40);
            GL.glTexCoord2f(0, 200); GL.glVertex3f(-25.0f, -4.85f, 40);
            GL.glEnd();
            DrawSingleLane(-14.0f, laneY, laneZ, false, true);
            DrawSingleLane(14.0f, laneY, laneZ, false, true);

            GL.glEnable(GL.GL_CULL_FACE);
            GL.glDisable(GL.GL_TEXTURE_2D);
        }
        private void DrawLaneDecorations()
        {
            float laneY = -4.9f;
            float laneZ = -10.0f;

            // Existing barriers
            DrawLaneBarriers(laneY, laneZ);

            // Existing half-pipes
            DrawTexturedHalfPipe(-5.7f, -4.85f, -60.0f, 100.0f, 0.7f, textureAluminum);  // Left side
            DrawTexturedHalfPipe(5.7f, -4.8f, -60.0f, 100.0f, 0.7f, textureAluminum);   // Right side

            DrawTexturedHalfPipe(-19.7f, -4.85f, -60.0f, 100.0f, 0.7f, textureAluminum); // Extra side
            DrawTexturedHalfPipe(-8.3f, -4.8f, -60.0f, 100.0f, 0.7f, textureAluminum);  // Extra side
            DrawTexturedHalfPipe(8.3f, -4.85f, -60.0f, 100.0f, 0.7f, textureAluminum);  // Extra side
            DrawTexturedHalfPipe(19.7f, -4.8f, -60.0f, 100.0f, 0.7f, textureAluminum);  // Extra side
        }
        private void DrawPinSetter(float offsetX)
        {
            GL.glPushMatrix();
            GL.glTranslatef(offsetX, 0.0f, 0.0f);  // הזזת המתקן לפי המסלול
            GL.glDisable(GL.GL_CULL_FACE);

            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureAluminum);
            GL.glBegin(GL.GL_QUADS);

            // חלקים מרובעים - גוף המתקן (החזית)
            GL.glNormal3f(0.0f, 0.0f, 1.0f);
            GL.glTexCoord2f(0.0f, 0.5f); GL.glVertex3f(-5.0f, -0.7f, -50.0f);
            GL.glTexCoord2f(0.5f, 0.5f); GL.glVertex3f(5.0f, -0.7f, -50.0f);
            GL.glTexCoord2f(0.5f, 0.0f); GL.glVertex3f(5.0f, -0.3f, -50.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-5.0f, -0.3f, -50.0f);

            GL.glTexCoord2f(0.0f, 0.5f); GL.glVertex3f(-5.0f, -0.1f, -50.0f);
            GL.glTexCoord2f(0.5f, 0.5f); GL.glVertex3f(5.0f, -0.1f, -50.0f);
            GL.glTexCoord2f(0.5f, 0.0f); GL.glVertex3f(5.0f, 0.3f, -50.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-5.0f, 0.3f, -50.0f);

            // דפנות הצד - ימין ושמאל
            GL.glNormal3f(1.0f, 0.0f, 0.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(-5.0f, -0.7f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(-5.0f, -0.7f, -55.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(-5.0f, 0.3f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(-5.0f, 0.3f, -50.0f);

            GL.glNormal3f(0.0f, 0.0f, -1.0f);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex3f(5.0f, -0.7f, -50.0f);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex3f(5.0f, -0.7f, -55.0f);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex3f(5.0f, 0.3f, -55.0f);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex3f(5.0f, 0.3f, -50.0f);

            GL.glEnd();

            // גלילים אנכיים בפינות הקדמיות
            GL.glPushMatrix();
            GL.glTranslatef(5.0f, 0.3f, -50.0f);
            GL.glRotatef(90, 1, 0, 0);
            GLU.gluCylinder(quadric, 0.05f, 0.05f, 1.0f, 32, 32);
            GL.glPopMatrix();

            GL.glPushMatrix();
            GL.glTranslatef(-5.0f, 0.3f, -50.0f);
            GL.glRotatef(90, 1, 0, 0);
            GLU.gluCylinder(quadric, 0.05f, 0.05f, 1.0f, 32, 32);
            GL.glPopMatrix();

            // גלילים אופקיים לאורך המתקן
            float[] yPositions = { 0.3f, -0.1f, -0.3f, -0.7f };
            foreach (float y in yPositions)
            {
                GL.glPushMatrix();
                GL.glTranslatef(-5.0f, y, -50.0f);
                GL.glRotatef(90, 0, 1, 0);
                GLU.gluCylinder(quadric, 0.05f, 0.05f, 10.0f, 32, 32);
                GL.glPopMatrix();
            }

            // גלילים אנכיים אחוריים בצד ימין
            GL.glPushMatrix();
            GL.glTranslatef(5.0f, 0.3f, -55.0f);
            GLU.gluCylinder(quadric, 0.05f, 0.05f, 5.0f, 32, 32);
            GL.glPopMatrix();

            GL.glPushMatrix();
            GL.glTranslatef(5.0f, -0.7f, -55.0f);
            GLU.gluCylinder(quadric, 0.05f, 0.05f, 5.0f, 32, 32);
            GL.glPopMatrix();

            // גלילים אנכיים אחוריים בצד שמאל
            GL.glPushMatrix();
            GL.glTranslatef(-5.0f, 0.3f, -55.0f);
            GLU.gluCylinder(quadric, 0.05f, 0.05f, 5.0f, 32, 32);
            GL.glPopMatrix();

            GL.glPushMatrix();
            GL.glTranslatef(-5.0f, -0.7f, -55.0f);
            GLU.gluCylinder(quadric, 0.05f, 0.05f, 5.0f, 32, 32);
            GL.glPopMatrix();

            GL.glPopMatrix();  // סיום כל ה-translate
            GL.glEnable(GL.GL_CULL_FACE);

        }
        private void DrawTexturedHalfPipe(float x, float y, float z, float length, float radius, uint textureID)
        {
            GLU.gluQuadricNormals(quadric, GLU.GLU_SMOOTH);
            GLU.gluQuadricTexture(quadric, 1);


            // קביעת צבע הגדר
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureAluminum);

            GL.glPushMatrix();
            GL.glTranslatef(x + 0.7f, y, z);
            GLU.gluCylinder(quadric, 0.08f, 0.08f, 100, 16, 16);
            GL.glPopMatrix();

            GL.glPushMatrix();
            GL.glTranslatef(x - 0.7f, y, z);
            GLU.gluCylinder(quadric, 0.08f, 0.08f, 100, 16, 16);
            GL.glPopMatrix();

            GL.glPushMatrix();
            GL.glTranslatef(x, y, z);
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureID);

            // מאפשר לראות מבפנים וגם מבחוץ
            GL.glDisable(GL.GL_CULL_FACE);


            int segments = 32;
            float angleStep = (float)(Math.PI / (segments - 1));

            GL.glBegin(GL.GL_QUAD_STRIP);

            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)Math.PI + i * angleStep;
                float xOffset = (float)Math.Cos(angle) * radius;
                float yOffset = (float)Math.Sin(angle) * radius;

                // חישוב נורמל כלפי פנים כדי שהטקסטורה תהיה על הקיר הפנימי
                Vector3 normal = Vector3.Normalize(new Vector3(xOffset, yOffset, 0));
                GL.glNormal3f(0.0f, 1.0f, 0.0f);

                float texCoord = i / (float)segments;
                GL.glTexCoord2f(texCoord, 0);
                GL.glVertex3f(xOffset, yOffset, 0);

                GL.glTexCoord2f(texCoord, 1);
                GL.glVertex3f(xOffset, yOffset, length);
            }

            GL.glEnd();

            GL.glEnable(GL.GL_CULL_FACE);
            GL.glDisable(GL.GL_TEXTURE_2D);
            GL.glPopMatrix();
        }
        private void DrawLaneBarriers(float laneY, float laneZ)
        {
            float barrierHeight = 1.0f;   // גובה כל עמוד
            float barrierRadius = 0.05f;  // עובי הגדר
            int segmentCount = 10;        // מספר עמודים לאורך כל המסלול
            float laneLength = 100.0f;     // אורך המסלול
            float spacing = laneLength / (segmentCount - 1);
            float startZ = 39.5f;

            GLU.gluQuadricNormals(quadric, GLU.GLU_SMOOTH);
            GLU.gluQuadricTexture(quadric, 1);


            // קביעת צבע הגדר
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, textureAluminum);

            GL.glPushMatrix();
            GL.glTranslatef(-20.4f, -4.85f, 40.0f);
            GL.glRotatef(90, 0, 1, 0);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, 40.8f, 16, 16);
            GL.glPopMatrix();

            //  מסלול ראשי - ציור עמודים לאורך צד שמאל
            for (int i = 0; i < segmentCount; i++)
            {
                float z = (startZ - i * spacing);

                GL.glPushMatrix();
                GL.glTranslatef(-5.0f, laneY + 1.0f, z);  // מיקום בצד שמאל
                GL.glRotatef(90, 1, 0, 0);

                GLU.gluCylinder(quadric, barrierRadius, barrierRadius, barrierHeight, 16, 16);
                GL.glPopMatrix();
            }

            // ציור עמודים לאורך צד ימין
            for (int i = 0; i < segmentCount; i++)
            {
                float z = startZ - i * spacing;
                GL.glPushMatrix();
                GL.glTranslatef(5.0f, laneY + 1.0f, z);  // מיקום בצד ימין
                GL.glRotatef(90, 1, 0, 0);

                GLU.gluCylinder(quadric, barrierRadius, barrierRadius, barrierHeight, 16, 16);
                GL.glPopMatrix();
            }
            //
            //מסלול שמאלי - ציור עמודים שמאל
            //
            for (int i = 0; i < segmentCount; i++)
            {
                float z = (startZ - i * spacing);

                GL.glPushMatrix();
                GL.glTranslatef(-19.0f, laneY + 1.0f, z);  // מיקום בצד שמאל
                GL.glRotatef(90, 1, 0, 0);

                GLU.gluCylinder(quadric, barrierRadius, barrierRadius, barrierHeight, 16, 16);
                GL.glPopMatrix();
            }
            //מסלול שמאלי - ציור עמודים ימין
            for (int i = 0; i < segmentCount; i++)
            {
                float z = (startZ - i * spacing);

                GL.glPushMatrix();
                GL.glTranslatef(-9.0f, laneY + 1.0f, z);  // מיקום בצד שמאל
                GL.glRotatef(90, 1, 0, 0);

                GLU.gluCylinder(quadric, barrierRadius, barrierRadius, barrierHeight, 16, 16);
                GL.glPopMatrix();
            }

            //
            //מסלול ימני - ציור עמודים שמאל
            //
            for (int i = 0; i < segmentCount; i++)
            {
                float z = (startZ - i * spacing);

                GL.glPushMatrix();
                GL.glTranslatef(9.0f, laneY + 1.0f, z);  // מיקום בצד שמאל
                GL.glRotatef(90, 1, 0, 0);

                GLU.gluCylinder(quadric, barrierRadius, barrierRadius, barrierHeight, 16, 16);
                GL.glPopMatrix();
            }
            //מסלול שמאלי - ציור עמודים ימין
            for (int i = 0; i < segmentCount; i++)
            {
                float z = (startZ - i * spacing);

                GL.glPushMatrix();
                GL.glTranslatef(19.0f, laneY + 1.0f, z);  // מיקום בצד שמאל
                GL.glRotatef(90, 1, 0, 0);

                GLU.gluCylinder(quadric, barrierRadius, barrierRadius, barrierHeight, 16, 16);
                GL.glPopMatrix();
            }

            //מוט עליון - מסלול שמאלי שמאל
            GL.glPushMatrix();
            GL.glTranslatef(-19.0f, laneY + 0.5f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();
            GL.glPushMatrix();
            GL.glTranslatef(-19.0f, laneY + 1.0f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();

            GL.glPushMatrix();
            GL.glTranslatef(-9.0f, laneY + 0.5f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();
            GL.glPushMatrix();
            GL.glTranslatef(-9.0f, laneY + 1.0f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();

            //מוט עליון - מסלול שמאלי ימין
            GL.glPushMatrix();
            GL.glTranslatef(9.0f, laneY + 0.5f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();
            GL.glPushMatrix();
            GL.glTranslatef(9.0f, laneY + 1.0f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();

            GL.glPushMatrix();
            GL.glTranslatef(19.0f, laneY + 0.5f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();
            GL.glPushMatrix();
            GL.glTranslatef(19.0f, laneY + 1.0f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();

            // ציור מוט עליון לאורך כל צד שמאל
            GL.glPushMatrix();
            GL.glTranslatef(-5.0f, laneY + 0.5f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();

            GL.glPushMatrix();
            GL.glTranslatef(-5.0f, laneY + 1.0f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();

            // ציור מוט עליון לאורך כל צד ימין
            GL.glPushMatrix();
            GL.glTranslatef(5.0f, laneY + 0.5f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();
            GL.glPushMatrix();
            GL.glTranslatef(5.0f, laneY + 1.0f, laneZ - laneLength / 2);
            GLU.gluCylinder(quadric, barrierRadius, barrierRadius, laneLength, 16, 16);
            GL.glPopMatrix();
        }
        private void DrawPinsAndBall(bool drawShadow = true, bool isReflection = false)
        {
            GL.glLightfv(GL.GL_LIGHT0, GL.GL_POSITION, lightPinsPos);

            foreach (var pin in pins)
            {
                pin.Draw(quadric, pin.Position.X, pin.Position.Y, pin.Position.Z, drawShadow, isReflection);
            }
            bowlingBall.Draw(textureBall, bowlingBall.Position.X, bowlingBall.Position.Y, bowlingBall.Position.Z, drawShadow, isReflection);
        }
        private void DrawArrow()
        {
            if (!showArrow) return; // אם לא צריך להציג את החץ – יוצאים מהפונקציה

            GL.glPushMatrix();

            // הצבת החץ בנקודת ההתחלה של הכדור
            GL.glTranslatef(bowlingBall.Position.X, bowlingBall.Position.Y + 1.0f, bowlingBall.Position.Z);

            // סיבוב החץ בהתאם לכיוון שנקבע
            GL.glRotatef(-arrowDirection, 0, 1, 0);

            // ❗ כיבוי התאורה והטקסטורה כדי לשלוט על הצבע
            GL.glDisable(GL.GL_LIGHTING);
            GL.glDisable(GL.GL_TEXTURE_2D);

            GL.glColor3f(0.0f, 0.0f, 5.0f); // קביעת צבע שחור
            GL.glLineWidth(15.0f); // קביעת עובי החץ

            // ציור החץ
            GL.glBegin(GL.GL_LINES);

            // גוף החץ
            GL.glVertex3f(0, 0, 0);
            GL.glVertex3f(0, 0, -arrowLength);

            // ראש החץ
            GL.glVertex3f(0, 0, -arrowLength);
            GL.glVertex3f(0.5f, 0, -arrowLength + 1.5f);
            GL.glVertex3f(0, 0, -arrowLength);
            GL.glVertex3f(-0.5f, 0, -arrowLength + 1.5f);

            GL.glEnd();

            // ❗ הפעלה מחדש של התאורה והטקסטורה
            GL.glEnable(GL.GL_LIGHTING);
            GL.glEnable(GL.GL_TEXTURE_2D);

            GL.glPopMatrix();
        }
        private void DrawReflections()
        {
            float laneY = -4.8f;
            float laneZ = -7.3f;
            float laneWidth = 10.0f;
            float laneSpacing = 4.0f;
            float[] laneOffsetsX = { -laneWidth - laneSpacing, 0.0f, laneWidth + laneSpacing };
            GL.glEnable(GL.GL_BLEND);

            Console.WriteLine("DrawReflections: Starting reflections.");

            foreach (float offsetX in laneOffsetsX)
            {
                GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
                // 1️⃣ הפעלת Stencil Buffer כדי להגדיר את משטח הרצפה
                GL.glEnable(GL.GL_STENCIL_TEST);
                GL.glStencilOp(GL.GL_REPLACE, GL.GL_REPLACE, GL.GL_REPLACE);
                GL.glStencilFunc(GL.GL_ALWAYS, 1, 0xFFFFFFFF);
                GL.glColorMask((byte)GL.GL_FALSE, (byte)GL.GL_FALSE, (byte)GL.GL_FALSE, (byte)GL.GL_FALSE);
                GL.glDisable(GL.GL_DEPTH_TEST);

                DrawSingleLane(offsetX, laneY, laneZ, false, true); // ציור הרצפה (משמש כ-Stencil)

                GL.glColorMask((byte)GL.GL_TRUE, (byte)GL.GL_TRUE, (byte)GL.GL_TRUE, (byte)GL.GL_TRUE);
                GL.glEnable(GL.GL_DEPTH_TEST);

                GL.glStencilFunc(GL.GL_EQUAL, 1, 0xFFFFFFFF);
                GL.glStencilOp(GL.GL_KEEP, GL.GL_KEEP, GL.GL_KEEP);

                GL.glEnable(GL.GL_STENCIL_TEST);

                // 2️⃣ ציור ההשתקפות בתוך גבולות הרצפה
                GL.glPushMatrix();
                GL.glTranslatef(0, 2 * laneY, 0);  // מזיז את ההשתקפות כדי שתתאים לרצפה
                GL.glScalef(1, -1, 1);
                GL.glEnable(GL.GL_NORMALIZE);
                // הופך את ציר ה-Z ליצירת השתקפות
                GL.glEnable(GL.GL_CULL_FACE);
                GL.glCullFace(GL.GL_FRONT);
                DrawPinsAndBall(true, false);
                GL.glCullFace(GL.GL_BACK);
                DrawPinsAndBall(false, true);

                Console.WriteLine("RenderScene: Pins drawn.");
                GL.glDisable(GL.GL_CULL_FACE);
                GL.glPopMatrix();



                // 3️⃣ ציור הרצפה השקופה כך שנראה את ההשתקפויות
                GL.glDepthMask((byte)GL.GL_FALSE);
                DrawSingleLane(offsetX, laneY, laneZ, false, true);
                GL.glDepthMask((byte)GL.GL_TRUE);

                GL.glDisable(GL.GL_STENCIL_TEST);

                DrawPinsAndBall(true, false);

            }
            GL.glDisable(GL.GL_BLEND);

            GL.glFlush();

            Console.WriteLine("DrawReflections: Finished drawing reflections.");
        }
        ///////////////////////
        /// ///////////////////
        /// ///End Draw////////
        /// ///////////////////
        /// ///////////////////


        ////////////////////////
        /// ////////////////////
        /// //Bars And Buttons//
        /// ////////////////////
        /// ////////////////////
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"Key Pressed: {e.KeyCode}");

            if (e.KeyCode == Keys.Up)
            {
                bowlingBall.MoveBallForward();
            }
            else if (e.KeyCode == Keys.Down)
            {
                bowlingBall.MoveBallBackward();
            }
        }                
        private void ShotButton_Click(object sender, EventArgs e)
        {
            float directionValue = directionbar.Value; // כיוון במעלות מהמחוון
            float powerValue = powerbar.Value; // עוצמת הירי

            bowlingBall.ShootBall(directionValue, powerValue);
            showArrow = false;

        }
        private void DirectionBar_Scroll(object sender, ScrollEventArgs e)
        {
            arrowDirection = directionbar.Value; // עדכון זווית החץ
            float directionValue = directionbar.Value; // מקבל את הערך מהמחוון
            panel1.Invalidate();
        }
        private void PowerBar_Scroll(object sender, ScrollEventArgs e)
        {
            float powerValue = powerbar.Value; // מקבל את הערך מהמחוון
            Console.WriteLine($"💥 עוצמת הזריקה התעדכנה: {powerValue}");
        }
        private void TrackBarX_Scroll(object sender, EventArgs e)
        {
            rotationX = Math.Max(-15, Math.Min(15, trackBarX.Value)); // 📌 מגביל בין -45 ל- 45
            //Console.WriteLine($"X Rotation: {rotationX}");
            panel1.Invalidate();
            //Console.WriteLine("Invalidate Called X");

        }
        private void TrackBarY_Scroll(object sender, EventArgs e)
        {
            rotationY = Math.Max(-180, Math.Min(180, trackBarY.Value)); // 📌 מגביל בין -90 ל- 90
            //Console.WriteLine($"Y Rotation: {rotationY}");
            panel1.Invalidate();
            //Console.WriteLine("Invalidate Called Y");

        }
        private void TrackBarZ_Scroll(object sender, EventArgs e)
        {
            cameraDistance = Math.Max(-180.0f, Math.Min(180.0f, trackBarZ.Value)); // 📌 מגביל בין 10 ל-100
            //Console.WriteLine($"Zoom Distance: {cameraDistance}");
            panel1.Invalidate();
            //Console.WriteLine("Invalidate Called Z");

        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
            {
                Form1_KeyDown(this, new KeyEventArgs(keyData)); // שולח את האירוע ישירות לפונקציה של הכדור
                return true;  // ❌ מונע מהפקודה לעבור ל-TrackBars
            }
            return base.ProcessDialogKey(keyData);
        }      
        private void TrackBars_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true; // מונע מהחצים לשלוט ב-TrackBars ומאפשר להם לעבור הלאה
        }
        ////////////////////////
        /// ////////////////////
        /// End Bars And Buttons
        /// ////////////////////
        /// ////////////////////

        private void SetCamera()
        {
            GLU.gluLookAt(
                0.0, -0.5, cameraDistance,
                0.0, -5.0, cameraDistance - 10.0,
                0.0, 1.0, 0.0
                );
            GL.glRotatef(rotationX, 1, 0, 0);
            GL.glRotatef(rotationY, 0, 1, 0);
            GL.glRotatef(rotationZ, 0, 0, 1);
            Console.WriteLine("RenderScene: Camera set.");

        }
        private void DrawScene()
        {
            DrawRoom();
            DrawLaneDecorations();
            DrawPinSetter(0);
            DrawPinSetter(-14);
            DrawPinSetter(14);

        }        
        private void Panel1_Paint(object sender, PaintEventArgs e)
        {            
            RenderScene();
        }
        private void RenderScene()
        {
            Console.WriteLine("=== Start RenderScene ===");
            Console.WriteLine($"RenderScene count : Count({ri++})");
            

            GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT | GL.GL_STENCIL_BUFFER_BIT);
            GL.glLoadIdentity();

            SetCamera();
            DrawScene();
            DrawReflections();
            DrawArrow(); // ציור החץ
            bowlingBall.UpdateBallPhysics();

            WGL.wglSwapBuffers(hdc);
            //GL.glFlush();

            Console.WriteLine("=== End RenderScene ===");
            
        }
        public void CheckBallPinCollisions()
        {
            foreach (var pin in pins)
            {
                float distance = (bowlingBall.Position - pin.Position).Length();
                float collisionThreshold =  (bowlingBall.ballRadius + pin.Radius) * 1.4f;

                Console.WriteLine($"Checking collision: Ball({bowlingBall.Position}), Pin({pin.Position}), Distance: {distance}, Threshold: {collisionThreshold}");

                if (distance < collisionThreshold) // 🎯 אם הכדור פוגע בפין
                {
                    //Console.WriteLine($"Collision detected with Pin at {pin.Position}!");
                    Vector3 collisionNormal = Vector3.Normalize(pin.Position - bowlingBall.Position);
                    float impactForce = Math.Max(0, (collisionThreshold - distance) * 2.9f);

                    pin.ApplyForce(collisionNormal * impactForce);
                    pin.IsFalling = true;

                }
            }
        }


        private void LoadTexture(string filename, out uint textureID)
        {
            textureID = 0; // אתחול ברירת מחדל למניעת שגיאת CS0177

            if (!File.Exists(filename))
            {
                Console.WriteLine("Error: File not found - " + filename);
                return; // יציאה מהפונקציה אם הקובץ לא נמצא
            }

            try
            {
                Bitmap bmp = new Bitmap(filename);
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                uint[] textures = new uint[1];
                GL.glGenTextures(1, textures);
                textureID = textures[0]; // כעת מובטח שהוא מאותחל

                GL.glBindTexture(GL.GL_TEXTURE_2D, textureID);
                GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, (int)GL.GL_RGB, bmp.Width, bmp.Height, 0, GL.GL_BGR_EXT, 0x1401, bmpData.Scan0);

                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_LINEAR);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_LINEAR);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (int)GL.GL_REPEAT);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (int)GL.GL_REPEAT);


                bmp.UnlockBits(bmpData);
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading texture: " + ex.Message);
            }
        }
        private void LoadAllTextures()
        {
            
            LoadTexture("pic/wood.jpg", out textureWood);
            LoadTexture("pic/floor.jpg", out textureFloor);
            LoadTexture("pic/ceiling.jpg", out textureCeiling);
            LoadTexture("pic/wall.jpg", out textureWall);
            LoadTexture("pic/lane.png", out textureLane);
            LoadTexture("pic/drakwall.jpg", out textureDarkWall);
            LoadTexture("pic/ball1.jpg", out textureBall);
            LoadTexture("pic/pin.png", out texturePin);
            LoadTexture("pic/pin1.png", out texturePin1);
            LoadTexture("pic/aluminum.jpg", out textureAluminum);
            LoadTexture("pic/wallblue.png", out textureWallBlue);
            LoadTexture("pic/frontwalldown.png", out textureFrontdown);

        }

        ////////////////////////
        /// ////////////////////
        /// ///////Extra////////
        /// ////////////////////
        /// ////////////////////
        private void CreatePinsForLane(List<Pin> pinList, float offsetX)
        {
            pinList.Clear();  // מאתחלים מחדש בכל יצירה

            float startZ = -46.9f;
            float spacing = 1.8f;
            float pinY = -4.75f;

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col <= row; col++)
                {
                    float x = offsetX + col * spacing - (row * spacing / 2);
                    float z = startZ - row * spacing;
                    Pin pin = new Pin(new Vector3(x, pinY, z), quadric, texturePin, texturePin1);
                    pinList.Add(pin);
                }
            }
        }
        private void CreateStaticPins()
        {
            //CreatePinsForLane(leftLanePins, -14.0f);  // מסלול שמאלי
            //CreatePinsForLane(rightLanePins, 14.0f);   // מסלול ימני
            
            foreach (var pin in leftLanePins) //מסלול שמאלי
            {
                pin.GenerateDisplayLists();
            }
            foreach (var pin in rightLanePins) // מסלול ימני
            {
                pin.GenerateDisplayLists();
            }
        }
        void DrawAxes(float length)
        {
            //GL.glDisable(GL.GL_LIGHTING);  // 🔸 מבטלים תאורה כדי להבטיח צבעים ברורים
            GL.glLineWidth(3.0f); // הופך את הצירים לעבים יותר

            GL.glBegin(GL.GL_LINES);

            // 🔵 X-AXIS (כחול)
            GL.glColor3f(0.0f, 0.0f, 1.0f);
            GL.glVertex3f(0.0f, 0.0f, 0.0f);
            GL.glVertex3f(length, 0.0f, 0.0f);

            // 🟢 Y-AXIS (ירוק)
            GL.glColor3f(0.0f, 1.0f, 0.0f);
            GL.glVertex3f(0.0f, 0.0f, 0.0f);
            GL.glVertex3f(0.0f, length, 0.0f);

            // 🔴 Z-AXIS (אדום)
            GL.glColor3f(1.0f, 0.0f, 0.0f);
            GL.glVertex3f(0.0f, 0.0f, 0.0f);
            GL.glVertex3f(0.0f, 0.0f, length);

            GL.glEnd();

            //GL.glEnable(GL.GL_LIGHTING);  // 🔸 מחזירים את התאורה
        }
    }
}
