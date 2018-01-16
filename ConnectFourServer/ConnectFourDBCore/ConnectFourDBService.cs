using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourDBCore
{
    public class ConnectFourDBService
    {

        #region user_authentication
        public bool CheckIfValidLogin(string username, string password)
        {
            using (var db = new ConnectFourContext())
            {
                if (db.Users.Any(c => c.userName == username && c.password == password))
                {
                    return true;
                }
                return false;
            }


        }

        public void RegisterUser(string username, string password)
        {
            using (var db = new ConnectFourContext())
            {
                if (db.Users.Any(u => u.userName == username))
                {
                    throw new UserNameAlreadyExistsException();
                }

                User user = new User
                {
                    userName = username,
                    password = password
                };
                db.Users.Add(user);
                db.SaveChanges();
            }
        }
        #endregion



        #region game_logic
        //Input: username that indicates the user that is just connected 
        //Output: list of users returned by username which is waiting for partner for playing
        public IEnumerable<String> getConnectedUsersByUsernameThatWaitingForPartner(string username)
        {
            using (var db = new ConnectFourContext())
            {
                List<String> usersByUsername = (from u in db.Users
                                                where u.userName != username && u.status == USER_STATUS.WAITING_FOR_ENEMY
                                                select u.userName).ToList();

                return usersByUsername;
            }
        }
        #endregion

        #region exceptions
        [Serializable]
        private class UserNameAlreadyExistsException : Exception
        {
            public UserNameAlreadyExistsException()
            {
            }

            public UserNameAlreadyExistsException(string message) : base(message)
            {
            }

            public UserNameAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected UserNameAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
        #endregion
    }
}
