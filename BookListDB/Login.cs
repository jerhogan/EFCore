using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace BookListDB
{
    public class Login
    {
        static private Screens _Screens = null;

        public void SetupScreens(Screens screens)
        {
            _Screens = screens;
            Commands.SetScreens(screens);
        }
        public static string GetConsoleString(string prompt)
        {
            string ret_value;

            Console.Write("Please enter " + prompt + ":");
            ret_value = Console.ReadLine();
            return (ret_value);
        }

        public static string ReadPasswordLine(string prompt)
        {
            Console.Write("Please enter " + prompt + ":");
            string pass = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Enter)
                {
                    if (!(key.KeyChar < ' '))
                    {
                        pass += key.KeyChar;
                        Console.Write("*");
                    }
                    else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        Console.Write(Convert.ToChar(ConsoleKey.Backspace));
                        pass = pass.Remove(pass.Length - 1);
                        Console.Write(" ");
                        Console.Write(Convert.ToChar(ConsoleKey.Backspace));
                    }
                }
            } while (key.Key != ConsoleKey.Enter);
            
            Console.WriteLine();

            return pass;
        }
        public static void SetContext(BookListContext context)
        {
            Commands.SetContext(context);

            _Screens.Initialise(context);

            _Screens.ReadScreenIds();
        }

        public struct BookTypeType
        {
            public string bkString;
            public int shopping_list_no;
        }
        static BookTypeType GetBookType(string prefix)
        {
            string readerType;
            BookTypeType ret;
            ret.bkString = "BT_KINDLE";
            ret.shopping_list_no = 0;

            readerType = GetConsoleString(prefix + " ((A)mazon Kindle, Rakuten (K)obo, (W)ishlist, (G)oodReads, G(o)ogle, A(d)obe, (P)aper, (H)ard-back or (S)hopping List");
            if (readerType.Length > 0)
            {
                switch (readerType.ToUpper()[0])
                {
                    case 'A':
                        ret.bkString = "BT_KINDLE";
                        break;

                    case 'K':
                        ret.bkString = "BT_KOBO";
                        break;

                    case 'G':
                        ret.bkString = "BT_GOOD_READS";
                        break;

                    case 'W':
                        ret.bkString = "BT_WISH_LIST";
                        break;

                    case 'O':
                        ret.bkString = "BT_GOOGLE";
                        break;

                    case 'D':
                        ret.bkString = "BT_ADOBE";
                        break;

                    case 'P':
                        ret.bkString = "BT_PAPER";
                        break;

                    case 'H':
                        ret.bkString = "BT_HARD";
                        break;

                    case 'S':
                        bool success;

                        ret.bkString = "BT_SHOPPING_LIST";
                        success = Int32.TryParse(GetConsoleString("Shopping List Number"), out ret.shopping_list_no);

                        if (!success)
                        {
                            Logger.OutputError("Book row no entered was not in numeric format");
                        }
                        break;

                    default:
                        // ignore
                        break;
                }
            }
            return (ret);
        }
            
        public static void LoginUser()
        {
            string username;
            string password;
            string encrypted;
            bool loginSuccess;
            const int allowedRetries = 3;
            int retries = 1;

            do
            {
                Logger.OutputInformation("Please Log-in.");
                username = GetConsoleString("user name");
                password = ReadPasswordLine ("password");
                encrypted = Encode_Decode.Encrypt(password);
                /*                string decrypted = Encode_Decode.Decrypt(encrypted);
                                Console.WriteLine("password = " + password + " encrypted = " + encrypted + " decrypted = " + decrypted);*/
                Factory factory = new Factory();
                List<object> pars = new List<object>();
                pars.Add(username);
                pars.Add(encrypted);
                LoginUserCommand c = (LoginUserCommand) factory.GetCommand(CommandType.Login, pars);
                if (c != null)
                {
                    loginSuccess =  c.Apply();
                    Commands.Add(c);
                }
                else
                    loginSuccess = false;
                if (!loginSuccess)
                {
                    Logger.OutputInformation("Failed to login " + username);
                }
                ++retries;
            }
            while (!loginSuccess && retries <= allowedRetries);
            if (!loginSuccess)
            {
                Logger.OutputInformation("Failure to login max " + allowedRetries + " attempts allowed.");
            }

            _Screens.DisplayBooksScreen();
        }

        public static void RegisterNewUser()
        {
            string FirstName;
            string MiddleName;
            string Surname;
            string Password;
            string UserName;
            string Email;
            string Encrypted;

            Logger.OutputInformation("Please Register a New User.");
            FirstName = GetConsoleString("First Name");
            MiddleName = GetConsoleString("Middle Name");
            Surname = GetConsoleString("Surname");
            Password = ReadPasswordLine("Password");
            UserName = GetConsoleString("User Name");
            Email = GetConsoleString("Email");
            Encrypted = Encode_Decode.Encrypt(Password);

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(FirstName);
            pars.Add(MiddleName);
            pars.Add(Surname);
            pars.Add(Encrypted);
            pars.Add(UserName);
            pars.Add(Email);
            RegisterNewUserCommand c = (RegisterNewUserCommand)factory.GetCommand(CommandType.RegisterNewUser, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }
        public static void EmailUserPassword()
        {
            string userName;

            userName = GetConsoleString("User Name for new email password");

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(userName);
            EmailUserPasswordCommand c = (EmailUserPasswordCommand)factory.GetCommand(CommandType.EmailUserPassword, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }
        public static void ChangeUserPassword()
        {
            string userName;
            string password;
            string encryptedPassword;
            string newPassword;
            string newEncryptedPassword;
            string repeatPassword;
            string repeatEncryptedPassword;
            

            Logger.OutputInformation("Please Change User Password.");

            userName = GetConsoleString("User Name");
            password = ReadPasswordLine("Old Password");
            encryptedPassword = Encode_Decode.Encrypt(password);
            newPassword = ReadPasswordLine("New Password");
            newEncryptedPassword = Encode_Decode.Encrypt(newPassword);
            repeatPassword = ReadPasswordLine("Repeat New Password");
            repeatEncryptedPassword = Encode_Decode.Encrypt(repeatPassword);

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(userName);
            pars.Add(encryptedPassword);
            pars.Add(newEncryptedPassword);
            pars.Add(repeatEncryptedPassword);
            ChangeUserPasswordCommand c = (ChangeUserPasswordCommand)factory.GetCommand(CommandType.ChangeUserPassword, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }
        public static void DownPage()
        {
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            DownPageCommand c = (DownPageCommand)factory.GetCommand(CommandType.DownPage, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            _Screens.DisplayBooksScreen();
        }
        public static void UpPage()
        {
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            UpPageCommand c = (UpPageCommand)factory.GetCommand(CommandType.UpPage, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            _Screens.DisplayBooksScreen();
        }
        public static void DownOne()
        {
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            DownOneCommand c = (DownOneCommand)factory.GetCommand(CommandType.DownOne, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            _Screens.DisplayBooksScreen();
        }
        public static void UpOne()
        {
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            UpOneCommand c = (UpOneCommand)factory.GetCommand(CommandType.UpOne, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            _Screens.DisplayBooksScreen();
        }
        public static void Home()
        {
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            HomeCommand c = (HomeCommand)factory.GetCommand(CommandType.Home, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            _Screens.DisplayBooksScreen();
        }
        public static void End()
        {
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            EndCommand c = (EndCommand)factory.GetCommand(CommandType.End, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            _Screens.DisplayBooksScreen();
        }
        public static void PrintBookEntry()
        {
            bool success;

            Logger.OutputInformation("Please Print Some Books from Database.");

            success = Int32.TryParse(GetConsoleString("Book Row Number"), out int bookRowNo);

            if (!success)
            {
                Logger.OutputError("Book row no entered was not in numeric format");
            }
            else
            {
                Factory factory = new Factory();
                List<object> pars = new List<object>();
                pars.Add(bookRowNo);
                JumpToCommand c = (JumpToCommand)factory.GetCommand(CommandType.JumpTo, pars);
                if (c != null)
                {
                    c.Apply();
                    Commands.Add(c);
                    _Screens.DisplayBooksScreen();
                }
            }
        }

        public static void Order()
        {
            string fieldTypeString, orderTypeString;
            FieldType fieldType;
            OrderType orderType;

            Logger.OutputInformation("Please Order Some Books from Database.");
            fieldTypeString = GetConsoleString("Order What (A)uthor, (I)ndex, (T)itle, ta(G), (B)ook Type, or (R)ead");
            orderTypeString = GetConsoleString("What order? (A)scending, or (D)escending");
            orderType = OrderType.Ascending;
            if (orderTypeString.Length > 0)
            {
                if (orderTypeString.ToUpper()[0] == 'D')
                    orderType = OrderType.Descending;
            }
            fieldType = FieldType.fIndex;
            if (fieldTypeString.Length > 0)
            {
                switch (fieldTypeString.ToUpper()[0])
                {
                    case 'I':
                        fieldType = FieldType.fIndex;
                        break;

                    case 'T':
                        fieldType = FieldType.fTitle;
                        break;

                    case 'A':
                        fieldType = FieldType.fAuthor;
                        break;

                    case 'G':
                        fieldType = FieldType.fTag;
                        break;

                    case 'B':
                        fieldType = FieldType.fBookType;
                        break;

                    case 'R':
                        fieldType = FieldType.fRead;
                        break;

                    default:
                        // ignore
                        break;
                }
            }
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(fieldType);
            pars.Add(orderType);
            OrderCommand c = (OrderCommand)factory.GetCommand(CommandType.Order, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            _Screens.DisplayBooksScreen();
        }
        public static void Filter()
        {
            string fieldTypeString;
            FieldType fieldType = FieldType.fTitle;

            Logger.OutputInformation("Please Filter Some Books from Database.");
            fieldTypeString = GetConsoleString("Filter by (A)uthor, (T)itle, ta(G), (B)ook Type or (Read)");
            if (fieldTypeString.Length > 0)
            {
                switch (fieldTypeString.ToUpper()[0])
                {
                    case 'T':
                        fieldType = FieldType.fTitle;
                        break;

                    case 'A':
                        fieldType = FieldType.fAuthor;
                        break;

                    case 'G':
                        fieldType = FieldType.fTag;
                        break;

                    case 'B':
                        fieldType = FieldType.fBookType;
                        break;

                    case 'R':
                        fieldType = FieldType.fRead;
                        break;

                    default:
                        // ignore
                        break;
                }
            }
            string searchString;

            searchString = GetConsoleString("Enter Search String");
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(fieldType);
            pars.Add(searchString);
            FilterCommand c = (FilterCommand) factory.GetCommand(CommandType.Filter, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
            _Screens.DisplayBooksScreen();
        }
        public static void ConvertWebPage()
        {
            string inputFile;
            BookTypeType book_type;

            Logger.OutputInformation("Please Convert Web Page File to Import File for Database.");
            inputFile = GetConsoleString("Input Web Page File Name");

            book_type = GetBookType("Convert Web Page");
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(inputFile);
            pars.Add(book_type.bkString);
            pars.Add(book_type.shopping_list_no);
            ConvertCommand c = (ConvertCommand)factory.GetCommand(CommandType.Convert, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
            Logger.OutputInformation("Input file {0} processed", inputFile);
        }
        public static void Import()
        {
            string inputFile;

            Logger.OutputInformation("Please Import File for Database.");
            inputFile = GetConsoleString("Import File Name");

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(inputFile);
            ImportCommand c = (ImportCommand)factory.GetCommand(CommandType.Import, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            Logger.OutputInformation("Import file {0} processed", inputFile);
        }
        public static void XmlSave()
        {
            string outputFile;

            Logger.OutputInformation("Please Save XML File from database File for Database.");
            outputFile = GetConsoleString("XML File Name");

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(outputFile);
            XmlSaveCommand c = (XmlSaveCommand)factory.GetCommand(CommandType.XmlSave, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            Logger.OutputInformation("XML file {0} processed", outputFile);
        }
        public static void XmlLoad()
        {
            string inputFile;

            Logger.OutputInformation("Please Load XML File from database File for Database.");
            inputFile = GetConsoleString("XML File Name");

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(inputFile);
            XmlLoadCommand c = (XmlLoadCommand)factory.GetCommand(CommandType.XmlLoad, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }

            Logger.OutputInformation("XML file {0} processed", inputFile);
        }
        public static void DeleteBook()
        {
            bool success;
            FieldType fieldType = FieldType.fAll;
            string delValue = "";

            Logger.OutputInformation("Please Delete a Book from the Database.");

            success = Int32.TryParse(GetConsoleString("Book Row Number"), out int bookRowNo);

            if (!success)
            {
                Logger.OutputError("Book row no entered was not in numeric format");
                return;
            }
            string bookFieldType;

            do
            {
                bookFieldType = GetConsoleString("(B)ook, (A)uthor, Ta(G)");
            } while (bookFieldType.ToUpper()[0] != 'B' && bookFieldType.ToUpper()[0] != 'A' &&
                        bookFieldType.ToUpper()[0] != 'G');

            switch (bookFieldType.ToUpper()[0])
            {
                case 'B':
                    fieldType = FieldType.fAll;
                    break;

                case 'A':
                    delValue = GetConsoleString("Author Name to Delete");
                    fieldType = FieldType.fAuthor;
                    break;

                case 'G':
                    delValue = GetConsoleString("Tag Name to Delete");
                    fieldType = FieldType.fTag;
                    break;

                default:
                    // ignore
                    break;
            }
            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(fieldType);
            pars.Add(bookRowNo);
            pars.Add(delValue);
            DeleteCommand c = (DeleteCommand)factory.GetCommand(CommandType.Delete, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }
        static public void CreateBook()
        {
            string query, user, tagValues, readString;
            bool read;
            BookTypeType book_type;

            Logger.OutputInformation("Please Create a Book in the Database.");

            query = GetConsoleString("Book Query String").Replace(",", ";");
            user = GetConsoleString("User Name").Replace(",", ";");
            book_type = GetBookType("Create Book Type");
            readString = GetConsoleString("Read Already (Y/N)").Replace(",", ";");
            read = readString.Length > 0 && readString.ToUpper()[0] == 'Y';
            tagValues = GetConsoleString("Tag Values").Replace(",", ";");

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(query);
            pars.Add(user);
            pars.Add(book_type.bkString);
            pars.Add(book_type.shopping_list_no);
            pars.Add(read);
            pars.Add(tagValues);
            CreateBookCommand c = (CreateBookCommand)factory.GetCommand(CommandType.CreateBook, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }

        public static void UpdateBook()
        {
            bool success;

            Logger.OutputInformation("Please Update a Book from the Database.");

            success = Int32.TryParse(GetConsoleString("Book Row Number"), out int bookRowNo);

            if (!success)
            {
                Logger.OutputError("Book row no entered was not in numeric format");
                return;
            }

            string bookFieldType;
            FieldType fieldType = FieldType.fTitle;

            do
            {
                bookFieldType = GetConsoleString("(T)itle, (A)uthor, or ta(G)");
            } while (bookFieldType.ToUpper()[0] != 'A' && bookFieldType.ToUpper()[0] != 'T' &&
                     bookFieldType.ToUpper()[0] != 'G');

            string newValue = "";

            switch (bookFieldType.ToUpper()[0])
            {
                case 'A':
                    fieldType = FieldType.fAuthor;
                    newValue = GetConsoleString("Author Name to Add");
                    break;

                case 'T':
                    fieldType = FieldType.fTitle;
                    newValue = GetConsoleString("Title to Add");
                    break;

                case 'G':
                    fieldType = FieldType.fTag;
                    newValue = GetConsoleString("Tag Name to Add");
                    break;

                default:
                    // Ignore
                    break;
            }

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(fieldType);
            pars.Add(bookRowNo);
            pars.Add(newValue);
            UpdateBookCommand c = (UpdateBookCommand)factory.GetCommand(CommandType.UpdateBook, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }

        static public void FillInEmptyTags()
        {
            Logger.OutputInformation("Please Fill in empty tags in the Database.");

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            FillInEmptyTagsCommand c = (FillInEmptyTagsCommand)factory.GetCommand(CommandType.FillInEmptyTags, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }
        static public void DisplayCounts()
        {
            Logger.OutputInformation("Please Display Counts in Database.");

            string bookFieldType;
            FieldType fieldType1 = FieldType.fAuthor;
            FieldType fieldType2 = FieldType.fBookType;

            do
            {
                bookFieldType = GetConsoleString("(A)uthor, (T)ag, (B)ook Type, (U)ser, (R)ead or (*)all");
            } while (bookFieldType.ToUpper()[0] != 'A' && bookFieldType.ToUpper()[0] != 'T' &&
                     bookFieldType.ToUpper()[0] != 'B' && bookFieldType.ToUpper()[0] != 'U' &&
                     bookFieldType.ToUpper()[0] != 'R' && bookFieldType.ToUpper()[0] != '*');
            switch (bookFieldType.ToUpper()[0])
            {
                case 'A':
                    fieldType1 = FieldType.fAuthor;
                    break;

                case 'T':
                    fieldType1 = FieldType.fTag;
                    break;

                case 'B':
                    fieldType1 = FieldType.fBookType;
                    break;

                case 'U':
                    fieldType1 = FieldType.fUser;
                    break;

                case 'R':
                    fieldType1 = FieldType.fRead;
                    break;

                case '*':
                    fieldType1 = FieldType.fAll;
                    break;

                default:
                    // Ignore
                    break;
            }

            do
            {
                bookFieldType = GetConsoleString("Second (B)ook Type, (R)ead, (N)one or (*)all");
            } while (bookFieldType.ToUpper()[0] != 'B' && bookFieldType.ToUpper()[0] != 'R' &&
                     bookFieldType.ToUpper()[0] != 'N' && bookFieldType.ToUpper()[0] != '*');
            switch (bookFieldType.ToUpper()[0])
            {
                case 'B':
                    fieldType2 = FieldType.fBookType;
                    break;

                case 'R':
                    fieldType2 = FieldType.fRead;
                    break;

                case 'N':
                    fieldType2 = FieldType.fNone;
                    break;

                case '*':
                    fieldType2 = FieldType.fAll;
                    break;

                default:
                    // Ignore
                    break;
            }

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            pars.Add(fieldType1);
            pars.Add(fieldType2);
            DisplayCountsCommand c = (DisplayCountsCommand) factory.GetCommand(CommandType.DisplayCounts, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }
        static public void DisplayVersion()
        {
            Logger.OutputInformation("Please Display the Software Version.");

            Factory factory = new Factory();
            List<object> pars = new List<object>();
            DisplayVersionCommand c = (DisplayVersionCommand)factory.GetCommand(CommandType.DisplayVersion, pars);
            if (c != null)
            {
                c.Apply();
                Commands.Add(c);
            }
        }
    }
}
