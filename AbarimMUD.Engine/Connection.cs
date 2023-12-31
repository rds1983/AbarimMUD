﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NLog;
using AbarimMUD.Utils;

namespace AbarimMUD
{
	public sealed class Connection
	{
		private readonly Logger _logger;

		private readonly Socket _socket;
		private readonly byte[] _buffer = new byte[1024];
		private readonly StringBuilder _inputBuffer = new StringBuilder();
		private bool _alive = true;

		public Socket Socket
		{
			get { return _socket; }
		}

		public string LoggerName
		{
			get
			{
				var remote = (IPEndPoint)_socket.RemoteEndPoint;
				return string.Format("{0}", remote.Address);
			}
		}

		public Logger Logger
		{
			get { return _logger; }
		}

		public event EventHandler<GenericEventArgs<string>> Received;

		public Connection(Socket socket)
		{
			if (socket == null)
			{
				throw new ArgumentNullException("socket");
			}

			_socket = socket;
			_logger = LogManager.GetLogger(LoggerName);
			_socket.BeginReceive(_buffer, 0, _buffer.Length, 0, ReadCallback, null);
		}

		private void ReadCallback(IAsyncResult ar)
		{
			try
			{
				string content;

				// Retrieve the state object and the handler socket
				// from the asynchronous state object.

				// Read data from the client socket. 
				var bytesRead = _socket.EndReceive(ar);
				do
				{
					if (bytesRead <= 0)
					{
						_alive = false;
						break;
					}
					var rawData = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
					if (rawData.Contains("Core.Supports.Set"))
					{
						// Ignore this data, as it comes from mudlet bug
						break;
					}
					_inputBuffer.Append(Encoding.ASCII.GetString(_buffer, 0, bytesRead));

					// Check for end-of-file tag. If it is not there, read 
					// more data.
					content = _inputBuffer.ToString();
					var rlPos = content.IndexOf("\n");
					if (rlPos == -1)
					{
						break;
					}
					// '\n' ends a command
					var data = content.Substring(0, rlPos).Trim();

					_logger.Info("Incomming data: '{0}'", data);

					var ev = Received;
					if (ev != null)
					{
						ev(this, new GenericEventArgs<string>(data));
					}

					// Reset input buffer
					_inputBuffer.Clear();
				} while (false);

				if (_alive)
				{
					_socket.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(ReadCallback), null);
				}
				else
				{
					_logger.Info("Connection has been closed by the remote host");
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
		}

		public void Send(string data)
		{
			if (!_alive)
			{
				_logger.Warn("Try to send to disconnected client: '{0}'", data);
				return;
			}

			_logger.Info("Sending to client: '{0}'", data);

			var result = Encoding.UTF8.GetBytes(data);
			_socket.Send(result);
		}

		public void Disconnect()
		{
			_logger.Info("Closing connection...");
			_alive = false;
			_socket.Disconnect(true);
			_logger.Info("Connection has been closed");
		}
	}
}