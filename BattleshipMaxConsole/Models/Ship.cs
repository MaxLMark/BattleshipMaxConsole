using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipMaxConsole.Models
{
    public class Ship
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public string[] Coordinates { get; set; }
    }
}
