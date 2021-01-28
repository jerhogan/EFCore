using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    class Program
    {
        public static void Main()
        {
            Logger.OutputInformation("Starting JH Book List Console Application.");
            Screens screens = new Screens();

            var context = Initialize.GetContext();
            BookListContextExtensions.AddScreens(screens);
            Login logIn;
            logIn = new Login();
            logIn.SetupScreens(screens);
            Login.SetContext(context);
            context.EnsureSeedDataForContext();

            string cmdString;
            do
            {
                cmdString = Login.GetConsoleString("JHConsoleBookListCmd");
                if (cmdString.Length > 0)
                {
                    switch (cmdString.ToUpper()[0])
                    {
                        case 'R':
                            Login.RegisterNewUser();
                            break;

                        case 'L':
                            Login.LoginUser();
                            User user;
                            user = context.Users.FirstOrDefault(u => u.UserId == Login.currentUserId);
                            Logger.OutputInformation("Welcome user " + user.UserName + " !");
                            break;

                        case 'E':
                            Login.EmailUserPassword();
                            break;

                        case 'C':
                            Login.ChangeUserPassword();
                            break;

                        case 'P':
                            Login.PrintBookEntry();
                            break;

                        case 'Q':
                            // Just Quit
                            break;

                        case 'D':
                            Login.DownPage();
                            break;

                        case 'U':
                            Login.UpPage();
                            break;

                        case '-':
                            Login.DownOne();
                            break;

                        case '+':
                            Login.UpOne();
                            break;

                        case 'H':
                            Login.Home();
                            break;

                        case 'N':
                            Login.End();
                            break;

                        case 'O':
                            Login.Order();
                            break;

                        case 'F':
                            Login.Filter();
                            break;

                        case 'V':
                            Login.ConvertWebPage();
                            break;

                        case 'I':
                            Login.Import();
                            break;

                        case 'X':
                            Login.XmlSave();
                            break;

                        case 'A':
                            Login.XmlLoad();
                            break;

                        case 'T':
                            Login.UpdateBook();
                            break;

                        case '1':
                            Login.DeleteBook();
                            break;

                        case '2':
                            Login.CreateBook();
                            break;

                        case '*':
                            Login.FillInEmptyTags();
                            break;

                        case '#':
                            Login.DisplayCounts();
                            break;

                        case 'M':
                            Login.DisplayVersion();
                            break;

                        case '?':
                            Logger.OutputInformation ("(R)egister");
                            Logger.OutputInformation("(L)ogin");
                            Logger.OutputInformation("(E)mail New Password");
                            Logger.OutputInformation("(C)hange Password");
                            Logger.OutputInformation("(P)rint Book Details");
                            Logger.OutputInformation("Page (D)own");
                            Logger.OutputInformation("Page (U)p");
                            Logger.OutputInformation("(+) Line Up");
                            Logger.OutputInformation("(-) Line Up");
                            Logger.OutputInformation("(H)ome");
                            Logger.OutputInformation("e(N)d");
                            Logger.OutputInformation("(O)rder");
                            Logger.OutputInformation("(F)ilter");
                            Logger.OutputInformation("con(V)ert web page to import database file");
                            Logger.OutputInformation("(I)mport from list");
                            Logger.OutputInformation("(X)ml save to file");
                            Logger.OutputInformation("xml lo(A)d from file");
                            Logger.OutputInformation("1 - delete book");
                            Logger.OutputInformation("2 - create book");
                            Logger.OutputInformation("upda(T)e book");
                            Logger.OutputInformation("Fill in (*) empty tags");
                            Logger.OutputInformation("(#) Counts");
                            Logger.OutputInformation("(M)ajor/Minor Version Number");
                            Logger.OutputInformation("(?) Help");
                            Logger.OutputInformation("(Q)uit");
                                break;
                        default:
                            // Ignore
                            break;
                    }
                }
            }
            while (cmdString.Length == 0 || cmdString.ToUpper()[0] != 'Q');



            Logger.OutputInformation("Reached End");
            Console.ReadKey();
            Logger.OutputInformation("Exiting JH Book List Console Application.");
        }
    }

    /*
            // The console window
            public static void Main(String[] args)
            {

                Console.Title = "Secure Password v2";
                Console.WriteLine("Output---");
                Console.WriteLine("");

                Console.WriteLine("Password:  " + Global.strPassword);

                string strEncrypted = (Encrypt(Global.strPassword));
                Console.WriteLine("Encrypted: " + strEncrypted);

                string strDecrypted = (Decrypt(strEncrypted));
                Console.WriteLine("Decrypted: " + strDecrypted);

                Console.ReadKey();
            }



     */
}

