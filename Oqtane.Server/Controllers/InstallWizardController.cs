using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

// *** NOTE: THIS CODE CANNOT BE USED IN A PRODUCTION APPLICATATION ***
// AT THIS TIME THE BLAZOR TEAM HAS NOT RELEASED CODE FOR AUTHENTICATION AND AUTHORIZATION
// SO THIS CODE DOES NOT CONTAIN ** ANY SECURITY ** (BUT IS **REQUIRES** IT!)
// THEREFORE: DO-NOT-USE-THIS-CODE-IN-PRODUCTION
namespace Oqtane.Controllers
{
    //api/InstallWizard
    [Route("api/[controller]")]
    public class InstallWizardController : Controller
    {
        // NewDatabaseVersion should always be "00.00.00"
        string NewDatabaseVersion = "00.00.00";

        // TargetDatabaseVersion should be changed to 
        // the version that the current code requires
        // ** Each upgrade will set this value and it will be
        // ** read when the appication starts up each time
        string TargetDatabaseVersion = "01.20.00";

        private string _DefaultConnection;
        private string _DefaultFilesPath;
        private IConfigurationRoot _configRoot { get; set; }
        private readonly IWritableOptions<ConnectionStrings> _connectionString;
        private readonly IWebHostEnvironment _hostEnvironment;

        public InstallWizardController(
            IConfigurationRoot configRoot,
            IOptions<ConnectionStrings> ConnectionStrings,
            IWritableOptions<ConnectionStrings> connectionString,
            IWebHostEnvironment hostEnvironment)
        {
            _configRoot = configRoot;
            _DefaultConnection = ConnectionStrings.Value.DefaultConnection;
            _connectionString = connectionString;
            _hostEnvironment = hostEnvironment;

            // Set WebRootPath to wwwroot\Files directory
            _hostEnvironment.WebRootPath =
                System.IO.Path.Combine(
                    Directory.GetCurrentDirectory(),
                    @"wwwroot");

            // We need to create a Files directory if none exists
            // This will be used if the Administrator does not set a Files directory
            // Set WebRootPath to wwwroot\Files directory
            _DefaultFilesPath =
                System.IO.Path.Combine(
                    Directory.GetCurrentDirectory(),
                    @"wwwroot\Files");

            // Create wwwroot\Files directory if needed
            if (!Directory.Exists(_DefaultFilesPath))
            {
                DirectoryInfo di =
                    Directory.CreateDirectory(_DefaultFilesPath);
            }
        }

        // ********************************************************
        // Setupstatus

        // api/InstallWizard/CurrentVersion
        [HttpGet("[action]")]
        [AllowAnonymous]
        #region public Version CurrentVersion()
        public VersionInfo CurrentVersion()
        {
            // Version object to return
            VersionInfo objVersion = new VersionInfo();
            objVersion.VersionNumber = NewDatabaseVersion;
            objVersion.isUpToDate = false;

            try
            {
                objVersion = GetDatabaseVersion(NewDatabaseVersion, GetConnectionString());
                objVersion.isNewDatabase =
                    (objVersion.VersionNumber == NewDatabaseVersion);
                objVersion.isUpToDate =
                    (objVersion.VersionNumber == TargetDatabaseVersion);
            }
            catch
            {

            }

            // Return the result
            return objVersion;
        }
        #endregion

