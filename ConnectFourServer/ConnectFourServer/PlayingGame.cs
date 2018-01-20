using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourServer
{
    public class PlayingGame
    {
        //contains the game board
        public ConnectFourBoard Board { get; set; }
        // contains the game start time
        public DateTime GameStartTime { get; set; }
        // equal to the player username that its his turn in game now,
        
        public string Turn { get; set; }
    }
}
