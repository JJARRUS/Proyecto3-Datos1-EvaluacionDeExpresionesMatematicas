using System.Net.Sockets;
using System.Text;

// US7 Task 40: Prueba multicliente sin PowerShell
// Lanza N clientes en paralelo contra el servidor TCP para enviar EVAL y leer la respuesta.
// Uso: dotnet run --project tests/ClientLoadTest -- [host] [puerto] [clientes]

// Config de host/puerto/cantidad de clientes desde argumentos con defaults.
string host = args.Length > 0 ? args[0] : "127.0.0.1";
int port = args.Length > 1 && int.TryParse(args[1], out var p) ? p : 5000;
int clientCount = args.Length > 2 && int.TryParse(args[2], out var c) ? c : 3;

Console.WriteLine($"US7 Task 40: Prueba multicliente -> host={host}, port={port}, clients={clientCount}");

// Pool de expresiones RPN; se reciclan si hay más clientes que expresiones.
var expressions = new[] { "2 3 +", "10 5 -", "4 2 *", "6 3 /", "5 5 +" };

// Crear y lanzar los clientes en paralelo.
var tasks = Enumerable.Range(0, clientCount)
    .Select(i => RunClientAsync(host, port, expressions[i % expressions.Length], i + 1))
    .ToArray();

var results = await Task.WhenAll(tasks);

int ok = results.Count(r => r);
Console.WriteLine($"Resultado: {ok} / {clientCount} OK");

return ok == clientCount ? 0 : 1;

// Cliente TCP individual: abre conexión, envía EVAL y valida respuesta OK.
static async Task<bool> RunClientAsync(string host, int port, string expr, int clientId)
{
    var name = $"Cliente-{clientId}";
    try
    {
        using var client = new TcpClient();
        await client.ConnectAsync(host, port);

        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.UTF8);

        Console.WriteLine($"[{name}] Conectado. Enviando: EVAL {expr}");
        await writer.WriteLineAsync($"EVAL {expr}");

        var response = await reader.ReadLineAsync();
        Console.WriteLine($"[{name}] Resp: {response}");

        return response != null && response.StartsWith("OK", StringComparison.OrdinalIgnoreCase);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{name}] Error: {ex.Message}");
        return false;
    }
}