        // POST: /api/InstallWizard/ConnectionSetting
        [HttpPost("[action]")]
        #region public Status ConnectionSetting([FromBody]ConnectionSetting objConnectionSetting)
        public Status ConnectionSetting([FromBody]ConnectionSetting objConnectionSetting)
        {
            // The return message
            Status objStatus = new Status();
            objStatus.Success = true;

            // Do not run if we can connect to the current database
            if (CurrentVersion().isNewDatabase == false)
            {
                objStatus.Success = true;
                objStatus.StatusMessage = "Database already set-up";
                return objStatus;
            }
            else
            {
                if (objConnectionSetting.DatabaseType == "File")
                {
                    #region File Database
                    string datadirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
                    string connectionString = GetConnectionString();
                    connectionString = connectionString.Replace("|DataDirectory|", datadirectory);

                    // check if database exists                    
                    bool databaseExists = DatabaseConnectionValid(connectionString);

                    // create database if it does not exist
                    if (!databaseExists)
                    {
                        string masterConnectionString = "";
                        string databaseName = "";
                        string[] fragments = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                        foreach (string fragment in fragments)
                        {
                            if (fragment.ToLower().Contains("initial catalog=") || fragment.ToLower().Contains("database="))
                            {
                                databaseName = fragment.Substring(fragment.IndexOf("=") + 1);
                            }
                            else
                            {
                                if (!fragment.ToLower().Contains("attachdbfilename="))
                                {
                                    masterConnectionString += fragment + ";";
                                }
                            }
                        }

                        // Create a unique database name
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
                        databaseName = "Oqtane-" + timestamp;

                        SqlConnection connection = new SqlConnection(masterConnectionString);

                        try
                        {
                            using (connection)
                            {
                                connection.Open();
                                SqlCommand command;
                                command = new SqlCommand("CREATE DATABASE [" + databaseName + "] ON ( NAME = '" + databaseName + "', FILENAME = '" + datadirectory + "\\" + databaseName + ".mdf')", connection);
                                command.ExecuteNonQuery();

                                UpdateDatabaseConnectionString($"Data Source=(LocalDb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\{databaseName}.mdf;Initial Catalog=Oqtane;Integrated Security=SSPI;");

                                // sleep to allow SQL server to attach new database
                                Thread.Sleep(5000);
                            }
                        }
                        catch (Exception ex)
                        {
                            objStatus.Success = false;
                            objStatus.StatusMessage = ex.GetBaseException().ToString();
                            return objStatus;
                        }

                        // Initialize Database
                        objStatus = InitializeDatabase();

                        return objStatus;
                    }
                    #endregion
                }
                else
                {
                    #region Server Database
                    // Create a Database connection string
                    string strConnectionString = CreateDatabaseConnectionString(objConnectionSetting);

                    // Test the database connection string
                    if (DatabaseConnectionValid(strConnectionString))
                    {
                        try
                        {
                            // Update the appsettings.json file
                            UpdateDatabaseConnectionString(strConnectionString);

                            // Initialize Database
                            objStatus = InitializeDatabase();
                        }
                        catch (Exception ex)
                        {
                            // appsettings.json file update error
                            objStatus.Success = false;
                            objStatus.StatusMessage = ex.GetBaseException().Message;
                        }
                    }
                    else
                    {
                        // Bad connection setting
                        objStatus.Success = false;
                        objStatus.StatusMessage = "Connection settings are not valid";
                    }
                    #endregion
                }
            }

            // Return the result
            return objStatus;
        }
        #endregion        

        // api/InstallWizard/UpdateDatabase
        [HttpGet("[action]")]
        //[Authorize] -- Required for a production version of this application!
        #region public Status UpdateDatabase()
        public Status UpdateDatabase()
        {
            Status objStatus = new Status();
            objStatus.Success = true;

            // Must be a Super User to proceed
            //if (UtilitySecurity.IsSuperUser(this.User.Identity.Name, GetConnectionString()))
            //{
            // Run update scripts
            objStatus = RunUpdateScripts(NewDatabaseVersion, _hostEnvironment, GetConnectionString());
            //}
            //else
            //{
            //    objStatus.Success = false;
            //    objStatus.StatusMessage = "Must be a Super User to proceed";
            //}

            // Return the result
            return objStatus;
        }
        #endregion

