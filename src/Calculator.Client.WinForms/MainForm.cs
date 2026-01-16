using System;
using System.Windows.Forms;
using System.Threading.Tasks;

public partial class MainForm : Form
{
    private TcpApiClient _client;
    private const string SERVER_IP = "127.0.0.1";
    private const int SERVER_PORT = 8080;
    private TextBox textBoxExpression;
    private TextBox textBoxResult;
    private Label labelError;

    public MainForm()
    {
        InitializeComponent();
        _client = new TcpApiClient(SERVER_IP, SERVER_PORT);
    }

    private void InitializeComponent()
    {
        // 1. Diseñar la interfaz gráfica básica
        this.Text = "Calculadora - Cliente";
        this.Size = new System.Drawing.Size(700, 400);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new System.Drawing.Font("Arial", 10);
        this.AutoScaleMode = AutoScaleMode.Font;

        // ==================== PANEL SUPERIOR - ENTRADA ====================
        var panelInput = new Panel
        {
            Dock = DockStyle.Top,
            Height = 130,
            Padding = new Padding(15)
        };

        var labelExpression = new Label
        {
            Text = "Ingrese la expresión matemática:",
            Location = new System.Drawing.Point(15, 15),
            AutoSize = true,
            Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
        };

        // 2. Campo de entrada para la expresión
        textBoxExpression = new TextBox
        {
            Location = new System.Drawing.Point(15, 40),
            Size = new System.Drawing.Size(450, 30),
            Font = new System.Drawing.Font("Arial", 12),
            Multiline = false,
            Text = ""
        };

        // 2. Botón para calcular
        var buttonCalculate = new Button
        {
            Text = "Calcular",
            Location = new System.Drawing.Point(480, 40),
            Size = new System.Drawing.Size(130, 30),
            BackColor = System.Drawing.Color.LightBlue,
            Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
            Cursor = Cursors.Hand
        };

        // 4. Etiqueta para mensajes de error
        labelError = new Label
        {
            Text = "",
            Location = new System.Drawing.Point(15, 75),
            Size = new System.Drawing.Size(595, 40),
            AutoSize = false,
            ForeColor = System.Drawing.Color.Red,
            Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
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
            Padding = new Padding(15),
            BackColor = System.Drawing.Color.LightGray
        };

        var labelResult = new Label
        {
            Text = "Resultado:",
            Location = new System.Drawing.Point(15, 15),
            AutoSize = true,
            Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
        };

        textBoxResult = new TextBox
        {
            Location = new System.Drawing.Point(15, 40),
            Size = new System.Drawing.Size(630, 30),
            Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
            ReadOnly = true,
            Text = "Esperando entrada...",
            BackColor = System.Drawing.Color.White,
            ForeColor = System.Drawing.Color.DarkBlue
        };

        panelResult.Controls.Add(labelResult);
        panelResult.Controls.Add(textBoxResult);

        // ==================== AGREGAR PANELES AL FORMULARIO ====================
        this.Controls.Add(panelResult);
        this.Controls.Add(panelInput);

        // ==================== 3. CONECTAR EVENTOS ====================
        buttonCalculate.Click += async (sender, e) => await ButtonCalculate_ClickAsync();

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

    // 3. Evento conectado al botón Calcular
    private async Task ButtonCalculate_ClickAsync()
    {
        string expression = textBoxExpression.Text.Trim();

        // 4. Validar que la expresión no esté vacía
        if (string.IsNullOrEmpty(expression))
        {
            ShowError("Por favor ingrese una expresión válida");
            textBoxResult.Text = "Error";
            textBoxResult.ForeColor = System.Drawing.Color.Red;
            return;
        }

        // Limpiar mensaje de error
        ClearError();

        // Enviar al servidor y mostrar respuesta (US10)
        try
        {
            textBoxResult.Text = "Procesando...";
            textBoxResult.ForeColor = System.Drawing.Color.Orange;

            var response = await _client.EvaluateRawAsync(expression);

            if (response.StartsWith("OK ", StringComparison.OrdinalIgnoreCase))
            {
                var payload = response.Substring(3).Trim();
                textBoxResult.Text = $"Resultado: {payload}";
                textBoxResult.ForeColor = System.Drawing.Color.Green;
            }
            else if (response.StartsWith("ERR ", StringComparison.OrdinalIgnoreCase))
            {
                var error = response.Substring(4).Trim();
                ShowError($"Error: {error}");
                textBoxResult.Text = "Error";
                textBoxResult.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                ShowError("Respuesta inesperada del servidor");
                textBoxResult.Text = response;
                textBoxResult.ForeColor = System.Drawing.Color.Red;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error de conexión: {ex.Message}");
            textBoxResult.Text = "Error";
            textBoxResult.ForeColor = System.Drawing.Color.Red;
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
}

