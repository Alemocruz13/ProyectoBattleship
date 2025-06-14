namespace ProyectoBattleship
{
    public partial class Tablero : Form
    {
        // Variables principales del juego
        private int indiceBarcoActual = 0;
        private int[] tamaniosBarcos = new int[] { 5, 4, 3, 3, 2 };
        private string orientacionBarco = "horizontal";
        private bool[,] celdasOcupadasAliado = new bool[10, 10];
        private Button[,] botonesTableroAliado = new Button[10, 10];

        private int? ultimaFilaHover = null;
        private int? ultimaColumnaHover = null;

        private bool[,] tableroEnemigo = new bool[10, 10];
        private Button[,] botonesTableroEnemigo = new Button[10, 10];
        private bool[,] celdasDisparadasUsuario = new bool[10, 10];
        private bool[,] celdasDisparadasCPU = new bool[10, 10];

        private bool juegoIniciado = false;
        private bool turnoUsuario;

        Random random = new Random();

        // Variables de la IA
        private List<(int, int)> celdasPorProbar = new();
        private List<(int, int)> impactosActuales = new();
        private string? orientacionCaza = null;
        private bool modoCaza = false;

        // Barcos y sistema de victoria
        private int barcosAliadosRestantes = 5;
        private int barcosEnemigosRestantes = 5;
        private int[,] idBarcoAliado = new int[10, 10];
        private int[,] idBarcoEnemigo = new int[10, 10];
        private Dictionary<int, int> vidaBarcoAliado = new();
        private Dictionary<int, int> vidaBarcoEnemigo = new();

        public Tablero()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;

            GenerarTableroAliado();    // Crear tablero del jugador
            GenerarTableroEnemigo();   // Crear tablero enemigo
            ColocarBarcosEnemigo();    // Colocar barcos enemigos
            GenerarDataAliada();       // Inicializar datos aliados
        }

        void GenerarDataAliada()
        {
            // Inicializar datos de barcos aliados si es necesario
        }

        // Crear tablero del jugador
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

                    // Hover: mostrar posible colocación de barco
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

                    // Salir del hover: restaurar color
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

                    boton.Click += ColocarBarco_Click; // Colocar barco

                    panelAliado.Controls.Add(boton);
                    botonesTableroAliado[filaActual, columnaActual] = boton;
                }
            }
            this.Controls.Add(panelAliado);
        }

        // Crear tablero enemigo
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
                    boton.Click += DispararEnemigo_Click; // Disparar al enemigo

                    // Hover: mostrar objetivo
                    boton.MouseEnter += (sender, e) =>
                    {
                        ((Button)sender).BackgroundImage = imagenHover;
                        ((Button)sender).BackgroundImageLayout = ImageLayout.Stretch;
                    };

                    // Salir del hover: quitar objetivo
                    boton.MouseLeave += (sender, e) =>
                    {
                        ((Button)sender).BackgroundImage = null;
                    };

                    panelEnemigo.Controls.Add(boton);
                    botonesTableroEnemigo[fila, columna] = boton;
                }
            }
            this.Controls.Add(panelEnemigo);
        }

        // Colocar barcos enemigos aleatoriamente
        private void ColocarBarcosEnemigo()
        {
            int[] barcos = new int[] { 5, 4, 3, 3, 2 };
            int barcoId = 1;
            vidaBarcoEnemigo.Clear();
            idBarcoEnemigo = new int[10, 10];

            foreach (int tamaño in barcos)
            {
                bool colocado = false;
                while (!colocado)
                {
                    int fila = random.Next(0, 10);
                    int columna = random.Next(0, 10);
                    string direccion = random.Next(0, 2) == 0 ? "horizontal" : "vertical";

                    if (CabeElBarcoEnemigo(fila, columna, tamaño, direccion))
                    {
                        vidaBarcoEnemigo[barcoId] = tamaño;
                        for (int k = 0; k < tamaño; k++)
                        {
                            int f = fila + (direccion == "vertical" ? k : 0);
                            int c = columna + (direccion == "horizontal" ? k : 0);
                            tableroEnemigo[f, c] = true;
                            idBarcoEnemigo[f, c] = barcoId;
                        }
                        colocado = true;
                        barcoId++;
                    }
                }
            }
        }

        // Verificar si cabe el barco enemigo
        private bool CabeElBarcoEnemigo(int fila, int columna, int tamaño, string direccion)
        {
            for (int k = 0; k < tamaño; k++)
            {
                int f = fila + (direccion == "vertical" ? k : 0);
                int c = columna + (direccion == "horizontal" ? k : 0);

                if (f >= 10 || c >= 10 || tableroEnemigo[f, c])
                    return false;
            }
            return true;
        }

        // Crear botón de celda
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

        // Disparar al tablero enemigo
        private void DispararEnemigo_Click(object sender, EventArgs e)
        {
            if (!juegoIniciado || !turnoUsuario)
                return;

            Button boton = sender as Button;
            var (fila, columna) = ((int, int))boton.Tag;

            if (celdasDisparadasUsuario[fila, columna])
                return;

            celdasDisparadasUsuario[fila, columna] = true;

            if (tableroEnemigo[fila, columna])
            {
                boton.BackColor = Color.Red;
                boton.FlatAppearance.BorderColor = Color.Red;

                int id = idBarcoEnemigo[fila, columna];
                vidaBarcoEnemigo[id]--;
                if (vidaBarcoEnemigo[id] == 0)
                {
                    barcosEnemigosRestantes--;
                    MarcarBarcoHundidoEnemigo(id); // Marcar barco hundido
                    if (barcosEnemigosRestantes == 0)
                    {
                        MessageBox.Show("¡Ganaste! Has hundido todos los barcos enemigos.");
                        ReiniciarJuego();
                        return;
                    }
                    else
                    {
                        MessageBox.Show("¡Hundiste un barco enemigo!");
                    }
                }
            }
            else
            {
                boton.BackColor = Color.FromArgb(40, 40, 40);
            }
            boton.Enabled = false;

            turnoUsuario = false;
            DisparoCPU(); // Turno de la CPU
        }

        // Turno de la CPU (IA)
        private void DisparoCPU()
        {
            Task.Delay(500).ContinueWith(_ =>
            {
                int fila, columna;

                if (celdasPorProbar.Count > 0)
                {
                    (fila, columna) = celdasPorProbar[0];
                    celdasPorProbar.RemoveAt(0);
                }
                else
                {
                    do
                    {
                        fila = random.Next(0, 10);
                        columna = random.Next(0, 10);
                    } while (celdasDisparadasCPU[fila, columna]);
                    // Reiniciar caza si no hay celdas por probar
                    impactosActuales.Clear();
                    orientacionCaza = null;
                    modoCaza = false;
                }

                celdasDisparadasCPU[fila, columna] = true;

                this.Invoke(() =>
                {
                    if (celdasOcupadasAliado[fila, columna])
                    {
                        botonesTableroAliado[fila, columna].BackColor = Color.Red;
                        botonesTableroAliado[fila, columna].FlatAppearance.BorderColor = Color.Red;

                        modoCaza = true;
                        impactosActuales.Add((fila, columna));
                        ActualizarCeldasPorProbar();

                        int id = idBarcoAliado[fila, columna];
                        vidaBarcoAliado[id]--;
                        if (vidaBarcoAliado[id] == 0)
                        {
                            barcosAliadosRestantes--;
                            MarcarBarcoHundidoAliado(id); // Marcar barco hundido
                            if (barcosAliadosRestantes == 0)
                            {
                                MessageBox.Show("¡Perdiste! La CPU hundió todos tus barcos.");
                                ReiniciarJuego();
                                return;
                            }
                            else
                            {
                                MessageBox.Show("¡La CPU hundió uno de tus barcos!");
                            }
                            // Limpiar caza si hundió el barco
                            impactosActuales.RemoveAll(pos => idBarcoAliado[pos.Item1, pos.Item2] == id);
                            ActualizarCeldasPorProbar();
                        }
                    }
                    else
                    {
                        botonesTableroAliado[fila, columna].BackColor = Color.FromArgb(40, 40, 40);
                    }
                    botonesTableroAliado[fila, columna].Enabled = false;

                    if (celdasPorProbar.Count == 0)
                    {
                        modoCaza = false;
                        impactosActuales.Clear();
                        orientacionCaza = null;
                    }

                    turnoUsuario = true;
                });
            });
        }

        // IA: actualizar celdas a probar según impactos
        private void ActualizarCeldasPorProbar()
        {
            // Si hay dos o más impactos, determinar orientación
            if (impactosActuales.Count >= 2)
            {
                var (f1, c1) = impactosActuales[0];
                var (f2, c2) = impactosActuales[1];
                orientacionCaza = (f1 == f2) ? "horizontal" : "vertical";
            }

            celdasPorProbar.Clear();

            if (orientacionCaza == null)
            {
                // Sin orientación: buscar alrededor de impactos
                foreach (var (fila, columna) in impactosActuales)
                {
                    var direcciones = new (int, int)[]
                    {
                            (-1, 0), (1, 0), (0, -1), (0, 1)
                    };
                    foreach (var (df, dc) in direcciones)
                    {
                        int nf = fila + df;
                        int nc = columna + dc;
                        if (nf >= 0 && nf < 10 && nc >= 0 && nc < 10 && !celdasDisparadasCPU[nf, nc])
                        {
                            if (!celdasPorProbar.Contains((nf, nc)))
                                celdasPorProbar.Add((nf, nc));
                        }
                    }
                }
            }
            else
            {
                // Con orientación: buscar extremos de la línea de impactos
                var ordenados = impactosActuales
                    .OrderBy(x => orientacionCaza == "horizontal" ? x.Item2 : x.Item1)
                    .ToList();

                if (ordenados.Count == 0)
                    return; // No hay impactos

                var primero = ordenados.First();
                var ultimo = ordenados.Last();

                if (orientacionCaza == "horizontal")
                {
                    // Extremo izquierdo
                    int nf = primero.Item1;
                    int nc = primero.Item2 - 1;
                    if (nc >= 0 && !celdasDisparadasCPU[nf, nc])
                        celdasPorProbar.Add((nf, nc));
                    // Extremo derecho
                    nf = ultimo.Item1;
                    nc = ultimo.Item2 + 1;
                    if (nc < 10 && !celdasDisparadasCPU[nf, nc])
                        celdasPorProbar.Add((nf, nc));
                }
                else // vertical
                {
                    // Extremo arriba
                    int nf = primero.Item1 - 1;
                    int nc = primero.Item2;
                    if (nf >= 0 && !celdasDisparadasCPU[nf, nc])
                        celdasPorProbar.Add((nf, nc));
                    // Extremo abajo
                    nf = ultimo.Item1 + 1;
                    nc = ultimo.Item2;
                    if (nf < 10 && !celdasDisparadasCPU[nf, nc])
                        celdasPorProbar.Add((nf, nc));
                }
            }
        }

        // Colocar barco del jugador
        private void ColocarBarco_Click(object sender, EventArgs e)
        {
            if (indiceBarcoActual >= tamaniosBarcos.Length)
            {
                return;
            }

            Button boton = sender as Button;
            var (fila, columna) = ((int, int))boton.Tag;
            int tamanio = tamaniosBarcos[indiceBarcoActual];

            int idBarco = indiceBarcoActual + 1;
            vidaBarcoAliado[idBarco] = tamanio;

            if (!CabeElBarco(fila, columna, tamanio, orientacionBarco))
            {
                MessageBox.Show("No se puede colocar aquí.");
                return;
            }

            for (int k = 0; k < tamanio; k++)
            {
                int f = fila + (orientacionBarco == "vertical" ? k : 0);
                int c = columna + (orientacionBarco == "horizontal" ? k : 0);

                celdasOcupadasAliado[f, c] = true;
                botonesTableroAliado[f, c].BackColor = Color.LimeGreen;
                botonesTableroAliado[f, c].FlatAppearance.BorderColor = Color.LimeGreen;
                botonesTableroAliado[f, c].Enabled = false;
                idBarcoAliado[f, c] = idBarco;
            }

            indiceBarcoActual++;

            if (indiceBarcoActual >= tamaniosBarcos.Length)
            {
                // Desactivar tablero tras colocar todos los barcos
                for (int filaBotones = 0; filaBotones < 10; filaBotones++)
                {
                    for (int columnaBotones = 0; columnaBotones < 10; columnaBotones++)
                    {
                        botonesTableroAliado[filaBotones, columnaBotones].Enabled = false;
                    }
                }
                juegoIniciado = true;
                turnoUsuario = random.Next(0, 2) == 0;
                MessageBox.Show(turnoUsuario ? "¡Tu turno primero!" : "¡La CPU comienza!");
                if (!turnoUsuario)
                {
                    DisparoCPU();
                }
            }
        }

        // Verificar si cabe el barco del jugador
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

        // Cambiar orientación del barco con la tecla R
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.R)
            {
                orientacionBarco = orientacionBarco == "horizontal" ? "vertical" : "horizontal";

                // Restaurar colores
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

                // Mostrar posible colocación en nueva orientación
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

        // Marcar barco aliado hundido
        private void MarcarBarcoHundidoAliado(int id)
        {
            for (int f = 0; f < 10; f++)
                for (int c = 0; c < 10; c++)
                    if (idBarcoAliado[f, c] == id)
                        botonesTableroAliado[f, c].BackColor = Color.DarkRed;
        }

        // Marcar barco enemigo hundido
        private void MarcarBarcoHundidoEnemigo(int id)
        {
            for (int f = 0; f < 10; f++)
                for (int c = 0; c < 10; c++)
                    if (idBarcoEnemigo[f, c] == id)
                        botonesTableroEnemigo[f, c].BackColor = Color.DarkRed;
        }

        // Reiniciar el juego
        private void ReiniciarJuego()
        {
            this.Controls.Clear();
            indiceBarcoActual = 0;
            orientacionBarco = "horizontal";
            celdasOcupadasAliado = new bool[10, 10];
            botonesTableroAliado = new Button[10, 10];
            ultimaFilaHover = null;
            ultimaColumnaHover = null;
            tableroEnemigo = new bool[10, 10];
            botonesTableroEnemigo = new Button[10, 10];
            celdasDisparadasUsuario = new bool[10, 10];
            celdasDisparadasCPU = new bool[10, 10];
            juegoIniciado = false;
            celdasPorProbar.Clear();
            impactosActuales.Clear();
            orientacionCaza = null;
            modoCaza = false;
            barcosAliadosRestantes = 5;
            barcosEnemigosRestantes = 5;
            idBarcoAliado = new int[10, 10];
            idBarcoEnemigo = new int[10, 10];
            vidaBarcoAliado.Clear();
            vidaBarcoEnemigo.Clear();

            GenerarTableroAliado();
            GenerarTableroEnemigo();
            ColocarBarcosEnemigo();
            GenerarDataAliada();
        }
    }
}
