﻿// Copyright © 2024 Lionk Project

using Lionk.Core.View;
using Newtonsoft.Json;

namespace LionkApp.Components.Model;

/// <summary>
/// Dashboard item model.
/// </summary>
public class DashboardItemModel
{
    /// <summary>
    /// Gets the unique identifier of the dashboard history.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardItemModel"/> class.
    /// </summary>
    /// <param name="componentName"> The component instance name.</param>
    /// <param name="viewType"> The view type.</param>
    public DashboardItemModel(string componentName, Type viewType)
    {
        ComponentInstanceName = componentName;
        ViewType = viewType;
        Indexes = new int[Enum.GetValues<ViewContext>().Length];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardItemModel"/> class.
    /// </summary>
    /// <param name="id"> The unique identifier of the dashboard item model.</param>
    /// <param name="componentName"> The component instance name.</param>
    /// <param name="viewType"> The view type.</param>
    /// <param name="indexes"> The indexes of selected views.</param>
    [JsonConstructor]
    public DashboardItemModel(Guid id, string componentName, Type viewType, int[] indexes)
    {
        Id = id;
        ComponentInstanceName = componentName;
        ViewType = viewType;
        Indexes = indexes;
    }

    /// <summary>
    /// Gets or sets the component instance name.
    /// </summary>
    public string ComponentInstanceName { get; set; }

    /// <summary>
    /// Gets or sets the view type.
    /// </summary>
    public Type ViewType { get; set; }

    /// <summary>
    /// Gets the indexes of selected views.
    /// </summary>
    public int[] Indexes { get; }

    /// <summary>
    /// This method saves the current index of the view.
    /// </summary>
    /// <param name="viewContext"> The view context.</param>
    /// <param name="index"> The index of the view.</param>
    public void SaveCurrentIndex(ViewContext viewContext, int index) => Indexes[(int)viewContext] = index;
}
