using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using PrimalEditor.Common;
using PrimalEditor.Utilities;

namespace PrimalEditor.Content;

static class ContentInfoCache
{
    private static readonly object _lock = new object();
    private static readonly Dictionary<string, ContentInfo> _contentInfoCache = new Dictionary<string, ContentInfo>();

    private static string _cacheFilePath = string.Empty;
    private static bool _isDirty;

    public static ContentInfo Add(string file)
    {
        lock (_lock)
        {
            var fileInfo = new FileInfo(file);

            Debug.Assert(!fileInfo.IsDirectory());

            if (!_contentInfoCache.ContainsKey(file) || _contentInfoCache[file].DateModified.IsOlder(fileInfo.LastWriteTime))
            {
                var info = AssetRegistry.GetAssetInfo(file) ?? Asset.GetAssetInfo(file);

                Debug.Assert(info != null);

                _contentInfoCache[file] = new ContentInfo(file, info.Icon);
                _isDirty = true;
            }

            Debug.Assert(_contentInfoCache.ContainsKey(file));

            return _contentInfoCache[file];
        }
    }

    public static void Reset(string projectPath)
    {
        lock (_lock)
        {
            if (!string.IsNullOrEmpty(_cacheFilePath) && _isDirty)
            {
                SaveInfoCache();

                _cacheFilePath = string.Empty;
                _contentInfoCache.Clear();
                _isDirty = false;
            }

            if (!string.IsNullOrEmpty(projectPath))
            {
                Debug.Assert(Directory.Exists(_cacheFilePath));

                _cacheFilePath = $@"{projectPath}.Primal\ContentInfoCache.bin";

                LoadInfoCache();
            }
        }
    }

    public static void Save() => Reset(string.Empty);

    private static void SaveInfoCache()
    {
        try
        {
            using var writer = new BinaryWriter(File.Open(_cacheFilePath, FileMode.Create, FileAccess.Write));

            writer.Write(_contentInfoCache.Keys.Count);

            foreach (var key in _contentInfoCache.Keys)
            {
                var info = _contentInfoCache[key];

                writer.Write(key);
                writer.Write(info.DateModified.ToBinary());
                writer.Write(info.Icon.Length);
                writer.Write(info.Icon);
            }

            _isDirty = false;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);

            Logger.Log(MessageType.Warning, "Failed to save Content Browser cache file.");
        }
    }

    private static void LoadInfoCache()
    {
        if (!File.Exists(_cacheFilePath)) return;

        try
        {
            using var reader = new BinaryReader(File.Open(_cacheFilePath, FileMode.Open, FileAccess.Read));

            var numberOfEntries = reader.ReadInt32();

            for (int i = 0; i < numberOfEntries; i++)
            {
                var assetFile = reader.ReadString();
                var date = DateTime.FromBinary(reader.ReadInt64());
                var iconSize = reader.ReadInt32();
                var icon = reader.ReadBytes(iconSize);

                if (File.Exists(assetFile))
                {
                    _contentInfoCache[assetFile] = new ContentInfo(assetFile, icon, null, date);
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);

            Logger.Log(MessageType.Warning, "Failed to read Content Browser cache file.");

            _contentInfoCache.Clear();
        }
    }
}
