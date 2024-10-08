﻿// Copyright © 2024 Lionk Project

using Lionk.Auth.Abstraction;
using Lionk.Utils;
using Newtonsoft.Json;

namespace Lionk.Auth.Identity;

/// <summary>
///     This class is used to handle the users json files.
/// </summary>
public class UserFileHandler : IUserRepository
{
#if DEBUG
    private static readonly string _fileName = "users_debug.json";
#else
    private static readonly string _fileName = "users.json";
#endif

    private static readonly FolderType _folderType = FolderType.Data;

    private static readonly string _folderPath = "users";

    /// <summary>
    ///     Gets the path of the users file.
    /// </summary>
    public static string UsersPath { get; } = Path.Combine(_folderPath, _fileName);

    /// <summary>
    ///     Initializes static members of the <see cref="UserFileHandler" /> class.
    /// </summary>
    static UserFileHandler()
    {
        string dataFolder = Path.Combine(ConfigurationUtils.GetFolderPath(_folderType), _folderPath);
        FileHelper.CreateDirectoryIfNotExists(dataFolder);
    }

    /// <summary>
    ///     Method to save a notification in history.
    /// </summary>
    /// <param name="user"> The notification to save.</param>
    public void SaveUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        HashSet<User> users = GetUsers();
        users.Add(user);
        WriteUsers(users);
    }

    /// <summary>
    ///     Method to update a user.
    /// </summary>
    /// <param name="user"> The user to update.</param>
    public void UpdateUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        HashSet<User> users = GetUsers();
        users.RemoveWhere(u => u.Id == user.Id);
        users.Add(user);
        WriteUsers(users);
    }

    /// <summary>
    ///     Method to write the notifications in the file.
    /// </summary>
    /// <param name="users"> The list of notifications to write.</param>
    private static void WriteUsers(HashSet<User> users)
    {
        string json = JsonConvert.SerializeObject(users, Formatting.Indented);
        ConfigurationUtils.SaveFile(UsersPath, json, _folderType);
    }

    /// <summary>
    ///     Method to get all the notifications saved.
    /// </summary>
    /// <returns> The list of notifications saved.</returns>
    /// <exception cref="ArgumentNullException"> If file exists but the result of the deserialization is null.</exception>
    public HashSet<User> GetUsers()
    {
        string json = ConfigurationUtils.ReadFile(UsersPath, _folderType);
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }

        HashSet<User> users = JsonConvert.DeserializeObject<HashSet<User>>(json) ?? throw new ArgumentNullException(nameof(users));
        return users;
    }

    /// <summary>
    ///     Method to delete an user.
    /// </summary>
    /// <param name="user"> The user to delete.</param>
    public void DeleteUser(User user)
    {
        HashSet<User> users = GetUsers();
        users.RemoveWhere(u => u.Id == user.Id);
        WriteUsers(users);
    }
}
