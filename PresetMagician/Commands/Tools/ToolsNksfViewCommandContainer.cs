﻿using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ToolsNksfViewCommandContainer : AbstractOpenDialogCommandContainer
    {
        public ToolsNksfViewCommandContainer(ICommandManager commandManager, IServiceLocator serviceLocator)
            : base(Commands.Tools.NksfView, nameof(NKSFViewModel), true, commandManager, serviceLocator)
        {
        }
    }
}