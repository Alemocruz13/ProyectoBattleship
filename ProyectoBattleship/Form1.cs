namespace ProyectoBattleship
{
    public partial class Tablero : Form
    {
        // Constantes del juego
        private const int TAMANIO_TABLERO = 10;
        private static readonly int[] TAMANIOS_BARCOS = { 5, 4, 3, 3, 2 };

        // Variables principales del juego
        private int indiceBarcoActual = 0;
        private string orientacionBarco = "horizontal";
        private bool[,] celdasOcupadasAliado = new bool[TAMANIO_TABLERO, TAMANIO_TABLERO];
        private Button[,] botonesTableroAliado = new Button[TAMANIO_TABLERO, TAMANIO_TABLERO];

        private int? ultimaFilaHover = null;
        private int? ultimaColumnaHover = null;

        private bool[,] tableroEnemigo = new bool[TAMANIO_TABLERO, TAMANIO_TABLERO];
        private Button[,] botonesTableroEnemigo = new Button[TAMANIO_TABLERO, TAMANIO_TABLERO];
        private bool[,] celdasDisparadasUsuario = new bool[TAMANIO_TABLERO, TAMANIO_TABLERO];
        private bool[,] celdasDisparadasCPU = new bool[TAMANIO_TABLERO, TAMANIO_TABLERO];

        private bool juegoIniciado = false;
        private bool turnoUsuario;

        private readonly Random random = new();

        // Variables de la IA
        private List<(int, int)> celdasPorProbar = new();
        private List<(int, int)> impactosActuales = new();
        private string? orientacionCaza = null;
        private bool modoCaza = false;

        // Barcos y sistema de victoria
        private int barcosAliadosRestantes = TAMANIOS_BARCOS.Length;
        private int barcosEnemigosRestantes = TAMANIOS_BARCOS.Length;
        private int[,] idBarcoAliado = new int[TAMANIO_TABLERO, TAMANIO_TABLERO];
        private int[,] idBarcoEnemigo = new int[TAMANIO_TABLERO, TAMANIO_TABLERO];
        private Dictionary<int, int> vidaBarcoAliado = new();
        private Dictionary<int, int> vidaBarcoEnemigo = new();

        // Diccionario para sonidos precargados
        private Dictionary<string, System.Media.SoundPlayer> sonidos = new();

        public Tablero()
        {
            InitializeComponent();
            this.Text = "Battleship Game";
            this.BackColor = Color.Black;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(580, 1050);
            this.AutoSizeMode = AutoSizeMode.GrowOnly;

            PrecargarSonidos();
            InicializarJuego();
        }

        // Inicializa o reinicia todas las variables y tableros del juego.
        private void InicializarJuego()
        {
            indiceBarcoActual = 0;
            orientacionBarco = "horizontal";
            celdasOcupadasAliado = new bool[TAMANIO_TABLERO, TAMANIO_TABLERO];
            botonesTableroAliado = new Button[TAMANIO_TABLERO, TAMANIO_TABLERO];
            ultimaFilaHover = null;
            ultimaColumnaHover = null;
            tableroEnemigo = new bool[TAMANIO_TABLERO, TAMANIO_TABLERO];
            botonesTableroEnemigo = new Button[TAMANIO_TABLERO, TAMANIO_TABLERO];
            celdasDisparadasUsuario = new bool[TAMANIO_TABLERO, TAMANIO_TABLERO];
            celdasDisparadasCPU = new bool[TAMANIO_TABLERO, TAMANIO_TABLERO];
            juegoIniciado = false;
            celdasPorProbar = new();
            impactosActuales = new();
            orientacionCaza = null;
            modoCaza = false;
            barcosAliadosRestantes = TAMANIOS_BARCOS.Length;
            barcosEnemigosRestantes = TAMANIOS_BARCOS.Length;
            idBarcoAliado = new int[TAMANIO_TABLERO, TAMANIO_TABLERO];
            idBarcoEnemigo = new int[TAMANIO_TABLERO, TAMANIO_TABLERO];
            vidaBarcoAliado = new();
            vidaBarcoEnemigo = new();

            this.Controls.Clear();
            GenerarTableroAliado();
            GenerarTableroEnemigo();
            ColocarBarcosEnemigo();
            GenerarDataAliada();
        }

        // Precargar sonidos en memoria
        private void PrecargarSonidos()
        {
            string[] archivos = { "shoot1.wav", "shoot2.wav", "explosion.wav", "win.wav", "lose.wav" };
            foreach (var archivo in archivos)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", archivo);
                if (File.Exists(path))
                {
                    var player = new System.Media.SoundPlayer(path);
                    player.Load();
                    sonidos[archivo] = player;
                }
            }
        }

        private void GenerarDataAliada()
        {
            // Inicializar datos de barcos aliados si es necesario
        }

        // Crear tablero del jugador
        private void GenerarTableroAliado()
        {
            FlowLayoutPanel panelAliado = new FlowLayoutPanel
            {
                Size = new Size(400, 400),
                BackColor = Color.Black,
                Location = new Point(80, 500),
                Margin = new Padding(0)
            };

            for (int fila = 0; fila < TAMANIO_TABLERO; fila++)
            {
                for (int columna = 0; columna < TAMANIO_TABLERO; columna++)
                {
                    int filaActual = fila;
                    int columnaActual = columna;

                    Button boton = CrearBoton($"btnA_{filaActual}_{columnaActual}");
                    boton.Tag = (filaActual, columnaActual);

                    boton.MouseEnter += BotonAliado_MouseEnter;
                    boton.MouseLeave += BotonAliado_MouseLeave;
                    boton.Click += ColocarBarco_Click;

                    panelAliado.Controls.Add(boton);
                    botonesTableroAliado[filaActual, columnaActual] = boton;
                }
            }
            this.Controls.Add(panelAliado);
        }

        // Crear tablero enemigo
        private void GenerarTableroEnemigo()
        {
            Image imagenHover = Image.FromFile("resources/Target.png");

            FlowLayoutPanel panelEnemigo = new FlowLayoutPanel
            {
                Size = new Size(400, 400),
                BackColor = Color.Black,
                Location = new Point(80, 80),
                Margin = new Padding(0)
            };

            for (int fila = 0; fila < TAMANIO_TABLERO; fila++)
            {
                for (int columna = 0; columna < TAMANIO_TABLERO; columna++)
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
                    botonesTableroEnemigo[fila, columna] = boton;
                }
            }
            this.Controls.Add(panelEnemigo);
        }

        // Colocar barcos enemigos aleatoriamente
        private void ColocarBarcosEnemigo()
        {
            int barcoId = 1;
            vidaBarcoEnemigo.Clear();
            idBarcoEnemigo = new int[TAMANIO_TABLERO, TAMANIO_TABLERO];

            foreach (int tamaño in TAMANIOS_BARCOS)
            {
                bool colocado = false;
                while (!colocado)
                {
                    int fila = random.Next(0, TAMANIO_TABLERO);
                    int columna = random.Next(0, TAMANIO_TABLERO);
                    string direccion = random.Next(0, 2) == 0 ? "horizontal" : "vertical";

                    if (CabeElBarcoGenerico(tableroEnemigo, fila, columna, tamaño, direccion))
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

        // Verifica si un barco cabe en el tablero especificado.
        private bool CabeElBarcoGenerico(bool[,] tablero, int fila, int columna, int tamanio, string orientacion)
        {
            for (int k = 0; k < tamanio; k++)
            {
                int f = fila + (orientacion == "vertical" ? k : 0);
                int c = columna + (orientacion == "horizontal" ? k : 0);

                if (f >= TAMANIO_TABLERO || c >= TAMANIO_TABLERO || tablero[f, c]) return false;
            }
            return true;
        }

        // Crear botón de celda
        private Button CrearBoton(string nombre)
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

            bool acierto = false;

            if (tableroEnemigo[fila, columna])
            {
                acierto = true;
                PlaySound("shoot2.wav");
                boton.BackColor = Color.Red;
                boton.FlatAppearance.BorderColor = Color.DarkRed;

                int id = idBarcoEnemigo[fila, columna];
                vidaBarcoEnemigo[id]--;
                if (vidaBarcoEnemigo[id] == 0)
                {
                    barcosEnemigosRestantes--;
                    PlaySound("explosion.wav");
                    MarcarBarcoHundidoEnemigo(id);
                    if (barcosEnemigosRestantes == 0)
                    {
                        PlaySound("win.wav");
                        MessageBox.Show("¡Ganaste! Has hundido todos los barcos enemigos.");
                        ReiniciarJuego();
                        return;
                    }
                }
            }
            else
            {
                PlaySound("shoot1.wav");
                boton.BackColor = Color.FromArgb(40, 40, 40);
            }
            boton.Enabled = false;

            if (!acierto)
            {
                turnoUsuario = false;
                DisparoCPU();
            }
        }

        // Turno de la CPU (IA)
        private async void DisparoCPU()
        {
            await Task.Delay(500);
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
                    fila = random.Next(0, TAMANIO_TABLERO);
                    columna = random.Next(0, TAMANIO_TABLERO);
                } while (celdasDisparadasCPU[fila, columna]);
                impactosActuales.Clear();
                orientacionCaza = null;
                modoCaza = false;
            }

            celdasDisparadasCPU[fila, columna] = true;

            await Task.Delay(500);

            bool acierto = false;

            if (celdasOcupadasAliado[fila, columna])
            {
                acierto = true;
                PlaySound("shoot2.wav");
                botonesTableroAliado[fila, columna].BackColor = Color.Red;
                botonesTableroAliado[fila, columna].FlatAppearance.BorderColor = Color.DarkRed;

                await SacudirForm(this);

                modoCaza = true;
                impactosActuales.Add((fila, columna));
                ActualizarCeldasPorProbar();

                int id = idBarcoAliado[fila, columna];
                vidaBarcoAliado[id]--;
                if (vidaBarcoAliado[id] == 0)
                {
                    barcosAliadosRestantes--;
                    PlaySound("explosion.wav");
                    MarcarBarcoHundidoAliado(id);
                    if (barcosAliadosRestantes == 0)
                    {
                        PlaySound("lose.wav");
                        MessageBox.Show("¡Perdiste! La CPU hundió todos tus barcos.");
                        ReiniciarJuego();
                        return;
                    }
                    impactosActuales.RemoveAll(pos => idBarcoAliado[pos.Item1, pos.Item2] == id);
                    ActualizarCeldasPorProbar();
                }
            }
            else
            {
                botonesTableroAliado[fila, columna].BackColor = Color.FromArgb(40, 40, 40);
                PlaySound("shoot1.wav");
            }
            botonesTableroAliado[fila, columna].Enabled = false;

            if (celdasPorProbar.Count == 0)
            {
                modoCaza = false;
                impactosActuales.Clear();
                orientacionCaza = null;
            }

            if (!acierto)
            {
                turnoUsuario = true;
            }
            else
            {
                DisparoCPU();
            }
        }

        // Animación de sacudida para el formulario con parpadeo rojo
        private async Task SacudirForm(Form form, int intensidad = 12, int repeticiones = 7, int delay = 15)
        {
            var originalLocation = form.Location;
            var rand = new Random();
            var originalFormColor = form.BackColor;

            for (int i = 0; i < repeticiones; i++)
            {
                form.BackColor = Color.Red;
                int offsetX = rand.Next(-intensidad, intensidad + 1);
                int offsetY = rand.Next(-intensidad, intensidad + 1);
                form.Location = new Point(originalLocation.X + offsetX, originalLocation.Y + offsetY);
                await Task.Delay(delay);
                form.BackColor = originalFormColor;
                await Task.Delay(delay);
            }
            form.Location = originalLocation;
            form.BackColor = originalFormColor;
        }

        // IA: actualizar celdas a probar según impactos
        private void ActualizarCeldasPorProbar()
        {
            if (impactosActuales.Count >= 2)
            {
                var (f1, c1) = impactosActuales[0];
                var (f2, c2) = impactosActuales[1];
                orientacionCaza = (f1 == f2) ? "horizontal" : "vertical";
            }

            celdasPorProbar.Clear();

            if (orientacionCaza == null)
            {
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
                        if (nf >= 0 && nf < TAMANIO_TABLERO && nc >= 0 && nc < TAMANIO_TABLERO && !celdasDisparadasCPU[nf, nc])
                        {
                            if (!celdasPorProbar.Contains((nf, nc)))
                                celdasPorProbar.Add((nf, nc));
                        }
                    }
                }
            }
            else
            {
                var ordenados = impactosActuales
                    .OrderBy(x => orientacionCaza == "horizontal" ? x.Item2 : x.Item1)
                    .ToList();

                if (ordenados.Count == 0)
                    return;

                var primero = ordenados.First();
                var ultimo = ordenados.Last();

                if (orientacionCaza == "horizontal")
                {
                    int nf = primero.Item1;
                    int nc = primero.Item2 - 1;
                    if (nc >= 0 && !celdasDisparadasCPU[nf, nc])
                        celdasPorProbar.Add((nf, nc));
                    nf = ultimo.Item1;
                    nc = ultimo.Item2 + 1;
                    if (nc < TAMANIO_TABLERO && !celdasDisparadasCPU[nf, nc])
                        celdasPorProbar.Add((nf, nc));
                }
                else
                {
                    int nf = primero.Item1 - 1;
                    int nc = primero.Item2;
                    if (nf >= 0 && !celdasDisparadasCPU[nf, nc])
                        celdasPorProbar.Add((nf, nc));
                    nf = ultimo.Item1 + 1;
                    nc = ultimo.Item2;
                    if (nf < TAMANIO_TABLERO && !celdasDisparadasCPU[nf, nc])
                        celdasPorProbar.Add((nf, nc));
                }
            }
        }

        // Colocar barco del jugador
        private void ColocarBarco_Click(object sender, EventArgs e)
        {
            if (indiceBarcoActual >= TAMANIOS_BARCOS.Length)
                return;

            Button boton = sender as Button;
            var (fila, columna) = ((int, int))boton.Tag;
            int tamanio = TAMANIOS_BARCOS[indiceBarcoActual];
            int idBarco = indiceBarcoActual + 1;
            vidaBarcoAliado[idBarco] = tamanio;

            if (!CabeElBarcoGenerico(celdasOcupadasAliado, fila, columna, tamanio, orientacionBarco))
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

            if (indiceBarcoActual >= TAMANIOS_BARCOS.Length)
            {
                for (int filaBotones = 0; filaBotones < TAMANIO_TABLERO; filaBotones++)
                {
                    for (int columnaBotones = 0; columnaBotones < TAMANIO_TABLERO; columnaBotones++)
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

        // Cambiar orientación del barco con la tecla R
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.R)
            {
                orientacionBarco = orientacionBarco == "horizontal" ? "vertical" : "horizontal";

                for (int fila = 0; fila < TAMANIO_TABLERO; fila++)
                {
                    for (int columna = 0; columna < TAMANIO_TABLERO; columna++)
                    {
                        if (!celdasOcupadasAliado[fila, columna])
                        {
                            botonesTableroAliado[fila, columna].BackColor = Color.Black;
                        }
                    }
                }

                if (ultimaFilaHover.HasValue && ultimaColumnaHover.HasValue && indiceBarcoActual < TAMANIOS_BARCOS.Length)
                {
                    int fila = ultimaFilaHover.Value;
                    int columna = ultimaColumnaHover.Value;
                    int tamanio = TAMANIOS_BARCOS[indiceBarcoActual];
                    if (CabeElBarcoGenerico(celdasOcupadasAliado, fila, columna, tamanio, orientacionBarco))
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
            for (int f = 0; f < TAMANIO_TABLERO; f++)
                for (int c = 0; c < TAMANIO_TABLERO; c++)
                    if (idBarcoAliado[f, c] == id)
                        botonesTableroAliado[f, c].BackColor = Color.DarkRed;
        }

        // Marcar barco enemigo hundido
        private void MarcarBarcoHundidoEnemigo(int id)
        {
            for (int f = 0; f < TAMANIO_TABLERO; f++)
                for (int c = 0; c < TAMANIO_TABLERO; c++)
                    if (idBarcoEnemigo[f, c] == id)
                        botonesTableroEnemigo[f, c].BackColor = Color.DarkRed;
        }

        // Reiniciar el juego
        private void ReiniciarJuego()
        {
            InicializarJuego();
        }

        // Método para reproducir sonidos precargados
        private void PlaySound(string fileName)
        {
            try
            {
                if (sonidos.TryGetValue(fileName, out var player))
                {
                    player.Stop();
                    player.Play();
                }
            }
            catch
            {
                // Ignorar errores de sonido
            }
        }

        // Manejadores de eventos para hover en tablero aliado
        private void BotonAliado_MouseEnter(object? sender, EventArgs e)
        {
            if (indiceBarcoActual >= TAMANIOS_BARCOS.Length) return;

            var boton = sender as Button;
            var (filaActual, columnaActual) = ((int, int))boton.Tag;

            ultimaFilaHover = filaActual;
            ultimaColumnaHover = columnaActual;

            int tamanio = TAMANIOS_BARCOS[indiceBarcoActual];
            if (CabeElBarcoGenerico(celdasOcupadasAliado, filaActual, columnaActual, tamanio, orientacionBarco))
            {
                for (int k = 0; k < tamanio; k++)
                {
                    int f = filaActual + (orientacionBarco == "vertical" ? k : 0);
                    int c = columnaActual + (orientacionBarco == "horizontal" ? k : 0);

                    if (!celdasOcupadasAliado[f, c])
                        botonesTableroAliado[f, c].BackColor = Color.LightGreen;
                }
            }
        }

        private void BotonAliado_MouseLeave(object? sender, EventArgs e)
        {
            if (indiceBarcoActual >= TAMANIOS_BARCOS.Length) return;

            var boton = sender as Button;
            var (filaActual, columnaActual) = ((int, int))boton.Tag;

            int tamanio = TAMANIOS_BARCOS[indiceBarcoActual];
            if (CabeElBarcoGenerico(celdasOcupadasAliado, filaActual, columnaActual, tamanio, orientacionBarco))
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
        }
    }
}
