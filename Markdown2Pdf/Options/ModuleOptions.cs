﻿using Markdown2Pdf.Helper;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Linq;

namespace Markdown2Pdf.Options;

//todo: tests
//todo: create plan on how to upgrade to never version
//todo: option none

/// <summary>
/// Options that decide from where to load additional modules.
/// </summary>
public class ModuleOptions {
  public ModuleLocation ModuleLocation { get;}

  public string? ModulePath { get; }

  private ModuleOptions(ModuleLocation moduleLocation, string? modulePath = null) {
    this.ModuleLocation = moduleLocation;
    ModulePath = modulePath;
  }

  /// <summary>
  /// Loads the node_modules over remote http-requests.
  /// </summary>
  /// <remarks>This option requires an internet connection.</remarks>
  public static ModuleOptions Remote => new(ModuleLocation.Remote);

  /// <summary>
  /// Loads the node_modules from the systems global npm node_module directory (needs npm installed and in path).
  /// </summary>
  public static ModuleOptions Global => new(ModuleLocation.Global, _LoadGlobalModulePath());

  private static string _LoadGlobalModulePath() {
    //todo: better error handling for cmd command
    var result = CommandLineHelper.RunCommand("npm list -g");
    var globalModulePath = Path.Combine(Regex.Split(result, "\r\n|\r|\n").First(), "node_modules");

    if (!Directory.Exists(globalModulePath))
      throw new ArgumentException($"Could not locate node_modules at \"{globalModulePath}\"");

    return globalModulePath;
  }

  /// <summary>
  /// Loads the node_modules from the given (local) path.
  /// </summary>
  /// <param name="modulePath">The path to the node_module directory.</param>
  public static ModuleOptions FromLocalPath(string modulePath) => new(ModuleLocation.Custom, modulePath);
}

public enum ModuleLocation {
  Remote,
  Global,
  Custom
}