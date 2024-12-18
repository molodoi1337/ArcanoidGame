using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArcanoidGame
{
    public partial class Form1 : Form
    {
        // Элементы игры
        private Label labelScore;
        private Label labelLives;
        private Panel racket;
        private Panel ball;
        private Timer gameTimer;
        private int ballSpeedX, ballSpeedY;
        private int score = 0;
        private int lives = 3;
        private bool isGamePaused = false;

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Настройка формы
            this.Text = "Арканоид";
            this.ClientSize = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.KeyPreview = true; // Для перехвата нажатий клавиш

            InitializeComponents();

            // Создание кирпичей
            CreateBricks();

            // Настройка таймера игры
            gameTimer = new Timer();
            gameTimer.Interval = 16; // 60 кадров в секунду
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            // Настройка начальных значений скорости мяча
            Random rand = new Random();
            ballSpeedX = rand.Next(2, 6) * (rand.Next(2) == 0 ? 1 : -1);
            ballSpeedY = -rand.Next(3, 6);
        }

        private void InitializeComponents()
        {
            // Счет
            labelScore = new Label();
            labelScore.Text = "Счет: 0";
            labelScore.ForeColor = Color.White;
            labelScore.Font = new Font("Arial", 14);
            labelScore.Location = new Point(10, 10);
            this.Controls.Add(labelScore);

            // Количество жизней
            labelLives = new Label();
            labelLives.Text = "Жизни: 3";
            labelLives.ForeColor = Color.White;
            labelLives.Font = new Font("Arial", 14);
            labelLives.Location = new Point(700, 10);
            this.Controls.Add(labelLives);

            // Ракетка
            racket = new Panel();
            racket.Size = new Size(100, 20);
            racket.BackColor = Color.White;
            racket.Location = new Point(350, 550);
            this.Controls.Add(racket);

            // Мяч
            ball = new Panel();
            ball.Size = new Size(20, 20);
            ball.BackColor = Color.Red;
            ball.Location = new Point(390, 530);
            this.Controls.Add(ball);

            // Настройка контуров мяча на круглый
            System.Drawing.Drawing2D.GraphicsPath path =
                new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(ball.ClientRectangle);
            ball.Region = new Region(path);
        }

        // Создание кирпичей на форме
        private void CreateBricks()
        {
            int brickWidth = 60;
            int brickHeight = 20;
            int padding = 5;
            int numberOfColumns = 10;
            int numberOfRows = 4;
            Color[] rowColors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green };

            for (int row = 0; row < numberOfRows; row++)
            {
                for (int col = 0; col < numberOfColumns; col++)
                {
                    Panel brick = new Panel();
                    brick.Size = new Size(brickWidth, brickHeight);
                    brick.Location = new Point(col * (brickWidth + padding), row * (brickHeight + padding) + 50);
                    brick.BackColor = rowColors[row];
                    this.Controls.Add(brick);
                }
            }
        }

        // Обработчик таймера для движения мяча и столкновений
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Изменение положения мяча
            ball.Location = new Point(ball.Left + ballSpeedX, ball.Top + ballSpeedY);

            // Проверка на столкновение с границами экрана
            if (ball.Left <= 0 || ball.Right >= this.ClientSize.Width)
            {
                ballSpeedX = -ballSpeedX; // Отскок от стенки
            }
            if (ball.Top <= 0)
            {
                ballSpeedY = -ballSpeedY; // Отскок от верхней границы
            }
            if (ball.Bottom >= this.ClientSize.Height)
            {
                // Потеря мяча

                lives--;
                labelLives.Text = "Жизни: " + lives;
                if (lives == 0)
                {
                    PauseGame();
                    MessageBox.Show("Игра окончена!");
                    this.Close(); // Закрытие игры
                }
                else
                {
                    ResetBallPosition();
                }
            }

            // Столкновение с ракеткой
            if (ball.Bounds.IntersectsWith(racket.Bounds) && ballSpeedY > 0)
            {
                ballSpeedY = -ballSpeedY; // Отскок от ракетки
            }

            // Проверка столкновений с кирпичами
            CheckBallCollisionWithBricks();
        }

        // Сброс позиции мяча после потери
        private void ResetBallPosition()
        {
            Random rand = new Random();
            ball.Location = new Point(rand.Next(0, this.ClientSize.Width - ball.Width), this.ClientSize.Height - 50);
            ballSpeedX = rand.Next(2, 6) * (rand.Next(2) == 0 ? 1 : -1);
            ballSpeedY = -rand.Next(3, 6);
            gameTimer.Start();
        }

        // Проверка столкновений мяча с кирпичами
        private void CheckBallCollisionWithBricks()
        {
            Rectangle ballRectangle = ball.Bounds;

            for (int i = this.Controls.Count - 1; i >= 0; i--)
            {
                Control item = this.Controls[i];
                if (item is Panel && item != ball && item != racket)
                {
                    if (ballRectangle.IntersectsWith(item.Bounds))
                    {
                        // Столкновение с кирпичом
                        ballRectangle.Intersect(item.Bounds);
                        if (ballRectangle.Height < ballRectangle.Width) // Столкновение с верхней или нижней стороной
                        {
                            ballSpeedY = -ballSpeedY; // Меняем направление по Y
                        }
                        else
                        {
                            ballSpeedX = -ballSpeedX; // Меняем направление по X
                        }

                        // Удаление кирпича и начисление очков
                        this.Controls.Remove(item);
                        score += 10; // Добавить очки
                        labelScore.Text = "Счет: " + score;

                        //Моментик!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Проверка на победу(Если не работает,то по scor'у сделать)
                        if (this.Controls.Count == 4) // Только мяч и ракетка остались
                        {
                            MessageBox.Show("Победа! Все кирпичи уничтожены.");
                        }

                        break; // Прерывание, т.к. мяч может только с одним кирпичом столкнуться за раз
                    }
                }
            }
        }

        // Обработчик нажатия клавиши
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space)
            {
                if (isGamePaused)
                {
                    StartGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        // Старт игры
        private void StartGame()
        {
            isGamePaused = false;
            gameTimer.Start();
            labelLives.Text = "Жизни: " + lives;
            labelScore.Text = "Счет: " + score;
        }

        // Пауза игры
        private void PauseGame()
        {
            isGamePaused = true;
            gameTimer.Stop();
        }

        // Движение ракетки
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Центр ракетки
            int racketCenterX = racket.Width / 2;
            if (e.X > racketCenterX && e.X < this.ClientSize.Width - racketCenterX)
            {
                racket.Location = new Point(e.X - racketCenterX, racket.Top);
            }
        }
    }
}
