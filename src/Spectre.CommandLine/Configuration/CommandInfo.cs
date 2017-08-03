﻿using System;
using System.Collections.Generic;

namespace Spectre.CommandLine.Configuration
{
    internal sealed class CommandInfo : ICommandContainer
    {
        public string Name { get; }
        public string Description { get; set; }

        public Type CommandType { get; }
        public Type SettingsType { get; }

        public CommandInfo Parent { get; }

        public ICollection<CommandInfo> Commands { get; }
        public ICollection<CommandParameter> Parameters { get; }

        public bool IsProxy => CommandType == null;

        public CommandInfo(CommandInfo parent, string name, Type commandType, Type settingsType)
        {
            Parent = parent;
            Name = name;
            CommandType = commandType;
            SettingsType = settingsType;
            Commands = new List<CommandInfo>();
            Parameters = new List<CommandParameter>();
        }
    }
}