namespace BlazorApp1.Authentication
{
    public class UserAccountService
    {
        private List<UserAccount> users;    //Fetch data from DB instead of implementing List (list for test purposes)

        public UserAccountService()
        {
            /*users = new List<UserAccount>
            {
                new UserAccount{UserName="admin",Password="admin",Role="Admininstrator"},
                new UserAccount {UserName="user",Password="user",Role="User"}
            };*/

        }

        public UserAccount? GetByUserName(string userName)
        {
            return users.FirstOrDefault(x => x.UserName == userName); ;
        }
    }
}
