using websiteLogin.Models;
using System.Linq;

namespace ContosoUniversity.Data
{
    public static class DbInitializer
    {
        //The DbInitializer is only used for test purposes
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            //Insert data here
            var users = new User[]
            {
            //new User{UserName="test", UserPassword="test"}
            };
            var ToDoLists = new ToDoList[]
            {
                //new ToDoList{Beskrivelse="test"}
            };

            //Adds data to the database
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            foreach (ToDoList u in ToDoLists)
            {
                context.ToDoLists.Add(u);
            }
            context.SaveChanges();
        }
    }
}