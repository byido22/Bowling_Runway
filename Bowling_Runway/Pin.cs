using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenGL;



namespace Bowling_Runway
{
    public class Pin
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity = Vector3.Zero;
        public Vector3 InitialNormal { get; private set; }

        public float[] ShadowMatrix { get; private set; }
        public float RotationX { get; set; }
        public float RotationZ { get; set; }
        public float Radius = 0.4f;
        public bool IsFalling { get; set; }
        private float AngularVelocityX = 0.0f;
        private float AngularVelocityZ = 0.0f;
        public bool ShouldDraw { get; private set; } = true;
        public Form1 form;  // ✅ מחזיק הפניה לטופס הראשי
        float[] pos = new float[4];      
        private uint pinDisplayList=0;
        private uint shadowDisplayList=0;
        private GLUquadric quadric;
        private uint texturePin;
        private uint texturePin1;
        public bool soundPlayed = false; // נבדוק אם כבר ניגנו את הסאונד

        public Pin(Vector3 position, GLUquadric quadric, uint texturePin, uint texturePin1)
        {
            Position = position;
            InitialNormal = new Vector3(0, 1, 0);
            this.quadric = quadric;  // שמירת ה-quadric שהתקבל מ-Form1
            this.texturePin = texturePin;
            this.texturePin1 = texturePin1;
            GL.glEnable(GL.GL_LIGHTING);

        }

