﻿using BattleshipMaxConsole.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BattleshipMaxConsole.Data
{
    public class Game
    {
        Dictionary<int, string> responses = new Dictionary<int, string>()
        {
            {210, "210 BATTLESHIP/1.0" },
            {220, "220 <remote player name>" },
            {221, "221 Client Starts" },
            {222, "222 Host Starts" },
            {230, "230 Miss!" },
            {241, "241 You hit my Carrier" },
            {242, "242 You hit my Battleship" },
            {243, "243 You hit my Destroyer" },
            {244, "244 You hit my Submarine" },
            {245, "245 You hit my Patrol Boat" },
            {251, "251 You sunk my Carrier" },
            {252, "252 You sunk my Battleship" },
            {253, "253 You sunk my Destroyer" },
            {254, "254 You sunk my Submarine" },
            {255, "255 You sunk my Patrol Boat" },
            {260, "260 You win!" },
            {270, "270 Connection closed" },
            {500, "500 Syntax error" },
            {501, "501 Sequence error" }
        };
        TcpListener _listener;
        private List<Ship> _ships = new List<Ship>()
        {
            new Ship()
            {
                Name = "Carrier",
                Health = 5,
                Coordinates = new string[]{ "0.3", "0.4", "0.5", "0.6", "0.7" }

            },
            new Ship()
            {
                Name = "Battleship",
                Health = 4,
                Coordinates = new string[]{"1.6","1.7","1.8","1.9"}

            },
            new Ship()
            {
                Name = "Destroyer",
                Health = 3,
                Coordinates = new string[]{"3.3","3.4","3.5"}
            },
            new Ship()
            {
                Name = "Submarine",
                Health = 3,
                Coordinates = new string[]{"5.1","5.2","5.3"}
            },
            new Ship()
            {
                Name = "Patrol Boat",
                Health = 2,
                Coordinates = new string[]{"6.3","6.4"}
            },
        };
        private List<string> _targets = new List<string>();
        private List<string> _confirmedHits = new List<string>();
        private List<string> _confirmedMisses = new List<string>();
        private int PlayerHealth = 17;
        public string User { get; set; }
        public string Opponent { get; set; }
        public string[,] CreatePlayerGrid()
        {
            var grid = new string[10, 10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var currentCoordinate = i + "." + j;
                    var hit = _targets.Where(x => x == currentCoordinate);

                    var shipDetected = _ships.Where(x => x.Coordinates.Contains(currentCoordinate)).FirstOrDefault();

                    if (hit != null)
                    {
                        grid[i, j] = "H";
                    }
                    if (shipDetected != null)
                    {
                        var nameArray = shipDetected.Name.ToCharArray();

                        grid[i, j] = $"{nameArray[0]}";
                    }
                    else if (shipDetected != null && hit != null)
                    {
                        shipDetected.Health--;
                        grid[i, j] = "X";
                    }
                    else
                    {

                        grid[i, j] = "~";

                    }
                }
            }

            return grid;
        }
        public string[,] CreateEnemyGrid()
        {
            var grid = new string[10, 10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    //hit or miss
                    grid[i, j] = "~";
                }
            }

            return grid;
        }
        public void StartListen(int port)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                Console.WriteLine($"Starts listening on port: {port}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Failed to open socket. Probably already taken.");
                Environment.Exit(1);
            }
        }
        public void Host()
        {
            while (true)
            {
                Console.WriteLine("Waiting for opponent...");


                using (var client = _listener.AcceptTcpClient())
                using (var networkStream = client.GetStream())
                using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
                using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
                {
                    var turn = 0;
                    Console.WriteLine($"Client has connected {client.Client.RemoteEndPoint}!");
                    writer.WriteLine(responses[210]);
                    TcpMessage(true, responses[210]);

                    while (client.Connected)
                    {
                        turn++;

                        var command = reader.ReadLine();
                        TcpMessage(false, command);
                        
                        if (turn == 1)
                        {
                            Opponent = command.Substring(5);
                            writer.WriteLine("220 " + User);
                            TcpMessage(true, "220 " + User);
                            break;
                        }

                        if (string.Equals(command.Substring(0,5), "START",StringComparison.InvariantCultureIgnoreCase))
                        {
                            //TODO: fix logic for randomize player to start!!!
                            Random rnd = new Random();
                            var playerToStart = rnd.Next(1, 2);
                            if (playerToStart == 1)
                            {
                                var uStartMessage = "222 I " + User + ", will start!";
                                //222
                                //You start
                                writer.WriteLine(uStartMessage);
                                TcpMessage(true, uStartMessage);
                                var yourFirstturn = Console.ReadLine();
                                writer.WriteLine(yourFirstturn);
                                TcpMessage(false, yourFirstturn);
                                break;
                            }
                            else if (playerToStart == 2)
                            {
                                //221
                                //Opponent start
                                var oStartMessage = "221 You, player " + Opponent + ", will start!";
                                writer.WriteLine(oStartMessage);
                                TcpMessage(true, oStartMessage);
                                break;
                            }

                        }

                        if (string.Equals(command.Substring(0, 4), "FIRE", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //check if hit????
                            var response = ConvertAndAddTarget(command.Substring(5, 2));
                            //Add target
                            writer.WriteLine(response);
                            TcpMessage(true, response);
                            break;
                        }
                        if (responses.Any(x => x.Key == int.Parse(command.Substring(0, 3))))
                        {
                            //IF MATCH??? write message
                            TcpMessage(true, responses[int.Parse(command.Substring(0, 3))]);
                            break;
                        }

                        if (string.Equals(command, "QUIT", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //TODO: Shut down!
                            writer.WriteLine(responses[270]);
                            TcpMessage(true, responses[270]);
                            break;
                        }

                        if (string.Equals(command, "DATE", StringComparison.InvariantCultureIgnoreCase))
                        {
                            writer.WriteLine(DateTime.UtcNow.ToString("o"));
                            break;
                        }

                        writer.WriteLine($"UNKNOWN COMMAND: {command}");
                    }
                }

            }

        }
        public void ConnectToServer(string hostAdress, string hostPort)
        {
            using (var client = new TcpClient(hostAdress, int.Parse(hostPort)))
            using (var networkStream = client.GetStream())
            using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
            using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
            {
                Console.WriteLine($"Connected to {client.Client.RemoteEndPoint}");
                while (client.Connected)
                {
                    Console.WriteLine("Enter text to send: (Write QUIT to end plix)");
                    var text = Console.ReadLine();

                    if (text == "QUIT") break;

                    // Skicka text
                    writer.WriteLine(text);

                    if (!client.Connected) break;

                    // Läs minst en rad
                    do
                    {
                        var line = reader.ReadLine();
                        Console.WriteLine($"Answere: {line}");

                    } while (networkStream.DataAvailable);

                };

            }

        }
        public void Play(string hostAdress, string hostPort)
        {


            if (hostAdress == "")
            {
                if (hostPort != "")
                {
                    StartListen(int.Parse(hostPort));
                    Host();
                }
                else
                {
                    Console.WriteLine("Something went reeaaally wrong because you did not enter port");
                }
            }
            else if (hostAdress != "" && hostPort != "")
            {
                ConnectToServer(hostAdress, hostPort);
            }


        }
        public void TcpMessage(bool blue, string message)
        {
            if (blue)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine(message);
            Console.ResetColor();

        }
        public string ConvertAndAddTarget(string target)
        {
            //Lägg till targets
            var targetToAdd = ParseLocation(target);
            var shipHit = _ships.Where(s => s.Coordinates.Contains(targetToAdd)).FirstOrDefault();

            if (_targets.Contains(targetToAdd))
            {
                //target already exists????

                return "503";

            }
            _targets.Add(targetToAdd);

            if (shipHit != null)
            {

                PlayerHealth--;
                if (PlayerHealth == 0)
                {
                    return responses[260];
                }

                if (shipHit.Name == "Carrier")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        return responses[241];

                    }
                    return responses[251];

                }
                if (shipHit.Name == "Battleship")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        return responses[242];

                    }
                    return responses[252];
                }
                if (shipHit.Name == "Destroyer")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        return responses[243];

                    }
                    return responses[253];
                }
                if (shipHit.Name == "Submarine")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        return responses[244];

                    }
                    return responses[254];
                }
                if (shipHit.Name == "Patrol Boat")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        return responses[245];

                    }
                    return responses[255];
                }
            }
            return responses[230];
        }
        public string ParseLocation(string location)
        {
            var i = location.Substring(0, 1);
            var rest = "." + location.Substring(1, 1);
            switch (i)
            {
                case "A":
                    return "0" + rest;
                case "B":
                    return "1" + rest;
                case "C":
                    return "2" + rest;
                case "D":
                    return "3" + rest;
                case "E":
                    return "4" + rest;
                case "F":
                    return "5" + rest;
                case "G":
                    return "6" + rest;
                case "H":
                    return "7" + rest;
                case "I":
                    return "8" + rest;
                case "J":
                    return "9" + rest;
                default:
                    return "false location woot";
            }

        }

    }
}
