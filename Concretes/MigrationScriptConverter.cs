using SqlMigrationRunner.Interfaces;
using SqlMigrationRunner.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlMigrationRunner.Concretes
{
    public class MigrationScriptConverter : IMigrationScriptConverter
    {
        public MigrationScript GetMigrationScript(string fullFilePath)
        {
            if (!File.Exists(fullFilePath))
            {
                return null;
            }

            return ToMigrationScript(fullFilePath);
        }

        public MigrationScript[] GetMigrationScripts(string folderFullPath, bool includedSubFolders)
        {
            if (!Directory.Exists(folderFullPath)) 
            {
                return null;
            }

            string[] sqlScripts = Directory.GetFiles(folderFullPath, "*.sql", includedSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            if (sqlScripts == null || !sqlScripts.Any()) 
            {
                return null;
            }

            return sqlScripts.Select(ToMigrationScript).ToArray();
        }

        private static MigrationScript ToMigrationScript(string fullFilePath) 
        {
            string fileName = Path.GetFileNameWithoutExtension(fullFilePath);
            string[] tokens = fileName.Split(new [] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length != 2 || string.IsNullOrEmpty(tokens[0]) || string.IsNullOrEmpty(tokens[1])) 
            {
                return null;
            }

            return new MigrationScript() 
            {
                Name = GetMigrationScriptName(tokens[1]),
                Sql = File.ReadAllText(fullFilePath),
                Version = new MigrationVersion(tokens[0].ToLower().Replace("v", string.Empty))
            };
        }

        private static string GetMigrationScriptName(string camelCaseName) 
        {
            return Regex.Replace(camelCaseName, "(\\B[A-Z])", " $1");
        }
    }
}
