// Implementación del servidor TCP para recibir y procesar expresiones matemáticas de clientes
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Calculator.ArbolExprecion;

namespace Calculator.Server;

/// Servidor TCP que escucha en un puerto específico y acepta conexiones de múltiples clientes.
/// Procesa comandos EVAL (evaluar expresiones RPN) e HIST (historial de evaluaciones).
public sealed class CalculatorServer
{
    // Puerto TCP en el que el servidor escuchará conexiones entrantes
    private readonly int _port;
    
    // Ruta del archivo CSV donde se registrará el historial de evaluaciones
    private readonly string _csvPath;


    /// Constructor del servidor TCP.

    /// "port" Es el puerto en el que escuchará el servidor
    /// "csvPath" Es la ruta completa del archivo CSV para logging
    public CalculatorServer(int port, string csvPath)
    {
        _port = port;
        _csvPath = csvPath;
    }




    /// <summary>
    /// US7 Task 37: Implementa un bucle infinito que acepta múltiples clientes de forma concurrente.
    /// Cada cliente se atiende en una tarea separada sin bloquear la aceptación de nuevos clientes.
    /// </summary>
    /// <param name="ct">Token de cancelación para detener el servidor ordenadamente</param>
    public async Task RunAsync(CancellationToken ct)
    {
        // Crear listener TCP que escucha en todas las interfaces de red (0.0.0.0) en el puerto configurado
        var listener = new TcpListener(IPAddress.Any, _port);
        
        // Iniciar el listener para comenzar a aceptar conexiones
        listener.Start();

        // Bucle principal del servidor: ejecuta hasta que se solicite cancelación (CTRL+C)
        while (!ct.IsCancellationRequested)
        {
            // Esperar de forma asíncrona a que un cliente se conecte
            var client = await listener.AcceptTcpClientAsync(ct);
            
            // Task 37: Task.Run ejecuta HandleClientAsync en un thread pool, permitiendo concurrencia
            // El "_" descarta la Task para que no se espere su finalización (fire-and-forget)
            // Esto permite que el servidor continúe aceptando nuevos clientes inmediatamente
            // sin esperar a que el cliente actual termine su comunicación
            _ = Task.Run(() => HandleClientAsync(client, ct), ct);
        }
    }




    /// Manejo de la comunicación con un cliente TCP específico.
    /// "client" Cliente TCP conectado
    /// "ct" Token de cancelación para cerrar ordenadamente
    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        // using asegura que los recursos se liberen automáticamente al finalizar
        using (client) // Cerrar el TcpClient al finalizar
        {
            using var stream = client.GetStream(); // Obtener el stream de red para leer/escribir datos
            using var reader = new StreamReader(stream, Encoding.UTF8); // Lector para recibir texto del cliente

            // Escritor para enviar respuestas
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };



        // Bucle de comunicación con el cliente: leer comandos hasta que se solicite cancelación
            while (!ct.IsCancellationRequested)
            {
            // Leer una línea de texto del cliente de forma asíncrona
            var line = await reader.ReadLineAsync();


            // Si ReadLineAsync devuelve null, significa que el cliente cerró la conexión
                if (line == null) break;

            // Comando EVAL: evaluar una expresión matemática en notación RPN
            // Formato: "EVAL <expresión_RPN>" o bien notación postfija (ej: "EVAL 3 4 +")
                if (line.StartsWith("EVAL ", StringComparison.OrdinalIgnoreCase))
            {
                // Extraer la expresión RPN eliminando el prefijo "EVAL "
                var expr = line.Substring(5).Trim();
                try
                {
                    // Parsear la expresión RPN y construir el árbol de expresión
                    var root = RpnParser.Parse(expr);
                    
                    // Evaluar el árbol de expresión y obtener el resultado numérico
                    double result = root.Evaluate();

                    // Registrar la evaluación en el archivo CSV (timestamp, expresión, resultado)
                    CsvLog.Append(_csvPath, DateTime.UtcNow, expr, (int)result);
                    
                    // Enviar respuesta exitosa al cliente: "OK <resultado>"
                    await writer.WriteLineAsync($"OK {result}");
                }
                catch (Exception ex)
                {
                    // Si hay error (sintaxis inválida, división por cero, etc.), enviar mensaje de error
                    await writer.WriteLineAsync($"ERR {ex.Message}");
                }
            }
            // Comando HIST: solicitar el historial completo de evaluaciones
                else if (line.Equals("HIST", StringComparison.OrdinalIgnoreCase))
                {
                // Leer todas las líneas del archivo CSV y enviarlas al cliente
                foreach (var row in CsvLog.ReadAllLines(_csvPath))
                        await writer.WriteLineAsync(row);

                // Enviar marcador de fin de historial
                    await writer.WriteLineAsync("END");
                }
            // Comando no reconocido: enviar error
                else
                {
                    await writer.WriteLineAsync("ERR Comando no reconocido");
                }
            }
        }
        // Al salir del bucle, los 'using' liberan automáticamente los recursos (stream, reader, writer, client)
    }
}