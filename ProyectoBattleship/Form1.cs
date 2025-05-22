namespace ProyectoBattleship
{
    public partial class Tablero : Form
    {
        // Variables para colocar barcos
        private int barcoActual = 0;
        private int[] barcos = new int[] { 5, 4, 3, 3, 2 };
        private string direccion = "horizontal";
        private bool[,] tableroAliado = new bool[10, 10];
        private Button[,] botonesAliados = new Button[10, 10];

        public Tablero()
        {
            InitializeComponent();
            GenerarTableroAliado();
            GenerarTableroEnemigo();
        }

        void GenerarTableroAliado()
        {
            int filas = 10, columnas = 10;
            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
            {
                Size = new Size(400, 400),
                BackColor = Color.Black,
                Location = new Point(80, 500),
                Margin = new Padding(0)
            };

            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    int fi = i;
                    int fj = j;

                    Button button = CrearBoton($"btnA_{fi}_{fj}");
                    button.Tag = (fi, fj);

                    button.MouseEnter += (sender, e) =>
                    {
                        if (barcoActual >= barcos.Length) return;

                        int tamaño = barcos[barcoActual];
                        if (CabeElBarco(fi, fj, tamaño, direccion))
                        {
                            for (int k = 0; k < tamaño; k++)
                            {
                                int f = fi + (direccion == "vertical" ? k : 0);
                                int c = fj + (direccion == "horizontal" ? k : 0);

                                if (!tableroAliado[f, c])
                                    botonesAliados[f, c].BackColor = Color.LightGreen;
                            }
                        }
                    };

                    button.MouseLeave += (sender, e) =>
                    {
                        if (barcoActual >= barcos.Length) return;

                        int tamaño = barcos[barcoActual];
                        if (CabeElBarco(fi, fj, tamaño, direccion))
                        {
                            for (int k = 0; k < tamaño; k++)
                            {
                                int f = fi + (direccion == "vertical" ? k : 0);
                                int c = fj + (direccion == "horizontal" ? k : 0);

                                if (!tableroAliado[f, c])
                                    botonesAliados[f, c].BackColor = Color.Black;
                            }
                        }
                    };

                    button.Click += ColocarBarcos_Click;

                    flowLayoutPanel.Controls.Add(button);
                    botonesAliados[fi, fj] = button;
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
                Size = new Size(400, 400),
                BackColor = Color.Black,
                Location = new Point(80, 80),
                Margin = new Padding(0)
            };

            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    Button button = CrearBoton($"btnE_{i}_{j}");
                    button.Tag = (i, j);
                    button.Click += ButtonEnemigo_Click;

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

        Button CrearBoton(string nombre)
        {
            return new Button
            {
                Size = new Size(40, 40),
                Margin = new Padding(0),
                Name = nombre,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Black,
                FlatAppearance =
                {
                    BorderSize = 1,
                    BorderColor = Color.DarkGreen,
                },
                UseVisualStyleBackColor = false
            };
        }

        private void ButtonEnemigo_Click(object sender, EventArgs e)
        {
            Button boton = sender as Button;
            var (fila, columna) = ((int, int))boton.Tag;

            MessageBox.Show($"Disparo en ({fila}, {columna})");

            boton.BackColor = Color.Red;
            boton.Enabled = false;
        }

        private void ColocarBarcos_Click(object sender, EventArgs e)
        {
            if (barcoActual >= barcos.Length)
            {
                MessageBox.Show("¡Ya colocaste todos los barcos!");
                return;
            }

            Button boton = sender as Button;
            var (fila, columna) = ((int, int))boton.Tag;
            int tamaño = barcos[barcoActual];

            if (!CabeElBarco(fila, columna, tamaño, direccion))
            {
                MessageBox.Show("No se puede colocar aquí.");
                return;
            }

            for (int k = 0; k < tamaño; k++)
            {
                int f = fila + (direccion == "vertical" ? k : 0);
                int c = columna + (direccion == "horizontal" ? k : 0);

                tableroAliado[f, c] = true;
                botonesAliados[f, c].BackColor = Color.LimeGreen;
                botonesAliados[f, c].Enabled = false;
            }

            barcoActual++;
        }

        private bool CabeElBarco(int fila, int columna, int tamaño, string direccion)
        {
            for (int k = 0; k < tamaño; k++)
            {
                int f = fila + (direccion == "vertical" ? k : 0);
                int c = columna + (direccion == "horizontal" ? k : 0);

                if (f >= 10 || c >= 10 || tableroAliado[f, c]) return false;
            }
            return true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.R)
            {
                direccion = direccion == "horizontal" ? "vertical" : "horizontal";
                MessageBox.Show($"Dirección actual: {direccion}");
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
