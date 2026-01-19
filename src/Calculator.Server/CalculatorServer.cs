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




 
    /// US7 Task 37: Implementa un bucle infinito que acepta múltiples clientes de forma concurrente.
    /// Cada cliente se atiende en una tarea separada sin bloquear la aceptación de nuevos clientes.
    /// "ct" Token de cancelación para detener el servidor ordenadamente
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




 
    /// US7 Task 38: Manejo de la comunicación con un cliente TCP específico.
    /// US7 Task 39: Cada cliente tiene streams aislados (reader/writer propios) garantizando no-interferencia.
    /// Crea una sesión identificada para cada conexión.

    /// "client" Cliente TCP conectado
    /// "ct" Token de cancelación para cerrar ordenadamente
    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        // US7 Task 38: Crear una sesión identificada para este cliente
        var remoteEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
        var localEndpoint = client.Client.LocalEndPoint as IPEndPoint;
        var session = new ClientSession(remoteEndpoint?.Address, remoteEndpoint?.Port ?? 0, 
                                        localEndpoint?.Address, localEndpoint?.Port ?? 0);

        // Registrar conexión de cliente
        Console.WriteLine($"Cliente conectado: {session}");

        // Task 39: Cada cliente tiene su propio TcpClient, NetworkStream, StreamReader y StreamWriter.
        // using asegura que los recursos se liberen automáticamente al finalizar
        using (client) // Cerrar el TcpClient al finalizar
        {
            using var stream = client.GetStream(); // Obtener el stream de red para leer/escribir datos
            
            // US8 Task 42: Configurar StreamReader con UTF-8 para recepción correcta de mensajes del cliente
            // Lee líneas delimitadas por \n o \r\n según protocolo definido
            using var reader = new StreamReader(stream, Encoding.UTF8);

            // Writer aislado para enviar respuestas solo a este cliente
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Bucle de comunicación con el cliente: leer comandos hasta que se solicite cancelación
            while (!ct.IsCancellationRequested)
            {
                // US8 Task 42: Leer una línea completa del cliente (espera hasta recibir \n o \r\n)
                // ReadLineAsync bloquea hasta que llega el delimitador, implementando el framing del protocolo
                var line = await reader.ReadLineAsync();

                // US8 Task 42: Si ReadLineAsync devuelve null, el cliente cerró la conexión limpiamente
                if (line == null) break;

                // US8 Task 42: Ignorar líneas vacías (solo espacios o tabuladores)
                line = line.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // Incrementar contador de comandos para esta sesión
                session.IncrementCommandCount();

                // US8 Task 41: Comando EVAL - recibe expresión desde cliente
                // Formato del mensaje: "EVAL" (tokens separados por espacios)
                // Ejemplo: "EVAL 3 4 +" -> evalúa expresión en notación postfija
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

                        // Registrar la evaluación en el archivo CSV (timestamp, sessionId, expresión, resultado)
                        CsvLog.Append(_csvPath, DateTime.UtcNow, expr, (int)result, session.SessionId);
                        
                        // Enviar respuesta exitosa al cliente: "OK <resultado>"
                        await writer.WriteLineAsync($"OK {result}");
                        
                        Console.WriteLine($"[{session.SessionId:N}] EVAL: {expr} = {result}");
                    }
                    catch (Exception ex)
                    {
                        // Si hay error (sintaxis inválida, división por cero, etc.), enviar mensaje de error
                        await writer.WriteLineAsync($"ERR {ex.Message}");
                        Console.WriteLine($"[{session.SessionId:N}] ERROR: {ex.Message}");
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
                    
                    Console.WriteLine($"[{session.SessionId:N}] HIST: historial enviado");
                }
                // Comando no reconocido: enviar error
                else
                {
                    await writer.WriteLineAsync("ERR Comando no reconocido");
                    Console.WriteLine($"[{session.SessionId:N}] Comando desconocido: {line}");
                }
            }
        }

        // Registrar desconexión de cliente
        Console.WriteLine($"X Cliente desconectado: {session.SessionId:N} (Comandos procesados: {session.CommandCount})");
    }
}