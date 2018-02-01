using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourDBCore
{
    public enum USER_STATUS
    {
        OTHER,
        PLAYING,
    }
    public class User
    {

        [Key]
        public int userId { get; set; }

        public string userName { get; set; }

        public string password { get; set; }

        public int numberOfGames { get; set; }

        public int numberOfWins { get; set; }

        public int numberOfLoses { get; set; }

        public USER_STATUS status { get; set; }
        
        public int numberOfPoints { get; set; }

        public virtual ICollection<Game> Games { get; set; }
    }
}
