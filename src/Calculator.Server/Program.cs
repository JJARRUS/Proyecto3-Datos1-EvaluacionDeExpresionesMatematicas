// US5: Punto de entrada del servidor TCP
using System;
using System.IO;
using System.Threading;
using Calculator.Server;

// Puerto TCP en el que escuchará el servidor
var port = 5000;

// Ruta del archivo CSV para el historial
var csvPath = Path.Combine(AppContext.BaseDirectory, "data", "operations.csv");

// Configurar cancelación solo con CTRL+C explícito
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
	e.Cancel = true;
	cts.Cancel();
	Console.WriteLine("Servidor cancelado por CTRL+C");
};

// Crear e iniciar el servidor
var server = new CalculatorServer(port, csvPath);
Console.WriteLine($"Servidor escuchando en TCP {port}. CSV: {csvPath}");
Console.WriteLine("Presiona CTRL+C para detener...");

try
{
	await server.RunAsync(cts.Token);
}
catch (OperationCanceledException)
{
	Console.WriteLine("Servidor detenido");
}

