using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.FileLists;

public class FFTPackFileList
{
    public FrozenDictionary<int, string> IndexToKnownFileName { get; private set; }
    public FrozenDictionary<string, int> FileNameToKnownIndex { get; private set; }

    public bool Load(string dir)
    {
        var indexToFilenames = new Dictionary<int, string>();
        var fileNamesToIndices = new Dictionary<string, int>();

        string fileListPath = Path.Combine(dir, "FileLists", "fftpack.txt");
        if (!File.Exists(fileListPath))
        {
            return false;
        }

        using var sw = File.OpenText(fileListPath);

        while (!sw.EndOfStream)
        {
            var line = sw.ReadLine();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                continue;

            string[] split = line.Split('|');
            if (split.Length < 2)
                continue;

            if (!int.TryParse(split[0], out int fileIndex))
                continue;

            string fftPackPath = split[1].Replace('\\', '/');
            indexToFilenames.TryAdd(fileIndex, fftPackPath);
            fileNamesToIndices.TryAdd(fftPackPath, fileIndex);
        }

        IndexToKnownFileName = indexToFilenames.ToFrozenDictionary();
        FileNameToKnownIndex = fileNamesToIndices.ToFrozenDictionary();

        return true;
    }
}
