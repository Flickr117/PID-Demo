using System;
using System.Drawing;
using System.Windows.Forms;

namespace pidDemo
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CircleMoverForm());
        }
    }

    public class CircleMoverForm : Form
    {
        private bool isDragging = false;
        private Point circlePosition = new Point(100, 100);
        private int circleRadius = 30;
        private Point targetPosition;
        private int targetRadius = 10;

        private double Kp = 0.0;
        private double Ki = 0.0;
        private double Kd = 0.0;

        private double integralX = 0.0;
        private double integralY = 0.0;
        private double previousErrorX = 0.0;
        private double previousErrorY = 0.0;

        private TextBox kpTextBox;
        private TextBox kiTextBox;
        private TextBox kdTextBox;

        public CircleMoverForm()
        {
            this.Text = "Circle Mover with PID";
            this.ClientSize = new Size(800, 600);
            this.DoubleBuffered = true;

            targetPosition = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);

            this.MouseDown += new MouseEventHandler(Form_MouseDown);
            this.MouseMove += new MouseEventHandler(Form_MouseMove);
            this.MouseUp += new MouseEventHandler(Form_MouseUp);
            this.Paint += new PaintEventHandler(Form_Paint);

            Label kpLabel = new Label { Location = new Point(this.ClientSize.Width - 200, 10), Text = "Proportional (Kp):", AutoSize = true };
            kpTextBox = new TextBox { Location = new Point(this.ClientSize.Width - 100, 10), Text = Kp.ToString() };

            Label kiLabel = new Label { Location = new Point(this.ClientSize.Width - 200, 40), Text = "Integral (Ki):", AutoSize = true };
            kiTextBox = new TextBox { Location = new Point(this.ClientSize.Width - 100, 40), Text = Ki.ToString() };

            Label kdLabel = new Label { Location = new Point(this.ClientSize.Width - 200, 70), Text = "Derivative (Kd):", AutoSize = true };
            kdTextBox = new TextBox { Location = new Point(this.ClientSize.Width - 100, 70), Text = Kd.ToString() };

            Label hintLabel = new Label
            {
                Location = new Point(10, this.ClientSize.Height - 100),
                Text = "Higher Kp = Faster response, but might overshoot.\n" +
                       "Lower Kp = Slower response, but no overshooting.\n" +
                       "Higher Ki = Fixes small errors, but may overcorrect.\n" +
                       "Lower Ki = Leaves small errors, but smoother behavior.\n" +
                       "Higher Kd = Smooths out the movement, but may slow down the system.\n" +
                       "Lower Kd = Faster corrections, but may overshoot.",
                AutoSize = true
            };

            this.Controls.Add(kpLabel);
            this.Controls.Add(kiLabel);
            this.Controls.Add(kdLabel);
            this.Controls.Add(kpTextBox);
            this.Controls.Add(kiTextBox);
            this.Controls.Add(kdTextBox);
            this.Controls.Add(hintLabel);

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 20; // 50 updates per second
            timer.Tick += new EventHandler(UpdatePosition);
            timer.Start();
        }

        private void Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double distance = Math.Sqrt(Math.Pow(e.X - circlePosition.X, 2) + Math.Pow(e.Y - circlePosition.Y, 2));
                if (distance <= circleRadius)
                {
                    isDragging = true;
                    integralX = 0.0; // Reset integral component
                    integralY = 0.0; // Reset integral component
                    previousErrorX = 0.0; // Reset derivative component
                    previousErrorY = 0.0; // Reset derivative component
                }
            }
        }

        private void Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                circlePosition = e.Location;
                this.Invalidate();
            }
        }

        private void Form_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void Form_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.FillEllipse(Brushes.Blue, circlePosition.X - circleRadius, circlePosition.Y - circleRadius, circleRadius * 2, circleRadius * 2);
            e.Graphics.FillEllipse(Brushes.Red, targetPosition.X - targetRadius, targetPosition.Y - targetRadius, targetRadius * 2, targetRadius * 2);
        }

        private void UpdatePosition(object? sender, EventArgs e)
        {
            if (isDragging)
                return; // Do NOT apply PID control if the ball is being dragged

            if (double.TryParse(kpTextBox.Text, out double kp))
                Kp = kp;
            if (double.TryParse(kiTextBox.Text, out double ki))
                Ki = ki;
            if (double.TryParse(kdTextBox.Text, out double kd))
                Kd = kd;

            double errorX = targetPosition.X - circlePosition.X;
            double errorY = targetPosition.Y - circlePosition.Y;

            integralX += errorX;
            integralY += errorY;

            integralX = Math.Max(Math.Min(integralX, 100), -100);
            integralY = Math.Max(Math.Min(integralY, 100), -100);

            double derivativeX = errorX - previousErrorX;
            double derivativeY = errorY - previousErrorY;

            double controlX = Kp * errorX + Ki * integralX + Kd * derivativeX;
            double controlY = Kp * errorY + Ki * integralY + Kd * derivativeY;

            int moveX = (int)Math.Round(controlX);
            int moveY = (int)Math.Round(controlY);

            circlePosition.X = Math.Max(0, Math.Min(this.ClientSize.Width - circleRadius * 2, circlePosition.X + moveX));
            circlePosition.Y = Math.Max(0, Math.Min(this.ClientSize.Height - circleRadius * 2, circlePosition.Y + moveY));

            previousErrorX = errorX;
            previousErrorY = errorY;

            this.Invalidate(); 
        }
    }
}
