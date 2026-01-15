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

    public void Start()
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        Console.WriteLine($"Servidor TCP escuchando en el puerto {_port}");
    }
}