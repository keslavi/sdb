//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Data;
//namespace SDB.Adapter.MySql
//{
//	public class Updater
//	{
//		string dirScript = "";
//		Adapter.MySql.DB db = null;
        
//#region Initilization
//		public Updater(string ScriptPath)
//		{
//			dirScript = ScriptPath;
//			db = new DB();
//		}
//		public Updater()
//		{
//			dirScript = ScriptDirectory("");
//			db = new DB();
//		}
//		public Updater(string ScriptPath, string Alias)
//		{
//			dirScript = ScriptDirectory(ScriptPath);
//			db = new DB(Alias);
//		}

//#endregion
//#region VersionInfo
//	   /// <summary>
//		/// returns the default directory for the db script or the location specified on intialization
//		/// </summary>
//		public string DirScript
//		{
//			get
//			{
//				if (dirScript=="")
//				{
//					string p=Common.Properties.Root + @"scripts\sql\sql\updater\";
//					if (File.Exists(p + "_Script.txt"))
//						dirScript=p;
//					else
//					{
//						p=p.Substring(0,p.LastIndexOf(@"\",p.Length-1));
//						if (!File.Exists(p + "_Script.txt"))
//							throw new Exception  ("Can't Find Script Path");                        
//					}
//					dirScript=p;
//				}
//				return dirScript;
//			}
//			set
//			{
//				dirScript=value;
//			}
//		}
//		/// <summary>
//		/// Returns the highest script available in the script directory
//		/// </summary>
//		public int VersionScript
//		{
//			get
//			{
//				int ret = 0;
//				string[] files = Directory.GetFiles(DirScript, "*.up.sql");
//				Array.Sort(files);
//				string filename = files[files.Length - 1];
//				Regex reg = new Regex(@"\d+");
//				Match match = reg.Match(filename.Substring(filename.LastIndexOf(@"\") + 1));
//				if (match.Success)
//					ret = int.Parse(match.Groups[0].Value);
//				else
//					throw new Exception("version number not found: " + filename);
//				return ret;
//			}
//		}
//		/// <summary>
//		/// gets or sets the version number in the database
//		/// </summary>
//		public int VersionDb
//		{
//			get
//			{
//				int ret=-1;
//				string sql = "Select Version from _tVersion where id=(select max(id) from _tVersion)";
//				try
//				{
//					DataSet ds = db.Fill(sql);
//					ret = int.Parse(ds.Tables[0].Rows[0][0].ToString());
//				}
//				catch (System.IndexOutOfRangeException e)
//				{
//					ret = -1;
//				}
//				catch (Exception e)
//				{
//					if (e.Message.IndexOf("Invalid object name") > 0)
//						ret = -1;
//					else
//						throw e;
//				}
//				return ret;
//			}
//			set
//			{
//				string sql = "";
//				sql += "insert into _tVersion (Version, dDate)";
//				sql += "\r\tselect {0},getdate()";
//				db.ExecuteNonQuery(string.Format(sql,value));
//				int ver = VersionDb;
//				if (ver !=value)
//					throw new System.Data.DBConcurrencyException("The Version remained at " + ver.ToString() + " rather than changing to " + value.ToString() + ".");
//			}
//		}
//		/// <summary>
//		/// Look at the path information and intelligently complete the path as needed
//		/// </summary>
//		/// <param name="Path"></param>
//		/// <returns></returns>
//		private string ScriptDirectory(string Path)
//		{
//			//try to find the path if it isn't specified.
//			//if this is a full path don't add the root info
//			if (Path.IndexOf(@"\\") == -1 & Path.IndexOf(":") == -1)
//			{
//				//if there is no partial path info then add the default
//				if (Path.IndexOf(@"\") == -1 & Path.IndexOf("/") == -1)
//					Path = @"scripts\sql\SQL\Updater" + Path;

//				if (System.IO.File.Exists(SDB.Common.Properties.Root + Path))
//					Path = SDB.Common.Properties.Root + Path;
//				else
//				{
//					if (System.IO.File.Exists(SDB.Common.Properties.RootPath + Path))
//						Path = SDB.Common.Properties.RootPath + Path;
//				}
//			}
//			return Path;
//		}


