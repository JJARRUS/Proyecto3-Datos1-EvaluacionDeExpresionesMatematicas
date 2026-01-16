using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Client.WinForms;

/// <summary>
/// Cliente TCP para conectarse al servidor de calculadora.
/// Task 28: Implementar el cliente TCP capaz de iniciar una conexión con el servidor.
/// Task 30: Implementar el envío de expresiones matemáticas desde el cliente al servidor.
/// </summary>
public sealed class TcpApiClient
{
	private readonly string _host;
	private readonly int _port;

	public TcpApiClient(string host, int port)
	{
		_host = host;
		_port = port;
	}

	/// <summary>
	/// Task 30: Envía una expresión matemática en notación postfija (RPN) al servidor y recibe el resultado.
	/// Protocolo: Envía "EVAL <expresión>" y espera respuesta "OK <resultado>" o "ERR <mensaje>".
	/// </summary>
	/// <param name="rpn">Expresión en notación postfija, ej: "3 4 +"</param>
	/// <returns>Resultado numérico de la evaluación</returns>
	public async Task<int> EvalAsync(string rpn)
	{
		using var client = new TcpClient();
		await client.ConnectAsync(_host, _port);
		
		using var stream = client.GetStream();
		using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
		using var reader = new StreamReader(stream, Encoding.UTF8);

		// Enviar comando EVAL con la expresión RPN al servidor
		await writer.WriteLineAsync($"EVAL {rpn}");
		
		// Leer respuesta del servidor
		var response = await reader.ReadLineAsync();

		if (response == null)
			throw new Exception("Servidor no respondió.");

		// Parsear respuesta exitosa: "OK <resultado>"
		if (response.StartsWith("OK ", StringComparison.OrdinalIgnoreCase))
			return int.Parse(response.Substring(3).Trim());

		// Parsear respuesta de error: "ERR <mensaje>"
		if (response.StartsWith("ERR ", StringComparison.OrdinalIgnoreCase))
			throw new Exception(response.Substring(4).Trim());

		throw new Exception("Respuesta inválida del servidor.");
	}

	/// <summary>
	/// Obtiene el historial de operaciones del servidor en formato CSV.
	/// </summary>
	/// <returns>Lista de líneas CSV del historial</returns>
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
			var line = await reader.ReadLineAsync();
			if (line == null) break;
			if (line == "END") break;
			lines.Add(line);
		}

		return lines;
	}

	/// <summary>
	/// Versión raw que devuelve la respuesta completa del servidor (para compatibilidad).
	/// </summary>
	public async Task<string> EvaluateRawAsync(string expression)
	{
		using var client = new TcpClient();
		await client.ConnectAsync(_host, _port);
		
		using var stream = client.GetStream();
		using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
		using var reader = new StreamReader(stream, Encoding.UTF8);

		await writer.WriteLineAsync($"EVAL {expression}");
		var response = await reader.ReadLineAsync();

		if (response == null)
			throw new InvalidOperationException("El servidor no respondió");

		return response;
	}
}