        // POST: /api/InstallWizard/CreateAdminLogin
        [HttpPost("[action]")]
        [AllowAnonymous]
        #region public Status CreateAdminLogin([FromBody]Register Register)
        public RegisterStatus CreateAdminLogin([FromBody]Register objRegister)
        {
            // RegisterStatus to return
            RegisterStatus objRegisterStatus = new RegisterStatus();
            objRegisterStatus.status = "Registration Failure";
            objRegisterStatus.isSuccessful = false;

            // Test for a strong password
            if (!IsPasswordStrong(objRegister.password))
            {
                objRegisterStatus.status = "The password is not strong enough.";
                objRegisterStatus.isSuccessful = false;
                return objRegisterStatus;
            }

            // Do not run if we can connect to the current database
            if (CurrentVersion().isNewDatabase == false)
            {
                objRegisterStatus.isSuccessful = false;
                objRegisterStatus.status = "Cannot create the Admin account because the database is already set-up. Reload your web browser to upgrade using the updated database connection.";
            }
            else
            {
                // Run the scripts to set-up the database
                Status objStatus = RunUpdateScripts(NewDatabaseVersion, _hostEnvironment, GetConnectionString());

                if (!objStatus.Success)
                {
                    // If scripts have an error return it
                    objRegisterStatus.isSuccessful = false;
                    objRegisterStatus.status = objStatus.StatusMessage;
                }
                else
                {
                    //// Create the Administrator
                    //string strCurrentHostLocation = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
                    //objRegisterStatus = RegisterController.RegisterUser(
                    //    objRegister, GetConnectionString(), _hostEnvironment, _userManager, _signInManager, strCurrentHostLocation, true, true);

                    //// There was an error creating the Administrator
                    //if (!objRegisterStatus.isSuccessful)
                    //{
                    //    // Delete the record in the version table 
                    //    // So the install can be run again
                    //    objStatus = ResetVersionTable();

                    //    if (!objStatus.Success)
                    //    {
                    //        // If there is an error return it
                    //        objRegisterStatus.isSuccessful = false;
                    //        objRegisterStatus.status = objStatus.StatusMessage;
                    //    }
                    //    else
                    //    {
                    //        //  Delete the user in case they were partially created
                    //        objStatus = DeleteAllUsers();

                    //        if (!objStatus.Success)
                    //        {
                    //            // If there is an error return it
                    //            objRegisterStatus.isSuccessful = false;
                    //            objRegisterStatus.status = objStatus.StatusMessage;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    // Update the created user to be a SuperUser
                    //    objStatus = MakeUserASuperUser(objRegister.userName);

                    //    #region Set the upload file path
                    //    try
                    //    {
                    //        string strDefaultFilesPath = ADefHelpDeskApp.Controllers.ApplicationSettingsController.GetFilesPath(_DefaultFilesPath, GetConnectionString());

                    //        // Get GeneralSettings
                    //        GeneralSettings objGeneralSettings = new GeneralSettings(GetConnectionString());
                    //        objGeneralSettings.UpdateFileUploadPath(GetConnectionString(), strDefaultFilesPath);
                    //    }
                    //    catch
                    //    {
                    //        // Do nothing if this fails
                    //        // Admin can set the file path manually
                    //    } 
                    //    #endregion

                    //    if (!objStatus.Success)
                    //    {
                    //        // If there is an error return it
                    //        objRegisterStatus.isSuccessful = false;
                    //        objRegisterStatus.status = objStatus.StatusMessage;
                    //    }
                    //}
                }
            }

            return objRegisterStatus;
        }
        #endregion

        // Helpers

