using Calculator.Server;

var port = 5000;

var server = new CalculatorServer(port);
server.Start();

Console.WriteLine("Presione CTRL+C para cerrar el servidor");
Console.ReadLine();
