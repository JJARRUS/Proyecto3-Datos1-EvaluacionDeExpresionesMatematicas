using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Client.WinForms;

/// Cliente TCP para conectarse al servidor de calculadora.
/// Task 28: Implementar el cliente TCP capaz de iniciar una conexión con el servidor.
/// Task 30: Implementar el envío de expresiones matemáticas desde el cliente al servidor.
/// Task 31: Implementar la recepción de la respuesta enviada por el servidor al cliente.
public sealed class TcpApiClient
{
	private readonly string _host;
	private readonly int _port;

	public TcpApiClient(string host, int port)
	{
		_host = host;
		_port = port;
	}

	/// Task 31: Recibe y procesa la respuesta del servidor.
	/// Envía "EVAL <expresión>" y espera respuesta "OK <resultado>" o "ERR <mensaje>".

	/// "rpn" Expresión en notación postfija, ej: "3 4 +
	/// Resultado numérico de la evaluación
	public async Task<int> EvalAsync(string rpn)
	{
		using var client = new TcpClient();
		await client.ConnectAsync(_host, _port);
		
		using var stream = client.GetStream();
		using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
		using var reader = new StreamReader(stream, Encoding.UTF8);

		await writer.WriteLineAsync($"EVAL {rpn}");
		
		// Task 31: Recibir respuesta del servidor desde el stream
		var response = await reader.ReadLineAsync();

		if (response == null)
			throw new Exception("Servidor no respondió.");

		// Task 31: Procesar respuesta exitosa: "OK <resultado>"
		if (response.StartsWith("OK ", StringComparison.OrdinalIgnoreCase))
			return int.Parse(response.Substring(3).Trim());

		// Task 31: Procesar respuesta de error: "ERR <mensaje>"
		if (response.StartsWith("ERR ", StringComparison.OrdinalIgnoreCase))
			throw new Exception(response.Substring(4).Trim());

		throw new Exception("Respuesta inválida del servidor.");
	}
	/// Task 31: Recibe el historial de operaciones del servidor.
	/// Lee líneas CSV hasta recibir "END".
	/// Lista de líneas CSV del historial
	public async Task<List<string>> HistAsync()
	{
		using var client = new TcpClient();
		await client.ConnectAsync(_host, _port);
		
		using var stream = client.GetStream();
		using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
		using var reader = new StreamReader(stream, Encoding.UTF8);

		await writer.WriteLineAsync("HIST");

		var lines = new List<string>();
		while (true)
		{
			// Task 31: Recibir cada línea CSV del historial
			var line = await reader.ReadLineAsync();
			if (line == null) break;
			if (line == "END") break; // Marcador de fin del historial
			lines.Add(line);
		}

		return lines;
	}

	/// Versión raw que devuelve la respuesta completa del servidor (para compatibilidad).
	/// Task 31: Recibe la respuesta sin procesar.
	public async Task<string> EvaluateRawAsync(string expression)
	{
		using var client = new TcpClient();
		await client.ConnectAsync(_host, _port);
		
		using var stream = client.GetStream();
		using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
		using var reader = new StreamReader(stream, Encoding.UTF8);

		await writer.WriteLineAsync($"EVAL {expression}");
		
		// Task 31: Recibir respuesta del servidor
		var response = await reader.ReadLineAsync();

		if (response == null)
			throw new InvalidOperationException("El servidor no respondió");

		return response;
	}
}

