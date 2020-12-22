using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Поле на котором живет змея
        Entity field;
        // голова змеи
        Head head;
        // вся змея
        List<PositionedEntity> snake;
        // яблоко
        Apple apple;
        // супер яблоко
        SuperApple superApple;
        //количество очков
        int score;
        //таймер по которому 
        DispatcherTimer moveTimer;
        // progress bar timer
        DispatcherTimer progressBarTimer;
        int appleCount = 0;
        bool superAppleOnScreen = false;
        bool gameStarted = false;

        //конструктор формы, выполняется при запуске программы
        public MainWindow()
        {
            InitializeComponent();

            snake = new List<PositionedEntity>();
            //создаем поле 300х300 пикселей
            field = new Entity(600, 600, "pack://application:,,,/Resources/snake.png");

            //создаем таймер срабатывающий раз в 400 мс
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            moveTimer.Tick += new EventHandler(moveTimer_Tick);

            //создаем таймер срабатывающий раз в 100 мс
            progressBarTimer = new DispatcherTimer();
            progressBarTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            progressBarTimer.Tick += new EventHandler(progressBarTimer_Tick);
        }

        //метод перерисовывающий экран
        private void UpdateField()
        {
            //обновляем положение элементов змеи
            foreach (var p in snake)
            {
                Canvas.SetTop(p.image, p.y);
                Canvas.SetLeft(p.image, p.x);
            }

            if (!superAppleOnScreen)
            {
                //обновляем положение яблока
                Canvas.SetTop(apple.image, apple.y);
                Canvas.SetLeft(apple.image, apple.x);
            }
            else
            {
                //обновляем положение супер яблока
                Canvas.SetTop(superApple.image, superApple.y);
                Canvas.SetLeft(superApple.image, superApple.x);
            }

            //обновляем количество очков
            lblScore.Content = String.Format("{0}000", score);
        }

        //обработчик тика таймера. Все движение происходит здесь
        void moveTimer_Tick(object sender, EventArgs e)
        {
            //в обратном порядке двигаем все элементы змеи
            foreach (var p in Enumerable.Reverse(snake))
            {
                p.move();
            }

            //проверяем, что голова змеи не врезалась в тело
            foreach (var p in snake.Where(x => x != head))
            {
                //если координаты головы и какой либо из частей тела совпадают
                if (p.x == head.x && p.y == head.y)
                {
                    if (superAppleOnScreen)
                    {
                        progressBarTimer.Stop();
                        Score_Multiplier.Content = "x 1";
                        superAppleOnScreen = false;
                        superApple.remove();
                        canvas1.Children.Remove(superApple.image);
                        canvas1.Children.Add(apple.image);
                        apple.move();
                        progressBar.Value = 100;
                    }
                    appleCount = 0;
                    //мы проиграли
                    moveTimer.Stop();
                    tbGameOver.Visibility = Visibility.Visible;
                    return;
                }
            }

            //проверяем, что голова змеи не вышла за пределы поля
            if (head.x < 40 || head.x >= 540 || head.y < 40 || head.y >= 540)
            {
                //мы проиграли
                if (superAppleOnScreen)
                {
                    progressBarTimer.Stop();
                    Score_Multiplier.Content = "x 1";
                    superAppleOnScreen = false;
                    superApple.remove();
                    canvas1.Children.Remove(superApple.image);
                    canvas1.Children.Add(apple.image);
                    apple.move();
                    progressBar.Value = 100;
                }
                appleCount = 0;
                moveTimer.Stop();
                tbGameOver.Visibility = Visibility.Visible;
                return;
            }

            if (!superAppleOnScreen)
            {
                //проверяем, что голова змеи врезалась в яблоко
                if (head.x == apple.x && head.y == apple.y)
                {
                    //увеличиваем счет
                    score++;
                    appleCount++;
                    if (appleCount == 4) 
                    {
                        appleCount = 0;
                        Random rnd = new Random();
                        if (rnd.Next(0, 2) == 1) // шанс выпадения супер яблока
                        {
                            apple.remove();
                            canvas1.Children.Add(superApple.image);
                            canvas1.Children.Remove(apple.image);
                            progressBarTimer.Start();
                            Score_Multiplier.Content = "x 10";
                            superAppleOnScreen = true;
                            superApple.move();
                            progressBar.Value = 100;
                        }
                        else { apple.move(); }
                    }
                    else { apple.move(); }
                    // добавляем новый сегмент к змее
                    var part = new BodyPart(snake.Last());
                    canvas1.Children.Add(part.image);
                    snake.Add(part);
                }
            }
            else
            {
                if (head.x == superApple.x && head.y == superApple.y)
                {
                    progressBarTimer.Stop();
                    Score_Multiplier.Content = "x 1";
                    progressBar.Value = 100;
                    //увеличиваем счет
                    score += 10;
                    appleCount = 0;
                    superAppleOnScreen = false;
                    superApple.remove();
                    canvas1.Children.Remove(superApple.image);
                    canvas1.Children.Add(apple.image);
                    apple.move();
                }
            }
            //перерисовываем экран
            UpdateField();
        }

        void progressBarTimer_Tick(object sender, EventArgs e)
        {
            progressBar.Value-=2;
            if (progressBar.Value == 0)
            {
                Score_Multiplier.Content = "x 1";
                progressBarTimer.Stop();
                progressBar.Value = 100;
                appleCount = 0;
                superAppleOnScreen = false;
                superApple.remove();
                canvas1.Children.Remove(superApple.image);
                canvas1.Children.Add(apple.image);
                apple.move();
                UpdateField();
            }
        }

            // Обработчик нажатия на кнопку клавиатуры
            private void Window_KeyDown(object sender, KeyEventArgs e)
            {
            if (gameStarted)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        head.direction = Head.Direction.UP;
                        break;
                    case Key.Down:
                        head.direction = Head.Direction.DOWN;
                        break;
                    case Key.Left:
                        head.direction = Head.Direction.LEFT;
                        break;
                    case Key.Right:
                        head.direction = Head.Direction.RIGHT;
                        break;
                }
            }
            if (e.Key == Key.R)
            {
                button1_Click(sender, e);
            }
            }

        // Обработчик нажатия кнопки "Start"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (superAppleOnScreen)
            {
                progressBarTimer.Stop();
                Score_Multiplier.Content = "x 1";
                progressBar.Value = 100;
                appleCount = 0;
                superAppleOnScreen = false;
                superApple.remove();
                canvas1.Children.Remove(superApple.image);
                canvas1.Children.Add(apple.image);
            }
            button1.Visibility = Visibility.Hidden;
            canvas1.Visibility = Visibility.Visible;
            progressBar.Visibility = Visibility.Visible;
            label1.Visibility = Visibility.Visible;
            lblScore.Visibility = Visibility.Visible;
            Score_Multiplier.Visibility = Visibility.Visible;
            gameStarted = true;
            appleCount = 0;
            superAppleOnScreen = false;
            // обнуляем счет
            score = 0;
            // обнуляем змею
            snake.Clear();
            // очищаем канвас
            canvas1.Children.Clear();
            // скрываем надпись "Game Over"
            tbGameOver.Visibility = Visibility.Hidden;

            // добавляем поле на канвас
            canvas1.Children.Add(field.image);
            // создаем новое яблоко и добавлем его
            apple = new Apple(snake);
            canvas1.Children.Add(apple.image);
            // создаем новое супер яблоко
            superApple = new SuperApple(snake);
            // создаем голову
            head = new Head();
            snake.Add(head);
            canvas1.Children.Add(head.image);

            //запускаем таймер
            moveTimer.Start();
            UpdateField();

        }

        public class Entity
        {
            protected int m_width;
            protected int m_height;

            Image m_image;
            public Entity(int w, int h, string image)
            {
                m_width = w;
                m_height = h;
                m_image = new Image();
                m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
                m_image.Width = w;
                m_image.Height = h;

            }

            public Image image
            {
                get
                {
                    return m_image;
                }
            }
        }

        public class PositionedEntity : Entity
        {
            protected int m_x;
            protected int m_y;
            public PositionedEntity(int x, int y, int w, int h, string image)
                : base(w, h, image)
            {
                m_x = x;
                m_y = y;
            }

            public virtual void move() { }

            public int x
            {
                get
                {
                    return m_x;
                }
                set
                {
                    m_x = value;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
                set
                {
                    m_y = value;
                }
            }
        }

        public class Apple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Apple(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/apple.png")
            {
                m_snake = s;
                move();
            }
            public void remove()
            {
                x = -40;
                y = -40;
            }
            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class SuperApple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public SuperApple(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/fruit.png")
            {
                m_snake = s;
                move();
            }
            public void remove()
            {
                x = -40;
                y = -40;
            }
            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Head : PositionedEntity
        {
            public enum Direction
            {
                RIGHT, DOWN, LEFT, UP, NONE
            };

            Direction m_direction;

            public Direction direction
            {
                set
                {
                    m_direction = value;
                    RotateTransform rotateTransform = new RotateTransform(90 * (int)value);
                    image.RenderTransform = rotateTransform;
                }
            }

            public Head()
                : base(280, 280, 40, 40, "pack://application:,,,/Resources/head.png")
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                m_direction = Direction.NONE;
            }

            public override void move()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y += 40;
                        break;
                    case Direction.UP:
                        y -= 40;
                        break;
                    case Direction.LEFT:
                        x -= 40;
                        break;
                    case Direction.RIGHT:
                        x += 40;
                        break;
                }
            }
        }

        public class BodyPart : PositionedEntity
        {
            PositionedEntity m_next;
            public BodyPart(PositionedEntity next)
                : base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/body.png")
            {
                m_next = next;
            }

            public override void move()
            {
                x = m_next.x;
                y = m_next.y;
            }
        }
    }
}
