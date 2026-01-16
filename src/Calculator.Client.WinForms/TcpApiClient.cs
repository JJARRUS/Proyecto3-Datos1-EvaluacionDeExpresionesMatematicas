using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// US10 – Cliente TCP para integrar GUI con servidor
public class TcpApiClient : IDisposable
{
	private readonly string _serverIp;
	private readonly int _serverPort;
	private TcpClient? _client;
	private StreamReader? _reader;
	private StreamWriter? _writer;

	public TcpApiClient(string serverIp, int serverPort)
	{
		_serverIp = serverIp;
		_serverPort = serverPort;
	}

	private async Task ConnectAsync()
	{
		if (_client?.Connected == true) return;

		_client = new TcpClient();
		await _client.ConnectAsync(_serverIp, _serverPort);
		var stream = _client.GetStream();
		_reader = new StreamReader(stream, Encoding.UTF8);
		_writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
	}

	// Enviar expresión y obtener resultado
	public async Task<string> EvaluateRawAsync(string expression)
	{
		if (string.IsNullOrWhiteSpace(expression))
			throw new ArgumentException("La expresión no puede estar vacía");

		await ConnectAsync();
		await _writer!.WriteLineAsync($"EVAL {expression}");

		var response = await _reader!.ReadLineAsync();
		if (response == null)
			throw new InvalidOperationException("El servidor no respondió");

		return response;
	}

	public void Dispose()
	{
		_reader?.Dispose();
		_writer?.Dispose();
		_client?.Dispose();
	}
}

