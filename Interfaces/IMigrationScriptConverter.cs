using SqlMigrationRunner.Models;

namespace SqlMigrationRunner.Interfaces
{
    public interface IMigrationScriptConverter
    {
        MigrationScript GetMigrationScript(string fullFilePath);

        MigrationScript[] GetMigrationScripts(string folderFullPath, bool includedSubFolders);
    }
}
