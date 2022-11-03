using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using websiteLogin.Models;

namespace websiteLogin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private static int UserID;

        //Database context reference
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        //Views
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Signup()
        {
            return View();
        }
        public IActionResult ToDoList()
        {
            return View();
        }
        public IActionResult ShowToDoList()
        {
            //Gets todo lists based of userID
            var data = _db.ToDoLists.Where(s => s.UserId.Equals(UserID)).ToList();

            //Decrypts "Beskrivelse"
            foreach (var item in data)
            {
                item.Beskrivelse = Decrypt(item.Beskrivelse);
            }

            //Stores the data in viewbag for .cshtml 
            ViewBag.storedToDoList = data;
            return View();
        }

        //Logout
        public ActionResult Logout()
        {
            return RedirectToAction("Login");
        }

        //Login
        [HttpPost]
        public ActionResult Login(User _user)
        {
            //Hash input password for later compare
            var tempHashPassword = HashPassword(_user.UserPassword);

            //Get hash password
            var storedPassword = _db.Users.Where(s => s.UserName == _user.UserName).Select(x => x.UserPassword).FirstOrDefault();

            //Checks if there is a match with the database
            var data = _db.Users.Where(s => s.UserName.Equals(_user.UserName) && s.UserPassword.Equals(storedPassword)).ToList();

            //if data contains something
            if (data.Count() > 0 && tempHashPassword == storedPassword)
            {
                //Store userId for later use and change page
                UserID = data.FirstOrDefault().UserId;
                return RedirectToAction("ToDoList", UserID);
            }
            else
            {
                //Error for use in .cshtml
                ViewBag.error = "Login failed";
                return View();
            }
        }

        //Signup
        [HttpPost]
        public ActionResult Signup(User user)
        {
            //Checks if the username exist
            var check = _db.Users.FirstOrDefault(s => s.UserName == user.UserName);
            if (check == null)
            {
                //Hashing the password
                user.UserPassword = HashPassword(user.UserPassword);

                //Adds the user to the database and redirects to login page
                _db.Users.Add(user);
                _db.SaveChanges();
                return RedirectToAction("Login");
            }
            else
            {
                //Error for use in .cshtml
                ViewBag.error = "Username already exists";
                return View();
            }
        }

        //ToDoList
        [HttpPost]
        public ActionResult ToDoList(ToDoList toDoList)
        {
            //Checks if there is something in the "Beskrivelse" field
            var check = _db.ToDoLists.FirstOrDefault(s => s.Beskrivelse == toDoList.Beskrivelse);
            if (check == null)
            {
                //Encrypts the "Beskrivelse"
                toDoList.Beskrivelse = Encryption(toDoList.Beskrivelse);

                //Sets the userId then adds it to the database
                toDoList.UserId = UserID;
                _db.ToDoLists.Add(toDoList);
                _db.SaveChanges();
                return View();
            }
            else
            {
                //Error for use in .cshtml
                ViewBag.error = "fejl";
                return View();
            }
        }

        //Hashing - (User password only in this case)
        //SHA256 - (Secure Hash Algorithm)
        public string HashPassword(string str)
        {
            //Creates an instance of the default implementation of System.Security.Cryptography.SHA256
            var sha = SHA256.Create();

            //Encodes all the characters in the specified string into a sequence of bytes
            var asByteArray = Encoding.Default.GetBytes(str);

            //Computes the hash value for the specified byte array
            var hashedPassword = sha.ComputeHash(asByteArray);

            //And returns the hash value
            return Convert.ToBase64String(hashedPassword);
        }

        //Encryption - (ToDoList only in this case)
        //AES Algorithm
        public string Encryption(string str) 
        {
            //Sets the key
            string encryptionKey = "MAKV2SPBNI99212";

            //Encodes all the characters in the specified string into a sequence of bytes
            byte[] clearBytes = Encoding.Unicode.GetBytes(str);

            //Initializes a new instance
            using (Aes encryptor = Aes.Create())
            {
                //Initializes a new instance of the System.Security.Cryptography.Rfc2898DeriveBytes using a password and salt to derive the key.
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                //Sets the secret key based on pseudo-random key
                encryptor.Key = pdb.GetBytes(32);

                //Sets the initialization vector based on pseudo-random key
                encryptor.IV = pdb.GetBytes(16);

                //Initializes a new instance
                using (MemoryStream ms = new MemoryStream())
                {
                    //Initializes a new instance
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        //Writes a sequence of bytes to the current CryptoStream and advances the current position within the stream by the number of bytes written
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    //Converts the (stream contents to a byte array, regardless of the System.IO.MemoryStream.Position) to an string
                    str = Convert.ToBase64String(ms.ToArray());
                }
            }
            return str;
        }

        //Decryption - (ToDoList only in this case)
        //AES Algorithm
        private static string Decrypt(string str)
        {
            //Sets the SAME key as in the encryption fase
            string encryptionKey = "MAKV2SPBNI99212";

            //Converts the string to an byte array
            byte[] cipherBytes = Convert.FromBase64String(str);

            //Initializes a new instance
            using (Aes encryptor = Aes.Create())
            {
                //Same as in the encryption fase
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                //Initializes a new instance
                using (MemoryStream ms = new MemoryStream())
                {
                    //Initializes a new instance
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        //Same as in the encryption fase
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    //Gets the encoding
                    str = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return str;
        }
    }
}
