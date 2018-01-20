using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourServer
{
    public class PlayingPlayers
    {
        //Player 1 starts the game because player1 sent the request
        //Player 1 play with red circle and player2 with black
        public string Player1 { get; set; }
        public IConnectFourServiceCallback CallBackPlayer1 { get; set; }
        public string Player2 { get; set; }
        public IConnectFourServiceCallback CallBackPlayer2 { get; set; }

    }
}
