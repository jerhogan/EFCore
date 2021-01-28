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
        public static int currentUserId = 0;
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
        public static void LoginUser()
        {
            string username;
            string password;
            string encrypted;
            bool loginSuccess;

            do
            {
                Logger.OutputInformation("Please Log-in.");
                username = GetConsoleString("user name");
                password = ReadPasswordLine ("password");
                encrypted = Encode_Decode.Encrypt(password);
                /*                string decrypted = Encode_Decode.Decrypt(encrypted);
                                Console.WriteLine("password = " + password + " encrypted = " + encrypted + " decrypted = " + decrypted);*/
                LoginUserCommand c = new LoginUserCommand(username, encrypted);
                loginSuccess =  c.Apply();
                Commands.Add(c);

            }
            while (!loginSuccess);
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

            RegisterNewUserCommand c = new RegisterNewUserCommand(FirstName,
                MiddleName, Surname, Encrypted, UserName, Email);
            c.Apply();
            Commands.Add(c);
        }
        public static void EmailUserPassword()
        {
            string userName;

            userName = GetConsoleString("User Name for new email password");

            EmailUserPasswordCommand c = new EmailUserPasswordCommand(userName);
            c.Apply();
            Commands.Add(c);
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

            ChangeUserPasswordCommand c = new ChangeUserPasswordCommand(userName, 
                encryptedPassword, newEncryptedPassword, repeatEncryptedPassword);
            c.Apply();
            Commands.Add(c);

        }
        public static void DownPage()
        {
            DownPageCommand c = new DownPageCommand();
            c.Apply();
            Commands.Add(c);

            _Screens.DisplayBooksScreen();
        }
        public static void UpPage()
        {
            UpPageCommand c = new UpPageCommand();
            c.Apply();
            Commands.Add(c);

            _Screens.DisplayBooksScreen();
        }
        public static void DownOne()
        {
            DownOneCommand c = new DownOneCommand();
            c.Apply();
            Commands.Add(c);

            _Screens.DisplayBooksScreen();
        }
        public static void UpOne()
        {
            UpOneCommand c = new UpOneCommand();
            c.Apply();
            Commands.Add(c);

            _Screens.DisplayBooksScreen();
        }
        public static void Home()
        {
            HomeCommand c = new HomeCommand();
            c.Apply();
            Commands.Add(c);

            _Screens.DisplayBooksScreen();
        }
        public static void End()
        {
            EndCommand c = new EndCommand();
            c.Apply();
            Commands.Add(c);

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
                JumpToCommand c = new JumpToCommand(bookRowNo);
                c.Apply();
                Commands.Add(c);
                _Screens.DisplayBooksScreen();
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
            OrderCommand c = new OrderCommand(fieldType, orderType);
            c.Apply();
            Commands.Add(c);
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
            FilterCommand c = new FilterCommand(fieldType, searchString);
            c.Apply();
            Commands.Add(c);
            _Screens.DisplayBooksScreen();
        }
        public static void ConvertWebPage()
        {
            string inputFile;
            string readerType;
            string bookTypeString = "BT_KINDLE";

            Logger.OutputInformation("Please Convert Web Page File to Import File for Database.");
            inputFile = GetConsoleString("Input Web Page File Name");

            readerType = GetConsoleString("Convert ((A)mazon Kindle, Rakuten (K)obo, (W)ishlist or (G)oodReads");
            if (readerType.Length > 0)
            {
                switch (readerType.ToUpper()[0])
                {
                    case 'A':
                        bookTypeString = "BT_KINDLE";
                        break;

                    case 'K':
                        bookTypeString = "BT_KOBO";
                        break;

                    case 'G':
                        bookTypeString = "BT_GOOD_READS";
                        break;

                    case 'W':
                        bookTypeString = "BT_WISH_LIST";
                        break;

                    default:
                        // ignore
                        break;
                }
                ConvertCommand c = new ConvertCommand(inputFile, bookTypeString);
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

            ImportCommand c = new ImportCommand(inputFile);
            c.Apply();
            Commands.Add(c);

            Logger.OutputInformation("Import file {0} processed", inputFile);
        }
        public static void XmlSave()
        {
            string outputFile;

            Logger.OutputInformation("Please Save XML File from database File for Database.");
            outputFile = GetConsoleString("XML File Name");

            XmlSaveCommand c = new XmlSaveCommand(outputFile);
            c.Apply();
            Commands.Add(c);

            Logger.OutputInformation("XML file {0} processed", outputFile);
        }
        public static void XmlLoad()
        {
            string inputFile;

            Logger.OutputInformation("Please Load XML File from database File for Database.");
            inputFile = GetConsoleString("XML File Name");

            XmlLoadCommand c = new XmlLoadCommand(inputFile);
            c.Apply();
            Commands.Add(c);

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
            DeleteCommand c = new DeleteCommand(fieldType, bookRowNo, delValue);
            c.Apply();
            Commands.Add(c);
        }
        static public void CreateBook()
        {
            string query, user, bookType, tagValues, readString;
            bool read;
            bool success;

            Logger.OutputInformation("Please Create a Book in the Database.");

            query = GetConsoleString("Book Query String").Replace(",", ";");
            user = GetConsoleString("User Name").Replace(",", ";");
            bookType = GetConsoleString("Book Type").Replace(",", ";");
            success = Int32.TryParse(bookType, out int bookTypeRowNo);
            if (!success)
                bookTypeRowNo = 1;
            readString = GetConsoleString("Read Already (Y/N)").Replace(",", ";");
            read = readString.Length > 0 && readString.ToUpper()[0] == 'Y';
            tagValues = GetConsoleString("Tag Values").Replace(",", ";");

            CreateBookCommand c = new CreateBookCommand(query, user, bookTypeRowNo, read, tagValues);
            c.Apply();
            Commands.Add(c);
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

            UpdateBookCommand c = new UpdateBookCommand(fieldType, bookRowNo, newValue);
            c.Apply();
            Commands.Add(c);
        }

        static public void FillInEmptyTags()
        {
            Logger.OutputInformation("Please Fill in empty tags in the Database.");

            FillInEmptyTagsCommand c = new FillInEmptyTagsCommand();
            c.Apply();
            Commands.Add(c);
        }
        static public void DisplayCounts()
        {
            Logger.OutputInformation("Please Display Counts in Database.");

            string bookFieldType;
            FieldType fieldType = FieldType.fAuthor;

            do
            {
                bookFieldType = GetConsoleString("(A)uthor, (T)ag, (B)ook Type, (U)ser, (R)eador (*)all");
            } while (bookFieldType.ToUpper()[0] != 'A' && bookFieldType.ToUpper()[0] != 'T' &&
                     bookFieldType.ToUpper()[0] != 'B' && bookFieldType.ToUpper()[0] != 'U' &&
                     bookFieldType.ToUpper()[0] != 'R' && bookFieldType.ToUpper()[0] != '*');
            switch (bookFieldType.ToUpper()[0])
            {
                case 'A':
                    fieldType = FieldType.fAuthor;
                    break;

                case 'T':
                    fieldType = FieldType.fTag;
                    break;

                case 'B':
                    fieldType = FieldType.fBookType;
                    break;

                case 'U':
                    fieldType = FieldType.fUser;
                    break;

                case 'R':
                    fieldType = FieldType.fRead;
                    break;

                case '*':
                    fieldType = FieldType.fAll;
                    break;

                default:
                    // Ignore
                    break;
            }

            DisplayCountsCommand c = new DisplayCountsCommand(fieldType);
            c.Apply();
            Commands.Add(c);
        }
        static public void DisplayVersion()
        {
            Logger.OutputInformation("Please Display the Software Version.");

            DisplayVersionCommand c = new DisplayVersionCommand();
            c.Apply();
            Commands.Add(c);
        }
    }
}
