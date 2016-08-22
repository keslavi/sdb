# sdb

c# database adapter/connector for SqlServer, DB2, Oracle, MySql... ruby style pattern for additional databases

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. 

SqlServer has the most support in here, will update the other databases as needed.


### Prerequisities

.Net ^4.0 (built at 4.5.2)

```
usage: 
db adapter looks for connectionstring info in the following order:
/db.config  custom location housing multiple database connections
/db.enc.config encrypted version.
[web|app].config if no name is specified adapter will use the first connection string in ConnectionStrings

Note the use of an Alias field to specify which connection settings to use. 

executing a proc and returning a result, more granular control is supported as needed:
	DataSet ds=null;
	using (var db=new SDB.Adapter.Sql())
	{
		//matches parameter names to collection
		ds=db.fill(ProcName, NameValueCollection)
	}

Updater Simplest usage: 
				SDB.Adapter.SQL.Updater up=new SDB.Adapter.SQL.Updater();
				if (up.VersionScript > up.VersionDb)
				{
					up.Update(up.VersionScript);
				}
				//by default looks in /scripts/updater/sql/00000.description.[up|dn].sql




```

### Installing

Git clone https://github.com/keslavi/sdb.git

for standalone usage, 
git clone --depth 1 https://github.com/keslavi/sdb.git

Most of the time you'll want to Build and use /bin/*.dll in a seperate project.

```

See Usage for base examples. will include a sample web app at some point. 


## Running the tests
TBD| n/a

## Deployment

generally build as Release and deploy /bin into an existing project

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

Steve Cranford 

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone who's code was used
* Inspiration
* etc
