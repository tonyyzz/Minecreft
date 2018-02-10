using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.Connections.UDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Minecreft.ConsoleTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("请选择Mode：");
			Console.WriteLine("1 - TCP服务器");
			Console.WriteLine("2 - UDP服务器");
			Console.WriteLine("3 - 客户端");
			int serverMode = 1;


			while (true)
			{
				var key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.D1)
				{
					serverMode = 1;
					break;
				}
				else if (key.Key == ConsoleKey.D2)
				{
					serverMode = 2;
					break;
				}
				else if (key.Key == ConsoleKey.D3)
				{
					serverMode = 3;
					break;
				}
				else
				{
					Console.WriteLine("select mode try again");
				}
			}

			switch (serverMode)
			{
				case 1: { Console.WriteLine("选择Mode：TCP服务器"); } break;
				case 2: { Console.WriteLine("选择Mode：UDP服务器"); } break;
				case 3: { Console.WriteLine("选择Mode：客户端"); } break;
				default:
					break;
			}
			string packetTypeStr = "Message";
			int tcpPort = 5788;
			int udpPort = 5789;

			//IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, port); //服务器测试（通过）

			//IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("47.94.21.115"), port); //客户端连接测试

			//IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port); //服务器测试（行不通）
			//IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("::1"), port); //服务器测试（行不通）
			switch (serverMode)
			{
				case 1: //TCP
					{

						IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, tcpPort);
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
								Console.WriteLine("TCP客户端发来的数据：" + incomingString);
								NetworkComms.SendObject<string>(packetTypeStr, ((System.Net.IPEndPoint)connection.ConnectionInfo.RemoteEndPoint).Address.ToString(),
							((System.Net.IPEndPoint)connection.ConnectionInfo.RemoteEndPoint).Port,
							"这是TCP服务器发送给TCP客户端的数据");
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
						Console.WriteLine("TCP服务器等待客户端连接...");
					}
					break;
				case 2: //UDP
					{
						bool hasReceived = false;
						IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, udpPort);
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
								Console.WriteLine("UDP客户端发来的数据：" + incomingString);
								UDPConnection.SendObject<string>(packetTypeStr, "这是UDP服务器发送给UDP客户端的数据",
									(System.Net.IPEndPoint)connection.ConnectionInfo.RemoteEndPoint,
									NetworkComms.DefaultSendReceiveOptions,
									ApplicationLayerProtocolStatus.Enabled);

								if (!hasReceived)
								{
									hasReceived = true;
									ThreadPool.QueueUserWorkItem(o =>
									{
										Thread.Sleep(3000);

										NetworkComms.Shutdown();
										//NetworkComms.CloseAllConnections(ConnectionType.UDP);
									});
								}
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

						Connection.StartListening(ConnectionType.UDP, iPEndPoint);
						Console.WriteLine("Listening for TCP messages on:");
						foreach (System.Net.IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.UDP))
						{
							Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
						}
						Console.WriteLine("等待UDP客户端连接...");
					}
					break;
				case 3: //Client
					{
						IPEndPoint tcpIPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), tcpPort);
						TCPConnection tcpCconn = TCPConnection.GetConnection(new ConnectionInfo(tcpIPEndPoint.Address.ToString(), tcpIPEndPoint.Port));

						//处理服务器发来的消息
						tcpCconn.AppendIncomingPacketHandler<string>(packetTypeStr, (packetHeader, connection, message) =>
						{
							Console.WriteLine("接收TCP服务器的数据：" + message);
						});
						//AppendIncomingUnmanagedPacketHandler
						tcpCconn.AppendIncomingUnmanagedPacketHandler((packetHeader, connection, incomingObject) =>
						{
							Console.WriteLine("AppendIncomingUnmanagedPacketHandler");
						});
						//AppendShutdownHandler
						tcpCconn.AppendShutdownHandler((connection) =>
						{
							//客户端与服务器断开时的处理
							Console.WriteLine("TCP客户端与TCP服务器断开时的处理");
						});

						//RemoveShutdownHandler
						//tcpCconn.RemoveShutdownHandler((connection) =>
						//{
						//	Console.WriteLine("RemoveShutdownHandler");
						//});

						//屏蔽掉tcp消息
						//tcpCconn.RemoveIncomingPacketHandler();
						//tcpCconn.RemoveIncomingUnmanagedPacketHandler();



						ThreadPool.QueueUserWorkItem(o =>
						{
							for (int i = 0; i < 10; i++)
							{
								tcpCconn.SendObject<string>(packetTypeStr, "TCP测试数据" + i);
								Thread.Sleep(20);
							}
						});







						IPEndPoint udpIPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), udpPort);

						UDPConnection udpCconn = null;


						udpCconn = UDPConnection.GetConnection(new ConnectionInfo(udpIPEndPoint.Address.ToString(), udpIPEndPoint.Port), UDPOptions.Handshake);

						//处理服务器发来的消息
						udpCconn.AppendIncomingPacketHandler<string>(packetTypeStr, (packetHeader, connection, message) =>
						{
							Console.WriteLine("接收UDP服务器的数据：" + message);
						});
						//AppendIncomingUnmanagedPacketHandler
						udpCconn.AppendIncomingUnmanagedPacketHandler((packetHeader, connection, incomingObject) =>
						{
							Console.WriteLine("AppendIncomingUnmanagedPacketHandler");
						});
						//AppendShutdownHandler
						udpCconn.AppendShutdownHandler((connection) =>
						{
							//客户端与服务器断开时的处理
							Console.WriteLine("UDP客户端与UDP服务器断开时的处理");
						});
						//屏蔽掉udp消息
						//udpCconn.RemoveIncomingPacketHandler();
						//udpCconn.RemoveIncomingUnmanagedPacketHandler();

						//RemoveShutdownHandler
						//udpCconn.RemoveShutdownHandler((connection) =>
						//{
						//	Console.WriteLine("RemoveShutdownHandler");
						//});
						ThreadPool.QueueUserWorkItem(o =>
						{
							for (int i = 0; i < 10; i++)
							{
								udpCconn.SendObject<string>(packetTypeStr, "UDP测试数据" + i);
								Thread.Sleep(20);
							}
							//udpCconn.CloseConnection(false);
						});

					}
					break;
				default:
					break;
			}
			Console.ReadKey();
		}
	}
}
