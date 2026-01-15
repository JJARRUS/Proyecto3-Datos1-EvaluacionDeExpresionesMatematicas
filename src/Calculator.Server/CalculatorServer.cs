using System.Net;
using System.Net.Sockets;

namespace Calculator.Server;

public sealed class CalculatorServer
{
    private readonly int _port;

    public CalculatorServer(int port)
    {
        _port = port;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        while (!ct.IsCancellationRequested)
        {
            TcpClient client = await listener.AcceptTcpClientAsync(ct);
            // conexión aceptada (aún no se maneja)
        }
    }
}