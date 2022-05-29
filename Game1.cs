using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace asteroids
{
    /* Referências
     * https://github.com/Dominoman/Asteroids/tree/master/Asteroids
     * https://drive.google.com/file/d/1jbV8HlqYXspsgZC-Mg1TQxys0kWgfBmd/view
     * https://www.dreamincode.net/forums/topic/100575-xna-30-game-tutorial-part-1/
     */
    public class Game1 : Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Sprite ship;

        int ScreenWidth, ScreenHeight;
        int level = 0;
        int score = 0;
        int lives;

        KeyboardState oldState;

        Sprite bullet;
        List<Sprite> bullets = new List<Sprite>();

        List<Texture2D> asteroidTextures = new List<Texture2D>();
        List<Sprite> asteroids = new List<Sprite>();

        SpriteFont myFont;
        Texture2D banner;

        float distance = 0.0f;

        Random random = new Random();

        bool GameOver = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            SetupGame();
        }


        private void SetupGame()
        {
            //Size da janela
            ScreenHeight = graphics.GraphicsDevice.Viewport.Height;
            ScreenWidth = graphics.GraphicsDevice.Viewport.Width;
            SetupShip();
        }

        private void SetupShip()
        {
            //Mete o Ship parado no centro do ecrã
            ship.Rotation = 0;
            ship.Velocity = Vector2.Zero;
            bullets.Clear();
            ship.Position = new Vector2(ScreenWidth / 2, ScreenHeight / 2);
            ship.Create();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //dar load a todas as sprites e texturas
            ship = new Sprite(Content.Load<Texture2D>("ship"));
            bullet = new Sprite(Content.Load<Texture2D>("bullet"));

            for (int i = 1; i < 4; i++)
                asteroidTextures.Add(Content.Load<Texture2D>("large" + i.ToString()));

            for (int i = 1; i < 4; i++)
                asteroidTextures.Add(Content.Load<Texture2D>("medium" + i.ToString()));

            for (int i = 1; i < 4; i++)
                asteroidTextures.Add(Content.Load<Texture2D>("small" + i.ToString()));

            myFont = Content.Load<SpriteFont>("myFont");

            banner = Content.Load<Texture2D>("BANNER");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();

            //Fecha o jogo
            if (newState.IsKeyDown(Keys.Escape))
                this.Exit();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (GameOver)
            {
                asteroids.Clear();
                ship.Kill();

                //No menu se o Enter for carregado o jogo começa
                if (newState.IsKeyDown(Keys.Enter))
                {
                    level = 0;
                    score = 0;
                    lives = 3;
                    SetupGame();
                    CreateAsteroids();
                    GameOver = false;
                }
                else
                {
                    return;
                }
            }

            //Velocidade da rotação para a esquerda
            if (newState.IsKeyDown(Keys.Left))
            {
                ship.Rotation -= 0.05f;
            }

            //Velocidade da rotação para a direita
            if (newState.IsKeyDown(Keys.Right))
            {
                ship.Rotation += 0.05f;
            }

            //Se o space for pressionado e largado é disparada apenas uma bala
            if (newState.IsKeyUp(Keys.Space) && oldState.IsKeyDown(Keys.Space))
            {
                FireBullet();
            }

            if (newState.IsKeyDown(Keys.Up))
            {
                AccelerateShip();
            }
            //Quando se larga o Up o Ship desacelera até parar
            else if (newState.IsKeyUp(Keys.Up))
            {
                DecelerateShip();
            }

            oldState = newState;

            UpdateShip();
            UpdateAsteroids();
            UpdateBullets();
            AllDead();

            base.Update(gameTime);
        }
        private void AccelerateShip()
        {
            ship.Velocity += new Vector2(
                //Velocidade da aceleração contraria ao ship
                (float)(Math.Cos(ship.Rotation - MathHelper.PiOver2) * 0.05f),
                //Velocidade da aceleração a favor do ship
                (float)((Math.Sin(ship.Rotation - MathHelper.PiOver2) * 0.05f)));

            //Se o ship passar uma certa velocidade a aceleração aumenta
            if (ship.Velocity.X > 5.0f)
            {
                //para a direita
                ship.Velocity = new Vector2(5.0f, ship.Velocity.Y);
            }
            if (ship.Velocity.X < -5.0f)
            {
                //para a esquerda
                ship.Velocity = new Vector2(-5.0f, ship.Velocity.Y);
            }
            if (ship.Velocity.Y > 5.0f)
            {
                //para baixo
                ship.Velocity = new Vector2(ship.Velocity.X, 50.0f);
            }
            if (ship.Velocity.Y < -5.0f)
            {
                //para cima
                ship.Velocity = new Vector2(ship.Velocity.X, -5.0f);
            }
        }

        private void DecelerateShip()
        {
            if (ship.Velocity.X < 0)
            {
                //para a direita
                ship.Velocity = new Vector2(ship.Velocity.X + 0.02f, ship.Velocity.Y);
            }

            if (ship.Velocity.X > 0)
            {
                //para a esquerda
                ship.Velocity = new Vector2(ship.Velocity.X - 0.02f, ship.Velocity.Y);
            }

            if (ship.Velocity.Y < 0)
            {
                //para baixo
                ship.Velocity = new Vector2(ship.Velocity.X, ship.Velocity.Y + 0.02f);
            }

            if (ship.Velocity.Y > 0)
            {
                //para cima
                ship.Velocity = new Vector2(ship.Velocity.X, ship.Velocity.Y - 0.02f);
            }
        }

        public void UpdateShip()
        {
            //Isto é o que faz o ship voltar ao ecra quando vai contra as paredes (sempre contrarias)
            ship.Position += ship.Velocity;

            //Ao retirar o ship nao volta
            if (ship.Position.X + ship.Width < 0)
            {
                //direita para a esquerda
                ship.Position = new Vector2(ScreenWidth, ship.Position.Y);
            }
            if (ship.Position.X - ship.Width > ScreenWidth)
            {
                //esquerda para a direita
                ship.Position = new Vector2(0, ship.Position.Y);
            }
            if (ship.Position.Y + ship.Height < 0)
            {
                //baixo para cima
                ship.Position = new Vector2(ship.Position.X, ScreenHeight);
            }
            if (ship.Position.Y - ship.Height > ScreenHeight)
            {
                //cima para baixo
                ship.Position = new Vector2(ship.Position.X, 0);
            }
        }
        private void AllDead()
        {
            bool allDead = true;

            foreach (Sprite s in asteroids)
            {
                if (s.Alive)
                    allDead = false;
            }

            if (allDead)
            {
                //Se todos os asteroides forem destruidos o ship é colocado de novo no centro e os asteroides sao reiniciados
                SetupGame();
                level++;
                asteroids.Clear();
                CreateAsteroids();
            }

        }

        private void CreateAsteroids()
        {
            int value;

            for (int i = 0; i < 4 + level; i++)
            {
                //Coloca os asteroids na lista e coloca-os aleotoriamente no ecrâ com uma velocidade e rotação também aleatoria
                int index = random.Next(0, 3);

                Sprite tempSprite = new Sprite(asteroidTextures[index]);
                asteroids.Add(tempSprite);
                asteroids[i].Index = index;

                double xPos = 0;
                double yPos = 0;

                value = random.Next(0, 8);

                switch (value)
                {
                    //posiçoes possiveis 0-1 esquerda 2-3 direita 4-5 cima e 6-7 baixo
                    case 0:
                    case 1:
                        xPos = asteroids[i].Width + random.NextDouble();
                        yPos = random.NextDouble() * ScreenHeight;
                        break;
                    case 2:
                    case 3:
                        xPos = ScreenWidth - random.NextDouble();
                        yPos = random.NextDouble() * ScreenHeight;
                        break;
                    case 4:
                    case 5:
                        xPos = random.NextDouble() * ScreenWidth;
                        yPos = asteroids[i].Height + random.NextDouble();
                        break;
                    default:
                        xPos = random.NextDouble() * ScreenWidth;
                        yPos = ScreenHeight - random.NextDouble();
                        break;
                }

                asteroids[i].Position = new Vector2((float)xPos, (float)yPos);

                asteroids[i].Velocity = RandomVelocity();

                asteroids[i].Rotation = (float)random.NextDouble() *
                        MathHelper.Pi * 4 - MathHelper.Pi * 2;

                asteroids[i].Create();
            }
        }

        private void UpdateAsteroids()
        {
            //faz com que os asteroids voltem ao ecra quando vao contra as paredes
            foreach (Sprite a in asteroids)
            {
                a.Position += a.Velocity;

                if (a.Position.X + a.Width < 0)
                {
                    //direita para a esquerda
                    a.Position = new Vector2(ScreenWidth, a.Position.Y);
                }
                if (a.Position.Y + a.Height < 0)
                {
                    //baixo para cima
                    a.Position = new Vector2(a.Position.X, ScreenHeight);
                }
                if (a.Position.X - a.Width > ScreenWidth)
                {
                    //direita para a esquerda
                    a.Position = new Vector2(0, a.Position.Y);
                }
                if (a.Position.Y - a.Height > ScreenHeight)
                {
                    //cima para baixo
                    a.Position = new Vector2(a.Position.X, 0);
                }

                //Se algum asteroid estiver "vivo" e for contra o ship, o asteroid e destruido, o ship perde uma vida e o ship retoma a posição inicial
                if (a.Alive && CheckShipCollision(a))
                {
                    a.Kill();
                    lives--;
                    SetupShip();
                    if (lives < 1)
                        GameOver = true;
                }
            }

        }

        private bool CheckShipCollision(Sprite asteroid)
        {
            Vector2 position1 = asteroid.Position;
            Vector2 position2 = ship.Position;

            //Ao usar o absoluto conseguimos obter sempre valores positivos e obter a distância ente os asteroids e o ship
            float Cathetus1 = Math.Abs(position1.X - position2.X);
            float Cathetus2 = Math.Abs(position1.Y - position2.Y);

            //Ao saber os valores anteriores obtemos um ponto médio, para calcular a distância final podemos aplicar o teorema de pitágoras
            Cathetus1 *= Cathetus1; 
            Cathetus2 *= Cathetus2;

            distance = (float)Math.Sqrt(Cathetus1 + Cathetus2);

            //Se a distancia calculada for menor  significa que está a ocorrer uma colisão, só gera uma pequena margem de erro
            if ((int)distance < ship.Width)
                return true;

            return false;
        }

        private bool CheckAsteroidCollision(Sprite asteroid, Sprite bullet)
        {
            Vector2 position1 = asteroid.Position;
            Vector2 position2 = bullet.Position;
            //Ao usar o absoluto conseguimos obter sempre valores positivos e obter a distância ente os asteroids e as balas
            float Cathetus1 = Math.Abs(position1.X - position2.X);
            float Cathetus2 = Math.Abs(position1.Y - position2.Y);

            //Ao saber os valores anteriores obtemos um ponto médio, para calcular a distância final podemos aplicar o teorema de pitágoras
            Cathetus1 *= Cathetus1;
            Cathetus2 *= Cathetus2;

            distance = (float)Math.Sqrt(Cathetus1 + Cathetus2);

            //Se a distancia calculada for menor significa que está a ocorrer uma colisão, só gera uma pequena margem de erro
            if ((int)distance < asteroid.Width)
                return true;

            return false;
        }

        private void UpdateBullets()
        {
            List<Sprite> destroyed = new List<Sprite>();

            foreach (Sprite b in bullets)
            {
                b.Position += b.Velocity;
                foreach (Sprite a in asteroids)
                {
                    //verifica se a bala atinge o asteroid apagando os dois caso aconteça e adicionando ao score

                    if (a.Alive && CheckAsteroidCollision(a, b))
                    {
                        if (a.Index < 3)
                            score += 25;
                        else if (a.Index < 6)
                            score += 50;
                        else
                            score += 100;

                        a.Kill();
                        destroyed.Add(a);
                        b.Kill();
                    }
                }
                //Se a bala passar das bordas do screen apaga as balas
                if (b.Position.X < 0)
                    b.Kill();
                else if (b.Position.Y < 0)
                    b.Kill();
                else if (b.Position.X > ScreenWidth)
                    b.Kill();
                else if (b.Position.Y > ScreenHeight)
                    b.Kill();
            }

            for (int i = 0; i < bullets.Count; i++)
            {
                if (!bullets[i].Alive)
                {
                    bullets.RemoveAt(i);
                    i--;
                }
            }

            foreach (Sprite a in destroyed)
            {
                SplitAsteroid(a);
            }
        }

        private void SplitAsteroid(Sprite a)
        {
            //caso o index seja menor que 3 significa que é um meteoro grande transformando assim num médio
            if (a.Index < 3)
            {
                for (int i = 0; i < 2; i++)
                {
                    int index = random.Next(3, 6);
                    NewAsteroid(a, index);
                }
            }
            //caso o index seja menor que 6 significa que é um meteoro médio transformando assim num pequeno
            else if (a.Index < 6)
            {
                for (int i = 0; i < 2; i++)
                {
                    int index = random.Next(6, 9);
                    NewAsteroid(a, index);
                }
            }
        }

        private void NewAsteroid(Sprite a, int index)
        {
            Sprite tempSprite = new Sprite(asteroidTextures[index]);

            //cria um novo asteroid quando outro é partido na mesma posição mas com velocidades e rotações randoms
            tempSprite.Index = index;
            tempSprite.Position = a.Position;
            tempSprite.Velocity = RandomVelocity();

            tempSprite.Rotation = (float)random.NextDouble() *
                MathHelper.Pi * 4 - MathHelper.Pi * 2;

            tempSprite.Create();
            asteroids.Add(tempSprite);

        }

        private Vector2 RandomVelocity()
        {
            //determina uma velocidade aleatória
            float xVelocity = (float)(random.NextDouble() * 2 + .5);
            float yVelocity = (float)(random.NextDouble() * 2 + .5);

            if (random.Next(2) == 1)
                xVelocity *= -1.0f;

            if (random.Next(2) == 1)
                yVelocity *= -1.0f;

            return new Vector2(xVelocity, yVelocity);
        }

        private void FireBullet()
        {
            Sprite newBullet = new Sprite(bullet.Texture);

            //Contém a velocidade da bala
            Vector2 velocity = new Vector2(
                (float)Math.Cos(ship.Rotation - (float)MathHelper.PiOver2),
                (float)Math.Sin(ship.Rotation - (float)MathHelper.PiOver2));
            //mete o vetor com 1 de length
            velocity.Normalize();
            velocity *= 6.0f;

            newBullet.Velocity = velocity;

            newBullet.Position = ship.Position + newBullet.Velocity;
            newBullet.Create();
            //adiciona á lista de balas
            bullets.Add(newBullet);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //caso inicie o jogo ou perca as vidas todas aparece o menu
            if (GameOver)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend); ;

                Vector2 position2 = new Vector2(0.0f, 20.0f);
                spriteBatch.Draw(banner, position2, Color.White);

                string text = "";

                Vector2 size = myFont.MeasureString(text);

                position2 = new Vector2((ScreenWidth / 2) - (size.X / 2),
                    (ScreenHeight / 2) - (size.Y * 2));

                spriteBatch.DrawString(myFont, text, position2, Color.White);

                text = "PRESS <ENTER> TO START";
                size = myFont.MeasureString(text);

                position2 = new Vector2((ScreenWidth / 2) - (size.X / 2),
                    (ScreenHeight / 2) + (size.Y * 2));

                spriteBatch.DrawString(myFont, text, position2, Color.White);

                spriteBatch.End();

                return;
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Vector2 position = new Vector2(10, 10);

            //score
            spriteBatch.DrawString(myFont,
                "Score = " + score.ToString(),
                position,
                Color.White);

            Rectangle shipRect;

            for (int i = 0; i < lives; i++)
            {
                shipRect = new Rectangle(i * ship.Width + 10,
                       40,
                       ship.Width,
                       ship.Height);

                spriteBatch.Draw(ship.Texture, shipRect, Color.White);
            }

            spriteBatch.Draw(ship.Texture,
                    ship.Position,
                    null,
                    Color.White,
                    ship.Rotation,
                    ship.Center,
                    ship.Scale,
                    SpriteEffects.None,
                    1.0f);

            foreach (Sprite b in bullets)
            {
                if (b.Alive)
                {
                    spriteBatch.Draw(b.Texture,
                        b.Position,
                        null,
                        Color.White,
                        b.Rotation,
                        b.Center,
                        b.Scale,
                        SpriteEffects.None,
                        1.0f);
                }
            }
            
            foreach (Sprite a in asteroids)
            {

                if (a.Alive)
                {
                    spriteBatch.Draw(a.Texture,
                        a.Position,
                        null,
                        Color.Red,
                        a.Rotation,
                        a.Center,
                        a.Scale,
                        SpriteEffects.None,
                        1.0f);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}