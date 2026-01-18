using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Calculator.Server;


/// US7 Task 39: Utilidad para registrar y leer historial en CSV.
/// Thread-safe: usa lock para prevenir condiciones de carrera entre múltiples clientes concurrentes.

public static class CsvLog
{
	// Lock estático para sincronizar acceso al archivo CSV entre múltiples clientes
	private static readonly object _fileLock = new object();

	/// Agrega una entrada al archivo CSV. Crea el directorio si no existe.
	/// Thread-safe: múltiples clientes pueden llamar concurrentemente sin corrupción de datos.

	public static void Append(string path, DateTime utc, string expr, double result)
	{
		// Task 39: lock asegura que solo un cliente escriba al CSV a la vez
		lock (_fileLock)
		{
			try
			{
				var dir = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
					Directory.CreateDirectory(dir);

				var line = string.Format("{0},{1},{2}", utc.ToString("o"), expr, result);

				// Abrir en modo append con UTF-8
				using var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
				using var writer = new StreamWriter(fs, new UTF8Encoding(false));
				writer.WriteLine(line);
			}
			catch
			{
				// Implementación mínima: ignora errores de IO para no interrumpir el servidor.
			}
		}
	}


	/// Lee todas las líneas del historial CSV, o vacío si no existe.
	/// Thread-safe: garantiza lectura consistente incluso si otro cliente está escribiendo.

	public static IEnumerable<string> ReadAllLines(string path)
	{
		// Task 39: lock asegura que la lectura no interfiera con escrituras concurrentes
		lock (_fileLock)
		{
			try
			{
				// Leer todas las líneas dentro del lock para obtener snapshot consistente
				return File.Exists(path) ? File.ReadAllLines(path, Encoding.UTF8) : Array.Empty<string>();
			}
			catch
			{
				// Implementación mínima: en caso de error, devolver vacío.
				return Array.Empty<string>();
			}
		}
	}
}

