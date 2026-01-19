using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Calculator.ArbolExprecion;

namespace Calculator.Client.WinForms;

public partial class MainForm : Form
{
    private TcpApiClient _client;
    
    // Task 29: Configuración del cliente para conectarse al servidor
    // Dirección IP del servidor de calculadora
    private const string SERVER_IP = "127.0.0.1";
    // Puerto TCP del servidor de calculadora
    private const int SERVER_PORT = 5000;
    
    private TextBox textBoxExpression;
    private TextBox textBoxResult;
    private Label labelError;
    private Label labelStatus;
    private DataGridView dataGridHistory;

    public MainForm()
    {
        InitializeComponent();
        // Task 29: Inicializar cliente con dirección y puerto configurados
        _client = new TcpApiClient(SERVER_IP, SERVER_PORT);
        UpdateConnectionStatus();
    }

    private void InitializeComponent()
    {
        // 1. Diseñar la interfaz gráfica básica
        this.Text = "Calculadora - Cliente";
        this.Size = new System.Drawing.Size(820, 560);
        this.MinimumSize = new System.Drawing.Size(780, 520);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new System.Drawing.Font("Arial", 10);
        this.AutoScaleMode = AutoScaleMode.Font;

        // Layout principal con cinco filas: header, input, resultado, historial, estado
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(10),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // Header
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));  // Input
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));   // Resultado
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Historial
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));   // Status

        // ==================== HEADER ====================
        var panelHeader = new Panel { Dock = DockStyle.Fill };
        var labelTitle = new Label
        {
            Text = "Calculadora de Expresiones (Cliente)",
            AutoSize = true,
            Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(5, 10)
        };
        var labelHint = new Label
        {
            Text = "Operadores: +  -  *  /  %  **  and(&)  or(|)  xor(^)  not(~)" +
                   "    • Usa paréntesis para agrupar",
            AutoSize = true,
            Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular),
            Location = new System.Drawing.Point(5, 30)
        };
        panelHeader.Controls.Add(labelTitle);
        panelHeader.Controls.Add(labelHint);

        // ==================== PANEL SUPERIOR - ENTRADA ====================
        var panelInput = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };

        var labelExpression = new Label
        {
            Text = "Expresión:",
            Location = new System.Drawing.Point(5, 5),
            AutoSize = true,
            Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold)
        };

        textBoxExpression = new TextBox
        {
            Location = new System.Drawing.Point(5, 28),
            Size = new System.Drawing.Size(600, 34),
            Font = new System.Drawing.Font("Segoe UI", 12),
            Multiline = false,
            Text = ""
        };

        var buttonCalculate = new Button
        {
            Text = "Calcular",
            Location = new System.Drawing.Point(620, 26),
            Size = new System.Drawing.Size(120, 36),
            BackColor = System.Drawing.Color.SteelBlue,
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        buttonCalculate.FlatAppearance.BorderSize = 0;

        labelError = new Label
        {
            Text = "",
            Location = new System.Drawing.Point(5, 70),
            Size = new System.Drawing.Size(735, 60),
            AutoSize = false,
            ForeColor = System.Drawing.Color.Firebrick,
            Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
            Visible = false
        };

        panelInput.Controls.Add(labelExpression);
        panelInput.Controls.Add(textBoxExpression);
        panelInput.Controls.Add(buttonCalculate);
        panelInput.Controls.Add(labelError);

        // ==================== PANEL RESULTADO ====================
        var panelResult = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
            BackColor = System.Drawing.Color.Gainsboro
        };

        var labelResult = new Label
        {
            Text = "Resultado:",
            Location = new System.Drawing.Point(5, 8),
            AutoSize = true,
            Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold)
        };

        textBoxResult = new TextBox
        {
            Location = new System.Drawing.Point(5, 32),
            Size = new System.Drawing.Size(735, 34),
            Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
            ReadOnly = true,
            Text = "Esperando entrada...",
            BackColor = System.Drawing.Color.White,
            ForeColor = System.Drawing.Color.FromArgb(30, 64, 175)
        };

        panelResult.Controls.Add(labelResult);
        panelResult.Controls.Add(textBoxResult);

        // ==================== PANEL HISTORIAL ====================
        var panelHistory = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };

        var panelHistoryTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 35,
            Padding = new Padding(0, 0, 0, 6)
        };

        var labelHistory = new Label
        {
            Text = "Historial de evaluaciones:",
            AutoSize = true,
            Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(0, 5)
        };

        var buttonLoadHistory = new Button
        {
            Text = "Cargar Historial",
            AutoSize = true,
            Font = new System.Drawing.Font("Segoe UI", 9),
            BackColor = System.Drawing.Color.LightGray,
            FlatStyle = FlatStyle.Flat,
            Location = new System.Drawing.Point(250, 5),
            Cursor = Cursors.Hand
        };
        buttonLoadHistory.FlatAppearance.BorderSize = 1;

        panelHistoryTop.Controls.Add(labelHistory);
        panelHistoryTop.Controls.Add(buttonLoadHistory);

        dataGridHistory = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            BackgroundColor = System.Drawing.Color.White,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false
        };
        dataGridHistory.Columns.Add("Expresion", "Expresión");
        dataGridHistory.Columns.Add("Resultado", "Resultado");
        dataGridHistory.Columns.Add("Fecha", "Fecha/Hora");

        panelHistory.Controls.Add(dataGridHistory);
        panelHistory.Controls.Add(panelHistoryTop);

        // ==================== STATUS BAR ====================
        var panelStatus = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
            BackColor = System.Drawing.Color.WhiteSmoke
        };

        labelStatus = new Label
        {
            Text = $"Servidor: {SERVER_IP}:{SERVER_PORT} (listo)",
            AutoSize = true,
            ForeColor = System.Drawing.Color.FromArgb(34, 85, 34),
            Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Regular),
            Location = new System.Drawing.Point(5, 6)
        };

        panelStatus.Controls.Add(labelStatus);

        // ==================== AGREGAR PANELES AL LAYOUT ====================
        layout.Controls.Add(panelHeader, 0, 0);
        layout.Controls.Add(panelInput, 0, 1);
        layout.Controls.Add(panelResult, 0, 2);
        layout.Controls.Add(panelHistory, 0, 3);
        layout.Controls.Add(panelStatus, 0, 4);

        this.Controls.Add(layout);

        // ==================== 3. CONECTAR EVENTOS ====================
        buttonCalculate.Click += async (sender, e) => await ButtonCalculate_ClickAsync();
        buttonLoadHistory.Click += async (sender, e) => await ButtonLoadHistory_ClickAsync();

        // Permitir presionar Enter en el textbox
        textBoxExpression.KeyPress += (sender, e) =>
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                buttonCalculate.PerformClick();
            }
        };
    }

    // Task 30/31: Evento conectado al botón Calcular - Envía expresión y recibe respuesta
    private async Task ButtonCalculate_ClickAsync()
    {
        string expression = textBoxExpression.Text.Trim();

        if (string.IsNullOrEmpty(expression))
        {
            ShowError("Por favor ingrese una expresión válida");
            textBoxResult.Text = "Error";
            textBoxResult.ForeColor = System.Drawing.Color.Red;
            return;
        }

        ClearError();

        try
        {
            textBoxResult.Text = "Procesando...";
            textBoxResult.ForeColor = System.Drawing.Color.DarkGoldenrod;
            labelStatus.Text = "Convirtiendo a postfija...";
            labelStatus.ForeColor = System.Drawing.Color.DarkGoldenrod;

            // Convertir expresión infija a postfija
            var parser = new RpnParser();
            var postfixTokens = parser.ConvertToPostfix(expression);
            var postfixExpression = string.Join(" ", postfixTokens);

            labelStatus.Text = "Enviando al servidor...";

            // Enviar expresión RPN al servidor
            var response = await _client.EvaluateRawAsync(postfixExpression);

            // Task 31: Recibir y procesar la respuesta del servidor
            if (response.StartsWith("OK ", StringComparison.OrdinalIgnoreCase))
            {
                // Respuesta exitosa: extraer resultado de "OK <valor>"
                var payload = response.Substring(3).Trim();
                textBoxResult.Text = $"Resultado: {payload}";
                textBoxResult.ForeColor = System.Drawing.Color.FromArgb(34, 139, 34);
                labelStatus.Text = "Respuesta OK";
                labelStatus.ForeColor = System.Drawing.Color.FromArgb(34, 85, 34);

                dataGridHistory.Rows.Insert(0, postfixExpression, payload, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else if (response.StartsWith("ERR ", StringComparison.OrdinalIgnoreCase))
            {
                // Respuesta de error: extraer mensaje de "ERR <mensaje>"
                var error = response.Substring(4).Trim();
                ShowError($"Error: {error}");
                textBoxResult.Text = "Error";
                textBoxResult.ForeColor = System.Drawing.Color.Red;
                labelStatus.Text = "Error recibido del servidor";
                labelStatus.ForeColor = System.Drawing.Color.Firebrick;
            }
            else
            {
                // Respuesta inesperada o inválida
                ShowError("Respuesta inesperada del servidor");
                textBoxResult.Text = response;
                textBoxResult.ForeColor = System.Drawing.Color.Red;
                labelStatus.Text = "Respuesta inesperada";
                labelStatus.ForeColor = System.Drawing.Color.Firebrick;
            }
        }
        catch (Exception ex)
        {
            // Task 31: Manejar errores de conexión o recepción
            ShowError($"Error: {ex.Message}");
            textBoxResult.Text = "Error";
            textBoxResult.ForeColor = System.Drawing.Color.Red;
            labelStatus.Text = "Error al procesar";
            labelStatus.ForeColor = System.Drawing.Color.Firebrick;
        }
        finally
        {
            textBoxExpression.Clear();
            textBoxExpression.Focus();
        }
    }

    // 4. Mostrar mensaje de error
    private void ShowError(string message)
    {
        labelError.Text = message;
        labelError.Visible = true;
    }

    // Limpiar mensaje de error
    private void ClearError()
    {
        labelError.Text = "";
        labelError.Visible = false;
    }

    // Task 29: Actualizar el estado de la conexión
    private void UpdateConnectionStatus()
    {
        if (labelStatus != null)
        {
            labelStatus.Text = $"Servidor: {SERVER_IP}:{SERVER_PORT} (listo)";
            labelStatus.ForeColor = System.Drawing.Color.FromArgb(34, 85, 34);
        }
    }

    // US14: Cargar historial desde el servidor
    private async Task ButtonLoadHistory_ClickAsync()
    {
        try
        {
            labelStatus.Text = "Cargando historial del servidor...";
            labelStatus.ForeColor = System.Drawing.Color.DarkGoldenrod;

            // Solicitar historial al servidor (US14 Task 1)
            var historyLines = await _client.HistAsync();

            // US14 Task 4: Manejar caso sin operaciones registradas
            if (historyLines.Count == 0)
            {
                ShowError("No hay historial registrado en el servidor");
                labelStatus.Text = "Historial vacío";
                labelStatus.ForeColor = System.Drawing.Color.DarkGoldenrod;
                return;
            }

            // LIMPIAR TODA LA GRID
            dataGridHistory.Rows.Clear();

            // US14 Task 3: Procesar datos CSV y mostrar en grid
            int rowsAdded = 0;
            
            foreach (var line in historyLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Formato del servidor CSV: timestamp;UUID;expresión_postfija;resultado
                var parts = line.Split(';');
                if (parts.Length >= 4)
                {
                    string timestamp = parts[0];
                    string uuid = parts[1];
                    string exprPostfix = parts[2];
                    string resultado = parts[3];
                    
                    // Formatear timestamp ISO a formato legible
                    if (DateTime.TryParse(timestamp, out DateTime dt))
                        timestamp = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    // Mostrar: expresión_postfija, resultado, timestamp
                    // Insertar al inicio para que más reciente quede arriba
                    dataGridHistory.Rows.Insert(0, exprPostfix, resultado, timestamp);
                    rowsAdded++;
                }
            }

            labelStatus.Text = $"Historial cargado: {rowsAdded} operaciones";
            labelStatus.ForeColor = System.Drawing.Color.FromArgb(34, 85, 34);
            ClearError();
        }
        catch (Exception ex)
        {
            ShowError($"Error al cargar historial: {ex.Message}");
            labelStatus.Text = "Error cargando historial";
            labelStatus.ForeColor = System.Drawing.Color.Firebrick;
        }
    }
}