        #region public static Version GetDatabaseVersion(string NewDatabaseVersion, string ConnectionString)
        public static VersionInfo GetDatabaseVersion(string NewDatabaseVersion, string ConnectionString)
        {
            // Version object to return
            VersionInfo objVersion = new VersionInfo();

            // If Version returned is NewDatabaseVersion 
            // we will assume the Version table does not exist
            objVersion.VersionNumber = NewDatabaseVersion;

            var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
            optionsBuilder.UseSqlServer(ConnectionString);

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    // There is actually a connection string
                    // Test it by trying to read the Version table
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(
                        "SELECT top 1 VersionNumber from OqtaneDatabaseVersion",
                        connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                objVersion.VersionNumber = Convert.ToString(reader.GetValue(0));
                            }
                        }
                    }
                }
            }
            catch
            {
                // Do nothing if we cannot connect
                // the method will return NewDatabaseVersion
            }

            return objVersion;
        }
        #endregion

        #region private string CreateDatabaseConnectionString(ConnectionSetting objConnectionSetting)
        private string CreateDatabaseConnectionString(ConnectionSetting objConnectionSetting)
        {
            StringBuilder SB = new StringBuilder();
            string strConnectionString = "";

            string strUserInfo = (!objConnectionSetting.IntegratedSecurity) ?
                String.Format("uid={0};pwd={1}",
                objConnectionSetting.Username,
                objConnectionSetting.Password) :
                "integrated security=True";

            strConnectionString = String.Format("{0}data source={1};initial catalog={2};{3}",
                SB.ToString(),
                objConnectionSetting.ServerName,
                objConnectionSetting.DatabaseName,
                strUserInfo);

            return strConnectionString;
        }
        #endregion

        #region private bool DatabaseConnectionValid(string strConnectionString)
        private bool DatabaseConnectionValid(string strConnectionString)
        {
            bool boolDatabaseConnectionValid = true;

            // check if database exists
            SqlConnection connection = new SqlConnection(strConnectionString);

            try
            {
                using (connection)
                {
                    connection.Open();
                }

                boolDatabaseConnectionValid = true;
            }
            catch
            {
                // Could not connect
                boolDatabaseConnectionValid = false;
            }
            
            return boolDatabaseConnectionValid;
        }
        #endregion

        #region private string UpdateDatabaseConnectionString(string strConnectionString)
        private string UpdateDatabaseConnectionString(string strConnectionString)
        {
            // Update DefaultConnection in the appsettings.json file            
            _connectionString.Update(opt =>
            {
                opt.DefaultConnection = strConnectionString;
            });

            // *********************************
            // Reload configuration
            ReloadConfiguration();

            return strConnectionString;
        }
        #endregion

        #region public static Status RunUpdateScripts(string _NewDatabaseVersion, IWebHostEnvironment _hostEnvironment, string ConnectionString)
        public static Status RunUpdateScripts(string _NewDatabaseVersion, IWebHostEnvironment _hostEnvironment, string ConnectionString)
        {
            Status objStatus = new Status();
            objStatus.Success = true;

            // Get the update scripts
            Dictionary<int, string> ColScripts = UpdateScripts();
            VersionInfo objVersion = GetDatabaseVersion(_NewDatabaseVersion, ConnectionString);

            foreach (var sqlScript in ColScripts)
            {
                try
                {
                    // Get the script version
                    int intCurrentDatabaseVersion = ConvertVersionToInteger(objVersion.VersionNumber);

                    // Only run the script if it is higher than the 
                    // current database version
                    if (sqlScript.Key > intCurrentDatabaseVersion)
                    {
                        // Run the script
                        var connection = new SqlConnection(ConnectionString);

                        using (connection)
                        {
                            string strSQLScript = GetSQLScript(sqlScript.Value, _hostEnvironment);
                            strSQLScript = strSQLScript.Replace("{ConnectionString}", ConnectionString);

                            connection.Open();
                            SqlCommand command;
                            command = new SqlCommand(strSQLScript, connection);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    objStatus.StatusMessage = ex.Message;
                    objStatus.Success = false;
                    return objStatus;
                }
            }

            return objStatus;
        }
        #endregion

        #region private Status InitializeDatabase() 
        private Status InitializeDatabase()
        {
            Status objStatus = new Status();
            objStatus.Success = true;
            objStatus.StatusMessage = $"Database Initialized!";

            // Create the OqtaneDatabaseVersion table
            // Set the version to 00.10.00
            SqlConnection connection = new SqlConnection(GetConnectionString());

            try
            {
                using (connection)
                {
                    connection.Open();
                    SqlCommand command;

                    string sql = "CREATE TABLE [dbo].[OqtaneDatabaseVersion]([VersionNumber] [varchar](10) NOT NULL, CONSTRAINT [PK_OqtaneDatabaseVersion] PRIMARY KEY CLUSTERED ";
                    sql = sql + "([VersionNumber] ASC) WITH ( ";
                    sql = sql + "PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] ";
                    sql = sql + "/** Update Version **/ DELETE FROM OqtaneDatabaseVersion INSERT INTO OqtaneDatabaseVersion(VersionNumber) VALUES (N'00.10.00')";

                    command = new SqlCommand(sql, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                objStatus.Success = false;
                objStatus.StatusMessage = ex.GetBaseException().ToString();
            }
            
            return objStatus;
        }
        #endregion

        #region ResetVersionTable()
        private Status ResetVersionTable()
        {
            Status objStatus = new Status();
            objStatus.Success = true;
            objStatus.StatusMessage = "";

            var connection = new SqlConnection(GetConnectionString());

            using (connection)
            {
                try
                {
                    string strSQLScript = "Delete from OqtaneDatabaseVersion";

                    connection.Open();
                    SqlCommand command;
                    command = new SqlCommand(strSQLScript, connection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    objStatus.Success = false;
                    objStatus.StatusMessage = ex.GetBaseException().Message;
                }
            }

            return objStatus;
        }
        #endregion

        #region private Status DeleteAllUsers()
        private Status DeleteAllUsers()
        {
            Status objStatus = new Status();
            objStatus.Success = true;
            objStatus.StatusMessage = "";

            var connection = new SqlConnection(GetConnectionString());

            using (connection)
            {
                try
                {
                    string strSQLScript = "delete from Users";

                    connection.Open();
                    SqlCommand command;
                    command = new SqlCommand(strSQLScript, connection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    objStatus.Success = false;
                    objStatus.StatusMessage = ex.GetBaseException().Message;
                }
            }

            return objStatus;
        }
        #endregion

        // Utility

        #region private static Dictionary<int, string> UpdateScripts()
        private static Dictionary<int, string> UpdateScripts()
        {
            Dictionary<int, string> ColScripts = new Dictionary<int, string>();

            ColScripts.Add(ConvertVersionToInteger("01.00.00.sql"), "01.00.00.sql");
            ColScripts.Add(ConvertVersionToInteger("01.20.00.sql"), "01.20.00.sql");

            return ColScripts;
        }
        #endregion

        #region private static String GetSQLScript(string SQLScript, IWebHostEnvironment _hostEnvironment)
        private static String GetSQLScript(string SQLScript, IWebHostEnvironment _hostEnvironment)
        {
            string strSQLScript;
            string strFilePath = _hostEnvironment.ContentRootPath + $@"\Scripts\{SQLScript}";
            using (StreamReader reader = new StreamReader(strFilePath))
            {
                strSQLScript = reader.ReadToEnd();                
                reader.Close();
            }
            return strSQLScript;
        }
        #endregion

        #region private static int ConvertVersionToInteger(string strParamVersion)
        private static int ConvertVersionToInteger(string strParamVersion)
        {
            int intVersionNumber = 0;

            // Strip out possible .sql in string
            string strVersion = strParamVersion.Replace(".sql", "");

            // Split into parts seperated by periods
            char[] splitchar = { '.' };
            var strSegments = strVersion.Split(splitchar);

            // Process the segments
            int i = 0;
            List<int> colMultiplyers = new List<int> { 10000, 100, 1 };
            foreach (var strSegment in strSegments)
            {
                int intSegmentNumber = Convert.ToInt32(strSegment);
                intVersionNumber = intVersionNumber + (intSegmentNumber * colMultiplyers[i]);
                i++;
            }

            return intVersionNumber;
        }
        #endregion

        #region private void ReloadConfiguration()
        private void ReloadConfiguration()
        {
            //// Temporarily rename the web.config file
            //// to release the locks on any assemblies
            //string WebConfigOrginalFileNameAndPath =
            //    _hostEnvironment.ContentRootPath + @"\Web.config";
            //string WebConfigTempFileNameAndPath =
            //    _hostEnvironment.ContentRootPath + @"\Web.config.txt";

            //System.IO.File.Copy(WebConfigOrginalFileNameAndPath,
            //    WebConfigTempFileNameAndPath);
            //System.IO.File.Delete(WebConfigOrginalFileNameAndPath);
            //// Give the site time to release locks on the assemblies
            //Task.Delay(2000).Wait(); // Wait 2 seconds with blocking
            //                         // Rename the temp web.config file back to web.config
            //                         // so the site will be active again
            //System.IO.File.Copy(WebConfigTempFileNameAndPath,
            //    WebConfigOrginalFileNameAndPath);
            //System.IO.File.Delete(WebConfigTempFileNameAndPath);

            // Finally a Config Reload
            _configRoot.Reload();
        }
        #endregion

        #region private string GetConnectionString()
        private string GetConnectionString()
        {
            // Use this method because the Instal Wizard updated the
            // connection string and we want to make sure we get
            // the latest one
            string finalConnectionString = "ERRROR:UNSET-CONECTION-STRING";

            try
            {
                string strConnectionString = _configRoot.GetConnectionString("DefaultConnection");

                // Handle if this is a file database
                string datadirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
                finalConnectionString = strConnectionString.Replace("|DataDirectory|", datadirectory);
            }
            catch
            {
                // Do nothing
            }

            return finalConnectionString;
        }
        #endregion

        #region private bool IsPasswordStrong(string paramPassword)
        private bool IsPasswordStrong(string paramPassword)
        {
            bool hasUpperCase = false;
            bool hasLowercase = false;
            bool hasSpecialCharacter = false;
            bool hasNumberCharacter = false;

            if (paramPassword == null)
            {
                return false;
            }

            if (paramPassword.Length < 9)
            {
                return false;
            }

            foreach (var character in paramPassword.ToCharArray())
            {
                if (Char.IsUpper(character))
                {
                    hasUpperCase = true;
                }

                if (Char.IsLower(character))
                {
                    hasLowercase = true;
                }

                if (!Char.IsLetterOrDigit(character))
                {
                    hasSpecialCharacter = true;
                }

                if (Char.IsNumber(character))
                {
                    hasNumberCharacter = true;
                }
            }

            return (hasUpperCase && hasLowercase && hasSpecialCharacter && hasNumberCharacter);
        }
        #endregion
    }
}
