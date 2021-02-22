using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    public enum CommandType
    {
        Login,
        JumpTo,
        RegisterNewUser,
        EmailUserPassword,
        ChangeUserPassword,
        DownPage,
        UpPage,
        DownOne,
        UpOne,
        Home,
        End,
        Order,
        Filter,
        Convert,
        Import,
        XmlSave,
        XmlLoad,
        Delete,
        CreateBook,
        UpdateBook,
        FillInEmptyTags,
        DisplayCounts,
        DisplayVersion
    }

    /// <summary>
    /// Implementation of Factory - Used to create objects.
    /// </summary>
    public class Factory
    {
        private bool checkPars(List<object> pars, int expectedNumberOfPars, CommandType cmdType)
        {
            bool parsOK;
            string error;

            parsOK = true;
            if (pars.Count != expectedNumberOfPars)
            {
                parsOK = false;
                error = GetCommandName(cmdType);
                error += " Command wrong number of parameters, got " + pars.Count +
                         " expected " + expectedNumberOfPars;
                Logger.OutputError(error);
            }

            return (parsOK);
        }
        private string GetCommandName(CommandType type)
        {
            return (type.ToString());
        }
        public Command GetCommand(CommandType type, List<object> pars)
        {
            switch (type)
            {
                case CommandType.Login:
                    if (checkPars(pars, 2, type))
                        return (new LoginUserCommand((string) pars[0] /* UserName */, (string) pars[1]/* Encrypted */));
                    break;

                case CommandType.JumpTo:
                    if (checkPars(pars, 1, type))
                        return (new JumpToCommand((int) pars[0] /* bookRowNo */));
                    break;

                case CommandType.RegisterNewUser:
                    if (checkPars(pars, 6, type))
                        return (new RegisterNewUserCommand((string) pars[0] /* FirstName */,
                                                           (string) pars[1] /* MiddleName */,
                                                           (string) pars[2] /* Surname */,
                                                           (string) pars[3] /* Encrypted */,
                                                           (string) pars[4] /* UserName */,
                                                           (string) pars[5] /* Email */));
                    break;

                case CommandType.EmailUserPassword:
                    if (checkPars(pars, 1, type))
                        return (new EmailUserPasswordCommand((string)pars[0] /* userName */));
                    break;

                case CommandType.ChangeUserPassword:
                    if (checkPars(pars, 4, type))
                        return (new ChangeUserPasswordCommand((string)pars[0] /* userName */,
                                                              (string)pars[1] /* encryptedPassword */,
                                                              (string)pars[2] /* newEncryptedPassword */,
                                                              (string)pars[3] /* repeatEncryptedPassword */));
                    break;

                case CommandType.DownPage:
                    if (checkPars(pars, 0, type))
                        return (new DownPageCommand());
                    break;

                case CommandType.UpPage:
                    if (checkPars(pars, 0, type))
                        return (new UpPageCommand());
                    break;

                case CommandType.DownOne:
                    if (checkPars(pars, 0, type))
                        return (new DownOneCommand());
                    break;

                case CommandType.UpOne:
                    if (checkPars(pars, 0, type))
                        return (new UpOneCommand());
                    break;

                case CommandType.Home:
                    if (checkPars(pars, 0, type))
                        return (new HomeCommand());
                    break;

                case CommandType.End:
                    if (checkPars(pars, 0, type))
                        return (new EndCommand());
                    break;

                case CommandType.Order:
                    if (checkPars(pars, 2, type))
                        return (new OrderCommand((FieldType) pars[0] /* fieldType */,
                                                 (OrderType) pars[1] /* orderType */));
                    break;

                case CommandType.Filter:
                    if (checkPars(pars, 2, type))
                        return (new FilterCommand((FieldType) pars[0] /* fieldType */,
                                                  (string) pars[1] /* searchString */));
                    break;

                case CommandType.Convert:
                    if (checkPars(pars, 3, type))
                        return (new ConvertCommand((string) pars[0] /* inputFile */,
                                                   (string) pars[1] /* bookTypeString */,
                                                   (int) pars[2] /* shoppingListNo */));
                    break;

                case CommandType.Import:
                    if (checkPars(pars, 1, type))
                        return (new ImportCommand((string)pars[0] /* inputFile */));
                    break;

                case CommandType.XmlSave:
                    if (checkPars(pars, 1, type))
                        return (new XmlSaveCommand((string)pars[0] /* outputFile */));
                    break;

                case CommandType.XmlLoad:
                    if (checkPars(pars, 1, type))
                        return (new XmlLoadCommand((string)pars[0] /* inputFile */));
                    break;

                case CommandType.Delete:
                    if (checkPars(pars, 3, type))
                        return (new DeleteCommand((FieldType) pars[0] /* field */,
                                                   (int) pars[1] /* bookIndex */,
                                                   (string) pars[2] /* delValue */));
                    break;

                case CommandType.CreateBook:
                    if (checkPars(pars, 6, type))
                        return (new CreateBookCommand((string) pars[0] /* query */,
                                                      (string) pars[1] /* user */,
                                                      (string)pars[2] /* bookTypeString */,
                                                      (int)pars[3] /* shoppingListNo */,
                                                      (bool) pars[4] /* read */,
                                                      (string)pars[5] /* tagValues */));
                    break;

                case CommandType.UpdateBook:
                    if (checkPars(pars, 3, type))
                        return (new UpdateBookCommand((FieldType) pars[0] /* field */,
                                                      (int) pars[1] /* bookIndex */,
                                                      (string) pars[2] /* newValue */));
                    break;

                case CommandType.FillInEmptyTags:
                    if (checkPars(pars, 0, type))
                        return (new FillInEmptyTagsCommand());
                    break;

                case CommandType.DisplayCounts:
                    if (checkPars(pars, 2, type))
                        return (new DisplayCountsCommand((FieldType) pars[0] /* fieldType1 */,
                                                         (FieldType) pars[1] /* fieldType2 */));
                    break;

                case CommandType.DisplayVersion:
                    if (checkPars(pars, 0, type))
                        return (new DisplayVersionCommand());
                    break;

                default:
                    throw new NotSupportedException();
            }

            return (null);
        }
    }
}
