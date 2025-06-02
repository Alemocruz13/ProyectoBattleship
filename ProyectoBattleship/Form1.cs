namespace ProyectoBattleship
{
    public partial class Tablero : Form
    {
        private int indiceBarcoActual = 0;
        private int[] tamaniosBarcos = new int[] { 5, 4, 3, 3, 2 };
        private string orientacionBarco = "horizontal";
        private bool[,] celdasOcupadasAliado = new bool[10, 10];
        private Button[,] botonesTableroAliado = new Button[10, 10];

        private int? ultimaFilaHover = null;
        private int? ultimaColumnaHover = null;

        bool[,] tableroEnemigo = new bool[10, 10];
        Random random = new Random();

        public Tablero()
        {
            InitializeComponent();
            GenerarTableroAliado();
            GenerarTableroEnemigo();
            ColocarBarcosEnemigo();
        }

        void GenerarTableroAliado()
        {
            int filas = 10, columnas = 10;
            FlowLayoutPanel panelAliado = new FlowLayoutPanel
            {
                Size = new Size(400, 400),
                BackColor = Color.Black,
                Location = new Point(80, 500),
                Margin = new Padding(0)
            };

            for (int fila = 0; fila < filas; fila++)
            {
                for (int columna = 0; columna < columnas; columna++)
                {
                    int filaActual = fila;
                    int columnaActual = columna;

                    Button boton = CrearBoton($"btnA_{filaActual}_{columnaActual}");
                    boton.Tag = (filaActual, columnaActual);

                    boton.MouseEnter += (sender, e) =>
                    {
                        if (indiceBarcoActual >= tamaniosBarcos.Length) return;

                        ultimaFilaHover = filaActual;
                        ultimaColumnaHover = columnaActual;

                        int tamanio = tamaniosBarcos[indiceBarcoActual];
                        if (CabeElBarco(filaActual, columnaActual, tamanio, orientacionBarco))
                        {
                            for (int k = 0; k < tamanio; k++)
                            {
                                int f = filaActual + (orientacionBarco == "vertical" ? k : 0);
                                int c = columnaActual + (orientacionBarco == "horizontal" ? k : 0);

                                if (!celdasOcupadasAliado[f, c])
                                    botonesTableroAliado[f, c].BackColor = Color.LightGreen;
                            }
                        }
                    };

                    boton.MouseLeave += (sender, e) =>
                    {
                        if (indiceBarcoActual >= tamaniosBarcos.Length) return;

                        int tamanio = tamaniosBarcos[indiceBarcoActual];
                        if (CabeElBarco(filaActual, columnaActual, tamanio, orientacionBarco))
                        {
                            for (int k = 0; k < tamanio; k++)
                            {
                                int f = filaActual + (orientacionBarco == "vertical" ? k : 0);
                                int c = columnaActual + (orientacionBarco == "horizontal" ? k : 0);

                                if (!celdasOcupadasAliado[f, c])
                                    botonesTableroAliado[f, c].BackColor = Color.Black;
                            }
                        }

                        ultimaFilaHover = null;
                        ultimaColumnaHover = null;
                    };

                    boton.Click += ColocarBarco_Click;

                    panelAliado.Controls.Add(boton);
                    botonesTableroAliado[filaActual, columnaActual] = boton;
                }
            }
            this.Controls.Add(panelAliado);
        }

        void GenerarTableroEnemigo()
        {
            Image imagenHover = Image.FromFile("resources/Target.png");

            int filas = 10, columnas = 10;
            FlowLayoutPanel panelEnemigo = new FlowLayoutPanel
            {
                Size = new Size(400, 400),
                BackColor = Color.Black,
                Location = new Point(80, 80),
                Margin = new Padding(0)
            };

            for (int fila = 0; fila < filas; fila++)
            {
                for (int columna = 0; columna < columnas; columna++)
                {
                    Button boton = CrearBoton($"btnE_{fila}_{columna}");
                    boton.Tag = (fila, columna);
                    boton.Click += DispararEnemigo_Click;

                    boton.MouseEnter += (sender, e) =>
                    {
                        ((Button)sender).BackgroundImage = imagenHover;
                        ((Button)sender).BackgroundImageLayout = ImageLayout.Stretch;
                    };

                    boton.MouseLeave += (sender, e) =>
                    {
                        ((Button)sender).BackgroundImage = null;
                    };

                    panelEnemigo.Controls.Add(boton);
                }
            }
            this.Controls.Add(panelEnemigo);
        }
        private void ColocarBarcosEnemigo()
        {
            int[] barcos = new int[] { 5, 4, 3, 3, 2 };

            foreach (int tama�o in barcos)
            {
                bool colocado = false;
                while (!colocado)
                {
                    int fila = random.Next(0, 10);
                    int columna = random.Next(0, 10);
                    string direccion = random.Next(0, 2) == 0 ? "horizontal" : "vertical";

                    if (CabeElBarcoEnemigo(fila, columna, tama�o, direccion))
                    {
                        for (int k = 0; k < tama�o; k++)
                        {
                            int f = fila + (direccion == "vertical" ? k : 0);
                            int c = columna + (direccion == "horizontal" ? k : 0);
                            tableroEnemigo[f, c] = true;
                        }
                        colocado = true;
                    }
                }
            }
        }

        private bool CabeElBarcoEnemigo(int fila, int columna, int tama�o, string direccion)
        {
            for (int k = 0; k < tama�o; k++)
            {
                int f = fila + (direccion == "vertical" ? k : 0);
                int c = columna + (direccion == "horizontal" ? k : 0);

                if (f >= 10 || c >= 10 || tableroEnemigo[f, c])
                    return false;
            }
            return true;
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

        private void DispararEnemigo_Click(object sender, EventArgs e)
        {
            Button boton = sender as Button;
            var (fila, columna) = ((int, int))boton.Tag;

            MessageBox.Show($"Disparo en ({fila}, {columna})");

            boton.BackColor = Color.Red;
            boton.Enabled = false;
        }

        private void ColocarBarco_Click(object sender, EventArgs e)
        {
            if (indiceBarcoActual >= tamaniosBarcos.Length)
            {
                MessageBox.Show("�Ya colocaste todos los barcos!");
                return;
            }

            Button boton = sender as Button;
            var (fila, columna) = ((int, int))boton.Tag;
            int tamanio = tamaniosBarcos[indiceBarcoActual];

            if (!CabeElBarco(fila, columna, tamanio, orientacionBarco))
            {
                MessageBox.Show("No se puede colocar aqu�.");
                return;
            }

            for (int k = 0; k < tamanio; k++)
            {
                int f = fila + (orientacionBarco == "vertical" ? k : 0);
                int c = columna + (orientacionBarco == "horizontal" ? k : 0);

                celdasOcupadasAliado[f, c] = true;
                botonesTableroAliado[f, c].BackColor = Color.LimeGreen;
                botonesTableroAliado[f, c].Enabled = false;
            }

            indiceBarcoActual++;
        }

        private bool CabeElBarco(int fila, int columna, int tamanio, string orientacion)
        {
            for (int k = 0; k < tamanio; k++)
            {
                int f = fila + (orientacion == "vertical" ? k : 0);
                int c = columna + (orientacion == "horizontal" ? k : 0);

                if (f >= 10 || c >= 10 || celdasOcupadasAliado[f, c]) return false;
            }
            return true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.R)
            {
                orientacionBarco = orientacionBarco == "horizontal" ? "vertical" : "horizontal";

                for (int fila = 0; fila < 10; fila++)
                {
                    for (int columna = 0; columna < 10; columna++)
                    {
                        if (!celdasOcupadasAliado[fila, columna])
                        {
                            botonesTableroAliado[fila, columna].BackColor = Color.Black;
                        }
                    }
                }

                if (ultimaFilaHover.HasValue && ultimaColumnaHover.HasValue && indiceBarcoActual < tamaniosBarcos.Length)
                {
                    int fila = ultimaFilaHover.Value;
                    int columna = ultimaColumnaHover.Value;
                    int tamanio = tamaniosBarcos[indiceBarcoActual];
                    if (CabeElBarco(fila, columna, tamanio, orientacionBarco))
                    {
                        for (int k = 0; k < tamanio; k++)
                        {
                            int f = fila + (orientacionBarco == "vertical" ? k : 0);
                            int c = columna + (orientacionBarco == "horizontal" ? k : 0);

                            if (!celdasOcupadasAliado[f, c])
                                botonesTableroAliado[f, c].BackColor = Color.LightGreen;
                        }
                    }
                }

                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