//#endregion
//		/// <summary>
//		/// Update the database to the latest version of the scripts
//		/// </summary>
//		public void Update()
//		{
//			Update(this.VersionScript);
//		}
//		/// <summary>
//		/// Upgrade or downgrade the database to the script version specified
//		/// </summary>
//		/// <param name="Version"></param>
//		public void Update(int Version)
//		{
//			//determine if this is upgrade or downgrade to pull the correct files.
//			string suffix = GetFileSuffix(Version);
//			string[] fileList = Directory.GetFiles(DirScript, suffix);
//			Array.Sort(fileList);
//			if (suffix.IndexOf("up") > 0)
//				Upgrade (fileList,Version);
//			else
//				Downgrade(fileList,Version);
//		}
//		private void ExecuteScript(string filename, int NewDBVersion)
//		{
//			StreamReader sr = new StreamReader(filename);
//			string script = sr.ReadToEnd();
//			sr.Close();

//			string Exceptions = db.ExecuteScript(script, true);
//			if (Exceptions == "")
//				//keep track of the version in case of upgrade failure
//				VersionDb = NewDBVersion;
//			else
//				throw new System.Data.SyntaxErrorException(filename + "\r" + Exceptions);
//		}
//		/// <summary>
//		/// Upgrade the database from it's current version to the selected version
//		/// </summary>
//		/// <param name="fileList"></param>
//		/// <param name="Version"></param>
//		private void Upgrade (string[] fileList,int Version)
//		{
//			int verDb = VersionDb;
//			int verScript = -1;
//			foreach (string filename in fileList)
//			{
//				verScript = this.ScriptVersion(filename);
//				if (verScript > verDb && verScript <= Version)
//				{
//					ExecuteScript(filename, verScript);
//				}
//			}
//		}
//		/// <summary>
//		/// downgrade the database to the specified version
//		/// </summary>
//		/// <param name="fileList"></param>
//		/// <param name="Version"></param>
//		private void Downgrade(string[] fileList,int Version)
//		{
//			Array.Reverse(fileList);
//			int verDb = VersionDb;
//			int RollbackStart = VersionDb;

//			int verScript = -1;
//			foreach (string filename in fileList)
//			{
//				verScript = this.ScriptVersion(filename);
//				if (verScript<=RollbackStart && verScript>Version)
//				{
//					ExecuteScript(filename, verScript);
//				}
//				//if an error didn't occur this should be the correct version now
//				VersionDb = Version;
//			}
//		}
//		/// <summary>
//		///  retrieves the script version from the filename
//		/// </summary>
//		/// <param name="filename"></param>
//		/// <returns></returns>
//		private int ScriptVersion(string filename)
//		{
//			int ret=-1;
//			Regex reg = new Regex(@"\d+");
//			Match match = reg.Match(filename.Substring(filename.LastIndexOf(@"\") + 1));
//			if (match.Success)
//				ret = int.Parse(match.Groups[0].Value);
//			else
//				throw new System.IO.FileLoadException(filename + " is not formatted as an Update file, Version Number not found \r(format: ####.description.[up|dn].sql)");
//			return ret;
//		}
//		/// <summary>
//		/// get the correct suffix based on upgrade or downgrade.
//		/// </summary>
//		/// <param name="Version"></param>
//		/// <returns></returns>
//		private string GetFileSuffix(int Version)
//		{
//			string suffix="*.up.sql";
//			int verDB = VersionDb;
//			int verScript = VersionScript;
//			if (verDB == Version)
//				throw new Exception("DB Version and Script Version are the same!");

//			if (Version<verDB)
//				suffix="*.dn.sql";
//			return suffix;
//		}

//		 //??? left off trying to build script updater
//		//string pathScript = "";
//		//int versionScript = 0;
//		//int versionDB = 0;
//		//Updater(string PathScript)
//		//{
//		//    pathScript = "";
//		//    if (pathScript.Substring(pathScript.Length - 1) != @"\")
//		//        pathScript = pathScript + @"\";
//		//}
//		//int VersionScript
//		//{
//		//    get
//		//    {
//		//        float ver = 0;

//		//        string[] files = Directory.GetFiles(pathScript, "_Version.vb");
//		//        // if (File.Exists(pathScript + "_version.txt"))
//		//        StreamReader sr = new StreamReader(pathScript + "_version.vb");
//		//        // string s=sr.
//		//        return 0;
//		//    }
//		//    set
//		//    {
//		//        string[] files = Directory.GetFiles(pathScript, "_*-Version.vb");
//		//        foreach (string file in files)
//		//        {
//		//            int ver = 0;
//		//            string s = file.Substring(file.IndexOf("-"));
//		//            System.IO.File.
//		//            ver = float.Parse(s);
//		//        }
//		//    }
//		//}
//		//int VersionDB
//		//{
//		//    get
//		//    {
//		//        return 0;
//		//    }


//		//}



//	}
//}
