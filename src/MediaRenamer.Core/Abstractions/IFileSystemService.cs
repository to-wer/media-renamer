namespace MediaRenamer.Core.Abstractions;

public interface IFileSystemService
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);

    IEnumerable<string> GetFiles(string path);
    bool  FileExists(string path);
    void DeleteFile(string path);
    void MoveFile(string sourcePath, string targetPath);
}