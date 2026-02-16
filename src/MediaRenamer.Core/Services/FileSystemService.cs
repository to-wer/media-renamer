using MediaRenamer.Core.Abstractions;

namespace MediaRenamer.Core.Services;

public class FileSystemService : IFileSystemService
{
    public IEnumerable<string> GetFiles(string path)
    {
        return Directory
            .GetFiles(path, "*.*", SearchOption.AllDirectories);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public void MoveFile(string sourcePath, string targetPath)
    {
        File.Move(sourcePath, targetPath);
    }
}