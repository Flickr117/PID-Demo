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
            Application.Run(new MainForm());
        }
    }

    public partial class MainForm : Form
    {
        private double Kp = 0.0;
        private double Ki = 0.0;
        private double Kd = 0.0;

        private PointF circlePosition;
        private PointF targetPosition;
        private float circleRadius = 50; // Blue circle radius
        private float targetRadius = 10; // Red circle radius

        private double integralX = 0.0;
        private double integralY = 0.0;
        private double previousErrorX = 0.0;
        private double previousErrorY = 0.0;
        private double previousDerivativeX = 0.0;
        private double previousDerivativeY = 0.0;

        private bool isDragging = false;

        private Label kpLabel;
        private Label kiLabel;
        private Label kdLabel;
        private TextBox kpTextBox;
        private TextBox kiTextBox;
        private TextBox kdTextBox;
        private Label hintLabel;

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            circlePosition = new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
            targetPosition = new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2);

            kpLabel = new Label { Location = new Point(this.ClientSize.Width - 200, 10), Text = "Proportional (Kp):", AutoSize = true };
            kpTextBox = new TextBox { Location = new Point(this.ClientSize.Width - 100, 10), Text = Kp.ToString() };

            kiLabel = new Label { Location = new Point(this.ClientSize.Width - 200, 40), Text = "Integral (Ki):", AutoSize = true };
            kiTextBox = new TextBox { Location = new Point(this.ClientSize.Width - 100, 40), Text = Ki.ToString() };

            kdLabel = new Label { Location = new Point(this.ClientSize.Width - 200, 70), Text = "Derivative (Kd):", AutoSize = true };
            kdTextBox = new TextBox { Location = new Point(this.ClientSize.Width - 100, 70), Text = Kd.ToString() };

            hintLabel = new Label
            {
                Location = new Point(10, this.ClientSize.Height - 100),
                Text = "Higher Kp = Faster response, but might overshoot.\n" +
                       "Lower Kp = Slower response, but no overshooting.\n" +
                       "Higher Ki = Fixes small errors, but may overcorrect.\n" +
                       "Lower Ki = Leaves small errors, but smoother behavior.\n" +
                       "Higher Kd: Smooths out movements by dampening rapid changes, making the system slower but more stable.\n" +
                       "Lower Kd: Leads to less damping, making the system respond faster but bouncier or more oscillatory.",
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

            this.Paint += new PaintEventHandler(Form_Paint);
            this.MouseDown += new MouseEventHandler(Form_MouseDown);
            this.MouseUp += new MouseEventHandler(Form_MouseUp);
            this.MouseMove += new MouseEventHandler(Form_MouseMove);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "MainForm";
            this.ResumeLayout(false);
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

            double alpha = 0.1;
            double filteredDerivativeX = alpha * (errorX - previousErrorX) + (1 - alpha) * previousDerivativeX;
            double filteredDerivativeY = alpha * (errorY - previousErrorY) + (1 - alpha) * previousDerivativeY;

            double controlX = Kp * errorX + Ki * integralX + Kd * filteredDerivativeX;
            double controlY = Kp * errorY + Ki * integralY + Kd * filteredDerivativeY;

            int moveX = (int)Math.Round(controlX);
            int moveY = (int)Math.Round(controlY);

            circlePosition.X = Math.Max(0, Math.Min(this.ClientSize.Width - circleRadius * 2, circlePosition.X + moveX));
            circlePosition.Y = Math.Max(0, Math.Min(this.ClientSize.Height - circleRadius * 2, circlePosition.Y + moveY));

            previousErrorX = errorX;
            previousErrorY = errorY;
            previousDerivativeX = filteredDerivativeX;
            previousDerivativeY = filteredDerivativeY;

            this.Invalidate();
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
                }
            }
        }

        private void Form_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                circlePosition = new PointF(e.X, e.Y);
                this.Invalidate();
            }
        }
    }
}
