using System.ComponentModel.DataAnnotations;

namespace ConnectFourDBCore
{
    public class Game
    {
        [Key]
        public int gameId { get; set; }

        public int winner { get; set; } // is equal to 0 if draw
                                        //or to user id if there is a winner
        public virtual User user1 { get; set; }

        public virtual User user2 { get; set; }
    }
}