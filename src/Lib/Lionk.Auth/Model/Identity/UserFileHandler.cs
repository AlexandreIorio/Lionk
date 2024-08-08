﻿// Copyright © 2024 Lionk Project

using Lionk.Auth.Identity;
using Lionk.Utils;
using Newtonsoft.Json;

namespace Lionk.Auth.Model.Identity;

/// <summary>
/// This class is used to handle the users witj json files.
/// </summary>
public class UserFileHandler
{
    private static readonly FolderType _folderType = FolderType.Data;
    private static readonly string _folderPath = "users";
    private static readonly string _usersPath = Path.Combine(_folderPath, "users.json");

    /// <summary>
    /// Initializes static members of the <see cref="UserFileHandler"/> class.
    /// </summary>
    static UserFileHandler()
    {
        string datafolder = Path.Combine(ConfigurationUtils.GetFolderPath(_folderType), _folderPath);
        FileHelper.CreateDirectoryIfNotExists(datafolder);
    }

    /// <summary>
    /// Method to save a notification in history.
    /// </summary>
    /// <param name="user"> The notification to save.</param>
    public static void SaveNotification(User user)
    {
        HashSet<User> users = GetUsers();
        users.Add(user);
        WriteUsers(users);
    }

    /// <summary>
    /// Method to write the notifications in the file.
    /// </summary>
    /// <param name="users"> The list of notifications to write.</param>
    private static void WriteUsers(HashSet<User> users)
    {
        string json = JsonConvert.SerializeObject(users, Formatting.Indented);
        ConfigurationUtils.SaveFile(_usersPath, json, _folderType);
    }

    /// <summary>
    /// Method to get all the notifications saved.
    /// </summary>
    /// <returns> The list of notifications saved.</returns>
    /// <exception cref="ArgumentNullException"> If file exists but the result of the deserialization is null.</exception>
    public static HashSet<User> GetUsers()
    {
        string json = ConfigurationUtils.ReadFile(_usersPath, _folderType);
        if (string.IsNullOrEmpty(json)) return new();
        HashSet<User> users = JsonConvert.DeserializeObject<HashSet<User>>(json) ?? throw new ArgumentNullException(nameof(users));
        return users;
    }
}
