# Protocolo de Mensajes

## EVAL (US8 Task 41)

- Descripción: El cliente envía una expresión matemática en notación postfija (RPN) para que el servidor la evalúe.
- Transporte: TCP sobre _UTF-8_.
- Framing: Una línea por mensaje, terminada en `\n` (o `\r\n`).
  
	En TCP los datos viajan como un flujo continuo de bytes, sin límites naturales entre “mensajes”. Para que el servidor sepa dónde termina un comando y empieza el siguiente, cada petición se envía como una línea completa de texto. Esto significa que, tras el contenido del comando (por ejemplo `EVAL 2 3 +`), se añade un salto de línea. El servidor lee usando una operación de “leer línea” y no procesará la petición hasta que reciba ese final de línea. Se aceptan `\n` (convención Unix) y `\r\n` (convención Windows).
  
	Ejemplo de mensaje completo: `EVAL 2 3 +\n` o `EVAL 2 3 +\r\n`.
    
- Formato de petición: `EVAL <expresión_RPN>`
	- `EVAL` en mayúsculas (no sensible a mayúsculas/minúsculas en el servidor).
	- `<expresión_RPN>`: tokens separados por espacios.
	- Tokens válidos:
		- Números: enteros o decimales con punto (`.`), p. ej. `2`, `3.5`.
		- Operadores: `+`, `-`, `*`, `/`.
	- Espacios extra se ignoran al inicio/final; los internos separan tokens.

### Semántica RPN

- La expresión se procesa en una pila:
	- Números: se apilan.
	- Operador binario: desapila dos operandos (`left`, `right`) y apila el resultado de `left op right`.
- La pila debe terminar con un único valor; si hay tokens insuficientes o sobran, es error de sintaxis.

## Respuesta del servidor

- Éxito: `OK <resultado>`
	- `<resultado>` se devuelve como número (puede ser decimal).
- Error: `ERR <mensaje>`
	- `<mensaje>` describe el problema (sintaxis inválida, división por cero, etc.).

## Ejemplos

- Petición: `EVAL 2 3 +`  → Respuesta: `OK 5`
- Petición: `EVAL 10 5 -` → Respuesta: `OK 5`
- Petición: `EVAL 4 2 *`  → Respuesta: `OK 8`
- Petición: `EVAL 6 3 /`  → Respuesta: `OK 2`

## Notas

- El servidor soporta `HIST` para historial: envía líneas CSV y finaliza con `END`.
- El codificador es `UTF-8` y se recomienda enviar una línea por comando.


## Codificación, UTF-8 y HIST

- Qué es encoding: el mecanismo que convierte texto en bytes para poder enviarlo por la red o guardarlo en archivos, y viceversa. Cada carácter (p. ej., `A`, `ñ`) se representa como una secuencia de bytes.
- Por qué UTF-8: es una codificación universal (Unicode), compatible con ASCII, eficiente para textos comunes y estándar en la web y sistemas modernos. Evita problemas de interpretación de caracteres entre Windows/Linux/macOS.
- Regla del protocolo: todos los mensajes (peticiones y respuestas) se codifican en UTF-8. Cada comando se envía como una línea completa terminada en salto de línea (`\n` o `\r\n`) para delimitar el final del mensaje.
- Ejemplo práctico: el cliente envía `EVAL 2 3 +` y añade el salto de línea; el servidor lee la línea en UTF-8 y responde `OK 5` con su propio salto de línea.


- Qué es HIST: El cliente solicita el historial completo de operaciones evaluadas por el servidor.
- Formato de petición: `HIST` (una sola palabra, no requiere argumentos).
- Respuesta del servidor: 
  - Envía cada entrada del historial como una línea CSV: `timestamp,sessionId,expresión,resultado`.
  - Finaliza con la línea `END` para indicar que no hay más datos.
- Ejemplo:
  ```
  Cliente: HIST
  Servidor: 2026-01-18T06:54:23.2871328Z,a1b2c3d4-...,2 3 +,5
  Servidor: 2026-01-18T06:54:23.4347263Z,e5f6g7h8-...,4 2 *,8
  Servidor: END
  ```

## EndPoints TCP

En el contexto de TCP, un **EndPoint** representa la combinación de dirección IP y puerto que identifica uno de los extremos de una conexión. Toda conexión TCP tiene dos EndPoints: uno para el cliente y otro para el servidor.

El **RemoteEndPoint** corresponde al cliente que inició la conexión. Desde la perspectiva del servidor, este endpoint es "remoto" porque representa la máquina que se conectó desde otro lugar de la red. Por ejemplo, si un cliente se conecta desde la IP `192.168.1.100` usando el puerto `54321`, ese será su RemoteEndPoint. El servidor usa esta información para identificar de dónde proviene cada conexión y registrar sesiones individuales por cliente.

Por otro lado, el **LocalEndPoint** representa al servidor mismo: la dirección IP y el puerto donde está escuchando conexiones entrantes. Si el servidor está configurado para escuchar en `0.0.0.0:5000`, significa que acepta conexiones en el puerto `5000` desde cualquier interfaz de red disponible. Este endpoint es "local" porque describe dónde está recibiendo el servidor las conexiones.

En código C#, ambos endpoints se obtienen accediendo al `Socket` subyacente del `TcpClient`. La propiedad `client.Client` devuelve el socket TCP de bajo nivel, y desde ahí se pueden consultar `RemoteEndPoint` y `LocalEndPoint`. Como estos devuelven objetos genéricos de tipo `EndPoint`, se realiza un casteo a `IPEndPoint` para poder acceder a las propiedades `.Address` (la dirección IP) y `.Port` (el puerto numérico). Ejemplo:

```csharp
var remoteEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
var localEndpoint = client.Client.LocalEndPoint as IPEndPoint;
```

Esto permite al servidor identificar con precisión cada sesión de cliente y registrar información como IP, puerto, y hora de conexión.