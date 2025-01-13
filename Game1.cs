using Android.Media.TV;
using Android.Service.Controls.Templates;
using Java.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended.VectorDraw;
using System;
using System.Collections;

namespace AndroidProj
{
    public class Game1 : Game
    {
        public const int mapWidth = 24;
        public const int mapHeight = 24;
        public int[,] worldMap = new int[mapWidth,mapHeight];


        private PrimitiveDrawing _primitiveDrawing;
        PrimitiveBatch _primitiveBatch;
        // pixel 5 screen is 2340 x 1080
        private Matrix _localProjection;
        private Matrix _localView;

        // RayCast is a point and one ray. point is start of ray, dir is the direction
        public RayCast ray = new RayCast();
        // wall is a line segment. two endpoints
        public Wall[] walls = new Wall[9]; // normally 5 but 9 total to include boundary
        public Wall w = new Wall();
        // when chcking for intersection, bool is used if intersection exists.
        // if intersection exists, then find the point by calling another function
        bool IsIntersect = false;
        // touch is location on the screen where the user touches the screen.
        public Vector2 touch = Vector2.Zero;
        // particle is a point plus an array of rays, so there can be raycasts in all directions
        public Particle part = new Particle();
        public Random rand = new Random();

        // Ipoint is the intersect point where the ray intersects the wall
        public Vector2 Ipoint;
        // TouchCollection is the touch object obtained from android device
        TouchCollection touchCollection = new TouchCollection();

        private GraphicsDeviceManager _graphics;
        //private SpriteBatch _spriteBatch;

        // split the screen so there's the top "map" and the bottom first person view
        private Viewport topViewport = new Viewport(0, 0, 1080, 1000);
        private Viewport bottomViewport = new Viewport(0, 10001, 1080, 1000);

        public Rectangle rect = new Rectangle();

        // scene from the coding train video. Same number of rays from Paricle.
        public float[] scene = new float[40];

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            topViewport = new Viewport();
            topViewport.X = 0;
            topViewport.Y = 0;
            topViewport.Width = 1080;
            topViewport.Height = 1000;

            bottomViewport = new Viewport();
            bottomViewport.X = 0;
            bottomViewport.Y = 1001;
            bottomViewport.Width = 1080;
            bottomViewport.Height = 1140;
            // TODO: Add your initialization logic here

