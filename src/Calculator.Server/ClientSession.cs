using System;
using System.Net;

namespace Calculator.Server;

#nullable enable

/// US7-Task 38: Estructura que identifica de forma única cada conexión de cliente.
/// Permite rastrear sesiones independientes en el servidor concurrente.

public class ClientSession
{
    /// Identificador único para esta sesión de cliente (GUID)
    public Guid SessionId { get; }

    /// Dirección IP del cliente remoto
    public IPAddress? ClientAddress { get; }

    /// Puerto del cliente remoto
    public int ClientPort { get; }

    /// Dirección IP local del servidor (donde se aceptó la conexión)
    public IPAddress? LocalAddress { get; }

    /// Puerto local del servidor
    public int LocalPort { get; }

    /// Fecha y hora en UTC cuando se estableció la conexión
    public DateTime ConnectedAt { get; }

    /// Contador de comandos procesados en esta sesión
    public int CommandCount { get; private set; }

    /// Constructor: crea una sesión con información de conexión TCP
    public ClientSession(IPAddress? clientAddr, int clientPort, IPAddress? localAddr, int localPort)
    {
        SessionId = Guid.NewGuid();
        ClientAddress = clientAddr;
        ClientPort = clientPort;
        LocalAddress = localAddr;
        LocalPort = localPort;
        ConnectedAt = DateTime.UtcNow;
        CommandCount = 0;
    }

    /// Incrementa el contador de comandos procesados
    public void IncrementCommandCount() => CommandCount++;

    /// Retorna una representación legible de la sesión
    public override string ToString()
    {
        return $"[Session {SessionId:N}] Client={ClientAddress}:{ClientPort} Local={LocalAddress}:{LocalPort} ConnectedAt={ConnectedAt:O}";
    }
}
