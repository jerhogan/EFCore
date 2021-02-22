using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
	public static class Commands
    {
		public static List<Command> commands = new List<Command> ();
		public static Screens screens;
		public static BookListContext _context;

		public static void SetScreens(Screens newScreens)
        {
			screens = newScreens;
        }
		public static void SetContext(BookListContext context)
		{
			_context = context;
		}
		public static void Add(Command cmd)
        {
			commands.Add(cmd);
			Logger.OutputInformation(cmd.Display() + " just executed.");
        }
    }
    abstract public class Command
    {
//        public Screens _screens;
        public int _index;
		public FieldType field;
		public FieldType field1;
		public FieldType field2;
		public OrderType order;
		public string query;
		public string bookType;
		public string inputFile;
		public string outputFile;
		public string newValue;
		public string userName;
		public bool read;
		public string tagValues;
		public string FirstName;
		public string MiddleName;
		public string Surname;
		public string Encrypted;
		public string NewEncrypted;
		public string RepeatEncrypted;
		public string Email;
		public string delValue;
		public int bookTypeRowNo;
		public int shoppingListNo;
		public int _bookRowNo;
		abstract public bool Apply();
		abstract public string Display();
	}

	public class JumpToCommand : Command
    {
		public JumpToCommand (int newBookIdNo)
        {
			_bookRowNo = newBookIdNo;
        }
        override public bool Apply()
        {
            Commands.screens.JumpTo(_bookRowNo);
            return (true);
        }
		override public string Display()
		{
			return ("Command JumpTo(" + _bookRowNo + ")");
		}
	}
	public class RegisterNewUserCommand : Command
    {
		public RegisterNewUserCommand (string FirstName, 
			string MiddleName, string Surname, string Encrypted, 
			string UserName, string Email)
        {
			this.FirstName = FirstName;
			this.MiddleName = MiddleName;
			this.Surname = Surname;
			this.Encrypted = Encrypted;
			this.userName = UserName;
			this.Email = Email;
		}
		override public bool Apply()
        {
            Commands.screens.RegisterNewUser(FirstName, 
			MiddleName, Surname, Encrypted, userName, Email);
            return (true);
        }
		override public string Display()
		{
			return ("Command RegisterNewUser(" + FirstName + ", " + MiddleName +
				    ", " + Surname + ", " + Encrypted + ", " + userName + ", " +
					Email + ")");
		}
	}
	public class LoginUserCommand : Command
    {
		public LoginUserCommand(string UserName, string Encrypted)
		{
			this.userName = UserName;
			this.Encrypted = Encrypted;
		}
		override public bool Apply()
        {
            return (Commands.screens.LoginUser(userName, Encrypted));
        }
		override public string Display()
		{
			return ("Command LoginUser");
		}
	}
	public class EmailUserPasswordCommand : Command
	{
		public EmailUserPasswordCommand(string userName)
        {
			this.userName = userName;
        }
		override public bool Apply()
		{
			Commands.screens.EmailUserPassword(userName);
			return (true);
        }
		override public string Display()
		{
			return ("Command EmailUserPassword");
		}
	}
	public class ChangeUserPasswordCommand : Command
	{
		public ChangeUserPasswordCommand (string userName, string encryptedPassword, 
			        string newEncryptedPassword, string repeatEncryptedPassword)
        {
			this.userName = userName;
			Encrypted = encryptedPassword;
			NewEncrypted = newEncryptedPassword;
			RepeatEncrypted = repeatEncryptedPassword;
        }
		override public bool Apply()
		{
			Commands.screens.ChangeUserPassword(userName, Encrypted, NewEncrypted,
				                                RepeatEncrypted);
			return (false);
        }
		override public string Display()
		{
			return ("Command ChangeUserPassword");
		}
	}
	public class DownPageCommand : Command
	{
		override public bool Apply()
		{
			Commands.screens.DownPage();
			return (true);
        }
		override public string Display()
		{
			return ("Command DownPage");
		}
	}
	public class UpPageCommand : Command
	{
		override public bool Apply()
		{
			Commands.screens.UpPage();
			return (true);
        }
		override public string Display()
		{
			return ("Command UpPage");
		}
	}
	public class DownOneCommand : Command
	{
		override public bool Apply()
		{
			Commands.screens.DownOne();
			return (true);
        }
		override public string Display()
		{
			return ("Command DownOne");
		}
	}
	public class UpOneCommand : Command
	{
		override public bool Apply()
		{
			Commands.screens.UpOne();
			return (true);
        }
		override public string Display()
		{
			return ("Command UpOne");
		}
	}
	public class HomeCommand : Command
	{
		override public bool Apply()
		{
			Commands.screens.Home();
			return (true);
        }
		override public string Display()
		{
			return ("Command Home");
		}
	}
	public class EndCommand : Command
	{
		override public bool Apply()
		{
			Commands.screens.End();
			return (true);
        }
		override public string Display()
		{
			return ("Command End");
		}
	}
	public class OrderCommand : Command
	{
		public OrderCommand (FieldType fieldType, OrderType orderType)
        {
			field = fieldType;
			order = orderType;
        }
		override public bool Apply()
		{
			Commands.screens.Order(this, field, order);
			return (true);
        }
		override public string Display()
		{
			return ("Command Order(" + field + ", " + order + ")");
		}
	}
	public class FilterCommand : Command
	{
		public FilterCommand(FieldType fieldType, string queryString)
		{
			field = fieldType;
			query = queryString;
		}
		override public bool Apply()
		{
			Commands.screens.Filter(this, field, query);
			return (true);
        }
		override public string Display()
		{
			return ("Command Filter(" + field + ", " + query + ")");
		}
	}
	public class ConvertCommand : Command
	{
		public ConvertCommand (string inputFile, string bookTypeString, int shoppingListNo)
        {
			this.inputFile = inputFile;
			bookType = bookTypeString;
			this.shoppingListNo = shoppingListNo;
        }

		override public bool Apply()
		{
			Commands.screens.Convert(inputFile, bookType, shoppingListNo);
			return (true);
        }
		override public string Display()
		{
			if (bookType == "BT_SHOPPING_LIST")
				return ("Command Convert(" + bookType + "#" + shoppingListNo + ", " + inputFile + ")");
			else
				return ("Command Convert(" + bookType + ", " + inputFile + ")");
		}
	}
	public class ImportCommand : Command
	{
		public ImportCommand(string inputFile)
        {
			this.inputFile = inputFile;
        }
		override public bool Apply()
		{
			Commands.screens.Import(inputFile);
			return (true);
        }
		override public string Display()
		{
			return ("Command Import(" + inputFile + ")");
		}
	}
	public class XmlSaveCommand : Command
	{
		public XmlSaveCommand (string outputFile)
        {
			this.outputFile = outputFile;
        }
		override public bool Apply()
		{
			Commands.screens.XmlSave(outputFile);
			return (true);
        }
		override public string Display()
		{
			return ("Command XMLSave(" + outputFile + ")");
		}
	}
	public class XmlLoadCommand : Command
	{
		public XmlLoadCommand(string inputFile)
		{
			this.inputFile = inputFile;
		}
		override public bool Apply()
		{
			Commands.screens.XmlLoad(inputFile);
			return (true);
        }
		override public string Display()
		{
			return ("Command XMLoad(" + inputFile + ")");
		}
	}
	public class UpdateBookCommand : Command
	{
		public UpdateBookCommand (FieldType field, int bookIndex, string newValue)
        {
			this.field = field;
			_index = bookIndex;
			this.newValue = newValue;
        }

		override public bool Apply()
		{
			Commands.screens.Update(field, _index, newValue);
			return (true);
        }
		override public string Display()
		{
			return ("Command UpdateBook(" + field + ", " +  _index + ", " + newValue + ")");
		}
	}
	public class DeleteCommand : Command
	{
		public DeleteCommand(FieldType field, int bookIndex, string delValue)
		{
			this.field = field;
			_index = bookIndex;
			this.delValue = delValue;
		}
		override public bool Apply()
		{
			Commands.screens.Delete(field, _index, delValue);
			return (true);
        }
		override public string Display()
		{
			return ("Command DeleteBook(" + _index + ")");
		}
	}
	public class CreateBookCommand : Command
	{
		public CreateBookCommand (string query, string user, string bookTypeString, int shoppingListNo, bool read, 
			                      string tagValues)
        {
			this.query = query;
			userName = user;
			bookType = bookTypeString;
			this.shoppingListNo = shoppingListNo;
			this.read = read;
			this.tagValues = tagValues;
        }
		override public bool Apply()
		{
			Commands.screens.CreateBook(query, userName, bookType, shoppingListNo, read, tagValues);
			return (true);
        }
		override public string Display()
		{
			return ("Command CreateBook(" + query + ", " + userName + ", " + bookType + ", " + shoppingListNo + 
				    ", " + read + ", " + tagValues + ")");
		}
	}
	public class FillInEmptyTagsCommand : Command
	{
		public FillInEmptyTagsCommand()
		{
		}
		override public bool Apply()
		{
			Commands.screens.FillInEmptyTags();
			return (true);
		}
		override public string Display()
		{
			return ("Command FillInEmptyTags()");
		}
	}
	public class DisplayCountsCommand : Command
	{
		public DisplayCountsCommand(FieldType field1, FieldType field2)
		{
			this.field1 = field1;
			this.field2 = field2;
		}
		override public bool Apply()
		{
			Commands.screens.DisplayCounts(field1, field2);
			return (true);
		}
		override public string Display()
		{
			return ("Command DisplayCounts()");
		}
	}
	public class DisplayVersionCommand : Command
	{
		public DisplayVersionCommand()
		{
		}
		override public bool Apply()
		{
			Commands.screens.DisplayVersion();
			return (true);
		}
		override public string Display()
		{
			return ("Command DisplayVersion()");
		}
	}
}