            worldMap = new int[mapWidth,mapHeight] {
                { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
                  { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,0,0,0,3,0,0,0,1},
                  { 1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,4,0,0,0,0,5,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,4,0,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
                  { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // _spriteBatch is the default
            // primitiveBatch is the monogame.extended version
            //_spriteBatch = new SpriteBatch(GraphicsDevice);
            _primitiveBatch = new PrimitiveBatch(GraphicsDevice);
            _primitiveDrawing = new PrimitiveDrawing(_primitiveBatch);
            //_localProjection = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            //_localView = Matrix.Identity;
            // ray is the single raycast used in dev/testing
            ray.point = new Vector2(100, 100);
            ray.dir = new Vector2(1, 0);

            // single wall
            w.a = new Vector2(700, 50);
            w.b = new Vector2(700, 500);

            // initial particle location and instantiate the raycasts around the point
            part.pos = new Vector2(200, 200);
            part.RAYS();
            // create 5 random walls
            for (int i = 0; i < 9; i++)
            {
                walls[i] = new Wall();
                walls[i].a = new Vector2(rand.Next(1080), rand.Next(1080));
                walls[i].b = new Vector2(rand.Next(1080), rand.Next(1080));
            }

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            // get the state of the touch screen
            touchCollection = TouchPanel.GetState();
            if (touchCollection.Count > 0)
            {
                // if there's a touch
                if (touchCollection[0].State == TouchLocationState.Moved || touchCollection[0].State == TouchLocationState.Pressed)
                {
                    // debug info to console
                    //System.Diagnostics.Debug.WriteLine(touchCollection[0].Position);
                    touch = touchCollection[0].Position;
                    // from ray.point look at where the finger is touching on screen.
                    // from point to finger, does that ray intersect the wall.
                    //ray.LookAt(touch);

                    // move the particle to where the user is touching
                    part.pos = touch;
                    // update the rays when particle is moved
                    part.RAYS();
                }
            }
            //writes to console
            /*IsIntersect = IsIntersection(w, ray);
            System.Diagnostics.Debug.WriteLine(IsIntersect);
            if (IsIntersect)
            {
                Ipoint = IntersectPoint(w, ray);
            }*/ // Looks to see if a line from point, to TOUCH intersects wall W
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Viewport original = _graphics.GraphicsDevice.Viewport;
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            _graphics.GraphicsDevice.Viewport = topViewport;
            //_graphics.ApplyChanges();
            _localProjection = Matrix.CreateOrthographicOffCenter(0f, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            
            _localView = Matrix.Identity;

            Vector2 tray = ray.point;
            Vector2 tray1 = ray.dir;
            //tray1.X = tray1.X * 100;
            Vector2 total = new Vector2(tray.X + tray1.X, tray.Y + tray1.Y);

            Vector2 temp = Vector2.Zero;
            // distance d
            float d;

            // TODO: Add your drawing code here
            _primitiveBatch.Begin(ref _localProjection, ref _localView);
            //_primitiveDrawing.DrawSegment(w.a, w.b, Color.White);
            //_primitiveDrawing.DrawSegment(tray, total, Color.White); // Draw from a specific ray to the wall
            // draw the particle location
            _primitiveDrawing.DrawCircle(part.pos, 25, Color.White);
            walls[5].a = new Vector2(0, 0);
            walls[5].b = new Vector2(topViewport.Width, 0);
            walls[6].a = new Vector2(topViewport.Width, 0);
            walls[6].b = new Vector2(bottomViewport.Width, topViewport.Height);
            walls[7].a = new Vector2(0, topViewport.Height);
            walls[7].b = new Vector2(topViewport.Width, topViewport.Height);
            walls[8].a = new Vector2(0, 0);
            walls[8].b = new Vector2(0, topViewport.Height);

            _primitiveDrawing.DrawSegment(walls[5].a, walls[5].b, Color.White);
            _primitiveDrawing.DrawSegment(walls[6].a, walls[6].b, Color.White);
            _primitiveDrawing.DrawSegment(walls[7].a, walls[7].b, Color.White);
            _primitiveDrawing.DrawSegment(walls[8].a, walls[8].b, Color.White);
            // draw all the walls
            for (int i = 0; i < 5; i ++)
            {
                _primitiveDrawing.DrawSegment(walls[i].a, walls[i].b, Color.White);
            }
            // for all the rays in particle
            for ( int r = 0; r < part.rays.Length; r++)
            {
                // get closest point
                Vector2 closest = Vector2.Zero;
                // default distance is super far
                // C# has Double.PositiveInfinity as type double
                float rec = 2555;
                scene[r] = rec;
                for (int j = 0; j < 9; j++)
                {
                    //_primitiveDrawing.DrawSegment(part.pos, new Vector2(part.pos.X + part.rays[r].dir.X * 100, part.pos.Y + part.rays[r].dir.Y * 100), Color.White);
                    // if ray does intersect wall
                    if (IsIntersection(walls[j], part.rays[r]))
                    {
                        // find the intersect point
                        temp = IntersectPoint(walls[j], part.rays[r]);
                        // get the distance
                        d = Vector2.Distance(part.pos, temp);
                        // compare if the distance is the closes one, if so store it.
                        if (d < rec)
                        {
                            rec = d;
                            scene[r] = d;
                            closest = temp;
                        }
                        //rec = Math.Min(d, rec);
                        //_primitiveDrawing.DrawSegment(part.pos, temp, Color.White);
                    }
                }
                // if closest exists for the ray, then draw the point
                if (closest != Vector2.Zero)
                {
                    _primitiveDrawing.DrawSegment(part.pos, closest, Color.White);
                }
            }
            /*if (IsIntersect)
            {
                _primitiveDrawing.DrawCircle(Ipoint, 25, Color.White);
            }*/
            _primitiveBatch.End();

            _graphics.GraphicsDevice.Viewport = bottomViewport;
            //_graphics.ApplyChanges();
            //_localProjection = Matrix.CreateOrthographicOffCenter(0f, bottomViewport.Width, bottomViewport.Height, 0f, 0f, 1f);
            _localProjection = Matrix.CreateOrthographicOffCenter(_graphics.GraphicsDevice.Viewport.X, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height, _graphics.GraphicsDevice.Viewport.Y, 0f, 1f);
            _localView = Matrix.Identity;
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            _primitiveBatch.Begin(ref _localProjection, ref _localView);
            float width = bottomViewport.Width / scene.Length;
            for (int q = 0; q < scene.Length; q++)
            {
                //_primitiveDrawing.DrawSegment(new Vector2(bottomViewport.X, bottomViewport.Y), new Vector2(bottomViewport.Width, original.Height), Color.Black);
                if (scene[q] == 2555)
                {
                    _primitiveDrawing.DrawSolidRectangle(new Vector2(q * width, 0), width, bottomViewport.Height, Color.Black);
                }
                else
                {
                    // start at half point of screen.
                    Vector2 MidScene = new Vector2(q * width, 1071);
                    // draw the bottom half of the rectangle
                    // draw the top half by starting at half way - scene[1] [assuming y+ is down the screen and y- is up the screen
                    //_primitiveDrawing.DrawSolidRectangle(new Vector2(q * width, bottomViewport.Y), width, bottomViewport.Y-scene[q], Color.White);
                    //_primitiveDrawing.DrawSolidRectangle(new Vector2(q * width, bottomViewport.Y+600), width, -scene[q], Color.White);
                    _primitiveDrawing.DrawSolidRectangle(MidScene, width, -(DoTheThing(scene[q])/2), Color.White);
                    _primitiveDrawing.DrawSolidRectangle(MidScene, width, (DoTheThing(scene[q])/2), Color.White);
                    // 0 is super close. and screen width is the "max" length 
                    //System.Diagnostics.Debug.WriteLine(scene[q].ToString());
                }
            }
            // This draws from upper left to bottom right of LOWER VIEWPORT
            //_primitiveDrawing.DrawSegment(new Vector2(bottomViewport.X, bottomViewport.Y), new Vector2(bottomViewport.Width, bottomViewport.Height), Color.Black);
            _primitiveDrawing.DrawSegment(new Vector2(bottomViewport.X, bottomViewport.Y + (bottomViewport.Height / 2) -500), new Vector2(bottomViewport.Width, bottomViewport.Height), Color.Black);
            // bottomViewport.Y + (bottomViewport.Height / 2) -500 == 1140/2 - 500 == 70 ==== 1071
            _primitiveBatch.End();

            _graphics.GraphicsDevice.Viewport = original;
            base.Draw(gameTime);
        }

        // Does the ray intersect the wall
        public bool IsIntersection(Wall w, RayCast r)
        {
            Vector2 res = Vector2.Zero;
            float x1 = w.a.X;
            float y1 = w.a.Y;
            float x2 = w.b.X;
            float y2 = w.b.Y;

            float x3 = r.point.X;
            float y3 = r.point.Y;
            float x4 = r.point.X + r.dir.X;
            float y4 = r.point.Y + r.dir.Y;

            float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3-x4);
            if (den == 0)
                return false;

            float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            float u = - ((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;

            return (t > 0 && t < 1 && u > 0);
        }

        // Find the point at which ray intersects wall
        public Vector2 IntersectPoint(Wall w, RayCast r)
        {
            Vector2 p = Vector2.Zero;
            float x1 = w.a.X;
            float y1 = w.a.Y;
            float x2 = w.b.X;
            float y2 = w.b.Y;

            float x3 = r.point.X;
            float y3 = r.point.Y;
            float x4 = r.point.X + r.dir.X;
            float y4 = r.point.Y + r.dir.Y;

            float den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den;
            float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / den;
            p.X = x1 + t * (x2 - x1);
            p.Y = y1 + t * (y2 - y1);
            return p;
        }

        public float DoTheThing(float x)
        {
            // 0 = 1100
            // 1100 = 0
            // b = -1100
            // m = 1
            int b = 550;

            return (float)Math.Clamp((b - x), 0d, (double)b);
        }
    }
}