        public void SetShadowMatrix(float[] matrix)
        {
            ShadowMatrix = matrix;
        }        
        public void UpdatePhysics()
        {
            
            if (IsFalling)
            {               
                // הוספת כוח כבידה
                Velocity += new Vector3(0, -0.5f, 0);
                Position += Velocity;

                // עדכון סיבוב טבעי של הפין
                RotationX += AngularVelocityX;
                RotationZ += AngularVelocityZ;

                // איזון החיכוך
                AngularVelocityX *= 0.98f;
                AngularVelocityZ *= 0.98f;
                Velocity *= 0.95f; // פחות האטה

                // מניעת נפילה מתחת לרצפה
                if (Position.Y < 0)
                {
                    Position = new Vector3(Position.X, 0, Position.Z);
                    Velocity = Vector3.Zero;
                    AngularVelocityX = 0;
                    AngularVelocityZ = 0;
                    IsFalling = false;
                }
            }
        }
        public void ApplyForce(Vector3 force)
        {
            if (force.Length() > 0.01f) // 💡 מוודא שלא מדובר בכוח אפסי
            {
                Velocity += force * 1.5f; // 📌 הגדלת הכוח המופעל על הפין
                IsFalling = true;
            }
        }
        public void Draw(GLUquadric quadric, float posX, float posY, float posZ, bool drawShadow = true, bool isReflection = false)
        {
            if (!ShouldDraw) return;

            // הפעלת תאורה דו-צדדית פעם אחת במערכת הראשית, לא כל ציור מחדש
            //GL.glLightModeli(GL.GL_LIGHT_MODEL_TWO_SIDE, (int)GL.GL_TRUE);  

            float[] pinMaterialDiffuse = { 1.5f, 1.5f, 1.5f, 1.0f };
            float[] pinMaterialSpecular = { 2.5f, 2.5f, 2.5f, 1.0f };
            float shininess = 50.0f;

            GL.glPushMatrix();
            GL.glTranslatef(posX, posY, posZ);

            // אם זה השתקפות, שנה את התאורה בהתאם
            if (isReflection)
            {
                pos[0] = 0; pos[1] = -10; pos[2] = -30; pos[3] = 0;
                GL.glLightfv(GL.GL_LIGHT0, GL.GL_POSITION, pos);

                float[] reflectionSpecular = { 1.0f, 1.0f, 1.0f, 1.0f };
                GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_SPECULAR, reflectionSpecular);
                shininess = 30.0f;
            }
            else
            {
                pos[0] = 0; pos[1] = 10; pos[2] = -30; pos[3] = 0;
                GL.glLightfv(GL.GL_LIGHT0, GL.GL_POSITION, pos);
            }

            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_DIFFUSE, pinMaterialDiffuse);
            GL.glMaterialf(GL.GL_FRONT_AND_BACK, GL.GL_SHININESS, shininess);

            GL.glRotatef(RotationX, 1, 0, 0);
            GL.glRotatef(RotationZ, 0, 0, 1);

            GL.glDisable(GL.GL_TEXTURE_2D);
            GL.glRotatef(90, 0, 1, 0);
            GL.glCallList(pinDisplayList);
            GL.glPopMatrix();

            if (drawShadow && !isReflection)
            {
                DrawShadow(quadric);
            }
        }
        private void DrawShadow(GLUquadric quadric)
        {
            if (ShadowMatrix == null)
            {
                Console.WriteLine($"⚠️ Pin at {Position} missing shadow matrix!");
                return;
            }

            GL.glDisable(GL.GL_TEXTURE_2D);

            GL.glPushMatrix();
            GL.glTranslatef(Position.X, Position.Y, Position.Z);

            GL.glMultMatrixf(ShadowMatrix);

            //GL.glDisable(GL.GL_LIGHTING);

            // צבע שחור עם שקיפות חלקית (צל)
            GL.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);

            GL.glRotatef(RotationX, 1, 0, 0);
            GL.glRotatef(RotationZ, 0, 0, 1);
            GL.glRotatef(90, 0, 1, 0);

            GL.glCallList(shadowDisplayList);  // שימוש ברשימת צל

            //GL.glEnable(GL.GL_LIGHTING);
            GL.glPopMatrix();
            GL.glEnable(GL.GL_TEXTURE_2D);

        }
        public void SetFalling(bool falling)
            {
                IsFalling = falling;
                ShouldDraw = true;
            }       
        private void DrawParabolicBowlingPin(bool isShadow)
        {
            GL.glPushMatrix();
            GL.glTranslatef(0.0f, 0.0f, 0.0f);
            GL.glRotatef(-90, 1, 0, 0);

            float height = 2.0f;
            int segments = 40;
            float h_mid = 1.0f;
            float baseRadius = 0.2f;
            float maxRadius = 0.4f;
            float a = -0.25f;

            float prevRadius = baseRadius;
            float prevHeight = 0.0f;

            // גוף הפין - צבע לבן
            if (isShadow)
            {
                GL.glDisable(GL.GL_TEXTURE_2D);
                GL.glEnable(GL.GL_BLEND);
                GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
                //GL.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);
            }
            else
            {
                GL.glDisable(GL.GL_BLEND);
                GL.glEnable(GL.GL_TEXTURE_2D);
                GL.glBindTexture(GL.GL_TEXTURE_2D, texturePin1); // גוף הפין - לבן
                GL.glColor3f(1.0f, 1.0f, 1.0f);
            }
            for (int i = 1; i <= segments; i++)
            {
                float h = (float)i / segments * height;
                float radius = (float)(a * Math.Pow(h - h_mid, 2) + maxRadius);

                GL.glPushMatrix();
                GL.glTranslatef(0.0f, 0.0f, prevHeight);
                GLU.gluCylinder(quadric, prevRadius, radius, h - prevHeight, 32, 32);
                GL.glPopMatrix();

                prevHeight = h;
                prevRadius = radius;

            }

            // צוואר הפין
            float neckStartHeight = prevHeight - 0.25f;
            float neckEndHeight = prevHeight + 0.3f;
            float neckMaxRadius = maxRadius * 0.5f;
            float neckMinRadius = maxRadius * 0.25f;
            float ringThickness = 0.3f;

            int neckSegments = segments / 4;
            for (int i = 0; i <= neckSegments; i++)
            {
                float t = (float)i / neckSegments;
                float h = neckStartHeight + t * (neckEndHeight - neckStartHeight);
                float radius;

                if (t < 0.5f)
                {
                    radius = neckMaxRadius - (neckMaxRadius - neckMinRadius) * (float)Math.Pow(2 * t, 1.5);
                    if (isShadow)
                    {
                        GL.glDisable(GL.GL_TEXTURE_2D);
                        GL.glEnable(GL.GL_BLEND);
                        GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
                        GL.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);
                    }
                    else
                    {
                        GL.glDisable(GL.GL_BLEND);
                        GL.glEnable(GL.GL_TEXTURE_2D);
                        GL.glBindTexture(GL.GL_TEXTURE_2D, texturePin1); // גוף הפין - לבן
                        GL.glColor3f(1.0f, 1.0f, 1.0f);
                    }
                    GL.glPushMatrix();
                    GL.glTranslatef(0.0f, 0.0f, prevHeight + 0.09f);
                    GLU.gluCylinder(quadric, prevRadius, radius, h - prevHeight, 32, 32);
                    GL.glPopMatrix();
                }
                else if (Math.Abs(t - 0.5f) < 0.01f)
                {
                    // יצירת טבעת
                    if (isShadow)
                    {
                        GL.glDisable(GL.GL_TEXTURE_2D);
                        GL.glEnable(GL.GL_BLEND);
                        GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
                        GL.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);
                    }
                    else
                    {
                        GL.glDisable(GL.GL_BLEND);
                        GL.glEnable(GL.GL_TEXTURE_2D);
                        GL.glBindTexture(GL.GL_TEXTURE_2D, texturePin); // גוף הפין - לבן
                        GL.glColor3f(1.0f, 1.0f, 1.0f);
                    }
                    GL.glPushMatrix();
                    GL.glTranslatef(0.0f, 0.0f, prevHeight + 0.01f);
                    GLU.gluCylinder(quadric, prevRadius, prevRadius, ringThickness, 32, 1);
                    GL.glPopMatrix();

                    prevHeight += ringThickness;
                    continue;
                }
                else
                {
                    radius = (neckMinRadius + (neckMaxRadius - neckMinRadius) * (float)Math.Pow(2 * (t - 0.5f), 1.5)) + 0.03f;
                    if (isShadow)
                    {
                        GL.glDisable(GL.GL_TEXTURE_2D);
                        GL.glEnable(GL.GL_BLEND);
                        GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
                        GL.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);
                    }
                    else
                    {
                        GL.glDisable(GL.GL_BLEND);
                        GL.glEnable(GL.GL_TEXTURE_2D);
                        GL.glBindTexture(GL.GL_TEXTURE_2D, texturePin1); // גוף הפין - לבן
                        GL.glColor3f(1.0f, 1.0f, 1.0f);
                    }
                    GL.glPushMatrix();
                    GL.glTranslatef(0.0f, 0.0f, prevHeight + 0.15f);
                    GLU.gluCylinder(quadric, prevRadius, radius, h - prevHeight, 32, 32);
                    GL.glPopMatrix();
                }

                prevHeight = h;
                prevRadius = radius;
            }
            Console.WriteLine("Actually drawing pin...");

            // ראש הפין - צבע לבן
            if (isShadow)
            {
                GL.glDisable(GL.GL_TEXTURE_2D);
                GL.glEnable(GL.GL_BLEND);
                GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
                GL.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);
            }
            else
            {
                GL.glDisable(GL.GL_BLEND);
                GL.glEnable(GL.GL_TEXTURE_2D);
                GL.glBindTexture(GL.GL_TEXTURE_2D, texturePin1); // גוף הפין - לבן
                GL.glColor3f(1.0f, 1.0f, 1.0f);
            }
            float ballRadius = prevRadius + 0.04f;
            GL.glPushMatrix();
            GL.glTranslatef(0.0f, 0.0f, prevHeight + 0.3f);
            GLU.gluSphere(quadric, ballRadius, 32, 32);
            GL.glPopMatrix();
            prevHeight += ballRadius; // עדכון הגובה החדש של הכדור
            GL.glPopMatrix();
        }
        public void GenerateDisplayLists()
        {
            if (pinDisplayList != 0 && shadowDisplayList != 0) return;  // כבר נוצרו

            pinDisplayList = GL.glGenLists(1);
            GL.glNewList(pinDisplayList, GL.GL_COMPILE);
            DrawParabolicBowlingPin(false);
            GL.glEndList();

            shadowDisplayList = GL.glGenLists(1);
            GL.glNewList(shadowDisplayList, GL.GL_COMPILE);
            GL.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);  // צל תמיד שחור
            DrawParabolicBowlingPin(true);
            GL.glEndList();

            Console.WriteLine("✅ Pin display lists generated");
        }
    }
}
