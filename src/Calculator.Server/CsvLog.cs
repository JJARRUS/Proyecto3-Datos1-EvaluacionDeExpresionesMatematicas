using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Calculator.Server;

/// Utilidad mínima para registrar y leer historial en CSV.
/// Implementación sencilla para evitar errores en el servidor.
public static class CsvLog
{
	/// Agrega una entrada al archivo CSV. Crea el directorio si no existe.
	public static void Append(string path, DateTime utc, string expr, double result)
	{
		try
		{
			var dir = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var line = string.Format("{0},{1},{2}", utc.ToString("o"), expr, result);

			// Abrir en modo append con UTF-8, compartido para lectura
			using var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
			using var writer = new StreamWriter(fs, new UTF8Encoding(false));
			writer.WriteLine(line);
		}
		catch
		{
			// Implementación mínima: ignora errores de IO para no interrumpir el servidor.
		}
	}

	/// Lee todas las líneas del historial CSV, o vacío si no existe.
	public static IEnumerable<string> ReadAllLines(string path)
	{
		try
		{
			return File.Exists(path) ? File.ReadLines(path, Encoding.UTF8) : Array.Empty<string>();
		}
		catch
		{
			// Implementación mínima: en caso de error, devolver vacío.
			return Array.Empty<string>();
		}
	}
}

