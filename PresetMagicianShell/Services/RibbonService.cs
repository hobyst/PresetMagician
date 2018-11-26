﻿using System.Windows;
using Orchestra.Services;
using PresetMagicianShell.Views;

namespace PresetMagicianShell.Services
{
    public class RibbonService : IRibbonService
    {
        #region IRibbonService Members

        public FrameworkElement GetRibbon()
        {
            return new RibbonView();
        }

        public FrameworkElement GetMainView()
        {
            return new MainView();
        }

        public FrameworkElement GetStatusBar()
        {
            return new StatusBarView();
        }

        #endregion IRibbonService Members
    }
}