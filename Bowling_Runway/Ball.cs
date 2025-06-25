using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenGL;
using System.Windows.Forms;
using System.Media;



namespace Bowling_Runway
{
    public class Ball
    {
        public Vector3 Position { get;  set; }
        public Vector3 Velocity { get;  set; } // ✅ הוספת private set
        public float[] ShadowMatrix { get; set; }  // מטריצת צל

        //public float ballPositionZ = 37.0f;
        public float ballPositionZ = -35.0f;

        private Timer ballTimer; // טיימר להזזת הכדור
        public float ballSpeed = 0.0f; // מהירות התנועה
        public float ballRotation = 0.0f; // זווית הסיבוב של הכדור
        public float ballRadius = 1.0f; // רדיוס הכדור (השתמש בערך המתאים לספירה)
        public float acceleration = 0.02f; // תאוצה (כמה מהר הכדור מאיץ)
        public float maxSpeed = 1.8f; // מהירות מקסימלית של הכדור
        public bool decelerationPhase = false;
        private Form1 form;  // ✅ מחזיק הפניה לטופס הראשי
        public float[] lightPos = { 0.0f, 15.0f, 30.0f, 0.0f };  // מקור אור קרוב יותר למסלול, כמו שהסברתי
        float[] lightSpecular = { 1.0f, 1.0f, 1.0f, 1.0f };
        private Vector3 ballDirection = new Vector3(0, 0, -1); // כיוון התנועה של הכדור
        private float soundTriggerZ = -45.0f; // ✅ הקו שבו ננגן את הסאונד
        public bool soundPlayed = false; // נבדוק אם כבר ניגנו את הסאונד

        private float minX = -5.0f;  // גבול שמאלי
        private float maxX = 5.0f;   // גבול ימני


        public Ball(Form1 formInstance)
        {
            form = formInstance;
            Position = new Vector3(0.0f, -3.8f, 35.0f);
            Velocity = Vector3.Zero;
        }

        public void SetShadowMatrix(float[] matrix)
        {
            ShadowMatrix = matrix;
        }

        public void Draw(uint texture, float posX, float posY, float posZ, bool drawShadow=true, bool isReflection=false)
        {
            GL.glLightModeli(GL.GL_LIGHT_MODEL_TWO_SIDE, (int)GL.GL_TRUE);  // הפעלת תאורה דו-צדדית

            if (drawShadow)
            {
                GL.glPushMatrix();
                GL.glDisable(GL.GL_TEXTURE_2D);
                DrawShadow();
                GL.glPopMatrix();
                GL.glLightfv(GL.GL_LIGHT0, GL.GL_SPECULAR, lightPos);
                GL.glEnable(GL.GL_LIGHTING);
            }
            

            if (isReflection)
            {
                GL.glNormal3f(0, 1, 1); // נורמל כלפי מעלה
                GL.glLightfv(GL.GL_LIGHT0, GL.GL_SPECULAR, lightPos);
                GL.glEnable(GL.GL_LIGHTING);
            }
            GL.glPushMatrix();

            GL.glTranslatef(posX, posY, posZ);  // ✅ קובע את המיקום מחוץ למחלקה
            GL.glRotatef(ballRotation, 1.0f, 0.0f, 0.0f);
        
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, texture);
            GLU.gluQuadricTexture(form.quadric, 1);
            GLU.gluSphere(form.quadric, 1.0f, 32, 32);
        
            GL.glDisable(GL.GL_TEXTURE_2D);
            GL.glPopMatrix();
        }
        
        private void DrawShadow()
        {
            if (ShadowMatrix == null) return;
          
            GL.glPushMatrix();
            GL.glDisable(GL.GL_LIGHTING);
            GL.glDisable(GL.GL_TEXTURE_2D);
            GL.glTranslatef(Position.X, Position.Y-1.0f, Position.Z);
            GL.glMultMatrixf(ShadowMatrix);
        
            
            GL.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);
            GLU.gluSphere(form.quadric, 1.0f, 32, 32);
        
