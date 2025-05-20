namespace ProyectoBattleship
{
    public partial class Tablero : Form
    {
        public Tablero()
        {
            InitializeComponent();
            GenerarTableroAliado();
            GenerarTableroEnemigo();
        }

        void GenerarTableroAliado()
        {
            Image imagenFondo = Image.FromFile("resources/SeaTexture.png");

            int filas = 10, columnas = 10;
            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Size = new Size(500, 500),
                Location = new Point(100, 100),
                BackgroundImage = imagenFondo,
                BackgroundImageLayout = ImageLayout.Stretch,
                Margin = new Padding(0)
            };

            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    Button button = CrearBoton($"btnA_{i}_{j}", Color.DarkCyan);
                    flowLayoutPanel.Controls.Add(button);
                }
            }
            this.Controls.Add(flowLayoutPanel);
        }

        void GenerarTableroEnemigo()
        {
            Image imagenHover = Image.FromFile("resources/Target.png");

            int filas = 10, columnas = 10;
            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Size = new Size(500, 500),
                BackColor = Color.Black,
                Location = new Point(700, 100),
                Margin = new Padding(0)
            };

            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    Button button = CrearBoton($"btnE_{i}_{j}", Color.LimeGreen);
                    button.MouseEnter += (sender, e) =>
                    {
                        ((Button)sender).BackgroundImage = imagenHover;
                        ((Button)sender).BackgroundImageLayout = ImageLayout.Stretch;
                    };

                    button.MouseLeave += (sender, e) =>
                    {
                        ((Button)sender).BackgroundImage = null;
                    };
                    flowLayoutPanel.Controls.Add(button);
                }
            }
            this.Controls.Add(flowLayoutPanel);
        }

        Button CrearBoton(string nombre, Color borderColor)
        {
            return new Button
            {
                Size = new Size(50, 50),
                Margin = new Padding(0),
                Name = nombre,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                FlatAppearance =
        {
            BorderSize = 1,
            BorderColor = borderColor,
            MouseDownBackColor = Color.Transparent,
            MouseOverBackColor = Color.Transparent
        },
                UseVisualStyleBackColor = false
            };
        }
    }
}
