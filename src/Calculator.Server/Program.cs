// US5: Punto de entrada del servidor TCP
using Calculator.Server;

// Puerto TCP en el que escuchará el servidor
var port = 5000;

// Crear e iniciar el servidor
var server = new CalculatorServer(port);
server.Start();

Console.WriteLine("Presione CTRL+C para cerrar el servidor");
Console.ReadLine();