            GL.glEnable(GL.GL_LIGHTING);
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glPopMatrix();
        }

        public void InitializeBallTimer()
        {
            if (ballTimer == null) // בודק שהטיימר לא מאותחל פעמיים
            {
                ballTimer = new Timer();
                ballTimer.Interval = 35; // 60 FPS (16ms לפריים)
                ballTimer.Tick += BallTimer_Tick;
                ballTimer.Start();
            }
        }
        private static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }


        public void ShootBall(float directionAngle, float power)
        {
            float radians = (float)(Math.PI / 180) * directionAngle; // המרה ידנית ממעלות לרדיאנים
            ballDirection = new Vector3((float)Math.Sin(radians), 0, -(float)Math.Cos(radians)); // חישוב וקטור כיוון

            ballSpeed = Clamp(power / 10f, 0, 2.0f);
            PlayThrowSound();
        }

        

        private void BallTimer_Tick(object sender, EventArgs e)
        {
            if (Math.Abs(ballSpeed) > 0.001f)
            {
                // 🔹 עדכון מיקום הכדור לפי הכיוון שנקבע
                Position += ballDirection * ballSpeed;
                ballRotation -= (ballSpeed * 360.0f) / (2 * (float)Math.PI * ballRadius);
                ballSpeed *= 0.99f;
                foreach (var pin in form.pins)
                {
                    pin.UpdatePhysics();
                }
                // ✅ שמירה על הכדור בתוך גבולות המסלול
                if (Position.X < minX)
                {
                    Position = new Vector3(minX, Position.Y, Position.Z);
                    ballDirection.X = -ballDirection.X * 0.7f; // משנה כיוון עם חיכוך
                }
                else if (Position.X > maxX)
                {
                    Position = new Vector3(maxX, Position.Y, Position.Z);
                    ballDirection.X = -ballDirection.X * 0.7f; // משנה כיוון עם חיכוך
                }                

                if (Math.Abs(ballSpeed) < 0.01f)
                {
                    ballSpeed = 0;
                    decelerationPhase = false;
                }

                form.CheckBallPinCollisions();
                if (Position.Z <= soundTriggerZ && !soundPlayed)
                {
                    PlayPinHitSound();
                    soundPlayed = true; // ✅ מבטיח שהסאונד יופעל רק פעם אחת
                }
            }

            if (Math.Abs(ballSpeed) > 0.001f || form.pins.Any(p => p.IsFalling))
            {
                
                form.panel1.Invalidate();
            }
        }
        public void PlayPinHitSound()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("pic/pin_hit.wav"); // 🔊 קובץ סאונד
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error playing sound: {ex.Message}");
            }
        }

        public void PlayThrowSound()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("pic/throw.wav"); // 🔊 קובץ סאונד
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error playing sound: {ex.Message}");
            }
        }

        public void MoveBallForward()
        {
            if (ballSpeed < maxSpeed) // האצה הדרגתית
            {
                ballSpeed += acceleration;
            }
        }

        public void MoveBallBackward()
        {
            if (ballSpeed > -maxSpeed) // האטה אחורה
            {
                ballSpeed -= acceleration;
            }
        }

        public void CheckCollision(Pin pin)
        {
            float collisionThreshold = ballRadius + pin.Radius;

            Vector3 direction = pin.Position - Position;
            float distance = direction.Length();
            Console.WriteLine($"Checking Collision: Ball({Position}) - Pin({pin.Position}) | Distance: {distance}");

            if (distance < collisionThreshold)
            {
                Console.WriteLine($"⚡ Ball hit Pin at {pin.Position}!");

                Vector3 collisionNormal = Vector3.Normalize(direction);
                float impactForce = (collisionThreshold - distance) * 3.0f; // הכפלת הכוח
                Vector3 impactVelocity = Velocity * 0.6f;  // הוספת רכיב המהירות של הכדור

                pin.ApplyForce(collisionNormal * impactForce + impactVelocity);
            }
        }

        public void UpdateBallPhysics()
        {
            Position += Velocity; // עדכון המיקום

            // האטה טבעית (חיכוך)
            Velocity *= 0.98f;

            // עצירה מוחלטת אם המהירות קטנה מאוד
            if (Velocity.Length() < 0.05f)
            {
                Velocity = Vector3.Zero;
            }
        }

        public void UpdatePhysics()
        {
            Position += Velocity;
            Velocity *= 0.98f;  // חיכוך רצפה

            if (Position.Y < -4.9f)
            {
                Position = new Vector3(Position.X, -4.9f, Position.Z);
            }
        }
        public void ApplyForce(Vector3 force)
        {
            Velocity += force; // ✅ מעדכן את המהירות באמצעות פונקציה ייעודית
        }

    }
}


