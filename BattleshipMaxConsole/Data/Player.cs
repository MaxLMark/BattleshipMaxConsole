using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipMaxConsole.Data
{
    public class Player
    {
        public string Name { get; set; }
        public string HostAddress { get; set; }
        public string Port { get; set; }
        public static string[,] Grid { get; set; }
        public static string[,] OpponentGrid { get; set; }
    }
}
