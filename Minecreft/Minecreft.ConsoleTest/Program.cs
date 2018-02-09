using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Minecreft.ConsoleTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("请选择Mode：");
			Console.WriteLine("1 - 服务器");
			Console.WriteLine("2 - 客户端");
			var serverMode = false;
			while (true)
			{
				var key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.D1)
				{
					serverMode = true;
					Console.WriteLine("选择Mode：服务器");
					break;
				}
				else if (key.Key == ConsoleKey.D2)
				{
					serverMode = false;
					Console.WriteLine("选择Mode：客户端");
					break;
				}
				else
				{
					Console.WriteLine("select mode try again");
				}
			}
			string packetTypeStr = "Message";
			System.Net.IPEndPoint iPEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("192.168.1.102"), 5555);
			if (serverMode)
			{
				//有连接进来
				NetworkComms.AppendGlobalConnectionEstablishHandler(connection =>
				{
					Console.WriteLine("有连接进来");
				});
				//有消息进来
				NetworkComms.AppendGlobalIncomingPacketHandler<string>(packetTypeStr,
					(packetHeader, connection, incomingString) =>
				{
					//Console.WriteLine("\n  ... Incoming message from " + connection.ToString() + " saying '" + incomingString + "'.");
					Console.WriteLine("客户端发来的数据：" + incomingString);
					NetworkComms.SendObject<string>(packetTypeStr, ((System.Net.IPEndPoint)connection.ConnectionInfo.RemoteEndPoint).Address.ToString(),
						((System.Net.IPEndPoint)connection.ConnectionInfo.RemoteEndPoint).Port,
						"这是发送给客户端的数据");
				});
				//AppendGlobalIncomingUnmanagedPacketHandler
				NetworkComms.AppendGlobalIncomingUnmanagedPacketHandler(
					(packetHeader, connection, bytes) =>
				{
					Console.WriteLine("AppendGlobalIncomingUnmanagedPacketHandler");
				});
				//RemoveGlobalConnectionCloseHandler
				NetworkComms.RemoveGlobalConnectionCloseHandler((connection) =>
				{
					Console.WriteLine("RemoveGlobalConnectionCloseHandler");
				});
				//RemoveGlobalIncomingPacketHandler
				NetworkComms.RemoveGlobalIncomingPacketHandler<string>(packetTypeStr,
					(packetHeader, connection, incomingObject) =>
					{
						Console.WriteLine("RemoveGlobalIncomingPacketHandler");
					});
				//RemoveGlobalIncomingUnmanagedPacketHandler
				NetworkComms.RemoveGlobalIncomingUnmanagedPacketHandler<string>((packetHeader, connection, incomingObject) =>
				{
					Console.WriteLine("RemoveGlobalIncomingUnmanagedPacketHandler");
				});

				//有连接关闭
				NetworkComms.AppendGlobalConnectionCloseHandler(connection =>
				{
					Console.WriteLine("连接关闭");
				});

				Connection.StartListening(ConnectionType.TCP, iPEndPoint);
				Console.WriteLine("Listening for TCP messages on:");
				foreach (System.Net.IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
				{
					Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
				}
			}
			else
			{
				TCPConnection conn = TCPConnection.GetConnection(new ConnectionInfo(iPEndPoint.Address.ToString(), iPEndPoint.Port));

				//处理服务器发来的消息
				conn.AppendIncomingPacketHandler<string>(packetTypeStr, (packetHeader, connection, message) =>
				{
					Console.WriteLine("接收服务器的数据：" + message);
				});
				//AppendIncomingUnmanagedPacketHandler
				conn.AppendIncomingUnmanagedPacketHandler((packetHeader, connection, incomingObject) =>
				{
					Console.WriteLine("AppendIncomingUnmanagedPacketHandler");
				});
				//AppendShutdownHandler
				conn.AppendShutdownHandler((connection) =>
				{
					//客户端与服务器断开时的处理
					Console.WriteLine("客户端与服务器断开时的处理");
				});

				//conn.RemoveIncomingPacketHandler();
				//conn.RemoveIncomingUnmanagedPacketHandler();

				//RemoveShutdownHandler
				conn.RemoveShutdownHandler((connection) =>
				{
					Console.WriteLine("RemoveShutdownHandler");
				});
				for (int i = 0; i < 2; i++)
				{
					conn.SendObject<string>(packetTypeStr, "测试数据" + i);
					Thread.Sleep(200);
				}
				//NetworkComms.Shutdown();
			}
			Console.ReadKey();
		}
	}
}
